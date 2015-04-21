using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microsoft.DirectX;
using WorldWind.Menu;
using WorldWind.VisualControl;

namespace WorldWind.Renderable
{
	/// <summary>
	/// The base class for objects to be rendered as part of the scene.
	/// </summary>
	public abstract class RenderableObject : IRenderable, IComparable
	{
		/// <summary>
		/// True when object is ready to be rendered.
		/// </summary>
		public bool isInitialized;

		/// <summary>
		/// True for objects the user can interact with.
		/// </summary>
		public bool isSelectable;

		public RenderableObjectList ParentList;

		public string dbfPath = "";
		public bool dbfIsInZip = false;
		

		protected string name;
		protected string m_description = null;
		protected Hashtable _metaData = new Hashtable();
		protected Vector3 position;
		protected Quaternion orientation;
		protected bool isOn = true;		
		protected byte m_opacity = 255;
		protected RenderPriority m_renderPriority = RenderPriority.SurfaceImages;
		protected Form m_propertyBrowser;

		protected Image m_thumbnailImage;
		protected string m_iconImagePath;
		protected Image m_iconImage;
		protected World m_world;
		string m_thumbnail;

		public string Description
		{
			get{ return m_description; }
			set{ m_description = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.RenderableObject"/> class.
		/// </summary>
		/// <param name="name">Object description</param>
		protected RenderableObject(string name)
		{
			this.name = name;
		}

		protected RenderableObject(string name, World parentWorld)
		{
			this.name = name;
			this.m_world = parentWorld;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Object description</param>
		/// <param name="position">Object position (XYZ world coordinates)</param>
		/// <param name="orientation">Object rotation (Quaternion)</param>
		protected RenderableObject(string name, Vector3 position, Quaternion orientation)
		{
			this.name = name;
			this.position = position;
			this.orientation = orientation;
		}
		
		public abstract void Initialize(DrawArgs drawArgs);

		public abstract void Update(DrawArgs drawArgs);

		public abstract void Render(DrawArgs drawArgs);

		public virtual bool Initialized
		{	
			get
			{
				return isInitialized;
			}	
		}

		/// <summary>
		/// The planet this layer is a part of.
		/// </summary>
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public virtual World World
		{
			get
			{
				return m_world;
			}
		}

		/// <summary>
		/// Path to a Thumbnail image(e.g. for use as a Toolbar button).
		/// </summary>
		public virtual string Thumbnail
		{
			get
			{
				return m_thumbnail;
			}
			set
			{
				m_thumbnail = ImageHelper.FindResource(value);
			}
		}

		/// <summary>
		/// The image referenced by Thumbnail. 
		/// </summary>
		public virtual Image ThumbnailImage
		{
			get
			{
				if(m_thumbnailImage==null)
				{
					if(m_thumbnail==null)
						return null;
					try
					{
						if(File.Exists(m_thumbnail))
							m_thumbnailImage = ImageHelper.LoadImage(m_thumbnail);
					}
					catch {}
				}
				return m_thumbnailImage;
			}
		}

		/// <summary>
		/// Path for an icon for the object, such as an image to be used in the Active Layer window.
		/// This can be different than the Thumbnail(e.g. an ImageLayer can have an IconImage, and no Thumbnail).
		/// </summary>
		public string IconImagePath
		{
			get
			{
				return m_iconImagePath;
			}
			set
			{
				m_iconImagePath = value;
			}
		}

		/// <summary>
		/// The icon image referenced by IconImagePath. 
		/// </summary>
		public Image IconImage
		{
			get
			{
				if(m_iconImage==null)
				{
					if(m_iconImagePath==null)
						return null;
					try
					{
						if(File.Exists(m_iconImagePath))
							m_iconImage = ImageHelper.LoadImage(m_iconImagePath);
					}
					catch {}
				}
				return m_iconImage;
			}
		}

		/// <summary>
		/// Called when object is disabled.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// User interaction (mouse click)
		/// </summary>
		public abstract bool PerformSelectionAction(DrawArgs drawArgs);

		public int CompareTo(object obj)
		{
			RenderableObject robj = obj as RenderableObject;
			if(obj == null)
				return 1;

			return this.m_renderPriority.CompareTo(robj.RenderPriority);
		}

		/// <summary>
		/// Permanently delete the layer
		/// </summary>
		public virtual void Delete()
		{

			RenderableObjectList list = this.ParentList;

			string xmlConfigFile = (string)this.MetaData["XmlSource"];

			if (this.ParentList.Name == "Earth" & xmlConfigFile != null)
			{

				string message = "Permanently delete layer '" + this.Name + "' and rename its .xml config file to .bak?";
				if (DialogResult.Yes != MessageBox.Show(message, "Delete layer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2))
					return;
					//throw new Exception("Delete cancelled.");

				//MessageBox.Show("Checking xml source of " + this.Name);

				if (xmlConfigFile.Contains("http"))
					throw new Exception("Can't delete network layers.");

				//MessageBox.Show("Found " + xmlConfigFile);
				if (File.Exists(xmlConfigFile.Replace(".xml", ".bak")))
				{
					//MessageBox.Show("Deleting old .bak");
					File.Delete(xmlConfigFile.Replace(".xml", ".bak"));

				}
				//MessageBox.Show("Moving old .xml");
				File.Move(xmlConfigFile, xmlConfigFile.Replace(".xml", ".bak"));

				//MessageBox.Show("File backed up, now removing from LM");
				this.ParentList.Remove(this);
				//World.RenderableObjects.Remove(this);

				//MessageBox.Show("Removed");
			}
			else if (xmlConfigFile == null)
			{
				string message = "Delete plugin layer '" + this.Name + "'?\n\nThis may cause problems for a running plugin that expects the layer to be\nthere.  Restart the plugin in question to replace the layer after deleting.";
				if (DialogResult.Yes != MessageBox.Show(message, "Delete layer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2))
					return;
					//throw new Exception("Delete cancelled.");

				this.ParentList.Remove(this);
			}
			else
			{
				throw new Exception("Can't delete this sub-item from the layer manager.  Try deleting the top-level entry for this layer.");
			}




			// Needs re-thinking...
			/*
			string xmlConfigFile = (string)MetaData["XmlSource"];
			//MessageBox.Show(xmlConfigFile);
			if(xmlConfigFile == null)
				xmlConfigFile = (string)ParentList.MetaData["XmlSource"];																					   
				
			if(xmlConfigFile == null || !File.Exists(xmlConfigFile))
				throw new ApplicationException("Error deleting layer.");
				
			XmlDocument doc = new XmlDocument();
			doc.Load(xmlConfigFile);

			XmlNodeList list;
			XmlElement root = doc.DocumentElement;
			list = root.SelectNodes("//ChildLayerSet[@Name='" + namestring + "'] || //*[Name='" + namestring + "']");
			MessageBox.Show("Checked childlayersets, returned " + list[0].ToString());

			ParentList.Remove(Name);
			MessageBox.Show("Removed " + Name + ", now changing " + xmlConfigFile);

			if (list != null)
				MessageBox.Show("Removing " + list[0].ToString() + " from xml");

			list[0].ParentNode.RemoveChild(list[0]);
			MessageBox.Show("node removed, now saving file");

			if (File.Exists(xmlConfigFile.Replace(".xml", ".bak")))
			{
				MessageBox.Show("Deleting old .bak");
				File.Delete(xmlConfigFile.Replace(".xml", ".bak"));

			}
			MessageBox.Show("Moving old .xml");
			File.Move(xmlConfigFile, xmlConfigFile.Replace(".xml",".bak"));

			MessageBox.Show("Saving new xml");
			doc.Save(xmlConfigFile);
			*/
		}

		/// <summary>
		/// Fills the context menu with menu items specific to the layer.
		/// </summary>
		/// <param name="menu">Pre-initialized context menu.</param>
		public virtual void BuildContextMenu( ContextMenu menu )
		{
			menu.MenuItems.Add("Goto", new EventHandler(OnGotoClick));
			menu.MenuItems.Add("Properties...", new EventHandler(OnPropertiesClick));
			menu.MenuItems.Add("Info...", new EventHandler(OnInfoClick));
			menu.MenuItems.Add("Delete...", new EventHandler(OnDeleteClick)); 

			
			if(dbfPath != "")
				menu.MenuItems.Add("Dbf Info", new EventHandler(OnDbfInfo));
			
			
		}

		/// <summary>
		/// Returns a String that represents the current SelectedObject.
		/// </summary>
		public override string ToString()
		{
			return name;
		}

		#region Properties

		/// <summary>
		/// The object's render priority determining in what order it will be rendered
		/// compared to the other objects.
		/// </summary>
		[Description("The object's render priority determining in what order it will be rendered compared to the other objects.")]
		public virtual RenderPriority RenderPriority
		{
			get
			{
				return this.m_renderPriority;
			}
			set
			{
				this.m_renderPriority = value;
				if(ParentList != null)
					ParentList.SortChildren();
			}
		}

		/// <summary>
		/// How transparent this object should appear (0=invisible, 255=opaque)
		/// </summary>
		[Description("Controls the amount of light allowed to pass through this object. (0=invisible, 255=opaque).")]
		public virtual byte Opacity
		{
			get
			{
				return this.m_opacity;
			}
			set
			{
				this.m_opacity = value;
				if(value == 0)
				{
					// invisible - turn off
					if(this.isOn)
						this.IsOn = false;
				}
				else
				{
					// visible - turn back on
					if(!this.isOn)
						this.IsOn = true;
				}
			}
		}

		[Browsable(false)]
		public virtual Hashtable MetaData
		{
			get
			{
				return this._metaData;
			}
		}

		/// <summary>
		/// Hide/Show this object.
		/// </summary>
		[Description("This layer's enabled status.")]
		public virtual bool IsOn
		{
			get
			{
				return this.isOn;
			}
			set
			{
				if(isOn && !value)
					this.Dispose();
				this.isOn = value;
			}
		}

		/// <summary>
		/// Describes this object
		/// </summary>
		[Description("This layer's name.")]
		public virtual string Name 
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		/// <summary>
		/// Object position (XYZ world coordinates)
		/// </summary>
		[Browsable(false)]
		public virtual Vector3 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = value;
			}
		}

		/// <summary>
		/// Object rotation (Quaternion)
		/// </summary>
		[Browsable(false)]
		public virtual Quaternion Orientation
		{
			get
			{
				return this.orientation;
			}
			set
			{
				this.orientation = value;
			}
		}

		#endregion

		#region Menu items

		///<summary>
		///  Goes to the Shapefiles's DBF Information Window
		/// </summary>
		protected virtual void OnDbfInfo(object sender, EventArgs e)
		{			
			ShapeFileInfoDlg sfid = new ShapeFileInfoDlg(dbfPath, dbfIsInZip);
			sfid.Show();	
		}

		///<summary>
		///  Goes to the extent specified by the bounding box for the QTS layer
        ///  or to the lat/lon for icons
		/// </summary>
		protected virtual void OnGotoClick(object sender, EventArgs e)
		{
			lock(this.ParentList.ChildObjects.SyncRoot)
			{
				for(int i = 0; i < this.ParentList.ChildObjects.Count; i++)
				{
					RenderableObject ro = (RenderableObject)this.ParentList.ChildObjects[i];
					if(ro.Name.Equals(name))
					{
                        if (ro is QuadTileSet)
                        {
                            QuadTileSet qts = (QuadTileSet)ro;
                            DrawArgs.Camera.SetPosition((qts.North + qts.South) / 2, (qts.East + qts.West) / 2);
                            double perpendicularViewRange = (qts.North - qts.South > qts.East - qts.West ? qts.North - qts.South : qts.East - qts.West);
                            double altitude = qts.LayerRadius * Math.Sin(MathEngine.DegreesToRadians(perpendicularViewRange * 0.5));

                            DrawArgs.Camera.Altitude = altitude;

                            break;
                        }
                        if (ro is Icon)
						{
							Icon ico = (Icon)ro;
							DrawArgs.Camera.SetPosition(ico.Latitude,ico.Longitude);
							DrawArgs.Camera.Altitude/=2;
				
							break;
						}
                        if (ro is ShapeFileLayer)
                        {
                            ShapeFileLayer slayer = (ShapeFileLayer)ro;
                            DrawArgs.Camera.SetPosition((slayer.North + slayer.South) / 2, (slayer.East + slayer.West) / 2);
                            double perpendicularViewRange = (slayer.North - slayer.South > slayer.East - slayer.West ? slayer.North - slayer.South : slayer.East - slayer.West);
                            double altitude = slayer.MaxAltitude;

                            DrawArgs.Camera.Altitude = altitude;

                            break;
                        }
					}
				}
			}
		}

		
		/// <summary>
		/// Layer info context menu item
		/// </summary>
		protected virtual void OnInfoClick(object sender, EventArgs e)
		{
			LayerManagerItemInfo lmii = new LayerManagerItemInfo(MetaData);
			lmii.ShowDialog();
		}

		/// <summary>
		/// Layer properties context menu item
		/// </summary>
		protected virtual void OnPropertiesClick(object sender, EventArgs e)
		{
			if(m_propertyBrowser!=null)
				m_propertyBrowser.Dispose();

			m_propertyBrowser = new PropertyBrowser(this);
			m_propertyBrowser.Show();
		}

		/// <summary>
		/// Delete layer context menu item
		/// </summary>
		protected virtual void OnDeleteClick(object sender, EventArgs e)
		{
			//World w = this.World;

			/*if (this.ParentList.Name != "Earth")
			{
				MessageBox.Show("Can't delete sub-items from layers.  Try deleting the top-level layer.", "Error deleting layer");
				return;
			}*/

			//MessageBox.Show("Delete click fired");


			try
			{
				this.Delete();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message,"Layer Delete");
			}

		}

		#endregion

	}

	/// <summary>
	/// The render priority determines in what order objects are rendered.
	/// Objects with higher priority number are rendered over lower priority objects.
	/// </summary>
	public enum RenderPriority
	{
		SurfaceImages = 0,
		TerrainMappedImages = 100,
		AtmosphericImages = 200,
		LinePaths = 300,
		Icons = 400,
		Placenames = 500,
		Custom = 600
	}
}
