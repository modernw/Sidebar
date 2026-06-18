using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

// ============================================================
// 静态助手类：跨进程读取托盘图标数据 + 模拟鼠标操作
// ============================================================
// ============================================================
// 静态助手类：跨进程读取托盘图标数据 + 模拟鼠标操作
// XP / Vista+ 全兼容，修复 XP 图标错位问题
// ============================================================
public static class TrayIconHelper
{
	private const int TB_GETBUTTON = 0x0417;
	private const int TB_BUTTONCOUNT = 0x0418;

	private static int _osVersion = -1;
	public static int OSVersion
	{
		get
		{
			if (_osVersion == -1)
			{
				if (Environment.OSVersion.Version.Major == 5)
					_osVersion = 0;   // XP
				else
					_osVersion = 4;   // Vista+
			}
			return _osVersion;
		}
	}

	#region Win32

	[DllImport ("user32.dll")]
	private static extern IntPtr FindWindow (string c, string w);
	[DllImport ("user32.dll")]
	private static extern IntPtr FindWindowEx (IntPtr p, IntPtr c, string cls, string t);
	[DllImport ("user32.dll")]
	private static extern uint GetWindowThreadProcessId (IntPtr h, out uint pid);
	[DllImport ("kernel32.dll")]
	private static extern IntPtr OpenProcess (uint acc, bool inh, uint pid);
	[DllImport ("kernel32.dll")]
	private static extern bool CloseHandle (IntPtr h);
	[DllImport ("kernel32.dll")]
	private static extern IntPtr VirtualAllocEx (IntPtr h, IntPtr a, uint s, uint t, uint p);
	[DllImport ("kernel32.dll")]
	private static extern bool VirtualFreeEx (IntPtr h, IntPtr a, uint s, uint t);
	[DllImport ("kernel32.dll")]
	private static extern bool ReadProcessMemory (IntPtr h, IntPtr a, out TBBUTTON b, int s, IntPtr r);
	[DllImport ("kernel32.dll")]
	private static extern bool ReadProcessMemory (IntPtr h, IntPtr a, out TRAYDATA b, int s, IntPtr r);
	[DllImport ("kernel32.dll")]
	private static extern bool ReadProcessMemory (IntPtr h, IntPtr a, out TRAYDATA_XP b, int s, IntPtr r);
	[DllImport ("user32.dll")]
	private static extern IntPtr SendMessage (IntPtr h, int m, IntPtr w, IntPtr l);
	[DllImport ("comctl32.dll")]
	private static extern IntPtr ImageList_GetIcon (IntPtr himl, int i, int flags);

	private const int TB_GETIMAGELIST = 0x0431;

	// XP ImageList 读取
	public static IntPtr GetIconFromImageList_XP (IntPtr hToolbar, int index)
	{
		IntPtr himl = SendMessage (hToolbar, TB_GETIMAGELIST, IntPtr.Zero, IntPtr.Zero);

		if (himl == IntPtr.Zero) return IntPtr.Zero;

		// ILD_NORMAL = 0
		return ImageList_GetIcon (himl, index, 0);
	}
	#endregion

	#region Structs

	[StructLayout (LayoutKind.Sequential)]
	private struct TBBUTTON
	{
		public int iBitmap;
		public int idCommand;
		public byte fsState;
		public byte fsStyle;
		public byte b0;
		public byte b1;
		public IntPtr dwData;
		public IntPtr iString;
	}

	// Vista+
	[StructLayout (LayoutKind.Sequential)]
	public struct TRAYDATA
	{
		public IntPtr hwnd;
		public uint uID;
		public uint uCallbackMessage;
		public uint r0;
		public uint r1;
		public IntPtr hIcon;
	}

	// XP 正确结构（关键！）
	[StructLayout (LayoutKind.Sequential)]
	public struct TRAYDATA_XP
	{
		public uint idCommand;
		public IntPtr hwnd;
		public uint uID;
		public uint uCallbackMessage;
		public IntPtr hIcon;
	}

	#endregion

