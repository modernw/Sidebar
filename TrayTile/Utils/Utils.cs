using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using Sidebar.Win32;
using static Sidebar.Win32WindowNative;
using HWND = Sidebar.HWND;
namespace WindowsModern.TrayTile.Utils
{
	public static class Native
	{
		public static List<AutomationElement> FindTrayToolbars ()
		{
			var toolbars = new List<AutomationElement> ();
			Condition trayWndCondition = new PropertyCondition (AutomationElement.ClassNameProperty, "Shell_TrayWnd");
			AutomationElement trayWnd = AutomationElement.RootElement.FindFirst (TreeScope.Children, trayWndCondition);
			if (trayWnd == null) return toolbars;
			Condition notifyWndCondition = new PropertyCondition (AutomationElement.ClassNameProperty, "TrayNotifyWnd");
			AutomationElement notifyWnd = trayWnd.FindFirst (TreeScope.Descendants, notifyWndCondition);
			if (notifyWnd == null) return toolbars;
			Condition toolbarCondition = new PropertyCondition (AutomationElement.ClassNameProperty, "ToolbarWindow32");
			var foundToolbars = notifyWnd.FindAll (TreeScope.Children, toolbarCondition);
			foreach (AutomationElement tb in foundToolbars)
			{
				if (!toolbars.Contains (tb))
					toolbars.Add (tb);
			}
			Condition sysPagerCondition = new PropertyCondition (AutomationElement.ClassNameProperty, "SysPager");
			AutomationElement sysPager = notifyWnd.FindFirst (TreeScope.Children, sysPagerCondition);
			if (sysPager != null)
			{
				var pagerToolbars = sysPager.FindAll (TreeScope.Children, toolbarCondition);
				foreach (AutomationElement tb in pagerToolbars)
				{
					if (!toolbars.Contains (tb))
						toolbars.Add (tb);
				}
			}
			return toolbars;
		}
		public static AutomationElement FindOverflowToolbar ()
		{
			Condition overflowWndCondition = new PropertyCondition (AutomationElement.ClassNameProperty, "NotifyIconOverflowWindow");
			AutomationElement overflowWnd = AutomationElement.RootElement.FindFirst (TreeScope.Children, overflowWndCondition);
			if (overflowWnd == null) return null;
			Condition toolbarCondition = new PropertyCondition (AutomationElement.ClassNameProperty, "ToolbarWindow32");
			return overflowWnd.FindFirst (TreeScope.Children, toolbarCondition);
		}
		public static List<AutomationElement> FindAllTrayToolbars ()
		{
			var allToolbars = FindTrayToolbars ();
			var overflowToolbar = FindOverflowToolbar ();
			if (overflowToolbar != null && !allToolbars.Contains (overflowToolbar))
			{
				allToolbars.Add (overflowToolbar);
			}
			return allToolbars;
		}
		public static List<AutomationElement> GetAllToolbarButtons (AutomationElement ae)
		{
			var list = new List<AutomationElement> ();
			if (ae == null) return list;
			Condition buttonCondition = new PropertyCondition (AutomationElement.ControlTypeProperty, ControlType.Button);
			var buttons = ae.FindAll (TreeScope.Children, buttonCondition);
			foreach (AutomationElement btn in buttons)
			{
				if (btn == null) continue;
				if (!list.Contains (btn))
					list.Add (btn);
			}
			return list;
		}
		public static List<AutomationElement> GetAllToolbarButtons (List<AutomationElement> aelist)
		{
			List<AutomationElement> list = null;
			foreach (var i in aelist)
			{
				var templist = GetAllToolbarButtons (i);
				if (list == null) list = templist;
				else
				{
					foreach (var btn in templist)
					{
						if (btn == null) continue;
						if (!list.Contains (btn)) list.Add (btn);
					}
				}
			}
			return list ?? new List<AutomationElement> ();
		}
		public static List<AutomationElement> GetAllToolbarButtons () => GetAllToolbarButtons (FindAllTrayToolbars ());
	}
	public static class AutomationElementExtensions
	{
		#region Win32 API 声明
		[DllImport ("user32.dll")]
		private static extern IntPtr SendMessage (IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
		[DllImport ("user32.dll")]
		private static extern bool ScreenToClient (IntPtr hWnd, ref POINT lpPoint);
		[DllImport ("user32.dll")]
		private static extern bool ClientToScreen (IntPtr hWnd, ref POINT lpPoint);
		[StructLayout (LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;
			public POINT (int x, int y) { X = x; Y = y; }
		}
		private const uint WM_LBUTTONDOWN = 0x0201;
		private const uint WM_LBUTTONUP = 0x0202;
		private const uint WM_RBUTTONDOWN = 0x0204;
		private const uint WM_RBUTTONUP = 0x0205;
		private const uint WM_MOUSEMOVE = 0x0200;
		private const uint WM_MOUSELEAVE = 0x02A3;
		private static IntPtr MakeLParam (int x, int y) => (IntPtr)((y << 16) | (x & 0xFFFF));
		#endregion
		#region 核心辅助方法
		/// <summary>
		/// 获取元素本身的句柄（如果存在），否则获取父窗口句柄
		/// </summary>
		private static IntPtr GetTargetHandle (AutomationElement element)
		{
			IntPtr hWnd = (IntPtr)element.Current.NativeWindowHandle;
			if (hWnd != IntPtr.Zero)
				return hWnd;
			var parent = TreeWalker.ControlViewWalker.GetParent (element);
			if (parent != null)
				return (IntPtr)parent.Current.NativeWindowHandle;
			return IntPtr.Zero;
		}
		/// <summary>
		/// 获取元素中心点相对于目标窗口客户区的坐标（如果元素无自身句柄，则用父窗口）
		/// </summary>
		private static bool GetClientPoint (AutomationElement element, IntPtr hWndTarget, out POINT clientPoint)
		{
			clientPoint = new POINT (0, 0);
			try
			{
				var rect = element.Current.BoundingRectangle;
				if (rect.IsEmpty) return false;
				int screenX = (int)(rect.Left + rect.Width / 2);
				int screenY = (int)(rect.Top + rect.Height / 2);
				POINT pt = new POINT (screenX, screenY);
				if (!ScreenToClient (hWndTarget, ref pt))
					return false;
				clientPoint = pt;
				return true;
			}
			catch (ElementNotAvailableException)
			{
				return false;
			}
		}
		/// <summary>
		/// 发送鼠标消息（点击）
		/// </summary>
		private static void SendMouseClick (AutomationElement element, uint downMsg, uint upMsg)
		{
			if (element == null) return;
			IntPtr hWnd = GetTargetHandle (element);
			if (hWnd == IntPtr.Zero) return;
			if ((IntPtr)element.Current.NativeWindowHandle != IntPtr.Zero)
			{
				SendMessage (hWnd, downMsg, IntPtr.Zero, IntPtr.Zero);
				SendMessage (hWnd, upMsg, IntPtr.Zero, IntPtr.Zero);
				return;
			}
			POINT pt;
			if (!GetClientPoint (element, hWnd, out pt))
				return;
			IntPtr lParam = MakeLParam (pt.X, pt.Y);
			SendMessage (hWnd, downMsg, IntPtr.Zero, lParam);
			SendMessage (hWnd, upMsg, IntPtr.Zero, lParam);
		}
		#endregion
		#region 公共扩展方法
		/// <summary>
		/// 左键单击（不移动鼠标，优先使用 InvokePattern，否则发送消息）
		/// </summary>
		public static void TryClick (this AutomationElement element)
		{
			if (element == null) return;
			object pattern;
			if (element.TryGetCurrentPattern (InvokePattern.Pattern, out pattern))
			{
				((InvokePattern)pattern).Invoke ();
				return;
			}
			SendMouseClick (element, WM_LBUTTONDOWN, WM_LBUTTONUP);
		}
		/// <summary>
		/// 左键双击（仅发送消息，无可靠 Pattern）
		/// </summary>
		public static void DoubleClick (this AutomationElement element)
		{
			if (element == null) return;
			SendMouseClick (element, WM_LBUTTONDOWN, WM_LBUTTONUP);
			SendMouseClick (element, WM_LBUTTONDOWN, WM_LBUTTONUP);
		}
		/// <summary>
		/// 右键单击
		/// </summary>
		public static void RightClick (this AutomationElement element)
		{
			if (element == null) return;
			SendMouseClick (element, WM_RBUTTONDOWN, WM_RBUTTONUP);
		}
		/// <summary>
		/// 模拟悬停（发送 WM_MOUSEMOVE 到元素中心，不移动物理鼠标）
		/// 注意：部分控件可能需要实际鼠标位置才触发悬停效果，此方法不一定有效。
		/// </summary>
		public static void Hover (this AutomationElement element)
		{
			if (element == null) return;
			IntPtr hWnd = GetTargetHandle (element);
			if (hWnd == IntPtr.Zero) return;
			if ((IntPtr)element.Current.NativeWindowHandle != IntPtr.Zero)
			{
				SendMessage (hWnd, WM_MOUSEMOVE, IntPtr.Zero, IntPtr.Zero);
				return;
			}
			POINT pt;
			if (!GetClientPoint (element, hWnd, out pt))
				return;
			IntPtr lParam = MakeLParam (pt.X, pt.Y);
			SendMessage (hWnd, WM_MOUSEMOVE, IntPtr.Zero, lParam);
		}
		/// <summary>
		/// 模拟离开（发送 WM_MOUSELEAVE 消息）
		/// 注意：通常需要先使用 TrackMouseEvent 才能收到该消息，直接发送可能无效。
		/// </summary>
		public static void Leave (this AutomationElement element)
		{
			if (element == null) return;
			IntPtr hWnd = GetTargetHandle (element);
			if (hWnd == IntPtr.Zero) return;
			SendMessage (hWnd, WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
		}
		#endregion
	}
	public static class ToolbarIconExtractor
	{
		#region P/Invoke
		[DllImport ("user32.dll", SetLastError = true)]
		private static extern IntPtr SendMessage (IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll")]
		private static extern uint GetWindowThreadProcessId (IntPtr hWnd, out uint lpdwProcessId);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess (uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle (IntPtr hObject);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern bool IsWow64Process (IntPtr hProcess, out bool wow64Process);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern IntPtr VirtualAllocEx (IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern bool VirtualFreeEx (IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern bool ReadProcessMemory (IntPtr hProcess, IntPtr lpBaseAddress, byte [] lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead);

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern bool WriteProcessMemory (IntPtr hProcess, IntPtr lpBaseAddress, byte [] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesWritten);
		#endregion

		#region Constants
		private const uint WM_USER = 0x0400;
		private const uint TB_GETBUTTON = WM_USER + 23;
		private const uint TB_BUTTONCOUNT = WM_USER + 24;

		private const uint PROCESS_QUERY_INFORMATION = 0x0400;
		private const uint PROCESS_VM_OPERATION = 0x0008;
		private const uint PROCESS_VM_READ = 0x0010;
		private const uint PROCESS_VM_WRITE = 0x0020;

		private const uint MEM_COMMIT = 0x1000;
		private const uint MEM_RESERVE = 0x2000;
		private const uint MEM_RELEASE = 0x8000;
		private const uint PAGE_READWRITE = 0x04;
		#endregion

		#region Structures
		[StructLayout (LayoutKind.Sequential, Pack = 4)]
		private struct TBBUTTON32
		{
			public int iBitmap;
			public int idCommand;
			public byte fsState;
			public byte fsStyle;
			public byte bReserved1;
			public byte bReserved2;
			public int dwData;   // 32-bit pointer
			public int iString;
		}

		[StructLayout (LayoutKind.Sequential, Pack = 8)]
		private struct TBBUTTON64
		{
			public int iBitmap;
			public int idCommand;
			public byte fsState;
			public byte fsStyle;
			public byte bReserved1;
			public byte bReserved2;
			public IntPtr dwData;   // 64-bit pointer
			public IntPtr iString;
		}
		#endregion

		#region Public API
		/// <summary>
		/// 从托盘按钮 AutomationElement 提取图标 HICON。
		/// 通过工具提示文本匹配定位按钮，不受顺序影响。
		/// </summary>
		public static IntPtr ExtractIconFromButton (AutomationElement buttonElement)
		{
			if (buttonElement == null) return IntPtr.Zero;

			string tooltip = buttonElement.Current.Name;
			if (string.IsNullOrEmpty (tooltip)) return IntPtr.Zero;

			AutomationElement toolbar = FindParentToolbar (buttonElement);
			if (toolbar == null) return IntPtr.Zero;

			IntPtr hWnd = new IntPtr (toolbar.Current.NativeWindowHandle);
			if (hWnd == IntPtr.Zero) return IntPtr.Zero;

			uint pid;
			GetWindowThreadProcessId (hWnd, out pid);
			if (pid == 0) return IntPtr.Zero;

			uint access = PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE;
			IntPtr hProcess = OpenProcess (access, false, pid);
			if (hProcess == IntPtr.Zero) return IntPtr.Zero;

			try
			{
				bool targetIs64 = IsProcess64Bit (hProcess);
				int buttonCount = (int)SendMessage (hWnd, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
				if (buttonCount <= 0) return IntPtr.Zero;

				int structSize = targetIs64 ? Marshal.SizeOf (typeof (TBBUTTON64)) : Marshal.SizeOf (typeof (TBBUTTON32));
				IntPtr remoteBuffer = VirtualAllocEx (hProcess, IntPtr.Zero, (IntPtr)structSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
				if (remoteBuffer == IntPtr.Zero) return IntPtr.Zero;

				try
				{
					byte [] zero = new byte [structSize];
					IntPtr wrote;
					WriteProcessMemory (hProcess, remoteBuffer, zero, (IntPtr)structSize, out wrote);

					for (int i = 0; i < buttonCount; i++)
					{
						IntPtr res = SendMessage (hWnd, TB_GETBUTTON, (IntPtr)i, remoteBuffer);
						if (res == IntPtr.Zero) continue;

						byte [] localBuf = new byte [structSize];
						IntPtr bytesRead;
						if (!ReadProcessMemory (hProcess, remoteBuffer, localBuf, (IntPtr)structSize, out bytesRead))
							continue;
						if (bytesRead.ToInt32 () != structSize) continue;

						IntPtr pNotifyData;
						GCHandle gch = GCHandle.Alloc (localBuf, GCHandleType.Pinned);
						try
						{
							IntPtr localPtr = gch.AddrOfPinnedObject ();
							if (targetIs64)
							{
								TBBUTTON64 btn = (TBBUTTON64)Marshal.PtrToStructure (localPtr, typeof (TBBUTTON64));
								pNotifyData = btn.dwData;
							}
							else
							{
								TBBUTTON32 btn = (TBBUTTON32)Marshal.PtrToStructure (localPtr, typeof (TBBUTTON32));
								pNotifyData = (IntPtr)btn.dwData;
							}
						}
						finally
						{
							gch.Free ();
						}

						if (pNotifyData == IntPtr.Zero) continue;

						// 读取 NOTIFYICONDATA 的 cbSize
						byte [] cbSizeBuf = new byte [4];
						if (!ReadProcessMemory (hProcess, pNotifyData, cbSizeBuf, (IntPtr)4, out bytesRead))
							continue;
						if (bytesRead.ToInt32 () != 4) continue;
						uint cbSize = BitConverter.ToUInt32 (cbSizeBuf, 0);

						// 获取 szTip 偏移（在 hIcon 之后）
						int tipOffset = GetTipOffset (targetIs64, cbSize);
						if (tipOffset < 0) continue;

						// 读取 szTip（Unicode，最多 128 字符）
						byte [] tipBuf = new byte [256];
						IntPtr tipAddr = pNotifyData + tipOffset;
						if (!ReadProcessMemory (hProcess, tipAddr, tipBuf, (IntPtr)256, out bytesRead))
							continue;
						if (bytesRead.ToInt32 () < 2) continue;

						string currentTip = Encoding.Unicode.GetString (tipBuf).TrimEnd ('\0');
						if (string.Equals (currentTip, tooltip, StringComparison.Ordinal))
						{
							// 匹配成功，提取该按钮的图标
							return ExtractIconFromToolbarByIndex (hWnd, hProcess, targetIs64, i);
						}
					}
				}
				finally
				{
					VirtualFreeEx (hProcess, remoteBuffer, IntPtr.Zero, MEM_RELEASE);
				}
			}
			finally
			{
				CloseHandle (hProcess);
			}

			return IntPtr.Zero;
		}

		/// <summary>
		/// 通过工具栏窗口句柄和按钮索引直接提取图标（备用方法）。
		/// </summary>
		public static IntPtr ExtractIconFromToolbar (IntPtr hWnd, int buttonIndex)
		{
			if (hWnd == IntPtr.Zero) return IntPtr.Zero;

			uint pid;
			GetWindowThreadProcessId (hWnd, out pid);
			if (pid == 0) return IntPtr.Zero;

			uint access = PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE;
			IntPtr hProcess = OpenProcess (access, false, pid);
			if (hProcess == IntPtr.Zero) return IntPtr.Zero;

			try
			{
				bool targetIs64 = IsProcess64Bit (hProcess);
				return ExtractIconFromToolbarByIndex (hWnd, hProcess, targetIs64, buttonIndex);
			}
			finally
			{
				CloseHandle (hProcess);
			}
		}
		#endregion

		#region Helpers
		private static AutomationElement FindParentToolbar (AutomationElement element)
		{
			TreeWalker walker = TreeWalker.ControlViewWalker;
			AutomationElement parent = walker.GetParent (element);
			while (parent != null)
			{
				if (string.Equals (parent.Current.ClassName, "ToolbarWindow32", StringComparison.Ordinal))
					return parent;
				parent = walker.GetParent (parent);
			}
			return null;
		}

		private static bool IsProcess64Bit (IntPtr hProcess)
		{
			bool isWow64;
			if (IsWow64Process (hProcess, out isWow64))
			{
				if (Environment.Is64BitOperatingSystem)
					return !isWow64;
				else
					return false;
			}
			return IntPtr.Size == 8; // fallback
		}

		private static int GetTipOffset (bool targetIs64, uint cbSize)
		{
			// 计算 szTip 在 NOTIFYICONDATA 中的偏移。
			// 在标准 NOTIFYICONDATAW 中，结构为：
			// cbSize (4) + hWnd (指针) + uID (4) + uFlags (4) + uCallbackMessage (4) + hIcon (指针) + szTip (128*2)
			// 但 Windows 版本不同，我们根据 cbSize 判断。
			// 这里采用常见偏移：有 uCallbackMessage 时 hIcon 后即 szTip。
			int hIconOffset;
			if (targetIs64)
			{
				if (cbSize >= 504) // Vista+ 结构
					hIconOffset = 32;
				else
					hIconOffset = 24; // 旧版
			}
			else
			{
				if (cbSize >= 504)
					hIconOffset = 20;
				else
					hIconOffset = 16;
			}
			int ptrSize = targetIs64 ? 8 : 4;
			return hIconOffset + ptrSize; // hIcon 后就是 szTip
		}

		private static IntPtr ExtractIconFromToolbarByIndex (IntPtr hWnd, IntPtr hProcess, bool targetIs64, int buttonIndex)
		{
			int structSize = targetIs64 ? Marshal.SizeOf (typeof (TBBUTTON64)) : Marshal.SizeOf (typeof (TBBUTTON32));
			IntPtr remoteBuffer = VirtualAllocEx (hProcess, IntPtr.Zero, (IntPtr)structSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
			if (remoteBuffer == IntPtr.Zero) return IntPtr.Zero;

			try
			{
				byte [] zero = new byte [structSize];
				IntPtr wrote;
				WriteProcessMemory (hProcess, remoteBuffer, zero, (IntPtr)structSize, out wrote);

				IntPtr res = SendMessage (hWnd, TB_GETBUTTON, (IntPtr)buttonIndex, remoteBuffer);
				if (res == IntPtr.Zero) return IntPtr.Zero;

				byte [] localBuf = new byte [structSize];
				IntPtr bytesRead;
				if (!ReadProcessMemory (hProcess, remoteBuffer, localBuf, (IntPtr)structSize, out bytesRead))
					return IntPtr.Zero;
				if (bytesRead.ToInt32 () != structSize) return IntPtr.Zero;

				IntPtr pNotifyData;
				GCHandle gch = GCHandle.Alloc (localBuf, GCHandleType.Pinned);
				try
				{
					IntPtr localPtr = gch.AddrOfPinnedObject ();
					if (targetIs64)
					{
						TBBUTTON64 btn = (TBBUTTON64)Marshal.PtrToStructure (localPtr, typeof (TBBUTTON64));
						pNotifyData = btn.dwData;
					}
					else
					{
						TBBUTTON32 btn = (TBBUTTON32)Marshal.PtrToStructure (localPtr, typeof (TBBUTTON32));
						pNotifyData = (IntPtr)btn.dwData;
					}
				}
				finally
				{
					gch.Free ();
				}

				if (pNotifyData == IntPtr.Zero) return IntPtr.Zero;

				byte [] cbSizeBuf = new byte [4];
				if (!ReadProcessMemory (hProcess, pNotifyData, cbSizeBuf, (IntPtr)4, out bytesRead))
					return IntPtr.Zero;
				if (bytesRead.ToInt32 () != 4) return IntPtr.Zero;
				uint cbSize = BitConverter.ToUInt32 (cbSizeBuf, 0);

				int hIconOffset;
				if (targetIs64)
					hIconOffset = (cbSize >= 504) ? 32 : 24;
				else
					hIconOffset = (cbSize >= 504) ? 20 : 16;

				if (hIconOffset < 0) return IntPtr.Zero;

				int ptrSize = targetIs64 ? 8 : 4;
				byte [] hIconBuf = new byte [ptrSize];
				IntPtr pIconAddr = pNotifyData + hIconOffset;
				if (!ReadProcessMemory (hProcess, pIconAddr, hIconBuf, (IntPtr)ptrSize, out bytesRead))
					return IntPtr.Zero;
				if (bytesRead.ToInt32 () != ptrSize) return IntPtr.Zero;

				return targetIs64 ? (IntPtr)BitConverter.ToInt64 (hIconBuf, 0)
								  : (IntPtr)BitConverter.ToInt32 (hIconBuf, 0);
			}
			finally
			{
				VirtualFreeEx (hProcess, remoteBuffer, IntPtr.Zero, MEM_RELEASE);
			}
		}
		#endregion
	}
}
