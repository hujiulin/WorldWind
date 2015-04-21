using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using WorldWind.Configuration;
using WorldWind.Net;

using ICSharpCode.SharpZipLib.Zip;

namespace WorldWind.Terrain
{
	/// <summary>
	/// Terrain tile download request
	/// </summary>
	public class TerrainDownloadRequest : GeoSpatialDownloadRequest
	{
		public TerrainTile TerrainTile;

        const string ContentTypeZip = "application/zip";
		const string ContentType7z = "application/x-7z-compressed";
		const string ContentTypeXCompressed = "application/x-compressed";

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Terrain.TerrainDownloadRequest"/> class.
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="owner"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="targetLevel"></param>
		public TerrainDownloadRequest(TerrainTile tile, TerrainTileService owner, int row, int col, int targetLevel) : base(owner)
		{
			TerrainTile = tile;
			download.Url = String.Format(CultureInfo.InvariantCulture,
				"{0}?T={1}&L={2}&X={3}&Y={4}",
				owner.ServerUrl,
				owner.DataSet,
				targetLevel, col, row );
		}

		/// <summary>
		/// Western bound of current request (decimal degrees)
		/// </summary>
		public override float West
		{
			get 
			{
				return (float)TerrainTile.West;
			}
		}

		/// <summary>
		/// Eastern bound of current request (decimal degrees)
		/// </summary>
		public override float East
		{
			get 
			{
				return (float)TerrainTile.East;
			}
		}

		/// <summary>
		/// Northern bound of current request (decimal degrees)
		/// </summary>
		public override float North
		{
			get
			{
				return (float)TerrainTile.North;
			}
		}

		/// <summary>
		/// Southern bound of current request (decimal degrees)
		/// </summary>
		public override float South
		{
			get 
			{
				return (float)TerrainTile.South;
			}
		}
		
		/// <summary>
		/// Terrain request color
		/// </summary>
		public override int Color
		{
			get
			{
				return World.Settings.DownloadTerrainRectangleColor.ToArgb();
			}
		}

		protected void ProcessFile()
		{
            if (download.ContentType == ContentTypeZip)
            {
                string compressedPath = download.SavedFilePath + ".zip";

                DirectoryInfo tempDirectory = new DirectoryInfo(Path.GetDirectoryName(compressedPath) + "\\" + System.DateTime.Now.Ticks.ToString());
                tempDirectory.Create();

                string tempFullPath = Path.Combine(tempDirectory.FullName, Path.GetFileNameWithoutExtension(download.SavedFilePath));

                // Remove any old temporary file
                if (File.Exists(compressedPath))
                    File.Delete(compressedPath);
                
                if (File.Exists(tempFullPath))
                    File.Delete(tempFullPath);

                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);

                File.Move(download.SavedFilePath, compressedPath);

                FastZip fastZip = new FastZip();
                fastZip.ExtractZip(
                    compressedPath,
                    tempDirectory.FullName,
                    "");

                File.Move(tempFullPath, SaveFilePath);

                try
                {
                    File.Delete(compressedPath);
                    tempDirectory.Delete();
                }
                catch { }
                
            }
            else if (download.ContentType == ContentType7z || download.ContentType == ContentTypeXCompressed) 
			{
				// Decompress 7z
				string compressedPath = download.SavedFilePath + ".7z";

				// Parent of destination
				string tempDirectory = Path.GetDirectoryName(Path.GetDirectoryName(download.SavedFilePath));
				string tempFullPath = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension( download.SavedFilePath ) );

				// Remove any old temporary file
				if(File.Exists(compressedPath))
					File.Delete(compressedPath);
				if(File.Exists(tempFullPath))
					File.Delete(tempFullPath);

				File.Move(download.SavedFilePath, compressedPath);

				ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\7za.exe");
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;
				psi.Arguments = String.Format(CultureInfo.InvariantCulture,
					" x -y -o\"{1}\" \"{0}\"", compressedPath, tempDirectory);

				using( Process p = Process.Start(psi) )
				{
					p.WaitForExit();
					if(p.ExitCode!=0)
						throw new ApplicationException(string.Format("7z decompression of file '{0}' failed.", compressedPath));
				}

				File.Delete(compressedPath);
				File.Move(tempFullPath, SaveFilePath);
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
				ProcessFile();
			}
			catch(FileNotFoundException)
			{
				FlagBadTile();
			}
			catch(WebException caught)
			{
				HttpWebResponse response = caught.Response as HttpWebResponse;
				if(response!=null && response.StatusCode==HttpStatusCode.NotFound)
				{
					FlagBadTile();
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Download tile in foreground
		/// </summary>
		public void DownloadInForeground()
		{
			try
			{
				download.DownloadFile(download.SavedFilePath);
				ProcessFile();
			}
			catch(FileNotFoundException)
			{
				FlagBadTile();
			}
			catch(WebException caught)
			{
				HttpWebResponse response = caught.Response as HttpWebResponse;
				if(response!=null && response.StatusCode==HttpStatusCode.NotFound)
				{
					FlagBadTile();
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Creates an empty file signalling the current request is for some reason permanently unavailable.
		/// </summary>
		void FlagBadTile()
		{
			// Server says it doesn't have the file, so don't hammer the server with repeated requests.
			// Create an empty tile file on disk to 'flag' the missing file.
			// TerrainTile will handle removal of the empty file and reissuing a request to the server.
			using( Stream flagFile = File.Create(SaveFilePath) )
			{
			}
		}

		/// <summary>
		/// Calculates the relative importance of this download.
		/// </summary>
		public override float CalculateScore()
		{
			return 0;
		}
	}
}