using System;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;
using System.Windows.Forms;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.Net;
using ICSharpCode.SharpZipLib.Zip;
using Utility;

namespace NLT.Plugins
{
	internal class ShapeFileLoaderGUI:System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;

		public static ShapeFileLoader m_ShapeLoad = null;


		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(456, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(88, 24);
			this.button1.TabIndex = 2;
			this.button1.Text = "Browse";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(240, 96);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(88, 24);
			this.button3.TabIndex = 1;
			this.button3.Text = "Load";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(40, 18);
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(400, 20);
			this.textBox1.TabIndex = 3;
			this.textBox1.Text = "Local Shape File";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(40, 58);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(400, 20);
			this.textBox2.TabIndex = 4;
			this.textBox2.Text = "http://";
			this.textBox2.TextChanged += new System.EventHandler(this.OnTextChanged);
			// 
			// radioButton1
			// 
			this.radioButton1.Checked = true;
			this.radioButton1.Location = new System.Drawing.Point(8, 16);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(16, 24);
			this.radioButton1.TabIndex = 7;
			this.radioButton1.TabStop = true;
			// 
			// radioButton2
			// 
			this.radioButton2.Location = new System.Drawing.Point(8, 56);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(16, 24);
			this.radioButton2.TabIndex = 8;
			// 
			// ShapeFileLoaderGUI
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(568, 134);
			this.Controls.Add(this.radioButton2);
			this.Controls.Add(this.radioButton1);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button1);
			this.MaximizeBox = false;
			this.Name = "ShapeFileLoaderGUI";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Shape File Loader";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
			this.ResumeLayout(false);

		}


		private void button1_Click(object sender, System.EventArgs e)
		{
			this.openFileDialog1.Filter = "All supported Shape File formats|*.shp;*.zip;*.xml|"+
				"Shape files (*.shp)|*.shp|Zip files (*.zip)|*.zip|XML files (*.xml)|*xml";
			if(this.openFileDialog1.ShowDialog(this.Parent) == DialogResult.OK)
			{
				//shape file selected show XML generator
				textBox1.Text = openFileDialog1.FileName;
				radioButton1.Checked=true;
				radioButton2.Checked=false;
			}
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			//TODO: delete test
			//ShapeFileInfoTool.showShapeFileInfoDlg(
			//	"F:\\Software Dani\\Visual Studio Projects\\WorldWind\\shapefile samples\\spain\\sp.dbf");
			//return;

			//Local Shaefile loading
			if(radioButton1.Checked)
			{

				string localPath = textBox1.Text.Trim();
				if(localPath.ToLower().EndsWith(".zip"))
				{					
					//Loads .zip from local path
					m_ShapeLoad.createXmlLoadShape(localPath);
				}
				else if(localPath.ToLower().EndsWith(".shp"))
				{
					//loads .shp+.dbf from local path
					m_ShapeLoad.createXmlLoadShape(localPath);
				}
				else if(localPath.ToLower().EndsWith(".xml"))
				{
					//loads .xml+.shp+.dbf from local path
					m_ShapeLoad.loadShapeFileWithAlreadyExistingXML(localPath,true);
				}
				else
				{
					System.Windows.Forms.MessageBox.Show
						("Shape file could not be loaded: Wrong file format");
				}

			}
			//Remote Schapefile Loading
			if(radioButton2.Checked)
			{
				string remotePath = textBox2.Text.Trim();
				if(remotePath.ToLower().EndsWith(".zip"))
				{
					//Loads .zip from remote path
					m_ShapeLoad.loadZipFileFromURL(remotePath);
				}
				else if(remotePath.ToLower().EndsWith(".shp"))
				{
					//loads .shp+.dbf from remote path
					m_ShapeLoad.loadShpFileFromURL(remotePath);
				}
				else if(remotePath.ToLower().EndsWith(".xml"))
				{
					//loads xml+zip or xml+shp from url
					m_ShapeLoad.loadXMLFileFromURL(remotePath);
				}
				else
				{
					System.Windows.Forms.MessageBox.Show
						("Shape file could not be loaded: Wrong file format");
				}
			}
		}


		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Visible = false;
			m_ShapeLoad.m_MenuItem.Checked = false;
		}
		
		public ShapeFileLoaderGUI(ShapeFileLoader shapeload)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			if(shapeload!=null)
			{
				m_ShapeLoad = shapeload;
			}
		}

		private void OnTextChanged(object sender, System.EventArgs e)
		{
			radioButton1.Checked=false;
			radioButton2.Checked=true;
		}

		private void OnDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
			{
				String[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (File.Exists(s[0])&&(s[0].EndsWith(".xml")||
					s[0].EndsWith(".shp")||	s[0].EndsWith(".zip")))
				{
					e.Effect = DragDropEffects.Link;
				}
			}
		}

		private void OnDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			string[] path = (string[])e.Data.GetData(DataFormats.FileDrop);
			textBox1.Text = path[0];
			Update();

		}
	}
	/// <summary>
	/// Summary description for ShapeFileLoader.
	/// </summary>
	public class ShapeFileLoader : WorldWind.PluginEngine.Plugin
	{

		private string m_ShapeFileRootDirectory = null;
		ShapeFileLoaderGUI m_Gui = null;
		public MenuItem m_MenuItem = null;
		private string m_defaultxml = "@default.xml";

		ArrayList actualDownloads = new ArrayList();
		ArrayList xmlDownloads = new ArrayList();
		
		public override void Load() 
		{
			try
			{
				if(ParentApplication.WorldWindow.CurrentWorld != null && ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("Earth") >= 0)
				{
					m_MenuItem = new MenuItem("Shapefile loader");
					m_MenuItem.Click += new EventHandler(menuItemClicked);
					ParentApplication.ToolsMenu.MenuItems.Add(3, m_MenuItem );

					m_Gui = new ShapeFileLoaderGUI(this);
					m_Gui.Owner = ParentApplication;
				}

				m_ShapeFileRootDirectory = 
					String.Format("{0}\\ShapeFiles", 
					Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));

				DirectoryInfo rootShapeDir = new DirectoryInfo(m_ShapeFileRootDirectory);

				if(!rootShapeDir.Exists)
				{
					rootShapeDir.Create();
				}
				else
				{
					foreach(FileInfo shapeConfigFile in rootShapeDir.GetFiles("*.xml"))
					{
						if(shapeConfigFile.Name!=this.m_defaultxml)
							loadShapeFileWithAlreadyExistingXML(shapeConfigFile.FullName,true);							
					}
				}		
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
			base.Load ();
		}		


		public void ParseUri(string[] args) 
		{	

			foreach(string rawArg in args)
			{		

				string arg = rawArg.Trim();
				if(arg.Length<=0)
					continue;			

				if(arg.StartsWith("worldwind://"))
				{
					arg.Trim().ToLower(CultureInfo.InvariantCulture);
					WorldWindUri uri = new WorldWindUri();

					string url = arg.Replace("worldwind://", "");
					if(url.Length == 0)
						throw new UriFormatException("Incomplete URI");

					string[] functions = url.Split('!');

					foreach(string function in functions)
					{
						if(function.IndexOf("shapefile/") == 0)
						{
							if(m_ShapeFileRootDirectory == null)
							{
								m_ShapeFileRootDirectory = 
									String.Format("{0}\\ShapeFiles", 
									Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));
							}
							string functionString = function.Replace("shapefile/", "").Trim();									
							if(functionString.ToLower().EndsWith(".xml"))
							{								
								//loads xml+zip or xml+shp from url
								loadXMLFileFromURL(functionString);
							}
							else if(functionString.ToLower().EndsWith(".zip"))
							{
								//Loads .zip from remote path
								loadZipFileFromURL(functionString);
							}
							else if(functionString.ToLower().EndsWith(".shp"))
							{
								//loads .shp+.dbf from remote path								
								loadShpFileFromURL(functionString);
							}
							else
								throw new UriFormatException("Wrong URI format");
						}
					}
				}
			}
		}


		public override void Unload()
		{
			base.Unload ();
		}
		

		#region DownloadFromURL		

		/// <summary>
		/// Update XML and relative files
		/// </summary>
		/// <param name="shapeConfigFilePath"></param>
		public void UpdateXMLAndFiles(string shapeConfigFilePath, string downloadURL)
		{
			//Delete old files			
			Directory.Delete(Path.GetDirectoryName(shapeConfigFilePath)+"\\"+
				Path.GetFileNameWithoutExtension(shapeConfigFilePath), true);
			File.Delete(shapeConfigFilePath);

			//Download and load the new ones
			loadXMLFileFromURL(downloadURL);
		}

		/// <summary>
		/// Method to load Shape files from shape+xml zip packs supplied in URL
		/// </summary>
		/// <param name="zipURL"></param>
		public void loadZipFileFromURL(string zipURL)
		{
			
			string zippath = m_ShapeFileRootDirectory+"\\"+Path.GetFileName(zipURL);
			int Nr=1;
			while(File.Exists(zippath))
			{
				zippath = m_ShapeFileRootDirectory+"\\"+Path.GetFileNameWithoutExtension(zipURL)
					+"~"+Nr.ToString()+Path.GetExtension(zipURL);
				Nr++;
			}

			WebDownload dl = new WebDownload(zipURL);
			dl.CompleteCallback += new DownloadCompleteHandler(ZipDownloadComplete);
			dl.SavedFilePath = zippath;
			actualDownloads.Add(dl);
			dl.BackgroundDownloadFile();
		
		}


		/// <summary>
		/// Method to load Shape files from shp+dbf supplied in URL
		/// </summary>
		/// <param name="shpURL"></param>
		public void loadShpFileFromURL(string shpURL)
		{		
			string shppath = m_ShapeFileRootDirectory+"\\"+Path.GetFileName(shpURL);
	
			int Nr=1;
			while(File.Exists(shppath)||File.Exists(Path.ChangeExtension(shppath,".dbf")))
			{
				shppath = m_ShapeFileRootDirectory+"\\"+Path.GetFileNameWithoutExtension(shpURL)
					+"~"+Nr.ToString()+Path.GetExtension(shpURL);
				Nr++;
			}		
						
			WebDownload dl1 = new WebDownload(shpURL);
			WebDownload dl3 = new WebDownload(Path.ChangeExtension(shpURL,".dbf"));

			dl1.CompleteCallback += new DownloadCompleteHandler(ShpDownloadComplete);			
			dl1.SavedFilePath = shppath;
			actualDownloads.Add(dl1);
			dl1.BackgroundDownloadFile();
			
			dl3.CompleteCallback += new DownloadCompleteHandler(ShpDownloadComplete);			
			dl3.SavedFilePath = Path.ChangeExtension(shppath,".dbf");
			actualDownloads.Add(dl3);
			dl3.BackgroundDownloadFile();

		}	
	

		/// <summary>
		/// Method to load Shape files from shp+shx+dbf supplied in URL
		/// </summary>
		/// <param name="shpURL"></param>
		public void loadXMLFileFromURL(string xmlURL)
		{
			XmlDocument doc=new XmlDocument();
			doc.Load(xmlURL);  
			
			try
			{
				doc.SelectSingleNode
					("/LayerSet/DownloadPage/URL").InnerText=xmlURL;
			}
			catch{/*no timestamp*/}

			string xmlSaveLocation = m_ShapeFileRootDirectory+"\\"+Path.GetFileName(xmlURL);
			string xmlFilesFolder = m_ShapeFileRootDirectory+"\\"
										+Path.GetFileNameWithoutExtension(xmlURL);
			string relFolder=Path.GetFileNameWithoutExtension(xmlURL)+"\\";

			//find a not used name to store the shapefile
			int Nr=1;
			while(File.Exists(xmlSaveLocation)||Directory.Exists(xmlFilesFolder))
			{
				xmlSaveLocation = m_ShapeFileRootDirectory+"\\"+Path.GetFileNameWithoutExtension(xmlURL)
					+"~"+Nr.ToString()+Path.GetExtension(xmlURL);
				xmlFilesFolder = m_ShapeFileRootDirectory+"\\"+Path.GetFileNameWithoutExtension(xmlURL)
					+"~"+Nr.ToString();
				relFolder=Path.GetFileNameWithoutExtension(xmlURL)+"~"+Nr.ToString()+"\\";
				Nr++;
			}		

			XmlNodeList namelist = doc.GetElementsByTagName("ShapeFilePath");

			ArrayList filePaths = new ArrayList();
			filePaths.Add(xmlSaveLocation);

			string xmlFolder=xmlURL.Remove(1+xmlURL.LastIndexOfAny(new char[]{'/','\\'}),
				xmlURL.Length-1-xmlURL.LastIndexOfAny(new char[]{'/','\\'}));			
			//get the paths to the xml related files
			foreach(XmlNode node in namelist)
			{
				if(!filePaths.Contains(xmlFolder+node.InnerText))	
				{
					if(node.InnerText.ToLower().EndsWith(".shp"))
					{
						filePaths.Add(xmlFolder+node.InnerText);
						filePaths.Add(xmlFolder+node.InnerText.Replace(".shp",".dbf"));
					}
					else
						filePaths.Add(xmlFolder+node.InnerText);
				}								
				node.InnerText=relFolder+Path.GetFileName(node.InnerText);
			}

			//Save xml
			doc.Save(xmlSaveLocation);

			//download relative files and load xml
			if(filePaths.Count!=1)
				Directory.CreateDirectory(xmlFilesFolder);			

			xmlDownloads.Add(filePaths);

			WebDownload[] wd=new WebDownload[filePaths.Count-1];
			for(int i=1; i< filePaths.Count; i++)
			{					
				wd[i-1]= new WebDownload(((string)filePaths[i]));
				wd[i-1].CompleteCallback += new DownloadCompleteHandler(xmlRelatedFileDownloadComplete);			
				wd[i-1].SavedFilePath = xmlFilesFolder+"\\"+Path.GetFileName(((string)filePaths[i]));				
				wd[i-1].BackgroundDownloadFile();
			}			
		}
		#endregion

		#region DownloadComplete handler
		/// <summary>
		/// Web download callback for shp+shx+dbf
		/// </summary>
		private void ShpDownloadComplete(WebDownload dl)
		{
			if(dl.SavedFilePath.ToLower().EndsWith(".shp"))
			{
				for(int i=0; i<actualDownloads.Count; i++)
				{
					if( ((WebDownload)actualDownloads[i]).SavedFilePath==
						Path.ChangeExtension(dl.SavedFilePath,".dbf"))
					{
						if( ((WebDownload)actualDownloads[i]).IsComplete )
						{
							((WebDownload)actualDownloads[i]).Dispose();
							actualDownloads.RemoveAt(i);
							createXmlLoadShape(dl.SavedFilePath);
						}
						else
							return;
					}				
				}
			}
			else 
			{
				for(int i=0; i<actualDownloads.Count; i++)
				{
					if( ((WebDownload)actualDownloads[i]).SavedFilePath==
						Path.ChangeExtension(dl.SavedFilePath,".shp"))
					{
						if( ((WebDownload)actualDownloads[i]).IsComplete )
						{
							((WebDownload)actualDownloads[i]).Dispose();
							actualDownloads.RemoveAt(i);
							createXmlLoadShape(Path.ChangeExtension(dl.SavedFilePath,".shp"));
						}
						else
							return;
					}				
				}
			}						
			dl.Dispose();
		}

		
		/// <summary>
		/// Web download callback for Zips
		/// </summary>
		private void ZipDownloadComplete(WebDownload dl)
		{
			createXmlLoadShape(dl.SavedFilePath);			
			dl.Dispose();
		}
		/// <summary>
		/// Web download callback for Files relative to a xml
		/// used by loadXMLFileFromURL
		/// </summary>
		private void xmlRelatedFileDownloadComplete(WebDownload dl)
		{	
			for(int i=0; i<xmlDownloads.Count; i++)
			{	
				for(int u=1; u<((ArrayList)(xmlDownloads[i])).Count; u++)//xmlDownloads[0] is the xml path
				{					
					if(((string)((ArrayList)(xmlDownloads[i]))[u]) ==
						dl.Url)
					{						
						((ArrayList)(xmlDownloads[i])).RemoveAt(u);
						if(((ArrayList)(xmlDownloads[i])).Count <= 1)
						{								
							//load xml (all files have been downloaded)
							loadShapeFileWithAlreadyExistingXML(
								((string)((ArrayList)(xmlDownloads[i]))[0]),false);
							xmlDownloads.RemoveAt(i);
						}
					}
				}
				
			}

			dl.Dispose();
		}

		#endregion

		#region Load local Shapefiles

		/// <summary>
		/// This method generated a default configuration for a ShapeFile and loads it
		/// </summary>
		/// <param name="shapepath">Full Path to Shapefile being loaded</param>
		public void createXmlLoadShape(string shapepath)
		{
			//confirm existense of .shp,.dbf and .shx
			string shpfile = shapepath;
			//string shxfile = shapepath.Replace(".shp",".shx");
			string dbffile= Path.ChangeExtension(shapepath,".dbf");
						
			string specific = shpfile.Substring(shpfile.LastIndexOf("\\")+1);
			specific = specific.Split('.')[0];

			XmlDocument docNav;

			//shx files are not used to render shapefiles
			if((shapepath.ToLower().EndsWith(".shp")&&
				File.Exists(shapepath)/*&&File.Exists(shxfile)*/&&File.Exists(dbffile))||
				(shapepath.ToLower().EndsWith(".zip")&&File.Exists(shapepath)))
			{
				string defaultxmlpath = m_ShapeFileRootDirectory+"\\"+m_defaultxml;
				string specificxmlpath = m_ShapeFileRootDirectory+"\\"+specific+".xml";
				int Nr=1;
				while(File.Exists(specificxmlpath))
				{
					specificxmlpath = m_ShapeFileRootDirectory+"\\"+specific+"~"+Nr.ToString()+".xml";
					Nr++;
				}
				// Open the XML.
				try
				{
					docNav = new XmlDocument();
					docNav.Load(defaultxmlpath);
					//select all linked shapefiles
					XmlNodeList shapelist = docNav.GetElementsByTagName("ShapeFilePath");
					//replace all linked shapefiles with shapefile being loaded
					foreach(XmlNode node in shapelist)
					{
						node.InnerText = shpfile;
					}
					
					XmlNodeList namelist = docNav.GetElementsByTagName("Name");

					//replace all linked shapefiles with shapefile being loaded
					foreach(XmlNode node in namelist)
					{
						node.InnerText = specific;
					}

					//Set the shapefile layername to name+extension of the loaded file
					//to avoid same layername to all loaded shapefiles without xml
					docNav.SelectSingleNode("/LayerSet")
						.Attributes["Name",""].InnerText=
						Path.GetFileNameWithoutExtension(specificxmlpath)+
						Path.GetExtension(shapepath);

					//Timestamp
					docNav.SelectSingleNode("/LayerSet/TimeStamp/year").InnerText=DateTime.Now.ToUniversalTime().Year.ToString();
					docNav.SelectSingleNode("/LayerSet/TimeStamp/month").InnerText=DateTime.Now.ToUniversalTime().Month.ToString();
					docNav.SelectSingleNode("/LayerSet/TimeStamp/day").InnerText=DateTime.Now.ToUniversalTime().Day.ToString();
					docNav.SelectSingleNode("/LayerSet/TimeStamp/hour").InnerText=DateTime.Now.ToUniversalTime().Hour.ToString();
					docNav.SelectSingleNode("/LayerSet/TimeStamp/minute").InnerText=DateTime.Now.ToUniversalTime().Minute.ToString();	

					docNav.Save(specificxmlpath);
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
					return;
				}
				loadShapeFileWithAlreadyExistingXML(specificxmlpath,false);
			}
		}


		/// <summary>
		/// This method Loads a shapefile 
		/// </summary>
		/// <param name="shapeConfigFilePath">Full Path to the Shapefiles XML configuration file</param>
		/// <param name="checkForUpdate">true if it should check for a newer version of the shapefile</param>
		public void loadShapeFileWithAlreadyExistingXML(string shapeConfigFilePath, bool checkForUpdate)
		{
			//Check for updates
			if(checkForUpdate)
			{
				try
				{
					XmlDocument doc=new XmlDocument();
					doc.Load(shapeConfigFilePath);  
					string downloadURL;
					
					if((downloadURL=doc.SelectSingleNode("/LayerSet/DownloadPage/URL").InnerText)!="")
					{
						if(checkIfUpdateNeeded(downloadURL,shapeConfigFilePath))
						{
							//Update xml+files
							UpdateXMLAndFiles(shapeConfigFilePath, downloadURL);								
						}
					}
				}
				catch{}
			}


			XPathNavigator nav;
			XPathDocument docNav;
			
			// Open the XML.
			docNav = new XPathDocument(shapeConfigFilePath);			

			// Create a navigator to query with XPath.
			nav = docNav.CreateNavigator();

			XPathNodeIterator layersetIter = nav.Select("/LayerSet");
			if(layersetIter.Count > 0)
			{
				while(layersetIter.MoveNext())
				{
					string layersetName = layersetIter.Current.GetAttribute("Name","");
					if(layersetName == null)
						continue;
					string showOnlyOneLayerString = layersetIter.Current.GetAttribute("ShowOnlyOneLayer", "");
					string showAtStartupString = layersetIter.Current.GetAttribute("ShowAtStartup", "");
					bool showOnlyOneLayer = false;
					bool showAtStartup = false;
					try
					{
						showOnlyOneLayer = ParseBool(showOnlyOneLayerString);
						
					}
					catch(Exception)
					{
					}

					try
					{
						showAtStartup = ParseBool(showAtStartupString);
					}
					catch{}
					
					WorldWind.Renderable.RenderableObjectList newLayerSetList
						= new RenderableObjectList(layersetName);					
					
					newLayerSetList.ShowOnlyOneLayer = showOnlyOneLayer;
					
					newLayerSetList.ParentList = ParentApplication.WorldWindow.CurrentWorld.RenderableObjects;					
					
					if(World.Settings.UseDefaultLayerStates)
					{
						newLayerSetList.IsOn = showAtStartup;
					}
					else
					{
						newLayerSetList.IsOn = ConfigurationLoader.IsLayerOn(newLayerSetList);
					}
					XPathNodeIterator shapeIter = layersetIter.Current.Select("ShapeFileDescriptor");
					if(shapeIter.Count > 0)
					{
						while(shapeIter.MoveNext())
						{
							string name = getInnerTextFromFirstChild(shapeIter.Current.Select("Name"));
							string shapeFilePath = getInnerTextFromFirstChild(shapeIter.Current.Select("ShapeFilePath"));
							string dataKey = getInnerTextFromFirstChild(shapeIter.Current.Select("DataKey"));

							string showLabelsString = getInnerTextFromFirstChild(shapeIter.Current.Select("ShowLabels"));
							string polygonFillString = getInnerTextFromFirstChild(shapeIter.Current.Select("PolygonFill"));
							string outlinePolygonsString = getInnerTextFromFirstChild(shapeIter.Current.Select("OutlinePolygons"));
							string lineWidthString = getInnerTextFromFirstChild(shapeIter.Current.Select("LineWidth"));
							string iconFilePath = getInnerTextFromFirstChild(shapeIter.Current.Select("IconFilePath"));
							string iconWidthString = getInnerTextFromFirstChild(shapeIter.Current.Select("IconWidth"));
							string iconHeightString = getInnerTextFromFirstChild(shapeIter.Current.Select("IconHeight"));
							string iconOpacityString = getInnerTextFromFirstChild(shapeIter.Current.Select("IconOpacity"));
							string scaleColorsToDataString = getInnerTextFromFirstChild(shapeIter.Current.Select("ScaleColorsToData"));

							/*Altitude Rendering*/
							string maxAltString = getInnerTextFromFirstChild(shapeIter.Current.Select("MaxAltitude"));
							string minAltString = getInnerTextFromFirstChild(shapeIter.Current.Select("MinAltitude"));

							/*Tile Size Rendering*/
							string lztsdString = getInnerTextFromFirstChild(shapeIter.Current.Select("LevelZeroTileSize"));

							/*LatLong Bounding Box*/
							string northString = getInnerTextFromFirstChild(shapeIter.Current.Select("North"));
							string southString = getInnerTextFromFirstChild(shapeIter.Current.Select("South"));
							string eastString = getInnerTextFromFirstChild(shapeIter.Current.Select("East"));
							string westString = getInnerTextFromFirstChild(shapeIter.Current.Select("West"));

							/*Opacity added by Argon helm*/
							string layerOpacityString = getInnerTextFromFirstChild(shapeIter.Current.Select("LayerOpacity")); 
							
							bool showLabels = false;
							bool polygonFill = false;
							float lineWidth = 1.0f;
							bool outlinePolygons = false;
							bool scaleColorsToData = false;
							int iconWidth = 32;
							int iconHeight = 32;
							byte iconOpacity = 255;

							/*Layer Opacity added by Argon Helm*/
							byte layerOpacity = 255; 

							/*Altitude Rendering*/
							double maxAlt = double.MaxValue;
							double minAlt = 0;

							/*Tile Size Rendering*/
							float lztsd = 180.0f/5;

							/*LatLong Bounding Box*/
							GeographicBoundingBox bounds = new GeographicBoundingBox(90.0,-90,-180.0,180.0);

							System.Drawing.Color lineColor = System.Drawing.Color.Black;
							System.Drawing.Color polygonColor = System.Drawing.Color.Black;
							System.Drawing.Color labelColor = System.Drawing.Color.White;
							WorldWind.ShapeFillStyle shapeFillStyle = getShapeFillStyleFromString(getInnerTextFromFirstChild(shapeIter.Current.Select("PolygonFillStyle")));

							XPathNodeIterator lineColorIter = shapeIter.Current.Select("LineColor");
							if(lineColorIter.Count > 0)
							{
								lineColor = getColorFromXPathIter(lineColorIter);
							}

							XPathNodeIterator polygonColorIter = shapeIter.Current.Select("PolygonColor");
							if(polygonColorIter.Count > 0)
							{
								polygonColor = getColorFromXPathIter(polygonColorIter);
							}

							XPathNodeIterator labelColorIter = shapeIter.Current.Select("LabelColor");
							if(labelColorIter.Count > 0)
							{
								labelColor = getColorFromXPathIter(labelColorIter);
							}

							showAtStartupString = shapeIter.Current.GetAttribute("ShowAtStartup","");
							try
							{
								if(showAtStartupString != null)
								{
									showAtStartup = ParseBool(showAtStartupString);
								}
								else
								{
									showAtStartup = false;
								}

								if(scaleColorsToDataString != null)
								{
									scaleColorsToData = ParseBool(scaleColorsToDataString);
								}
								
								if(showLabelsString != null)
								{
									showLabels = ParseBool(showLabelsString);
								}

								if(polygonFillString != null)
								{
									polygonFill = ParseBool(polygonFillString);
								}

								if(lineWidthString != null)
								{
									lineWidth = float.Parse(lineWidthString);
								}

								if(outlinePolygonsString != null)
								{
									outlinePolygons = ParseBool(outlinePolygonsString);
								}
								if(iconHeightString != null)
								{
									iconHeight = int.Parse(iconHeightString);
								}
								if(iconWidthString != null)
								{
									iconWidth = int.Parse(iconWidthString);
								}
								if(iconOpacityString != null)
								{
									iconOpacity = byte.Parse(iconOpacityString);
								}
								/*Altitude Rendering*/
								if(minAltString!=null)
								{
									minAlt=ParseDouble(minAltString);
								}
								if(maxAltString!=null)
								{
									maxAlt=ParseDouble(maxAltString);
								}

								/*Lztsd rendering*/
								if(lztsdString!=null)
								{
									lztsd=float.Parse(lztsdString);
								}

								/*latlon bounds*/
								if(northString!=null&&southString!=null&&westString!=null&&eastString!=null)
								{
									bounds = new GeographicBoundingBox(ParseDouble(northString),
										ParseDouble(southString),
										ParseDouble(westString),
										ParseDouble(eastString));
								}
								/*Layer Opacity added by argon helm*/
								if(layerOpacityString != null) 
								{
									layerOpacity = byte.Parse(layerOpacityString);
								}
							}
							catch(Exception ex)
							{
								Log.Write(ex);
							}
							string scalarMinString = getInnerTextFromFirstChild(shapeIter.Current.Select("ScalarMin"));
							string scalarMaxString = getInnerTextFromFirstChild(shapeIter.Current.Select("ScalarMax"));
							string scalarFilterMinString = getInnerTextFromFirstChild(shapeIter.Current.Select("ScalarFilterMin"));
							string scalarFilterMaxString = getInnerTextFromFirstChild(shapeIter.Current.Select("ScalarFilterMax"));
							string[] noDataValues = getStringValues(shapeIter.Current.Select("NoDataValue"));
							string[] activeDataValues = getStringValues(shapeIter.Current.Select("ActiveDataValue"));

							double scalarMin = double.NaN;
							double scalarMax = double.NaN;
							double scalarFilterMin = double.NaN;
							double scalarFilterMax = double.NaN;
							
							if(scalarMinString != null)
							{
								scalarMin = ParseDouble(scalarMinString);
							}

							if(scalarMaxString != null)
							{
								scalarMax = ParseDouble(scalarMaxString);
							}

							if(scalarFilterMinString != null)
							{
								scalarFilterMin = ParseDouble(scalarFilterMinString);
							}

							if(scalarFilterMaxString != null)
							{
								scalarFilterMax = ParseDouble(scalarFilterMaxString);
							}							
							/*if(!Path.IsPathRooted(shapeFilePath))
							{
								shapeFilePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + shapeFilePath;
							}*/
							//path is relative to xml file not to executing apps directory
							if(!Path.IsPathRooted(shapeFilePath))
							{
								shapeFilePath = shapeConfigFilePath.Remove(
									shapeConfigFilePath.LastIndexOfAny(new char[]{'\\','/'}),
									shapeConfigFilePath.Length-
									shapeConfigFilePath.LastIndexOfAny(new char[]{'\\','/'})) 
									+ "\\" + shapeFilePath;
							}

							

							WorldWind.ShapeFileLayer shapeFileLayer = new ShapeFileLayer(
								name,
								ParentApplication.WorldWindow.CurrentWorld,
								shapeFilePath,
								minAlt,
								maxAlt,
								lztsd,
								bounds,
								dataKey,
								scaleColorsToData,
								scalarFilterMin,
								scalarFilterMax,
								scalarMin,
								scalarMax,
								noDataValues,
								activeDataValues,
								polygonFill,
								outlinePolygons,
								polygonColor,
								shapeFillStyle,
								lineColor,
								lineWidth,
								showLabels,
								labelColor,
								Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + iconFilePath,
								iconWidth,
								iconHeight,
								iconOpacity
								);

							/*Layer Opacity added by Argon Helm*/
							shapeFileLayer.Opacity = layerOpacity;
							shapeFileLayer.ParentList = newLayerSetList;

							// this goes after opacity because setting opacity "turns on" a renderable object
							if(World.Settings.UseDefaultLayerStates)
							{
								shapeFileLayer.IsOn = showAtStartup;
							}
							else
							{
								shapeFileLayer.IsOn = ConfigurationLoader.IsLayerOn(shapeFileLayer);
							}			
							if(shapeFilePath.ToLower().EndsWith(".shp"))
							{								
								shapeFileLayer.dbfPath=Path.ChangeExtension(shapeFilePath,".dbf");
								shapeFileLayer.dbfIsInZip=false;								
							}
							else if(shapeFilePath.ToLower().EndsWith(".zip"))
							{
								shapeFileLayer.dbfIsInZip=true;
								shapeFileLayer.dbfPath=shapeFilePath;								
							}
							newLayerSetList.Add(shapeFileLayer);
							
						}
					}

					ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Add(newLayerSetList);

					
				}
			}			
			
		}

		#endregion
		
		/// <summary>
		/// Method to check if the timestamp have a newer dateTime than the existing xml
		/// </summary>
		/// <param name="shpURL"></param>
		public bool checkIfUpdateNeeded(string xmlPath, string oldXmlPath)
		{	
			if(oldXmlPath=="")
				oldXmlPath=m_ShapeFileRootDirectory+"\\"+Path.GetFileName(xmlPath);

			
			if(!File.Exists(oldXmlPath))
				return true;

			XmlDocument doc=new XmlDocument();
			doc.Load(xmlPath);  
            
			string year=doc.SelectSingleNode("/LayerSet/TimeStamp/year").InnerText;
			string month=doc.SelectSingleNode("/LayerSet/TimeStamp/month").InnerText;
			string day=doc.SelectSingleNode("/LayerSet/TimeStamp/day").InnerText;
			string hour=doc.SelectSingleNode("/LayerSet/TimeStamp/hour").InnerText;
			string minute=doc.SelectSingleNode("/LayerSet/TimeStamp/minute").InnerText;			

			DateTime date=new DateTime(
				int.Parse(year),
				int.Parse(month),
				int.Parse(day),
				int.Parse(hour),
				int.Parse(minute),
				0);  //no seconds...

			XmlDocument doc2=new XmlDocument();
			doc2.Load(oldXmlPath);
        
			string year2=doc2.SelectSingleNode("/LayerSet/TimeStamp/year").InnerText;
			string month2=doc2.SelectSingleNode("/LayerSet/TimeStamp/month").InnerText;
			string day2=doc2.SelectSingleNode("/LayerSet/TimeStamp/day").InnerText;
			string hour2=doc2.SelectSingleNode("/LayerSet/TimeStamp/hour").InnerText;
			string minute2=doc2.SelectSingleNode("/LayerSet/TimeStamp/minute").InnerText;			

			DateTime date2=new DateTime(
				int.Parse(year2),
				int.Parse(month2),
				int.Parse(day2),
				int.Parse(hour2),
				int.Parse(minute2),
				0);  //no seconds...

			if(date.Ticks<date2.Ticks)//local file is newer
			{
				return false;
			}			
			else// remote file is newer or the same
			{
				return true;
			}
			
		}
	

		private static bool ParseBool(string booleanString)
		{
			if(booleanString == null || booleanString.Trim().Length == 0)
			{
				return false;
			}

			booleanString = booleanString.Trim().ToLower();

			if(booleanString == "1")
				return true;
			else if(booleanString == "0")
				return false;
			else if(booleanString == "t")
				return true;
			else if(booleanString == "f")
				return false;
			else
				return bool.Parse(booleanString);

		}

		public static double ParseDouble(string s)
		{
			return double.Parse(s, CultureInfo.InvariantCulture);
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

		static System.Drawing.Color getColorFromXPathIter(XPathNodeIterator iter)
		{
			iter.MoveNext();
			byte r = 0;
			byte g = 0;
			byte b = 0;
			byte a = 255;

			string redString = getInnerTextFromFirstChild(iter.Current.Select("Red"));
			string greenString = getInnerTextFromFirstChild(iter.Current.Select("Green"));
			string blueString = getInnerTextFromFirstChild(iter.Current.Select("Blue"));
			string alphaString = getInnerTextFromFirstChild(iter.Current.Select("Alpha"));

			r = byte.Parse(redString);
			g = byte.Parse(greenString);
			b = byte.Parse(blueString);
			if(alphaString != null)
			{
				a = byte.Parse(alphaString);
			}

			return System.Drawing.Color.FromArgb(a, r, g, b);
		}

		static string[] getStringValues(XPathNodeIterator iter)
		{
			if(iter.Count > 0)
			{
				string[] returnStrings = new string[iter.Count];
				for(int i = 0; i < iter.Count; i++)
				{
					iter.MoveNext();
					returnStrings[i] = iter.Current.Value;
				}
				return returnStrings;
			}
			else
			{
				return null;
			}
		}

		static WorldWind.ShapeFillStyle getShapeFillStyleFromString(string shapeFillStyleString)
		{
			if(String.Compare(shapeFillStyleString, "BackwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.BackwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "Cross", true) == 0)
			{
				return ShapeFillStyle.Cross;
			}
			else if(String.Compare(shapeFillStyleString, "DarkDownwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.DarkDownwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "DarkHorizontal", true) == 0)
			{
				return ShapeFillStyle.DarkHorizontal;
			}
			else if(String.Compare(shapeFillStyleString, "DarkUpwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.DarkUpwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "DarkVertical", true) == 0)
			{
				return ShapeFillStyle.DarkVertical;
			}
			else if(String.Compare(shapeFillStyleString, "DashedDownwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.DashedDownwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "DashedHorizontal", true) == 0)
			{
				return ShapeFillStyle.DashedHorizontal;
			}
			else if(String.Compare(shapeFillStyleString, "DashedUpwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.DashedUpwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "DashedVertical", true) == 0)
			{
				return ShapeFillStyle.DashedVertical;
			}
			else if(String.Compare(shapeFillStyleString, "DiagonalBrick", true) == 0)
			{
				return ShapeFillStyle.DiagonalBrick;
			}
			else if(String.Compare(shapeFillStyleString, "DiagonalCross", true) == 0)
			{
				return ShapeFillStyle.DiagonalCross;
			}
			else if(String.Compare(shapeFillStyleString, "Divot", true) == 0)
			{
				return ShapeFillStyle.Divot;
			}
			else if(String.Compare(shapeFillStyleString, "DottedDiamond", true) == 0)
			{
				return ShapeFillStyle.DottedDiamond;
			}
			else if(String.Compare(shapeFillStyleString, "DottedGrid", true) == 0)
			{
				return ShapeFillStyle.DottedGrid;
			}
			else if(String.Compare(shapeFillStyleString, "ForwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.ForwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "Horizontal", true) == 0)
			{
				return ShapeFillStyle.Horizontal;
			}
			else if(String.Compare(shapeFillStyleString, "LargeCheckerBoard", true) == 0)
			{
				return ShapeFillStyle.LargeCheckerBoard;
			}
			else if(String.Compare(shapeFillStyleString, "LargeConfetti", true) == 0)
			{
				return ShapeFillStyle.LargeConfetti;
			}
			else if(String.Compare(shapeFillStyleString, "LargeGrid", true) == 0)
			{
				return ShapeFillStyle.LargeGrid;
			}
			else if(String.Compare(shapeFillStyleString, "LightDownwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.LightDownwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "LightHorizontal", true) == 0)
			{
				return ShapeFillStyle.LightHorizontal;
			}
			else if(String.Compare(shapeFillStyleString, "LightUpwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.LightUpwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "LightVertical", true) == 0)
			{
				return ShapeFillStyle.LightVertical;
			}
			else if(String.Compare(shapeFillStyleString, "Max", true) == 0)
			{
				return ShapeFillStyle.Max;
			}
			else if(String.Compare(shapeFillStyleString, "Min", true) == 0)
			{
				return ShapeFillStyle.Min;
			}
			else if(String.Compare(shapeFillStyleString, "NarrowHorizontal", true) == 0)
			{
				return ShapeFillStyle.NarrowHorizontal;
			}
			else if(String.Compare(shapeFillStyleString, "NarrowVertical", true) == 0)
			{
				return ShapeFillStyle.NarrowVertical;
			}
			else if(String.Compare(shapeFillStyleString, "OutlinedDiamond", true) == 0)
			{
				return ShapeFillStyle.OutlinedDiamond;
			}
			else if(String.Compare(shapeFillStyleString, "Percent05", true) == 0)
			{
				return ShapeFillStyle.Percent05;
			}
			else if(String.Compare(shapeFillStyleString, "Percent10", true) == 0)
			{
				return ShapeFillStyle.Percent10;
			}
			else if(String.Compare(shapeFillStyleString, "Percent20", true) == 0)
			{
				return ShapeFillStyle.Percent20;
			}
			else if(String.Compare(shapeFillStyleString, "Percent25", true) == 0)
			{
				return ShapeFillStyle.Percent25;
			}
			else if(String.Compare(shapeFillStyleString, "Percent30", true) == 0)
			{
				return ShapeFillStyle.Percent30;
			}
			else if(String.Compare(shapeFillStyleString, "Percent40", true) == 0)
			{
				return ShapeFillStyle.Percent40;
			}
			else if(String.Compare(shapeFillStyleString, "Percent50", true) == 0)
			{
				return ShapeFillStyle.Percent50;
			}
			else if(String.Compare(shapeFillStyleString, "Percent60", true) == 0)
			{
				return ShapeFillStyle.Percent60;
			}
			else if(String.Compare(shapeFillStyleString, "Percent70", true) == 0)
			{
				return ShapeFillStyle.Percent70;
			}
			else if(String.Compare(shapeFillStyleString, "Percent75", true) == 0)
			{
				return ShapeFillStyle.Percent75;
			}
			else if(String.Compare(shapeFillStyleString, "Percent80", true) == 0)
			{
				return ShapeFillStyle.Percent80;
			}
			else if(String.Compare(shapeFillStyleString, "Percent90", true) == 0)
			{
				return ShapeFillStyle.Percent90;
			}
			else if(String.Compare(shapeFillStyleString, "Plaid", true) == 0)
			{
				return ShapeFillStyle.Plaid;
			}
			else if(String.Compare(shapeFillStyleString, "Shingle", true) == 0)
			{
				return ShapeFillStyle.Shingle;
			}
			else if(String.Compare(shapeFillStyleString, "SmallCheckerBoard", true) == 0)
			{
				return ShapeFillStyle.SmallCheckerBoard;
			}
			else if(String.Compare(shapeFillStyleString, "SmallConfetti", true) == 0)
			{
				return ShapeFillStyle.SmallConfetti;
			}
			else if(String.Compare(shapeFillStyleString, "SmallGrid", true) == 0)
			{
				return ShapeFillStyle.SmallGrid;
			}
			else if(String.Compare(shapeFillStyleString, "SolidDiamond", true) == 0)
			{
				return ShapeFillStyle.SolidDiamond;
			}
			else if(String.Compare(shapeFillStyleString, "Sphere", true) == 0)
			{
				return ShapeFillStyle.Sphere;
			}
			else if(String.Compare(shapeFillStyleString, "Trellis", true) == 0)
			{
				return ShapeFillStyle.Trellis;
			}
			else if(String.Compare(shapeFillStyleString, "Wave", true) == 0)
			{
				return ShapeFillStyle.Wave;
			}
			else if(String.Compare(shapeFillStyleString, "Weave", true) == 0)
			{
				return ShapeFillStyle.Weave;
			}
			else if(String.Compare(shapeFillStyleString, "WideDownwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.WideDownwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "WideUpwardDiagonal", true) == 0)
			{
				return ShapeFillStyle.WideUpwardDiagonal;
			}
			else if(String.Compare(shapeFillStyleString, "ZigZag", true) == 0)
			{
				return ShapeFillStyle.ZigZag;
			}
			else
			{
				return ShapeFillStyle.Solid;
			}
		}

		void menuItemClicked(object sender, EventArgs e)
		{
			if(m_Gui.Visible)
			{
				m_Gui.Visible = false;
				m_MenuItem.Checked = false;
			}
			else
			{
				
				m_Gui.Visible = true;
				m_MenuItem.Checked = true;
			}
		}
	}
}
