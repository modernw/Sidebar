using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsModern.QuickLaunchTile
{
	/// <summary>
	/// TilePanel.xaml 的交互逻辑
	/// </summary>
	public partial class TilePanel: UserControl, IDisposable 
	{
		private QuickLaunchMonitor _monitor;

		// 直接使用 QuickLaunchItemViewModel 的集合，与 XAML 绑定匹配
		public ObservableCollection<QuickLaunchItemViewModel> TrayIcons { get; private set; }

		public TilePanel ()
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

		}

		public void Cleanup ()
		{
			_monitor?.Dispose ();
		}

		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{

		}

		public void Dispose ()
		{
			_monitor?.Dispose ();
			_monitor = null;
		}
	}
}
