using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WindowsModern.SearchTile
{
	public class TextEmptyToVisibilityConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			var text = value as string;
			return string.IsNullOrEmpty (text) ? Visibility.Visible : Visibility.Collapsed;
		}
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
	public class ProviderToIconConverter: IValueConverter
	{
		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			SearchProvider provider = SearchProvider.Default; // 默认值
			if (value != null && value is SearchProvider)
			{
				provider = (SearchProvider)value;
			}

			string iconName;
			switch (provider)
			{
				case SearchProvider.Corpnet:
					iconName = "Corpnet.ico";
					break;
				case SearchProvider.HowDoI:
					iconName = "HowDoI.ico";
					break;
				case SearchProvider.MSN:
					iconName = "MSN.ico";
					break;
				default:
				case SearchProvider.Default:
					iconName = "MyStuff.ico";
					break;
			}

			Uri iconUri = new Uri (System.IO.Path.Combine (Tile.TileFolder.FolderPath, $"Images\\{iconName}"),UriKind.RelativeOrAbsolute);
			return new BitmapImage (iconUri);
		}
		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}
