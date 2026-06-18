using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Sidebar.Win32;
using static Sidebar.Win32WindowNative;

namespace Sidebar
{
	/// <summary>
	/// 封装 Windows 应用程序桌面工具栏（AppBar），可用于实现类似任务栏的停靠窗口。
	/// 支持 Windows XP 及以上，提供注册、位置协商、通知、状态管理等全功能。
	/// 窗口关闭时自动注销 AppBar，防止崩溃后残留。
	/// </summary>
	public class AppBar: IDisposable
	{
		private static int s_callbackMessageId;
		private static readonly object s_lock = new object ();
		protected readonly HWND _hwnd;
		private bool _disposed;
		private bool _isRegistered;
		protected HwndSource _hwndSource;
		private HwndSourceHook _hook;
		protected Window _window; // 若从 Window 构造，用于订阅 Closed 事件
		/// <summary>
		/// 使用已有的窗口句柄初始化 AppBar。
		/// </summary>
		public AppBar (HWND hwnd)
		{
			if (!hwnd.IsValid)
				throw new ArgumentException ("无效的窗口句柄", nameof (hwnd));
			_hwnd = hwnd;
			InitializeHook ();
		}
		/// <summary>
		/// 使用 WPF Window 初始化 AppBar（自动获取句柄）。
		/// </summary>
		public AppBar (Window window) : this (GetHwndFromWindow (window))
		{
			if (window == null)
				throw new ArgumentNullException (nameof (window));
			_window = window;
			_window.Closed += OnWindowClosed;
		}
		private void OnWindowClosed (object sender, EventArgs e)
		{
			Dispose ();
		}
		public bool IsRegistered => _isRegistered && !_disposed;
		private AppBarEdge _currEdge = AppBarEdge.Left;
		/// <summary>
		/// 获取或设置 AppBar 当前停靠的边缘。
		/// 设置会立即调用 SetPos 应用新边缘（使用当前窗口矩形）。
		/// </summary>
		public AppBarEdge Edge
		{
			get
			{
				ThrowIfDisposed ();
				return _currEdge;
			}
			set
			{
				if (IsRegistered)
				{
					ThrowIfDisposed ();
					if (!_isRegistered)
						throw new InvalidOperationException ("AppBar 尚未注册。");
					_currEdge = value;
					SetAppBarPosition (value, _hwnd.Rect);
				}
				else _currEdge = value;
			}
		}
		public SidebarDirection Direction
		{
			get
			{
				switch (Edge)
				{
					case AppBarEdge.Left: return SidebarDirection.Left;
					default:
					case AppBarEdge.Right: return SidebarDirection.Right;
				}
			}
			set
			{
				if (IsRegistered)
				{
					switch (value)
					{
						case SidebarDirection.Left: Edge = AppBarEdge.Left; break;
						case SidebarDirection.Right: Edge = AppBarEdge.Right; break;
					}
				}
				else
				{
					switch (value)
					{
						case SidebarDirection.Left: _currEdge = AppBarEdge.Left; break;
						case SidebarDirection.Right: _currEdge = AppBarEdge.Right; break;
					}
				}
			}
		}
		/// <summary>
		/// 获取或设置 AppBar 的当前矩形（屏幕坐标）。
		/// 设置时会同时更新边缘和矩形。
		/// </summary>
		public RECT Rect
		{
			get
			{
				ThrowIfDisposed ();
				if (!_isRegistered) return _hwnd.Rect;
				var data = CreateAppBarData (AppBarMessage.QueryPos);
				data.rc = _hwnd.Rect;
				SHAppBarMessage ((int)AppBarMessage.QueryPos, ref data);
				return data.rc;
			}
			set
			{
				ThrowIfDisposed ();
				if (!_isRegistered)
					throw new InvalidOperationException ("AppBar 尚未注册。");
				var data = CreateAppBarData (AppBarMessage.QueryPos);
				data.rc = value;
				SHAppBarMessage ((int)AppBarMessage.QueryPos, ref data);
				SetAppBarPosition ((AppBarEdge)data.uEdge, data.rc);
			}
		}
		/// <summary>
		/// 当 AppBar 收到系统通知时触发。
		/// </summary>
		public event EventHandler<AppBarNotificationEventArgs> Notified;
		public int Message { get; private set; }
		/// <summary>
		/// 注册 AppBar。系统将为此窗口保留工作区空间。
		/// 调用后窗口大小/位置由系统协商决定。
		/// </summary>
		/// <param name="edge">停靠边缘</param>
		/// <param name="desiredRect">期望的矩形（可空），若空则使用窗口当前矩形</param>
		public bool Register (AppBarEdge ?edge, RECT? desiredRect = null)
		{
			ThrowIfDisposed ();
			if (_isRegistered)
				return true;
			int callbackMsg = GetCallbackMessageId ();
			Message = callbackMsg;
			RECT rect = desiredRect ?? _hwnd.Rect;
			var newData = new APPBARDATA ();
			newData.cbSize = Marshal.SizeOf (typeof (APPBARDATA));
			newData.hWnd = _hwnd.Handle;        // 窗口句柄
			newData.uCallBackMessage = callbackMsg;
			newData.uEdge = (int)edge;
			newData.rc = rect;
			int retVal = SHAppBarMessage ((int)AppBarMessage.New, ref newData);
			if (retVal == 0)
				return false;   
			var queryData = new APPBARDATA ();
			queryData.cbSize = Marshal.SizeOf (typeof (APPBARDATA));
			queryData.hWnd = _hwnd.Handle;
			queryData.uCallBackMessage = callbackMsg;
			queryData.uEdge = (int)edge;
			queryData.rc = rect;
			SHAppBarMessage ((int)AppBarMessage.QueryPos, ref queryData);
			SetAppBarPosition (edge ?? _currEdge, queryData.rc);
			_currEdge = edge ?? _currEdge;
			_isRegistered = true;
			return true;
		}
		private int RegisterCallBackMessage ()
		{
			string uniqueMessageString = Guid.NewGuid ().ToString ();
			return RegisterWindowMessageW (uniqueMessageString);
		}
		public void Register (SidebarDirection ?direction, RECT? desiredRect = null)
		{
			switch (direction ?? Direction)
			{
				case SidebarDirection.Left: Register (AppBarEdge.Left, desiredRect); break;
				case SidebarDirection.Right: Register (AppBarEdge.Right, desiredRect); break;
			}
		}
		/// <summary>
		/// 注销 AppBar，恢复系统工作区，并允许其他窗口占用该区域。
		/// 可多次调用。
		/// </summary>
		public virtual void Unregister ()
		{
			if (!_isRegistered) return;
			var data = CreateAppBarData (AppBarMessage.Remove);
			SHAppBarMessage ((int)AppBarMessage.Remove, ref data);
			_isRegistered = false;
		}
		/// <summary>
		/// 查询指定边缘和矩形的建议位置（不改变窗口）。
		/// </summary>
		public RECT QueryPos (AppBarEdge edge, RECT proposedRect)
		{
			ThrowIfDisposed ();
			var data = CreateAppBarData (AppBarMessage.QueryPos);
			data.uEdge = (int)edge;
			data.rc = proposedRect;
			SHAppBarMessage ((int)AppBarMessage.QueryPos, ref data);
			return data.rc;
		}
		/// <summary>
		/// 设置 AppBar 的停靠边缘和屏幕矩形（发送 ABM_SETPOS）。
		/// 窗口将被移动到指定位置。
		/// </summary>
		/// <param name="edge">新的停靠边缘</param>
		/// <param name="rect">目标屏幕坐标矩形</param>
		public void SetPos (AppBarEdge edge, RECT rect)
		{
			ThrowIfDisposed ();
			if (!_isRegistered)
				throw new InvalidOperationException ("AppBar 尚未注册。");
			SetAppBarPosition (edge, rect);
		}
		/// <summary>
		/// 设置 AppBar 的屏幕矩形，保持当前停靠边缘不变（发送 ABM_SETPOS）。
		/// 窗口将被移动到指定位置。
		/// </summary>
		/// <param name="rect">目标屏幕坐标矩形</param>
		public void SetPos (RECT rect)
		{
			ThrowIfDisposed ();
			if (!_isRegistered)
				throw new InvalidOperationException ("AppBar 尚未注册。");
			SetAppBarPosition (Edge, rect);
		}
		/// <summary>
		/// 激活 AppBar（使其接收输入焦点等）。
		/// </summary>
		public void Activate ()
		{
			ThrowIfDisposed ();
			if (!_isRegistered)
				throw new InvalidOperationException ("AppBar 尚未注册。");
			var data = CreateAppBarData (AppBarMessage.Activate);
			SHAppBarMessage ((int)AppBarMessage.Activate, ref data);
		}
		/// <summary>
		/// 获取当前 AppBar 的 ABM_GETSTATE 标志。
		/// </summary>
		public int GetState ()
		{
			ThrowIfDisposed ();
			var data = CreateAppBarData (AppBarMessage.GetState);
			return SHAppBarMessage ((int)AppBarMessage.GetState, ref data);
		}
		/// <summary>
		/// 设置 AppBar 的状态标志（通常用于设置始终位于顶部等行为）。
		/// </summary>
		public void SetState (int state)
		{
			ThrowIfDisposed ();
			var data = CreateAppBarData (AppBarMessage.SetState);
			data.lParam = state;
			SHAppBarMessage ((int)AppBarMessage.SetState, ref data);
		}
		/// <summary>
		/// 通知系统 AppBar 的位置已改变（用于自定义位置变更后刷新工作区）。
		/// </summary>
		public void WindowPosChanged ()
		{
			ThrowIfDisposed ();
			if (!_isRegistered)
				throw new InvalidOperationException ("AppBar 尚未注册。");
			var data = CreateAppBarData (AppBarMessage.WindowPosChanged);
			SHAppBarMessage ((int)AppBarMessage.WindowPosChanged, ref data);
		}
		/// <summary>
		/// 获取任务栏（系统 AppBar）的停靠边缘和矩形。
		/// </summary>
		public static void GetTaskBarPos (out AppBarEdge edge, out RECT rect)
		{
			var data = CreateAppBarDataStatic (AppBarMessage.GetTaskBarPos);
			SHAppBarMessage ((int)AppBarMessage.GetTaskBarPos, ref data);
			edge = (AppBarEdge)data.uEdge;
			rect = data.rc;
		}
		/// <summary>
		/// 获取指定边缘的自动隐藏栏句柄。
		/// </summary>
		public static HWND GetAutoHideBar (AppBarEdge edge)
		{
			var data = CreateAppBarDataStatic (AppBarMessage.GetAutoHideBar);
			data.uEdge = (int)edge;
			SHAppBarMessage ((int)AppBarMessage.GetAutoHideBar, ref data);
			return new HWND (data.hWnd);
		}
		/// <summary>
		/// 设置指定边缘的自动隐藏栏句柄。
		/// </summary>
		public static void SetAutoHideBar (AppBarEdge edge, HWND hwnd)
		{
			var data = CreateAppBarDataStatic (AppBarMessage.SetAutoHideBar);
			data.uEdge = (int)edge;
			data.hWnd = hwnd.Handle;
			SHAppBarMessage ((int)AppBarMessage.SetAutoHideBar, ref data);
		}
		private void SetAppBarPosition (AppBarEdge edge, RECT rect)
		{
			var setData = CreateAppBarData (AppBarMessage.SetPos);
			setData.uEdge = (int)edge;
			setData.rc = rect;
			SHAppBarMessage ((int)AppBarMessage.SetPos, ref setData);
			_hwnd.Move (rect.Left, rect.Top, rect.Width, rect.Height, repaint: true);
		}
		private APPBARDATA CreateAppBarData (AppBarMessage msg)
		{
			var data = APPBARDATA.Create ();
			data.hWnd = _hwnd.Handle;
			data.uCallBackMessage = GetCallbackMessageId ();
			return data;
		}
		private static APPBARDATA CreateAppBarDataStatic (AppBarMessage msg)
		{
			var data = APPBARDATA.Create ();
			data.uCallBackMessage = GetCallbackMessageId ();
			return data;
		}
		private static int GetCallbackMessageId ()
		{
			if (s_callbackMessageId == 0)
			{
				lock (s_lock)
				{
					if (s_callbackMessageId == 0)
						s_callbackMessageId = RegisterWindowMessageW ("AppBarMessage");
				}
			}
			return s_callbackMessageId;
		}
		private void InitializeHook ()
		{
			_hwndSource = HwndSource.FromHwnd (_hwnd.Handle);
			if (_hwndSource != null)
			{
				_hook = new HwndSourceHook (WndProcHook);
				_hwndSource.AddHook (_hook);
			}
		}
		private IntPtr WndProcHook (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == GetCallbackMessageId ())
			{
				var notification = (AppBarNotification)wParam.ToInt32 ();
				OnNotified (notification, lParam);
				handled = true;
			}
			return IntPtr.Zero;
		}
		protected virtual void OnNotified (AppBarNotification notification, IntPtr lParam)
		{
			var handler = Notified;
			if (handler != null)
				handler (this, new AppBarNotificationEventArgs (notification, lParam));
		}
		private static HWND GetHwndFromWindow (Window window)
		{
			var helper = new WindowInteropHelper (window);
			IntPtr handle = helper.EnsureHandle ();
			return new HWND (handle);
		}
		private void ThrowIfDisposed ()
		{
			if (_disposed)
				throw new ObjectDisposedException (nameof (AppBar));
		}
		#region IDisposable Support
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		protected virtual void Dispose (bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_window != null)
						_window.Closed -= OnWindowClosed;
					if (_hwndSource != null && _hook != null)
						_hwndSource.RemoveHook (_hook);
				}
				Unregister ();
				_disposed = true;
			}
		}
		~AppBar ()
		{
			Dispose (false);
		}
		#endregion
	}
	/// <summary>
	/// 为 <see cref="AppBar.Notified"/> 事件提供数据。
	/// </summary>
	public class AppBarNotificationEventArgs: EventArgs
	{
		public AppBarNotification Notification { get; private set; }
		public IntPtr Data { get; private set; }
		public AppBarNotificationEventArgs (AppBarNotification notification, IntPtr data)
		{
			Notification = notification;
			Data = data;
		}
	}
}