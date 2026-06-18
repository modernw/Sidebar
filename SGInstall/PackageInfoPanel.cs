using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidebar;
using System.IO;
namespace SGInstall
{
	public partial class PackageInfoPanel: UserControl
	{
		public PackageInfoPanel ()
		{
			InitializeComponent ();
			labelTitle.Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts, null, FontStyle.Bold);
			Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts);
		}
		[Browsable (false)]
		public TilePackageBase Package
		{
			set
			{
				if (value is TilePackageBundle)
				{
					var b = value as TilePackageBundle;
					var sp = b?.Packages?.FirstOrDefault ();
					var m = sp?.Manifest;
					var i = m?.Identity;
					var p = m?.Properties;
					var sr = sp?.StringResources;
					labelTitle.Text = sr?.SuitableResource (p?.DisplayName, p?.DisplayName) ?? p?.DisplayName ?? "";
					labelPublisher.Text = String.Format (
						Program.StringResources.SuitableResource ("INSTALLER_INFO_PUBLISHER"),
						sr?.SuitableResource (p?.Publisher, p?.Publisher) ?? p?.Publisher ??""
					);
					labelVersion.Text = String.Format (
						Program.StringResources.SuitableResource ("INSTALLER_INFO_VERSION"),
						i?.Version.Expression ?? ""
					);
					try
					{
						var lp = sp?.FileResources?.SuitableResource (p?.Logo, p?.Logo) ?? p?.Logo;
						var bts = sp?.ExtractFile (lp);
						using (var st = new MemoryStream (bts, false))
						{
							pictureBox1.Image = Image.FromStream (st);
						}
					}
					catch
					{
						pictureBox1.Image = null;
					}
				}
				else if (value is TilePackage)
				{
					var sp = value as TilePackage;
					var m = sp?.Manifest;
					var i = m?.Identity;
					var p = m?.Properties;
					var sr = sp?.StringResources;
					labelTitle.Text = sr?.SuitableResource (p?.DisplayName, p?.DisplayName) ?? "";
					labelPublisher.Text = String.Format (
						Program.StringResources.SuitableResource ("INSTALLER_INFO_PUBLISHER"),
						sr?.SuitableResource (p?.Publisher, p?.Publisher) ?? ""
					);
					labelVersion.Text = String.Format (
						Program.StringResources.SuitableResource ("INSTALLER_INFO_VERSION"),
						i?.Version.Expression ?? ""
					);
					try
					{
						var lp = sp?.FileResources?.SuitableResource (p?.Logo, p?.Logo) ?? p?.Logo;
						var bts = sp?.ExtractFile (lp);
						using (var st = new MemoryStream (bts, false))
						{
							pictureBox1.Image = Image.FromStream (st);
						}
					}
					catch
					{
						pictureBox1.Image = null;
					}
				}
				else
				{
					labelTitle.Text = "";
					labelPublisher.Text = "";
					labelVersion.Text = "";
					labelCheck.Text = "";
				}
				var checkPass = IsSuitablePackage (value);
				labelCheck.Text =
					Program.StringResources.SuitableResource (
						checkPass ? "INSTALLER_INFO_CANINS" : "INSTALLER_INFO_CANNOTINS"
					);
				if (value == null) labelCheck.Text = "";
			}
		}
		private Sidebar.Version GetSystemVersion () => new Sidebar.Version (
				(ushort)Environment.OSVersion.Version.Major,
				(ushort)Environment.OSVersion.Version.Minor,
				(ushort)Environment.OSVersion.Version.Build,
				(ushort)Environment.OSVersion.Version.Revision
			);
		private bool IsSuitablePackage (TilePackageBase tp)
		{
			if (tp == null) return false;
			if (tp is TilePackageBundle)
			{
				var b = tp as TilePackageBundle;
				foreach (var sp in b.Packages)
				{
					if (sp.Manifest.Identity.ProcessorArchitecture == ProcessorDetector.GetCurrentArchitecture ())
					{
						return sp.Manifest.Prerequisites.OSMinVersion <= GetSystemVersion ();
					}
				}
				return false;
			}
			else if (tp is TilePackage)
			{
				var a = tp as TilePackage;
				return (a.Manifest.Identity.ProcessorArchitecture == ProcessorArchitecture.Neutral ||
					a.Manifest.Identity.ProcessorArchitecture == ProcessorDetector.GetCurrentArchitecture ()) &&
					a.Manifest.Prerequisites.OSMinVersion <= GetSystemVersion ();
			}
			return false;
		}
		[Browsable (true)]
		public bool ShowCheck
		{
			get { return labelCheck.Visible; }
			set { labelCheck.Visible = value; }
		}
	}
}
