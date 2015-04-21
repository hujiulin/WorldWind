using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace WorldWind
{
	/// <summary>
	///  Interop functionality for WorldWind namespace
	/// </summary>
	sealed internal class NativeMethods
	{
		private NativeMethods()
		{}

		public const int WM_COPYDATA = 0x004A;
		public const int WM_ACTIVATEAPP = 0x001C;

		[DllImport("user32.dll")]
		internal static extern bool SendMessage(
			IntPtr hWnd,
			int Msg,
			IntPtr wParam,
			ref CopyDataStruct lParam);

		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern IntPtr LocalAlloc(int flag, int size);

		[DllImport("kernel32.dll", SetLastError=true)]
		internal static extern IntPtr LocalFree(IntPtr p);

		/// <summary>
		/// API function to find window based on WindowName and class.
		/// </summary>
		[DllImport("user32.dll")]
		internal static extern IntPtr FindWindow (string lpClassName, string lpWindowName);

		/// <summary>
		/// Sends string arguments to running instance of World Wind.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool SendArgs(IntPtr targetHWnd, string args)
		{
			if (targetHWnd == IntPtr.Zero)
				return false;

			CopyDataStruct cds = new CopyDataStruct();
			try
			{
				cds.cbData = (args.Length + 1) * 2;
				cds.lpData = NativeMethods.LocalAlloc(0x40, cds.cbData);
				Marshal.Copy(args.ToCharArray(), 0, cds.lpData, args.Length);
				cds.dwData =  (IntPtr) 1;

				return SendMessage( targetHWnd, WM_COPYDATA, /*Handle*/System.IntPtr.Zero, ref cds );
			}
			finally
			{
				cds.Dispose();
			}
		}

		internal struct CopyDataStruct : IDisposable
		{
			public IntPtr dwData;
			public int cbData;
			public IntPtr lpData;

			public void Dispose()
			{
				if (this.lpData!=IntPtr.Zero)
				{
					LocalFree(this.lpData);
					this.lpData = IntPtr.Zero;
				}
			}
		}
	}
}
