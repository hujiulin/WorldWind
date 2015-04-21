//----------------------------------------------------------------------------
// NAME: Stars3D
// VERSION: 1.1
// DESCRIPTION: Renders a 3D star background to the world. Data comes from the Hipparcos catalog (ESA, 1997). Adds itself as a layer in Layer Manager (key: L). Right click on layer for settings.
// DEVELOPER: Patrick Murris
// WEBSITE: http://www.alpix.com/3d/worldwin
//----------------------------------------------------------------------------
// Based on the Sky plugin, itself based on Bjorn Reppen 'Atmosphere' plugin
// Star data extracted using VizieR at http://cdsweb.u-strasbg.fr
//----------------------------------------------------------------------------
// 1.1 Dec  27, 2005	Added bright stars halo/flare
// 1.0 Nov  19, 2005	First light with up to 31k stars from the Hipparcos catalog
//----------------------------------------------------------------------------

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using WorldWind.Renderable;
using WorldWind.Camera;
using WorldWind;
using System.IO;
using System;
using System.Drawing;
using System.Globalization;

namespace Stars3D.Plugin
{
	/// <summary>
	/// The plugin (main class)
	/// </summary>
	public class Stars3D : WorldWind.PluginEngine.Plugin 
	{
		/// <summary>
		/// Name displayed in layer manager
		/// </summary>
		public static string LayerName = "Starfield";

		/// <summary>
		/// Plugin entry point - All plugins must implement this function
		/// </summary>
		public override void Load() 
		{
			if(ParentApplication.WorldWindow.CurrentWorld != null && ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("SDSS") == -1)
			{
				Stars3DLayer layer = new Stars3DLayer(LayerName, PluginDirectory, ParentApplication.WorldWindow);
				ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.ChildObjects.Insert(0,layer);
			}
		}
		/// <summary>
		/// Unloads our plugin
		/// </summary>
		public override void Unload() 
		{
			ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Remove(LayerName);
		}
	}

	/// <summary>
	/// Sky dome
	/// </summary>
	public class Stars3DLayer : RenderableObject
	{
		static string version = "1.1";
		string settingsFileName = "Stars3D.ini";
		string pluginPath;
		public World world;
		public DrawArgs drawArgs;
		Form pDialog;
		private VertexBuffer StarListVB = null;
		private int StarCount = 0;
		private float FlareMag = 4;		// Bright stars flare threshold magnitude
		private Mesh FlareMesh;
		private int FlareCount = 0;
		private bool showFlares = true;		// Default show bright stars flare
		private int refWidth;		
		private double sphereRadius;
		private Texture texture;
		public string textureFileName = "Flare.png";

		// default star catalog
		public string catalogFileName = "Hipparcos_Stars_Mag6x5044.tsv";

		/// <summary>
		/// Constructor
		/// </summary>
		public Stars3DLayer(string LayerName, string pluginPath, WorldWind.WorldWindow worldWindow) : base(LayerName)
		{
			this.pluginPath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), @"Plugins\stars3d\");
			this.world = worldWindow.CurrentWorld;
			this.drawArgs = worldWindow.DrawArgs;
			this.RenderPriority = RenderPriority.SurfaceImages;
			//this.sphereRadius = this.drawArgs.WorldCamera.WorldRadius * 20;
			ReadSettings();
		}
		
		/// <summary>
		/// Read saved settings from ini file
		/// </summary>
		public void ReadSettings()
		{
			string line = "";
			try 
			{
				TextReader tr = File.OpenText(Path.Combine(pluginPath, settingsFileName));
				line = tr.ReadLine();
				tr.Close();
			}
			catch(Exception) {}
			if(line != "")
			{
				string[] settingsList = line.Split(';');
				string saveVersion = settingsList[0];	// version when settings where saved
				if(settingsList[1] != null) catalogFileName = settingsList[1];
				if(settingsList.Length >= 3) showFlares = (settingsList[2] == "False") ? false : true;
			}
		}

