using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClockTile.Faces
{
	public interface IUnionTimeSetter
	{
		DateTime CurrentTime { set; }
		string CurrentTimeZone { set; }
	}
}
