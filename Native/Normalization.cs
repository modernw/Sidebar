using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sidebar
{
	[ComVisible (true)]
	public static class StringNormalize
	{
		public static bool NEmpty (this string str)
		{
			return (str ?? "").Trim ().Length <= 0;
		}
		public static string NNormalize (this string str, bool upper = false)
		{
			if (upper) return (str ?? "").Trim ()?.ToUpperInvariant ();
			else return (str ?? "").Trim ()?.ToLowerInvariant ();
		}
		public static bool NEquals (this string self, string another)
		{
			return self?.NNormalize () == another?.NNormalize ();
		}
		public static int NCompareTo (this string self, string another)
		{
			return (self ?? "").NNormalize ().CompareTo (another?.NNormalize () ?? "");
		}
	}
}
