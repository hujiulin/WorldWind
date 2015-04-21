using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using GeometryUtility;
using PolygonCuttingEar;

using Tao.OpenGl;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for PolygonFeature.
	/// </summary>
	public class PolygonFeature : WorldWind.Renderable.RenderableObject
	{
		System.Drawing.Color m_polygonColor = System.Drawing.Color.Yellow;
		CustomVertex.PositionNormalColored[] m_vertices = null;
		double m_distanceAboveSurface = 0;
		float m_verticalExaggeration = World.Settings.VerticalExaggeration;
		double m_minimumDisplayAltitude = 0;
		double m_maximumDisplayAltitude = double.MaxValue;
		System.Drawing.Color m_outlineColor = System.Drawing.Color.Black;
		bool m_outline = false;
		LineFeature[] m_lineFeature = null;
		AltitudeMode m_altitudeMode = AltitudeMode.ClampedToGround;
		public BoundingBox BoundingBox = null;
		bool m_extrude = false;
		bool m_fill = true;
		LinearRing m_outerRing = null;
		LinearRing[] m_innerRings = null;
		
		public bool Fill
		{
			get{ return m_fill; }
			set
			{
				m_fill = value;
				if(m_vertices != null)
					UpdateVertices();
			}
		}

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

		public System.Drawing.Color OutlineColor
		{
			get{ return m_outlineColor; }
			set
			{
				m_outlineColor = value;
				if(m_vertices != null)
				{
					UpdateVertices();
				}
			}
		}

		public bool Outline
		{
			get{ return m_outline; }
			set
			{
				m_outline = value;
				if(m_vertices != null)
				{
					UpdateVertices();
				}
			}
		}

		public double DistanceAboveSurface
		{
			get{ return m_distanceAboveSurface; }
			set
			{ 
				m_distanceAboveSurface = value; 
				if(m_vertices != null)
					UpdateVertices();
			}
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
				if(m_vertices != null)
					UpdateVertices();
			}
		}

		public PolygonFeature(
			string name, 
			World parentWorld, 
			LinearRing outerRing,
			LinearRing[] innerRings,
			System.Drawing.Color polygonColor) : base(name, parentWorld)
		{
			RenderPriority = WorldWind.Renderable.RenderPriority.LinePaths;
			m_outerRing = outerRing;
			m_innerRings = innerRings;
			m_polygonColor = polygonColor;

			double minY = double.MaxValue;
			double maxY = double.MinValue;
			double minX = double.MaxValue;
			double maxX = double.MinValue;
			double minZ = double.MaxValue;
			double maxZ = double.MinValue;

			for(int i = 0; i < m_outerRing.Points.Length; i++)
			{
				if(m_outerRing.Points[i].X < minX)
					minX = m_outerRing.Points[i].X;
				if(m_outerRing.Points[i].X > maxX)
					maxX = m_outerRing.Points[i].X;

				if(m_outerRing.Points[i].Y < minY)
					minY = m_outerRing.Points[i].Y;
				if(m_outerRing.Points[i].Y > maxY)
					maxY = m_outerRing.Points[i].Y;

				if(m_outerRing.Points[i].Z < minZ)
					minZ = m_outerRing.Points[i].Z;
				if(m_outerRing.Points[i].Z > maxZ)
					maxZ = m_outerRing.Points[i].Z;
			}
			
			// set a uniform Z for all the points
			for(int i = 0; i < m_outerRing.Points.Length; i++)
			{
				if(m_outerRing.Points[i].Z != maxZ)
					m_outerRing.Points[i].Z = maxZ;
			}

			if(m_innerRings != null && m_innerRings.Length > 0)
			{
				for(int n = 0; n < m_innerRings.Length; n++)
				{
					for(int i = 0; i < m_innerRings[n].Points.Length; i++)
					{
						if(m_innerRings[n].Points[i].Z != maxZ)
							m_innerRings[n].Points[i].Z = maxZ;
					}
				}
			}

			m_geographicBoundingBox = new GeographicBoundingBox(maxY, minY, minX, maxX, minZ, maxZ);

			minZ += parentWorld.EquatorialRadius;
			maxZ += parentWorld.EquatorialRadius;

			BoundingBox = new BoundingBox(
				(float)minY, (float)maxY, (float)minX, (float)maxX, (float)minZ, (float)maxZ);
		}

		Polygon m_polygon = null;
		GeographicBoundingBox m_geographicBoundingBox = null;

		public override void Initialize(DrawArgs drawArgs)
		{
			UpdateVertices();
			isInitialized = true;
		}

		System.Collections.ArrayList primList = new ArrayList();
		System.Collections.ArrayList tessList = new ArrayList();
		PrimitiveType m_primitiveType = PrimitiveType.PointList;

		private void f(System.IntPtr vertexData) 
		{
			try
			{
				double[] v = new double[3];
				System.Runtime.InteropServices.Marshal.Copy(vertexData, v, 0, 3);

				Point3d p = new Point3d(v[0], v[1], 0);
				tessList.Add(p);
			} 
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		} 

		private void e() 
		{
			finishTesselation((Point3d[])tessList.ToArray(typeof(Point3d)));
		}

		private void b(int which) 
		{
			tessList.Clear();
			switch(which)
			{
				case 4:
					m_primitiveType = PrimitiveType.TriangleList;
					break;
				case 5:
					m_primitiveType = PrimitiveType.TriangleStrip;
					break;
				case 6:
					m_primitiveType = PrimitiveType.TriangleFan;
					break;
			}
			primTypes.Add(m_primitiveType);
		}

		private void r(int which) 
		{
			Log.Write(Log.Levels.Error, "error: " + which.ToString());
		}

		private void getTessellation()
		{
			try
			{
			
				primList.Clear();
				primTypes.Clear();

				System.Collections.ArrayList pointList = new ArrayList();
				for(int i = 0; i < m_outerRing.Points.Length; i++)
				{
					double[] p = new double[3];
					p[0] = m_outerRing.Points[i].X;
					p[1] = m_outerRing.Points[i].Y;
					p[2] = m_outerRing.Points[i].Z;
					
					pointList.Add(p);
				}

				Glu.GLUtesselator tess = Glu.gluNewTess();
				Glu.gluTessCallback(tess, Glu.GLU_TESS_BEGIN, new Glu.TessBeginCallback(b));
				Glu.gluTessCallback(tess, Glu.GLU_TESS_END, new Glu.TessEndCallback(e));

				Glu.gluTessCallback(tess, Glu.GLU_TESS_ERROR, new Glu.TessErrorCallback(r));
				Glu.gluTessCallback(tess, Glu.GLU_TESS_VERTEX, new Glu.TessVertexCallback(f));

				Glu.gluTessBeginPolygon(tess, IntPtr.Zero);
				Glu.gluTessBeginContour(tess);
		
				for(int i = 0; i < pointList.Count - 1; i++)
				{
					double[] p = (double[])pointList[i];
					Glu.gluTessVertex(tess, p, p);
				}
				Glu.gluTessEndContour(tess);
				
				if(m_innerRings != null && m_innerRings.Length > 0)
				{
					for(int i = 0; i < m_innerRings.Length; i++)
					{
						Glu.gluTessBeginContour(tess);
						for(int j = m_innerRings[i].Points.Length - 1; j >= 0; j--)
						{
							double[] p = new double[3];
							p[0] = m_innerRings[i].Points[j].X;
							p[1] = m_innerRings[i].Points[j].Y;
							p[2] = m_innerRings[i].Points[j].Z;
							Glu.gluTessVertex(tess, p, p);
						}
						Glu.gluTessEndContour(tess);
					}
				}
				
				Glu.gluTessEndPolygon(tess);
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}

		ArrayList primTypes = new ArrayList();

		private void finishTesselation(Point3d[] tesselatorList)
		{
			int polygonColor = System.Drawing.Color.FromArgb(m_polygonColor.A, m_polygonColor.R, m_polygonColor.G, m_polygonColor.B).ToArgb();
			CustomVertex.PositionNormalColored[] vertices = new CustomVertex.PositionNormalColored[tesselatorList.Length];
		
			for(int i = 0; i < vertices.Length; i++)
			{
				Point3d sphericalPoint = tesselatorList[i];
					
				double terrainHeight = 0;
				if(m_altitudeMode == AltitudeMode.RelativeToGround)
				{
					if(World.TerrainAccessor != null)
					{
						terrainHeight = World.TerrainAccessor.GetElevationAt(
							sphericalPoint.Y,
							sphericalPoint.X,
							(100.0 / DrawArgs.Camera.ViewRange.Degrees)
							);
					}
				}

				Vector3 xyzVector = MathEngine.SphericalToCartesian(
					sphericalPoint.Y, 
					sphericalPoint.X, 
					World.EquatorialRadius + m_verticalExaggeration * (m_geographicBoundingBox.MaximumAltitude + m_distanceAboveSurface + terrainHeight));

				vertices[i].Color = polygonColor;
				vertices[i].X = xyzVector.X;
				vertices[i].Y = xyzVector.Y;
				vertices[i].Z = xyzVector.Z;
			}

			primList.Add(vertices);
		}

		private void UpdateVertices()
		{
			m_verticalExaggeration = World.Settings.VerticalExaggeration;

			if(m_altitudeMode == AltitudeMode.ClampedToGround)
			{
				if(m_polygon != null)
				{
					m_polygon.Remove = true;
					m_polygon = null;
				}

				m_polygon = new Polygon();
				m_polygon.outerBoundary = m_outerRing;
				m_polygon.innerBoundaries = m_innerRings;
				m_polygon.PolgonColor = m_polygonColor;
				m_polygon.Fill = m_fill;
				m_polygon.ParentRenderable = this;
				this.World.ProjectedVectorRenderer.Add(m_polygon);

				if(m_vertices != null)
					m_vertices = null;
				
				if(m_lineFeature != null)
				{
					m_lineFeature[0].Dispose();
					m_lineFeature = null;
				}
				
				return;
			}

			getTessellation();
			
			if(m_extrude || m_outline)
			{
				m_lineFeature = new LineFeature[1 + (m_innerRings != null && m_innerRings.Length > 0 ? m_innerRings.Length : 0)];

				Point3d[] linePoints = new Point3d[m_outerRing.Points.Length + 1];
				for(int i = 0; i < m_outerRing.Points.Length; i++)
				{
					linePoints[i] = m_outerRing.Points[i];
				}

				linePoints[linePoints.Length - 1] = m_outerRing.Points[0];
				
				m_lineFeature[0] = new LineFeature(Name, World, linePoints, m_polygonColor);
				m_lineFeature[0].DistanceAboveSurface = m_distanceAboveSurface;
				m_lineFeature[0].MinimumDisplayAltitude = m_minimumDisplayAltitude;
				m_lineFeature[0].MaximumDisplayAltitude = m_maximumDisplayAltitude;
				m_lineFeature[0].AltitudeMode = AltitudeMode;
				m_lineFeature[0].Opacity = Opacity;
				m_lineFeature[0].Outline = m_outline;
				m_lineFeature[0].LineColor = m_outlineColor;
				m_lineFeature[0].Extrude = m_extrude;

				if(m_innerRings != null && m_innerRings.Length > 0)
				{
					for(int i = 0; i < m_innerRings.Length; i++)
					{
						Point3d[] innerPoints = new Point3d[m_innerRings[i].Points.Length + 1];
						for(int j = 0; j < m_innerRings[i].Points.Length; j++)
						{
							innerPoints[j] = m_innerRings[i].Points[j];
						}

						innerPoints[innerPoints.Length - 1] = m_innerRings[i].Points[0];
				
						m_lineFeature[1 + i] = new LineFeature(Name, World, innerPoints, m_polygonColor);
						m_lineFeature[1 + i].DistanceAboveSurface = m_distanceAboveSurface;
						m_lineFeature[1 + i].MinimumDisplayAltitude = m_minimumDisplayAltitude;
						m_lineFeature[1 + i].MaximumDisplayAltitude = m_maximumDisplayAltitude;
						m_lineFeature[1 + i].AltitudeMode = AltitudeMode;
						m_lineFeature[1 + i].Opacity = Opacity;
						m_lineFeature[1 + i].Outline = m_outline;
						m_lineFeature[1 + i].LineColor = m_outlineColor;
						m_lineFeature[1 + i].Extrude = m_extrude;
					}
				}
			}
			else
			{
				if(m_lineFeature != null && m_lineFeature.Length > 0)
				{
					for(int i = 0; i < m_lineFeature.Length; i++)
					{
						if(m_lineFeature[i] != null)
						{
							m_lineFeature[i].Dispose();
							m_lineFeature[i] = null;
						}	
					}
					m_lineFeature = null;
				}
			}
		}

		public override void Dispose()
		{
			if(m_polygon != null)
			{
				m_polygon.Remove = true;
				m_polygon = null;
			}

			if(m_lineFeature != null)
			{
				for(int i = 0; i < m_lineFeature.Length; i++)
				{
					if(m_lineFeature[i] != null)
						m_lineFeature[i].Dispose();
				}
				m_lineFeature = null;
			}
		}

		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if(drawArgs.WorldCamera.Altitude >= m_minimumDisplayAltitude && drawArgs.WorldCamera.Altitude <= m_maximumDisplayAltitude)
				{
					if(!isInitialized)
						Initialize(drawArgs);

					if(m_verticalExaggeration != World.Settings.VerticalExaggeration)
					{
						UpdateVertices();
					}

					if(m_lineFeature != null)
					{
						for(int i = 0; i < m_lineFeature.Length; i++)
						{
							if(m_lineFeature[i] != null)
								m_lineFeature[i].Update(drawArgs);
						}
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isInitialized /*|| m_vertices == null*/ || drawArgs.WorldCamera.Altitude < m_minimumDisplayAltitude || drawArgs.WorldCamera.Altitude > m_maximumDisplayAltitude)
			{
				return;
			}
			
			if(!drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox))
				return;

			try
			{
				Cull currentCull = drawArgs.device.RenderState.CullMode;
				drawArgs.device.RenderState.CullMode = Cull.None;
			
				drawArgs.device.Transform.World = Matrix.Translation(
					(float)-drawArgs.WorldCamera.ReferenceCenter.X,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Z
					);
				
				//if(m_vertices != null)
				if(primList.Count > 0)
				{
					drawArgs.device.VertexFormat = CustomVertex.PositionNormalColored.Format;
					drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
					for(int i = 0; i < primList.Count; i++)
					{
						int vertexCount = 0;
						PrimitiveType primType = (PrimitiveType)primTypes[i];
						CustomVertex.PositionNormalColored[] vertices = (CustomVertex.PositionNormalColored[])primList[i];
						
						if(primType == PrimitiveType.TriangleList)
							vertexCount = vertices.Length / 3;
						else
							vertexCount = vertices.Length - 2;
					
						drawArgs.device.DrawUserPrimitives(
							primType,//PrimitiveType.TriangleList, 
							vertexCount, 
							vertices);
					}
				}

				if(m_lineFeature != null)
				{
					for(int i = 0; i < m_lineFeature.Length; i++)
					{
						if(m_lineFeature[i] != null)
							m_lineFeature[i].Render(drawArgs);
					}
				}

				drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
				drawArgs.device.RenderState.CullMode = currentCull;
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}

		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

	}
}
