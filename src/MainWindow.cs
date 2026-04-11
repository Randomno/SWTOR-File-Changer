using Force.Crc32;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using nsHashCreator;
using nsHashDictionary;
using nsHasherFunctions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using ZstdSharp;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

#nullable disable
namespace SWTOR_File_Changer
{
    [DesignerGenerated]
    public class MainWindow : Form
    {
        private FileReplacer replacer = new();
        /* I tried using this to address CA1416 but it doesn't seem to work
        
		[System.Runtime.Versioning.SupportedOSPlatform("windows")]
        */

		private static List<WeakReference> __ENCList = new List<WeakReference>();
        private IContainer components;
        [AccessedThroughProperty("btnChangeFiles")]
        private Button _btnChangeFiles;
        [AccessedThroughProperty("btnRestoreBackup")]
        private Button _btnRestoreBackup;
        [AccessedThroughProperty("txtInstallationFolder")]
        private TextBox _txtInstallationFolder;
        [AccessedThroughProperty("btnBrowseInstallation")]
        private Button _btnBrowseInstallation;
        [AccessedThroughProperty("ListBox1")]
        private ListBox _ListBox1;
        [AccessedThroughProperty("FolderBrowserDialog1")]
        private FolderBrowserDialog _FolderBrowserDialog1;
        [AccessedThroughProperty("chkBackup")]
        private CheckBox _chkBackup;
		[AccessedThroughProperty("Verify")]
		private CheckBox _Verify;
		[AccessedThroughProperty("Label1")]
        private Label _Label1;
        [AccessedThroughProperty("Label2")]
        private Label _Label2;
        [AccessedThroughProperty("radEnvironmentPTS")]
        private RadioButton _radEnvironmentPTS;
        [AccessedThroughProperty("radEnvironmentLive")]
        private RadioButton _radEnvironmentLive;
        [AccessedThroughProperty("btnExtractFile")]
        private Button _btnExtractFile;
        [AccessedThroughProperty("ProgressBar1")]
        private ProgressBar _ProgressBar1;
        [AccessedThroughProperty("btnExtractNode")]
        private Button _btnExtractNode;
        public Hashtable changeList;
        public Hashtable origNamesList;
        public bool editNode;
        public Hashtable nodeChangeList;
        public Hashtable bucketList;
        private HashDictionary hashDict;
        private HashCreator hashCreated;

        [DebuggerNonUserCode]
        static MainWindow()
        {
        }

        public MainWindow()
        {
			Shown += new EventHandler(MainWindow_Shown);
			__ENCAddToList(this);
            changeList = new Hashtable();
            origNamesList = new Hashtable();
            editNode = false;
            nodeChangeList = new Hashtable();
            bucketList = new Hashtable();
            hashDict = new HashDictionary();
            hashCreated = new HashCreator(hashDict, Hasher.HasherType.TOR);
            InitializeComponent();
        }

        [DebuggerNonUserCode]
        private static void __ENCAddToList(object value)
        {
            lock (__ENCList)
            {
                if (__ENCList.Count == __ENCList.Capacity)
                {
                    int index1 = 0;
                    int num = checked(__ENCList.Count - 1);
                    for (int index2 = 0; index2 <= num; index2++)
                    {
                        if (__ENCList[index2].IsAlive)
                        {
                            if (index2 != index1)
                                __ENCList[index1] = __ENCList[index2];
                            checked { ++index1; }
                        }
                    }
                    __ENCList.RemoveRange(index1, checked(__ENCList.Count - index1));
                    __ENCList.Capacity = __ENCList.Count;
                }
                __ENCList.Add(new WeakReference(RuntimeHelpers.GetObjectValue(value)));
            }
        }

