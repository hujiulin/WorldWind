//----------------------------------------------------------------------------
// NAME: GlobalClouds
// VERSION: 0.6
// DESCRIPTION: Download and renders the latest global cloud cover around the world. Updates every three hours and keeps ten days history. Adds itself as a layer in Layer Manager (key: L). Right click on layer for settings.
// DEVELOPER: Patrick Murris
// WEBSITE: http://www.alpix.com/3d/worldwin
//----------------------------------------------------------------------------
// Based on the Stars plugin, itself based on Bjorn Reppen 'Atmosphere' plugin
// Latest cloud map comes from a random server listed at http://xplanet.sourceforge.net/clouds.php 
// and is mirrored at http://www.twobeds.com/nasa/clouds/clouds_2048.jpg for this plugin.
//
// 0.6 Feb  10, 2006	Fixed 'Error loading texture' bug after 10 days plugin not used (History cleanup at starup removed)
// 0.5 Dec  22, 2005	Promoted to RenderPriority.AtmosphericImages
// 0.4 Dec   6, 2005	Added 'Earth only' test and fixed ZBuffer bug
// 0.3 Nov  29, 2005	Added next and previous buttons in settings 
// 0.2 Nov  26, 2005	Added download and processing of latest cloud cover 
// 0.1 Nov  22, 2005	Testing global cloud layer... 
//----------------------------------------------------------------------------
// Known issues : Doesnt handle http errors as expected (try/catch/retrycount++...)
//		: May leave garbage if plugin unloaded while downloading
//----------------------------------------------------------------------------


using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using WorldWind.Renderable;
using WorldWind.Camera;
using WorldWind.Net;
using WorldWind.VisualControl;
using WorldWind;
using System.IO;
using System.Drawing;
using System.Net;
using System.Collections;
using System;

namespace Murris.Plugins
{
	/// <summary>
	/// The plugin (main class)
	/// </summary>
	public class GlobalClouds : WorldWind.PluginEngine.Plugin 
	{
        private WorldWind.WindowsControlMenuButton m_ToolbarItem;
        private Control control = new Control();
        private EventHandler evhand;
        private GlobalCloudsLayer layer;
		/// <summary>
		/// Name displayed in layer manager
		/// </summary>
		public static string LayerName = "GlobalClouds";

		/// <summary>
		/// Plugin entry point - All plugins must implement this function
		/// </summary>
        public override void Load()
        {
            if (ParentApplication.WorldWindow.CurrentWorld != null && ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("Earth") >= 0)
            {
                // Add layer visibility controller (and save it to make sure you can kill it later!)
                control.Visible = true;
                evhand = new EventHandler(control_VisibleChanged);
                control.VisibleChanged += evhand;
                // Add toolbar item
                m_ToolbarItem = new WorldWind.WindowsControlMenuButton("Global Clouds", Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), @"Data\Icons\Interface\earth-eastern.png"), control);
                m_Application.WorldWindow.MenuBar.AddToolsMenuButton(m_ToolbarItem);

                layer = new GlobalCloudsLayer(LayerName, PluginDirectory, ParentApplication.WorldWindow);
				layer.IsOn = World.Settings.ShowClouds;
                //ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.ChildObjects.Insert(0,layer);
                ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Add(layer);

