namespace SGInstall
{
	partial class MainForm
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.wizardPageContainer1 = new SGInstall.WizardPageContainer();
			this.SuspendLayout();
			// 
			// wizardPageContainer1
			// 
			this.wizardPageContainer1.CurrentId = null;
			this.wizardPageContainer1.CurrentIndex = -1;
			this.wizardPageContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wizardPageContainer1.Font = new System.Drawing.Font("微软雅黑", 9F);
			this.wizardPageContainer1.Location = new System.Drawing.Point(0, 0);
			this.wizardPageContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.wizardPageContainer1.Name = "wizardPageContainer1";
			this.wizardPageContainer1.Size = new System.Drawing.Size(648, 388);
			this.wizardPageContainer1.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(648, 388);
			this.Controls.Add(this.wizardPageContainer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "Form1";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private WizardPageContainer wizardPageContainer1;
	}
}

