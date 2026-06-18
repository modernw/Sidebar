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
	/// PanelFace8.xaml 的交互逻辑
	/// </summary>
	public partial class PanelFace8: UserControl, IUnionTimeSetter
	{
		public PanelFace8 ()
		{
			InitializeComponent ();
			_longDateFormat = Tile.TileFolder.StringResources.SuitableResource ("FORMAT_YMD");
		}
		private DateTime _dateTime = new DateTime ();
		private string _timeZone = "";
		private string _longDateFormat = "";
		public DateTime CurrentTime
		{
			set
			{
				_dateTime = value;
				ClockPart.CurrentTime = value;
				ClockShortTime.CurrentTime = value;
				UpdateDisplay ();
			}
		}
		public string CurrentTimeZone
		{
			set
			{
				_timeZone = value;
				ClockPart.TimeZoneId = value;
				ClockShortTime.TimeZoneId = value;
				UpdateDisplay ();
			}
		}
		private void UpdateDisplay ()
		{
			var _utcTime = _dateTime;
			var _timeZoneId = _timeZone;
			ClockShortTime.CurrentTime = _utcTime;
			ClockShortTime.TimeZoneId = _timeZoneId;
			DateTime targetTime = ConvertToTargetTimeZone (_utcTime, _timeZoneId);
			ClockDayInWeek.Text = GetLocalizedDayOfWeek (targetTime.DayOfWeek);
			ClockDate.Text = GetLocalizedDate (targetTime);
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
				return utcTime;
			}
		}
		private string GetLocalizedDayOfWeek (DayOfWeek dayOfWeek)
		{
			try
			{
				string [] dayNames = CultureInfo.CurrentUICulture.DateTimeFormat.DayNames;
				return dayNames [(int)dayOfWeek];
			}
			catch
			{
				string [] englishDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
				return englishDays [(int)dayOfWeek];
			}
		}
		private string GetLocalizedDate (DateTime date)
		{
			try
			{
				string pattern = _longDateFormat ?? "MMMM d, yyyy";
				return date.ToString (pattern, CultureInfo.CurrentUICulture);
			}
			catch
			{
				return date.ToString ("MMMM d, yyyy", CultureInfo.InvariantCulture);
			}
		}
	}
}
