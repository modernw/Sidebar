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
using ClockTile;
namespace ClockTile.Faces
{
	/// <summary>
	/// PanelFace1.xaml 的交互逻辑
	/// </summary>
	public partial class PanelFace1: UserControl, IUnionTimeSetter
	{
		private string currFormat = "h:mm tt, MMM d";
		public PanelFace1 ()
		{
			InitializeComponent ();
			currFormat = Tile.TileFolder.StringResources.SuitableResource ("FORMAT_MD_HMT") ?? currFormat;
		}
		private DateTime currTime = new DateTime ();
		private string timeZone = "";
		public DateTime CurrentTime
		{
			set
			{
				ClockCtrl.CurrentTime = value;
				currTime = value;
				UpdateDateTimeString (currTime, timeZone);
			}
		}
		public string CurrentTimeZone
		{
			set
			{
				ClockCtrl.TimeZoneId = value;
				timeZone = value;
				UpdateDateTimeString (currTime, timeZone);
			}
		}
		private void UpdateDateTimeString (DateTime ct, string tz = null)
		{
			DateTime localTime = ConvertUtcToTargetTimeZone (ct, tz);
			string pattern = currFormat;
			string formatted = localTime.ToString (pattern, CultureInfo.CurrentCulture);
			if (string.IsNullOrWhiteSpace (formatted)) formatted = localTime.ToString (pattern, CultureInfo.InvariantCulture);
			ClockDateTime.Text = formatted;
		}
		private DateTime ConvertUtcToTargetTimeZone (DateTime utcTime, string timeZoneId)
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
	}
}
