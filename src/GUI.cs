using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.VisualBasic;

namespace FileChanger;

// TODO don't define this here
public enum Env
{
	Live,
	PTS
}
public partial class GUI : Form
{
	public Config config;

	private Env env;

	private FileSystemWatcher settingsWatcher;

	private readonly TextBoxLogger logger;
	private readonly FileReplacer replacer;

	private readonly string FILES_DIR = "files";

	public GUI()
	{
		Shown += new EventHandler(GUI_Shown);
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
		ParseSettings();

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
		config.hashChangeList.Clear();
		config.nodeChangeList.Clear();
		GuiChangeList.Items.Clear();
		string[] settingsLines = File.ReadAllLines("settings.txt");
		for (int index = 0; index < settingsLines.Length; index++)
		{
			string[] currentLine = settingsLines[index].Split(' ');
			if (currentLine.Length >= 3)
			{
				string replaceOp = currentLine[0].ToLower();
				switch (replaceOp)
				{
					case "replace":
						GuiChangeList.Items.Add("Replace " + currentLine[1] + " with " + currentLine[2]);
						if (!config.changeList.ContainsKey(currentLine[1]))
							config.changeList.Add(currentLine[1], Path.Combine(FILES_DIR, currentLine[2]));
						break;
					case "replacehash":
						GuiChangeList.Items.Add("Replace " + currentLine[1] + " with " + currentLine[2]);
						var parts = currentLine[1].Split("_");
						ulong hash = Convert.ToUInt32(parts[0], 16) | ((ulong)Convert.ToUInt32(parts[1], 16) << 32);
						if (!config.hashChangeList.ContainsKey(hash))
							config.hashChangeList.Add(hash, Path.Combine(FILES_DIR, currentLine[2]));
						break;
					case "replacenode":
						GuiChangeList.Items.Add("Replace node " + currentLine[1] + " with " + currentLine[2]);
						if (!config.nodeChangeList.ContainsKey(currentLine[1]))
							config.nodeChangeList.Add(currentLine[1], Path.Combine(FILES_DIR, currentLine[2]));
						break;
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
		GuiChangeList.Invoke(new Action(ParseSettings));
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
			Description = "Select SWTOR installation folder"
		};
		if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
		{
			// triggers textInstallationFolder_TextChanged
			textInstallationFolder.Text = folderBrowserDialog.SelectedPath;
		}
	}

	private void btnRestoreBackup_Click(object sender, EventArgs e)
	{
		string[] files = Directory.GetFiles("backup", "*.tor", SearchOption.TopDirectoryOnly);
		progressBar.Maximum = files.Length;
		Enabled = false;
		replacer.RestoreBackup(files, textInstallationFolder.Text);
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
		string fileName = Interaction.InputBox("Enter the file that should be extracted", "Extract a file");
		if (fileName == "")
			return;

		config.torFiles = GetTorFileList();
		progressBar.Maximum = config.torFiles.Count;
		Enabled = false;

		byte[] extractedData = await Task.Run(() =>
			replacer.ExtractFile(config, fileName));

		if (extractedData != null)
		{
			string outputPath = "extracted\\" + fileName.Substring(fileName.LastIndexOf("/") + 1);
			File.WriteAllBytes(outputPath, extractedData);
			logger.Log($"Extracted file {fileName}");
		}
		else
		{
			logger.Error($"Could not find file {fileName}");
		}
		progressBar.Value = 0;
		Enabled = true;
	}

	private async void btnExtractNode_Click(object sender, EventArgs e)
	{
		// 1) Ask for the specific node to extract
		string nodeKey = Interaction.InputBox(
			"Enter the full node path to extract",
			"Extract a node"
		);
		if (string.IsNullOrEmpty(nodeKey)) return;

		string assetsDir = Path.Combine(textInstallationFolder.Text, "Assets");
		config.torFiles = Directory.GetFiles(assetsDir, "swtor_*main_global_1.tor").ToList();

		// TODO progress bar
		Enabled = false;
		byte[] extractedData = await Task.Run(() =>
			replacer.ExtractNode(config, nodeKey));

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
			logger.Log($"Extracted node {nodeKey}");
		}
		else
		{
			logger.Error($"Could not find node {nodeKey}");
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

	private void btnChangeFiles_Click(object sender, EventArgs e)
	{
		config.torFiles = GetTorFileList();
		config.createBackup = chkBackup.Checked;
		Enabled = false;
		replacer.Replace(config);
		Enabled = true;
		ParseSettings(); // to clear the change list
	}

	private void NodeEditor_Click(object sender, EventArgs e)
	{
		NodeGui nodeEditor = new(logger);
		nodeEditor.Show();
	}
}
