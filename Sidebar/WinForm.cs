using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sidebar
{
	public class WinFormWrapper: IWin32Window
	{
		public WinFormWrapper (IntPtr handle)
		{
			Handle = handle;
		}
		public IntPtr Handle { get; private set; }
	}
}
