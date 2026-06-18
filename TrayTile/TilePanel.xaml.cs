using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace WindowsModern.TrayTile
{
	public partial class TilePanel: UserControl
	{
		private TrayIconWatcher _watcher;

		public TilePanel ()
		{
			InitializeComponent ();
			this.Loaded += TilePanel_Loaded;
		}
		private void TilePanel_Loaded (object sender, RoutedEventArgs e)
		{
			_watcher = new TrayIconWatcher ();
			SystemTrayContainer.ItemsSource = _watcher.TrayIcons;
			_watcher.Start ();
		}

		private void TilePanel_Unloaded (object sender, RoutedEventArgs e)
		{
			_watcher?.Stop ();
			_watcher?.Dispose ();
			_watcher = null;
		}

		// 预览右键：阻止父菜单并模拟托盘右键（不移动鼠标）
		private void SystemTrayContainer_PreviewMouseRightButtonDown (object sender, MouseButtonEventArgs e)
		{
			ToggleButton btn = FindVisualParent<ToggleButton> (e.OriginalSource as DependencyObject);
			if (btn == null) return;
			TrayIconInfo info = btn.Tag as TrayIconInfo;
			if (info == null || info.OwnerHwnd == IntPtr.Zero) return;

			//TrayIconHelper.SimulateRightClick (info.OwnerHwnd, info.IconId,
			//								  info.CallbackMessage, info.Version);
			e.Handled = true;
		}

		private void TilePanel_ContextMenuOpening (object sender, ContextMenuEventArgs e)
		{
			if (e.OriginalSource is ToggleButton ||
				FindVisualParent<ToggleButton> (e.OriginalSource as DependencyObject) != null)
				e.Handled = true;
		}

		private void ToggleButton_MouseDoubleClick (object sender, MouseButtonEventArgs e)
		{
			ToggleButton btn = sender as ToggleButton;
			if (btn == null) return;
			TrayIconInfo info = btn.Tag as TrayIconInfo;
			if (info == null || info.OwnerHwnd == IntPtr.Zero) return;
			//TrayIconHelper.SimulateLeftClick (info.OwnerHwnd, info.IconId,
			//								 info.CallbackMessage, info.Version);
			e.Handled = true;
		}

		private static T FindVisualParent<T> (DependencyObject child) where T : DependencyObject
		{
			DependencyObject parent = child;
			while (parent != null && !(parent is T))
				parent = VisualTreeHelper.GetParent (parent);
			return parent as T;
		}
	}
}