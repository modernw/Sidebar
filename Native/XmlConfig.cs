using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Sidebar
{
	/// <summary>
	/// 表示一个配置节（例如全局配置或某个用户的配置），支持通过索引器读写任意类型的值。
	/// </summary>
	[ComVisible (true)]
	public class ConfigSection
	{
		private XElement _root;
		private readonly object _lock;
		private readonly Action _saveHandler;
		internal ConfigSection (XElement root, object lockObj, Action saveHandler)
		{
			_root = root;
			_lock = lockObj;
			_saveHandler = saveHandler;
		}
		/// <summary>
		/// 索引器，获取或设置配置项的值。
		/// </summary>
		public object this [string key]
		{
			get
			{
				if (_root == null) return null;
				var elem = GetSettingElement (key);
				if (elem == null) return null;
				var typeAttr = elem.Attribute ("type");
				if (typeAttr == null || string.IsNullOrEmpty (typeAttr.Value)) return null;
				var type = Type.GetType (typeAttr.Value);
				if (type == null) return null;
				var json = elem.Value;
				if (string.IsNullOrEmpty (json)) return null;
				return JsonConvert.DeserializeObject (json, type);
			}
			set
			{
				if (_root == null) throw new InvalidOperationException ("Configuration section is not available.");
				var elem = GetSettingElement (key);
				var typeName = value?.GetType ().AssemblyQualifiedName ?? typeof (object).AssemblyQualifiedName;
				var json = value == null ? "null" : JsonConvert.SerializeObject (value);
				lock (_lock)
				{
					if (elem == null)
					{
						elem = new XElement ("Setting",
							new XAttribute ("name", key),
							new XAttribute ("type", typeName),
							json);
						_root.Add (elem);
					}
					else
					{
						elem.Attribute ("type").Value = typeName;
						elem.Value = json;
					}
					_saveHandler?.Invoke ();
				}
			}
		}
		/// <summary>
		/// 获取配置项，并转换为指定类型。
		/// </summary>
		public T Get<T> (string key, T defaultValue = default (T))
		{
			var val = this [key];
			if (val == null) return defaultValue;
			try { return (T)val; } catch { return defaultValue; }
		}
		/// <summary>
		/// 设置配置项。
		/// </summary>
		public void Set<T> (string key, T value) => this [key] = value;
		/// <summary>
		/// 判断配置项是否存在。
		/// </summary>
		public bool ContainsKey (string key) => GetSettingElement (key) != null;
		/// <summary>
		/// 删除配置项。
		/// </summary>
		public bool Remove (string key)
		{
			var elem = GetSettingElement (key);
			if (elem == null) return false;
			lock (_lock)
			{
				elem.Remove ();
				_saveHandler?.Invoke ();
			}
			return true;
		}
		private XElement GetSettingElement (string key)
		{
			return _root?.Elements ("Setting").FirstOrDefault (e => {
				var nameAttr = e.Attribute ("name");
				return nameAttr != null && nameAttr.Value == key;
			});
		}
		internal void SetRoot (XElement newRoot)
		{
			_root = newRoot;
		}
	}
	/// <summary>
	/// 管理整个配置文件，提供全局和用户配置节。
	/// </summary>
	[ComVisible (true)]
	public class XmlConfig
	{
		private static readonly object _staticLock = new object ();
		private static XmlConfig _defaultInstance;
		private static string _currentUserName;
		private static ConfigSection _globalSection;
		private static ConfigSection _userSection;
		private readonly object _instanceLock = new object ();
		private readonly string _filePath;
		private XDocument _doc;
		public ConfigSection Global { get; private set; }
		public ConfigSection CurrentUser { get; private set; }
		public static ConfigSection GlobalSettings
		{
			get
			{
				if (_defaultInstance == null)
					throw new InvalidOperationException ("XmlConfig not initialized. Call Initialize first.");
				return _defaultInstance.Global;
			}
		}
		public static ConfigSection CurrentUserSettings
		{
			get
			{
				if (_defaultInstance == null)
					throw new InvalidOperationException ("XmlConfig not initialized. Call Initialize first.");
				return _defaultInstance.CurrentUser;
			}
		}
		public static string CurrentUserName
		{
			get { return _currentUserName; }
			set
			{
				if (_currentUserName == value) return;
				_currentUserName = value;
				if (_defaultInstance != null)
					_defaultInstance.SwitchUser (value);
			}
		}
		/// <summary>
		/// 初始化默认配置文件（静态使用）。传入目录或完整文件路径。
		/// </summary>
		public static void Initialize (string path)
		{
			lock (_staticLock)
			{
				_defaultInstance = new XmlConfig (path);
				_currentUserName = Environment.UserName; // 默认当前 Windows 用户名
				_defaultInstance.SwitchUser (_currentUserName);
			}
		}
		/// <summary>
		/// 构造函数，创建一个新的配置文件实例。
		/// </summary>
		/// <param name="path">文件路径，或者目录（会自动加上 Config.xml）</param>
		public XmlConfig (string path)
		{
			// 如果传入的是目录，则拼接 Config.xml
			if (Directory.Exists (path) || (!File.Exists (path) && !Path.HasExtension (path)))
			{
				_filePath = Path.Combine (path, "Config.xml");
			}
			else
			{
				_filePath = path;
			}

			LoadOrCreate ();
			Global = new ConfigSection (_doc.Root.Element ("Global"), _instanceLock, Save);
			CurrentUser = new ConfigSection (null, _instanceLock, Save);
		}
		private void LoadOrCreate ()
		{
			var dir = Path.GetDirectoryName (_filePath);
			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			if (File.Exists (_filePath))
			{
				_doc = XDocument.Load (_filePath);
			}
			else
			{
				_doc = new XDocument (
					new XElement ("Config",
						new XElement ("Global"),
						new XElement ("Users")
					)
				);
				Save ();
			}
		}
		private void Save ()
		{
			lock (_instanceLock)
			{
				_doc.Save (_filePath);
			}
		}
		private void SwitchUser (string userName)
		{
			lock (_instanceLock)
			{
				var usersRoot = _doc.Root.Element ("Users");
				if (usersRoot == null)
				{
					usersRoot = new XElement ("Users");
					_doc.Root.Add (usersRoot);
					Save ();
				}

				XElement userNode = null;
				if (!string.IsNullOrEmpty (userName))
				{
					userNode = usersRoot.Elements ("User")
						.FirstOrDefault (e => e.Attribute ("name")?.Value == userName);
					if (userNode == null)
					{
						userNode = new XElement ("User", new XAttribute ("name", userName));
						usersRoot.Add (userNode);
						Save ();
					}
				}
				CurrentUser.SetRoot (userNode);
			}
		}
	}
}