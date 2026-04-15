using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.VisualBasic;
using HashCreatorNS;
using HashDictionaryNS;
using HasherFunctionsNS;
using System.Collections;
using ZstdSharp;
using System.IO.Compression;

namespace FileChanger
{
	public partial class GUI : Form
	{
		private FileReplacer replacer;

		public Hashtable changeList;
		public Hashtable origNamesList;
		public bool editNode;
		public Hashtable nodeChangeList;
		public Hashtable bucketList;
		private HashDictionary hashDict;
		private HashCreator hashCreated;

		private ILogger logger;

		public GUI()
		{
			Shown += new EventHandler(GUI_Shown);
			changeList = new Hashtable();
			origNamesList = new Hashtable();
			editNode = false;
			nodeChangeList = new Hashtable();
			bucketList = new Hashtable();
			hashDict = new HashDictionary();
			hashCreated = new HashCreator(hashDict, Hasher.HasherType.TOR);
			InitializeComponent();
			logger = new TextBoxLogger(textLog);
			replacer = new FileReplacer(logger);
		}

		private void GUI_Shown(object sender, EventArgs e)
		{

			changeList = new Hashtable();
			if (!Directory.Exists("backup"))
				Directory.CreateDirectory("backup");
			if (!Directory.Exists("files"))
				Directory.CreateDirectory("files");
			if (!Directory.Exists("extracted"))
				Directory.CreateDirectory("extracted");
			if (!File.Exists("settings.txt"))
				File.WriteAllText("settings.txt", "");
			string[] settingsLines = File.ReadAllLines("settings.txt");
			for (int index = 0; index < settingsLines.Length; index++)
			{
				string[] currentLine = settingsLines[index].Split(' ');
				if (currentLine.Length != 0)
				{
					string replaceOp = currentLine[0].ToLower();
					if (replaceOp == "replace")
					{
						if (currentLine.Length >= 3)
						{
							listChange.Items.Add("Replace " + currentLine[1] + " by " + currentLine[2]);
							if (!changeList.ContainsKey(Helpers.FileNameToHash(currentLine[1])))
							{
								changeList.Add(Helpers.FileNameToHash(currentLine[1]), currentLine[2]);
								origNamesList.Add(Helpers.FileNameToHash(currentLine[1]), currentLine[1]);
							}
						}
					}
					else if (replaceOp == "replacehash")
					{
					}
					else if (replaceOp == "replacenode" && currentLine.Length >= 3)
					{
						listChange.Items.Add("Replace Node " + currentLine[1] + " by " + currentLine[2]);
						editNode = true;
						if (!nodeChangeList.ContainsKey(currentLine[1]))
							nodeChangeList.Add(currentLine[1], currentLine[2]);
					}
				}
			}
			// TODO constant
			for (int i = 0; i < 997; i++)
			{
				bucketList.Add(Helpers.FileNameToHash("/resources/systemgenerated/buckets/" + i.ToString() + ".bkt"), true);
			}
			if (!File.Exists("installfolder.txt"))
				File.WriteAllText("installfolder.txt", "");
			string dirPath = File.ReadAllText("installfolder.txt");
			if (Directory.Exists(dirPath))
			{
				textInstallationFolder.Text = dirPath;
			}
		}
		private List<string> GetTorFileList()
		{
			List<string> files = Directory.GetFiles(textInstallationFolder.Text + "\\Assets", "swtor_*.tor", SearchOption.TopDirectoryOnly).ToList();
			files.Add(textInstallationFolder.Text + "\\swtor\\retailclient\\main_gfx_1.tor");

			return files;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new()
			{
				Description = "Please select the folder where you have SWTOR installed."
			};
			if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
			{
				// triggers textInstallationFolder_TextChanged
				textInstallationFolder.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void btnChangeFiles_Click(object sender, EventArgs e)
		{
			List<string> files = GetTorFileList();
			progressBar.Maximum = files.Count;
			Enabled = false;
			for (int index = 0; index < files.Count; index++)
			{
				progressBar.Value = index;
				Application.DoEvents();
				string str = files[index].Substring(checked(files[index].LastIndexOf("\\") + 1));
				if (!(radioEnvPTS.Checked & !str.StartsWith("swtor_test_")) && !(radioEnvLive.Checked & str.StartsWith("swtor_test_")))
					try
					{
						replacer.LoadArchiveReplaceFiles(files[index], chkBackup.Checked, editNode, changeList, origNamesList, nodeChangeList, bucketList);
					}
					catch (Exception ex)
					{
						logger.Log(ex.Message);
					}
			}
			if (false)
			{
				// TODO
				logger.Log("Verify");
			}
			else
			{
				logger.Log("Finished editing files!");
			}
			progressBar.Value = 0;
			Enabled = true;
		}

		private void btnRestoreBackup_Click(object sender, EventArgs e)
		{
			string[] files = Directory.GetFiles("backup", "*.tor", SearchOption.TopDirectoryOnly);
			progressBar.Maximum = files.Length;
			Enabled = false;
			for (int index = 0; index < files.Length; index++)
			{
				progressBar.Value = index;
				Application.DoEvents();
				string fileName = files[index].Substring(files[index].LastIndexOf("\\") + 1);
				string targetPath = textInstallationFolder.Text + "\\Assets\\" + fileName;
				if (fileName.Equals("main_gfx_1.tor", StringComparison.OrdinalIgnoreCase))
				{
					targetPath = textInstallationFolder.Text + "\\swtor\\retailclient\\" + fileName;
				}
				File.Copy(files[index], targetPath, true);
				File.Delete(files[index]);
			}
			logger.Log("Finished restoring backup!");
			progressBar.Value = 0;
			Enabled = true;
		}

		private void textInstallationFolder_TextChanged(object sender, EventArgs e)
		{
			File.WriteAllText("installfolder.txt", textInstallationFolder.Text);
		}

		private void btnExtractFile_Click(object sender, EventArgs e)
		{
			// TODO maybe remove Visual Basic element
			string fileName = Interaction.InputBox("Please enter the file that should be extracted", "Extract a file");
			if (fileName == "")
				return;

			ulong hash = Helpers.FileNameToHash(fileName.ToLower());
			List<string> files = GetTorFileList();
			progressBar.Maximum = files.Count;
			Enabled = false;

			byte[] extractedData = replacer.ExtractFile(hash, files, radioEnvPTS.Checked, radioEnvLive.Checked);

			if (extractedData != null)
			{
				//string sha1 = Convert.ToHexString(SHA1.HashData(extractedData));
				string outputPath = "extracted\\" + fileName.Substring(fileName.LastIndexOf("/") + 1);
				File.WriteAllBytes(outputPath, extractedData);
				logger.Log("The file " + fileName + " was successfully extracted!\n"
					//+ sha1
					);
			}
			else
			{
				logger.Log("The file " + fileName + " could not be found.");
			}
			Enabled = true;
		}

		private void btnExtractNode_Click(object sender, EventArgs e)
		{
			// 1) Ask for the specific node to extract
			string nodeKey = Interaction.InputBox(
				"Please enter the full node path to extract",
				"Extract a node"
			);
			if (string.IsNullOrEmpty(nodeKey)) return;

			bool found = false;
			string assetsDir = Path.Combine(textInstallationFolder.Text, "Assets");
			foreach (var archive in Directory.GetFiles(assetsDir, "swtor_*main_global_1.tor"))
			{
				if (found) break;

				using var fs = new FileStream(archive, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using var br = new BinaryReader(fs);

				// 2) Validate .tor v6 header
				if (br.ReadByte() != (byte)'M' || br.ReadByte() != (byte)'Y' ||
					br.ReadByte() != (byte)'P' || br.ReadByte() != 0 ||
					br.ReadUInt32() != 6U)
				{
					continue;
				}

				// 3) Build file-table lists
				var tableOffsets = new List<ulong> { 16UL };
				var tableCaps = new List<uint> { br.ReadUInt32() };
				ulong nextPtr = br.ReadUInt64();
				while (nextPtr != 0 && nextPtr < (ulong)fs.Length)
				{
					fs.Seek((long)nextPtr, SeekOrigin.Begin);
					uint capN = br.ReadUInt32();
					ulong follow = br.ReadUInt64();
					tableOffsets.Add(nextPtr);
					tableCaps.Add(capN);
					nextPtr = follow;
				}

				// 4) Walk every bucket entry
				for (int t = 0; t < tableOffsets.Count && !found; t++)
				{
					ulong tblOff = tableOffsets[t];
					uint tblCap = tableCaps[t];

					for (uint idx = 0; idx < tblCap && !found; idx++)
					{
						long metaPos = (long)tblOff + 12 + idx * 34;
						if (metaPos + 34 > fs.Length) break;

						fs.Seek(metaPos, SeekOrigin.Begin);
						ulong dataOff = br.ReadUInt64();
						uint metaSize = br.ReadUInt32();
						uint compSize = br.ReadUInt32();
						br.ReadUInt32();            // uncomprSize
						ulong fileId = br.ReadUInt64();
						br.ReadUInt32();            // CRC32
						br.ReadUInt16();            // compType

						if (!bucketList.ContainsKey(fileId))
							continue;


						// 5) Decompress the .bkt payload
						long bucketPos = (long)dataOff + metaSize;
						if (bucketPos + compSize > fs.Length)
						{
							continue;
						}
						fs.Seek(bucketPos, SeekOrigin.Begin);
						byte[] compBkt = br.ReadBytes((int)compSize);
						byte[] bucketData;
						try
						{
							bucketData = new Decompressor().Unwrap(compBkt).ToArray();
						}
						catch (Exception ex)
						{
							continue;
						}

						// 6) Parse PBUK + first (empty) DBLB
						using var ms = new MemoryStream(bucketData);
						using var dr = new BinaryReader(ms);

						ms.Position = 8;
						uint firstLen = dr.ReadUInt32();
						ms.Position += firstLen;  // skip exactly skipLen bytes :contentReference[oaicite:0]{index=0}

						// 7) Scan remaining DBLB sections
						while (!found && ms.Position + 4 <= bucketData.Length)
						{
							long sectionLenPos = ms.Position;
							uint sectionLen = dr.ReadUInt32();
							long sectionStart = ms.Position;
							long sectionEnd = Math.Min(sectionStart + sectionLen, bucketData.Length);

							dr.ReadUInt32();          // magic "DBLB"
							uint version = dr.ReadUInt32();

							// 8) Walk every entry in this section
							long entryPos = sectionStart + 8;  // skip magic+ver
							while (!found && entryPos + 4 <= sectionEnd)
							{
								ms.Position = entryPos;
								uint entryLen = dr.ReadUInt32();
								if (entryLen == 0) break;

								long entryStart = entryPos;
								ms.Position = entryStart + 4;

								// Read version‐dependent header to get dataOffset
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
								dr.ReadUInt16();      // descOff

								// 9) Read the inlineKey
								string inlineKey = Helpers.ReadStringAt(
									bucketData,
									(int)(entryStart + nameOff)
								);

								// 10) If it’s our target, extract it
								if (inlineKey.Equals(nodeKey, StringComparison.Ordinal))
								{

									// Pull out the compressed node blob
									int blobPos = (int)(entryStart + dataOffset);
									int blobLen = (int)(entryLen - dataOffset);
									byte[] blob = bucketData.AsSpan(blobPos, blobLen).ToArray();

									// Decompress DEFLATE vs ZSTD
									byte[] nodeData = null;
									bool ok = false;
									if (blobLen >= 2 && blob[0] == 0x78 && blob[1] == 0x9C)
									{
										using var def = new DeflateStream(
											new MemoryStream(blob),
											CompressionMode.Decompress
										);
										using var outMs = new MemoryStream();
										def.CopyTo(outMs);
										nodeData = outMs.ToArray();
										ok = true;
									}
									if (!ok)
									{
										nodeData = new Decompressor()
												   .Unwrap(blob)
												   .ToArray();
										ok = true;
									}

									if (ok && nodeData != null)
									{
										Directory.CreateDirectory("extracted");
										string safe = inlineKey
											.Replace("/", "_")
											.Replace("\\", "_");
										string outPath = Path.Combine(
											"extracted",
											safe + ".node"
										);
										File.WriteAllBytes(outPath, nodeData);
									}

									found = true;
									break;  // stop entry loop
								}

								// advance to next entry (pad to 8 bytes) :contentReference[oaicite:1]{index=1}
								long rel = entryStart - sectionStart;
								long padded = rel + entryLen + 7 & ~7L;
								entryPos = sectionStart + padded;
							}

							// move to the next section
							ms.Position = sectionStart + sectionLen;
						}
					}
				}
			}

			// 11) Report result
			if (found)
				logger.Log($"Extracted node \"{nodeKey}\".");
			else
				logger.Log($"Could not find node \"{nodeKey}\".");
		}
	}
}
