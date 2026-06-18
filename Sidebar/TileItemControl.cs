using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sidebar
{
	public partial class TileItemControl: UserControl
	{
		private enum Status
		{
			Normal,
			Lighting,
			Pressed,
			Selected,
			Focus
		}
		private Status buttonStatus
		{
			get
			{
				if (isclicked) return Status.Pressed;
				if (ishover) return Status.Lighting;
				if (selected) return Status.Selected;
				if (isfocus) return Status.Focus;
				return Status.Normal;
			}
		}

		private bool ishover = false;
		private bool isclicked = false;
		private bool isfocus = false;
		private bool selected = false;

		private Image logoImage;
		private string titleText = string.Empty;
		private string publisherText = string.Empty;
		private Font titleFont;
		private Font publisherFont;
		private TileMgrItem item = null;

		public TileItemControl ()
		{
			//InitializeComponent ();

			// 初始化字体
			titleFont = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts, 12f);
			publisherFont = FontHelper.GetFirstAvailableFont (FontHelper.preferredFonts, SystemFonts.DefaultFont.Size);

			// 启用自绘和双缓冲
			SetStyle (ControlStyles.AllPaintingInWmPaint |
					 ControlStyles.UserPaint |
					 ControlStyles.OptimizedDoubleBuffer |
					 ControlStyles.ResizeRedraw, true);

			TabStop = true;
			UpdateStatusDisplay ();
		}

		/// <summary>
		/// 保留此方法以兼容旧接口，现已无实际作用。
		/// </summary>
		[Browsable (false)]
		public void CreateOverlay () { }

		#region 公共接口

		public new string Name
		{
			get { return titleText; }
			set
			{
				if (titleText != value)
				{
					titleText = value;
					Invalidate ();
				}
			}
		}

		public string Publisher
		{
			get { return publisherText; }
			set
			{
				if (publisherText != value)
				{
					publisherText = value;
					Invalidate ();
				}
			}
		}

		[Browsable (false)]
		public string LogoPath
		{
			set
			{
				try
				{
					Logo = Image.FromFile (value);
				}
				catch
				{
					Logo = null;
				}
			}
		}

		public Image Logo
		{
			get { return logoImage; }
			set
			{
				if (logoImage != value)
				{
					logoImage?.Dispose ();
					logoImage = value;
					Invalidate ();
				}
			}
		}

		[Browsable (false)]
		public TileMgrItem Value
		{
			get { return item; }
			set
			{
				item = value;
				Name = item?.Title;
				Publisher = item?.Publisher;
				LogoPath = item?.Logo;
			}
		}

		public bool Selected
		{
			get { return selected; }
			set
			{
				if (selected != value)
				{
					selected = value;
					UpdateStatusDisplay ();
				}
			}
		}

		#endregion

		protected virtual void UpdateStatusDisplay ()
		{
			switch (buttonStatus)
			{
				case Status.Normal:
					BackColor = SystemColors.Control;
					ForeColor = SystemColors.ControlText;
					break;
				case Status.Lighting:
					BackColor = SystemColors.ControlLight;
					ForeColor = SystemColors.ControlText;
					break;
				case Status.Pressed:
					BackColor = SystemColors.ControlDark;
					ForeColor = SystemColors.ControlText;
					break;
				case Status.Selected:
					BackColor = SystemColors.Highlight;
					ForeColor = SystemColors.HighlightText;
					break;
				case Status.Focus:
					BackColor = SystemColors.Control;
					ForeColor = SystemColors.ControlText;
					break;
				default:
					BackColor = SystemColors.Control;
					ForeColor = SystemColors.ControlText;
					break;
			}
			Invalidate ();
		}

		#region 绘制

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			e.Graphics.Clear (BackColor);
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			// 绘制图片
			if (logoImage != null)
			{
				int imageSize = Math.Min (44, ClientSize.Height - 6);
				if (imageSize > 0)
				{
					Rectangle imageRect = new Rectangle (3, (ClientSize.Height - imageSize) / 2, imageSize, imageSize);
					g.DrawImage (logoImage, imageRect);
				}
			}

			// 绘制文本区域（模拟原 TableLayoutPanel 的行分布）
			int row1Height = 30;
			int row2Height = 20;
			int totalHeight = ClientSize.Height;
			int remaining = totalHeight - row1Height - row2Height;
			int topMargin = remaining > 0 ? remaining / 2 : 0;

			Rectangle titleRect = new Rectangle (50, topMargin, ClientSize.Width - 50, row1Height);
			Rectangle pubRect = new Rectangle (50, topMargin + row1Height, ClientSize.Width - 50, row2Height);

			TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
									TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

			TextRenderer.DrawText (g, titleText ?? string.Empty, titleFont, titleRect, ForeColor, flags);
			TextRenderer.DrawText (g, publisherText ?? string.Empty, publisherFont, pubRect, ForeColor, flags);
		}

		#endregion

		#region 鼠标与焦点事件

		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter (e);
			if (!ishover)
			{
				ishover = true;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
			if (ishover)
			{
				ishover = false;
				UpdateStatusDisplay ();
			}
			// 当鼠标离开控件时，强制释放按下状态
			if (isclicked)
			{
				isclicked = false;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
			if (!isclicked && e.Button == MouseButtons.Left)
			{
				isclicked = true;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
			if (isclicked)
			{
				isclicked = false;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
			if (!isfocus)
			{
				isfocus = true;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave (e);
			if (isfocus)
			{
				isfocus = false;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);
			if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && !isclicked)
			{
				isclicked = true;
				UpdateStatusDisplay ();
			}
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			base.OnKeyUp (e);
			if (isclicked)
			{
				isclicked = false;
				UpdateStatusDisplay ();
			}
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				logoImage?.Dispose ();
				titleFont?.Dispose ();
				publisherFont?.Dispose ();
			}
			base.Dispose (disposing);
		}
	}
}