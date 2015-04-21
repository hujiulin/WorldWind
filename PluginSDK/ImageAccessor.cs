using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.IO;
using WorldWind.Net.Wms;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ImageAccessor.
	/// </summary>
	public class ImageAccessor : IDisposable
	{
		#region Private Members

		protected string	m_dataDirectory;
		protected int	m_textureSizePixels;
		protected double m_levelZeroTileSizeDegrees;
		protected int m_numberLevels;
		protected Texture m_serverLogo;
		protected Rectangle m_serverLogoSize;
		TimeSpan m_DataExpirationTime = TimeSpan.MaxValue;

		/// <summary>
		/// File extension of the source image file format
		/// </summary>
		protected string m_imageFileExtension;

		/// <summary>
		/// Cache subdirectory for this layer
		/// </summary>
		protected string m_cacheDirectory;

		protected string m_duplicateTexturePath;
		protected WMSLayerAccessor m_wmsLayerAccessor;
		protected ImageTileService m_imageTileService;

		#endregion

		#region Properties

		public TimeSpan DataExpirationTime
		{
			get
			{
				return m_DataExpirationTime;
			}
			set
			{
				m_DataExpirationTime = value;
			}
		}

		public double LevelZeroTileSizeDegrees
		{
			get
			{
				return m_levelZeroTileSizeDegrees;
			}
		}

		public int LevelCount
		{
			get
			{
				return m_numberLevels;
			}
		}

		public int TextureSizePixels
		{
			get
			{
				return m_textureSizePixels;
			}
		}

		public string ImageExtension
		{
			get
			{
				return m_imageFileExtension;
			}
			set
			{
				// Strip any leading dot
				m_imageFileExtension = value.Replace(".", "");
			}
		}

		public bool IsDownloadableLayer
		{
			get
			{
				if(m_wmsLayerAccessor != null)
					return true;
				if(m_imageTileService != null)
					return true;

				return false;
			}
		}

		public string ServerLogoPath
		{
			get
			{
				if(m_wmsLayerAccessor != null)
					return m_wmsLayerAccessor.LogoFilePath;
				else if(m_imageTileService  != null)
					return m_imageTileService .ServerLogoPath;
				else
					return null;
			}
		}

		/// <summary>
		/// Server logo texture
		/// </summary>
		public Texture ServerLogo
		{
			get
			{
				return m_serverLogo;
			}
			set
			{
				m_serverLogo = value;
			}
		}

		/// <summary>
		/// Server logo size (pixels)
		/// </summary>
		public Rectangle ServerLogoSize
		{
			get
			{
				return m_serverLogoSize;
			}
			set
			{
				m_serverLogoSize = value;
			}
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageAccessor"/> class.
		/// </summary>
		/// <param name="permanentTextureDirectory"></param>
		/// <param name="textureSizePixels"></param>
		/// <param name="levelZeroTileSizeDegrees"></param>
		/// <param name="numberLevels"></param>
		/// <param name="imageFileExtension"></param>
		/// <param name="cacheDirectory"></param>
		public ImageAccessor(
			string permanentTextureDirectory, 
			int textureSizePixels, 
			double levelZeroTileSizeDegrees,
			int numberLevels,
			string imageFileExtension,
			string cacheDirectory)
		{
			m_dataDirectory = permanentTextureDirectory;
			m_textureSizePixels = textureSizePixels;
			m_levelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
			m_numberLevels = numberLevels;
			ImageExtension = imageFileExtension;
			m_cacheDirectory = cacheDirectory;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageAccessor"/> class.
		/// </summary>
		/// <param name="permanentTextureDirectory"></param>
		/// <param name="textureSizePixels"></param>
		/// <param name="levelZeroTileSizeDegrees"></param>
		/// <param name="numberLevels"></param>
		/// <param name="imageFileExtension"></param>
		/// <param name="cacheDirectory"></param>
		/// <param name="duplicateTextureFilePath"></param>
		public ImageAccessor(
			string permanentTextureDirectory, 
			int textureSizePixels, 
			double levelZeroTileSizeDegrees,
			int numberLevels,
			string imageFileExtension,
			string cacheDirectory,
			string duplicateTextureFilePath
			)
		{
			m_dataDirectory = permanentTextureDirectory;
			m_textureSizePixels = textureSizePixels;
			m_levelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
			m_numberLevels = numberLevels;
			ImageExtension = imageFileExtension;
			m_cacheDirectory = cacheDirectory;
			m_duplicateTexturePath = duplicateTextureFilePath;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageAccessor"/> class.
		/// </summary>
		/// <param name="permanentTextureDirectory"></param>
		/// <param name="textureSizePixels"></param>
		/// <param name="levelZeroTileSizeDegrees"></param>
		/// <param name="numberLevels"></param>
		/// <param name="imageFileExtension"></param>
		/// <param name="cacheDirectory"></param>
		/// <param name="wmsLayerAccessor"></param>
		public ImageAccessor(
			string permanentTextureDirectory, 
			int textureSizePixels, 
			double levelZeroTileSizeDegrees,
			int numberLevels,
			string imageFileExtension,
			string cacheDirectory,
			WMSLayerAccessor wmsLayerAccessor
			)
		{
			m_dataDirectory = permanentTextureDirectory;
			m_textureSizePixels = textureSizePixels;
			m_levelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
			m_numberLevels = numberLevels;
			ImageExtension = imageFileExtension;
			m_cacheDirectory = cacheDirectory;
			m_wmsLayerAccessor = wmsLayerAccessor;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageAccessor"/> class.
		/// </summary>
		/// <param name="permanentTextureDirectory"></param>
		/// <param name="textureSizePixels"></param>
		/// <param name="levelZeroTileSizeDegrees"></param>
		/// <param name="numberLevels"></param>
		/// <param name="imageFileExtension"></param>
		/// <param name="cacheDirectory"></param>
		/// <param name="imageTileService"></param>
		public ImageAccessor(
			string permanentTextureDirectory, 
			int textureSizePixels, 
			double levelZeroTileSizeDegrees,
			int numberLevels,
			string imageFileExtension,
			string cacheDirectory,
			ImageTileService imageTileService
			)
		{
			m_dataDirectory = permanentTextureDirectory;
			m_textureSizePixels = textureSizePixels;
			m_levelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
			m_numberLevels = numberLevels;
			ImageExtension = imageFileExtension;
			m_cacheDirectory = cacheDirectory;
			m_imageTileService  = imageTileService;
		}

		public ImageTileInfo GetImageTileInfo(int level, int row, int col)
		{
			return GetImageTileInfo(level, row, col, true);
		}

		public ImageTileInfo GetImageTileInfo(int level, int row, int col, bool allowCache)
		{
			if(level >= m_numberLevels)
				throw new ArgumentException("Level " + level.ToString() + " not available.");

			string relativePath = String.Format(@"{0}\{1:D4}\{1:D4}_{2:D4}.{3}", level, row, col, m_imageFileExtension);
			
			if(m_dataDirectory != null)
			{
				// Search data directory first
				string rawFullPath = Path.Combine( m_dataDirectory, relativePath );
				if(File.Exists(rawFullPath))
					return new ImageTileInfo(rawFullPath);
			}
	
			// Try cache with default file extension
			string cacheFullPath = Path.Combine( m_cacheDirectory, relativePath );
			if(File.Exists(cacheFullPath))
				return new ImageTileInfo(cacheFullPath);

			// Try cache but accept any valid image file extension
			const string ValidExtensions = ".bmp.dds.dib.hdr.jpg.jpeg.pfm.png.ppm.tga.gif.tif";
			
			if(allowCache)
			{
				string cacheSearchPath = Path.GetDirectoryName(cacheFullPath);
				if(Directory.Exists(cacheSearchPath))
				{
					foreach( string imageFile in Directory.GetFiles(
						cacheSearchPath, 
						Path.GetFileNameWithoutExtension(cacheFullPath) + ".*") )
					{
						string extension = Path.GetExtension(imageFile).ToLower();
						if(ValidExtensions.IndexOf(extension)<0)
							continue;

						if(m_imageTileService  != null)
						{
							return new ImageTileInfo(
								imageFile, 
								m_imageTileService.GetImageTileServiceUri(level, row, col) );
						}
			
						if(m_wmsLayerAccessor != null)
						{
							double tileRange = m_levelZeroTileSizeDegrees * Math.Pow(0.5, level);
							double west = -180.0 + col * tileRange;
							double south = -90.0 + row * tileRange;

							string fileUri = m_wmsLayerAccessor.GetWMSRequestUrl(
								west, south, west + tileRange, south + tileRange,
								m_textureSizePixels, m_textureSizePixels);

							return new ImageTileInfo(imageFile, fileUri);
						}

						return new ImageTileInfo(imageFile);
					}
				}
			}

			if(m_imageTileService  != null)
			{
				return new ImageTileInfo(
					cacheFullPath, 
					m_imageTileService.GetImageTileServiceUri(level, row, col) );
			}
			
			if(m_wmsLayerAccessor != null)
			{
				double tileRange = m_levelZeroTileSizeDegrees * Math.Pow(0.5, level);
				double west = -180.0 + col * tileRange;
				double south = -90.0 + row * tileRange;

				string fileUri = m_wmsLayerAccessor.GetWMSRequestUrl(
					west, south, west + tileRange, south + tileRange,
					m_textureSizePixels, m_textureSizePixels);

				return new ImageTileInfo(Path.Combine(m_cacheDirectory, relativePath), fileUri);
			}
			
			// No success, return our "duplicate" tile if any
			if(m_duplicateTexturePath != null && File.Exists(m_duplicateTexturePath))
			{
				return new ImageTileInfo(m_duplicateTexturePath, 
					Path.Combine(m_cacheDirectory, m_duplicateTexturePath));
			}

			return null;
			//throw new ApplicationException("No image access method available.");
		}
		#region IDisposable Members

		public void Dispose()
		{
			if(m_serverLogo!=null)
			{
				m_serverLogo.Dispose();
				m_serverLogo = null;
			}

			GC.SuppressFinalize(this);
		}

		#endregion
	}

	public class ImageTileInfo
	{
		#region Private Members

		string m_imagePath;
		string m_uri;

		#endregion

		#region Properties

		/// <summary>
		/// Local full path to the image file (cache or data)
		/// </summary>
		public string ImagePath
		{
			get
			{
				return m_imagePath;
			}
			set
			{
				m_imagePath = value;
			}
		}

		/// <summary>
		/// Uri for downloading image from network
		/// </summary>
		public string Uri
		{
			get
			{
				return m_uri;
			}
		}

		#endregion
		
		public ImageTileInfo(string imagePath)
		{
			m_imagePath = imagePath;
		}

		public ImageTileInfo(string imagePath, string uri)
		{
			m_imagePath = imagePath;
			m_uri = uri;
		}

		/// <summary>
		/// Check if this image tile is available locally.
		/// </summary>
		public bool Exists()
		{
			if (File.Exists(m_imagePath))
				return true;
			return false;
		}
	}
}
