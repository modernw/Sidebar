using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WindowsModern.SlideshowTile
{
	public partial class ConfigPanel: UserControl
	{
		private List<string> picSource = null;

		public ConfigPanel ()
		{
			InitializeComponent ();
			InitStrings ();
			InitSettings ();
		}

		public void InitStrings ()
		{
			var sr = Tile.StringResources;
			GroupPicSrc.Header = sr.SuitableResource ("OPTIONS_PICSRC");
			BtnAddItem.Content = sr.SuitableResource ("OPTIONS_PICSRC_ADD");
			BtnRemoveItem.Content = sr.SuitableResource ("OPTIONS_PICSRC_REMV");
			BtnMoveUp.Content = sr.SuitableResource ("OPTIONS_PICSRC_MVUP");
			BtnMoveDown.Content = sr.SuitableResource ("OPTIONS_PICSRC_MVDN");
			InputIncludeSubDir.Content = sr.SuitableResource ("OPTIONS_SUBDIR");
			LabelCycleDelay.Text = sr.SuitableResource ("OPTIONS_DELAY");
			InputRandomPlay.Content = sr.SuitableResource ("OPTIONS_RANDOM");
		}

		public void InitSettings ()
		{
			var opt = Tile.TileOptions;
			var delays = new [] { 5, 10, 15, 30, 60, 120, 300 };
			SelectCycleDelay.ItemsSource = delays;
			picSource = new List<string> (opt.PictureSource);
			ListPictureSource.ItemsSource = picSource;
			InputIncludeSubDir.IsChecked = opt.IncludeSubFolder;
			SelectCycleDelay.SelectedItem = opt.CycleDelaySecond;
			InputRandomPlay.IsChecked = opt.RandomPlay;
		}

		private void UserControl_Loaded (object sender, RoutedEventArgs e)
		{
			// 可保留为空，或添加加载时的初始化逻辑
		}

		public void OnClickOK ()
		{
			if (picSource.Count <= 0)
				throw new InvalidOperationException (Tile.StringResources.SuitableResource ("MSG_ERR_LIST_EMPTY"));

			if (SelectCycleDelay.SelectedItem == null)
				throw new Exception (Tile.StringResources.SuitableResource ("MSG_ERR_INVALID_DELAY"));

			var opt = Tile.TileOptions;
			var optPicSource = opt.PictureSource;

			if (!picSource.SequenceEqual (optPicSource, StringComparer.OrdinalIgnoreCase))
			{
				optPicSource.Clear ();
				foreach (var item in picSource)
				{
					optPicSource.Add (item);
				}
			}

			if (InputIncludeSubDir.IsChecked != opt.IncludeSubFolder)
				opt.IncludeSubFolder = InputIncludeSubDir.IsChecked ?? true;
			if ((int)SelectCycleDelay.SelectedItem != opt.CycleDelaySecond)
				opt.CycleDelaySecond = (int)SelectCycleDelay.SelectedItem;
			if (InputRandomPlay.IsChecked != opt.RandomPlay)
				opt.RandomPlay = InputRandomPlay.IsChecked ?? true;
		}
		string lastDir = "";
		private void BtnAddItem_Click (object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog ();
			dialog.Description = Tile.StringResources.SuitableResource ("DLG_SELECT_FOLDER");
			dialog.SelectedPath = lastDir;
			if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
			{
				string selectedPath = dialog.SelectedPath;

				if (!picSource.Contains (selectedPath, StringComparer.OrdinalIgnoreCase))
				{
					picSource.Add (selectedPath);
					ListPictureSource.Items.Refresh ();
				}
				else
				{
					MessageBox.Show (
						Tile.StringResources.SuitableResource ("MSG_INFO_FOLDER_EXISTS"),
						Tile.StringResources.SuitableResource ("TITLE_INFO_BOX"),
						MessageBoxButton.OK,
						MessageBoxImage.Information);
				}
			}
		}

		private void BtnRemoveItem_Click (object sender, RoutedEventArgs e)
		{
			if (ListPictureSource.SelectedIndex < 0)
			{
				MessageBox.Show (
					Tile.StringResources.SuitableResource ("MSG_WARN_SELECT_REMOVE"),
					Tile.StringResources.SuitableResource ("TITLE_INFO_BOX"),
					MessageBoxButton.OK,
					MessageBoxImage.Information);
				return;
			}

			int selectedIndex = ListPictureSource.SelectedIndex;
			picSource.RemoveAt (selectedIndex);
			ListPictureSource.Items.Refresh ();
		}

		private void BtnMoveUp_Click (object sender, RoutedEventArgs e)
		{
			int selectedIndex = ListPictureSource.SelectedIndex;
			if (selectedIndex <= 0)
			{
				MessageBox.Show (
					Tile.StringResources.SuitableResource ("MSG_WARN_MOVE_UP"),
					Tile.StringResources.SuitableResource ("TITLE_INFO_BOX"),
					MessageBoxButton.OK,
					MessageBoxImage.Information);
				return;
			}

			string item = picSource [selectedIndex];
			picSource.RemoveAt (selectedIndex);
			picSource.Insert (selectedIndex - 1, item);

			ListPictureSource.Items.Refresh ();
			ListPictureSource.SelectedIndex = selectedIndex - 1;
		}

		private void BtnMoveDown_Click (object sender, RoutedEventArgs e)
		{
			int selectedIndex = ListPictureSource.SelectedIndex;

			if (selectedIndex < 0 || selectedIndex >= picSource.Count - 1)
			{
				MessageBox.Show (
					Tile.StringResources.SuitableResource ("MSG_WARN_MOVE_DOWN"),
					Tile.StringResources.SuitableResource ("TITLE_INFO_BOX"),
					MessageBoxButton.OK,
					MessageBoxImage.Information);
				return;
			}

			string item = picSource [selectedIndex];
			picSource.RemoveAt (selectedIndex);
			picSource.Insert (selectedIndex + 1, item);

			ListPictureSource.Items.Refresh ();
			ListPictureSource.SelectedIndex = selectedIndex + 1;
		}
	}
}