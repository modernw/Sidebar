using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using Sidebar;
namespace WindowsModern.SlideshowTile
{
	public class Config: IDisposable, INotifyPropertyChanged
	{
		public ITileConfig TileConfig { get; }
		private ObservableCollection<string> _picSrc;
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged (string propName)
		{
			PropertyChanged?.Invoke (null, new PropertyChangedEventArgs (propName));
		}
		public ObservableCollection<string> PictureSource
		{
			get
			{
				if (_picSrc == null)
				{
					LoadPictureSource ();
					_picSrc.CollectionChanged += OnPictureSourceChanged;
				}
				return _picSrc;
			}
		}
		private bool includeSubDir = true;
		public bool IncludeSubFolder
		{
			get { return includeSubDir; }
			set
			{
				TileConfig.Ini ["Settings"] ["IncludeSubFolder"] = includeSubDir = value;
				OnPropertyChanged (nameof (IncludeSubFolder));
			}
		}
		private int cycleDelay = 10;
		public int CycleDelaySecond
		{
			get { return cycleDelay; }
			set
			{
				var s = value;
				switch (value)
				{
					case 5:
					case 10:
					case 15:
					case 30:
					case 60:
					case 120:
					case 300:
						s = value;
						break;
					default:
						if (value < 5) s = 5;
						else if (value > 300) s = 300;
						else s = value;
						break;
				}
				TileConfig.Ini ["Settings"] ["CycleDelay"] = cycleDelay = s;
				OnPropertyChanged (nameof (CycleDelaySecond));
			}
		}
		private bool randomPlay = true;
		public bool RandomPlay
		{
			get { return randomPlay; }
			set
			{
				TileConfig.Ini ["Settings"] ["Random"] = randomPlay = value;
				OnPropertyChanged (nameof (RandomPlay));
			}
		}
		private string currPicRecord = "";
		public string CurrentPictureRecord
		{
			get { return currPicRecord; }
			set
			{
				TileConfig.Ini ["Settings"] ["CurrentPicture"] = currPicRecord = value;
			}
		}
		private void Init ()
		{
			var includeSubVal = TileConfig.Ini ["Settings"].GetKey ("IncludeSubFolder").ReadBool (true);
			includeSubDir = includeSubVal;
			var cycleVal = TileConfig.Ini ["Settings"].GetKey ("CycleDelay").ReadInt (10);
			if (cycleVal < 5) cycleVal = 5;
			else if (cycleVal > 300) cycleVal = 300;
			cycleDelay = cycleVal;
			randomPlay = TileConfig.Ini ["Settings"].GetKey ("Random").ReadBool (true);
			currPicRecord = TileConfig.Ini ["Settings"].GetKey ("CurrentPicture").ReadString ();
		}
		private void LoadPictureSource ()
		{
			var xml = TileConfig.Xml;
			if (xml?.Global != null)
			{
				var list = xml.Global.Get<List<string>> ("PictureSource", new List<string> ());
				_picSrc = new ObservableCollection<string> (list ?? new List<string> ());
			}
			else
			{
				_picSrc = new ObservableCollection<string> ();
			}
			if (_picSrc.Count == 0)
			{
				var set = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
				AddIfValid (set, Environment.GetFolderPath (Environment.SpecialFolder.MyPictures));
				AddIfValid (set, Environment.GetFolderPath (Environment.SpecialFolder.CommonPictures));
				foreach (var path in set)
				{
					_picSrc.Add (path);
				}
				if (xml?.Global != null)
				{
					var list = _picSrc.ToList ();
					xml.Global ["PictureSource"] = list;
				}
			}
		}
		private static void AddIfValid (HashSet<string> set, string path)
		{
			if (!string.IsNullOrEmpty (path) && System.IO.Directory.Exists (path))
				set.Add (path);
		}
		private void OnPictureSourceChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (TileConfig.Xml?.Global != null)
			{
				var list = _picSrc.ToList ();
				TileConfig.Xml.Global ["PictureSource"] = list;
			}
			OnPropertyChanged (nameof (PictureSource));
		}
		public Config (ITileConfig conf)
		{
			TileConfig = conf;
			Init ();
		}
		public void Dispose ()
		{
			if (_picSrc != null)
			{
				_picSrc.CollectionChanged -= OnPictureSourceChanged;
				_picSrc = null;
			}
		}
	}
}
