using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using Utility;

namespace WorldWind
{
	public class SurfaceTile : IDisposable
	{
		#region Private Members
		int m_Level;
		double m_North;
		double m_South;
		double m_West;
		double m_East;
		bool m_Initialized = false;
		Device m_Device = null;
		Texture m_RenderTexture = null;
	//	Surface m_RenderSurface = null;
	//	bool m_InitTexture = true;
	//	bool m_DeviceBindingMade = false;
		float[,] m_HeightData = null;
		CustomVertex.TransformedColoredTextured[] m_RenderToTextureVertices = new CustomVertex.TransformedColoredTextured[4];
		DynamicTexture m_DynamicTexture = null;
		bool m_RequiresUpdate = false;
		float m_VerticalExaggeration = float.NaN;

		System.DateTime m_LastUpdate = System.DateTime.Now;

		WorldSurfaceRenderer m_ParentWorldSurfaceRenderer;
		BoundingBox m_BoundingBox;
		short[] m_NwIndices = null;
		short[] m_NeIndices = null;
		short[] m_SwIndices = null;
		short[] m_SeIndices = null;

		SurfaceTile m_NorthWestChild;
		SurfaceTile m_NorthEastChild;
		SurfaceTile m_SouthWestChild;
		SurfaceTile m_SouthEastChild;

		short[] m_IndicesElevated;
		
		#endregion

		/// <summary>
		/// Creates a new <see cref="SurfaceTile"/> instance.
		/// </summary>
		/// <param name="north">North. (in degrees)</param>
		/// <param name="south">South. (in degrees)</param>
		/// <param name="west">West. (in degrees)</param>
		/// <param name="east">East. (in degrees)</param>
		/// <param name="level">Level.</param>
		/// <param name="parentWorldSurfaceRenderer">Parent world surface renderer.</param>
		public SurfaceTile(double north, double south, double west, double east, int level,
			WorldSurfaceRenderer parentWorldSurfaceRenderer)
		{
			m_North = north;
			m_South = south;
			m_West = west;
			m_East = east;
			m_Level = level;
			m_ParentWorldSurfaceRenderer = parentWorldSurfaceRenderer;
		
			float scale = 1.1f;

			Vector3 v = MathEngine.SphericalToCartesian(
				Angle.FromDegrees(south),
				Angle.FromDegrees(west),
				m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel);

			Vector3 v0 = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
			Vector3 v1 = Vector3.Scale(v0, scale);

			v = MathEngine.SphericalToCartesian(
				Angle.FromDegrees(south),
				Angle.FromDegrees(east),
				m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel);

			Vector3 v2 = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
			Vector3 v3 = Vector3.Scale(v2, scale);

			v = MathEngine.SphericalToCartesian(
				Angle.FromDegrees(north),
				Angle.FromDegrees(west),
				m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel);

			Vector3 v4 = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
			Vector3 v5 = Vector3.Scale(v4, scale);

			v = MathEngine.SphericalToCartesian(
				Angle.FromDegrees(north),
				Angle.FromDegrees(east),
				m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel);

			Vector3 v6 = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
			Vector3 v7 = Vector3.Scale(v6, scale);
			
			m_BoundingBox = new BoundingBox(v0, v1, v2, v3, v4, v5, v6, v7);

			int thisVertexDensityElevatedPlus2 = ((int)m_ParentWorldSurfaceRenderer.SamplesPerTile / 2 + 2);
			m_IndicesElevated = new short[2 * thisVertexDensityElevatedPlus2 * thisVertexDensityElevatedPlus2 * 3]; 

			for(int i = 0; i < thisVertexDensityElevatedPlus2; i++)
			{
				int elevated_idx = (2*3*i*thisVertexDensityElevatedPlus2);
				for(int j = 0; j < thisVertexDensityElevatedPlus2; j++)
				{
					m_IndicesElevated[elevated_idx] = (short)(i*(thisVertexDensityElevatedPlus2 + 1) + j);
					m_IndicesElevated[elevated_idx + 1] = (short)((i+1)*(thisVertexDensityElevatedPlus2 + 1) + j);
					m_IndicesElevated[elevated_idx + 2] = (short)(i*(thisVertexDensityElevatedPlus2 + 1) + j+1);

					m_IndicesElevated[elevated_idx + 3] = (short)(i*(thisVertexDensityElevatedPlus2 + 1) + j+1);
					m_IndicesElevated[elevated_idx + 4] = (short)((i+1)*(thisVertexDensityElevatedPlus2 + 1) + j);
					m_IndicesElevated[elevated_idx + 5] = (short)((i+1)*(thisVertexDensityElevatedPlus2 + 1) + j+1);

					elevated_idx += 6;
				}
			}
		}

		/// <summary>
		/// Gets the render texture.
		/// </summary>
		/// <value></value>
		public Texture RenderTexture
		{
			get
			{
				return m_RenderTexture;
			}
		}

		private void OnDeviceReset(object sender, EventArgs e)
		{
			Device dev = (Device)sender;

			m_Device = dev;

			m_RenderTexture = new Texture(
				dev,
				WorldSurfaceRenderer.RenderSurfaceSize,
				WorldSurfaceRenderer.RenderSurfaceSize,
				1,
				Usage.RenderTarget,
				Format.X8R8G8B8,
				Pool.Default);

			m_RequiresUpdate = true;
		}

		private void OnDeviceDispose(object sender, EventArgs e)
		{
			if(m_RenderTexture != null && !m_RenderTexture.Disposed)
			{
				m_RenderTexture.Dispose();
			}

		/*	if(m_RenderSurface != null && !m_RenderSurface.Disposed)
			{
				m_RenderSurface.Dispose();
			}*/
		}

