using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Net;
using WorldWind.Terrain;
using Utility;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Displays images on the globe (used by Rapid Fire MODIS)
	/// </summary>
	public class DownloadableImageFromIconSet : RenderableObject
	{
		#region Private Members
		World m_ParentWorld;
		float layerRadius;
		DrawArgs drawArgs;
		DownloadableIcon currentlyDisplayed;
		System.Collections.Hashtable textureHash = new System.Collections.Hashtable();
		TerrainAccessor _terrainAccessor;
		System.Collections.Hashtable downloadableIconList = new System.Collections.Hashtable();
		DownloadableIcon[] returnList = new DownloadableIcon[0];
		#endregion

		#region Properties
		public DownloadableIcon[] DownloadableIcons
		{
			get{ return returnList; }
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.DownloadableImageFromIconSet"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="worldCenter"></param>
		/// <param name="layerRadius"></param>
		/// <param name="drawArgs"></param>
		/// <param name="terrainAccessor"></param>
		public DownloadableImageFromIconSet(string name,
			World parentWorld, 
			float distanceAboveSurface,
			DrawArgs drawArgs,
			TerrainAccessor terrainAccessor) : base(name, parentWorld.Position, parentWorld.Orientation)
		{
			this.m_ParentWorld = parentWorld;
			this.layerRadius = (float)parentWorld.EquatorialRadius + distanceAboveSurface;
			this.isSelectable = true;
			this.drawArgs = drawArgs;
			this._terrainAccessor = terrainAccessor;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.isInitialized = true;
		}

		public void AddDownloadableIcon(string name, float altitude, 
			float west, float south, float east, float north, string imageUrl, string saveTexturePath, string iconFilePath, int iconSize, string caption)
		{
			Texture t = null;
			
			if(!this.textureHash.Contains(iconFilePath))
			{
				t = ImageHelper.LoadTexture(iconFilePath);
				lock(this.textureHash.SyncRoot)
				{
					this.textureHash.Add(iconFilePath, t);
				}
			}
			else
			{
				t = (Texture)this.textureHash[iconFilePath];
			}
			DownloadableIcon di = new DownloadableIcon(name, m_ParentWorld, 
				this.layerRadius - (float)m_ParentWorld.EquatorialRadius + altitude, west, south, east, north,
				imageUrl, saveTexturePath, t, iconSize, caption, this._terrainAccessor);
			di.IsOn = true;
			di.IconFilePath = iconFilePath;

			lock(this.downloadableIconList.SyncRoot)
			{
				if(!this.downloadableIconList.Contains(di.Name))
					this.downloadableIconList.Add(di.Name, di);

				this.returnList = new DownloadableIcon[this.downloadableIconList.Count];
				int c = 0;
				foreach(DownloadableIcon dicon in this.downloadableIconList.Values)
				{
					this.returnList[c++] = dicon;
				}

			}
		}

		public void LoadDownloadableIcon(string name)
		{
			DownloadableIcon di = null;
			lock(this.downloadableIconList.SyncRoot)
			{
				if(this.downloadableIconList.Contains(name))
				{
					di = (DownloadableIcon)this.downloadableIconList[name];
				}
			}
			if(di != null)
			{
				if(this.currentlyDisplayed != null)
					this.currentlyDisplayed.Dispose();

				di.DownLoadImage(drawArgs);
				this.currentlyDisplayed = di;
			}
		}

		public void RemoveDownloadableIcon(string name)
		{
			try
			{
				DownloadableIcon di = null;
				lock(this.downloadableIconList.SyncRoot)
				{
					if(this.downloadableIconList.Contains(name))
					{
						di = (DownloadableIcon)this.downloadableIconList[name];
						this.downloadableIconList.Remove(name);
					}
					this.returnList = new DownloadableIcon[this.downloadableIconList.Count];
					int c = 0;
					foreach(DownloadableIcon dicon in this.downloadableIconList.Values)
					{
						this.returnList[c++] = dicon;
					}
				}
			
				if(this.currentlyDisplayed != null && name == this.currentlyDisplayed.Name)
				{
					this.currentlyDisplayed = null;
				}

				if(di != null)
					di.Dispose();
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
		}

		public override void Dispose()
		{
			this.isInitialized = false;
			lock(this.downloadableIconList.SyncRoot)
			{
				foreach(string key in this.downloadableIconList.Keys)
				{
					DownloadableIcon di = (DownloadableIcon)this.downloadableIconList[key];
					di.Dispose();
				}
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			if(this.ShowOnlyCurrentlySelected)
				return false;

			lock(this.downloadableIconList.SyncRoot)
			{
				foreach(string key in this.downloadableIconList.Keys)
				{
					if(this.currentlyDisplayed != null && key == this.currentlyDisplayed.Name)
						continue;
					DownloadableIcon di = (DownloadableIcon)this.downloadableIconList[key];
					if(di.WasClicked(drawArgs))
					{
						if(this.currentlyDisplayed != null)
						{
							this.currentlyDisplayed.LoadImage = false;
						}
						di.LoadImage = true;
						di.PerformSelectionAction(drawArgs);
						this.currentlyDisplayed = di;
						return true;
					}
				}
			}
			return false;
		}

		public bool ShowOnlyCurrentlySelected = false;

		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if(!this.isInitialized)
					this.Initialize(drawArgs);
				
				lock(this.downloadableIconList.SyncRoot)
				{
					foreach(DownloadableIcon di in this.downloadableIconList.Values)
					{
						if(!di.isInitialized)
							di.Initialize(drawArgs);
						di.Update(drawArgs);
					}
				}
			}
			catch
			{
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!this.ShowOnlyCurrentlySelected)
			{
				lock(this.downloadableIconList.SyncRoot)
				{
					foreach(DownloadableIcon di in this.downloadableIconList.Values)
						di.Render(drawArgs);
				}
			}
			else
			{
				if(this.currentlyDisplayed != null)
					this.currentlyDisplayed.Render(drawArgs);
			}
		}

		
	}
	/// <summary>
	/// Used by MODIS Icons
	/// </summary>
	public class DownloadableIcon : RenderableObject
	{
		Texture iconTexture;
		public bool LoadImage;
		public bool IsTextureAvailable;
		public ImageLayer imageLayer;
		float north;
		float south;
		float east;
		float west;
		float centerLat;
		float centerLon;
		float layerRadius;
		public Thread downloadThread;
		string imageUrl;
		string saveTexturePath;
		string caption;
		Sprite sprite;
		string iconFilePath = null;
		Rectangle spriteSize;
			
		int iconSize;
		DrawArgs drawArgs;
		TerrainAccessor _terrainAccessor;
		CustomVertex.TransformedColored[] progressBar = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[4];
		CustomVertex.TransformedColored[] progressBarOutline = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[5];
		static int progressDefaultColor =  System.Drawing.Color.Red.ToArgb();
		static int progressColorLoading = System.Drawing.Color.CornflowerBlue.ToArgb();
		static int progressColorConversion = System.Drawing.Color.YellowGreen.ToArgb();
		static int bottomLeftTextColor = System.Drawing.Color.Cyan.ToArgb();


		World m_ParentWorld;
		float downloadProgress;
		DownloadState downloadState = DownloadState.Pending;

		public int Width
		{
			get{ return iconSize / 2; }
		}
		public int Height
		{
			get{ return iconSize / 2; }
		}
		public float Latitude
		{
			get{ return centerLat;	}
		}
		public float Longitude
		{
			get{ return centerLon; }
		}
		public string IconFilePath
		{
			get{ return iconFilePath; }
			set{ iconFilePath = value; }
		}
		public float North
		{
			get{ return north; }
		}
		public float South
		{
			get{ return south; }
		}
		public float West
		{
			get{ return west; }
		}
		public float East
		{
			get{ return East; }
		}
		public string SaveTexturePath
		{
			get{ return saveTexturePath; }
		}

		public DownloadableIcon(string name,
			World parentWorld,
			float distanceAboveSurface, float west, float south, float east, float north, string imageUrl, string saveTexturePath, Texture iconTexture, int iconSize, string caption, TerrainAccessor terrainAccessor)
			: base(name, parentWorld.Position, parentWorld.Orientation)
		{
			this.imageUrl = imageUrl;
			this.saveTexturePath = saveTexturePath;
			this.iconTexture = iconTexture;
			this.iconSize = iconSize;

			this.north = north;
			this.south = south;
			this.west = west;
			this.east = east;
			this.caption = caption;

			this._terrainAccessor = terrainAccessor;

			this.centerLat = 0.5f*(this.north + this.south);
			this.centerLon = 0.5f*(this.west + this.east);

			this.m_ParentWorld = parentWorld;
			this.layerRadius = (float)parentWorld.EquatorialRadius + distanceAboveSurface;
			this.IsTextureAvailable = File.Exists(saveTexturePath);
			if(this.IsTextureAvailable)
				this.downloadProgress = 1.0f;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.drawArgs = drawArgs;

			using(Surface s = this.iconTexture.GetSurfaceLevel(0))
			{
				SurfaceDescription desc = s.Description;
				this.spriteSize = new Rectangle(0,0, desc.Width, desc.Height);
			}

			this.sprite = new Sprite(drawArgs.device);
			
			for (int i=0; i<progressBarOutline.Length; i++) 
				progressBarOutline[i].Z = 0.5f;
			for (int i=0; i<progressBar.Length; i++) 
				progressBar[i].Z = 0.5f;
	
			this.isInitialized = true;
		}

		public override void Dispose()
		{
			this.isInitialized = false;
			this.LoadImage = false;
			if(this.imageLayer != null)
				this.imageLayer.Dispose();

			this.downloadState = DownloadState.Cancelled;

			if(this.sprite != null)
				this.sprite.Dispose();
		}

		public bool WasClicked(DrawArgs drawArgs)
		{
			int halfIconWidth = (int)(0.3f * this.iconSize);
			int halfIconHeight = (int)(0.3f * this.iconSize);

			Vector3 projectedPoint = MathEngine.SphericalToCartesian(0.5f*(this.north + this.south), 0.5f*(this.west + this.east), this.layerRadius);
			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(projectedPoint))
				return false;
			Vector3 translationVector = new Vector3(
				(float)(projectedPoint.X - drawArgs.WorldCamera.ReferenceCenter.X),
				(float)(projectedPoint.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
				(float)(projectedPoint.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

			// Find closest mouse-over icon
			projectedPoint = drawArgs.WorldCamera.Project(translationVector);


			int top = (int)projectedPoint.Y - halfIconHeight;
			int bottom = (int)projectedPoint.Y + halfIconHeight;
			int left = (int)projectedPoint.X - halfIconWidth;
			int right = (int)projectedPoint.X + halfIconWidth;

			try
			{
				if(DrawArgs.LastMousePosition.X < right && DrawArgs.LastMousePosition.X > left && DrawArgs.LastMousePosition.Y > top && DrawArgs.LastMousePosition.Y < bottom)
				{
					return true;
				}
				else
					return false;
			}
			catch
			{
			}
			return false;
		}

		public bool DownLoadImage(DrawArgs drawArgs)
		{
			if(this.imageLayer != null || (this.downloadThread != null && this.downloadThread.IsAlive))
				return false;

			this.LoadImage = true;
			this.drawArgs = drawArgs;
			//download the thing...
			if(!this.IsTextureAvailable)
			{
				this.downloadThread = new Thread(new ThreadStart(this.DownloadImage));
				this.downloadThread.Name = "DownloadableImageFromIconSet.DownloadImage";
				this.downloadThread.IsBackground = true;
				this.downloadThread.Start();
			}
			else
			{
				this.imageLayer = new ImageLayer(this.Name, this.m_ParentWorld, 
					this.layerRadius - (float)m_ParentWorld.EquatorialRadius, this.saveTexturePath, this.south, this.north, this.west, this.east, 255, this._terrainAccessor);
			}
			return true;
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return DownLoadImage(drawArgs);
		}

		public void DownloadImage()
		{
			try
			{
				this.downloadState = DownloadState.Downloading;
				this.downloadProgress = 0.0f;

				if(File.Exists(this.saveTexturePath))
					File.Delete(this.saveTexturePath);

				if(!Directory.Exists(Path.GetDirectoryName(this.saveTexturePath)))
					Directory.CreateDirectory(Path.GetDirectoryName(this.saveTexturePath));
				using( WebDownload dl = new WebDownload(imageUrl))
				{
					dl.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
					dl.DownloadMemory();

					this.downloadState = DownloadState.Converting;
					ImageHelper.ConvertToDxt1(dl.ContentStream, saveTexturePath);
				}

				if(this.downloadState == DownloadState.Cancelled)
					return;

				this.IsTextureAvailable = true;
				if(this.LoadImage)
				{
					this.imageLayer = new ImageLayer(this.Name,
						m_ParentWorld,
						this.layerRadius - (float)m_ParentWorld.EquatorialRadius, this.saveTexturePath, this.south, this.north, this.west, this.east, 255, /*this.terrainInfo*/null);
				}
				this.downloadState = DownloadState.Pending;
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
		}

		void UpdateProgress(int current, int total)
		{
			downloadProgress = total>0 ? (float)current / total : 0;
		}

		public override void Update(DrawArgs drawArgs)
		{
			if(this.LoadImage && this.imageLayer != null && !this.imageLayer.isInitialized)
			{
				drawArgs.Repaint = true;
				this.imageLayer.Initialize(drawArgs);
			}

			if(!this.LoadImage && this.imageLayer != null && this.imageLayer.isInitialized)
			{
				drawArgs.Repaint = true;
				this.imageLayer.Dispose();
			}

			if(this.imageLayer != null && this.imageLayer.isInitialized)
			{
				this.imageLayer.Update(drawArgs);
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!this.isInitialized)
				return;

			if(this.imageLayer != null && this.imageLayer.isInitialized && this.LoadImage)
			{
				if(!this.imageLayer.DisableZBuffer)
					this.imageLayer.DisableZBuffer = true;

				this.imageLayer.Render(drawArgs);
				drawArgs.defaultDrawingFont.DrawText(null,this.caption, new System.Drawing.Rectangle(10, drawArgs.screenHeight - 50, drawArgs.screenWidth - 10, 50), DrawTextFormat.NoClip | DrawTextFormat.WordBreak, bottomLeftTextColor );
				return;
			}

			Vector3 centerPoint = MathEngine.SphericalToCartesian(0.5f*(this.north + this.south), 0.5f*(this.west + this.east), this.layerRadius);
			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(centerPoint))
				return;

			Vector3 translationVector = new Vector3(
				(float)(centerPoint.X - drawArgs.WorldCamera.ReferenceCenter.X),
				(float)(centerPoint.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
				(float)(centerPoint.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

			// Find closest mouse-over icon
			Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);

			// This value indicates the non-zoomed scale factor for icons
			const float baseScaling = 0.5f;

			// This value indicates the (maximum) added scale factor for when the mouse is over the icon
			float zoomScaling = 0.5f;

			// This value determines when the icon will start to zoom
			float selectionRadius = 0.5f * this.iconSize;

			float dx = DrawArgs.LastMousePosition.X - projectedPoint.X;
			float dy = DrawArgs.LastMousePosition.Y - projectedPoint.Y;
			float dr = (float)Math.Sqrt(dx*dx + dy*dy);

			bool renderDescription = false;
			if(dr > selectionRadius)
				zoomScaling = 0;
			else
			{
				zoomScaling *= (selectionRadius - dr) / selectionRadius;
				renderDescription = true;
				DrawArgs.MouseCursor = CursorType.Hand;
			}

			float scaleFactor = baseScaling + zoomScaling;
			int halfIconWidth = (int)(0.5f * this.iconSize * scaleFactor);
			int halfIconHeight = (int)(0.5f * this.iconSize * scaleFactor);

			if(this.downloadState != DownloadState.Pending)
			{
				halfIconWidth = (int)(0.5f * this.iconSize);
				halfIconHeight = (int)(0.5f * this.iconSize);
			}

			float scaleWidth = (float) 2.0f * halfIconWidth / this.spriteSize.Width;
			float scaleHeight = (float) 2.0f * halfIconHeight / this.spriteSize.Height;

			this.sprite.Begin(SpriteFlags.AlphaBlend);
			this.sprite.Transform = Matrix.Transformation2D(new Vector2(0.0f, 0.0f), 0.0f, new Vector2(scaleWidth, scaleHeight),
				new Vector2(0,0),
				0.0f, new Vector2(projectedPoint.X, projectedPoint.Y));

			this.sprite.Draw(this.iconTexture, this.spriteSize, new Vector3(1.32f*this.iconSize, 1.32f*this.iconSize, 0), new Vector3(0,0,0), System.Drawing.Color.White);
			this.sprite.End();

			if(this.caption != null && renderDescription)
			{
				drawArgs.defaultDrawingFont.DrawText(null, this.caption, 
					new Rectangle((int)projectedPoint.X + halfIconWidth + 5, (int)projectedPoint.Y - halfIconHeight, drawArgs.screenWidth, drawArgs.screenHeight),
					DrawTextFormat.WordBreak | DrawTextFormat.NoClip,
					System.Drawing.Color.White.ToArgb());
			}

			if(this.downloadState != DownloadState.Pending || this.IsTextureAvailable)
			{
				int progressColor = progressDefaultColor;
				if(this.IsTextureAvailable)
					progressColor = progressColorLoading;
				else if(this.downloadState == DownloadState.Converting)
				{
					progressColor = progressColorConversion;
					if(System.DateTime.Now.Millisecond < 500)
						return;
				}
					
				progressBarOutline[0].X = projectedPoint.X - halfIconWidth;
				progressBarOutline[0].Y = projectedPoint.Y + halfIconHeight + 1;
				progressBarOutline[0].Color = progressColor;

				progressBarOutline[1].X = projectedPoint.X + halfIconWidth;
				progressBarOutline[1].Y = projectedPoint.Y + halfIconHeight + 1;
				progressBarOutline[1].Color = progressColor;
					
				progressBarOutline[2].X = projectedPoint.X + halfIconWidth;
				progressBarOutline[2].Y = projectedPoint.Y + halfIconHeight + 3;
				progressBarOutline[2].Color = progressColor;
					
				progressBarOutline[3].X = projectedPoint.X - halfIconWidth;
				progressBarOutline[3].Y = projectedPoint.Y + halfIconHeight + 3;
				progressBarOutline[3].Color = progressColor;

				progressBarOutline[4].X = projectedPoint.X - halfIconWidth;
				progressBarOutline[4].Y = projectedPoint.Y + halfIconHeight + 1;
				progressBarOutline[4].Color = progressColor;

				drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, progressBarOutline);
				
				int barlength = (int)(downloadProgress * 2 * halfIconWidth);

				progressBar[0].X = projectedPoint.X - halfIconWidth;
				progressBar[0].Y = projectedPoint.Y + halfIconHeight + 1;
				progressBar[0].Color = progressColor;

				progressBar[1].X = projectedPoint.X - halfIconWidth;
				progressBar[1].Y = projectedPoint.Y + halfIconHeight + 3;
				progressBar[1].Color = progressColor;
					
				progressBar[2].X = projectedPoint.X - halfIconWidth + barlength;
				progressBar[2].Y = projectedPoint.Y + halfIconHeight + 1;
				progressBar[2].Color = progressColor;
					
				progressBar[3].X = projectedPoint.X - halfIconWidth + barlength;
				progressBar[3].Y = projectedPoint.Y + halfIconHeight + 3;
				progressBar[3].Color = progressColor;

				drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, progressBar);
			}
		}

		private void nt_ProgressCallback(int bytesRead, int totalBytes)
		{
			this.downloadProgress = (float)bytesRead / totalBytes;
			this.drawArgs.Repaint = true;
		}
	}

	enum DownloadState
	{
		Pending,
		Downloading,
		Converting,
		Cancelled,
	}
}
