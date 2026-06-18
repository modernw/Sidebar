using System.Runtime.InteropServices;
using System.Windows;

namespace Sidebar
{
	[ComVisible (true)]
	public interface ITheme
	{
		string FolderPath { get; }
		string ThemeMainFile { get; }
		ResourceDictionary ResourceDictionary { get; }
	}
}
