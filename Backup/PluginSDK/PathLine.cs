using System;
using System.Globalization;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using Utility;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Summary description for PathLine.
	/// </summary>
	public class PathLine : RenderableObject
	{
		public CustomVertex.PositionColored[] linePoints;
		BoundingBox boundingBox;
		float heightAboveSurface;
		string terrainFileName;
		float north;
		float south;
		float east;
		float west;

		World _parentWorld;

		int lineColor;
		Vector3[] sphericalCoordinates = new Vector3[0]; // x = lat, y = lon, z = height

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.PathLine"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="terrainfileName"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		public PathLine(string name, World parentWorld, string terrainfileName, float heightAboveSurface, 
			System.Drawing.Color lineColor) 
			: base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			this._parentWorld = parentWorld;
			//this.terrainManager = terrainManager;
			this.terrainFileName = terrainfileName;
			this.heightAboveSurface = heightAboveSurface;
			this.lineColor = lineColor.ToArgb();
			
			if(this.terrainFileName == null)
			{
				this.isInitialized = true;
				this.isLoaded = true;
				this.linePoints = new CustomVertex.PositionColored[0];
				return;
			}
			
			FileInfo inFile = new FileInfo(this.terrainFileName);
			if(!inFile.Exists)
			{
				this.isInitialized = true;
				this.isLoaded = true;
				this.linePoints = new CustomVertex.PositionColored[0];
				return;
			}
			
			if(inFile.FullName.IndexOf('_') == -1)
			{
				return;
			}

			string[] parsedFileName = inFile.Name.Replace(".wwb","").Split('_');

			if(parsedFileName.Length < 5)
			{
				return;
			}
			else
			{
				this.north = (float)Int32.Parse(parsedFileName[1], CultureInfo.InvariantCulture);
				this.south = (float)Int32.Parse(parsedFileName[2], CultureInfo.InvariantCulture);
				this.west = (float)Int32.Parse(parsedFileName[3], CultureInfo.InvariantCulture);
				this.east = (float)Int32.Parse(parsedFileName[4], CultureInfo.InvariantCulture);
			}

			this.boundingBox = new BoundingBox( this.south, this.north, this.west, this.east, 
				(float)this._parentWorld.EquatorialRadius, 
				(float)this._parentWorld.EquatorialRadius + this.heightAboveSurface );
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.isInitialized = true;
		}

		public void Load(DrawArgs drawArgs)
		{
			this.linePoints = new CustomVertex.PositionColored[0];
			if(this.terrainFileName == null)
			{
				this.isInitialized = true;
				return;
			}
			
			FileInfo inFile = new FileInfo(this.terrainFileName);
			if(!inFile.Exists)
			{
				this.isInitialized = true;
				return;
			}

			using( FileStream fs = new FileStream(inFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read) ) 
			{
				byte[] buffer = new byte[inFile.Length];
				int bytesRead = fs.Read(buffer, 0, (int)inFile.Length);
				
				using( MemoryStream ms = new MemoryStream(buffer) )
				using( BinaryReader br = new BinaryReader(ms, System.Text.Encoding.ASCII) ) 
				{
					int numCoords = br.ReadInt32();

					this.sphericalCoordinates = new Vector3[numCoords];
					for(int i = 0; i < numCoords; i++) 
					{
						this.sphericalCoordinates[i].X = br.ReadSingle();
						this.sphericalCoordinates[i].Y = br.ReadSingle();
					}
					this.linePoints = new CustomVertex.PositionColored[numCoords];

					for(int i = 0; i < numCoords; i++) 
					{
						Vector3 v = MathEngine.SphericalToCartesian(this.sphericalCoordinates[i].X,this.sphericalCoordinates[i].Y, this._parentWorld.EquatorialRadius + World.Settings.VerticalExaggeration * this.heightAboveSurface);
						this.linePoints[i].X = v.X;
						this.linePoints[i].Y = v.Y;
						this.linePoints[i].Z = v.Z;
					
						this.linePoints[i].Color = this.lineColor;
					}
				}
			}
			this.isLoaded = true;
		}

		public override void Dispose()
		{
			this.isLoaded = false;
			this.linePoints = new CustomVertex.PositionColored[0];
		}

		public void SaveToFile(string fileName)
		{
			if(!Directory.Exists(Path.GetDirectoryName(fileName)))
				Directory.CreateDirectory(Path.GetDirectoryName(fileName));

			using( BinaryWriter output = new BinaryWriter(new FileStream(fileName, FileMode.Create)) ) 
			{
				output.Write(this.sphericalCoordinates.Length);
				for(int i = 0; i < this.sphericalCoordinates.Length; i++) {
					output.Write(this.sphericalCoordinates[i].X);
					output.Write(this.sphericalCoordinates[i].Y);
				}
			}		
		}

		bool isLoaded = false;

		public override void Update(DrawArgs drawArgs)
		{
			/*if((this.north <= drawArgs.WorldCamera.Latitude + drawArgs.WorldCamera.ViewRange  &&
				this.south >= drawArgs.WorldCamera.Latitude - drawArgs.WorldCamera.ViewRange &&
				this.west >= drawArgs.WorldCamera.Longitude - drawArgs.WorldCamera.ViewRange &&
				this.east <= drawArgs.WorldCamera.Longitude + drawArgs.WorldCamera.ViewRange) || 
				(drawArgs.WorldCamera.Latitude < this.north &&
				 drawArgs.WorldCamera.Latitude > this.south &&
				 drawArgs.WorldCamera.Longitude > this.west &&
				 drawArgs.WorldCamera.Longitude < this.east))
			{*/
			if(this.boundingBox == null || drawArgs.WorldCamera.ViewFrustum.Intersects(this.boundingBox))
			{
				if(!this.isLoaded)
					this.Load(drawArgs);
			}
			else
				this.Dispose();
					
			//else
			//{
			//	this.isInitialized = false;
			//	this.linePoints = new CustomVertex.PositionColored[0];
			//}
			/*}
			else
			{
				this.isInitialized = false;
				this.linePoints = new CustomVertex.PositionColored[0];
			}*/
			
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		float verticalExaggeration = 1.0f;

		public void AddPointToPath(float latitude, float longitude, bool terrainMapped, float heightAboveSurface)
		{

/*			if(terrainMapped)
			{
				heightAboveSurface += this.terrainManager.GetHeightAt(latitude, longitude) * this.verticalExaggeration;
			}*/
			//Always add first coordinate

			Vector3[] newPoints = null;

			if(sphericalCoordinates.Length>0)
			{
				float startLatitude = sphericalCoordinates[sphericalCoordinates.Length - 1].X;
				float startLongitude = sphericalCoordinates[sphericalCoordinates.Length - 1].Y;
				newPoints = BuildSegment(Angle.FromDegrees(startLatitude),Angle.FromDegrees(startLongitude),
					Angle.FromDegrees(latitude),Angle.FromDegrees(longitude),
					heightAboveSurface);
			}
			

			Vector3[] newCoords = new Vector3[this.sphericalCoordinates.Length + 1];
			
			this.sphericalCoordinates.CopyTo(newCoords,0);

			newCoords[newCoords.Length - 1].X = latitude;
			newCoords[newCoords.Length - 1].Y = longitude;
			newCoords[newCoords.Length - 1].Z = heightAboveSurface;

			this.sphericalCoordinates = newCoords;

			//TODO: HACK work fix this
			if(newPoints == null)
			{
				CustomVertex.PositionColored[] newLine = new CustomVertex.PositionColored[this.linePoints.Length+1];
				this.linePoints.CopyTo(newLine, 0);


				Vector3 newPoint = MathEngine.SphericalToCartesian(latitude, longitude, this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface );
				newLine[newLine.Length - 1].Color = this.lineColor;
				newLine[newLine.Length - 1] = new Microsoft.DirectX.Direct3D.CustomVertex.PositionColored(newPoint.X, newPoint.Y, newPoint.Z, System.Drawing.Color.Red.ToArgb());
			
				//Need to build path if points are spread too far apart

				lock(this.linePoints.SyncRoot)
				{
					this.linePoints = newLine;
				}
			}
			else
			{
				foreach(Vector3 newPoint in newPoints)
				{
					CustomVertex.PositionColored[] newLine = new CustomVertex.PositionColored[this.linePoints.Length+1];
					this.linePoints.CopyTo(newLine, 0);


					//Vector3 newPoint = MathEngine.SphericalToCartesian(latitude, longitude, this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface );
					newLine[newLine.Length - 1].Color = this.lineColor;
					newLine[newLine.Length - 1] = new Microsoft.DirectX.Direct3D.CustomVertex.PositionColored(newPoint.X, newPoint.Y, newPoint.Z, System.Drawing.Color.Red.ToArgb());
			
					//Need to build path if points are spread too far apart

					lock(this.linePoints.SyncRoot)
					{
						this.linePoints = newLine;
					}
				}
			}
			if(this.linePoints.Length == 1)
			{
				this.north = latitude;
				this.south = latitude;
				this.west = longitude;
				this.east = longitude;
			}
			else
			{
				if(latitude > this.north)
					this.north = latitude;
				if(latitude < this.south)
					this.south = latitude;
				if(longitude < this.west)
					this.west = longitude;
				if(longitude > this.east)
					this.east = longitude;
			}
		}
		
		//TODO: Improve rendering code to segment spherical coordinates 
		//which are too far apart to follow curvature
		public Vector3[] BuildSegment(Angle startLatitude,Angle startLongitude,Angle endLatitude,Angle endLongitude,float heightAboveSurface)
		{		
			//check how far the point being added is from the last point
			Angle angularDistance = World.ApproxAngularDistance(
				startLatitude, 
				startLongitude,
				endLatitude, 
				endLongitude );
			Vector3[] newPoints = null;
			if(angularDistance.Degrees>=2.0)
			{
				int samples = (int)(angularDistance.Radians*30);  // 1 point for every 2 degrees.
				if(samples<2)
					samples = 2;

				Angle lat,lon=Angle.Zero;
				newPoints=new Vector3[samples];
				for(int i=0; i<samples; i++)
				{
					float t = (float)i / (samples-1);
					World.IntermediateGCPoint(t, startLatitude, startLongitude, endLatitude,endLongitude,
						angularDistance, out lat, out lon );
					newPoints[i] = MathEngine.SphericalToCartesian(lat, lon, this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface );
				}
			}
			return newPoints;
		}


		public override void Render(DrawArgs drawArgs)
		{
			try
			{
				if(!this.isLoaded)
					return;

				if(this.linePoints.Length > 1)
				{
					/*if(this.north > drawArgs.WorldCamera.Latitude + drawArgs.WorldCamera.ViewRange  ||
						this.south < drawArgs.WorldCamera.Latitude - drawArgs.WorldCamera.ViewRange ||
						this.west < drawArgs.WorldCamera.Longitude - drawArgs.WorldCamera.ViewRange ||
						this.east > drawArgs.WorldCamera.Longitude + drawArgs.WorldCamera.ViewRange)
						return;
	*/
					if(this.boundingBox != null)
					{
						if(!drawArgs.WorldCamera.ViewFrustum.Intersects(this.boundingBox))
							return;
					}

					if(this.linePoints != null)
					{
						drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
						drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;

						drawArgs.device.Transform.World = Matrix.Translation(
							(float)-drawArgs.WorldCamera.ReferenceCenter.X,
							(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
							(float)-drawArgs.WorldCamera.ReferenceCenter.Z
							);

						drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.linePoints.Length - 1, this.linePoints);
						drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
					}
				}
			}
			catch(Exception caught)
			{
				Log.DebugWrite( caught );
			}
		}
	}
}
