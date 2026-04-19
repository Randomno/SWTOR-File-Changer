namespace FileChanger
{
	partial class GUI
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			btnChangeFiles = new System.Windows.Forms.Button();
			label1 = new System.Windows.Forms.Label();
			textInstallationFolder = new System.Windows.Forms.TextBox();
			btnBrowse = new System.Windows.Forms.Button();
			label2 = new System.Windows.Forms.Label();
			listChange = new System.Windows.Forms.ListBox();
			btnRestoreBackup = new System.Windows.Forms.Button();
			btnExtractFile = new System.Windows.Forms.Button();
			btnExtractNode = new System.Windows.Forms.Button();
			progressBar = new System.Windows.Forms.ProgressBar();
			textLog = new System.Windows.Forms.TextBox();
			chkBackup = new System.Windows.Forms.CheckBox();
			radioEnvPTS = new System.Windows.Forms.RadioButton();
			radioEnvLive = new System.Windows.Forms.RadioButton();
			btnSettings = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// btnChangeFiles
			// 
			btnChangeFiles.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			btnChangeFiles.Location = new System.Drawing.Point(727, 372);
			btnChangeFiles.Name = "btnChangeFiles";
			btnChangeFiles.Size = new System.Drawing.Size(119, 29);
			btnChangeFiles.TabIndex = 5;
			btnChangeFiles.Text = "Start changing files";
			btnChangeFiles.UseVisualStyleBackColor = true;
			btnChangeFiles.Click += btnChangeFiles_Click;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 17);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(144, 15);
			label1.TabIndex = 1;
			label1.Text = "SWTOR installation folder:";
			// 
			// textInstallationFolder
			// 
			textInstallationFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textInstallationFolder.Location = new System.Drawing.Point(162, 12);
			textInstallationFolder.Name = "textInstallationFolder";
			textInstallationFolder.ReadOnly = true;
			textInstallationFolder.Size = new System.Drawing.Size(603, 23);
			textInstallationFolder.TabIndex = 6;
			textInstallationFolder.TextChanged += textInstallationFolder_TextChanged;
			// 
			// btnBrowse
			// 
			btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			btnBrowse.Location = new System.Drawing.Point(771, 12);
			btnBrowse.Name = "btnBrowse";
			btnBrowse.Size = new System.Drawing.Size(75, 23);
			btnBrowse.TabIndex = 1;
			btnBrowse.Text = "Browse...";
			btnBrowse.UseVisualStyleBackColor = true;
			btnBrowse.Click += btnBrowse_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 54);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(122, 15);
			label2.TabIndex = 4;
			label2.Text = "List of files to change:";
			// 
			// listChange
			// 
			listChange.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listChange.FormattingEnabled = true;
			listChange.Location = new System.Drawing.Point(13, 72);
			listChange.Name = "listChange";
			listChange.Size = new System.Drawing.Size(834, 289);
			listChange.TabIndex = 5;
			listChange.TabStop = false;
			// 
			// btnRestoreBackup
			// 
			btnRestoreBackup.Location = new System.Drawing.Point(13, 372);
			btnRestoreBackup.Name = "btnRestoreBackup";
			btnRestoreBackup.Size = new System.Drawing.Size(111, 29);
			btnRestoreBackup.TabIndex = 2;
			btnRestoreBackup.Text = "Restore backup";
			btnRestoreBackup.UseVisualStyleBackColor = true;
			btnRestoreBackup.Click += btnRestoreBackup_Click;
			// 
			// btnExtractFile
			// 
			btnExtractFile.Location = new System.Drawing.Point(130, 372);
			btnExtractFile.Name = "btnExtractFile";
			btnExtractFile.Size = new System.Drawing.Size(75, 29);
			btnExtractFile.TabIndex = 3;
			btnExtractFile.Text = "Extract file";
			btnExtractFile.UseVisualStyleBackColor = true;
			btnExtractFile.Click += btnExtractFile_Click;
			// 
			// btnExtractNode
			// 
			btnExtractNode.Location = new System.Drawing.Point(211, 372);
			btnExtractNode.Name = "btnExtractNode";
			btnExtractNode.Size = new System.Drawing.Size(86, 29);
			btnExtractNode.TabIndex = 4;
			btnExtractNode.Text = "Extract node";
			btnExtractNode.UseVisualStyleBackColor = true;
			btnExtractNode.Click += btnExtractNode_Click;
			// 
			// progressBar
			// 
			progressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			progressBar.Location = new System.Drawing.Point(303, 372);
			progressBar.Name = "progressBar";
			progressBar.Size = new System.Drawing.Size(302, 29);
			progressBar.TabIndex = 9;
			// 
			// textLog
			// 
			textLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textLog.Location = new System.Drawing.Point(15, 409);
			textLog.Multiline = true;
			textLog.Name = "textLog";
			textLog.ReadOnly = true;
			textLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			textLog.Size = new System.Drawing.Size(831, 157);
			textLog.TabIndex = 10;
			textLog.TabStop = false;
			// 
			// chkBackup
			// 
			chkBackup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			chkBackup.AutoSize = true;
			chkBackup.Checked = true;
			chkBackup.CheckState = System.Windows.Forms.CheckState.Checked;
			chkBackup.Location = new System.Drawing.Point(619, 378);
			chkBackup.Name = "chkBackup";
			chkBackup.Size = new System.Drawing.Size(102, 19);
			chkBackup.TabIndex = 11;
			chkBackup.Text = "Create backup";
			chkBackup.UseVisualStyleBackColor = true;
			// 
			// radioEnvPTS
			// 
			radioEnvPTS.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			radioEnvPTS.AutoSize = true;
			radioEnvPTS.Location = new System.Drawing.Point(801, 47);
			radioEnvPTS.Name = "radioEnvPTS";
			radioEnvPTS.Size = new System.Drawing.Size(45, 19);
			radioEnvPTS.TabIndex = 12;
			radioEnvPTS.Text = "PTS";
			radioEnvPTS.UseVisualStyleBackColor = true;
			radioEnvPTS.CheckedChanged += radioEnvPTS_CheckedChanged;
			// 
			// radioEnvLive
			// 
			radioEnvLive.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			radioEnvLive.AutoSize = true;
			radioEnvLive.Checked = true;
			radioEnvLive.Location = new System.Drawing.Point(749, 47);
			radioEnvLive.Name = "radioEnvLive";
			radioEnvLive.Size = new System.Drawing.Size(46, 19);
			radioEnvLive.TabIndex = 13;
			radioEnvLive.TabStop = true;
			radioEnvLive.Text = "Live";
			radioEnvLive.UseVisualStyleBackColor = true;
			radioEnvLive.CheckedChanged += radioEnvLive_CheckedChanged;
			// 
			// btnSettings
			// 
			btnSettings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			btnSettings.Location = new System.Drawing.Point(643, 40);
			btnSettings.Name = "btnSettings";
			btnSettings.Size = new System.Drawing.Size(100, 29);
			btnSettings.TabIndex = 14;
			btnSettings.Text = "Open settings";
			btnSettings.UseVisualStyleBackColor = true;
			btnSettings.Click += btnSettings_Click;
			// 
			// GUI
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(858, 578);
			Controls.Add(btnSettings);
			Controls.Add(radioEnvLive);
			Controls.Add(radioEnvPTS);
			Controls.Add(chkBackup);
			Controls.Add(textLog);
			Controls.Add(progressBar);
			Controls.Add(btnExtractNode);
			Controls.Add(btnExtractFile);
			Controls.Add(btnRestoreBackup);
			Controls.Add(listChange);
			Controls.Add(label2);
			Controls.Add(btnBrowse);
			Controls.Add(textInstallationFolder);
			Controls.Add(label1);
			Controls.Add(btnChangeFiles);
			Name = "GUI";
			Text = "SWTOR File Changer";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Button btnChangeFiles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textInstallationFolder;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox listChange;
		private System.Windows.Forms.Button btnRestoreBackup;
		private System.Windows.Forms.Button btnExtractFile;
		private System.Windows.Forms.Button btnExtractNode;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.TextBox textLog;
		private System.Windows.Forms.CheckBox chkBackup;
		private System.Windows.Forms.RadioButton radioEnvPTS;
		private System.Windows.Forms.RadioButton radioEnvLive;
		private System.Windows.Forms.Button btnSettings;
	}
}