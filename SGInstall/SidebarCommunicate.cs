using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidebar;
namespace SGInstall
{
	public static class SidebarNotification
	{
		public static void NotifyPinTile (string familyName)
		{
			SidebarMail.Send ("NotifyPinTile", familyName);
		}
		public static void NotifyUnpinTile (string familyName)
		{
			SidebarMail.Send ("NotifyUnpinTile", familyName);
		}
	}
}
