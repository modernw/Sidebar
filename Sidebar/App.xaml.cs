using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace Sidebar
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App: Application
	{
		internal static ThemeManager ThemeMgr { get; } = new ThemeManager ();
		internal static ProgramFolder ProgramFolder { get; } = ProgramFolder.GlobalFolder;
		internal static ProgramFolder CurrentUserFolder { get; } = ProgramFolder.CurrentUserFolder;
		internal static SidebarConfig GlobalConfig { get; } = new SidebarConfig (ProgramFolder);
		internal static SidebarConfig CurrentUserConfig { get; } = new SidebarConfig (CurrentUserFolder);
		internal static TileManager TileMgr { get; } = new TileManager ();
		internal static Mutex GlobalMutex = null;
		internal const string AppIdentity = "WindowsModern.Sidebar!App";
		private void Application_Startup (object sender, StartupEventArgs e)
		{
			Directory.SetCurrentDirectory (AppDomain.CurrentDomain.BaseDirectory);
			bool createdNew = false;
			GlobalMutex = new Mutex (true, AppIdentity, out createdNew);
			if (!createdNew)
			{
				Shutdown ();
				return;
			}
			try
			{
				System.Windows.Forms.Application.EnableVisualStyles ();
				System.Windows.Forms.Application.SetCompatibleTextRenderingDefault (false);
			}
			catch { }
			Resources.Add ("GlobalConfig", GlobalConfig);
			Resources.Add ("CurrentUserConfig", CurrentUserConfig);
			//BrowserEmulation.SetWebBrowserEmulation ();
			var theme = ThemeMgr.CurrentUserTheme;
			ThemeManager.Apply (theme);
			var mainwnd = new MainWindow ();
			this.MainWindow = mainwnd;
			mainwnd.Show ();
			SidebarPipe.StartServer ();
		}
		private void Application_Exit (object sender, ExitEventArgs e)
		{
			SidebarPipe.StopServer ();
			SidebarPipe.Message = null;
			SidebarPipe.Mail = null;
		}
	}
}