		/// <summary>
		/// Save settings in ini file
		/// </summary>
		public void SaveSettings()
		{
			string line = version + ";" + catalogFileName + ";" + showFlares.ToString();
			try
			{
				StreamWriter sw = new StreamWriter(Path.Combine(pluginPath, settingsFileName));
				sw.Write(line);
				sw.Close();
			}
			catch(Exception) {}
		}

		#region RenderableObject

		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			if(!isInitialized)
				return;

			// Camera & Device shortcuts ;)
			CameraBase camera = drawArgs.WorldCamera;
			Device device = drawArgs.device;

			// Read star catalog and build vertex list if not done yet
			if(StarListVB == null  || refWidth != device.Viewport.Width) 
			{
				if(StarListVB != null) StarListVB = null;
				if(FlareMesh != null) {FlareMesh.Dispose(); FlareMesh = null;}
				LoadStars();
			}


			// if(camera.Altitude < 500e3) return;
			if(drawArgs.device.RenderState.Lighting)
			{
				drawArgs.device.RenderState.Lighting = false;
				drawArgs.device.RenderState.Ambient = World.Settings.StandardAmbientColor;
			}

			// save world and projection transform
			Matrix origWorld = device.Transform.World;
			Matrix origProjection = device.Transform.Projection;

			// Save fog status
			bool origFog = device.RenderState.FogEnable;
			device.RenderState.FogEnable = false;

			// Set new projection (to avoid being clipped) - probably better ways of doing this?
			float aspectRatio =  (float)device.Viewport.Width / device.Viewport.Height;
            device.Transform.Projection = Matrix.PerspectiveFovRH((float)camera.Fov.Radians, aspectRatio, (float)(0.5f * sphereRadius), (float)(2.0f * sphereRadius));


