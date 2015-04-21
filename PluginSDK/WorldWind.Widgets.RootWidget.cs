using System;

namespace WorldWind.Widgets
{
	/// <summary>
	/// Summary description for Widget.
	/// </summary>
	public class RootWidget : IWidget, IInteractive
	{
		IWidget m_ParentWidget = null;
		IWidgetCollection m_ChildWidgets = new WidgetCollection();
		System.Windows.Forms.Control m_ParentControl;

		public RootWidget(System.Windows.Forms.Control parentControl) 
		{
			m_ParentControl = parentControl;
		}

		#region Methods
		public void Render(DrawArgs drawArgs)
		{
			for(int index = m_ChildWidgets.Count - 1; index >= 0; index--)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;
				if(currentWidget != null)
				{
					if(currentWidget.ParentWidget == null || currentWidget.ParentWidget != this)
						currentWidget.ParentWidget = this;

					currentWidget.Render(drawArgs);
				}
			}
		}
		#endregion

		#region Properties
		public System.Drawing.Point AbsoluteLocation
		{
			get
			{
				return new System.Drawing.Point(0,0);
			}
		}
		public string Name
		{
			get
			{
				return "Main Frame";
			}
			set
			{

			}
		}
		public IWidgetCollection ChildWidgets
		{
			get
			{
				return m_ChildWidgets;
			}
			set
			{
				m_ChildWidgets = value;	
			}
		}
		
		public IWidget ParentWidget
		{
			get
			{
				return m_ParentWidget;
			}
			set
			{
				m_ParentWidget = value;
			}
		}
		
		System.Drawing.Point m_Location = new System.Drawing.Point(0,0);
		bool m_Enabled = false;
		bool m_Visible = false;
		object m_Tag = null;

		public System.Drawing.Point ClientLocation
		{
			get
			{
				return m_Location;
			}
			set
			{
				m_Location = value;
			}
		}

		public System.Drawing.Size ClientSize
		{
			get
			{
				return m_ParentControl.Size;
			}
			set
			{
				m_ParentControl.Size = value;
			}
		}
		public bool Enabled
		{
			get
			{
				return m_Enabled;	
			}
			set
			{
				m_Enabled = value;
			}
		}
		public bool Visible
		{
			get
			{
				return m_Visible;	
			}
			set
			{
				m_Visible = value;
			}
		}
		public object Tag
		{
			get
			{
				return m_Tag;	
			}
			set
			{
				m_Tag = value;
			}
		}
		#endregion

		#region IInteractive Members

		public bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;

				if(currentWidget != null && currentWidget is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[index] as IInteractive;

					bool handled = currentInteractive.OnMouseDown(e);
					if(handled)
						return handled;
				}
			}

			return false;
		}

		public bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;

				if(currentWidget != null && currentWidget is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[index] as IInteractive;

					bool handled = currentInteractive.OnMouseUp(e);
					if(handled)
						return handled;
				}
			}

			return false;
		}

		public bool OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
		{
			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;

				if(currentWidget != null && currentWidget is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[index] as IInteractive;

					bool handled = currentInteractive.OnKeyPress(e);
					if(handled)
						return handled;
				}
			}
			return false;
		}

		public bool OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;

				if(currentWidget != null && currentWidget is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[index] as IInteractive;

					bool handled = currentInteractive.OnKeyDown(e);
					if(handled)
						return handled;
				}
			}
			return false;
		}

		public bool OnKeyUp(System.Windows.Forms.KeyEventArgs e)
		{
			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;

				if(currentWidget != null && currentWidget is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[index] as IInteractive;

					bool handled = currentInteractive.OnKeyUp(e);
					if(handled)
						return handled;
				}
			}
			return false;
		}

		public bool OnMouseEnter(EventArgs e)
		{
			// TODO:  Add RootWidget.OnMouseEnter implementation
			return false;
		}

		public bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;

				if(currentWidget != null && currentWidget is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[index] as IInteractive;

					bool handled = currentInteractive.OnMouseMove(e);
					if(handled)
						return handled;
				}
			}

			return false;
		}

		public bool OnMouseLeave(EventArgs e)
		{
			// TODO:  Add RootWidget.OnMouseLeave implementation
			return false;
		}

		public bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
			// TODO:  Add RootWidget.OnMouseWheel implementation
			return false;
		}

		#endregion
	}
}
