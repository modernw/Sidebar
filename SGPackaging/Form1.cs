using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sidebar;
namespace SGPackaging
{
	public partial class Form1: Form
	{
		public Form1 ()
		{
			InitializeComponent ();
			InitializeDataGridView ();
		}
		private bool _isPackaging = false;
		private BindingList<PackageItem> _packageList = new BindingList<PackageItem> ();
		private void InitializeDataGridView ()
		{
			// 设置列映射（确保列名与设计器中的一致，或者重新创建列）
			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.Columns.Clear ();

			// 文件路径列
			DataGridViewTextBoxColumn filePathCol = new DataGridViewTextBoxColumn ();
			filePathCol.Name = "filePathColumn";
			filePathCol.HeaderText = "File Path";
			filePathCol.DataPropertyName = "FilePath";
			filePathCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			filePathCol.FillWeight = 60;
			filePathCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;  // 允许换行
			dataGridView1.Columns.Add (filePathCol);

			// 文件名列
			DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn ();
			nameCol.Name = "nameColumn";
			nameCol.HeaderText = "File Name";
			nameCol.DataPropertyName = "FileName";
			nameCol.ReadOnly = true;
			nameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridView1.Columns.Add (nameCol);

			// 架构列
			DataGridViewTextBoxColumn archCol = new DataGridViewTextBoxColumn ();
			archCol.Name = "archColumn";
			archCol.HeaderText = "Architecture";
			archCol.DataPropertyName = "Architecture";
			archCol.ReadOnly = true;
			archCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridView1.Columns.Add (archCol);

			// 设置数据源
			dataGridView1.DataSource = _packageList;
			dataGridView1.AllowUserToAddRows = false; 
			dataGridView1.ReadOnly = false;
			dataGridView1.AllowDrop = true;
			dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;  // 自动调整行高
			//dataGridView1.RowTemplate.MinimumHeight = 30;

			// 事件绑定
			dataGridView1.DragEnter += DataGridView1_DragEnter;
			dataGridView1.DragDrop += DataGridView1_DragDrop;
			dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
			dataGridView1.UserDeletingRow += DataGridView1_UserDeletingRow;
			dataGridView1.EditingControlShowing += DataGridView1_EditingControlShowing; // 新增：编辑控件显示事件

			CreateContextMenu ();
		}
		private void DataGridView1_EditingControlShowing (object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			// 仅对文件路径列启用多行编辑
			if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.OwningColumn.Name == "filePathColumn")
			{
				TextBox tb = e.Control as TextBox;
				if (tb != null)
				{
					tb.Multiline = true;           // 启用多行
					tb.WordWrap = true;            // 自动换行
					tb.ScrollBars = ScrollBars.Vertical; // 显示垂直滚动条
					tb.Height = 60;                // 可选：设置编辑框高度，便于多行输入
				}
			}
		}
		private void DataGridView1_DragEnter (object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent (DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
		}

		private void DataGridView1_DragDrop (object sender, DragEventArgs e)
		{
			string [] files = (string [])e.Data.GetData (DataFormats.FileDrop);
			AddFiles (files);
		}

		private void DataGridView1_CellDoubleClick (object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;
			PackageItem item = _packageList [e.RowIndex];
			if (openFileDialog1.ShowDialog () == DialogResult.OK)
			{
				string fullPath = Path.GetFullPath (openFileDialog1.FileName);
				// 去重检查（排除当前项）
				bool duplicate = false;
				foreach (PackageItem p in _packageList)
				{
					if (p == item) continue;
					if (string.Equals (p.FilePath, fullPath, StringComparison.OrdinalIgnoreCase))
					{
						duplicate = true;
						break;
					}
				}
				if (duplicate)
				{
					MessageBox.Show ("This file already exists in the list.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				item.FilePath = fullPath;
			}
		}

		// 在删除行之前释放 PackageItem 资源
		private void DataGridView1_UserDeletingRow (object sender, DataGridViewRowCancelEventArgs e)
		{
			PackageItem item = e.Row.DataBoundItem as PackageItem;
			if (item != null)
				item.Dispose ();
		}

		// 添加文件的辅助方法
		private void AddFiles (string [] filePaths)
		{
			foreach (string path in filePaths)
			{
				if (string.IsNullOrWhiteSpace (path)) continue;
				string fullPath = Path.GetFullPath (path);
				bool exists = false;
				foreach (PackageItem item in _packageList)
				{
					if (string.Equals (item.FilePath, fullPath, StringComparison.OrdinalIgnoreCase))
					{
						exists = true;
						break;
					}
				}
				if (!exists)
					_packageList.Add (new PackageItem (fullPath));
			}
		}
		private void CreateContextMenu ()
		{
			ContextMenuStrip menu = new ContextMenuStrip ();

			// 删除当前行
			menu.Items.Add ("Delete", null, (s, e) =>
			{
				if (dataGridView1.CurrentRow == null) return;
				PackageItem item = dataGridView1.CurrentRow.DataBoundItem as PackageItem;
				if (item != null)
				{
					item.Dispose ();
					_packageList.Remove (item);
				}
			});

			// 复制文件路径
			menu.Items.Add ("Copy Path", null, (s, e) =>
			{
				if (dataGridView1.CurrentRow == null) return;
				PackageItem item = dataGridView1.CurrentRow.DataBoundItem as PackageItem;
				if (item != null && !string.IsNullOrEmpty (item.FilePath))
					Clipboard.SetText (item.FilePath);
			});

			// 复制文件名
			menu.Items.Add ("Copy File Name", null, (s, e) =>
			{
				if (dataGridView1.CurrentRow == null) return;
				PackageItem item = dataGridView1.CurrentRow.DataBoundItem as PackageItem;
				if (item != null)
					Clipboard.SetText (item.FileName);
			});

			// 添加文件（通过对话框）
			menu.Items.Add ("Add File(s)...", null, (s, e) =>
			{
				using (OpenFileDialog ofd = new OpenFileDialog ())
				{
					ofd.Multiselect = true;
					ofd.Filter = "Package files (*.sgpkg)|*.sgpkg|All files (*.*)|*.*";
					if (ofd.ShowDialog () == DialogResult.OK)
						AddFiles (ofd.FileNames);
				}
			});

			// 清空所有
			menu.Items.Add ("Clear All", null, (s, e) =>
			{
				if (MessageBox.Show ("Clear all rows?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					foreach (PackageItem item in _packageList)
						item.Dispose ();
					_packageList.Clear ();
				}
			});

			dataGridView1.ContextMenuStrip = menu;
		}
		private List<string> GetPackageFileList ()
		{
			List<string> paths = new List<string> ();
			foreach (PackageItem item in _packageList)
			{
				if (!string.IsNullOrEmpty (item.FilePath))
					paths.Add (item.FilePath);
			}
			return paths;
		}
		private void button1_Click (object sender, EventArgs e)
		{
			var res = folderBrowserDialog1.ShowDialog (this);
			if (res == DialogResult.OK)
			{
				textBox1.Text = folderBrowserDialog1.SelectedPath;
			}
		}
		private void button2_Click (object sender, EventArgs e)
		{
			var res = folderBrowserDialog2.ShowDialog (this);
			if (res == DialogResult.OK)
			{
				textBox2.Text = folderBrowserDialog2.SelectedPath;
			}
		}
		private void button4_Click (object sender, EventArgs e)
		{
			var res = folderBrowserDialog3.ShowDialog (this);
			if (res == DialogResult.OK)
			{
				textBox5.Text = folderBrowserDialog3.SelectedPath;
			}
		}
		private void OnProgress (int curr, int total, double p)
		{
			if (progressBar1.InvokeRequired)
			{
				progressBar1.Invoke (new Action (() => OnProgress (curr, total, p)));
				return;
			}
			progressBar1.Maximum = total;
			progressBar1.Value = curr;
		}
		private void InitProgress ()
		{
			if (progressBar1.InvokeRequired)
			{
				progressBar1.Invoke (new Action (() => InitProgress ()));
				return;
			}
			progressBar1.Value = 0;
			progressBar1.Maximum = 100;
		}
		private void FullProgress ()
		{
			if (progressBar1.InvokeRequired)
			{
				progressBar1.Invoke (new Action (() => FullProgress ()));
				return;
			}
			progressBar1.Value = progressBar1.Maximum;
		}
		private void OutputSingle (string content, bool withTimestamp = true)
		{
			if (textBox4.InvokeRequired)
			{
				textBox4.Invoke (new Action (() => OutputSingle (content, withTimestamp)));
				return;
			}
			string timestamp = withTimestamp ? $"[{DateTime.Now:HH:mm:ss}] " : "";
			textBox4.Text += timestamp + content;
		}
		private void OutputLineSingle (string content, bool withTimestamp = true) => OutputSingle (content + Environment.NewLine, withTimestamp);
		private void OutputBundle (string content, bool withTimestamp = true)
		{
			if (textBox7.InvokeRequired)
			{
				textBox7.Invoke (new Action (() => OutputBundle (content, withTimestamp)));
				return;
			}
			string timestamp = withTimestamp ? $"[{DateTime.Now:HH:mm:ss}] " : "";
			textBox7.Text += timestamp + content;
		}
		private void OutputLineBundle (string content, bool withTimestamp = true) => OutputBundle (content + Environment.NewLine, withTimestamp);
		private void button5_Click (object sender, EventArgs e)
		{
			PackageBundle ();
		}
		private void PackageSingle ()
		{
			if (string.IsNullOrWhiteSpace (textBox1.Text) || string.IsNullOrWhiteSpace (textBox2.Text) || string.IsNullOrWhiteSpace (textBox3.Text))
			{
				OutputLineSingle ("Error: Source folder, destination folder or package name cannot be empty.");
				return;
			}
			_isPackaging = true;
			tabControl1.Enabled = false;
			button3.Enabled = false;
			OutputLineSingle ("Starting single package creation...");
			Task.Factory.StartNew (() => {
				try
				{
					OutputLineSingle ($"Source directory: {textBox1.Text}");
					OutputLineSingle ($"Destination directory: {textBox2.Text}");
					OutputLineSingle ($"Package name: {textBox3.Text}");
					OutputLineSingle ("Creating archive...");
					using (var zip = TilePackageWriteManager.MakeSinglePackage (textBox1.Text, OnProgress))
					{
						OutputLineSingle ("Archive created. Adding signature and integrity data...");
						TilePackageWriteManager.SavePackageFile (zip, textBox2.Text, textBox3.Text);
					}
					OutputLineSingle ($"Package successfully created: {Path.Combine (textBox2.Text, textBox3.Text)}");
					return true;
				}
				catch (Exception ex)
				{
					OutputLineSingle ($"Packaging failed: {ex.Message}");
					return false;
				}
			}).ContinueWith (task => {
				_isPackaging = false;
				tabControl1.Enabled = true;
				button3.Enabled = true;
				if (task.Result)
				{
					FullProgress ();
					OutputLineSingle ("All done.");
				}
				else
				{
					OutputLineSingle ("An error occurred during packaging, rollback performed.");
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}
		private void button3_Click (object sender, EventArgs e)
		{
			PackageSingle ();
		}
		private void PackageBundle ()
		{
			List<string> fileList = GetPackageFileList ();
			if (fileList.Count < 2)
			{
				OutputLineBundle ("Error: At least two packages are required for a bundle.");
				return;
			}
			if (string.IsNullOrWhiteSpace (textBox5.Text) || string.IsNullOrWhiteSpace (textBox6.Text))
			{
				OutputLineBundle ("Error: Destination folder and bundle name cannot be empty.");
				return;
			}
			if (!Directory.Exists (textBox5.Text))
			{
				try
				{
					Directory.CreateDirectory (textBox5.Text);
				}
				catch (Exception ex)
				{
					OutputLineBundle ($"Error: Cannot create destination folder: {ex.Message}");
					return;
				}
			}
			_isPackaging = true;
			tabControl1.Enabled = false;
			button5.Enabled = false;
			textBox7.Clear ();
			InitProgress (); 
			OutputLineBundle ("Starting bundle creation...");
			OutputLineBundle ($"Bundle name: {textBox6.Text}");
			OutputLineBundle ($"Destination folder: {textBox5.Text}");
			OutputLineBundle ($"Total packages: {fileList.Count}");
			foreach (string fp in fileList)
			{
				OutputLineBundle ($"  - {Path.GetFileName (fp)}");
			}
			Task.Factory.StartNew (() =>
			{
				try
				{
					using (var zip = TilePackageWriteManager.MakeBundlePackage (fileList, OnProgress))
					{
						OutputLineBundle ("Bundle archive created. Adding signature and integrity data...");
						TilePackageWriteManager.SavePackageFile (zip, textBox5.Text, textBox6.Text);
					}
					string outputPath = Path.Combine (textBox5.Text, textBox6.Text);
					OutputLineBundle ($"Bundle successfully created: {outputPath}");
					return true;
				}
				catch (Exception ex)
				{
					OutputLineBundle ($"Bundle creation failed: {ex.Message}");
					return false;
				}
			}).ContinueWith (task =>
			{
				_isPackaging = false;
				tabControl1.Enabled = true;
				button5.Enabled = true;
				if (task.Result)
				{
					FullProgress (); 
					OutputLineBundle ("All done.");
				}
				else
				{
					OutputLineBundle ("An error occurred, rollback performed.");
					InitProgress ();
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ()); // 回到 UI 线程
		}
		private void Form1_FormClosed (object sender, FormClosedEventArgs e)
		{
			foreach (PackageItem item in _packageList)
				item?.Dispose ();
		}
		private void button6_Click (object sender, EventArgs e)
		{
			PackageItem item = new PackageItem ();

			_packageList.Add (item);

			int index = _packageList.Count - 1;

			if (index >= 0)
			{
				dataGridView1.ClearSelection ();

				dataGridView1.Rows [index].Selected = true;

				dataGridView1.CurrentCell =
					dataGridView1.Rows [index].Cells [0];

				dataGridView1.BeginEdit (true);
			}
		}
		private void button7_Click (object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow == null)
				return;

			PackageItem item =
				dataGridView1.CurrentRow.DataBoundItem as PackageItem;

			if (item == null)
				return;

			item.Dispose ();

			_packageList.Remove (item);
		}
		private void button8_Click (object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow == null)
				return;

			PackageItem item =
				dataGridView1.CurrentRow.DataBoundItem as PackageItem;

			if (item == null)
				return;

			item.FilePath = "";
		}
	}
}
