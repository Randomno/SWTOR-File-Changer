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
using System.Collections;
using ZstdSharp;
using System.IO.Compression;

namespace FileChanger
{
	// TODO don't define this here
	public enum Env
	{
		Live,
		PTS
	}
	public partial class GUI : Form
	{

		public Hashtable changeList;
		public Hashtable origNamesList;
		public bool editNode;
		public Hashtable nodeChangeList;
		public Hashtable bucketList;

		public Config config;

		private Env env;

		private FileSystemWatcher settingsWatcher;

		private TextBoxLogger logger;
		private FileReplacer replacer;

		private readonly string FILES_DIR = "files";
		private readonly int BUCKET_COUNT = 997; // todo make this configurable?

		public GUI()
		{
			Shown += new EventHandler(GUI_Shown);
			changeList = new Hashtable();
			origNamesList = new Hashtable();
			editNode = false;
			nodeChangeList = new Hashtable();
			bucketList = new Hashtable();
			InitializeComponent();
			logger = new TextBoxLogger(textLog);
			replacer = new FileReplacer(logger);

			config = new Config();
		}

		private void ReportProgress(int value)
		{
			if (progressBar.InvokeRequired)
			{
				// threading stuff again
				progressBar.BeginInvoke(new Action(() => ReportProgress(value)));
				return;
			}
			progressBar.Value = value;
			progressBar.Update();
		}

