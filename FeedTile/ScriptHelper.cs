using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsModern.FeedTile
{
	[ComVisible (true)]
	public class ScriptHelper
	{
		public void OpenUrl (string url)
		{
			var l = new Uri (url, UriKind.RelativeOrAbsolute);
			Process.Start (l.AbsoluteUri);
		}
		public string GetString (string resId)
		{
			return Tile.TileFolder?.StringResources?.SuitableResource (resId);
		}
	}
}
