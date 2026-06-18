using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WindowsModern.FeedTile
{
	public partial class FlyoutPanel: UserControl
	{
		private Timer _refreshTimer;
		private CancellationTokenSource _refreshCts;
		private List<FeedData> _validFeeds = new List<FeedData> ();   // 只存储 Feed 引用，不含 Items 副本
		private int _currentFeedIndex = 0;
		private const int MaxItemsPerFeed = 8;
		private readonly object _dataLock = new object ();

		public FlyoutPanel ()
		{
			InitializeComponent ();
		}

		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{
			ScheduleRefresh ();
			_refreshTimer = new Timer (_ => ScheduleRefresh (), null, TimeSpan.FromHours (1), TimeSpan.FromHours (1));
		}

		private void UserControl_Unloaded (object sender, RoutedEventArgs e)
		{
			_refreshTimer?.Dispose ();
			_refreshTimer = null;
			_refreshCts?.Cancel ();
			_refreshCts?.Dispose ();
			_refreshCts = null;
		}

		private void ScheduleRefresh ()
		{
			lock (this)
			{
				_refreshCts?.Cancel ();
				_refreshCts?.Dispose ();
				_refreshCts = new CancellationTokenSource ();
				var token = _refreshCts.Token;
				ThreadPool.QueueUserWorkItem (_ => LoadDataAsync (token));
			}
		}

		private void LoadDataAsync (CancellationToken token)
		{
			try
			{
				// 在 UI 线程获取有效 Feed 列表（只取引用，不复制 Items）
				List<FeedData> validFeeds = null;
				Application.Current.Dispatcher.Invoke (new Action (() => {
					validFeeds = Tile.Feeds
						.Where (f => f.Items != null && f.Items.Count > 0)
						.ToList ();
				}));

				if (token.IsCancellationRequested) return;

				// 更新缓存和 UI
				Application.Current.Dispatcher.Invoke (new Action (() => {
					lock (_dataLock)
					{
						_validFeeds = validFeeds;
						if (_validFeeds.Count == 0)
						{
							FeedTitle.Text = Tile.TileFolder.StringResources.SuitableResource ("FLYOUT_NOFEEDS");
							ButtonBack.ToolTip = null;
							ButtonNext.ToolTip = null;
							FeedItemList.Children.Clear ();
							var placeholder = new TextBlock {
								Text = Tile.TileFolder.StringResources.SuitableResource ("FLYOUT_NOITEMS"),
								FontSize = 12,
								Foreground = System.Windows.Media.Brushes.Gray,
							};
							placeholder.SetResourceReference (TextBlock.FontFamilyProperty, "GlobalFontFamily");
							FeedItemList.Children.Add (placeholder);
							return;
						}
						if (_currentFeedIndex >= _validFeeds.Count) _currentFeedIndex = 0;
						DisplayCurrentFeed ();
					}
				}));
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine ($"FlyoutPanel refresh error: {ex.Message}");
			}
		}

		private void DisplayCurrentFeed ()
		{
			if (_validFeeds.Count == 0) return;
			var currentFeed = _validFeeds [_currentFeedIndex];

			// 直接从原始 Feed 中获取最新 N 条（按时间排序）
			var itemsToShow = currentFeed.Items
				.OrderByDescending (item => item.PublishDate)
				.Take (MaxItemsPerFeed)
				.ToList ();

			FeedTitle.Text = currentFeed.Title;

			// 更新按钮 ToolTip
			int prevIndex = (_currentFeedIndex - 1 + _validFeeds.Count) % _validFeeds.Count;
			int nextIndex = (_currentFeedIndex + 1) % _validFeeds.Count;
			ButtonBack.ToolTip = string.Format ("({0}/{1}) {2}", prevIndex + 1, _validFeeds.Count, _validFeeds [prevIndex].Title);
			ButtonNext.ToolTip = string.Format ("({0}/{1}) {2}", nextIndex + 1, _validFeeds.Count, _validFeeds [nextIndex].Title);

			// 刷新条目列表
			FeedItemList.Children.Clear ();
			foreach (var item in itemsToShow)
			{
				var feedItemControl = new FlyoutFeedItem ();
				feedItemControl.SetFeedItem (item);
				feedItemControl.RefreshFeed ();
				FeedItemList.Children.Add (feedItemControl);
			}
		}

		private void SwitchToPrevFeed ()
		{
			lock (_dataLock)
			{
				if (_validFeeds.Count == 0) return;
				_currentFeedIndex = (_currentFeedIndex - 1 + _validFeeds.Count) % _validFeeds.Count;
				DisplayCurrentFeed ();
			}
		}

		private void SwitchToNextFeed ()
		{
			lock (_dataLock)
			{
				if (_validFeeds.Count == 0) return;
				_currentFeedIndex = (_currentFeedIndex + 1) % _validFeeds.Count;
				DisplayCurrentFeed ();
			}
		}

		private void ButtonBack_Click (object sender, RoutedEventArgs e)
		{
			SwitchToPrevFeed ();
		}

		private void ButtonNext_Click (object sender, RoutedEventArgs e)
		{
			SwitchToNextFeed ();
		}
	}
}