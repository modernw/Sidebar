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
	/// PanelFace2.xaml 的交互逻辑
	/// </summary>
	public partial class PanelFace2: UserControl, IUnionTimeSetter
	{
		public PanelFace2 ()
		{
			InitializeComponent ();
		}
		public DateTime CurrentTime
		{
			set
			{
				ClockCtrl.CurrentTime = value;
				ClockShortTime.CurrentTime = value;
				ClockSmallCalendar.CurrentDate = value;
			}
		}
		public string CurrentTimeZone
		{
			set
			{
				ClockCtrl.TimeZoneId = value;
				ClockSmallCalendar.TimeZoneId = value;
				ClockShortTime.TimeZoneId = value;
			}
		}
	}
}
