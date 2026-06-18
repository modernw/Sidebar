using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGInstall.Pages
{
	class Preinstall: WizardPage
	{
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private PackageInfoPanel packageInfoPanel1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label1;
		public override string Id => "preinstall";
		public override string DisplayName => "Preinstall";
		public override bool CanBack => false;
		public override bool ShowBack => false;
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
			textBox1.Text = WizardShare.GetPackageDescription () ?? "";
			switch (WizardShare.GetPackageNextOperation ())
			{
				case GadgetStatus.NotExist:
					label1.Text = Program.StringResources.SuitableResource ("INSTALLER_PRE_TITLE_INSTALL", "Install?"); break;
				case GadgetStatus.NeedUpdate:
					label1.Text = Program.StringResources.SuitableResource ("INSTALLER_PRE_TITLE_UPDATE", "Update?"); break;
				case GadgetStatus.Reinstall:
					label1.Text = Program.StringResources.SuitableResource ("INSTALLER_PRE_TITLE_REINSTALL", "Reinstall?"); break;
			}
			if (WizardShare.isSilent || WizardShare.isVerySilent)
				PageContainer.Next ();
		}
		public Preinstall ()
		{
			InitializeComponent ();
			label1.Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts, 16F);
			Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts);
		}
		private void InitializeComponent ()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.packageInfoPanel1 = new SGInstall.PackageInfoPanel();
			this.textBox1 = new System.Windows.Forms.TextBox();
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
			this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 111F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
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
			this.packageInfoPanel1.ShowCheck = true;
			this.packageInfoPanel1.Size = new System.Drawing.Size(519, 103);
			this.packageInfoPanel1.TabIndex = 1;
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.Color.White;
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Location = new System.Drawing.Point(23, 182);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(519, 226);
			this.textBox1.TabIndex = 2;
			// 
			// Preinstall
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "Preinstall";
			this.Size = new System.Drawing.Size(565, 421);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}
	}
}
