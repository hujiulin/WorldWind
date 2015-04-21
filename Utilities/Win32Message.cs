using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Utility
{
	/// <summary>
	/// Utility class to retrieve Win32 Error message text.
	/// This class also provides a mechanism to specify
	/// additional Win32 DLLs (e.g. winhttp.dll, netmsg.dll) and
	/// their associated error ranges - messages will automatically
	/// be pulled from these DLLs when the number is in the 
	/// corresponding range. 
	/// </summary>
	public class Win32Message
	{
		/// <summary>
		/// Constructor, static class (only static methods)
		/// </summary>
		private Win32Message()
		{
		}
		
		// Constant declarations
		const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
		const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
		const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
		const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;

		const int LOAD_LIBRARY_AS_DATAFILE = 0x02;

		// WINHTTP DLL Error range
		const int WINHTTP_ERROR_BASE = 12000;
		const int WINHTTP_ERROR_LAST = WINHTTP_ERROR_BASE + 184;

		// NETMSG DLL Error range
		const int NERR_BASE = 2100;
		const int MAX_NERR = NERR_BASE+899;

		// internal class that describes resource DLL range and name
		internal class DllDescriptor 
		{
			/// <summary>
			/// First message in the range reserved for this DLL
			/// </summary>
			public int firstMessage;
			/// <summary>
			/// Last message in the range reserved for this DLL
			/// </summary>
			public int lastMessage;
			/// <summary>
			/// The name of the DLL, e.g. "WINHTTP.DLL"
			/// </summary>
			public string dllName;

			/// <summary>
			/// Describes a DLL that can be searched for error messages
			/// </summary>
			/// <param name="first">First error number in the range</param>
			/// <param name="last">Last error number in the range</param>
			/// <param name="dll">File name of the associated dll, e.g. WINHTTP.DLL</param>
			public DllDescriptor(int first, int last, string dll) 
			{
				firstMessage = first;
				lastMessage = last;
				dllName = dll;
			}
		}

		// the list of resource DLLs we will search 
		private static ArrayList m_dllDescriptors = new ArrayList();

		// static constructor: initialize list of resource DLLs
		static Win32Message() 
		{
			m_dllDescriptors.Add(new DllDescriptor(WINHTTP_ERROR_BASE, WINHTTP_ERROR_LAST, "winhttp"));
			m_dllDescriptors.Add(new DllDescriptor(NERR_BASE, MAX_NERR, "netmsg.dll"));
		}


		// Win32 Function declarations
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibraryEx(
			string lpFileName, // file name of module
			int[] hFile, // reserved, must be NULL
			uint dwFlags // entry-point execution option
			);

		[DllImport("kernel32.dll")]
		private static extern int FreeLibrary(
			IntPtr hModule
			);

		[DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern int FormatMessage(
			int dwFlags,
			IntPtr lpSource,
			int dwMessageId,
			int dwLanguageId,
			out IntPtr MsgBuffer,
			int nSize,
			IntPtr Arguments
			);

		/// <summary>
		/// Get message string given error number
		/// </summary>
		/// <param name="lastError">The error number</param>
		/// <returns>Associated error message</returns>
		public static string GetMessage(int lastError) 
		{
			IntPtr hModule = IntPtr.Zero; // handle to resource DLL (if any)
			IntPtr pMessageBuffer; // pointer to unmananged message string
			int dwBufferLength; // length of the above

			// initialize the error message we will return with a generic one
			string errorMessage = String.Format("Last Win32 Error #{0:X8}", lastError);

			// set up the format flags: 
			int dwFormatFlags = 
				FORMAT_MESSAGE_ALLOCATE_BUFFER | // allocate the memory for us
				FORMAT_MESSAGE_IGNORE_INSERTS |  // no placeholder replacements
				FORMAT_MESSAGE_FROM_SYSTEM ;     // search system tables too

			// loop over known DLLs with corresponding message ranges
			foreach(DllDescriptor dllDesc in m_dllDescriptors) 
			{
				// If lastError is in the matching range, load the message source.
				if(lastError >= dllDesc.firstMessage && lastError <= dllDesc.lastMessage) 
				{
					// load DLL as datafile
					hModule = LoadLibraryEx(dllDesc.dllName, null, LOAD_LIBRARY_AS_DATAFILE);

					// if successful, add corresponding bit - will search in module first
					if(hModule != IntPtr.Zero) dwFormatFlags |= FORMAT_MESSAGE_FROM_HMODULE;
					break; // exit the loop - even if hModule is null, makes no sense to search further
				}
			}
         
			dwBufferLength = FormatMessage(dwFormatFlags,
				hModule,
				lastError,
				1024, // MAKELANGID (LANG_NEUTRAL, SUBLANG_DEFAULT)
				out pMessageBuffer,
				0,
				IntPtr.Zero);

			if(dwBufferLength > 0) 
			{
				errorMessage = Marshal.PtrToStringUni(pMessageBuffer);
				Marshal.FreeHGlobal(pMessageBuffer);
			}

			if(hModule != IntPtr.Zero) FreeLibrary(hModule);

			return errorMessage;
		}
	}
}
