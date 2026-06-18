using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Sidebar
{
	public static class StartupManager
	{
		[DllImport ("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern uint GetModuleFileName (
			IntPtr hModule,
			StringBuilder lpFilename,
			int nSize);
		public static string GetExecutablePath ()
		{
			StringBuilder sb = new StringBuilder (260);
			GetModuleFileName (IntPtr.Zero, sb, sb.Capacity);
			return sb.ToString ();
		}
		// 启动项名称（唯一标识）
		private const string RunKeyName = "WindowsModern.Sidebar"; 
		private static string RunKeyPath =
			@"Software\Microsoft\Windows\CurrentVersion\Run";
		/// <summary>
		/// 获取当前程序完整启动命令（含参数）
		/// </summary>
		private static string GetExecutableCommand ()
		{
			string exePath = GetExecutablePath ();
			return "\"" + exePath + "\"";
		}
		/// <summary>
		/// 是否已设置开机启动
		/// </summary>
		public static bool IsEnabled ()
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey (RunKeyPath, false))
			{
				if (key == null) return false;

				object value = key.GetValue (RunKeyName);
				if (value == null) return false;

				string current = value.ToString ();
				return string.Equals (current, GetExecutableCommand (),
					StringComparison.OrdinalIgnoreCase);
			}
		}
		/// <summary>
		/// 启用开机启动（幂等，保证唯一）
		/// </summary>
		public static void Enable ()
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey (RunKeyPath, true))
			{
				if (key == null) return;
				key.DeleteValue (RunKeyName, false);
				key.SetValue (RunKeyName, GetExecutableCommand (),
					RegistryValueKind.String);
			}
		}
		/// <summary>
		/// 关闭开机启动
		/// </summary>
		public static void Disable ()
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey (RunKeyPath, true))
			{
				if (key == null) return;
				key.DeleteValue (RunKeyName, false);
			}
		}
		/// <summary>
		/// 切换状态
		/// </summary>
		public static void Toggle ()
		{
			if (IsEnabled ())
				Disable ();
			else
				Enable ();
		}
	}
}
