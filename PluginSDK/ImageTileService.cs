using System;
using System.Globalization;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ImageTileService.
	/// </summary>
	public class ImageTileService
	{
		#region Private Members
		TimeSpan _cacheExpirationTime = TimeSpan.MaxValue;
		string _datasetName;
		string _serverUri;
		string _serverLogoPath;
		#endregion

		#region Properties

		public TimeSpan CacheExpirationTime
		{
			get
			{
				return this._cacheExpirationTime;
			}								   
		}

		public string ServerLogoPath
		{
			get
			{
				return this._serverLogoPath;
			}
			set
			{
				this._serverLogoPath = value;
			}								   
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="datasetName"></param>
		/// <param name="serverUri"></param>
		/// <param name="serverLogoPath"></param>
		/// <param name="cacheExpirationTime"></param>
		public ImageTileService(
			string datasetName,
			string serverUri,
			string serverLogoPath,
			TimeSpan cacheExpirationTime)
		{
			this._serverUri = serverUri;
			this._datasetName = datasetName;
			this._serverLogoPath = serverLogoPath;
			this._cacheExpirationTime = cacheExpirationTime;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="datasetName"></param>
		/// <param name="serverUri"></param>
		/// <param name="serverLogoPath"></param>
		public ImageTileService(
			string datasetName,
			string serverUri,
			string serverLogoPath)
		{
			this._serverUri = serverUri;
			this._datasetName = datasetName;
			this._serverLogoPath = serverLogoPath;
		}

		public virtual string GetImageTileServiceUri(int level, int row, int col)
		{
			return String.Format(CultureInfo.InvariantCulture, "{0}?T={1}&L={2}&X={3}&Y={4}", this._serverUri, this._datasetName, level, col, row);
		}
	}
}
