using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NASA.Plugins
{
    /// <summary>
    /// Summary description for BMNG.
    /// </summary>
    public class BMNG : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ComboBox comboBoxBmngVersion;
        private System.Windows.Forms.TrackBar trackBarMonth;
        private System.Windows.Forms.StatusBar statusBarMonth;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItemHelp;
        private System.Windows.Forms.MenuItem menuItemAbout;

        WorldWind.Renderable.RenderableObjectList m_BlueMarbleList = null;
        WorldWind.Renderable.RenderableObjectList m_ShadedList = null;
        WorldWind.Renderable.RenderableObjectList m_ShadedBathyList = null;

        WorldWind.Renderable.ImageLayer m_BlueMarbleBase = null;
        WorldWind.Renderable.QuadTileSet m_BlueMarbleTiled = null;
        WorldWind.Renderable.RenderableObjectList m_RenderableList = new WorldWind.Renderable.RenderableObjectList("The Blue Marble");

        WorldWind.Renderable.RenderableObjectList[,] m_RenderableLayers = new WorldWind.Renderable.RenderableObjectList[3, 12];
        WorldWind.Renderable.ImageLayer[,] m_ImageLayers = new WorldWind.Renderable.ImageLayer[3, 12];
        WorldWind.Renderable.QuadTileSet[,] m_QuadTileLayers = new WorldWind.Renderable.QuadTileSet[3, 12];
        public WorldWind.WorldWindow m_WorldWindow = null;
        public MenuItem m_MenuItem;

        private Timer timer;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        string m_BmngBaseImageUrl = "http://worldwind28.arc.nasa.gov/public/";

        public BMNG(WorldWind.WorldWindow worldWindow, MenuItem menuItem)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.m_MenuItem = menuItem; // Plugin menu item ref

            int lastSelectedDatasetIndex = 1;
            try
            {
                using (StreamReader reader = new StreamReader(Path.GetDirectoryName(Application.ExecutablePath) + "\\Plugins\\BlueMarble\\settings.txt"))
                {
                    lastSelectedDatasetIndex = int.Parse(reader.ReadLine().Trim());

                }
            }
            catch
            {
            }

            comboBoxBmngVersion.SelectedIndex = lastSelectedDatasetIndex;

            m_WorldWindow = worldWindow;
            m_RenderableList.ShowOnlyOneLayer = true;
            bool foundImagesObject = false;
            lock (m_WorldWindow.CurrentWorld.RenderableObjects.ChildObjects.SyncRoot)
            {
                foreach (WorldWind.Renderable.RenderableObject ro in m_WorldWindow.CurrentWorld.RenderableObjects.ChildObjects)
                {
                    if (ro is WorldWind.Renderable.RenderableObjectList && (ro.Name == "Images"))	// SF FIX: don't add to layers called 'xxxx images'!
                    {
                        WorldWind.Renderable.RenderableObjectList imagesList = ro as WorldWind.Renderable.RenderableObjectList;
                        imagesList.ChildObjects.Insert(0, m_RenderableList);
                        foundImagesObject = true;
                        break;
                    }
                }
            }

            if (!foundImagesObject)
            {
                m_WorldWindow.CurrentWorld.RenderableObjects.ChildObjects.Add(m_RenderableList);
            }

            m_BlueMarbleBase = new WorldWind.Renderable.ImageLayer(
                "Blue Marble Base Image",
                m_WorldWindow.CurrentWorld,
                0,
                null,
                -90, 90, -180, 180, 1.0f, null);
            m_BlueMarbleBase.ImageUrl = "http://worldwind.arc.nasa.gov/downloads/land_shallow_topo_2048.dds";

            WorldWind.NltImageStore ia = new WorldWind.NltImageStore("106", "http://nww.terraserver-usa.com/nwwtile.ashx");
            ia.DataDirectory = null;
            ia.LevelZeroTileSizeDegrees = 36.0;
            ia.LevelCount = 4;
            ia.ImageExtension = "jpg";
            ia.CacheDirectory = String.Format("{0}\\Blue Marble", m_WorldWindow.Cache.CacheDirectory);

            WorldWind.ImageStore[] ias = new WorldWind.ImageStore[1];
            ias[0] = ia;
            m_BlueMarbleTiled = new WorldWind.Renderable.QuadTileSet(
                "Blue Marble Tiled",
                m_WorldWindow.CurrentWorld,
                0,
                90, -90, -180, 180,
                true,
                ias);

            m_BlueMarbleTiled.ServerLogoFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\meatball.png";

            m_BlueMarbleList = new WorldWind.Renderable.RenderableObjectList("Blue Marble");
            m_BlueMarbleList.IsOn = false;
            m_BlueMarbleList.Add(m_BlueMarbleBase);
            m_BlueMarbleList.Add(m_BlueMarbleTiled);

            m_ShadedList = new WorldWind.Renderable.RenderableObjectList("BMNG");
            m_ShadedList.ShowOnlyOneLayer = true;
            m_ShadedList.IsOn = false;

            for (int i = 0; i < 12; i++)
            {
                m_ImageLayers[0, i] = new WorldWind.Renderable.ImageLayer(
                    String.Format("Base Image - {0}.2004", i + 1),
                    m_WorldWindow.CurrentWorld,
                    0,
                    null,
                    -90, 90, -180, 180, 1.0f, null);
                m_ImageLayers[0, i].ImageUrl = String.Format("{0}world.topo.2004{1:D2}.jpg", m_BmngBaseImageUrl, i + 1);

                WorldWind.NltImageStore imageStore = new WorldWind.NltImageStore(String.Format("bmng.topo.2004{0:D2}", i + 1), "http://worldwind25.arc.nasa.gov/tile/tile.aspx");
                imageStore.DataDirectory = null;
                imageStore.LevelZeroTileSizeDegrees = 36.0;
                imageStore.LevelCount = 5;
                imageStore.ImageExtension = "jpg";
                imageStore.CacheDirectory = String.Format("{0}\\BMNG\\{1}", m_WorldWindow.Cache.CacheDirectory, String.Format("BMNG (Shaded) Tiled - {0}.2004", i + 1));

                ias = new WorldWind.ImageStore[1];
                ias[0] = imageStore;

                m_QuadTileLayers[0, i] = new WorldWind.Renderable.QuadTileSet(
                    String.Format("Tiled - {0}.2004", i + 1),
                    m_WorldWindow.CurrentWorld,
                    0,
                    90, -90, -180, 180,
                    true,
                    ias);

                m_QuadTileLayers[0, i].ServerLogoFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\meatball.png";

                m_RenderableLayers[0, i] = new WorldWind.Renderable.RenderableObjectList(String.Format("{0}.2004", i + 1));
                m_RenderableLayers[0, i].IsOn = false;

                m_RenderableLayers[0, i].Add(m_ImageLayers[0, i]);
                m_RenderableLayers[0, i].Add(m_QuadTileLayers[0, i]);
                m_ShadedList.Add(m_RenderableLayers[0, i]);
            }

            m_ShadedBathyList = new WorldWind.Renderable.RenderableObjectList("BMNG (Bathymetry)");
            m_ShadedBathyList.ShowOnlyOneLayer = true;
            m_ShadedBathyList.IsOn = false;

            for (int i = 0; i < 12; i++)
            {
                m_ImageLayers[1, i] = new WorldWind.Renderable.ImageLayer(
                    String.Format("Base Image - {0}.2004", i + 1),
                    m_WorldWindow.CurrentWorld,
                    0,
                    String.Format("{0}\\Data\\Earth\\BmngBathy\\world.topo.bathy.2004{1:D2}.jpg", Path.GetDirectoryName(Application.ExecutablePath), i + 1),
                    -90, 90, -180, 180, 1.0f, null);

                //	m_ImageLayers[1, i].ImageUrl = String.Format("{0}world.topo.bathy.2004{1:D2}.jpg", m_BmngBaseImageUrl, i+1);

                WorldWind.NltImageStore imageStore = new WorldWind.NltImageStore(String.Format("bmng.topo.bathy.2004{0:D2}", i + 1), "http://worldwind25.arc.nasa.gov/tile/tile.aspx");
                imageStore.DataDirectory = null;
                imageStore.LevelZeroTileSizeDegrees = 36.0;
                imageStore.LevelCount = 5;
                imageStore.ImageExtension = "jpg";
                imageStore.CacheDirectory = String.Format("{0}\\BMNG\\{1}", m_WorldWindow.Cache.CacheDirectory, String.Format("BMNG (Shaded + Bathymetry) Tiled - {0}.2004", i + 1));

                ias = new WorldWind.ImageStore[1];
                ias[0] = imageStore;

                m_QuadTileLayers[1, i] = new WorldWind.Renderable.QuadTileSet(
                        String.Format("Tiled - {0}.2004", i + 1),
                        m_WorldWindow.CurrentWorld,
                        0,
                        90, -90, -180, 180, true, ias);

                m_QuadTileLayers[0, i].ServerLogoFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\meatball.png";

                m_RenderableLayers[1, i] = new WorldWind.Renderable.RenderableObjectList(String.Format("{0}.2004", i + 1));
                m_RenderableLayers[1, i].IsOn = false;

                m_RenderableLayers[1, i].Add(m_ImageLayers[1, i]);
                m_RenderableLayers[1, i].Add(m_QuadTileLayers[1, i]);
                m_ShadedBathyList.Add(m_RenderableLayers[1, i]);
            }

            /*	m_UnShadedList = new WorldWind.Renderable.RenderableObjectList("BMNG (UnShaded)");
                m_UnShadedList.ShowOnlyOneLayer = true;
                m_UnShadedList.IsOn = false;
			
                for(int i = 0; i < 12; i++)
                {
                    m_ImageLayers[2, i] = new WorldWind.Renderable.ImageLayer(
                        String.Format("Base Image - {0}.2004 un", i+1),
                        m_WorldWindow.CurrentWorld,
                        0,
                        null,
                        -90, 90, -180, 180, 1.0f, m_WorldWindow.CurrentWorld.TerrainAccessor);

                    m_ImageLayers[2, i].ImageUrl = String.Format("{0}world.2004{1:D2}.jpg", m_BmngBaseImageUrl, i+1);
                    m_ImageLayers[2, i].IsOn = false;

                    m_QuadTileLayers[2, i] = new WorldWind.Renderable.QuadTileSet(
                            String.Format("Tiled - {0}.2004", i+1),
                            m_WorldWindow.CurrentWorld,
                            0,
                            90, -90, -180, 180, m_WorldWindow.CurrentWorld.TerrainAccessor,
                            new WorldWind.ImageAccessor(
                            null, 512, 36.0, 5, "jpg",
                            String.Format("{0}\\BMNG\\{1}", m_WorldWindow.Cache.CacheDirectory, String.Format("BMNG (UnShaded) Tiled - {0}.2004", i+1)),
                            new WorldWind.ImageTileService(
                            String.Format("bmng.2004{0:D2}", i+1), "http://worldwind28.arc.nasa.gov/TestWebApp/WebForm1.aspx", 
                            Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\meatball.png")
                            ));

                    m_RenderableLayers[2, i] = new WorldWind.Renderable.RenderableObjectList(String.Format("{0}.2004", i+1));
                    m_RenderableLayers[2, i].IsOn = false;

                    m_RenderableLayers[2, i].Add(m_ImageLayers[1, i]);
                    m_RenderableLayers[2, i].Add(m_QuadTileLayers[1, i]);
                    m_UnShadedList.Add(m_RenderableLayers[2, i]);
                }*/

            m_RenderableList.Add(m_BlueMarbleList);
            m_RenderableList.Add(m_ShadedList);
            m_RenderableList.Add(m_ShadedBathyList);
            //	m_RenderableList.Add(m_UnShadedList);

            this.trackBarMonth.Value = System.DateTime.Now.Month - 1;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
            this.m_MenuItem.Checked = false;
            base.OnClosing(e);
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxBmngVersion = new System.Windows.Forms.ComboBox();
            this.trackBarMonth = new System.Windows.Forms.TrackBar();
            this.statusBarMonth = new System.Windows.Forms.StatusBar();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItemHelp = new System.Windows.Forms.MenuItem();
            this.menuItemAbout = new System.Windows.Forms.MenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMonth)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxBmngVersion
            // 
            this.comboBoxBmngVersion.Items.AddRange(new object[] {
																	 "BMNG",
																	 "BMNG (Bathymetry)",
																	 "Blue Marble (Original)"});
            this.comboBoxBmngVersion.Location = new System.Drawing.Point(8, 8);
            this.comboBoxBmngVersion.Name = "comboBoxBmngVersion";
            this.comboBoxBmngVersion.Size = new System.Drawing.Size(344, 21);
            this.comboBoxBmngVersion.TabIndex = 0;
            this.comboBoxBmngVersion.SelectedIndexChanged += new System.EventHandler(this.comboBoxBmngVersion_SelectedIndexChanged);
            // 
            // trackBarMonth
            // 
            this.trackBarMonth.Location = new System.Drawing.Point(8, 32);
            this.trackBarMonth.Maximum = 11;
            this.trackBarMonth.Name = "trackBarMonth";
            this.trackBarMonth.Size = new System.Drawing.Size(352, 45);
            this.trackBarMonth.TabIndex = 1;
            this.trackBarMonth.Value = 1;
            this.trackBarMonth.Scroll += new System.EventHandler(this.trackBarMonth_Scroll);
            // 
            // statusBarMonth
            // 
            this.statusBarMonth.Location = new System.Drawing.Point(0, 101);
            this.statusBarMonth.Name = "statusBarMonth";
            this.statusBarMonth.Size = new System.Drawing.Size(362, 22);
            this.statusBarMonth.SizingGrip = false;
            this.statusBarMonth.TabIndex = 2;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItemHelp});
            // 
            // menuItemHelp
            // 
            this.menuItemHelp.Index = 0;
            this.menuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemAbout});
            this.menuItemHelp.Text = "Help";
            // 
            // menuItemAbout
            // 
            this.menuItemAbout.Index = 0;
            this.menuItemAbout.Text = "About";
            this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "Jan";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(40, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 23);
            this.label3.TabIndex = 5;
            this.label3.Text = "Feb";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(64, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "Mar";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(96, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 23);
            this.label5.TabIndex = 7;
            this.label5.Text = "Apr";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(128, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 23);
            this.label6.TabIndex = 8;
            this.label6.Text = "May";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(160, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 23);
            this.label7.TabIndex = 9;
            this.label7.Text = "Jun";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(192, 80);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(24, 23);
            this.label8.TabIndex = 10;
            this.label8.Text = "Jul";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(216, 80);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 23);
            this.label9.TabIndex = 11;
            this.label9.Text = "Aug";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(248, 80);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 23);
            this.label10.TabIndex = 12;
            this.label10.Text = "Sep";
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(280, 80);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 23);
            this.label12.TabIndex = 13;
            this.label12.Text = "Oct";
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(304, 80);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(24, 23);
            this.label13.TabIndex = 14;
            this.label13.Text = "Nov";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(336, 80);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(24, 23);
            this.label14.TabIndex = 15;
            this.label14.Text = "Dec";
            // 
            // BMNG
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(362, 123);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.statusBarMonth);
            this.Controls.Add(this.trackBarMonth);
            this.Controls.Add(this.comboBoxBmngVersion);
            this.Controls.Add(this.label12);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Menu = this.mainMenu1;
            this.Name = "BMNG";
            this.Text = "Blue Marble Next-Generation Plugin v1.0";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMonth)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        BmngAboutDialog m_BmngAboutDialog = null;
        private void menuItemAbout_Click(object sender, System.EventArgs e)
        {
            if (m_BmngAboutDialog == null)
            {
                m_BmngAboutDialog = new BmngAboutDialog();
                m_BmngAboutDialog.Owner = this;
            }

            m_BmngAboutDialog.ShowDialog();


        }

        private void trackBarMonth_Scroll(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch
            { }
        }

        private string GetMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                default:
                    return "December";
            }
        }

        private void comboBoxBmngVersion_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + "\\Plugins\\BlueMarble\\settings.txt"))
            {
                sw.WriteLine(comboBoxBmngVersion.SelectedIndex);
            }
        }

        int m_CurrentMonth = -1;
        int m_CurrentVersion = -1;

        private void TurnOffLayers()
        {
            if (m_ImageLayers == null || m_QuadTileLayers == null)
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (m_RenderableLayers[i, j].IsOn)
                    {
                        m_RenderableLayers[i, j].IsOn = false;
                    }
                }
            }
        }

        private void setMonthLabelsAndSlider(bool enabled)
        {
            this.trackBarMonth.Visible = enabled;
            label2.Visible = enabled;
            label3.Visible = enabled;
            label4.Visible = enabled;
            label5.Visible = enabled;
            label6.Visible = enabled;
            label7.Visible = enabled;
            label8.Visible = enabled;
            label9.Visible = enabled;
            label10.Visible = enabled;
            label12.Visible = enabled;
            label13.Visible = enabled;
            label14.Visible = enabled;

            if (enabled)
            {
                this.Height = 168;
            }
            else
            {
                this.Height = 120;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                //Added TimeKeeper based Modis Month
                //if (WorldWind.TimeKeeper.CurrentTimeUtc.Month != trackBarMonth.Value)
                //    trackBarMonth.Value = WorldWind.TimeKeeper.CurrentTimeUtc.Month-1;


                if (m_CurrentMonth != trackBarMonth.Value)
                {
                    TurnOffLayers();

                    for (int i = 0; i < 2; i++)
                    {
                        m_RenderableLayers[i, trackBarMonth.Value].IsOn = true;
                    }

                    m_CurrentMonth = trackBarMonth.Value;
                    this.statusBarMonth.Text = GetMonth(trackBarMonth.Value + 1) + " - 2004";
                }

                if (m_CurrentVersion != comboBoxBmngVersion.SelectedIndex)
                {
                    m_CurrentVersion = comboBoxBmngVersion.SelectedIndex;

                    if (m_BlueMarbleList.IsOn)
                        m_BlueMarbleList.IsOn = false;

                    if (m_ShadedList.IsOn)
                        m_ShadedList.IsOn = false;

                    if (m_ShadedBathyList.IsOn)
                        m_ShadedBathyList.IsOn = false;


                    if (comboBoxBmngVersion.SelectedIndex == 2)//3)
                    {
                        // show blue marble (original)
                        m_BlueMarbleList.IsOn = true;
                        this.statusBarMonth.Text = "";
                        setMonthLabelsAndSlider(false);
                    }
                    else
                    {
                        setMonthLabelsAndSlider(true);
                        switch (comboBoxBmngVersion.SelectedIndex)
                        {
                            case 0:
                                m_ShadedList.IsOn = true;
                                break;
                            case 1:
                                m_ShadedBathyList.IsOn = true;
                                break;
                            //		case 2:
                            //			m_UnShadedList.IsOn = true;
                            //			break;
                            default:
                                break;

                        }

                    }

                }
            }
            catch
            { }
        }
    }

    public class BmngLoader : WorldWind.PluginEngine.Plugin
    {
        BMNG m_BmngForm = null;
        MenuItem m_MenuItem;
        WorldWind.WindowsControlMenuButton m_ToolbarItem;

        public override void Load()
        {
            if (ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("Earth") >= 0)
            {
                m_MenuItem = new MenuItem("Blue Marble");
                m_MenuItem.Click += new EventHandler(menuItemClicked);
                ParentApplication.PluginsMenu.MenuItems.Add(m_MenuItem);

                m_BmngForm = new BMNG(ParentApplication.WorldWindow, m_MenuItem);
                m_BmngForm.Owner = ParentApplication;

                m_ToolbarItem = new WorldWind.WindowsControlMenuButton(
                    "NASA Blue Marble",
                    Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Data\\Icons\\Interface\\bmng.png",
                    m_BmngForm);

                ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_ToolbarItem);

                base.Load();
            }
        }

        public override void Unload()
        {
            if (m_BmngForm != null)
            {
                m_BmngForm.Dispose();
                m_BmngForm = null;
                ParentApplication.PluginsMenu.MenuItems.Remove(m_MenuItem);
            }

            base.Unload();
        }

        private void menuItemClicked(object sender, System.EventArgs e)
        {
            if (m_BmngForm.Visible)
            {
                m_BmngForm.Visible = false;
                m_MenuItem.Checked = false;
            }
            else
            {
                m_BmngForm.Visible = true;
                m_MenuItem.Checked = true;
            }
        }
    }
}