using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sidebar;

namespace WindowsModern.FeedTile
{
	/// <summary>
	/// FeedArticlePanel.xaml 的交互逻辑
	/// </summary>
	public partial class FeedArticlePanel: UserControl
	{
		public FeedArticlePanel ()
		{
			InitializeComponent ();
			HtmlPage.Source = new Uri (System.IO.Path.Combine (Tile.TileFolder.FolderPath, "Pages\\Detail.html"), UriKind.RelativeOrAbsolute);
			HtmlPage.ObjectForScripting = new ScriptHelper ();
		}
		private FeedItemData item = null;
		public void SetFeedItem (FeedItemData i)
		{
			item = i;
			RefreshInfo ();
		}
		public void RefreshInfo ()
		{
			if (!HtmlPage.Dispatcher.CheckAccess ())
			{
				HtmlPage.Dispatcher.Invoke (new Action (() => RefreshInfo ()));
				return;
			}
			if (!(HtmlPage.IsLoaded && _isDocumentLoaded)) return;
			HtmlPage.InvokeScript ("setTitle", item?.Title, item?.Link);
			HtmlPage.InvokeScript ("setPubDate", item?.PublishDate.ToString ());
			HtmlPage.InvokeScript ("setChannel", item?.Parent?.Title, item?.Parent?.Link);
			HtmlPage.InvokeScript ("setDesc", item?.Description);
			HtmlPage.InvokeScript ("setUrl", item?.Link);
		}
		private bool _isDocumentLoaded = false;
		private void HtmlPage_LoadCompleted (object sender, NavigationEventArgs e)
		{
			if (e.Uri == HtmlPage.Source)
			{
				_isDocumentLoaded = true;
				try { ((dynamic)HtmlPage.Document).Silent = true; } catch { }
				RefreshInfo ();
			}
		}
		private void HtmlPage_Unloaded (object sender, RoutedEventArgs e)
		{
			//_isDocumentLoaded = false;
			SetFeedItem (null);
		}

		private void HtmlPage_Loaded (object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke (new Action (() =>
			{
				if (_isDocumentLoaded) RefreshInfo ();
			}), System.Windows.Threading.DispatcherPriority.Background);
		}

		private void HtmlPage_Navigating (object sender, NavigatingCancelEventArgs e)
		{
			_isDocumentLoaded = false;
		}
		public void ClearPage ()
		{
			SetFeedItem (null);
		}
	}
}
