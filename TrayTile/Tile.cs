using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Sidebar;
namespace WindowsModern.TrayTile
{
	public class Tile: TileBase
	{
		public static IProgramFolder TileFolder { get; set; }
		public static ISidebarFeatures SidebarFeatures { get; set; }
		private TilePanel tilePanel = null;
		public override void OnInitialize ()
		{
			TileFolder = this.Region;
			SidebarFeatures = this.Features;
			tilePanel = tilePanel ?? new TilePanel ();
			var tuip = TileUI as Panel;
			tuip.Children.Add (tilePanel);
		}
	}
}