	public static IntPtr GetTrayToolbarHandle ()
	{
		IntPtr hShell = FindWindow ("Shell_TrayWnd", null);
		IntPtr hNotify = FindWindowEx (hShell, IntPtr.Zero, "TrayNotifyWnd", null);
		IntPtr hSysPager = FindWindowEx (hNotify, IntPtr.Zero, "SysPager", null);

		if (hSysPager != IntPtr.Zero)
			return FindWindowEx (hSysPager, IntPtr.Zero, "ToolbarWindow32", null);

		return FindWindowEx (hNotify, IntPtr.Zero, "ToolbarWindow32", null);
	}

	public static int GetButtonCount ()
	{
		IntPtr h = GetTrayToolbarHandle ();
		return (int)SendMessage (h, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
	}

	public static TRAYDATA? GetTrayData (int index)
	{
		IntPtr hToolbar = GetTrayToolbarHandle ();
		if (hToolbar == IntPtr.Zero) return null;

		uint pid;
		GetWindowThreadProcessId (hToolbar, out pid);
		IntPtr hProcess = OpenProcess (0x0010 | 0x0008 | 0x0400, false, pid);
		if (hProcess == IntPtr.Zero) return null;

		try
		{
			TBBUTTON tb;
			IntPtr mem = VirtualAllocEx (hProcess, IntPtr.Zero,
				(uint)Marshal.SizeOf (typeof (TBBUTTON)), 0x1000, 0x04);

			SendMessage (hToolbar, TB_GETBUTTON, (IntPtr)index, mem);
			ReadProcessMemory (hProcess, mem, out tb,
				Marshal.SizeOf (typeof (TBBUTTON)), IntPtr.Zero);

			VirtualFreeEx (hProcess, mem, 0, 0x8000);

			// ★ XP 与 Vista+ 分开读取
			if (OSVersion == 0)
			{
				TRAYDATA_XP xp;
				ReadProcessMemory (hProcess, tb.dwData, out xp,
					Marshal.SizeOf (typeof (TRAYDATA_XP)), IntPtr.Zero);

				return new TRAYDATA {
					hwnd = xp.hwnd,
					uID = xp.uID,
					uCallbackMessage = xp.uCallbackMessage,
					hIcon = xp.hIcon
				};
			}
			else
			{
				TRAYDATA td;
				ReadProcessMemory (hProcess, tb.dwData, out td,
					Marshal.SizeOf (typeof (TRAYDATA)), IntPtr.Zero);
				return td;
			}
		}
		finally
		{
			CloseHandle (hProcess);
		}
	}
}

// ============================================================
// RelayCommand
// ============================================================
public class RelayCommand: ICommand
{
	private readonly Action _execute;
	public RelayCommand (Action execute) { _execute = execute; }
	public bool CanExecute (object parameter) { return true; }
	public event EventHandler CanExecuteChanged { add { } remove { } }
	public void Execute (object parameter) { _execute?.Invoke (); }
}

// ============================================================
// 数据模型（不再自动设置命令，由监视器设置）
// ============================================================
public class TrayIconInfo: INotifyPropertyChanged
{
	private string _toolTip;
	private IntPtr _iconHandle;
	private Rect _bounds;
	private bool _isVisible;

	public IntPtr OwnerHwnd { get; set; }
	public int IconId { get; set; }
	public int CallbackMessage { get; set; }
	public int Version { get; set; }

	public string ToolTip
	{
		get { return _toolTip; }
		set { if (_toolTip != value) { _toolTip = value; OnPropertyChanged ("ToolTip"); OnPropertyChanged ("TooltipText"); } }
	}

	public string TooltipText { get { return ToolTip; } }

	public IntPtr IconHandle
	{
		get { return _iconHandle; }
		set { if (_iconHandle != value) { _iconHandle = value; OnPropertyChanged ("IconHandle"); OnPropertyChanged ("IconSource"); } }
	}

	public Rect Bounds
	{
		get { return _bounds; }
		set { if (_bounds != value) { _bounds = value; OnPropertyChanged ("Bounds"); } }
	}

