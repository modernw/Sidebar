using System;
using System.Windows;
using System.Windows.Controls;

namespace ClockTile
{
	/// <summary>
	/// TimePicker.xaml 的交互逻辑
	/// </summary>
	public partial class TimePicker: UserControl
	{
		#region 依赖属性

		public static readonly DependencyProperty LocalTimeProperty =
			DependencyProperty.Register ("LocalTime", typeof (DateTime), typeof (TimePicker),
				new FrameworkPropertyMetadata (DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnLocalTimeChanged));

		public static readonly DependencyProperty UTCTimeProperty =
			DependencyProperty.Register ("UTCTime", typeof (DateTime), typeof (TimePicker),
				new FrameworkPropertyMetadata (DateTime.UtcNow, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnUTCTimeChanged));

		public static readonly DependencyProperty TimeZoneIdProperty =
			DependencyProperty.Register ("TimeZoneId", typeof (string), typeof (TimePicker),
				new PropertyMetadata (null, OnTimeZoneIdChanged));

		public static readonly DependencyProperty EnableSecondProperty =
			DependencyProperty.Register ("EnableSecond", typeof (bool), typeof (TimePicker),
				new PropertyMetadata (true, OnEnableSecondChanged));

		#endregion

		#region 路由事件

		public static readonly RoutedEvent TimeChangedEvent =
			EventManager.RegisterRoutedEvent ("TimeChanged", RoutingStrategy.Bubble,
				typeof (RoutedEventHandler), typeof (TimePicker));

		public event RoutedEventHandler TimeChanged
		{
			add { AddHandler (TimeChangedEvent, value); }
			remove { RemoveHandler (TimeChangedEvent, value); }
		}

		#endregion

		#region 属性

		/// <summary>
		/// 指定时区的本地时间（用于显示）
		/// </summary>
		public DateTime LocalTime
		{
			get { return (DateTime)GetValue (LocalTimeProperty); }
			set { SetValue (LocalTimeProperty, value); }
		}

		/// <summary>
		/// UTC 时间（用于存储）
		/// </summary>
		public DateTime UTCTime
		{
			get { return (DateTime)GetValue (UTCTimeProperty); }
			set { SetValue (UTCTimeProperty, value); }
		}

		/// <summary>
		/// 时区 ID（如果为空则使用本地时区）
		/// </summary>
		public string TimeZoneId
		{
			get { return (string)GetValue (TimeZoneIdProperty); }
			set { SetValue (TimeZoneIdProperty, value); }
		}

		/// <summary>
		/// 是否启用秒钟调整
		/// </summary>
		public bool EnableSecond
		{
			get { return (bool)GetValue (EnableSecondProperty); }
			set { SetValue (EnableSecondProperty, value); }
		}

		#endregion

		private bool _isUpdating = false;
		private TimeZoneInfo _currentTimeZone;

		public TimePicker ()
		{
			InitializeComponent ();
			InitializeControls ();
		}

		#region 初始化

		private void InitializeControls ()
		{
			// 初始化时区
			UpdateTimeZone ();

			// 在代码中动态设置 NumberInputBox 属性
			HourSelect.Minimum = 0;
			HourSelect.Maximum = 23;
			HourSelect.Increment = 1;
			HourSelect.DecimalPlaces = 0;
			HourSelect.Value = 0;

			MinuteSelect.Minimum = 0;
			MinuteSelect.Maximum = 59;
			MinuteSelect.Increment = 1;
			MinuteSelect.DecimalPlaces = 0;
			MinuteSelect.Value = 0;

			SecondSelect.Minimum = 0;
			SecondSelect.Maximum = 59;
			SecondSelect.Increment = 1;
			SecondSelect.DecimalPlaces = 0;
			SecondSelect.Value = 0;

			// 订阅控件值改变事件
			HourSelect.ValueChanged += TimeComponent_ValueChanged;
			MinuteSelect.ValueChanged += TimeComponent_ValueChanged;
			SecondSelect.ValueChanged += TimeComponent_ValueChanged;

			// 更新秒钟 UI 的可见性
			UpdateSecondVisibility ();

			// 初始化时间
			UpdateUIFromLocalTime ();
		}

		#endregion

		#region 事件处理

		private void TimeComponent_ValueChanged (object sender, RoutedEventArgs e)
		{
			if (_isUpdating)
				return;

			UpdateTimeFromUI ();
		}

		private static void OnLocalTimeChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimePicker control = (TimePicker)d;
			control.UpdateUIFromLocalTime ();
		}

