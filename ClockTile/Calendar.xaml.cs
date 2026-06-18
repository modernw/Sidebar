using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ClockTile
{
	public partial class Calendar: UserControl
	{
		private bool isUpdating = false;
		private DateTime _currentUtcDate = DateTime.UtcNow;

		public Calendar ()
		{
			InitializeComponent ();
			// 初始化图片（根据当前 CurrentDate 对应的年月）
			UpdateFromCurrentDate ();
		}

		// 当前日期（UTC 时间）
		public static readonly DependencyProperty CurrentDateProperty =
			DependencyProperty.Register ("CurrentDate", typeof (DateTime), typeof (Calendar),
				new FrameworkPropertyMetadata (DateTime.UtcNow, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentDateChanged));

		public DateTime CurrentDate
		{
			get { return (DateTime)GetValue (CurrentDateProperty); }
			set { SetValue (CurrentDateProperty, value); }
		}

		// 时区 ID
		public static readonly DependencyProperty TimeZoneIdProperty =
			DependencyProperty.Register ("TimeZoneId", typeof (string), typeof (Calendar),
				new FrameworkPropertyMetadata (null, OnTimeZoneIdChanged));

		public string TimeZoneId
		{
			get { return (string)GetValue (TimeZoneIdProperty); }
			set { SetValue (TimeZoneIdProperty, value); }
		}

		private static void OnCurrentDateChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as Calendar;
			if (control != null && !control.isUpdating)
			{
				control._currentUtcDate = (DateTime)e.NewValue;
				control.UpdateFromCurrentDate ();
			}
		}

		private static void OnTimeZoneIdChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as Calendar;
			control?.UpdateFromCurrentDate ();
		}

		// 根据 CurrentDate（UTC）和时区刷新日历显示和月份图片
		private void UpdateFromCurrentDate ()
		{
			if (MonthCalendar == null) return;

			DateTime targetDate = ConvertToTargetTimeZone (_currentUtcDate, TimeZoneId);
			isUpdating = true;

			// 只关心年月，避免设置日期部分导致异常
			if (MonthCalendar.DisplayDate.Year != targetDate.Year || MonthCalendar.DisplayDate.Month != targetDate.Month)
			{
				MonthCalendar.DisplayDate = targetDate;
			}
			SetMonthDisplayImage (targetDate.Month);
			isUpdating = false;
		}

		// 当用户通过日历控件切换月份时，更新 CurrentDate（UTC）
		private void MonthCalendar_DisplayDateChanged (object sender, CalendarDateChangedEventArgs e)
		{
			if (isUpdating) return;

			DateTime newDisplayDate = MonthCalendar.DisplayDate;
			// 将显示日期（本地时间，即目标时区时间）转换回 UTC
			DateTime newUtcDate = ConvertToUtc (newDisplayDate, TimeZoneId);
			_currentUtcDate = newUtcDate;

			isUpdating = true;
			CurrentDate = newUtcDate;  // 触发属性变更，通知外部
			SetMonthDisplayImage (newDisplayDate.Month);
			isUpdating = false;
		}

		// 辅助：将 UTC 时间转换到目标时区
		private DateTime ConvertToTargetTimeZone (DateTime utcTime, string timeZoneId)
		{
			if (string.IsNullOrWhiteSpace (timeZoneId))
			{
				return TimeZoneInfo.ConvertTime (utcTime, TimeZoneInfo.Local);
			}
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

		// 辅助：将目标时区下的本地时间转换回 UTC
		private DateTime ConvertToUtc (DateTime localTimeInTargetZone, string timeZoneId)
		{
			if (string.IsNullOrWhiteSpace (timeZoneId))
				return localTimeInTargetZone.ToUniversalTime ();
			try
			{
				var tz = TimeZoneInfo.FindSystemTimeZoneById (timeZoneId);
				return TimeZoneInfo.ConvertTimeToUtc (localTimeInTargetZone, tz);
			}
			catch
			{
				return localTimeInTargetZone.ToUniversalTime ();
			}
		}

		// 保留原有的月份图片设置方法（略作优化）
		private int lastMonthValue = 0;

		public void SetMonthDisplayImage (int month)
		{
			if (month < 1 || month > 12) return;
			if (lastMonthValue == month) return;
			MonthImage.Source = ImageSourceManager.GetImageNoCache (
				System.IO.Path.Combine (Tile.TileFolder.FolderPath, $"Images\\MonthPictures\\{month}.png")
			);
			lastMonthValue = month;
		}
		public static readonly DependencyProperty AutoUpdateProperty =
		 DependencyProperty.Register ("AutoUpdate", typeof (bool), typeof (Calendar),
			 new FrameworkPropertyMetadata (false, OnAutoUpdatePropertyChanged));

		public bool AutoUpdate
		{
			get { return (bool)GetValue (AutoUpdateProperty); }
			set { SetValue (AutoUpdateProperty, value); }
		}
		private static void OnAutoUpdatePropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as Calendar;
			control?.OnAutoUpdateChanged ();
		}
		private void OnAutoUpdateChanged ()
		{
			if (AutoUpdate)
				SubscribeSystemEvents ();
			else
				UnsubscribeSystemEvents ();
		}
		private void SubscribeSystemEvents ()
		{
			SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
			SystemEvents.TimeChanged += SystemEvents_TimeChanged;
		}
		private void UnsubscribeSystemEvents ()
		{
			SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
		}
		private void SystemEvents_TimeChanged (object sender, EventArgs e)
		{
			// 确保在 UI 线程更新
			Application.Current.Dispatcher.Invoke (new Action (() => {
				DateTime newUtcNow = DateTime.UtcNow;
				if (newUtcNow.Date != _currentUtcDate.Date)
				{
					CurrentDate = newUtcNow;   // 会触发 OnCurrentDateChanged，自动更新 _currentUtcDate 和界面
				}
			}));
		}
		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{
			OnAutoUpdateChanged ();
		}
		private void UserControl_Unloaded (object sender, RoutedEventArgs e)
		{
			UnsubscribeSystemEvents ();
		}
	}
}