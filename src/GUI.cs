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
		private HashDictionary hashDict;
		private HashCreator hashCreated;
		private Env env;

		private TextBoxLogger logger;
		private FileReplacer replacer;

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

		private void ReportProgress(int value)
		{
			progressBar.Value = value;
			progressBar.Update();
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

		private void btnExtractFile_Click(object sender, EventArgs e)
		{
			// TODO maybe remove Visual Basic element
			string fileName = Interaction.InputBox("Please enter the file that should be extracted", "Extract a file");
			if (fileName == "")
				return;

			List<string> torFiles = GetTorFileList();
			progressBar.Maximum = torFiles.Count;
			Enabled = false;

			byte[] extractedData = replacer.ExtractFile(fileName, torFiles, env, new Progress<int>(ReportProgress));

			if (extractedData != null)
			{
				//string sha1 = Convert.ToHexString(SHA1.HashData(extractedData));
				string outputPath = "extracted\\" + fileName.Substring(fileName.LastIndexOf("/") + 1);
				File.WriteAllBytes(outputPath, extractedData);
				logger.Log("The file " + fileName + " was successfully extracted!");
			}
			else
			{
				logger.Log("The file " + fileName + " could not be found.");
			}
			progressBar.Value = 0;
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

			string assetsDir = Path.Combine(textInstallationFolder.Text, "Assets");
			List<string> torFiles = Directory.GetFiles(assetsDir, "swtor_*main_global_1.tor").ToList();

			replacer.ExtractNode(nodeKey, torFiles, bucketList);
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
	}
}
