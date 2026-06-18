using System;
using System.Collections.ObjectModel;

namespace ThemeEditor
{
	/// <summary>
	/// 表示命名空间节点（用于 TreeView）
	/// </summary>
	public class NamespaceNode
	{
		public string Namespace { get; set; }
		public ObservableCollection<ControlTypeNode> Controls { get; set; } = new ObservableCollection<ControlTypeNode> ();
	}

	/// <summary>
	/// 表示具体的控件类型节点
	/// </summary>
	public class ControlTypeNode
	{
		public string Name { get; set; }
		public Type ControlType { get; set; }
	}
}