using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ClockTile.Faces
{
	public enum ClockTileFaceType
	{
		DateTimeClock,
		DayTimeClock,
		Time,
		DateTime,
		TimeClock,
		RowDateTimeClockSmall,
		RowDateTimeClockLarge,
		RowDateTimeClockFull
	}
	public class ClockFacePanel: IDisposable
	{
		public FrameworkElement Element { get; set; }
		public IUnionTimeSetter Setter { get; set; }
		public void Dispose ()
		{
			Element = null;
			Setter = null;
		}
	}
	public static class ClockFaceManager
	{
		public static ClockFacePanel GetClockFace (this ClockTileFaceType type)
		{
			switch (type)
			{
				case ClockTileFaceType.DateTimeClock:
					{
						var panel1 = new PanelFace1 ();
						return new ClockFacePanel {
							Element = panel1,
							Setter = panel1
						};
					} break;
				case ClockTileFaceType.DayTimeClock:
					{
						var panel2 = new PanelFace2 ();
						return new ClockFacePanel {
							Element = panel2,
							Setter = panel2
						};
					} break;
				case ClockTileFaceType.Time:
					{
						var panel3 = new PanelFace3 ();
						return new ClockFacePanel {
							Element = panel3,
							Setter = panel3
						};
					} break;
				case ClockTileFaceType.DateTime:
					{
						var panel4 = new PanelFace4 ();
						return new ClockFacePanel {
							Element = panel4,
							Setter = panel4
						};
					} break;
				case ClockTileFaceType.TimeClock:
					{
						var panel5 = new PanelFace5 ();
						return new ClockFacePanel {
							Element = panel5,
							Setter = panel5
						};
					} break;
				case ClockTileFaceType.RowDateTimeClockSmall:
					{
						var panel6 = new PanelFace6 ();
						return new ClockFacePanel {
							Element = panel6,
							Setter = panel6
						};
					} break;
				case ClockTileFaceType.RowDateTimeClockLarge:
					{
						var panel7 = new PanelFace7 ();
						return new ClockFacePanel {
							Element = panel7,
							Setter = panel7
						};
					} break;
				case ClockTileFaceType.RowDateTimeClockFull:
					{
						var panel8 = new PanelFace8 ();
						return new ClockFacePanel {
							Element = panel8,
							Setter = panel8
						};
					} break;
			}
			return null;
		}
		public static ClockTileFaceType GetFacePanelType (this ClockFacePanel cfp)
		{
			if (cfp.Element is PanelFace1) return ClockTileFaceType.DateTimeClock;
			else if (cfp.Element is PanelFace2) return ClockTileFaceType.DayTimeClock;
			else if (cfp.Element is PanelFace3) return ClockTileFaceType.Time;
			else if (cfp.Element is PanelFace4) return ClockTileFaceType.DateTime;
			else if (cfp.Element is PanelFace5) return ClockTileFaceType.TimeClock;
			else if (cfp.Element is PanelFace6) return ClockTileFaceType.RowDateTimeClockSmall;
			else if (cfp.Element is PanelFace7) return ClockTileFaceType.RowDateTimeClockLarge;
			else if (cfp.Element is PanelFace8) return ClockTileFaceType.RowDateTimeClockFull;
			return ClockTileFaceType.DateTimeClock;
		}
	}
}
