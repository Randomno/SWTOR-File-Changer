using Force.Crc32;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZstdSharp;
using System.IO.Compression;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SWTOR_File_Changer
{
    public class FileReplacer
    {
		public static uint ComputeCrc32(byte[] data)
		{
			return Force.Crc32.Crc32Algorithm.Compute(data);
		}

		private struct FileTable
		{
			public uint capacity;
			public ulong offset;
		}
		private struct FileEntry
		{
			public ulong offset;
			public uint metaDataSize;
			public uint comprSize;
			public uint uncomprSize;
			public uint metaDataChecksum;
			public ushort compressionType;
			public int fileTableNum;
			public int fileTableFileIndex;
		}


		public byte[] ExtractFile(ulong hash, List<string> files, bool isPTS, bool isLive)
		{
			for (int index = 0; index < files.Count; index++)
			{
				string str = files[index].Substring(checked(files[index].LastIndexOf("\\") + 1));
				if (!(isPTS & !str.StartsWith("swtor_test_")) && !(isLive & str.StartsWith("swtor_test_")))
				{
					FileStream input = new(files[index], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					/*
					var sha1 = SHA1.Create();
					string sha1OfArchive = Convert.ToHexString(sha1.ComputeHash(input));
					input.Position = 0;
					Debug.WriteLine(sha1OfArchive);
					*/
					BinaryReader binaryReader = new(input);

					// TODO log these errors differently. All this file reading code should be moved anyway
					if (binaryReader.ReadByte() != 77 | binaryReader.ReadByte() != 89 | binaryReader.ReadByte() != 80 | binaryReader.ReadByte() != 0)
					{
						MessageBox.Show(files[index] + " is not a valid .tor archive!");
					}
					if (binaryReader.ReadUInt32() != 6U)
					{
						MessageBox.Show("Only version 6 is supported; " + files[index] + " cannot be read!");
					}
					int num4 = (int)binaryReader.ReadUInt32();
					ulong offset = binaryReader.ReadUInt64();
					while (decimal.Compare(new decimal(offset), 0M) != 0 && decimal.Compare(new decimal(offset), new decimal(binaryReader.BaseStream.Length)) < 0)
					{
						binaryReader.BaseStream.Seek(checked((long)offset), SeekOrigin.Begin);
						ulong num5 = binaryReader.ReadUInt32();
						offset = binaryReader.ReadUInt64();
						decimal limit = decimal.Subtract(new decimal(num5), 1M);
						for (decimal num6 = 0M; ObjectFlowControl.ForLoopControl.ForNextCheckDec(num6, limit, 1M); num6 = decimal.Add(num6, 1M))
						{
							ulong num7 = binaryReader.ReadUInt64();
							if (decimal.Compare(new decimal(num7), 0M) != 0)
							{
								uint num8 = binaryReader.ReadUInt32();
								uint count = binaryReader.ReadUInt32();
								binaryReader.ReadUInt32();
								ulong num9 = binaryReader.ReadUInt64();
								binaryReader.ReadBytes(4);
								bool flag2 = binaryReader.ReadUInt16() > 0;
								if ((long)num9 == (long)hash)
								{
									// Found the file: read and decompress
									binaryReader.BaseStream.Position = (long)(num7 + num8);
									byte[] blob = binaryReader.ReadBytes((int)count);
									byte[] outBytes;
									if (flag2)
										outBytes = new Decompressor()
																.Unwrap(blob)   // Span<byte>
																.ToArray();      // -> byte[]
									else
										outBytes = blob;

									return outBytes;
								}
							}
							else
								break;
						}
					}
				}
			}

			return null;  // File not found
		}

		// TODO don't pass so many args in. origNamesList seems unnecessary. The last 2 args are only relevant to node changing.
		// TODO make this not necessarily write to file (for testing purposes), but not sure how to do it efficiently
		public void LoadArchiveReplaceFiles(string torFilePath, bool createBackup, bool editNode, Hashtable changeList, Hashtable origNamesList, Hashtable nodeChangeList = null, Hashtable bucketList = null, string replaceDir = "files")
		{
			int index1 = -1;
			int num1 = -1;
			List<FileTable> fileTableList = new();
			Hashtable hashtable = new();
			FileStream input = new(torFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			BinaryReader brReader = new(input);
			if (brReader.ReadByte() != 77 | brReader.ReadByte() != 89 | brReader.ReadByte() != 80 | brReader.ReadByte() != 0)
			{
				throw new InvalidDataException(torFilePath + " is not a valid .tor archive!");
			}
			if (brReader.ReadUInt32() != 6U)
			{
				throw new InvalidDataException("Only version 6 is supported; " + torFilePath + " cannot be read!");
			}
			int num4 = (int)brReader.ReadUInt32();
			ulong offset = brReader.ReadUInt64();
			while (decimal.Compare(new decimal(offset), 0M) != 0 && decimal.Compare(new decimal(offset), new decimal(brReader.BaseStream.Length)) < 0)
			{
				brReader.BaseStream.Seek(checked((long)offset), SeekOrigin.Begin);
				uint num5 = brReader.ReadUInt32();
				FileTable fileTable;
				fileTable.offset = offset;
				fileTable.capacity = num5;
				fileTableList.Add(fileTable);
				checked { ++index1; }
				num1 = -1;
				offset = brReader.ReadUInt64();
				long num6 = checked(num5 - 1L);
				for (long num7 = 0; num7 <= num6; num7++)
				{
					ulong num8 = brReader.ReadUInt64();
					if (decimal.Compare(new decimal(num8), 0M) != 0)
					{
						uint num9 = brReader.ReadUInt32();
						uint num10 = brReader.ReadUInt32();
						uint num11 = brReader.ReadUInt32();
						ulong key1 = brReader.ReadUInt64();
						uint num12 = brReader.ReadUInt32();
						ushort num13 = brReader.ReadUInt16();
						FileEntry fileEntry;
						fileEntry.offset = num8;
						fileEntry.metaDataSize = num9;
						fileEntry.comprSize = num10;
						fileEntry.uncomprSize = num11;
						fileEntry.metaDataChecksum = num12;
						fileEntry.compressionType = num13;
						fileEntry.fileTableNum = index1;
						fileEntry.fileTableFileIndex = checked((int)num7);
						num1 = checked((int)num7);
						if (!hashtable.ContainsKey(key1))
							hashtable[key1] = new List<FileEntry>();
						((List<FileEntry>)hashtable[key1]).Add(fileEntry);

						if (editNode
							&& torFilePath.EndsWith("_main_global_1.tor", StringComparison.OrdinalIgnoreCase)
							&& bucketList != null
							&& bucketList.ContainsKey(key1))
						{
							long restorePos = brReader.BaseStream.Position;
							try
							{

								// 1) read & decompress the bucket blob
								long metaStart = (long)fileEntry.offset + fileEntry.metaDataSize;
								brReader.BaseStream.Seek(metaStart, SeekOrigin.Begin);
								byte[] compBucket = brReader.ReadBytes((int)fileEntry.comprSize);

								byte[] bucketData;
								try
								{
									bucketData = new Decompressor()
												 .Unwrap(compBucket, (int)fileEntry.uncomprSize)
												 .ToArray();
								}
								catch (Exception ex)
								{
									// skip this entry and try the next one
									continue;
								}

								// 2) skip PBUK header + first (empty) DBLB
								using var ms = new MemoryStream(bucketData);
								using var dr = new BinaryReader(ms);
								ms.Position = 8;
								uint firstLen = dr.ReadUInt32();
								ms.Position += firstLen;

								// 3) scan every remaining DBLB section
								bool replaced = false;
								while (!replaced && ms.Position + 4 <= bucketData.Length)
								{
									long sectionLenPos = ms.Position;
									uint sectionLen = dr.ReadUInt32();
									long sectionStart = ms.Position;
									long sectionEnd = Math.Min(sectionStart + sectionLen, bucketData.Length);

									dr.ReadUInt32();              // magic “DBLB”
									uint version = dr.ReadUInt32();

									long entryPos = sectionStart + 8;
									while (!replaced && entryPos + 4 <= sectionEnd)
									{
										ms.Position = entryPos;
										uint entryLen = dr.ReadUInt32();
										if (entryLen == 0) break;

										long entryStart = entryPos;
										ms.Position = entryStart + 4;

										// read header for dataOffset
										ushort dataOffset;
										if (version == 1)
										{
											dr.ReadUInt16();       // bitset
											dataOffset = dr.ReadUInt16();
											dr.ReadUInt64();       // id
										}
										else
										{
											dr.ReadUInt32();       // zero
											dr.ReadUInt64();       // id
											dr.ReadUInt16();       // bitset
											dataOffset = dr.ReadUInt16();
										}

										ushort nameOff = dr.ReadUInt16();
										dr.ReadUInt16();          // descOff

										// 4) pull out the inlineKey string
										string inlineKey = Helpers.ReadStringAt(
											bucketData,
											(int)(entryStart + nameOff)
										);

										if (nodeChangeList.ContainsKey(inlineKey))
										{
											// 1) Load and compress the new node
											byte[] replacementRaw = File.ReadAllBytes(
												Path.Combine("files", nodeChangeList[inlineKey].ToString())
											);
											int origCompLen = (int)entryLen - dataOffset;

											ushort blobMagic = BitConverter.ToUInt16(bucketData, (int)(entryStart + dataOffset));
											byte[] replacementBlob;
											string compAlg;
											if (blobMagic == 0x9C78)  // DEFLATE magic
											{
												using var defMs = new MemoryStream();
												using (var def = new DeflateStream(defMs, CompressionLevel.Optimal, leaveOpen: true))
													def.Write(replacementRaw, 0, replacementRaw.Length);
												replacementBlob = defMs.ToArray();
												compAlg = "DEFLATE";
											}
											else if (blobMagic == 0xB528)  // ZSTD magic
											{
												var zstd = new Compressor();
												replacementBlob = zstd.Wrap(replacementRaw).ToArray();
												compAlg = "ZSTD";
											}
											else  // fallback to DEFLATE
											{
												using var defMs = new MemoryStream();
												using (var def = new DeflateStream(defMs, CompressionLevel.Optimal, leaveOpen: true))
													def.Write(replacementRaw, 0, replacementRaw.Length);
												replacementBlob = defMs.ToArray();
												compAlg = "DEFLATE";
											}

											// 2) If node too large, iteratively try lighter compression
											if (replacementBlob.Length > origCompLen)
											{
												if (compAlg == "ZSTD")
												{
													for (int lvl = 1; lvl <= 22; lvl++)
													{
														var trial = new Compressor(lvl).Wrap(replacementRaw).ToArray();
														if (trial.Length <= origCompLen)
														{
															replacementBlob = trial;
															break;
														}
													}
												}
												else  // DEFLATE: try raw
												{
													replacementBlob = replacementRaw;
												}
											}

											// 3) STILL too big? → full-bucket file-level replace
											if (replacementBlob.Length > origCompLen)
											{
												string bucketPath = $"resources/systemgenerated/buckets/{fileEntry.fileTableFileIndex}.bkt";
												var fullPath = Path.Combine("files", bucketPath.TrimStart('/'));
												Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
												File.WriteAllBytes(fullPath, bucketData);
												changeList[key1] = bucketPath;
												break;  // skip any in-place work
											}

											// 4) Node fits: splice it in & update DBLB
											Array.Copy(
												replacementBlob, 0,
												bucketData, (int)(entryStart + dataOffset),
												replacementBlob.Length
											);
											if (replacementBlob.Length < origCompLen)
												Array.Clear(
													bucketData,
													(int)(entryStart + dataOffset + replacementBlob.Length),
													origCompLen - replacementBlob.Length
												);

											uint newEntryLen = (uint)(dataOffset + replacementBlob.Length);
											Array.Copy(
												BitConverter.GetBytes(newEntryLen),
												0,
												bucketData,
												(int)entryStart,
												4
											);

											// 5) Recompress the entire bucket at descending ZSTD levels
											byte[] newComp = null;
											bool bucketFits = false;
											for (int lvl = 22; lvl >= 1; lvl--)
											{
												var trial = new Compressor(lvl).Wrap(bucketData).ToArray();
												if (trial.Length <= fileEntry.comprSize)
												{
													newComp = trial;
													bucketFits = true;
													break;
												}
											}

											// 6a) STILL too big? → full-bucket file-level replace
											if (!bucketFits)
											{
												string bucketPath = $"resources/systemgenerated/buckets/{fileEntry.fileTableFileIndex}.bkt";
												var fullPath = Path.Combine("files", bucketPath.TrimStart('/'));
												Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
												File.WriteAllBytes(fullPath, bucketData);
												changeList[key1] = bucketPath;
												break;
											}

											// 6b) Bucket fits: write it in-place
											using (var outFs = new FileStream(torFilePath, FileMode.Open, FileAccess.Write, FileShare.Read))
											using (var writer = new BinaryWriter(outFs))
											{
												writer.BaseStream.Position = metaStart;
												writer.Write(newComp);
												if (newComp.Length < fileEntry.comprSize)
													writer.Write(new byte[fileEntry.comprSize - newComp.Length]);

												// Update compSize in directory
												var ft = fileTableList[fileEntry.fileTableNum];
												long dirBase = (long)(
													ft.offset
													+ 12
													+ (ulong)(fileEntry.fileTableFileIndex * 34)
												);
												writer.BaseStream.Position = dirBase + 12;
												writer.Write((uint)newComp.Length);

												// Update CRC32 in directory
												uint newCrc = Crc32Algorithm.Compute(bucketData);
												long dirEntry = (long)(
													ft.offset
													+ 12
													+ (ulong)(fileEntry.fileTableFileIndex * 34)
													+ (8 + 4 + 4 + 4 + 8)
												);
												writer.BaseStream.Position = dirEntry;
												writer.Write(newCrc);

											}

											break;  // done with this node
										}
										// ── END NODE REPLACEMENT ──


										// advance to next entry (8-byte align)
										long rel = entryStart - sectionStart;
										long padded = rel + entryLen + 7 & ~7L;
										entryPos = sectionStart + padded;
									}

									ms.Position = sectionStart + sectionLen;
								}

							}
							finally
							{
								// restore file-stream pointer for the outer loop
								brReader.BaseStream.Seek(restorePos, SeekOrigin.Begin);
							}
						}
					}
				}
			}
			foreach (object key3 in (IEnumerable)changeList.Keys)
			{
				ulong key4 = Conversions.ToULong(key3);
				if (hashtable.ContainsKey(key4))
				{
					string replacePath = Path.Combine(replaceDir, changeList[key4].ToString());
					List<FileEntry> fileEntryList = (List<FileEntry>)hashtable[key4];
					if (!File.Exists(replacePath))
					{
						throw new InvalidDataException("File " + changeList[key4] + " could not be found, needed in " + origNamesList[key4] + ".");
					}
					else
					{
						if (createBackup)
						{
							if (!File.Exists("backup\\" + torFilePath.Substring(checked(torFilePath.LastIndexOf("\\") + 1))))
								File.Copy(torFilePath, "backup\\" + torFilePath.Substring(checked(torFilePath.LastIndexOf("\\") + 1)), true);
						}
						FileStream output = new(torFilePath, FileMode.Open, FileAccess.Write, FileShare.Read);
						BinaryWriter binaryWriter1 = new(output);
						// ── Read replacement file & compress with Zstandard ──
						byte[] rawData = File.ReadAllBytes(replacePath);
						byte[] compData = Helpers.Compress(rawData);
						int length2 = compData.Length;
						int length1 = rawData.Length;


						// ── Compute CRC32 of the raw (uncompressed) data ──
						uint newCrc = Crc32Algorithm.Compute(rawData);

						bool flag2 = false;
						int num20 = checked(fileEntryList.Count - 1);
						FileTable fileTable1;
						for (int index2 = 0; index2 <= num20; index2++)
						{
							FileEntry fileEntry = fileEntryList[index2];
							if (!flag2)
							{
								if (fileEntry.compressionType == 1)
								{
									if (length2 <= fileEntry.comprSize)
									{
										// ── Write LZ4 data in‑place and pad if smaller ──
										binaryWriter1.BaseStream.Position =
											(long)(fileEntry.offset + fileEntry.metaDataSize);
										binaryWriter1.Write(compData);
										if (compData.Length < fileEntry.comprSize)
										{
											binaryWriter1.Write(new byte[fileEntry.comprSize - compData.Length]);
										}

										// ── Update index metadata so the client sees the new sizes & CRC ──
										var ft = fileTableList[fileEntry.fileTableNum];
										long entryPos = (long)(ft.offset + (ulong)(fileEntry.fileTableFileIndex * 34));
										binaryWriter1.BaseStream.Seek(entryPos, SeekOrigin.Begin);
										binaryWriter1.Write(fileEntry.offset);        // data offset (unchanged)
										binaryWriter1.Write(fileEntry.metaDataSize);  // metadata size (unchanged)
										binaryWriter1.Write((uint)length2);           // new compressed size
										binaryWriter1.Write((uint)length1);           // new uncompressed size
										binaryWriter1.Write(key4);                    // file ID
										binaryWriter1.Write(newCrc);                  // new CRC32
										binaryWriter1.Write((ushort)1);               // compressionType = LZ4

										flag2 = true;


									}
								}
								else if (fileEntry.compressionType == 0 && length1 <= fileEntry.uncomprSize)
								{
									// ── Write raw data in‑place ──
									binaryWriter1.BaseStream.Position =
										(long)(fileEntry.offset + fileEntry.metaDataSize);
									binaryWriter1.Write(rawData);
									flag2 = true;

								}
							}
							else
							{
								fileTable1 = fileTableList[fileEntry.fileTableNum];
								// 34 bytes per record → pick the right entry
								long deleteEntryPos = (long)(fileTable1.offset + (ulong)(34 * fileEntry.fileTableFileIndex));
								binaryWriter1.BaseStream.Seek(deleteEntryPos, SeekOrigin.Begin);
								binaryWriter1.Write(0UL);
								binaryWriter1.Write(0UL);
								binaryWriter1.Write(0UL);
								binaryWriter1.Write(0UL);
								binaryWriter1.Write((ushort)0);
							}
						}
						if (!flag2)
						{
							long num22 = checked(num1 + 1);
							fileTable1 = fileTableList[index1];
							long capacity1 = fileTable1.capacity;
							if (num22 >= capacity1)
							{
								FileTable fileTable2;
								fileTable2.capacity = 1000U;
								binaryWriter1.Seek(0, SeekOrigin.End);
								fileTable2.offset = checked((ulong)binaryWriter1.BaseStream.Position);
								binaryWriter1.Write(fileTable2.capacity);
								binaryWriter1.Write(0UL);
								uint capacity2 = fileTable2.capacity;
								for (uint num23 = 1; num23 <= capacity2; num23++)
								{
									binaryWriter1.Write(0UL);
									binaryWriter1.Write(0UL);
									binaryWriter1.Write(0UL);
									binaryWriter1.Write(0UL);
									binaryWriter1.Write((ushort)0);
								}
								BinaryWriter binaryWriter2 = binaryWriter1;
								fileTable1 = fileTableList[index1];
								long nextTablePtrPos = (long)(fileTable1.offset + 4);
								binaryWriter2.BaseStream.Seek(nextTablePtrPos, SeekOrigin.Begin);
								binaryWriter1.Write(fileTable2.offset);
								fileTableList.Add(fileTable2);
								checked { ++index1; }
								num1 = -1;
							}
							FileEntry fileEntry = fileEntryList[0];
							binaryWriter1.Seek(0, SeekOrigin.End);
							ulong position = checked((ulong)binaryWriter1.BaseStream.Position);
							brReader.BaseStream.Position = checked((long)fileEntry.offset);
							byte[] buffer = brReader.ReadBytes(checked((int)fileEntry.metaDataSize));
							binaryWriter1.Write(buffer);
							// ── Write the new file data ──
							if (fileEntry.compressionType == 1)
								binaryWriter1.Write(compData);
							else
								binaryWriter1.Write(rawData);
							checked { ++num1; }
							BinaryWriter binaryWriter3 = binaryWriter1;
							fileTable1 = fileTableList[index1];
							long newEntryPos = (long)(fileTable1.offset + 12 + (ulong)(34 * num1));
							binaryWriter3.BaseStream.Seek(newEntryPos, SeekOrigin.Begin);
							binaryWriter1.Write(position);
							binaryWriter1.Write(fileEntry.metaDataSize);
							if (fileEntry.compressionType == 1)
								binaryWriter1.Write(checked((uint)length2));
							else
								binaryWriter1.Write(checked((uint)length1));
							binaryWriter1.Write(checked((uint)length1));
							binaryWriter1.Write(key4);      // file ID
							binaryWriter1.Write(newCrc);    // updated CRC32
							binaryWriter1.Write((ushort)1);
						}
						output.Close();
						output.Dispose();
					}
				}
			label_60:;
			}
			input.Close();
			input.Dispose();
		}
	}
}
