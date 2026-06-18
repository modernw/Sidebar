using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using IWshRuntimeLibrary;
namespace WindowsModern.QuickLaunchTile
{
	public class ShortcutInfo
	{
		public string Target { get; set; }
		public string LinkPath { get; set; }
		public string Path { get; set; }
		public string Arguments { get; set; }
		public string Description { get; set; }
		public string HotKey { get; set; }
		public int ShowCommand { get; set; }
		public string WorkingDirectory { get; set; }
		public Tuple <string, int> Icon { get; set; }
		public override bool Equals (object obj)
		{
			if (obj is ShortcutInfo) return Sidebar.StringNormalize.NEquals (LinkPath, (obj as ShortcutInfo)?.LinkPath);
			return base.Equals (obj);
		}
		public override int GetHashCode ()
		{
			return Sidebar.StringNormalize.NNormalize (LinkPath)?.GetHashCode () ?? base.GetHashCode ();
		}
	}
	// 用于存储 SCF 文件解析后的信息
	public class ScfInfo
	{
		public string FilePath { get; set; }        // SCF 文件自身的完整路径
		public string Title { get; set; }           // 从文件名推断的标题，如“显示桌面”
		public string CommandType { get; set; }     // 命令类型，如 "ToggleDesktop", "Explorer" 等
		public string IconPath { get; set; }        // 图标路径，从 IconFile 解析
		public int IconIndex { get; set; }          // 图标索引
	}
	public static class ScfParser
	{
		public static ScfInfo Parse (string filePath)
		{
			if (!System.IO.File.Exists (filePath))
				return null;

			var info = new ScfInfo {
				FilePath = filePath,
				// 假设文件名为“显示桌面.scf”，则Title为“显示桌面”
				Title = System.IO.Path.GetFileNameWithoutExtension (filePath)
			};

			try
			{
				var lines = System.IO.File.ReadAllLines (filePath);
				string currentSection = "";

				foreach (var line in lines)
				{
					// 移除前后空格，忽略空行和注释
					var trimmedLine = line.Trim ();
					if (string.IsNullOrEmpty (trimmedLine) || trimmedLine.StartsWith (";"))
						continue;

					// 检查是否是 Section Header，如 "[Shell]"
					if (trimmedLine.StartsWith ("[") && trimmedLine.EndsWith ("]"))
					{
						currentSection = trimmedLine.Trim ('[', ']').ToLower ();
						continue;
					}

					// 解析键值对
					var match = Regex.Match (trimmedLine, @"^\s*(.*?)\s*=\s*(.*)\s*$");
					if (match.Success)
					{
						var key = match.Groups [1].Value.Trim ();
						var value = match.Groups [2].Value.Trim ();

						if (currentSection == "shell")
						{
							if (key.ToLower () == "command")
								info.CommandType = value;

							else if (key.ToLower () == "iconfile")
							{
								// IconFile 的格式可能是 "path, index"，也可能是纯路径
								var parts = value.Split (',');
								info.IconPath = parts [0].Trim ();
								int index = -1;
								if (parts.Length > 1)
									int.TryParse (parts [1].Trim (), out index);
							}
						}
						else if (currentSection == "taskbar")
						{
							if (key.ToLower () == "command")
								info.CommandType = value; // 覆盖或补充命令类型
						}
						// 可以继续添加对 [IE] 节的解析，如果必要的话
					}
				}
			}
			catch
			{
				// 解析失败时，info.CommandType 可能为 null，界面可据此显示错误或忽略
			}

			return info;
		}
	}
	public static class Utils
	{
		public static string GetQuickLaunchFolder ()
		{
			return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch");
		}
		public static IEnumerable<ShortcutInfo> EnumShortcutLinks (string folder)
		{
			var result = new List<ShortcutInfo> ();

			if (!System.IO.Directory.Exists (folder))
				return result;

			WshShell shell = null;
			try
			{
				shell = new WshShell ();
				string [] lnkFiles = System.IO.Directory.GetFiles (folder, "*.lnk");

				foreach (string lnkPath in lnkFiles)
				{
					IWshShortcut shortcut = null;
					try
					{
						shortcut = (IWshShortcut)shell.CreateShortcut (lnkPath);

						// 解析图标位置字符串
						string iconPath = "";
						int iconIndex = 0;
						string iconLoc = shortcut.IconLocation;
						if (!string.IsNullOrEmpty (iconLoc))
						{
							int commaIndex = iconLoc.LastIndexOf (',');
							if (commaIndex >= 0)
							{
								iconPath = iconLoc.Substring (0, commaIndex);
								int.TryParse (iconLoc.Substring (commaIndex + 1), out iconIndex);
							}
							else
							{
								iconPath = iconLoc;
							}
						}

						var si = new ShortcutInfo {
							Target = shortcut.TargetPath,
							LinkPath = lnkPath,
							Path = shortcut.TargetPath,
							Arguments = shortcut.Arguments ?? "",
							Description = shortcut.Description ?? "",
							HotKey = shortcut.Hotkey,
							ShowCommand = (int)shortcut.WindowStyle,
							WorkingDirectory = shortcut.WorkingDirectory ?? "",
							Icon = new Tuple<string, int> (iconPath, iconIndex)
						};

						result.Add (si);
					}
					catch
					{
						// 单个快捷方式解析失败不影响整体
					}
					finally
					{
						if (shortcut != null)
							Marshal.ReleaseComObject (shortcut);
					}
				}
			}
			finally
			{
				if (shell != null)
					Marshal.ReleaseComObject (shell);
			}

			return result;
		}
		public static IEnumerable<ScfInfo> EnumScfLinks (string folder)
		{
			var result = new List<ScfInfo> ();

			if (!Directory.Exists (folder))
				return result;

			string [] scfFiles = Directory.GetFiles (folder, "*.scf");
			foreach (string scfPath in scfFiles)
			{
				var info = ScfParser.Parse (scfPath);
				if (info != null)
					result.Add (info);
			}
			return result;
		}
	}
	public class QuickLaunchMonitor: IDisposable
	{
		private readonly string _folderPath;
		private readonly Dispatcher _dispatcher;
		private FileSystemWatcher _watcher;
		private Timer _refreshTimer;
		private bool _isDisposed;

