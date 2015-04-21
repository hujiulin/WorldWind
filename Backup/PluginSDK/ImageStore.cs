using Microsoft.DirectX.Direct3D;
using WorldWind.Net.Wms;
using WorldWind.Renderable;
using System;
using System.IO;

namespace WorldWind
{
	/// <summary>
	/// Base class for calculating local image paths and remote download urls
	/// </summary>
	public class ImageStore
	{
		#region Private Members

		protected string	m_dataDirectory;
		protected double m_levelZeroTileSizeDegrees = 36;
		protected int m_levelCount = 1;
		protected string m_imageFileExtension;
		protected string m_cacheDirectory;
		protected string m_duplicateTexturePath;
        protected string m_serverlogo;

		#endregion

		#region Properties

		/// <summary>
		/// Coverage of outer level 0 bitmaps (decimal degrees)
		/// Level 1 has half the coverage, level 2 half of level 1 (1/4) etc.
		/// </summary>
		public double LevelZeroTileSizeDegrees
		{
			get
			{
				return m_levelZeroTileSizeDegrees;
			}
			set
			{
				m_levelZeroTileSizeDegrees = value;
			}
		}
        
        /// <summary>
        /// Server Logo path for Downloadable layers
        /// </summary>
        public string ServerLogo
        {
            get
            {
                return m_serverlogo;
            }
            set
            {
                m_serverlogo = value;
            }
        }

		/// <summary>
		/// Number of detail levels
		/// </summary>
		public int LevelCount
		{
			get
			{
				return m_levelCount;
			}
			set
			{
				m_levelCount = value;
			}
		}

		/// <summary>
		/// File extension of the source image file format
		/// </summary>
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

		/// <summary>
		/// Cache subdirectory for this layer
		/// </summary>
		public string CacheDirectory
		{
			get
			{
				return m_cacheDirectory;
			}
			set
			{
				m_cacheDirectory = value;
			}
		}

		/// <summary>
		/// Data directory for this layer (permanently stored images)
		/// </summary>
		public string DataDirectory
		{
			get
			{
				return m_dataDirectory;
			}
			set
			{
				m_dataDirectory = value;
			}
		}

		/// <summary>
		/// Default texture to be used (always ocean?)
		/// Can be either file or url
		/// </summary>
		public string DuplicateTexturePath
		{
			get
			{
				return m_duplicateTexturePath;
			}
			set
			{
				m_duplicateTexturePath = value;
			}
		}

		public virtual bool IsDownloadableLayer
		{
			get
			{
				return false;
			}
		}

		#endregion

		public virtual string GetLocalPath(QuadTile qt)
		{
			if(qt.Level >= m_levelCount)
				throw new ArgumentException(string.Format("Level {0} not available.", 
					qt.Level));

			string relativePath = String.Format(@"{0}\{1:D4}\{1:D4}_{2:D4}.{3}", 
				qt.Level, qt.Row, qt.Col, m_imageFileExtension);
			
			if(m_dataDirectory != null)
			{
				// Search data directory first
				string rawFullPath = Path.Combine( m_dataDirectory, relativePath );
				if(File.Exists(rawFullPath))
					return rawFullPath;
			}

            // If cache doesn't exist, fall back to duplicate texture path.
            if (m_cacheDirectory == null)
                return m_duplicateTexturePath;
	
			// Try cache with default file extension
			string cacheFullPath = Path.Combine( m_cacheDirectory, relativePath );
			if(File.Exists(cacheFullPath))
				return cacheFullPath;

			// Try cache but accept any valid image file extension
			const string ValidExtensions = ".bmp.dds.dib.hdr.jpg.jpeg.pfm.png.ppm.tga.gif.tif";
			
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

					return imageFile;
				}
			}

			return cacheFullPath;
		}

		/// <summary>
		/// Figure out how to download the image.
		/// TODO: Allow subclasses to have control over how images are downloaded, 
		/// not just the download url.
		/// </summary>
		protected virtual string GetDownloadUrl(QuadTile qt)
		{
			// No local image, return our "duplicate" tile if any
			if(m_duplicateTexturePath != null && File.Exists(m_duplicateTexturePath))
				return m_duplicateTexturePath;

			// No image available anywhere, give up
			return "";
		}

		/// <summary>
		/// Deletes the cached copy of the tile.
		/// </summary>
		/// <param name="qt"></param>
		public virtual void DeleteLocalCopy(QuadTile qt)
		{
			string filename = GetLocalPath(qt);
			if(File.Exists(filename))
				File.Delete(filename);
		}


		/// <summary>
		/// Converts image file to DDS
		/// </summary>
		protected virtual void ConvertImage(Texture texture, string filePath)
		{
			if(filePath.ToLower().EndsWith(".dds"))
				// Image is already DDS
				return;

			// User has selected to convert downloaded images to DDS
			string convertedPath = Path.Combine(
				Path.GetDirectoryName(filePath),
				Path.GetFileNameWithoutExtension(filePath)+".dds");

			TextureLoader.Save(convertedPath, ImageFileFormat.Dds, texture );

			// Delete the old file
			try
			{
				File.Delete(filePath);
			}
			catch
			{
			}
		}

		public Texture LoadFile(QuadTile qt)
		{
            string filePath = GetLocalPath(qt);
			qt.ImageFilePath = filePath;
			if(!File.Exists(filePath))
			{
				string badFlag = filePath + ".txt";
				if(File.Exists(badFlag))
				{
					FileInfo fi = new FileInfo(badFlag);
					if(DateTime.Now - fi.LastWriteTime < TimeSpan.FromDays(1))
					{
						return null;
					}
					// Timeout period elapsed, retry
					File.Delete(badFlag);
				}

				if(IsDownloadableLayer)
				{
                    QueueDownload(qt, filePath);
					return null;
				}

				if(DuplicateTexturePath == null)
					// No image available, neither local nor online.
					return null;

                filePath = DuplicateTexturePath;
			}

			// Use color key
			Texture texture = null;
			if(qt.QuadTileSet.HasTransparentRange)
			{
				texture = ImageHelper.LoadTexture( filePath, qt.QuadTileSet.ColorKey, 
					qt.QuadTileSet.ColorKeyMax);
			}
			else
			{
				texture = ImageHelper.LoadTexture( filePath, qt.QuadTileSet.ColorKey);
			}

			if(qt.QuadTileSet.CacheExpirationTime != TimeSpan.MaxValue)
			{
				FileInfo fi = new FileInfo(filePath);
				DateTime expiry = fi.LastWriteTimeUtc.Add(qt.QuadTileSet.CacheExpirationTime);
				if(DateTime.UtcNow > expiry)
					QueueDownload(qt, filePath);
			}

            // Only convert images that are downloadable (don't mess with things the user put here!)
			if(World.Settings.ConvertDownloadedImagesToDds && IsDownloadableLayer)
				ConvertImage(texture, filePath);

			return texture;
		}

		void QueueDownload(QuadTile qt, string filePath)
		{
			string url = GetDownloadUrl(qt);
			qt.QuadTileSet.AddToDownloadQueue(qt.QuadTileSet.Camera, 
				new GeoSpatialDownloadRequest(qt, this, filePath, url));
		}
	}
}
