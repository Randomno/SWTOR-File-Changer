using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileChanger;

public partial class NodeGui : Form
{
	readonly NodeEditor nodeEditor;

	// todo fix logging mess
	private readonly ILogger logger;

	public NodeGui(ILogger logger = null)
	{
		InitializeComponent();
		this.logger = logger ?? new ConsoleLogger();
		nodeEditor = new(logger);
	}

	private void btnBrowse_Click(object sender, EventArgs e)
	{
		OpenFileDialog dialog = new();
		dialog.Title = "Choose node file";

		if (dialog.ShowDialog() == DialogResult.OK)
			txtFilePath.Text = dialog.FileName;
	}

	private void btnOpen_Click(object sender, EventArgs e)
	{
		string filePath = txtFilePath.Text;

		// todo handle better
		if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
			return;

		ParseNode(filePath);
	}

	private void ParseNode(string filePath)
	{
		FileStream fileStream = new(filePath, FileMode.Open);
		BinaryReader br = new(fileStream);

		int rootFields = (int)br.ReadVarInt();
		logger.Log($"root fields: {rootFields}");

		int storedRootFields = (int)br.ReadVarInt();
		logger.Log($"stored root fields: {storedRootFields}");

		long prevId = 0;
		int x = 20;
		int y = 50;

		for (int i = 0; i < storedRootFields; i++)
		{
			long fieldId = br.ReadVarInt() + prevId;
			AddTextBox(fieldId.ToString(), x, y);
			x += 210;

			int domType = br.ReadByte();
			AddTextBox(domType.ToString(), x, y);
			x += 210;

			object value = 0;

			switch (domType)
			{
				case 2:
				case 5:
					value = br.ReadVarInt();
					break;
				case 6:
					value = br.ReadString();
					break;
			}

			AddTextBox(value.ToString(), x, y);

			//logger.Log($"{fieldId} {value}");
			prevId = fieldId;
			x = 20;
			y += 40;
		}
	}

	private void AddTextBox(string text, int x, int y)
	{
		TextBox textBox = new()
		{
			Text = text,
			Location = new Point(x, y),
			Size = new Size(200, 30)
		};

		Controls.Add(textBox);
	}
}