		/// <summary>
		/// Initializes the specified draw args.
		/// </summary>
		/// <param name="drawArgs">DrawArgs.</param>
		public void Initialize(DrawArgs drawArgs)
		{
			try
			{
				if(m_HeightData == null)
				{
					if(m_Level > 2)
					{
						Terrain.TerrainTile tt = 
							m_ParentWorldSurfaceRenderer.ParentWorld.TerrainAccessor.GetElevationArray(
							m_North, 
							m_South, 
							m_West, 
							m_East, 
							(int)m_ParentWorldSurfaceRenderer.SamplesPerTile + 1);
						
						if(tt.ElevationData != null)
						{
							m_HeightData = tt.ElevationData;
						}
						else
						{
							m_HeightData = new float[(uint)m_ParentWorldSurfaceRenderer.SamplesPerTile + 1, (uint)m_ParentWorldSurfaceRenderer.SamplesPerTile + 1];
						}
					}
					else
					{	
						m_HeightData = new float[(uint)m_ParentWorldSurfaceRenderer.SamplesPerTile + 1, (uint)m_ParentWorldSurfaceRenderer.SamplesPerTile + 1];
					}
				}

				if(m_DynamicTexture == null)
				{
					m_DynamicTexture = new DynamicTexture();
					buildTerrainMesh();
				}
			}
			catch(Exception ex)
			{	
				Log.Write(ex);
			}

			m_Initialized = true;
		}
				
		/// <summary>
		/// Updates the render surface.
		/// </summary>
		/// <param name="drawArgs">Draw args.</param>
		public void UpdateRenderSurface(DrawArgs drawArgs)
		{
			if(m_RenderTexture == null)
			{
				drawArgs.device.DeviceReset += new EventHandler(OnDeviceReset);
				OnDeviceReset(drawArgs.device, null);
			}
		
			Viewport view = new Viewport();
			view.Width = WorldSurfaceRenderer.RenderSurfaceSize;
			view.Height = WorldSurfaceRenderer.RenderSurfaceSize;
			
			if(drawArgs.RenderWireFrame)
			{
				drawArgs.device.RenderState.FillMode = FillMode.Solid;
			}
			using(Surface renderSurface = m_RenderTexture.GetSurfaceLevel(0))
			{
				m_ParentWorldSurfaceRenderer.RenderToSurface.BeginScene(renderSurface, view);
					drawArgs.device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Black, 1.0f, 0);

					drawArgs.device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
					drawArgs.device.RenderState.ZBufferEnable = false;

					double latRange = m_North - m_South;
					double lonRange = m_East - m_West;

					lock(m_ParentWorldSurfaceRenderer.SurfaceImages.SyncRoot)
					{
						for(int i = 0; i < m_ParentWorldSurfaceRenderer.SurfaceImages.Count; i++)
						{
							SurfaceImage currentSurfaceImage = m_ParentWorldSurfaceRenderer.SurfaceImages[i] as SurfaceImage;
					
							if(currentSurfaceImage == null || 
								currentSurfaceImage.ImageTexture == null || 
								currentSurfaceImage.ImageTexture.Disposed || 
								!currentSurfaceImage.Enabled ||
								(currentSurfaceImage.North > m_North && currentSurfaceImage.South >= m_North) ||
								(currentSurfaceImage.North <= m_South && currentSurfaceImage.South < m_South) ||
								(currentSurfaceImage.West < m_West && currentSurfaceImage.East <= m_West) ||
								(currentSurfaceImage.West >= m_East && currentSurfaceImage.East > m_East)
								)
							{
								continue;
							}

							currentSurfaceImage.Opacity = currentSurfaceImage.ParentRenderable.Opacity;

							Vector2 tex = currentSurfaceImage.GetTextureCoordinate(m_North, m_West);
							m_RenderToTextureVertices[0].X = 0;
							m_RenderToTextureVertices[0].Y = 0;
							m_RenderToTextureVertices[0].Z = 0.0f;
							m_RenderToTextureVertices[0].Tu = tex.Y;
							m_RenderToTextureVertices[0].Tv = tex.X;
							m_RenderToTextureVertices[0].Color = System.Drawing.Color.FromArgb(
								currentSurfaceImage.ParentRenderable.Opacity,
								0,0,0).ToArgb();

							tex = currentSurfaceImage.GetTextureCoordinate(m_South, m_West);
							m_RenderToTextureVertices[1].X = 0;
							m_RenderToTextureVertices[1].Y = WorldSurfaceRenderer.RenderSurfaceSize;
							m_RenderToTextureVertices[1].Z = 0.0f;
							m_RenderToTextureVertices[1].Tu = tex.Y;
							m_RenderToTextureVertices[1].Tv = tex.X;
							m_RenderToTextureVertices[1].Color = System.Drawing.Color.FromArgb(
								currentSurfaceImage.ParentRenderable.Opacity,
								0,0,0).ToArgb();

							tex = currentSurfaceImage.GetTextureCoordinate(m_North, m_East);
							m_RenderToTextureVertices[2].X = WorldSurfaceRenderer.RenderSurfaceSize;
							m_RenderToTextureVertices[2].Y = 0;
							m_RenderToTextureVertices[2].Z = 0.0f;
							m_RenderToTextureVertices[2].Tu = tex.Y;
							m_RenderToTextureVertices[2].Tv = tex.X;
							m_RenderToTextureVertices[2].Color = System.Drawing.Color.FromArgb(
								currentSurfaceImage.ParentRenderable.Opacity,
								0,0,0).ToArgb();

							tex = currentSurfaceImage.GetTextureCoordinate(m_South, m_East);
							m_RenderToTextureVertices[3].X = WorldSurfaceRenderer.RenderSurfaceSize;
							m_RenderToTextureVertices[3].Y = WorldSurfaceRenderer.RenderSurfaceSize;
							m_RenderToTextureVertices[3].Z = 0.0f;
							m_RenderToTextureVertices[3].Tu = tex.Y;
							m_RenderToTextureVertices[3].Tv = tex.X;
							m_RenderToTextureVertices[3].Color = System.Drawing.Color.FromArgb(
								currentSurfaceImage.ParentRenderable.Opacity,
								0,0,0).ToArgb();
						
							drawArgs.device.RenderState.AlphaBlendEnable = true;
							drawArgs.device.RenderState.SourceBlend = Blend.SourceAlpha;
							drawArgs.device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

							drawArgs.device.SamplerState[0].BorderColor = System.Drawing.Color.FromArgb(0,0,0,0);
							drawArgs.device.SamplerState[1].BorderColor = System.Drawing.Color.FromArgb(0,0,0,0);
							drawArgs.device.SetTexture(0, currentSurfaceImage.ImageTexture);
							drawArgs.device.SetTexture(1, currentSurfaceImage.ImageTexture);
							drawArgs.device.TextureState[1].TextureCoordinateIndex = 0;
							
							drawArgs.device.SamplerState[0].MinFilter = TextureFilter.Linear;
							drawArgs.device.SamplerState[0].MagFilter = TextureFilter.Linear;
							drawArgs.device.SamplerState[0].AddressU = TextureAddress.Clamp;
							drawArgs.device.SamplerState[0].AddressV = TextureAddress.Clamp;
					
							drawArgs.device.SamplerState[1].MinFilter = TextureFilter.Point;
							drawArgs.device.SamplerState[1].MagFilter = TextureFilter.Point;
							drawArgs.device.SamplerState[1].AddressU = TextureAddress.Border;
							drawArgs.device.SamplerState[1].AddressV = TextureAddress.Border;

							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
							drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
							drawArgs.device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
							drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
							drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;
							drawArgs.device.TextureState[0].AlphaArgument2 = TextureArgument.TextureColor;

							drawArgs.device.TextureState[1].ColorOperation = TextureOperation.SelectArg1;						
							drawArgs.device.TextureState[1].ColorArgument1 = TextureArgument.Current;
							drawArgs.device.TextureState[1].AlphaOperation = TextureOperation.Modulate;
							drawArgs.device.TextureState[1].AlphaArgument1 = TextureArgument.TextureColor;
							drawArgs.device.TextureState[1].AlphaArgument2 = TextureArgument.Diffuse;
							
							drawArgs.device.TextureState[2].ColorOperation = TextureOperation.Disable;
							drawArgs.device.TextureState[2].AlphaOperation = TextureOperation.Disable;

							drawArgs.device.DrawUserPrimitives(
								PrimitiveType.TriangleStrip,
								2,
								m_RenderToTextureVertices);

							drawArgs.device.SetTexture(0, null);
							drawArgs.device.SetTexture(1, null);

                            drawArgs.device.SetTextureStageState(1, TextureStageStates.TextureCoordinateIndex, 1);
						}
						m_ParentWorldSurfaceRenderer.RenderToSurface.EndScene(Filter.Box);
						drawArgs.device.RenderState.ZBufferEnable = true;
					}
			}

