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

namespace ClockTile
{
	/// <summary>
	/// SmallCalendar.xaml 的交互逻辑
	/// </summary>
	public partial class SmallCalendar: UserControl
	{
		private bool isLoaded = false;
		public SmallCalendar ()
		{
			InitializeComponent ();
		}
		public static readonly DependencyProperty CurrentDateProperty =
			DependencyProperty.Register ("CurrentDate", typeof (DateTime), typeof (SmallCalendar),
				new FrameworkPropertyMetadata (DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentDateChanged));
		public DateTime CurrentDate
		{
			get { return (DateTime)GetValue (CurrentDateProperty); }
			set { SetValue (CurrentDateProperty, value); }
		}
		public static readonly DependencyProperty TimeZoneIdProperty =
		  DependencyProperty.Register ("TimeZoneId", typeof (string), typeof (SmallCalendar),
			  new FrameworkPropertyMetadata (null, OnTimeZoneIdChanged));
		public string TimeZoneId
		{
			get { return (string)GetValue (TimeZoneIdProperty); }
			set { SetValue (TimeZoneIdProperty, value); }
		}
		private static void OnCurrentDateChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as SmallCalendar;
			if (control != null && control.isLoaded)
				control.UpdateDisplay ();
		}
		private static void OnTimeZoneIdChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as SmallCalendar;
			if (control != null && control.isLoaded)
				control.UpdateDisplay ();
		}
		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{
			isLoaded = true;
			UpdateDisplay ();
		}
		private void UpdateDisplay ()
		{
			if (!isLoaded) return;
			DateTime displayDate = CurrentDate;
			if (!string.IsNullOrWhiteSpace (TimeZoneId))
			{
				try
				{
					TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById (TimeZoneId);
					displayDate = TimeZoneInfo.ConvertTimeFromUtc (CurrentDate, tz);
				}
				catch
				{
				}
			}
			else
			{
				displayDate = TimeZoneInfo.ConvertTime (CurrentDate, TimeZoneInfo.Local);
			}
			SCDay.Text = displayDate.Day.ToString ();
			string localizedAbbr = GetLocalizedAbbreviatedDayName (displayDate.DayOfWeek);
			if (localizedAbbr.Length > 3)
				localizedAbbr = GetEnglishAbbreviatedDayName (displayDate.DayOfWeek);
			SCDayInWeek.Text = localizedAbbr.ToUpperInvariant ();
		}
		private string GetLocalizedAbbreviatedDayName (DayOfWeek dayOfWeek)
		{
			CultureInfo culture = CultureInfo.CurrentUICulture;
			string [] abbrDays = culture.DateTimeFormat.AbbreviatedDayNames;
			return abbrDays [(int)dayOfWeek];
		}
		private string GetEnglishAbbreviatedDayName (DayOfWeek dayOfWeek)
		{
			string [] englishAbbr = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
			return englishAbbr [(int)dayOfWeek];
		}
	}
}
