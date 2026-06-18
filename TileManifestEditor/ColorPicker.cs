using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TileManifestEditor
{
	public partial class ColorPicker: UserControl
	{
		public ColorPicker ()
		{
			InitializeComponent ();
		}
		[Browsable (true)]
		public Color Value
		{
			get { return panelPreviewColor.BackColor; }
			set { panelPreviewColor.BackColor = value; }
		}
		private void buttonPick_Click (object sender, EventArgs e)
		{
			colorDialog1.Color = panelPreviewColor.BackColor;
			var result = colorDialog1.ShowDialog (this);
			if (result == DialogResult.OK) Value = colorDialog1.Color;  
		}
	}
}