	public bool IsVisible
	{
		get { return _isVisible; }
		set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged ("IsVisible"); } }
	}

	public ImageSource IconSource
	{
		get
		{
			if (IconHandle == IntPtr.Zero) return null;
			try { return Imaging.CreateBitmapSourceFromHIcon (IconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions ()); }
			catch { return null; }
		}
	}

	public ICommand LeftClickCommand { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged (string propertyName)
	{
		PropertyChangedEventHandler handler = PropertyChanged;
		if (handler != null) handler (this, new PropertyChangedEventArgs (propertyName));
	}
}

// ============================================================
// 监视器（后台线程 + 差异化更新）
// ============================================================
public class TrayIconWatcher: IDisposable
{
	private readonly ObservableCollection<TrayIconInfo> _trayIcons = new ObservableCollection<TrayIconInfo> ();
	private Timer _timer;
	private bool _isRunning;
	private volatile bool _isRefreshing; // 防止并发刷新

	public ObservableCollection<TrayIconInfo> TrayIcons { get { return _trayIcons; } }

	public void Start ()
	{
		if (_isRunning) return;
		_isRunning = true;
		// 每 2 秒刷新一次
		_timer = new Timer (TimerCallback, null, 0, 2000);
	}

	public void Stop ()
	{
		_isRunning = false;
		if (_timer != null) { _timer.Dispose (); _timer = null; }
	}

	public void Dispose () { Stop (); }

	private void TimerCallback (object state)
	{
		Dispatcher dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
		// 使用 BeginInvoke 异步调度，防止定时器回调阻塞
		dispatcher.BeginInvoke (new Action (RefreshAsync), DispatcherPriority.Background);
	}

	private void RefreshAsync ()
	{
		if (!_isRunning || _isRefreshing) return;
		_isRefreshing = true;

		ThreadPool.QueueUserWorkItem (_ => {
			try
			{
				List<TrayIconInfo> freshItems = GetCurrentTrayIcons (); // 后台执行耗时操作
				Dispatcher dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
				dispatcher.Invoke (new Action (() => ApplyChanges (freshItems)));
			}
			finally
			{
				_isRefreshing = false;
			}
		});
	}

	// 在后台线程中执行：获取所有托盘图标数据
	private List<TrayIconInfo> GetCurrentTrayIcons ()
	{
		List<TrayIconInfo> list = new List<TrayIconInfo> ();
		AutomationElement trayToolbar = FindTrayToolbar ();

		if (trayToolbar == null)
		{
			// 纯 Win32 备用
			int count = TrayIconHelper.GetButtonCount ();
			for (int i = 0; i < count; i++)
			{
				TrayIconInfo info = CreateIconInfoFromIndex (i);
				if (info != null) list.Add (info);
			}
		}
		else
		{
			System.Windows.Automation.Condition buttonCondition = new PropertyCondition (
				AutomationElement.ControlTypeProperty, ControlType.Button);
			AutomationElementCollection buttons = trayToolbar.FindAll (TreeScope.Children, buttonCondition);
			int index = 0;
			foreach (AutomationElement button in buttons)
			{
				TrayIconInfo info = CreateIconInfoFromUIElement (button, index);
				if (info != null) list.Add (info);
				index++;
			}
		}
		return list;
	}

	// 在后台线程中调用，因此不涉及 UI 元素
	private TrayIconInfo CreateIconInfoFromUIElement (AutomationElement button, int index)
	{
		TrayIconInfo info = new TrayIconInfo ();
		info.ToolTip = button.Current.Name;
		info.Bounds = button.Current.BoundingRectangle;
		info.IsVisible = !button.Current.IsOffscreen;
		info.Version = TrayIconHelper.OSVersion;

		try
		{
			TrayIconHelper.TRAYDATA? td = TrayIconHelper.GetTrayData (index);
			if (td.HasValue)
			{
				var d = td.Value;
				info.IconHandle = d.hIcon;
				info.OwnerHwnd = d.hwnd;
				info.IconId = (int)d.uID;
				info.CallbackMessage = (int)d.uCallbackMessage;
			}
		}
		catch { }
		return info;
	}

