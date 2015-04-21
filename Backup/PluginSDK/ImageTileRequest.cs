using System;
using System.IO;
using System.Net;
using WorldWind.Renderable;

namespace WorldWind.Net
{
	/// <summary>
	/// Image tile download request
	/// </summary>
	public class ImageTileRequest : GeoSpatialDownloadRequest
	{
		QuadTile m_quadTile;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Net.ImageTileRequest"/> class.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="quadTile"></param>
		public ImageTileRequest(object owner, QuadTile quadTile) : 
			base( owner, quadTile.ImageTileInfo.Uri )
		{
			m_quadTile = quadTile;
			download.DownloadType = DownloadType.Wms;
			SaveFilePath = QuadTile.ImageTileInfo.ImagePath;
		}

		/// <summary>
		/// Western bound of current request (decimal degrees)
		/// </summary>
		public override float West
		{
			get 
			{
				return (float)m_quadTile.West;
			}
		}

		/// <summary>
		/// Eastern bound of current request (decimal degrees)
		/// </summary>
		public override float East
		{
			get 
			{
				return (float)m_quadTile.East;
			}
		}

		/// <summary>
		/// Northern bound of current request (decimal degrees)
		/// </summary>
		public override float North
		{
			get
			{
				return (float)m_quadTile.North;
			}
		}

		/// <summary>
		/// Southern bound of current request (decimal degrees)
		/// </summary>
		public override float South
		{
			get 
			{
				return (float)m_quadTile.South;
			}
		}

		/// <summary>
		/// Image request color
		/// </summary>
		public override int Color
		{
			get
			{
				return World.Settings.downloadProgressColor;
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

		/// <summary>
		/// Tile download completed callback
		/// </summary>
		protected override void DownloadComplete()
		{
			try
			{
				download.Verify();

				if(download.SavedFilePath != null && File.Exists(download.SavedFilePath))
					// Rename from .xxx.tmp -> .xxx
					File.Move(download.SavedFilePath, SaveFilePath);

				// Make the quad tile reload the new image
				m_quadTile.isInitialized = false;
				QuadTile.DownloadRequest = null;
			}
			catch(WebException caught)
			{
				System.Net.HttpWebResponse response = caught.Response as System.Net.HttpWebResponse;
				if(response!=null && response.StatusCode==System.Net.HttpStatusCode.NotFound)
					FlagBadFile();
			}
			catch(IOException)
			{
				FlagBadFile();
			}	
		}

		/// <summary>
		/// Creates an empty file signalling the current request is for some reason permanently unavailable.
		/// </summary>
		void FlagBadFile()
		{
			// Flag the file as missing
			File.Create(SaveFilePath + ".txt");
			
			try
			{
				if(File.Exists(SaveFilePath))
					File.Delete(SaveFilePath);
			}
			catch 
			{
			}
		}

		/// <summary>
		/// Calculates the relative importance of this download by how 
		/// big a chunk of screen space (pixels) it occupies
		/// </summary>
		public override float CalculateScore()
		{
			float screenArea = QuadTile.BoundingBox.CalcRelativeScreenArea(QuadTile.QuadTileArgs.Camera);
			return screenArea;
		}
	}
}
