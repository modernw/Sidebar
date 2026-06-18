using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace ClockTile
{
	public static class ImageSourceManager
	{
		private static readonly Dictionary<string, BitmapImage> _cache =
			new Dictionary<string, BitmapImage> (StringComparer.OrdinalIgnoreCase);
		private static readonly object _lock = new object ();
		/// <summary>
		/// 获取指定路径的 BitmapImage 实例（缓存）
		/// </summary>
		/// <param name="filePath">文件路径，支持相对路径或 pack URI</param>
		/// <returns>共享的 BitmapImage 实例</returns>
		public static BitmapImage GetImage (string filePath)
		{
			if (string.IsNullOrEmpty (filePath))
				throw new ArgumentNullException (nameof (filePath));
			lock (_lock)
			{
				BitmapImage cached;
				if (_cache.TryGetValue (filePath, out cached)) return cached;
				BitmapImage bitmap = new BitmapImage ();
				bitmap.BeginInit ();
				bitmap.UriSource = new Uri (filePath, UriKind.RelativeOrAbsolute);
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.CreateOptions = BitmapCreateOptions.None; // 可根据需要修改
				bitmap.EndInit ();
				bitmap.Freeze (); // 冻结以便跨线程访问，提高性能
				_cache [filePath] = bitmap;
				return bitmap;
			}
		}
		public static BitmapImage GetImageNoCache (string filePath)
		{
			if (string.IsNullOrEmpty (filePath))
				throw new ArgumentNullException (nameof (filePath));
			lock (_lock)
			{
				BitmapImage cached;
				if (_cache.TryGetValue (filePath, out cached)) return cached;
				BitmapImage bitmap = new BitmapImage ();
				bitmap.BeginInit ();
				bitmap.UriSource = new Uri (filePath, UriKind.RelativeOrAbsolute);
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.CreateOptions = BitmapCreateOptions.None; // 可根据需要修改
				bitmap.EndInit ();
				bitmap.Freeze (); // 冻结以便跨线程访问，提高性能
				return bitmap;
			}
		}
		/// <summary>
		/// 清除所有缓存
		/// </summary>
		public static void ClearCache ()
		{
			lock (_lock)
			{
				_cache.Clear ();
			}
		}
	}
}
