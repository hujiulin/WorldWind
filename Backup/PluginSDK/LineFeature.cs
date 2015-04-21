using System;
using WorldWind;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	/// <summary>
	/// Summary description for LineFeature.
	/// </summary>
	public class LineFeature : WorldWind.Renderable.RenderableObject
	{
		#region Static Members
		#endregion

		#region Private Members
		double m_distanceAboveSurface = 0;
		Point3d[] m_points = null;
		CustomVertex.PositionColoredTextured[] m_wallVertices = null;

		CustomVertex.PositionColored[] m_topVertices = null;
		CustomVertex.PositionColored[] m_bottomVertices = null;
		CustomVertex.PositionColored[] m_sideVertices = null;

		System.Drawing.Color m_lineColor = System.Drawing.Color.Black;
		float m_verticalExaggeration = World.Settings.VerticalExaggeration;
		double m_minimumDisplayAltitude = 0;
		double m_maximumDisplayAltitude = double.MaxValue;
		string m_imageUri = null;
		Texture m_texture = null;
		System.Drawing.Color m_polygonColor = System.Drawing.Color.Black;
		bool m_outline = true;
		float m_lineWidth = 1.0f;
		bool m_extrude = false;
		AltitudeMode m_altitudeMode = AltitudeMode.ClampedToGround;
		long m_numPoints = 0;
		#endregion

		/// <summary>
		/// Boolean indicating whether or not the line needs rebuilding.
		/// </summary>
		public bool NeedsUpdate = true;

		public bool Extrude
		{
			get{ return m_extrude; }
			set{ m_extrude = value; }
		}

		public AltitudeMode AltitudeMode
		{
			get{ return m_altitudeMode; }
			set{ m_altitudeMode = value; }
		}

		public System.Drawing.Color LineColor
		{
			get{ return m_lineColor; }
			set
			{ 
				m_lineColor = value; 
				NeedsUpdate = true;
			}
		}

		public float LineWidth
		{
			get{ return m_lineWidth; }
			set
			{
				m_lineWidth = value;
				NeedsUpdate = true;
			}
		}

		public double DistanceAboveSurface
		{
			get{ return m_distanceAboveSurface; }
			set
			{ 
				m_distanceAboveSurface = value; 
				if(m_topVertices != null)
				{
					NeedsUpdate = true;
//					UpdateVertices();
				}
			}
		}

		public System.Drawing.Color PolygonColor
		{
			get{ return m_polygonColor; }
			set
			{
				m_polygonColor = value;
				if(m_topVertices != null)
				{
					NeedsUpdate = true;
//					UpdateVertices();
				}
			}
		}

		public bool Outline
		{
			get{ return m_outline; }
			set
			{
				m_outline = value;
				if(m_topVertices != null)
				{
					NeedsUpdate = true;
//					UpdateVertices();
				}
			}
		}

		public Point3d[] Points
		{
			get
			{
				// if the array size is correct just return it
				if (m_numPoints == m_points.LongLength)
					return m_points; 

				// return an array the correct size.
				Point3d[] points = new Point3d[m_numPoints];
				for (int i = 0; i < m_numPoints; i++)
				{
					points[i] = m_points[i];
				}
				return points;
			}
			set
			{
				m_points = value;
				m_numPoints = m_points.LongLength;
				NeedsUpdate = true;
			}
		}

		public long NumPoints
		{
			get { return m_numPoints; }
		}

		public double MinimumDisplayAltitude
		{
			get{ return m_minimumDisplayAltitude; }
			set{ m_minimumDisplayAltitude = value; }
		}

		public double MaximumDisplayAltitude
		{
			get{ return m_maximumDisplayAltitude; }
			set{ m_maximumDisplayAltitude = value; }
		}

		public override byte Opacity
		{
			get
			{
				return base.Opacity;
			}
			set
			{
				base.Opacity = value;
				if(m_topVertices != null)
				{
					UpdateVertices();
				}
			}
		}

		public LineFeature(string name, World parentWorld, Point3d[] points, System.Drawing.Color lineColor) : base(name, parentWorld)
		{
			m_points = points;
			m_lineColor = lineColor;
			m_polygonColor = lineColor;
			m_numPoints = m_points.LongLength;

			RenderPriority = WorldWind.Renderable.RenderPriority.LinePaths;
		}
		
		public LineFeature(string name, World parentWorld, Point3d[] points, string imageUri) : base(name, parentWorld)
		{
			m_points = points;
			m_imageUri = imageUri;
			m_numPoints = m_points.LongLength;

			RenderPriority = WorldWind.Renderable.RenderPriority.LinePaths;
		}

		public override void Dispose()
		{
			if(m_texture != null && !m_texture.Disposed)
			{
				m_texture.Dispose();
				m_texture = null;
			}

			if(m_lineString != null)
			{
				m_lineString.Remove = true;
				m_lineString = null;
			}
			NeedsUpdate = true;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if(m_points == null)
			{
				isInitialized = true;
				return;
			}

			if(m_imageUri != null)
			{
				//load image
				if(m_imageUri.ToLower().StartsWith("http://"))
				{
					string savePath = string.Format("{0}\\image", ConfigurationLoader.GetRenderablePathString(this));
					System.IO.FileInfo file = new System.IO.FileInfo(savePath);
					if(!file.Exists)
					{
						WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(m_imageUri);

						if(!file.Directory.Exists)
							file.Directory.Create();

						download.DownloadFile(file.FullName, WorldWind.Net.DownloadType.Unspecified);
					}

					m_texture = ImageHelper.LoadTexture(file.FullName);
				}
				else
				{
					m_texture = ImageHelper.LoadTexture(m_imageUri);
				}
			}
            
			UpdateVertices();

			isInitialized = true;
		}

		/// <summary>
		/// Adds a point to the end of the line.
		/// </summary>
		/// <param name="x">Lon</param>
		/// <param name="y">Lat</param>
		/// <param name="z">Alt (meters)</param>
		public void AddPoint(double x, double y, double z)
		{
            Point3d point = new Point3d(x, y, z);
            //TODO:Divide into subsegments if too far
            if (m_numPoints > 0)
            {
                Angle startlon = Angle.FromDegrees(m_points[m_numPoints - 1].X);
                Angle startlat = Angle.FromDegrees(m_points[m_numPoints - 1].Y);
                double startalt = m_points[m_numPoints - 1].Z;
                Angle endlon = Angle.FromDegrees(x);
                Angle endlat = Angle.FromDegrees(y);
                double endalt = z;

                Angle dist = World.ApproxAngularDistance(startlat, startlon, endlat, endlon);
                if (dist.Degrees > 0.25)
                {

                    double stepSize = 0.25;
                    int samples = (int)(dist.Degrees / stepSize);

                    for (int i = 0; i < samples; i++)
                    {
                        Angle lat, lon = Angle.Zero;
                        float frac = (float)i / samples;
                        World.IntermediateGCPoint(frac, startlat, startlon, endlat, endlon,
                        dist, out lat, out lon);
                        double alt = startalt + frac * (endalt - startalt);
                        Point3d pointint = new Point3d(lon.Degrees, lat.Degrees, alt);
                        AddPoint(pointint);
                    }
                    AddPoint(point);
                }
                else
                {       
                    AddPoint(point);
                }
            }
            else
            {
                AddPoint(point);
            }
			
		}

		/// <summary>
		/// Adds a point to the line at the end of the line.
		/// </summary>
		/// <param name="point">The Point3d object to add.</param>
		public void AddPoint(Point3d point)
		{
			// if the array is too small grow it.
			if (m_numPoints >= m_points.LongLength)
			{
				long growSize = m_points.LongLength / 2;
				if (growSize < 10) growSize = 10;

				Point3d[] points = new Point3d[m_points.LongLength + growSize];

				for (int i = 0; i < m_numPoints; i++)
				{
					points[i] = m_points[i];
				}
				m_points = points;
			}
			m_points[m_numPoints] = point;
			m_numPoints++;
			NeedsUpdate = true;
		}

		private void UpdateVertices()
		{
			try
			{
				m_verticalExaggeration = World.Settings.VerticalExaggeration;

				UpdateTexturedVertices();

				if(m_lineString != null && m_outline && m_wallVertices != null && m_wallVertices.Length > m_topVertices.Length)
				{
					UpdateOutlineVertices();
				}

				NeedsUpdate = false;
			}
			catch(Exception ex)
			{
				Utility.Log.Write(ex);
			}
		}

		private void UpdateOutlineVertices()
		{
			m_bottomVertices = new CustomVertex.PositionColored[m_numPoints];
			m_sideVertices = new CustomVertex.PositionColored[m_numPoints * 2];
			
			for(int i = 0; i < m_numPoints; i++)
			{
				m_sideVertices[2 * i] = m_topVertices[i];

				Vector3 xyzVertex = new Vector3(
					m_wallVertices[2 * i + 1].X,
					m_wallVertices[2 * i + 1].Y,
					m_wallVertices[2 * i + 1].Z);

				m_bottomVertices[i].X = xyzVertex.X;
				m_bottomVertices[i].Y = xyzVertex.Y;
				m_bottomVertices[i].Z = xyzVertex.Z;
				m_bottomVertices[i].Color = m_lineColor.ToArgb();

				m_sideVertices[2 * i + 1] = m_bottomVertices[i];
			}
		}

		LineString m_lineString = null;
		private void UpdateTexturedVertices()
		{
			if(m_altitudeMode == AltitudeMode.ClampedToGround)
			{
				if(m_lineString != null)
				{
					m_lineString.Remove = true;
					m_lineString = null;
				}

				m_lineString = new LineString();
				m_lineString.Coordinates = Points;
				m_lineString.Color = LineColor;
				m_lineString.LineWidth = LineWidth;
				m_lineString.ParentRenderable = this;
				this.World.ProjectedVectorRenderer.Add(m_lineString);

				if(m_wallVertices != null)
					m_wallVertices = null;

				return;
			}

			if(m_extrude || m_altitudeMode == AltitudeMode.RelativeToGround)
			{
				m_wallVertices = new CustomVertex.PositionColoredTextured[m_numPoints * 2];
			}

			float textureCoordIncrement = 1.0f / (float)(m_numPoints - 1);
			m_verticalExaggeration = World.Settings.VerticalExaggeration;
			int vertexColor = m_polygonColor.ToArgb();

			m_topVertices = new CustomVertex.PositionColored[m_numPoints];

			for(int i = 0; i < m_numPoints; i++)
			{
				double terrainHeight = 0;
				if(m_altitudeMode == AltitudeMode.RelativeToGround)
				{
					if(World.TerrainAccessor != null)
					{
						terrainHeight = World.TerrainAccessor.GetElevationAt(
							m_points[i].Y,
							m_points[i].X,
							(100.0 / DrawArgs.Camera.ViewRange.Degrees)
							);
					}
				}

				Vector3 xyzVertex = MathEngine.SphericalToCartesian(
					m_points[i].Y, 
					m_points[i].X, 
					m_verticalExaggeration * (m_distanceAboveSurface + terrainHeight + m_points[i].Z) + World.EquatorialRadius
					);
				
				m_topVertices[i].X = xyzVertex.X;
				m_topVertices[i].Y = xyzVertex.Y;
				m_topVertices[i].Z = xyzVertex.Z;
				m_topVertices[i].Color = m_lineColor.ToArgb();
				
				if(m_extrude || m_altitudeMode == AltitudeMode.RelativeToGround)
				{
					m_wallVertices[2 * i].X = xyzVertex.X;
					m_wallVertices[2 * i].Y = xyzVertex.Y;
					m_wallVertices[2 * i].Z = xyzVertex.Z;
					m_wallVertices[2 * i].Color = vertexColor;
					m_wallVertices[2 * i].Tu = i * textureCoordIncrement;
					m_wallVertices[2 * i].Tv = 1.0f;

					xyzVertex = MathEngine.SphericalToCartesian(
						m_points[i].Y, 
						m_points[i].X, 
						m_verticalExaggeration * (m_distanceAboveSurface + terrainHeight) + World.EquatorialRadius
						);

					m_wallVertices[2 * i + 1].X = xyzVertex.X;
					m_wallVertices[2 * i + 1].Y = xyzVertex.Y;
					m_wallVertices[2 * i + 1].Z = xyzVertex.Z;
					m_wallVertices[2 * i + 1].Color = vertexColor;
					m_wallVertices[2 * i + 1].Tu = i * textureCoordIncrement;
					m_wallVertices[2 * i + 1].Tv = 0.0f;
				}
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Update(DrawArgs drawArgs)
		{
			if(drawArgs.WorldCamera.Altitude >= m_minimumDisplayAltitude && drawArgs.WorldCamera.Altitude <= m_maximumDisplayAltitude)
			{
				if(!isInitialized)
					Initialize(drawArgs);

				if (NeedsUpdate || (m_verticalExaggeration != World.Settings.VerticalExaggeration))
					UpdateVertices();
			}
			
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isInitialized || drawArgs.WorldCamera.Altitude < m_minimumDisplayAltitude || drawArgs.WorldCamera.Altitude > m_maximumDisplayAltitude)
			{
				return;
			}

			try
			{
				if(m_lineString != null)
					return;

				Cull currentCull = drawArgs.device.RenderState.CullMode;
				drawArgs.device.RenderState.CullMode = Cull.None;
			
				Vector3 rc = new Vector3(
					(float)drawArgs.WorldCamera.ReferenceCenter.X,
					(float)drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)drawArgs.WorldCamera.ReferenceCenter.Z
					);

				drawArgs.device.Transform.World = Matrix.Translation(
					(float)-drawArgs.WorldCamera.ReferenceCenter.X,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Z
					);

				if(m_wallVertices != null)
				{
					drawArgs.device.RenderState.ZBufferEnable = true;
					
					if(m_texture != null && !m_texture.Disposed)
					{
						drawArgs.device.SetTexture(0, m_texture);
						drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
						drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Add;
						drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
					}
					else
					{
						drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
					}
					
					drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
					
					drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_wallVertices.Length - 2, m_wallVertices);
					
					if(m_outline)
					{

						drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
						drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
						drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, m_topVertices.Length - 1, m_topVertices);
						
						if(m_bottomVertices != null)
							drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, m_bottomVertices.Length - 1, m_bottomVertices);
							
						if(m_sideVertices != null)
							drawArgs.device.DrawUserPrimitives(PrimitiveType.LineList, m_sideVertices.Length / 2, m_sideVertices);

					}
				}
				else
				{
					drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
					drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
					drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, m_topVertices.Length - 1, m_topVertices);
				}

				drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
				drawArgs.device.RenderState.CullMode = currentCull;
			}
			catch//(Exception ex)
			{
				//Utility.Log.Write(ex);
			}
		}
	}
}
