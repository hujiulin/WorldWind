using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using Utility;

namespace WorldWind.Net
{
	/// <summary>
	/// Utility class - determines required proxy (if any) for download.
	/// Currently able to handle:
	/// - Explicit use of no proxy
	/// - retrieval of current Windows settings (default)
	/// - Proxy authentication
	/// - Proxy scripting (autoproxy)
	/// - Proxy autodiscovery.
	/// </summary>
	public sealed class ProxyHelper
	{
		// Constant declarations
		const int WINHTTP_ACCESS_TYPE_DEFAULT_PROXY = 0;
		const int WINHTTP_ACCESS_TYPE_NO_PROXY = 1;
		const int WINHTTP_ACCESS_TYPE_NAMED_PROXY = 3;

		const int WINHTTP_AUTOPROXY_AUTO_DETECT = 0x00000001;
		const int WINHTTP_AUTOPROXY_CONFIG_URL = 0x00000002;
		const int WINHTTP_AUTOPROXY_RUN_INPROCESS = 0x00010000;
		const int WINHTTP_AUTOPROXY_RUN_OUTPROCESS_ONLY = 0x00020000;

		const int WINHTTP_AUTO_DETECT_TYPE_DHCP = 0x00000001;
		const int WINHTTP_AUTO_DETECT_TYPE_DNS_A = 0x00000002;

		// Win32 Structure declarations

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] 
			struct WINHTTP_AUTOPROXY_OPTIONS 
		{ 
			[MarshalAs(UnmanagedType.U4)] 
			public int dwFlags; 
			[MarshalAs(UnmanagedType.U4)] 
			public int dwAutoDetectFlags; 
			public string lpszAutoConfigUrl; 
			public IntPtr lpvReserved; 
			[MarshalAs(UnmanagedType.U4)] 
			public int dwReserved; 
			public bool fAutoLoginIfChallenged; 
		} 

