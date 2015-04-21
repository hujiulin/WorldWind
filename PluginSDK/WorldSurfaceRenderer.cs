using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for SurfaceRenderer.
	/// </summary>
	public class WorldSurfaceRenderer
	{
		public const int RenderSurfaceSize = 256;
		
		#region Private Members
		RenderToSurface m_Rts = null;
		const int m_NumberRootTilesHigh = 5;
		uint m_SamplesPerTile;
		WorldWind.World m_ParentWorld;
		SurfaceTile[] m_RootSurfaceTiles;
		double m_DistanceAboveSeaLevel = 0;
		bool m_Initialized = false;
		System.Collections.ArrayList m_SurfaceImages = new System.Collections.ArrayList();
		System.Collections.Queue m_TextureLoadQueue = new System.Collections.Queue();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the surface images.
		/// </summary>
		/// <value></value>
		public System.Collections.ArrayList SurfaceImages
		{
			get
			{
				return m_SurfaceImages;
			}
		}
		
		/// <summary>
		/// Gets the distance above sea level in meters.
		/// </summary>
		/// <value></value>
		public double DistanceAboveSeaLevel
		{
			get
			{
				return m_DistanceAboveSeaLevel;
			}
		}
		
		/// <summary>
		/// Gets the samples per tile.  Also can be considered the Vertex Density or Mesh Density of each SurfaceTile
		/// </summary>
		/// <value></value>
		public uint SamplesPerTile
		{
			get
			{
				return m_SamplesPerTile;
			}
		}
		
		/// <summary>
		/// Gets the parent world.
		/// </summary>
		/// <value></value>
		public WorldWind.World ParentWorld
		{
			get
			{
				return m_ParentWorld;
			}
		}
		
		#endregion

		public WorldSurfaceRenderer(
			uint samplesPerTile,
			double distanceAboveSeaLevel,
			WorldWind.World parentWorld
			)
		{
			m_SamplesPerTile = samplesPerTile;
			m_ParentWorld = parentWorld;
			m_DistanceAboveSeaLevel = distanceAboveSeaLevel;

			double tileSize = 180.0f / m_NumberRootTilesHigh;

			m_RootSurfaceTiles = new SurfaceTile[m_NumberRootTilesHigh * (m_NumberRootTilesHigh * 2)];
			for(int i = 0; i < m_NumberRootTilesHigh; i++)
			{
				for(int j = 0; j < m_NumberRootTilesHigh * 2; j++)
				{
					m_RootSurfaceTiles[i * m_NumberRootTilesHigh * 2 + j] = new SurfaceTile(
						(i + 1) * tileSize - 90.0f,
						i * tileSize - 90.0f,
						j * tileSize - 180.0f,
						(j + 1) * tileSize - 180.0f,
						0,
						this);

				}
			}
		}

		public System.DateTime LastChange = System.DateTime.Now;

		public void AddSurfaceImage(SurfaceImage surfaceImage)
		{
			if(surfaceImage.ImageTexture != null)
			{
				lock(m_SurfaceImages.SyncRoot)
				{
					m_SurfaceImages.Add(surfaceImage);
					m_SurfaceImages.Sort();
				}

				LastChange = System.DateTime.Now;
			}
			else
			{
				lock(m_TextureLoadQueue.SyncRoot)
				{
					m_TextureLoadQueue.Enqueue(surfaceImage);
				}
			}
		}

		public RenderToSurface RenderToSurface
		{
			get
			{
				return m_Rts;
			}
		}

		Device m_Device = null;
		private void OnDeviceReset(object sender, EventArgs e)
		{
			Device dev = (Device)sender;

			m_Device = dev;

			m_Rts = new RenderToSurface(
				dev,
				RenderSurfaceSize,
				RenderSurfaceSize,
				Format.X8R8G8B8,
				true,
				DepthFormat.D16);
		}

		public void RemoveSurfaceImage(string imageResource)
		{
			try
			{
				lock(m_SurfaceImages.SyncRoot)
				{
					for(int i = 0; i < m_SurfaceImages.Count; i++)
					{
						SurfaceImage current = m_SurfaceImages[i] as SurfaceImage;
						if(current != null && current.ImageFilePath == imageResource)
						{
							m_SurfaceImages.RemoveAt(i);
							break;
						}
					}

					m_SurfaceImages.Sort();
				}
				LastChange = System.DateTime.Now;
			}
			catch(System.Threading.ThreadAbortException)
			{
			}
			catch(Exception ex)
			{
				Log.DebugWrite(ex);
			}
		}

		public int NumberTilesUpdated = 0;

		public void Dispose()
		{
			m_Initialized = false;
			if(m_Device != null)
			{
				m_Device.DeviceReset -= new EventHandler(OnDeviceReset);
			}

			if(m_Rts != null && !m_Rts.Disposed)
			{
				m_Rts.Dispose();
			}
			lock(m_RootSurfaceTiles.SyncRoot)
			{
				foreach(SurfaceTile st in m_RootSurfaceTiles)
					st.Dispose();
			}
		}

		public void Initialize(DrawArgs drawArgs)
		{
		//	drawArgs.device.DeviceReset += new EventHandler(OnDeviceReset);
		//	OnDeviceReset(drawArgs.device, null);
			foreach(SurfaceTile st in m_RootSurfaceTiles)
			{
				st.Initialize(drawArgs);
			}
			m_Initialized = true;
		}

		public void Update(DrawArgs drawArgs)
		{
			try
			{
				if(!m_Initialized)
				{
					Initialize(drawArgs);
				}

				foreach(SurfaceTile tile in m_RootSurfaceTiles)
				{
					tile.Update(drawArgs);
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}	

		public void RenderSurfaceImages(DrawArgs drawArgs)
		{
			if(this.m_Rts == null)
			{
				drawArgs.device.DeviceReset += new EventHandler(OnDeviceReset);
				OnDeviceReset(drawArgs.device, null);
			}
			if(!m_Initialized)
				return;

			if(m_TextureLoadQueue.Count > 0)
			{
				SurfaceImage si = m_TextureLoadQueue.Dequeue() as SurfaceImage;
				if(si != null)
				{
					si.ImageTexture = ImageHelper.LoadTexture( si.ImageFilePath );

					lock(this.m_SurfaceImages.SyncRoot)
					{
						m_SurfaceImages.Add(si);
						m_SurfaceImages.Sort();
					}
				}
				drawArgs.TexturesLoadedThisFrame++;
			}

			NumberTilesUpdated = 0;
			foreach(SurfaceTile tile in m_RootSurfaceTiles)
			{
				if(tile != null)
					tile.Render(drawArgs);
			}
		}
	}


}
