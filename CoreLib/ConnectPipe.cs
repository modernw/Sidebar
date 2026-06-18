using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
namespace Sidebar
{
	public static class SidebarPipe
	{
		public static string PipeName => "WindowsModern.Sidebar!ConnectPipe";
		public delegate void MessageReceiveHandler (string message);
		public delegate void MessageMailReceiveHandler (string name, object datas, Type dataType);
		public static MessageReceiveHandler Message { get; set; }
		public static MessageMailReceiveHandler Mail { get; set; }
		private static void OnMessage (string msg)
		{
			Message?.Invoke (msg);
		}
		private static void OnMail (string name, object datas, Type type)
		{
			Mail?.Invoke (name, datas, type);
		}
		private static NamedPipeServerStream _pipeServer;
		private static Thread _listenerThread;
		private static bool _isRunning;
		/// <summary>
		/// 启动管道服务器，开始监听客户端连接。
		/// </summary>
		public static void StartServer ()
		{
			if (_isRunning) return;

			_isRunning = true;
			_listenerThread = new Thread (ListenLoop) {
				IsBackground = true,
				Name = "SidebarPipeListener"
			};
			_listenerThread.Start ();
		}
		/// <summary>
		/// 停止管道服务器，释放资源。
		/// </summary>
		public static void StopServer ()
		{
			_isRunning = false;
			_pipeServer?.Close ();
			_pipeServer?.Dispose ();
			_listenerThread?.Join (1000);
		}
		private static void ListenLoop ()
		{
			while (_isRunning)
			{
				try
				{
					using (var server = new NamedPipeServerStream (PipeName, PipeDirection.In, 1))
					{
						_pipeServer = server;
						server.WaitForConnection ();
						using (var reader = new StreamReader (server, Encoding.UTF8))
						{
							string message = reader.ReadLine ();
							if (!string.IsNullOrEmpty (message))
							{
								string mailName = null;
								object mailData = null;
								Type mailType = null;
								bool isMail = TryParseMail (message, out mailName, out mailData, out mailType);
								if (isMail)
								{
									if (Mail != null)
										Mail (mailName, mailData, mailType);
								}
								else
								{
									if (Message != null)
										Message (message);
								}
							}
						}
					}
				}
				catch (IOException) { }
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine ("SidebarPipe 错误: " + ex.Message);
				}
				if (_isRunning)
					Thread.Sleep (10);
			}
		}
		/// <summary>
		/// 客户端向管道服务器发送消息（同步，自动处理连接和释放）
		/// </summary>
		/// <param name="message">要发送的文本消息</param>
		/// <param name="serverName">服务器计算机名，本机为 "."</param>
		/// <param name="timeoutMs">连接超时毫秒数，默认 1000</param>
		/// <returns>是否发送成功</returns>
		public static bool SendMessage (string message, string serverName = ".", int timeoutMs = 1000)
		{
			if (string.IsNullOrEmpty (message)) return false;
			using (var client = new NamedPipeClientStream (serverName, PipeName, PipeDirection.Out))
			{
				try
				{
					client.Connect (timeoutMs);
				}
				catch (TimeoutException)
				{
					return false;
				}
				catch (IOException)
				{
					return false;
				}
				catch (Exception)
				{
					return false;
				}
				try
				{
					using (var writer = new StreamWriter (client, Encoding.UTF8))
					{
						writer.WriteLine (message);
						writer.Flush ();
					}
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}
		/// <summary>
		/// 尝试将 JSON 字符串解析为 SidebarMail 对象
		/// </summary>
		private static bool TryParseMail (string json, out string name, out object data, out Type dataType)
		{
			name = null;
			data = null;
			dataType = null;
			try
			{
				var mail = JsonConvert.DeserializeObject<SidebarMail> (json);
				if (mail == null || string.IsNullOrEmpty (mail.Name))
					return false;
				name = mail.Name;
				data = mail.Data;
				dataType = data?.GetType ();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
	public class SidebarMail
	{
		public static bool Send (string name, object datas = null)
		{
			var mail = new SidebarMail {
				Name = name,
				Data = datas,
				DataType = datas?.GetType () 
			};
			var str = JsonConvert.SerializeObject (mail);
			return SidebarPipe.SendMessage (str);
		}
		public string Name { get; set; }
		public object Data { get; set; }
		[JsonIgnore]
		public Type DataType { get; set; }
	}
}
