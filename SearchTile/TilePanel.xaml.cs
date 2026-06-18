using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Sidebar;

namespace WindowsModern.SearchTile
{
	/// <summary>
	/// TilePanel.xaml 的交互逻辑
	/// </summary>
	public partial class TilePanel: UserControl, IDisposable
	{
		private FlyoutPanel flyoutPanel = null;
		private DependencyPropertyDescriptor _tagDescriptor;
		private EventHandler _tagChangedHandler;

		public TilePanel ()
		{
			InitializeComponent ();
			SearchBox.Tag = Tile.TileFolder.StringResources.SuitableResource ("TILE_SEARCHFOR");
			flyoutPanel = new FlyoutPanel ();
			flyoutPanel.TextChanged += FlyoutPanel_TextChanged;
			flyoutPanel.SearchClick += FlyoutPanel_SearchClick;

			// 保存委托实例，以便后续移除
			_tagChangedHandler = (s, e) => UpdateClip ();
			_tagDescriptor = DependencyPropertyDescriptor.FromProperty (
				FrameworkElement.TagProperty,
				typeof (FrameworkElement));
			_tagDescriptor.AddValueChanged (ClipGrid, _tagChangedHandler);
		}

		private void FlyoutPanel_SearchClick (object sender, RoutedEventArgs e)
		{
			InvokeSearch ();
		}

		private void FlyoutPanel_TextChanged (object sender, TextChangedEventArgs e)
		{
			SearchBox.Text = flyoutPanel.Text;
		}

		private void UserControl_SizeChanged (object sender, System.Windows.SizeChangedEventArgs e)
		{
			UpdateClip ();
		}

		private void UpdateClip ()
		{
			if (ClipGrid == null || ClipGrid.ActualWidth <= 0 || ClipGrid.ActualHeight <= 0)
				return;

			double radius = 4;
			if (ClipGrid.Tag != null)
			{
				double parsed;
				if (double.TryParse (ClipGrid.Tag.ToString (), out parsed))
					radius = parsed;
			}

			ClipGrid.Clip = new RectangleGeometry {
				RadiusX = radius,
				RadiusY = radius,
				Rect = new Rect (0, 0, ClipGrid.ActualWidth, ClipGrid.ActualHeight)
			};
		}

		public void OnFlyoutInit (UIElement flyoutContent)
		{
			(flyoutPanel?.Parent as Panel)?.Children?.Clear ();
			(flyoutContent as Panel).Children.Add (flyoutPanel = flyoutPanel ?? new FlyoutPanel ());
			flyoutPanel.Text = SearchBox.Text;
		}

		public void OnFlyoutClose ()
		{
			(flyoutPanel?.Parent as Panel)?.Children?.Clear ();
		}

		public void InvokeSearch ()
		{
			SystemSearch.Search (SearchBox.Text, Tile.TileOptions.Provider);
		}

		public void Dispose ()
		{
			// 移除 Tag 属性监听
			if (_tagDescriptor != null && _tagChangedHandler != null)
			{
				_tagDescriptor.RemoveValueChanged (ClipGrid, _tagChangedHandler);
				_tagDescriptor = null;
				_tagChangedHandler = null;
			}

			// 清理 flyoutPanel 相关事件和引用
			if (flyoutPanel != null)
			{
				flyoutPanel.TextChanged -= FlyoutPanel_TextChanged;
				flyoutPanel.SearchClick -= FlyoutPanel_SearchClick;
				(flyoutPanel.Parent as Panel)?.Children?.Clear ();
				flyoutPanel = null;
			}
		}

		private void SearchBox_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				InvokeSearch ();
				e.Handled = true;
			}
		}
	}
}