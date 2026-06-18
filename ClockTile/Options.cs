using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidebar;
using ClockTile.Faces;
using System.ComponentModel;

namespace ClockTile
{
	public class TileOptions: INotifyPropertyChanged
	{
		public ITileConfig Config { get; }
		private bool _enableAlarm = false;
		private bool _alarmEveryDay = false;
		private DateTime _alarmTime = new DateTime ();
		private string _alarmRing = "";
		private string _alarmMessage = "";
		private double _alarmVolume = 1;
		private ClockTileFaceType _clockType = ClockTileFaceType.DateTimeClock;
		private string _timeZone = "";
		public bool EnableAlarm
		{
			get { return _enableAlarm; }
			set
			{
				Config.Ini ["Settings"] ["EnableAlarm"] = _enableAlarm = value;
				OnPropertyChanged ("EnableAlarm");
			}
		}
		public bool AlarmEveryDay
		{
			get { return _alarmEveryDay; }
			set
			{
				Config.Ini ["Settings"] ["AlarmEveryDay"] = _alarmEveryDay = value;
				OnPropertyChanged ("AlarmEveryDay");
			}
		}
		public DateTime AlarmTime
		{
			get { return _alarmTime; }
			set
			{
				_alarmTime = value;
				Config.Ini ["Settings"].GetKey ("AlarmTime").Write (value);
				OnPropertyChanged ("AlarmTime");
			}
		}
		public string AlarmRing
		{
			get { return _alarmRing; }
			set
			{
				Config.Ini ["Settings"] ["AlarmRing"] = _alarmRing = value;
				OnPropertyChanged ("AlarmRing");
			}
		}
		public double AlarmVolume
		{
			get { return _alarmVolume; }
			set
			{
				Config.Ini ["Settings"] ["AlarmVolume"] = _alarmVolume = value;
				OnPropertyChanged ("AlarmVolume");
			}
		}
		public string AlarmMessage
		{
			get { return _alarmMessage; }
			set
			{
				Config.Ini ["Settings"] ["AlarmMessage"] = _alarmMessage = value;
				OnPropertyChanged ("AlarmMessage");
			}
		}
		public ClockTileFaceType ClockDisplayType
		{
			get { return _clockType; }
			set
			{
				Config.Ini ["Settings"] ["ClockDisplayType"] = (int)value;
				_clockType = value;
				OnPropertyChanged ("ClockDisplayType");
			}
		}
		public string TimeZone
		{
			get { return _timeZone; }
			set
			{
				Config.Ini ["Settings"] ["TimeZone"] = value;
				_timeZone = value;
				OnPropertyChanged ("TimeZone");
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void InitConfigValues ()
		{
			var settings = Config.Ini ["Settings"];
			_alarmVolume = settings.GetKey ("AlarmVolume").ReadDouble (1);
			_enableAlarm = settings.GetKey ("EnableAlarm").ReadBool (false);
			_alarmEveryDay = settings.GetKey ("AlarmEveryDay").ReadBool (false);
			_alarmTime = settings.GetKey ("AlarmTime").ReadDateTime (DateTime.Today.AddHours (8)); // 默认早上8点
			_alarmRing = settings.GetKey ("AlarmRing").ReadString ("");
			_alarmMessage = settings.GetKey ("AlarmMessage").ReadString ("");
			_clockType = (ClockTileFaceType)settings.GetKey ("ClockDisplayType").ReadInt ((int)ClockTileFaceType.DateTimeClock);
			_timeZone = settings.GetKey ("TimeZone").ReadString ("");
		}
		public TileOptions (ITileConfig currUserConf)
		{
			Config = currUserConf;
			InitConfigValues ();
		}
		private void OnPropertyChanged (string propertyName)
		{
			PropertyChanged?.Invoke (null, new PropertyChangedEventArgs (propertyName));
		}
	}
}
