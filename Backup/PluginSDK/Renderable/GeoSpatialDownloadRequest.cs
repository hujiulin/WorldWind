using WorldWind.Net;
using System;
using System.IO;
using Utility;

namespace WorldWind.Renderable
{
	public class GeoSpatialDownloadRequest : IDisposable
	{
		public float ProgressPercent;
		WebDownload download;
		string m_localFilePath;
		string m_url;
		QuadTile m_quadTile;
        ImageStore m_imageStore;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.GeoSpatialDownloadRequest"/> class.
		/// </summary>
		/// <param name="quadTile"></param>
		public GeoSpatialDownloadRequest(QuadTile quadTile, ImageStore imageStore, string localFilePath, string downloadUrl)
		{
			m_quadTile = quadTile;
			m_url = downloadUrl;
			m_localFilePath = localFilePath;
            m_imageStore = imageStore;
		}

		/// <summary>
		/// Whether the request is currently being downloaded
		/// </summary>
		public bool IsDownloading
		{
			get
			{
				return (download != null);
			}
		}

		public bool IsComplete
		{
			get
			{
				if(download==null)
					return true;
				return download.IsComplete;
			}
		}

		public QuadTile QuadTile
		{
			get
			{
				return m_quadTile;
			}
		}

		public double TileWidth 
		{
			get
			{
				return m_quadTile.East - m_quadTile.West;
			}
		}

		private void DownloadComplete(WebDownload downloadInfo)
		{
            Log.Write(Log.Levels.Debug+1, "GSDR", "Download completed for " + downloadInfo.Url);
            try
			{
				downloadInfo.Verify();

				m_quadTile.QuadTileSet.NumberRetries = 0;

				// Rename temp file to real name
				File.Delete(m_localFilePath);
				File.Move(downloadInfo.SavedFilePath, m_localFilePath);

				// Make the quad tile reload the new image
				m_quadTile.DownloadRequest = null;
				m_quadTile.Initialize();
			}
			catch(System.Net.WebException caught)
			{
				System.Net.HttpWebResponse response = caught.Response as System.Net.HttpWebResponse;
				if(response!=null && response.StatusCode==System.Net.HttpStatusCode.NotFound)
				{
					using(File.Create(m_localFilePath + ".txt"))
					{}
					return;
				}
				m_quadTile.QuadTileSet.NumberRetries++;
			}
			catch
			{
				using(File.Create(m_localFilePath + ".txt"))
				{}
                if (File.Exists(downloadInfo.SavedFilePath))
                {
                    try
                    {
                        File.Delete(downloadInfo.SavedFilePath);
                    }
                    catch (Exception e)
                    {
                        Log.Write(Log.Levels.Error, "GSDR", "could not delete file " + downloadInfo.SavedFilePath + ":");
                        Log.Write(e);
                    }
                }
			}
			finally
			{
                if(download != null)
    				download.IsComplete = true;
				m_quadTile.QuadTileSet.RemoveFromDownloadQueue(this);

                // potential deadlock! -step
                // Immediately queue next download
                m_quadTile.QuadTileSet.ServiceDownloadQueue();
			}
		}

		public virtual void StartDownload()
		{
            Log.Write(Log.Levels.Debug, "GSDR", "Starting download for " + m_url);
            QuadTile.IsDownloadingImage = true;
			download = new WebDownload(m_url);
			download.DownloadType = DownloadType.Wms;
			download.SavedFilePath = m_localFilePath + ".tmp";
			download.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
			download.CompleteCallback += new WorldWind.Net.DownloadCompleteHandler(DownloadComplete);
			download.BackgroundDownloadFile();
		}

		void UpdateProgress( int pos, int total )
		{
			if(total==0)
				// When server doesn't provide content-length, use this dummy value to at least show some progress.
				total = 50*1024; 
			pos = pos % (total+1);
			ProgressPercent = (float)pos/total;
		}

		public virtual void Cancel()
		{
			if (download!=null)
				download.Cancel();
		}

		public override string ToString()
		{
			return m_imageStore.GetLocalPath(QuadTile);
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			if(download!=null)
			{
				download.Dispose();
				download=null;
			}
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
