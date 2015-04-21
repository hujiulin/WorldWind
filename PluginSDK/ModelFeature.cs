using System;
using System.IO;
using System.Drawing;
using WorldWind;
using System.Xml;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;

namespace WorldWind.Renderable
{
    /// <summary>
    /// This class Loads and Renders at a specific lat,lon,alt a given
    /// Model(ie Textured Mesh) in Direct X or Other supported Format
    /// </summary>
    public class ModelFeature:WorldWind.Renderable.RenderableObject
    {
		public float Latitude;
		public float Longitude;
		public float Scale = 1;
		public float Altitude;
        public float Rotx = 0, Roty = 0, Rotz = 0;
		public float vertExaggeration=1;
		public bool isVertExaggerable = true;
		public float currentElevation = 0;
		public bool isElevationRelative2Ground = true;
 
		
		
		Vector3 worldXyz; // XYZ World coordinates
		string meshFileName;
		Mesh mesh;
		Texture[] meshTextures;            // Textures for the mesh
		static Material[] meshMaterials;
		string errorMsg;

        public ModelFeature(string name,World parentWorld,string fileName,float Latitude,
            float Longitude,float Altitude,float Scale,float rotX,float rotY,float rotZ)
            : base(name,parentWorld) 
		{
			meshFileName = fileName;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Altitude = Altitude;
            this.Scale = Scale;
            this.Rotx = rotX;
            this.Roty = rotY;
            this.Rotz = rotZ;
		}

		/// <summary>
		/// Determine if the object is visible
		/// </summary>
		bool IsVisible(WorldWind.Camera.CameraBase camera)
		{
			if(worldXyz == Vector3.Empty)
				worldXyz = MathEngine.SphericalToCartesian(Latitude, Longitude, camera.WorldRadius );
			return camera.ViewFrustum.ContainsPoint(worldXyz);
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(errorMsg != null)
			{
				//System.Windows.Forms.MessageBox.Show( errorMsg, "Model failed to load.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				errorMsg = null;
				IsOn = false;
				isInitialized = false;
				return;
			}

			if(!IsVisible(drawArgs.WorldCamera))
			{
				// Mesh is not in view, unload it to save memory
				if(isInitialized)
					Dispose();
				return;
			}

			if(!isInitialized)
				return;

			drawArgs.device.RenderState.CullMode = Cull.None;
			drawArgs.device.RenderState.Lighting = true;
			drawArgs.device.RenderState.AmbientColor = 0x808080;
			drawArgs.device.RenderState.NormalizeNormals = true;
			
			drawArgs.device.Lights[0].Diffuse = Color.FromArgb(255, 255, 255);
			drawArgs.device.Lights[0].Type = LightType.Directional; 
			drawArgs.device.Lights[0].Direction = new Vector3(1f,1f,1f);
			drawArgs.device.Lights[0].Enabled = true; 

			drawArgs.device.SamplerState[0].AddressU = TextureAddress.Wrap;
			drawArgs.device.SamplerState[0].AddressV = TextureAddress.Wrap;

			drawArgs.device.RenderState.AlphaBlendEnable = true;
			drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;

			// Put the light somewhere up in space
			drawArgs.device.Lights[0].Position = new Vector3(
				(float)worldXyz.X*2f,
				(float)worldXyz.Y*1f,
				(float)worldXyz.Z*1.5f);

			Matrix currentWorld = drawArgs.device.Transform.World;
			drawArgs.device.Transform.World = Matrix.RotationX((float)MathEngine.DegreesToRadians(Rotx));
            drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(Roty));
			drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(Rotz));
			drawArgs.device.Transform.World *= Matrix.Scaling(Scale, Scale, Scale);

