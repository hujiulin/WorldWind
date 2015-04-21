using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;

/*namespace WorldWind.NewWidgets
{
	/// <summary>
	/// Summary description for Widgets
	/// </summary>
	public class LayerManager : FormWidget
	{
		TreeView tv = new TreeView();

		public LayerManager()
		{
			this.Text = "Layer Manager";
			this.Name = "Layer Manager";
			this.AutoHideHeader = false;
			this.HeaderColor = System.Drawing.Color.FromArgb(
				120,
				System.Drawing.Color.Coral.R,
				System.Drawing.Color.Coral.G,
				System.Drawing.Color.Coral.B);
			this.BackgroundColor = System.Drawing.Color.FromArgb(
				100, 0, 0, 0);

			//this.TextFont = WorldWindow.AppContext.CreateFont("Ariel", 10.0f, System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Bold);
			tv.ParentWidget = this;
			
			this.ChildWidgets.Add(tv);
			
			ClientSize = new System.Drawing.Size(300, 200);
		}

		private LayerManagerTreeNode getTreeNodeFromIRenderable(WorldWind.Renderable.RenderableObject renderable)
		{
			LayerManagerTreeNode rootNode = new LayerManagerTreeNode(renderable.Name);
			rootNode.Tag = renderable;

			if(renderable is WorldWind.Renderable.RenderableObjectList)
			{
				WorldWind.Renderable.RenderableObjectList rol = (WorldWind.Renderable.RenderableObjectList)renderable;

				if(rol.ChildObjects != null && rol.ChildObjects.Count > 0)
				{
					for(int childIndex = 0; childIndex < rol.ChildObjects.Count; childIndex++)
					{
						rootNode.Children.Add(getTreeNodeFromIRenderable((WorldWind.Renderable.RenderableObject)rol.ChildObjects[childIndex]));
					}
				}
			}

			return rootNode;
		}

		private void mergeNodes(LayerManagerTreeNode source, LayerManagerTreeNode dest)
		{
			if(source.Text != dest.Text)
			{
				dest.Text = source.Text;
			}

			for(int i = 0; i < source.Children.Count; i++)
			{
				if(dest.Children.Count <= i)
				{
					dest.Children.Add(source.Children[i] as LayerManagerTreeNode);
				}
				else
				{
					mergeNodes(source.Children[i] as LayerManagerTreeNode, dest.Children[i] as LayerManagerTreeNode);
				}
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			tv.ClientLocation = new System.Drawing.Point(0, 20);
			tv.ClientSize = new System.Drawing.Size(ClientSize.Width, ClientSize.Height - 20);
			
			if(drawArgs.CurrentWorld != null)
			{
				for(int renderableIndex = 0; renderableIndex < drawArgs.CurrentWorld.RenderableObjects.ChildObjects.Count; renderableIndex++)
				{
					LayerManagerTreeNode currentNode = getTreeNodeFromIRenderable((WorldWind.Renderable.RenderableObject)drawArgs.CurrentWorld.RenderableObjects.ChildObjects[renderableIndex]);

					if(tv.Nodes.Count <= renderableIndex)
					{
						tv.Nodes.Add(currentNode);
					}
					else
					{
						mergeNodes(currentNode, tv.Nodes[renderableIndex] as LayerManagerTreeNode);
					}
				}
			}
			
			base.Render(drawArgs);
		}
	}

    public class LayerManagerTreeNode : TreeNode
    {
        public LayerManagerTreeNode(string text)
            : base(text)
        {
            this.m_ContextMenu = new System.Windows.Forms.ContextMenu();
            m_ContextMenu.MenuItems.Add("dummy");
        }
    }

}*/
