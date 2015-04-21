using System;
using System.Runtime.InteropServices;

namespace WorldWind
{
	public sealed class PerformanceTimer
	{
		#region Instance Data

		public static long TicksPerSecond;
		#endregion

		#region Creation

		/// <summary>
		/// Static class
		/// </summary>
 		private PerformanceTimer() 
		{
		}

		/// <summary>
		/// Static constructor
		/// </summary>
		static PerformanceTimer()
		{
			// Read timer frequency
			long tickFrequency = 0;
			if (!QueryPerformanceFrequency(ref tickFrequency))
				throw new NotSupportedException("The machine doesn't appear to support high resolution timer.");
			TicksPerSecond = tickFrequency;

			System.Diagnostics.Debug.WriteLine("tickFrequency = " + tickFrequency);
		}
		#endregion

		#region High Resolution Timer functions

		[System.Security.SuppressUnmanagedCodeSecurity] 
		[DllImport("kernel32")]
		private static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

		[System.Security.SuppressUnmanagedCodeSecurity] 
		[DllImport("kernel32")]
		public static extern bool QueryPerformanceCounter(ref long PerformanceCount);

		#endregion
	}
}
