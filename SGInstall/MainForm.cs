using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidebar;
namespace SGInstall
{
	public partial class MainForm: Form
	{
		public MainForm ()
		{
			InitializeComponent ();
			Text = Program.StringResources.SuitableResource ("INSTALLER_WNDTITLE", "Sidebar Gadget Installer");
			wizardPageContainer1.Pages.Add (new Pages.FileSelect ());
			wizardPageContainer1.Pages.Add (new Pages.Loading ());
			wizardPageContainer1.Pages.Add (new Pages.Preinstall ());
			wizardPageContainer1.Pages.Add (new Pages.Installing ());
			wizardPageContainer1.Pages.Add (new Pages.InstallSuccess ());
			wizardPageContainer1.Pages.Add (new Pages.InstallFailed ());
		}
		private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
		{
			WizardShare.currPkg?.Dispose ();
		}
		private void MainForm_Load (object sender, EventArgs e)
		{
			var args = Environment.GetCommandLineArgs ();
			var filePath = "";
			for (var i = 1; i < args.Length; i++)
			{
				if (File.Exists (args [i]) && string.IsNullOrWhiteSpace (filePath))
				{
					filePath = args [i];
				}
				if (args [i].NEquals ("-silent") || args [i].NEquals ("/silent"))
				{
					WizardShare.isSilent = true;
				}
				if (args [i].NEquals ("-verysilent") || args [i].NEquals ("/verysilent"))
				{
					WizardShare.isSilent = true;
					WizardShare.isVerySilent = true;
					this.Visible = false;
				}
			}
			if (File.Exists (filePath))
			{
				wizardPageContainer1.CurrentId = "select_file";
				wizardPageContainer1.Mail ("form", "loading", "PackageFilePath", filePath);
				wizardPageContainer1.CurrentId = "loading";
			}
			else wizardPageContainer1.CurrentId = "select_file";
		}
	}
}
