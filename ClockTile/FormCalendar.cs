using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClockTile
{
	public partial class FormCalendar: UserControl
	{
		public FormCalendar ()
		{
			InitializeComponent ();
		}

		private void monthCalendar1_Resize (object sender, EventArgs e)
		{
			calendarContainer.Size = new Size (
				monthCalendar1.Width - 1,
				monthCalendar1.Height - 1
			);
			monthCalendar1.Location = new Point (-1, -1);
		}
	}
}
