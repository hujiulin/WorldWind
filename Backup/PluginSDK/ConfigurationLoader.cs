using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.IO;
using WorldWind;
using WorldWind.Camera;
using WorldWind.Terrain;
using WorldWind.Renderable;
using System.Globalization;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ConfigurationLoader.
	/// </summary>
	public class ConfigurationLoader
	{
		public static double ParseDouble(string s)
		{
			return double.Parse(s, CultureInfo.InvariantCulture);
		}

        public static void XMLValidationCallback(object sender, ValidationEventArgs args)
        {
            string file = "(unknown)";
            XmlReader reader = sender as XmlReader;
            if (reader != null)
                file = reader.BaseURI;

            if (args.Severity == XmlSeverityType.Warning)
            {
                Log.Write(Log.Levels.Warning, "CONF", "Warning: " + args.Message);
                Log.Write(Log.Levels.Warning, "CONF", "  in " + file);
            }
            else
            {
                Log.Write(Log.Levels.Error, "CONF", "Error: " + args.Message);
                Log.Write(Log.Levels.Error, "CONF", "  in " + file);
                //throw args.Exception;
            }
        }

		public static World Load(string filename, Cache cache)
		{
            Log.Write(Log.Levels.Debug, "CONF", "Loading " + filename);

            // get the World Wind Settings through reflection to avoid changing the signature of Load().
            Assembly a = Assembly.GetEntryAssembly();
            Type appType = a.GetType("WorldWind.MainApplication");
            System.Reflection.FieldInfo finfo = appType.GetField("Settings", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
            WorldWindSettings settings = finfo.GetValue(null) as WorldWindSettings;

            XmlReaderSettings readerSettings = new XmlReaderSettings();

            if (settings.ValidateXML)
            {
                Log.Write(Log.Levels.Debug, "CONF", "validating " + filename + " against WorldXmlDescriptor.xsd and LayerSet.xsd");
                readerSettings.ValidationType = ValidationType.Schema;
                /* load the schema to validate against instead of hoping for an inline schema reference */
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(null, settings.ConfigPath + "/WorldXmlDescriptor.xsd");
                schemas.Add(null, settings.ConfigPath + "/Earth/LayerSet.xsd");


                readerSettings.Schemas = schemas;
                readerSettings.ValidationEventHandler += new ValidationEventHandler(XMLValidationCallback);
                readerSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            }
            else
            {
                Log.Write(Log.Levels.Debug, "CONF", "loading " + filename + " without validation");
                readerSettings.ValidationType = ValidationType.None;
            }

            try
            {
                XmlReader docReader = XmlReader.Create(filename, readerSettings);
                XPathDocument docNav = new XPathDocument(docReader);
                XPathNavigator nav = docNav.CreateNavigator();

                XPathNodeIterator worldIter = nav.Select("/World[@Name]");
                if (worldIter.Count > 0)
                {
                    worldIter.MoveNext();
                    string worldName = worldIter.Current.GetAttribute("Name", "");
                    double equatorialRadius = ParseDouble(worldIter.Current.GetAttribute("EquatorialRadius", ""));
                    string layerDirectory = worldIter.Current.GetAttribute("LayerDirectory", "");

                    if (layerDirectory.IndexOf(":") < 0)
                    {
                        layerDirectory = Path.Combine(Path.GetDirectoryName(filename), layerDirectory);
                    }

                    TerrainAccessor[] terrainAccessor = getTerrainAccessorsFromXPathNodeIterator(worldIter.Current.Select("TerrainAccessor"),
                        System.IO.Path.Combine(cache.CacheDirectory, worldName));

                    World newWorld = new World(
                        worldName,
                        new Microsoft.DirectX.Vector3(0, 0, 0),
                        new Microsoft.DirectX.Quaternion(0, 0, 0, 0),
                        equatorialRadius,
                        cache.CacheDirectory,
                        (terrainAccessor != null ? terrainAccessor[0] : null)//TODO: Oops, World should be able to handle an array of terrainAccessors
                        );


                    newWorld.RenderableObjects = getRenderablesFromLayerDirectory(layerDirectory, newWorld, cache);

                    return newWorld;
                }
            }
            catch (XmlSchemaException ex)
            {
                Log.Write(Log.Levels.Error, "CONF", "Exception caught during XML parsing: " + ex.Message);
                Log.Write(Log.Levels.Error, "CONF", "File " + filename + " was not read successfully.");
                // TODO: should pop up a message box or something.
                return null;
            }

			return null;
		}

        private static RenderableObjectList getRenderablesFromLayerDirectory(string layerDirectory, World parentWorld, Cache cache)
		{
			RenderableObjectList renderableCollection = new RenderableObjectList(parentWorld.Name);

			DirectoryInfo layerDir = new DirectoryInfo(layerDirectory);
			if(!layerDir.Exists)
			{
				return renderableCollection;
			}

			foreach(FileInfo layerFile in layerDir.GetFiles("*.xml"))
			{
				RenderableObjectList currentRenderable = getRenderableFromLayerFile(layerFile.FullName, parentWorld, cache);
				if(currentRenderable != null)
				{
					renderableCollection.Add(currentRenderable);
				}
			}

			return renderableCollection;
		}

		private static bool ParseBool(string booleanString)
		{
			if(booleanString == null || booleanString.Trim().Length == 0)
			{
				return false;
			}

            booleanString = booleanString.Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture);

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

        public static RenderableObjectList getRenderableFromLayerFile(string layerFile, World parentWorld, Cache cache)
		{
			return getRenderableFromLayerFile(layerFile, parentWorld, cache, true);
		}

        public static RenderableObjectList getRenderableFromLayerFile(string layerFile, World parentWorld, Cache cache, bool enableRefresh)
		{
            Log.Write(Log.Levels.Debug+1, "CONF", "Loading renderable from " + layerFile);
            try
			{
				XPathDocument docNav = null;
				XPathNavigator nav = null;

                XmlReaderSettings readerSettings = new XmlReaderSettings();

                // get the World Wind Settings through reflection.
                Assembly a = Assembly.GetEntryAssembly();
                Type appType = a.GetType("WorldWind.MainApplication");
                System.Reflection.FieldInfo finfo = appType.GetField("Settings", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
                WorldWindSettings settings = finfo.GetValue(null) as WorldWindSettings;

                if (settings.ValidateXML)
                {
                    Log.Write(Log.Levels.Debug, "CONF", "validating " + layerFile + " against LayerSet.xsd");
                    readerSettings.ValidationType = ValidationType.Schema;
                    // load the schema to validate against instead of hoping for an inline schema reference
                    XmlSchemaSet schemas = new XmlSchemaSet();
                    // TODO: LayerSet.xsd should probably live someplace else as it applies to all worlds, not just Earth


                    schemas.Add(null, settings.ConfigPath + "/Earth/LayerSet.xsd");

                    readerSettings.Schemas = schemas;
                    readerSettings.ValidationEventHandler += new ValidationEventHandler(XMLValidationCallback);
                    readerSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                }
                else
                {
                    Log.Write(Log.Levels.Debug, "CONF", "loading " + layerFile + " without validation");
                    readerSettings.ValidationType = ValidationType.None;
                }

                try
                {
                    if (layerFile.IndexOf(@"http://") < 0)
                    {
                        XmlReader docReader = XmlReader.Create(layerFile, readerSettings);
                        docNav = new XPathDocument(docReader); 
                    }
                    else
                    {
                        Angle[] bbox = CameraBase.getViewBoundingBox();
                        string viewBBox = string.Format(CultureInfo.InvariantCulture,
                            "{0},{1},{2},{3}",
                            bbox[0].ToString().TrimEnd('°'), bbox[1].ToString().TrimEnd('°'), bbox[2].ToString().TrimEnd('°'), bbox[3].ToString().TrimEnd('°'));

                        //See if there is a ? already in the URL
                        int flag = layerFile.IndexOf("?");
                        if (flag == -1)
                            layerFile = layerFile + "?BBOX=" + viewBBox;
                        else
                            layerFile = layerFile + "&BBOX=" + viewBBox;

                        WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(layerFile);
                        download.DownloadMemory();

                        XmlReader docReader = XmlReader.Create(download.ContentStream, readerSettings);
                        docNav = new XPathDocument(docReader); 
   
                    }

                    


                    nav = docNav.CreateNavigator();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
					return null;
				}

				XPathNodeIterator iter = nav.Select("/LayerSet");

				if(iter.Count > 0)
				{
					iter.MoveNext();
					string redirect = iter.Current.GetAttribute("redirect", "");
					redirect = redirect.Replace("${WORLDWINDVERSION}", System.Windows.Forms.Application.ProductVersion);
                    string redirectWithoutBBOX = redirect;
					if(redirect != null && redirect.Length > 0)
					{
						FileInfo layerFileInfo = new FileInfo(layerFile);

						try
						{
							Angle[] bbox = CameraBase.getViewBoundingBox();
							string viewBBox = string.Format(CultureInfo.InvariantCulture, 
								"{0},{1},{2},{3}",
								bbox[0].ToString().TrimEnd('°'),bbox[1].ToString().TrimEnd('°'), bbox[2].ToString().TrimEnd('°'),bbox[3].ToString().TrimEnd('°'));
							
							//See if there is a ? already in the URL
							int flag = redirect.IndexOf("?");
							if (flag == -1)
								redirect = redirect + "?BBOX="+viewBBox;
							else 
								redirect = redirect + "&BBOX="+viewBBox;
							
							WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(redirect);

							string username = iter.Current.GetAttribute("username", "");
							string password = iter.Current.GetAttribute("password", "");
							
							if(username != null)
							{
							////	download.UserName = username;
							////	download.Password = password;
							}

							FileInfo tempDownloadFile = new FileInfo(layerFile.Replace(layerFileInfo.Extension, "_.tmp"));

							download.DownloadFile(tempDownloadFile.FullName, WorldWind.Net.DownloadType.Unspecified);

							tempDownloadFile.Refresh();
							if(tempDownloadFile.Exists && tempDownloadFile.Length > 0)
							{
								FileInfo tempStoreFile = new FileInfo(tempDownloadFile.FullName.Replace("_.tmp", ".tmp"));
								if(tempStoreFile.Exists)
									tempStoreFile.Delete();

								tempDownloadFile.MoveTo(tempStoreFile.FullName);
							}
							
							download.Dispose();

							using(StreamWriter writer = new StreamWriter(layerFile.Replace(layerFileInfo.Extension, ".uri"), false))
							{
								writer.WriteLine(redirectWithoutBBOX);
							}
						}
						catch(Exception ex)
						{
							Log.Write(ex);
						}

						return getRenderableFromLayerFile(layerFile.Replace(layerFileInfo.Extension, ".tmp"), parentWorld, cache);
					}
					else
					{
						RenderableObjectList parentRenderable = null;

						string sourceUri = null;
						if(layerFile.EndsWith(".tmp"))
						{
							//get source url
							using(StreamReader reader = new StreamReader(layerFile.Replace(".tmp", ".uri")))
							{
								sourceUri = reader.ReadLine();
							}
						}
						string refreshString = iter.Current.GetAttribute("Refresh", "");
						if(refreshString != null && refreshString.Length > 0)
						{

							if(iter.Current.Select("Icon").Count > 0)
							{
								parentRenderable = new Icons(iter.Current.GetAttribute("Name", ""),
									(sourceUri != null ? sourceUri : layerFile),
									TimeSpan.FromSeconds(ParseDouble(refreshString)),
									parentWorld,
									cache);
							}
							else
							{
								parentRenderable = new RenderableObjectList(
									iter.Current.GetAttribute("Name", ""),
									(sourceUri != null ? sourceUri : layerFile),
									TimeSpan.FromSeconds(ParseDouble(refreshString)),
									parentWorld,
									cache);
							}
							
						}
						else
						{
							if(iter.Current.Select("Icon").Count > 0)
							{
								parentRenderable = new Icons(iter.Current.GetAttribute("Name", ""));
							}
							else
							{
								parentRenderable = new RenderableObjectList(iter.Current.GetAttribute("Name", ""));
							}
						}

						parentRenderable.ParentList = parentWorld.RenderableObjects;
				
						if(World.Settings.useDefaultLayerStates)
						{
							parentRenderable.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
						}
						else
						{
							parentRenderable.IsOn = IsLayerOn(parentRenderable);
						}

						string disableExpansionString = iter.Current.GetAttribute("DisableExpansion", "");
						if(disableExpansionString != null)
						{
							parentRenderable.DisableExpansion = ParseBool(disableExpansionString);
						}

						string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
						if(description != null && description.Length > 0)
							parentRenderable.Description = description;

						parentRenderable.ShowOnlyOneLayer = ParseBool(iter.Current.GetAttribute("ShowOnlyOneLayer", ""));
						
						parentRenderable.MetaData.Add("XmlSource", (sourceUri != null ? sourceUri : layerFile));
						
						parentRenderable.MetaData.Add("World", parentWorld);
						parentRenderable.MetaData.Add("Cache", cache);
						parentRenderable.ParentList = parentWorld.RenderableObjects;

						string renderPriorityString = iter.Current.GetAttribute("RenderPriority", "");
						if(renderPriorityString != null)
						{
                            if (String.Compare(renderPriorityString, "Icons", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.Icons;
							}
                            else if (String.Compare(renderPriorityString, "LinePaths", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.LinePaths;
							}
                            else if (String.Compare(renderPriorityString, "Placenames", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.Placenames;
							}
                            else if (String.Compare(renderPriorityString, "AtmosphericImages", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.AtmosphericImages;
							}
						}

						string infoUri = iter.Current.GetAttribute("InfoUri", "");

						if(infoUri != null && infoUri.Length > 0)
						{
							if(parentRenderable.MetaData.Contains("InfoUri"))
							{
								parentRenderable.MetaData["InfoUri"] = infoUri;
							}
							else
							{
								parentRenderable.MetaData.Add("InfoUri", infoUri);
							}
						}

						addImageLayersFromXPathNodeIterator(iter.Current.Select("ImageLayer"), parentWorld, parentRenderable);
						addQuadTileLayersFromXPathNodeIterator(iter.Current.Select("QuadTileSet"), parentWorld, parentRenderable, cache);
						addPathList(iter.Current.Select("PathList"), parentWorld, parentRenderable);
						addPolygonFeature(iter.Current.Select("PolygonFeature"), parentWorld, parentRenderable);
						addLineFeature(iter.Current.Select("LineFeature"), parentWorld, parentRenderable);
                        addModelFeature(iter.Current.Select("ModelFeature"), parentWorld, parentRenderable);
                        //addWater(iter.Current.Select("Water"), parentWorld, parentRenderable);
						addTiledPlacenameSet(iter.Current.Select("TiledPlacenameSet"), parentWorld, parentRenderable);
                        addTiledWFSPlacenameSet(iter.Current.Select("TiledWFSPlacenameSet"), parentWorld, parentRenderable, cache);
						addIcon(iter.Current.Select("Icon"), parentWorld, parentRenderable, cache);
						addScreenOverlays(iter.Current.Select("ScreenOverlay"), parentWorld, parentRenderable, cache);
						addChildLayerSet(iter.Current.Select("ChildLayerSet"), parentWorld, parentRenderable, cache);

						addExtendedInformation(iter.Current.Select("ExtendedInformation"), parentRenderable);
				
						if(parentRenderable.RefreshTimer != null && enableRefresh)
						{
							parentRenderable.RefreshTimer.Start();
						}
						return parentRenderable;
					}
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
				//Log.Write(Log.Levels.Debug, layerFile);
			}
            Log.Write(Log.Levels.Warning, "CONF", "WARNING: no renderable created for " + layerFile);

			return null;
		}

		public static bool IsLayerOn(RenderableObject ro)
		{
			string path = getRenderablePathString(ro);
			foreach(string s in World.Settings.loadedLayers)
			{
				if(s.Equals(path))
				{
					return true;
				}
			}

			return false;
		}

		private static RenderableObjectList addChildLayerSet(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable, Cache cache)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string layerName = iter.Current.GetAttribute("Name", "");
					bool showAtStartup = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					bool showOnlyOneLayer = ParseBool(iter.Current.GetAttribute("ShowOnlyOneLayer", ""));

					string redirect = iter.Current.GetAttribute("redirect", "");

					if(redirect != null && redirect.Length > 0)
					{
						return ConfigurationLoader.getRenderableFromLayerFile(redirect, parentWorld, cache);
					}

					RenderableObjectList rol = new RenderableObjectList(layerName);
					if(iter.Current.Select("Icon").Count > 0)
					{
						rol = new Icons(layerName);
					}
					
					rol.ParentList = parentRenderable;

					if(World.Settings.useDefaultLayerStates)
					{
						rol.IsOn = showAtStartup;
					}
					else
					{
						rol.IsOn = IsLayerOn(rol);
					}

					string disableExpansionString = iter.Current.GetAttribute("DisableExpansion", "");
					if(disableExpansionString != null)
					{
						rol.DisableExpansion = ParseBool(disableExpansionString);
					}


					rol.ShowOnlyOneLayer = showOnlyOneLayer;
					rol.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);

					string renderPriorityString = iter.Current.GetAttribute("RenderPriority", "");
					if(renderPriorityString != null)
					{
                        if (String.Compare(renderPriorityString, "Icons", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
						{
							rol.RenderPriority = RenderPriority.Icons;
						}
                        else if (String.Compare(renderPriorityString, "LinePaths", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
						{
							rol.RenderPriority = RenderPriority.LinePaths;
						}
                        else if (String.Compare(renderPriorityString, "Placenames", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
						{
							rol.RenderPriority = RenderPriority.Placenames;
						}
                        else if (String.Compare(renderPriorityString, "AtmosphericImages", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
						{
							rol.RenderPriority = RenderPriority.AtmosphericImages;
						}
					}

					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
					if(description != null && description.Length > 0)
						rol.Description = description;

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if(infoUri != null && infoUri.Length > 0)
					{
						if(rol.MetaData.Contains("InfoUri"))
						{
							rol.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							rol.MetaData.Add("InfoUri", infoUri);
						}
					}

					addImageLayersFromXPathNodeIterator(iter.Current.Select("ImageLayer"), parentWorld, rol);
					addQuadTileLayersFromXPathNodeIterator(iter.Current.Select("QuadTileSet"), parentWorld, rol, cache);
					addPolygonFeature(iter.Current.Select("PolygonFeature"), parentWorld, rol);
					addLineFeature(iter.Current.Select("LineFeature"), parentWorld, rol);
					addPathList(iter.Current.Select("PathList"), parentWorld, rol);
					addTiledPlacenameSet(iter.Current.Select("TiledPlacenameSet"), parentWorld, rol);
					addIcon(iter.Current.Select("Icon"), parentWorld, rol, cache);
					addScreenOverlays(iter.Current.Select("ScreenOverlay"), parentWorld, rol, cache);
					addChildLayerSet(iter.Current.Select("ChildLayerSet"), parentWorld, rol, cache);

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), rol);
					parentRenderable.Add(rol);
				}
			}

			return null;
		}

		private static ImageTileService getImageTileServiceFromXPathNodeIterator(XPathNodeIterator iter)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string serverUrl = getInnerTextFromFirstChild(iter.Current.Select("ServerUrl"));
					string dataSetName = getInnerTextFromFirstChild(iter.Current.Select("DataSetName"));
					string serverLogoFilePath = getInnerTextFromFirstChild(iter.Current.Select("ServerLogoFilePath"));
					
					TimeSpan cacheExpiration = getCacheExpiration(iter.Current.Select("CacheExpirationTime"));

					if(serverLogoFilePath != null && serverLogoFilePath.Length > 0 && !Path.IsPathRooted(serverLogoFilePath))
					{
						serverLogoFilePath = Path.Combine(
							Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
							serverLogoFilePath);
					}

					
					if(serverUrl != null &&
						dataSetName != null)
					{
						return new ImageTileService(dataSetName, serverUrl, serverLogoFilePath, cacheExpiration);
					}
				}
			}

			return null;
		}

        private static ImageStore getImageStoreFromXPathNodeIterator(string name, XPathNodeIterator imageAccessorIter, RenderableObjectList parentRenderable, Cache cache)
        {
			double levelZeroTileSizeDegrees = ParseDouble(getInnerTextFromFirstChild(imageAccessorIter.Current.Select("LevelZeroTileSizeDegrees")));
			int numberLevels = Int32.Parse(getInnerTextFromFirstChild(imageAccessorIter.Current.Select("NumberLevels")));
			int textureSizePixels = Int32.Parse(getInnerTextFromFirstChild(imageAccessorIter.Current.Select("TextureSizePixels")));
			string imageFileExtension = getInnerTextFromFirstChild(imageAccessorIter.Current.Select("ImageFileExtension"));
			string permanentDirectory = getInnerTextFromFirstChild(imageAccessorIter.Current.Select("PermanentDirectory"));
			if(permanentDirectory == null || permanentDirectory.Length == 0)
				permanentDirectory = getInnerTextFromFirstChild(imageAccessorIter.Current.Select("PermanantDirectory"));


			TimeSpan dataExpiration = getCacheExpiration(imageAccessorIter.Current.Select("DataExpirationTime"));

			string duplicateTilePath = getInnerTextFromFirstChild(imageAccessorIter.Current.Select("DuplicateTilePath"));
			string cacheDir = getInnerTextFromFirstChild(imageAccessorIter.Current.Select("CacheDirectory"));
			
			if(cacheDir == null || cacheDir.Length == 0)
			{
				cacheDir = String.Format("{0}{1}{2}{1}{3}", cache.CacheDirectory, Path.DirectorySeparatorChar, getRenderablePathString(parentRenderable), name);
			}
			else
			{
				cacheDir = Path.Combine(cache.CacheDirectory, cacheDir);
			}

			if(permanentDirectory != null && permanentDirectory.IndexOf(":") < 0)
			{
				permanentDirectory = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), permanentDirectory);
			}

			if(duplicateTilePath != null && duplicateTilePath.IndexOf(":") < 0)
			{
				duplicateTilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), duplicateTilePath);
			}

            byte opacity = 255;

            // case 1 : permanent directory specified.
			if(permanentDirectory != null)
			{
				ImageStore ia = new ImageStore();
				ia.DataDirectory = permanentDirectory;
				ia.LevelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
				ia.LevelCount = numberLevels;
				ia.ImageExtension = imageFileExtension;
				//doesn't work when this is set
				//ia.CacheDirectory = cacheDir;
                
				
				if(duplicateTilePath != null && duplicateTilePath.Length > 0)
				{
					ia.DuplicateTexturePath = duplicateTilePath; 
				}
                return ia;
            }

            // case 2: ImageTileService specified
			XPathNodeIterator imageTileServiceIter = imageAccessorIter.Current.Select("ImageTileService");
			if(imageTileServiceIter.Count > 0)
			{
				imageTileServiceIter.MoveNext();
				
				string serverUrl = getInnerTextFromFirstChild(imageTileServiceIter.Current.Select("ServerUrl"));
				string dataSetName = getInnerTextFromFirstChild(imageTileServiceIter.Current.Select("DataSetName"));
				string serverLogoFilePath = getInnerTextFromFirstChild(imageTileServiceIter.Current.Select("ServerLogoFilePath"));
	
				TimeSpan cacheExpiration = getCacheExpiration(imageTileServiceIter.Current.Select("CacheExpirationTime"));

				if(serverLogoFilePath != null && serverLogoFilePath.Length > 0 && !Path.IsPathRooted(serverLogoFilePath))
				{
					serverLogoFilePath = Path.Combine(
						Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
						serverLogoFilePath);
				}
				
				string opacityString = getInnerTextFromFirstChild(imageTileServiceIter.Current.Select("Opacity"));

				if(opacityString != null)
					opacity = byte.Parse(opacityString);

				ImageStore ia = new NltImageStore(dataSetName, serverUrl);
				ia.DataDirectory = null;
				ia.LevelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
				ia.LevelCount = numberLevels;
				ia.ImageExtension = imageFileExtension;
				ia.CacheDirectory = cacheDir;
                ia.ServerLogo = serverLogoFilePath;

                return ia;
            }

            // case 3: WMSAccessor specified
			XPathNodeIterator wmsAccessorIter = imageAccessorIter.Current.Select("WMSAccessor");
			if(wmsAccessorIter.Count > 0)
			{
				wmsAccessorIter.MoveNext();
				
				WorldWind.Net.Wms.WmsImageStore wmsLayerStore = new WorldWind.Net.Wms.WmsImageStore();
			
				wmsLayerStore.ImageFormat = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("ImageFormat"));

				wmsLayerStore.ImageExtension = imageFileExtension;
				wmsLayerStore.CacheDirectory = cacheDir;
				//wmsLayerAccessor.IsTransparent = ParseBool(getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("UseTransparency")));
				wmsLayerStore.ServerGetMapUrl = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("ServerGetMapUrl"));
				wmsLayerStore.Version = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("Version"));
				wmsLayerStore.WMSLayerName = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("WMSLayerName"));

				string username = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("Username"));
				string password = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("Password"));
				string wmsStyleName = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("WMSLayerStyle"));
				string serverLogoPath = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("ServerLogoFilePath"));
				string opacityString = getInnerTextFromFirstChild(wmsAccessorIter.Current.Select("Opacity"));
				
				
				if(serverLogoPath != null && serverLogoPath.Length > 0 && !Path.IsPathRooted(serverLogoPath))
				{
					serverLogoPath = Path.Combine(
						Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
						serverLogoPath);
				}
				if(opacityString != null)
					opacity = byte.Parse(opacityString);

                TimeSpan cacheExpiration = getCacheExpiration(imageAccessorIter.Current.Select("CacheExpirationTime"));

			    //if(username != null && username.Length > 0)
			    //    wmsLayerStore.Username = username;

			    //if(password != null)
			    //    wmsLayerAccessor.Password = password;

				if(wmsStyleName != null && wmsStyleName.Length > 0)
					wmsLayerStore.WMSLayerStyle = wmsStyleName;
				else
					wmsLayerStore.WMSLayerStyle = "";

				wmsLayerStore.LevelCount = numberLevels;
				wmsLayerStore.LevelZeroTileSizeDegrees = levelZeroTileSizeDegrees;

                return wmsLayerStore;
            }
            Log.Write(Log.Levels.Warning, "CONF", "WARNING: no valid image store found!");
            return null;
        }

		private static void addQuadTileLayersFromXPathNodeIterator(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable, Cache cache)
		{
			while(iter.MoveNext())
			{
                string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
                double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
				bool showAtStartup = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));

                Log.Write(Log.Levels.Debug+1, "CONF", "adding QuadTileSet "+name);

				double north = 0;
				double south = 0;
				double west = 0;
				double east = 0;
				
				XPathNodeIterator boundingBoxIter = iter.Current.Select("BoundingBox");
				if(boundingBoxIter.Count > 0)
				{
					boundingBoxIter.MoveNext();
					north = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("North")));
					south = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("South")));
					west = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("West")));
					east = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("East")));
				}

				string terrainMappedString = getInnerTextFromFirstChild(iter.Current.Select("TerrainMapped"));
				string renderStrutsString = getInnerTextFromFirstChild(iter.Current.Select("RenderStruts"));

				TimeSpan dataExpiration = getCacheExpiration(iter.Current.Select("CacheExpirationTime"));
				
				bool terrainMapped = true;

				if(terrainMappedString != null)
				{
					terrainMapped = ParseBool(terrainMappedString);
				}
				XPathNodeIterator imageAccessorIter = iter.Current.Select("descendant::ImageAccessor");

                if (imageAccessorIter.Count == 0)
                {
                    Log.Write(Log.Levels.Warning, "CONF", "skipping QuadTileSet without any ImageAccessor");
                    return;
                }

                int currentStore = 0;
                ImageStore[] imageStores = new ImageStore[imageAccessorIter.Count];

                while (imageAccessorIter.MoveNext())
                {
                    imageStores[currentStore] = getImageStoreFromXPathNodeIterator(name, imageAccessorIter, parentRenderable, cache);
                    currentStore++;
                }

				QuadTileSet qts = null;

				qts = new QuadTileSet(
					name,
					parentWorld,
					distanceAboveSurface,
					north,
					south,
					west,
					east,
					terrainMapped,
					imageStores
					);
                if(imageStores[0].IsDownloadableLayer)
                    qts.ServerLogoFilePath = imageStores[0].ServerLogo;

				qts.CacheExpirationTime = dataExpiration;

				string infoUri = iter.Current.GetAttribute("InfoUri", "");

				if(infoUri != null && infoUri.Length > 0)
				{
					if(qts.MetaData.Contains("InfoUri"))
					{
						qts.MetaData["InfoUri"] = infoUri;
					}
					else
					{
						qts.MetaData.Add("InfoUri", infoUri);
					}
				}

                string effectFile = getInnerTextFromFirstChild(iter.Current.Select("Effect"));
                if (effectFile != null && effectFile.Length > 0)
                {
                    Log.Write(Log.Levels.Debug, "CONF", "QuadTileSet with effect " + effectFile);
                    if (qts.MetaData.Contains("EffectPath"))
                    {
                        qts.MetaData["EffectPath"] = effectFile;
                    }
                    else
                    {
                        qts.MetaData.Add("EffectPath", effectFile);
                    }
                }


				string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
				if(description != null && description.Length > 0)
					qts.Description = description;

				if(iter.Current.Select("TransparentColor").Count > 0)
				{
					System.Drawing.Color c = getColor(iter.Current.Select("TransparentColor"));
					qts.ColorKey = c.ToArgb();
				}

				if(iter.Current.Select("TransparentMinValue").Count > 0)
				{
					qts.ColorKey = int.Parse(getInnerTextFromFirstChild(iter.Current.Select("TransparentMinValue")));
				}

				if(iter.Current.Select("TransparentMaxValue").Count > 0)
				{
					qts.ColorKeyMax = int.Parse(getInnerTextFromFirstChild(iter.Current.Select("TransparentMaxValue")));
				}

				if(renderStrutsString != null)
				{
					qts.RenderStruts = ParseBool(renderStrutsString);
				}

				qts.ParentList = parentRenderable;
				if(World.Settings.useDefaultLayerStates)
				{
					qts.IsOn = showAtStartup;
				}
				else
				{
					qts.IsOn = IsLayerOn(qts);
				}

				qts.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
				addExtendedInformation(iter.Current.Select("ExtendedInformation"), qts);
				parentRenderable.Add(qts);
			}
		}

		private static TimeSpan getCacheExpiration(XPathNodeIterator iter)
		{
			TimeSpan ts = TimeSpan.MaxValue;

			if(iter.Count > 0)
			{
				iter.MoveNext();

				string daysString = getInnerTextFromFirstChild(iter.Current.Select("Days"));
				string hoursString = getInnerTextFromFirstChild(iter.Current.Select("Hours"));
				string minutesString = getInnerTextFromFirstChild(iter.Current.Select("Mins"));
				string secondsString = getInnerTextFromFirstChild(iter.Current.Select("Seconds"));
				
				double days = 0;
				double hours = 0;
				double minutes = 0;
				double seconds = 0;


				if(daysString != null)
					days = ParseDouble(daysString);

				if(hoursString != null)
					hours = ParseDouble(hoursString);

				if(minutesString != null)
					minutes = ParseDouble(minutesString);

				if(secondsString != null)
					seconds = ParseDouble(secondsString);

				ts = new TimeSpan((int)days, (int)hours, (int)minutes, (int)seconds);
			}

			return ts;
		}

		private static void addExtendedInformation(XPathNodeIterator iter, RenderableObject renderable)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string toolBarImage = getInnerTextFromFirstChild(iter.Current.Select("ToolBarImage"));

					if(toolBarImage != null)
					{
						if(toolBarImage.Length > 0 && !Path.IsPathRooted(toolBarImage))
							Path.Combine(
								Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
								toolBarImage);

						renderable.MetaData.Add("ToolBarImagePath", toolBarImage);
					}
				}
			}
		}

		public static string GetRenderablePathString(RenderableObject renderable)
		{
			return getRenderablePathString(renderable);
		}

		private static string getRenderablePathString(RenderableObject renderable)
		{
			if(renderable.ParentList == null)
			{
				return renderable.Name;
			}
			else
			{
				return getRenderablePathString(renderable.ParentList) + Path.DirectorySeparatorChar + renderable.Name;
			}
		}

		private static void addImageLayersFromXPathNodeIterator(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
					string texturePath = getInnerTextFromFirstChild(iter.Current.Select("TexturePath"));
					byte opacity = byte.Parse(getInnerTextFromFirstChild(iter.Current.Select("Opacity")));

					TimeSpan dataExpiration = getCacheExpiration(iter.Current.Select("CacheExpirationTime"));
					string imageUrl = null;

					if(texturePath.StartsWith("http://"))
					{
						imageUrl = texturePath;
						texturePath = null;
					}

					XPathNodeIterator boundingBoxIter = iter.Current.Select("BoundingBox");
					if(boundingBoxIter.Count > 0)
					{
						boundingBoxIter.MoveNext();

						double north = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("North")));
						double south = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("South")));
						double west = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("West")));
						double east = ParseDouble(getInnerTextFromFirstChild(boundingBoxIter.Current.Select("East")));

						ImageLayer im = new ImageLayer(
							name,
							parentWorld,
							distanceAboveSurface,
							texturePath,
							south,
							north,
							west,
							east,
							opacity, 
							parentWorld.TerrainAccessor);

						im.ImageUrl = imageUrl;
						im.CacheExpiration = dataExpiration;

						string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
						if(description != null && description.Length > 0)
							im.Description = description;

						addExtendedInformation(iter.Current.Select("ExtendedInformation"), im);

						string infoUri = iter.Current.GetAttribute("InfoUri", "");

						if(infoUri != null && infoUri.Length > 0)
						{
							if(im.MetaData.Contains("InfoUri"))
							{
								im.MetaData["InfoUri"] = infoUri;
							}
							else
							{
								im.MetaData.Add("InfoUri", infoUri);
							}
						}

						im.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);

						im.ParentList = parentRenderable;
						if(World.Settings.useDefaultLayerStates)
						{
							im.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
						}
						else
						{
							im.IsOn = IsLayerOn(im);
						}
						parentRenderable.ChildObjects.Add(
							im
							);
					}
				}
			}
		}

		private static void addScreenOverlays(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable, Cache cache)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					string imageUri = getInnerTextFromFirstChild(iter.Current.Select("ImageUri"));
					string startXString = getInnerTextFromFirstChild(iter.Current.Select("StartX"));
					string startYString = getInnerTextFromFirstChild(iter.Current.Select("StartY"));
					string widthString = getInnerTextFromFirstChild(iter.Current.Select("Width"));
					string heightString = getInnerTextFromFirstChild(iter.Current.Select("Height"));
					string opacityString = getInnerTextFromFirstChild(iter.Current.Select("Opacity"));
					string showHeaderString = getInnerTextFromFirstChild(iter.Current.Select("ShowHeader"));
					string alignmentString = getInnerTextFromFirstChild(iter.Current.Select("Alignment"));
					string clickableUrl = getInnerTextFromFirstChild(iter.Current.Select("ClickableUrl"));
					string refreshTimeString = iter.Current.GetAttribute("Refresh", "");
					string hideBorderString = getInnerTextFromFirstChild(iter.Current.Select("HideBorder"));

					if(startXString != null && startYString != null)
					{
						int startX = int.Parse(startXString);
						int startY = int.Parse(startYString);

						WorldWind.Renderable.ScreenOverlay overlay = new ScreenOverlay(name, startX, startY, imageUri);
					
						if(widthString != null)
						{
							overlay.Width = int.Parse(widthString);
						}
						if(heightString != null)
						{
							overlay.Height = int.Parse(heightString);
						}

						if(alignmentString != null)
						{
                            if (alignmentString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("left"))
							{
								overlay.Alignment = ScreenAlignment.Left;
							}
                            else if (alignmentString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("right"))
							{
								overlay.Alignment = ScreenAlignment.Right;
							}
						}

						if(clickableUrl != null && clickableUrl.Length > 0)
						{
							overlay.ClickableUrl = clickableUrl;
						}
						
						if(hideBorderString != null && hideBorderString.Length > 0)
							overlay.HideBorder = ParseBool(hideBorderString);

						if(iter.Current.Select("BorderColor").Count != 0)
						{
							overlay.BorderColor = getColor(iter.Current.Select("BorderColor"));
						}

						string cachePath = String.Format("{0}{1}{2}{1}{3}{1}{3}", cache.CacheDirectory, Path.DirectorySeparatorChar, getRenderablePathString(parentRenderable), name);

						if(refreshTimeString != null && refreshTimeString.Length > 0)
						{
							overlay.RefreshTimeSec = ParseDouble(refreshTimeString);
						}

						overlay.SaveFilePath = cachePath;
						addExtendedInformation(iter.Current.Select("ExtendedInformation"), overlay);

						string infoUri = iter.Current.GetAttribute("InfoUri", "");

						if(infoUri != null && infoUri.Length > 0)
						{
							if(overlay.MetaData.Contains("InfoUri"))
							{
								overlay.MetaData["InfoUri"] = infoUri;
							}
							else
							{
								overlay.MetaData.Add("InfoUri", infoUri);
							}
						}

						string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
						if(description != null && description.Length > 0)
							overlay.Description = description;

						overlay.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
						overlay.ParentList = parentRenderable;
						if(opacityString != null)
						{
							overlay.Opacity = byte.Parse(opacityString);
						}

						if(showHeaderString != null)
						{
							overlay.ShowHeader = ParseBool(showHeaderString);
						}

						if(World.Settings.useDefaultLayerStates)
						{
							overlay.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
						}
						else
						{
							overlay.IsOn = IsLayerOn(overlay);
						}

						parentRenderable.ChildObjects.Add(
							overlay
							);
					}
				}
			}
		}

		private static void addScreenOverlaysToIcon(XPathNodeIterator iter, World parentWorld, Icon icon, Cache cache)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					string imageUri = getInnerTextFromFirstChild(iter.Current.Select("ImageUri"));
					string startXString = getInnerTextFromFirstChild(iter.Current.Select("StartX"));
					string startYString = getInnerTextFromFirstChild(iter.Current.Select("StartY"));
					string widthString = getInnerTextFromFirstChild(iter.Current.Select("Width"));
					string heightString = getInnerTextFromFirstChild(iter.Current.Select("Height"));
					string opacityString = getInnerTextFromFirstChild(iter.Current.Select("Opacity"));
					string showHeaderString = getInnerTextFromFirstChild(iter.Current.Select("ShowHeader"));
					string refreshTimeString = iter.Current.GetAttribute("Refresh", "");
					string alignmentString = getInnerTextFromFirstChild(iter.Current.Select("Alignment"));
					string clickableUrl = getInnerTextFromFirstChild(iter.Current.Select("ClickableUrl"));
					string hideBorderString = getInnerTextFromFirstChild(iter.Current.Select("HideBorder"));

					if(startXString != null && startYString != null)
					{
						int startX = int.Parse(startXString);
						int startY = int.Parse(startYString);

						WorldWind.Renderable.ScreenOverlay overlay = new ScreenOverlay(name, startX, startY, imageUri);
					
						if(widthString != null)
						{
							overlay.Width = int.Parse(widthString);
						}
						if(heightString != null)
						{
							overlay.Height = int.Parse(heightString);
						}

						if(alignmentString != null)
						{
                            if (alignmentString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("left"))
							{
								overlay.Alignment = ScreenAlignment.Left;
							}
                            else if (alignmentString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("right"))
							{
								overlay.Alignment = ScreenAlignment.Right;
							}
						}

						if(clickableUrl != null && clickableUrl.Length > 0)
						{
							overlay.ClickableUrl = clickableUrl;
						}

						if(hideBorderString != null && hideBorderString.Length > 0)
							overlay.HideBorder = ParseBool(hideBorderString);

						string cachePath = String.Format("{0}{1}{2}{1}{3}{1}{3}", cache.CacheDirectory, Path.DirectorySeparatorChar, getRenderablePathString(icon), name);

						if(refreshTimeString != null && refreshTimeString.Length > 0)
						{
							overlay.RefreshTimeSec = ParseDouble(refreshTimeString);
						}

						string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
						if(description != null && description.Length > 0)
							overlay.Description = description;

						overlay.SaveFilePath = cachePath;
						addExtendedInformation(iter.Current.Select("ExtendedInformation"), overlay);

						string infoUri = iter.Current.GetAttribute("InfoUri", "");

						if(infoUri != null && infoUri.Length > 0)
						{
							if(overlay.MetaData.Contains("InfoUri"))
							{
								overlay.MetaData["InfoUri"] = infoUri;
							}
							else
							{
								overlay.MetaData.Add("InfoUri", infoUri);
							}
						}

						overlay.MetaData.Add("XmlSource", (string)icon.MetaData["XmlSource"]);
						overlay.ParentList = icon.ParentList;
						if(opacityString != null)
						{
							overlay.Opacity = byte.Parse(opacityString);
						}

						if(showHeaderString != null)
						{
							overlay.ShowHeader = ParseBool(showHeaderString);
						}

						if(World.Settings.useDefaultLayerStates)
						{
							overlay.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
						}
						else
						{
							overlay.IsOn = IsLayerOn(overlay);
						}

						icon.AddOverlay(overlay);
					}
				}
			}
		}
		
		private static System.Drawing.Color getColor(XPathNodeIterator iter)
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

		private static Microsoft.DirectX.Direct3D.FontDescription getDisplayFont(XPathNodeIterator iter)
		{
			Microsoft.DirectX.Direct3D.FontDescription fd = new Microsoft.DirectX.Direct3D.FontDescription();
			
			if(iter.MoveNext())
			{
				
				fd.FaceName = getInnerTextFromFirstChild(iter.Current.Select("Family"));
				fd.Height = (int) (float.Parse(getInnerTextFromFirstChild(iter.Current.Select("Size"))) * 1.5f);
			
				XPathNodeIterator styleIter = iter.Current.Select("Style");
				if(styleIter.Count > 0)
				{
					styleIter.MoveNext();

					string isBoldString = getInnerTextFromFirstChild(styleIter.Current.Select("IsBold"));
					string isItalicString = getInnerTextFromFirstChild(styleIter.Current.Select("IsItalic"));

					if(isBoldString != null)
					{
						bool isBold = ParseBool(isBoldString);
						if(isBold)
							fd.Weight = Microsoft.DirectX.Direct3D.FontWeight.Bold;
					}

					if(isItalicString != null)
					{
						bool isItalic = ParseBool(isItalicString);
						if(isItalic)
							fd.IsItalic = isItalic;
					}
				}
				else
				{
					fd.Weight = Microsoft.DirectX.Direct3D.FontWeight.Regular;
				}
			}
			
			return fd;
		}

		private static void addTiledPlacenameSet(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
					double minimumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MinimumDisplayAltitude")));
					double maximumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MaximumDisplayAltitude")));
					
					string placenameListFilePath = getInnerTextFromFirstChild(iter.Current.Select("PlacenameListFilePath"));

					if(!Path.IsPathRooted(placenameListFilePath))
					{
						Path.Combine(
							Path.GetDirectoryName(
							System.Windows.Forms.Application.ExecutablePath),
							placenameListFilePath);
					}
					string winColorName = getInnerTextFromFirstChild(iter.Current.Select("WinColorName"));
					string iconFilePath = getInnerTextFromFirstChild(iter.Current.Select("IconFilePath"));

					Microsoft.DirectX.Direct3D.FontDescription fd = getDisplayFont(iter.Current.Select("DisplayFont"));

					System.Drawing.Color c = System.Drawing.Color.White;

					if(winColorName != null)
					{
						c = System.Drawing.Color.FromName(winColorName);
					}
					else
					{
						c = getColor(iter.Current.Select("RGBColor"));
					}

					TiledPlacenameSet tps = new TiledPlacenameSet(
						name,
						parentWorld,
						distanceAboveSurface,
						maximumDisplayAltitude,
						minimumDisplayAltitude,
						placenameListFilePath,
						fd,
						c,
						iconFilePath);

					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
					if(description != null && description.Length > 0)
						tps.Description = description;

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), tps);

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if(infoUri != null && infoUri.Length > 0)
					{
						if(tps.MetaData.Contains("InfoUri"))
						{
							tps.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							tps.MetaData.Add("InfoUri", infoUri);
						}
					}

					tps.MetaData.Add("PlacenameDataFile", placenameListFilePath);
					tps.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
					tps.ParentList = parentRenderable;
					
					if(World.Settings.useDefaultLayerStates)
					{
						tps.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					}
					else
					{
						tps.IsOn = IsLayerOn(tps);
					}
					parentRenderable.ChildObjects.Add(
						tps
						);

					parentRenderable.RenderPriority = RenderPriority.Placenames;
					
				}
			}
		}

        private static void addTiledWFSPlacenameSet(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable, Cache cache)
        {
            if (iter.Count > 0)
            {
                while (iter.MoveNext())
                {
                    string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
                    double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
                    double minimumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MinimumDisplayAltitude")));
                    double maximumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MaximumDisplayAltitude")));

                    string wfsBaseUrl = getInnerTextFromFirstChild(iter.Current.Select("WFSBaseURL"));
                    string typename = getInnerTextFromFirstChild(iter.Current.Select("TypeName"));
                    string labelfield = getInnerTextFromFirstChild(iter.Current.Select("LabelField"));
                    /*
                    if (!Path.IsPathRooted(wfsBaseUrl))
                    {
                        Path.Combine(
                            Path.GetDirectoryName(
                            System.Windows.Forms.Application.ExecutablePath),
                            wfsBaseUrl);
                    }
                    */
                    string winColorName = getInnerTextFromFirstChild(iter.Current.Select("WinColorName"));
                    string iconFilePath = getInnerTextFromFirstChild(iter.Current.Select("IconFilePath"));

                    Microsoft.DirectX.Direct3D.FontDescription fd = getDisplayFont(iter.Current.Select("DisplayFont"));

                    System.Drawing.Color c = System.Drawing.Color.White;

                    if (winColorName != null)
                    {
                        c = System.Drawing.Color.FromName(winColorName);
                    }
                    else
                    {
                        c = getColor(iter.Current.Select("RGBColor"));
                    }
                    //TODO:Validate URL
                    //Construct WFS Base URL
                    wfsBaseUrl += "TypeName=" + typename + "&Request=GetFeature&Service=WFS";

                    TiledWFSPlacenameSet twps = new TiledWFSPlacenameSet(
                        name,
                        parentWorld,
                        distanceAboveSurface,
                        maximumDisplayAltitude,
                        minimumDisplayAltitude,
                        wfsBaseUrl,
                        typename,
                        labelfield,
                        fd,
                        c,
                        iconFilePath,
						cache);

                    string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
                    if (description != null && description.Length > 0)
                        twps.Description = description;

                    addExtendedInformation(iter.Current.Select("ExtendedInformation"), twps);

                    string infoUri = iter.Current.GetAttribute("InfoUri", "");

                    if (infoUri != null && infoUri.Length > 0)
                    {
                        if (twps.MetaData.Contains("InfoUri"))
                        {
                            twps.MetaData["InfoUri"] = infoUri;
                        }
                        else
                        {
                            twps.MetaData.Add("InfoUri", infoUri);
                        }
                    }

                    twps.MetaData.Add("WFSBaseURL", wfsBaseUrl);
                    twps.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
                    twps.ParentList = parentRenderable;

                    if (World.Settings.useDefaultLayerStates)
                    {
                        twps.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
                    }
                    else
                    {
                        twps.IsOn = IsLayerOn(twps);
                    }
                    parentRenderable.ChildObjects.Add(
                        twps
                        );

                    parentRenderable.RenderPriority = RenderPriority.Placenames;

                }
            }
        }

		private static void addIcon(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable, Cache cache)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					XPathNodeIterator nameIter = iter.Current.Select("Name");
					string name = getInnerTextFromFirstChild(nameIter);
		
					double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
					double latitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("Latitude/Value")));
					double longitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("Longitude/Value")));
					
					string refreshString = iter.Current.GetAttribute("Refresh", "");
					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));

					string isRotatedString = getInnerTextFromFirstChild(iter.Current.Select("IsRotated"));
					string rotationDegreesString =  getInnerTextFromFirstChild(iter.Current.Select("RotationDegrees"));

					string minimumDisplayAltitudeString = getInnerTextFromFirstChild(iter.Current.Select("MinimumDisplayAltitude"));
					string maximumDisplayAltitudeString = getInnerTextFromFirstChild(iter.Current.Select("MaximumDisplayAltitude"));

					string clickableUrl = getInnerTextFromFirstChild(iter.Current.Select("ClickableUrl"));
					
					string textureFilePath = getInnerTextFromFirstChild(iter.Current.Select("TextureFilePath"));

                    if (textureFilePath.Length > 0 && !Path.IsPathRooted(textureFilePath) && !textureFilePath.ToLower(System.Globalization.CultureInfo.InvariantCulture).StartsWith("http://"))
						// Use absolute path to icon image
						textureFilePath = Path.Combine( 
							Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), textureFilePath );

					if(!Path.IsPathRooted(textureFilePath))
					{
						Path.Combine(
							Path.GetDirectoryName(
							System.Windows.Forms.Application.ExecutablePath),
							textureFilePath);
					}
					
					WorldWind.Renderable.Icon ic = new WorldWind.Renderable.Icon(
						name,
						latitude,
						longitude,
						distanceAboveSurface );
					
					string nameAlwaysVisibleString = nameIter.Current.GetAttribute("AlwaysVisible", "");
					if(nameAlwaysVisibleString != null && nameAlwaysVisibleString.Length > 0)
					{
						ic.NameAlwaysVisible = ParseBool(nameAlwaysVisibleString);
					}

					ic.TextureFileName = textureFilePath;
					ic.Width =	int.Parse(getInnerTextFromFirstChild(iter.Current.Select("IconWidthPixels")));
					ic.Height = int.Parse(getInnerTextFromFirstChild(iter.Current.Select("IconHeightPixels")));
					
					if(refreshString != null && refreshString.Length > 0)
					{
						ic.RefreshInterval = TimeSpan.FromSeconds(ParseDouble(refreshString));
					}

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if(infoUri != null && infoUri.Length > 0)
					{
						if(ic.MetaData.Contains("InfoUri"))
						{
							ic.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							ic.MetaData.Add("InfoUri", infoUri);
						}
					}

					if(description != null)
						ic.Description = description;

					if(clickableUrl != null)
						ic.ClickableActionURL = clickableUrl;

					if(maximumDisplayAltitudeString != null)
						ic.MaximumDisplayDistance = ParseDouble(maximumDisplayAltitudeString);

					if(minimumDisplayAltitudeString != null)
						ic.MinimumDisplayDistance = ParseDouble(minimumDisplayAltitudeString);

					string onClickZoomAltString = getInnerTextFromFirstChild(iter.Current.Select("OnClickZoomAltitude"));
					string onClickZoomHeadingString = getInnerTextFromFirstChild(iter.Current.Select("OnClickZoomHeading"));
					string onClickZoomTiltString = getInnerTextFromFirstChild(iter.Current.Select("OnClickZoomTilt"));
					
					if(onClickZoomAltString != null)
						ic.OnClickZoomAltitude = ParseDouble(onClickZoomAltString);

					if(onClickZoomHeadingString != null)
						ic.OnClickZoomHeading = ParseDouble(onClickZoomHeadingString);

					if(onClickZoomTiltString != null)
						ic.OnClickZoomTilt = ParseDouble(onClickZoomTiltString);

					if(isRotatedString != null)
					{
						ic.IsRotated = ParseBool(isRotatedString);
					}

					if(rotationDegreesString != null)
					{
						if(!ic.IsRotated)
							ic.IsRotated = true;

						ic.Rotation = Angle.FromDegrees(ParseDouble(rotationDegreesString));
					}

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), ic);
					
					ic.ParentList = parentRenderable;

					string cachePath = String.Format("{0}{1}{2}{1}{3}{1}{3}", cache.CacheDirectory, Path.DirectorySeparatorChar, getRenderablePathString(parentRenderable), name);

					ic.SaveFilePath = cachePath;

					if(World.Settings.useDefaultLayerStates)
					{
						ic.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					}
					else
					{
						ic.IsOn = IsLayerOn(ic);
					}
					ic.MetaData["XmlSource"] = (string)parentRenderable.MetaData["XmlSource"];

					Icons parentIconList = (Icons)parentRenderable;

					parentIconList.Add(ic);

					addScreenOverlaysToIcon(iter.Current.Select("ScreenOverlay"), parentWorld, ic, cache);
				}
			}
		}

		private static void addPathList(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
					double minimumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MinDisplayAltitude")));
					double maximumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MaxDisplayAltitude")));
					
					string pathsDirectory = getInnerTextFromFirstChild(iter.Current.Select("PathsDirectory"));

					if(!Path.IsPathRooted(pathsDirectory))
					{
						Path.Combine(
							Path.GetDirectoryName(
							System.Windows.Forms.Application.ExecutablePath),
							pathsDirectory);
					}
					
					string winColorName = getInnerTextFromFirstChild(iter.Current.Select("WinColorName"));
					System.Drawing.Color c = System.Drawing.Color.White;

					if(winColorName != null)
					{
						c = System.Drawing.Color.FromName(winColorName);
					}
					else
					{
						c = getColor(iter.Current.Select("RGBColor"));
					}

					PathList pl = new PathList(
						name,
						parentWorld,
						minimumDisplayAltitude,
						maximumDisplayAltitude,
						pathsDirectory,
						distanceAboveSurface,
						c,
						parentWorld.TerrainAccessor
						);

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), pl);

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if(infoUri != null && infoUri.Length > 0)
					{
						if(pl.MetaData.Contains("InfoUri"))
						{
							pl.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							pl.MetaData.Add("InfoUri", infoUri);
						}
					}

					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
					if(description != null && description.Length > 0)
						pl.Description = description;

					pl.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
					pl.ParentList = parentRenderable;

					if(World.Settings.useDefaultLayerStates)
					{
						pl.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					}
					else
					{
						pl.IsOn = IsLayerOn(pl);
					}
					parentRenderable.ChildObjects.Add(
						pl
						);

					parentRenderable.RenderPriority = RenderPriority.LinePaths;
					
				}
			}
		}

		private static void addLineFeature(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					
					string distanceAboveSurfaceString = getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface"));
					string minimumDisplayAltitudeString = getInnerTextFromFirstChild(iter.Current.Select("MinimumDisplayAltitude"));
					string maximumDisplayAltitudeString = getInnerTextFromFirstChild(iter.Current.Select("MaximumDisplayAltitude"));
					string opacityString = getInnerTextFromFirstChild(iter.Current.Select("Opacity"));
					//string extrudeHeightString = getInnerTextFromFirstChild(iter.Current.Select("ExtrudeHeight"));
					string extrudeUpwardsString = getInnerTextFromFirstChild(iter.Current.Select("Extrude"));
					string imageUri = getInnerTextFromFirstChild(iter.Current.Select("ImageUri"));
					string outlineString = getInnerTextFromFirstChild(iter.Current.Select("Outline"));
					string lineWidthString = getInnerTextFromFirstChild(iter.Current.Select("LineWidth"));
                    string altitudemodeString = getInnerTextFromFirstChild(iter.Current.Select("AltitudeMode"));

					XPathNodeIterator posListIter = iter.Current.Select("LineString/posList");
					posListIter.MoveNext();

					string lineString = getInnerTextFromFirstChild(posListIter);
					
					string[] lineParts = lineString.Split(' ');
					Point3d[] points = new Point3d[lineParts.Length];
					
					for(int i = 0; i < lineParts.Length; i++)
					{
						string[] pointParts = lineParts[i].Split(',');
						points[i] = new Point3d();
						points[i].X = ParseDouble(pointParts[0]);
						points[i].Y = ParseDouble(pointParts[1]);
						
						if(pointParts.Length > 2)
							points[i].Z = ParseDouble(pointParts[2]);

					}

					System.Drawing.Color c = System.Drawing.Color.Black;
					
					if(iter.Current.Select("RGBColor").Count > 0)
					{
						c =	getColor(iter.Current.Select("RGBColor"));
					}

					if(iter.Current.Select("FeatureColor").Count > 0)
					{
						c =	getColor(iter.Current.Select("FeatureColor"));
					}

					LineFeature lf = null;
					
					if(imageUri != null && imageUri.Length > 0)
					{
						lf = new LineFeature(name, parentWorld, points, imageUri);
					}
					else
					{
						lf = new LineFeature(
							name,
							parentWorld,
							points,
							c);
					}

					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
					if(description != null && description.Length > 0)
						lf.Description = description;

					if(iter.Current.Select("OutlineColor").Count > 0)
					{
						lf.LineColor =	getColor(iter.Current.Select("OutlineColor"));
					}
					
					if(lineWidthString != null)
						lf.LineWidth = (float)ParseDouble(lineWidthString);

					if(outlineString != null && outlineString.Length > 0)
						lf.Outline = ParseBool(outlineString);

					if(opacityString != null && opacityString.Length > 0)
						lf.Opacity = byte.Parse(opacityString);

					if(distanceAboveSurfaceString != null && distanceAboveSurfaceString.Length > 0)
						lf.DistanceAboveSurface = ParseDouble(distanceAboveSurfaceString);

					if(minimumDisplayAltitudeString != null && minimumDisplayAltitudeString.Length > 0)
						lf.MinimumDisplayAltitude = ParseDouble(minimumDisplayAltitudeString);

					if(maximumDisplayAltitudeString != null && maximumDisplayAltitudeString.Length > 0)
						lf.MaximumDisplayAltitude = ParseDouble(maximumDisplayAltitudeString);

                    if (altitudemodeString != null && altitudemodeString.Length > 0)
                    {
                        if (altitudemodeString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("absolute")) lf.AltitudeMode = AltitudeMode.Absolute;
                        if (altitudemodeString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("relativetoground")) lf.AltitudeMode = AltitudeMode.RelativeToGround;
                        if (altitudemodeString.ToLower(System.Globalization.CultureInfo.InvariantCulture).Equals("clampedtoground")) lf.AltitudeMode = AltitudeMode.ClampedToGround;
                    }

                    if(extrudeUpwardsString!=null)
                    {
                        bool extrude = Convert.ToBoolean(extrudeUpwardsString);
                        lf.Extrude = extrude;
                    }

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), lf);

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if(infoUri != null && infoUri.Length > 0)
					{
						if(lf.MetaData.Contains("InfoUri"))
						{
							lf.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							lf.MetaData.Add("InfoUri", infoUri);
						}
					}

					lf.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
					lf.ParentList = parentRenderable;

					if(World.Settings.useDefaultLayerStates)
					{
						lf.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					}
					else
					{
						lf.IsOn = IsLayerOn(lf);
					}
					parentRenderable.ChildObjects.Add(
						lf
						);

					parentRenderable.RenderPriority = RenderPriority.LinePaths;
					
				}
			}
		}

		private static void addPolygonFeature(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
		{
			if(iter.Count > 0)
			{
				while(iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					
					string distanceAboveSurfaceString = getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface"));
					string minimumDisplayAltitudeString = getInnerTextFromFirstChild(iter.Current.Select("MinimumDisplayAltitude"));
					string maximumDisplayAltitudeString = getInnerTextFromFirstChild(iter.Current.Select("MaximumDisplayAltitude"));
					string opacityString = getInnerTextFromFirstChild(iter.Current.Select("Opacity"));
					string extrudeHeightString = getInnerTextFromFirstChild(iter.Current.Select("ExtrudeHeight"));
					string extrudeUpwardsString = getInnerTextFromFirstChild(iter.Current.Select("ExtrudeUpwards"));
					string imageUri = getInnerTextFromFirstChild(iter.Current.Select("ImageUri"));
					string outlineString = getInnerTextFromFirstChild(iter.Current.Select("Outline"));

					XPathNodeIterator posListIter = iter.Current.Select("exterior/LinearRing/posList");
					posListIter.MoveNext();

					string lineString = getInnerTextFromFirstChild(posListIter);
					
					string[] lineParts = lineString.Split(' ');
					Point3d[] points = new Point3d[lineParts.Length];
					
					for(int i = 0; i < lineParts.Length; i++)
					{
						string[] pointParts = lineParts[i].Split(',');
						points[i] = new Point3d();
						points[i].X = ParseDouble(pointParts[0]);
						points[i].Y = ParseDouble(pointParts[1]);
						
						if(pointParts.Length > 2)
							points[i].Z = ParseDouble(pointParts[2]);

					}

					System.Drawing.Color c = System.Drawing.Color.Black;
					System.Drawing.Color outlineColor = System.Drawing.Color.Black;
					
					if(iter.Current.Select("FeatureColor").Count > 0)
					{
						c =	getColor(iter.Current.Select("FeatureColor"));
					}

					if(iter.Current.Select("OutlineColor").Count > 0)
					{
						outlineColor =	getColor(iter.Current.Select("OutlineColor"));
					}

					PolygonFeature pf = null;
					
					LinearRing outerRing = new LinearRing();
					outerRing.Points = points;

					pf = new PolygonFeature(name, parentWorld, outerRing, null, c);

					pf.OutlineColor = outlineColor;

					if(outlineString != null && outlineString.Length > 0)
						pf.Outline = ParseBool(outlineString);

					if(opacityString != null && opacityString.Length > 0)
						pf.Opacity = byte.Parse(opacityString);

					if(distanceAboveSurfaceString != null && distanceAboveSurfaceString.Length > 0)
						pf.DistanceAboveSurface = ParseDouble(distanceAboveSurfaceString);

					if(minimumDisplayAltitudeString != null && minimumDisplayAltitudeString.Length > 0)
						pf.MinimumDisplayAltitude = ParseDouble(minimumDisplayAltitudeString);

					if(maximumDisplayAltitudeString != null && maximumDisplayAltitudeString.Length > 0)
						pf.MaximumDisplayAltitude = ParseDouble(maximumDisplayAltitudeString);

					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
					if(description != null && description.Length > 0)
						pf.Description = description;

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), pf);

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if(infoUri != null && infoUri.Length > 0)
					{
						if(pf.MetaData.Contains("InfoUri"))
						{
							pf.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							pf.MetaData.Add("InfoUri", infoUri);
						}
					}

					pf.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
					pf.ParentList = parentRenderable;

					if(World.Settings.useDefaultLayerStates)
					{
						pf.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					}
					else
					{
						pf.IsOn = IsLayerOn(pf);
					}
					parentRenderable.ChildObjects.Add(
						pf
						);

					parentRenderable.RenderPriority = RenderPriority.LinePaths;
					
				}
			}
		}

        private static void addModelFeature(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
        {
            if (iter.Count > 0)
            {
                while (iter.MoveNext())
                {
                    string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
                    float lat = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("Latitude")), CultureInfo.InvariantCulture);
                    float lon = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("Longitude")), CultureInfo.InvariantCulture);
                    float alt = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")), CultureInfo.InvariantCulture);
                    float scaleFactor = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("ScaleFactor")), CultureInfo.InvariantCulture);
                    string meshFilePath = getInnerTextFromFirstChild(iter.Current.Select("MeshFilePath"));

                    float rotX = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.SelectSingleNode("Orientation")
                        .Select("RotationX")), CultureInfo.InvariantCulture);
                    float rotY = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.SelectSingleNode("Orientation")
                        .Select("RotationY")), CultureInfo.InvariantCulture);
                    float rotZ = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.SelectSingleNode("Orientation")
                        .Select("RotationZ")), CultureInfo.InvariantCulture);

                    ModelFeature model = new ModelFeature(name, parentWorld
                        , meshFilePath, lat, lon, alt,scaleFactor,rotX,rotY,rotZ);
                     if(iter.Current.Select("IsVerticalExaggerable").Count > 0)
                    {
						model.isVertExaggerable = Convert.ToBoolean(getInnerTextFromFirstChild(iter.Current.Select("IsVerticalExaggerable")));
					}
                    if(iter.Current.Select("IsElevationRelativeToGround").Count > 0)
                    {
						model.isElevationRelative2Ground = Convert.ToBoolean(getInnerTextFromFirstChild(iter.Current.Select("IsElevationRelativeToGround")));
					}
                   parentRenderable.Add(model);
                }
            }
        }
        /*
        private static void addWater(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable)
        {
            if (iter.Count > 0)
            {
                while (iter.MoveNext())
                {
                    string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
                    bool isBumpMapped = getInnerTextFromFirstChild(iter.Current.Select("EffectType")).Equals("Bump");
                    float lat = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("Latitude")));
                    float lon = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("Longitude")));
                    float alt = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
                    float scaleFactor = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.Select("ScaleFactor")));
                    /*
                    float rotX = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.SelectSingleNode("Orientation")
                        .Select("RotationX")));
                    float rotY = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.SelectSingleNode("Orientation")
                        .Select("RotationY")));
                    float rotZ = Convert.ToSingle(getInnerTextFromFirstChild(iter.Current.SelectSingleNode("Orientation")
                        .Select("RotationZ")));
                    */
	    /*
                    Water waterbody = new Water(name, parentWorld,isBumpMapped,lat,lon,alt,scaleFactor);
                    parentRenderable.Add(waterbody);
                }
            }
        }
        */ 

		private static TerrainAccessor[] getTerrainAccessorsFromXPathNodeIterator(XPathNodeIterator iter, string cacheDirectory)
		{
			System.Collections.ArrayList terrainAccessorList = new System.Collections.ArrayList();

			while(iter.MoveNext())
			{
				string terrainAccessorName = iter.Current.GetAttribute("Name", "");
				if(terrainAccessorName == null)
				{
					// TODO: Throw exception?
					continue;
				}

				XPathNodeIterator latLonBoxIter = iter.Current.Select("LatLonBoundingBox");
				if(latLonBoxIter.Count != 1)
				{
					// TODO: Throw exception?
					continue;
				}
				
				double north = 0;
				double south = 0;
				double west = 0;
				double east = 0;

				latLonBoxIter.MoveNext();

				north = ParseDouble(getInnerTextFromFirstChild(latLonBoxIter.Current.Select("North")));
				south = ParseDouble(getInnerTextFromFirstChild(latLonBoxIter.Current.Select("South")));
				west = ParseDouble(getInnerTextFromFirstChild(latLonBoxIter.Current.Select("West")));
				east = ParseDouble(getInnerTextFromFirstChild(latLonBoxIter.Current.Select("East")));
				

				TerrainAccessor[] higerResolutionSubsets = getTerrainAccessorsFromXPathNodeIterator(
					iter.Current.Select("HigherResolutionSubsets"),
					Path.Combine(cacheDirectory, terrainAccessorName));

				XPathNodeIterator tileServiceIter = iter.Current.Select("TerrainTileService");
				if(tileServiceIter.Count == 1)
				{
					string serverUrl = null;
					string dataSetName = null;
					double levelZeroTileSizeDegrees = double.NaN;
					uint numberLevels = 0;
					uint samplesPerTile = 0;
					string dataFormat = null;
					string fileExtension = null;
					string compressionType = null;

					tileServiceIter.MoveNext();
					
					serverUrl = getInnerTextFromFirstChild(tileServiceIter.Current.Select("ServerUrl"));
					dataSetName = getInnerTextFromFirstChild(tileServiceIter.Current.Select("DataSetName"));
					levelZeroTileSizeDegrees = ParseDouble(getInnerTextFromFirstChild(tileServiceIter.Current.Select("LevelZeroTileSizeDegrees")));
					numberLevels = uint.Parse(getInnerTextFromFirstChild(tileServiceIter.Current.Select("NumberLevels")));
					samplesPerTile = uint.Parse(getInnerTextFromFirstChild(tileServiceIter.Current.Select("SamplesPerTile")));
					dataFormat = getInnerTextFromFirstChild(tileServiceIter.Current.Select("DataFormat"));
					fileExtension = getInnerTextFromFirstChild(tileServiceIter.Current.Select("FileExtension"));
					compressionType = getInnerTextFromFirstChild(tileServiceIter.Current.Select("CompressonType"));

					TerrainTileService tts = new TerrainTileService(
						serverUrl,
						dataSetName,
						levelZeroTileSizeDegrees,
						(int)samplesPerTile,
						fileExtension,
						(int)numberLevels,
						Path.Combine(cacheDirectory, terrainAccessorName),
						World.Settings.TerrainTileRetryInterval,
						dataFormat);

					TerrainAccessor newTerrainAccessor = new NltTerrainAccessor(
						terrainAccessorName,
						west,
						south,
						east,
						north,
						tts,
						higerResolutionSubsets);

					terrainAccessorList.Add(newTerrainAccessor);
				}
				//TODO: Add Floating point terrain Accessor code
				//TODO: Add WMSAccessor code and make it work in TerrainAccessor (which it currently doesn't)
			}

			if(terrainAccessorList.Count > 0)
			{
				return (TerrainAccessor[])terrainAccessorList.ToArray(typeof(TerrainAccessor));
			}
			else
			{
				return null;
			}
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
	}
}
