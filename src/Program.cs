using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChanger
{
	public interface ILogger
	{
		void Log(string message);
	}

	public class TextBoxLogger : ILogger
	{
		private TextBox _textBox;

		public TextBoxLogger(TextBox textBox)
		{
			_textBox = textBox;
		}

		public void Log(string message)
		{
			if (_textBox.InvokeRequired)
			{
				// required for threading stuff apparently
				_textBox.Invoke(() => _textBox.AppendText(message + Environment.NewLine));
			}
			else
			{
				_textBox.AppendText(message + Environment.NewLine);
			}
		}
	}

	public class ConsoleLogger : ILogger
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}
	}

	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new GUI());
		}
	}
}
