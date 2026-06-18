using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

namespace Sidebar
{
	internal struct ScreenInfo
	{
		public int ScreenWidth;
		public int ScreenHeight;
		public int DPI;

		public override bool Equals (object obj)
		{
			if (!(obj is ScreenInfo)) return false;
			var other = (ScreenInfo)obj;
			return ScreenWidth == other.ScreenWidth &&
				   ScreenHeight == other.ScreenHeight &&
				   DPI == other.DPI;
		}

		public override int GetHashCode ()
		{
			return ScreenWidth.GetHashCode () ^ ScreenHeight.GetHashCode () ^ DPI.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("Screen({0}x{1}, DPI:{2})", ScreenWidth, ScreenHeight, DPI);
		}
	}

	/// <summary>屏幕变化检测结果</summary>
	internal struct ScreenChangeResult
	{
		public bool SizeChanged;
		public bool ScaleChanged;
	}

	/// <summary>监听系统屏幕设置变化（分辨率、DPI等），基于 Win32 消息。</summary>
	public static class ScreenChangeNotifier
	{
		private const int WM_DISPLAYCHANGE = 0x007E;      // 分辨率/显示器数量改变
		private const int WM_SETTINGCHANGE = 0x001A;      // 系统设置改变（可能含 DPI）
		private const int WM_DPICHANGED = 0x02E0;         // DPI 改变（Win8.1+）

		private static HwndSource _hwndSource;
		private static int _lastWorkingAreaHeight;
		private static int _lastWorkingAreaTop;

		/// <summary>屏幕分辨率或显示器数量变化时触发。</summary>
		public static event EventHandler SizeChanged;

		/// <summary>屏幕缩放比例（DPI）变化时触发。</summary>
		public static event EventHandler ScaleChanged;

		public static event EventHandler WorkingAreaHeightChanged;

		/// <summary>维护各屏幕的信息缓存，用于检测真正的改变</summary>
		private static Dictionary<int, ScreenInfo> _screenInfoCache = new Dictionary<int, ScreenInfo> ();

		/// <summary>开始监听。必须在 UI 线程调用，需传入有效窗口。</summary>
		public static void StartListening (Window window)
		{
			if (_hwndSource != null) return;

			// 初始化缓存
			InitializeScreenCache ();

			if (window.IsLoaded)
			{
				AttachHook (window);
			}
			else
			{
				window.Loaded += new RoutedEventHandler (delegate (object s, RoutedEventArgs e)
				{
					AttachHook (window);
				});
			}
		}

		/// <summary>停止监听。</summary>
		public static void StopListening ()
		{
			if (_hwndSource != null)
			{
				_hwndSource.RemoveHook (WndProc);
				_hwndSource.Dispose ();
				_hwndSource = null;
			}
			_screenInfoCache.Clear ();
		}

		private static void AttachHook (Window window)
		{
			if (_hwndSource != null) return;
			var hwnd = new WindowInteropHelper (window).Handle;
			_hwndSource = HwndSource.FromHwnd (hwnd);
			_hwndSource.AddHook (WndProc);
			var currScreen = (ScreenHelper.GetScreenByHWND (hwnd) ?? Screen.PrimaryScreen);
			_lastWorkingAreaTop = currScreen.WorkingArea.Top;
			_lastWorkingAreaHeight = currScreen.WorkingArea.Height;
		}
		private static void CheckWorkingAreaHeightChange ()
		{
			var currScreen = (ScreenHelper.GetScreenByHWND (_hwndSource.Handle) ?? Screen.PrimaryScreen);
			int currentHeight = currScreen.WorkingArea.Height;
			int currentTop = currScreen.WorkingArea.Top;
			if (currentHeight != _lastWorkingAreaHeight || currentTop != _lastWorkingAreaTop)
			{
				_lastWorkingAreaHeight = currentHeight;
				_lastWorkingAreaTop = currentTop;
				if (WorkingAreaHeightChanged != null)
					WorkingAreaHeightChanged (null, EventArgs.Empty);
			}
		}
		/// <summary>
		/// 初始化屏幕信息缓存
		/// </summary>
		private static void InitializeScreenCache ()
		{
			_screenInfoCache.Clear ();
			int screenIndex = 0;
			foreach (Screen screen in Screen.AllScreens)
			{
				_screenInfoCache [screenIndex] = GetScreenInfo (screen);
				screenIndex++;
			}
		}

