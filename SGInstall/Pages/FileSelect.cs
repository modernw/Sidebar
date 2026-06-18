using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGInstall.Pages
{
	class FileSelect: WizardPage
	{
		public FileSelect ()
		{
			InitializeComponent ();
			label1.Text = Program.StringResources.SuitableResource ("INSTALLER_SELECTFILE", "Please select a package file to install.");
			openFileDialog1.Filter = String.Format (
				"{0}|*.sgpkg;*.sgpkgbundle|{1}|*.*",
				Program.StringResources.SuitableResource ("INSTALLER_FILETYPE_PKG", "Sidebar Gadget Package (*.sgpkg, *.sgpkgbundle)"),
				Program.StringResources.SuitableResource ("INSTALLER_FILETYPE_ALL", "All Files (*.*)")
			);
			label1.Font = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts, 16F);
		}
		public override bool ShowBack => false;
		public override string NextButtonTitle => Program.ProgramFolder.StringResources.SuitableResource ("INSTALLER_BTN_SELECTFILE", "Open File");
		public override string Id => "select_file";
		public override string DisplayName => "Select File";
		public override void OnAlreadyLoad ()
		{
			if (WizardShare.isVerySilent)
				PageContainer.Cancel ();
		}
		public override bool OnNextButtonClick (object sender, EventArgs e)
		{
			var res = openFileDialog1.ShowDialog (this);
			if (res == System.Windows.Forms.DialogResult.OK)
			{
				return Send ("loading", "PackageFilePath", openFileDialog1.FileName);
			}
			return false;
		}
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Label label1;

		private void InitializeComponent ()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
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
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(579, 401);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(23, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(533, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please select a package file to install.";
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Filter = "Sidebar Gadget Package (*.sgpkg, *.sgpkgbundle)|*.sgpkg;*.sgpkgbundle|All Files (" +
    "*.*)|*.*";
			// 
			// FileSelect
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "FileSelect";
			this.Size = new System.Drawing.Size(579, 401);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}
	}
}
