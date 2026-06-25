using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

		public void RestoreBackup(string[] backups, string installFolder, IProgress<int> progress = null)
		{
			int i = 0;
			if (backups.Length == 0)
			{
				logger.Warning("Nothing to restore!");
				return;
			}
			foreach (var path in backups)
			{
				progress?.Report(i++);
				string fileName = Path.GetFileName(path);
				string targetPath = fileName.Equals("main_gfx_1.tor", StringComparison.OrdinalIgnoreCase)
					? installFolder + "\\swtor\\retailclient\\" + fileName
					: installFolder + "\\Assets\\" + fileName;
				File.Copy(path, targetPath, true);
				File.Delete(path);
				logger.Log("Replaced " + targetPath + " with " + fileName);
			}
			logger.Log("Finished restoring backup!");
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

		public byte[] ExtractNode(Config config)
		{
			HashSet<ulong> bucketHashes = new();
			List<FileEntry> buckets = new();

			// this was faster to do when the program starts, but it's fast anyway and saves passing the bucket list in config
			for (int i = 0; i == BUCKET_LIMIT; i++)
			{
				bucketHashes.Add(Helpers.FileNameToHash("/resources/systemgenerated/buckets/" + i.ToString() + ".bkt"));
			}

			foreach (var archivePath in config.torFiles)
			{
				if (!archivePath.Contains("main_global_1.tor")){
					continue;
				}

				buckets = FindMatchesInArchive(archivePath, bucketHashes, out _);
			}

			if (buckets.Count == 0)
			{
				logger.Error("Could not find any bkt files.");
				return null;
			}

			foreach (FileEntry bucket in buckets)
			{

			}

			return null;
		}

		public void Replace(Config config)
		{
			ReplaceFiles(config);
		}
		// doesn't replace nodes. Replace node function does that then calls this
		// todo have an option to pass the location of a file in the archive if it's known, for faster bucket file replacement
		public void ReplaceFiles(Config config)
		{
			Dictionary<ulong, string> replacementsByHash = new();

			// could potentially get rid of changeList and just pass everything through hashChangeList
			foreach (var (gamePath, replacementFile) in config.changeList)
				replacementsByHash[Helpers.FileNameToHash(gamePath)] = replacementFile;

			foreach (var (hash, replacementFile) in config.hashChangeList)
				replacementsByHash[hash] = replacementFile;

			foreach (var archivePath in config.torFiles)
			{
				List<FileEntry> matches = FindMatchesInArchive(archivePath, replacementsByHash.Keys, out long appendPos);

				if (matches.Count == 0)
				{
					//logger.Log($"No matches in {archivePath}");
					continue;
				}

				logger.Log($"Match in {archivePath}");

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
					break;
				}
			}
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
			logger.Log("Reading " +  archivePath);
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
					fileCount++;
					ulong hash = br.ReadUInt64();

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

			//logger.Log(appendPos);

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
						logger.Log("replace in place and add skippable frame");
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
						logger.Log("replace in place exactly");
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

	}
}
