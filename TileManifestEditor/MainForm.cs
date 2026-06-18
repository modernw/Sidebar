using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sidebar;
namespace TileManifestEditor
{
	public partial class MainForm: Form
	{
		// 当前编辑的文件路径
		private string currentFilePath;

		public MainForm ()
		{
			InitializeComponent ();
		}
		private Sidebar.Version GetCurrentOSVersion ()
		{
			var osVer = Environment.OSVersion.Version;
			return new Sidebar.Version (
				(ushort)Math.Max (0, osVer.Major),
				(ushort)Math.Max (0, osVer.Minor),
				(ushort)Math.Max (0, osVer.Build),
				(ushort)Math.Max (0, osVer.Revision));
		}
		/// <summary>
		/// 设置 Min 版本的自定义控件值
		/// </summary>
		private void SetMinCustomValues (Sidebar.Version ver)
		{
			inputOsMinCVerMajor.Value = ver.Major;
			inputOsMinCVerMinor.Value = ver.Minor;
			inputOsMinCVerBuild.Value = ver.Build;
			inputOsMinCVerRevision.Value = ver.Revision;
		}
		/// <summary>
		/// 启用/禁用 Min 版本的自定义控件
		/// </summary>
		private void EnableMinCustomControls (bool enabled)
		{
			inputOsMinCVerMajor.Enabled = enabled;
			inputOsMinCVerMinor.Enabled = enabled;
			inputOsMinCVerBuild.Enabled = enabled;
			inputOsMinCVerRevision.Enabled = enabled;
		}
		/// <summary>
		/// 启用/禁用 Max 版本的自定义控件
		/// </summary>
		private void EnableMaxCustomControls (bool enabled)
		{
			inputOsMaxCVerMajor.Enabled = enabled;
			inputOsMaxCVerMinor.Enabled = enabled;
			inputOsMaxCVerBuild.Enabled = enabled;
			inputOsMaxCVerRevision.Enabled = enabled;
		}
		/// <summary>
		/// 设置 Max 版本的自定义控件值
		/// </summary>
		private void SetMaxCustomValues (Sidebar.Version ver)
		{
			inputOsMaxCVerMajor.Value = ver.Major;
			inputOsMaxCVerMinor.Value = ver.Minor;
			inputOsMaxCVerBuild.Value = ver.Build;
			inputOsMaxCVerRevision.Value = ver.Revision;
		}
		/// <summary>
		/// 所有需要在代码中设置的初始化工作（枚举下拉框、事件绑定、弃用样式等）
		/// </summary>
		private void InitializeForm ()
		{
			// ---------- 枚举下拉框数据源 ----------
			selectTilePropType.DataSource = Enum.GetValues (typeof (TileType));
			selectTileVEOverflow.DataSource = Enum.GetValues (typeof (TileOverflow));
			selectTileVEDfltTileSize.DataSource = Enum.GetValues (typeof (TileSize));
			selectTileVEForegroundColor.DataSource = Enum.GetValues (typeof (TileForegroundColor));
			selectTilePreOsMin.DataSource = OSVersion.OsSupportMappings.ToList ();
			selectTilePreOsMax.DataSource = OSVersion.OsSupportMappings.ToList ();
			selectTilePreOsMin.DisplayMember = "DisplayName";
			selectTilePreOsMin.ValueMember = "OSVersion";
			selectTilePreOsMax.DisplayMember = "DisplayName";
			selectTilePreOsMax.ValueMember = "OSVersion";
			SetMinCustomValues (GetCurrentOSVersion ());
			SetMaxCustomValues (GetCurrentOSVersion ());
			// 不可编辑
			selectTilePropType.DropDownStyle = ComboBoxStyle.DropDownList;
			selectTileVEOverflow.DropDownStyle = ComboBoxStyle.DropDownList;
			selectTileVEDfltTileSize.DropDownStyle = ComboBoxStyle.DropDownList;
			selectTileVEForegroundColor.DropDownStyle = ComboBoxStyle.DropDownList;
			
			// 架构选择
			selectTileIdArchi.DataSource = Enum.GetValues (typeof (ProcessorArchitecture))
				.Cast<ProcessorArchitecture> ()
				.Where (a => a != ProcessorArchitecture.Unknown)  // 排除 Unknown
				.ToList ();
			selectTileIdArchi.DropDownStyle = ComboBoxStyle.DropDownList;
			// 设置默认值（通常是 Neutral，表示中性的任意平台）
			selectTileIdArchi.SelectedItem = ProcessorArchitecture.Neutral;

			// ---------- 数值范围（设计器已设置大部分，这里统一确认） ----------
			// Version 四个 NumericUpDown 已在设计器设置 Maximum = 65535
			inputTileIdVersionMajor.Minimum = 0;
			inputTileIdVersionMinor.Minimum = 0;
			inputTileIdVersionBuild.Minimum = 0;
			inputTileIdVersionRevision.Minimum = 0;

			// Rail 高度相关已在设计器设 Minimum 20
			// Flyout 宽/高已在设计器设 Minimum 60/21

			// ---------- 弃用标记（灰色） ----------
			// Properties 页的 Type
			label12.ForeColor = Color.Gray;       // "Type" 标签
			selectTilePropType.ForeColor = Color.Gray;

			// Grid Style 整个标签页标题加 (deprecated)
			tabPage5.Text += " (deprecated)";
			// 将该页中所有控件的文字设为灰色
			SetDeprecatedStyle (tableLayoutPanel7);

			// ---------- 按钮事件 ----------
			buttonBrowser.Click += (s, e) => {
				using (var ofd = new OpenFileDialog {
					Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
					Title = "选择清单文件"
				})
				{
					if (ofd.ShowDialog () == DialogResult.OK)
						inputManifestXmlPath.Text = ofd.FileName;
				}
			};

			buttonLoad.Click += (s, e) => LoadManifest ();
			buttonSave.Click += (s, e) => SaveManifest ();
		}

