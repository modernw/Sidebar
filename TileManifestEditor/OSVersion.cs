using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidebar;
namespace TileManifestEditor
{
	public struct StartupOS
	{
		public string DisplayName { get; set; }
		public Sidebar.Version OSVersion { get; set; }
		public override bool Equals (object obj)
		{
			if (obj is StartupOS)
			{
				return this.OSVersion == ((StartupOS)obj).OSVersion;
			}
			return base.Equals (obj);
		}
		public override int GetHashCode ()
		{
			return OSVersion.GetHashCode ();
		}
		public override string ToString ()
		{
			return $"{DisplayName} ({OSVersion})";
		}
	}
	public static class OSVersion
	{
		public static readonly StartupOS [] OsSupportMappings = 
		{
			new StartupOS { DisplayName = "Windows XP Service Pack 3", OSVersion = new Sidebar.Version (5, 1, 2600, 0) },
			new StartupOS { DisplayName = "Windows Vista Service Pack 2", OSVersion = new Sidebar.Version (6, 0, 6002, 0) },
			new StartupOS { DisplayName = "Windows 7 Service Pack 1", OSVersion = new Sidebar.Version (6, 1, 7601, 0) },
			new StartupOS { DisplayName = "Windows 8", OSVersion = new Sidebar.Version (6, 2, 9200, 0) },
			new StartupOS { DisplayName = "Windows 8.1", OSVersion = new Sidebar.Version (6, 3, 9600, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1507)", OSVersion = new Sidebar.Version (10, 0, 10240, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1511)", OSVersion = new Sidebar.Version (10, 0, 10586, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1607)", OSVersion = new Sidebar.Version (10, 0, 14393, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1703)", OSVersion = new Sidebar.Version (10, 0, 15063, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1709)", OSVersion = new Sidebar.Version (10, 0, 16299, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1803)", OSVersion = new Sidebar.Version (10, 0, 17134, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1809)", OSVersion = new Sidebar.Version (10, 0, 17763, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1903)", OSVersion = new Sidebar.Version (10, 0, 18362, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 1909)", OSVersion = new Sidebar.Version (10, 0, 18363, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 2004)", OSVersion = new Sidebar.Version (10, 0, 19041, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 20H2)", OSVersion = new Sidebar.Version (10, 0, 19042, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 21H1)", OSVersion = new Sidebar.Version (10, 0, 19043, 0) },
			new StartupOS { DisplayName = "Windows 10 (Version 21H2)", OSVersion = new Sidebar.Version (10, 0, 19044, 0) },
			new StartupOS { DisplayName = "Windows 11 (Version 21H2)", OSVersion = new Sidebar.Version (10, 0, 22000, 0) },
			new StartupOS { DisplayName = "Windows 11 (Version 22H2)", OSVersion = new Sidebar.Version (10, 0, 22621, 0) },
			new StartupOS { DisplayName = "Windows 11 (Version 23H2)", OSVersion = new Sidebar.Version (10, 0, 22631, 0) },
			new StartupOS { DisplayName = "Windows 11 (Version 24H2)", OSVersion = new Sidebar.Version (10, 0, 26100, 0) },
			new StartupOS { DisplayName = "Future version of Windows", OSVersion = new Sidebar.Version (10, 0, 26100, 1) },
			new StartupOS { DisplayName = "Custom...", OSVersion = new Sidebar.Version (0, 0, 0, 0) }
		};
		public static bool IsInclude (this StartupOS [] stos, Sidebar.Version ver)
		{
			foreach (var item in stos)
			{
				if (item.OSVersion == ver) return true;
			}
			return false;
		}
		public static int GetIndex (this StartupOS [] stos, Sidebar.Version ver)
		{
			for (var i = 0; i < stos.Length; i ++)
			{
				if (stos [i].OSVersion == ver) return i;
			}
			return -1;
		}
		public static StartupOS ?GetItem (this StartupOS [] stos, Sidebar.Version ver)
		{
			foreach (var item in stos)
			{
				if (item.OSVersion == ver) return item;
			}
			return null;
		}
	}
}