				m_ToolbarItem.SetPushed(World.Settings.ShowClouds);
               
            }
        }

		/// <summary>
		/// Unloads our plugin
		/// </summary>
		public override void Unload() 
		{
            // Remove layer controller
            control.VisibleChanged -= evhand;
            control.Dispose();

            // Remove toolbar item
            if (m_ToolbarItem != null)
                m_Application.WorldWindow.MenuBar.RemoveToolsMenuButton(m_ToolbarItem);

			ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Remove(LayerName);
		}
        /// <summary>
        /// Toggles visibility on the CompassLayer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void control_VisibleChanged(object sender, EventArgs e)
        {
            if (control.Visible)
                layer.IsOn = true;
            else
                layer.IsOn = false;
        }
	}

	/// <summary>
	/// GlobalCloudsLayer
	/// </summary>
	public class GlobalCloudsLayer : RenderableObject
	{
		static string version = "0.6";
		string settingsFileName = "GlobalClouds.ini";
		string serverListFileName = "GlobalCloudsServers.txt";
		string pluginPath;
		public World world;
		public DrawArgs drawArgs;
		Mesh layerMesh;
		Texture texture = null;
		Form pDialog;

		// current GlobalClouds bitmap
		public string textureFileName = "";

		public string latestFileName = "";
		public DateTime latestTime = DateTime.MinValue;
		public int historyDays = 10;					// How many days to keep cloud maps
		public int refreshHours = 3;					// How often to refresh cloud map

		/// <summary>
		/// Constructor
		/// </summary>
		public GlobalCloudsLayer(string LayerName, string pluginPath, WorldWindow worldWindow) : base(LayerName)
		{
			this.pluginPath = pluginPath;
			this.world = worldWindow.CurrentWorld;
			this.drawArgs = worldWindow.DrawArgs;
			this.RenderPriority = RenderPriority.AtmosphericImages;
			//CleanupHistory();
			CleanupJpg();
			FindLatest();
			ReadSettings();
			if(textureFileName == "" && latestFileName != "") textureFileName = latestFileName;

			//MessageBox.Show("Server url : " + GetServerUrl(),"Info.", MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		
		/// <summary>
		/// Find which cloud map is the latest and at what date/time
		/// </summary>
		public void FindLatest()
		{
			DirectoryInfo di = new DirectoryInfo(pluginPath);
			FileInfo[] imgFiles = di.GetFiles("clouds*.png");
			for(int i = 0; i < imgFiles.Length; i++)
			{
				if(imgFiles[i].LastWriteTime > latestTime)
				{
					latestFileName = imgFiles[i].Name;
					latestTime = imgFiles[i].LastWriteTime;
				}
			}
		}

		public override bool IsOn
		{
			get
			{
				return base.IsOn;
			}
			set
			{
				World.Settings.ShowClouds = value;
				base.IsOn = value;
			}
		}


		/// <summary>
		/// Delete cloud maps older than historyDays
		/// </summary>
		public void CleanupHistory()
		{
			DateTime oldest = DateTime.Now.AddDays(-historyDays);
			DirectoryInfo di = new DirectoryInfo(pluginPath);
			FileInfo[] imgFiles = di.GetFiles("clouds*.png");
			for(int i = 0; i < imgFiles.Length; i++)
			{
				if(imgFiles[i].LastWriteTime < oldest)
				{
					File.Delete(Path.Combine(pluginPath, imgFiles[i].Name));
				}
			}
		}
		/// <summary>
		/// Delete .jpg cloud maps
		/// </summary>
		public void CleanupJpg()
		{
			DirectoryInfo di = new DirectoryInfo(pluginPath);
			FileInfo[] imgFiles = di.GetFiles("clouds*.jpg");
			for(int i = 0; i < imgFiles.Length; i++)
			{
				File.Delete(Path.Combine(pluginPath, imgFiles[i].Name));
			}
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
			catch(Exception caught) {}
			if(line != "")
			{
				string[] settingsList = line.Split(';');
				string saveVersion = settingsList[0];	// version when settings where saved
				if(settingsList[1] != null) textureFileName = settingsList[1];
			}
		}

		/// <summary>
		/// Save settings in ini file
		/// </summary>
		public void SaveSettings()
		{
			string line = version + ";" + textureFileName;
			try
			{
				StreamWriter sw = new StreamWriter(Path.Combine(pluginPath, settingsFileName));
				sw.Write(line);
				sw.Close();
			}
			catch(Exception caught) {}
		}

		#region RenderableObject

		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			if(!isInitialized) return;
			if(world.Name != "Earth") return;	// Earth only

			// Check for update
			if(!isDownloading && DateTime.Now > latestTime.AddHours(refreshHours)) 
			{
				if(retryCount < maxRetry && DateTime.Now > lastDownloadTime.AddSeconds(retryDelaySeconds))
				{
					StartDownload(pluginPath, "clouds_" + DateTimeStamp(DateTime.Now) + ".jpg");
				}
			}

			// Camera & Device shortcuts ;)
			CameraBase camera = drawArgs.WorldCamera;
			//Device device = drawArgs.device;

			// Render cloud layer
			if(texture != null)
			{
				double cloudAlt = 20e3; // clouds altitude in meters (20 x 10e3)

				if(camera.Altitude < 4000e3) return;

				double sphereRadius = camera.WorldRadius + cloudAlt; 
			
				// Create sphere
				if(layerMesh == null) 
					layerMesh = TexturedSphere(drawArgs.device, (float)sphereRadius, 64, 64);
			
				// set texture
				drawArgs.device.SetTexture(0,texture);
                drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Modulate;
                drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                drawArgs.device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
                drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
                drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
				drawArgs.device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
			
				// save world and projection transform
				Matrix origWorld = drawArgs.device.Transform.World;
				Matrix origProjection = drawArgs.device.Transform.Projection;

				// Save fog status and disable fog
				bool origFog = drawArgs.device.RenderState.FogEnable;
				drawArgs.device.RenderState.FogEnable = false;

				// Set new projection (to avoid being clipped) - probably better ways of doing this?
				float aspectRatio =  (float)drawArgs.device.Viewport.Width / drawArgs.device.Viewport.Height;
				drawArgs.device.Transform.Projection = Matrix.PerspectiveFovRH((float)camera.Fov.Radians, aspectRatio, 1, float.MaxValue );
				
				//translate to the camera reference center
				drawArgs.device.Transform.World = Matrix.Translation(
					(float)-drawArgs.WorldCamera.ReferenceCenter.X,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Z
					);

				// draw
				drawArgs.device.RenderState.ZBufferEnable = false;
				layerMesh.DrawSubset(0);

				// Restore device states
				drawArgs.device.Transform.World = origWorld;
				drawArgs.device.Transform.Projection = origProjection;
				drawArgs.device.RenderState.FogEnable = origFog;
				drawArgs.device.RenderState.ZBufferEnable = true;
			}

			// Render progress bar if downloading
			if(isDownloading)
			{
				if(progressBar == null) progressBar = new WorldWind.VisualControl.ProgressBar(40,4);
				progressBar.Draw(drawArgs, drawArgs.screenWidth - 34, drawArgs.screenHeight - 10, ProgressPercent, downloadProgressColor);
				drawArgs.device.RenderState.ZBufferEnable = true;
			}

		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
			if(textureFileName != "")
			{
				try
				{
					texture = TextureLoader.FromFile(drawArgs.device, Path.Combine(pluginPath, textureFileName));
					isInitialized = true;	
				}
				catch
				{
					//isOn = false;
					//MessageBox.Show("Error loading texture " + Path.Combine(pluginPath, textureFileName) + ".","Layer initialization failed.", MessageBoxButtons.OK, MessageBoxIcon.Error );
					textureFileName = "";
				}
			}
			else
			{
				StartDownload(pluginPath, "clouds_" + DateTimeStamp(DateTime.Now) + ".jpg");
				isInitialized = true;	
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
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
			isInitialized = false;
			if(texture!=null)
			{
				texture.Dispose();
				texture = null;
			}

			if(layerMesh != null)
			{
				layerMesh.Dispose();
				layerMesh = null;
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
			private System.Windows.Forms.Button btnOK;
			private System.Windows.Forms.Button btnCancel;
			private System.Windows.Forms.Button btnPrevious;
			private System.Windows.Forms.Button btnNext;
			private GlobalCloudsLayer layer;
			private string savedTextureFileName;

			public propertiesDialog( GlobalCloudsLayer layer )
			{
				this.layer = layer;
				InitializeComponent();
				//this.Icon = WorldWind.PluginEngine.Plugin.Icon;
				// Init texture list with *.jpg and/or *.png
				DirectoryInfo di = new DirectoryInfo(layer.pluginPath);
				FileInfo[] imgFiles = di.GetFiles("*.png");				
				cboTexture.Items.AddRange(imgFiles);
				// select current bitmap
				int i = cboTexture.FindString(layer.textureFileName);
				if(i != -1) cboTexture.SelectedIndex = i;
				// Save current textureFileName
				savedTextureFileName = layer.textureFileName;
				//this.Text += layer.version;
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
				this.btnPrevious = new System.Windows.Forms.Button();
				this.btnNext = new System.Windows.Forms.Button();
				this.lblTexture = new System.Windows.Forms.Label();
				this.cboTexture = new System.Windows.Forms.ComboBox();
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
				// btnPrevious
				// 
				this.btnPrevious.Location = new System.Drawing.Point(20, 59);
				this.btnPrevious.Name = "btnPrevious";
				this.btnPrevious.TabIndex = 2;
				this.btnPrevious.Text = "Previous";
				this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
				// 
				// btnNext
				// 
				this.btnNext.Location = new System.Drawing.Point(106, 59);
				this.btnNext.Name = "btnNext";
				this.btnNext.TabIndex = 3;
				this.btnNext.Text = "Next";
				this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
				// 
				// lblTexture
				// 
				this.lblTexture.AutoSize = true;
				this.lblTexture.Location = new System.Drawing.Point(16, 28);
				this.lblTexture.Name = "lblTexture";
				this.lblTexture.Size = new System.Drawing.Size(82, 16);
				this.lblTexture.TabIndex = 4;
				this.lblTexture.Text = "Clouds map :";
				// 
				// cboTexture
				// 
				this.cboTexture.Location = new System.Drawing.Point(96, 25);
				this.cboTexture.Name = "cboTexture";
				this.cboTexture.Size = new System.Drawing.Size(296, 21);
				this.cboTexture.TabIndex = 5;
				this.cboTexture.Text = "Select texture file";
				this.cboTexture.DropDownStyle = ComboBoxStyle.DropDownList;
				this.cboTexture.MaxDropDownItems = 10;
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
				this.Controls.Add(this.btnOK);
				this.Controls.Add(this.btnCancel);
				this.Controls.Add(this.btnPrevious);
				this.Controls.Add(this.btnNext);
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "pDialog";
				this.ShowInTaskbar = false;
				this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
				//this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
				//this.Location = new System.Drawing.Point(layer.drawArgs.CurrentMousePosition.X + 10, layer.drawArgs.CurrentMousePosition.Y - 10);
				this.Text = "GlobalClouds properties ";
				this.TopMost = true;
				this.ResumeLayout(false);

			}
			#endregion

			private void btnOK_Click(object sender, System.EventArgs e)
			{
				if(cboTexture.SelectedItem != null) 
				{
					if(cboTexture.SelectedItem.ToString().IndexOf(".png") != -1) 
					{	// Update texture and save settings
						layer.Dispose();
						layer.textureFileName = cboTexture.SelectedItem.ToString();
						layer.Initialize(layer.drawArgs);
						layer.SaveSettings();
					}
				}
				// Close this form
				this.Close();
			}

			private void btnCancel_Click(object sender, System.EventArgs e)
			{
				if(layer.textureFileName != savedTextureFileName)
				{	// Rreset texture if it has changed
					layer.Dispose();
					layer.textureFileName = savedTextureFileName;
					layer.Initialize(layer.drawArgs);
				}
				// Close this form
				this.Close();
			}

			private void btnPrevious_Click(object sender, System.EventArgs e)
			{
				layer.GotoPrevious();
				// select current bitmap
				int i = cboTexture.FindString(layer.textureFileName);
				if(i != -1) cboTexture.SelectedIndex = i;
			}

			private void btnNext_Click(object sender, System.EventArgs e)
			{
				layer.GotoNext();
				// select current bitmap
				int i = cboTexture.FindString(layer.textureFileName);
				if(i != -1) cboTexture.SelectedIndex = i;
			}
		}

		// Switching cloud maps from history
		// ---------------------------------

		public ArrayList historyList;
		public void BuildHistoryList()
		{
			historyList = new ArrayList();
			DirectoryInfo di = new DirectoryInfo(pluginPath);
			FileInfo[] imgFiles = di.GetFiles("clouds*.png");
			for(int i = 0; i < imgFiles.Length; i++)
			{
				historyList.Add(imgFiles[i].Name);
			}
			historyList.Sort();
		}


		public void GotoPrevious()
		{
			if(historyList == null) BuildHistoryList();
			int i = historyList.IndexOf(textureFileName);
			if(i != -1)
			{
				if(i > 0) i--;  else i = historyList.Count - 1;
				textureFileName = (string)historyList[i];
				Dispose();
				Initialize(drawArgs);
			}
		}

		public void GotoNext()
		{
			if(historyList == null) BuildHistoryList();
			int i = historyList.IndexOf(textureFileName);
			if(i != -1)
			{
				if(i < historyList.Count - 1) i++;  else i = 0;
				textureFileName = (string)historyList[i];
				Dispose();
				Initialize(drawArgs);
			}
		}


		// Downloading new cloud maps
		// --------------------------

		public bool isDownloading = false;
		public float ProgressPercent;
		WebDownload download;
		WorldWind.VisualControl.ProgressBar progressBar;
		int downloadProgressColor = Color.FromArgb(180,200,200,200).ToArgb();
		int maxRetry = 3;
		int retryCount = 0;
		int retryDelaySeconds = 60;
		DateTime lastDownloadTime = DateTime.MinValue;
		
		private void DownloadCloudMap(string filePath, string fileName) // NOT USED (synchronous)
		{
			string url = GetServerUrl();
			System.Net.WebClient wc = new System.Net.WebClient();
			wc.DownloadFile(url, Path.Combine(filePath, fileName));
			wc.Dispose();
		}

		public virtual void StartDownload(string filePath, string fileName) // Asynch download here
		{
			string url = GetServerUrl();
			download = new WebDownload(url);
			download.SavedFilePath = Path.Combine(filePath, fileName);
			download.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
			download.CompleteCallback += new DownloadCompleteHandler(DownloadComplete);
			download.BackgroundDownloadFile();
			isDownloading = true;
		}

		// Read server list and pick one
		public string GetServerUrl()
		{
			Random r = new Random();
			ArrayList serverList = new ArrayList();
			string line;
			TextReader tr = File.OpenText(Path.Combine(pluginPath, serverListFileName));
			while ((line = tr.ReadLine()) != null) 
			{
				if(line.StartsWith("http://")) serverList.Add(line);
			}
			tr.Close();
			string Url = (string)serverList[r.Next(serverList.Count - 1)];
			return Url;
		}

		void UpdateProgress( int pos, int total )
		{
			if(total==0)
				// When server doesn't provide content-length, use this dummy value to at least show some progress.
				total = 50*1024; 
			pos = pos % (total+1);
			ProgressPercent = (float)pos/total;
		}

		// Download done: process image and update current texture (or deal with errors)
		private void DownloadComplete(WebDownload downloadInfo)
		{
			try
			{
				// Errors are annoying because they repeat
				//downloadInfo.Verify();
				
				// To do : read original cloud date in comment info inside jpg
			
				// Process image and creat .png
				MakeAlphaPng(downloadInfo.SavedFilePath);
				//Delete jpg and cleanup history
				//File.Delete(downloadInfo.SavedFilePath);
				CleanupHistory();
				// Refresh latest info (date and filename)
				FindLatest();
				// Update texture with latest
				Dispose();
				textureFileName = latestFileName;
				Initialize(drawArgs);
				SaveSettings();
				retryCount = 0;

			}
			catch(System.Net.WebException caught) // This doesnt work ;(
			{
				System.Net.HttpWebResponse response = caught.Response as System.Net.HttpWebResponse;
				//if(response!=null && response.StatusCode==System.Net.HttpStatusCode.NotFound)
				if(response!=null)
				{
					// display response.StatusDescription;
					//MessageBox.Show("Error downloading cloud map from " + downloadInfo.Url + " (" + response.StatusDescription + ").", "Download failed.", MessageBoxButtons.OK, MessageBoxIcon.Error );
					Utility.Log.Write(Utility.Log.Levels.Warning, "Error downloading cloud map from " + downloadInfo.Url + " (" + response.StatusDescription + ").");
					
					//return;
				}
				retryCount++;
			}
			catch(Exception caugth)
			{
				//MessageBox.Show("Error downloading cloud map from " + downloadInfo.Url + " (" + caugth.Message + ").", "Download failed.", MessageBoxButtons.OK, MessageBoxIcon.Error );
				Utility.Log.Write(Utility.Log.Levels.Warning, "Error downloading cloud map from " + downloadInfo.Url + " (" + caugth.Message + ").");

				if(File.Exists(downloadInfo.SavedFilePath))
					File.Delete(downloadInfo.SavedFilePath);
				retryCount++;
			}
			finally
			{
				download.IsComplete = true;
				isDownloading = false;
				lastDownloadTime = DateTime.Now;
			}
		}

		// Return a date formated as YYYYMMDD-HHMM
		public string DateTimeStamp(DateTime d) 
		{
			return d.Year.ToString() + d.Month.ToString("d2") + d.Day.ToString("d2") + "-" + d.Hour.ToString("d2") + d.Minute.ToString("d2");

		}

		/// <summary>
		/// Creates a alpha channelled .png out of a B&W .jpg
		/// </summary>
		/// <param name="filePath">The path to the .jpg file.</param>
		/// <param name="fileName">The file name of the .jpg.</param>
		private void MakeAlphaPng(string filePath)
		{
			Bitmap b1, b2;
			int x, y;
			b1 = new Bitmap(filePath);
			b2 = new Bitmap(b1.Width, b1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			for(x = 0; x < b1.Width; x++)
			{
				for(y = 0; y < b1.Height; y++)
				{
					Color p = b1.GetPixel(x,y);
					b2.SetPixel(x, y, Color.FromArgb(p.R, p.R, p.G, p.B));
				}
				//ProgressPercent = (float)(x / (b1.Width - 1));
			}
			b2.Save(filePath.Replace(".jpg", ".png"), System.Drawing.Imaging.ImageFormat.Png);
			b1.Dispose();
			b2.Dispose();
			b1 = null;
			b2 = null;
		}


		/// <summary>
		/// Creates a PositionNormalTextured sphere centered on zero
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
		private Mesh TexturedSphere(Device device, float radius, int slices, int stacks)
		{
			int numVertices = (slices+1)*(stacks+1);
			int numFaces	= slices*stacks*2;
			int indexCount	= numFaces * 3;

			Mesh mesh = new Mesh(numFaces,numVertices,MeshFlags.Managed,CustomVertex.PositionNormalTextured.Format,device);

			// Get the original sphere's vertex buffer.
			int [] ranks = new int[1];
			ranks[0] = mesh.NumberVertices;
			System.Array arr = mesh.VertexBuffer.Lock(0,typeof(CustomVertex.PositionNormalTextured),LockFlags.None,ranks);

			// Set the vertex buffer
			int vertIndex=0;
			for(int stack=0;stack<=stacks;stack++)
			{
				double latitude = -90 + ((float)stack/stacks*(float)180.0);
				for(int slice=0;slice<=slices;slice++)
				{
					CustomVertex.PositionNormalTextured pnt = new CustomVertex.PositionNormalTextured();
					double longitude = 180 - ((float)slice/slices*(float)360);
					Vector3 v = MathEngine.SphericalToCartesian( latitude, longitude, radius);
					pnt.X = v.X;
					pnt.Y = v.Y;
					pnt.Z = v.Z;
					pnt.Tu = 1.0f-(float)slice/slices;
					pnt.Tv = 1.0f-(float)stack/stacks;
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
					arr.SetValue(topVertex,i++);		// outside text.
					arr.SetValue((short)(topVertex+1),i++);	// outside text.
					arr.SetValue(bottomVertex,i++);
					arr.SetValue((short)(topVertex+1),i++);	// outside text.
					arr.SetValue((short)(bottomVertex+1),i++); // outside text.
					bottomVertex++;
					topVertex++;
				}
			}
			mesh.IndexBuffer.SetData(arr,0,LockFlags.None);
            mesh.IndexBuffer.Unlock();
			mesh.ComputeNormals();

			return mesh;
		}


		#endregion
	}
}
