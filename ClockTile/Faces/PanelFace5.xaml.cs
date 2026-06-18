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

namespace ClockTile.Faces
{
	/// <summary>
	/// PanelFace5.xaml 的交互逻辑
	/// </summary>
	public partial class PanelFace5: UserControl, IUnionTimeSetter
	{
		public PanelFace5 ()
		{
			InitializeComponent ();
		}
		public DateTime CurrentTime
		{
			set
			{
				ClockPart.CurrentTime = value;
				ShortTime.CurrentTime = value;
			}
		}
		public string CurrentTimeZone
		{
			set
			{
				ClockPart.TimeZoneId = value;
				ShortTime.TimeZoneId = value;
			}
		}
	}
}
