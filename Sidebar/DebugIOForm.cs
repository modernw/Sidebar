using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sidebar
{
	public partial class DebugIOForm: Form, IDebugOutput
	{
		private Queue<string> messageBuffer = new Queue<string> ();
		private readonly object bufferLock = new object ();
		private Dictionary<string, IDebugCommand> commands = new Dictionary<string, IDebugCommand> ();
		private const int MaxLines = 1000;
		private bool autoScroll = true;

		// 颜色定义
		private static readonly Color ColorNormal = Color.Black;
		private static readonly Color ColorError = Color.Red;
		private static readonly Color ColorWarning = Color.Orange;
		private static readonly Color ColorInfo = Color.Blue;
		private static readonly Color ColorCommand = Color.Green;

		public DebugIOForm ()
		{
			InitializeComponent ();

			// 初始化事件处理
			sendButton.Click += SendButton_Click;
			inputTextBox.KeyDown += InputTextBox_KeyDown;
			this.FormClosing += DebugOutputForm_FormClosing;

			// 注册默认命令
			RegisterDefaultCommands ();

			// 初始化
			outputTextBox.AppendText ("=== Debug Output Window ===\r\n");
			outputTextBox.AppendText ("Type 'help' to see available commands\r\n");
			outputTextBox.AppendText ("\r\n");
		}

		/// <summary>
		/// 输出普通消息
		/// </summary>
		public void WriteLine (string message)
		{
			WriteLineInternal (message, ColorNormal);
		}

		/// <summary>
		/// 输出格式化消息
		/// </summary>
		public void WriteLine (string format, params object [] args)
		{
			try
			{
				WriteLine (string.Format (format, args));
			}
			catch (Exception ex)
			{
				WriteLine ($"Format error: {ex.Message}");
			}
		}

		/// <summary>
		/// 输出错误消息
		/// </summary>
		public void WriteError (string message)
		{
			WriteLineInternal ($"[ERROR] {message}", ColorError);
		}

		/// <summary>
		/// 输出格式化错误消息
		/// </summary>
		public void WriteError (string format, params object [] args)
		{
			try
			{
				WriteError (string.Format (format, args));
			}
			catch (Exception ex)
			{
				WriteError ($"Format error: {ex.Message}");
			}
		}
		/// <summary>
		/// 输出警告消息
		/// </summary>
		public void WriteWarning (string message)
		{
			WriteLineInternal ($"[WARN] {message}", ColorWarning);
		}
		/// <summary>
		/// 输出格式化警告消息
		/// </summary>
		public void WriteWarning (string format, params object [] args)
		{
			try
			{
				WriteWarning (string.Format (format, args));
			}
			catch (Exception ex)
			{
				WriteWarning ($"Format error: {ex.Message}");
			}
		}
		/// <summary>
		/// 清空输出
		/// </summary>
		public void Clear ()
		{
			if (InvokeRequired)
			{
				Invoke (new Action (() => Clear ()));
				return;
			}
			outputTextBox.Clear ();
		}
		/// <summary>
		/// 内部写入方法
		/// </summary>
		private void WriteLineInternal (string message, Color color)
		{
			if (InvokeRequired)
			{
				Invoke (new Action (() => WriteLineInternal (message, color)));
				return;
			}

			try
			{
				// 时间戳
				string timestamp = DateTime.Now.ToString ("HH:mm:ss.fff");
				string output = $"[{timestamp}] {message}";

				// 限制行数
				int lineCount = outputTextBox.Lines.Length;
				if (lineCount > MaxLines)
				{
					// 删除前 100 行
					int removeLength = outputTextBox.Lines.Take (100).Sum (l => l.Length + 2);
					outputTextBox.Select (0, removeLength);
					outputTextBox.SelectedText = "";
				}

				// 添加文本
				outputTextBox.AppendText (output + "\r\n");

				// 设置颜色
				int startPos = outputTextBox.TextLength - output.Length - 2;
				int endPos = output.Length;
				outputTextBox.Select (startPos, endPos);
				outputTextBox.SelectionColor = color;
				outputTextBox.Select (outputTextBox.TextLength, 0);

				// 自动滚动到底部
				if (autoScroll)
				{
					outputTextBox.ScrollToCaret ();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine ($"DebugOutputForm error: {ex.Message}");
			}
		}
		/// <summary>
		/// 注册调试命令
		/// </summary>
		public void RegisterCommand (string name, IDebugCommand command)
		{
			if (string.IsNullOrWhiteSpace (name) || command == null)
				return;

			commands [name.ToLower ()] = command;
		}
		/// <summary>
		/// 注册默认命令
		/// </summary>
		private void RegisterDefaultCommands ()
		{
			RegisterCommand ("help", new HelpCommand (this));
			RegisterCommand ("clear", new ClearCommand (this));
			RegisterCommand ("time", new TimeCommand (this));
			RegisterCommand ("echo", new EchoCommand (this));
			RegisterCommand ("commands", new CommandsCommand (this));
		}
		/// <summary>
		/// 执行命令
		/// </summary>
		private void ExecuteCommand (string input)
		{
			if (string.IsNullOrWhiteSpace (input)) return;
			WriteLineInternal ($"> {input}", ColorCommand);
			string [] parts = input.Split (new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0) return;
			string commandName = parts [0].ToLower ();
			string [] args = parts.Skip (1).ToArray ();
			IDebugCommand command = null;
			if (commands.TryGetValue (commandName, out command))
			{
				try
				{
					if (!command.Execute (commandName, args))
					{
						WriteLine ($"Command '{commandName}' failed");
					}
				}
				catch (Exception ex)
				{
					WriteError ($"Exception executing command '{commandName}': {ex.Message}");
				}
			}
			else
			{
				WriteWarning ($"Unknown command: '{commandName}'. Type 'help' for available commands.");
			}
		}
		/// <summary>
		/// Send 按钮点击
		/// </summary>
		private void SendButton_Click (object sender, EventArgs e)
		{
			string input = inputTextBox.Text.Trim ();
			if (!string.IsNullOrEmpty (input))
			{
				ExecuteCommand (input);
				inputTextBox.Clear ();
				inputTextBox.Focus ();
			}
		}
		/// <summary>
		/// 输入框 KeyDown 事件（支持 Enter 键）
		/// </summary>
		private void InputTextBox_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				e.Handled = true;
				SendButton_Click (sendButton, EventArgs.Empty);
			}
			else if (e.KeyCode == Keys.Up)
			{
				e.Handled = true;
				// 可以在这里实现命令历史
			}
			else if (e.KeyCode == Keys.Down)
			{
				e.Handled = true;
				// 可以在这里实现命令历史
			}
		}
		private void DebugOutputForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			// 防止关闭，改为隐藏
			e.Cancel = true;
			this.Hide ();
		}
		/// <summary>
		/// 获取所有已注册的命令
		/// </summary>
		public IEnumerable<string> GetRegisteredCommands ()
		{
			return commands.Keys.OrderBy (k => k);
		}
		/// <summary>
		/// 输出信息消息
		/// </summary>
		public void WriteInfo (string message)
		{
			WriteLineInternal ($"[INFO] {message}", ColorInfo);
		}
		public void WriteInfo (string format, params object [] args)
		{
			try
			{
				WriteInfo (string.Format (format, args));
			}
			catch (Exception ex)
			{
				WriteInfo ($"Format error: {ex.Message}");
			}
		}
	}
	/// <summary>
	/// 调试输出接口
	/// </summary>
	public interface IDebugOutput
	{
		void WriteLine (string message);
		void WriteLine (string format, params object [] args);
		void WriteError (string message);
		void WriteError (string format, params object [] args);
		void WriteWarning (string message);
		void WriteWarning (string format, params object [] args);
		void Clear ();
	}
	/// <summary>
	/// 调试命令接口
	/// </summary>
	public interface IDebugCommand
	{
		bool Execute (string command, string [] args);
		string GetHelp ();
	}
	/// <summary>
	/// Help 命令
	/// </summary>
	public class HelpCommand: IDebugCommand
	{
		private DebugIOForm form;

		public HelpCommand (DebugIOForm form)
		{
			this.form = form;
		}

		public bool Execute (string command, string [] args)
		{
			form.WriteLine ("Available commands:");
			foreach (var cmd in form.GetRegisteredCommands ())
			{
				form.WriteLine ($"  {cmd}");
			}
			form.WriteLine ("\nType '<command> help' for more information about a command.");
			return true;
		}

		public string GetHelp ()
		{
			return "Display available commands";
		}
	}

	/// <summary>
	/// Clear 命令
	/// </summary>
	public class ClearCommand: IDebugCommand
	{
		private DebugIOForm form;

		public ClearCommand (DebugIOForm form)
		{
			this.form = form;
		}

		public bool Execute (string command, string [] args)
		{
			form.Clear ();
			return true;
		}

		public string GetHelp ()
		{
			return "Clear the debug output";
		}
	}

	/// <summary>
	/// Time 命令
	/// </summary>
	public class TimeCommand: IDebugCommand
	{
		private DebugIOForm form;

		public TimeCommand (DebugIOForm form)
		{
			this.form = form;
		}

		public bool Execute (string command, string [] args)
		{
			form.WriteLine ($"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
			form.WriteLine ($"Ticks: {DateTime.Now.Ticks}");
			return true;
		}

		public string GetHelp ()
		{
			return "Display current system time";
		}
	}

	/// <summary>
	/// Echo 命令
	/// </summary>
	public class EchoCommand: IDebugCommand
	{
		private DebugIOForm form;

		public EchoCommand (DebugIOForm form)
		{
			this.form = form;
		}

		public bool Execute (string command, string [] args)
		{
			if (args.Length == 0)
			{
				form.WriteLine ("");
				return true;
			}

			string message = string.Join (" ", args);
			form.WriteLine (message);
			return true;
		}

		public string GetHelp ()
		{
			return "Echo a message";
		}
	}

	/// <summary>
	/// Commands 命令（列出所有命令）
	/// </summary>
	public class CommandsCommand: IDebugCommand
	{
		private DebugIOForm form;

		public CommandsCommand (DebugIOForm form)
		{
			this.form = form;
		}

		public bool Execute (string command, string [] args)
		{
			form.WriteLine ("Registered commands:");
			int index = 1;
			foreach (var cmd in form.GetRegisteredCommands ())
			{
				form.WriteLine ($"  {index}. {cmd}");
				index++;
			}
			return true;
		}

		public string GetHelp ()
		{
			return "List all registered commands";
		}
	}
}
