using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Sidebar
{
	public static class FontHelper
	{
		public static readonly string [] preferredFonts = new string []
			{
				"微软雅黑",
				"Microsoft YaHei",
				"Segoe UI",
				"Ebrima",
				"Nirmala",
				"Gadugi",
				"Segoe UI Emoji",
				"Segoe UI Symbol",
				"Meiryo",
				"Leelawadee",
				"Microsoft JhengHei",
				"Malgun Gothic",
				"Estrangelo Edessa",
				"Microsoft Himalaya",
				"Microsoft New Tai Lue",
				"Microsoft PhagsPa",
				"Microsoft Tai Le",
				"Microsoft Yi Baiti",
				"Mongolian Baiti",
				"MV Boli",
				"Myanmar Text",
				"Javanese Text",
				"Cambria Math"
			};
		/// <summary>
		/// 从指定的字体名称列表中查找第一个已安装的字体。
		/// </summary>
		/// <param name="fontNames">字体名称列表，按优先级排列</param>
		/// <param name="defaultFontSize">默认字体大小（如果未指定，使用系统默认字体大小）</param>
		/// <returns>可用的 Font 对象；如果都找不到，返回系统默认字体</returns>
		public static Font GetFirstAvailableFont (string [] fontNames, float? defaultFontSize = null)
		{
			var installedFonts = new System.Drawing.Text.InstalledFontCollection ();
			FontFamily [] families = installedFonts.Families;
			float targetSize = defaultFontSize ?? SystemFonts.DefaultFont.Size;
			foreach (string fontName in fontNames)
			{
				foreach (FontFamily family in families)
				{
					if (family.Name.Equals (fontName, StringComparison.InvariantCultureIgnoreCase))
					{
						return new Font (family, targetSize);
					}
				}
			}
			return SystemFonts.DefaultFont;
		}
	}
}