        [DebuggerNonUserCode]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing || components == null)
                    return;
                components.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [DebuggerStepThrough]
        private void InitializeComponent()
        {
            components = new Container();
            BtnChangeFiles = new Button();
            BtnRestoreBackup = new Button();
            TxtInstallationFolder = new TextBox();
            BtnBrowseInstallation = new Button();
            ListBox1 = new ListBox();
            FolderBrowserDialog1 = new FolderBrowserDialog();
            ChkBackup = new CheckBox();
			Verify = new CheckBox();
			Label1 = new Label();
            Label2 = new Label();
            RadEnvironmentPTS = new RadioButton();
            RadEnvironmentLive = new RadioButton();
            BtnExtractFile = new Button();
            ProgressBar1 = new ProgressBar();
            BtnExtractNode = new Button();
            SuspendLayout();
            BtnChangeFiles.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Button btnChangeFiles1 = BtnChangeFiles;
            Point point1 = new Point(778, 283);
            Point point2 = point1;
            btnChangeFiles1.Location = point2;
            BtnChangeFiles.Name = "btnChangeFiles";
            Button btnChangeFiles2 = BtnChangeFiles;
            Size size1 = new Size(111, 23);
            Size size2 = size1;
            btnChangeFiles2.Size = size2;
            BtnChangeFiles.TabIndex = 0;
            BtnChangeFiles.Text = "Start changing files";
            BtnChangeFiles.UseVisualStyleBackColor = true;
            BtnRestoreBackup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Button btnRestoreBackup1 = BtnRestoreBackup;
            point1 = new Point(12, 283);
            Point point3 = point1;
            btnRestoreBackup1.Location = point3;
            BtnRestoreBackup.Name = "btnRestoreBackup";
            Button btnRestoreBackup2 = BtnRestoreBackup;
            size1 = new Size(96, 23);
            Size size3 = size1;
            btnRestoreBackup2.Size = size3;
            BtnRestoreBackup.TabIndex = 1;
            BtnRestoreBackup.Text = "Restore backup";
            BtnRestoreBackup.UseVisualStyleBackColor = true;
            TxtInstallationFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TextBox installationFolder1 = TxtInstallationFolder;
            point1 = new Point(151, 12);
            Point point4 = point1;
            installationFolder1.Location = point4;
            TxtInstallationFolder.Name = "txtInstallationFolder";
            TxtInstallationFolder.ReadOnly = true;
            TextBox installationFolder2 = TxtInstallationFolder;
            size1 = new Size(658, 20);
            Size size4 = size1;
            installationFolder2.Size = size4;
            TxtInstallationFolder.TabIndex = 2;
            BtnBrowseInstallation.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Button browseInstallation1 = BtnBrowseInstallation;
            point1 = new Point(814, 9);
            Point point5 = point1;
            browseInstallation1.Location = point5;
            BtnBrowseInstallation.Name = "btnBrowseInstallation";
            Button browseInstallation2 = BtnBrowseInstallation;
            size1 = new Size(75, 23);
            Size size5 = size1;
            browseInstallation2.Size = size5;
            BtnBrowseInstallation.TabIndex = 3;
            BtnBrowseInstallation.Text = "Browse...";
            BtnBrowseInstallation.UseVisualStyleBackColor = true;
            ListBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ListBox1.FormattingEnabled = true;
            ListBox listBox1_1 = ListBox1;
            point1 = new Point(12, 65);
            Point point6 = point1;
            listBox1_1.Location = point6;
            ListBox1.Name = "ListBox1";
            ListBox listBox1_2 = ListBox1;
            size1 = new Size(877, 212);
            Size size6 = size1;
            listBox1_2.Size = size6;
            ListBox1.TabIndex = 4;
            ChkBackup.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ChkBackup.AutoSize = true;
            ChkBackup.Checked = true;
            ChkBackup.CheckState = CheckState.Checked;
            CheckBox chkBackup1 = ChkBackup;
            point1 = new Point(676, 287);
            Point point7 = point1;
            chkBackup1.Location = point7;
            ChkBackup.Name = "chkBackup";
            CheckBox chkBackup2 = ChkBackup;
            size1 = new Size(96, 17);
            Size size7 = size1;
            chkBackup2.Size = size7;
            ChkBackup.TabIndex = 5;
            ChkBackup.Text = "Create backup";
            ChkBackup.UseVisualStyleBackColor = true;


			Verify.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			Verify.AutoSize = true;
			Verify.Checked = false;
			Verify.CheckState = CheckState.Unchecked;
			CheckBox Verify1 = Verify;
            // hiding this off screen for now
			point1 = new Point(676, 377);
			Point point15 = point1;
			Verify1.Location = point15;
			Verify.Name = "Verify";
			CheckBox Verify2 = Verify;
			size1 = new Size(96, 37);
			Size size15 = size1;
			Verify2.Size = size15;
			Verify.TabIndex = 5;
			Verify.Text = "Verify replacement";
			Verify.UseVisualStyleBackColor = true;


			Label1.AutoSize = true;
            Label label1_1 = Label1;
            point1 = new Point(13, 46);
            Point point8 = point1;
            label1_1.Location = point8;
            Label1.Name = "Label1";
            Label label1_2 = Label1;
            size1 = new Size(110, 13);
            Size size8 = size1;
            label1_2.Size = size8;
            Label1.TabIndex = 6;
            Label1.Text = "List of files to change:";
            Label2.AutoSize = true;
            Label label2_1 = Label2;
            point1 = new Point(12, 15);
            Point point9 = point1;
            label2_1.Location = point9;
            Label2.Name = "Label2";
            Label label2_2 = Label2;
            size1 = new Size(132, 13);
            Size size9 = size1;
            label2_2.Size = size9;
            Label2.TabIndex = 7;
            Label2.Text = "SWTOR installation folder:";
            RadEnvironmentPTS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            RadEnvironmentPTS.AutoSize = true;
            RadioButton radEnvironmentPts1 = RadEnvironmentPTS;
            point1 = new Point(843, 46);
            Point point10 = point1;
            radEnvironmentPts1.Location = point10;
            RadEnvironmentPTS.Name = "radEnvironmentPTS";
            RadioButton radEnvironmentPts2 = RadEnvironmentPTS;
            size1 = new Size(46, 17);
            Size size10 = size1;
            radEnvironmentPts2.Size = size10;
            RadEnvironmentPTS.TabIndex = 8;
            RadEnvironmentPTS.Text = "PTS";
            RadEnvironmentPTS.UseVisualStyleBackColor = true;
            RadEnvironmentLive.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            RadEnvironmentLive.AutoSize = true;
            RadEnvironmentLive.Checked = true;
            RadioButton radEnvironmentLive1 = RadEnvironmentLive;
            point1 = new Point(764, 46);
            Point point11 = point1;
            radEnvironmentLive1.Location = point11;
            RadEnvironmentLive.Name = "radEnvironmentLive";
            RadioButton radEnvironmentLive2 = RadEnvironmentLive;
            size1 = new Size(73, 17);
            Size size11 = size1;
            radEnvironmentLive2.Size = size11;
            RadEnvironmentLive.TabIndex = 9;
            RadEnvironmentLive.TabStop = true;
            RadEnvironmentLive.Text = "Live client";
            RadEnvironmentLive.UseVisualStyleBackColor = true;
            BtnExtractFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Button btnExtractFile1 = BtnExtractFile;
            point1 = new Point(114, 283);
            Point point12 = point1;
            btnExtractFile1.Location = point12;
            BtnExtractFile.Name = "btnExtractFile";
            Button btnExtractFile2 = BtnExtractFile;
            size1 = new Size(66, 23);
            Size size12 = size1;
            btnExtractFile2.Size = size12;
            BtnExtractFile.TabIndex = 10;
            BtnExtractFile.Text = "Extract file";
            BtnExtractFile.UseVisualStyleBackColor = true;
            ProgressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ProgressBar progressBar1_1 = ProgressBar1;
            point1 = new Point(268, 283);
            Point point13 = point1;
            progressBar1_1.Location = point13;
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar progressBar1_2 = ProgressBar1;
            size1 = new Size(402, 21);
            Size size13 = size1;
            progressBar1_2.Size = size13;
            ProgressBar1.TabIndex = 11;
            BtnExtractNode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Button btnExtractNode1 = BtnExtractNode;
            point1 = new Point(186, 283);
            Point point14 = point1;
            btnExtractNode1.Location = point14;
            BtnExtractNode.Name = "btnExtractNode";
            Button btnExtractNode2 = BtnExtractNode;
            size1 = new Size(76, 23);
            Size size14 = size1;
            btnExtractNode2.Size = size14;
            BtnExtractNode.TabIndex = 12;
            BtnExtractNode.Text = "Extract node";
            BtnExtractNode.UseVisualStyleBackColor = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            size1 = new Size(901, 350);
            ClientSize = size1;
            Controls.Add(BtnExtractNode);
            Controls.Add(ProgressBar1);
            Controls.Add(BtnExtractFile);
            Controls.Add(RadEnvironmentLive);
            Controls.Add(RadEnvironmentPTS);
            Controls.Add(Label2);
            Controls.Add(Label1);
            Controls.Add(ChkBackup);
			Controls.Add(Verify);
			Controls.Add(ListBox1);
            Controls.Add(BtnBrowseInstallation);
            Controls.Add(TxtInstallationFolder);
            Controls.Add(BtnRestoreBackup);
            Controls.Add(BtnChangeFiles);
            Name = nameof(MainWindow);
            Text = "SWTOR File Changer, v3.0";
            ResumeLayout(false);
            PerformLayout();
        }

