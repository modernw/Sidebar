using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WindowsModern.TrayTile.Utils
{
	public interface ITrayIcon: INotifyPropertyChanged
	{
		long Key { get; }
		string ToolTip { get; }
		string ClassName { get; }
		ImageSource Icon { get; }
		ulong Tag { get; set; }
		object Data { get; }
		byte Type { get; }
		void OnHover ();
		void OnLeave ();
		void OnClick ();
		void OnRightClick ();
		void OnDoubleClick ();
	}
	public class TrayIconFromToolbar: ITrayIcon
	{
		private AutomationElement buttonElement = null;
		private ulong tag = 0;
		private ImageSource imgsrc = null;
		public TrayIconFromToolbar (AutomationElement ae)
		{
			if (ae == null) throw new ArgumentNullException ("Element is null");
			buttonElement = ae;
			try
			{
				var hicon = ToolbarIconExtractor.ExtractIconFromButton (buttonElement);
				if (hicon != IntPtr.Zero)
				{
					imgsrc = Imaging.CreateBitmapSourceFromHIcon (hicon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions ());
				}
			}
			catch { }
			OnPropertyChanged (nameof (ClassName));
			OnPropertyChanged (nameof (Data));
			OnPropertyChanged (nameof (Icon));
			OnPropertyChanged (nameof (Key));
			OnPropertyChanged (nameof (ToolTip));
			OnPropertyChanged (nameof (Tag));
		}
		public string ClassName => buttonElement?.Current.ClassName;
		public object Data => buttonElement;
		public ImageSource Icon => imgsrc;
		public long Key
		{
			get
			{
				var runtimeId = buttonElement.GetRuntimeId ();
				if (runtimeId == null || runtimeId.Length == 0) return -1;
				else if (runtimeId.Length == 1) return (long)(uint)runtimeId [0];
				else return ((long)(uint)runtimeId [0]) | ((long)(uint)runtimeId [1] << sizeof (int));
			}
		}
		public ulong Tag
		{
			get { return tag; }
			set { tag = value; OnPropertyChanged (nameof (Tag)); }
		}
		public string ToolTip
		{
			get
			{
				if (buttonElement == null) return string.Empty;
				try
				{
					object helpValue = buttonElement.GetCurrentPropertyValue (AutomationElement.HelpTextProperty);
					var helpText = helpValue as string;
					if (helpValue != AutomationElement.NotSupported && helpValue is string && !string.IsNullOrEmpty (helpText))
						return helpText;
				}
				catch {}
				var legacy = LegacyAccess.GetLegacyPattern (buttonElement);
				if (legacy == null) return null;
				string name;
				int hr = legacy.get_CurrentName (out name);
				if (hr == 0 && !string.IsNullOrEmpty (name)) return name;
				string tooltip = buttonElement.Current.Name;
				if (!string.IsNullOrEmpty (tooltip)) return tooltip;
				return string.Empty;
			}
		}
		public byte Type => 0;
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged (string propName) => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propName));
		public void OnClick () => buttonElement?.TryClick ();
		public void OnDoubleClick () => buttonElement?.DoubleClick ();
		public void OnHover () => buttonElement?.Hover ();
		public void OnLeave () => buttonElement?.Leave ();
		public void OnRightClick () => buttonElement?.RightClick ();
	}
}
