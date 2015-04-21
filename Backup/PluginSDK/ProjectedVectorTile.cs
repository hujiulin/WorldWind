using System;
using System.Collections;
using System.Drawing;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;

namespace WorldWind
{
	public class ProjectedVectorTile
	{
		public static string CachePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Cache");
		bool m_Initialized = false;
		bool m_Initializing = false;
		bool m_Disposing = false;

		public int Level = 0;
		public int Row = 0;
		public int Col = 0;

		ProjectedVectorRenderer m_parentProjectedLayer = null;
		
		Renderable.ImageLayer m_NwImageLayer;
		Renderable.ImageLayer m_NeImageLayer;
		Renderable.ImageLayer m_SwImageLayer;
		Renderable.ImageLayer m_SeImageLayer;

		public GeographicBoundingBox m_geographicBoundingBox = null;
		public BoundingBox BoundingBox;
		
		public ProjectedVectorTile(
			GeographicBoundingBox geographicBoundingBox,
			ProjectedVectorRenderer parentLayer
			)
		{
			m_geographicBoundingBox = geographicBoundingBox;
			m_parentProjectedLayer = parentLayer;

			BoundingBox = new BoundingBox( (float)geographicBoundingBox.South, (float)geographicBoundingBox.North, (float)geographicBoundingBox.West, (float)geographicBoundingBox.East, 
				(float)(parentLayer.World.EquatorialRadius + geographicBoundingBox.MinimumAltitude), (float)(parentLayer.World.EquatorialRadius + geographicBoundingBox.MaximumAltitude + 300000f));
		}

		public void Dispose()
		{
			m_Initialized = false;
			m_Disposing = true;
			if(m_NorthWestChild != null)
			{
				m_NorthWestChild.Dispose();
			}

			if(m_NorthEastChild != null)
			{
				m_NorthEastChild.Dispose();
			}

			if(m_SouthWestChild != null)
			{
				m_SouthWestChild.Dispose();
			}

			if(m_SouthEastChild != null)
			{
				m_SouthEastChild.Dispose();
			}
			
			if(m_NwImageLayer != null)
			{
				m_NwImageLayer.Dispose();
				m_NwImageLayer = null;
			}

			if(m_NeImageLayer != null)
			{
				m_NeImageLayer.Dispose();
				m_NeImageLayer = null;
			}

			if(m_SwImageLayer != null)
			{
				m_SwImageLayer.Dispose();
				m_SwImageLayer = null;
			}

			if(m_SeImageLayer != null)
			{
				m_SeImageLayer.Dispose();
				m_SeImageLayer = null;
			}

			if(m_ImageStream != null)
			{
				m_ImageStream.Close();
			}

			if(!m_Initializing)
			{
				m_Disposing = false;
			}
		}

