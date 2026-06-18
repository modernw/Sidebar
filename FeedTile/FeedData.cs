using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sidebar;
namespace WindowsModern.FeedTile
{
	public class FeedData
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Link { get; set; }
		public string Url { get; set; }
		public int UnreadCount { get; set; }
		public string Path { get; set; } = "";
		[JsonIgnore]
		public List<FeedItemData> Items { get; } = new List<FeedItemData> ();
		public override bool Equals (object obj)
		{
			if (obj is FeedData) return Path.NEquals ((obj as FeedData).Path);
			return base.Equals (obj);
		}
		public override int GetHashCode ()
		{
			return Path.GetHashCode ();
		}
	}
	public class FeedItemData
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Link { get; set; }
		public string LocalId { get; set; }
		public bool IsRead { get; set; }
		public DateTime PublishDate { get; set; } = DateTime.MinValue;
		public FeedData Parent { get; set; }
	}
}
