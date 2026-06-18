using System;
using System.Collections.Generic;
using System.Globalization;
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
	/// PanelFace7.xaml 的交互逻辑
	/// </summary>
	public partial class PanelFace7: UserControl, IUnionTimeSetter
	{
		public PanelFace7 ()
		{
			InitializeComponent ();
		}
		private DateTime _utcTime = new DateTime ();
		private string _timeZoneId = null;
		public DateTime CurrentTime
		{
			set
			{
				_utcTime = value;
				ClockPart.CurrentTime = value;
				ShortTime.CurrentTime = value;
				UpdateDisplay ();
			}
		}
		public string CurrentTimeZone
		{
			set
			{
				_timeZoneId = value;
				ClockPart.TimeZoneId = value;
				ShortTime.TimeZoneId = value;
				UpdateDisplay ();
			}
		}
		private void UpdateDisplay ()
		{
			ShortTime.CurrentTime = _utcTime;
			ShortTime.TimeZoneId = _timeZoneId;
			ClockPart.CurrentTime = _utcTime;
			ClockPart.TimeZoneId = _timeZoneId;
			DateTime targetTime = ConvertToTargetTimeZone (_utcTime, _timeZoneId);
			Date.Text = GetLocalizedMonthDay (targetTime);
		}
		private DateTime ConvertToTargetTimeZone (DateTime utcTime, string timeZoneId)
		{
			if (string.IsNullOrWhiteSpace (timeZoneId))
				return TimeZoneInfo.ConvertTime (utcTime, TimeZoneInfo.Local);
			try
			{
				var tz = TimeZoneInfo.FindSystemTimeZoneById (timeZoneId);
				return TimeZoneInfo.ConvertTimeFromUtc (utcTime, tz);
			}
			catch
			{
				return utcTime.ToLocalTime ();
			}
		}
		private string GetLocalizedMonthDay (DateTime date)
		{
			try
			{
				return date.ToString ("m", CultureInfo.CurrentUICulture);
			}
			catch
			{
				return date.ToString ("MMMM d", CultureInfo.InvariantCulture);
			}
		}
	}
}
