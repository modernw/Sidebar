using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SGInstall.Pages
{
	class Installing: WizardPage
	{
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private PackageInfoPanel packageInfoPanel1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label1;
		public Installing ()
		{
			InitializeComponent ();
			label1.Text = Program.StringResources.SuitableResource ("INSTALLER_ING_TITLE", "Installing...");
			label1.Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts, 16F);
			Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts);
		}
		public override string Id => "installing";
		public override string DisplayName => "Progress";
		public override bool CanBack => false;
		public override bool ShowBack => false;
		public override bool CanCancel => false;
		public override bool ShowCancel => false;
		public override bool CanNext => false;
		public override bool ShowNext => false;
		public override string NextButtonTitle
		{
			get
			{
				switch (WizardShare.GetPackageNextOperation ())
				{
					case GadgetStatus.NotExist: return Program.StringResources.SuitableResource ("INSTALLER_BTN_INSTALL", "Install");
					case GadgetStatus.NeedUpdate: return Program.StringResources.SuitableResource ("INSTALLER_BTN_UPDATE", "Update");
					case GadgetStatus.Reinstall: return Program.StringResources.SuitableResource ("INSTALLER_BTN_REINSTALL", "Reinstall");
				}
				return base.NextButtonTitle;
			}
		}
		public override void OnAlreadyLoad ()
		{
			packageInfoPanel1.Package = WizardShare.currPkg;
			InstallPackage ();
		}
		private void InstallPackage ()
		{
			var operation = WizardShare.GetPackageNextOperation ();
			var familyName = WizardShare.GetPackageFamilyName ();
			if (operation == GadgetStatus.NeedUpdate || operation == GadgetStatus.Reinstall)
			{
				Task.Factory.StartNew (() =>
				{
					Sidebar.SidebarMail.Send ("NotifyUnpinTile", familyName);
					Thread.Sleep (500);
				}).ContinueWith (_ =>
				{
					Task<bool> updateTask = Task.Factory.StartNew (() =>
					{
						try
						{
							Program.TileMgr.UpdatePackage (WizardShare.currPkg.FileName, OnProgress);
							return true; 
						}
						catch
						{
							return false;
						}
					});
					updateTask.ContinueWith (t =>
					{
						if (t.Result)
						{
							ToSuccessPage ();
						}
						else
						{
							PerformInstall (); 
						}
					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}, TaskScheduler.FromCurrentSynchronizationContext ());
			}
			else if (operation == GadgetStatus.NotExist)
			{
				PerformInstall ();
			}
		}
		private void PerformInstall ()
		{
			Task.Factory.StartNew (() =>
			{
				Program.TileMgr.InstallPackage (WizardShare.currPkg.FileName, OnProgress);
			}).ContinueWith (task =>
			{
				if (task.Exception != null)
				{
					ToFailedPage (task.Exception.InnerException ?? task.Exception);
				}
				else
				{
					ToSuccessPage ();
				}
			}, TaskScheduler.FromCurrentSynchronizationContext ());
		}
		private void OnProgress (int curr, int total, double percent)
		{
			if (progressBar1.InvokeRequired)
			{
				progressBar1.Invoke (new Action (() => OnProgress (curr, total, percent)));
				return;
			}
			progressBar1.Maximum = total;
			progressBar1.Value = curr;
		}
		private void ToSuccessPage ()
		{
			if (this.InvokeRequired) this.Invoke (new Action (ToSuccessPage));
			else PageContainer.Jump ("install_success");
		}
		private void ToFailedPage (Exception e)
		{
			if (this.InvokeRequired) this.Invoke (new Action<Exception> (ToFailedPage), e);
			else
			{
				Send ("install_failed", "SetException", e, e.GetType ());
				PageContainer.Jump ("install_failed");
			}
		}
		private void InitializeComponent ()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.packageInfoPanel1 = new SGInstall.PackageInfoPanel();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.packageInfoPanel1, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.progressBar1, 1, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 121F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(565, 421);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(23, 23);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(519, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "Will install a gadget.";
			// 
			// packageInfoPanel1
			// 
			this.packageInfoPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.packageInfoPanel1.Font = new System.Drawing.Font("微软雅黑", 9F);
			this.packageInfoPanel1.Location = new System.Drawing.Point(23, 62);
			this.packageInfoPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.packageInfoPanel1.Name = "packageInfoPanel1";
			this.packageInfoPanel1.ShowCheck = false;
			this.packageInfoPanel1.Size = new System.Drawing.Size(519, 113);
			this.packageInfoPanel1.TabIndex = 1;
			// 
			// progressBar1
			// 
			this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressBar1.Location = new System.Drawing.Point(23, 352);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(519, 26);
			this.progressBar1.Step = 1;
			this.progressBar1.TabIndex = 2;
			// 
			// Installing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "Installing";
			this.Size = new System.Drawing.Size(565, 421);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}
	}
}
