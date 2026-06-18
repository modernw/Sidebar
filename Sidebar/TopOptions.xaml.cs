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
	/// TopOptions.xaml 的交互逻辑
	/// </summary>
	public partial class TopOptions: UserControl
	{
		public TopOptions ()
		{
			InitializeComponent ();
			TopOptionsLabel.Text = App.ProgramFolder.StringResources.SuitableResource ("SIDEBAR_OPTIONS", "Options");
		}
		// 依赖属性，允许外部设置图标的 Style
		public static readonly DependencyProperty IconStyleProperty =
			DependencyProperty.Register ("IconStyle", typeof (Style), typeof (TopOptions),
				new PropertyMetadata (null, OnIconStyleChanged));
		public Style IconStyle
		{
			get { return (Style)GetValue (IconStyleProperty); }
			set { SetValue (IconStyleProperty, value); }
		}
		private static void OnIconStyleChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (TopOptions)d;
			ctrl.TopOptionsIcon.Style = (Style)e.NewValue;
		}
	}
}
