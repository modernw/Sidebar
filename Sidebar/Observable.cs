using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sidebar
{
	public class UniqueObservableCollection<T>: ObservableCollection<T>
	{
		private readonly Func<T, object> _keySelector;

		public UniqueObservableCollection (Func<T, object> keySelector)
		{
			_keySelector = keySelector;
		}
		protected override void InsertItem (int index, T item)
		{
			var key = _keySelector (item);
			for (int i = 0; i < Count; i++)
			{
				if (Equals (_keySelector (this [i]), key))
				{
					RemoveAt (i);
					if (i < index) index--;
					break;
				}
			}
			base.InsertItem (index, item);
		}
		protected override void SetItem (int index, T item)
		{
			var key = _keySelector (item);
			for (int i = 0; i < Count; i++)
			{
				if (i != index && Equals (_keySelector (this [i]), key))
				{
					RemoveAt (i);
					if (i < index) index--;
					break;
				}
			}
			base.SetItem (index, item);
		}
	}
}
