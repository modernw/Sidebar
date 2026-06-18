using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidebar;

namespace SGInstall
{
	public enum GadgetStatus
	{
		NotExist,
		NeedUpdate,
		Reinstall
	};
	public static class WizardShare
	{
		public static TilePackageBase currPkg = null;
		public static bool isSilent = false;
		public static bool isVerySilent = false;
		public static string GetPackageDescription ()
		{
			if (currPkg == null) return "";
			else if (currPkg is TilePackage)
			{
				var a = currPkg as TilePackage;
				return a.StringResources.SuitableResource (a.Manifest.Properties.Description, a.Manifest.Properties.Description) ?? a.Manifest.Properties.Description;
			}
			else if (currPkg is TilePackageBundle)
			{
				var b = currPkg as TilePackageBundle;
				var a = b.Packages.FirstOrDefault ();
				if (a == null) return "";
				try
				{
					return a.StringResources.SuitableResource (a.Manifest.Properties.Description, a.Manifest.Properties.Description) ?? a.Manifest.Properties.Description;
				}
				catch
				{
					return a.Manifest.Properties.Description ?? "";
				}
			}
			return "";
		}
		public static GadgetStatus GetPackageNextOperation ()
		{
			if (currPkg == null) return GadgetStatus.NotExist;
			TileIdentity id = null;
			if (currPkg is TilePackage)
			{
				var a = currPkg as TilePackage;
				id = a.Manifest.Identity;
			}
			else if (currPkg is TilePackageBundle)
			{
				var b = currPkg as TilePackageBundle;
				var a = b.Packages.FirstOrDefault ();
				id = a?.Manifest?.Identity;
			}
			if (id == null) return GadgetStatus.NotExist;
			var installedPackage = Program.TileMgr.GetByFamilyName (id.FamilyName);
			if (installedPackage == null) return GadgetStatus.NotExist;
			if (id.Version > installedPackage.Manifest.Identity.Version) return GadgetStatus.NeedUpdate;
			else return GadgetStatus.Reinstall;
		}
		public static string GetPackageFamilyName ()
		{
			if (currPkg == null) return "";
			else if (currPkg is TilePackage)
			{
				var a = currPkg as TilePackage;
				return a.Manifest.Identity.FamilyName;
			}
			else if (currPkg is TilePackageBundle)
			{
				var b = currPkg as TilePackageBundle;
				return b.Manifest.Identity.FamilyName;
			}
			return "";
		}
	}
}
