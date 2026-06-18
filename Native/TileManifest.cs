using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Sidebar
{
	[ComVisible (true)]
	[XmlRoot ("Identity")]
	public interface ITileIdentity
	{
		[XmlAttribute ("Name")]
		string Name { get; }
		[XmlAttribute ("Publisher")]
		string Publisher { get; }
		[XmlAttribute ("Version")]
		Version Version { get; }
		[XmlAttribute ("ProcessorArchitecture")]
		ProcessorArchitecture ProcessorArchitecture { get; }
		[XmlIgnore]
		string PublisherId { get; }
		[XmlIgnore]
		string FamilyName { get; }
		[XmlIgnore]
		string FullName { get; }
		[XmlIgnore]
		Guid Id { get; }
	}
	[ComVisible (true)]
	[XmlRoot ("Properties")]
	public interface ITileProperties
	{
		[XmlElement ("DisplayName")]
		string DisplayName { get; }
		[XmlElement ("PublisherDisplayName")]
		string PublisherDisplayName { get; }
		[XmlIgnore]
		string Publisher { get; }
		[XmlElement ("Description")]
		string Description { get; }
		[XmlElement ("Logo")]
		string Logo { get; }
		[XmlElement ("Type")]
		[Obsolete (DescRes.NeverUse, false)]
		TileType Type { get; }
	}
	[ComVisible (true)]
	[XmlRoot ("Prerequisites")]
	public interface ITilePrerequisites
	{
		[XmlElement ("OSMinVersion")]
		Version OSMinVersion { get; }
		[XmlElement ("OSMaxVersionTested")]
		Version OSMaxVersionTested { get; }
	}
	[ComVisible (true)]
	[XmlRoot ("RailStyle")]
	public interface ITileRailStyle
	{
		[XmlElement ("MinHeight")]
		int MinHeight { get; }
		[XmlElement ("MaxHeight")]
		int MaxHeight { get; }
		[XmlElement ("DefaultHeight")]
		int DefaultHeight { get; }
		[XmlElement ("CanPinBottom")]
		bool CanPinBottom { get; }
		[XmlElement ("TileHasFlyout")]
		bool TileHasFlyout { get; }
		[XmlElement ("FlyoutWidth")]
		int FlyoutWidth { get; }
		[XmlElement ("FlyoutHeight")]
		int FlyoutHeight { get; }
		[XmlElement ("FlyoutCanResize")]
		bool FlyoutCanResize { get; }
		[XmlElement ("Overflow")]
		TileOverflow Overflow { get; }
		[XmlElement ("DisplayName")]
		string DisplayName { get; }
		[XmlElement ("TileHasProperties")]
		bool TileHasProperties { get; }
		[XmlElement ("Logo")]
		string Logo { get; }
	}
	[ComVisible (true)]
	[XmlRoot ("GridStyle")]
	[Obsolete (DescRes.NeverUse, false)]
	public interface ITileGridStyle
	{
		[XmlElement ("Badge")]
		string Badge { get; }
		[XmlElement ("DefaultTileSize")]
		TileSize DefaultTileSize { get; }
		[XmlElement ("DisplayName")]
		string DisplayName { get; }
		[XmlElement ("SmallTile")]
		string SmallTile { get; }
		[XmlElement ("MediumTile")]
		string MediumTile { get; }
		[XmlElement ("WideTile")]
		string WideTile { get; }
		[XmlElement ("LargeTile")]
		string LargeTile { get; }
		[XmlElement ("BackgroundColor")]
		string BackgroundColor { get; }
		[XmlElement ("ForegroundColor")]
		TileForegroundColor ForegroundColor { get; }
		[XmlElement ("ShowNameOnMediumTile")]
		bool ShowNameOnMediumTile { get; }
		[XmlElement ("ShowNameOnWideTile")]
		bool ShowNameOnWideTile { get; }
		[XmlElement ("ShowNameOnLargeTile")]
		bool ShowNameOnLargeTile { get; }
		[XmlElement ("EnableInteraction")]
		bool EnableInteraction { get; }
	}
	[ComVisible (true)]
	[XmlRoot ("VisualElements")]
	public interface ITileVisualElements
	{
		[XmlElement ("RailStyle")]
		ITileRailStyle RailStyle { get; }
		[XmlElement ("GridStyle")]
		[Obsolete (DescRes.NeverUse, false)]
		ITileGridStyle GridStyle { get; }
	}
	[ComVisible (true)]
	[XmlRoot ("Manifest")]
	public interface ITileManifest
	{
		[XmlElement ("Identity")]
		ITileIdentity Identity { get; }
		[XmlElement ("Properties")]
		ITileProperties Properties { get; }
		[XmlElement ("Prerequisites")]
		ITilePrerequisites Prerequisites { get; }
		[XmlElement ("VisualElements")]
		ITileVisualElements VisualElements { get; }
	}
}
