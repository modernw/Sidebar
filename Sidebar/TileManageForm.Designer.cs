namespace Sidebar
{
	partial class TileManageForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tileListView = new System.Windows.Forms.FlowLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tileTitle = new System.Windows.Forms.Label();
			this.tilePublisher = new System.Windows.Forms.Label();
			this.tileVersion = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tileLogo = new System.Windows.Forms.PictureBox();
			this.tileDiscription = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.addToButton = new System.Windows.Forms.Button();
			this.removeTileButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tileLogo)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tileListView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
			this.splitContainer1.Size = new System.Drawing.Size(804, 495);
			this.splitContainer1.SplitterDistance = 288;
			this.splitContainer1.TabIndex = 0;
			// 
			// tileListView
			// 
			this.tileListView.AutoScroll = true;
			this.tileListView.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tileListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tileListView.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.tileListView.Location = new System.Drawing.Point(0, 0);
			this.tileListView.Name = "tileListView";
			this.tileListView.Size = new System.Drawing.Size(288, 495);
			this.tileListView.TabIndex = 0;
			this.tileListView.WrapContents = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.tileTitle, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.tilePublisher, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.tileVersion, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.tileDiscription, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 9);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 11;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(512, 495);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tileTitle
			// 
			this.tileTitle.AutoSize = true;
			this.tileTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tileTitle.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.tileTitle.Location = new System.Drawing.Point(20, 20);
			this.tileTitle.Margin = new System.Windows.Forms.Padding(0);
			this.tileTitle.Name = "tileTitle";
			this.tileTitle.Size = new System.Drawing.Size(362, 27);
			this.tileTitle.TabIndex = 0;
			// 
			// tilePublisher
			// 
			this.tilePublisher.AutoSize = true;
			this.tilePublisher.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tilePublisher.Location = new System.Drawing.Point(20, 55);
			this.tilePublisher.Margin = new System.Windows.Forms.Padding(0);
			this.tilePublisher.Name = "tilePublisher";
			this.tilePublisher.Size = new System.Drawing.Size(362, 15);
			this.tilePublisher.TabIndex = 1;
			// 
			// tileVersion
			// 
			this.tileVersion.AutoSize = true;
			this.tileVersion.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tileVersion.Location = new System.Drawing.Point(20, 78);
			this.tileVersion.Margin = new System.Windows.Forms.Padding(0);
			this.tileVersion.Name = "tileVersion";
			this.tileVersion.Size = new System.Drawing.Size(362, 42);
			this.tileVersion.TabIndex = 2;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.tableLayoutPanel2.Controls.Add(this.tileLogo, 1, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(382, 20);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel1.SetRowSpan(this.tableLayoutPanel2, 5);
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(110, 100);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// tileLogo
			// 
			this.tileLogo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tileLogo.Location = new System.Drawing.Point(10, 0);
			this.tileLogo.Margin = new System.Windows.Forms.Padding(0);
			this.tileLogo.Name = "tileLogo";
			this.tileLogo.Size = new System.Drawing.Size(100, 100);
			this.tileLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.tileLogo.TabIndex = 0;
			this.tileLogo.TabStop = false;
			// 
			// tileDiscription
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.tileDiscription, 2);
			this.tileDiscription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tileDiscription.Location = new System.Drawing.Point(23, 131);
			this.tileDiscription.Multiline = true;
			this.tileDiscription.Name = "tileDiscription";
			this.tileDiscription.ReadOnly = true;
			this.tileDiscription.Size = new System.Drawing.Size(466, 283);
			this.tileDiscription.TabIndex = 5;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoScroll = true;
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this.addToButton);
			this.flowLayoutPanel1.Controls.Add(this.removeTileButton);
			this.flowLayoutPanel1.Controls.Add(this.deleteButton);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(20, 425);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(472, 60);
			this.flowLayoutPanel1.TabIndex = 6;
			// 
			// addToButton
			// 
			this.addToButton.AutoSize = true;
			this.addToButton.Location = new System.Drawing.Point(3, 3);
			this.addToButton.Name = "addToButton";
			this.addToButton.Size = new System.Drawing.Size(146, 30);
			this.addToButton.TabIndex = 0;
			this.addToButton.UseVisualStyleBackColor = true;
			this.addToButton.Click += new System.EventHandler(this.addToButton_Click);
			// 
			// removeTileButton
			// 
			this.removeTileButton.AutoSize = true;
			this.removeTileButton.Location = new System.Drawing.Point(155, 3);
			this.removeTileButton.Name = "removeTileButton";
			this.removeTileButton.Size = new System.Drawing.Size(146, 30);
			this.removeTileButton.TabIndex = 2;
			this.removeTileButton.UseVisualStyleBackColor = true;
			this.removeTileButton.Click += new System.EventHandler(this.removeTileButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.AutoSize = true;
			this.deleteButton.Location = new System.Drawing.Point(307, 3);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(146, 30);
			this.deleteButton.TabIndex = 1;
			this.deleteButton.UseVisualStyleBackColor = true;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// TileManageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(804, 495);
			this.Controls.Add(this.splitContainer1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TileManageForm";
			this.ShowIcon = false;
			this.Text = "TileManager";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TileManageForm_FormClosed);
			this.Load += new System.EventHandler(this.TileManager_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tileLogo)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label tileTitle;
		private System.Windows.Forms.Label tilePublisher;
		private System.Windows.Forms.Label tileVersion;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.PictureBox tileLogo;
		private System.Windows.Forms.TextBox tileDiscription;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button addToButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.FlowLayoutPanel tileListView;
		private System.Windows.Forms.Button removeTileButton;
	}
}