using System;
using System.ComponentModel;
using System.Collections;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Represents a parent node in the layer manager tree.  Contains a list of sub-nodes.
	/// </summary>
	public class RenderableObjectList : RenderableObject
	{
		protected ArrayList m_children = new ArrayList();
		
		string m_DataSource = null;
		TimeSpan m_RefreshInterval = TimeSpan.MaxValue;

		World m_ParentWorld = null;
		Cache m_Cache = null;
		System.Timers.Timer m_RefreshTimer = null;
		public bool ShowOnlyOneLayer;
		
		private bool m_disableExpansion = false;
		public bool DisableExpansion
		{
			get{ return m_disableExpansion; }
			set{ m_disableExpansion = value; }
		}

		public System.Timers.Timer RefreshTimer
		{
			get
			{
				return m_RefreshTimer;
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.RenderableObjectList"/> class.
		/// </summary>
		/// <param name="name"></param>
		public RenderableObjectList(string name) : base(name, new Vector3(0,0,0), new Quaternion())
		{
			this.isSelectable = true;
		}

		public RenderableObjectList(
			string name, 
			string dataSource, 
			TimeSpan refreshInterval,
			World parentWorld,
			Cache cache
			) : base(name, new Vector3(0,0,0), new Quaternion())
		{
			isSelectable = true;
			m_DataSource = dataSource;
			m_RefreshInterval = refreshInterval;

			m_ParentWorld = parentWorld;
			m_Cache = cache;

			m_RefreshTimer = new System.Timers.Timer(
				refreshInterval.Hours * 60 * 60 * 1000 +
				refreshInterval.Minutes * 60 * 1000 + 
				refreshInterval.Seconds * 1000
				);
			m_RefreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_RefreshTimer_Elapsed);
		}

		public void StartRefreshTimer()
		{
			if(m_RefreshTimer != null)
			{
				m_RefreshTimer.Start();
			}
		}

		public virtual RenderableObject GetObject(string name)
		{
			try
			{
				foreach(RenderableObject ro in this.m_children)
				{
					if(ro.Name.Equals(name))
						return ro;
				}
			}
			catch
			{	
			}
			return null;
		}

		/// <summary>
		/// Enables layer with specified name
		/// </summary>
		/// <returns>False if layer not found.</returns>
		public virtual bool Enable(string name)
		{
			if(name==null || name.Length==0)
				return true;

			string lowerName = name.ToLower();
			foreach(RenderableObject ro in m_children)
			{
				if(ro.Name.ToLower()==lowerName)
				{
					ro.IsOn = true;
					return true;
				}

				RenderableObjectList rol = ro as RenderableObjectList;
				if(rol==null)
					continue;

				// Recurse down
				if(rol.Enable(name))
				{
					rol.isOn = true;
					return true;
				}
			}

			return false;
		}

		public virtual void RemoveAll()
		{
			try
			{	
				while(m_children.Count > 0)
				{
					RenderableObject ro = (RenderableObject)m_children[0];
					m_children.RemoveAt(0);
					ro.Dispose();
				}
			}
			catch{}
		}

		public virtual void TurnOffAllChildren()
		{
			foreach(RenderableObject ro in this.m_children)
				ro.IsOn = false;
		}

		/// <summary>
		/// List containing the children layers
		/// </summary>
		[Browsable(false)]
		public virtual ArrayList ChildObjects
		{
			get
			{
				return this.m_children;
			}
		}

		/// <summary>
		/// Number of child objects.
		/// </summary>
		[Browsable(false)]
		public virtual int Count
		{
			get
			{
				return this.m_children.Count;
			}
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if(!this.IsOn)
				return;

			try
			{
				foreach(RenderableObject ro in this.m_children)
				{
					try
					{
						if(ro.IsOn)
							ro.Initialize(drawArgs);
					}
					catch(Exception caught)
					{
						Log.Write(Log.Levels.Error, "ROBJ", string.Format("{0}: {1} ({2})", 
							Name, caught.Message,  ro.Name));
					}
				}
			}
			catch
			{}

			this.isInitialized = true;
		}


		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if(!this.IsOn)
					return;

				if(!this.isInitialized)
					this.Initialize(drawArgs);

				
				foreach(RenderableObject ro in this.m_children)
				{
					if(ro.ParentList == null)
						ro.ParentList = this;

					if(ro.IsOn)
					{
						ro.Update(drawArgs);
					}
				}
			}
			catch(Exception)
			{
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			try
			{
				if(!this.IsOn)
					return false;

				foreach(RenderableObject ro in this.m_children)
				{
					if(ro.IsOn && ro.isSelectable)
					{
						if (ro.PerformSelectionAction(drawArgs))
							return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			try
			{
				if(!this.IsOn)
					return;

				lock(this.m_children.SyncRoot)
				{
					foreach(RenderableObject ro in this.m_children)
					{
						if(ro.IsOn)
							ro.Render(drawArgs);
					}
				}
			}
			catch
			{
			}
		}

		public override void Dispose()
		{
			try
			{
				this.isInitialized = false;

				foreach(RenderableObject ro in this.m_children)
					ro.Dispose();

				if(m_RefreshTimer != null && m_RefreshTimer.Enabled)
					m_RefreshTimer.Stop();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Add a child object to this layer.
		/// </summary>
		public virtual void Add(RenderableObject ro)
		{
			try
			{
				lock(this.m_children.SyncRoot)
				{
					RenderableObjectList dupList = null;
					RenderableObject duplicate = null;
					ro.ParentList = this;
					foreach(RenderableObject childRo in m_children)
					{
						if(childRo is RenderableObjectList && childRo.Name == ro.Name)
						{
							dupList = (RenderableObjectList)childRo;
							break;
						}
						else if(childRo.Name == ro.Name)
						{
							duplicate = childRo;
							break;
						}
					}

					if(dupList != null)
					{
						RenderableObjectList rol = (RenderableObjectList)ro;

						foreach(RenderableObject childRo in rol.ChildObjects)
						{
							dupList.Add(childRo);
						}
					}
					else
					{
						if(duplicate != null)
						{
							for(int i = 1; i < 100; i++)
							{
								ro.Name = string.Format("{0} [{1}]", duplicate.Name, i);
								bool found = false;
								foreach(RenderableObject childRo in m_children)
								{
									if(childRo.Name == ro.Name)
									{
										found = true;
										break;
									}
								}
							
								if(!found)
								{
									break;
								}
							}
						}
						
						m_children.Add(ro);
					}
					SortChildren();
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Removes a layer from the child layer list
		/// </summary>
		/// <param name="objectName">Name of object to remove</param>
		public virtual void Remove(string objectName)
		{
			lock(this.m_children.SyncRoot)
			{
				for(int i = 0; i < this.m_children.Count; i++)
				{
					RenderableObject ro = (RenderableObject)this.m_children[i];
					if(ro.Name.Equals(objectName))
					{
						this.m_children.RemoveAt(i);
						ro.Dispose();
						ro.ParentList = null;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Removes a layer from the child layer list
		/// </summary>
		/// <param name="layer">Layer to be removed.</param>
		public virtual void Remove(RenderableObject layer)
		{
			lock(this.m_children.SyncRoot)
			{
				this.m_children.Remove(layer);
				layer.Dispose();
				layer.ParentList = null;
			}
		}

		/// <summary>
		/// Sorts the children list according to priority
		/// TODO: Redesign the render tree to perhaps a list, to enable proper sorting 
		/// </summary>
		public virtual void SortChildren()
		{
			int index = 0;
			while(index+1 < m_children.Count)
			{
				RenderableObject a = (RenderableObject)m_children[index];
				RenderableObject b = (RenderableObject)m_children[index+1];
				if(a.RenderPriority > b.RenderPriority)
				{
					// Swap
					m_children[index] = b;
					m_children[index+1] = a;
					index = 0;
					continue;
				}
				index++;
			}
		}

		private void UpdateRenderable(RenderableObject oldRenderable, RenderableObject newRenderable)
		{
			if(oldRenderable is Icon && newRenderable is Icon)
			{
				Icon oldIcon = (Icon)oldRenderable;
				Icon newIcon = (Icon)newRenderable;

				oldIcon.SetPosition( (float)newIcon.Latitude, (float)newIcon.Longitude, (float)newIcon.Altitude);
			}
			else if(oldRenderable is RenderableObjectList && newRenderable is RenderableObjectList)
			{
				RenderableObjectList oldList = (RenderableObjectList)oldRenderable;
				RenderableObjectList newList = (RenderableObjectList)newRenderable;

				compareRefreshLists(newList, oldList);
			}
		}

		private void compareRefreshLists(RenderableObjectList newList, RenderableObjectList curList)
		{
			ArrayList addList = new ArrayList();
			ArrayList delList = new ArrayList();

			foreach(RenderableObject newObject in newList.ChildObjects)
			{
				bool foundObject = false;
				foreach(RenderableObject curObject in curList.ChildObjects)
				{
					string xmlSource = curObject.MetaData["XmlSource"] as string;
						
					if(xmlSource != null && xmlSource == m_DataSource && newObject.Name == curObject.Name)
					{
						foundObject = true;
						UpdateRenderable(curObject, newObject);
						break;
					}
				}

				if(!foundObject)
				{
					addList.Add(newObject);
				}
			}

			foreach(RenderableObject curObject in curList.ChildObjects)
			{
				bool foundObject = false;
				foreach(RenderableObject newObject in newList.ChildObjects)
				{
					string xmlSource = newObject.MetaData["XmlSource"] as string;
					if(xmlSource != null && xmlSource == m_DataSource && newObject.Name == curObject.Name)
					{
						foundObject = true;
						break;
					}
				}

				if(!foundObject)
				{
					string src = (string)curObject.MetaData["XmlSource"];

					if(src != null || src == m_DataSource)
						delList.Add(curObject);
				}
			}

			foreach(RenderableObject o in addList)
			{
				curList.Add(o);
			}

			foreach(RenderableObject o in delList)
			{
				curList.Remove(o);
			}
		}

		bool hasSkippedFirstRefresh = false;

		private void m_RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(!hasSkippedFirstRefresh)
			{
				hasSkippedFirstRefresh = true;
				return;
			}

			try
			{
				string dataSource = m_DataSource;

				RenderableObjectList newList = ConfigurationLoader.getRenderableFromLayerFile(dataSource, m_ParentWorld, m_Cache, false);

				if(newList != null)
				{
					compareRefreshLists(newList, this);
				}
			}
			catch(Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
