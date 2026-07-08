namespace FileChanger
{
	partial class NodeGui
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
			txtFilePath = new System.Windows.Forms.TextBox();
			btnBrowse = new System.Windows.Forms.Button();
			btnOpen = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// txtFilePath
			// 
			txtFilePath.Location = new System.Drawing.Point(12, 13);
			txtFilePath.Name = "txtFilePath";
			txtFilePath.ReadOnly = true;
			txtFilePath.Size = new System.Drawing.Size(614, 23);
			txtFilePath.TabIndex = 0;
			// 
			// btnBrowse
			// 
			btnBrowse.Location = new System.Drawing.Point(632, 12);
			btnBrowse.Name = "btnBrowse";
			btnBrowse.Size = new System.Drawing.Size(75, 23);
			btnBrowse.TabIndex = 1;
			btnBrowse.Text = "Browse...";
			btnBrowse.UseVisualStyleBackColor = true;
			btnBrowse.Click += btnBrowse_Click;
			// 
			// btnOpen
			// 
			btnOpen.Location = new System.Drawing.Point(713, 12);
			btnOpen.Name = "btnOpen";
			btnOpen.Size = new System.Drawing.Size(75, 23);
			btnOpen.TabIndex = 2;
			btnOpen.Text = "Open";
			btnOpen.UseVisualStyleBackColor = true;
			btnOpen.Click += btnOpen_Click;
			// 
			// NodeGui
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(btnOpen);
			Controls.Add(btnBrowse);
			Controls.Add(txtFilePath);
			Name = "NodeGui";
			Text = "NodeEdit";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.TextBox txtFilePath;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnOpen;
	}
}