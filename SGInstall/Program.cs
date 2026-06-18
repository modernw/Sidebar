using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sidebar;
namespace SGInstall
{
	static class Program
	{
		internal static ProgramFolder ProgramFolder { get; } = ProgramFolder.GlobalFolder;
		internal static ProgramFolder CurrentUserFolder { get; } = ProgramFolder.CurrentUserFolder;
		internal static TileManager TileMgr { get; } = new TileManager ();
		internal const string AppIdentity = "WindowsModern.Sidebar!Installer";
		internal static ILocaleResources StringResources => ProgramFolder.StringResources;
		internal static IPathResources FileResources => ProgramFolder.FileResources;
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new MainForm ());
		}
	}
}
