using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace onlyconnect
{
	/// <summary>
	/// This class implements IStream for an easier way
	/// to read back the output from IPersistStreamInit
	/// Thanks to microsoft.public.dotnet.general
	/// </summary>
	public class ComStream : IStream 
	{
		private Stream stream;
		public ComStream(Stream stream) 
		{
			this.stream = stream;
		}

		void IStream.Clone(out IStream ppstm) 
		{
			ppstm = null;
		}

		public void Commit(int grfCommitFlags) 
		{

		}

		public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten) 
		{

		}

		public void LockRegion(long libOffset, long cb, int dwLockType) 
		{

		}

		public void Read(byte[] pv, int cb, IntPtr pcbRead) 
		{
			stream.Read( pv, (int)stream.Position, cb );
		}

		public void Revert() 
		{
		}

		void IStream.Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
		{
			stream.Seek( dlibMove, (SeekOrigin)dwOrigin );
		}

		public void SetSize(long libNewSize) 
		{
			stream.SetLength( libNewSize );
		}

		public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag) 
		{
            pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG();
		}

		public void UnlockRegion(long libOffset, long cb, int dwLockType) 
		{
		}

		public void Write(byte[] pv, int cb, IntPtr pcbWritten) 
		{
			stream.Write( pv, 0, cb );
		}
	}
	

}