			if(drawArgs.RenderWireFrame)
			{
				drawArgs.device.RenderState.FillMode = FillMode.WireFrame;
			}
			m_LastUpdate = System.DateTime.Now;
			m_RequiresUpdate = false;
		}

		private void ComputeChildrenTiles(DrawArgs drawArgs)
		{
			if(m_NorthWestChild == null)
			{
				SurfaceTile nwc = new SurfaceTile(
					m_North,
					0.5f*(m_South+m_North),
					m_West,
					0.5f*(m_West+m_East),
					m_Level + 1,
					m_ParentWorldSurfaceRenderer
					);
			
				nwc.Initialize(drawArgs);
				m_NorthWestChild = nwc;
			}

			if(m_NorthEastChild == null)
			{
				SurfaceTile nec = new SurfaceTile(
					m_North,
					0.5f*(m_South+m_North),
					0.5f*(m_West+m_East),
					m_East,
					m_Level + 1,
					m_ParentWorldSurfaceRenderer
					);
			
				nec.Initialize(drawArgs);
				m_NorthEastChild = nec;
			}

			if(m_SouthWestChild == null)
			{
				SurfaceTile swc = new SurfaceTile(
					0.5f*(m_South+m_North),
					m_South,
					m_West,
					0.5f*(m_West+m_East),
					m_Level + 1,
					m_ParentWorldSurfaceRenderer
					);
			
				swc.Initialize(drawArgs);
				m_SouthWestChild = swc;
			}

			if(m_SouthEastChild == null)
			{
				SurfaceTile sec = new SurfaceTile(
					0.5f*(m_South+m_North),
					m_South,
					0.5f*(m_West+m_East),
					m_East,
					m_Level + 1,
					m_ParentWorldSurfaceRenderer
					);
				
				sec.Initialize(drawArgs);
				m_SouthEastChild = sec;
			}
		}
		
		private Vector2 GetTextureCoordinate(SurfaceImage surfaceImage, double latitude, double longitude)
		{
			double deltaLat = surfaceImage.North - latitude;
			double deltaLon = longitude - surfaceImage.West;

			double latRange = surfaceImage.North - surfaceImage.South;
			double lonRange = surfaceImage.East - surfaceImage.West;

			Vector2 v = new Vector2(
				(float)(deltaLat / latRange),
				(float)(deltaLon / lonRange));

			return v;
		}

