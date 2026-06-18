namespace TileManifestEditor
{
	partial class ColorPicker
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

		#region 组件设计器生成的代码

		/// <summary> 
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent ()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panelPreviewColor = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonPick = new System.Windows.Forms.Button();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.tableLayoutPanel1.SuspendLayout();
			this.panelPreviewColor.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.panelPreviewColor, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonPick, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(191, 32);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// panelPreviewColor
			// 
			this.panelPreviewColor.AutoSize = true;
			this.panelPreviewColor.ColumnCount = 3;
			this.panelPreviewColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panelPreviewColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.panelPreviewColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panelPreviewColor.Controls.Add(this.label1, 1, 1);
			this.panelPreviewColor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPreviewColor.Location = new System.Drawing.Point(0, 0);
			this.panelPreviewColor.Margin = new System.Windows.Forms.Padding(0);
			this.panelPreviewColor.Name = "panelPreviewColor";
			this.panelPreviewColor.RowCount = 3;
			this.panelPreviewColor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panelPreviewColor.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.panelPreviewColor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panelPreviewColor.Size = new System.Drawing.Size(92, 32);
			this.panelPreviewColor.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.White;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(14, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Preview";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonPick
			// 
			this.buttonPick.AutoSize = true;
			this.buttonPick.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonPick.Location = new System.Drawing.Point(92, 0);
			this.buttonPick.Margin = new System.Windows.Forms.Padding(0);
			this.buttonPick.Name = "buttonPick";
			this.buttonPick.Size = new System.Drawing.Size(84, 32);
			this.buttonPick.TabIndex = 1;
			this.buttonPick.Text = "Pick";
			this.buttonPick.UseVisualStyleBackColor = true;
			this.buttonPick.Click += new System.EventHandler(this.buttonPick_Click);
			// 
			// ColorPicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(100, 32);
			this.Name = "ColorPicker";
			this.Size = new System.Drawing.Size(191, 32);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panelPreviewColor.ResumeLayout(false);
			this.panelPreviewColor.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel panelPreviewColor;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonPick;
		private System.Windows.Forms.ColorDialog colorDialog1;
	}
}
