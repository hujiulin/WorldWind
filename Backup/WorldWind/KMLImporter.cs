//----------------------------------------------------------------------------
// NAME: KMLImporter
// DESCRIPTION: KMLImporter allows you to import Placemarks from KML and KMZ files
// DEVELOPER: ShockFire
// WEBSITE: http://shockfire.blogger.com
// VERSION: 1.08
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;
using Utility;

namespace KMLPlugin
{
	/// <summary>
	/// Main plugin class
	/// </summary>
	public class KMLImporter : WorldWind.PluginEngine.Plugin
	{
		private const string version = "1.08";				// The version of this plugin

		private const int IconSizeConstant = 32;			// The default icon size used for scaling

		private RIcons KMLIcons;							// Main Icon container

		private MenuItem tempMenu = new MenuItem();			// Temp menu item for storing file MenuItems
		private MenuItem aboutMenuItem = new MenuItem();	// About menu item
		private MenuItem pluginMenuItem = new MenuItem();	// Plugin menu item (for children)
		private MenuItem napalmMenuItem = new MenuItem();	// Napalm enable/disable menu item
		private MenuItem labelMenuItem = new MenuItem();	// drawAllLabels enable/disable menu item

		private Hashtable iconStyles = new Hashtable();		// The main Style storage

		private Hashtable bitmapCache = new Hashtable();	// Hashtable to cache Bitmaps from various sources

		internal string KMLPath;							// Temp internal argument passing variable

		private ArrayList networkLinks = new ArrayList();	// Stores created NetworkLinks

		private World m_world;								// The world this KMLImporter is associated with

		static string KmlDirectory = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "kml");

		#region Plugin methods
		/// <summary>
		/// Loads this plugin. Initializes variables and adds layers and menu items
		/// </summary>
		public override void Load()
		{
			// Load settings
			Settings.LoadSettings(Path.Combine(KmlDirectory, "KMLImporter.xml"));

			// Initialize the main Icons layer
			KMLIcons = new RIcons("KML Icons");
			KMLIcons.IsOn = false;

			// Setup Drag&Drop functionality
			m_Application.WorldWindow.DragEnter += new DragEventHandler(WorldWindow_DragEnter);
			m_Application.WorldWindow.DragDrop += new DragEventHandler(WorldWindow_DragDrop);

			// Add a menu item to the File menu and the Help menu
			MenuItem loadMenuItem = new MenuItem();
			loadMenuItem.Text = "Import KML/KMZ file...";
			loadMenuItem.Click += new EventHandler(loadMenu_Click);
			aboutMenuItem.Text = "About KMLImporter";
			aboutMenuItem.Click += new EventHandler(aboutMenu_Click);
			int mergeOrder = 0;
			foreach (MenuItem menuItem in m_Application.MainMenu.MenuItems)
			{
				if (menuItem.Text.Replace("&", "") == "File")
				{
					foreach (MenuItem subMenuItem in menuItem.MenuItems)
					{
						subMenuItem.MergeOrder = mergeOrder;

						if (subMenuItem.Text == "-")
							mergeOrder = 2;		// Everything after this should come after our new items
					}

					tempMenu.Text = menuItem.Text;
					tempMenu.MergeOrder = 1;	// MergeOrder 1 will have 0 before it and 2 after it
					tempMenu.MenuItems.Add(loadMenuItem);
					tempMenu.MenuItems.Add(new MenuItem("-"));
					menuItem.MergeMenu(tempMenu);
				}

				if (menuItem.Text.Replace("&", "") == "Help")
					menuItem.MenuItems.Add(aboutMenuItem);
			}

			// Napalm enable/disable menu item
			bool bEnabled = Napalm.NapalmIsEnabled(KmlDirectory);
			if (bEnabled)
				napalmMenuItem.Text = "Disable KMLImporter autoupdate";
			else
				napalmMenuItem.Text = "Enable KMLImporter autoupdate";
			napalmMenuItem.Click += new EventHandler(napalmMenu_Click);
			pluginMenuItem.MenuItems.Add(napalmMenuItem);

			// Allways show labels enable/disable menu item
			labelMenuItem.Text = "Show all labels";
			labelMenuItem.Checked = Settings.ShowAllLabels;
			labelMenuItem.Click += new EventHandler(labelMenuItem_Click);
			pluginMenuItem.MenuItems.Add(labelMenuItem);

			// Add a menu item to the Plugins menu
			pluginMenuItem.Text = "KMLImporter";
			m_Application.PluginsMenu.MenuItems.Add(pluginMenuItem);

			// Some magic to provide backward compability
			Type typecontroller = typeof(MainApplication);
			System.Reflection.PropertyInfo finfo = typecontroller.GetProperty("CmdArgs", BindingFlags.Static|BindingFlags.Public|BindingFlags.GetProperty);
			string[] temp = null;
			if(finfo != null)
			{
				temp = (string[])finfo.GetValue(null, null);

				// If command line arguments are available, try to find one pointing to a kml/kmz file
				if (temp != null)
				{
					foreach (string arg in temp)
					{
						if (!File.Exists(arg))
							continue;

						string fExt = Path.GetExtension(arg);

						if (fExt != ".kml" && fExt != ".kmz")
							continue;

						LoadDiskKM(arg);
						break;
					}
				}
			}

			// Add the main Icons layer to the globe
			m_Application.WorldWindow.CurrentWorld.RenderableObjects.Add(KMLIcons);

			//Set the currentworld
			m_world = m_Application.WorldWindow.CurrentWorld;

			base.Load();
		}

		/// <summary>
		/// Unloads this plugin. Removes layers and menu items
		/// </summary>
		public override void Unload()
		{
			// Cleanup
			Cleanup();

			// Save settings
			Settings.SaveSettings(Path.Combine(KmlDirectory, "KMLImporter.xml"));

			// Remove the icon layer
			m_Application.WorldWindow.CurrentWorld.RenderableObjects.Remove(KMLIcons);

			// Disable Drag&Drop functionality
			this.Application.WorldWindow.DragEnter -= new DragEventHandler(WorldWindow_DragEnter);
			this.Application.WorldWindow.DragDrop -= new DragEventHandler(WorldWindow_DragDrop);

			// Remove the menu items
			foreach (MenuItem menuItem in m_Application.MainMenu.MenuItems)
			{
				if (menuItem.Text.Replace("&", "") == "File")
				{
					foreach (MenuItem subMenuItem in menuItem.MenuItems)
					{
						if (subMenuItem.Text == tempMenu.MenuItems[0].Text)
						{
							menuItem.MenuItems.RemoveAt(subMenuItem.Index+1);
							menuItem.MenuItems.RemoveAt(subMenuItem.Index);
							break;
						}
					}
				}

				if (menuItem.Text.Replace("&", "") == "Help")
					menuItem.MenuItems.Remove(aboutMenuItem);
			}
			tempMenu.MenuItems.Clear();
			//m_Application.PluginsMenu.MenuItems.Remove(napalmMenuItem);
            m_Application.PluginsMenu.MenuItems.Remove(pluginMenuItem);
			
			try
			{
				// Delete the temp kmz extract directory
				if (Directory.Exists(Path.Combine(KmlDirectory, "kmz")))
					Directory.Delete(Path.Combine(KmlDirectory, "kmz"), true);

				foreach (string kmlfile in Directory.GetFiles(KmlDirectory, "*.kml"))
				{
					try
					{
						File.Delete(kmlfile);
					}
					catch (System.IO.IOException)
					{	}
				}

				foreach (string kmzfile in Directory.GetFiles(KmlDirectory, "*.kmz"))
				{
					try
					{
						File.Delete(kmzfile);
					}
					catch (System.IO.IOException)
					{	}
				}
			}
			catch (Exception) {}

			base.Unload();
		}

		#endregion

		#region KMx loading methods
		/// <summary>
		/// Loads either a KML or KMZ file from disk
		/// </summary>
		/// <param name="filename"></param>
		private void LoadDiskKM(string filename)
		{
			if (Path.GetExtension(filename) == ".kmz")
			{
				bool shouldReturn;
				string ExtractedKMLPath = ExtractKMZ(filename, out shouldReturn);
				if (shouldReturn)
					return;
				Spawn_LoadKML(ExtractedKMLPath);
			}
			else
			{
				Spawn_LoadKML(filename);
			}
			KMLIcons.IsOn = true;
		}

		/// <summary>
		/// Loads a KML file in a new thread
		/// </summary>
		/// <param name="path">The path to the KML file to load</param>
		private void Spawn_LoadKML(string path)
		{
			KMLPath = path;

			ThreadStart threadStart = new ThreadStart(LoadKMLFile);
			Thread kmlThread = new System.Threading.Thread(threadStart);
			kmlThread.Name = "KMLImporter worker thread";
			kmlThread.IsBackground = true;
			kmlThread.Start();

			Napalm.Update(KmlDirectory, version);
		}

