using System;
using System.Globalization;

namespace WorldWind.Net.Wms
{
	/// <summary>
	/// Calculates URLs for WMS layers.
	/// </summary>
	public class WmsImageStore : ImageStore
	{
		#region Private Members
		
		string m_serverGetMapUrl;
		string m_wmsLayerName;
		string m_wmsLayerStyle;
		string m_imageFormat;
		string m_version;
		int	m_textureSizePixels = 512;

		#endregion	

		#region Properties
		
		public override bool IsDownloadableLayer
		{
			get
			{
				return true;
			}
		}

		public virtual string ServerGetMapUrl
		{
			get
			{
				return m_serverGetMapUrl;
			}
			set
			{
				m_serverGetMapUrl = value;
			}
		}

		public virtual string WMSLayerName
		{
			get
			{
				return m_wmsLayerName;
			}
			set
			{
				m_wmsLayerName = value;
			}
		}

		public virtual string WMSLayerStyle
		{
			get
			{
				return m_wmsLayerStyle;
			}
			set
			{
				m_wmsLayerStyle = value;
			}
		}

		public virtual string ImageFormat
		{
			get
			{
				return m_imageFormat;
			}
			set
			{
				m_imageFormat = value;
			}
		}

		public virtual string Version
		{
			get
			{
				return m_version;
			}
			set
			{
				m_version = value;
			}
		}

		/// <summary>
		/// Bitmap width/height
		/// </summary>
		public int TextureSizePixels
		{
			get
			{
				return m_textureSizePixels;
			}
			set
			{
				m_textureSizePixels = value;
			}
		}

		#endregion

		#region Public Methods

		protected override string GetDownloadUrl(WorldWind.Renderable.QuadTile qt)
		{
			if(m_serverGetMapUrl.IndexOf('?')>=0)
			{
				// Allow custom format string url
				// http://server.net/path?imageformat=png&width={WIDTH}&north={NORTH}...
				string url = m_serverGetMapUrl;
				url = url.Replace("{WIDTH}", m_textureSizePixels.ToString(CultureInfo.InvariantCulture));
				url = url.Replace("{HEIGHT}", m_textureSizePixels.ToString(CultureInfo.InvariantCulture));
				url = url.Replace("{WEST}", qt.West.ToString(CultureInfo.InvariantCulture));
				url = url.Replace("{EAST}", qt.East.ToString(CultureInfo.InvariantCulture));
				url = url.Replace("{NORTH}", qt.North.ToString(CultureInfo.InvariantCulture));
				url = url.Replace("{SOUTH}", qt.South.ToString(CultureInfo.InvariantCulture));

				return url;
			}
			else
			{
				string url = string.Format(CultureInfo.InvariantCulture, 
					"{0}?request=GetMap&layers={1}&srs=EPSG:4326&width={2}&height={3}&bbox={4},{5},{6},{7}&format={8}&version={9}&styles={10}",
					m_serverGetMapUrl,
					m_wmsLayerName, 
					m_textureSizePixels, 
					m_textureSizePixels, 
					qt.West, qt.South, qt.East, qt.North,  
					m_imageFormat, 
					m_version,  
					m_wmsLayerStyle );

				return url;
			}
		}

		#endregion
	}
}