		private static void OnUTCTimeChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimePicker control = (TimePicker)d;
			control.UpdateUIFromUTCTime ();
		}

		private static void OnTimeZoneIdChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimePicker control = (TimePicker)d;
			control.UpdateTimeZone ();
			// 时区改变后，需要重新从 UTC 时间转换显示
			control.UpdateUIFromUTCTime ();
		}

		private static void OnEnableSecondChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimePicker control = (TimePicker)d;
			control.UpdateSecondVisibility ();
			// 如果禁用秒钟，需要重新计算时间（秒为 0）
			if (!(bool)e.NewValue)
			{
				control.UpdateTimeFromUI ();
			}
		}

		#endregion

		#region 公共方法

		/// <summary>
		/// 设置为系统当前时间（转换为指定时区）
		/// </summary>
		public void SetCurrentTime ()
		{
			DateTime utcNow = DateTime.UtcNow;
			UTCTime = utcNow;
			LocalTime = TimeZoneInfo.ConvertTimeFromUtc (utcNow, _currentTimeZone);
		}

		/// <summary>
		/// 设置为系统 UTC 时间
		/// </summary>
		public void SetCurrentUTCTime ()
		{
			UTCTime = DateTime.UtcNow;
		}

		/// <summary>
		/// 获取时间跨度
		/// </summary>
		public TimeSpan GetTimeSpan ()
		{
			int seconds = EnableSecond ? (int)SecondSelect.Value : 0;
			return new TimeSpan ((int)HourSelect.Value, (int)MinuteSelect.Value, seconds);
		}

		/// <summary>
		/// 设置时间（指定时区的本地时间）
		/// </summary>
		public void SetTime (int hours, int minutes, int seconds = 0)
		{
			_isUpdating = true;
			try
			{
				HourSelect.Value = Math.Max (0, Math.Min (23, hours));
				MinuteSelect.Value = Math.Max (0, Math.Min (59, minutes));

				if (EnableSecond)
				{
					SecondSelect.Value = Math.Max (0, Math.Min (59, seconds));
				}
				else
				{
					SecondSelect.Value = 0;
				}

				UpdateTimeFromUI ();
			}
			finally
			{
				_isUpdating = false;
			}
		}

		#endregion

		#region 私有方法

		/// <summary>
		/// 更新时区信息
		/// </summary>
		private void UpdateTimeZone ()
		{
			try
			{
				if (String.IsNullOrWhiteSpace (TimeZoneId))
				{
					// 使用本地时区
					_currentTimeZone = TimeZoneInfo.Local;
				}
				else
				{
					// 根据 TimeZoneId 获取时区信息
					_currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById (TimeZoneId);
				}
			}
			catch (TimeZoneNotFoundException)
			{
				// 如果时区 ID 无效，回退到本地时区
				_currentTimeZone = TimeZoneInfo.Local;
			}
			catch (InvalidTimeZoneException)
			{
				// 如果时区信息无效，回退到本地时区
				_currentTimeZone = TimeZoneInfo.Local;
			}
		}

		/// <summary>
		/// 更新秒钟控件的可见性
		/// </summary>
		private void UpdateSecondVisibility ()
		{
			Visibility secondVisibility = EnableSecond ? Visibility.Visible : Visibility.Collapsed;
			SecondSelect.Visibility = secondVisibility;
			MinuteSecondDivide.Visibility = secondVisibility;
		}

		/// <summary>
		/// 从本地时间（指定时区）更新 UI
		/// </summary>
		private void UpdateUIFromLocalTime ()
		{
			if (_isUpdating)
				return;

			_isUpdating = true;
			try
			{
				HourSelect.Value = LocalTime.Hour;
				MinuteSelect.Value = LocalTime.Minute;

				if (EnableSecond)
				{
					SecondSelect.Value = LocalTime.Second;
				}
				else
				{
					SecondSelect.Value = 0;
				}
			}
			finally
			{
				_isUpdating = false;
			}
		}

		/// <summary>
		/// 从 UTC 时间转换为指定时区，然后更新 UI
		/// </summary>
		private void UpdateUIFromUTCTime ()
		{
			if (_isUpdating)
				return;

			_isUpdating = true;
			try
			{
				// 将 UTC 时间转换为指定时区的本地时间
				DateTime localTimeInTimeZone = TimeZoneInfo.ConvertTimeFromUtc (UTCTime, _currentTimeZone);
				LocalTime = localTimeInTimeZone;

				HourSelect.Value = localTimeInTimeZone.Hour;
				MinuteSelect.Value = localTimeInTimeZone.Minute;

				if (EnableSecond)
				{
					SecondSelect.Value = localTimeInTimeZone.Second;
				}
				else
				{
					SecondSelect.Value = 0;
				}
			}
			finally
			{
				_isUpdating = false;
			}
		}

		/// <summary>
		/// 从 UI 更新时间（指定时区的本地时间 → UTC 存储）
		/// </summary>
		private void UpdateTimeFromUI ()
		{
			_isUpdating = true;
			try
			{
				int hours = (int)HourSelect.Value;
				int minutes = (int)MinuteSelect.Value;
				int seconds = EnableSecond ? (int)SecondSelect.Value : 0;

				// 获取指定时区的今天日期
				DateTime today = TimeZoneInfo.ConvertTime (DateTime.Today, _currentTimeZone);

				// 创建指定时区的本地时间
				DateTime localTimeInTimeZone = new DateTime (today.Year, today.Month, today.Day, hours, minutes, seconds);

				// 转换为 UTC 时间（用于存储）
				DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc (localTimeInTimeZone, _currentTimeZone);

				LocalTime = localTimeInTimeZone;
				UTCTime = utcTime;

				// 触发 TimeChanged 路由事件
				RoutedEventArgs args = new RoutedEventArgs (TimeChangedEvent);
				RaiseEvent (args);
			}
			finally
			{
				_isUpdating = false;
			}
		}

		#endregion
	}
}