		/// <summary>
		/// 从 Screen 对象获取屏幕信息
		/// </summary>
		private static ScreenInfo GetScreenInfo (Screen screen)
		{
			// 获取 DPI（Windows 10+ 支持）
			int dpi = 96; // 默认 DPI
			try
			{
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd (IntPtr.Zero);
				dpi = (int)graphics.DpiX;
				graphics.Dispose ();
			}
			catch
			{
				// 使用默认 DPI
			}

			ScreenInfo info = new ScreenInfo ();
			info.ScreenWidth = screen.Bounds.Width;
			info.ScreenHeight = screen.Bounds.Height;
			info.DPI = dpi;
			return info;
		}

		/// <summary>
		/// 检测屏幕是否有变化，更新缓存
		/// </summary>
		private static ScreenChangeResult DetectScreenChanges ()
		{
			ScreenChangeResult result = new ScreenChangeResult ();
			result.SizeChanged = false;
			result.ScaleChanged = false;

			// 获取当前屏幕信息
			Dictionary<int, ScreenInfo> currentScreens = new Dictionary<int, ScreenInfo> ();
			int screenIndex = 0;
			foreach (Screen screen in Screen.AllScreens)
			{
				currentScreens [screenIndex] = GetScreenInfo (screen);
				screenIndex++;
			}

			// 检查屏幕数量是否改变（添加/移除显示器）
			if (currentScreens.Count != _screenInfoCache.Count)
			{
				result.SizeChanged = true;
				result.ScaleChanged = true;
			}
			else
			{
				// 检查每个屏幕的信息是否改变
				for (int i = 0; i < currentScreens.Count; i++)
				{
					if (!_screenInfoCache.ContainsKey (i))
					{
						result.SizeChanged = true;
						break;
					}

					ScreenInfo oldInfo = _screenInfoCache [i];
					ScreenInfo newInfo = currentScreens [i];

					// 检测分辨率改变
					if (oldInfo.ScreenWidth != newInfo.ScreenWidth || oldInfo.ScreenHeight != newInfo.ScreenHeight)
					{
						result.SizeChanged = true;
					}

					// 检测 DPI 改变
					if (oldInfo.DPI != newInfo.DPI)
					{
						result.ScaleChanged = true;
					}
				}
			}

			// 更新缓存
			if (result.SizeChanged || result.ScaleChanged)
			{
				_screenInfoCache = currentScreens;
			}

			return result;
		}

		private static IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_DISPLAYCHANGE:
					ScreenChangeResult result1 = DetectScreenChanges ();
					if (result1.SizeChanged)
					{
						if (SizeChanged != null)
						{
							SizeChanged (null, EventArgs.Empty);
						}
					}
					if (result1.ScaleChanged)
					{
						if (ScaleChanged != null)
						{
							ScaleChanged (null, EventArgs.Empty);
						}
					}
					break;

				case WM_DPICHANGED:
					ScreenChangeResult result2 = DetectScreenChanges ();
					if (result2.ScaleChanged)
					{
						if (ScaleChanged != null)
						{
							ScaleChanged (null, EventArgs.Empty);
						}
					}
					if (result2.SizeChanged)
					{
						if (SizeChanged != null)
						{
							SizeChanged (null, EventArgs.Empty);
						}
					}
					break;

				case WM_SETTINGCHANGE:
					ScreenChangeResult result3 = DetectScreenChanges ();
					if (result3.SizeChanged)
					{
						if (SizeChanged != null)
						{
							SizeChanged (null, EventArgs.Empty);
						}
					}
					if (result3.ScaleChanged)
					{
						if (ScaleChanged != null)
						{
							ScaleChanged (null, EventArgs.Empty);
						}
					}
					CheckWorkingAreaHeightChange ();
					break;
			}

			return IntPtr.Zero;
		}
	}
}