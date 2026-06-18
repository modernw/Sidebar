using System;
using System.ComponentModel;
using System.Resources;

namespace Sidebar
{
	public static class DescRes
	{
		public const string NeverUse = "No longer used during design and development, but not treated as an error.";
	}
	public class LocalizedDescriptionAttribute: DescriptionAttribute
	{
		private readonly string _resourceKey;
		private readonly ResourceManager _resourceManager;
		public LocalizedDescriptionAttribute (string resourceKey, Type resourceType)
		{
			_resourceKey = resourceKey;
			_resourceManager = new ResourceManager (resourceType);
		}
		public override string Description
		{
			get
			{
				string localized = _resourceManager.GetString (_resourceKey);
				return string.IsNullOrEmpty (localized) ? _resourceKey : localized;
			}
		}
	}
}
