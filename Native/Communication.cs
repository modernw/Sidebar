using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Sidebar
{
	[ComVisible (true)]
	public class NotifyIconNotification
	{
		public int Timeout { get; set; } = 5000;
		public string Title { get; set; }
		public string Content { get; set; }
		public ToolTipIcon Icon { get; set; } = ToolTipIcon.None;
	}
}
