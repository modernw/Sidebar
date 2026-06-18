using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Win32.Init;
namespace WindowsModern.PowerTile
{
	public class Options: INotifyPropertyChanged
	{
		private InitConfig ini;
		public Options (InitConfig iniConf)
		{
			ini = iniConf;
			InitValues ();
		}
		private void InitValues ()
		{
			keepScreen = ini ["Settings"].GetKey ("KeepScreen").ReadBool ();
		}
		private bool keepScreen = false;
		public bool KeepScreen
		{
			get { return keepScreen; }
			set
			{
				ini ["Settings"] ["KeepScreen"] = keepScreen = value;
				OnPropertyChanged (nameof (KeepScreen));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged (string propName)
		{
			PropertyChanged?.Invoke (null, new PropertyChangedEventArgs (propName));
		}
	}
}