		/// <summary>
		/// Loads a KML file
		/// </summary>
		private void LoadKMLFile()
		{
			if (KMLPath == null || KMLIcons == null)
				return;

			Cleanup();

			WaitMessage waitMessage = new WaitMessage();
			KMLIcons.ChildObjects.Add(waitMessage);

			// Create a reader to read the file
			try
			{
				System.IO.StreamReader sr = new StreamReader(KMLPath);

				// Read all data from the reader
				string kml = sr.ReadToEnd();

				try
				{
					// Load the actual kml data
					LoadKML(kml, KMLIcons);
				}
				catch (Exception ex)
				{
					Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString());
					MessageBox.Show(
						String.Format(CultureInfo.InvariantCulture, "Error loading KML file '{0}':\n\n{1}", KMLPath, ex.ToString()), 
						"KMLImporter error", 
						MessageBoxButtons.OK,
						MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1,
						base.Application.RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading : MessageBoxOptions.ServiceNotification);
				}

				// Close the reader
				sr.Close();
			}
			catch(Exception ex) // Catch error if stream reader failed
			{
				Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString());
				MessageBox.Show(
					String.Format(CultureInfo.InvariantCulture, "Error opening KML file '{0}':\n\n{1}", KMLPath, ex.ToString()), 
					"KMLImporter error", 
					MessageBoxButtons.OK,
					MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1,
					base.Application.RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading : MessageBoxOptions.ServiceNotification);
			}

			// Cleanup
			KMLIcons.ChildObjects.Remove(waitMessage);
			KMLPath = null;				
		}

		/// <summary>
		/// Load given KML data
		/// </summary>
		/// <param name="kml">The KML data to load</param>
		internal void LoadKML(string kml, RIcons layer)
		{
			kml = kml.Replace("xmlns=\"http://earth.google.com/kml/2.0\"", "");	// HACK
			kml = kml.Replace("xmlns='http://earth.google.com/kml/2.0'", "");	// DOUBLE HACK
			kml = kml.Replace("xmlns=\"http://earth.google.com/kml/2.1\"", "");	// MULTI HACK!
			kml = kml.Replace("xmlns='http://earth.google.com/kml/2.1'", "");	// M-M-M-M-M-M-M-MONSTER HACK!!!!

			// Open the downloaded xml in an XmlDocument to allow for XPath searching
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(kml);

			// Try to find some sort of name for this kml from various places
			XmlNode node = doc.SelectSingleNode("//Document[name]/name");
		
			if(layer.Name == null || layer.Name.Length == 0 || layer.Name.Equals("KML Icons"))
			{
				if (node != null)
					layer.Name = node.InnerText;
			}
			// Parse Style and StyleMap nodes and store them
			ParseStyles(doc, KMLPath);

			// Load Placemarks recursively and put them in folders
			XmlNode inNode = doc.SelectSingleNode("/kml/Document");
			if (inNode == null)
				inNode = doc.SelectSingleNode("/kml");
	///		if(inNode == null)
	//			inNode = doc.SelectSingleNode("/Document");
			if(inNode != null)
				ParseRenderables(inNode, layer, KMLPath);
		}
		#endregion

		#region Parsing methods
		/// <summary>
		/// Parses Styles and StyleMaps and stores them
		/// </summary>
		/// <param name="doc">The document to load styles from</param>
		/// <param name="KmlPath">The path to the KML file that is being loaded</param>
		private void ParseStyles(XmlDocument doc, string KmlPath)
		{
			// Load IconStyle elements and extract the images
			XmlNodeList styles = doc.SelectNodes("//Style[@id]");
			foreach (XmlNode xstyle in styles)
			{
				string name = xstyle.Attributes.GetNamedItem("id").InnerText;
				if (iconStyles.ContainsKey(name))
					continue;

				Style style = GetStyle(xstyle, new Style(), KmlPath);
				if (style != null)
					iconStyles.Add(name, style);
				
			}

			// Load StyleMaps and extract the images linked
			XmlNodeList stylemaps = doc.SelectNodes("//StyleMap[@id]");
			foreach (XmlNode stylemap in stylemaps)
			{
				string name = stylemap.Attributes.GetNamedItem("id").InnerText;
				if (iconStyles.ContainsKey(name))
					continue;

				System.Xml.XmlNode stylemapNode = stylemap.SelectSingleNode("Pair[key=\"normal\"]/styleUrl");
				if (stylemapNode == null)
					continue;

				string normalName = stylemapNode.InnerText.Replace("#", "");
				XmlNode normalNode = doc.SelectSingleNode("//Style[@id='"+normalName+"']");

				Style style = GetStyle(normalNode, new Style(), KmlPath);
				if (style != null)
					iconStyles.Add(name, style);
			}

		}

		/// <summary>
		/// Parses everything that is not a style
		/// </summary>
		/// <param name="inNode">The node containing renderables</param>
		/// <param name="layer">The layer to add the resulting renderables to</param>
		/// <param name="KmlPath">The path to the KML file that is being loaded</param>
		private void ParseRenderables(XmlNode inNode, RIcons layer, string KmlPath)
		{
			// Extract and set layer visibility for the current layer
			XmlNode visible = inNode.SelectSingleNode("visibility");
			if (visible != null)
			{
				if (visible.InnerText == "0")
					layer.IsOn = false;
			}

			// Parse all Folders
			ParseFolders(inNode, layer, KmlPath);

			// Parse NetworkLinks
			ParseNetworkLinks(inNode, layer);

			// Parse GroundOverlays
			ParseGroundOverlays(inNode, layer);

			//Parse ScreenOverlays
			ParseScreenOverlays(inNode, layer);

			// Parse Placemarks
			ParsePlacemarks(inNode, layer, KmlPath);

			// Parse LineStrings
			ParseLineStrings(inNode, layer);

			// Parse Polygons
			ParsePolygons(inNode, layer);

			// Parse MultiGeometry
			ParseMultiGeometry(inNode, layer);

			// Update metadata for this layer
			layer.MetaData["Child count"] = layer.ChildObjects.Count;
		}

		/// <summary>
		/// Locates Folders and parses them recursively
		/// </summary>
		/// <param name="inNode">The XmlNode to extract the Folders from</param>
		/// <param name="layer">The layer to add the folders to</param>
		/// <param name="KmlPath">The path to the KML file that is being loaded</param>
		private void ParseFolders(XmlNode inNode, RIcons layer, string KmlPath)
		{
			// Find Folders and initialize them recursively
			XmlNodeList folders = inNode.SelectNodes("Folder");
			foreach (XmlNode node in folders)
			{
				try
				{
					// Find the name of the folder
					string foldername = "Folder";
					XmlNode nameNode = node.SelectSingleNode("name");
					if (nameNode != null)
						foldername = nameNode.InnerText;

					// See if the folder already exists
					RIcons folder = null;
					foreach (RenderableObject ro in layer.ChildObjects)
					{
						RIcons ricons = ro as RIcons;
						if ((ricons != null) && (ro.Name == foldername))
						{
							folder = ricons;
						}
					}

					// Create a new folder if it doesn't exist yet
					if (folder == null)
					{
						folder = new RIcons(foldername);
						layer.Add(folder);
					}

					XmlNode visibilityNode = node.SelectSingleNode("visibility");
					if(visibilityNode != null)
						layer.IsOn = (visibilityNode.InnerText == "1" ? true : false);

					// Parse placemarks into the folder
					ParseRenderables(node, folder, KmlPath);
				}
				catch (Exception ex)
                { Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString()); }
			}
		}

		/// <summary>
		/// Parse NetworkLinks
		/// </summary>
		/// <param name="inNode">The XmlNode to load NetworkLinks from</param>
		/// <param name="layer">The layer to add NetworkLinks to</param>
		private void ParseNetworkLinks(XmlNode inNode, RIcons layer)
		{
			// Find NetworkLinks, initialize them and download them for the first time
			XmlNodeList networklinks = inNode.SelectNodes("NetworkLink");
			foreach (XmlNode node in networklinks)
			{
				try
				{
					// Find out the name for this NetworkLink
					string nlName = "NetworkLink";
					XmlNode nameNode = node.SelectSingleNode("name");
					if (nameNode != null)
						nlName = nameNode.InnerText;

					// See if a folder for this NetworkLink already exists
					RIcons folder = null;
					foreach (RenderableObject ro in layer.ChildObjects)
					{
						RIcons ricons = ro as RIcons;
						if ((ricons != null) && (ro.Name == nlName))
						{
							folder = ricons;
						}
					}

					// Create a new folder if none is available
					if (folder == null)
					{
						folder = new RIcons(nlName);
						layer.Add(folder);
					}

					XmlNode visibilityNode = node.SelectSingleNode("visibility");
					if(visibilityNode != null)
						folder.IsOn = (visibilityNode.InnerText == "1" ? true : false);

					// Find the URL to download the file from
					string loadFile = null;
					XmlNode hrefNode = node.SelectSingleNode("Url/href");
					if ((hrefNode != null) && (hrefNode.InnerText.Length > 0))
						loadFile = hrefNode.InnerText;

					// Give up if no URL can be found
					if (loadFile == null)
						continue;

					int tickSeconds = -1;
					int viewSeconds = -1;

					bool fired = false;
					if (node.SelectSingleNode("Url/refreshMode") != null)
					{
						if (node.SelectSingleNode("Url/refreshMode").InnerText == "onInterval")
						{
							string refreshText = node.SelectSingleNode("Url/refreshInterval").InnerText;
							tickSeconds = Convert.ToInt32(refreshText, CultureInfo.InvariantCulture);
						}
						if (node.SelectSingleNode("Url/refreshMode").InnerText == "once")
						{
							NetworkLink netLink = new NetworkLink(this, folder, loadFile, -1, -1);
							netLink.Fire();
							netLink.Dispose();

							fired = true;
						}
					}

					if ((node.SelectSingleNode("Url/viewRefreshMode") != null) && (node.SelectSingleNode("Url/viewRefreshMode").InnerText == "onStop"))
					{
						string refreshText = node.SelectSingleNode("Url/viewRefreshTime").InnerText;
						viewSeconds = Convert.ToInt32(refreshText, CultureInfo.InvariantCulture);						
					}

					// Initialize the actual NetworkLink object to handle updates for us
					if (tickSeconds != -1 || viewSeconds != -1)
					{
						NetworkLink netLink = new NetworkLink(this, folder, loadFile, tickSeconds*1000, viewSeconds*1000);
						netLink.Fire();
						networkLinks.Add(netLink);
					}
					else if(!fired)
					{
						NetworkLink netLink = new NetworkLink(this, folder, loadFile, -1, -1);
						netLink.Fire();
						netLink.Dispose();
					}
				}
				catch (Exception ex)
                { Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString()); }
			}
		}

		/// <summary>
		/// Parses Placemarks
		/// </summary>
		/// <param name="inNode">The node containing Placemarks</param>
		/// <param name="layer">The layer to add the resulting icons or folders to</param>
		/// <param name="KmlPath">The path to the KML file that is being loaded</param>
		private void ParsePlacemarks(XmlNode inNode, RIcons layer, string KmlPath)
		{
			foreach (WorldWind.Renderable.RenderableObject ro in layer.ChildObjects)
			{
				RIcon ricon = ro as RIcon;
				if (ricon != null)
					ricon.HasBeenUpdated = false;
			}

			// Parse all Placemarks that have a name and location
			XmlNodeList placemarks = inNode.SelectNodes("Placemark[name and Point]");
			foreach (XmlNode node in placemarks)
			{
				try
				{
					string name = node.SelectSingleNode("name").InnerText;
					RIcon update = null;

					// Extract the location string for this Placemark node and split it up
					string loc = node.SelectSingleNode("Point/coordinates").InnerText.Trim();

					LLA lla = ParseCoordinate(loc);

					string desc = null;
					string uri = null;

					// Extract a description and make sure it's not too long
					XmlNode xnode = node.SelectSingleNode("description");
					if (xnode != null)
					{
						string descRaw = xnode.InnerText;
						uri = SearchUri(descRaw);

                        // Ashish Datta - commented so that the HTML stays in the description property.
                        desc = descRaw;

						if (desc.Length > 505)
							desc = desc.Substring(0, 500) + "...";
					}

					float rotation = 0;
					bool bRotated = false;
					string rotRaw = null;

					// Locate a node containing rotation 
					XmlNode rotNode1 = node.SelectSingleNode("Point/rotation");
					if (rotNode1 != null)
						rotRaw = rotNode1.InnerText;
					else 
					{
						XmlNode rotNode2 = node.SelectSingleNode("IconStyle/heading");
						if (rotNode2 != null)
							rotRaw = rotNode2.InnerText;
						else
						{
							XmlNode rotNode3 = node.SelectSingleNode("Style/IconStyle/heading");
							if (rotNode3 != null)
								rotRaw = rotNode3.InnerText;
						}
					}

					// If rotation was found parse it
					if (rotRaw != null)
					{
						rotation = Convert.ToSingle(rotRaw, CultureInfo.InvariantCulture);
						bRotated = true;
					}

					// Find a style for this icon
					Style style = LocateStyle(node, KmlPath);

					// Check if this icon has to be extruded
					bool bExtrude = false;
					XmlNode extrudeNode = node.SelectSingleNode("Point/extrude");
					if (extrudeNode != null)
					{
						if (extrudeNode.InnerText == "1")
							bExtrude = true;
					}

					// See if this icon already exists, and store it if it does
					foreach (WorldWind.Renderable.RenderableObject ro in layer.ChildObjects)
					{
						RIcon ricon = ro as RIcon;
						if (ricon != null)
						{
							if ((ro.Name == name) && ((style == null) || ((ricon.NormalIcon == style.NormalIcon) && (!ricon.HasBeenUpdated))))
							{
								update = ricon;
								update.HasBeenUpdated = true;
								
								break;
							}
						}
					}

					// If a previous icons has been found update it's location
					if (update != null)
					{
						update.IsRotated = bRotated;
						if (bRotated)
						{
							update.Rotation = Angle.FromDegrees(rotation);
						}
                        if (style != null)
                        {
                            update.Height = Double.IsNaN(style.NormalScale) ? IconSizeConstant : (int)(style.NormalScale * Math.Min(((Bitmap)bitmapCache[style.NormalIcon]).Height, IconSizeConstant));
                            update.Width = Double.IsNaN(style.NormalScale) ? IconSizeConstant : (int)(style.NormalScale * Math.Min(((Bitmap)bitmapCache[style.NormalIcon]).Width, IconSizeConstant));
                            update.Description = desc;
                            update.SetPosition(lla.lat, lla.lon, lla.alt);
                        }
					}
					else
					{
						// Create the icon with either the generated bitmap or the default dot
						if (style != null)
						{
							CreateIcon(layer, name, desc, uri, lla.lat, lla.lon, lla.alt, style, bRotated, rotation, bExtrude);
						}
						else
						{
							// Use the default 'tack' icon if no style was found
							string pal3Path = Path.Combine(KmlDirectory, "icons/palette-3.png");
							if (File.Exists(pal3Path))
							{
								if (!bitmapCache.Contains(pal3Path))
									bitmapCache.Add(pal3Path, (Bitmap)Bitmap.FromFile(pal3Path));
								Style pinStyle = new Style(GetSubImage(new Style(pal3Path), 448, 64, 64, 64));

								CreateIcon(layer, name, desc, uri, lla.lat, lla.lon, lla.alt, pinStyle, bRotated, rotation, bExtrude);
							}
						}
					}
				}
				catch (Exception ex)
                { Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString()); }
			}

			// Cleanup icons that have not been updated
			RemoveUnusedIcons(layer);
		}

		/// <summary>
		/// Parse Ground Overlays
		/// </summary>
		/// <param name="inNode">The node containing Ground Overlays</param>
		/// <param name="layer">The layer to add the resulting Ground Overlays to</param>
		private void ParseGroundOverlays(XmlNode inNode, RIcons layer)
		{
			// Parse all Placemarks that have a name and LineString
			XmlNodeList groundOverlays = inNode.SelectNodes("GroundOverlay[name and LatLonBox]");
			foreach (XmlNode node in groundOverlays)
			{
				// Extract the name from this node
				XmlNode nameNode = node.SelectSingleNode("name");
				string name = nameNode.InnerText;
				
				XmlNode latLonBoxNode = node.SelectSingleNode("LatLonBox");
				//Parse Coordinates
				if(latLonBoxNode != null)
				{
					XmlNode northNode = latLonBoxNode.SelectSingleNode("north");
					XmlNode southNode = latLonBoxNode.SelectSingleNode("south");
					XmlNode westNode = latLonBoxNode.SelectSingleNode("west");
					XmlNode eastNode = latLonBoxNode.SelectSingleNode("east");

					double north = ConfigurationLoader.ParseDouble(northNode.InnerText);
					double south = ConfigurationLoader.ParseDouble(southNode.InnerText);
					double west = ConfigurationLoader.ParseDouble(westNode.InnerText);
					double east = ConfigurationLoader.ParseDouble(eastNode.InnerText);
					
					// Create GroundOverlay

					WorldWind.Renderable.ImageLayer imageLayer = new ImageLayer(
						name,
						ParentApplication.WorldWindow.CurrentWorld,
						0,
						null,
						south,
						north,
						west,
						east,
						1.0,
						ParentApplication.WorldWindow.CurrentWorld.TerrainAccessor
						);

					imageLayer.DisableZBuffer = true;
					imageLayer.ImageUrl = node.SelectSingleNode("Icon/href").InnerText;
					
					XmlNode visibilityNode = node.SelectSingleNode("visibility");
					if(visibilityNode != null)
						imageLayer.IsOn = (visibilityNode.InnerText == "1" ? true : false);

					layer.Add(imageLayer);
				}
			}
		}

		/// <summary>
		/// This Method parses screen overlays and adds to renderables
		///  using ScreenOverlay Object
		/// </summary>
		/// <param name="inNode">The node containing the Screen Overlay</param>
		/// <param name="layer">The layer to add the resulting Screen Overlay to</param>
		private void ParseScreenOverlays(XmlNode inNode, RIcons layer)
		{
			XmlNodeList screenOverlays = inNode.SelectNodes("ScreenOverlay");
			if(screenOverlays!=null)
			{
				foreach(XmlNode screenOverlayNode in screenOverlays)
				{
					XmlNode nameNode =  screenOverlayNode.SelectSingleNode("name");
					String name = "";
					if(nameNode != null)
						name = nameNode.InnerText;

                    
                    XmlNode uriNode = screenOverlayNode.SelectSingleNode("Icon/href");
					String uri = "http://www.apogee.com.au/logo_topleft.gif";
                    if (uriNode != null)
                    {
                        uri = uriNode.InnerText;
                    }

                    float posX = 0;
                    float posY = 0;
                    ScreenUnits posXUnits = ScreenUnits.Pixels;
                    ScreenUnits posYUnits = ScreenUnits.Pixels;

                    XmlNode positionNode = screenOverlayNode.SelectSingleNode("screenXY");
                    if (positionNode != null)
                    {
                        if (positionNode.Attributes["x"] != null)
                        {
                            posX = float.Parse(positionNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture);
                            
                            if (positionNode.Attributes["xunits"].InnerText.ToLower() == "fraction")
                            {
                                posXUnits = ScreenUnits.Fraction;
                            }
                        }

                        if (positionNode.Attributes["y"] != null)
                        {
                            posY = float.Parse(positionNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture);
                            
                            if (positionNode.Attributes["yunits"].InnerText.ToLower() == "fraction")
                            {
                                posYUnits = ScreenUnits.Fraction;
                            }
                        }
                    }

                    ScreenOverlay scoverlay = new ScreenOverlay(name, posX, posY, uri);
                    scoverlay.PositionXUnits = posXUnits;
                    scoverlay.PositionYUnits = posYUnits;
                    scoverlay.ShowHeader = false;

                    XmlNode sizeNode = screenOverlayNode.SelectSingleNode("size");
                    if (sizeNode != null)
                    {
                        if (sizeNode.Attributes["x"] != null)
                        {
                            scoverlay.Width = float.Parse(sizeNode.Attributes["x"].InnerText, CultureInfo.InvariantCulture);
                            
                            if(sizeNode.Attributes["xunits"].InnerText.ToLower() == "fraction")
                            {
                                scoverlay.SizeXUnits = ScreenUnits.Fraction;
                            }
                        }

                        if (sizeNode.Attributes["y"] != null)
                        {
                            scoverlay.Height = float.Parse(sizeNode.Attributes["y"].InnerText, CultureInfo.InvariantCulture);
                             
                            if (sizeNode.Attributes["yunits"].InnerText.ToLower() == "fraction")
                            {
                                scoverlay.SizeYUnits = ScreenUnits.Fraction;
                            }
                        }
                    }
					
                    layer.Add(scoverlay);
				}
			}
		}

		/// <summary>
		/// Parses LineStrings
		/// </summary>
		/// <param name="inNode">The node containing LineStrings</param>
		/// <param name="layer">The layer to add the resulting lines to</param>
		/// <param name="KmlPath">The path to the KML file that is being loaded</param>
		private void ParseLineStrings(XmlNode inNode, RIcons layer)
		{
			// Parse all Placemarks that have a name and LineString
			XmlNodeList lineStrings = inNode.SelectNodes("Placemark[name and LineString]");
			foreach (XmlNode node in lineStrings)
			{
				// Extract the name from this node
				XmlNode nameNode = node.SelectSingleNode("name");
				string name = nameNode.InnerText;
				Style style = null;

				// get StyleUrl
				XmlNode styleUrlNode = node.SelectSingleNode("styleUrl");
				if(styleUrlNode != null)
				{
					string styleUrlKey = styleUrlNode.InnerText.Trim();
					if(styleUrlKey.StartsWith("#"))
						styleUrlKey = styleUrlKey.Substring(1, styleUrlKey.Length - 1);

					style = (Style)iconStyles[styleUrlKey];
				}
				else
				{
					XmlNode styleNode = node.SelectSingleNode("Style");
					if(styleNode != null)
						style = GetStyle( styleNode, new Style(), "");
				}

				if(style == null)
					style = new Style();
				
				if(style.LineStyle == null)
					style.LineStyle = new LineStyle();

				if(style.PolyStyle == null)
					style.PolyStyle = new PolyStyle();

				// See if this LineString has to be extruded to the ground
				bool extrude = false;
				XmlNode extrudeNode = node.SelectSingleNode("LineString/extrude");
				if (extrudeNode != null)
					extrude = Convert.ToBoolean(Convert.ToInt16(extrudeNode.InnerText));

				//Parse Coordinates
				XmlNode outerRingNode = node.SelectSingleNode("LineString/coordinates");
				if(outerRingNode != null)
				{
					// Parse the list of line coordinates
					Point3d[] points = ParseCoordinates(outerRingNode);
                    LineFeature line = new LineFeature(name, m_world, points, System.Drawing.Color.FromArgb(style.LineStyle.Color.Color));
					
                    XmlNode altitudeModeNode = node.SelectSingleNode("LineString/altitudeMode");
                    line.AltitudeMode = GetAltitudeMode(altitudeModeNode);
                    
                    line.LineWidth = (float)style.LineStyle.Width.Value;

					
                    if (extrude)
					{
						line.Extrude = true;

						if(style.PolyStyle.Color != null)
						{
							line.PolygonColor = System.Drawing.Color.FromArgb(style.PolyStyle.Color.Color);
						}
					}

					XmlNode visibilityNode = node.SelectSingleNode("visibility");
					if(visibilityNode != null)
						line.IsOn = (visibilityNode.InnerText == "1" ? true : false);
                    
                    layer.Add(line);
				}
			}
		}



		/// <summary>
		/// Parses Multi Polygons
		/// </summary>
		/// <param name="inNode">The node containing Polygons</param>
		/// <param name="layer">The layer to add the resulting Polygons to</param>
		private void ParseMultiGeometry(XmlNode inNode, RIcons layer)
		{
			// Parse all Placemarks that have a name and Polygon
			XmlNodeList placemarkNodes = inNode.SelectNodes("Placemark[name and MultiGeometry]");
			Random rand = new Random((int)DateTime.Now.Ticks);

			foreach(XmlNode placemarkNode in placemarkNodes)
			{
				XmlNode nameNode = placemarkNode.SelectSingleNode("name");
				string name = nameNode.InnerText;

				// change this to something that unifies the geometry into a single object instead of a user-accessible list
				RIcons multiGeometryList = new RIcons(name);

				Style style = null;

				// get StyleUrl
				XmlNode styleUrlNode = placemarkNode.SelectSingleNode("styleUrl");
				if(styleUrlNode != null)
				{
					string styleUrlKey = styleUrlNode.InnerText.Trim();
					if(styleUrlKey.StartsWith("#"))
						styleUrlKey = styleUrlKey.Substring(1, styleUrlKey.Length - 1);

					style = (Style)iconStyles[styleUrlKey];
				}
				else
				{
					XmlNode styleNode = placemarkNode.SelectSingleNode("Style");
					if(styleNode != null)
						style = GetStyle( styleNode, new Style(), "");
				}

				if(style == null)
					style = new Style();
				
				if(style.LineStyle == null)
					style.LineStyle = new LineStyle();

				if(style.PolyStyle == null)
					style.PolyStyle = new PolyStyle();

				XmlNodeList lineStringNodes = placemarkNode.SelectNodes("MultiGeometry/LineString");
				foreach(XmlNode lineStringNode in lineStringNodes)
				{
					bool extrude = false;
					XmlNode extrudeNode = lineStringNode.SelectSingleNode("extrude");
					if (extrudeNode != null)
						extrude = Convert.ToBoolean(Convert.ToInt16(extrudeNode.InnerText));

					XmlNode coordinateNode = lineStringNode.SelectSingleNode("coordinates");
					Point3d[] points = ParseCoordinates(coordinateNode);

					XmlNode altitudeModeNode = lineStringNode.SelectSingleNode("altitudeMode");
					AltitudeMode altitudeMode = GetAltitudeMode(altitudeModeNode);

					if(points != null && points.Length > 0)
					{
						LineFeature line = new LineFeature(
							name, 
							m_world, 
							points,
							System.Drawing.Color.FromArgb(style.LineStyle.Color.Color)
							);

						line.AltitudeMode = altitudeMode;
						if(style.PolyStyle.Color != null)
							line.PolygonColor = System.Drawing.Color.FromArgb(style.PolyStyle.Color.Color);
						
						line.LineWidth = (float)style.LineStyle.Width.Value;
						line.Extrude = extrude;

						multiGeometryList.Add(line);
					}
				}

				XmlNodeList polygonNodes = placemarkNode.SelectNodes("MultiGeometry/Polygon");
				foreach(XmlNode polygonNode in polygonNodes)
				{
					bool extrude = false;
					XmlNode extrudeNode = polygonNode.SelectSingleNode("extrude");
					if (extrudeNode != null)
						extrude = Convert.ToBoolean(Convert.ToInt16(extrudeNode.InnerText));

					XmlNode altitudeModeNode = polygonNode.SelectSingleNode("altitudeMode");
					AltitudeMode altitudeMode = GetAltitudeMode(altitudeModeNode);

					LinearRing outerRing = null;
					LinearRing[] innerRings = null;

					// Parse Outer Ring
					XmlNode outerRingNode = polygonNode.SelectSingleNode("outerBoundaryIs/LinearRing/coordinates");
					if (outerRingNode != null)
					{
						Point3d[] points = ParseCoordinates(outerRingNode);
						
						outerRing = new LinearRing();
						outerRing.Points = points;
					}
					
					// Parse Inner Ring
					XmlNodeList innerRingNodes = polygonNode.SelectNodes("innerBoundaryIs");
					if (innerRingNodes != null)
					{
						innerRings = new LinearRing[innerRingNodes.Count];
						for(int i = 0; i < innerRingNodes.Count; i++)
						{
							Point3d[] points = ParseCoordinates(innerRingNodes[i]);

							innerRings[i] = new LinearRing();
							innerRings[i].Points = points;
						}
					}

					if(outerRing != null)
					{
						PolygonFeature polygonFeature = new PolygonFeature(
							name, 
							m_world,
							outerRing,
							innerRings,
							(style.PolyStyle.Color != null ? System.Drawing.Color.FromArgb(style.PolyStyle.Color.Color) : System.Drawing.Color.Yellow)
							);

						polygonFeature.Extrude = extrude;
						polygonFeature.AltitudeMode = altitudeMode;
						polygonFeature.Outline = style.PolyStyle.Outline;
						if(style.LineStyle.Color != null)
							polygonFeature.OutlineColor = System.Drawing.Color.FromArgb(style.LineStyle.Color.Color);

						multiGeometryList.Add(polygonFeature);
					}
				}

				XmlNode visibilityNode = placemarkNode.SelectSingleNode("visibility");
				if(visibilityNode != null)
					multiGeometryList.IsOn = (visibilityNode.InnerText == "1" ? true : false);

				layer.Add(multiGeometryList);
			}
		}

		private AltitudeMode GetAltitudeMode(XmlNode altitudeModeNode)
		{
			if(altitudeModeNode == null || altitudeModeNode.InnerText == null || altitudeModeNode.InnerText.Length == 0)
				return AltitudeMode.ClampedToGround;

			if(altitudeModeNode != null && altitudeModeNode.InnerText.Length > 0)
			{
				switch(altitudeModeNode.InnerText)
				{
					case "clampedToGround":
						return AltitudeMode.ClampedToGround;
					case "relativeToGround":
						return AltitudeMode.RelativeToGround;
					case "absolute":
						return AltitudeMode.Absolute;
				}
			}
			
			return AltitudeMode.ClampedToGround;
		}

		/// <summary>
		/// Parses Polygons
		/// </summary>
		/// <param name="inNode">The node containing Polygons</param>
		/// <param name="layer">The layer to add the resulting Polygons to</param>
		private void ParsePolygons(XmlNode inNode, RIcons layer)
		{
			// Parse all Placemarks that have a name and Polygon
			XmlNodeList polygons = inNode.SelectNodes("Placemark[name and Polygon]");
			Random rand = new Random((int)DateTime.Now.Ticks);

			foreach (XmlNode node in polygons)
			{
				// Extract the name from this node
				XmlNode nameNode = node.SelectSingleNode("name");
				string name = nameNode.InnerText;
				
				Style style = null;

				// get StyleUrl
				XmlNode styleUrlNode = node.SelectSingleNode("styleUrl");
				if(styleUrlNode != null)
				{
					string styleUrlKey = styleUrlNode.InnerText.Trim();
					if(styleUrlKey.StartsWith("#"))
						styleUrlKey = styleUrlKey.Substring(1, styleUrlKey.Length - 1);

					style = (Style)iconStyles[styleUrlKey];
				}
				else
				{
					XmlNode styleNode = node.SelectSingleNode("Style");
					if(styleNode != null)
						style = GetStyle( styleNode, new Style(), "");
				}

				if(style == null)
					style = new Style();
				
				if(style.LineStyle == null)
					style.LineStyle = new LineStyle();

				if(style.PolyStyle == null)
					style.PolyStyle = new PolyStyle();

				// See if this LineString has to be extruded to the ground
				bool extrude = false;
				
				XmlNode extrudeNode = node.SelectSingleNode("Polygon/extrude");
				if (extrudeNode != null)
					extrude = Convert.ToBoolean(Convert.ToInt16(extrudeNode.InnerText));

				XmlNode altitudeModeNode = node.SelectSingleNode("Polygon/altitudeMode");
				AltitudeMode altitudeMode = GetAltitudeMode(altitudeModeNode);

				LinearRing outerRing = null;
				LinearRing[] innerRings = null;

				// Parse Outer Ring
				XmlNode outerRingNode = node.SelectSingleNode("Polygon/outerBoundaryIs/LinearRing/coordinates");
				if (outerRingNode != null)
				{
					Point3d[] points = ParseCoordinates(outerRingNode);
					Console.WriteLine(points.Length);

					outerRing = new LinearRing();
					outerRing.Points = points;
				}

				// Parse Inner Ring
				XmlNodeList innerRingNodes = node.SelectNodes("Polygon/innerBoundaryIs");
				if (innerRingNodes != null)
				{
					innerRings = new LinearRing[innerRingNodes.Count];
					for(int i = 0; i < innerRingNodes.Count; i++)
					{
						Point3d[] points = ParseCoordinates(innerRingNodes[i]);
						innerRings[i] = new LinearRing();
						innerRings[i].Points = points;
					}
				}

				if(outerRing != null)
				{
					PolygonFeature polygonFeature = new PolygonFeature(
						name, m_world,
						outerRing,
						innerRings,
						System.Drawing.Color.FromArgb(style.PolyStyle.Color.Color));

					polygonFeature.Extrude = extrude;
					polygonFeature.AltitudeMode = altitudeMode;
					polygonFeature.Outline = style.PolyStyle.Outline;
					if(style.LineStyle.Color != null)
						polygonFeature.OutlineColor = System.Drawing.Color.FromArgb(style.LineStyle.Color.Color);

					XmlNode visibilityNode = node.SelectSingleNode("visibility");
					if(visibilityNode != null)
						polygonFeature.IsOn = (visibilityNode.InnerText == "1" ? true : false);

					layer.Add(polygonFeature);
				}
			}
		}

		/// <summary>
		/// Parse a list of coordinates
		/// </summary>
		/// <param name="coordinatesNode">The node containing coordinates to parse</param>
		private Point3d[] ParseCoordinates(XmlNode coordinatesNode)
		{
			string coordlist = coordinatesNode.InnerText.Trim();
			char[] splitters = {'\n', ' ', '\t', ','};
			string[] lines = coordlist.Split(splitters);
			
			ArrayList tokenList = new ArrayList();
			ArrayList points = new ArrayList();
			
			int tokenCount = 0;
			for(int i = 0; i < lines.Length; i++)
			{
				string token = lines[i].Trim();
				if(token.Length == 0 || token == String.Empty)
					continue;

				tokenCount++;
				tokenList.Add(token);
				if(tokenCount == 3)
				{
					double lon = double.Parse((string)tokenList[tokenList.Count - 3], CultureInfo.InvariantCulture);
					double lat = double.Parse((string)tokenList[tokenList.Count - 2], CultureInfo.InvariantCulture);
					double alt = double.Parse((string)tokenList[tokenList.Count - 1], CultureInfo.InvariantCulture);

					points.Add(new Point3d(lon, lat, alt));
					tokenCount = 0;
				}
			}
/*
			
			for (int i = 0; i < lines.Length; i++)
			{
				string loc = lines[i].Trim();
				if (loc == String.Empty)
					continue;
				
				LLA lla = ParseCoordinate(loc);

				points.Add(new Point3d(lla.lon, lla.lat, lla.alt));
			}
*/
			return (Point3d[])points.ToArray(typeof(Point3d));
		}

		/// <summary>
		/// Parses a string containing a coordinate
		/// </summary>
		/// <param name="loc">The string containing a coordinate</param>
		/// <returns>The parsed coordinate</returns>
		private LLA ParseCoordinate(string loc)
		{
			// get rid of a leading comma from bad user input
			if(loc.StartsWith(","))
				loc = loc.Substring(1, loc.Length - 1);

			// get rid of a trailing comma from bad user input
			if(loc.EndsWith(","))
				loc = loc.Substring(0, loc.Length - 1);

			string sLon="0", sLat="0", sAlt="0";

			if (loc.Split(',').Length == 3)				// Includes altitude
			{
				sLon = loc.Substring(0, loc.IndexOf(",")).Trim();
				sLat = loc.Substring(loc.IndexOf(",") + 1, loc.LastIndexOf(",") - loc.IndexOf(",") - 1).Trim();
				sAlt = loc.Substring(loc.LastIndexOf(",") + 1, loc.Length - loc.LastIndexOf(",") - 1).Trim();
			}
			else										// Lat and Lon only (assume 0 altitude)
			{
				sLon = loc.Substring(0, loc.IndexOf(",")).Trim();
				sLat = loc.Substring(loc.LastIndexOf(",") + 1, loc.Length - loc.LastIndexOf(",") - 1).Trim();
			}

			// Convert extracted positions to numbers
			float lat = Convert.ToSingle(sLat, CultureInfo.InvariantCulture);
			float lon = Convert.ToSingle(sLon, CultureInfo.InvariantCulture);
			float alt = Convert.ToSingle(sAlt, CultureInfo.InvariantCulture);

			LLA lla = new LLA(lat, lon, alt);
			return lla;
		}
		#endregion

		#region Style handling methods
		/// <summary>
		/// Modifies a style with Style tags loaded from an XmlNode
		/// </summary>
		/// <param name="style">The XmlNode containing override information</param>
		/// <param name="oldStyle">The style to override</param>
		/// <param name="KmlPath">The path to the KML file that is being loaded</param>
		/// <returns>The style with overridden values</returns>
		private Style GetStyle(XmlNode style, Style oldStyle, string KmlPath)
		{
			try
			{
				Style overStyle = oldStyle;

				bool bPalette = false;

				// Determine the scale, if any, for this style
				XmlNode scaleNode = style.SelectSingleNode("IconStyle/scale");
				if (scaleNode != null)
					overStyle.NormalScale = Convert.ToDouble(scaleNode.InnerText, CultureInfo.InvariantCulture);

				// Search for style tags in location 1
				XmlNode hrefNode = style.SelectSingleNode("IconStyle/Icon/href");
				if (hrefNode != null)
				{
					string filename = hrefNode.InnerText;
					if (filename.StartsWith("root://"))													// Use palette bitmap
					{
						filename = Path.Combine(KmlDirectory, filename.Remove(0, 7));
						if (File.Exists(filename))
						{
							bPalette = true;
							overStyle.NormalIcon = GetDiskImage(filename);
						}
					}
					else if (filename.StartsWith("http://"))											// Use bitmap from the internet
					{
						overStyle.NormalIcon = GetWebImage(filename);
					}
					else if (File.Exists(Path.Combine(Path.GetDirectoryName(KmlPath), filename)))		// Use a file from disk
					{
						overStyle.NormalIcon = GetDiskImage(Path.Combine(Path.GetDirectoryName(KmlPath), filename));
					}
					else if (File.Exists(Path.Combine(KmlDirectory, filename)))					// Use a file from disk
					{
						overStyle.NormalIcon = GetDiskImage(Path.Combine(KmlDirectory, filename));
					}
				}

				// See if we need to cut this style to a substyle
				XmlNode wNode = style.SelectSingleNode("IconStyle/Icon/w");
				XmlNode hNode = style.SelectSingleNode("IconStyle/Icon/h");
				if (wNode != null && hNode != null)
				{
					int w = Convert.ToInt32(wNode.InnerText, CultureInfo.InvariantCulture);
					int h = Convert.ToInt32(hNode.InnerText, CultureInfo.InvariantCulture);

					int x = 0, y = 0;
					XmlNode xNode = style.SelectSingleNode("IconStyle/Icon/x");
					if (xNode != null)
						x = Convert.ToInt32(xNode.InnerText, CultureInfo.InvariantCulture);
					XmlNode yNode = style.SelectSingleNode("IconStyle/Icon/y");
					if (yNode != null)
						y = Convert.ToInt32(yNode.InnerText, CultureInfo.InvariantCulture);

					if (bPalette)
						overStyle.NormalIcon = GetSubImage(overStyle, x*2, y*2, w*2, h*2);
					else
						overStyle.NormalIcon = GetSubImage(overStyle, x, y, w, h);
				}

				// Search for style tags in a possible secondary location
				XmlNode iconNode = style.SelectSingleNode("icon");
				if (iconNode != null)
				{
					string filename = iconNode.InnerText;
					if (!filename.StartsWith("http://"))
						return null;

					overStyle.NormalIcon = GetWebImage(filename);
				}

				XmlNode balloonStyleNode = style.SelectSingleNode("BalloonStyle");
				if(balloonStyleNode != null)
				{
					BalloonStyle balloonStyle = new BalloonStyle();
						
					XmlNode balloonTextNode = balloonStyleNode.SelectSingleNode("text");
					if(balloonTextNode != null)
					{
						TextElement textElement = new TextElement();
							
						textElement.Text = balloonTextNode.InnerText;

						XmlNode textNodeColor = balloonTextNode.SelectSingleNode("textColor");
						if(textNodeColor != null)
							textElement.TextColor = new ColorElement(ParseColor(textNodeColor.InnerText));

						balloonStyle.Text = textElement;
					}

					XmlNode balloonTextColorNode = balloonStyleNode.SelectSingleNode("textColor");
					if(balloonTextColorNode != null)
						balloonStyle.TextColor = new ColorElement(ParseColor(balloonTextColorNode.InnerText));

					XmlNode balloonColorNode = balloonStyleNode.SelectSingleNode("color");
					if(balloonColorNode != null)
						balloonStyle.Color = new ColorElement(ParseColor(balloonColorNode.InnerText));

					overStyle.BalloonStyle = balloonStyle;
				}

				XmlNode iconStyleNode = style.SelectSingleNode("IconStyle");
				if(iconStyleNode != null)
				{
					XmlNode iconElementNode = iconStyleNode.SelectSingleNode("Icon");
					IconElement iconElement = new IconElement();
					
					if(iconElementNode != null)
					{
						XmlNode iconElementHrefNode = iconElementNode.SelectSingleNode("href");
						if (iconElementHrefNode != null)
						{
							string filename = iconElementHrefNode.InnerText;
							if (filename.StartsWith("root://"))													// Use palette bitmap
							{
								filename = Path.Combine(KmlDirectory, filename.Remove(0, 7));
								if (File.Exists(filename))
								{
									bPalette = true;
									iconElement.href = GetDiskImage(filename);
								}
							}
							else if (filename.StartsWith("http://"))											// Use bitmap from the internet
							{
								iconElement.href = GetWebImage(filename);
							}
							else if (File.Exists(Path.Combine(Path.GetDirectoryName(KmlPath), filename)))		// Use a file from disk
							{
								iconElement.href = GetDiskImage(Path.Combine(Path.GetDirectoryName(KmlPath), filename));
							}
							else if (File.Exists(Path.Combine(KmlDirectory, filename)))					// Use a file from disk
							{
								iconElement.href = GetDiskImage(Path.Combine(KmlDirectory, filename));
							}
						}

						// See if we need to cut this style to a substyle
						XmlNode iconElementWNode = iconElementNode.SelectSingleNode("w");
						XmlNode iconElementHNode = iconElementNode.SelectSingleNode("h");
						if (iconElementWNode != null && iconElementHNode != null)
						{
							int w = Convert.ToInt32(wNode.InnerText, CultureInfo.InvariantCulture);
							int h = Convert.ToInt32(hNode.InnerText, CultureInfo.InvariantCulture);

							int x = 0, y = 0;
							XmlNode xNode = iconElementNode.SelectSingleNode("x");
							if (xNode != null)
								x = Convert.ToInt32(xNode.InnerText, CultureInfo.InvariantCulture);
							XmlNode yNode = iconElementNode.SelectSingleNode("y");
							if (yNode != null)
								y = Convert.ToInt32(yNode.InnerText, CultureInfo.InvariantCulture);

							if (bPalette)
								iconElement.href = GetSubImage(overStyle, x*2, y*2, w*2, h*2);
							else
								iconElement.href = GetSubImage(overStyle, x, y, w, h);
						
						}
						IconStyle iconStyle = new IconStyle(iconElement);
					
						XmlNode iconStyleColorNode = iconStyleNode.SelectSingleNode("color");
						if(iconStyleColorNode != null)
							iconStyle.Color = new ColorElement(ParseColor(iconStyleColorNode.InnerText));

						XmlNode iconStyleColorModeNode = iconStyleNode.SelectSingleNode("colorMode");
						if(iconStyleColorModeNode != null)
						{
							iconStyle.ColorMode = (iconStyleColorModeNode.InnerText.ToLower() == "random" ? ColorMode.Random : ColorMode.Normal);
						}
					
						XmlNode iconStyleHeadingNode = iconStyleNode.SelectSingleNode("heading");
						if(iconStyleHeadingNode != null)
							iconStyle.Heading = new DecimalElement(double.Parse(iconStyleHeadingNode.InnerText, CultureInfo.InvariantCulture));

						XmlNode iconStyleScaleNode = iconStyleNode.SelectSingleNode("scale");
						if(iconStyleScaleNode != null)
							iconStyle.Scale = new DecimalElement(double.Parse(iconStyleScaleNode.InnerText, CultureInfo.InvariantCulture));

						overStyle.IconStyle = iconStyle;
					}
				}

				XmlNode labelStyleNode = style.SelectSingleNode("LabelStyle");
				if(labelStyleNode != null)
				{
					LabelStyle labelStyle = new LabelStyle();
						
					XmlNode labelColorNode = labelStyleNode.SelectSingleNode("color");
					if(labelColorNode != null)
						labelStyle.Color = new ColorElement(ParseColor(labelColorNode.InnerText));

					XmlNode labelColorModeNode = labelStyleNode.SelectSingleNode("colorMode");
					if(labelColorModeNode != null)
						labelStyle.ColorMode = (labelColorModeNode.InnerText.ToLower() == "random" ? ColorMode.Random : ColorMode.Normal);
					

					XmlNode labelScaleNode = labelStyleNode.SelectSingleNode("scale");
					if(labelScaleNode != null)
						labelStyle.Scale = new DecimalElement(double.Parse(labelScaleNode.InnerText, CultureInfo.InvariantCulture));

					overStyle.LabelStyle = labelStyle;
				}

				XmlNode lineStyleNode = style.SelectSingleNode("LineStyle");
				if(lineStyleNode != null)
				{
					LineStyle lineStyle = new LineStyle();
						
					XmlNode lineColorNode = lineStyleNode.SelectSingleNode("color");
					if(lineColorNode != null)
						lineStyle.Color = new ColorElement(ParseColor(lineColorNode.InnerText));

					XmlNode lineColorModeNode = lineStyleNode.SelectSingleNode("colorMode");
					if(lineColorModeNode != null)
						lineStyle.ColorMode = (lineColorModeNode.InnerText.ToLower() == "random" ? ColorMode.Random : ColorMode.Normal);

					XmlNode lineWidthNode = lineStyleNode.SelectSingleNode("width");
					if(lineWidthNode != null)
						lineStyle.Width = new DecimalElement(double.Parse(lineWidthNode.InnerText, CultureInfo.InvariantCulture));

					overStyle.LineStyle = lineStyle;
				}

				XmlNode polyStyleNode = style.SelectSingleNode("PolyStyle");
				if(polyStyleNode != null)
				{
					PolyStyle polyStyle = new PolyStyle();
						
					XmlNode polyColorNode = polyStyleNode.SelectSingleNode("color");
					if(polyColorNode != null)
						polyStyle.Color = new ColorElement(ParseColor(polyColorNode.InnerText));

					XmlNode polyColorModeNode = polyStyleNode.SelectSingleNode("colorMode");
					if(polyColorModeNode != null)
						polyStyle.ColorMode = (polyColorModeNode.InnerText.ToLower() == "random" ? ColorMode.Random : ColorMode.Normal);
					
					XmlNode polyFillNode = polyStyleNode.SelectSingleNode("fill");
					if(polyFillNode != null)
						polyStyle.Fill = (polyFillNode.InnerText == "1" ? true : false);

					XmlNode polyOutlineNode = polyStyleNode.SelectSingleNode("outline");
					if(polyOutlineNode != null)
						polyStyle.Outline = (polyOutlineNode.InnerText == "1" ? true : false);

					overStyle.PolyStyle = polyStyle;
				}

				return overStyle;
			}
			catch (System.Net.WebException ex)
			{
                Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString());
			}

			return null;
		}

		private int ParseColor(string s)
		{
			string a = s.Substring(0,2);
			string b = s.Substring(2,2);
			string g = s.Substring(4,2);
			string r = s.Substring(6,2);

			return System.Drawing.Color.FromArgb(
				int.Parse(a, System.Globalization.NumberStyles.HexNumber),
				int.Parse(r, System.Globalization.NumberStyles.HexNumber),
				int.Parse(g, System.Globalization.NumberStyles.HexNumber),
				int.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArgb();

		}

		/// <summary>
		/// Extracts a rectangular selection from a given style
		/// </summary>
		/// <param name="filename">The file to extract the SubBitmap from</param>
		/// <param name="x">x</param>
		/// <param name="y">y</param>
		/// <param name="w">width</param>
		/// <param name="h">height</param>
		/// <returns>The generated bitmap</returns>
		private string GetSubImage(Style style, int x, int y, int w, int h)
		{
			// Try using a cached version
			string key = style.NormalIcon+ "|" 
				+x.ToString("D5", CultureInfo.InvariantCulture)
				+y.ToString("D5", CultureInfo.InvariantCulture)
				+w.ToString("D5", CultureInfo.InvariantCulture)
				+h.ToString("D5", CultureInfo.InvariantCulture);
			if (bitmapCache.ContainsKey(key))
				return key;

			// Create a new bitmap to draw into
			Bitmap outImage = new Bitmap(w, h);
			Graphics graphics = Graphics.FromImage(outImage);

			// Draw a region into the newly created bitmap
			RectangleF destinationRect = new RectangleF(0, 0, w, h);
            if (style.NormalIcon != null && bitmapCache.Contains(style.NormalIcon))
            {
                System.Drawing.Bitmap bit = ((Bitmap)bitmapCache[style.NormalIcon]);
                RectangleF sourceRect = new RectangleF(x, bit.Height - y - h, w, h);
                graphics.DrawImage((Bitmap)bitmapCache[style.NormalIcon], destinationRect, sourceRect, GraphicsUnit.Pixel);
                graphics.Flush();

                // Cache the generated bitmap
                bitmapCache.Add(key, outImage);

                return key;
            }
            else
            {
                return null;
            }
		}

		/// <summary>
		/// Retrieves a bitmap from cache or a file on disk
		/// </summary>
		/// <param name="filename">The filename to open the bitmap from</param>
		/// <returns>The found bitmap</returns>
		private string GetDiskImage(string filename)
		{
			// Try using a cached version
			if (bitmapCache.ContainsKey(filename))
				return filename;

			// Load the bitmap from disk
			Bitmap bit = (Bitmap)Bitmap.FromFile(filename);
			bitmapCache.Add(filename, bit);

			return filename;
		}

		/// <summary>
		/// Retrieves a bitmap from cache or the web
		/// </summary>
		/// <param name="filename">The URI to download the bitmap from</param>
		/// <returns>The downloaded bitmap</returns>
		private string GetWebImage(string filename)
		{
			// Try using a cached version
			if (bitmapCache.ContainsKey(filename))
				return filename;

			// Download the file from the web
			WebDownload myDownload = new WebDownload(filename);
			myDownload.DownloadMemory();

			// Load it into a Bitmap
			Bitmap bit = (Bitmap)System.Drawing.Bitmap.FromStream(myDownload.ContentStream);

			// Cache the downloaded bitmap
			myDownload.Dispose();
			bitmapCache.Add(filename, bit);
			return filename;
		}

		/// <summary>
		/// Locates a referenced style
		/// </summary>
		/// <param name="node"></param>
		/// <param name="KmlPath"></param>
		/// <returns>The located style, or null if none was found</returns>
		private Style LocateStyle(XmlNode node, string KmlPath)
		{
			Style style = null;

			// Takes care of 'normal' IconStyles
			XmlNode styleNode = node.SelectSingleNode("styleUrl");
			if (styleNode != null)
			{
				string styleUrl = styleNode.InnerText.Replace("#", "");
				style = (Style)iconStyles[styleUrl];
			}

			// Check if there's an inline Style node
			styleNode = node.SelectSingleNode("Style");
			if (styleNode != null)
			{
				if (style != null)
					style = GetStyle(styleNode, style, KmlPath);
				else
					style = GetStyle(styleNode, new Style(), KmlPath);
			}
			return style;
		}
		#endregion

		#region Drag&Drop handling methods
		/// <summary>
		/// Checks if the object being dropped is a kml or kmz file
		/// </summary>
		private void WorldWindow_DragEnter(object sender, DragEventArgs e)
		{
			if (DragDropIsValid(e))
				e.Effect = DragDropEffects.All;
		}

		/// <summary>
		/// Handles dropping of a kml/kmz file (by loading that file)
		/// </summary>
		private void WorldWindow_DragDrop(object sender, DragEventArgs e)
		{
			if (DragDropIsValid(e))
			{
				// transfer the filenames to a string array
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (files.Length > 0 && File.Exists(files[0]))
				{
					LoadDiskKM(files[0]);
				}
			}
		}

		/// <summary>
		/// Determines if this plugin can handle the dropped item
		/// </summary>
		private static bool DragDropIsValid(DragEventArgs e)
		{
			if( e.Data.GetDataPresent(DataFormats.FileDrop, false))
			{
				if (((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1)
				{
					string extension = Path.GetExtension(((string[])e.Data.GetData(DataFormats.FileDrop))[0]).ToLower(CultureInfo.InvariantCulture);
					if ((extension == ".kml") || (extension == ".kmz"))
						return true;
				}
			}
			return false;
		}
		#endregion

		#region Misc. methods
		internal string ExtractKMZ(string filename, out bool bError)
		{
			bError = false;

			FileInfo fileInfo = new FileInfo(filename);
			// Create a folder 'kmz' to extract the kmz file to
			string ExtractPath = Path.Combine(KmlDirectory, "kmz\\" + fileInfo.Name);
			if (!Directory.Exists(ExtractPath))
				Directory.CreateDirectory(ExtractPath);

			// Extract the kmz file
			FastZip fz = new FastZip();
			fz.ExtractZip(filename, ExtractPath, "");

			// Try to find the extracted kml file to load
			string ExtractedKMLPath = null;
			if (File.Exists(Path.Combine(ExtractPath, "doc.kml")))
				ExtractedKMLPath = Path.Combine(ExtractPath, "doc.kml");
			else
			{
				ExtractedKMLPath = GetKMLFromDirectory(ExtractPath);
				if (ExtractedKMLPath == null)
					bError = true;
			}

			return ExtractedKMLPath;
		}

		private string GetKMLFromDirectory(string ExtractPath)
		{
			string[] folders = Directory.GetDirectories(ExtractPath);
			foreach (string folder in folders)
			{
				string tempPath = GetKMLFromDirectory(folder);
				if (tempPath != null)
					return tempPath;
			}

			string[] kmlfiles = Directory.GetFiles(ExtractPath, "*.kml");
			if (kmlfiles.Length > 0)
				return kmlfiles[0];
			else
				return null;
		}
		/// <summary>
		/// Creates a scaled icon on the globe
		/// </summary>
		/// <param name="Name">The name of the item</param>
		/// <param name="Desc">The description</param>
		/// <param name="Lat">The latitude for the icon</param>
		/// <param name="Lon">The longitude for the icon</param>
		/// <param name="Alt">The altitude to draw the icon at</param>
		/// <param name="bitmapPath">The path to the bitmap to show</param>
		private void CreateIcon(RIcons layer, string Name, string Desc, string Uri, float Lat, float Lon, float Alt, 
			Style style, bool bRotated, float rotation, bool bExtrude)
		{
			// Create the icon and set initial settings
			RIcon ic = new RIcon(
				Name,									// name
				Lat,										// latitude
				Lon,										// longitude
				style.NormalIcon,							// helper
				Alt);									// altitude

			// Set optional icon settings
			if (Desc != null)
				ic.Description = Desc;
			if (Uri != null)
				ic.ClickableActionURL = Uri;
			if (bRotated)
			{
				ic.Rotation = Angle.FromDegrees(rotation);
				ic.IsRotated = true;
			}
			ic.m_drawGroundStick = bExtrude;
            if (style.NormalIcon != null && bitmapCache.Contains(style.NormalIcon))
            {
                ic.Image = (Bitmap)bitmapCache[style.NormalIcon];
                ic.Height = Double.IsNaN(style.NormalScale) ? IconSizeConstant : (int)(style.NormalScale * Math.Min(((Bitmap)bitmapCache[style.NormalIcon]).Height, IconSizeConstant));
                ic.Width = Double.IsNaN(style.NormalScale) ? IconSizeConstant : (int)(style.NormalScale * Math.Min(((Bitmap)bitmapCache[style.NormalIcon]).Width, IconSizeConstant));
            }

			// Add the icon to the layer
			layer.Add(ic);
		}

		/// <summary>
		/// Handles selecting and loading the selected KML/KMZ file
		/// </summary>
		private void loadMenu_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.CheckFileExists = true;
			fileDialog.Filter = "KML/KMZ files (*.kml *.kmz)|*.kml;*.kmz";
			fileDialog.Multiselect = false;
			fileDialog.RestoreDirectory = true;
			DialogResult result = fileDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				LoadDiskKM(fileDialog.FileName);
			}
		}

		/// <summary>
		/// Cleans up used resources
		/// </summary>
		private void Cleanup()
		{
			foreach (RenderableObject ro in KMLIcons.ChildObjects)
			{
				ro.Dispose();
			}
			KMLIcons.ChildObjects.Clear();

			foreach (Object bit in bitmapCache)
			{
				Bitmap bitmap = bit as Bitmap;
				if (bitmap != null)
					bitmap.Dispose();
			}
			bitmapCache.Clear();

			foreach (NetworkLink netLink in networkLinks)
			{
				netLink.Dispose();
			}
			networkLinks.Clear();

			iconStyles.Clear();
		}

		/// <summary>
		/// Shows information about KMLImporter on a Form
		/// </summary>
		private void aboutMenu_Click(object sender, EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		/// <summary>
		/// Toggles the Napalm enabled state
		/// </summary>
		private void napalmMenu_Click(object sender, EventArgs e)
		{
			bool bEnabled = Napalm.NapalmChangeStatus(KmlDirectory, napalmMenuItem.Text.StartsWith("Enable"));
			if (bEnabled)
				napalmMenuItem.Text = "Disable KMLImporter autoupdate";
			else
				napalmMenuItem.Text = "Enable KMLImporter autoupdate";
		}

		/// <summary>
		/// Toggles the 'drawAllLabels' state
		/// </summary>
		private void labelMenuItem_Click(object sender, EventArgs e)
		{
			labelMenuItem.Checked = Settings.ShowAllLabels = !labelMenuItem.Checked;
		}

		/// <summary>
		/// Returns the URI found in the first href tag
		/// </summary>
		/// <param name="source">The string to search</param>
		/// <returns>The found URI, or null if no URI was found</returns>
		private static string SearchUri(string source)
		{
			int i = source.IndexOf("<a href");
			if (i != -1)
			{
				int start = source.Substring(i).IndexOf("\"") + i+1;
				int end = source.Substring(start+1).IndexOf("\"") + start+1;
				return source.Substring(start, end-start);
			}

			int start2 = source.IndexOf("http://"); 
			if (start2 != -1) 
			{ 
				int end1 = source.Substring(start2+1).IndexOf("\n"); 
				int end2 = source.Substring(start2+1).IndexOf(" "); 
				int end3 = source.Length -1; 
                  
             
				if (end1 == -1) 
					end1 = Int32.MaxValue;
				else
					end1 += start2+1;
				if (end2 == -1) 
					end2 = Int32.MaxValue;
				else
					end2 += start2+1;
				if (end3 == -1) 
					end3 = Int32.MaxValue; 
  
				int compareend1 = (end1 < end2) ? end1 : end2; 
				int compareend2 = (end3 < compareend1) ? end3 : compareend1;

				string uri = source.Substring(start2, compareend2-start2);
				uri = uri.Replace(@"&amp;", @"&");
				uri = uri.Replace(@"&lt;", @"<");
				uri = uri.Replace(@"&gt;", @">");
				uri = uri.Replace(@"&apos;", @"'");
				uri = uri.Replace(@"&quot;", "\"");
				return uri;
			}

			return null;
		}

		/// <summary>
		/// Removes the tags from a string
		/// </summary>
		/// <param name="input">The string to remove the tags from</param>
		/// <returns>The input string without tags</returns>
		private static string RemoveTags(string source)
		{
			while (true)
			{
				if ((source.IndexOf("<") == -1) || (source.IndexOf(">") == -1))
					break;

				int start = source.IndexOf("<");
				int stop = source.IndexOf(">") + 1;
				int count = stop - start;

				if (count < 0)
					break;

				source = source.Remove(start, count);
			}

			source = source.Replace("&nbsp;", "");

			return source.Trim();
		}

		/// <summary>
		/// Removes unused icons from a given layer
		/// </summary>
		/// <param name="layer">The layer to remove icons from</param>
		private static void RemoveUnusedIcons(RIcons layer)
		{
			// Stores removed icons
			ArrayList VFD = new ArrayList();

			// Search for removed icons
			foreach (WorldWind.Renderable.RenderableObject ro in layer.ChildObjects)
			{
				RIcon ricon = ro as RIcon;
				if ((ricon != null) && (!ricon.HasBeenUpdated))
				{
					VFD.Add(ro);
				}
			}

			// Remove all icons that were found to be removed
			foreach (WorldWind.Renderable.RenderableObject ro in VFD)
			{
				layer.Remove(ro);
				ro.Dispose();
			}
		}
		#endregion
	}

	/// <summary>
	/// Napalm autoupdater code
	/// </summary>
	class Napalm
	{
		private const string plugName = "KMLImporter";
		private const string baseUrl = "http://worldwind.arc.nasa.gov";
		private delegate void UpdateDelegate (string PluginDirectory, string version);

		/// <summary>
		/// Empty private constructor, because this class only contains static methods
		/// </summary>
		private Napalm()
		{
			
		}

		/// <summary>
		/// Starts an async update
		/// </summary>
		/// <param name="PluginDirectory"></param>
		/// <param name="version"></param>
		internal static void Update(string PluginDirectory, string version)
		{
			UpdateDelegate udel = new UpdateDelegate(WebUpdate);
			udel.BeginInvoke(PluginDirectory, version, null, null);
		}
		/// <summary>
		/// Updates this plugin (and supporting files) from the internet
		/// </summary>
		private static void WebUpdate(string PluginDirectory, string version)
		{
			CultureInfo icy = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			// Now go check for a new version (in the background)
			try
			{
				if (!NapalmIsEnabled(PluginDirectory))
					return;

				if (!File.Exists(Path.Combine(PluginDirectory, plugName+".cs")))
					return;

				// Download the versioning file
				string URL = String.Format(icy, "{0}/{1}/{1}_ver.txt", baseUrl, plugName);
				WebClient verDownloader = new WebClient();
				Stream response = new MemoryStream(verDownloader.DownloadData(URL));

				// Create a reader to read the response from the server
				System.IO.StreamReader sr = new StreamReader(response);

				string ver = sr.ReadLine();

				// Try to update if this version appears to be out of date
				if (ver != version)
				{
					try
					{
						if (Convert.ToSingle(ver, CultureInfo.InvariantCulture) < Convert.ToSingle(version, CultureInfo.InvariantCulture))
							return;
					}
					catch (Exception)
					{
						return;
					}

					// Download the new .cs file
					string CsURL = String.Format(icy, "{0}/{1}/{1}.cs", baseUrl, plugName);
					string CsPath = Path.Combine(PluginDirectory, String.Format(icy, "{0}.cs_", plugName));
					WebClient csDownloader = new WebClient();
					csDownloader.DownloadFile(CsURL, CsPath);

					// Napalm v2.0 secure autoupdater block
					try
					{
						// Create a hash of the file that was downloaded
						System.IO.StreamReader streamreader = new StreamReader(CsPath);
						byte[] testStringBytes = GetHashBytes(streamreader.ReadToEnd());

						RSAParameters RSAKeyInfo = new RSAParameters();
						System.IO.StreamReader keyreader = new System.IO.StreamReader(Path.Combine(PluginDirectory, "key"));

						RSAKeyInfo.Modulus = Convert.FromBase64String(keyreader.ReadLine());
						RSAKeyInfo.Exponent = Convert.FromBase64String(keyreader.ReadLine());
						byte[] SignedHashValue = Convert.FromBase64String(sr.ReadLine());

						RSACryptoServiceProvider RSAdecr = new RSACryptoServiceProvider();
						RSAdecr.ImportParameters(RSAKeyInfo);
            
						RSAPKCS1SignatureDeformatter RSADeformatter = new RSAPKCS1SignatureDeformatter(RSAdecr);
						RSADeformatter.SetHashAlgorithm("SHA1");

						if (!RSADeformatter.VerifySignature(testStringBytes, SignedHashValue))
						{
                            Log.Write(Log.Levels.Error, String.Format(icy, "{0}: The file signature is not valid!", plugName));
							return;
						}
					}
					catch (Exception ex)		// General signature checking / cryptography error
					{
                        Log.Write(Log.Levels.Error, "Signature checking error:\n" + ex);
						return;
					}

					// Backup the current file, and replace it with the file that was just downloaded
					if ((File.Exists(CsPath)) && (new FileInfo(CsPath).Length > 2))
					{
						// Just delete a backup of the same version if it already exists
						string bcpPath = Path.Combine(PluginDirectory, plugName+"_v"+version+".cs_");
						if (File.Exists(bcpPath))
							File.Delete(bcpPath);

						string tempPath = Path.Combine(PluginDirectory, plugName+".cs_");
						string plugPath = Path.Combine(PluginDirectory, plugName+".cs");

						// Move the old file out of the way and replace it with the new version
						File.Move(plugPath,	bcpPath);
						for (int i = 0; (i < 5) && (File.Exists(tempPath)); i++)
						{
							try
							{
								File.Move(tempPath,	plugPath);
							}
							catch (Exception)
							{
								System.Threading.Thread.Sleep(800);
							}
						}
					}

					// Notify the user, because we love users
					string message = String.Format(icy, "The {0} plugin has been updated.\n", plugName);
					message += "If you experience any problems it is recommended that you reload the plugin.\n";
					MessageBox.Show(message, 
						plugName+" updated",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information,
						MessageBoxDefaultButton.Button1,
						MessageBoxOptions.ServiceNotification);
				}
				else if (ver != version)		// This means this is probably an internal plugin, so we can't autoupdate it
				{
					if (MessageBox.Show("A new version of the "+plugName+" plugin is available.\nWould you like to go to the website to download the latest version?",
						"PlaneTracker update available",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Information,
						MessageBoxDefaultButton.Button1,
						MessageBoxOptions.ServiceNotification) == DialogResult.Yes)
						System.Diagnostics.Process.Start("http://www.worldwindcentral.com/wiki/Add-on:KMLImporter");
				}
			}
			catch (Exception)		// We don't really care if this failed
			{ }
		}

		/// <summary>
		/// Gets a byte array representing the hash of a given string
		/// </summary>
		/// <param name="s">The string to hash</param>
		/// <returns>A byte array containing the hash</returns>
		private static byte[] GetHashBytes(string s)
		{
			byte[] data = System.Text.Encoding.UTF8.GetBytes(s);
			byte[] key = Convert.FromBase64String("szLIWrCoPJ3DSWInZx5Ye7sRz0MKBG3JpmgP2KgzlcWGvuJMqNiD77DVQuIRFvgbc5UCEFRhS5Ii5khitfOXhg==");	// A random key that needs to be kept constant
			byte[] hash = new HMACSHA1(key).ComputeHash(data);
			return hash;
		}

		/// <summary>
		/// Checks whether Napalm is enabled
		/// </summary>
		/// <param name="PluginDirectory">The directory that contains the key file</param>
		/// <returns>true if Napalm is enabled, false if Napalm is not enabled</returns>
		internal static bool NapalmIsEnabled(string PluginDirectory)
		{
			string keyPath = Path.Combine(PluginDirectory, "key");
			if (!File.Exists(keyPath))
				return false;

			StreamReader reader = new StreamReader(keyPath);
			string keyline1 = reader.ReadLine();
			string keyline2 = reader.ReadLine();
			string keyline3 = reader.ReadLine();
			reader.Close();

			if ((keyline1 != null) && (keyline2 != null) && (keyline3 != null) && (keyline3.Length > 0))
				return false;
			else
				return true;
		}

		/// <summary>
		/// Sets Napalm's enabled state
		/// </summary>
		/// <param name="PluginDirectory">The directory that contains the key file</param>
		/// <param name="bEnabled">Whether to enable Napalm</param>
		/// <returns>Whether Napalm was enabled</returns>
		internal static bool NapalmChangeStatus(string PluginDirectory, bool bEnabled)
		{
			string keyPath = Path.Combine(PluginDirectory, "key");
			if (!File.Exists(keyPath))
				return false;

			StreamReader reader = new StreamReader(keyPath);
			string keyline1 = reader.ReadLine();
			string keyline2 = reader.ReadLine();
			reader.Close();

			if ((keyline1 == null) || (keyline2 == null))
				return false;

			StreamWriter writer = new StreamWriter(keyPath);
			writer.WriteLine(keyline1);
			writer.WriteLine(keyline2);
			if (!bEnabled)
			{
				string[] possibleText = new string[] {
														 "DisableNapalm",
														 "You see; random characters",
														 "WARNING: Do not try to read the above lines out loud",
														 "\"Sharks with frickin' laser beams attached to their heads!\"",
														 "\"Oh, my, yes.\"",
														 "\"Windmills do not work that way! Good night!\"",
														 "\"I am Holly, the ship's computer, with an IQ of 6000, the same IQ as 6000 PE teachers.\"",
														 "\"Spoon!\"",
														 "\"Contrary to popular opinion, cats cannot see in the dark. They just know where you are.\""};
				Random rand = new Random();
				string disabledString = possibleText[rand.Next(possibleText.Length-1)];
				writer.WriteLine(disabledString);
			}
			writer.Flush();
			writer.Close();

			return bEnabled;
		}
	}

	/// <summary>
	/// Stores settings and has methods to save/load these settings to/from a file
	/// </summary>
	class Settings
	{
		internal static bool ShowAllLabels;				// Whether to draw all labels for icons

		/// <summary>
		/// Empty private constructor, because this class only contains static methods
		/// </summary>
		private Settings()
		{
		}
  
		/// <summary>
		/// Loads settings from an XML file
		/// </summary>
		/// <param name="file">The file to load from</param>
		internal static void LoadSettings(string file)
		{
			try
			{
				XmlDocument xmldoc = new XmlDocument();
				xmldoc.Load(file);
				XmlNode node;

				// ShowAllLabels 
				node = xmldoc.SelectSingleNode("KMLImporter/ShowAllLabels");
				if (node != null) 
				{ 
					ShowAllLabels = System.Convert.ToBoolean(node.InnerText, CultureInfo.InvariantCulture); 
				}
			}
			catch (System.IO.IOException)
			{	}
			catch (System.Xml.XmlException)
			{	}
		}

		/// <summary>
		/// Saves settings to an XML file
		/// </summary>
		/// <param name="file">The file to save to</param>
		internal static void SaveSettings(string file)
		{
			try
			{
				// Open writer
				System.Xml.XmlTextWriter xmlwriter = new System.Xml.XmlTextWriter(file, System.Text.Encoding.Default);
				xmlwriter.Formatting = System.Xml.Formatting.Indented;

				// Start document
				xmlwriter.WriteStartDocument();
				xmlwriter.WriteStartElement("KMLImporter");

				// ShowAllLabels
				xmlwriter.WriteStartElement("ShowAllLabels");
				xmlwriter.WriteString(ShowAllLabels.ToString(CultureInfo.InvariantCulture));
				xmlwriter.WriteEndElement();

				// End document
				xmlwriter.WriteEndElement();
				xmlwriter.WriteEndDocument();

				// Close writer
				xmlwriter.Flush();
				xmlwriter.Close();
			}
			catch (System.IO.IOException)
			{	}
			catch (System.Xml.XmlException)
			{	}
		}
	}

	/// <summary>
	/// Represents a NetworkLink. Updates a layer from a remote source periodically.
	/// </summary>
	class NetworkLink
	{
		private string url;
		private RIcons layer;
		private System.Timers.Timer tickTimer = new System.Timers.Timer();
		private System.Timers.Timer viewTimer = new System.Timers.Timer();
		private KMLImporter owner;
		private bool bUpdating = false;
		bool m_firedStartup = false;

		private Matrix lastView = Matrix.Identity;
		private bool bViewStopped = false;

		/// <summary>
		/// Creates and initializes a new NetworkLink
		/// </summary>
		/// <param name="owner">The owner of this NetworkLink</param>
		/// <param name="layer">The layer to update the NetworkLink to</param>
		/// <param name="url">The URL to update the NetworkLink from</param>
		/// <param name="tickTime">The interval, in milliseconds, at which the NetworkLink should update</param>
		/// <param name="viewTime">The time, in milliseconds, after the view stops moving which the NetworkLink should update</param>
		internal NetworkLink(KMLImporter owner, RIcons layer, string url, int tickTime, int viewTime)
		{
			this.owner = owner;
			this.url = url;
			this.layer = layer;

			if (tickTime > 0)
			{
				tickTimer.Interval = (double)tickTime;
				tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
				tickTimer.Start();
			}

			if (viewTime > 0)
			{
				viewTimer.Interval = (double)viewTime;
				viewTimer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
				viewTimer.Start();				
			}
		}

		/// <summary>
		/// Gets the visible bounding box for the application in lat/lon degrees.
		/// </summary>
		/// <returns>An array of Angles in minx.miny,maxx, maxy order</returns>
		private static string GetBBox()
		{
			CultureInfo ic = CultureInfo.InvariantCulture;

			// TODO: Correct the ViewRange for non-square windows.
			// Is is accurate horizontally but not vertically.
			Angle lat = DrawArgs.Camera.Latitude;
			Angle lon = DrawArgs.Camera.Longitude;
			Angle vr  = DrawArgs.Camera.ViewRange;

			Angle North = lat + (0.5 * vr);
			Angle South = lat - (0.5 * vr);
			Angle East  = lon + (0.5 * vr);
			Angle West  = lon - (0.5 * vr);

			//minX(West), minY(South), maxX(East), MaxY(North)
			return "BBOX=" + West.Degrees.ToString(ic) +" "+ South.Degrees.ToString(ic) +" "+ East.Degrees.ToString(ic) +" "+ North.Degrees.ToString(ic);
		}

		/// <summary>
		/// Fires off a download
		/// </summary>
		internal void Fire()
		{
			if (viewTimer.Enabled)
				timer_Elapsed(viewTimer, null);
			else
				timer_Elapsed(null, null);
		}

		/// <summary>
		/// Downloads a KML/KMZ file from the given URL
		/// </summary>
		private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (bUpdating)
				return;
			bUpdating = true;

			try
			{
				if (!m_firedStartup || (layer != null && layer.IsOn))
				{
					string fullurl = url;
					if (sender == viewTimer)
					{
						WorldWind.DrawArgs drawArgs = owner.ParentApplication.WorldWindow.DrawArgs;
						if (!bViewStopped)
						{
							if (drawArgs.WorldCamera.ViewMatrix != lastView)
							{
								lastView = drawArgs.WorldCamera.ViewMatrix;
								bUpdating = false;
								return;
							}
							bViewStopped = true;
						}
						else
						{
							if (drawArgs.WorldCamera.ViewMatrix != lastView)
							{
								lastView = drawArgs.WorldCamera.ViewMatrix;
								bViewStopped = false;
							}
							bUpdating = false;
							return;
						}
						fullurl += (fullurl.IndexOf('?') == -1 ? "?" : "&") + GetBBox();
					}

					string saveFile = Path.GetFileName(Uri.EscapeDataString(url));
					if(saveFile == null || saveFile.Length == 0)
						saveFile = "temp.kml";

					saveFile = Path.Combine(owner.PluginDirectory + "\\kml\\temp\\",saveFile);

                    FileInfo saveFileInfo = new FileInfo(saveFile);
                    if (!saveFileInfo.Directory.Exists)
                        saveFileInfo.Directory.Create();

					WebDownload myClient = new WebDownload(fullurl);
					myClient.DownloadFile(saveFile);

					// Extract the file if it is a kmz file
					string kmlFile = saveFile;
					if (Path.GetExtension(saveFile) == ".kmz")
					{
						bool bError = false;
						kmlFile = owner.ExtractKMZ(saveFile, out bError);

						if (bError)
						{
							return;
						}
					}

					owner.KMLPath = kmlFile;

					// Create a reader to read the file
					StreamReader sr = new StreamReader(kmlFile);

					// Read all data from the reader
					string kml = sr.ReadToEnd();

					sr.Close();
					
					try
					{
						// Load the actual kml data
						owner.LoadKML(kml, layer);
					}
					catch (Exception ex)
					{
                        Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString());

						MessageBox.Show(
							String.Format(CultureInfo.InvariantCulture, "Error loading KML file '{0}':\n\n{1}", kmlFile, ex.ToString()), 
							"KMLImporter error", 
							MessageBoxButtons.OK, 
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							MessageBoxOptions.ServiceNotification);
					}
					m_firedStartup = true;
				}
			}
			catch (Exception ex)
			{
                Log.Write(Log.Levels.Error, "KMLImporter: " + ex.ToString());
			}

			bUpdating = false;
		}

		/// <summary>
		/// Stops the timer
		/// </summary>
		internal void Dispose()
		{
			tickTimer.Stop();
			viewTimer.Stop();
		}
	}

	/// <summary>
	/// Helper class. Represents a KML Style or StyleMap
	/// </summary>
	class Style
	{
		#region Public members
		public BalloonStyle BalloonStyle = null;
		public IconStyle IconStyle = null;
		public LabelStyle LabelStyle = null;
		public LineStyle LineStyle = null;
		public PolyStyle PolyStyle = null;

		internal string NormalIcon
		{
			get
			{
				return normalIcon;
			}
			set
			{
				normalIcon = value;
			}
		}

		internal double NormalScale
		{
			get
			{
				return normalScale;
			}
			set
			{
				normalScale = value;
			}
		}

		#endregion

		#region Private members
		private string normalIcon;
		private double normalScale = Double.NaN;
		#endregion

		/// <summary>
		/// Creates a new Style
		/// </summary>
		internal Style()
		{
			this.normalIcon = null;
		}

		/// <summary>
		/// Creates a new Style
		/// </summary>
		/// <param name="normalIcon">The normal Bitmap to use for this Style</param>
		internal Style(string normalIcon)
		{
			this.normalIcon = normalIcon;
		}

		/// <summary>
		/// Convert a hex string to a .NET Color object.
		/// </summary>
		/// <param name="hexColor">a hex string: "FFFFFF", "#000000"</param>
		public static Color HexToColor(string hexColor)
		{
			string hc = ExtractHexDigits(hexColor);
			if (hc.Length != 8)
			{
				// you can choose whether to throw an exception
				//throw new ArgumentException("hexColor is not exactly 6 digits.");
				return Color.White;
			}
			string a = hc.Substring(0, 2);
			string r = hc.Substring(2, 2);
			string g = hc.Substring(4, 2);
			string b = hc.Substring(6, 2);
			Color color = Color.White;
			try
			{
				int ai 
					= Int32.Parse(a, System.Globalization.NumberStyles.HexNumber);
				int ri 
					= Int32.Parse(r, System.Globalization.NumberStyles.HexNumber);
				int gi 
					= Int32.Parse(g, System.Globalization.NumberStyles.HexNumber);
				int bi 
					= Int32.Parse(b, System.Globalization.NumberStyles.HexNumber);
				color = Color.FromArgb(ri, gi, bi);
			}
			catch
			{
				// you can choose whether to throw an exception
				//throw new ArgumentException("Conversion failed.");
				return Color.White;
			}
			return color;
		}
		/// <summary>
		/// Extract only the hex digits from a string.
		/// </summary>
		private static string ExtractHexDigits(string input)
		{
			// remove any characters that are not digits (like #)
			Regex isHexDigit 
				= new Regex("[abcdefABCDEF\\d]+", RegexOptions.Compiled);
			string newnum = "";
			foreach (char c in input)
			{
				if (isHexDigit.IsMatch(c.ToString()))
					newnum += c.ToString();
			}
			return newnum;
		}
	}

	/// <summary>
	/// LatLonAlt
	/// </summary>
	class LLA
	{
		internal float lat;
		internal float lon;
		internal float alt;

		/// <summary>
		/// Creates a new instance of LLA
		/// </summary>
		/// <param name="lat">Latitude</param>
		/// <param name="lon">Longitude</param>
		/// <param name="alt">Altitude</param>
		public LLA(float lat, float lon, float alt)
		{
			this.lat = lat;
			this.lon = lon;
			this.alt = alt;
		}
	}

	class BalloonStyle
	{
		public string id = null;
		public TextElement Text = null;
		public ColorElement TextColor = null;
		public ColorElement Color = null;
	}

	class IconStyle
	{
		public string id = null;
		public ColorElement Color = new ColorElement(System.Drawing.Color.White.ToArgb());
		public ColorMode ColorMode = ColorMode.Normal;
		public DecimalElement Heading = null;
		public DecimalElement Scale = new DecimalElement(1.0);
		public IconElement Icon = null;

		public IconStyle(IconElement icon)
		{
			Icon = icon;
		}
	}

	class LabelStyle
	{
		public string id = null;
		public ColorElement Color = new ColorElement(System.Drawing.Color.White.ToArgb());
		public ColorMode ColorMode = ColorMode.Normal;
		public DecimalElement Scale = new DecimalElement(1.0);
	}

	class LineStyle
	{
		public string id = null;
		public ColorElement Color = new ColorElement(System.Drawing.Color.Gray.ToArgb());
		public ColorMode ColorMode = ColorMode.Normal;
		public DecimalElement Width = new DecimalElement(1);
	}

	class PolyStyle
	{
		public string id = null;
		public ColorElement Color = new ColorElement(System.Drawing.Color.DarkGray.ToArgb());
		public ColorMode ColorMode = ColorMode.Normal;
		public bool Fill = true;
		public bool Outline = true;
	}

	class IconElement
	{
		public string href = null;
		public IntegerElement x = null;
		public IntegerElement y = null;
		public IntegerElement w = null;
		public IntegerElement h = null;
	}

	class TextElement
	{
		public string Text = null;
		public ColorElement TextColor = null;
	}

	class ColorElement
	{
		public int Color = System.Drawing.Color.Black.ToArgb();

		public ColorElement(int color)
		{
			Color = color;
		}
	}

	class IntegerElement
	{
		public int Value = 0;

		public IntegerElement(int v)
		{
			Value = v;
		}
	}

	class DecimalElement
	{
		public double Value = 0;

		public DecimalElement(double d)
		{
			Value = d;
		}
	}

	enum ColorMode
	{
		Normal,
		Random
	}

	/// <summary>
	/// Holds a collection of icons
	/// </summary>
	class RIcons : Icons
	{
		#region Private members
		private new Hashtable m_textures = new Hashtable(); // Texture cache
		
		private new Sprite m_sprite;
		private Line m_groundStick;
		private Vector2[] groundStick = new Vector2[2];

		private new RIcon mouseOverIcon; // The closest icon the mouse is currently over

		private const int minIconZoomAltitude = 5000000;

		private static int hotColor = Color.White.ToArgb();
		private static int normalColor = Color.FromArgb(150,255,255,255).ToArgb();
		private static int descriptionColor = Color.White.ToArgb();

		private ArrayList labelRectangles = new ArrayList();
        private Form mainForm = MainApplication.ActiveForm;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:RIcons"/> class 
		/// </summary>
		/// <param name="name">The name to give the layer</param>
		internal RIcons(string name) : base(name) 
		{
			this.MetaData.Add("Child count", 0);
		}

		/// <summary>
		/// Adds an icon to this layer. Deprecated.
		/// </summary>
		internal void AddIcon(RIcon icon)
		{
			icon.ParentList = this;
			Add(icon);
		}

		/// <summary>
		/// Draw the icon
		/// </summary>
		protected virtual void Render(DrawArgs drawArgs, RIcon icon, Vector3 projectedPoint)
		{
			if (!icon.isInitialized)
				icon.Initialize(drawArgs);

			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
				return;

			Vector3 referenceCenter = new Vector3(
				(float)drawArgs.WorldCamera.ReferenceCenter.X,
				(float)drawArgs.WorldCamera.ReferenceCenter.Y,
				(float)drawArgs.WorldCamera.ReferenceCenter.Z);

			IconTexture iconTexture = GetTexture(icon);
			bool isMouseOver = icon == mouseOverIcon;
			if(isMouseOver)
			{
				// Mouse is over
				isMouseOver = true;

				if(icon.isSelectable)
					DrawArgs.MouseCursor = CursorType.Hand;

				string description = icon.Description;

                // Ashish Datta - Commented. Descriptions will appear in bubbles not on the bottom of the screen.

                /*
				if(description==null)
					description = icon.ClickableActionURL;
				if(description!=null)
				{
					// Render description field
					DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak | DrawTextFormat.Bottom;
					int left = 10;
					if(World.Settings.ShowLayerManager)
						left += World.Settings.LayerManagerWidth;
					Rectangle rect = Rectangle.FromLTRB(left, 10, drawArgs.screenWidth - 10, drawArgs.screenHeight - 10 );

					// Draw outline
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );
					
					rect.Offset(2,0);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					rect.Offset(0,2);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					rect.Offset(-2,0);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					// Draw description
					rect.Offset(1,-1);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect, 
						format, descriptionColor );
				}*/
			}

			int color = isMouseOver ? hotColor : normalColor;
			if(iconTexture==null || isMouseOver || Settings.ShowAllLabels)
			{
				// Render label
				if(icon.Name != null)
				{
					// Render name field
					const int labelWidth = 1000; // Dummy value needed for centering the text
					if(iconTexture==null)
					{
						// Center over target as we have no bitmap
						Rectangle realrect = drawArgs.defaultDrawingFont.MeasureString(m_sprite, icon.Name, DrawTextFormat.WordBreak, color);
						realrect.X = (int)projectedPoint.X - (labelWidth>>1);
						realrect.Y = (int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1));

						bool bDraw = true;

						foreach (Rectangle drawnrect in labelRectangles)
						{
							if (realrect.IntersectsWith(drawnrect))
							{
								bDraw = false;
								break;
							}
						}

						if (bDraw)
						{
							labelRectangles.Add(realrect);
							drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, realrect, DrawTextFormat.Center, color);
						}
					}
					else
					{
						// Adjust text to make room for icon
						int spacing = (int)(icon.Width * 0.3f);
						if(spacing>10)
							spacing = 10;
						int offsetForIcon = (icon.Width>>1) + spacing;

						// Text to the right
						Rectangle rightrect = drawArgs.defaultDrawingFont.MeasureString(m_sprite, icon.Name, DrawTextFormat.WordBreak, color);
						rightrect.X = (int)projectedPoint.X + offsetForIcon;
						rightrect.Y = (int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1));

						// Text to the left
						Rectangle leftrect = drawArgs.defaultDrawingFont.MeasureString(m_sprite, icon.Name, DrawTextFormat.WordBreak, color);
						leftrect.X = (int)projectedPoint.X - offsetForIcon - rightrect.Width;
						leftrect.Y = (int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1));

						bool bDrawRight = true;
						bool bDrawLeft = true;

						foreach (Rectangle drawnrect in labelRectangles)
						{
							if (rightrect.IntersectsWith(drawnrect))
							{
								bDrawRight = false;
							}
							if (leftrect.IntersectsWith(drawnrect))
							{
								bDrawLeft = false;
							}
							if (!bDrawRight && !bDrawLeft)
							{
								break;
							}
						}

						if (bDrawRight)
						{
							labelRectangles.Add(rightrect);
							drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, rightrect, DrawTextFormat.WordBreak, color);
						}
						else if (bDrawLeft)
						{
							labelRectangles.Add(leftrect);
							drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, leftrect, DrawTextFormat.WordBreak, color);
						}
					}
				}
			}

			if (icon.m_drawGroundStick)
			{
				Vector3 projectedGroundPoint = drawArgs.WorldCamera.Project(icon.m_groundPoint - referenceCenter);

				m_groundStick.Begin();
				groundStick[0].X = projectedPoint.X;
				groundStick[0].Y = projectedPoint.Y;
				groundStick[1].X = projectedGroundPoint.X;
				groundStick[1].Y = projectedGroundPoint.Y;

				m_groundStick.Draw(groundStick, Color.Red.ToArgb());
				m_groundStick.End();
			}

			if(iconTexture!=null)
			{
				float factor = 1;
				if (drawArgs.WorldCamera.Altitude > minIconZoomAltitude)
					factor -= (float)((drawArgs.WorldCamera.Altitude - minIconZoomAltitude) / drawArgs.WorldCamera.Altitude);

				// Render icon
				float xscale = factor * ((float)icon.Width / iconTexture.Width);
				float yscale = factor * ((float)icon.Height / iconTexture.Height);
				m_sprite.Transform = Matrix.Scaling(xscale, yscale, 0);
				
				if (icon.IsRotated) m_sprite.Transform *= Matrix.RotationZ((float)icon.Rotation.Radians - (float) drawArgs.WorldCamera.Heading.Radians);

				m_sprite.Transform *= Matrix.Translation(projectedPoint.X, projectedPoint.Y, 0);
				m_sprite.Draw( iconTexture.Texture,
					new Vector3(iconTexture.Width>>1, iconTexture.Height>>1,0),
					Vector3.Empty,
					color);
				
				// Reset transform to prepare for text rendering later
				m_sprite.Transform = Matrix.Identity;
			}
		}

		/// <summary>
		/// Retrieve an icon's texture
		/// </summary>
		private IconTexture GetTexture(RIcon icon)
		{
			object key = icon.Image;
			if(key==null)
				return null;
			IconTexture res = (IconTexture)m_textures[key];
			return res;
		}


		#region RenderableObject methods
		/// <summary>
		/// Add a child object to this layer.
		/// </summary>
		public override void Add(RenderableObject ro)
		{
			ro.ParentList = this;
			m_children.Add(ro);
			isInitialized = false;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(m_sprite != null)
			{
				m_sprite.Dispose();
				m_sprite = null;
			}
			if(m_groundStick != null)
			{
				m_groundStick.Dispose();
				m_groundStick = null;
			}

			m_sprite = new Sprite(drawArgs.device);
			m_groundStick = new Line(drawArgs.device);

			// Load all textures
			foreach(RenderableObject ro in m_children)
			{
				RIcon icon = ro as RIcon;
				if(icon==null)
				{
					// Child is not an icon
					if(ro.IsOn)
						ro.Initialize(drawArgs);
					continue;
				}

				// Child is an icon
				icon.Initialize(drawArgs);

				object key = null;
				IconTexture iconTexture = null;

				// Icon image from bitmap
				if(icon.Image != null)
				{
					iconTexture = (IconTexture)m_textures[icon.Image];
					if(iconTexture==null)
					{
						// Create new texture from image
						key = icon.Image;
						iconTexture = new IconTexture( drawArgs.device, icon.Image);
					}
				}

				if(iconTexture==null)
					// No texture set
					continue;

				if(key!=null)
				{
					// New texture, cache it
					m_textures.Add(key, iconTexture);

					// Use default dimensions if not set
					if(icon.Width==0)
						icon.Width = iconTexture.Width;
					if(icon.Height==0)
						icon.Height = iconTexture.Height;
				}
			}

			// Compute mouse over bounding boxes
			foreach(RenderableObject ro in m_children)
			{
				RIcon icon = ro as RIcon;
				if(icon==null)
					// Child is not an icon
					continue;

				if(GetTexture(icon)==null)
				{
					// Label only 
					icon.SelectionRectangle = drawArgs.defaultDrawingFont.MeasureString(null, icon.Name, DrawTextFormat.None, 0);
				}
				else
				{
					// Icon only
					icon.SelectionRectangle = new Rectangle(0, 0, icon.Width, icon.Height);
				}

				// Center the box at (0,0)
				icon.SelectionRectangle.Offset(-icon.SelectionRectangle.Width/2, -icon.SelectionRectangle.Height/2 );
			}

			isInitialized = true;
		}

		public override void Dispose()
		{
			base.Dispose();

			if(m_textures != null)
			{
				foreach(IconTexture iconTexture in m_textures.Values)
					iconTexture.Texture.Dispose();
				m_textures.Clear();
			}

			if(m_sprite != null)
			{
				m_sprite.Dispose();
				m_sprite = null;
			}

			if (m_groundStick != null)
			{
				m_groundStick.Dispose();
				m_groundStick = null;
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			foreach(RenderableObject ro in m_children)
			{
				if(!ro.IsOn)
					continue;
				if(!ro.isSelectable)
					continue;

				RIcon icon = ro as RIcon;
				if(icon==null)
				{
					// Child is not an icon
					if (ro.PerformSelectionAction(drawArgs))
						return true;
					continue;
				}

				if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
					continue;

				Vector3 projectedPoint = drawArgs.WorldCamera.Project(icon.Position);
                if (!icon.SelectionRectangle.Contains(
                    (DrawArgs.LastMousePosition.X - (int)projectedPoint.X) / 5,
                    (DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y) / 5)){
                    continue;
                }

				try
				{
                    // Ashish Datta - Show the description in a bubble instead of just trying to open a URL.

                    foreach (RIcon r in m_children)
                    {
                        if (r.DescriptionBubble != null)
                        {
                            r.IsDescriptionVisible = false;
                            r.DescriptionBubble.isVisible = false;
                            r.DescriptionBubble.Dispose();
                        }
                    }

                    if (icon.DescriptionBubble != null)
                        icon.DescriptionBubble.Dispose();

                    icon.DescriptionBubble = new KMLDialog();
                    icon.DescriptionBubble.Owner = mainForm;

                    if (icon.IsDescriptionVisible == false)
                    {
                        icon.IsDescriptionVisible = true;
                    }
                    else
                    {
                        icon.DescriptionBubble.Dispose();
                        icon.IsDescriptionVisible = false;
                    }

					return true;

				}
                catch (Exception)
				{
				}
			}

			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(!isInitialized)
				return;

			Vector3 referenceCenter = new Vector3(
				(float)drawArgs.WorldCamera.ReferenceCenter.X,
				(float)drawArgs.WorldCamera.ReferenceCenter.Y,
				(float)drawArgs.WorldCamera.ReferenceCenter.Z);

			// First render everything except icons
			foreach(RenderableObject ro in m_children)
			{
				if(ro is RIcon)
					continue;

				if(!ro.IsOn)
					continue;

				// Child is not an icon
				ro.Render(drawArgs);
			}

			labelRectangles.Clear();

			int closestIconDistanceSquared = int.MaxValue;
			RIcon closestIcon = null;

			// Now render just the icons
			m_sprite.Begin(SpriteFlags.AlphaBlend);
			foreach(RenderableObject ro in m_children)
			{
				if(!ro.IsOn)
					continue;

				RIcon icon = ro as RIcon;
				if(icon == null)
					continue;

				// Find closest mouse-over icon
				Vector3 projectedPoint = drawArgs.WorldCamera.Project(icon.Position - referenceCenter);

                // Ashish Datta - Show/Hide the description bubble.
                
                if (icon.IsDescriptionVisible == true)
                {
                    if (!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
                    {
                        icon.DescriptionBubble.Hide();
                        icon.IsDescriptionVisible = false;
                    }
                    else
                    {
                        if (icon.DescriptionBubble.isVisible == true)
                        {
                            icon.DescriptionBubble.Location = new Point((int)projectedPoint.X + (icon.Width/4), (int)projectedPoint.Y);
                            icon.DescriptionBubble.Show();

                            if(icon.DescriptionBubble.HTMLIsSet == false)
                                icon.DescriptionBubble.SetHTML(icon.Description);
                            
                            icon.DescriptionBubble.BringToFront();
                        }

                    }
                }

				int dx = DrawArgs.LastMousePosition.X - (int)projectedPoint.X;
				int dy = DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y;
				if( icon.SelectionRectangle.Contains( dx, dy ) )
				{
					// Mouse is over, check whether this icon is closest
					int distanceSquared = dx*dx + dy*dy;
					if(distanceSquared < closestIconDistanceSquared)
					{
						closestIconDistanceSquared = distanceSquared;
						closestIcon = icon;
					}
				}

				if(icon != mouseOverIcon)
					Render(drawArgs, icon, projectedPoint);
			}

			labelRectangles.Clear();

			// Render the mouse over icon last (on top)
			if(mouseOverIcon != null)
				Render(drawArgs, mouseOverIcon, drawArgs.WorldCamera.Project(mouseOverIcon.Position - referenceCenter));

			mouseOverIcon = closestIcon;

			m_sprite.End();
		}
		#endregion
	}

	/// <summary>
	/// One icon in an RIcons layer
	/// </summary>
	class RIcon : RenderableObject
	{
		#region Public members
		internal string m_description;	    // Longer description of icon (addition to name)

		internal Bitmap Image;					// Icon image
		internal int Width;						// Icon on-screen rendered width (pixels)
		internal int Height;					// Icon on-screen rendered height (pixels)

        internal new string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                isSelectable = value != null;
                m_description = value;
            }
        }
		internal string ClickableActionURL // On-Click browse to location
		{
			get
			{
				return m_clickableActionURL;
			}
			set 
			{
				isSelectable = value != null;
				m_clickableActionURL = value;
			}
		}
		internal Rectangle SelectionRectangle;	// Bounding box centered at (0,0) used to calculate whether mouse is over icon/label

		internal Angle Rotation
		{
			get
			{
				return m_rotation;
			}
			set
			{
				m_rotation = value;
			}
		}

		internal bool IsRotated
		{
			get
			{
				return m_isRotated;
			}
			set
			{
				m_isRotated = value;
			}
		}


		internal bool m_drawGroundStick = true;
		internal Vector3 m_groundPoint = new Vector3();

		internal string NormalIcon;

		internal bool HasBeenUpdated = true;

        // Ashish Datta - the bubble memebers.
        internal KMLDialog DescriptionBubble;
        internal bool IsDescriptionVisible = false;

		#endregion

		#region Private members
		private Angle m_rotation;
		private bool m_isRotated;

		private string m_clickableActionURL;

		private float m_latitude;
		private float m_longitude;
		private float m_altitude;				// The icon altitude above sea level

		Matrix lastView = Matrix.Identity;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:RIcon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		internal RIcon(string name, float latitude, float longitude, string normalicon, float heightAboveSurface) : base( name )
		{
			m_latitude = latitude;
			m_longitude = longitude;
			NormalIcon = normalicon;
			this.m_altitude = heightAboveSurface;
		}

		/// <summary>
		/// Sets the geographic position of the icon.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="altitude">The icon altitude above sea level.</param>
		internal void SetPosition(float latitude, float longitude, float altitude)
		{
			m_latitude = latitude;
			m_longitude = longitude;
			this.m_altitude = altitude;

			// Recalculate XYZ coordinates
			isInitialized = false;
		}


		#region RenderableObject methods
		public override void Initialize(DrawArgs drawArgs)
		{
			float samplesPerDegree = 50.0f / ((float)drawArgs.WorldCamera.ViewRange.Degrees);
			float elevation = (float)drawArgs.CurrentWorld.TerrainAccessor.GetElevationAt(m_latitude, m_longitude, samplesPerDegree);
			float altitude = (float)(World.Settings.VerticalExaggeration * m_altitude + World.Settings.VerticalExaggeration * elevation);
			Position = MathEngine.SphericalToCartesian(m_latitude, m_longitude, 
				altitude + drawArgs.WorldCamera.WorldRadius);

			if (m_drawGroundStick)
			{
				// compute the ground point
				double gselevation = drawArgs.WorldCamera.WorldRadius;

				if((((WorldWindow)drawArgs.parentControl).CurrentWorld.TerrainAccessor != null) && (drawArgs.WorldCamera.Altitude < 300000))
				{
					gselevation += elevation * World.Settings.VerticalExaggeration;
				}

				m_groundPoint = MathEngine.SphericalToCartesian(m_latitude, m_longitude, gselevation);
			}

			isInitialized = true;
		}

		/// <summary>
		/// Disposes the icon (when disabled)
		/// </summary>
		public override void Dispose()
		{
			// Nothing to dispose
            // Ashish Datta - make sure the description bubble is destroyed and not visisble.
            if (this.DescriptionBubble != null)
                this.DescriptionBubble.Dispose();
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			// Handled by parent
			return false;
		}

		public override void Update(DrawArgs drawArgs)
		{
			// Handled by parent

			if(drawArgs.WorldCamera.ViewMatrix != lastView && drawArgs.CurrentWorld.TerrainAccessor != null && drawArgs.WorldCamera.Altitude < 300000)
			{
				float samplesPerDegree = 50.0f / ((float)drawArgs.WorldCamera.ViewRange.Degrees);
				float elevation = (float)drawArgs.CurrentWorld.TerrainAccessor.GetElevationAt(m_latitude, m_longitude, samplesPerDegree);
				float altitude = (float)(World.Settings.VerticalExaggeration * m_altitude + World.Settings.VerticalExaggeration * elevation);
				Position = MathEngine.SphericalToCartesian(m_latitude, m_longitude, 
					altitude + drawArgs.WorldCamera.WorldRadius);

				lastView = drawArgs.WorldCamera.ViewMatrix;
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			// Handled by parent
		}
		#endregion
	}

	/// <summary>
	/// Renders a message to the lower right corner
	/// </summary>
	class WaitMessage : RenderableObject
	{
		#region Private members
		private string _Text = "Please wait, loading KML file.";
		private int color = Color.White.ToArgb();
		private int distanceFromCorner = 25;
		#endregion

		/// <summary>
		/// Creates a new WaitMessage
		/// </summary>
		internal WaitMessage() : base("KML WaitMessage", Vector3.Empty, Quaternion.Identity)
		{
			// We want to be drawn on top of everything else
			this.RenderPriority = RenderPriority.Icons;

			// true to make this layer active on startup, this is equal to the checked state in layer manager
			this.IsOn = true;
		}


		#region RenderableObject methods
		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			// Draw the current text using default font in lower right corner
			Rectangle bounds = drawArgs.defaultDrawingFont.MeasureString(null, _Text, DrawTextFormat.None, 0);
			drawArgs.defaultDrawingFont.DrawText(null, _Text, 
				drawArgs.screenWidth-bounds.Width-distanceFromCorner, drawArgs.screenHeight-bounds.Height-distanceFromCorner,
				color );
		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}
		#endregion
	}

	/// <summary>
	/// A Form with information about KMLImporter
	/// </summary>
	class AboutForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label3;
		private System.ComponentModel.Container components = null;

		internal AboutForm()
		{
			InitializeComponent();
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


		private void button1_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void linkLabel1_Click(object sender, EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("http://shockfire.blogspot.com/");
			}
			catch (Exception) {}
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.button1 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(120, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(160, 48);
			this.label1.TabIndex = 0;
			this.label1.Text = "KMLImporter";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(96, 144);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(224, 24);
			this.label2.TabIndex = 1;
			this.label2.Text = "Created by Tim van den Hamer (ShockFire)";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// linkLabel1
			// 
			this.linkLabel1.Location = new System.Drawing.Point(96, 176);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(224, 16);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "http://shockfire.blogspot.com/";
			this.linkLabel1.Click += new EventHandler(linkLabel1_Click);
			this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(168, 216);
			this.button1.Name = "button1";
			this.button1.TabIndex = 3;
			this.button1.Text = "OK";
			this.button1.Click += new EventHandler(button1_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(96, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(224, 64);
			this.label3.TabIndex = 4;
            this.label3.Text = "KMLImporter is a NASA World Wind plugin that allows you to read kml/kmz files. It is still under development and as such doesnt support all features of kml.";
			// 
			// AboutForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(416, 265);
			this.ControlBox = false;
			this.Controls.Add(this.label3);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.ShowInTaskbar = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.Text = "About KMLImporter";
			this.ResumeLayout(false);
		}
		#endregion
	}
}