		public ObservableCollection<QuickLaunchItemViewModel> Items { get; private set; }

		public QuickLaunchMonitor (string folderPath, Dispatcher dispatcher)
		{
			if (folderPath == null)
				throw new ArgumentNullException ("folderPath");
			if (dispatcher == null)
				throw new ArgumentNullException ("dispatcher");

			_folderPath = folderPath;
			_dispatcher = dispatcher;
			Items = new ObservableCollection<QuickLaunchItemViewModel> ();

			RefreshItems ();
			StartWatching ();
		}

		private void RefreshItems ()
		{
			if (_dispatcher.CheckAccess ())
				DoRefresh ();
			else
				_dispatcher.Invoke (new Action (DoRefresh));
		}

		private void DoRefresh ()
		{
			var newItems = new List<QuickLaunchItemViewModel> ();

			try
			{
				// 加载 .lnk 快捷方式
				foreach (var link in Utils.EnumShortcutLinks (_folderPath))
				{
					newItems.Add (new QuickLaunchItemViewModel (link));
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine ("加载 LNK 失败: " + ex.Message);
			}

			try
			{
				// 加载 .scf 命令文件
				foreach (var scf in Utils.EnumScfLinks (_folderPath))
				{
					newItems.Add (new QuickLaunchItemViewModel (scf));
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine ("加载 SCF 失败: " + ex.Message);
			}

			// 在 UI 线程更新集合
			Items.Clear ();
			foreach (var item in newItems)
			{
				Items.Add (item);
			}
		}

		private void StartWatching ()
		{
			// 监视所有文件，在事件中过滤 .lnk 和 .scf
			_watcher = new FileSystemWatcher (_folderPath) {
				NotifyFilter = NotifyFilters.FileName |
							   NotifyFilters.LastWrite |
							   NotifyFilters.Size |
							   NotifyFilters.CreationTime,
				IncludeSubdirectories = false,
				EnableRaisingEvents = true
			};

			_watcher.Created += OnFileSystemChanged;
			_watcher.Deleted += OnFileSystemChanged;
			_watcher.Changed += OnFileSystemChanged;
			_watcher.Renamed += OnFileSystemChanged;
		}

		private void OnFileSystemChanged (object sender, FileSystemEventArgs e)
		{
			// 只关心 .lnk 和 .scf 文件
			string ext = Path.GetExtension (e.FullPath)?.ToLower ();
			if (ext != ".lnk" && ext != ".scf")
				return;

			// 防抖处理
			if (_refreshTimer != null)
				_refreshTimer.Dispose ();
			_refreshTimer = new Timer (RefreshTimerCallback, null, 300, Timeout.Infinite);
		}

		private void RefreshTimerCallback (object state)
		{
			RefreshItems ();
		}

		public void Dispose ()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				if (_watcher != null)
				{
					_watcher.EnableRaisingEvents = false;
					_watcher.Dispose ();
					_watcher = null;
				}
				if (_refreshTimer != null)
				{
					_refreshTimer.Dispose ();
					_refreshTimer = null;
				}
			}
		}
	}
	public class QuickLaunchItemViewModel
	{
		public string FilePath { get; private set; }
		public ShortcutInfo Data { get; private set; }
		public ImageSource IconSource { get; private set; }
		public string Description { get; private set; }
		public string Title { get; private set; }
		public ICommand LeftClickCommand { get; private set; }
		public QuickLaunchItemViewModel (ShortcutInfo info)
		{
			FilePath = info?.LinkPath;
			Data = info;
			Title = System.IO.Path.GetFileNameWithoutExtension (info.LinkPath);
			Description = System.IO.Path.GetFileNameWithoutExtension (info.LinkPath);
			if (!string.IsNullOrWhiteSpace (info.Description))
				Description += "\n" + info.Description;

			LeftClickCommand = new RelayCommand (() => {
				try
				{
					Process.Start (new ProcessStartInfo (info.LinkPath) {
						WorkingDirectory = info.WorkingDirectory,
						Arguments = info.Arguments,
						UseShellExecute = true
					});
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine ("启动失败：" + ex.Message);
				}
			});

			IconSource = ExtractIcon (info);
		}
		private ImageSource ExtractIcon (ShortcutInfo info)
		{
			// 第一优先级：使用 Icon 属性指定的路径和索引
			string iconPath = info.Icon != null ? info.Icon.Item1 : null;
			int iconIndex = info.Icon != null ? info.Icon.Item2 : 0;

			if (!string.IsNullOrEmpty (iconPath))
			{
				var result = ExtractIconFromFile (iconPath, iconIndex);
				if (result != null)
					return result;
			}

			// 第二优先级：回退到快捷方式目标路径（info.Path）
			if (!string.IsNullOrEmpty (info.Path))
			{
				// 目标文件默认使用索引 0
				return ExtractIconFromFile (info.Path, 0);
			}

			return null;
		}
		/// <summary>
		/// 从指定文件中提取指定索引的图标，并转换为 WPF ImageSource
		/// </summary>
		private ImageSource ExtractIconFromFile (string filePath, int index)
		{
			try
			{
				// 展开可能存在的环境变量
				string expandedPath = Environment.ExpandEnvironmentVariables (filePath);
				if (!System.IO.File.Exists (expandedPath) && !Directory.Exists (expandedPath))
					return null;

				IntPtr [] iconHandles = new IntPtr [1];
				// 使用 ExtractIconEx 可以获取指定索引的大图标或小图标
				// 参数：文件路径、索引、大图标数组、小图标数组、要提取的图标数量
				int count = NativeMethods.ExtractIconEx (expandedPath, index, null, iconHandles, 1);
				if (count > 0 && iconHandles [0] != IntPtr.Zero)
				{
					try
					{
						return Imaging.CreateBitmapSourceFromHIcon (
							iconHandles [0],
							Int32Rect.Empty,
							BitmapSizeOptions.FromEmptyOptions ());
					}
					finally
					{
						NativeMethods.DestroyIcon (iconHandles [0]);
					}
				}

				// 如果 ExtractIconEx 失败，尝试用默认方式提取整个文件的第一个图标
				using (var icon = System.Drawing.Icon.ExtractAssociatedIcon (expandedPath))
				{
					if (icon == null) return null;
					return Imaging.CreateBitmapSourceFromHIcon (
						icon.Handle,
						Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions ());
				}
			}
			catch
			{
				return null;
			}
		}
		// 在你的 QuickLaunchItemViewModel 或某个工厂方法中
		public QuickLaunchItemViewModel (ScfInfo info)
		{
			FilePath = info?.FilePath;
			Data = null; // ScfInfo 不是 ShortcutInfo
			Title = info.Title;
			Description = info.CommandType;
			IconSource = ExtractIconFromSCF (info);

			//LeftClickCommand = new RelayCommand (() => {
			//	try
			//	{
			//		switch (info.CommandType?.ToLower ())
			//		{
			//			case "toggledesktop":
			//				// 模拟显示桌面的操作（最小化所有窗口）
			//				// 需要引用 COM 组件 "Microsoft Shell Controls and Automation" (Shell32)
			//				Shell32.Shell shell = new Shell32.Shell ();
			//				shell.MinimizeAll ();
			//				Marshal.ReleaseComObject (shell);
			//				break;
			//			case "explorer":
			//				Process.Start ("explorer.exe");
			//				break;
			//			// 可以继续添加其他命令的处理
			//			default:
			//				// 对未知命令，可以尝试直接启动该文件，让Windows自己处理
			//				Process.Start (new ProcessStartInfo (info.FilePath) { UseShellExecute = true });
			//				break;
			//		}
			//	}
			//	catch (Exception ex)
			//	{
			//		System.Diagnostics.Debug.WriteLine ("启动失败：" + ex.Message);
			//	}
			//});
			LeftClickCommand = new RelayCommand (() => {
				try
				{
					Process.Start (new ProcessStartInfo (info.FilePath) {
						UseShellExecute = true
					});
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine ("启动失败：" + ex.Message);
				}
			});
		}
		private ImageSource ExtractIconFromSCF (ScfInfo info)
		{
			// 第一优先级：使用 Icon 属性指定的路径和索引
			string iconPath = info.IconPath != null ? info.IconPath : null;
			int iconIndex = info.IconIndex;

			if (!string.IsNullOrEmpty (iconPath))
			{
				var result = ExtractIconFromFile (iconPath, iconIndex);
				if (result != null)
					return result;
			}

			// 第二优先级：回退到快捷方式目标路径（info.Path）
			if (!string.IsNullOrEmpty (info.FilePath))
			{
				// 目标文件默认使用索引 0
				return ExtractIconFromFile (info.FilePath, 0);
			}

			return null;
		}
	}
	internal static class NativeMethods
	{
		[DllImport ("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int ExtractIconEx (string lpszFile, int nIconIndex,
			IntPtr [] phiconLarge, IntPtr [] phiconSmall, int nIcons);
		[DllImport ("user32.dll", SetLastError = true)]
		public static extern bool DestroyIcon (IntPtr hIcon);
	}
	public class RelayCommand: ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;
		public RelayCommand (Action execute, Func<bool> canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}
		public bool CanExecute (object parameter)
		{
			return _canExecute == null || _canExecute ();
		}
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
		public void Execute (object parameter)
		{
			_execute ();
		}
	}
}
