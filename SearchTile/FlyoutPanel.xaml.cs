using System;
using System.Collections.Generic;
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

namespace WindowsModern.SearchTile
{
	/// <summary>
	/// FlyoutPanel.xaml 的交互逻辑
	/// </summary>
	public partial class FlyoutPanel: UserControl
	{
		public static List<KeyValuePair<SearchProvider, string>> providerComboInitList;
		public FlyoutPanel ()
		{
			InitializeComponent ();
			var sr = Tile.TileFolder.StringResources;
			providerComboInitList = new List<KeyValuePair<SearchProvider, string>> {
				new KeyValuePair<SearchProvider, string> (SearchProvider.Corpnet, sr.SuitableResource ("PROVIDER_CORPNET")),
				new KeyValuePair<SearchProvider, string> (SearchProvider.HowDoI, sr.SuitableResource ("PROVIDER_HOWDOI")),
				new KeyValuePair<SearchProvider, string> (SearchProvider.Default, sr.SuitableResource ("PROVIDER_MYSTUFF")),
				new KeyValuePair<SearchProvider, string> (SearchProvider.MSN, sr.SuitableResource ("PROVIDER_MSNSEARCH"))
			};
			SearchProviderSelect.SelectedValuePath = "Key";
			SearchProviderSelect.ItemsSource = providerComboInitList;
			SearchProviderSelect.SelectedValue = Tile.TileOptions.Provider;
		}
		public event TextChangedEventHandler TextChanged;
		public event RoutedEventHandler SearchClick;
		private void SearchText_TextChanged (object sender, TextChangedEventArgs e)
		{
			TextChanged?.Invoke (sender, e);
		}
		public string Text
		{
			get { return SearchText.Text; }
			set { SearchText.Text = value; }
		}
		private void SearchProviderSelect_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			if (Tile.TileOptions == null) return;
			if (SearchProviderSelect.SelectedValue == null) return;
			Tile.TileOptions.Provider = (SearchProvider)SearchProviderSelect.SelectedValue;
		}
		private void Button_Click (object sender, RoutedEventArgs e)
		{
			SearchClick?.Invoke (sender, e);
		}
		private void SearchText_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				SearchClick?.Invoke (sender, e);
				e.Handled = true; // 防止“叮”一声
			}
		}
	}
}
