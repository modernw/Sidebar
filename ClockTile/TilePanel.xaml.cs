using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClockTile.Faces;
using Sidebar;
namespace ClockTile
{
	/// <summary>
	/// ClockTile.xaml 的交互逻辑
	/// </summary>
	public partial class TilePanel: UserControl, IDisposable
	{
		public TilePanel ()
		{
			InitializeComponent ();
			ChangeTimePanel ();
			Tile.Options.PropertyChanged += Options_PropertyChanged;
		}
		private ClockFacePanel currentPanel = null;
		private DispatcherTimer _timer;
		private bool _isLoaded = false;
		public void Dispose ()
		{
			StopTimer ();
			_isLoaded = false;
			currentPanel?.Dispose ();
			currentPanel = null;
			this.Loaded -= OnLoaded;
			this.Unloaded -= OnUnloaded;
			Tile.Options.PropertyChanged -= Options_PropertyChanged;
		}
		private void ChangeTimePanel ()
		{
			TilePanelContainer.Children.Clear ();
			currentPanel?.Dispose ();
			currentPanel = null;
			var facetype = Tile.Options.ClockDisplayType;
			currentPanel = facetype.GetClockFace ();
			currentPanel.Setter.CurrentTime = DateTime.UtcNow;
			currentPanel.Setter.CurrentTimeZone = Tile.Options.TimeZone;
			TilePanelContainer.Children.Add (currentPanel.Element);
			Tile.SidebarFeatures.Request (new SidebarRequest (Tile.TileInstance) { RequestName = "OpacityAnime" });
		}
		private void Options_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "TimeZone":
					if (currentPanel != null && currentPanel.Setter != null)
						currentPanel.Setter.CurrentTimeZone = Tile.Options.TimeZone;
					break;
				case "ClockDisplayType":
					ChangeTimePanel ();
					break;
			}
		}
		private void OnLoaded (object sender, RoutedEventArgs e)
		{
			_isLoaded = true;
			StartTimer ();
			UpdateCurrentTime ();
		}
		private void OnUnloaded (object sender, RoutedEventArgs e)
		{
			_isLoaded = false;
			StopTimer ();
		}
		private void StartTimer ()
		{
			if (_timer != null) return;
			_timer = new DispatcherTimer (DispatcherPriority.Normal);
			_timer.Tick += OnTimerTick;
			DateTime now = DateTime.UtcNow;
			int delayMilliseconds = 1000 - now.Millisecond;
			if (delayMilliseconds < 50) delayMilliseconds = 1000; // 避免过短
			_timer.Interval = TimeSpan.FromMilliseconds (delayMilliseconds);
			_timer.Start ();
		}
		private void StopTimer ()
		{
			if (_timer != null)
			{
				_timer.Stop ();
				_timer.Tick -= OnTimerTick;
				_timer = null;
			}
		}
		private void OnTimerTick (object sender, EventArgs e)
		{
			if (_timer.Interval.TotalSeconds != 1)
			{
				_timer.Interval = TimeSpan.FromSeconds (1);
			}
			UpdateCurrentTime ();
		}
		private void UpdateCurrentTime ()
		{
			if (!_isLoaded) return;
			if (currentPanel == null) return;
			var timeSetter = currentPanel.Setter;
			if (timeSetter != null)
			{
				timeSetter.CurrentTime = DateTime.UtcNow;
				//timeSetter.CurrentTimeZone = Tile.Options.TimeZone;
			}
		}
	}
}