		public static System.Drawing.Drawing2D.HatchStyle getGDIHatchStyle(WorldWind.ShapeFillStyle shapeFillStyle)
		{
			if(shapeFillStyle == WorldWind.ShapeFillStyle.BackwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Cross)
			{
				return System.Drawing.Drawing2D.HatchStyle.Cross;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DarkDownwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.DarkDownwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DarkHorizontal)
			{
				return System.Drawing.Drawing2D.HatchStyle.DarkHorizontal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DarkUpwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.DarkUpwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DarkVertical)
			{
				return System.Drawing.Drawing2D.HatchStyle.DarkVertical;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DashedDownwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.DashedDownwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DashedHorizontal)
			{
				return System.Drawing.Drawing2D.HatchStyle.DashedHorizontal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DashedUpwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.DashedUpwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DashedVertical)
			{
				return System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DiagonalBrick)
			{
				return System.Drawing.Drawing2D.HatchStyle.DiagonalBrick;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DiagonalCross)
			{
				return System.Drawing.Drawing2D.HatchStyle.DiagonalCross;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Divot)
			{
				return System.Drawing.Drawing2D.HatchStyle.Divot;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DottedDiamond)
			{
				return System.Drawing.Drawing2D.HatchStyle.DottedDiamond;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.DottedGrid)
			{
				return System.Drawing.Drawing2D.HatchStyle.DottedGrid;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.ForwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Horizontal)
			{
				return System.Drawing.Drawing2D.HatchStyle.Horizontal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LargeCheckerBoard)
			{
				return System.Drawing.Drawing2D.HatchStyle.LargeCheckerBoard;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LargeConfetti)
			{
				return System.Drawing.Drawing2D.HatchStyle.LargeConfetti;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LargeGrid)
			{
				return System.Drawing.Drawing2D.HatchStyle.LargeGrid;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LightDownwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.LightDownwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LightHorizontal)
			{
				return System.Drawing.Drawing2D.HatchStyle.LightHorizontal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LightUpwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.LightUpwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.LightVertical)
			{
				return System.Drawing.Drawing2D.HatchStyle.LightVertical;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Max)
			{
				return System.Drawing.Drawing2D.HatchStyle.Max;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Min)
			{
				return System.Drawing.Drawing2D.HatchStyle.Min;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.NarrowHorizontal)
			{
				return System.Drawing.Drawing2D.HatchStyle.NarrowHorizontal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.NarrowVertical)
			{
				return System.Drawing.Drawing2D.HatchStyle.NarrowVertical;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.OutlinedDiamond)
			{
				return System.Drawing.Drawing2D.HatchStyle.OutlinedDiamond;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent05)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent05;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent10)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent10;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent20)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent20;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent25)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent25;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent30)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent30;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent40)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent40;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent50)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent50;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent60)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent60;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent70)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent70;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent75)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent75;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent80)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent80;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Percent90)
			{
				return System.Drawing.Drawing2D.HatchStyle.Percent90;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Plaid)
			{
				return System.Drawing.Drawing2D.HatchStyle.Plaid;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Shingle)
			{
				return System.Drawing.Drawing2D.HatchStyle.Shingle;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.SmallCheckerBoard)
			{
				return System.Drawing.Drawing2D.HatchStyle.SmallCheckerBoard;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.SmallConfetti)
			{
				return System.Drawing.Drawing2D.HatchStyle.SmallConfetti;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.SmallGrid)
			{
				return System.Drawing.Drawing2D.HatchStyle.SmallGrid;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.SolidDiamond)
			{
				return System.Drawing.Drawing2D.HatchStyle.SolidDiamond;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Sphere)
			{
				return System.Drawing.Drawing2D.HatchStyle.Sphere;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Trellis)
			{
				return System.Drawing.Drawing2D.HatchStyle.Trellis;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Wave)
			{
				return System.Drawing.Drawing2D.HatchStyle.Wave;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.Weave)
			{
				return System.Drawing.Drawing2D.HatchStyle.Weave;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.WideDownwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.WideDownwardDiagonal;
			}
			else if(shapeFillStyle == WorldWind.ShapeFillStyle.WideUpwardDiagonal)
			{
				return System.Drawing.Drawing2D.HatchStyle.WideUpwardDiagonal;
			}
			else
			{
				return System.Drawing.Drawing2D.HatchStyle.ZigZag;
			}
			
		}

		/*public byte Opacity
		{
			get
			{
				return m_parentProjectedLayer.Opacity;
			}
			set
			{
				if(m_NwImageLayer != null)
				{
					m_NwImageLayer.Opacity = value;
				}
				if(m_NeImageLayer != null)
				{
					m_NeImageLayer.Opacity = value;
				}
				if(m_SwImageLayer != null)
				{
					m_SwImageLayer.Opacity = value;
				}
				if(m_SeImageLayer != null)
				{
					m_SeImageLayer.Opacity = value;
				}

				if(m_NorthWestChild != null)
				{
					m_NorthWestChild.Opacity = value;
				}
				if(m_NorthEastChild != null)
				{
					m_NorthEastChild.Opacity = value;
				}
				if(m_SouthWestChild != null)
				{
					m_SouthWestChild.Opacity = value;
				}
				if(m_SouthEastChild != null)
				{
					m_SouthEastChild.Opacity = value;
				}
			}
		}*/

		private Renderable.ImageLayer CreateImageLayer(double north, double south, double west, double east, DrawArgs drawArgs, string imagePath)
		{
			Bitmap b = null;
			Graphics g = null;
			Renderable.ImageLayer imageLayer = null;
			GeographicBoundingBox geoBB = new GeographicBoundingBox(north, south, west, east);
			int numberPolygonsInTile = 0;

			FileInfo imageFile = new FileInfo(imagePath);

			if(!m_parentProjectedLayer.EnableCaching ||
				!imageFile.Exists
				)
			{
				if(m_parentProjectedLayer.LineStrings != null)
				{
					for(int i = 0; i < m_parentProjectedLayer.LineStrings.Length; i++)
					{
						if(!m_parentProjectedLayer.LineStrings[i].Visible)
							continue;
						
						GeographicBoundingBox currentBoundingBox = m_parentProjectedLayer.LineStrings[i].GetGeographicBoundingBox();

						if(currentBoundingBox != null && !currentBoundingBox.Intersects(geoBB))
						{
							continue;
						}
						else
						{
							if(b == null)
							{
								b = new Bitmap(
									m_parentProjectedLayer.TileSize.Width,
									m_parentProjectedLayer.TileSize.Height,
									System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							}

							if(g == null)
							{
								g = Graphics.FromImage(b);
							}

							drawLineString(
								m_parentProjectedLayer.LineStrings[i],
								g,
								geoBB,
								b.Size
								);
							
							numberPolygonsInTile++;
						}
					}
				}

				if(m_parentProjectedLayer.Polygons != null)
				{
					for(int i = 0; i < m_parentProjectedLayer.Polygons.Length; i++)
					{
						if(!m_parentProjectedLayer.Polygons[i].Visible)
							continue;

						GeographicBoundingBox currentBoundingBox = m_parentProjectedLayer.Polygons[i].GetGeographicBoundingBox();

						if(currentBoundingBox != null && !currentBoundingBox.Intersects(geoBB))
						{
							continue;
						}
						else
						{
							if(b == null)
							{
								b = new Bitmap(
									m_parentProjectedLayer.TileSize.Width,
									m_parentProjectedLayer.TileSize.Height,
									System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							}

							if(g == null)
							{
								g = Graphics.FromImage(b);
							}
							drawPolygon(
								m_parentProjectedLayer.Polygons[i],
								g,
								geoBB,
								b.Size);
	
							numberPolygonsInTile++;
						}
					}
				}
			}

			if(b != null)
			{
				System.Drawing.Imaging.BitmapData srcInfo = b.LockBits(new Rectangle(0, 0, 
					b.Width, b.Height), 
					System.Drawing.Imaging.ImageLockMode.ReadOnly, 
					System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				bool isBlank = true;
				unsafe
				{
					int* srcPointer = (int*)srcInfo.Scan0;
					for(int i = 0; i < b.Height; i++) 
					{
						for(int j = 0; j < b.Width; j++) 
						{
							int color = *srcPointer++;
						
							if(((color >> 24) & 0xff) > 0)
							{
								isBlank = false;
								break;
							}
						}

						srcPointer += (srcInfo.Stride>>2) - b.Width;
					}
				}

				b.UnlockBits(srcInfo);
				if(isBlank)
					numberPolygonsInTile = 0;
			}
          
		//	if(!m_parentProjectedLayer.EnableCaching)
		//	{
				string id = System.DateTime.Now.Ticks.ToString();

				if(b != null && numberPolygonsInTile > 0)
				{
					MemoryStream ms = new MemoryStream();
					b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

					//must copy original stream into new stream, if not, error occurs, not sure why
					m_ImageStream = new MemoryStream(ms.GetBuffer());
					
					imageLayer = new WorldWind.Renderable.ImageLayer(
						id,
						m_parentProjectedLayer.World,
						0,
						m_ImageStream,
						System.Drawing.Color.Black.ToArgb(),
						(float)south,
						(float)north,
						(float)west,
						(float)east,
						1.0f//(float)m_parentProjectedLayer.Opacity / 255.0f
						,
						m_parentProjectedLayer.World.TerrainAccessor);
					
					ms.Close();
				}

		/*	}
			else if(imageFile.Exists || numberPolygonsInTile > 0)
			{
				string id = System.DateTime.Now.Ticks.ToString();

				if(b != null)
				{
					MemoryStream ms = new MemoryStream();
					b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
					if(!imageFile.Directory.Exists)
						imageFile.Directory.Create();

					//must copy original stream into new stream, if not, error occurs, not sure why
					m_ImageStream = new MemoryStream(ms.GetBuffer());
					ImageHelper.ConvertToDxt3(m_ImageStream, imageFile.FullName);
					
					ms.Close();
				}

				imageLayer = new WorldWind.Renderable.ImageLayer(
					id,
					m_parentProjectedLayer.World,
					0,
					imageFile.FullName,
					//System.Drawing.Color.Black.ToArgb(),
					(float)south,
					(float)north,
					(float)west,
					(float)east,
					1.0f,//(float)m_parentProjectedLayer.Opacity / 255.0f,
					m_parentProjectedLayer.World.TerrainAccessor);
				
				imageLayer.TransparentColor = System.Drawing.Color.Black.ToArgb();
			}
		*/			
			if(b != null)
			{
				b.Dispose();
			}
			if(g != null)
			{
				g.Dispose();
			}

			b = null;
			g = null;

			//might not be necessary
			//GC.Collect();

			return imageLayer;
		}
		
		Stream m_ImageStream;

		public void Initialize(DrawArgs drawArgs)
		{
			try
			{
				m_Initializing = true;

				
				UpdateImageLayers(drawArgs);
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
			finally
			{
				m_Initializing = false;
				if(m_Disposing)
				{
					Dispose();
					m_Initialized = false;
				}
				else
				{
					m_Initialized = true;
				}
				
			}
		}

		private void UpdateImageLayers(DrawArgs drawArgs)
		{
			try
			{
				m_LastUpdate = System.DateTime.Now;

				if(m_NwImageLayer != null)
					m_NwImageLayer.Dispose();
			
				if(m_NeImageLayer != null)
					m_NeImageLayer.Dispose();
			
				if(m_SwImageLayer != null)
					m_SwImageLayer.Dispose();

				if(m_SeImageLayer != null)
					m_SeImageLayer.Dispose();

				double centerLatitude = 0.5 * (m_geographicBoundingBox.North + m_geographicBoundingBox.South);
				double centerLongitude = 0.5 * (m_geographicBoundingBox.West + m_geographicBoundingBox.East);

				m_NwImageLayer = CreateImageLayer(m_geographicBoundingBox.North, centerLatitude, m_geographicBoundingBox.West, centerLongitude, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					null,//ShapeTile.CachePath,
					"R",//ConfigurationLoader.GetRenderablePathString(m_parentProjectedLayer),
					Level + 1,
					2 * Row + 1,
					2 * Col));

				m_NeImageLayer = CreateImageLayer(m_geographicBoundingBox.North, centerLatitude, centerLongitude, m_geographicBoundingBox.East, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					null,//ShapeTile.CachePath,
					"R",//ConfigurationLoader.GetRenderablePathString(m_parentProjectedLayer),
					Level + 1,
					2 * Row + 1,
					2 * Col + 1));

				m_SwImageLayer = CreateImageLayer(centerLatitude, m_geographicBoundingBox.South, m_geographicBoundingBox.West, centerLongitude, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					null,//ShapeTile.CachePath,
					"R",//ConfigurationLoader.GetRenderablePathString(m_parentProjectedLayer),
					Level + 1,
					2 * Row,
					2 * Col));

				m_SeImageLayer = CreateImageLayer(centerLatitude, m_geographicBoundingBox.South, centerLongitude, m_geographicBoundingBox.East, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					null,//ShapeTile.CachePath,
					"R",//ConfigurationLoader.GetRenderablePathString(m_parentProjectedLayer),
					Level + 1,
					2 * Row,
					2 * Col + 1));

				if(m_NwImageLayer != null)
				{
					m_NwImageLayer.Initialize(drawArgs);
				}
				if(m_NeImageLayer != null)
				{
					m_NeImageLayer.Initialize(drawArgs);
				}
				if(m_SwImageLayer != null)
				{
					m_SwImageLayer.Initialize(drawArgs);
				}
				if(m_SeImageLayer != null)
				{
					m_SeImageLayer.Initialize(drawArgs);
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}

		System.DateTime m_LastUpdate = System.DateTime.Now;

		private void drawLineString(LineString lineString, Graphics g, GeographicBoundingBox dstBB, Size imageSize)
		{
			using(Pen p = new Pen(lineString.Color, lineString.LineWidth))
			{
				g.DrawLines(p,
							getScreenPoints(lineString.Coordinates, 0, lineString.Coordinates.Length, dstBB, imageSize));

			}
		}

		private void drawPolygon(Polygon polygon, Graphics g, GeographicBoundingBox dstBB, Size imageSize)
		{
			if(polygon.innerBoundaries != null && polygon.innerBoundaries.Length > 0)
			{
				if(polygon.Fill)
				{
					using(SolidBrush brush = new SolidBrush(polygon.PolgonColor))
					{
						g.FillPolygon(brush, getScreenPoints(polygon.outerBoundary.Points, 0, polygon.outerBoundary.Points.Length, dstBB, imageSize));
					}
				}
				
				if(polygon.Outline)
				{
					using(Pen p = new Pen(polygon.OutlineColor, polygon.LineWidth))
					{
						g.DrawPolygon(p, 
								getScreenPoints(polygon.outerBoundary.Points, 0, polygon.outerBoundary.Points.Length, dstBB, imageSize));					
					}
				}

				if(polygon.Fill)
				{
					using(SolidBrush brush = new SolidBrush(System.Drawing.Color.Black))
					{
						for(int i = 0; i < polygon.innerBoundaries.Length; i++)
						{
							g.FillPolygon(brush, 
								getScreenPoints(polygon.innerBoundaries[i].Points, 0, polygon.innerBoundaries[i].Points.Length, dstBB, imageSize)
								);
						}
					}
				}				
			}
			else
			{
				if(polygon.Fill)
				{
					using(SolidBrush brush = new SolidBrush(polygon.PolgonColor))
					{
						g.FillPolygon(brush, getScreenPoints(polygon.outerBoundary.Points, 0, polygon.outerBoundary.Points.Length, dstBB, imageSize));
					}
				}

				if(polygon.Outline)
				{
					using(Pen p = new Pen(polygon.OutlineColor, polygon.LineWidth))
					{
						g.DrawPolygon(p, getScreenPoints(polygon.outerBoundary.Points, 0, polygon.outerBoundary.Points.Length, dstBB, imageSize));
					}
				}
			}
		}

		private System.Drawing.Point[] getScreenPoints(Point3d[] sourcePoints, int offset, int length, GeographicBoundingBox dstBB, Size dstImageSize)
		{
			double degreesPerPixelX = (dstBB.East - dstBB.West) / (double)dstImageSize.Width;
			double degreesPerPixelY = (dstBB.North - dstBB.South) / (double)dstImageSize.Height;

			ArrayList screenPointList = new ArrayList();
			for(int i = offset; i < offset + length; i++)
			{
				double screenX = (sourcePoints[i].X - dstBB.West) / degreesPerPixelX;
				double screenY = (dstBB.North - sourcePoints[i].Y) / degreesPerPixelY;

				if(screenPointList.Count > 0)
				{
					Point v = (Point)screenPointList[screenPointList.Count - 1];
					if(v.X == (int)screenX && v.Y == (int)screenY)
					{
						continue;
					}
				}

				screenPointList.Add(new Point((int)screenX, (int)screenY));
			}

			if(screenPointList.Count <= 1)
				return new Point[] { new Point(0,0), new Point(0,0) };

			return (Point[])screenPointList.ToArray(typeof(Point));
		}
		
		private ProjectedVectorTile ComputeChild( DrawArgs drawArgs, double childSouth, double childNorth, double childWest, double childEast, double tileSize ) 
		{
			ProjectedVectorTile child = new ProjectedVectorTile(
				new GeographicBoundingBox(childNorth, childSouth, childWest, childEast),
				m_parentProjectedLayer);

			return child;
		}

		ProjectedVectorTile m_NorthWestChild;
		ProjectedVectorTile m_NorthEastChild;
		ProjectedVectorTile m_SouthWestChild;
		ProjectedVectorTile m_SouthEastChild;

		float m_verticalExaggeration = World.Settings.VerticalExaggeration;

		protected virtual void CreateMesh(GeographicBoundingBox geographicBoundingBox, int meshPointCount, ref CustomVertex.PositionColoredTextured[] vertices, ref short[] indices)
		{
			int upperBound = meshPointCount - 1;
			float scaleFactor = (float)1/upperBound;
			double latrange = Math.Abs(geographicBoundingBox.North - geographicBoundingBox.South);
			double lonrange;
			if(geographicBoundingBox.West < geographicBoundingBox.East)
				lonrange = geographicBoundingBox.East - geographicBoundingBox.West;
			else
				lonrange = 360.0f + geographicBoundingBox.East - geographicBoundingBox.West;

			double layerRadius = m_parentProjectedLayer.World.EquatorialRadius;

			int opacityColor = System.Drawing.Color.FromArgb(
				//m_parentProjectedLayer.Opacity,
				0,0,0).ToArgb();
			vertices = new CustomVertex.PositionColoredTextured[meshPointCount * meshPointCount];
			for(int i = 0; i < meshPointCount; i++)
			{
				for(int j = 0; j < meshPointCount; j++)
				{	
					double height = 0;
					if(m_parentProjectedLayer.World.TerrainAccessor != null)
						height = m_verticalExaggeration * m_parentProjectedLayer.World.TerrainAccessor.GetElevationAt(
							(double)geographicBoundingBox.North - scaleFactor * latrange * i,
							(double)geographicBoundingBox.West + scaleFactor * lonrange * j,
							(double)upperBound / latrange);

					Vector3 pos = MathEngine.SphericalToCartesian( 
						geographicBoundingBox.North - scaleFactor*latrange*i,
						geographicBoundingBox.West + scaleFactor*lonrange*j, 
						layerRadius + height);
					
					vertices[i*meshPointCount + j].X = pos.X;
					vertices[i*meshPointCount + j].Y = pos.Y;
					vertices[i*meshPointCount + j].Z = pos.Z;
					
					vertices[i*meshPointCount + j].Tu = j*scaleFactor;
					vertices[i*meshPointCount + j].Tv = i*scaleFactor;
					vertices[i*meshPointCount + j].Color = opacityColor;
				}
			}

			indices = new short[2 * upperBound * upperBound * 3];
			for(int i = 0; i < upperBound; i++)
			{
				for(int j = 0; j < upperBound; j++)
				{
					indices[(2*3*i*upperBound) + 6*j] = (short)(i*meshPointCount + j);
					indices[(2*3*i*upperBound) + 6*j + 1] = (short)((i+1)*meshPointCount + j);
					indices[(2*3*i*upperBound) + 6*j + 2] = (short)(i*meshPointCount + j+1);
	
					indices[(2*3*i*upperBound) + 6*j + 3] = (short)(i*meshPointCount + j+1);
					indices[(2*3*i*upperBound) + 6*j + 4] = (short)((i+1)*meshPointCount + j);
					indices[(2*3*i*upperBound) + 6*j + 5] = (short)((i+1)*meshPointCount + j+1);
				}
			}
		}

		public virtual void ComputeChildren(DrawArgs drawArgs)
		{
			float tileSize = (float)(0.5*(m_geographicBoundingBox.North - m_geographicBoundingBox.South));
			//TODO: Stop children computation at some lower level
			if(tileSize>0.0001)
			{

				double CenterLat = 0.5f*(m_geographicBoundingBox.North + m_geographicBoundingBox.South);
				double CenterLon = 0.5f*(m_geographicBoundingBox.East + m_geographicBoundingBox.West);
			
				if(m_NorthWestChild == null && m_NwImageLayer != null && m_Initialized)
				{
					m_NorthWestChild = ComputeChild(drawArgs, CenterLat, m_geographicBoundingBox.North, m_geographicBoundingBox.West, CenterLon, tileSize );
					m_NorthWestChild.Level = Level++;
					m_NorthWestChild.Row = 2 * Row + 1;
					m_NorthWestChild.Col = 2 * Col;
					
					m_NorthWestChild.Initialize(drawArgs);
				}

				if(m_NorthEastChild == null && m_NeImageLayer != null && m_Initialized)
				{
					m_NorthEastChild = ComputeChild(drawArgs, CenterLat, m_geographicBoundingBox.North, CenterLon, m_geographicBoundingBox.East, tileSize );
					m_NorthEastChild.Level = Level++;
					m_NorthEastChild.Row = 2 * Row + 1;
					m_NorthEastChild.Col = 2 * Col + 1;

					m_NorthEastChild.Initialize(drawArgs);
				}

				if(m_SouthWestChild == null && m_SwImageLayer != null && m_Initialized)
				{
					m_SouthWestChild = ComputeChild(drawArgs, m_geographicBoundingBox.South, CenterLat, m_geographicBoundingBox.West, CenterLon, tileSize );
					m_SouthWestChild.Level = Level++;
					m_SouthWestChild.Row = 2 * Row;
					m_SouthWestChild.Col = 2 * Col;

					m_SouthWestChild.Initialize(drawArgs);
				}

				if(m_SouthEastChild == null && m_SeImageLayer != null && m_Initialized)
				{
					m_SouthEastChild = ComputeChild(drawArgs, m_geographicBoundingBox.South, CenterLat, CenterLon, m_geographicBoundingBox.East, tileSize );
					m_SouthEastChild.Level = Level++;
					m_SouthEastChild.Row = 2 * Row;
					m_SouthEastChild.Col = 2 * Col + 1;

					m_SouthEastChild.Initialize(drawArgs);
				}
			}
		}

		public void Update(DrawArgs drawArgs)
		{
			try
			{
				double centerLatitude = 0.5 * (m_geographicBoundingBox.North + m_geographicBoundingBox.South);
				double centerLongitude = 0.5 * (m_geographicBoundingBox.West + m_geographicBoundingBox.East);
				double tileSize = m_geographicBoundingBox.North - m_geographicBoundingBox.South;

				if(!m_Initialized)
				{
					if(drawArgs.WorldCamera.ViewRange * 0.5f < Angle.FromDegrees(ShapeTileArgs.TileDrawDistance * tileSize) 
						&& MathEngine.SphericalDistance(Angle.FromDegrees(centerLatitude), Angle.FromDegrees(centerLongitude), 
						drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees( ShapeTileArgs.TileSpreadFactor * tileSize * 1.25f )
						&& drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
						)
					{
						Initialize(drawArgs);
					}
				}

				if(m_Initialized)
				{
					if(m_LastUpdate < m_parentProjectedLayer.LastUpdate)
						UpdateImageLayers(drawArgs);

					if(m_NwImageLayer != null)
					{
						m_NwImageLayer.Update(drawArgs);
					}
					if(m_NeImageLayer != null)
					{
						m_NeImageLayer.Update(drawArgs);
					}
					if(m_SwImageLayer != null)
					{
						m_SwImageLayer.Update(drawArgs);
					}
					if(m_SeImageLayer != null)
					{
						m_SeImageLayer.Update(drawArgs);
					}

					if(
						drawArgs.WorldCamera.ViewRange < Angle.FromDegrees(ShapeTileArgs.TileDrawDistance*tileSize) && 
						MathEngine.SphericalDistance( Angle.FromDegrees(centerLatitude), Angle.FromDegrees(centerLongitude), 
						drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees( ShapeTileArgs.TileSpreadFactor*tileSize )
						&& drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
						)
					{
						if(m_NorthEastChild == null && m_NorthWestChild == null && m_SouthEastChild == null && m_SouthWestChild == null)
						{
							ComputeChildren(drawArgs);
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

				if(m_Initialized)
				{
					if(drawArgs.WorldCamera.ViewRange > Angle.FromDegrees( ShapeTileArgs.TileDrawDistance*tileSize*1.5f )
						|| MathEngine.SphericalDistance(Angle.FromDegrees(centerLatitude), Angle.FromDegrees(centerLongitude), drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) > Angle.FromDegrees( ShapeTileArgs.TileSpreadFactor*tileSize*1.5f ))
					{
						if(this.Level != 0)
						//{
						Dispose();
						//}
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}

		public void Render(DrawArgs drawArgs)
		{
			try
			{
				if(m_Initialized)
				{
					if(m_NorthWestChild != null && m_NorthWestChild.m_Initialized)
					{
						m_NorthWestChild.Render(drawArgs);
					}
					else if(m_NwImageLayer != null && m_NwImageLayer.Initialized)
					{
						m_NwImageLayer.Render(drawArgs);
					}

					if(m_NorthEastChild != null && m_NorthEastChild.m_Initialized)
					{
						m_NorthEastChild.Render(drawArgs);
					}
					else if(m_NeImageLayer != null && m_NeImageLayer.Initialized)
					{
						m_NeImageLayer.Render(drawArgs);
					}

					if(m_SouthWestChild != null && m_SouthWestChild.m_Initialized)
					{
						m_SouthWestChild.Render(drawArgs);
					}
					else if(m_SwImageLayer != null && m_SwImageLayer.Initialized)
					{
						m_SwImageLayer.Render(drawArgs);
					}

					if(m_SouthEastChild != null && m_SouthEastChild.m_Initialized)
					{
						m_SouthEastChild.Render(drawArgs);
					}
					else if(m_SeImageLayer != null && m_SeImageLayer.Initialized)
					{
						m_SeImageLayer.Render(drawArgs);
					}
				}
			}
			catch//(Exception ex)
			{
				//Log.Write(ex);
			}
		}
	}
}
