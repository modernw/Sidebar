using System.ComponentModel;
using Microsoft.Win32.Init;
using Sidebar;
namespace WindowsModern.SearchTile
{
	public class Options: INotifyPropertyChanged
	{
		private InitConfig tileConfig;
		private SearchProvider ?_provider = null;
		public SearchProvider Provider
		{
			get { return _provider ?? SearchProvider.Default; }
			set
			{
				_provider = value;
				var ps = "";
				switch (value)
				{
					case SearchProvider.Corpnet: ps = "corpnet"; break;
					case SearchProvider.HowDoI: ps = "howdoi"; break;
					case SearchProvider.MSN: ps = "msn"; break;
					default:
					case SearchProvider.Default: ps = ""; break;
				}
				tileConfig ["Settings"] ["Provider"] = ps;
				OnPropertyChanged (nameof (Provider));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged (string propName)
		{
			PropertyChanged?.Invoke (null, new PropertyChangedEventArgs (propName));
		}
		private void InitValues ()
		{
			var ps = tileConfig ["Settings"].GetKey ("Provider").ReadString () ?? "";
			if (ps.NEquals ("corpnet")) _provider = SearchProvider.Corpnet;
			else if (ps.NEquals ("howdoi")) _provider = SearchProvider.HowDoI;
			else if (ps.NEquals ("msn")) _provider = SearchProvider.MSN;
			else _provider = SearchProvider.Default;
		}
		public Options (InitConfig currUserConf) { tileConfig = currUserConf; InitValues (); }
	}
}