		/// <summary>
		/// Builds the terrain mesh. Also re-builds the terrain mesh, such as when vertical exaggeration has changed.
		/// </summary>
		private void buildTerrainMesh()
		{
			int thisVertexDensityElevatedPlus3 = ((int)m_ParentWorldSurfaceRenderer.SamplesPerTile / 2 + 3);

			double scaleFactor = (float)1/(m_ParentWorldSurfaceRenderer.SamplesPerTile);
			double latrange = (float)Math.Abs(m_North - m_South );
			double lonrange = (float)Math.Abs(m_East - m_West );

			m_DynamicTexture.nwVerts = new CustomVertex.PositionNormalTextured[thisVertexDensityElevatedPlus3 * thisVertexDensityElevatedPlus3];
			m_DynamicTexture.neVerts = new CustomVertex.PositionNormalTextured[thisVertexDensityElevatedPlus3 * thisVertexDensityElevatedPlus3];
			m_DynamicTexture.swVerts = new CustomVertex.PositionNormalTextured[thisVertexDensityElevatedPlus3 * thisVertexDensityElevatedPlus3];
			m_DynamicTexture.seVerts = new CustomVertex.PositionNormalTextured[thisVertexDensityElevatedPlus3 * thisVertexDensityElevatedPlus3];

			int base_idx;
			for(int i = 0; i < thisVertexDensityElevatedPlus3; i++)
			{
				base_idx = i*thisVertexDensityElevatedPlus3;
                    
				for(int j = 0; j < thisVertexDensityElevatedPlus3; j++)
				{
					float height = -30000;
					if(i == 0 || j == 0 || i == thisVertexDensityElevatedPlus3 - 1 || j == thisVertexDensityElevatedPlus3 - 1)
					{
						double latitude = 0;
						double longitude = 0;
						float tu = 0.0f;
						float tv = 0.0f;

						if(j == 0)
						{
							longitude = m_West;
							tu = 0.0f;
						}
						else if(j == thisVertexDensityElevatedPlus3 - 1)
						{
							longitude = 0.5*(m_West + m_East);
							tu = 0.5f;
						}
						else
						{
							longitude = m_West + (float)scaleFactor*lonrange*(j-1);
							tu = (float)(scaleFactor * (j - 1));
						}

						if(i == 0)
						{
							latitude = m_North;
							tv = 0.0f;
						}
						else if(i == thisVertexDensityElevatedPlus3 - 1)
						{
							latitude = 0.5*(m_North + m_South);
							tv = 0.5f;
						}
						else
						{
							latitude = m_North - scaleFactor*latrange*(i-1);
							tv = (float)(scaleFactor * (i - 1));
						}
						
						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(latitude),
							Angle.FromDegrees(longitude),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.nwVerts[base_idx].X = v.X;
						m_DynamicTexture.nwVerts[base_idx].Y = v.Y;
						m_DynamicTexture.nwVerts[base_idx].Z = v.Z;
						m_DynamicTexture.nwVerts[base_idx].Tu = tu;
						m_DynamicTexture.nwVerts[base_idx].Tv = tv;


					}
					else
					{

						height = (float)m_HeightData[(i - 1), (j - 1)];

						height *= World.Settings.VerticalExaggeration;

						
						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(m_North - scaleFactor * latrange * (i - 1)),
							Angle.FromDegrees(m_West + scaleFactor * lonrange * (j - 1)),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.nwVerts[base_idx].X = v.X;
						m_DynamicTexture.nwVerts[base_idx].Y = v.Y;
						m_DynamicTexture.nwVerts[base_idx].Z = v.Z;
						m_DynamicTexture.nwVerts[base_idx].Tu = (float)(scaleFactor * (j - 1));
						m_DynamicTexture.nwVerts[base_idx].Tv = (float)(scaleFactor * (i - 1));
					}

					base_idx += 1;
				}
			}

			for(int i = 0; i < thisVertexDensityElevatedPlus3; i++)
			{
				base_idx = i*thisVertexDensityElevatedPlus3;
                    
				for(int j = 0; j < thisVertexDensityElevatedPlus3; j++)
				{
					float height = -30000;
					if(i == 0 || j == 0 || i == thisVertexDensityElevatedPlus3 - 1 || j == thisVertexDensityElevatedPlus3 - 1)
					{
						double latitude = 0;
						double longitude = 0;
						float tu = 0.0f;
						float tv = 0.0f;

						if(j == 0)
						{
							longitude = 0.5*(m_West + m_East);
							tu = 0.5f;
						}
						else if(j == thisVertexDensityElevatedPlus3 - 1)
						{
							longitude = m_East;
							tu = 1.0f;
						}
						else
						{
							longitude = 0.5*(m_West+m_East) + (float)scaleFactor*lonrange*(j-1);
							tu = (float)(0.5f + scaleFactor * (j - 1));
						}

						if(i == 0)
						{
							latitude = m_North;
							tv = 0.0f;
						}
						else if(i == thisVertexDensityElevatedPlus3 - 1)
						{
							latitude = 0.5*(m_North + m_South);
							tv = 0.5f;
						}
						else
						{
							latitude = m_North - scaleFactor*latrange*(i-1);
							tv = (float)(scaleFactor * (i - 1));
						}

						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(latitude),
							Angle.FromDegrees(longitude),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.neVerts[base_idx].X = v.X;
						m_DynamicTexture.neVerts[base_idx].Y = v.Y;
						m_DynamicTexture.neVerts[base_idx].Z = v.Z;
						m_DynamicTexture.neVerts[base_idx].Tu = tu;
						m_DynamicTexture.neVerts[base_idx].Tv = tv;
					}
					else
					{
						height = (float)m_HeightData[(i - 1), (j-1)+(m_ParentWorldSurfaceRenderer.SamplesPerTile / 2)];
						height *= World.Settings.VerticalExaggeration;
						 
						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(m_North - scaleFactor*latrange*(i-1)),
							Angle.FromDegrees(0.5f*(m_West+m_East) + (float)scaleFactor*lonrange*(j-1)),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.neVerts[base_idx].X = v.X;
						m_DynamicTexture.neVerts[base_idx].Y = v.Y;
						m_DynamicTexture.neVerts[base_idx].Z = v.Z;
						m_DynamicTexture.neVerts[base_idx].Tu = (float)(0.5f + scaleFactor * (j - 1));
						m_DynamicTexture.neVerts[base_idx].Tv = (float)(scaleFactor * (i - 1));
					}
					base_idx += 1;
				}
			}

			for(int i = 0; i < thisVertexDensityElevatedPlus3; i++)
			{
				base_idx = i*thisVertexDensityElevatedPlus3;
                    
				for(int j = 0; j < thisVertexDensityElevatedPlus3; j++)
				{
					float height = -30000;
					if(i == 0 || j == 0 || i == thisVertexDensityElevatedPlus3 - 1 || j == thisVertexDensityElevatedPlus3 - 1)
					{
						double latitude = 0;
						double longitude = 0;
						float tu = 0.0f;
						float tv = 0.0f;

						if(j == 0)
						{
							longitude = m_West;
							tu = 0.0f;
						}
						else if(j == thisVertexDensityElevatedPlus3 - 1)
						{
							longitude = 0.5*(m_West + m_East);
							tu = 0.5f;
						}
						else
						{
							longitude = m_West + (float)scaleFactor*lonrange*(j-1);
							tu = (float)(scaleFactor * (j - 1));
						}

						if(i == 0)
						{
							latitude = 0.5*(m_North + m_South);
							tv = 0.5f;
						}
						else if(i == thisVertexDensityElevatedPlus3 - 1)
						{
							latitude = m_South;
							tv = 1.0f;
						}
						else
						{
							latitude = 0.5*(m_North+m_South) - scaleFactor*latrange*(i-1);
							tv = (float)(0.5f * scaleFactor * (i - 1));
						}

						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(latitude),
							Angle.FromDegrees(longitude),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.swVerts[base_idx].X = v.X;
						m_DynamicTexture.swVerts[base_idx].Y = v.Y;
						m_DynamicTexture.swVerts[base_idx].Z = v.Z;
						m_DynamicTexture.swVerts[base_idx].Tu = tu;
						m_DynamicTexture.swVerts[base_idx].Tv = tv;
					}
					else
					{
						height = (float)m_HeightData[m_ParentWorldSurfaceRenderer.SamplesPerTile / 2 + (i - 1), (j-1)];
						height *= World.Settings.VerticalExaggeration;

						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(0.5*(m_North+m_South) - scaleFactor*latrange*(i-1)),
							Angle.FromDegrees(m_West + (float)scaleFactor*lonrange*(j-1)),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.swVerts[base_idx].X = v.X;
						m_DynamicTexture.swVerts[base_idx].Y = v.Y;
						m_DynamicTexture.swVerts[base_idx].Z = v.Z;
						m_DynamicTexture.swVerts[base_idx].Tu = (float)(scaleFactor * (j - 1));
						m_DynamicTexture.swVerts[base_idx].Tv = (float)(0.5f + scaleFactor * (i - 1));
					}
					base_idx += 1;
				}
			}

			for(int i = 0; i < thisVertexDensityElevatedPlus3; i++)
			{
				base_idx = i*thisVertexDensityElevatedPlus3;
                    
				for(int j = 0; j < thisVertexDensityElevatedPlus3; j++)
				{
					float height = -30000;
					if(i == 0 || j == 0 || i == thisVertexDensityElevatedPlus3 - 1 || j == thisVertexDensityElevatedPlus3 - 1)
					{
						double latitude = 0;
						double longitude = 0;
						float tu = 0.0f;
						float tv = 0.0f;

						if(j == 0)
						{
							longitude = 0.5*(m_West + m_East);
							tu = 0.5f;
						}
						else if(j == thisVertexDensityElevatedPlus3 - 1)
						{
							longitude = m_East;
							tu = 1.0f;
						}
						else
						{
							longitude = 0.5*(m_West+ m_East) + (float)scaleFactor*lonrange*(j-1);
							tu = (float)(0.5f + scaleFactor * (j - 1));
						}

						if(i == 0)
						{
							latitude = 0.5*(m_North + m_South);
							tv = 0.5f;
						}
						else if(i == thisVertexDensityElevatedPlus3 - 1)
						{
							latitude = m_South;
							tv = 1.0f;
						}
						else
						{
							latitude = 0.5*(m_North+m_South) - scaleFactor*latrange*(i-1);
							tv = (float)(0.5f + scaleFactor * (i - 1));
						}

						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(latitude),
							Angle.FromDegrees(longitude),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.seVerts[base_idx].X = v.X;
						m_DynamicTexture.seVerts[base_idx].Y = v.Y;
						m_DynamicTexture.seVerts[base_idx].Z = v.Z;
						m_DynamicTexture.seVerts[base_idx].Tu = tu;
						m_DynamicTexture.seVerts[base_idx].Tv = tv;
					}
					else
					{
						height = (float)m_HeightData[m_ParentWorldSurfaceRenderer.SamplesPerTile / 2 + (i - 1), (j-1)+(m_ParentWorldSurfaceRenderer.SamplesPerTile / 2)];
								
						height *= World.Settings.VerticalExaggeration;

						Vector3 v = MathEngine.SphericalToCartesian(
							Angle.FromDegrees(0.5*(m_North+m_South) - scaleFactor*latrange*(i-1)),
							Angle.FromDegrees(0.5*(m_West+ m_East) + (float)scaleFactor*lonrange*(j-1)),
							m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius + m_ParentWorldSurfaceRenderer.DistanceAboveSeaLevel + height);

						m_DynamicTexture.seVerts[base_idx].X = v.X;
						m_DynamicTexture.seVerts[base_idx].Y = v.Y;
						m_DynamicTexture.seVerts[base_idx].Z = v.Z;
						m_DynamicTexture.seVerts[base_idx].Tu = (float)(0.5f + scaleFactor * (j - 1));
						m_DynamicTexture.seVerts[base_idx].Tv = (float)(0.5f + scaleFactor * (i - 1));
					}
					base_idx += 1;
				}
				
			}
			double equatoralRadius = m_ParentWorldSurfaceRenderer.ParentWorld.EquatorialRadius;

			try
			{
				if(m_VerticalExaggeration != World.Settings.VerticalExaggeration)
				{
				//	m_NwIndices = CreateTriangleIndicesBTT(m_DynamicTexture.nwVerts, (int)m_ParentWorldSurfaceRenderer.SamplesPerTile/2, 1, equatoralRadius); 
				//	m_NeIndices = CreateTriangleIndicesBTT(m_DynamicTexture.neVerts, (int)m_ParentWorldSurfaceRenderer.SamplesPerTile/2, 1, equatoralRadius); 
				//	m_SwIndices = CreateTriangleIndicesBTT(m_DynamicTexture.swVerts, (int)m_ParentWorldSurfaceRenderer.SamplesPerTile/2, 1, equatoralRadius); 
				//	m_SeIndices = CreateTriangleIndicesBTT(m_DynamicTexture.seVerts, (int)m_ParentWorldSurfaceRenderer.SamplesPerTile/2, 1, equatoralRadius); 
				
				//	calculate_normals(ref m_DynamicTexture.nwVerts, m_IndicesElevated);
				//	calculate_normals(ref m_DynamicTexture.neVerts, m_IndicesElevated);
				//	calculate_normals(ref m_DynamicTexture.swVerts, m_IndicesElevated);
				//	calculate_normals(ref m_DynamicTexture.seVerts, m_IndicesElevated);
					
					m_VerticalExaggeration = World.Settings.VerticalExaggeration;
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}
		
		// Create indice list (PM)
		private short[] CreateTriangleIndicesBTT(CustomVertex.PositionTextured[] ElevatedVertices, int VertexDensity, int Margin, double LayerRadius)
		{
			BinaryTriangleTree Tree = new BinaryTriangleTree(ElevatedVertices, VertexDensity, Margin, LayerRadius);
			Tree.BuildTree(7); // variance 0 = best fit
			Tree.BuildIndices();
			return Tree.Indices;
		}

		private short[] CreateTriangleIndicesRegular(CustomVertex.PositionTextured[] ElevatedVertices, int VertexDensity, int Margin, double LayerRadius)
		{
			short[] indices;
			int thisVertexDensityElevatedPlus2 = (VertexDensity + (Margin * 2));
			indices = new short[2 * thisVertexDensityElevatedPlus2 * thisVertexDensityElevatedPlus2 * 3]; 

			for(int i = 0; i < thisVertexDensityElevatedPlus2; i++)
			{
				int elevated_idx = (2*3*i*thisVertexDensityElevatedPlus2);
				for(int j = 0; j < thisVertexDensityElevatedPlus2; j++)
				{
					indices[elevated_idx] = (short)(i*(thisVertexDensityElevatedPlus2 + 1) + j);
					indices[elevated_idx + 1] = (short)((i+1)*(thisVertexDensityElevatedPlus2 + 1) + j);
					indices[elevated_idx + 2] = (short)(i*(thisVertexDensityElevatedPlus2 + 1) + j+1);

					indices[elevated_idx + 3] = (short)(i*(thisVertexDensityElevatedPlus2 + 1) + j+1);
					indices[elevated_idx + 4] = (short)((i+1)*(thisVertexDensityElevatedPlus2 + 1) + j);
					indices[elevated_idx + 5] = (short)((i+1)*(thisVertexDensityElevatedPlus2 + 1) + j+1);

					elevated_idx += 6;
				}
			}
			return indices;
		}

		/// <summary>
		/// Updates the specified draw args.
		/// </summary>
		/// <param name="drawArgs">Draw args.</param>
		public void Update(DrawArgs drawArgs)
		{
			try
			{
				if(!m_Initialized)
					Initialize(drawArgs);
				
				float scaleFactor = 1f/(m_ParentWorldSurfaceRenderer.SamplesPerTile);
				float latrange = (float)Math.Abs(m_North - m_South );
				float lonrange = (float)Math.Abs(m_East - m_West );
				
				int thisVertexDensityElevatedPlus3 = ((int)m_ParentWorldSurfaceRenderer.SamplesPerTile / 2 + 3);

				scaleFactor = (float)1/(m_ParentWorldSurfaceRenderer.SamplesPerTile);
				latrange = (float)Math.Abs(m_North - m_South );
				lonrange = (float)Math.Abs(m_East - m_West );

				double centerLatitude = 0.5*(m_North + m_South);
				double centerLongitude = 0.5*(m_East + m_West);
				double tileSize = (m_North - m_South);

				if(m_VerticalExaggeration != World.Settings.VerticalExaggeration)
				{
				//	buildTerrainMesh();
				}

				if(drawArgs.WorldCamera.TrueViewRange < Angle.FromDegrees(3.0f*tileSize) 
						&& MathEngine.SphericalDistance( Angle.FromDegrees(centerLatitude), Angle.FromDegrees(centerLongitude), 
						drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees( 2.9f*tileSize )
						&& drawArgs.WorldCamera.ViewFrustum.Intersects(m_BoundingBox))
				{
					if(m_NorthWestChild == null || m_NorthEastChild == null || m_SouthWestChild == null || m_SouthEastChild == null)
					{
						ComputeChildrenTiles(drawArgs);
					}
					else
					{
						if(m_NorthEastChild != null)
						{
							m_NorthEastChild.Update(drawArgs);
						}
						if(m_NorthWestChild != null)
						{
							m_NorthWestChild.Update(drawArgs);
						}
						if(m_SouthEastChild != null)
						{
							m_SouthEastChild.Update(drawArgs);
						}
						if(m_SouthWestChild != null)
						{
							m_SouthWestChild.Update(drawArgs);
						}
					}			
				}
				else 
				{
					if(m_NorthWestChild != null)
					{
						m_NorthWestChild.Dispose();
						m_NorthWestChild = null;
					}

					if(m_NorthEastChild != null)
					{
						m_NorthEastChild.Dispose();
						m_NorthEastChild = null;
					}

					if(m_SouthEastChild != null)
					{
						m_SouthEastChild.Dispose();
						m_SouthEastChild = null;
					}

					if(m_SouthWestChild != null)
					{
						m_SouthWestChild.Dispose();
						m_SouthWestChild = null;
					}
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

		/// <summary>
		/// Disposes this instance. Releases any resources from the graphics device, also disposes of "child" surface tiles.
		/// </summary>
		public void Dispose()
		{
			m_Initialized = false;
			m_BoundingBox = null;

			if(m_Device != null)
			{
				m_Device.DeviceReset -= new EventHandler(OnDeviceReset);
				m_Device.Disposing -= new EventHandler(OnDeviceDispose);
				
				OnDeviceDispose(m_Device, null);
			}
	
			if(m_NorthWestChild != null)
			{
				m_NorthWestChild.Dispose();
				m_NorthWestChild = null;
			}
			if(m_NorthEastChild != null)
			{
				m_NorthEastChild.Dispose();
				m_NorthEastChild = null;
			}
			if(m_SouthWestChild != null)
			{
				m_SouthWestChild.Dispose();
				m_SouthWestChild = null;
			}
			if(m_SouthEastChild != null)
			{
				m_SouthEastChild.Dispose();
				m_SouthEastChild = null;
			}
			
		}

		private void calculate_normals(ref CustomVertex.PositionNormalTextured[] vertices, short[] indices)
		{
			System.Collections.ArrayList[] normal_buffer = new System.Collections.ArrayList[vertices.Length];
			for(int i = 0; i < vertices.Length; i++)
			{
				normal_buffer[i] = new System.Collections.ArrayList();
			}
			for(int i = 0; i < m_IndicesElevated.Length; i += 3)
			{
				Vector3 p1 = vertices[indices[i+0]].Position;
				Vector3 p2 = vertices[indices[i+1]].Position;
				Vector3 p3 = vertices[indices[i+2]].Position;

				Vector3 v1 = p2 - p1;
				Vector3 v2 = p3 - p1;
				Vector3 normal = Vector3.Cross(v1, v2);

				normal.Normalize();

				// Store the face's normal for each of the vertices that make up the face.
				normal_buffer[indices[i+0]].Add( normal );
				normal_buffer[indices[i+1]].Add( normal );
				normal_buffer[indices[i+2]].Add( normal );
			}


			// Now loop through each vertex vector, and avarage out all the normals stored.
			for( int i = 0; i < vertices.Length; ++i )
			{
				for( int j = 0; j < normal_buffer[i].Count; ++j )
				{
					Vector3 curNormal = (Vector3)normal_buffer[i][j];
					
					if(vertices[i].Normal == Vector3.Empty)
						vertices[i].Normal = curNormal;
					else
						vertices[i].Normal += curNormal;
				}
    
				vertices[i].Normal.Multiply(1.0f / normal_buffer[i].Count);
				
			}
		}

		/// <summary>
		/// Determines whether this surface tile is renderable.
		/// </summary>
		/// <param name="drawArgs">Draw args.</param>
		/// <returns>
		/// 	<c>true</c> if the surface tile is renderable; otherwise, <c>false</c>.
		/// </returns>
		public bool IsRenderable(DrawArgs drawArgs)
		{
			double _centerLat = 0.5*(m_North + m_South);
			double _centerLon = 0.5*(m_East + m_West);
			double m_DynamicTextureSize = (m_North - m_South);

			if(!m_Initialized || 
				drawArgs.WorldCamera.TrueViewRange / 2 > Angle.FromDegrees( 3.0*m_DynamicTextureSize*1.5f ) || 
				MathEngine.SphericalDistance(Angle.FromDegrees(_centerLat), Angle.FromDegrees(_centerLon),
				drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) > Angle.FromDegrees( 3.0*m_DynamicTextureSize*1.5f ) ||
				m_BoundingBox == null || 
				!drawArgs.WorldCamera.ViewFrustum.Intersects(m_BoundingBox) ||
				m_DynamicTexture == null //||
				//m_RenderTexture == null ||
				//m_RenderTexture.Disposed
				)
			{
			//	if(m_Level == 0)
			//	{
			//		return true;
			//	}
			//	else
			//	{
					return false;
			//	}
			}
			else
			{
				return true;
			}
		}

		private bool checkSurfaceImageChange()
		{
			//TODO: Make this smart enough to check only *this* surface tile
			if(m_ParentWorldSurfaceRenderer.LastChange > m_LastUpdate)
			{
				return true;
			}

			lock(m_ParentWorldSurfaceRenderer.SurfaceImages.SyncRoot)
			{
				for(int i = 0; i < m_ParentWorldSurfaceRenderer.SurfaceImages.Count; i++)
				{
					SurfaceImage currentSurfaceImage = m_ParentWorldSurfaceRenderer.SurfaceImages[i] as SurfaceImage;
					
					if(currentSurfaceImage.LastUpdate > m_LastUpdate || currentSurfaceImage.Opacity != currentSurfaceImage.ParentRenderable.Opacity)
					{
						if(currentSurfaceImage == null || 
							currentSurfaceImage.ImageTexture == null || 
							currentSurfaceImage.ImageTexture.Disposed || 
							!currentSurfaceImage.Enabled ||
							(currentSurfaceImage.North > m_North && currentSurfaceImage.South >= m_North) ||
							(currentSurfaceImage.North <= m_South && currentSurfaceImage.South < m_South) ||
							(currentSurfaceImage.West < m_West && currentSurfaceImage.East <= m_West) ||
							(currentSurfaceImage.West >= m_East && currentSurfaceImage.East > m_East)
							)
						{
							continue;
						}
						else
						{
							return true;
						}
					}
					else
					{
						continue;
					}
				}
			}

			return false;
		}

		int m_FramesSinceLastUpdate = 0;
		public static int SurfaceTileRefreshHz = 35;

		/// <summary>
		/// Renders the surface tile.
		/// </summary>
		/// <param name="drawArgs">Draw args.</param>
		public void Render(DrawArgs drawArgs)
		{
			try
			{
				if(!IsRenderable(drawArgs))
				{
					if(m_RenderTexture != null)
					{
						return;
					}
				}

				bool nwRendered = false;
				bool neRendered = false;
				bool swRendered = false;
				bool seRendered = false;

				if(m_NorthWestChild != null && 
					m_NorthWestChild.m_Initialized && 
					m_NorthWestChild.IsRenderable(drawArgs))
				{
					if(m_NorthWestChild.RenderTexture != null ||
						(m_NorthWestChild.RenderTexture == null && this.m_ParentWorldSurfaceRenderer.NumberTilesUpdated < 2))
					{
						m_NorthWestChild.Render(drawArgs);
						nwRendered = true;
					}
				}
				
				if(m_NorthEastChild != null && 
					m_NorthEastChild.m_Initialized && 
					m_NorthEastChild.IsRenderable(drawArgs))
				{
					if(m_NorthEastChild.RenderTexture != null ||
						(m_NorthEastChild.RenderTexture == null && this.m_ParentWorldSurfaceRenderer.NumberTilesUpdated < 2))
					{
						
						m_NorthEastChild.Render(drawArgs);
						neRendered = true;
					}
				}
				
				if(m_SouthWestChild != null &&
					m_SouthWestChild.m_Initialized &&
					m_SouthWestChild.IsRenderable(drawArgs))
				{
					if(m_SouthWestChild.RenderTexture != null ||
						(m_SouthWestChild.RenderTexture == null && this.m_ParentWorldSurfaceRenderer.NumberTilesUpdated < 2))
					{
						
						m_SouthWestChild.Render(drawArgs);
						swRendered = true;
					}
				}	
				
				if(m_SouthEastChild != null && m_SouthEastChild.m_Initialized && m_SouthEastChild.IsRenderable(drawArgs))
				{
					if(m_SouthEastChild.RenderTexture != null ||
						(m_SouthEastChild.RenderTexture == null && this.m_ParentWorldSurfaceRenderer.NumberTilesUpdated < 2))
					{
						
						m_SouthEastChild.Render(drawArgs);
						seRendered = true;
					}
				}

				if(nwRendered && neRendered && swRendered && seRendered)
					return;

				if(m_RenderTexture == null || m_RequiresUpdate || checkSurfaceImageChange() || m_FramesSinceLastUpdate++ > SurfaceTileRefreshHz)
				{
					drawArgs.device.EndScene();
					UpdateRenderSurface(drawArgs);
					drawArgs.device.BeginScene();

					m_FramesSinceLastUpdate = 0;
					m_ParentWorldSurfaceRenderer.NumberTilesUpdated++;
				}

				if(m_RenderTexture == null)
					return;
				
				drawArgs.device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
				drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
				drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
				drawArgs.device.TextureState[1].ColorOperation = TextureOperation.Disable;
				drawArgs.device.TextureState[1].AlphaOperation = TextureOperation.Disable;
				drawArgs.device.RenderState.ZBufferEnable = true;
				
				drawArgs.device.SetTexture(0, m_RenderTexture);

				drawArgs.device.SamplerState[0].MinFilter = TextureFilter.Linear;
				drawArgs.device.SamplerState[0].MagFilter = TextureFilter.Linear;
				drawArgs.device.SamplerState[0].AddressU = TextureAddress.Clamp;
				drawArgs.device.SamplerState[0].AddressV = TextureAddress.Clamp;

				if(!nwRendered && m_DynamicTexture.nwVerts != null)
				{
					drawArgs.device.DrawIndexedUserPrimitives(
						PrimitiveType.TriangleList,
						0,
						m_DynamicTexture.nwVerts.Length,
						(m_NwIndices != null ? m_NwIndices.Length / 3 : m_IndicesElevated.Length / 3),
						(m_NwIndices != null ? m_NwIndices : m_IndicesElevated),
						true,
						m_DynamicTexture.nwVerts);
				}
	
				if(!neRendered && m_DynamicTexture.neVerts != null)
				{
					drawArgs.device.DrawIndexedUserPrimitives(
						PrimitiveType.TriangleList,
						0,
						m_DynamicTexture.neVerts.Length,
						(m_NeIndices != null ? m_NeIndices.Length / 3 : m_IndicesElevated.Length / 3),
						(m_NeIndices != null ? m_NeIndices : m_IndicesElevated),
						true,
						m_DynamicTexture.neVerts);
				}	

				if(!swRendered && m_DynamicTexture.swVerts != null)
				{
					drawArgs.device.DrawIndexedUserPrimitives(
						PrimitiveType.TriangleList,
						0,
						m_DynamicTexture.nwVerts.Length,
						(m_SwIndices != null ? m_SwIndices.Length / 3 : m_IndicesElevated.Length / 3),
						(m_SwIndices != null ? m_SwIndices : m_IndicesElevated),
						true,
						m_DynamicTexture.swVerts);
				}	
						
				if(!seRendered && m_DynamicTexture.seVerts != null)
				{
					drawArgs.device.DrawIndexedUserPrimitives(
						PrimitiveType.TriangleList,
						0,
						m_DynamicTexture.seVerts.Length,
						(m_SeIndices != null ? m_SeIndices.Length / 3 : m_IndicesElevated.Length / 3),
						(m_SeIndices != null ? m_SeIndices : m_IndicesElevated),
						true,
						m_DynamicTexture.seVerts);
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
			}
			catch(Exception ex)
			{
				//TODO: Remove this
				Log.Write(ex);
			}
		}
	}

	class DynamicTexture : System.IDisposable
	{		
		public CustomVertex.PositionNormalTextured[] nwVerts;
		public CustomVertex.PositionNormalTextured[] neVerts;
		public CustomVertex.PositionNormalTextured[] swVerts;
		public CustomVertex.PositionNormalTextured[] seVerts;

		public DynamicTexture()
		{
		}
		
		#region IDisposable Members

		public void Dispose()
		{
		}
		#endregion
	}
}
