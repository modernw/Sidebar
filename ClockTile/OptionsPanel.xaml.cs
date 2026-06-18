using System;
using System.Collections.Generic;
using System.IO;
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
using Sidebar;
using ClockTile.Faces;
using System.Diagnostics;

namespace ClockTile
{
	/// <summary>
	/// OptionsPanel.xaml 的交互逻辑
	/// </summary>
	public partial class OptionsPanel: UserControl, IDisposable
	{
		private MediaPlayer mediaPlayer = new MediaPlayer ();
		private bool isPlaying = false;
		private Dictionary<ClockTileFaceType, ClockFacePanel> clockLayouts = new Dictionary<ClockTileFaceType, ClockFacePanel> ();
		public OptionsPanel ()
		{
			InitializeComponent ();
			InitLocaleStrings ();
			InitSelectDatas ();
			InitSettingsValues ();
			mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
			mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
		}
		private void MediaPlayer_MediaFailed (object sender, ExceptionEventArgs e)
		{
			MessageBox.Show (e.ErrorException.Message, e.ErrorException.GetType ().ToString (), MessageBoxButton.OK, MessageBoxImage.Error);
		}
		private void MediaPlayer_MediaEnded (object sender, EventArgs e)
		{
			isPlaying = false;
			PlayButton.Content = PlayButton.Content = "4";
			mediaPlayer.Close ();
		}
		private void MediaPlayer_MediaOpened (object sender, EventArgs e)
		{
			isPlaying = true;
			PlayButton.Content = PlayButton.Content = "<";
		}
		private void InitLocaleStrings ()
		{
			var sr = Tile.TileFolder.StringResources;
			TagAlarm.Header = sr.SuitableResource ("OPTIONS_TAG_ALARM");
			EnableAlarm.Content = sr.SuitableResource ("OPTIONS_ALARM_SET");
			RepeatEveryDay.Content = sr.SuitableResource ("OPTIONS_ALARM_REPEAT");
			SongToPlay.Text = sr.SuitableResource ("OPTIONS_ALARM_STP");
			LabelVolume.Text = sr.SuitableResource ("OPTIONS_ALARM_VOLUME");
			LabelMessage.Text = sr.SuitableResource ("OPTIONS_ALARM_MSG");
			TagFaces.Header = sr.SuitableResource ("OPTIONS_TAG_FACES");
			LabelClockLayout.Text = sr.SuitableResource ("OPTIONS_FACES_CL");
			TagInternational.Header = sr.SuitableResource ("OPTIONS_TAG_INTER");
			LabelTimeZone.Text = sr.SuitableResource ("OPTIONS_INTER_TZ");
			RadioUseLocal.Content = sr.SuitableResource ("OPTIONS_INTER_LOCAL");
			OpenControlPanel.Content = sr.SuitableResource ("OPTIONS_INTER_CTZ");
			UseCuston.Content = sr.SuitableResource ("OPTIONS_INTER_INTER");
		}
		public static IEnumerable<string> GetAudioFilesRelative (string directory)
		{
			if (!Directory.Exists (directory))
				return Enumerable.Empty<string> ();
			var extensions = new HashSet<string> (StringComparer.OrdinalIgnoreCase)
			{
				".wav", ".mp3", ".wma", ".m4a", ".aac", ".flac", ".ogg"
			};
			var allFiles = Directory.EnumerateFiles (directory, "*.*", SearchOption.AllDirectories);
			var audioFiles = allFiles.Where (f => extensions.Contains (System.IO.Path.GetExtension (f)));
			return audioFiles.Select (f => GetRelativePath (directory, f));
		}
		private static string GetRelativePath (string basePath, string fullPath)
		{
			if (!basePath.EndsWith (Path.DirectorySeparatorChar.ToString ()))
				basePath += Path.DirectorySeparatorChar;
			Uri baseUri = new Uri (basePath);
			Uri fullUri = new Uri (fullPath);
			Uri relativeUri = baseUri.MakeRelativeUri (fullUri);
			string relativePath = Uri.UnescapeDataString (relativeUri.ToString ())
									  .Replace ('/', Path.DirectorySeparatorChar);
			return relativePath;
		}
		private void InitSelectDatas ()
		{
			var r = Tile.TileFolder;
			var sf = System.IO.Path.Combine (r.FolderPath, "Sounds");
			var extensions = new HashSet<string> (StringComparer.OrdinalIgnoreCase)
			{
				".wav", ".mp3", ".wma", ".m4a", ".aac", ".flac", ".ogg"
			};
			var audioRelPaths = GetAudioFilesRelative (sf);
			var soundItems = audioRelPaths;
			SoundList.ItemsSource = soundItems;
			foreach (ClockTileFaceType e in Enum.GetValues (typeof (ClockTileFaceType)))
			{
				var face = e.GetClockFace ();
				face.Setter.CurrentTime = DateTime.UtcNow;
				face.Setter.CurrentTimeZone = Tile.Options.TimeZone;
				clockLayouts [e] = face;
			}
			TileLayoutList.ItemsSource = clockLayouts;
			TileLayoutList.SelectedValuePath = "Key";
			TimeZoneList.ItemsSource = TimeZoneInfo.GetSystemTimeZones ();
		}
		private void InitSettingsValues ()
		{
			var pr = Tile.Options;
			EnableAlarm.IsChecked = pr.EnableAlarm;
			AlarmTime.TimeZoneId = pr.TimeZone;
			AlarmTime.UTCTime = pr.AlarmTime;
			RepeatEveryDay.IsChecked = pr.AlarmEveryDay;
			SoundList.SelectedValue = pr.AlarmRing;
			VolumeControl.Value = pr.AlarmVolume * 100;
			AlarmMessage.Text = pr.AlarmMessage;
			TileLayoutList.SelectedValue = pr.ClockDisplayType;
			var tzId = pr.TimeZone;
			if (string.IsNullOrEmpty (tzId))
			{
				RadioUseLocal.IsChecked = true;
				UseCuston.IsChecked = false;
				TimeZoneList.SelectedValue = TimeZoneInfo.Local;
			}
			else
			{
				RadioUseLocal.IsChecked = false;
				UseCuston.IsChecked = true;
				TimeZoneList.SelectedValue = TimeZoneInfo.FindSystemTimeZoneById (tzId);
			}
		}
		public void OkButton_Click (object sender, PropertiesAboutEventArgs e)
		{
			if ((EnableAlarm.IsChecked ?? false) && SoundList.SelectedValue == null)
				throw new InvalidDataException (Tile.TileFolder.StringResources.SuitableResource ("ERROR_NEEDRING"));
			if (TileLayoutList.SelectedValue == null)
				throw new InvalidDataException (Tile.TileFolder.StringResources.SuitableResource ("ERROR_LAYOUTNOTSET"));
			if ((UseCuston.IsChecked ?? false) && TimeZoneList.SelectedValue == null)
				throw new InvalidDataException (Tile.TileFolder.StringResources.SuitableResource ("ERROR_INVALIDTIMEZONE"));
			var pr = Tile.Options;
			if (EnableAlarm.IsChecked != pr.EnableAlarm)
				pr.EnableAlarm = EnableAlarm.IsChecked ?? false;
			if (AlarmTime.UTCTime != pr.AlarmTime)
				pr.AlarmTime = AlarmTime.UTCTime;
			if (RepeatEveryDay.IsChecked != pr.AlarmEveryDay)
				pr.AlarmEveryDay = RepeatEveryDay.IsChecked ?? false;
			if (SoundList.SelectedValue as string != pr.AlarmRing)
				pr.AlarmRing = SoundList.SelectedValue as string;
			if (VolumeControl.Value * 0.01 != pr.AlarmVolume)
				pr.AlarmVolume = VolumeControl.Value * 0.01;
			if (AlarmMessage.Text != pr.AlarmMessage)
				pr.AlarmMessage = AlarmMessage.Text;
			if (pr.ClockDisplayType != (ClockTileFaceType)TileLayoutList.SelectedValue)
				pr.ClockDisplayType = (ClockTileFaceType)TileLayoutList.SelectedValue;
			string newTimeZone;
			if (RadioUseLocal.IsChecked ?? false) newTimeZone = "";
			else if (UseCuston.IsChecked ?? false)
			{
				newTimeZone = (TimeZoneList.SelectedValue as TimeZoneInfo).Id;
			}
			else newTimeZone = "";
			if (!newTimeZone.NEquals (pr.TimeZone))
				pr.TimeZone = newTimeZone;
		}
		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{

		}
		private void PlayButton_Click (object sender, RoutedEventArgs e)
		{
			if (isPlaying)
			{
				mediaPlayer?.Stop ();
				isPlaying = false;
				PlayButton.Content = PlayButton.Content = "4";
				mediaPlayer.Close ();
				return;
			}
			var r = Tile.TileFolder;
			var sf = System.IO.Path.Combine (r.FolderPath, "Sounds");
			var selected = SoundList.SelectedValue as string;
			if (selected == null) return;
			var fp = Path.Combine (sf, selected);
			mediaPlayer.Open (new Uri (fp, UriKind.RelativeOrAbsolute));
			mediaPlayer.Play ();
		}
		private void UserControl_Unloaded (object sender, RoutedEventArgs e)
		{
			mediaPlayer.Stop ();
			mediaPlayer.Close ();
		}
		private void VolumeControl_ValueChanged (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			mediaPlayer.Volume = VolumeControl.Value * 0.01;
		}
		public void Dispose ()
		{
			foreach (var kv in clockLayouts)
			{
				(kv.Value?.Element?.Parent as Panel)?.Children?.Clear ();
				kv.Value?.Dispose ();
			}
			clockLayouts.Clear ();
			clockLayouts = null;
		}
		private void OpenControlPanel_Click (object sender, RoutedEventArgs e)
		{
			Process.Start ("timedate.cpl");
		}
	}
}
