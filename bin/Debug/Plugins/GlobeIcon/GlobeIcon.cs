//----------------------------------------------------------------------------
// NAME: Globe Icon
// VERSION: 0.5
// DESCRIPTION: WW 1.4 : Displays a mini globe showing your current location. Adds itself as a layer in Layer Manager (key: L). Right click on layer for settings.
// DEVELOPER: Patrick Murris
// WEBSITE: http://www.alpix.com/3d/worldwin
//----------------------------------------------------------------------------
// Based on the Compass plugin, itself based on Bjorn Reppen 'Atmosphere' plugin
// 0.6 Jan	20, 2007	Add setting to remember on/off status and fix ./, issue
// 0.5 Nov   7, 2006	WW 1.4 : fixed device.TextureState[0] in Render()
//			Fixed RenderPriority
// 0.4 Mar  10, 2006	Fixed device.Transform.View bug (Thanks nht)
// 0.3 Dec   7, 2005	Fixed a couple things (Top-Left placement and texture directory path)
// 0.2 Dec   6, 2005	Added 'Earth only' test and optimized rendering
// 0.1 Dec   6, 2005	First try
//----------------------------------------------------------------------------

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using WorldWind.Renderable;
using WorldWind.Camera;
using WorldWind;
using System.IO;
using System.Drawing;
using System;
using System.Globalization;

namespace Murris.Plugins
{
    /// <summary>
    /// The plugin (main class)
    /// </summary>
    public class GlobeIcon : WorldWind.PluginEngine.Plugin
    {
        /// <summary>
        /// Name displayed in layer manager
        /// </summary>
        public static string LayerName = "Globe Overview";

        WorldWind.WindowsControlMenuButton m_ToolbarItem;
        private Control control = new Control();
        EventHandler evhand;
        GlobeIconLayer layer;

        /// <summary>
        /// Plugin entry point - All plugins must implement this function
        /// </summary>
        public override void Load()
        {
            layer = new GlobeIconLayer(LayerName, PluginDirectory, ParentApplication.WorldWindow);

            // Add layer visibility controller (and save it to make sure you can kill it later!)
            control.Visible = true;
            evhand = new EventHandler(control_VisibleChanged);
            control.VisibleChanged += evhand;

            string imgPath = Path.Combine(PluginDirectory, "GlobeIcon.png");

            if (File.Exists(imgPath) == false)
            {
                Utility.Log.Write(new Exception("imgPath not found " + imgPath));
            }
            m_ToolbarItem = new WorldWind.WindowsControlMenuButton(
                "Globe Overview",
                imgPath,
                control);

            ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_ToolbarItem);


            //ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.ChildObjects.Insert(0,layer);
            ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Add(layer);
            m_ToolbarItem.SetPushed(layer.IsOn);
        }

        /// <summary>
        /// Unloads our plugin
        /// </summary>
        public override void Unload()
        {
		// write settings
		layer.SaveSettings();

            // Remove layer controller
            control.VisibleChanged -= evhand;
            control.Dispose();

            // Remove toolbar item
            if (m_ToolbarItem != null)
                m_Application.WorldWindow.MenuBar.RemoveToolsMenuButton(m_ToolbarItem);

            ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Remove(LayerName);
        }

