using System;
using System.Collections.Generic;
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

namespace Sidebar
{
	/// <summary>
	/// DragPlaceholder.xaml 的交互逻辑
	/// </summary>
	public partial class DragPlaceholder: UserControl
	{
		public DragPlaceholder (double ?height = null)
		{
			InitializeComponent ();
			if (height != null)Height = height ?? double.NaN;
		}
		public static readonly DependencyProperty IsPinnedProperty =
			DependencyProperty.Register (
				"IsPinned",
				typeof (bool),
				typeof (DragPlaceholder),
				new PropertyMetadata (false, OnIsPinnedChanged));
		public bool IsPinned
		{
			get { return (bool)GetValue (IsPinnedProperty); }
			set { SetValue (IsPinnedProperty, value); }
		}
		private static void OnIsPinnedChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tile = (DragPlaceholder)d;
			bool newValue = (bool)e.NewValue;
			tile.UpdatePinnedState (newValue);
		}
		private void UpdatePinnedState (bool pinned)
		{
			if (pinned)
			{
				Splitter.SetResourceReference (StyleProperty, "PinnedTileSplitterPanel");
				DockPanel.SetDock (Splitter, Dock.Top);
			}
			else
			{
				Splitter.SetResourceReference (StyleProperty, "TileSplitterPanel");
				DockPanel.SetDock (Splitter, Dock.Bottom);
			}
		}
		public static readonly DependencyProperty IsLastProperty =
			DependencyProperty.Register ("IsLast", typeof (bool), typeof (DragPlaceholder), new PropertyMetadata (false, OnIsLastChanged));
		public bool IsLast
		{
			get { return (bool)GetValue (IsLastProperty); }
			set { SetValue (IsLastProperty, value); }
		}
		private static void OnIsLastChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tile = (DragPlaceholder)d;
			bool newValue = (bool)e.NewValue;
			tile.UpdateLastStatus (newValue);
		}
		private void UpdateLastStatus (bool newValue)
		{
			Splitter.Visibility = newValue ? Visibility.Hidden : Visibility.Visible;
		}
	}
}
