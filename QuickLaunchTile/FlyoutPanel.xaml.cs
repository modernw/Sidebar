using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsModern.QuickLaunchTile
{
	/// <summary>
	/// FlyoutPanel.xaml 的交互逻辑
	/// </summary>
	public partial class FlyoutPanel: UserControl, IDisposable
	{
		private QuickLaunchMonitor _monitor;

		// 直接使用 QuickLaunchItemViewModel 的集合，与 XAML 绑定匹配
		public ObservableCollection<QuickLaunchItemViewModel> TrayIcons { get; private set; }

		public FlyoutPanel ()
		{
			InitializeComponent ();
			string quickLaunchFolder = Utils.GetQuickLaunchFolder ();
			_monitor = new QuickLaunchMonitor (quickLaunchFolder, Dispatcher);
			TrayIcons = _monitor.Items; // 现在类型一致，不会报错
			this.DataContext = this;
		}
		private void ToggleButton_MouseDoubleClick (object sender, MouseButtonEventArgs e)
		{
		}

		private void ItemContainer_PreviewMouseRightButtonDown (object sender, MouseButtonEventArgs e)
		{
			// 预留右键菜单处理
		}

		public void Cleanup ()
		{
			_monitor?.Dispose ();
		}

		public void Dispose ()
		{
			_monitor?.Dispose ();
			_monitor = null;
		}

		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{
			Tile.SidebarFeatures.Request (new Sidebar.SidebarRequest (Tile.TileInstance) {
				RequestName = "FlyoutUpdatePosition"
			});
		}

		private void UserControl_SizeChanged (object sender, SizeChangedEventArgs e)
		{
			Tile.SidebarFeatures.Request (new Sidebar.SidebarRequest (Tile.TileInstance) {
				RequestName = "FlyoutUpdatePosition"
			});
		}
	}
}
