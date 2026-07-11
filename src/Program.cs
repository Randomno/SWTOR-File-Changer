using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChanger;

public enum LogLevel { Info, Warning, Error }
public interface ILogger
{
	void Log(object message, LogLevel level = LogLevel.Info);
	void Error(object message);
	void Warning(object message);
	void Debug(object message);
}

public class TextBoxLogger : ILogger
{
	private readonly RichTextBox _textBox;

	public TextBoxLogger(RichTextBox textBox)
	{
		_textBox = textBox;
	}

	public void Log(object message, LogLevel level = LogLevel.Info)
	{
		if (_textBox.InvokeRequired)
		{
			// required for threading stuff apparently
			_textBox.BeginInvoke(() => Log(message, level));
			return;
		}

		var color = level switch
		{
			LogLevel.Error => System.Drawing.Color.Red,
			LogLevel.Warning => System.Drawing.Color.DarkOrange,
			_ => _textBox.ForeColor,
		};

		_textBox.SelectionColor = color;
		_textBox.AppendText(message + Environment.NewLine);
		_textBox.SelectionColor = _textBox.ForeColor;

	}
	public void Error(object message)
	{
		Log(message, LogLevel.Error);
	}
	public void Warning(object message)
	{
		Log(message, LogLevel.Warning);
	}
	public void Debug(object message)
	{
		if (Program.debug)
		{
			Log(message, LogLevel.Info);
		}
	}

}

public class ConsoleLogger : ILogger
{
	public void Log(object message, LogLevel level = LogLevel.Info)
	{
		Console.WriteLine(message);
	}
	public void Error(object message)
	{
		Log(message, LogLevel.Error);
	}
	public void Warning(object message)
	{
		Log(message, LogLevel.Warning);
	}
	public void Debug(object message)
	{
		if (Program.debug)
		{
			Log(message, LogLevel.Info);
		}
	}
}

internal static class Program
{
	public static bool debug = false;
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
#if DEBUG
		debug = true;
#endif
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new GUI());
	}
}
