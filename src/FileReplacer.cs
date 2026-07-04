using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileChanger
{
	public class FileReplacer
	{
		private readonly int BUCKET_LIMIT = 996;
		private readonly ILogger logger;
		public FileReplacer(ILogger logger = null)
		{
			this.logger = logger ?? new ConsoleLogger();
		}

		private struct FileEntry
		{
			public ulong offset;
			public uint metaDataSize;
			public uint comprSize;
			public uint uncomprSize;
			public ulong hash;
			public uint metaDataChecksum;
			public ushort compressionType;
		}

		private struct NodeEntry
		{
			public uint entryLength;
			public byte[] betweenLengthName;
			public string name;

			public long entryStart;
			public ushort dataOffset;
			public long absDataOffset;
			public int dataSize;
			public int isCompressed;

			public byte[] header;
			public byte[] nodeData;

			//public int paddingLength;
			//public ulong[] glommedClasses; // may be empty
		}

		// could use a generic here
		private static long GetPadding(long value, long multiple = 8)
		{
			return (multiple - (value % multiple)) % multiple;
		}
		private static int GetPadding(int value, int multiple = 8)
		{
			return (multiple - (value % multiple)) % multiple;
		}

		public void Replace(Config config)
		{
			ReplaceNodes(config);
			ReplaceFiles(config);
		}

		private HashSet<ulong> HashBuckets()
		{
			HashSet<ulong> bucketHashes = new();
			// this was faster to do when the program starts, but it's fast anyway and saves passing the bucket list in config
			for (int i = 0; i <= BUCKET_LIMIT; i++)
			{
				ulong hash = Helpers.FileNameToHash("/resources/systemgenerated/buckets/" + i.ToString() + ".bkt");
				bucketHashes.Add(hash);
			}
			return bucketHashes;
		}

		public void ReplaceNodes(Config config)
		{
			HashSet<ulong> bucketHashes = HashBuckets();
			List<FileEntry> buckets = new();
			string mainGlobalPath = "";

			foreach (var archivePath in config.torFiles)
			{
				if (!archivePath.Contains("main_global_1.tor"))
				{
					continue;
				}

				mainGlobalPath = archivePath;

				buckets = FindMatchesInArchive(archivePath, bucketHashes, out _);
			}

			if (buckets.Count == 0)
			{
				logger.Error("Could not find any bkt files.");
				return;
			}

			using FileStream output = new(mainGlobalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using BinaryReader br = new(output);

			foreach (FileEntry entry in buckets)
			{
				br.BaseStream.Position = (long)(entry.offset + entry.metaDataSize);
				byte[] bucket = br.ReadBytes((int)entry.comprSize);

				if (entry.compressionType == 1)
					bucket = Helpers.Decompress(bucket);

				HashSet<string> targets = new(config.nodeChangeList.Keys);
				List<NodeEntry> matches = FindNodeMatchesInBucket(bucket, targets);

				if (matches.Count == 0)
					continue;

				byte[] newBucket = ApplyNodeReplacements(bucket, matches, config.nodeChangeList, out bool modified);
				if (!modified)
					continue;

				// todo don't do this since it's tied to gui config, make an in-memory replace option instead
				string fullPath = Path.Combine("files", entry.hash.ToString() + ".txt");
				File.WriteAllBytes(fullPath, newBucket);
				config.hashChangeList.Add(entry.hash, fullPath);
			}

			return;
		}

		public byte[] ExtractNode(Config config, string nodePath)
		{
			// todo a lot of this method is duplicated with ReplaceNodes
			HashSet<ulong> bucketHashes = HashBuckets();
			List<FileEntry> buckets = new();
			var target = new HashSet<string> { nodePath };
			string mainGlobalPath = "";

			foreach (var archivePath in config.torFiles)
			{
				if (!archivePath.Contains("main_global_1.tor"))
				{
					continue;
				}

				mainGlobalPath = archivePath;

				buckets = FindMatchesInArchive(archivePath, bucketHashes, out _);
			}

			if (buckets.Count == 0)
			{
				logger.Error("Could not find any bkt files.");
				return null;
			}

			using FileStream output = new(mainGlobalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using BinaryReader br = new(output);

			foreach (FileEntry entry in buckets)
			{
				br.BaseStream.Position = (long)(entry.offset + entry.metaDataSize);
				byte[] bucket = br.ReadBytes((int)entry.comprSize);

				if (entry.compressionType == 1)
					bucket = Helpers.Decompress(bucket);

				List<NodeEntry> matches = FindNodeMatchesInBucket(bucket, target);

				if (matches.Count == 0)
					continue;

				NodeEntry nodeEntry = matches[0];

				/*
				 * byte[] node = new byte[nodeEntry.dataSize];
				Array.Copy(bucket, nodeEntry.dataOffset, node, 0, nodeEntry.dataSize);

				if (nodeEntry.isCompressed == 1)
					node = Helpers.Decompress(node);
				*/

				return nodeEntry.nodeData;
			}

			return null;
		}

		// assuming matches is sorted by where they appear in bucket
		private byte[] ApplyNodeReplacements(byte[] bucket, List<NodeEntry> matches, Dictionary<string, string> nodeChangeList, out bool modified)
		{
			/*
			using var ms = new MemoryStream(bucket);
			using var bw = new BinaryWriter(ms);
			*/
			modified = false;

			var bucketList = new List<byte>(bucket);
			long accumOffset = 0;

			foreach (NodeEntry origNode in matches)
			{
				string replacementNode = nodeChangeList[origNode.name];

				if (!File.Exists(replacementNode))
				{
					logger.Warning("File " + replacementNode + " could not be found.");
					continue;
				}

				byte[] strippedNodeData = File.ReadAllBytes(replacementNode);

				if (strippedNodeData[0] == 0)
				{
					logger.Warning("replacement node " + replacementNode + " starts with 0, this will likely cause an error");
				}

				byte[] nodeData = new byte[origNode.header.Length + strippedNodeData.Length];

				origNode.header.CopyTo(nodeData, 0);
				strippedNodeData.CopyTo(nodeData, origNode.header.Length);

				if (origNode.isCompressed == 1)
				{
					byte[] comprNode = Helpers.Compress(nodeData, 22);

					if (comprNode.Length + 8 <= origNode.dataSize)
					{
						logger.Debug("replace node in place and add skippable frame");
						int skippableFrameLength = origNode.dataSize - comprNode.Length - 8;
						byte[] appended = new byte[origNode.dataSize];
						comprNode.CopyTo(appended, 0);
						BitConverter.GetBytes(0x184D2A50).CopyTo(appended, comprNode.Length); // skippable frame magic bytes
						BitConverter.GetBytes(skippableFrameLength).CopyTo(appended, comprNode.Length + 4);
						comprNode = appended;
					}
					else if (comprNode.Length < origNode.dataSize)
					{
						for (int i = 21; i >= -7; i--)
						{
							byte[] trial = Helpers.Compress(nodeData, i);
							if (trial.Length == origNode.dataSize)
							{
								comprNode = trial;
								break;
							}
						}
					}
					nodeData = comprNode;
					logger.Debug("orig len: " + origNode.dataSize + ", new len: " + nodeData.Length);
				}

				long adjustedEntryStart = origNode.entryStart + accumOffset;

				if (nodeData.Length != origNode.dataSize)
				{
					int origPadding = GetPadding((int)origNode.entryLength);
					int origLength = (int)origNode.entryLength + origPadding;

					int newLength = 50 + origNode.name.Length + 2 + nodeData.Length; // header + name + 2 null terminators + data
					int padding = GetPadding(newLength);
					byte[] replacementEntry = new byte[newLength + padding];

					using var nms = new MemoryStream(replacementEntry);
					using var nbw = new BinaryWriter(nms);

					nbw.Write(newLength);
					nbw.Write(origNode.betweenLengthName[..36]);
					nbw.Write(strippedNodeData.Length);
					nbw.Write(origNode.betweenLengthName[40..]);
					nbw.Write(Encoding.ASCII.GetBytes(origNode.name));
					nbw.Write((short)0);
					nbw.Write(nodeData);

					bucketList.RemoveRange((int)adjustedEntryStart, origLength);
					bucketList.InsertRange((int)adjustedEntryStart, replacementEntry);

					accumOffset += (newLength + padding) - origLength;
				}
				else
				{
					for (int i = 0; i < nodeData.Length; i++)
						bucketList[(int)adjustedEntryStart + origNode.dataOffset + i] = nodeData[i];
				}
				
				modified = true;
			}

			if (accumOffset != 0)
			{
				uint oldSectionLength = BitConverter.ToUInt32(bucketList.GetRange(24, 4).ToArray(), 0);
				uint newSectionLength = (uint)(oldSectionLength + accumOffset);
				byte[] lengthBytes = BitConverter.GetBytes(newSectionLength);
				for (int i = 0; i < 4; i++)
					bucketList[24 + i] = lengthBytes[i];
			}

			return bucketList.ToArray();
		}

		// todo this function got too big, shouldn't necessarily decompress/resolve glommed
		private List<NodeEntry> FindNodeMatchesInBucket(byte[] bucket, HashSet<string> targets)
		{
			List<NodeEntry> matches = new();
			using var ms = new MemoryStream(bucket);
			using var br = new BinaryReader(ms);

			if (br.ReadUInt32() != 0x4B554250)
			{
				logger.Warning("not a valid bkt file");
				return matches;
			}

			br.BaseStream.Position += 4;
			uint dblbLength = br.ReadUInt32();
			br.BaseStream.Position += dblbLength; // skip first empty DBLB
			dblbLength = br.ReadUInt32();
			long dblbStartOffset = br.BaseStream.Position;

			if (dblbStartOffset != 28)
			{
				logger.Warning("expected dblbStartOffset of 28, instead: " + dblbStartOffset);
			}

			br.BaseStream.Position += 4; // DBLB header

			if (br.ReadUInt32() == 1)
			{
				logger.Warning("dblb version 1 not supported");
				return matches;
			}

			while (br.BaseStream.Position + 4 < dblbStartOffset + dblbLength)
			{
				long entryStart = br.BaseStream.Position;
				uint entryLength = br.ReadUInt32();
				byte[] betweenLengthName = br.ReadBytes(46); // skip to node name
				string name = br.ReadCString();
				//logger.Log(node);

				if (targets.Contains(name))
				{
					logger.Debug("found " + name);
					br.BaseStream.Position = entryStart + 16;
					ushort bitset = br.ReadUInt16();
					int isCompressed = bitset & 1;
					ushort dataOffset = br.ReadUInt16();
					br.BaseStream.Position += 16;
					ushort numGlommed = br.ReadUInt16();
					ushort glommedOffset = br.ReadUInt16();

					int paddingLength = glommedOffset - dataOffset;
					int dataSize = (int)entryLength - dataOffset;
					long absDataOffset = entryStart + dataOffset;

					br.BaseStream.Position = absDataOffset;
					byte[] nodeData = br.ReadBytes(dataSize);

					if (isCompressed == 1)
					{
						nodeData = Helpers.Decompress(nodeData);
					}

					using var nms = new MemoryStream(nodeData);
					using var nbr = new BinaryReader(nms);

					byte[] header = nbr.ReadBytes(paddingLength + numGlommed * 8);

					/*
					nbr.BaseStream.Position += paddingLength;
					ulong[] glommedClasses = new ulong[numGlommed];
					
					for (int i = 0; i < numGlommed; i++)
					{
						glommedClasses[i] = nbr.ReadUInt64();
					}
					*/

					byte[] strippedNodeData = nbr.ReadBytes((int)(nms.Length - nms.Position));

					NodeEntry node = new()
					{
						entryLength = entryLength,
						betweenLengthName = betweenLengthName,
						name = name,
						entryStart = entryStart,
						dataOffset = dataOffset,
						absDataOffset = absDataOffset,
						dataSize = dataSize,
						isCompressed = isCompressed,
						header = header,
						nodeData = strippedNodeData

						/*
						paddingLength = paddingLength,
						glommedClasses = glommedClasses,
						*/
					};

					matches.Add(node);

					targets.Remove(name);

					if (targets.Count == 0)
					{
						break;
					}
				}
				long next = entryStart + entryLength;
				next += GetPadding(next - dblbStartOffset);
				br.BaseStream.Position = next;
			}
			return matches;
		}

		// todo have an option to pass the location of a file in the archive if it's known, for faster bucket file replacement
		public void ReplaceFiles(Config config)
		{
			Dictionary<ulong, string> replacementsByHash = new();

			// could potentially get rid of changeList and just pass everything through hashChangeList
			foreach (var (gamePath, replacementFile) in config.changeList)
				replacementsByHash[Helpers.FileNameToHash(gamePath)] = replacementFile;

			foreach (var (hash, replacementFile) in config.hashChangeList)
				replacementsByHash[hash] = replacementFile;

			if (replacementsByHash.Count == 0)
			{
				logger.Error("Nothing to replace!");
				return;
			}

			foreach (var archivePath in config.torFiles)
			{
				List<FileEntry> matches = FindMatchesInArchive(archivePath, replacementsByHash.Keys, out long appendPos);

				if (matches.Count == 0)
				{
					//logger.Log($"No matches in {archivePath}");
					continue;
				}

				logger.Debug($"Match in {archivePath}");

				if (config.createBackup)
				{
					string backupPath = Path.Combine("backup", Path.GetFileName(archivePath));
					if (!File.Exists(backupPath))
						File.Copy(archivePath, backupPath, true);
				}

				ApplyReplacements(archivePath, matches, replacementsByHash, appendPos);

				foreach (var match in matches)
				{
					replacementsByHash.Remove(match.hash);
				}

				if (replacementsByHash.Count == 0)
				{
					logger.Log("All replacements done.");
					return;
				}
			}

			logger.Warning("some file names were not found in archives");
		}

		public byte[] ExtractFile(Config config, string gamePath)
		{
			ulong hash = Helpers.FileNameToHash(gamePath);
			var target = new HashSet<ulong> { hash };

			foreach (var archivePath in config.torFiles)
			{
				List<FileEntry> matches = FindMatchesInArchive(archivePath, target, out _);

				if (matches.Count == 0)
					continue;

				FileEntry fileEntry = matches[0];

				using FileStream archive = new(archivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using BinaryReader br = new(archive);

				br.BaseStream.Position = (long)(fileEntry.offset + fileEntry.metaDataSize);
				byte[] file = br.ReadBytes((int)fileEntry.comprSize);

				if (fileEntry.compressionType == 1)
					file = Helpers.Decompress(file);

				return file;
			}

			return null;  // File not found
		}

		// appendPos is the position after the last file table entry, where new entries can be written
		private List<FileEntry> FindMatchesInArchive(string archivePath, ICollection<ulong> targets, out long appendPos)
		{
			List<FileEntry> matches = new();
			appendPos = 0;

			using FileStream archive = new(archivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using BinaryReader br = new(archive);

			if (br.ReadUInt32() != 0x0050594D)
			{
				logger.Warning(archivePath + " is not a valid .tor archive!");
				return matches;
			}
			//logger.Log("Reading " +  archivePath);
			if (br.ReadUInt32() != 6)
			{
				logger.Warning("Only version 6 is supported; " + archivePath + " cannot be read!");
				return matches;
			}

			br.BaseStream.Position += 4; // byte order mark
			ulong ftOffset = br.ReadUInt64();
			br.BaseStream.Position += 4; // ftCapacity
			uint numOfFiles = br.ReadUInt32();
			uint fileCount = 0;

			while (ftOffset != 0)
			{
				br.BaseStream.Position = (long)ftOffset;
				uint ftCapacity = br.ReadUInt32();
				ftOffset = br.ReadUInt64(); // next file table, 0 if last
				br.BaseStream.Position += 20; // go to hash of first entry

				for (var i = 0; i < ftCapacity && fileCount < numOfFiles; i++)
				{
					ulong hash = br.ReadUInt64();

					if (hash != 0)
					{
						fileCount++;
					}

					/*
					if (hash == Helpers.FileNameToHash("ft.sig"))
					{
						logger.Debug("found ft.sig, fileCount = " + fileCount + " , numOfFiles = " + numOfFiles);
					}
					*/

					if (targets.Contains(hash))
					{
						// found hash
						br.BaseStream.Position -= 28; // go back to start of entry
						FileEntry fileEntry = new()
						{
							offset = br.ReadUInt64(),
							metaDataSize = br.ReadUInt32(),
							comprSize = br.ReadUInt32(),
							uncomprSize = br.ReadUInt32(),
							hash = br.ReadUInt64(),					// read to make the initialiser nicer
							metaDataChecksum = br.ReadUInt32(),
							compressionType = br.ReadUInt16()
						};
						matches.Add(fileEntry);
						br.BaseStream.Position += 20; // go to hash of next entry
					}
					else
					{
						br.BaseStream.Position += 26; // skip rest of entry and go to hash of next entry
					}
				}
			}

			appendPos = br.BaseStream.Position - 20; // undo go to hash of next entry
			//logger.Debug($"append: {appendPos:X8}");

			return matches;
		}

		private void ApplyReplacements(string archivePath, List<FileEntry> matches, Dictionary<ulong, string> replacementsByHash, long appendPos)
		{
			using FileStream output = new(archivePath, FileMode.Open, FileAccess.Write, FileShare.Read);
			using BinaryWriter bw = new(output);

			foreach (var fileEntry in matches)
			{
				string replacementFile = replacementsByHash[fileEntry.hash];

				if (!File.Exists(replacementFile))
				{
					logger.Warning("File " + replacementFile + " could not be found.");
					continue;
				}

				byte[] data = File.ReadAllBytes(replacementFile);
				uint uncomprSize = (uint)data.Length;
				uint comprSize = uncomprSize; // identical to uncomprSize for uncompressed files

				if (fileEntry.compressionType == 1)
				{
					data = Helpers.Compress(data, 22);
					comprSize = (uint)data.Length;

					if (comprSize + 8 <= fileEntry.comprSize)
					{
						logger.Debug("replace in place and add skippable frame");
						uint lenBlank = fileEntry.comprSize - comprSize - 8;
						bw.BaseStream.Position = (long)(fileEntry.offset + fileEntry.metaDataSize);
						bw.Write(data);
						bw.Write(0x184D2A50); // skippable frame magic bytes
						bw.Write(lenBlank);
						bw.Write(new byte[lenBlank]);
						continue;
					}
					// todo potentially compress at lighter levels to see if it matches orig length exactly, in which case replace in place
					if (comprSize == fileEntry.comprSize)
					{
						logger.Debug("replace in place exactly");
						bw.BaseStream.Position = (long)(fileEntry.offset + fileEntry.metaDataSize);
						bw.Write(data);
						continue;
					}
				}
				else
				{
					// todo replace in place for uncompressed files when safe to have trailing zeros, but need to identify safe files somehow (e.g. by extension)
					if (data.Length == fileEntry.uncomprSize)
					{
						bw.BaseStream.Position = (long)(fileEntry.offset + fileEntry.metaDataSize);
						bw.Write(data);
						continue;
					}
				}
				// important todo new file table if necessary
				bw.Seek(0, SeekOrigin.End);
				ulong position = (ulong)bw.BaseStream.Position;
				bw.Write(data);
				bw.BaseStream.Position = appendPos;
				bw.Write(position);
				bw.Write(0);
				bw.Write(comprSize);
				bw.Write(uncomprSize);
				bw.Write(fileEntry.hash);
				bw.Write(0xDEADBEEF);
				bw.Write(fileEntry.compressionType);
				appendPos = bw.BaseStream.Position;
			}
		}

		public void RestoreBackup(string[] backups, string installFolder)
		{
			if (backups.Length == 0)
			{
				logger.Warning("Nothing to restore!");
				return;
			}
			foreach (var path in backups)
			{
				string fileName = Path.GetFileName(path);
				string targetPath = fileName.Equals("main_gfx_1.tor", StringComparison.OrdinalIgnoreCase)
					? installFolder + "\\swtor\\retailclient\\" + fileName
					: installFolder + "\\Assets\\" + fileName;
				File.Copy(path, targetPath, true);
				File.Delete(path);
				logger.Debug("Replaced " + targetPath + " with " + fileName);
			}
			logger.Log("Finished restoring backup!");
		}

	}
}
