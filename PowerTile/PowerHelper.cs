using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WindowsModern.PowerTile
{
	public static class SystemPowerHelper
	{
		// =========================================================
		// 屏幕常亮
		// =========================================================

		[DllImport ("kernel32.dll")]
		private static extern EXECUTION_STATE
			SetThreadExecutionState (
				EXECUTION_STATE esFlags);

		[Flags]
		private enum EXECUTION_STATE: uint
		{
			ES_CONTINUOUS = 0x80000000,
			ES_DISPLAY_REQUIRED = 0x00000002,
			ES_SYSTEM_REQUIRED = 0x00000001,
		}

		public static void SetScreenKeepAwake (
			bool keep)
		{
			if (keep)
			{
				SetThreadExecutionState (
					EXECUTION_STATE.ES_CONTINUOUS |
					EXECUTION_STATE.ES_DISPLAY_REQUIRED |
					EXECUTION_STATE.ES_SYSTEM_REQUIRED);
			}
			else
			{
				SetThreadExecutionState (
					EXECUTION_STATE.ES_CONTINUOUS);
			}
		}

		public static bool IsScreenKeepAwakeSupported ()
		{
			return true;
		}

		// =========================================================
		// 亮度
		// =========================================================

		public static event Action<byte>
			BrightnessChanged;

		private static ManagementEventWatcher
			brightnessWatcher;

		private static byte lastBrightness;

		public static bool IsBrightnessControlSupported ()
		{
			try
			{
				using (ManagementClass mclass =
					new ManagementClass (
						"WmiMonitorBrightnessMethods"))
				{
					mclass.Scope =
						new ManagementScope (
							@"\\.\root\wmi");

					return
						mclass.GetInstances ().Count > 0;
				}
			}
			catch
			{
				return false;
			}
		}

		public static byte GetCurrentBrightness ()
		{
			try
			{
				using (ManagementObjectSearcher searcher =
					new ManagementObjectSearcher (
						"root\\WMI",
						"SELECT * FROM WmiMonitorBrightness"))
				{
					foreach (ManagementObject obj
						in searcher.Get ())
					{
						return
							(byte)obj ["CurrentBrightness"];
					}
				}
			}
			catch
			{
			}

			return 0;
		}

		public static void SetBrightness (
			byte brightnessLevel)
		{
			try
			{
				using (ManagementClass mclass =
					new ManagementClass (
						"WmiMonitorBrightnessMethods"))
				{
					mclass.Scope =
						new ManagementScope (
							@"\\.\root\wmi");

					foreach (ManagementObject instance
						in mclass.GetInstances ())
					{
						object [] args =
						{
							UInt32.MaxValue,
							brightnessLevel
						};

						instance.InvokeMethod (
							"WmiSetBrightness",
							args);
					}
				}
			}
			catch
			{
			}
		}

		public static void StartBrightnessWatcher ()
		{
			if (brightnessWatcher != null)
				return;

			try
			{
				WqlEventQuery query =
					new WqlEventQuery (
						"SELECT * FROM WmiMonitorBrightnessEvent");

				brightnessWatcher =
					new ManagementEventWatcher (query);

				brightnessWatcher.EventArrived +=
					delegate (object sender,
						EventArrivedEventArgs e)
					{
						try
						{
							byte value =
								(byte)e.NewEvent
									.Properties ["Brightness"]
									.Value;

							if (value != lastBrightness)
							{
								lastBrightness = value;

								RaiseBrightnessChanged (
									value);
							}
						}
						catch
						{
						}
					};

				brightnessWatcher.Start ();

				lastBrightness =
					GetCurrentBrightness ();
			}
			catch
			{
			}
		}

		public static void StopBrightnessWatcher ()
		{
			if (brightnessWatcher != null)
			{
				try
				{
					brightnessWatcher.Stop ();
				}
				catch
				{
				}

				brightnessWatcher.Dispose ();
				brightnessWatcher = null;
			}
		}

		// =========================================================
		// 电源模式
		// =========================================================

		public enum PowerMode
		{
			PowerSaver,
			Balanced,
			HighPerformance,
			Custom
		}

		public static event Action<PowerMode>
			PerformanceModeChanged;

		private static ManagementEventWatcher
			powerSchemeWatcher;

		private static Timer
			pollingTimer;

		private static PowerMode
			lastPerformanceMode;

		// =========================================================
		// Legacy Scheme GUID
		// =========================================================

		private static readonly Guid
			GUID_BALANCED =
				new Guid (
					"381b4222-f694-41f0-9685-ff5bb260df2e");

		private static readonly Guid
			GUID_HIGH_PERFORMANCE =
				new Guid (
					"8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

		private static readonly Guid
			GUID_POWER_SAVER =
				new Guid (
					"a1841308-3541-4fab-bc81-f71556f20b4a");

		private static readonly Guid
			GUID_ULTIMATE_PERFORMANCE =
				new Guid (
					"e9a42b02-d5df-448d-aa00-03f14749eb61");

		// =========================================================
		// Win10/11 Overlay GUID
		// =========================================================

		private static readonly Guid
			GUID_OVERLAY_BEST_POWER_EFFICIENCY =
				new Guid (
					"961cc777-2547-4f9d-8174-7d86181b8a7a");

		private static readonly Guid
			GUID_OVERLAY_BALANCED =
				Guid.Empty;

		private static readonly Guid
			GUID_OVERLAY_BEST_PERFORMANCE =
				new Guid (
					"ded574b5-45a0-4f42-8737-46345c09c238");

		// =========================================================
		// Public API
		// =========================================================

		public static bool IsPerformanceModeSupported ()
		{
			return
				Environment.OSVersion.Version.Major >= 5;
		}

		public static void SetPerformanceMode (
			PowerMode mode)
		{
			if (mode == PowerMode.Custom)
				return;

			if (Environment.OSVersion.Version.Major < 6)
			{
				SetPerformanceModeXP (mode);
				return;
			}

			if (IsModernOverlaySupported ())
			{
				try
				{
					SetPerformanceModeOverlay (mode);
					return;
				}
				catch
				{
				}
			}

			SetPerformanceModeLegacy (mode);
		}

		public static PowerMode
			GetCurrentPerformanceMode ()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				return GetCurrentPerformanceModeXP ();
			}

			if (IsModernOverlaySupported ())
			{
				try
				{
					return
						GetCurrentPerformanceModeOverlay ();
				}
				catch
				{
				}
			}

			return
				GetCurrentPerformanceModeLegacy ();
		}

		// =========================================================
		// 获取当前友好名称
		// =========================================================

		public static string
			GetCurrentPowerModeFriendlyName ()
		{
			try
			{
				if (Environment.OSVersion.Version.Major < 6)
				{
					uint index = 0;

					if (!GetActivePwrScheme (
						ref index))
					{
						return "Unknown";
					}

					switch (index)
					{
						case 0:
							return "Home/Office Desk";

						case 1:
							return "Portable/Laptop";

						case 2:
							return "Presentation";

						case 3:
							return "Always On";

						case 4:
							return "Minimal Power Management";

						case 5:
							return "Max Battery";

						default:
							return "Custom";
					}
				}

				IntPtr ptr = IntPtr.Zero;

				try
				{
					uint result =
						PowerGetActiveScheme (
							IntPtr.Zero,
							out ptr);

					if (result != 0 ||
						ptr == IntPtr.Zero)
					{
						return "Unknown";
					}

					Guid schemeGuid =
						(Guid)Marshal.PtrToStructure (
							ptr,
							typeof (Guid));

					return
						ReadFriendlyName (
							schemeGuid);
				}
				finally
				{
					if (ptr != IntPtr.Zero)
					{
						LocalFree (ptr);
					}
				}
			}
			catch
			{
				return "Unknown";
			}
		}

		// =========================================================
		// FriendlyName
		// =========================================================

		private static string ReadFriendlyName (
			Guid schemeGuid)
		{
			IntPtr pScheme =
				IntPtr.Zero;

			try
			{
				pScheme =
					Marshal.AllocHGlobal (
						Marshal.SizeOf (
							typeof (Guid)));

				Marshal.StructureToPtr (
					schemeGuid,
					pScheme,
					false);

				uint size = 0;

				uint result =
					PowerReadFriendlyName (
						IntPtr.Zero,
						pScheme,
						IntPtr.Zero,
						IntPtr.Zero,
						null,
						ref size);

				if (result != 234 &&
					result != 0)
				{
					return schemeGuid.ToString ();
				}

				byte [] buffer =
					new byte [size];

				result =
					PowerReadFriendlyName (
						IntPtr.Zero,
						pScheme,
						IntPtr.Zero,
						IntPtr.Zero,
						buffer,
						ref size);

				if (result != 0)
				{
					return schemeGuid.ToString ();
				}

				return
					Encoding.Unicode
						.GetString (buffer)
						.TrimEnd ('\0');
			}
			catch
			{
				return schemeGuid.ToString ();
			}
			finally
			{
				if (pScheme != IntPtr.Zero)
				{
					Marshal.FreeHGlobal (
						pScheme);
				}
			}
		}

		// =========================================================
		// Watcher
		// =========================================================

		public static void StartPerformanceModeWatcher (
			int intervalMs = 1000)
		{
			if (powerSchemeWatcher != null ||
				pollingTimer != null)
			{
				return;
			}

			lastPerformanceMode =
				GetCurrentPerformanceMode ();

			if (IsModernOverlaySupported ())
			{
				StartPollingWatcher (
					intervalMs);

				return;
			}

			if (Environment.OSVersion.Version.Major >= 6)
			{
				try
				{
					WqlEventQuery query =
						new WqlEventQuery (
							"SELECT * FROM PowerManagementEvent");

					powerSchemeWatcher =
						new ManagementEventWatcher (
							query);

					powerSchemeWatcher.EventArrived +=
						OnPowerSchemeEvent;

					powerSchemeWatcher.Start ();

					return;
				}
				catch
				{
				}
			}

			StartPollingWatcher (
				intervalMs);
		}

		private static void StartPollingWatcher (
			int intervalMs)
		{
			pollingTimer =
				new Timer (
					delegate (object state)
					{
						try
						{
							PowerMode current =
								GetCurrentPerformanceMode ();

							if (current !=
								lastPerformanceMode)
							{
								lastPerformanceMode =
									current;

								RaisePerformanceModeChanged (
									current);
							}
						}
						catch
						{
						}
					},
					null,
					0,
					intervalMs);
		}

		public static void StopPerformanceModeWatcher ()
		{
			if (powerSchemeWatcher != null)
			{
				try
				{
					powerSchemeWatcher.Stop ();
				}
				catch
				{
				}

				powerSchemeWatcher.Dispose ();
				powerSchemeWatcher = null;
			}

			if (pollingTimer != null)
			{
				pollingTimer.Dispose ();
				pollingTimer = null;
			}
		}

		// =========================================================
		// UI线程安全
		// =========================================================

		private static void RunOnUIThread (
			Action action)
		{
			if (action == null)
				return;

			try
			{
				if (Application.Current != null)
				{
					Dispatcher dispatcher =
						Application.Current.Dispatcher;

					if (dispatcher != null)
					{
						if (dispatcher.CheckAccess ())
						{
							action ();
						}
						else
						{
							dispatcher.BeginInvoke (
								action,
								DispatcherPriority.Normal);
						}

						return;
					}
				}
			}
			catch
			{
			}

			action ();
		}

		private static void RaiseBrightnessChanged (
			byte value)
		{
			Action<byte> handler =
				BrightnessChanged;

			if (handler == null)
				return;

			RunOnUIThread (delegate
			{
				handler (value);
			});
		}

		private static void RaisePerformanceModeChanged (
			PowerMode mode)
		{
			Action<PowerMode> handler =
				PerformanceModeChanged;

			if (handler == null)
				return;

			RunOnUIThread (delegate
			{
				handler (mode);
			});
		}

		// =========================================================
		// Overlay Support
		// =========================================================

		private static bool IsModernOverlaySupported ()
		{
			Version ver =
				Environment.OSVersion.Version;

			if (!(ver.Major >= 10 &&
				ver.Build >= 16299))
			{
				return false;
			}

			try
			{
				SYSTEM_POWER_CAPABILITIES caps;

				if (!GetPwrCapabilities (
					out caps))
				{
					return false;
				}

				return
					caps.SystemBatteriesPresent;
			}
			catch
			{
				return false;
			}
		}

		// =========================================================
		// WMI Event
		// =========================================================

		private static void OnPowerSchemeEvent (
			object sender,
			EventArrivedEventArgs e)
		{
			try
			{
				uint eventType =
					(uint)e.NewEvent ["EventType"];

				if (eventType == 10)
				{
					PowerMode current =
						GetCurrentPerformanceMode ();

					if (current !=
						lastPerformanceMode)
					{
						lastPerformanceMode =
							current;

						RaisePerformanceModeChanged (
							current);
					}
				}
			}
			catch
			{
			}
		}

		// =========================================================
		// Win10/11 Overlay
		// =========================================================

		private static void SetPerformanceModeOverlay (
			PowerMode mode)
		{
			Guid overlayGuid;

			switch (mode)
			{
				case PowerMode.PowerSaver:

					overlayGuid =
						GUID_OVERLAY_BEST_POWER_EFFICIENCY;

					break;

				case PowerMode.HighPerformance:

					overlayGuid =
						GUID_OVERLAY_BEST_PERFORMANCE;

					break;

				default:

					overlayGuid =
						GUID_OVERLAY_BALANCED;

					break;
			}

			uint result =
				PowerSetActiveOverlayScheme (
					overlayGuid);

			if (result != 0)
			{
				throw new InvalidOperationException (
					"PowerSetActiveOverlayScheme failed: " +
					result);
			}
		}

		private static PowerMode
			GetCurrentPerformanceModeOverlay ()
		{
			try
			{
				Guid overlayGuid;

				uint result =
					PowerGetEffectiveOverlayScheme (
						out overlayGuid);

				if (result != 0)
				{
					return PowerMode.Custom;
				}

				if (overlayGuid ==
					GUID_OVERLAY_BEST_POWER_EFFICIENCY)
				{
					return PowerMode.PowerSaver;
				}

				if (overlayGuid ==
					GUID_OVERLAY_BEST_PERFORMANCE)
				{
					return PowerMode.HighPerformance;
				}

				if (overlayGuid ==
					GUID_OVERLAY_BALANCED)
				{
					return PowerMode.Balanced;
				}

				return PowerMode.Custom;
			}
			catch
			{
				return PowerMode.Custom;
			}
		}

		// =========================================================
		// Legacy
		// =========================================================

		private static void SetPerformanceModeLegacy (
			PowerMode mode)
		{
			Guid targetGuid;

			switch (mode)
			{
				case PowerMode.PowerSaver:

					targetGuid =
						GUID_POWER_SAVER;

					break;

				case PowerMode.HighPerformance:

					targetGuid =
						GUID_HIGH_PERFORMANCE;

					break;

				default:

					targetGuid =
						GUID_BALANCED;

					break;
			}

			PowerSetActiveScheme (
				IntPtr.Zero,
				targetGuid);
		}

		private static PowerMode
			GetCurrentPerformanceModeLegacy ()
		{
			IntPtr ptr =
				IntPtr.Zero;

			try
			{
				uint result =
					PowerGetActiveScheme (
						IntPtr.Zero,
						out ptr);

				if (result != 0 ||
					ptr == IntPtr.Zero)
				{
					return PowerMode.Custom;
				}

				Guid activeGuid =
					(Guid)Marshal.PtrToStructure (
						ptr,
						typeof (Guid));

				if (activeGuid ==
					GUID_POWER_SAVER)
				{
					return PowerMode.PowerSaver;
				}

				if (activeGuid ==
					GUID_BALANCED)
				{
					return PowerMode.Balanced;
				}

				if (activeGuid ==
					GUID_HIGH_PERFORMANCE ||
					activeGuid ==
					GUID_ULTIMATE_PERFORMANCE)
				{
					return PowerMode.HighPerformance;
				}

				return PowerMode.Custom;
			}
			finally
			{
				if (ptr != IntPtr.Zero)
				{
					LocalFree (ptr);
				}
			}
		}

		// =========================================================
		// XP
		// =========================================================

		private static PowerMode
			GetCurrentPerformanceModeXP ()
		{
			uint index = 0;

			if (GetActivePwrScheme (
				ref index))
			{
				if (index == 5)
					return PowerMode.PowerSaver;

				if (index == 0 ||
					index == 3)
				{
					return PowerMode.HighPerformance;
				}

				if (index == 1)
				{
					return PowerMode.Balanced;
				}

				return PowerMode.Custom;
			}

			return PowerMode.Custom;
		}

		private static void SetPerformanceModeXP (
			PowerMode mode)
		{
			uint index;

			switch (mode)
			{
				case PowerMode.PowerSaver:

					index = 5;

					break;

				case PowerMode.HighPerformance:

					index = 0;

					break;

				default:

					index = 1;

					break;
			}

			SetActivePwrScheme (
				index,
				IntPtr.Zero,
				IntPtr.Zero);
		}

		// =========================================================
		// API
		// =========================================================

		[DllImport ("powrprof.dll")]
		private static extern bool
			GetPwrCapabilities (
				out SYSTEM_POWER_CAPABILITIES caps);

		[DllImport (
			"powrprof.dll",
			SetLastError = true)]
		private static extern uint
			PowerGetActiveScheme (
				IntPtr UserRootPowerKey,
				out IntPtr ActivePolicyGuid);

		[DllImport (
			"powrprof.dll",
			SetLastError = true)]
		private static extern uint
			PowerSetActiveScheme (
				IntPtr UserRootPowerKey,
				[MarshalAs(UnmanagedType.LPStruct)]
				Guid SchemeGuid);

		[DllImport (
			"powrprof.dll",
			EntryPoint =
				"PowerSetActiveOverlayScheme")]
		private static extern uint
			PowerSetActiveOverlayScheme (
				[MarshalAs(UnmanagedType.LPStruct)]
				Guid OverlaySchemeGuid);

		[DllImport (
			"powrprof.dll",
			EntryPoint =
				"PowerGetEffectiveOverlayScheme")]
		private static extern uint
			PowerGetEffectiveOverlayScheme (
				out Guid EffectiveOverlayGuid);

		[DllImport (
			"powrprof.dll",
			CharSet = CharSet.Unicode,
			SetLastError = true)]
		private static extern uint
			PowerReadFriendlyName (
				IntPtr RootPowerKey,
				IntPtr SchemeGuid,
				IntPtr SubGroupOfPowerSettingGuid,
				IntPtr PowerSettingGuid,
				byte [] Buffer,
				ref uint BufferSize);

		[DllImport (
			"powrprof.dll",
			SetLastError = true)]
		private static extern bool
			GetActivePwrScheme (
				ref uint lpdwActiveIndex);

		[DllImport (
			"powrprof.dll",
			SetLastError = true)]
		private static extern bool
			SetActivePwrScheme (
				uint dwIndex,
				IntPtr lpGlobalPowerPolicy,
				IntPtr lpPowerScheme);

		[DllImport (
			"kernel32.dll",
			SetLastError = true)]
		private static extern IntPtr
			LocalFree (
				IntPtr hMem);

		// =========================================================
		// Struct
		// =========================================================

		[StructLayout (LayoutKind.Sequential)]
		private struct SYSTEM_POWER_CAPABILITIES
		{
			[MarshalAs (UnmanagedType.U1)]
			public bool PowerButtonPresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool SleepButtonPresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool LidPresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool SystemS1;

			[MarshalAs (UnmanagedType.U1)]
			public bool SystemS2;

			[MarshalAs (UnmanagedType.U1)]
			public bool SystemS3;

			[MarshalAs (UnmanagedType.U1)]
			public bool SystemS4;

			[MarshalAs (UnmanagedType.U1)]
			public bool SystemS5;

			[MarshalAs (UnmanagedType.U1)]
			public bool HiberFilePresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool FullWake;

			[MarshalAs (UnmanagedType.U1)]
			public bool VideoDimPresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool ApmPresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool UpsPresent;

			[MarshalAs (UnmanagedType.U1)]
			public bool ThermalControl;

			[MarshalAs (UnmanagedType.U1)]
			public bool ProcessorThrottle;

			public byte ProcessorMinThrottle;
			public byte ProcessorMaxThrottle;

			[MarshalAs (UnmanagedType.U1)]
			public bool FastSystemS4;

			[MarshalAs (
				UnmanagedType.ByValArray,
				SizeConst = 3)]
			public byte [] spare2;

			[MarshalAs (UnmanagedType.U1)]
			public bool DiskSpinDown;

			[MarshalAs (
				UnmanagedType.ByValArray,
				SizeConst = 8)]
			public byte [] spare3;

			[MarshalAs (UnmanagedType.U1)]
			public bool SystemBatteriesPresent;
		}
	}
}