	private TrayIconInfo CreateIconInfoFromIndex (int index)
	{
		TrayIconInfo info = new TrayIconInfo ();
		try
		{
			TrayIconHelper.TRAYDATA? td = TrayIconHelper.GetTrayData (index);
			if (td.HasValue)
			{
				var d = td.Value;
				info.IconHandle = d.hIcon;
				info.OwnerHwnd = d.hwnd;
				info.IconId = (int)d.uID;
				info.CallbackMessage = (int)d.uCallbackMessage;
				info.Version = TrayIconHelper.OSVersion;
				info.ToolTip = string.Format ("图标 {0}", index);
				info.Bounds = Rect.Empty;
				info.IsVisible = true;
			}
			else return null;
		}
		catch { return null; }
		return info;
	}

	// 在 UI 线程更新 ObservableCollection
	private void ApplyChanges (List<TrayIconInfo> newList)
	{
		var newDict = new Dictionary<string, TrayIconInfo> ();
		foreach (var info in newList)
		{
			string key = GetItemKey (info);
			if (!newDict.ContainsKey (key)) newDict.Add (key, info);
		}

		// 移除不存在项
		for (int i = _trayIcons.Count - 1; i >= 0; i--)
		{
			string key = GetItemKey (_trayIcons [i]);
			if (!newDict.ContainsKey (key))
				_trayIcons.RemoveAt (i);
		}

		// 更新或添加
		foreach (var kvp in newDict)
		{
			string key = kvp.Key;
			TrayIconInfo newInfo = kvp.Value;

			TrayIconInfo existing = null;
			foreach (var item in _trayIcons)
				if (GetItemKey (item) == key) { existing = item; break; }

			if (existing != null)
			{
				// 更新属性，保留原命令（避免重设绑定）
				existing.ToolTip = newInfo.ToolTip;
				existing.Bounds = newInfo.Bounds;
				existing.IsVisible = newInfo.IsVisible;
				existing.IconHandle = newInfo.IconHandle;
				existing.OwnerHwnd = newInfo.OwnerHwnd;
				existing.IconId = newInfo.IconId;
				existing.CallbackMessage = newInfo.CallbackMessage;
				existing.Version = newInfo.Version;
			}
			else
			{
				// 新图标：设置命令
				TrayIconInfo captured = newInfo;
				newInfo.LeftClickCommand = new RelayCommand (() => {
					if (captured.OwnerHwnd != IntPtr.Zero)
					{
						//TrayIconHelper.SimulateLeftClick (captured.OwnerHwnd, captured.IconId,
						//								 captured.CallbackMessage, captured.Version);
					}
				});
				_trayIcons.Add (newInfo);
			}
		}
	}

	private static string GetItemKey (TrayIconInfo info)
	{
		if (info.OwnerHwnd != IntPtr.Zero && info.IconId != 0)
			return string.Format ("{0}_{1}", info.OwnerHwnd, info.IconId);
		return string.Format ("{0}|{1},{2}", info.ToolTip, info.Bounds.X, info.Bounds.Y);
	}

	private static AutomationElement FindTrayToolbar ()
	{
		AutomationElement desktop = AutomationElement.RootElement;
		AutomationElement trayWnd = desktop.FindFirst (TreeScope.Children,
			new PropertyCondition (AutomationElement.ClassNameProperty, "Shell_TrayWnd"));
		if (trayWnd == null) return null;

		AutomationElement trayNotify = trayWnd.FindFirst (TreeScope.Descendants,
			new PropertyCondition (AutomationElement.ClassNameProperty, "TrayNotifyWnd"));
		if (trayNotify == null) return null;

		System.Windows.Automation.Condition toolbarCondition = new OrCondition (
			new PropertyCondition (AutomationElement.ClassNameProperty, "ToolbarWindow32"),
			new PropertyCondition (AutomationElement.ControlTypeProperty, ControlType.ToolBar));
		AutomationElement toolbar = trayNotify.FindFirst (TreeScope.Descendants, toolbarCondition);
		if (toolbar == null)
			toolbar = trayNotify.FindFirst (TreeScope.Descendants,
				new PropertyCondition (AutomationElement.ControlTypeProperty, ControlType.ToolBar));
		return toolbar;
	}
}