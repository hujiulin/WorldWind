using System;
using System.IO;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Summary description for ShapeLayer.
	/// </summary>
	public class ShapeLayer : RenderableObject
	{
		#region Private Members
		string _masterFilePath;
		World _parentWorld;
		double _minDisplayAltitude;
		double _maxDisplayAltitude;
		double _Altitude;
		int _color;
		string _scalarKey;
		ShapeIndex _shapeIndex;
		bool _showBoundaries;
		bool _showFilledPolygons;
		System.Collections.Hashtable _currentPlacenames = new Hashtable();
		System.Collections.Hashtable _currentPolygons = new System.Collections.Hashtable();
		System.Collections.Hashtable _currentBoundaries = new System.Collections.Hashtable();
		Font drawingFont;

		double _scalarMin;
		double _scalarMax;
		#endregion

		#region Public Methods

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.ShapeLayer"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="Altitude"></param>
		/// <param name="masterFilePath"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="font"></param>
		/// <param name="color"></param>
		/// <param name="scalarKey"></param>
		/// <param name="showBoundaries"></param>
		/// <param name="showFilledPolygons"></param>
		public ShapeLayer(
			string name,
			World parentWorld,
			double Altitude,
			string masterFilePath,
			double minDisplayAltitude,
			double maxDisplayAltitude,
			Font font,
			System.Drawing.Color color,
			string scalarKey,
			bool showBoundaries,
			bool showFilledPolygons) : base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0))
		{
			this._parentWorld = parentWorld;
			this._Altitude = Altitude;
			this._masterFilePath = masterFilePath;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this.drawingFont = font;
			this._color = color.ToArgb();
			this._scalarKey = scalarKey;
			this._showBoundaries = showBoundaries;
			this._showFilledPolygons = showFilledPolygons;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				if (File.Exists(this._masterFilePath))
				{
					using (FileStream stream = File.OpenRead(this._masterFilePath))
					using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
					{
						this._shapeIndex = new ShapeIndex();
						int count = reader.ReadInt32();
						this._shapeIndex.ShapeRecords = new ShapeRecord[count];
						for (int i = 0; i < count; i++)
						{
							this._shapeIndex.ShapeRecords[i] = new ShapeRecord();
							this._shapeIndex.ShapeRecords[i].ID = reader.ReadString();
							this._shapeIndex.ShapeRecords[i].Name = reader.ReadString();
							this._shapeIndex.ShapeRecords[i].West = reader.ReadDouble();
							this._shapeIndex.ShapeRecords[i].South = reader.ReadDouble();
							this._shapeIndex.ShapeRecords[i].East = reader.ReadDouble();
							this._shapeIndex.ShapeRecords[i].North = reader.ReadDouble();
							this._shapeIndex.ShapeRecords[i].PolygonFile = reader.ReadString();
							this._shapeIndex.ShapeRecords[i].BoundaryFile = reader.ReadString();

							int metaDataCount = reader.ReadInt32();
							//records[i].MetaData = new Hashtable();
							for (int j = 0; j < metaDataCount; j++)
							{
								string key = reader.ReadString();
								string data = reader.ReadString();
								this._shapeIndex.ShapeRecords[i].MetaData.Add(key, data);
							}

							int scalarDataCount = reader.ReadInt32();
							//records[i].ScalarData = new Hashtable();
							for (int j = 0; j < scalarDataCount; j++)
							{
								string key = reader.ReadString();
								double data = reader.ReadDouble();
								this._shapeIndex.ShapeRecords[i].ScalarData.Add(key, data);
							}
						}
					}
				}

				if(this._scalarKey != null)
				{
					//using(StreamWriter sw = new StreamWriter("lookout.txt", false, System.Text.Encoding.ASCII))
					//sw.WriteLine("*" + this._scalarKey + "*");
					//sw.WriteLine(this._scalarKey.Length);
					this._scalarMin = Double.MinValue;
					this._scalarMax = Double.MaxValue;
					
					foreach(ShapeRecord curRecord in this._shapeIndex.ShapeRecords)
					{
						foreach(string key in curRecord.ScalarData.Keys)
						{
							if(key.IndexOf(this._scalarKey) >= 0)
								this._scalarKey = key;
						}
						if(curRecord.ScalarData.Contains(this._scalarKey))
						{
							double curScalar = (double)curRecord.ScalarData[this._scalarKey];
							if(this._scalarMin == Double.MinValue || curScalar < this._scalarMin)
							{
								this._scalarMin = curScalar;
							}
							if(this._scalarMax == Double.MaxValue || curScalar > this._scalarMax)
							{
								this._scalarMax = curScalar;
							}
						}
						else
						{
							this._scalarKey = null;
							break;
						}
					}

					//sw.WriteLine(this._scalarMin);
					//sw.WriteLine(this._scalarMax);
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
			this.isInitialized = true;
		}

		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if(drawArgs.WorldCamera.Altitude <= this._maxDisplayAltitude &&
					drawArgs.WorldCamera.Altitude >= this._minDisplayAltitude)
				{
					if(!this.isInitialized)
						this.Initialize(drawArgs);

					foreach(ShapeRecord curShapeRecord in this._shapeIndex.ShapeRecords)
					{
						BoundingBox bb = new BoundingBox( 
							(float)curShapeRecord.South, (float)curShapeRecord.North, 
							(float)curShapeRecord.West, (float)curShapeRecord.East, 
							(float)(this._parentWorld.EquatorialRadius + this._Altitude + 0),
							(float)(this._parentWorld.EquatorialRadius + this._Altitude + 10000f));

						if(drawArgs.WorldCamera.ViewFrustum.Intersects(bb))
						{
							if(this._showFilledPolygons && curShapeRecord.PolygonFile != String.Empty)
							{
								if(!this._currentPolygons.Contains(curShapeRecord.ID))
								{
									double dv;

									double curScalar = (double)curShapeRecord.ScalarData[this._scalarKey];

									double r = 1.0;
									double g = 1.0;
									double b = 1.0;
									
									if (curScalar < this._scalarMin)
										curScalar = this._scalarMin;
									if (curScalar > this._scalarMax)
										curScalar = this._scalarMax;
									dv = this._scalarMax - this._scalarMin;

									if (curScalar < (this._scalarMin + 0.25 * dv)) 
									{
										r = 0;
										g = 4 * (curScalar - this._scalarMin) / dv;
									} 
									else if (curScalar < (this._scalarMin + 0.5 * dv)) 
									{
										r = 0;
										b = 1 + 4 * (this._scalarMin + 0.25 * dv - curScalar) / dv;
									} 
									else if (curScalar < (this._scalarMin + 0.75 * dv)) 
									{
										r = 4 * (curScalar - this._scalarMin - 0.5 * dv) / dv;
										b = 0;
									} 
									else 
									{
										g = 1 + 4 * (this._scalarMin + 0.75 * dv - curScalar) / dv;
										b = 0;
									}

									//float colorPercent = 1.0f;
									//if(this._scalarKey != null)
									//{
									//	double curScalar = (double)curShapeRecord.ScalarData[this._scalarKey];
									//	colorPercent = (float)((float)curScalar - (float)this._scalarMin) / ((float)this._scalarMax - (float)this._scalarMin);
									//}
									PolygonLayer newPoly = new PolygonLayer(
										curShapeRecord.ID,
										this._parentWorld,
										this._minDisplayAltitude,
										this._maxDisplayAltitude,
										this._Altitude,
										curShapeRecord.PolygonFile, 
										System.Drawing.Color.FromArgb((int)(255*r), (int)(255*g), (int)(255*b))
										//System.Drawing.Color.FromArgb((int)(this._color.R * colorPercent), (int)(this._color.G * colorPercent), (int)(this._color.B * colorPercent))
										);
									newPoly.Initialize(drawArgs);
									newPoly.Update(drawArgs);
									lock(this._currentPolygons.SyncRoot)
									{
										this._currentPolygons.Add(curShapeRecord.ID, newPoly);
									}
								}
								else
								{
									PolygonLayer curPoly = (PolygonLayer)this._currentPolygons[curShapeRecord.ID];
									curPoly.Update(drawArgs);
								}
							}

							if(this._showBoundaries && curShapeRecord.BoundaryFile != String.Empty)
							{
								if(!this._currentBoundaries.Contains(curShapeRecord.ID))
								{
									BoundaryLayer newBoundary = new BoundaryLayer(
										curShapeRecord.ID,
										this._parentWorld,
										this._Altitude,
										this._minDisplayAltitude,
										this._maxDisplayAltitude,
										curShapeRecord.BoundaryFile,
                              this._color);
									newBoundary.Initialize(drawArgs);
									newBoundary.Update(drawArgs);
									lock(this._currentBoundaries.SyncRoot)
									{
										this._currentBoundaries.Add(curShapeRecord.ID, newBoundary);
									}
								}
								else
								{
									BoundaryLayer curBoundary = (BoundaryLayer)this._currentBoundaries[curShapeRecord.ID];
									curBoundary.Update(drawArgs);
								}
							}
						}
						else
						{
							if(this._showFilledPolygons && curShapeRecord.PolygonFile != String.Empty)
							{
								if(this._currentPolygons.Contains(curShapeRecord.ID))
								{
									lock(this._currentPolygons.SyncRoot)
									{
										using( PolygonLayer oldPoly = (PolygonLayer)this._currentPolygons[curShapeRecord.ID])
											this._currentPolygons.Remove(curShapeRecord.ID);
									}
								}
							}

							if(this._showBoundaries && curShapeRecord.BoundaryFile != String.Empty)
							{
								if(this._currentBoundaries.Contains(curShapeRecord.ID))
								{
									lock(this._currentBoundaries.SyncRoot)
									{
										using( BoundaryLayer oldBoundary = (BoundaryLayer)this._currentBoundaries[curShapeRecord.ID] )
											this._currentBoundaries.Remove(curShapeRecord.ID);
									}
								}
							}
						}
					}
				}
				else
				{
					//remove current rendering resources
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Dispose()
		{
		}

		public override void Render(DrawArgs drawArgs)
		{
			try
			{
				if(this.isInitialized)
				{
					lock(this._currentPolygons.SyncRoot)
					{
						foreach(string key in this._currentPolygons.Keys)
						{
							PolygonLayer curPoly = (PolygonLayer)this._currentPolygons[key];
							curPoly.Render(drawArgs);
						}
					}
	
					lock(this._currentBoundaries.SyncRoot)
					{
						foreach(string key in this._currentBoundaries.Keys)
						{
							BoundaryLayer curBoundary = (BoundaryLayer)this._currentBoundaries[key];
							curBoundary.Render(drawArgs);
						}
					}
				}
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
		}

		#endregion
	}

	class ShapeRecord
	{
		#region Private Members
		string _id;
		string _polygonFile;
		string _boundaryFile;
		string _name;
		double _north;
		double _south;
		double _west;
		double _east;
		System.Collections.Hashtable _metaData = new System.Collections.Hashtable();
		System.Collections.Hashtable _scalarData = new System.Collections.Hashtable();
		#endregion

		#region Properties

		public string ID
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public string PolygonFile
		{
			get
			{
				return this._polygonFile;
			}
			set
			{
				this._polygonFile = value;
			}
		}

		public string BoundaryFile
		{
			get
			{
				return this._boundaryFile;
			}
			set
			{
				this._boundaryFile = value;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public double North
		{
			get
			{
				return this._north;
			}
			set
			{
				this._north = value;
			}
		}

		public double South
		{
			get
			{
				return this._south;
			}
			set
			{
				this._south = value;
			}
		}

		public double East
		{
			get
			{
				return this._east;
			}
			set
			{
				this._east = value;
			}
		}

		public double West
		{
			get
			{
				return this._west;
			}
			set
			{
				this._west = value;
			}
		}

		public System.Collections.Hashtable MetaData
		{
			get
			{
				return this._metaData;
			}
			set
			{
				this.MetaData = value;
			}
		}

		public System.Collections.Hashtable ScalarData
		{
			get
			{
				return this._scalarData;
			}
			set
			{
				this._scalarData = value;
			}
		}
		#endregion

		#region Public Methods

		#endregion
	}

	class ShapeIndex
	{
		#region Private Members
		ShapeRecord[] _shapeRecords;
		#endregion

		#region Properties
		public ShapeRecord[] ShapeRecords
		{
			get
			{
				return this._shapeRecords;
			}
			set
			{
				this._shapeRecords = value;
			}
		}
		#endregion

		#region Public Methods
		public static ShapeIndex FromFile(string indexFilePath)
		{
			try
			{
				using(StreamWriter sw = new StreamWriter("debugger.txt", false, System.Text.Encoding.ASCII))
				{
					FileInfo indexFileInfo = new FileInfo(indexFilePath);
					if(!indexFileInfo.Exists)
						return null;

					using( FileStream stream = indexFileInfo.OpenRead() )
					using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
					{
						ShapeIndex si = new ShapeIndex();
						int count = reader.ReadInt32();
						si.ShapeRecords = new ShapeRecord[count];
						for (int i = 0; i < count; i++)
						{
							si.ShapeRecords[i] = new ShapeRecord();
							si.ShapeRecords[i].ID = reader.ReadString();
							si.ShapeRecords[i].Name = reader.ReadString();
							si.ShapeRecords[i].West = reader.ReadDouble();
							si.ShapeRecords[i].South = reader.ReadDouble();
							si.ShapeRecords[i].East = reader.ReadDouble();
							si.ShapeRecords[i].North = reader.ReadDouble();
							si.ShapeRecords[i].PolygonFile = reader.ReadString();
							si.ShapeRecords[i].BoundaryFile = reader.ReadString();

							int metaDataCount = reader.ReadInt32();
							//records[i].MetaData = new Hashtable();
							for (int j = 0; j < metaDataCount; j++)
							{
								string key = reader.ReadString();
								string data = reader.ReadString();
								si.ShapeRecords[i].MetaData.Add(key, data);
							}

							int scalarDataCount = reader.ReadInt32();
							//records[i].ScalarData = new Hashtable();
							for (int j = 0; j < scalarDataCount; j++)
							{
								string key = reader.ReadString();
								double data = reader.ReadDouble();
								si.ShapeRecords[i].ScalarData.Add(key, data);
							}
						}
						return si;
					}
				}
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
			return null;
		}

		public void Save(string indexFilePath)
		{
			if(this._shapeRecords == null)
				return;

			using( FileStream stream = File.OpenWrite(indexFilePath) )
			using( BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.ASCII) ) 
			{
				writer.Write(this._shapeRecords.Length);

				foreach(ShapeRecord curRecord in this._shapeRecords) 
				{
					writer.Write(curRecord.ID);
					if(curRecord.Name == null)
						writer.Write(String.Empty);
					else
						writer.Write(curRecord.Name);

					writer.Write(curRecord.West);
					writer.Write(curRecord.South);
					writer.Write(curRecord.East);
					writer.Write(curRecord.North);

					if(curRecord.PolygonFile == null)
						writer.Write(String.Empty);
					else
						writer.Write(curRecord.PolygonFile);

					if(curRecord.BoundaryFile == null)
						writer.Write(String.Empty);
					else
						writer.Write(curRecord.BoundaryFile);

					writer.Write(curRecord.MetaData.Count);
					foreach(string key in curRecord.MetaData.Keys) 
					{
						writer.Write(key);
						writer.Write((string)curRecord.MetaData[key]);
					}

					writer.Write(curRecord.ScalarData.Count);
					foreach(string key in curRecord.ScalarData.Keys) 
					{
						writer.Write(key);
						writer.Write((double)curRecord.ScalarData[key]);
					}
				}
			}
		}
		#endregion
	}
}
