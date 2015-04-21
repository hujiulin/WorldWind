using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Utility
{
	/// <summary>
	/// BindingsCheck contains utility functions to determine whether the user is
	/// likely to encounter the "50 bindings problem" - a limitation in the 
	/// pre-SP1 .NET runtime 1.1 that makes downloads impossible if more than
	/// 50 protocol bindings exist. 
	/// The only public routine here is "FiftyBindingsWarning" - Checks if the 
	/// user has more than 50 protocol bindings and a CLR version that has the corresponding
	/// problem. If yes, a message is displayed to the user and (s)he is asked whether the .NET runtime
	/// service pack 1 download page should be opened.
	/// </summary>
	public sealed class BindingsCheck
	{
		/// <summary>
		/// Constructor, only static methods in this class
		/// </summary>
		private BindingsCheck()
		{
		}

		[DllImport("Ws2_32.dll")]
		static extern int WSCEnumProtocols(IntPtr lpiProtocols, IntPtr lpProtocolBuffer, ref long lpdwBufferLength, ref int lpErrno);

		// WSAENOBUFS indicates that there was not enough buffer space to store protocol info structures
		private const int WSAENOBUFS = 10055;

		// Find out how many protocol bindings exist on this machine
		// As there is no simple way to get the number of protocol bindings, what
		// we're doing here is determine the size of a single protocol info structure,
		// determine the size of all of them, and divide.
		// Returns the number of bindings or -1 if unable to do so.
		static int DetermineProtocolBindings() 
		{
			int errorNumber = 0;
			const long sizeOfOneProtocolInfoStructure = 628;

			long sizeOfAllProtocolInfoStructures = 0;
			// request all protocol info structures, but provide no space to store them -
			// this will yield the total size of the structure array in bytes
			WSCEnumProtocols(IntPtr.Zero, IntPtr.Zero, ref sizeOfAllProtocolInfoStructures, ref errorNumber);
			if(errorNumber != WSAENOBUFS) return -1; // not what we expected, unable to determine number of bindings

			// divide total size by size of one structure to get number of bindings.
			return (int)(sizeOfAllProtocolInfoStructures / sizeOfOneProtocolInfoStructure);
		}

		// Determines whether the CLR version is newer than the first one that included the fix.
		static bool IsBindingsHotFixApplied() 
		{
			// check if CLR version is 1.1.4322.946 or higher
			return System.Environment.Version >= new Version("1.1.4322.946");
		}
      
		/// <summary>
		/// Checks if the user has more than 50 protocol bindings and a CLR version that has the corresponding
		/// problem. If yes, a message is displayed to the user and (s)he is asked whether the .NET runtime
		/// service pack 1 download page should be opened.
		/// </summary>
		/// <returns>true if the user opted to go to the SP1 download page - this means that World Wind should
		/// NOT continue launching</returns>
		public static bool FiftyBindingsWarning() 
		{
			if(DetermineProtocolBindings() <= 50 || IsBindingsHotFixApplied()) return false;
         
			DialogResult userChoice = 
				MessageBox.Show(
				"Your computer has a large number of protocols (over 50) installed,\n" +
				"which is not supported by the currently installed .NET version.\n\n" +
				"As a result, World Wind may not be able to download imagery.\n\n" +
				"Service pack 1 for the .NET framework 1.1 fixes this problem -\n" +
				"do you want to go to the download page now?\n\n" + 
				"(Click No to continue launching World Wind)", 
				"NASA World Wind: Fifty protocol bindings problem detected", 
				MessageBoxButtons.YesNo
				);

			if(userChoice == DialogResult.Yes) 
			{
				System.Diagnostics.Process.Start("iexplore", "http://msdn.microsoft.com/netframework/downloads/updates/default.aspx");
				return true;
			}
			return false;
            
		}
	}
}
