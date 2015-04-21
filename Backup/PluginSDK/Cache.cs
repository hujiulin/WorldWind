using System;
using System.Collections;
using System.IO;
using System.Threading;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Maintains the cached data on disk (staying within limits)
	/// </summary>
	public class Cache : IDisposable
	{
		// default value for maximum Cache size is 2 Gigabytes
		public long CacheUpperLimit = 2L * 1024L * 1024L * 1024L;		  
		// default value for size where Cache cleanup stops is 1.5 Gigabytes(75% of max size)
		public long CacheLowerLimit = 1536L * 1024L * 1024L;
		public string CacheDirectory;
		public TimeSpan CleanupFrequency;
		Timer m_timer;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Cache"/> class.
		/// </summary>
		/// <param name="cacheDirectory">Location of the cache files.</param>
		/// <param name="cleanupFrequencyInterval">Frequency of cache cleanup.</param>
		/// <param name="totalRunTime">Total duration application has been running so far.</param>
		public Cache(
			string cacheDirectory,
			TimeSpan cleanupFrequencyInterval,
			TimeSpan totalRunTime)
		{
			this.CleanupFrequency = cleanupFrequencyInterval;
			this.CacheDirectory = cacheDirectory;
			Directory.CreateDirectory(this.CacheDirectory);

			// Start the timer
			double firstDueSeconds = cleanupFrequencyInterval.TotalSeconds - 
				totalRunTime.TotalSeconds % cleanupFrequencyInterval.TotalSeconds;
			m_timer = new Timer( new TimerCallback(OnTimer), null,
				(long)(firstDueSeconds*1000),
				(long)cleanupFrequencyInterval.TotalMilliseconds );
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="cacheDirectory">Location of the cache files.</param>
		/// <param name="cleanupFrequencyInterval">Frequency of cache cleanup.</param>
		/// <param name="totalRunTime">Total duration application has been running so far.</param>
		public Cache(
			string cacheDirectory,
			long cacheLowerLimit,
			long cacheUpperLimit,
			TimeSpan cleanupFrequencyInterval,
			TimeSpan totalRunTime)
			: this(cacheDirectory, cleanupFrequencyInterval, totalRunTime )
		{
			this.CacheLowerLimit = cacheLowerLimit;
			this.CacheUpperLimit = cacheUpperLimit;
		}

		/// <summary>
		/// Monitors the cache, makes sure it stays within limits.
		/// </summary>
		private void OnTimer(object state)
		{
			try
			{
				// We are are not in a hurry
				Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

				// dirSize is reported as the total of the file sizes, in bytes
				// TODO: use the on-disk filesize, not FileInfo.Length, to calculate dirSize
				long dirSize = GetDirectorySize(new DirectoryInfo(this.CacheDirectory));
				if(dirSize < this.CacheUpperLimit)
					return;

				ArrayList fileInfoList = GetDirectoryFileInfoList(new DirectoryInfo(this.CacheDirectory));
				while(dirSize > this.CacheLowerLimit)
				{
					if (fileInfoList.Count <= 100)
						break;

					FileInfo oldestFile = null;
					foreach(FileInfo curFile in fileInfoList)
					{
						if(oldestFile == null)
						{
							oldestFile = curFile;
							continue;
						}

						if(curFile.LastAccessTimeUtc < oldestFile.LastAccessTimeUtc)
						{
							oldestFile = curFile;
						}
					}

					fileInfoList.Remove(oldestFile);
					dirSize -= oldestFile.Length;
					try
					{
						File.Delete(oldestFile.FullName);

						// Recursively remove empty directories
						string directory = oldestFile.Directory.FullName;
						while(Directory.GetFileSystemEntries(directory).Length==0)
						{
							Directory.Delete(directory);
							directory = Path.GetDirectoryName(directory);
						}
					}
					catch(IOException)
					{
						// Ignore non-removable file - move on to next
					}
				}
			}
			catch(Exception caught)
			{
				Log.Write(Log.Levels.Error, "CACH", caught.Message);
			}
		}

		public static ArrayList GetDirectoryFileInfoList(DirectoryInfo inDir)
		{
			ArrayList returnList = new ArrayList();
			foreach(DirectoryInfo subDir in inDir.GetDirectories())
			{
				returnList.AddRange(GetDirectoryFileInfoList(subDir));
			}
			foreach(FileInfo fi in inDir.GetFiles())
			{
				returnList.Add(fi);
			}
			return returnList;
		}

		public static long GetDirectorySize(DirectoryInfo inDir)
		{
			long returnBytes = 0;
			foreach(DirectoryInfo subDir in inDir.GetDirectories())
			{
				returnBytes += GetDirectorySize(subDir);
			}
			foreach(FileInfo fi in inDir.GetFiles())
			{
				try
				{
					returnBytes += fi.Length;
				} 
				catch(System.IO.IOException)
				{
					// Ignore files that may have disappeared since we started scanning.
				}
			}
			return returnBytes;
		}

		public override string ToString()
		{
			return CacheDirectory;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if(m_timer!=null)
			{
				m_timer.Dispose();
				m_timer = null;
			}
		}

		#endregion
	}
}
