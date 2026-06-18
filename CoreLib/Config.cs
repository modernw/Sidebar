using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.Init;
namespace Sidebar
{
	public class ProgramFolder: IProgramFolder
	{
		public string FolderPath { get; }
		public bool Exists => !string.IsNullOrWhiteSpace (FolderPath) && Directory.Exists (FolderPath);
		public bool EnsureExists ()
		{
			if (!Exists) return Directory.CreateDirectory (FolderPath).Exists;
			else return true;
		}
		private ILocaleResources lr = null;
		private IPathResources pr = null;
		public ILocaleResources StringResources => lr;
		public IPathResources FileResources => pr;
		internal ProgramFolder (string folderPath, bool autoCreate = true)
		{
			if (!Directory.Exists (folderPath) && autoCreate)
				Directory.CreateDirectory (folderPath);
			FolderPath = folderPath;
			try { lr = LocaleResources.CreateFromFile (Path.Combine (FolderPath, "Locale.xml")); } catch { lr = new LocaleResources (); }
			try { pr = PathResources.CreateFromFile (Path.Combine (FolderPath, "Path.xml")); } catch { pr = new PathResources (); }
		}
		public InitConfig InitConfig => new InitConfig (Path.Combine (FolderPath, "config.ini"));
		public XmlConfig XmlConfig => new XmlConfig (FolderPath);
		public static ProgramFolder GlobalFolder { get; } = new ProgramFolder (AppDomain.CurrentDomain.BaseDirectory);
		public static ProgramFolder CurrentUserFolder { get; } = new ProgramFolder (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "Windows Modern\\Sidebar"));
		public static ProgramFolder CreateFromPath (string fp) => new ProgramFolder (fp);
	}
}
