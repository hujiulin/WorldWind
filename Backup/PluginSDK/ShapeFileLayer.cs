using System;
using System.Collections;
using System.Drawing;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ICSharpCode.SharpZipLib.Zip;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ShapeFileLayer.
	/// </summary>
	/// 

	//TODO: Upper and lower random color limits, line "caps" styles
	public class ShapeFileLayer : Renderable.RenderableObject
	{
		int m_NumberRootTilesHigh = 5;
		ShapeTileArgs m_ShapeTileArgs;
		ShapeTile[] m_RootTiles;
		string m_ShapeFilePath;
		
		double m_BoundingBoxXMin;
		double m_BoundingBoxYMin;
		double m_BoundingBoxXMax;
		double m_BoundingBoxYMax;
		double m_BoundingBoxZMin;
		double m_BoundingBoxZMax;
		double m_BoundingBoxMMin;
		double m_BoundingBoxMMax;

		double m_ScalarFilterMin = double.NaN;
		double m_ScalarFilterMax = double.NaN;
		double m_MinimumViewingAltitude = 0;
		double m_MaximumViewingAltitude = double.MaxValue;
		double m_lztsd = 36.0;


		int m_IconWidth = 0;
		int m_IconHeight = 0;
		string m_IconFilePath = null;
		Texture m_IconTexture = null;
		SurfaceDescription m_IconTextureDescription;

		byte m_IconOpacity = 255;

        public double North
        {
            get
            {
                return m_BoundingBoxYMax;
            }
        }

        public double South
        {
            get
            {
                return m_BoundingBoxYMin;
            }
        }

        public double East
        {
            get
            {
                return m_BoundingBoxXMax;
            }
        }

        public double West
        {
            get
            {
                return m_BoundingBoxXMin;
            }
        }

        public double MinAltitude
        {
            get
            {
                return m_MinimumViewingAltitude;
            }
        }

        public double MaxAltitude
        {
            get
            {
                return m_MaximumViewingAltitude;
            }
        }

		public ShapeFileLayer(
			string id,
			World parentWorld,
			string shapeFilePath,
			double minimumViewingAltitude,
			double maximumViewingAltitude,
			float lztsd,
			GeographicBoundingBox bounds,
			string dataKey,
			bool scaleColorsToData,
			double scalarFilterMin,
			double scalarFilterMax,
			double scaleMin,
			double scaleMax,
			string[] noDataValues,
			string[] activeDataValues,
			bool polygonFill,
			bool outlinePolygons,
			System.Drawing.Color polygonFillColor,
			ShapeFillStyle shapeFillHatchStyle,
			System.Drawing.Color lineColor,
			float lineWidth,
			bool showLabels,
			System.Drawing.Color labelColor,
			string iconFilePath,
			int iconWidth,
			int iconHeight,
			byte iconOpacity) : base(id, parentWorld.Position, parentWorld.Orientation)
		{

			this.RenderPriority = WorldWind.Renderable.RenderPriority.LinePaths;

			m_MinimumViewingAltitude = minimumViewingAltitude;
			m_MaximumViewingAltitude = maximumViewingAltitude;
			m_lztsd = lztsd;

			m_ShapeTileArgs = new ShapeTileArgs(
				parentWorld,
				new System.Drawing.Size(256, 256),
				parentWorld.EquatorialRadius,
				this,
				dataKey,
				scaleColorsToData,
				scaleMin,
				scaleMax,
				noDataValues,
				activeDataValues,
				polygonFill,
				outlinePolygons,
				polygonFillColor,
				shapeFillHatchStyle,
				lineColor,
				labelColor,
				lineWidth,
				showLabels
				);

			m_ScalarFilterMin = scalarFilterMin;
			m_ScalarFilterMax = scalarFilterMax;

			m_ShapeFilePath = shapeFilePath;

			m_IconFilePath = iconFilePath;
			m_IconWidth = iconWidth;
			m_IconHeight = iconHeight;
			m_IconOpacity = iconOpacity;
			/*Produces tile tree for whole earth*/
			/*Need to implement clipping*/			
			m_NumberRootTilesHigh = (int)(180.0f/m_lztsd);
			double tileSize = 180.0f/m_NumberRootTilesHigh;
			m_RootTiles = new ShapeTile[m_NumberRootTilesHigh * (m_NumberRootTilesHigh * 2)];

			System.Console.WriteLine("North:{0} South:{1} East:{2} West:{3}",
				bounds.North,bounds.South,bounds.East,bounds.West);
			int istart = 0;
			int iend = m_NumberRootTilesHigh;
			int jstart = 0;
			int jend = m_NumberRootTilesHigh * 2;

			int createdtiles = 0;
			for(int i = istart; i < iend; i++)
			{
				for(int j = jstart; j < jend; j++)
				{
					double north = (i + 1) * tileSize - 90.0f;
					double south = i  * tileSize - 90.0f;
					double west = j * tileSize - 180.0f;
					double east = (j + 1) * tileSize - 180.0f;
					m_RootTiles[i * m_NumberRootTilesHigh * 2 + j] = new ShapeTile(
							new GeographicBoundingBox(
							north,
							south,
							west,
							east),
							m_ShapeTileArgs);
					createdtiles++;
				}
			}
			Console.WriteLine("Created Tiles "+createdtiles);
		}

		public override void Dispose()
		{
			isInitialized = false;
			if(m_IconTexture != null && !m_IconTexture.Disposed)
			{
				m_IconTexture.Dispose();
			}
			if(m_Sprite != null && !m_Sprite.Disposed)
			{
				m_Sprite.Dispose();
			}
			foreach(ShapeTile shapeTile in m_RootTiles)
			{
				shapeTile.Dispose();
			}
		}

		public override byte Opacity 
		{
			get
			{
				return base.Opacity;
			}
			set
			{
				if(m_RootTiles != null)
				{
					foreach(ShapeTile tile in m_RootTiles)
					{
						if(tile != null)
						{
							tile.Opacity = value;
						}
					}
				}
				base.Opacity = value;	
			}
		}

		Sprite m_Sprite = null;
		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				m_Sprite = new Sprite(drawArgs.device);
				if(m_IconFilePath != null && File.Exists(m_IconFilePath))
				{
					m_IconTexture = ImageHelper.LoadIconTexture(m_IconFilePath);
					m_IconTextureDescription = m_IconTexture.GetLevelDescription(0);
				}
				if(m_ShapeFilePath.ToLower().EndsWith(".zip"))
				{					
					loadZippedShapeFile(m_ShapeFilePath);
				}
				else
				{					
					loadShapeFile(m_ShapeFilePath);
				}

				if((m_ShapeTileArgs.ShowLabels && m_ShapeTileArgs.DataKey != null) || m_IconTexture != null)
				{
					foreach(ShapeRecord record in m_ShapeTileArgs.ShapeRecords)
					{
						if(record.Value != null)
						{
							if(record.Point != null)
							{
								Shapefile_Point p = new Shapefile_Point();
								p.X = record.Point.X;
								p.Y = record.Point.Y;
								p.Tag = record.Value;
								m_LabelList.Add(p);
							}
							else if(record.MultiPoint != null)
							{
								Shapefile_Point p = new Shapefile_Point();
								p.X = 0.5 * (record.MultiPoint.BoundingBox.West + record.MultiPoint.BoundingBox.East);
								p.Y = 0.5 * (record.MultiPoint.BoundingBox.North + record.MultiPoint.BoundingBox.South);
								p.Tag = record.Value;
								m_LabelList.Add(p);
							}
							else if(record.PolyLine != null)
							{
								Shapefile_Point p = new Shapefile_Point();
								p.X = 0.5 * (record.PolyLine.BoundingBox.West + record.PolyLine.BoundingBox.East);
								p.Y = 0.5 * (record.PolyLine.BoundingBox.North + record.PolyLine.BoundingBox.South);
								p.Tag = record.Value;
								m_LabelList.Add(p);
							}
							else if(record.Polygon != null)
							{
								Shapefile_Point p = new Shapefile_Point();
								p.X = 0.5 * (record.Polygon.BoundingBox.West + record.Polygon.BoundingBox.East);
								p.Y = 0.5 * (record.Polygon.BoundingBox.North + record.Polygon.BoundingBox.South);
								p.Tag = record.Value;
								m_LabelList.Add(p);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
			finally
			{
				isInitialized = true;
			}
		}

		System.Collections.ArrayList m_LabelList = new ArrayList();
		System.Collections.ArrayList m_PointList = new ArrayList();

		public override void Update(DrawArgs drawArgs)
		{
			if(drawArgs.WorldCamera.AltitudeAboveTerrain >= m_MinimumViewingAltitude &&
				drawArgs.WorldCamera.AltitudeAboveTerrain <= m_MaximumViewingAltitude)
			{
				if(!isInitialized)
				{
					Initialize(drawArgs);
				}

				foreach(ShapeTile shapeTile in m_RootTiles)
				{
					if(shapeTile!=null&&(shapeTile.m_GeoBB.North-shapeTile.m_GeoBB.South)<=m_lztsd)
						shapeTile.Update(drawArgs);
				}
			}
			else
			{
				if(isInitialized)
				{
					Dispose();
				}
			}

		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isInitialized || 
				drawArgs.WorldCamera.AltitudeAboveTerrain < m_MinimumViewingAltitude ||
				drawArgs.WorldCamera.AltitudeAboveTerrain > m_MaximumViewingAltitude
				)
			{
				return;
			}

			try
			{
				foreach(ShapeTile shapeTile in m_RootTiles)
				{
					if(shapeTile!=null&&(shapeTile.m_GeoBB.North-shapeTile.m_GeoBB.South)<=m_lztsd)
						shapeTile.Render(drawArgs);
				}

				Vector3 referenceCenter = new Vector3(
					(float)drawArgs.WorldCamera.ReferenceCenter.X,
					(float)drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)drawArgs.WorldCamera.ReferenceCenter.Z);

				if(m_PointList.Count > 0)
				{
					drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
						
					float curPointSize = drawArgs.device.RenderState.PointSize;
						
					drawArgs.device.RenderState.PointSize = 5.0f;
					drawArgs.device.RenderState.ZBufferEnable = false;
					CustomVertex.PositionColored[] verts = new Microsoft.DirectX.Direct3D.CustomVertex.PositionColored[1];
					Vector3 camPoint = MathEngine.SphericalToCartesian(drawArgs.WorldCamera.Latitude.Degrees, drawArgs.WorldCamera.Longitude.Degrees, m_ShapeTileArgs.LayerRadius);
					
					drawArgs.device.Transform.World = Matrix.Translation(-referenceCenter);
					foreach(Vector3 v in m_PointList)
					{
						if(Vector3.Subtract(v, camPoint).Length() < m_ShapeTileArgs.LayerRadius)
						{
							verts[0].Color = m_ShapeTileArgs.LabelColor.ToArgb();
							verts[0].X = v.X;
							verts[0].Y = v.Y;
							verts[0].Z = v.Z;

							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
							drawArgs.device.DrawUserPrimitives(PrimitiveType.PointList, 1, verts);
						}
					}

					drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
					drawArgs.device.RenderState.PointSize = curPointSize;
					drawArgs.device.RenderState.ZBufferEnable = true;
				}

				if(m_LabelList.Count > 0)
				{
					System.Drawing.Color iconColor = System.Drawing.Color.FromArgb(m_IconOpacity, 255, 255, 255);
					foreach(Shapefile_Point p in m_LabelList)
					{
						Vector3 cartesianPoint = MathEngine.SphericalToCartesian(p.Y, p.X, drawArgs.WorldCamera.WorldRadius + drawArgs.WorldCamera.TerrainElevation);
					
						if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(cartesianPoint) ||
							MathEngine.SphericalDistanceDegrees(p.Y, p.X, drawArgs.WorldCamera.Latitude.Degrees, drawArgs.WorldCamera.Longitude.Degrees) > 90.0)
							continue;

						Vector3 projectedPoint = drawArgs.WorldCamera.Project(cartesianPoint - referenceCenter);

						/*if(isMouseOver)
						{
							// Mouse is over
							isMouseOver = true;

							if(icon.isSelectable)
								DrawArgs.MouseCursor = CursorType.Hand;

							string description = icon.Description;
							if(description==null)
								description = icon.ClickableActionURL;
							if(description!=null)
							{
								// Render description field
								DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak | DrawTextFormat.Bottom;
								int left = 10;
								if(World.Settings.showLayerManager)
									left += World.Settings.layerManagerWidth;
								Rectangle rect = Rectangle.FromLTRB(left, 10, drawArgs.screenWidth - 10, drawArgs.screenHeight - 10 );

								// Draw outline
								drawArgs.defaultDrawingFont.DrawText(
									m_sprite, description,
									rect,
									format, 0xb0 << 24 );
					
								rect.Offset(2,0);
								drawArgs.defaultDrawingFont.DrawText(
									m_sprite, description,
									rect,
									format, 0xb0 << 24 );

								rect.Offset(0,2);
								drawArgs.defaultDrawingFont.DrawText(
									m_sprite, description,
									rect,
									format, 0xb0 << 24 );

								rect.Offset(-2,0);
								drawArgs.defaultDrawingFont.DrawText(
									m_sprite, description,
									rect,
									format, 0xb0 << 24 );

								// Draw description
								rect.Offset(1,-1);
								drawArgs.defaultDrawingFont.DrawText(
									m_sprite, description,
									rect, 
									format, descriptionColor );
							}
						}*/
					m_Sprite.Begin(SpriteFlags.AlphaBlend);

						if(m_IconTexture != null)
						{
							float xscale = (float)m_IconWidth / m_IconTextureDescription.Width;
							float yscale = (float)m_IconHeight / m_IconTextureDescription.Height;
							m_Sprite.Transform = Matrix.Scaling(xscale,yscale,0);
							m_Sprite.Transform *= Matrix.Translation(projectedPoint.X, projectedPoint.Y, 0);
							m_Sprite.Draw( m_IconTexture,
								new Vector3( m_IconWidth>>1, m_IconHeight>>1,0),
								Vector3.Empty,
								iconColor.ToArgb() );
				
							// Reset transform to prepare for text rendering later
							m_Sprite.Transform = Matrix.Identity;
						}

						if(m_ShapeTileArgs.ShowLabels && m_ShapeTileArgs.DataKey != null)
						{
						
							// Render label
							if(p.Tag != null)
							{
								// Render name field
								const int labelWidth = 1000; // Dummy value needed for centering the text
								if(m_IconTexture==null)
								{
									// Center over target as we have no bitmap
									Rectangle rect = new Rectangle(
										(int)projectedPoint.X - (labelWidth>>1), 
										(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
										labelWidth, 
										drawArgs.screenHeight );

									drawArgs.defaultDrawingFont.DrawText(m_Sprite, p.Tag.ToString(), rect, DrawTextFormat.Center, m_ShapeTileArgs.LabelColor);
								}
								else
								{
									// Adjust text to make room for icon
									int spacing = (int)(m_IconWidth * 0.3f);
									if(spacing>10)
										spacing = 10;
									int offsetForIcon = (m_IconWidth>>1) + spacing;

									Rectangle rect = new Rectangle(
										(int)projectedPoint.X + offsetForIcon, 
										(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
										labelWidth, 
										drawArgs.screenHeight );

									drawArgs.defaultDrawingFont.DrawText(m_Sprite, p.Tag.ToString(), rect, DrawTextFormat.WordBreak, m_ShapeTileArgs.LabelColor);
								}
							}
						}
						
						m_Sprite.End();
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}

				
		//Loads a Zipped Shapefile without extracting
		//return==true means it was successfull
		private void loadZippedShapeFile(string shapeFilePath)
		{							
			//ZipFileIndexes
			int shpIndex = -1;
			int	dbfIndex = -1;

			try
			{		
				//Navigate the Zip to find the files and update their index
				ZipFile zFile = new ZipFile(shapeFilePath);
				foreach (ZipEntry ze in zFile) 
				{
					if(ze.Name.ToLower().EndsWith(".shp"))
						shpIndex=ze.ZipFileIndex;					
					else if(ze.Name.ToLower().EndsWith(".dbf"))
						dbfIndex=ze.ZipFileIndex;					
				}							
			}
			catch { /* Ignore */ }			


			if((dbfIndex == -1)||(shpIndex == -1))
				return ;
			
			System.Random random = new Random(Path.GetFileName(shapeFilePath).GetHashCode());

			ArrayList metaValues = new ArrayList();
			
			if(m_ShapeTileArgs.DataKey != null)
			{
				ExtendedZipInputStream dbfReader =
					new ExtendedZipInputStream(File.OpenRead(shapeFilePath));

				ZipEntry dbfEntry= null;			
				dbfEntry = dbfReader.GetNextEntry();
				for(int p=0;p<dbfIndex;p++)
				{
					dbfEntry = dbfReader.GetNextEntry();
				}

				if(!dbfEntry.IsFile)
					return;
				
				byte dbfVersion = dbfReader.ReadByte();

				byte updateYear = dbfReader.ReadByte();
				byte updateMonth = dbfReader.ReadByte();
				byte updateDay = dbfReader.ReadByte();

				int numberRecords = dbfReader.ReadInt32();
				short headerLength = dbfReader.ReadInt16();

				short recordLength = dbfReader.ReadInt16();
				byte[] reserved = dbfReader.ReadBytes(20);

				int numberFields = (headerLength - 33) / 32;

				// Read Field Descriptor Array
				DBF_Field_Header[] fieldHeaders = new DBF_Field_Header[numberFields];

				for (int i = 0; i < numberFields; i++)
				{
					char[] fieldNameChars = dbfReader.ReadChars(10);
					char fieldNameTerminator = dbfReader.ReadChar();
					string fn = new string(fieldNameChars);
					fieldHeaders[i].FieldName = fn.Trim().Replace(" ","");

					fieldHeaders[i].FieldType = dbfReader.ReadChar();
					byte[] reserved1 = dbfReader.ReadBytes(4);

					if(String.Compare(fieldHeaders[i].FieldName.Trim(), m_ShapeTileArgs.DataKey, true) == 0)
					{
						m_ShapeTileArgs.DataKey = fieldHeaders[i].FieldName;
							
						if(fieldHeaders[i].FieldType == 'N')
						{
							m_ShapeTileArgs.UseScalar = true;
						}
						else
						{
							m_ShapeTileArgs.UseScalar = false;
						}

					}

					fieldHeaders[i].FieldLength = dbfReader.ReadByte();

					byte[] reserved2 = dbfReader.ReadBytes(15);
					
				}
				byte headerTerminator = dbfReader.ReadByte();

				double scalarMin = double.MaxValue;
				double scalarMax = double.MinValue;
				for (int i = 0; i < numberRecords; i++)
				{
					//	Shapefile_Polygon curPoly = (Shapefile_Polygon)this.m_ShapeTileArgs.ShapeRecords[i];
								
					byte isValid = dbfReader.ReadByte();
					for (int j = 0; j < fieldHeaders.Length; j++)
					{
						char[] fieldDataChars = dbfReader.ReadChars(fieldHeaders[j].FieldLength);
						string fieldData = new string(fieldDataChars);

						if(fieldHeaders[j].FieldName == m_ShapeTileArgs.DataKey)
						{
							metaValues.Add(fieldData);

							if(fieldHeaders[j].FieldType == 'N')
							{
								try
								{
									if(m_ShapeTileArgs.ScaleMin == double.NaN || m_ShapeTileArgs.ScaleMax == double.NaN)
									{
										double data = double.Parse(fieldData);
										if(m_ShapeTileArgs.ScaleMin == double.NaN && data < scalarMin)
										{
											scalarMin = data;
										}

										if(m_ShapeTileArgs.ScaleMax == double.NaN && data > scalarMax)
										{
											scalarMax = data;
										}
									}
								}
								catch(Exception ex)
								{
									Log.Write(ex);
								}
							}
							else
							{
								if(!m_ShapeTileArgs.ColorAssignments.Contains(fieldData))
								{
									System.Drawing.Color newColor = 
										System.Drawing.Color.FromArgb(
										1 + random.Next(254),
										1 + random.Next(254),
										1 + random.Next(254));

									m_ShapeTileArgs.ColorAssignments.Add(fieldData, newColor);		
								}
							}
						}
					}

					if(m_ShapeTileArgs.UseScalar && m_ShapeTileArgs.ScaleMin == double.NaN)
					{
						m_ShapeTileArgs.ScaleMin = scalarMin;
					}
					if(m_ShapeTileArgs.UseScalar && m_ShapeTileArgs.ScaleMax == double.NaN)
					{
						m_ShapeTileArgs.ScaleMax = scalarMax;
					}
				}
				dbfReader.Close();
										
			}


			ExtendedZipInputStream shpReader= new ExtendedZipInputStream(File.OpenRead(shapeFilePath));
			
			ZipEntry shpEntry= null;			
			shpEntry = shpReader.GetNextEntry();
			for(int p=0;p<shpIndex;p++)
			{
				shpEntry = shpReader.GetNextEntry();
			}

			if(!shpEntry.IsFile)
				return ;

	
			//get file header info
			// Big-Endian Integer File Type
			byte[] fileTypeBytes = shpReader.ReadBytes(4);
			int fileType = 16 * 16 * 16 * 16 * 16 * 16 * fileTypeBytes[0] + 16 * 16 * 16 * 16 * fileTypeBytes[1] + 16 * 16 * fileTypeBytes[2] + fileTypeBytes[3];

			byte[] unused1 = shpReader.ReadBytes(5 * 4);

			byte[] fileLengthBytes = shpReader.ReadBytes(4);
			int fileLength = 16 * 16 * 16 * 16 * 16 * 16 * fileLengthBytes[0] + 16 * 16 * 16 * 16 * fileLengthBytes[1] + 16 * 16 * fileLengthBytes[2] + fileLengthBytes[3];

			int version = shpReader.ReadInt32();
			int shapeType = shpReader.ReadInt32();

			m_BoundingBoxXMin = shpReader.ReadDouble();
			m_BoundingBoxYMin = shpReader.ReadDouble();
			m_BoundingBoxXMax = shpReader.ReadDouble();
			m_BoundingBoxYMax = shpReader.ReadDouble();
			m_BoundingBoxZMin = shpReader.ReadDouble();
			m_BoundingBoxZMax = shpReader.ReadDouble();
			m_BoundingBoxMMin = shpReader.ReadDouble();
			m_BoundingBoxMMax = shpReader.ReadDouble();
			
			//start reading records...
			int bytesRead = 100;
			int counter = 0;

			while (bytesRead < shpEntry.Size)
			{
				ArrayList pendingPoints = new ArrayList();
				
				//read record header
				byte[] recordNumberBytes = shpReader.ReadBytes(4);
				byte[] contentLengthBytes = shpReader.ReadBytes(4);

				int recordNumber = 16 * 16 * 16 * 16 * 16 * 16 * recordNumberBytes[0] + 16 * 16 * 16 * 16 * recordNumberBytes[1] + 16 * 16 * recordNumberBytes[2] + recordNumberBytes[3];
				int contentLength = 16 * 16 * 16 * 16 * 16 * 16 * contentLengthBytes[0] + 16 * 16 * 16 * 16 * contentLengthBytes[1] + 16 * 16 * contentLengthBytes[2] + contentLengthBytes[3];

				//read shape type to determine record structure and content
				int recordShapeType = shpReader.ReadInt32();
				
				ShapeRecord newRecord = new ShapeRecord();

				if(recordShapeType == 0) //Null shape type -- generally used as a placeholder
				{
					newRecord.Null = new Shapefile_Null();
				}
				else if(recordShapeType == 1) //Point shape type
				{
					newRecord.Point = new Shapefile_Point();
					newRecord.Point.X = shpReader.ReadDouble();
					newRecord.Point.Y = shpReader.ReadDouble();					

					pendingPoints.Add(
						MathEngine.SphericalToCartesian(newRecord.Point.Y, newRecord.Point.X, m_ShapeTileArgs.LayerRadius));
				}
				else if(recordShapeType == 8) //Multi-point shape type
				{
					newRecord.MultiPoint = new Shapefile_MultiPoint();
					newRecord.MultiPoint.BoundingBox.West = shpReader.ReadDouble();
					newRecord.MultiPoint.BoundingBox.South = shpReader.ReadDouble();
					newRecord.MultiPoint.BoundingBox.East = shpReader.ReadDouble();
					newRecord.MultiPoint.BoundingBox.North = shpReader.ReadDouble();

					newRecord.MultiPoint.NumPoints = shpReader.ReadInt32();
					
					newRecord.MultiPoint.Points = new Shapefile_Point[newRecord.MultiPoint.NumPoints];
					for(int i = 0; i < newRecord.MultiPoint.NumPoints; i++)
					{
						newRecord.MultiPoint.Points[i] = new Shapefile_Point();
						newRecord.MultiPoint.Points[i].X = shpReader.ReadDouble();
						newRecord.MultiPoint.Points[i].Y = shpReader.ReadDouble();
						
						pendingPoints.Add(
							MathEngine.SphericalToCartesian(newRecord.MultiPoint.Points[i].Y, newRecord.MultiPoint.Points[i].X, m_ShapeTileArgs.LayerRadius));
					}
				}
				else if(recordShapeType == 3)
				{
					newRecord.PolyLine = new Shapefile_PolyLine();
				
					newRecord.PolyLine.BoundingBox.West = shpReader.ReadDouble();
					newRecord.PolyLine.BoundingBox.South = shpReader.ReadDouble();
					newRecord.PolyLine.BoundingBox.East = shpReader.ReadDouble();
					newRecord.PolyLine.BoundingBox.North = shpReader.ReadDouble();
				
					newRecord.PolyLine.NumParts = shpReader.ReadInt32();
					newRecord.PolyLine.NumPoints = shpReader.ReadInt32();
					
					newRecord.PolyLine.Parts = new int[newRecord.PolyLine.NumParts];

					for (int i = 0; i < newRecord.PolyLine.Parts.Length; i++)
					{
						newRecord.PolyLine.Parts[i] = shpReader.ReadInt32();					
					}

					newRecord.PolyLine.Points = new Shapefile_Point[newRecord.PolyLine.NumPoints];
					for (int i = 0; i < newRecord.PolyLine.Points.Length; i++)
					{
						newRecord.PolyLine.Points[i] = new Shapefile_Point();
						newRecord.PolyLine.Points[i].X = shpReader.ReadDouble();
						newRecord.PolyLine.Points[i].Y = shpReader.ReadDouble();						
					}
				}
				else if(recordShapeType == 5)
				{
					newRecord.Polygon = new Shapefile_Polygon();
				
					newRecord.Polygon.BoundingBox.West = shpReader.ReadDouble();
					newRecord.Polygon.BoundingBox.South = shpReader.ReadDouble();
					newRecord.Polygon.BoundingBox.East = shpReader.ReadDouble();
					newRecord.Polygon.BoundingBox.North = shpReader.ReadDouble();
				
					newRecord.Polygon.NumParts = shpReader.ReadInt32();
					newRecord.Polygon.NumPoints = shpReader.ReadInt32();
					
					newRecord.Polygon.Parts = new int[newRecord.Polygon.NumParts];

					for (int i = 0; i < newRecord.Polygon.Parts.Length; i++)
					{
						newRecord.Polygon.Parts[i] = shpReader.ReadInt32();
						
					}
				

					newRecord.Polygon.Points = new Shapefile_Point[newRecord.Polygon.NumPoints];

					for (int i = 0; i < newRecord.Polygon.Points.Length; i++)
					{
						newRecord.Polygon.Points[i] = new Shapefile_Point();
						

						byte[] temp=new byte[16];
						for(int t=0;t<16;t++)
						{
							temp[t]=shpReader.ReadByte();
						}		
						newRecord.Polygon.Points[i].X=BitConverter.ToDouble(temp,0);
						newRecord.Polygon.Points[i].Y=BitConverter.ToDouble(temp,8);
					}
					
				}

				bool ignoreRecord = false;
					
				if(metaValues != null && metaValues.Count > 0)
				{
					newRecord.Value = metaValues[counter];
				
					if(m_ShapeTileArgs.ActiveDataValues != null)
					{
						ignoreRecord = true;
						if(m_ShapeTileArgs.UseScalar)
						{
							double currentValue = double.Parse(newRecord.Value.ToString());
							foreach(string activeValueString in m_ShapeTileArgs.ActiveDataValues)
							{
								double activeValue = double.Parse(activeValueString);
								if(activeValue == currentValue)
								{
									ignoreRecord = false;
									break;
								}
							}
						}
						else
						{
							string currentValue = (string)newRecord.Value;
							foreach(string activeValue in m_ShapeTileArgs.ActiveDataValues)
							{
								if(String.Compare(activeValue.Trim(), currentValue.Trim(), true) == 0)
								{
									ignoreRecord = false;
									break;
								}
							}
						}
					}
					else
					{
						if(m_ShapeTileArgs.UseScalar)
						{
							double currentValue = double.Parse(newRecord.Value.ToString());
							if(m_ScalarFilterMin != double.NaN)
							{
								if(currentValue < m_ScalarFilterMin)
								{
									ignoreRecord = true;
								}
							}
				
							if(m_ScalarFilterMax != double.NaN)
							{
								if(currentValue > m_ScalarFilterMax)
								{
									ignoreRecord = true;
								}
							}

							if(m_ShapeTileArgs.NoDataValues != null)
							{
								foreach(string noDataValueString in m_ShapeTileArgs.NoDataValues)
								{
									double noDataValue = double.Parse(noDataValueString);
									//TODO: might consider using epsilon if floating point errors occur
									if(noDataValue == currentValue)
									{
										ignoreRecord = true;
										break;
									}
								}
							}
						}
						else
						{
							string currentValue = (string)newRecord.Value;
							if(m_ShapeTileArgs.NoDataValues != null)
							{
								foreach(string noDataValue in m_ShapeTileArgs.NoDataValues)
								{
									if(String.Compare(currentValue.Trim(), noDataValue.Trim(), true) == 0)
									{
										ignoreRecord = true;
										break;
									}
								}
							}
						}
					}
				}
				
				if(!ignoreRecord)
				{
					m_ShapeTileArgs.ShapeRecords.Add(newRecord);

					if(pendingPoints.Count > 0)
					{
						foreach(Vector3 v in pendingPoints)
							m_PointList.Add(v);
					}
				}

				bytesRead += 8 + contentLength * 2;
				counter++;
			}
		}



		private void loadShapeFile(string shapeFilePath)
		{								
			FileInfo shapeFile = new FileInfo(shapeFilePath);
			FileInfo dbaseFile = new FileInfo(shapeFile.FullName.Replace(".shp", ".dbf"));
			
			System.Random random = new Random(shapeFile.Name.GetHashCode());

			ArrayList metaValues = new ArrayList();
			
			if(m_ShapeTileArgs.DataKey != null && dbaseFile.Exists)
			{
				using (BinaryReader dbfReader = new BinaryReader(new BufferedStream(dbaseFile.OpenRead()), System.Text.Encoding.ASCII))
				{
					// First Read 32-byte file header
					int bytesRead = 0;
					byte dbfVersion = dbfReader.ReadByte();

					byte updateYear = dbfReader.ReadByte();
					byte updateMonth = dbfReader.ReadByte();
					byte updateDay = dbfReader.ReadByte();

					int numberRecords = dbfReader.ReadInt32();
					short headerLength = dbfReader.ReadInt16();

					short recordLength = dbfReader.ReadInt16();
					byte[] reserved = dbfReader.ReadBytes(20);

					bytesRead += 32;
					int numberFields = (headerLength - 33) / 32;

					// Read Field Descriptor Array
					DBF_Field_Header[] fieldHeaders = new DBF_Field_Header[numberFields];

					for (int i = 0; i < numberFields; i++)
					{
						char[] fieldNameChars = dbfReader.ReadChars(10);
						char fieldNameTerminator = dbfReader.ReadChar();
						string fn = new string(fieldNameChars);
						fieldHeaders[i].FieldName = fn.Trim().Replace(" ","");

						fieldHeaders[i].FieldType = dbfReader.ReadChar();
						byte[] reserved1 = dbfReader.ReadBytes(4);

						if(String.Compare(fieldHeaders[i].FieldName.Trim(), m_ShapeTileArgs.DataKey, true) == 0)
						{
							m_ShapeTileArgs.DataKey = fieldHeaders[i].FieldName;
							
							if(fieldHeaders[i].FieldType == 'N')
							{
								m_ShapeTileArgs.UseScalar = true;
							}
							else
							{
								m_ShapeTileArgs.UseScalar = false;
							}

						}

						fieldHeaders[i].FieldLength = dbfReader.ReadByte();

						byte[] reserved2 = dbfReader.ReadBytes(15);
						bytesRead += 32;

					}
					byte headerTerminator = dbfReader.ReadByte();

					double scalarMin = double.MaxValue;
					double scalarMax = double.MinValue;
					for (int i = 0; i < numberRecords; i++)
					{
					//	Shapefile_Polygon curPoly = (Shapefile_Polygon)this.m_ShapeTileArgs.ShapeRecords[i];
								
						byte isValid = dbfReader.ReadByte();
						for (int j = 0; j < fieldHeaders.Length; j++)
						{
							char[] fieldDataChars = dbfReader.ReadChars(fieldHeaders[j].FieldLength);
							string fieldData = new string(fieldDataChars);

							if(fieldHeaders[j].FieldName == m_ShapeTileArgs.DataKey)
							{
								metaValues.Add(fieldData);

								if(fieldHeaders[j].FieldType == 'N')
								{
									try
									{
										if(m_ShapeTileArgs.ScaleMin == double.NaN || m_ShapeTileArgs.ScaleMax == double.NaN)
										{
											double data = double.Parse(fieldData);
											if(m_ShapeTileArgs.ScaleMin == double.NaN && data < scalarMin)
											{
												scalarMin = data;
											}

											if(m_ShapeTileArgs.ScaleMax == double.NaN && data > scalarMax)
											{
												scalarMax = data;
											}
										}
									}
									catch(Exception ex)
									{
										Log.Write(ex);
									}
								}
								else
								{
									if(!m_ShapeTileArgs.ColorAssignments.Contains(fieldData))
									{
										System.Drawing.Color newColor = 
											System.Drawing.Color.FromArgb(
											1 + random.Next(254),
											1 + random.Next(254),
											1 + random.Next(254));

										m_ShapeTileArgs.ColorAssignments.Add(fieldData, newColor);		
									}
								}
							}
						}

						if(m_ShapeTileArgs.UseScalar && m_ShapeTileArgs.ScaleMin == double.NaN)
						{
							m_ShapeTileArgs.ScaleMin = scalarMin;
						}
						if(m_ShapeTileArgs.UseScalar && m_ShapeTileArgs.ScaleMax == double.NaN)
						{
							m_ShapeTileArgs.ScaleMax = scalarMax;
						}
					}
				}
			}

			FileInfo shapeFileInfo = new FileInfo(shapeFilePath);

			using( FileStream fs = File.OpenRead(shapeFileInfo.FullName) )
			{
				using (BinaryReader reader = new BinaryReader(new BufferedStream(fs)))
				{
					//get file header info
					// Big-Endian Integer File Type
					byte[] fileTypeBytes = reader.ReadBytes(4);
					int fileType = 16 * 16 * 16 * 16 * 16 * 16 * fileTypeBytes[0] + 16 * 16 * 16 * 16 * fileTypeBytes[1] + 16 * 16 * fileTypeBytes[2] + fileTypeBytes[3];

					byte[] unused1 = reader.ReadBytes(5 * 4);

					byte[] fileLengthBytes = reader.ReadBytes(4);
					int fileLength = 16 * 16 * 16 * 16 * 16 * 16 * fileLengthBytes[0] + 16 * 16 * 16 * 16 * fileLengthBytes[1] + 16 * 16 * fileLengthBytes[2] + fileLengthBytes[3];

					int version = reader.ReadInt32();
					int shapeType = reader.ReadInt32();

					m_BoundingBoxXMin = reader.ReadDouble();
					m_BoundingBoxYMin = reader.ReadDouble();
					m_BoundingBoxXMax = reader.ReadDouble();
					m_BoundingBoxYMax = reader.ReadDouble();
					m_BoundingBoxZMin = reader.ReadDouble();
					m_BoundingBoxZMax = reader.ReadDouble();
					m_BoundingBoxMMin = reader.ReadDouble();
					m_BoundingBoxMMax = reader.ReadDouble();

					//start reading records...
					int bytesRead = 100;
					int counter = 0;

					while (bytesRead < shapeFileInfo.Length)
					{
						ArrayList pendingPoints = new ArrayList();
					
						//read record header
						byte[] recordNumberBytes = reader.ReadBytes(4);
						byte[] contentLengthBytes = reader.ReadBytes(4);

						int recordNumber = 16 * 16 * 16 * 16 * 16 * 16 * recordNumberBytes[0] + 16 * 16 * 16 * 16 * recordNumberBytes[1] + 16 * 16 * recordNumberBytes[2] + recordNumberBytes[3];
						int contentLength = 16 * 16 * 16 * 16 * 16 * 16 * contentLengthBytes[0] + 16 * 16 * 16 * 16 * contentLengthBytes[1] + 16 * 16 * contentLengthBytes[2] + contentLengthBytes[3];

						//read shape type to determine record structure and content
						int recordShapeType = reader.ReadInt32();
						
						ShapeRecord newRecord = new ShapeRecord();

						if(recordShapeType == 0) //Null shape type -- generally used as a placeholder
						{
							newRecord.Null = new Shapefile_Null();
						}
						else if(recordShapeType == 1) //Point shape type
						{
							newRecord.Point = new Shapefile_Point();
							newRecord.Point.X = reader.ReadDouble();
							newRecord.Point.Y = reader.ReadDouble();

							pendingPoints.Add(
								MathEngine.SphericalToCartesian(newRecord.Point.Y, newRecord.Point.X, m_ShapeTileArgs.LayerRadius));
						}
						else if(recordShapeType == 8) //Multi-point shape type
						{
							newRecord.MultiPoint = new Shapefile_MultiPoint();
							newRecord.MultiPoint.BoundingBox.West = reader.ReadDouble();
							newRecord.MultiPoint.BoundingBox.South = reader.ReadDouble();
							newRecord.MultiPoint.BoundingBox.East = reader.ReadDouble();
							newRecord.MultiPoint.BoundingBox.North = reader.ReadDouble();

							newRecord.MultiPoint.NumPoints = reader.ReadInt32();
							newRecord.MultiPoint.Points = new Shapefile_Point[newRecord.MultiPoint.NumPoints];
							for(int i = 0; i < newRecord.MultiPoint.NumPoints; i++)
							{
								newRecord.MultiPoint.Points[i] = new Shapefile_Point();
								newRecord.MultiPoint.Points[i].X = reader.ReadDouble();
								newRecord.MultiPoint.Points[i].Y = reader.ReadDouble();

								pendingPoints.Add(
									MathEngine.SphericalToCartesian(newRecord.MultiPoint.Points[i].Y, newRecord.MultiPoint.Points[i].X, m_ShapeTileArgs.LayerRadius));
							}
						}
						else if(recordShapeType == 3)
						{
							newRecord.PolyLine = new Shapefile_PolyLine();
						
							newRecord.PolyLine.BoundingBox.West = reader.ReadDouble();
							newRecord.PolyLine.BoundingBox.South = reader.ReadDouble();
							newRecord.PolyLine.BoundingBox.East = reader.ReadDouble();
							newRecord.PolyLine.BoundingBox.North = reader.ReadDouble();
						
							newRecord.PolyLine.NumParts = reader.ReadInt32();
							newRecord.PolyLine.NumPoints = reader.ReadInt32();
							newRecord.PolyLine.Parts = new int[newRecord.PolyLine.NumParts];

							for (int i = 0; i < newRecord.PolyLine.Parts.Length; i++)
							{
								newRecord.PolyLine.Parts[i] = reader.ReadInt32();
							}

							newRecord.PolyLine.Points = new Shapefile_Point[newRecord.PolyLine.NumPoints];
							for (int i = 0; i < newRecord.PolyLine.Points.Length; i++)
							{
								newRecord.PolyLine.Points[i] = new Shapefile_Point();
								newRecord.PolyLine.Points[i].X = reader.ReadDouble();
								newRecord.PolyLine.Points[i].Y = reader.ReadDouble();
							}
						}
						else if(recordShapeType == 5)
						{
							newRecord.Polygon = new Shapefile_Polygon();
						
							newRecord.Polygon.BoundingBox.West = reader.ReadDouble();
							newRecord.Polygon.BoundingBox.South = reader.ReadDouble();
							newRecord.Polygon.BoundingBox.East = reader.ReadDouble();
							newRecord.Polygon.BoundingBox.North = reader.ReadDouble();
						
							newRecord.Polygon.NumParts = reader.ReadInt32();
							newRecord.Polygon.NumPoints = reader.ReadInt32();
							newRecord.Polygon.Parts = new int[newRecord.Polygon.NumParts];

							for (int i = 0; i < newRecord.Polygon.Parts.Length; i++)
							{
								newRecord.Polygon.Parts[i] = reader.ReadInt32();
							}

							newRecord.Polygon.Points = new Shapefile_Point[newRecord.Polygon.NumPoints];
							for (int i = 0; i < newRecord.Polygon.Points.Length; i++)
							{
								newRecord.Polygon.Points[i] = new Shapefile_Point();
								newRecord.Polygon.Points[i].X = reader.ReadDouble();
								newRecord.Polygon.Points[i].Y = reader.ReadDouble();
							}
						}

						bool ignoreRecord = false;
							
						if(metaValues != null && metaValues.Count > 0)
						{
							newRecord.Value = metaValues[counter];
						
							if(m_ShapeTileArgs.ActiveDataValues != null)
							{
								ignoreRecord = true;
								if(m_ShapeTileArgs.UseScalar)
								{
									double currentValue = double.Parse(newRecord.Value.ToString());
									foreach(string activeValueString in m_ShapeTileArgs.ActiveDataValues)
									{
										double activeValue = double.Parse(activeValueString);
										if(activeValue == currentValue)
										{
											ignoreRecord = false;
											break;
										}
									}
								}
								else
								{
									string currentValue = (string)newRecord.Value;
									foreach(string activeValue in m_ShapeTileArgs.ActiveDataValues)
									{
										if(String.Compare(activeValue.Trim(), currentValue.Trim(), true) == 0)
										{
											ignoreRecord = false;
											break;
										}
									}
								}
							}
							else
							{
								if(m_ShapeTileArgs.UseScalar)
								{
									double currentValue = double.Parse(newRecord.Value.ToString());
									if(m_ScalarFilterMin != double.NaN)
									{
										if(currentValue < m_ScalarFilterMin)
										{
											ignoreRecord = true;
										}
									}
						
									if(m_ScalarFilterMax != double.NaN)
									{
										if(currentValue > m_ScalarFilterMax)
										{
											ignoreRecord = true;
										}
									}

									if(m_ShapeTileArgs.NoDataValues != null)
									{
										foreach(string noDataValueString in m_ShapeTileArgs.NoDataValues)
										{
											double noDataValue = double.Parse(noDataValueString);
											//TODO: might consider using epsilon if floating point errors occur
											if(noDataValue == currentValue)
											{
												ignoreRecord = true;
												break;
											}
										}
									}
								}
								else
								{
									string currentValue = (string)newRecord.Value;
									if(m_ShapeTileArgs.NoDataValues != null)
									{
										foreach(string noDataValue in m_ShapeTileArgs.NoDataValues)
										{
											if(String.Compare(currentValue.Trim(), noDataValue.Trim(), true) == 0)
											{
												ignoreRecord = true;
												break;
											}
										}
									}
								}
							}
						}
						
						if(!ignoreRecord)
						{
							m_ShapeTileArgs.ShapeRecords.Add(newRecord);

							if(pendingPoints.Count > 0)
							{
								foreach(Vector3 v in pendingPoints)
									m_PointList.Add(v);
							}
						}

						bytesRead += 8 + contentLength * 2;
						counter++;
					}
				}
			}
		}
	}


	public class ExtendedZipInputStream 
	{
		private ZipInputStream zis;

		public ExtendedZipInputStream ( System.IO.Stream baseInputStream )
		{
			zis=new ZipInputStream(baseInputStream);
		}
		
		public ICSharpCode.SharpZipLib.Zip.ZipEntry GetNextEntry()
		{
			return zis.GetNextEntry();
		}

		public byte ReadByte()
		{
			return (byte)zis.ReadByte();
		}

		public int ReadByteAsInt()
		{
			return zis.ReadByte();
		}

		public Int32 ReadInt32()
		{
			byte[]temp=new byte[4];
			for(int i=0;i<4;i++)
				temp[i]=(byte)zis.ReadByte();			
			return BitConverter.ToInt32(temp,0);
		}
		
		public Int16 ReadInt16()
		{
			byte[]temp=new byte[2];
			for(int i=0;i<2;i++)
				temp[i]=(byte)zis.ReadByte();			
			return BitConverter.ToInt16(temp,0);
		}

		public double ReadDouble()
		{
			byte[]temp=new byte[8];
			for(int i=0;i<8;i++)
				temp[i]=(byte)zis.ReadByte();
			return BitConverter.ToDouble(temp,0);
		}

		public byte[] ReadBytes(int count)
		{
			byte[]temp=new byte[count];
			for(int i=0;i<count;i++)
				temp[i]=(byte)zis.ReadByte();
			return temp;
		}
		public char ReadChar()
		{
			return (char)zis.ReadByte();
		}
		public char[] ReadChars(int count)
		{
			char[]temp=new char[count];			
			for(int i=0;i<count;i++)
				temp[i]=(char)zis.ReadByte();

			return temp;
		}

		public void Close()
		{
			zis.Close();
		}
	}


	public class ShapeTileArgs
	{
		public static float TileDrawDistance = 2.5f;
		public static float TileSpreadFactor = 2.0f;
		public System.Collections.Hashtable ColorAssignments = new Hashtable();
		public ShapeFileLayer ParentShapeFileLayer;
		public double LayerRadius;

		bool m_ScaleColors = false;
		bool m_ShowLabels = false;
		string m_DataKey = null;
		bool m_OutlinePolygons = false;
		bool m_PolygonFill = false;
		ShapeFillStyle m_ShapeFillStyle = ShapeFillStyle.Solid;
		System.Drawing.Color m_PolygonColor;
		System.Drawing.Color m_LineColor;
		System.Drawing.Color m_LabelColor;
		bool m_UseScalar = false;
		float m_LineWidth = 1.0f;
		double m_ScaleMin = double.MaxValue;
		double m_ScaleMax = double.MinValue;
		bool m_IsPolyLine = false;
		string[] m_NoDataValues = null;
		string[] m_ActiveDataValues = null;
		System.Collections.ArrayList m_ShapeRecords = new System.Collections.ArrayList();
		World m_ParentWorld;
		Size m_TilePixelSize;

		public string[] ActiveDataValues
		{
			get
			{
				return m_ActiveDataValues;
			}
		}

		public bool ScaleColors
		{
			get
			{
				return m_ScaleColors;
			}
		}	

		public ShapeFillStyle ShapeFillStyle
		{
			get
			{
				return m_ShapeFillStyle;
			}
		}
		public bool IsPolyLine
		{
			get
			{
				return m_IsPolyLine;
			}
			set
			{
				m_IsPolyLine = value;
			}
		}
		public float LineWidth
		{
			get
			{
				return m_LineWidth;
			}
		}
		public bool OutlinePolygons
		{
			get
			{
				return m_OutlinePolygons;
			}
		}
		public System.Drawing.Color PolygonColor
		{
			get
			{
				return m_PolygonColor;
			}
		}
		public System.Drawing.Color LineColor
		{
			get
			{
				return m_LineColor;
			}
		}
		public System.Drawing.Color LabelColor
		{
			get
			{
				return m_LabelColor;
			}
		}
		public string[] NoDataValues
		{
			get
			{
				return m_NoDataValues;
			}
		}
		public bool PolygonFill
		{
			get
			{
				return m_PolygonFill;
			}
		}
		public bool UseScalar
		{
			get
			{
				return m_UseScalar;
			}
			set
			{
				m_UseScalar = value;
			}
		}
		public double ScaleMin
		{
			get
			{
				return m_ScaleMin;
			}
			set
			{
				m_ScaleMin = value;
			}
		}
		public double ScaleMax
		{
			get
			{
				return m_ScaleMax;
			}
			set
			{
				m_ScaleMax = value;
			}
		}
		public string DataKey
		{
			get
			{
				return m_DataKey;
			}
			set
			{
				m_DataKey = value;
			}
		}
		public System.Collections.ArrayList ShapeRecords
		{
			get
			{
				return m_ShapeRecords;
			}
			set
			{
				m_ShapeRecords = value;
			}
		}
		public World ParentWorld
		{
			get
			{
				return m_ParentWorld;
			}
		}
		public Size TilePixelSize
		{
			get
			{
				return m_TilePixelSize;
			}
		}
		public bool ShowLabels
		{
			get
			{
				return m_ShowLabels;
			}
		}

		public ShapeTileArgs(
			World parentWorld,
			Size tilePixelSize,
			double layerRadius,
			ShapeFileLayer parentShapeLayer,
			string dataKey,
			bool scaleColors,
			double scaleMin,
			double scaleMax,
			string[] noDataValues,
			string[] activeDataValues,
			bool polygonFill,
			bool outlinePolygons,
			System.Drawing.Color polygonColor,
			ShapeFillStyle shapeFillStyle,
			System.Drawing.Color lineColor,
			System.Drawing.Color labelColor,
			float lineWidth,
			bool showLabels
			)
		{
			ParentShapeFileLayer = parentShapeLayer;
			LayerRadius = layerRadius;

			m_ParentWorld = parentWorld;
			m_TilePixelSize = tilePixelSize;
			m_DataKey = dataKey;
			m_ScaleColors = scaleColors;
			m_PolygonFill = polygonFill;
			m_ScaleMin = scaleMin;
			m_ScaleMax = scaleMax;
			m_NoDataValues = noDataValues;
			m_ActiveDataValues = activeDataValues;
			m_PolygonFill = polygonFill;
			m_OutlinePolygons = outlinePolygons;
			m_PolygonColor = polygonColor;
			m_ShapeFillStyle = shapeFillStyle;
			m_LineColor = lineColor;
			m_LabelColor = labelColor;
			m_LineWidth = lineWidth;
			m_ShowLabels = showLabels;
		}
	}

	/// <summary>
	/// Polygon shape types can be filled with any of these styles, which are the same as the HatchStyles in the GDI .NET framework.
	/// The exception is the "Solid" style, signifies a "solid" fill style (no hatching).
	/// </summary>
	public enum ShapeFillStyle
	{
		Solid,
		BackwardDiagonal,
		Cross,
		DarkDownwardDiagonal,
		DarkHorizontal,
		DarkUpwardDiagonal,
		DarkVertical,
		DashedDownwardDiagonal,
		DashedHorizontal,
		DashedUpwardDiagonal,
		DashedVertical,
		DiagonalBrick,
		DiagonalCross,
		Divot,
		DottedDiamond,
		DottedGrid,
		ForwardDiagonal,
		Horizontal,
		LargeCheckerBoard,
		LargeConfetti,
		LargeGrid,
		LightDownwardDiagonal,
		LightHorizontal,
		LightUpwardDiagonal,
		LightVertical,
		Max,
		Min,
		NarrowHorizontal,
		NarrowVertical,
		OutlinedDiamond,
		Percent05,
		Percent10,
		Percent20,
		Percent25,
		Percent30,
		Percent40,
		Percent50,
		Percent60,
		Percent70,
		Percent75,
		Percent80,
		Percent90,
		Plaid,
		Shingle,
		SmallCheckerBoard,
		SmallConfetti,
		SmallGrid,
		SolidDiamond,
		Sphere,
		Trellis,
		Wave,
		Weave,
		WideDownwardDiagonal,
		WideUpwardDiagonal,
		ZigZag
	}

	public class ShapeTile
	{
		bool m_Initialized = false;
		bool m_Initializing = false;
		ShapeTileArgs m_ShapeTileArgs;
		bool m_Disposing = false;
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

		private Renderable.ImageLayer CreateImageLayer(double north, double south, double west, double east, DrawArgs drawArgs)
		{
			Bitmap b = null;
			Graphics g = null;
			Renderable.ImageLayer imageLayer = null;
			GeographicBoundingBox geoBB = new GeographicBoundingBox(north, south, west, east);
	
			int numberPolygonsInTile = 0;
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
						//	Log.Write((string)currentPoly.ScalarHash[m_ShapeTileArgs.ColorKey]);
						//	Log.Write(String.Format("Min: {0}, Max: {1}", m_ShapeTileArgs.ScaleMin, m_ShapeTileArgs.ScaleMax));
						//	Log.Write(String.Format("{0},{1},{2}", red, green, blue));
						//	Log.Write(ex);
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
            
			if(numberPolygonsInTile > 0)
			{
				string id = System.DateTime.Now.Ticks.ToString();

				MemoryStream ms = new MemoryStream();
				b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				//b.Save("Shapefiles\\imagecache\\"+north+""+south+""+east+""+west+ ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
				
				//must copy original stream into new stream, if not, error occurs, not sure why
				m_ImageStream = new MemoryStream(ms.GetBuffer());

			//	Texture texture = TextureLoader.FromStream(
			//		drawArgs.device, mss, 0, 0, 1,
			//		Usage.None, World.Settings.TextureFormat, Pool.Managed, Filter.Box, Filter.Box, System.Drawing.Color.Black.ToArgb());

			
				imageLayer = new WorldWind.Renderable.ImageLayer(
					id,
					m_ShapeTileArgs.ParentWorld,
					0,// should be distance above surface
					m_ImageStream,
					System.Drawing.Color.Black.ToArgb(),
					(float)south,
					(float)north,
					(float)west,
					(float)east,
					(float)m_ShapeTileArgs.ParentShapeFileLayer.Opacity / 255.0f,
					m_ShapeTileArgs.ParentWorld.TerrainAccessor);
					
				
				//mss.Close();
				ms.Close();
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

				m_NwImageLayer = CreateImageLayer(m_GeoBB.North, centerLatitude, m_GeoBB.West, centerLongitude, drawArgs);
				m_NeImageLayer = CreateImageLayer(m_GeoBB.North, centerLatitude, centerLongitude, m_GeoBB.East, drawArgs);
				m_SwImageLayer = CreateImageLayer(centerLatitude, m_GeoBB.South, m_GeoBB.West, centerLongitude, drawArgs);
				m_SeImageLayer = CreateImageLayer(centerLatitude, m_GeoBB.South, centerLongitude, m_GeoBB.East, drawArgs);

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
					m_NorthWestChild.Initialize(drawArgs);
				}

				if(m_NorthEastChild == null && m_NeImageLayer != null && m_Initialized)
				{
					m_NorthEastChild = ComputeChild(drawArgs, CenterLat, m_GeoBB.North, CenterLon, m_GeoBB.East, tileSize );
					m_NorthEastChild.Initialize(drawArgs);
				}

				if(m_SouthWestChild == null && m_SwImageLayer != null && m_Initialized)
				{
					m_SouthWestChild = ComputeChild(drawArgs, m_GeoBB.South, CenterLat, m_GeoBB.West, CenterLon, tileSize );
					m_SouthWestChild.Initialize(drawArgs);
				}

				if(m_SouthEastChild == null && m_SeImageLayer != null && m_Initialized)
				{
					m_SouthEastChild = ComputeChild(drawArgs, m_GeoBB.South, CenterLat, CenterLon, m_GeoBB.East, tileSize );
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
				Log.Write(ex);
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

	public class Shapefile_Polygon
	{
		public GeographicBoundingBox BoundingBox = new GeographicBoundingBox();
		public int NumParts;
		public int NumPoints;
		public int[] Parts;
		public Shapefile_Point[] Points;
	}

	public class Shapefile_PolyLine
	{
		public GeographicBoundingBox BoundingBox = new GeographicBoundingBox();
		public int NumParts;
		public int NumPoints;
		public int[] Parts;
		public Shapefile_Point[] Points;
	}

	public class Shapefile_Null
	{
		
	}

	public class Shapefile_Point
	{
		public double X;
		public double Y;
		public object Tag = null;
	}

	public class Shapefile_MultiPoint
	{
		public GeographicBoundingBox BoundingBox = new GeographicBoundingBox();
		public int NumPoints;
		public Shapefile_Point[] Points;
	}

	struct DBF_Field_Header
	{
		public string FieldName;
		public char FieldType;
		public byte FieldLength;
	}

	class ShapeRecord
	{
		#region Private Members
		string m_Id;
		Shapefile_Null m_Null = null;
		Shapefile_Point m_Point = null;
		Shapefile_MultiPoint m_MultiPoint = null;
		Shapefile_PolyLine m_PolyLine = null;
		Shapefile_Polygon m_Polygon = null;
		object m_Value = null;
		#endregion

		#region Properties
		public string ID
		{
			get
			{
				return m_Id;
			}
			set
			{
				m_Id = value;
			}
		}
		public Shapefile_Null Null
		{
			get
			{
				return m_Null;
			}
			set
			{
				m_Null = value;
			}
		}
		public Shapefile_Point Point
		{
			get
			{
				return m_Point;
			}
			set
			{
				m_Point = value;
			}
		}
		public Shapefile_MultiPoint MultiPoint
		{
			get
			{
				return m_MultiPoint;
			}
			set
			{
				m_MultiPoint = value;
			}
		}
		public Shapefile_PolyLine PolyLine
		{
			get
			{
				return m_PolyLine;
			}
			set
			{
				m_PolyLine = value;
			}
		}

		public Shapefile_Polygon Polygon
		{
			get
			{
				return m_Polygon;
			}
			set
			{
				m_Polygon = value;
			}
		}

		public object Value
		{
			get
			{
				return m_Value;
			}
			set
			{
				m_Value = value;
			}
		}
		
		#endregion

	}
}
