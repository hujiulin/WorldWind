using System;
using System.IO;
using Utility;

namespace WorldWind.Net
{
	/// <summary>
	/// Base class for various types of download requests (protocol independent)
	/// </summary>
	public class WebDownloadRequest : DownloadRequest
	{
		protected WebDownload download;
		protected static string TemporaryExtension = ".tmp";

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Net.WebDownloadRequest"/> class.
		/// </summary>
		/// <param name="owner">The object owning this request.</param>
		public WebDownloadRequest(object owner) : base(owner)
		{
			download = new WebDownload("");
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">The object owning this request.</param>
		/// <param name="url">The URL to download from.</param>
		public WebDownloadRequest(object owner, string url) : base(owner)
		{
			download = new WebDownload(url);
		}


		/// <summary>
		/// Destination output file from download.
		/// </summary>
		public string SaveFilePath
		{
			get
			{
				if(download.SavedFilePath == null)
					return null;
				return download.SavedFilePath.Substring(0, 
					download.SavedFilePath.Length - TemporaryExtension.Length);
			}
			set
			{
				download.SavedFilePath = value + TemporaryExtension;
			}
		}

		/// <summary>
		/// Value (0-1) indicating how far the download has progressed.
		/// </summary>
		public override float Progress
		{
			get
			{
				if(download==null)
					return 1;
				float total = download.ContentLength; 
				if(download.ContentLength==0)
					// When server doesn't provide content-length, use this dummy value to at least show some progress.
					total = 50*1024;
				float percent = (float)(download.BytesProcessed % (total+1))/total;
				return percent;
			}
		}

		/// <summary>
		/// A unique key identifying this request
		/// </summary>
		public override string Key
		{
			get
			{
				if(download==null)
					return null;
				return download.Url;
			}
		}

		/// <summary>
		/// Whether the request is currently being downloaded
		/// </summary>
		public override bool IsDownloading
		{
			get
			{
				if(download==null)
					return false;
				return download.IsDownloadInProgress;
			}
		}

		public override void Start()
		{
			download.CompleteCallback += new DownloadCompleteHandler(InternalDownloadComplete);
			if(download.SavedFilePath!=null && download.SavedFilePath.Length > 0)
				download.BackgroundDownloadFile();
			else
				download.BackgroundDownloadMemory();
		}

		/// <summary>
		/// Calculates the score of this request.  Used to prioritize downloads.  
		/// Override in derived class to allow prioritization.
		/// </summary>
		/// <returns>Relative score or float.MinValue if request is no longer of interest.</returns>
		public override float CalculateScore()
		{
			return 0;
		}

		/// <summary>
		/// Override to be notified when download has completed.
		/// </summary>
		protected virtual void InternalDownloadComplete(WebDownload download)
		{
			try
			{
				DownloadComplete();
			}
			catch(Exception caught)
			{
				Log.Write(Log.Levels.Error, "QUEU", download.Url + ": " + caught.Message);
			}

			OnComplete();
		}

		protected virtual void DownloadComplete()
		{
		}

		public override string ToString()
		{
			return download.Url;
		}

		public override void Dispose()
		{
			try
			{
				if(download != null)
				{
					download.Dispose();
					download = null;
				}
			}
			finally
			{
				base.Dispose();
			}
		}
	}
}
