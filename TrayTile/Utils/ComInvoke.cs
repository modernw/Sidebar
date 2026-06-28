using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace WindowsModern.TrayTile.Utils
{
	[ComImport]
	[Guid ("e22ad333-b25f-460c-83d0-0581107395c9")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IUIAutomationLegacyIAccessiblePattern
	{
		[PreserveSig]
		int get_CurrentName (out string pszName);
		[PreserveSig]
		int get_CurrentValue (out string pszValue);
		[PreserveSig]
		int get_CurrentState (out int pState);
		[PreserveSig]
		int get_CurrentRole (out int pRole);
		[PreserveSig]
		int DoDefaultAction ();
		[PreserveSig]
		int Select (int flagsSelect);
		[PreserveSig]
		int GetIAccessible ([MarshalAs (UnmanagedType.Interface)] out object ppAccessible);
	}
	[ComImport]
	[Guid ("618736e0-3c3d-11cf-810c-00aa00389b71")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAccessible
	{
	}
	public static class LegacyAccess
	{
		public static readonly int LegacyIAccessiblePatternId = 10019;
		public static IUIAutomationLegacyIAccessiblePattern GetLegacyPattern (AutomationElement element)
		{
			if (element == null) return null;
			AutomationPattern legacyPattern = AutomationPattern.LookupById (LegacyIAccessiblePatternId);
			if (legacyPattern == null) return null;
			object patternObj;
			if (element.TryGetCurrentPattern (legacyPattern, out patternObj))
			{
				return patternObj as IUIAutomationLegacyIAccessiblePattern;
			}
			return null;
		}
	}
}