			// Move the mesh to desired location on earth
			if (isVertExaggerable==true)
				vertExaggeration=World.Settings.VerticalExaggeration;
			else vertExaggeration=1;
			drawArgs.device.Transform.World *= Matrix.Translation(0,0,(float)drawArgs.WorldCamera.WorldRadius + (currentElevation * Convert.ToInt16(isElevationRelative2Ground) + Altitude)*vertExaggeration);
			drawArgs.device.Transform.World *= Matrix.RotationY((float)MathEngine.DegreesToRadians(90-Latitude));
			drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(Longitude));


            drawArgs.device.Transform.World *= Matrix.Translation(
                (float)-drawArgs.WorldCamera.ReferenceCenter.X,
                (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
                (float)-drawArgs.WorldCamera.ReferenceCenter.Z
                );

			for( int i = 0; i < meshMaterials.Length; i++ )
			{
				// Set the material and texture for this subset
				drawArgs.device.Material = meshMaterials[i];
				drawArgs.device.SetTexture(0, meshTextures[i]);

				// Draw the mesh subset
				mesh.DrawSubset(i);
			}

			drawArgs.device.Transform.World = currentWorld;
			drawArgs.device.RenderState.Lighting = false;
		}
	
		
		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
			if(!IsVisible(drawArgs.WorldCamera))
				return;

			try
			{
                if(meshFileName.EndsWith(".x"))
                    LoadDirectXMesh(drawArgs);
                else if(meshFileName.EndsWith(".dae")||meshFileName.EndsWith(".xml"))
                    LoadColladaMesh(drawArgs);
                if (mesh == null)
                    throw new InvalidMeshException();
				
				vertExaggeration = World.Settings.VerticalExaggeration;
				if (isElevationRelative2Ground==true)
					currentElevation = World.TerrainAccessor.GetElevationAt(Latitude, Longitude);
				

				isInitialized = true;
			}
			catch(Exception caught)
			{
				Utility.Log.Write( caught );
				errorMsg = "Failed to read mesh from " + meshFileName;
			}
		}

        /// <summary>
        /// Method to load Native Direct X Meshes
        /// </summary>
        private void LoadDirectXMesh(DrawArgs drawArgs)
        {
            ExtendedMaterial[] materials;
			GraphicsStream adj;
			mesh = Mesh.FromFile(meshFileName, MeshFlags.Managed, drawArgs.device, out adj, out materials );

			// Extract the material properties and texture names.
			meshTextures  = new Texture[materials.Length];					
			meshMaterials = new Material[materials.Length];
			string xFilePath = Path.GetDirectoryName(meshFileName);
			for(int i = 0; i < materials.Length; i++)
			{
				meshMaterials[i] = materials[i].Material3D;
				// Set the ambient color for the material (D3DX does not do this)
				meshMaterials[i].Ambient = meshMaterials[i].Diffuse;

				// Create the texture.
				if(materials[i].TextureFilename!=null)
				{
					string textureFilePath = Path.Combine(xFilePath, materials[i].TextureFilename);
					meshTextures[i] = TextureLoader.FromFile(drawArgs.device, textureFilePath);				
				}
			}
        }

        private void LoadColladaMesh(DrawArgs drawArgs)
        {

            XmlDocument colladaDoc = new XmlDocument();
            colladaDoc.Load(meshFileName);
            XmlNodeList geometryNodes = colladaDoc.SelectNodes("geometry");
            if (geometryNodes != null)
            {
                foreach (XmlNode geometryNode in geometryNodes)
                {
                    //Get Mesh Source Points
                    XmlNodeList sourceNodes = geometryNode.SelectNodes("mesh/source");
                    //Get Means to parse Mesh vertices
                    XmlNode vertexNode = geometryNode.SelectSingleNode("mesh/vertices");
                    //Get Means to parse Mesh edges
                    XmlNode lineNode = geometryNode.SelectSingleNode("mesh/lines");
                    //Get Faces/Triangles
                    XmlNodeList triangleNodes = geometryNode.SelectNodes("mesh/triangles");

                    //Store is a new Mesh object
                }
            }
            
        }

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
			if(!isInitialized)
				Initialize(drawArgs);
			else if ((vertExaggeration != World.Settings.VerticalExaggeration)&&(isVertExaggerable==true))
				Initialize(drawArgs);
			else if ((isElevationRelative2Ground==true)&&(currentElevation != World.TerrainAccessor.GetElevationAt(Latitude, Longitude)))
				Initialize(drawArgs);

		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
			isInitialized = false;
			if(mesh!=null)
				mesh.Dispose();
			if(meshTextures!=null)
				foreach(Texture t in meshTextures)
					if(t!=null)
						t.Dispose();
		}

		/// <summary>
		/// Gets called when user left clicks.
		/// RenderableObject abstract member (needed)
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

    }
}