		/// <summary>
		/// 递归将一个容器中所有控件的文本颜色设为灰色（弃用风格）
		/// </summary>
		private void SetDeprecatedStyle (Control container)
		{
			foreach (Control c in container.Controls)
			{
				if (c is Label || c is CheckBox || c is ComboBox || c is TextBox)
					c.ForeColor = Color.Gray;
				if (c is ColorPicker)   // 你的自定义控件可能也需要变灰
					c.ForeColor = Color.Gray;
				if (c.HasChildren)
					SetDeprecatedStyle (c);
			}
		}

		/// <summary>
		/// 加载 Manifest .xml 文件并填充界面
		/// </summary>
		private void LoadManifest ()
		{
			string path = inputManifestXmlPath.Text.Trim ();
			if (string.IsNullOrEmpty (path) || !File.Exists (path))
			{
				MessageBox.Show ("Please select a valid manifest file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				var manifest = TileManifest.FromFile (path);

				// Identity
				inputTileIdName.Text = manifest.Identity.Name;
				inputTileIdPublisher.Text = manifest.Identity.Publisher;
				Sidebar.Version ver = manifest.Identity.Version;
				inputTileIdVersionMajor.Value = ver.Major;
				inputTileIdVersionMinor.Value = ver.Minor;
				inputTileIdVersionBuild.Value = ver.Build < 0 ? 0 : ver.Build;
				inputTileIdVersionRevision.Value = ver.Revision < 0 ? 0 : ver.Revision;
				selectTileIdArchi.SelectedItem = manifest.Identity.ProcessorArchitecture;

				// Properties
				inputTilePropDispName.Text = manifest.Properties.DisplayName;
				inputTilePropPublisher.Text = manifest.Properties.PublisherDisplayName;
				inputTilePropDesc.Text = manifest.Properties.Description;
				inputTilePropLogo.Text = manifest.Properties.Logo;
				selectTilePropType.SelectedItem = manifest.Properties.Type;

				// Prerequisites
				if (manifest.Prerequisites != null)
				{
					// 处理 OSMinVersion
					var minVer = manifest.Prerequisites.OSMinVersion;
					int minIndex = OSVersion.OsSupportMappings.GetIndex (minVer);
					if (minIndex != -1)
					{
						selectTilePreOsMin.SelectedIndex = minIndex;
						// 禁用自定义版本控件（因为选中了预定义项）
						inputOsMinCVerMajor.Enabled = false;
						inputOsMinCVerMinor.Enabled = false;
						inputOsMinCVerBuild.Enabled = false;
						inputOsMinCVerRevision.Enabled = false;
					}
					else
					{
						// 未找到匹配项，选中 "Custom..." 项（索引为 OsSupportMappings.Length - 1）
						int customIndex = OSVersion.OsSupportMappings.Length - 1;
						selectTilePreOsMin.SelectedIndex = customIndex;
						// 填充自定义版本数值
						inputOsMinCVerMajor.Value = minVer.Major;
						inputOsMinCVerMinor.Value = minVer.Minor;
						inputOsMinCVerBuild.Value = minVer.Build < 0 ? 0 : minVer.Build;
						inputOsMinCVerRevision.Value = minVer.Revision < 0 ? 0 : minVer.Revision;
						// 启用自定义版本控件
						inputOsMinCVerMajor.Enabled = true;
						inputOsMinCVerMinor.Enabled = true;
						inputOsMinCVerBuild.Enabled = true;
						inputOsMinCVerRevision.Enabled = true;
					}

					// 处理 OSMaxVersion
					var maxVer = manifest.Prerequisites.OSMaxVersionTested;
					int maxIndex = OSVersion.OsSupportMappings.GetIndex (maxVer);
					if (maxIndex != -1)
					{
						selectTilePreOsMax.SelectedIndex = maxIndex;
						inputOsMaxCVerMajor.Enabled = false;
						inputOsMaxCVerMinor.Enabled = false;
						inputOsMaxCVerBuild.Enabled = false;
						inputOsMaxCVerRevision.Enabled = false;
					}
					else
					{
						int customIndex = OSVersion.OsSupportMappings.Length - 1;
						selectTilePreOsMax.SelectedIndex = customIndex;
						inputOsMaxCVerMajor.Value = maxVer.Major;
						inputOsMaxCVerMinor.Value = maxVer.Minor;
						inputOsMaxCVerBuild.Value = maxVer.Build < 0 ? 0 : maxVer.Build;
						inputOsMaxCVerRevision.Value = maxVer.Revision < 0 ? 0 : maxVer.Revision;
						inputOsMaxCVerMajor.Enabled = true;
						inputOsMaxCVerMinor.Enabled = true;
						inputOsMaxCVerBuild.Enabled = true;
						inputOsMaxCVerRevision.Enabled = true;
					}
				}
				else
				{
					// 如果 manifest 中没有 Prerequisites 节点，可以设置默认值或留空
					// 这里将两个 ComboBox 都设为 Custom，并清空自定义数值
					int customIndex = OSVersion.OsSupportMappings.Length - 1;
					selectTilePreOsMin.SelectedIndex = customIndex;
					selectTilePreOsMax.SelectedIndex = customIndex;
					inputOsMinCVerMajor.Value = 0;
					inputOsMinCVerMinor.Value = 0;
					inputOsMinCVerBuild.Value = 0;
					inputOsMinCVerRevision.Value = 0;
					inputOsMaxCVerMajor.Value = 0;
					inputOsMaxCVerMinor.Value = 0;
					inputOsMaxCVerBuild.Value = 0;
					inputOsMaxCVerRevision.Value = 0;
					inputOsMinCVerMajor.Enabled = true;
					inputOsMinCVerMinor.Enabled = true;
					inputOsMinCVerBuild.Enabled = true;
					inputOsMinCVerRevision.Enabled = true;
					inputOsMaxCVerMajor.Enabled = true;
					inputOsMaxCVerMinor.Enabled = true;
					inputOsMaxCVerBuild.Enabled = true;
					inputOsMaxCVerRevision.Enabled = true;
				}


				// Visual Elements - RailStyle
				var rail = manifest.VisualElements.RailStyle;
				inputTileVEMinHeight.Value = rail.MinHeight;
				inputTileVEMaxHeight.Value = rail.MaxHeight;
				inputTileVEDfltHeight.Value = rail.DefaultHeight;
				inputTileVECanPinBottom.Checked = rail.CanPinBottom;
				inputTileVEHasFlyout.Checked = rail.TileHasFlyout;
				inputTileVEFlyoutWidth.Value = rail.FlyoutWidth;
				inputTileVEFlyoutHeight.Value = rail.FlyoutHeight;
				inputTileVEFlyoutCanResize.Checked = rail.FlyoutCanResize;
				selectTileVEOverflow.SelectedItem = rail.Overflow;
				inputTileVERailDisplayName.Text = rail.DisplayName;
				inputTileVEHasProperties.Checked = rail.TileHasProperties;
				inputTileVERailLogo.Text = rail.Logo;

				// Visual Elements - GridStyle (弃用)
				var grid = manifest.VisualElements.GridStyle;
				if (grid != null)
				{
					inputTileVEBadgeLogo.Text = grid.Badge;
					selectTileVEDfltTileSize.SelectedItem = grid.DefaultTileSize;
					inputTileVEGridDispName.Text = grid.DisplayName;
					inputTileVESmallTileLogo.Text = grid.SmallTile;
					inputTileVEMediumTileLogo.Text = grid.MediumTile;
					inputTileVEWideTileLogo.Text = grid.WideTile;
					inputTileVELargeTileLogo.Text = grid.LargeTile;
					inputTileVEShowNameOnMediumTIle.Checked = grid.ShowNameOnMediumTile;
					inputTileVEShowNameOnWideLogo.Checked = grid.ShowNameOnWideTile;
					inputVEShowTileOnLargeTile.Checked = grid.ShowNameOnLargeTile;
					// 解析背景颜色，默认为 Transparent
					inputVEBackgroundColor.Value = ParseColor (grid.BackgroundColor);
					selectTileVEForegroundColor.SelectedItem = grid.ForegroundColor;
					inputTileVEGridTileCanInteractive.Checked = grid.EnableInteraction;
				}
				else
				{
					ClearGridStyleControls ();
				}

				currentFilePath = path;
				MessageBox.Show ("Load successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show ("Load Failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// 收集界面数据并保存为 Manifest .xml
		/// </summary>
		private void SaveManifest ()
		{
			try
			{
				// 收集 Identity
				string name = inputTileIdName.Text.Trim ();
				string publisher = inputTileIdPublisher.Text.Trim ();
				Sidebar.Version version = new Sidebar.Version (
					(ushort)inputTileIdVersionMajor.Value,
					(ushort)inputTileIdVersionMinor.Value,
					(ushort)inputTileIdVersionBuild.Value,
					(ushort)inputTileIdVersionRevision.Value);
				ProcessorArchitecture arch = (ProcessorArchitecture)selectTileIdArchi.SelectedItem;
				var ident = new TileIdentity (name, publisher, version, arch);

				// 收集 Properties
				var prop = new TileProperties (
					inputTilePropDispName.Text.Trim (),
					inputTilePropPublisher.Text.Trim (),
					inputTilePropDesc.Text.Trim (),
					inputTilePropLogo.Text.Trim (),
					(TileType)selectTilePropType.SelectedItem);

				// 收集 RailStyle
				var rail = new TileRailStyle (
					(int)inputTileVEMinHeight.Value,
					(int)inputTileVEMaxHeight.Value,
					(int)inputTileVEDfltHeight.Value,
					inputTileVECanPinBottom.Checked,
					inputTileVEHasFlyout.Checked,
					(int)inputTileVEFlyoutWidth.Value,
					(int)inputTileVEFlyoutHeight.Value,
					inputTileVEFlyoutCanResize.Checked,
					(TileOverflow)selectTileVEOverflow.SelectedItem,
					inputTileVERailDisplayName.Text.Trim (),
					inputTileVEHasProperties.Checked,
					inputTileVERailLogo.Text);

				// 收集 GridStyle
				var grid = new TileGridStyle (
					inputTileVEBadgeLogo.Text.Trim (),
					(TileSize)selectTileVEDfltTileSize.SelectedItem,
					inputTileVESmallTileLogo.Text.Trim (),
					inputTileVEMediumTileLogo.Text.Trim (),
					inputTileVEWideTileLogo.Text.Trim (),
					inputTileVELargeTileLogo.Text.Trim (),
					ColorToString (inputVEBackgroundColor.Value),
					(TileForegroundColor)selectTileVEForegroundColor.SelectedItem,
					inputTileVEShowNameOnMediumTIle.Checked,
					inputTileVEShowNameOnWideLogo.Checked,
					inputVEShowTileOnLargeTile.Checked,
					inputTileVEGridTileCanInteractive.Checked,
					inputTileVEGridDispName.Text.Trim ());

				var ve = new TileVisualElements (rail, grid);

				// 收集 Prerequisites
				var osMinVer = ((StartupOS)selectTilePreOsMin.SelectedItem).OSVersion;
				if (osMinVer.IsEmpty) osMinVer = new Sidebar.Version (
					(ushort)inputOsMinCVerMajor.Value,
					(ushort)inputOsMinCVerMinor.Value,
					(ushort)inputOsMinCVerBuild.Value,
					(ushort)inputOsMinCVerRevision.Value
				);
				var osMaxVer = ((StartupOS)selectTilePreOsMax.SelectedItem).OSVersion;
				if (osMaxVer.IsEmpty) osMaxVer = new Sidebar.Version (
					(ushort)inputOsMaxCVerMajor.Value,
					(ushort)inputOsMaxCVerMinor.Value,
					(ushort)inputOsMaxCVerBuild.Value,
					(ushort)inputOsMaxCVerRevision.Value
				);
				var pre = new TilePrerequisites (osMinVer, osMaxVer);

				var manifest = new TileManifest (ident, prop, pre, ve);

				// 确定保存路径
				string path = inputManifestXmlPath.Text.Trim ();
				if (string.IsNullOrEmpty (path))
				{
					using (var sfd = new SaveFileDialog {
						Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
						Title = "Save Manifest File:"
					})
					{
						if (sfd.ShowDialog () == DialogResult.OK)
							path = sfd.FileName;
						else
							return;
					}
				}

				manifest.ToFile (path);
				currentFilePath = path;
				inputManifestXmlPath.Text = path;
				MessageBox.Show ("Mainfest file has saved.", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show ("Saved Failed：" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// ---------- 辅助方法 ----------
		private Color ParseColor (string hexOrName)
		{
			if (string.IsNullOrWhiteSpace (hexOrName))
				return Color.Transparent;
			try
			{
				return ColorTranslator.FromHtml (hexOrName);
			}
			catch
			{
				return Color.Transparent;
			}
		}

		private string ColorToString (Color color)
		{
			if (color == Color.Transparent)
				return "Transparent";
			return ColorTranslator.ToHtml (color);
		}

		private void ClearGridStyleControls ()
		{
			inputTileVEBadgeLogo.Text = "";
			selectTileVEDfltTileSize.SelectedIndex = 0;
			inputTileVEGridDispName.Text = "";
			inputTileVESmallTileLogo.Text = "";
			inputTileVEMediumTileLogo.Text = "";
			inputTileVEWideTileLogo.Text = "";
			inputTileVELargeTileLogo.Text = "";
			inputTileVEShowNameOnMediumTIle.Checked = false;
			inputTileVEShowNameOnWideLogo.Checked = false;
			inputVEShowTileOnLargeTile.Checked = false;
			inputVEBackgroundColor.Value = Color.Transparent;
			selectTileVEForegroundColor.SelectedIndex = 0;
			inputTileVEGridTileCanInteractive.Checked = false;
		}
		private void MainForm_Load (object sender, EventArgs e)
		{
			InitializeForm ();
		}

		private void selectTilePreOsMin_SelectedIndexChanged (object sender, EventArgs e)
		{
			if (selectTilePreOsMin.SelectedItem is StartupOS)
			{
				var item = (StartupOS)selectTilePreOsMin.SelectedItem;
				var iscustom = item.OSVersion.IsEmpty;
				inputOsMinCVerMajor.Enabled = iscustom;
				inputOsMinCVerMinor.Enabled = iscustom;
				inputOsMinCVerBuild.Enabled = iscustom;
				inputOsMinCVerRevision.Enabled = iscustom;
			}
		}

		private void selectTilePreOsMax_SelectedIndexChanged (object sender, EventArgs e)
		{
			if (selectTilePreOsMax.SelectedItem is StartupOS)
			{
				var item = (StartupOS)selectTilePreOsMax.SelectedItem;
				var iscustom = item.OSVersion.IsEmpty;
				inputOsMaxCVerMajor.Enabled = iscustom;
				inputOsMaxCVerMinor.Enabled = iscustom;
				inputOsMaxCVerBuild.Enabled = iscustom;
				inputOsMaxCVerRevision.Enabled = iscustom;
			}
		}
	}
}
