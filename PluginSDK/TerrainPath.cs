using System;
using System.Collections;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using WorldWind.Terrain;
using Utility;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Class used to create and render a terrain following path
	/// TODO: Re-Implement terrain mapping based on new TerrainAccessor functionality
	/// </summary>
	public class TerrainPath : RenderableObject
	{
		float north;
		float south;
		float east;
		float west;
		BoundingBox boundingBox;
		World _parentWorld;
		BinaryReader _dataArchiveReader;
		long _fileOffset;
		long _fileSize;
		TerrainAccessor _terrainAccessor;
		float heightAboveSurface;
		string terrainFileName;
		//bool terrainMapped;
		public bool isLoaded;
		Vector3 lastUpdatedPosition;
		float verticalExaggeration = 1.0f;
		
		double _minDisplayAltitude, _maxDisplayAltitude; 

		int lineColor;
		public CustomVertex.PositionColored[] linePoints;

		Vector3[] sphericalCoordinates = new Vector3[0]; // x = lat, y = lon, z = height

		public int LineColor
		{
			get{ return lineColor; }
		}
	
		public Vector3[] SphericalCoordinates
		{
			get{ return sphericalCoordinates; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.TerrainPath"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="terrainFileName"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		/// <param name="terrainAccessor"></param>
		public TerrainPath(
			string name, 
			World parentWorld, 
			double minDisplayAltitude, 
			double maxDisplayAltitude, 
			string terrainFileName, 
			float heightAboveSurface, 
			System.Drawing.Color lineColor,
			TerrainAccessor terrainAccessor) 
			: base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			this._parentWorld = parentWorld;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this.terrainFileName = terrainFileName;
			this.heightAboveSurface = heightAboveSurface;
			//this.terrainMapped = terrainMapped;
			this.lineColor = lineColor.ToArgb();
			this._terrainAccessor = terrainAccessor;
			this.RenderPriority = RenderPriority.LinePaths;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.TerrainPath"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="dataArchiveReader"></param>
		/// <param name="fileOffset"></param>
		/// <param name="fileSize"></param>
		/// <param name="north"></param>
		/// <param name="south"></param>
		/// <param name="east"></param>
		/// <param name="west"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		/// <param name="terrainAccessor"></param>
		public TerrainPath(
			string name, 
			World parentWorld, 
			double minDisplayAltitude, 
			double maxDisplayAltitude, 
			BinaryReader dataArchiveReader,
			long fileOffset,
			long fileSize,
			double north,
			double south,
			double east, 
			double west,
			float heightAboveSurface, 
			System.Drawing.Color lineColor,
			TerrainAccessor terrainAccessor) 
			: base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			this._parentWorld = parentWorld;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this._dataArchiveReader = dataArchiveReader;
			this._fileOffset = fileOffset;
			this._fileSize = fileSize;
			this.heightAboveSurface = heightAboveSurface;
			//this.terrainMapped = terrainMapped;
			this.lineColor = lineColor.ToArgb();
			this._terrainAccessor = terrainAccessor;
			
			this.north = (float)north;
			this.south = (float)south;
			this.west = (float)west;
			this.east = (float)east;

			this.RenderPriority = RenderPriority.LinePaths;

			this.boundingBox = new BoundingBox( this.south, this.north, this.west, this.east, 
				(float)this._parentWorld.EquatorialRadius, 
				(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface));
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.verticalExaggeration = World.Settings.VerticalExaggeration;
			this.isInitialized = true;
		}

		public Vector3[] GetSphericalCoordinates()
		{
			float n = 0;
			float s = 0;
			float w = 0;
			float e = 0;

			return GetSphericalCoordinates(ref n, ref s, ref w, ref e);
		}

		public Vector3[] GetSphericalCoordinates(ref float northExtent, ref float southExtent, ref float westExtent, ref float eastExtent)
		{
			if(this._dataArchiveReader == null)
			{
				FileInfo file = new FileInfo(this.terrainFileName);

				if(!file.Exists)
				{
					northExtent = 0;
					southExtent = 0;
					westExtent = 0;
					eastExtent = 0;
					return null;
				}
				using( BufferedStream fs = new BufferedStream(file.OpenRead()) )
				using( BinaryReader br = new BinaryReader(fs) )
				{
					int numCoords = br.ReadInt32();

					Vector3[] coordinates = new Vector3[numCoords];
					coordinates[0].X = br.ReadSingle();
					coordinates[0].Y = br.ReadSingle();

					northExtent = coordinates[0].X;
					southExtent = coordinates[0].X;
					westExtent = coordinates[0].Y;
					eastExtent = coordinates[0].Y;

					for (int i = 1; i < numCoords; i++)
					{
						coordinates[i].X = br.ReadSingle();
						coordinates[i].Y = br.ReadSingle();

						if (northExtent < coordinates[i].X)
							northExtent = coordinates[i].X;
						if (eastExtent < coordinates[i].Y)
							eastExtent = coordinates[i].Y;
						if (southExtent > coordinates[i].X)
							southExtent = coordinates[i].X;
						if (westExtent > coordinates[i].Y)
							westExtent = coordinates[i].Y;
					}

					return coordinates;
				}
			}
			else
			{
				this._dataArchiveReader.BaseStream.Seek(this._fileOffset, SeekOrigin.Begin);
				
				int numCoords = this._dataArchiveReader.ReadInt32();
					
				byte numElements = this._dataArchiveReader.ReadByte();
				Vector3[] coordinates = new Vector3[numCoords];
				
				coordinates[0].X = (float)this._dataArchiveReader.ReadDouble();
				coordinates[0].Y = (float)this._dataArchiveReader.ReadDouble();
				if(numElements == 3)
					coordinates[0].Z = this._dataArchiveReader.ReadInt16();

				northExtent = coordinates[0].X;
				southExtent = coordinates[0].X;
				westExtent = coordinates[0].Y;
				eastExtent = coordinates[0].Y;

				for(int i = 1; i < numCoords; i++)
				{	
					coordinates[i].X = (float)this._dataArchiveReader.ReadDouble();
					coordinates[i].Y = (float)this._dataArchiveReader.ReadDouble();
					if(numElements == 3)
						coordinates[i].Z = this._dataArchiveReader.ReadInt16();

					if (northExtent < coordinates[i].X)
						northExtent = coordinates[i].X;
					if (eastExtent < coordinates[i].Y)
						eastExtent = coordinates[i].Y;
					if (southExtent > coordinates[i].X)
						southExtent = coordinates[i].X;
					if (westExtent > coordinates[i].Y)
						westExtent = coordinates[i].Y;
				}

				return coordinates;
			}
		}

		public void Load()
		{
			try
			{
				if(this.terrainFileName == null && this._dataArchiveReader == null)
				{
					this.isInitialized = true;
					return;
				}

				sphericalCoordinates = GetSphericalCoordinates(ref north, ref south, ref west, ref east);

				this.boundingBox = new BoundingBox( this.south, this.north, this.west, this.east, 
					(float)this._parentWorld.EquatorialRadius,
					(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface));

			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
			
			this.isLoaded = true;
		}

		public override void Dispose()
		{
			this.isLoaded = false;
			this.linePoints = null;
		}

		public void SaveToFile(string fileName)
		{
			using (BinaryWriter output = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
			{
				output.Write(this.sphericalCoordinates.Length);
				for (int i = 0; i < this.sphericalCoordinates.Length; i++)
				{
					output.Write(this.sphericalCoordinates[i].X);
					output.Write(this.sphericalCoordinates[i].Y);
				}
			}
		}

		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if(!drawArgs.WorldCamera.ViewFrustum.Intersects(boundingBox))
				{
					Dispose();
					return;
				}

				if(!isLoaded)
					Load();

				if(linePoints != null)
					if((lastUpdatedPosition - drawArgs.WorldCamera.Position).LengthSq() < 10*10) // Update if camera moved more than 10 meters
						if(Math.Abs(this.verticalExaggeration - World.Settings.VerticalExaggeration) < 0.01)
							// Already loaded and up-to-date
							return;

				verticalExaggeration = World.Settings.VerticalExaggeration;

				ArrayList renderablePoints = new ArrayList();
				Vector3 lastPointProjected = Vector3.Empty;
				Vector3 currentPointProjected;
				Vector3 currentPointXyz = Vector3.Empty;

                Vector3 rc = new Vector3(
                    (float)drawArgs.WorldCamera.ReferenceCenter.X,
                    (float)drawArgs.WorldCamera.ReferenceCenter.Y,
                    (float)drawArgs.WorldCamera.ReferenceCenter.Z
                    );

				for(int i = 0; i < sphericalCoordinates.Length; i++)
				{
					double altitude = 0;
					if(_parentWorld.TerrainAccessor != null && drawArgs.WorldCamera.Altitude < 3000000)
						altitude = _terrainAccessor.GetElevationAt(
							sphericalCoordinates[i].X, 
							sphericalCoordinates[i].Y,
							(100.0 / drawArgs.WorldCamera.ViewRange.Degrees));

					currentPointXyz = MathEngine.SphericalToCartesian(
						this.sphericalCoordinates[i].X, 
						this.sphericalCoordinates[i].Y, 
						this._parentWorld.EquatorialRadius + this.heightAboveSurface + 
						this.verticalExaggeration * altitude );

					currentPointProjected = drawArgs.WorldCamera.Project(currentPointXyz - rc);

					float dx = lastPointProjected.X - currentPointProjected.X;
					float dy = lastPointProjected.Y - currentPointProjected.Y;
					float distanceSquared = dx*dx + dy*dy;
					const float minimumPointSpacingSquaredPixels = 2*2;
					if(distanceSquared > minimumPointSpacingSquaredPixels)
					{
						renderablePoints.Add(currentPointXyz);
						lastPointProjected = currentPointProjected;
					}
				}

				// Add the last point if it's not already in there
				int pointCount = renderablePoints.Count;
				if(pointCount>0 && (Vector3)renderablePoints[pointCount-1] != currentPointXyz)
				{
					renderablePoints.Add(currentPointXyz);
					pointCount++;
				}

				CustomVertex.PositionColored[] newLinePoints = new CustomVertex.PositionColored[pointCount];
				for(int i = 0; i < pointCount; i++)
				{
					currentPointXyz = (Vector3)renderablePoints[i];
					newLinePoints[i].X = currentPointXyz.X;
					newLinePoints[i].Y = currentPointXyz.Y;
					newLinePoints[i].Z = currentPointXyz.Z;

					newLinePoints[i].Color = this.lineColor;
				}

				this.linePoints = newLinePoints;

				lastUpdatedPosition = drawArgs.WorldCamera.Position;
				System.Threading.Thread.Sleep(1);
			}
			catch
			{
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			try
			{
				if(!this.isLoaded)
					return;

				if(drawArgs.WorldCamera.Altitude > _maxDisplayAltitude)
					return;
				if(drawArgs.WorldCamera.Altitude < _minDisplayAltitude)
					return;

				if(this.linePoints == null)
					return;

				if(!drawArgs.WorldCamera.ViewFrustum.Intersects(this.boundingBox))
					return;

				drawArgs.numBoundaryPointsRendered += this.linePoints.Length;
				drawArgs.numBoundaryPointsTotal += this.sphericalCoordinates.Length;
				drawArgs.numBoundariesDrawn++;

				drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;

				drawArgs.device.Transform.World = Matrix.Translation(
					(float)-drawArgs.WorldCamera.ReferenceCenter.X,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Z
					);

				
				drawArgs.device.DrawUserPrimitives( PrimitiveType.LineStrip, this.linePoints.Length - 1, this.linePoints );
				drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
			}
			catch
			{
			}
		}
	}
}
