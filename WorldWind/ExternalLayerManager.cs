using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace WorldWind.CMPlugins.ExternalLayerManager
{
	/// <summary>
	/// Summary description for ExternalLayerManager.
	/// </summary>
	public class ExternalLayerManager : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		MenuItem parentMenuItem = null;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		WorldWindow m_WorldWindow = null;

		System.Timers.Timer m_UpdateTimer = new System.Timers.Timer(500);

		System.Collections.Hashtable m_NodeHash = new Hashtable();

		public ExternalLayerManager(WorldWindow ww, MenuItem menuItem)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			parentMenuItem = menuItem;

			m_WorldWindow = ww;

			
			
			m_UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_UpdateTimer_Elapsed);
			m_UpdateTimer.Start();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			m_UpdateTimer.Stop();
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			
			base.Dispose( disposing );
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Dispose(true);
			parentMenuItem.Checked = false;
			base.OnClosing (e);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			// 
			// treeView1
			// 
			this.treeView1.CheckBoxes = true;
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
			this.treeView1.ImageIndex = -1;
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Name = "treeView1";
			this.treeView1.SelectedImageIndex = -1;
			this.treeView1.Size = new System.Drawing.Size(121, 302);
			this.treeView1.TabIndex = 0;
			this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(121, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 302);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.CommandsVisibleIfAvailable = true;
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid1.LargeButtons = false;
			this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid1.Location = new System.Drawing.Point(124, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(316, 302);
			this.propertyGrid1.TabIndex = 2;
			this.propertyGrid1.Text = "propertyGrid1";
			this.propertyGrid1.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid1.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// ExternalLayerManager
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 302);
			this.Controls.Add(this.propertyGrid1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.treeView1);
			this.Name = "ExternalLayerManager";
			this.Text = "ExternalLayerManager";
			this.ResumeLayout(false);

		}
		#endregion

		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if(e.Node != null && e.Node.Tag != null)
			{
				RenderableObjectInfo roi = (RenderableObjectInfo)e.Node.Tag;
				this.propertyGrid1.SelectedObject = roi.Renderable;
			}
		}

		private string GetAbsoluteRenderableObjectPath(WorldWind.Renderable.RenderableObject ro)
		{
			if(ro.ParentList != null)
			{
				return GetAbsoluteRenderableObjectPath(ro.ParentList) + "//" + ro.Name;
			}
			else
			{
				return ro.Name;
			}
		}

		private string GetAbsoluteTreeNodePath(TreeNode tn)
		{
			if(tn.Parent != null)
			{
				return GetAbsoluteTreeNodePath(tn.Parent) + "//" + tn.Text;
			}
			else
			{
				return tn.Text;
			}
		}

		private void updateNode(TreeNode tn)
		{

			RenderableObjectInfo roi = (RenderableObjectInfo)tn.Tag;

			roi.LastSpotted = System.DateTime.Now;
			if(tn.Checked != roi.Renderable.IsOn)
			{
				tn.Checked = roi.Renderable.IsOn;
				//treeView1.BeginInvoke(new UpdateCheckStateNodeDelegate(this.UpdateCheckStateNode), new object[] {tn, roi.Renderable.IsOn});
			}

			if(roi.Renderable is WorldWind.Renderable.RenderableObjectList)
			{
				WorldWind.Renderable.RenderableObjectList rol = (WorldWind.Renderable.RenderableObjectList)roi.Renderable;
				for(int i = 0; i < rol.Count; i++)
				{
					WorldWind.Renderable.RenderableObject childRo = (WorldWind.Renderable.RenderableObject)rol.ChildObjects[i];
					string absolutePath = GetAbsoluteRenderableObjectPath(childRo);

					TreeNode correctNode = (TreeNode)m_NodeHash[absolutePath];
					if(correctNode == null)
					{
						correctNode = new TreeNode(childRo.Name);
						RenderableObjectInfo curRoi = new RenderableObjectInfo();
						curRoi.Renderable = childRo;
						correctNode.Tag = curRoi;

						m_NodeHash.Add(absolutePath, correctNode);
						treeView1.BeginInvoke(new UpdateChildNodeDelegate(this.UpdateChildNodeTree), new object[] {tn, correctNode});
					}

					updateNode(correctNode);
					
				}
			}
		}

		private void m_UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				System.DateTime updateStart = System.DateTime.Now;

				for(int i = 0; i < m_WorldWindow.CurrentWorld.RenderableObjects.Count; i++)
				{
					WorldWind.Renderable.RenderableObject curRo = (WorldWind.Renderable.RenderableObject)m_WorldWindow.CurrentWorld.RenderableObjects.ChildObjects[i];
					if(i >= this.treeView1.Nodes.Count)
					{
						// Add a node
						TreeNode correctNode = new TreeNode(curRo.Name);
						RenderableObjectInfo curRoi = new RenderableObjectInfo();
						curRoi.Renderable = curRo;
						correctNode.Tag = curRoi;

						m_NodeHash.Add(correctNode.Text, correctNode);
						this.treeView1.BeginInvoke(new AddTableTreeDelegate(this.AddTableTree), new object[] {correctNode});

						updateNode(correctNode);
					}
					else
					{
						//compare nodes
						TreeNode curTn = this.treeView1.Nodes[i];

						RenderableObjectInfo curRoi = (RenderableObjectInfo)curTn.Tag;
						if(curRoi.Renderable != null && curRoi.Renderable.Name == curRo.Name)
						{
							updateNode(curTn);
							continue;
						}
						else
						{
							if(!m_NodeHash.Contains(curRo.Name))
							{	
								//add it
								curRoi = new RenderableObjectInfo();
								curRoi.Renderable = curRo;
								curTn = new TreeNode(curRo.Name);
								curTn.Tag = curRoi;

								m_NodeHash.Add(curTn.Text, curTn);
								this.treeView1.BeginInvoke(new InsertTableTreeDelegate(this.InsertTableTree), new object[] {i, curTn});
							}
							else
							{
								curTn = (TreeNode)m_NodeHash[curRo.Name];
								try
								{
									treeView1.BeginInvoke(new RemoveTableTreeDelegate(this.RemoveTableTree), new object[] {curTn});
								}
								catch
								{}
							
								treeView1.BeginInvoke(new InsertTableTreeDelegate(this.InsertTableTree), new object[] {i, curTn});
							}
						}

						updateNode(curTn);
					}
				
				}

				for(int i = m_WorldWindow.CurrentWorld.RenderableObjects.Count; i < this.treeView1.Nodes.Count; i++)
				{
					this.treeView1.BeginInvoke(new RemoveAtTableTreeDelegate(this.RemoveAtTableTree), new object[] {i});
				}

				System.Collections.ArrayList deletionList = new ArrayList();
				foreach(TreeNode tn in m_NodeHash.Values)
				{
					RenderableObjectInfo roi = (RenderableObjectInfo)tn.Tag;
					if(roi == null || roi.Renderable == null || roi.LastSpotted < updateStart)
					{
						deletionList.Add(GetAbsoluteRenderableObjectPath(roi.Renderable));
					}
				}

				foreach(string key in deletionList)
				{
					m_NodeHash.Remove(key);
				}
			}
			catch
			{}
		}

		
		delegate 
			void UpdateChildNodeDelegate(TreeNode parent, TreeNode child);

		private void UpdateChildNodeTree(TreeNode parent, TreeNode child)
		{
			try
			{
				parent.Nodes.Add(child);
			}
			catch{}
		}


		delegate 
			void AddTableTreeDelegate(TreeNode tn);

		private void AddTableTree(TreeNode tn)
		{
			try
			{
				this.treeView1.Nodes.Add(tn);
			}
			catch{}
		}

		delegate 
			void InsertTableTreeDelegate(int index, TreeNode tn);

		private void InsertTableTree(int index, TreeNode tn)
		{
			try
			{
				this.treeView1.Nodes.Insert(index, tn);
			}
			catch{}
		}

		delegate 
			void ReplaceTableTreeDelegate(int index, TreeNode tn);

		private void ReplaceTableTree(int index, TreeNode tn)
		{
			try
			{
				this.treeView1.Nodes[index] = tn;
			}
			catch{}
		}

		delegate 
			void RemoveAtTableTreeDelegate(int index);

		private void RemoveAtTableTree(int index)
		{
			try
			{
				this.treeView1.Nodes.RemoveAt(index);
			}
			catch{}
		}

		delegate 
			void RemoveTableTreeDelegate(TreeNode tn);

		private void RemoveTableTree(TreeNode tn)
		{
			try
			{
				this.treeView1.Nodes.Remove(tn);
			}
			catch{}
		}

		private void treeView1_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			RenderableObjectInfo roi = (RenderableObjectInfo)e.Node.Tag;
			if(roi != null)
			{
				roi.Renderable.IsOn = e.Node.Checked;
			}
		}

		class RenderableObjectInfo
		{
			public DateTime LastSpotted = System.DateTime.Now;
			public WorldWind.Renderable.RenderableObject Renderable = null;

		}
	}

	public class ExternalLayerManagerLoader : WorldWind.PluginEngine.Plugin
	{
		MenuItem m_MenuItem;
		ExternalLayerManager m_Form = null;
		WorldWind.WindowsControlMenuButton m_ToolbarItem = null;

		/// <summary>
		/// Plugin entry point 
		/// </summary>
		public override void Load() 
		{
			m_MenuItem = new MenuItem("External Layer Manager");
			m_MenuItem.Click += new EventHandler(menuItemClicked);
			ParentApplication.PluginsMenu.MenuItems.Add( m_MenuItem );
			
				
			/*	m_ToolbarItem = new WorldWind.WindowsControlMenuButton(
					"Naval Research Labs, Monterey -- \"Real-Time\" Weather",
					Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Data\\Icons\\Interface\\nrl.png",
					m_Form);
			
				ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_ToolbarItem);
			*/
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
			if(m_Form != null && m_Form.Visible)
			{
				m_Form.Visible = false;
				m_Form.Dispose();
				m_Form = null;
				
				m_MenuItem.Checked = false;
			}
			else
			{
				m_Form = new ExternalLayerManager(ParentApplication.WorldWindow, m_MenuItem);
				m_Form.Owner = ParentApplication;
			
				m_Form.Visible = true;
				m_MenuItem.Checked = true;
			}
		}
	}
}