			// This is where we can rotate the star dome to acomodate time and seasons
			drawArgs.device.Transform.World = Matrix.Translation(
				(float)-drawArgs.WorldCamera.ReferenceCenter.X,
				(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
				(float)-drawArgs.WorldCamera.ReferenceCenter.Z
				);

			
			drawArgs.device.Transform.World *= Matrix.RotationZ(
				(float)(TimeKeeper.CurrentTimeUtc.Hour + 
				TimeKeeper.CurrentTimeUtc.Minute / 60.0 +
				TimeKeeper.CurrentTimeUtc.Second / 3600.0 + 
				TimeKeeper.CurrentTimeUtc.Millisecond / 3600000.0) / 24.0f * (float)(-2 * Math.PI));

			// Render textured flares if set
			if(showFlares)
			{
				device.SetTexture(0,texture);
                device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
                device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
				device.TextureState[0].ColorOperation = TextureOperation.Modulate;
                device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
				device.VertexFormat = CustomVertex.PositionTextured.Format;
				FlareMesh.DrawSubset(0);
			}

			// draw StarListVB
			device.SetTexture(0,null);
			device.VertexFormat = CustomVertex.PositionColored.Format;
            device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
            device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;
            device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
            device.TextureState[0].ColorArgument1 = TextureArgument.Diffuse;
				
            device.SetStreamSource(0, StarListVB, 0);
			device.DrawPrimitives(PrimitiveType.PointList, 0, StarCount );

			// Restore device states
			device.Transform.World = origWorld;
			device.Transform.Projection = origProjection;
			device.RenderState.FogEnable = origFog;

		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				texture = TextureLoader.FromFile(drawArgs.device, Path.Combine(pluginPath, textureFileName));
				isInitialized = true;	
			}
			catch
			{
				isOn = false;
				MessageBox.Show("Error loading texture " + Path.Combine(pluginPath, textureFileName) + ".","Layer initialization failed.", MessageBoxButtons.OK, 
					MessageBoxIcon.Error );
			}
		}

		// Read star catalog and build vertex list
		private void LoadStars()
		{
            string DecSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			sphereRadius = drawArgs.WorldCamera.WorldRadius * 20;
			refWidth = drawArgs.device.Viewport.Width;
			StarCount = 0;
			FlareCount = 0;
			int idxRAhms = 2;		// Catalog field indices
			int idxDEdms = 3;
			int idxVmag = 4;
			int idxBV = 5;
			string line;
			int isData = 0;

			// Count stars and flares
			TextReader tr = File.OpenText(Path.Combine(pluginPath, catalogFileName));
			while ((line = tr.ReadLine()) != null) 
			{
				if(line.Length < 3) continue;
				if(line.Substring(0, 1) == "#") continue;
				if(isData == 0 && line.IndexOf("RA") != -1) // Field names here
				{
					// Find out fields indices
					string[] fieldData = line.Split(';');
					for(int i = 0; i < fieldData.Length; i++) 
					{
						if(fieldData[i] == "RAhms") idxRAhms = i;
						if(fieldData[i] == "DEdms") idxDEdms = i;
						if(fieldData[i] == "Vmag") idxVmag = i;
						if(fieldData[i] == "B-V") idxBV = i;
					}
				}
				if(isData == 1) // Star data here
				{
					StarCount++; // just counting now...
					// Data in ';' separated values
					string[] starData = line.Split(';');
					string Vmag = starData[idxVmag];	// Aparent magnitude	" 4.78"
					// check magnitude -1.5 - 10 for flares
					double VM = Convert.ToDouble(Vmag.Replace(".", DecSep));
					if(VM < FlareMag) FlareCount++;
				}
				if(line.Substring(0, 3) == "---") isData = 1;
			}
			tr.Close();
			
			// Create vertex buffer for stars
			int idx = 0;
			StarListVB = new VertexBuffer( typeof(CustomVertex.PositionColored),
				StarCount, drawArgs.device,
				Usage.Points | Usage.WriteOnly,
				CustomVertex.PositionColored.Format,
				Pool.Managed );
			CustomVertex.PositionColored[] verts = new CustomVertex.PositionColored[StarCount];
			
			// Create mesh for flares
			int vertIndex=0;
			CustomVertex.PositionTextured pnt;
			Vector3 v;
			int numVertices = 4 * FlareCount;
			int numFaces	= 2 * FlareCount;
			FlareMesh = new Mesh(numFaces, numVertices, MeshFlags.Managed, CustomVertex.PositionTextured.Format, drawArgs.device);
			// Get the original mesh's vertex buffer.
			int [] ranks = new int[1];
			ranks[0] = FlareMesh.NumberVertices;
			System.Array arr = FlareMesh.VertexBuffer.Lock(0,typeof(CustomVertex.PositionTextured),LockFlags.None,ranks);
			
			// Now process star data and build vertex list and flare mesh
			double longitude = 0;
			double latitude = 0;
			double maxVdec = 0;
			double maxBVdec = -99;
			double minBVdec = 99;
			isData = 0;
			tr = File.OpenText(Path.Combine(pluginPath, catalogFileName));
            while ((line = tr.ReadLine()) != null) 
			{
				if(line.Length < 3) continue;
				if(line.Substring(0, 1) == "#") continue;
				if(isData == 1) // Star data here
				{
					// Data in ';' separated values
					string[] starData = line.Split(';');
					string RAhms = starData[idxRAhms];	// Right Asc in H, min, sec 	"00 01 35.85"
					string DEdms = starData[idxDEdms];	// Declinaison Degre min sec	"-77 03 55.1"
					string Vmag = starData[idxVmag];	// Aparent magnitude	" 4.78"
					string BV = starData[idxBV];		// B-V spectral color	" 1.254"
					// compute RAhms into longitude
					double RAh = Convert.ToDouble(RAhms.Substring(0, 2));
					double RAm = Convert.ToDouble(RAhms.Substring(3, 2));
					double RAs = Convert.ToDouble(RAhms.Substring(6, 5).Replace(".", DecSep)); 
					longitude = (RAh * 15) + (RAm * .25) + (RAs * 0.0041666) - 180;
					// compute DEdms into latitude
					string DEsign = DEdms.Substring(0, 1);
					double DEd = Convert.ToDouble(DEdms.Substring(1, 2));
					double DEm = Convert.ToDouble(DEdms.Substring(4, 2));
					double DEs = Convert.ToDouble(DEdms.Substring(7, 4).Replace(".", DecSep)); 
					latitude = DEd + (DEm / 60) + (DEs / 3600);
					if(DEsign == "-") latitude *= -1;
					// compute aparent magnitude -1.5 - 10 to grayscale 0 - 255
					double VM = Convert.ToDouble(Vmag.Replace(".", DecSep));
					double Vdec = 255 - ((VM + 1.5) * 255 / 10);
					if(Vdec > maxVdec) maxVdec = Vdec;
					Vdec += 20; // boost luminosity
					if(Vdec > 255) Vdec = 255;
					// convert B-V  -0.5 - 4 for rgb color select
					double BVdec = 0;
					try {BVdec = Convert.ToDouble(BV.Replace(".", DecSep));}
					catch {BVdec = 0;}
					if(BVdec > maxBVdec) maxBVdec = BVdec;
					if(BVdec < minBVdec) minBVdec = BVdec;
					
					// Place vertex for point star
					v = MathEngine.SphericalToCartesian( latitude, longitude, sphereRadius);			
					verts[idx].Position = new Vector3( v.X, v.Y, v.Z );
					// color based on B-V
					verts[idx].Color = Color.FromArgb(255, (int)Vdec, (int)Vdec, (int)Vdec).ToArgb(); // gray scale default
					if(BVdec < 4)   verts[idx].Color = Color.FromArgb(255, (int)(235*Vdec/255), (int)(96*Vdec/255), (int)(10*Vdec/255)).ToArgb(); // redish
					if(BVdec < 1.5) verts[idx].Color = Color.FromArgb(255, (int)(246*Vdec/255), (int)(185*Vdec/255), (int)(20*Vdec/255)).ToArgb(); // orange
					if(BVdec < 1)   verts[idx].Color = Color.FromArgb(255, (int)(255*Vdec/255), (int)(251*Vdec/255), (int)(68*Vdec/255)).ToArgb(); // yellow
					if(BVdec < .5)  verts[idx].Color = Color.FromArgb(255, (int)(255*Vdec/255), (int)(255*Vdec/255), (int)(255*Vdec/255)).ToArgb(); // white
					if(BVdec < 0)   verts[idx].Color = Color.FromArgb(255, (int)(162*Vdec/255), (int)(195*Vdec/255), (int)(237*Vdec/255)).ToArgb(); // light blue

					// Next vertex
					idx++;

					// if flare add 4 vertex to mesh
					if(VM < FlareMag) 
					{
						double flareFactor = sphereRadius * 5 / drawArgs.device.Viewport.Width;
						double l = (VM + 1.5) / (FlareMag + 1.5) * flareFactor;	// Size of half flare texture in meter
						// Calculate perp1 and perp2 so they form a plane perpendicular to the star vector and crossing earth center
						Vector3 perp1 = Vector3.Cross( v, new Vector3(1,1,1) );
						Vector3 perp2 = Vector3.Cross( perp1, v );
						perp1.Normalize();
						perp2.Normalize();
						perp1.Scale((float)l);
						perp2.Scale((float)l);
						Vector3 v1;

						//v = MathEngine.SphericalToCartesian( latitude + l, longitude - l, sphereRadius);			
						v1 = v + perp1 - perp2;
						pnt = new CustomVertex.PositionTextured();
						pnt.Position = new Vector3( v1.X, v1.Y, v1.Z );
						pnt.Tu = 0;
						pnt.Tv = 0;
						arr.SetValue(pnt,vertIndex++);
						//v = MathEngine.SphericalToCartesian( latitude + l, longitude + l, sphereRadius);			
						v1 = v + perp1 + perp2;
						pnt = new CustomVertex.PositionTextured();
						pnt.Position = new Vector3( v1.X, v1.Y, v1.Z );
						pnt.Tu = 1;
						pnt.Tv = 0;
						arr.SetValue(pnt,vertIndex++);
						//v = MathEngine.SphericalToCartesian( latitude - l, longitude - l, sphereRadius);			
						v1 = v - perp1 - perp2;
						pnt = new CustomVertex.PositionTextured();
						pnt.Position = new Vector3( v1.X, v1.Y, v1.Z );
						pnt.Tu = 0;
						pnt.Tv = 1;
						arr.SetValue(pnt,vertIndex++);
						//v = MathEngine.SphericalToCartesian( latitude - l, longitude + l, sphereRadius);			
						v1 = v - perp1 + perp2;
						pnt = new CustomVertex.PositionTextured();
						pnt.Position = new Vector3( v1.X, v1.Y, v1.Z );
						pnt.Tu = 1;
						pnt.Tv = 1;
						arr.SetValue(pnt,vertIndex++);
					}
					

				}
				if(line.Substring(0, 3) == "---") isData = 1;
			}
			tr.Close();
			//MessageBox.Show("FlareCount : " + FlareCount.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Error );


			// Set vertex buffer for stars
			StarListVB.SetData( verts, 0, LockFlags.None );

			// Set flare mesh indices
			FlareMesh.VertexBuffer.Unlock();
			ranks[0] = numFaces * 3;
			arr = FlareMesh.LockIndexBuffer(typeof(short),LockFlags.None,ranks);
			vertIndex = 0;
			for(int flare = 0; flare < FlareCount; flare++)
			{
				short v1 = (short)(flare * 4);
				arr.SetValue(v1, vertIndex++);
				arr.SetValue((short)(v1 + 1), vertIndex++);
				arr.SetValue((short)(v1 + 2),vertIndex++);
				arr.SetValue((short)(v1 + 1), vertIndex++);
				arr.SetValue((short)(v1 + 3), vertIndex++);
				arr.SetValue((short)(v1 + 2),vertIndex++);
			}
			FlareMesh.IndexBuffer.SetData(arr,0,LockFlags.None);
            FlareMesh.UnlockIndexBuffer();
        }

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
			if(!isInitialized)
				Initialize(drawArgs);
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
			isInitialized = false;
			if(StarListVB != null)
			{
				StarListVB = null;
			}
			if(texture != null)
			{
				texture.Dispose();
				texture = null;
			}
			if(FlareMesh != null)
			{
				FlareMesh.Dispose();
				FlareMesh = null;
			}
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

		/// <summary>
		/// Fills the context menu with menu items specific to the layer.
		/// </summary>
		public override void BuildContextMenu( ContextMenu menu )
		{
			menu.MenuItems.Add("Properties", new System.EventHandler(OnPropertiesClick));
		}

		/// <summary>
		/// Properties context menu clicked.
		/// </summary>
		public new void OnPropertiesClick(object sender, EventArgs e)
		{
			if(pDialog != null && ! pDialog.IsDisposed)
				// Already open
				return;

			// Display the dialog
			pDialog = new propertiesDialog(this);
			pDialog.Show();

		}

		/// <summary>
		/// Properties Dialog
		/// </summary>
		public class propertiesDialog : System.Windows.Forms.Form
		{
			private System.Windows.Forms.Label lblTexture;
			private System.Windows.Forms.ComboBox cboTexture;
			private System.Windows.Forms.Label lblFlares;
			private System.Windows.Forms.CheckBox chkFlares;
			private System.Windows.Forms.Button btnOK;
			private System.Windows.Forms.Button btnCancel;
			private Stars3DLayer layer;

			public propertiesDialog( Stars3DLayer layer )
			{
				InitializeComponent();
				//this.Icon = WorldWind.PluginEngine.Plugin.Icon;
				this.layer = layer;
				// Init star catalog list with *.tsv
				DirectoryInfo di = new DirectoryInfo(layer.pluginPath);
				FileInfo[] imgFiles = di.GetFiles("*.tsv");
				cboTexture.Items.AddRange(imgFiles);
				// select current catalog
				int i = cboTexture.FindString(layer.catalogFileName);
				if(i != -1) cboTexture.SelectedIndex = i;
				// Flares
				chkFlares.Checked = layer.showFlares;
			}

			#region Windows Form Designer generated code
			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			private void InitializeComponent()
			{
				this.btnCancel = new System.Windows.Forms.Button();
				this.btnOK = new System.Windows.Forms.Button();
				this.lblTexture = new System.Windows.Forms.Label();
				this.cboTexture = new System.Windows.Forms.ComboBox();
				this.lblFlares = new System.Windows.Forms.Label();
				this.chkFlares = new System.Windows.Forms.CheckBox();
				this.SuspendLayout();
				// 
				// btnCancel
				// 
				this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.btnCancel.Location = new System.Drawing.Point(311, 59);
				this.btnCancel.Name = "btnCancel";
				this.btnCancel.TabIndex = 0;
				this.btnCancel.Text = "Cancel";
				this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
				// 
				// btnOK
				// 
				this.btnOK.Location = new System.Drawing.Point(224, 59);
				this.btnOK.Name = "btnOK";
				this.btnOK.TabIndex = 1;
				this.btnOK.Text = "OK";
				this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
				// 
				// lblTexture
				// 
				this.lblTexture.AutoSize = true;
				this.lblTexture.Location = new System.Drawing.Point(16, 28);
				this.lblTexture.Name = "lblTexture";
				this.lblTexture.Size = new System.Drawing.Size(82, 16);
				this.lblTexture.TabIndex = 2;
				this.lblTexture.Text = "Stars catalog:";
				// 
				// cboTexture
				// 
				this.cboTexture.Location = new System.Drawing.Point(96, 25);
				this.cboTexture.Name = "cboTexture";
				this.cboTexture.Size = new System.Drawing.Size(296, 21);
				this.cboTexture.TabIndex = 3;
				this.cboTexture.Text = "Select catalog file";
				this.cboTexture.DropDownStyle = ComboBoxStyle.DropDownList;
				this.cboTexture.MaxDropDownItems = 10;
				// 
				// lblFlares
				// 
				this.lblFlares.AutoSize = true;
				this.lblFlares.Location = new System.Drawing.Point(16, 59);
				this.lblFlares.Name = "lblFlares";
				this.lblFlares.Size = new System.Drawing.Size(82, 16);
				this.lblFlares.TabIndex = 4;
				this.lblFlares.Text = "Bright stars :";
				// 
				// chkFlares
				// 
				this.chkFlares.Location = new System.Drawing.Point(96, 55);
				this.chkFlares.Name = "chkFlares";
				this.chkFlares.TabIndex = 5;
				this.chkFlares.Text = "Show halo";
				// 
				// frmFavorites
				// 
				this.AcceptButton = this.btnOK;
				this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				this.CancelButton = this.btnCancel;
				this.ClientSize = new System.Drawing.Size(406, 94);
				this.ControlBox = false;
				this.Controls.Add(this.cboTexture);
				this.Controls.Add(this.lblTexture);
				this.Controls.Add(this.lblFlares);
				this.Controls.Add(this.chkFlares);
				this.Controls.Add(this.btnOK);
				this.Controls.Add(this.btnCancel);
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "pDialog";
				this.ShowInTaskbar = false;
				this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
				//this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
				//this.Location = new System.Drawing.Point(layer.drawArgs.CurrentMousePosition.X + 10, layer.drawArgs.CurrentMousePosition.Y - 10);
				this.Text = "Stars properties";
				this.TopMost = true;
				this.ResumeLayout(false);

			}
			#endregion

			private void btnOK_Click(object sender, System.EventArgs e)
			{
				if(cboTexture.SelectedItem != null) 
				{
					//System.Windows.Forms.MessageBox.Show("Texture : " + cboTexture.SelectedItem.ToString());
					layer.Dispose();
					layer.catalogFileName = cboTexture.SelectedItem.ToString();
					layer.showFlares = chkFlares.Checked;
					layer.Initialize(layer.drawArgs);
					layer.SaveSettings();
				}
				// Close this form
				this.Close();
			}

			private void btnCancel_Click(object sender, System.EventArgs e)
			{

				// Close this form
				this.Close();
			}





		}



		/// <summary>
		/// Creates a PositionColored sphere centered on zero
		/// </summary>
		/// <param name="device">The current direct3D drawing device.</param>
		/// <param name="radius">The sphere's radius</param>
		/// <param name="slices">Number of slices (Horizontal resolution).</param>
		/// <param name="stacks">Number of stacks. (Vertical resolution)</param>
		/// <returns></returns>
		/// <remarks>
		/// Number of vertices in the sphere will be (slices+1)*(stacks+1)<br/>
		/// Number of faces	:slices*stacks*2
		/// Number of Indexes	: Number of faces * 3;
		/// </remarks>
		private Mesh ColoredSphere(Device device, float radius, int slices, int stacks)
		{
			int numVertices = (slices+1)*(stacks+1);
			int numFaces	= slices*stacks*2;
			int indexCount	= numFaces * 3;

			Mesh mesh = new Mesh(numFaces,numVertices,MeshFlags.Managed,CustomVertex.PositionColored.Format,device);

			// Get the original sphere's vertex buffer.
			int [] ranks = new int[1];
			ranks[0] = mesh.NumberVertices;
			System.Array arr = mesh.VertexBuffer.Lock(0,typeof(CustomVertex.PositionColored),LockFlags.None,ranks);

			// Set the vertex buffer
			int vertIndex=0;
			for(int stack=0;stack<=stacks;stack++)
			{
				double latitude = -90 + ((float)stack/stacks*(float)180.0);
				for(int slice=0;slice<=slices;slice++)
				{
					CustomVertex.PositionColored pnt = new CustomVertex.PositionColored();
					double longitude = 180 - ((float)slice/slices*(float)360);
					Vector3 v = MathEngine.SphericalToCartesian( latitude, longitude, radius);
					pnt.X = v.X;
					pnt.Y = v.Y;
					pnt.Z = v.Z;
					pnt.Color = Color.Black.ToArgb();
					//pnt.Tu = (float)slice/slices;
					//pnt.Tv = 1.0f-(float)stack/stacks;
					arr.SetValue(pnt,vertIndex++);
				}
			}

			mesh.VertexBuffer.Unlock();
			ranks[0]=indexCount;
			arr = mesh.LockIndexBuffer(typeof(short),LockFlags.None,ranks);
			int i=0;
			short bottomVertex = 0;
			short topVertex = 0;
			for(short x=0;x<stacks;x++)
			{
				bottomVertex = (short)((slices+1)*x);
				topVertex = (short)(bottomVertex + slices + 1);
				for(int y=0;y<slices;y++)
				{
					arr.SetValue(bottomVertex,i++);
					arr.SetValue((short)(topVertex+1),i++);
					arr.SetValue(topVertex,i++);
					arr.SetValue(bottomVertex,i++);
					arr.SetValue((short)(bottomVertex+1),i++);
					arr.SetValue((short)(topVertex+1),i++);
					bottomVertex++;
					topVertex++;
				}
			}
			mesh.IndexBuffer.SetData(arr,0,LockFlags.None);
			//mesh.ComputeNormals();

			return mesh;
		}


		#endregion
	}
}