        private void control_VisibleChanged(object sender, EventArgs e)
        {
            if (control.Visible)
                layer.IsOn = true;
            else
                layer.IsOn = false;
        }
    }

    /// <summary>
    /// Plugin layer
    /// </summary>
    public class GlobeIconLayer : RenderableObject
    {
        string version = "0.5";
        string pluginName;
        string settingsFileName = "GlobeIcon.ini";
        string pluginPath;
        public World world;
        public DrawArgs drawArgs;
        float offsetY, offsetZ;					// placement offset
        Texture texture;
        Mesh spriteMesh;
        Rectangle spriteSize, insetSize;
        Point spriteOffset, insetOffset;
        string spritePos = "Bottom-Right";		// default globe placement
        string insetPos = "Bottom-Left";		// default inset placement
        int globeRadius = 40;					// default globe radius in pixel
        int insetWidth = 110;					// default inset size in pixel
        int insetHeight = 80;
        bool showGlobe = true;					// default globe is on
        bool showInset = true;					// default inset is on
        Form pDialog;

        // default GlobeIcon bitmap from BMNG Bathy
        public string textureFileName = "world.topo.bathy.200407.jpg";
        public string texturePath;

        /// <summary>
        /// Constructor
        /// </summary>
        public GlobeIconLayer(string LayerName, string pluginPath, WorldWindow worldWindow)
            : base(LayerName)
        {
            this.pluginPath = pluginPath;
            this.pluginName = LayerName;
            this.world = worldWindow.CurrentWorld;
            this.drawArgs = worldWindow.DrawArgs;
            this.RenderPriority = RenderPriority.Custom;
            this.texturePath = Path.Combine(MainApplication.DirectoryPath, "Data/Earth/BmngBathy");
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
            catch (Exception caught) { }
            if (line != "")
            {
                string[] settingsList = line.Split(';');
                string saveVersion = settingsList[1];	// version when settings where saved
                if (settingsList[1] != null) textureFileName = settingsList[1];
                if (settingsList.Length >= 3) spritePos = settingsList[2];
                if (settingsList.Length >= 4) showGlobe = (settingsList[3] == "False") ? false : true;
                if (settingsList.Length >= 5) globeRadius = int.Parse(settingsList[4], CultureInfo.InvariantCulture);
                if (settingsList.Length >= 6) insetPos = settingsList[5];
                if (settingsList.Length >= 7) showInset = (settingsList[6] == "False") ? false : true;
                if (settingsList.Length >= 8) insetWidth = int.Parse(settingsList[7], CultureInfo.InvariantCulture);
                if (settingsList.Length >= 9) insetHeight = int.Parse(settingsList[8], CultureInfo.InvariantCulture);
				if (settingsList.Length >= 10) IsOn = bool.Parse(settingsList[9]);
            }
        }

        /// <summary>
        /// Save settings in ini file
        /// </summary>
        public void SaveSettings()
        {
            string line = version + ";"			// 0
                + textureFileName + ";"			// 1
                + spritePos + ";"				// 2
                + showGlobe.ToString() + ";"	// 3
                + globeRadius.ToString(CultureInfo.InvariantCulture) + ";"	// 4
                + insetPos + ";"				// 5
                + showInset.ToString() + ";"	// 6
                + insetWidth.ToString(CultureInfo.InvariantCulture) + ";"	// 7
                + insetHeight.ToString(CultureInfo.InvariantCulture) + ";" // 8
				+ IsOn.ToString();				// 9
            try
            {
                StreamWriter sw = new StreamWriter(Path.Combine(pluginPath, settingsFileName));
                sw.Write(line);
                sw.Close();
            }
            catch (Exception caught) { }
        }

        #region RenderableObject

        /// <summary>
        /// This is where we do our rendering 
        /// Called from UI thread = UI code safe in this function
        /// </summary>
        public override void Render(DrawArgs drawArgs)
        {
            if (!isInitialized) return;
            if (world.Name != "Earth") return;	// Earth only

            if (showGlobe) RenderGlobe(drawArgs);
            if (showInset) RenderInset(drawArgs);
        }

        public void RenderGlobe(DrawArgs drawArgs)
        {
            // Camera shortcut ;)
            CameraBase camera = drawArgs.WorldCamera;
            Device device = drawArgs.device;

            // Save fog status
            bool origFog = device.RenderState.FogEnable;
            device.RenderState.FogEnable = false;

            // Turn off lights if any (WW 1.4)
            if (drawArgs.device.RenderState.Lighting)
            {
                drawArgs.device.RenderState.Lighting = false;
                drawArgs.device.RenderState.Ambient = World.Settings.StandardAmbientColor;
            }

            // Save original projection and change it to ortho
            // Note: using pixels as units produce a 1:1 projection of textures
            Matrix origProjection = device.Transform.Projection;
            device.Transform.Projection = Matrix.OrthoRH((float)device.Viewport.Width, (float)device.Viewport.Height, -(float)4e6, (float)4e6);

            // Save original View and change it to place sprite
            // Note: the sprite is centered on origin, the camera view moves.
            // Compute placement offset
            switch (spritePos)
            {
                case "Bottom-Left":
                    // lower left corner
                    offsetY = (float)device.Viewport.Width / 2 - spriteSize.Width / 2 - 10;
                    offsetZ = (float)device.Viewport.Height / 2 - spriteSize.Height / 2 - 10;
                    break;
                case "Bottom-Center":
                    // bottom centered
                    offsetY = 0;
                    offsetZ = (float)device.Viewport.Height / 2 - spriteSize.Height / 2 - 10;
                    break;
                case "Bottom-Right":
                    // bottom right
                    offsetY = -((float)device.Viewport.Width / 2 - spriteSize.Width / 2 - 60);
                    offsetZ = (float)device.Viewport.Height / 2 - spriteSize.Height / 2 - 10;
                    break;
                case "Screen-Center":
                    // plain centered
                    offsetY = 0;
                    offsetZ = 0;
                    break;
                case "Top-Right":
                    // upper right corner
                    offsetY = -((float)device.Viewport.Width / 2 - spriteSize.Width / 2 - 10);
                    offsetZ = -((float)device.Viewport.Height / 2 - spriteSize.Height / 2 - 10);
                    if (World.Settings.ShowToolbar) offsetZ += 50;
                    if (World.Settings.ShowPosition) offsetZ += 140;
                    break;
                case "Top-Center":
                    // up center
                    offsetY = 0;
                    offsetZ = -((float)device.Viewport.Height / 2 - spriteSize.Height / 2 - 10);
                    if (World.Settings.ShowToolbar) offsetZ += 50;
                    break;
                case "Top-Left":
                    // upper left corner
                    offsetY = ((float)device.Viewport.Width / 2 - spriteSize.Width / 2 - 6);
                    offsetZ = -((float)device.Viewport.Height / 2 - spriteSize.Height / 2 - 6);
                    if (World.Settings.ShowToolbar) offsetZ += 42;
                    break;
            }
            Matrix origView = device.Transform.View;
            device.Transform.View = Matrix.LookAtRH(
                new Vector3((float)1e3, offsetY, offsetZ),	// Cam pos
                new Vector3(0, offsetY, offsetZ),			// Cam target
                new Vector3(0, 0, 1));						// Up vector

            // Offset, rotate and tilt
            Matrix origWorld = device.Transform.World;
            Matrix trans = Matrix.Translation(0, -(float)spriteOffset.X, (float)spriteOffset.Y);
            trans *= Matrix.RotationZ((float)camera.Longitude.Radians + (float)Math.PI);
            trans *= Matrix.RotationY(-(float)camera.Latitude.Radians);
            device.Transform.World = trans;

            // Render globe here
            if (texture != null)
            {
                // Create mesh
                if (spriteMesh == null) spriteMesh = TexturedSphere(device, (float)globeRadius, 24, 24);
                // set texture
                device.SetTexture(0, texture);
                //device.TextureState[0].ColorOperation = TextureOperation.BlendCurrentAlpha;
                //device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
                device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
                device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;

                // draw
                spriteMesh.DrawSubset(0);
                // Draw cross hairs
                DrawCrossHairs(drawArgs, (float)device.Viewport.Width / 2 - offsetY, (float)device.Viewport.Height / 2 + offsetZ);
            }

            // Restore device states
            device.Transform.World = origWorld;
            device.Transform.Projection = origProjection;
            device.Transform.View = origView;
            device.RenderState.ZBufferEnable = true;
            device.RenderState.FogEnable = origFog;
        }

        public void RenderInset(DrawArgs drawArgs)
        {
            // Camera shortcut ;)
            CameraBase camera = drawArgs.WorldCamera;
            Device device = drawArgs.device;

            // Save fog status
            bool origFog = device.RenderState.FogEnable;
            device.RenderState.FogEnable = false;

            // Turn off lights if any (WW 1.4)
            if (drawArgs.device.RenderState.Lighting)
            {
                drawArgs.device.RenderState.Lighting = false;
                drawArgs.device.RenderState.Ambient = World.Settings.StandardAmbientColor;
            }

            // Compute placement offset
            switch (insetPos)
            {
                case "Bottom-Left":
                    // lower left corner
                    offsetY = (float)device.Viewport.Width / 2 - insetSize.Width / 2 - 10;
                    offsetZ = (float)device.Viewport.Height / 2 - insetSize.Height / 2 - 10;
                    break;
                case "Bottom-Center":
                    // bottom centered
                    offsetY = 0;
                    offsetZ = (float)device.Viewport.Height / 2 - insetSize.Height / 2 - 10;
                    break;
                case "Bottom-Right":
                    // bottom right
                    offsetY = -((float)device.Viewport.Width / 2 - insetSize.Width / 2 - 60);
                    offsetZ = (float)device.Viewport.Height / 2 - insetSize.Height / 2 - 10;
                    break;
                case "Screen-Center":
                    // plain centered
                    offsetY = 0;
                    offsetZ = 0;
                    break;
                case "Top-Right":
                    // upper right corner
                    offsetY = -((float)device.Viewport.Width / 2 - insetSize.Width / 2 - 10);
                    offsetZ = -((float)device.Viewport.Height / 2 - insetSize.Height / 2 - 10);
                    if (World.Settings.ShowToolbar) offsetZ += 50;
                    if (World.Settings.ShowPosition) offsetZ += 140;
                    break;
                case "Top-Center":
                    // up center
                    offsetY = 0;
                    offsetZ = -((float)device.Viewport.Height / 2 - insetSize.Height / 2 - 10);
                    if (World.Settings.ShowToolbar) offsetZ += 50;
                    break;
                case "Top-Left":
                    // upper left corner
                    offsetY = ((float)device.Viewport.Width / 2 - insetSize.Width / 2 - 6);
                    offsetZ = -((float)device.Viewport.Height / 2 - insetSize.Height / 2 - 4);
                    if (World.Settings.ShowToolbar) offsetZ += 44;
                    break;
            }

            // Save original View and change it to face globe
            // Note: the sprite is centered on origin, the camera view moves.
            Matrix origView = device.Transform.View;
            device.Transform.View = Matrix.LookAtRH(
                new Vector3((float)1e3, 0, 0),	// Cam pos
                new Vector3(0, 0, 0),			// Cam target
                new Vector3(0, 0, 1));			// Up vector

            // Offset, rotate and tilt
            Matrix origWorld = device.Transform.World;
            Matrix trans = Matrix.Translation(0, -(float)insetOffset.X, (float)insetOffset.Y);
            trans *= Matrix.RotationZ((float)camera.Longitude.Radians + (float)Math.PI);
            trans *= Matrix.RotationY(-(float)camera.Latitude.Radians);
            device.Transform.World = trans;

            // Save default viewport
            Viewport origVp = device.Viewport;
            // Set inset viewport
            Viewport viewPort = new Viewport();
            viewPort.Width = insetSize.Width;
            viewPort.Height = insetSize.Height;
            viewPort.X = device.Viewport.Width / 2 - (int)offsetY - viewPort.Width / 2;
            viewPort.Y = device.Viewport.Height / 2 + (int)offsetZ - viewPort.Height / 2;
            device.Viewport = viewPort;

            // Save original projection and change it to ortho
            // Note: using pixels as units produce a 1:1 projection of textures
            float topAlt = 4000e3f; float topFactor = 4f;
            float botAlt = 200e3f; float botFactor = 14f;
            float zoomFactor = botFactor + ((topFactor - botFactor) / (topAlt - botAlt) * ((float)camera.Altitude - botAlt));
            if (zoomFactor > botFactor) zoomFactor = botFactor;
            Matrix origProjection = device.Transform.Projection;
            device.Transform.Projection = Matrix.OrthoRH((float)device.Viewport.Width / zoomFactor, (float)device.Viewport.Height / zoomFactor, -(float)4e6, (float)4e6);

            // Render globe inset here
            if (texture != null && zoomFactor > topFactor)
            {
                device.RenderState.ZBufferEnable = false;
                // Create mesh
                if (spriteMesh == null) spriteMesh = TexturedSphere(device, (float)globeRadius, 24, 24);
                // set texture
                device.SetTexture(0, texture);
                //device.TextureState[0].ColorOperation = TextureOperation.BlendCurrentAlpha;
                //device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
                device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
                device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;

                // draw
                spriteMesh.DrawSubset(0);
            }

            // Restore device states
            device.Transform.World = origWorld;
            device.Transform.Projection = origProjection;
            device.Transform.View = origView;
            device.RenderState.ZBufferEnable = true;
            device.RenderState.FogEnable = origFog;
            device.Viewport = origVp;

            // Draw cross hairs
            if (texture != null && zoomFactor > topFactor)
            {
                DrawCrossHairs(drawArgs, (float)device.Viewport.Width / 2 - offsetY, (float)device.Viewport.Height / 2 + offsetZ);
                DrawRectangle(drawArgs, (float)viewPort.X, (float)viewPort.Y, (float)viewPort.Width, (float)viewPort.Height);
            }
        }


        // Cross hairs
        int crossHairColor = Color.GhostWhite.ToArgb();
        CustomVertex.TransformedColored[] vertical = new CustomVertex.TransformedColored[4];
        CustomVertex.TransformedColored[] horizontal = new CustomVertex.TransformedColored[4];
        public void DrawCrossHairs(DrawArgs drawArgs, float X, float Y)
        {
            Device device = drawArgs.device;

            int crossHairSize = 10;
            int crossHairGap = 3;

            horizontal[0].X = X - crossHairSize;
            horizontal[0].Y = Y;
            horizontal[0].Z = 0.5f;
            horizontal[0].Color = crossHairColor;

            horizontal[1].X = X - crossHairGap;
            horizontal[1].Y = Y;
            horizontal[1].Z = 0.5f;
            horizontal[1].Color = crossHairColor;

            horizontal[2].X = X + crossHairGap;
            horizontal[2].Y = Y;
            horizontal[2].Z = 0.5f;
            horizontal[2].Color = crossHairColor;

            horizontal[3].X = X + crossHairSize;
            horizontal[3].Y = Y;
            horizontal[3].Z = 0.5f;
            horizontal[3].Color = crossHairColor;

            vertical[0].X = X;
            vertical[0].Y = Y - crossHairSize;
            vertical[0].Z = 0.5f;
            vertical[0].Color = crossHairColor;

            vertical[1].X = X;
            vertical[1].Y = Y - crossHairGap;
            vertical[1].Z = 0.5f;
            vertical[1].Color = crossHairColor;

            vertical[2].X = X;
            vertical[2].Y = Y + crossHairGap;
            vertical[2].Z = 0.5f;
            vertical[2].Color = crossHairColor;

            vertical[3].X = X;
            vertical[3].Y = Y + crossHairSize;
            vertical[3].Z = 0.5f;
            vertical[3].Color = crossHairColor;

            drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
            drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
            drawArgs.device.DrawUserPrimitives(PrimitiveType.LineList, 4, horizontal);
            drawArgs.device.DrawUserPrimitives(PrimitiveType.LineList, 4, vertical);
        }

        // Rectangle
        int rectangleColor = Color.GhostWhite.ToArgb();
        CustomVertex.TransformedColored[] rectangleVertices = new CustomVertex.TransformedColored[5];
        public void DrawRectangle(DrawArgs drawArgs, float X, float Y, float Width, float Height)
        {
            Device device = drawArgs.device;

            rectangleVertices[0].X = X;
            rectangleVertices[0].Y = Y;
            rectangleVertices[0].Z = 0.5f;
            rectangleVertices[0].Color = rectangleColor;

            rectangleVertices[1].X = X + Width;
            rectangleVertices[1].Y = Y;
            rectangleVertices[1].Z = 0.5f;
            rectangleVertices[1].Color = rectangleColor;

            rectangleVertices[2].X = X + Width;
            rectangleVertices[2].Y = Y + Height;
            rectangleVertices[2].Z = 0.5f;
            rectangleVertices[2].Color = rectangleColor;

            rectangleVertices[3].X = X;
            rectangleVertices[3].Y = Y + Height;
            rectangleVertices[3].Z = 0.5f;
            rectangleVertices[3].Color = rectangleColor;

            rectangleVertices[4].X = X;
            rectangleVertices[4].Y = Y;
            rectangleVertices[4].Z = 0.5f;
            rectangleVertices[4].Color = rectangleColor;

            drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
            drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
            drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, rectangleVertices.Length - 1, rectangleVertices);
        }

        /// <summary>
        /// RenderableObject abstract member (needed) 
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
        public override void Initialize(DrawArgs drawArgs)
        {
            Device device = drawArgs.device;
            try
            {
                texture = TextureLoader.FromFile(device, Path.Combine(texturePath, textureFileName));
                isInitialized = true;
            }
            catch
            {
                isOn = false;
                MessageBox.Show("Error loading texture " + Path.Combine(texturePath, textureFileName) + ".", "Layer initialization failed.", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            // Init dimensions
            spriteSize = new Rectangle(0, 0, globeRadius * 2, globeRadius * 2);
            spriteOffset = new Point(0, 0);
            insetSize = new Rectangle(0, 0, insetWidth, insetHeight);
            insetOffset = new Point(0, 0);
        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
        public override void Update(DrawArgs drawArgs)
        {
            if (!isInitialized)
                Initialize(drawArgs);
        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
        public override void Dispose()
        {
            isInitialized = false;
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }
            if (spriteMesh != null)
            {
                spriteMesh.Dispose();
                spriteMesh = null;
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
        public override void BuildContextMenu(ContextMenu menu)
        {
            menu.MenuItems.Add("Properties", new System.EventHandler(OnPropertiesClick));
        }

        /// <summary>
        /// Properties context menu clicked.
        /// </summary>
        public new void OnPropertiesClick(object sender, EventArgs e)
        {
            if (pDialog != null && !pDialog.IsDisposed)
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
            private System.Windows.Forms.Label lblGlobe;
            private System.Windows.Forms.CheckBox chkGlobe;
            private System.Windows.Forms.Label lblPosition;
            private System.Windows.Forms.ComboBox cboPosition;
            private System.Windows.Forms.Label lblSize;
            private System.Windows.Forms.ComboBox cboSize;
            private System.Windows.Forms.Label lblInset;
            private System.Windows.Forms.CheckBox chkInset;
            private System.Windows.Forms.Label lblPosition2;
            private System.Windows.Forms.ComboBox cboPosition2;
            private System.Windows.Forms.Label lblSize2;
            private System.Windows.Forms.ComboBox cboSize2;
            private System.Windows.Forms.Button btnOK;
            private System.Windows.Forms.Button btnCancel;
            private GlobeIconLayer layer;

            public propertiesDialog(GlobeIconLayer layer)
            {
                InitializeComponent();
                this.layer = layer;
                //this.Icon = WorldWind.PluginEngine.Plugin.Icon;
                this.Text = layer.pluginName + " " + layer.version + " properties";
                // Init texture list with *.png
                DirectoryInfo di = new DirectoryInfo(layer.texturePath);
                //DirectoryInfo di = new DirectoryInfo(Path.Combine(WorldWindSettings.WorldWindDirectory, "Data/Earth/BmngBathy"));
                FileInfo[] imgFiles = di.GetFiles("*.jpg");
                cboTexture.Items.AddRange(imgFiles);
                imgFiles = di.GetFiles("*.png");
                cboTexture.Items.AddRange(imgFiles);
                // select current bitmap
                int i = cboTexture.FindString(layer.textureFileName);
                if (i != -1) cboTexture.SelectedIndex = i;
                // Show globe
                chkGlobe.Checked = layer.showGlobe;
                // Positions globe
                cboPosition.Items.Add("Top-Left");
                cboPosition.Items.Add("Top-Center");
                cboPosition.Items.Add("Top-Right");
                cboPosition.Items.Add("Bottom-Left");
                cboPosition.Items.Add("Bottom-Center");
                cboPosition.Items.Add("Bottom-Right");
                cboPosition.Items.Add("Screen-Center");
                i = cboPosition.FindString(layer.spritePos);
                if (i != -1) cboPosition.SelectedIndex = i;
                // Size globe
                cboSize.Items.Add("64x64");
                cboSize.Items.Add("80x80");
                cboSize.Items.Add("100x100");
                cboSize.Items.Add("128x128");
                i = cboSize.FindString((layer.globeRadius * 2).ToString() + "x" + (layer.globeRadius * 2).ToString());
                if (i != -1) cboSize.SelectedIndex = i;
                // Show inset
                chkInset.Checked = layer.showInset;
                // Positions inset
                cboPosition2.Items.Add("Top-Left");
                cboPosition2.Items.Add("Top-Center");
                cboPosition2.Items.Add("Top-Right");
                cboPosition2.Items.Add("Bottom-Left");
                cboPosition2.Items.Add("Bottom-Center");
                cboPosition2.Items.Add("Bottom-Right");
                cboPosition2.Items.Add("Screen-Center");
                i = cboPosition2.FindString(layer.insetPos);
                if (i != -1) cboPosition2.SelectedIndex = i;
                // Size inset
                cboSize2.Items.Add("64x64");
                cboSize2.Items.Add("100x64");
                cboSize2.Items.Add("64x100");
                cboSize2.Items.Add("80x80");
                cboSize2.Items.Add("110x80");
                cboSize2.Items.Add("80x110");
                cboSize2.Items.Add("100x100");
                cboSize2.Items.Add("133x100");
                cboSize2.Items.Add("100x133");
                cboSize2.Items.Add("128x128");
                cboSize2.Items.Add("160x128");
                cboSize2.Items.Add("128x160");
                i = cboSize2.FindString(layer.insetWidth.ToString() + "x" + layer.insetHeight.ToString());
                if (i != -1) cboSize2.SelectedIndex = i;
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
                this.lblGlobe = new System.Windows.Forms.Label();
                this.chkGlobe = new System.Windows.Forms.CheckBox();
                this.lblPosition = new System.Windows.Forms.Label();
                this.cboPosition = new System.Windows.Forms.ComboBox();
                this.lblSize = new System.Windows.Forms.Label();
                this.cboSize = new System.Windows.Forms.ComboBox();
                this.lblInset = new System.Windows.Forms.Label();
                this.chkInset = new System.Windows.Forms.CheckBox();
                this.lblPosition2 = new System.Windows.Forms.Label();
                this.cboPosition2 = new System.Windows.Forms.ComboBox();
                this.lblSize2 = new System.Windows.Forms.Label();
                this.cboSize2 = new System.Windows.Forms.ComboBox();
                this.SuspendLayout();
                // 
                // btnCancel
                // 
                this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.btnCancel.Location = new System.Drawing.Point(311, 199);
                this.btnCancel.Name = "btnCancel";
                this.btnCancel.TabIndex = 0;
                this.btnCancel.Text = "Cancel";
                this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
                // 
                // btnOK
                // 
                this.btnOK.Location = new System.Drawing.Point(224, 199);
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
                this.lblTexture.Text = "Texture :";
                // 
                // cboTexture
                // 
                this.cboTexture.Location = new System.Drawing.Point(96, 25);
                this.cboTexture.Name = "cboTexture";
                this.cboTexture.Size = new System.Drawing.Size(296, 21);
                this.cboTexture.TabIndex = 3;
                this.cboTexture.Text = "Select image file";
                this.cboTexture.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cboTexture.MaxDropDownItems = 10;
                // 
                // lblGlobe
                // 
                this.lblGlobe.AutoSize = true;
                this.lblGlobe.Location = new System.Drawing.Point(16, 59);
                this.lblGlobe.Name = "lblGlobe";
                this.lblGlobe.Size = new System.Drawing.Size(82, 16);
                this.lblGlobe.TabIndex = 4;
                this.lblGlobe.Text = "Globe :";
                // 
                // chkGlobe
                // 
                this.chkGlobe.Location = new System.Drawing.Point(96, 55);
                this.chkGlobe.Name = "chkGlobe";
                this.chkGlobe.TabIndex = 5;
                this.chkGlobe.Text = "Display";
                // 
                // lblPosition
                // 
                this.lblPosition.AutoSize = true;
                this.lblPosition.Location = new System.Drawing.Point(220, 59);
                this.lblPosition.Name = "lblPosition";
                this.lblPosition.Size = new System.Drawing.Size(82, 16);
                this.lblPosition.TabIndex = 6;
                this.lblPosition.Text = "Placement :";
                // 
                // cboPosition
                // 
                this.cboPosition.Location = new System.Drawing.Point(292, 56);
                this.cboPosition.Name = "cboPosition";
                this.cboPosition.Size = new System.Drawing.Size(100, 21);
                this.cboPosition.TabIndex = 7;
                this.cboPosition.Text = "Select placement";
                this.cboPosition.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cboPosition.MaxDropDownItems = 10;
                // 
                // lblSize
                // 
                this.lblSize.AutoSize = true;
                this.lblSize.Location = new System.Drawing.Point(220, 90);
                this.lblSize.Name = "lblSize";
                this.lblSize.Size = new System.Drawing.Size(82, 16);
                this.lblSize.TabIndex = 8;
                this.lblSize.Text = "Size :";
                // 
                // cboSize
                // 
                this.cboSize.Location = new System.Drawing.Point(292, 87);
                this.cboSize.Name = "cboSize";
                this.cboSize.Size = new System.Drawing.Size(100, 21);
                this.cboSize.TabIndex = 9;
                this.cboSize.Text = "Select size";
                this.cboSize.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cboSize.MaxDropDownItems = 10;
                // 
                // lblInset
                // 
                this.lblInset.AutoSize = true;
                this.lblInset.Location = new System.Drawing.Point(16, 121);
                this.lblInset.Name = "lblInset";
                this.lblInset.Size = new System.Drawing.Size(82, 16);
                this.lblInset.TabIndex = 10;
                this.lblInset.Text = "Top view :";
                // 
                // chkInset
                // 
                this.chkInset.Location = new System.Drawing.Point(96, 117);
                this.chkInset.Name = "chkInset";
                this.chkInset.TabIndex = 11;
                this.chkInset.Text = "Display";
                // 
                // lblPosition2
                // 
                this.lblPosition2.AutoSize = true;
                this.lblPosition2.Location = new System.Drawing.Point(220, 121);
                this.lblPosition2.Name = "lblPosition2";
                this.lblPosition2.Size = new System.Drawing.Size(82, 16);
                this.lblPosition2.TabIndex = 12;
                this.lblPosition2.Text = "Placement :";
                // 
                // cboPosition2
                // 
                this.cboPosition2.Location = new System.Drawing.Point(292, 118);
                this.cboPosition2.Name = "cboPosition2";
                this.cboPosition2.Size = new System.Drawing.Size(100, 21);
                this.cboPosition2.TabIndex = 13;
                this.cboPosition2.Text = "Select placement";
                this.cboPosition2.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cboPosition2.MaxDropDownItems = 10;
                // 
                // lblSize2
                // 
                this.lblSize2.AutoSize = true;
                this.lblSize2.Location = new System.Drawing.Point(220, 152);
                this.lblSize2.Name = "lblSize2";
                this.lblSize2.Size = new System.Drawing.Size(82, 16);
                this.lblSize2.TabIndex = 14;
                this.lblSize2.Text = "Size :";
                // 
                // cboSize2
                // 
                this.cboSize2.Location = new System.Drawing.Point(292, 149);
                this.cboSize2.Name = "cboSize2";
                this.cboSize2.Size = new System.Drawing.Size(100, 21);
                this.cboSize2.TabIndex = 15;
                this.cboSize2.Text = "Select size";
                this.cboSize2.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cboSize2.MaxDropDownItems = 10;
                // 
                // frmFavorites
                // 
                this.AcceptButton = this.btnOK;
                this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
                this.CancelButton = this.btnCancel;
                this.ClientSize = new System.Drawing.Size(406, 236);
                this.ControlBox = false;
                this.Controls.Add(this.cboTexture);
                this.Controls.Add(this.lblTexture);
                this.Controls.Add(this.lblGlobe);
                this.Controls.Add(this.chkGlobe);
                this.Controls.Add(this.cboPosition);
                this.Controls.Add(this.lblPosition);
                this.Controls.Add(this.cboSize);
                this.Controls.Add(this.lblSize);
                this.Controls.Add(this.lblInset);
                this.Controls.Add(this.chkInset);
                this.Controls.Add(this.cboPosition2);
                this.Controls.Add(this.lblPosition2);
                this.Controls.Add(this.cboSize2);
                this.Controls.Add(this.lblSize2);
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
                this.Text = "Layer properties";
                this.TopMost = true;
                this.ResumeLayout(false);

            }
            #endregion

            private void btnOK_Click(object sender, System.EventArgs e)
            {
                if (cboTexture.SelectedItem != null)
                {
                    string[] size;
                    //System.Windows.Forms.MessageBox.Show("Texture : " + cboTexture.SelectedItem.ToString());
                    layer.Dispose();
                    layer.textureFileName = cboTexture.SelectedItem.ToString();
                    layer.showGlobe = chkGlobe.Checked;
                    layer.spritePos = cboPosition.SelectedItem.ToString();
                    size = cboSize.SelectedItem.ToString().Split('x');
                    layer.globeRadius = int.Parse(size[0]) / 2;
                    layer.showInset = chkInset.Checked;
                    layer.insetPos = cboPosition2.SelectedItem.ToString();
                    size = cboSize2.SelectedItem.ToString().Split('x');
                    layer.insetWidth = int.Parse(size[0]);
                    layer.insetHeight = int.Parse(size[1]);
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
            int numVertices = (slices + 1) * (stacks + 1);
            int numFaces = slices * stacks * 2;
            int indexCount = numFaces * 3;

            Mesh mesh = new Mesh(numFaces, numVertices, MeshFlags.Managed, CustomVertex.PositionNormalTextured.Format, device);

            // Get the original sphere's vertex buffer.
            int[] ranks = new int[1];
            ranks[0] = mesh.NumberVertices;
            System.Array arr = mesh.VertexBuffer.Lock(0, typeof(CustomVertex.PositionNormalTextured), LockFlags.None, ranks);

            // Set the vertex buffer
            int vertIndex = 0;
            for (int stack = 0; stack <= stacks; stack++)
            {
                double latitude = -90 + ((float)stack / stacks * (float)180.0);
                for (int slice = 0; slice <= slices; slice++)
                {
                    CustomVertex.PositionNormalTextured pnt = new CustomVertex.PositionNormalTextured();
                    double longitude = 180 - ((float)slice / slices * (float)360);
                    Vector3 v = MathEngine.SphericalToCartesian(latitude, longitude, radius);
                    pnt.X = v.X;
                    pnt.Y = v.Y;
                    pnt.Z = v.Z;
                    pnt.Tu = (float)slice / slices;
                    pnt.Tv = 1.0f - (float)stack / stacks;
                    arr.SetValue(pnt, vertIndex++);
                }
            }

            mesh.VertexBuffer.Unlock();
            ranks[0] = indexCount;
            arr = mesh.LockIndexBuffer(typeof(short), LockFlags.None, ranks);
            int i = 0;
            short bottomVertex = 0;
            short topVertex = 0;
            for (short x = 0; x < stacks; x++)
            {
                bottomVertex = (short)((slices + 1) * x);
                topVertex = (short)(bottomVertex + slices + 1);
                for (int y = 0; y < slices; y++)
                {
                    arr.SetValue(bottomVertex, i++);
                    arr.SetValue((short)(topVertex + 1), i++);
                    arr.SetValue(topVertex, i++);
                    arr.SetValue(bottomVertex, i++);
                    arr.SetValue((short)(bottomVertex + 1), i++);
                    arr.SetValue((short)(topVertex + 1), i++);
                    bottomVertex++;
                    topVertex++;
                }
            }
            mesh.IndexBuffer.SetData(arr, 0, LockFlags.None);
            mesh.ComputeNormals();

            return mesh;
        }


        #endregion
    }
}
