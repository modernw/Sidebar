using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sidebar
{
	[ComVisible (true)]
	public interface ILocaleResource: IDictionary<string, string>
	{
		string SuitableValue (string fallback = "", string localeName = null);
	}
	[ComVisible (true)]
	public interface ILocaleResources: IDictionary<string, ILocaleResource>
	{
		IDictionary<string, ILocaleResource> AllResources { get; }
		ILocaleResource AllValues (string resourceName);
		string SuitableResource (string resName, string fallback = "", string localeName = null);
	}
	[ComVisible (true)]
	public interface IPathResource: IDictionary<int, string>
	{
		string SuitableValue (string fallback = "", int dpiScale = -1);
	}
	[ComVisible (true)]
	public interface IPathResources: IDictionary<string, IPathResource>
	{
		IDictionary<string, IPathResource> AllResources { get; }
		IPathResource AllValues (string resourceName);
		string SuitableResource (string resName, string fallback = "", int scale = -1);
	}
}
