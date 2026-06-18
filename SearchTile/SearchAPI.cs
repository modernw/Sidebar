using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
namespace WindowsModern.SearchTile
{
	public enum SearchProvider
	{
		Default,      // My Stuff
		Corpnet,
		HowDoI,
		MSN
	}
	public static class SystemSearch
	{
		public static void Search (string text, SearchProvider? provider = null)
		{
			var p = provider ?? SearchProvider.Default;

			if (p == SearchProvider.MSN)
			{
				OpenWebSearch (text);
				return;
			}

			if (HasSearchMs ())
				OpenWithSearchMs (text, p);
			else
				OpenWithXPSearch (text);
		}
		private static bool HasSearchMs ()
		{
			using (var key = Registry.ClassesRoot.OpenSubKey (@"search-ms"))
				return key != null;
		}

		#region search-ms（XP+WDS / Vista+）
		private static void OpenWithSearchMs (string text, SearchProvider provider)
		{
			string p = "local";

			switch (provider)
			{
				case SearchProvider.Corpnet: p = "enterprise"; break;
				case SearchProvider.HowDoI: p = "help"; break;
				case SearchProvider.MSN: p = "web"; break;
			}

			string uri =
				$"search-ms:query={Uri.EscapeDataString (text)}&provider={p}";

			Process.Start (new ProcessStartInfo {
				FileName = uri,
				UseShellExecute = true
			});
		}
		private static void OpenWebSearch (string text)
		{
			string url = "https://www.bing.com/search?q=" + Uri.EscapeDataString (text);

			Process.Start (new ProcessStartInfo {
				FileName = url,
				UseShellExecute = true
			});
		}
		#endregion
		#region XP 原生 Search Companion 回退（重点）
		[DllImport ("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern void SHFindFiles (IntPtr pidlRoot, IntPtr pidlSavedSearch);
		private static void OpenWithXPSearch (string text)
		{
			SHFindFiles (IntPtr.Zero, IntPtr.Zero);
			InjectSearchText (text);
		}
		#endregion
		#region XP 注入搜索词（IURLSearchHook2）
		[ComImport]
		[Guid ("5EE44DA4-6D32-46E3-86BC-07540BBD7A1F")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		private interface IURLSearchHook2
		{
			void TranslateWithSearchContext (
				[MarshalAs (UnmanagedType.LPWStr)] string url,
				IntPtr searchContext,
				[MarshalAs (UnmanagedType.LPWStr)] out string translatedUrl);
		}
		private static void InjectSearchText (string text)
		{
			try
			{
				Type t = Type.GetTypeFromCLSID (
					new Guid ("5EE44DA4-6D32-46E3-86BC-07540BBD7A1F"));

				var hook = (IURLSearchHook2)Activator.CreateInstance (t);

				string outUrl;
				hook.TranslateWithSearchContext (text, IntPtr.Zero, out outUrl);
			}
			catch { }
		}

		#endregion
	}
}
