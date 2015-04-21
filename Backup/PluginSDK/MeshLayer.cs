using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Summary description for MeshLayer.
	/// </summary>
	public class MeshLayer : RenderableObject
	{
		Mesh mesh;
		string meshFilePath;
		ExtendedMaterial[] materials;
		Material[] meshMaterials;

		float lat;
		float lon;
		float layerRadius;
		float scaleFactor;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.MeshLayer"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="latitude"></param>
		/// <param name="longitude"></param>
		/// <param name="layerRadius"></param>
		/// <param name="scaleFactor"></param>
		/// <param name="meshFilePath"></param>
		/// <param name="orientation"></param>
		public MeshLayer(string name, float latitude, float longitude, float layerRadius, float scaleFactor, string meshFilePath, Quaternion orientation) : base(name, MathEngine.SphericalToCartesian(latitude, longitude, layerRadius), orientation) 
		{
			this.meshFilePath = meshFilePath;
			this.lat = latitude;
			this.lon = longitude;
			this.layerRadius = layerRadius;
			this.scaleFactor = scaleFactor;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				GraphicsStream adj;
				this.mesh = Mesh.FromFile(this.meshFilePath, MeshFlags.Managed, drawArgs.device, out adj, out this.materials );
				this.meshMaterials = new Material[this.materials.Length];
				//using(StreamWriter sw = new StreamWriter("mat.txt", true, System.Text.Encoding.ASCII))
			{
				//sw.WriteLine(this.meshMaterials.Length.ToString());
				for(int i = 0; i < this.materials.Length; i++)
				{
					this.meshMaterials[i] = this.materials[i].Material3D;
					this.meshMaterials[i].Ambient = this.meshMaterials[i].Diffuse;
				}
			}

				//this.mesh.ComputeNormals();
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
			this.isInitialized = true;
		}

		public override void Dispose()
		{
			// Don't dispose the mesh!
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
//			Vector3 here = MathEngine.SphericalToCartesian(drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude, this.layerRadius);
			Matrix currentWorld = drawArgs.device.Transform.World;
			drawArgs.device.RenderState.Lighting = true;
			drawArgs.device.RenderState.ZBufferEnable = true;
			drawArgs.device.Lights[0].Diffuse = System.Drawing.Color.White; 
			drawArgs.device.Lights[0].Type = LightType.Point; 
			drawArgs.device.Lights[0].Range = 100000;
			drawArgs.device.Lights[0].Position = new Vector3(this.layerRadius, 0, 0);
			drawArgs.device.Lights[0].Enabled = true ; 

			drawArgs.device.RenderState.CullMode = Cull.None;
			drawArgs.device.Transform.World = Matrix.Identity;
			drawArgs.device.Transform.World *= Matrix.Scaling(this.scaleFactor, this.scaleFactor, this.scaleFactor);
			//drawArgs.device.Transform.World *= Matrix.RotationX(MathEngine.RadiansToDegrees(90));
			drawArgs.device.Transform.World *= Matrix.Translation(0,0,-this.layerRadius);
			
			drawArgs.device.Transform.World *= Matrix.RotationY((float)MathEngine.DegreesToRadians(90-this.lat));
			drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(180+this.lon));
			
			//drawArgs.device.Transform.World *= Matrix.RotationQuaternion(drawArgs.WorldCamera.CurrentOrientation);
		
			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
			drawArgs.device.RenderState.NormalizeNormals = true;
		
			
			for( int i = 0; i < this.meshMaterials.Length; i++ )
			{
				drawArgs.device.Material = this.meshMaterials[i];
				this.mesh.DrawSubset(i);
			}
			
			drawArgs.device.Transform.World = currentWorld;
			drawArgs.device.RenderState.CullMode = Cull.Clockwise;
			drawArgs.device.RenderState.Lighting = false;
		}
	}
}
