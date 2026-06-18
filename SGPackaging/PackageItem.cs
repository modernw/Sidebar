using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sidebar;

namespace SGPackaging
{
	public class PackageItem: INotifyPropertyChanged, IDisposable
	{
		private string _filePath;
		private ProcessorArchitecture _architecture = ProcessorArchitecture.Unknown;
		private bool _isLoadingArch = false;
		private CancellationTokenSource _cts = null;
		private Task _currentLoadTask = null;

		public string FilePath
		{
			get { return _filePath; }
			set
			{
				if (_filePath != value)
				{
					_filePath = value;
					OnPropertyChanged ("FilePath");
					OnPropertyChanged ("FileName");
					LoadArchitectureAsync ();
				}
			}
		}

		public string FileName
		{
			get
			{
				if (string.IsNullOrEmpty (_filePath))
					return "";
				try
				{
					return Path.GetFileName (_filePath);
				}
				catch
				{
					return "";
				}
			}
		}

		public ProcessorArchitecture Architecture
		{
			get { return _architecture; }
			private set
			{
				if (_architecture != value)
				{
					_architecture = value;
					OnPropertyChanged ("Architecture");
				}
			}
		}

		public bool IsLoadingArchitecture
		{
			get { return _isLoadingArch; }
		}

		public PackageItem () { }

		public PackageItem (string filePath)
		{
			FilePath = filePath;
		}

		private void LoadArchitectureAsync ()
		{
			// 取消之前的加载任务
			CancelLoad ();

			if (string.IsNullOrEmpty (_filePath))
				return;

			_isLoadingArch = true;
			OnPropertyChanged ("IsLoadingArchitecture");

			_cts = new CancellationTokenSource ();
			CancellationToken token = _cts.Token;

			_currentLoadTask = Task.Factory.StartNew (() => {
				if (token.IsCancellationRequested)
					return null;
				return TilePackageReadManager.GetPackage (_filePath);
			}, token)
			.ContinueWith (task => {
				// 清理任务引用
				_currentLoadTask = null;
				// 如果任务被取消或有异常，不更新架构
				if (task.IsCanceled || task.Exception != null)
				{
					// 不修改 Architecture
				}
				else
				{
					TilePackageBase package = task.Result;
					ProcessorArchitecture newArch = ProcessorArchitecture.Unknown;
					TilePackageBundle bundle = package as TilePackageBundle;
					if (bundle != null)
					{
						newArch = bundle.Manifest.Identity.ProcessorArchitecture;
					}
					else
					{
						TilePackage single = package as TilePackage;
						if (single != null)
							newArch = single.Manifest.Identity.ProcessorArchitecture;
					}
					Architecture = newArch;
				}
				_isLoadingArch = false;
				OnPropertyChanged ("IsLoadingArchitecture");
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		private void CancelLoad ()
		{
			if (_cts != null)
			{
				_cts.Cancel ();
				_cts.Dispose ();
				_cts = null;
			}
			_currentLoadTask = null;
			_isLoadingArch = false;
			OnPropertyChanged ("IsLoadingArchitecture");
		}

		public void Dispose ()
		{
			CancelLoad ();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged (string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}