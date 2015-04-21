using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;

namespace WorldWind.Renderable
{
	public class BoundaryLayer : RenderableObject
	{
		#region Private Members
		World _parentWorld;
		double _distanceAboveSurface;
		double _minDisplayAltitude;
		double _maxDisplayAltitude;
		string _boundaryFilePath;
		int _color;
		CustomVertex.PositionColored[] vertices;
		#endregion

		#region Public Methods

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.BoundaryLayer"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="distanceAboveSurface"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="boundaryFilePath"></param>
		/// <param name="color"></param>
		public BoundaryLayer(
			string name,
			World parentWorld,
			double distanceAboveSurface,
			double minDisplayAltitude,
			double maxDisplayAltitude,
			string boundaryFilePath,
			int color) : base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0))
		{
			this._parentWorld = parentWorld;
			this._distanceAboveSurface = distanceAboveSurface;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this._boundaryFilePath = boundaryFilePath;
			this._color = color;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			FileInfo boundaryFileInfo = new FileInfo(this._boundaryFilePath);
			if(!boundaryFileInfo.Exists)
			{
				this.isInitialized = true;
				return;
			}

			using( FileStream boundaryFileStream = boundaryFileInfo.OpenRead() )
			using( BinaryReader boundaryFileReader = new BinaryReader(boundaryFileStream, System.Text.Encoding.ASCII) ) {
				int numBoundaries = boundaryFileReader.ReadInt32();
				int count = boundaryFileReader.ReadInt32();
				this.vertices = new CustomVertex.PositionColored[count];

				for(int i = 0; i < count; i++) {
					double lat = boundaryFileReader.ReadDouble();
					double lon = boundaryFileReader.ReadDouble();
					Vector3 v = MathEngine.SphericalToCartesian((float)lat, (float)lon, (float)(this._parentWorld.EquatorialRadius + this._distanceAboveSurface));
					this.vertices[i].X = v.X;
					this.vertices[i].Y = v.Y;
					this.vertices[i].Z = v.Z;
					this.vertices[i].Color = this._color;
				}

			}
			this.isInitialized = true;
		}

		public override void Dispose()
		{
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Update(DrawArgs drawArgs)
		{
			if(!this.isInitialized)
				this.Initialize(drawArgs);
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(this.isInitialized)
			{
				drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.vertices.Length - 1, this.vertices);
			}
		}
		#endregion
	}
}
