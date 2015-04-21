using System;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.ComponentModel;
using System.Windows.Forms;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.Net;
using Utility;

namespace NASA.Plugins
{
	/// <summary>
	/// Summary description for NRLMontereyGlobal.
	/// </summary>
	public class NRLMontereyGlobal : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.StatusBar statusBar1;
		WorldWindow m_WorldWindow = null;
		ImageLayer imageLayer = null;
		private System.Windows.Forms.ListBox listBox1;

		System.Collections.ArrayList currentImageList = new ArrayList();
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.TrackBar trackBar2;
		string m_CacheDirectory = null;
		int currentFrame = 0;
		bool isUpdating = false;
		NRLDataSet currentAnimatingDataSet = null;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItemHelp;
		private System.Windows.Forms.MenuItem menuItemAbout;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label opacityLabel;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label2;
		NRLDataSet curSelectedDataSet = null;

		public class NRLDataSet
		{
			#region Private Members
			string _name;
			string _urlDirectory;
			string _description;
			double _north;
			double _south;
			double _east;
			double _west;
			string _keywordFilter;
			#endregion

			#region Public Methods
			public NRLDataSet(
				string name,
				string urlDirectory,
				string description,
				double north,
				double south,
				double east,
				double west,
				string keywordFilter)
			{
				this._name = name;
				this._urlDirectory = urlDirectory;
				this._description = description;
				this._north = north;
				this._south = south;
				this._west = west;
				this._east = east;
				this._keywordFilter = keywordFilter;
			}
			#endregion

			#region Properties
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
			public string UrlDirectory
			{
				get
				{
					return this._urlDirectory;
				}
				set
				{
					this._urlDirectory = value;
				}
			}
			public string Description
			{
				get
				{
					return this._description;
				}
				set
				{
					this._description = value;
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
			public string KeywordFilter
			{
				get
				{
					return this._keywordFilter;
				}
				set
				{
					this._keywordFilter = value;
				}
			}
			#endregion
		}

		public NRLMontereyGlobal(WorldWindow ww, string cacheDirectory, string xmlFile)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_WorldWindow = ww;

			loadNrlDataSets(xmlFile, this.treeView1);

			this.m_CacheDirectory = cacheDirectory;
			if(!Directory.Exists(this.m_CacheDirectory))
				Directory.CreateDirectory(this.m_CacheDirectory);
			
			this.animationTimer.Elapsed +=new System.Timers.ElapsedEventHandler(animationTimer_Elapsed);
		}

		static string getInnerTextFromFirstChild(XPathNodeIterator iter)
		{
			if(iter.Count == 0)
			{
				return null;
			}
			else
			{
				iter.MoveNext();
				return iter.Current.Value;
			}
		}

		private TreeNode getTreeNodeFromNrlDataSet(XPathNodeIterator iter)
		{
			TreeNode newNode = new TreeNode(iter.Current.GetAttribute("Name", ""));
			XPathNodeIterator childDataSets = iter.Current.Select("ChildDataSets");
			if(childDataSets.Count > 0)
			{
				while(childDataSets.MoveNext())
				{
					XPathNodeIterator childIter = childDataSets.Current.Select("NRL_DataSet");
					while(childIter.MoveNext())
					{
						newNode.Nodes.Add(getTreeNodeFromNrlDataSet(childIter));
					}
				}
			}
			else
			{
				string directoryUrl = getInnerTextFromFirstChild(iter.Current.Select("Directory_Url"));
				string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
				string keywordFilter = getInnerTextFromFirstChild(iter.Current.Select("Directory_Keyword_Filter"));
				XPathNodeIterator geographicBoundingBoxIter = iter.Current.Select("GeographicBoundingBox");
				double north = 0;
				double south = 0;
				double west = 0;
				double east = 0;

				if(geographicBoundingBoxIter.Count > 0)
				{
					geographicBoundingBoxIter.MoveNext();
					north = double.Parse(getInnerTextFromFirstChild(geographicBoundingBoxIter.Current.Select("North/Value")), System.Globalization.CultureInfo.InvariantCulture);
                    south = double.Parse(getInnerTextFromFirstChild(geographicBoundingBoxIter.Current.Select("South/Value")), System.Globalization.CultureInfo.InvariantCulture);
                    east = double.Parse(getInnerTextFromFirstChild(geographicBoundingBoxIter.Current.Select("East/Value")), System.Globalization.CultureInfo.InvariantCulture);
                    west = double.Parse(getInnerTextFromFirstChild(geographicBoundingBoxIter.Current.Select("West/Value")), System.Globalization.CultureInfo.InvariantCulture);

					NRLDataSet newDataSet = new NRLDataSet(
						newNode.Text,
						directoryUrl,
						description,
						north,
						south,
						east,
						west,
						keywordFilter);
					
					newNode.Tag = newDataSet;
				}
			}

			return newNode;
		}

