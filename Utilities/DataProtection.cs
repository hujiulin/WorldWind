using System;
using System.Text;
using System.Runtime.InteropServices;


namespace Utility 
{
	/// <summary>
	/// Utility class to provide secure storage of data
	/// </summary>
	public class DataProtector 
	{
		// Constant declarations
		private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
		private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

		// Win32 structure declarations
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct DATA_BLOB 
		{
			public int cbData;
			public IntPtr pbData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct CRYPTPROTECT_PROMPTSTRUCT 
		{
			public int cbSize;
			public int dwPromptFlags;
			public IntPtr hwndApp;
			public String szPrompt;
		}

		// Win32 Function declarations
		[DllImport("Crypt32.dll", SetLastError=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptProtectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy,
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT 
			pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut
			);

		[DllImport("Crypt32.dll", SetLastError=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptUnprotectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy, 
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT 
			pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut
			);

		/// <summary>
		/// this enum is used to control whether a user-specific or machine-specific
		/// location is used to store sensitive data.
		/// </summary>
		public enum Store {USE_MACHINE_STORE = 1, USE_USER_STORE};
		private Store store;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:Utility.DataProtector"/> class. 
		/// </summary>
		/// <param name="tempStore">Store type - see enum Store for valid values</param>
		public DataProtector(Store tempStore) 
		{
			store = tempStore;
		}

		private static void InitPromptstruct(ref CRYPTPROTECT_PROMPTSTRUCT ps) 
		{
			ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
			ps.dwPromptFlags = 0;
			ps.hwndApp = IntPtr.Zero;
			ps.szPrompt = null;
		}

		/// <summary>
		/// Encrypt byte data
		/// </summary>
		/// <param name="plainText">The byte data to be encoded</param>
		/// <param name="optionalEntropy">Additional entropy, recommended for machine-specific case</param>
		/// <returns>Returns a byte array with the encoded data</returns>
		public byte[] Encrypt(byte[] plainText, byte[] optionalEntropy) 
		{
			bool retVal = false;

			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherTextBlob = new DATA_BLOB();
			DATA_BLOB entropyBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);

			int dwFlags;
			try 
			{
				try 
				{
					int bytesSize = plainText.Length;
					plainTextBlob.pbData = Marshal.AllocHGlobal(bytesSize);
					if(IntPtr.Zero == plainTextBlob.pbData) 
					{
						throw new Exception("Unable to allocate plaintext buffer.");
					}
					plainTextBlob.cbData = bytesSize;
					Marshal.Copy(plainText, 0, plainTextBlob.pbData, bytesSize);
				}
				catch(Exception ex) 
				{
					throw new Exception("Exception marshalling data. " + ex.Message);
				}

				if(Store.USE_MACHINE_STORE == store) 
				{
					//Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null
					if(null == optionalEntropy) 
					{
						//Allocate something
						optionalEntropy = new byte[0];
					}
					try 
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(optionalEntropy.Length);;
						if(IntPtr.Zero == entropyBlob.pbData) 
						{
							throw new Exception("Unable to allocate entropy data buffer.");
						}
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
						entropyBlob.cbData = bytesSize;
					}
					catch(Exception ex) 
					{
						throw new Exception("Exception  marshalling entropy data. " + 
							ex.Message);
					}
				}
				else 
				{
					//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				retVal = CryptProtectData(ref plainTextBlob, "", ref entropyBlob, 
					IntPtr.Zero, ref prompt, dwFlags, 
					ref cipherTextBlob);

				if(false == retVal) 
				{
					throw new Exception("Encryption failed. " + 
						Win32Message.GetMessage(Marshal.GetLastWin32Error()));
				}
				//Free the blob and entropy.
				if(IntPtr.Zero != plainTextBlob.pbData) 
				{
					Marshal.FreeHGlobal(plainTextBlob.pbData);
				}
				if(IntPtr.Zero != entropyBlob.pbData) 
				{
					Marshal.FreeHGlobal(entropyBlob.pbData);
				}
			}
			catch(Exception ex) 
			{
				throw new Exception("Exception encrypting. " + ex.Message);
			}

			byte[] cipherText = new byte[cipherTextBlob.cbData];
			Marshal.Copy(cipherTextBlob.pbData, cipherText, 0, cipherTextBlob.cbData);
			Marshal.FreeHGlobal(cipherTextBlob.pbData); 
			return cipherText;
		}

		/// <summary>
		/// Decrypt byte data
		/// </summary>
		/// <param name="cipherText">Data to be decoded</param>
		/// <param name="optionalEntropy">Additional entropy, recommended for machine-specific case</param>
		/// <returns>Returns a byte array with the encoded data</returns>
		public byte[] Decrypt(byte[] cipherText, byte[] optionalEntropy) 
		{
			bool retVal = false;

			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);

			try 
			{
				try 
				{
					int cipherTextSize = cipherText.Length;
					cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
					if(IntPtr.Zero == cipherBlob.pbData) 
					{
						throw new Exception("Unable to allocate cipherText buffer.");
					}
					cipherBlob.cbData = cipherTextSize;
					Marshal.Copy(cipherText, 0, cipherBlob.pbData, 
						cipherBlob.cbData);
				}
				catch(Exception ex) 
				{
					throw new Exception("Exception marshalling data. " + 
						ex.Message);
				}
				DATA_BLOB entropyBlob = new DATA_BLOB();
				int dwFlags;
				if(Store.USE_MACHINE_STORE == store) 
				{
					//Using the machine store, should be providing entropy.
					dwFlags = 
						CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null
					if(null == optionalEntropy) 
					{
						//Allocate something
						optionalEntropy = new byte[0];
					}
					try 
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);
						if(IntPtr.Zero == entropyBlob.pbData) 
						{
							throw new Exception("Unable to allocate entropy buffer.");
						}
						entropyBlob.cbData = bytesSize;
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, 
							bytesSize);
					}
					catch(Exception ex) 
					{
						throw new Exception("Exception marshalling entropy data. " + 
							ex.Message);
					}
				}
				else 
				{
					//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				retVal = CryptUnprotectData(ref cipherBlob, null, ref 
					entropyBlob, 
					IntPtr.Zero, ref prompt, dwFlags, 
					ref plainTextBlob);
				if(false == retVal) 
				{
					throw new Exception("Decryption failed. " + 
						Win32Message.GetMessage(Marshal.GetLastWin32Error()));
				}
				//Free the blob and entropy.
				if(IntPtr.Zero != cipherBlob.pbData) 
				{
					Marshal.FreeHGlobal(cipherBlob.pbData);
				}
				if(IntPtr.Zero != entropyBlob.pbData) 
				{
					Marshal.FreeHGlobal(entropyBlob.pbData);
				}
			}
			catch(Exception ex) 
			{
				throw new Exception("Exception decrypting. " + ex.Message);
			}
			byte[] plainText = new byte[plainTextBlob.cbData];
			Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
			Marshal.FreeHGlobal(plainTextBlob.pbData); 
			return plainText;
		}

		System.Text.UnicodeEncoding unienc = new System.Text.UnicodeEncoding();

		/// <summary>
		/// Encode data given as string, return as Base64d byte array
		/// </summary>
		/// <param name="plainText">The string to be encoded</param>
		/// <returns>The resulting encoded string</returns>
		public string EncryptStringToBase64(string plainText) 
		{
			byte [] theBytes = unienc.GetBytes(plainText);

			string encryptedString = null;
			try 
			{
				encryptedString = System.Convert.ToBase64String(Encrypt(theBytes, null));
			}
			catch(System.Exception caught) 
			{
				Log.Write(caught);
			}
			return encryptedString;
		}

		/// <summary>
		/// Decode encrypted data provided as Base64 string
		/// </summary>
		/// <param name="cypherText">The encoded data</param>
		/// <returns>The original string</returns>
		public string DecryptBase64AsString(string cypherText) 
		{
			byte [] theBytes = System.Convert.FromBase64String(cypherText);

			string decryptedString = null;

			try 
			{
				decryptedString = unienc.GetString(Decrypt(theBytes, null));
			}
			catch(System.Exception caught) 
			{
				Log.Write(caught);
			}
			return decryptedString;
		}

		/// <summary>
		/// Transparently decrypts data - if prefix "crypt:" is not present,
		/// data will be returned verbatim. This is also done if encryption fails, e.g.
		/// because the operating system does not support it.
		/// </summary>
		/// <param name="cypherText">The (potentially) encrypted string</param>
		/// <returns>Decrypted string or unchanged input</returns>
		public string TransparentDecrypt(string cypherText) 
		{
			if(cypherText.StartsWith("crypt:"))
			{
				string decryptedString = DecryptBase64AsString(cypherText.Substring(6));
				if(decryptedString != null) return decryptedString;            
			}
			return cypherText;
		}

		/// <summary>
		/// Encrypts data, adding a "crypt:" prefix to identify it as such.
		/// Should encryption fail e.g. because the operating system does not support it,
		/// the plaintext is returned verbatim.
		/// </summary>
		/// <param name="plainText">Plain text to be encrypted</param>
		/// <returns>Encrypted plaintext with "crypt:" prefix, or unchanged input</returns>
		public string TransparentEncrypt(string plainText)
		{
			string encryptedString = EncryptStringToBase64(plainText);
			if(encryptedString != null) return "crypt:"+encryptedString;
         
			return plainText;
		}
	}
}
