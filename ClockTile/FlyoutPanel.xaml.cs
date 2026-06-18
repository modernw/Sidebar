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
using System.Windows.Threading;

namespace ClockTile
{
	/// <summary>
	/// FlyoutPanel.xaml 的交互逻辑
	/// </summary>
	public partial class FlyoutPanel: UserControl
	{
		public FlyoutPanel ()
		{
			InitializeComponent ();
			_dtFormat = Tile.TileFolder.StringResources.SuitableResource ("FORMAT_MD_HMT");
			DisplayCalendar.AutoUpdate = true;
			InitValues ();
			Tile.Options.PropertyChanged += Options_PropertyChanged;
		}
		private void InitValues ()
		{
			DisplayClock.TimeZoneId = Tile.Options.TimeZone;
			DisplayShortTime.TimeZoneId = Tile.Options.TimeZone;
			DisplayCalendar.TimeZoneId = Tile.Options.TimeZone;
			DisplayAlarm.Visibility = Tile.Options.EnableAlarm ? Visibility.Visible : Visibility.Hidden;
		}
		private void Options_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "EnableAlarm":
				case "AlarmTime":
					DisplayAlarm.Visibility = Tile.Options.EnableAlarm ? Visibility.Visible : Visibility.Hidden;
					UpdateNextAlarm ();
					break;
				case "TimeZone":
					DisplayClock.TimeZoneId = Tile.Options.TimeZone;
					DisplayShortTime.TimeZoneId = Tile.Options.TimeZone;
					DisplayCalendar.TimeZoneId = Tile.Options.TimeZone;
					UpdateCurrentTime ();
					UpdateNextAlarm ();
					break;
			}
		}
		private DispatcherTimer timer;
		private bool isLoaded = false;
		private string _dtFormat = "";
		private void OnLoaded (object sender, RoutedEventArgs e)
		{
			if (timer != null) return;
			int delay = 1000 - DateTime.UtcNow.Millisecond;
			timer = new DispatcherTimer ();
			timer.Interval = TimeSpan.FromSeconds (1);
			timer.Tick += Timer_Tick;
			timer.Start ();
			UpdateCurrentTime ();
			isLoaded = true;
		}
		private void OnUnloaded (object sender, RoutedEventArgs e)
		{
			if (timer != null)
			{
				timer.Stop ();
				timer = null;
			}
			isLoaded = false;
		}
		private void Timer_Tick (object sender, EventArgs e)
		{
			if (!isLoaded) return;
			UpdateCurrentTime ();
			UpdateNextAlarm ();
		}
		private void UpdateCurrentTime ()
		{
			DateTime nowUtc = DateTime.UtcNow;
			DisplayClock.CurrentTime = nowUtc;
			DisplayShortTime.CurrentTime = nowUtc;
		}
		private void UpdateNextAlarm ()
		{
			if (!Tile.Options.EnableAlarm)
			{
				DisplayAlarmTip.Text = "";
				return;
			}

			DateTime nowUtc = DateTime.UtcNow;
			// 只取时间部分，忽略 AlarmTime 中的日期
			TimeSpan alarmTimeOfDay = Tile.Options.AlarmTime.TimeOfDay;
			DateTime todayAlarm = new DateTime (nowUtc.Year, nowUtc.Month, nowUtc.Day,
											   alarmTimeOfDay.Hours, alarmTimeOfDay.Minutes, alarmTimeOfDay.Seconds,
											   DateTimeKind.Utc);
			DateTime nextUtc = todayAlarm > nowUtc ? todayAlarm : todayAlarm.AddDays (1);

			// 一次性闹钟响过后，由外部将 EnableAlarm 设为 false，这里无需区分 EveryDay
			DateTime localTime = ConvertUtcToTargetTimeZone (nextUtc, Tile.Options.TimeZone);
			DisplayAlarmTip.Text = FormatAlarmTime (localTime);
		}
		private DateTime ConvertUtcToTargetTimeZone (DateTime utcTime, string timeZoneId)
		{
			if (string.IsNullOrWhiteSpace (timeZoneId))
				return utcTime.ToLocalTime ();
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
		private string FormatAlarmTime (DateTime localTime)
		{
			string pattern = _dtFormat ?? "h:mm tt, MMMM d";
			try
			{
				return localTime.ToString (pattern, CultureInfo.CurrentCulture);
			}
			catch
			{
				return localTime.ToString (pattern, CultureInfo.InvariantCulture);
			}
		}
	}
}
