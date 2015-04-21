using System;
using System.Collections;
using System.Drawing;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ShapeTile.
	/// </summary>
	public class ShapeTile
	{
		public static string CachePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Cache");
		bool m_Initialized = false;
		bool m_Initializing = false;
		ShapeTileArgs m_ShapeTileArgs;
		bool m_Disposing = false;

		public int Level = 0;
		public int Row = 0;
		public int Col = 0;
		//	SurfaceImage m_NwSurfaceImage;
		//	SurfaceImage m_NeSurfaceImage;
		//	SurfaceImage m_SwSurfaceImage;
		//	SurfaceImage m_SeSurfaceImage;
		Renderable.ImageLayer m_NwImageLayer;
		Renderable.ImageLayer m_NeImageLayer;
		Renderable.ImageLayer m_SwImageLayer;
		Renderable.ImageLayer m_SeImageLayer;

		public GeographicBoundingBox m_GeoBB;

		public BoundingBox BoundingBox;
		
		public ShapeTile(
			GeographicBoundingBox geoBB,
			ShapeTileArgs shapeTileArgs
			)
		{
			m_GeoBB = geoBB;
			m_ShapeTileArgs = shapeTileArgs;

			BoundingBox = new BoundingBox( (float)geoBB.South, (float)geoBB.North, (float)geoBB.West, (float)geoBB.East, 
				(float)m_ShapeTileArgs.LayerRadius, (float)m_ShapeTileArgs.LayerRadius + 300000f);
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

		private bool isShapeRecordInBounds(GeographicBoundingBox geoBB, ShapeRecord record)
		{
			if(record.Point != null)
			{
				if(record.Point.X < geoBB.West || record.Point.X > geoBB.East ||
					record.Point.Y > geoBB.North || record.Point.Y < geoBB.South)
				{
					return false;
				}
				else
				{	
					return true;
				}
			}
			else if(record.MultiPoint != null)
			{
				if(record.MultiPoint.BoundingBox.North <= geoBB.South ||
					record.MultiPoint.BoundingBox.South >= geoBB.North ||
					record.MultiPoint.BoundingBox.West >= geoBB.East ||
					record.MultiPoint.BoundingBox.East <= geoBB.West)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else if(record.PolyLine != null)
			{
				if(record.PolyLine.BoundingBox.North <= geoBB.South ||
					record.PolyLine.BoundingBox.South >= geoBB.North ||
					record.PolyLine.BoundingBox.West >= geoBB.East ||
					record.PolyLine.BoundingBox.East <= geoBB.West)
				{
					return false;
				}
				else
				{
					return true;
				}	
			}
			else if(record.Polygon != null)
			{
				if(record.Polygon.BoundingBox.North <= geoBB.South ||
					record.Polygon.BoundingBox.South >= geoBB.North ||
					record.Polygon.BoundingBox.West >= geoBB.East ||
					record.Polygon.BoundingBox.East <= geoBB.West)
				{
					return false;
				}
				else
				{
					return true;
				}	
			}

			return false;
		}


		public byte Opacity
		{
			get
			{
				return m_ShapeTileArgs.ParentShapeFileLayer.Opacity;
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
		}

		private Renderable.ImageLayer CreateImageLayer(double north, double south, double west, double east, DrawArgs drawArgs, string imagePath)
		{
			Bitmap b = null;
			Graphics g = null;
			Renderable.ImageLayer imageLayer = null;
			GeographicBoundingBox geoBB = new GeographicBoundingBox(north, south, west, east);
	
			int numberPolygonsInTile = 0;

			FileInfo imageFile = new FileInfo(imagePath);
			FileInfo shapeFile = new FileInfo(m_ShapeTileArgs.ParentShapeFileLayer.ShapeFilePath);
			FileInfo shapeXmlFile = null;
			if(m_ShapeTileArgs.ParentShapeFileLayer.MetaData.Contains("SourceXml"))
			{
				string sourceXml = (string)m_ShapeTileArgs.ParentShapeFileLayer.MetaData["SourceXml"];
				if(!sourceXml.ToLower().StartsWith("http://"))
				{
					shapeXmlFile = new FileInfo(sourceXml);
				}
			}

			if(!m_ShapeTileArgs.ParentShapeFileLayer.EnableCaching ||
				!imageFile.Exists || 
				shapeXmlFile == null ||
				shapeXmlFile.LastWriteTimeUtc > imageFile.LastWriteTimeUtc ||
				shapeFile.LastWriteTimeUtc > imageFile.LastWriteTimeUtc
				)
			{
				for(int i = 0; i < m_ShapeTileArgs.ShapeRecords.Count; i++)
				{
					ShapeRecord currentRecord = (ShapeRecord)m_ShapeTileArgs.ShapeRecords[i];
				
					if(currentRecord.Null != null || 
						currentRecord.Point != null || 
						currentRecord.MultiPoint != null ||
						!isShapeRecordInBounds(geoBB, currentRecord))
					{
						continue;
					}
					else
					{
						if(b == null)
						{
							b = new Bitmap(m_ShapeTileArgs.TilePixelSize.Width,
								m_ShapeTileArgs.TilePixelSize.Height,
								System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						}

						if(g == null)
						{
							g = Graphics.FromImage(b);
						}

						System.Drawing.Color color = m_ShapeTileArgs.PolygonColor;

						//Fix Black Tiles
						g.DrawLine(new Pen(color),0,0,1,1);
					
					
						if(m_ShapeTileArgs.UseScalar && m_ShapeTileArgs.ScaleColors)
						{
							double red = 1.0;
							double green = 1.0;
							double blue = 1.0;
							
							try
							{
								//TODO: make this a function and abstract to allow multiple gradient mappings
								double dv;

								double curScalar = double.Parse(currentRecord.Value.ToString());

								if (curScalar < m_ShapeTileArgs.ScaleMin)
									curScalar = m_ShapeTileArgs.ScaleMin;
								if (curScalar > m_ShapeTileArgs.ScaleMax)
									curScalar = m_ShapeTileArgs.ScaleMax;
							
								dv = m_ShapeTileArgs.ScaleMax - m_ShapeTileArgs.ScaleMin;

								if (curScalar < (m_ShapeTileArgs.ScaleMin + 0.25 * dv)) 
								{
									red = 0;
									green = 4 * (curScalar - m_ShapeTileArgs.ScaleMin) / dv;
								} 
								else if (curScalar < (m_ShapeTileArgs.ScaleMin + 0.5 * dv)) 
								{
									red = 0;
									blue = 1 + 4 * (m_ShapeTileArgs.ScaleMin + 0.25 * dv - curScalar) / dv;
								} 
								else if (curScalar < (m_ShapeTileArgs.ScaleMin + 0.75 * dv)) 
								{
									red = 4 * (curScalar - m_ShapeTileArgs.ScaleMin - 0.5 * dv) / dv;
									blue = 0;
								} 
								else 
								{
									green = 1 + 4 * (m_ShapeTileArgs.ScaleMin + 0.75 * dv - curScalar) / dv;
									blue = 0;
								}

								color = System.Drawing.Color.FromArgb((int)(255*red), (int)(255*green), (int)(255*blue));
							
							}
							catch(Exception)
							{
								//	Utility.Log.Write((string)currentPoly.ScalarHash[m_ShapeTileArgs.ColorKey]);
								//	Utility.Log.Write(String.Format("Min: {0}, Max: {1}", m_ShapeTileArgs.ScaleMin, m_ShapeTileArgs.ScaleMax));
								//	Utility.Log.Write(String.Format("{0},{1},{2}", red, green, blue));
								//	Utility.Log.Write(ex);
							}
						}
						else
						{
							if(m_ShapeTileArgs.ColorAssignments.Count > 0 && m_ShapeTileArgs.ScaleColors)
							{
								try
								{
									string colorAssignmentKey = (string)currentRecord.Value;
									foreach(string cak in m_ShapeTileArgs.ColorAssignments.Keys)
									{
										if(String.Compare(cak, colorAssignmentKey, true) == 0)
										{
											color = (System.Drawing.Color)m_ShapeTileArgs.ColorAssignments[cak];
											break;
										}
									}
								}
								catch(Exception)
								{
								}
							}
						}
					
						if(currentRecord.Polygon != null)
						{
							drawPolygon(currentRecord.Polygon,
								g,
								color,
								geoBB,
								b.Size);
						}
					
						if(m_ShapeTileArgs.ColorAssignments.Count == 0 ||  
							!m_ShapeTileArgs.ScaleColors)
						{
							color = m_ShapeTileArgs.LineColor;
						}

						if(currentRecord.PolyLine != null)
						{
							drawPolyLine(currentRecord.PolyLine,
								g,
								color,
								geoBB,
								b.Size);
						}
						numberPolygonsInTile++;
					}
				}
			}
            
			if(!m_ShapeTileArgs.ParentShapeFileLayer.EnableCaching)
			{
				string id = System.DateTime.Now.Ticks.ToString();

				if(b != null)
				{
					MemoryStream ms = new MemoryStream();
					
					//must copy original stream into new stream, if not, error occurs, not sure why
					m_ImageStream = new MemoryStream(ms.GetBuffer());
					
					imageLayer = new WorldWind.Renderable.ImageLayer(
						id,
						m_ShapeTileArgs.ParentWorld,
						0,
						m_ImageStream,
						System.Drawing.Color.Black.ToArgb(),
						(float)south,
						(float)north,
						(float)west,
						(float)east,
						(float)m_ShapeTileArgs.ParentShapeFileLayer.Opacity / 255.0f,
						m_ShapeTileArgs.ParentWorld.TerrainAccessor);
					
					ms.Close();
				}
			}
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
					m_ShapeTileArgs.ParentWorld,
					0,// should be distance above surface
					imageFile.FullName,//m_ImageStream,
					//0,//System.Drawing.Color.Black.ToArgb(),
					(float)south,
					(float)north,
					(float)west,
					(float)east,
					(float)m_ShapeTileArgs.ParentShapeFileLayer.Opacity / 255.0f,
					m_ShapeTileArgs.ParentWorld.TerrainAccessor);
			}
					
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

				double centerLatitude = 0.5 * (m_GeoBB.North + m_GeoBB.South);
				double centerLongitude = 0.5 * (m_GeoBB.West + m_GeoBB.East);

				m_NwImageLayer = CreateImageLayer(m_GeoBB.North, centerLatitude, m_GeoBB.West, centerLongitude, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					ShapeTile.CachePath,
					ConfigurationLoader.GetRenderablePathString(m_ShapeTileArgs.ParentShapeFileLayer),
					Level + 1,
					2 * Row + 1,
					2 * Col));

				m_NeImageLayer = CreateImageLayer(m_GeoBB.North, centerLatitude, centerLongitude, m_GeoBB.East, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					ShapeTile.CachePath,
					ConfigurationLoader.GetRenderablePathString(m_ShapeTileArgs.ParentShapeFileLayer),
					Level + 1,
					2 * Row + 1,
					2 * Col + 1));

				m_SwImageLayer = CreateImageLayer(centerLatitude, m_GeoBB.South, m_GeoBB.West, centerLongitude, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					ShapeTile.CachePath,
					ConfigurationLoader.GetRenderablePathString(m_ShapeTileArgs.ParentShapeFileLayer),
					Level + 1,
					2 * Row,
					2 * Col));

				m_SeImageLayer = CreateImageLayer(centerLatitude, m_GeoBB.South, centerLongitude, m_GeoBB.East, drawArgs,
					String.Format("{0}\\{1}\\{2}\\{3:D4}\\{3:D4}_{4:D4}.dds",
					ShapeTile.CachePath,
					ConfigurationLoader.GetRenderablePathString(m_ShapeTileArgs.ParentShapeFileLayer),
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
				Utility.Log.Write(ex);
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

		private void drawPolyLine(Shapefile_PolyLine polyLine, Graphics g, Color c, GeographicBoundingBox dstBB, Size imageSize)
		{
			using(Pen p = new Pen(c, m_ShapeTileArgs.LineWidth))
			{
				p.Color = c;
				if(polyLine.Parts.Length > 1)
				{
					for(int partsItr = 0; partsItr < polyLine.Parts.Length - 1; partsItr++)
					{
						g.DrawLines(p,
							getScreenPoints(polyLine.Points, polyLine.Parts[partsItr], polyLine.Parts[partsItr + 1] - polyLine.Parts[partsItr], dstBB, imageSize));

					}

					g.DrawLines(p,
						getScreenPoints(polyLine.Points, polyLine.Parts[polyLine.Parts.Length - 1],
						polyLine.Points.Length - polyLine.Parts[polyLine.Parts.Length - 1], dstBB, imageSize));
				}
				else
				{
					g.DrawLines(p, getScreenPoints(polyLine.Points, 0, polyLine.Points.Length, dstBB, imageSize));
				}
			}
		}

		private void drawPolygon(Shapefile_Polygon polygon, Graphics g, Color c, GeographicBoundingBox dstBB, Size imageSize)
		{
			if(polygon.Parts.Length > 1)
			{
				if(m_ShapeTileArgs.PolygonFill)
				{
					if(m_ShapeTileArgs.ShapeFillStyle == ShapeFillStyle.Solid)
					{
						using(SolidBrush brush = new SolidBrush(c))
						{
							g.FillPolygon(brush, getScreenPoints(polygon.Points, 0, polygon.Parts[1], dstBB, imageSize));
						}
					}
					else
					{
						using(System.Drawing.Drawing2D.HatchBrush brush = new System.Drawing.Drawing2D.HatchBrush(
								  getGDIHatchStyle(m_ShapeTileArgs.ShapeFillStyle),
								  c,
								  System.Drawing.Color.Black))
						{
							g.FillPolygon(brush, getScreenPoints(polygon.Points, 0, polygon.Parts[1], dstBB, imageSize));
						}
					}
				}
				
				if(m_ShapeTileArgs.OutlinePolygons)
				{
					using(Pen p = new Pen(m_ShapeTileArgs.LineColor, m_ShapeTileArgs.LineWidth))
					{
						for(int partsItr = 0; partsItr < polygon.Parts.Length - 1; partsItr++)
						{
							g.DrawPolygon(p, 
								getScreenPoints(polygon.Points, polygon.Parts[partsItr], polygon.Parts[partsItr+1] - polygon.Parts[partsItr], dstBB, imageSize));
						}

						g.DrawPolygon(p, 
							getScreenPoints(polygon.Points, polygon.Parts[polygon.Parts.Length - 1],
							polygon.Points.Length - polygon.Parts[polygon.Parts.Length - 1], dstBB, imageSize)
							);
					}
				}

				if(m_ShapeTileArgs.PolygonFill)
				{
					if(m_ShapeTileArgs.ShapeFillStyle == ShapeFillStyle.Solid)
					{
						using(SolidBrush brush = new SolidBrush(System.Drawing.Color.Black))
						{
							for(int partsItr = 1; partsItr < polygon.Parts.Length - 1; partsItr++)
							{
								g.FillPolygon(brush, 
									getScreenPoints(polygon.Points, polygon.Parts[partsItr], polygon.Parts[partsItr+1] - polygon.Parts[partsItr], dstBB, imageSize)
									);
							}

							g.FillPolygon(brush, 
								getScreenPoints(polygon.Points, polygon.Parts[polygon.Parts.Length - 1],
								polygon.Points.Length - polygon.Parts[polygon.Parts.Length - 1], dstBB, imageSize)
								);
						}
					}
					else
					{
						using(System.Drawing.Drawing2D.HatchBrush brush = new System.Drawing.Drawing2D.HatchBrush(
								  getGDIHatchStyle(m_ShapeTileArgs.ShapeFillStyle),
								  c,
								  System.Drawing.Color.Black))
						{
							for(int partsItr = 1; partsItr < polygon.Parts.Length - 1; partsItr++)
							{
								g.FillPolygon(brush, 
									getScreenPoints(polygon.Points, polygon.Parts[partsItr], polygon.Parts[partsItr+1] - polygon.Parts[partsItr], dstBB, imageSize)
									);
							}

							g.FillPolygon(brush, 
								getScreenPoints(polygon.Points, polygon.Parts[polygon.Parts.Length - 1],
								polygon.Points.Length - polygon.Parts[polygon.Parts.Length - 1], dstBB, imageSize)
								);
						}
					}
				}				
			}
			else
			{
				if(m_ShapeTileArgs.PolygonFill)
				{
					if(m_ShapeTileArgs.ShapeFillStyle == ShapeFillStyle.Solid)
					{
						using(SolidBrush brush = new SolidBrush(c))
						{
							g.FillPolygon(brush, getScreenPoints(polygon.Points, 0, polygon.Points.Length, dstBB, imageSize));
						}
					}
					else
					{
						using(System.Drawing.Drawing2D.HatchBrush brush = new System.Drawing.Drawing2D.HatchBrush(
								  getGDIHatchStyle(m_ShapeTileArgs.ShapeFillStyle),
								  c,
								  System.Drawing.Color.Black))
						{
							g.FillPolygon(brush, getScreenPoints(polygon.Points, 0, polygon.Points.Length, dstBB, imageSize));
						}
					}
				}

				
				if(m_ShapeTileArgs.OutlinePolygons)
				{
					using(Pen p = new Pen(m_ShapeTileArgs.LineColor, m_ShapeTileArgs.LineWidth))
					{
						g.DrawPolygon(p, getScreenPoints(polygon.Points, 0, polygon.Points.Length, dstBB, imageSize));
					}
				}
			}
		}

		private System.Drawing.Point[] getScreenPoints(Shapefile_Point[] sourcePoints, int offset, int length, GeographicBoundingBox dstBB, Size dstImageSize)
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
		
		private ShapeTile ComputeChild( DrawArgs drawArgs, double childSouth, double childNorth, double childWest, double childEast, double tileSize ) 
		{
			ShapeTile child = new ShapeTile(
				new GeographicBoundingBox(childNorth, childSouth, childWest, childEast),
				m_ShapeTileArgs);

			return child;
		}

		ShapeTile m_NorthWestChild;
		ShapeTile m_NorthEastChild;
		ShapeTile m_SouthWestChild;
		ShapeTile m_SouthEastChild;

		public virtual void ComputeChildren(DrawArgs drawArgs)
		{
			
			float tileSize = (float)(0.5*(m_GeoBB.North - m_GeoBB.South));
			//TODO: Stop children computation at some lower level
			if(tileSize>0.0001)
			{

				double CenterLat = 0.5f*(m_GeoBB.North + m_GeoBB.South);
				double CenterLon = 0.5f*(m_GeoBB.East + m_GeoBB.West);
			
				if(m_NorthWestChild == null && m_NwImageLayer != null && m_Initialized)
				{
					m_NorthWestChild = ComputeChild(drawArgs, CenterLat, m_GeoBB.North, m_GeoBB.West, CenterLon, tileSize );
					m_NorthWestChild.Level = Level++;
					m_NorthWestChild.Row = 2 * Row + 1;
					m_NorthWestChild.Col = 2 * Col;
					
					m_NorthWestChild.Initialize(drawArgs);
				}

				if(m_NorthEastChild == null && m_NeImageLayer != null && m_Initialized)
				{
					m_NorthEastChild = ComputeChild(drawArgs, CenterLat, m_GeoBB.North, CenterLon, m_GeoBB.East, tileSize );
					m_NorthEastChild.Level = Level++;
					m_NorthEastChild.Row = 2 * Row + 1;
					m_NorthEastChild.Col = 2 * Col + 1;

					m_NorthEastChild.Initialize(drawArgs);
				}

				if(m_SouthWestChild == null && m_SwImageLayer != null && m_Initialized)
				{
					m_SouthWestChild = ComputeChild(drawArgs, m_GeoBB.South, CenterLat, m_GeoBB.West, CenterLon, tileSize );
					m_SouthWestChild.Level = Level++;
					m_SouthWestChild.Row = 2 * Row;
					m_SouthWestChild.Col = 2 * Col;

					m_SouthWestChild.Initialize(drawArgs);
				}

				if(m_SouthEastChild == null && m_SeImageLayer != null && m_Initialized)
				{
					m_SouthEastChild = ComputeChild(drawArgs, m_GeoBB.South, CenterLat, CenterLon, m_GeoBB.East, tileSize );
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
				double centerLatitude = 0.5 * (m_GeoBB.North + m_GeoBB.South);
				double centerLongitude = 0.5 * (m_GeoBB.West + m_GeoBB.East);
				double tileSize = m_GeoBB.North - m_GeoBB.South;

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
						//if(this.level != 0)
						//{
						Dispose();
						//}
					}
				}
			}
			catch(Exception ex)
			{
				Utility.Log.Write(ex);
			}
		}

		public void Render(DrawArgs drawArgs)
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
	}

}