        internal virtual Button BtnChangeFiles
        {
            [DebuggerNonUserCode]
            get => _btnChangeFiles;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler eventHandler = new EventHandler(ChangeFiles);
                if (_btnChangeFiles != null)
                    _btnChangeFiles.Click -= eventHandler;
                _btnChangeFiles = value;
                if (_btnChangeFiles == null)
                    return;
                _btnChangeFiles.Click += eventHandler;
            }
        }

        internal virtual Button BtnRestoreBackup
        {
            [DebuggerNonUserCode]
            get => _btnRestoreBackup;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler eventHandler = new EventHandler(RestoreBackup);
                if (_btnRestoreBackup != null)
                    _btnRestoreBackup.Click -= eventHandler;
                _btnRestoreBackup = value;
                if (_btnRestoreBackup == null)
                    return;
                _btnRestoreBackup.Click += eventHandler;
            }
        }

        internal virtual TextBox TxtInstallationFolder
        {
            [DebuggerNonUserCode]
            get => _txtInstallationFolder;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _txtInstallationFolder = value;
            }
        }

        internal virtual Button BtnBrowseInstallation
        {
            [DebuggerNonUserCode]
            get => _btnBrowseInstallation;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler eventHandler = new EventHandler(ChangeInstallationFolder);
                if (_btnBrowseInstallation != null)
                    _btnBrowseInstallation.Click -= eventHandler;
                _btnBrowseInstallation = value;
                if (_btnBrowseInstallation == null)
                    return;
                _btnBrowseInstallation.Click += eventHandler;
            }
        }

        internal virtual ListBox ListBox1
        {
            [DebuggerNonUserCode]
            get => _ListBox1;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _ListBox1 = value;
            }
        }

        internal virtual FolderBrowserDialog FolderBrowserDialog1
        {
            [DebuggerNonUserCode]
            get => _FolderBrowserDialog1;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _FolderBrowserDialog1 = value;
            }
        }

        internal virtual CheckBox ChkBackup
        {
            [DebuggerNonUserCode]
            get => _chkBackup;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _chkBackup = value;
            }
		}

		internal virtual CheckBox Verify
		{
			[DebuggerNonUserCode]
			get => _Verify;
			[DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Verify = value;
			}
		}

		internal virtual Label Label1
        {
            [DebuggerNonUserCode]
            get => _Label1;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set => _Label1 = value;
        }

        internal virtual Label Label2
        {
            [DebuggerNonUserCode]
            get => _Label2;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set => _Label2 = value;
        }

        internal virtual RadioButton RadEnvironmentPTS
        {
            [DebuggerNonUserCode]
            get => _radEnvironmentPTS;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _radEnvironmentPTS = value;
            }
        }

        internal virtual RadioButton RadEnvironmentLive
        {
            [DebuggerNonUserCode]
            get => _radEnvironmentLive;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _radEnvironmentLive = value;
            }
        }

        internal virtual Button BtnExtractFile
        {
            [DebuggerNonUserCode]
            get => _btnExtractFile;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler eventHandler = new EventHandler(ExtractFileDialog);
                if (_btnExtractFile != null)
                    _btnExtractFile.Click -= eventHandler;
                _btnExtractFile = value;
                if (_btnExtractFile == null)
                    return;
                _btnExtractFile.Click += eventHandler;
            }
        }

        internal virtual ProgressBar ProgressBar1
        {
            [DebuggerNonUserCode]
            get => _ProgressBar1;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _ProgressBar1 = value;
            }
        }

        internal virtual Button BtnExtractNode
        {
            [DebuggerNonUserCode]
            get => _btnExtractNode;
            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler eventHandler = new EventHandler(ExtractNode);
                if (_btnExtractNode != null)
                    _btnExtractNode.Click -= eventHandler;
                _btnExtractNode = value;
                if (_btnExtractNode == null)
                    return;
                _btnExtractNode.Click += eventHandler;
            }
        }

        private void MainWindow_Shown(object sender, EventArgs e)
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
                            ListBox1.Items.Add("Replace " + currentLine[1] + " by " + currentLine[2]);
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
                        ListBox1.Items.Add("Replace Node " + currentLine[1] + " by " + currentLine[2]);
                        editNode = true;
                        if (!nodeChangeList.ContainsKey(currentLine[1]))
                            nodeChangeList.Add(currentLine[1], currentLine[2]);
                    }
                }
            }
            int num = 0;
            do
            {
                bucketList.Add(Helpers.FileNameToHash("/resources/systemgenerated/buckets/" + num.ToString() + ".bkt"), true);
                checked { ++num; }
            }
            while (num <= 996);
            if (!File.Exists("installfolder.txt"))
                File.WriteAllText("installfolder.txt", "");
            string dirPath = File.ReadAllText("installfolder.txt");
            if (!string.IsNullOrWhiteSpace(dirPath) && SetDefaultDirectory(dirPath) || SetDefaultDirectory("C:\\Program Files\\Electronic Arts\\BioWare\\Star Wars-The Old Republic") || SetDefaultDirectory("C:\\Program Files\\Electronic Arts\\BioWare\\Star Wars - The Old Republic") || SetDefaultDirectory("C:\\Program Files (x86)\\Electronic Arts\\BioWare\\Star Wars-The Old Republic") || SetDefaultDirectory("C:\\Program Files (x86)\\Electronic Arts\\BioWare\\Star Wars - The Old Republic") || SetDefaultDirectory("C:\\Programs\\ElectronicArts\\BioWare\\StarWars-The Old Republic"))
                return;
            SetDefaultDirectory(Environment.CurrentDirectory);
        }


        private List<string> GetTorFileList()
        {
			List<string> files = Directory.GetFiles(TxtInstallationFolder.Text + "\\Assets", "swtor_*.tor", SearchOption.TopDirectoryOnly).ToList();
			files.Add(TxtInstallationFolder.Text + "\\swtor\\retailclient\\main_gfx_1.tor");

            return files;
		}

        private bool SetDefaultDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                return false;
            TxtInstallationFolder.Text = dirPath;
            FolderBrowserDialog1.SelectedPath = dirPath;
            File.WriteAllText("installfolder.txt", dirPath);
            return true;
        }

        private void ChangeInstallationFolder(object sender, EventArgs e)
        {
            FolderBrowserDialog1.Description = "Please select the folder where you have SWTOR installed.";
            FolderBrowserDialog1.ShowDialog(this);
            TxtInstallationFolder.Text = FolderBrowserDialog1.SelectedPath;
            File.WriteAllText("installfolder.txt", FolderBrowserDialog1.SelectedPath);
        }

        private void RestoreBackup(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles("backup", "*.tor", SearchOption.TopDirectoryOnly);
            ProgressBar1.Maximum = files.Length;
            Enabled = false;
            int num1 = checked(files.Length - 1);
            for (int index = 0; index <= num1; index++)
            {
                ProgressBar1.Value = index;
                Application.DoEvents();
                string fileName = files[index].Substring(checked(files[index].LastIndexOf("\\") + 1));
                string targetPath = TxtInstallationFolder.Text + "\\Assets\\" + fileName;
                if (fileName.Equals("main_gfx_1.tor", StringComparison.OrdinalIgnoreCase))
                {
                    targetPath = TxtInstallationFolder.Text + "\\swtor\\retailclient\\" + fileName;
                }
                File.Copy(files[index], targetPath, true);
                File.Delete(files[index]);
            }
            MessageBox.Show("Finished restoring backup!");
            ProgressBar1.Value = 0;
            Enabled = true;
        }

        private void ChangeFiles(object sender, EventArgs e)
        {
            List<string> files = GetTorFileList();
            ProgressBar1.Maximum = files.Count;
            Enabled = false;
            for (int index = 0; index < files.Count; index++)
            {
                ProgressBar1.Value = index;
                Application.DoEvents();
                string str = files[index].Substring(checked(files[index].LastIndexOf("\\") + 1));
                if (!(RadEnvironmentPTS.Checked & !str.StartsWith("swtor_test_")) && !(RadEnvironmentLive.Checked & str.StartsWith("swtor_test_")))
                    try
                    {
                        replacer.LoadArchiveReplaceFiles(files[index], ChkBackup.Checked, editNode, changeList, origNamesList, nodeChangeList, bucketList);
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message);
                    }
            }
            if (Verify.Checked)
            {
                // TODO
				MessageBox.Show("Verify");
			}
            else
			{
				MessageBox.Show("Finished editing files!");
			}
            ProgressBar1.Value = 0;
            Enabled = true;
        }

       
		private void ExtractFileDialog(object sender, EventArgs e)
        {
            string fileName = Interaction.InputBox("Please enter the file that should be extracted", "Extract a file");
            if (fileName == "")
                return;
            
            ulong hash = Helpers.FileNameToHash(fileName.ToLower());
            List<string> files = GetTorFileList();
            ProgressBar1.Maximum = files.Count;
            Enabled = false;
            
            byte[] extractedData = replacer.ExtractFile(hash, files, RadEnvironmentPTS.Checked, RadEnvironmentLive.Checked);
            
            if (extractedData != null)
			{
				string sha1 = Convert.ToHexString(SHA1.HashData(extractedData));
				string outputPath = "extracted\\" + fileName.Substring(checked(fileName.LastIndexOf("/") + 1));
				File.WriteAllBytes(outputPath, extractedData);
                MessageBox.Show("The file " + fileName + " was successfully extracted!\n" + sha1);
            }
            else
            {
                MessageBox.Show("The file " + fileName + " could not be found.");
            }
            Enabled = true;
        }

        private void ExtractNode(object sender, EventArgs e)
        {
            // 1) Ask for the specific node to extract
            string nodeKey = Interaction.InputBox(
                "Please enter the full node path to extract",
                "Extract a node"
            );
            if (string.IsNullOrEmpty(nodeKey)) return;

            bool found = false;
            string assetsDir = Path.Combine(TxtInstallationFolder.Text, "Assets");
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
                MessageBox.Show($"Extracted node \"{nodeKey}\".");
            else
                MessageBox.Show($"Could not find node \"{nodeKey}\".");
        }



    }
}