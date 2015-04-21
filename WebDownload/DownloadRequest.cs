using System;

namespace WorldWind.Net
{
	/// <summary>
	/// Base class for various types of download requests (protocol independent)
	/// </summary>
	public abstract class DownloadRequest : IDisposable
	{
		internal static DownloadQueue Queue;
		object m_owner; 

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Net.DownloadRequest"/> class.
		/// </summary>
		/// <param name="owner"></param>
		protected DownloadRequest(object owner)
		{
			m_owner = owner;
		}

		/// <summary>
		/// A unique key identifying this request
		/// </summary>
		public abstract string Key
		{
			get;
		}

		/// <summary>
		/// The object that created this request
		/// </summary>
		public object Owner
		{
			get
			{
				return m_owner;
			}
			set
			{
				m_owner = value;
			}
		}

		/// <summary>
		/// Value (0-1) indicating how far the download has progressed.
		/// </summary>
		public abstract float Progress
		{
			get;
		}

		/// <summary>
		/// Whether the request is currently being downloaded
		/// </summary>
		public abstract bool IsDownloading
		{
			get;
		}

		/// <summary>
		/// Starts processing this request
		/// </summary>
		public abstract void Start();

		/// <summary>
		/// Calculates the score of this request.  Used to prioritize downloads.  
		/// Override in derived class to allow prioritization.
		/// </summary>
		/// <returns>Relative score or float.MinValue if request is no longer of interest.</returns>
		public virtual float CalculateScore()
		{
			return 0;
		}

		/// <summary>
		/// Derived classes should call this method to signal processing complete.
		/// </summary>
		public virtual void OnComplete()
		{
			Queue.OnComplete(this);
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
		}

		#endregion
	}
}