		/// <summary>
		/// Proxy information structure returned by WinHTTP interop
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] 
			struct WINHTTP_PROXY_INFO 
		{ 
			[MarshalAs(UnmanagedType.U4)] 
			public int dwAccessType; 
			public IntPtr pwszProxy;
			public IntPtr pwszProxyBypass;
		} 

		// Win32 Function declarations
		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)] 
		static extern IntPtr WinHttpOpen(
			string pwszUserAgent,
			int dwAccessType,
			IntPtr pwszProxyName,
			IntPtr pwszProxyBypass,
			int dwFlags
			);

		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)] 
		static extern bool WinHttpCloseHandle(IntPtr hInternet);

		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)] 
		static extern bool WinHttpGetProxyForUrl( 
			IntPtr hSession, 
			string lpcwszUrl, 
			ref WINHTTP_AUTOPROXY_OPTIONS pAutoProxyOptions, 
			ref WINHTTP_PROXY_INFO pProxyInfo 
			); 

		/// <summary>
		/// Constructor (static class)
		/// </summary>
		private ProxyHelper()
		{
		}

		// returns true if string is either null or of zero length
		static bool IsEmpty(string s) 
		{
			return (s == null || s.Length == 0);
		}

		// Keep track of WinHTTP Session handle as that is used to cache proxy information
		static IntPtr hSession = IntPtr.Zero;

		// Initialize Proxy Helper - open WinHTPP session
		static void OpenWinHttpSession()
		{
			hSession = WinHttpOpen("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322)", 
				WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, 
				IntPtr.Zero, // WINHTTP_NO_PROXY_NAME
				IntPtr.Zero, // WINHTTP_NO_PROXY_BYPASS
				0);
		}

		// Close WinHttp session
		static void CloseWinHttpSession() 
		{
			if(hSession != IntPtr.Zero) 
			{
				WinHttpCloseHandle(hSession); //Close http session
				hSession = IntPtr.Zero;
			}
		}

		// Set up a Credentials class given name, password and optionally, domain.
		static ICredentials DetermineCredentials(string name, string password, string domain) 
		{
			ICredentials theCreds = null;
			if(!IsEmpty(name)) 
			{
				theCreds = (domain == null) ? 
					new NetworkCredential(name, password) :
					new NetworkCredential(name, password, domain);
			}
			return theCreds;
		}

		// Determine dynamic proxy url, either automagically, or by downloading and running specified script
		static IWebProxy DetermineAutoProxyForUrl(string targetUrl, string proxyScriptUrl, ref int errorCode) 
		{
			if(hSession == IntPtr.Zero) 
			{
				OpenWinHttpSession();
			}

			WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions = new WINHTTP_AUTOPROXY_OPTIONS(); 
			WINHTTP_PROXY_INFO proxyInfo = new WINHTTP_PROXY_INFO(); 
			proxyInfo.pwszProxy = proxyInfo.pwszProxyBypass = IntPtr.Zero;

			if(!IsEmpty(proxyScriptUrl)) 
			{
				// The proxy script URL is already known.  
				// Therefore, auto-detection is not required.
				autoProxyOptions.dwFlags = WINHTTP_AUTOPROXY_CONFIG_URL;

				// Set the proxy auto configuration URL.
				autoProxyOptions.lpszAutoConfigUrl = proxyScriptUrl;
				autoProxyOptions.dwAutoDetectFlags = 0;
			}
			else 
			{  // must determine location of script
				autoProxyOptions.dwFlags = WINHTTP_AUTOPROXY_AUTO_DETECT;
				autoProxyOptions.dwAutoDetectFlags = (WINHTTP_AUTO_DETECT_TYPE_DHCP|WINHTTP_AUTO_DETECT_TYPE_DNS_A);
			}

			// If obtaining the PAC script requires NTLM/Negotiate
			// authentication, automatically supply the domain credentials of the client. 
			autoProxyOptions.fAutoLoginIfChallenged = true;

			//Get proxy
			bool result = WinHttpGetProxyForUrl(hSession, targetUrl, ref autoProxyOptions, ref proxyInfo); 

			// that failed - retrieve and safely store last Win32 error
			if(!result) 
			{
				errorCode = Marshal.GetLastWin32Error();
			}

			string proxyUrl = ""; 

			// get returned string, if any
			if(proxyInfo.pwszProxy != IntPtr.Zero) 
			{
				// get string from IntPtr in structure
				proxyUrl = Marshal.PtrToStringUni(proxyInfo.pwszProxy);
				Marshal.FreeHGlobal(proxyInfo.pwszProxy); // free Win32 string

				// split returned proxies into array
				string [] theUrls = proxyUrl.Split(';');

				// use only first one, get rid of "PROXY " prefix and whitespace
				proxyUrl = theUrls[0].Replace("PROXY ", "").Trim();
			}
			if(proxyInfo.pwszProxyBypass != IntPtr.Zero) Marshal.FreeHGlobal(proxyInfo.pwszProxyBypass);

			return IsEmpty(proxyUrl) ? null : new WebProxy(proxyUrl);
		}


		/// <summary>
		/// main working horse - determines proxy Url and sets up IWebProxy class complete with credentials 
		/// </summary>
		/// <param name="targetUrl">the file to download</param>
		/// <param name="useDefaultProxy">whether to use Internet Explorer settings</param>
		/// <param name="useDynamicProxy">If set, will download a script that provides the real proxy URL</param>
		/// <param name="proxyUrl">Script URL (if useDynamicProxy is true) or proxy URL</param>
		/// <param name="userName">User name (credentials)</param>
		/// <param name="password">Password (credentials)</param>
		/// <returns>An IWebProxy configured correspondingly</returns>
		public static IWebProxy DetermineProxyForUrl(
			string targetUrl,
			bool useDefaultProxy,
			bool useDynamicProxy,
			string proxyUrl,
			string userName,
			string password
			)
		{
			IWebProxy theProxy = null;

			if(useDefaultProxy) 
			{
				// Get Internet Explorer settings
                theProxy = WebRequest.DefaultWebProxy;
			}
			else 
			{
				if(useDynamicProxy) 
				{
					// need to use scripting, call corresponding method
					int errCode = 0;

					theProxy = DetermineAutoProxyForUrl(targetUrl, proxyUrl, ref errCode);

					if(errCode != 0) 
					{
						// failed, throw an exception
						throw new System.Exception(
							String.Format(
							CultureInfo.CurrentCulture,
							"Determining dynamic proxy for target url '{0}' using script url '{1}' failed with Win32 error '{2}'",
							targetUrl, IsEmpty(proxyUrl) ? "(none)" : proxyUrl, Win32Message.GetMessage(errCode))
							);
					}
				}
				else 
				{
					if(IsEmpty(proxyUrl)) 
					{
						// no scripting, no default -> no proxy
                        theProxy = null;
					}
					else 
					{
						// plain URL provided
						theProxy = new WebProxy(proxyUrl);
					}
				}
			}

			// add credentials to the mix if provided and if proxy exists
			if (theProxy != null)
				theProxy.Credentials = DetermineCredentials(userName, password, null);

			return theProxy;
		}
	}
}