		private void GUI_Shown(object sender, EventArgs e)
		{
			if (!Directory.Exists("backup"))
				Directory.CreateDirectory("backup");
			if (!Directory.Exists(FILES_DIR))
				Directory.CreateDirectory(FILES_DIR);
			if (!Directory.Exists("extracted"))
				Directory.CreateDirectory("extracted");
			if (!File.Exists("settings.txt"))
				File.WriteAllText("settings.txt", "");
			ParseSettingsOld();
			//ParseSettings();
			// TODO constant
			for (int i = 0; i < BUCKET_COUNT; i++)
			{
				bucketList.Add(Helpers.FileNameToHash("/resources/systemgenerated/buckets/" + i.ToString() + ".bkt"), true);
			}
			for (int i = 0; i < BUCKET_COUNT; i++)
			{
				config.bucketList.Add(Helpers.FileNameToHash("/resources/systemgenerated/buckets/" + i.ToString() + ".bkt"));
			}
			if (!File.Exists("installfolder.txt"))
				File.WriteAllText("installfolder.txt", "");
			string dirPath = File.ReadAllText("installfolder.txt");
			if (Directory.Exists(dirPath))
			{
				textInstallationFolder.Text = dirPath;
			}
			settingsWatcher = new(Directory.GetCurrentDirectory())
			{
				Filter = "settings.txt",
				EnableRaisingEvents = true
			};
			settingsWatcher.Changed += OnChanged;
		}
		private void ParseSettings()
		{
			config.changeList.Clear();
			config.nodeChangeList.Clear();
			listChange.Items.Clear();
			string[] settingsLines = File.ReadAllLines("settings.txt");
			for (int index = 0; index < settingsLines.Length; index++)
			{
				string[] currentLine = settingsLines[index].Split(' ');
				if (currentLine.Length >=3)
				{
					string replaceOp = currentLine[0].ToLower();
					if (replaceOp == "replace")
					{
						listChange.Items.Add("Replace " + currentLine[1] + " by " + currentLine[2]);
						if (!config.changeList.ContainsKey(currentLine[1]))
						{
							config.changeList.Add(currentLine[1], FILES_DIR + currentLine[2]);
						}
					}
					else if (replaceOp == "replacenode")
					{
						listChange.Items.Add("Replace Node " + currentLine[1] + " by " + currentLine[2]);
						if (!config.nodeChangeList.ContainsKey(currentLine[1]))
							config.nodeChangeList.Add(currentLine[1], currentLine[2]);
					}
				}
			}
		}
		private void ParseSettingsOld()
		{
			changeList.Clear();
			origNamesList.Clear();
			editNode = false;
			nodeChangeList.Clear();
			listChange.Items.Clear();
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
		}
		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed)
			{
				return;
			}
			ParseSettingsOld();
		}
		private List<string> GetTorFileList()
		{
			if (!Directory.Exists(textInstallationFolder.Text + "\\Assets"))
			{
				logger.Error("Assets folder not found in selected directory!");
				return null;
			}
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
			if (files == null) return;
			progressBar.Maximum = files.Count;
			Enabled = false;
			for (int index = 0; index < files.Count; index++)
			{
				progressBar.Value = index;
				Application.DoEvents();
				string str = files[index].Substring(checked(files[index].LastIndexOf("\\") + 1));
				if (!(radioEnvPTS.Checked & !str.StartsWith("swtor_test_")) && !(radioEnvLive.Checked & str.StartsWith("swtor_test_")))
					replacer.LoadArchiveReplaceFiles(files[index], chkBackup.Checked, editNode, changeList, origNamesList, nodeChangeList, bucketList);
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
			replacer.RestoreBackup(files, textInstallationFolder.Text, new Progress<int>(ReportProgress));
			progressBar.Value = 0;
			Enabled = true;
		}

		private void textInstallationFolder_TextChanged(object sender, EventArgs e)
		{
			File.WriteAllText("installfolder.txt", textInstallationFolder.Text);
		}

		private async void btnExtractFile_Click(object sender, EventArgs e)
		{
			// TODO maybe remove Visual Basic element
			string fileName = Interaction.InputBox("Please enter the file that should be extracted", "Extract a file");
			if (fileName == "")
				return;

			List<string> torFiles = GetTorFileList();
			progressBar.Maximum = torFiles.Count;
			Enabled = false;

			byte[] extractedData = await Task.Run(() =>
				replacer.ExtractFile(fileName, torFiles, env, new Progress<int>(ReportProgress)));

			if (extractedData != null)
			{
				string outputPath = "extracted\\" + fileName.Substring(fileName.LastIndexOf("/") + 1);
				File.WriteAllBytes(outputPath, extractedData);
				logger.Log("The file " + fileName + " was successfully extracted!");
			}
			else
			{
				logger.Error("The file " + fileName + " could not be found.");
			}
			progressBar.Value = 0;
			Enabled = true;
		}

		private async void btnExtractNode_Click(object sender, EventArgs e)
		{
			// 1) Ask for the specific node to extract
			string nodeKey = Interaction.InputBox(
				"Please enter the full node path to extract",
				"Extract a node"
			);
			if (string.IsNullOrEmpty(nodeKey)) return;

			string assetsDir = Path.Combine(textInstallationFolder.Text, "Assets");
			List<string> torFiles = Directory.GetFiles(assetsDir, "swtor_*main_global_1.tor").ToList();

			// TODO progress bar
			Enabled = false;
			byte[] extractedData = await Task.Run(() =>
				replacer.ExtractNode(nodeKey, torFiles, bucketList, new Progress<int>(ReportProgress)));

			if (extractedData != null)
			{

				string safe = nodeKey
					.Replace("/", "_")
					.Replace("\\", "_");
				string outputPath = Path.Combine(
					"extracted",
					safe + ".node"
				);
				File.WriteAllBytes(outputPath, extractedData);
				logger.Log($"Extracted node \"{nodeKey}\".");
			}
			else
			{
				logger.Log($"Could not find node \"{nodeKey}\".");
			}
			progressBar.Value = 0;
			Enabled = true;
		}

		private void radioEnvLive_CheckedChanged(object sender, EventArgs e)
		{
			if (radioEnvLive.Checked)
			{
				env = Env.Live;
			}
		}

		private void radioEnvPTS_CheckedChanged(object sender, EventArgs e)
		{
			if (radioEnvPTS.Checked)
			{
				env = Env.PTS;
			}
		}

		private void btnSettings_Click(object sender, EventArgs e)
		{

			if (!File.Exists("settings.txt"))
				File.WriteAllText("settings.txt", "");
			new Process
			{
				StartInfo = new ProcessStartInfo("settings.txt")
				{
					UseShellExecute = true
				}
			}.Start();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			List<string> files = GetTorFileList();
			if (files == null) return;
			progressBar.Maximum = files.Count;
			Enabled = false;
			for (int index = 0; index < files.Count; index++)
			{
				progressBar.Value = index;
				Application.DoEvents();
				string str = files[index].Substring(checked(files[index].LastIndexOf("\\") + 1));
				if (!(radioEnvPTS.Checked & !str.StartsWith("swtor_test_")) && !(radioEnvLive.Checked & str.StartsWith("swtor_test_")))
					replacer.LoadArchiveReplaceFilesTest(files[index], chkBackup.Checked, editNode, changeList, origNamesList, nodeChangeList, bucketList);
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

		private void button2_Click(object sender, EventArgs e)
		{
			replacer.Replace(config);
		}
	}
}