		private void loadNrlDataSets(string filepath, TreeView treeView)
		{
			try
			{				
				XPathNavigator nav;
				XPathDocument docNav;
				XPathNodeIterator NodeIter;
			
				// Open the XML.
				docNav = new XPathDocument(filepath);

				// Create a navigator to query with XPath.
				nav = docNav.CreateNavigator();

				NodeIter = nav.Select("/NRL_DataSet_List/NRL_DataSet");

				while(NodeIter.MoveNext())
				{
					treeView.Nodes.Add( getTreeNodeFromNrlDataSet(NodeIter));
				}				
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}

		}

		protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) 
		{
			switch(e.KeyCode) 
			{
				case Keys.Escape:
					Close();
					break;
				case Keys.F4:
					if(e.Modifiers==Keys.Control)
						Close();
					break;
			}

			base.OnKeyDown(e);
		}
		
		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			this.Visible = false;
			if(this.curSelectedDataSet == null)
				return;

			this.currentImageList.Clear();
			this.currentAnimatingImages.Clear();
			if(this.imageLayer != null)
			{
				m_WorldWindow.CurrentWorld.RenderableObjects.Remove(this.imageLayer.Name);
				this.imageLayer.Dispose();
				this.imageLayer = null;
			}

			if(this.buttonLoad.Text == "Cancel")
			{
				this.buttonLoad.Text = "Load";
			}
			m_WorldWindow.Focus();
			base.OnClosing (e);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NRLMontereyGlobal));
			this.buttonLoad = new System.Windows.Forms.Button();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItemHelp = new System.Windows.Forms.MenuItem();
			this.menuItemAbout = new System.Windows.Forms.MenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.trackBar2 = new System.Windows.Forms.TrackBar();
			this.opacityLabel = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonLoad
			// 
			this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoad.Location = new System.Drawing.Point(8, 336);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.Size = new System.Drawing.Size(328, 23);
			this.buttonLoad.TabIndex = 2;
			this.buttonLoad.Text = "Load";
			this.buttonLoad.Click += new System.EventHandler(this.button1_Click);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 443);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(344, 22);
			this.statusBar1.TabIndex = 3;
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.Location = new System.Drawing.Point(9, 128);
			this.listBox1.Name = "listBox1";
			this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBox1.Size = new System.Drawing.Size(320, 147);
			this.listBox1.TabIndex = 1;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// treeView1
			// 
			this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.treeView1.ImageIndex = -1;
			this.treeView1.Location = new System.Drawing.Point(9, 8);
			this.treeView1.Name = "treeView1";
			this.treeView1.SelectedImageIndex = -1;
			this.treeView1.Size = new System.Drawing.Size(320, 112);
			this.treeView1.TabIndex = 0;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
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
			this.menuItemAbout.Text = "About...";
			this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(8, 312);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(224, 23);
			this.label1.TabIndex = 4;
			this.label1.Text = "Select multiple entries for animations";
			// 
			// trackBar2
			// 
			this.trackBar2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.trackBar2.LargeChange = 10;
			this.trackBar2.Location = new System.Drawing.Point(256, 280);
			this.trackBar2.Maximum = 100;
			this.trackBar2.Name = "trackBar2";
			this.trackBar2.Size = new System.Drawing.Size(75, 42);
			this.trackBar2.SmallChange = 3;
			this.trackBar2.TabIndex = 9;
			this.trackBar2.TickFrequency = 5;
			this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.trackBar2.Value = 100;
			this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
			// 
			// opacityLabel
			// 
			this.opacityLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.opacityLabel.BackColor = System.Drawing.Color.Transparent;
			this.opacityLabel.Location = new System.Drawing.Point(272, 312);
			this.opacityLabel.Name = "opacityLabel";
			this.opacityLabel.Size = new System.Drawing.Size(44, 19);
			this.opacityLabel.TabIndex = 10;
			this.opacityLabel.Text = "Opacity";
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BackColor = System.Drawing.SystemColors.Control;
			this.textBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBox1.Location = new System.Drawing.Point(8, 392);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(328, 48);
			this.textBox1.TabIndex = 11;
			this.textBox1.Text = "";
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(8, 368);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 16);
			this.label2.TabIndex = 12;
			this.label2.Text = "Data Description:";
			// 
			// NRLMontereyGlobal
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(344, 465);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.opacityLabel);
			this.Controls.Add(this.trackBar2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.treeView1);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.buttonLoad);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(176, 296);
			this.Name = "NRLMontereyGlobal";
			this.Text = "NRL Monterey \"Real-Time\" Weather v.1.1";
			((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		System.Threading.Thread downloaderThread = null;
		System.Timers.Timer animationTimer = new System.Timers.Timer(100);
		System.Collections.ArrayList currentAnimatingImages = new ArrayList();

		private void button1_Click(object sender, System.EventArgs e)
		{
			try
			{
				if(this.curSelectedDataSet == null)
					return;

				this.currentImageList.Clear();
				this.currentAnimatingImages.Clear();
				if(this.imageLayer != null)
				{
					m_WorldWindow.CurrentWorld.RenderableObjects.Remove(this.imageLayer.Name);
					this.imageLayer.Dispose();
					this.imageLayer = null;
				}

				if(this.buttonLoad.Text == "Cancel")
				{
					this.buttonLoad.Text = "Load";
				}
				else
				{

					this.currentAnimatingDataSet = this.curSelectedDataSet;

					if(this.listBox1.SelectedItems.Count > 0)
					{
						this.animationTimer.Start();
						foreach(string item in this.listBox1.SelectedItems)
						{
							this.currentImageList.Add(item);
						}
						this.buttonLoad.Text = "Cancel";
						this.downloaderThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.downloadThreadFunc));
						this.downloaderThread.IsBackground = true;
						this.downloaderThread.Start();
					}
				}
			}
			catch(Exception ex)
			{
				this.statusBar1.Text = "Error";
				Log.Write(ex);
			}
		}

		private void downloadThreadFunc()
		{
			try
			{
				if(this.curSelectedDataSet != null)
				{
					foreach(string imageFilePath in this.currentImageList)
					{
						string imageFullPath = Path.Combine( this.m_CacheDirectory, imageFilePath );
						statusBar1.Text = "Downloading: " + imageFilePath;
						if(!File.Exists(imageFullPath))
						{
							using( WebClient client = new WebClient() )
								client.DownloadFile(this.curSelectedDataSet.UrlDirectory + imageFilePath, imageFullPath );
						}
						statusBar1.Text = "";
						this.currentAnimatingImages.Add(imageFilePath);
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}

		delegate
			void updateListDelegate(object item);
		
		void updateList(object item)
		{
			this.listBox1.Items.Add(item);
		}

		delegate
			void clearListDelegate();
		
		void clearList()
		{
			this.listBox1.Items.Clear();
		}

		private void animationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(this.isUpdating)
				return;

			this.isUpdating = true;
			if(this.currentAnimatingImages.Count > 0)
			{
				if(this.imageLayer == null)
				{
						byte opacity = (byte)(255 * ((float)this.trackBar2.Value / this.trackBar2.Maximum));
					this.imageLayer = new ImageLayer("NRL, Monterey -- \"Real-Time\" Weather", 
						m_WorldWindow.CurrentWorld, 
						30000.0f,
						Path.Combine( this.m_CacheDirectory, ((string)this.currentAnimatingImages[0]) ), 
						(float)this.currentAnimatingDataSet.South, (float)this.currentAnimatingDataSet.North,
						(float)this.currentAnimatingDataSet.West, (float)this.currentAnimatingDataSet.East,
						opacity, null);

					this.imageLayer.IsOn = true;
					m_WorldWindow.CurrentWorld.RenderableObjects.Add(this.imageLayer);
				}
				else if(this.currentAnimatingImages.Count != 1)
				{
					this.imageLayer.UpdateTexture( Path.Combine( this.m_CacheDirectory, ((string)this.currentAnimatingImages[currentFrame])));
					this.currentFrame++;

					if(this.currentFrame >= this.currentAnimatingImages.Count)
					{
						System.Threading.Thread.Sleep(1000);
						this.currentFrame = 0;
					}
				}
				
			}
			this.isUpdating = false;
		}

		private void UpdateListFromServer()
		{
			textBox1.Text = curSelectedDataSet.Description;
			if (this.curSelectedDataSet == null)
				return;

			this.listBox1.Items.Clear();

			this.treeView1.Enabled = false;
			try
			{
				string serverlistFilePath = Path.Combine( this.m_CacheDirectory, "serverlist.tmp");
				using (WebClient client = new WebClient())
					client.DownloadFile(curSelectedDataSet.UrlDirectory, serverlistFilePath );

				string line = null;
				using (StreamReader sr = new StreamReader( serverlistFilePath, System.Text.Encoding.ASCII))
					line = sr.ReadToEnd();
				string[] lineParts = line.Split('"');
				foreach (string part in lineParts)
				{
					string[] subParts = part.Split('.');
					//If you just want to include all "good" images from the website (then this if statement can be used
					//if ((part.IndexOf("icons/")==-1) && (part.IndexOf("<")<0) && ((part.IndexOf(".gif")!=-1) || (part.IndexOf(".jpg")!=-1) || (part.IndexOf(".png")!=-1) || (part.IndexOf(".tif")!=-1)))
					//{
						if (subParts.Length > 3 && part.IndexOf("<") < 0)
						{
							if (this.curSelectedDataSet.KeywordFilter == null || part.IndexOf(curSelectedDataSet.KeywordFilter) >= 0)
							{
								//string[] subParts = part.Split('.');
								//ListViewItem li = new ListViewItem(subParts[0] + "@" + subParts[1]);
								//li.Tag = part;
								this.listBox1.BeginInvoke(new updateListDelegate(this.updateList), new object[] { part });
							}
						}
					//}
				}
			}
			catch(Exception ex)
			{
				using(StreamWriter sw = new StreamWriter("nrlerror.txt", false, System.Text.Encoding.ASCII))
				{
					sw.WriteLine(ex.ToString());
				}
			}
			this.treeView1.Enabled = true;
		}

		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			try
			{
				if(e.Node.Tag != null)
				{
					this.curSelectedDataSet = (NRLDataSet)e.Node.Tag;
					System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateListFromServer));
					t.IsBackground = true;
					t.Start();
				
				}
				else
					this.curSelectedDataSet = null;
			}
			catch(Exception)
			{
				this.statusBar1.Text = "Error";
			}
		}
	
		private void trackBar2_Scroll(object sender, System.EventArgs e)
		{
			if(this.imageLayer != null) 
			{
				this.imageLayer.UpdateOpacity((float)this.trackBar2.Value / this.trackBar2.Maximum);
				this.Invalidate();
			}
		
		}

		private void menuItemAbout_Click(object sender, System.EventArgs e)
		{
			if(m_AboutDialog == null)
			{
				m_AboutDialog = new NrlMryAboutDialog();
				m_AboutDialog.Owner = this;
			}

			m_AboutDialog.ShowDialog();
		}

		NrlMryAboutDialog m_AboutDialog = null;

		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

	}

	public class NRLWeatherLoader : WorldWind.PluginEngine.Plugin
	{
		MenuItem m_MenuItem;
		NRLMontereyGlobal m_Form = null;
		WorldWind.WindowsControlMenuButton m_ToolbarItem = null;

		/// <summary>
		/// Plugin entry point 
		/// </summary>
		public override void Load() 
		{
			if(ParentApplication.WorldWindow.CurrentWorld != null && ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("Earth") >= 0)
			{
				m_MenuItem = new MenuItem("NRL Weather");
				m_MenuItem.Click += new EventHandler(menuItemClicked);
				ParentApplication.PluginsMenu.MenuItems.Add( m_MenuItem );
			
				m_Form = new NRLMontereyGlobal(ParentApplication.WorldWindow,
					Path.Combine(ParentApplication.WorldWindow.Cache.CacheDirectory, "NrlWeather"),
					Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Plugins\\NRLMonterey\\NRL_Monterey.xml");
				m_Form.Owner = ParentApplication;
			
				m_ToolbarItem = new WorldWind.WindowsControlMenuButton(
					"Naval Research Labs, Monterey -- \"Real-Time\" Weather",
					Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Data\\Icons\\Interface\\nrl.png",
					m_Form);
			
				ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_ToolbarItem);
			}
		}

		/// <summary>
		/// Unload our plugin
		/// </summary>
		public override void Unload() 
		{
			if(m_MenuItem!=null)
			{
				ParentApplication.ToolsMenu.MenuItems.Remove( m_MenuItem );
				m_MenuItem.Dispose();
				m_MenuItem = null;
			}

			if(m_ToolbarItem != null)
			{
				ParentApplication.WorldWindow.MenuBar.RemoveToolsMenuButton(m_ToolbarItem);
				m_ToolbarItem.Dispose();
				m_ToolbarItem = null;
			}

			if(m_Form != null)
			{
				m_Form.Dispose();
				m_Form = null;
			}
		}
	
		void menuItemClicked(object sender, EventArgs e)
		{
			if(m_Form.Visible)
			{
				m_Form.Visible = false;
				m_MenuItem.Checked = false;
			}
			else
			{
				
				m_Form.Visible = true;
				m_MenuItem.Checked = true;
			}
		}
	}
}
