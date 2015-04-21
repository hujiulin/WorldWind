//========================= (UNCLASSIFIED) ==============================
// Copyright © 2005-2006 The Johns Hopkins University /
// Applied Physics Laboratory.  All rights reserved.
//
// WorldWind Source Code - Copyright 2005 NASA World Wind 
// Modified under the NOSA License
//
//========================= (UNCLASSIFIED) ==============================
//
// LICENSE AND DISCLAIMER 
//
// Copyright (c) 2006 The Johns Hopkins University. 
//
// This software was developed at The Johns Hopkins University/Applied 
// Physics Laboratory (“JHU/APL”) that is the author thereof under the 
// “work made for hire” provisions of the copyright law.  Permission is 
// hereby granted, free of charge, to any person obtaining a copy of this 
// software and associated documentation (the “Software”), to use the 
// Software without restriction, including without limitation the rights 
// to copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit others to do so, subject to the 
// following conditions: 
//
// 1.  This LICENSE AND DISCLAIMER, including the copyright notice, shall 
//     be included in all copies of the Software, including copies of 
//     substantial portions of the Software; 
//
// 2.  JHU/APL assumes no obligation to provide support of any kind with 
//     regard to the Software.  This includes no obligation to provide 
//     assistance in using the Software nor to provide updated versions of 
//     the Software; and 
//
// 3.  THE SOFTWARE AND ITS DOCUMENTATION ARE PROVIDED AS IS AND WITHOUT 
//     ANY EXPRESS OR IMPLIED WARRANTIES WHATSOEVER.  ALL WARRANTIES 
//     INCLUDING, BUT NOT LIMITED TO, PERFORMANCE, MERCHANTABILITY, FITNESS
//     FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT ARE HEREBY DISCLAIMED.  
//     USERS ASSUME THE ENTIRE RISK AND LIABILITY OF USING THE SOFTWARE.  
//     USERS ARE ADVISED TO TEST THE SOFTWARE THOROUGHLY BEFORE RELYING ON 
//     IT.  IN NO EVENT SHALL THE JOHNS HOPKINS UNIVERSITY BE LIABLE FOR 
//     ANY DAMAGES WHATSOEVER, INCLUDING, WITHOUT LIMITATION, ANY LOST 
//     PROFITS, LOST SAVINGS OR OTHER INCIDENTAL OR CONSEQUENTIAL DAMAGES, 
//     ARISING OUT OF THE USE OR INABILITY TO USE THE SOFTWARE. 
//
using System;

using WorldWind;
//using WorldWind.Renderable;

namespace jhuapl.util
{
	/// <summary>
	/// Summary description for Widget.
	/// </summary>
	public class JHU_RootWidget : JHU_WidgetCollection, jhuapl.util.IWidget, jhuapl.util.IInteractive
	{
		jhuapl.util.IWidget m_parentWidget = null;
		jhuapl.util.IWidgetCollection m_ChildWidgets = new JHU_WidgetCollection();
		System.Windows.Forms.Control m_ParentControl;
		bool m_Initialized = false;

		public JHU_RootWidget(System.Windows.Forms.Control parentControl) 
		{
			m_ParentControl = parentControl;
		}

		#region Methods

		public void Initialize(DrawArgs drawArgs)
		{
		}

		public void Render(DrawArgs drawArgs)
		{
			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return;

			for(int index = m_ChildWidgets.Count - 1; index >= 0; index--)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;
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

		System.Drawing.Point m_location = new System.Drawing.Point(0,0);
		System.Drawing.Point m_ClientLocation = new System.Drawing.Point(0,120);

		public System.Drawing.Point AbsoluteLocation
		{
			get { return m_location; }
			set { m_location = value; }		
		}
		public string Name
		{
			get { return "Main Frame"; }
			set { }
		}
		
		public jhuapl.util.IWidget ParentWidget
		{
			get { return m_parentWidget; }
			set { m_parentWidget = value; }
		}

		public jhuapl.util.IWidgetCollection ChildWidgets
		{
			get { return m_ChildWidgets; }
			set { m_ChildWidgets = value; }
		}		

		bool m_enabled = true;
		bool m_visible = true;
		object m_tag = null;

		public System.Drawing.Point ClientLocation
		{
			get { return m_ClientLocation; }
			set { }
		}

		public System.Drawing.Size ClientSize
		{
			get
			{
				System.Drawing.Size mySize = m_ParentControl.Size;
				mySize.Height -= 120;
				return mySize;
			}
		}

		public System.Drawing.Size WidgetSize
		{
			get { return m_ParentControl.Size; }
			set { }
		}

		public bool Enabled
		{
			get { return m_enabled; }
			set { m_enabled = value; }
		}

		public bool Visible
		{
			get { return m_visible; }
			set { m_visible = value; }
		}

		protected bool m_countHeight = true;
		protected bool m_countWidth = true;
		public bool CountHeight
		{
			get { return m_countHeight; }
			set { m_countHeight = value; }
		}

		public bool CountWidth		
		{
			get { return m_countWidth; }
			set { m_countWidth = value; }
		}
		public System.Drawing.Point Location
		{
			get { return m_location; }
			set { m_location = value; }
		}

		public object Tag
		{
			get { return m_tag; }
			set { m_tag = value; }
		}
		public bool IsInitialized
		{
			get { return m_Initialized;}
			set { m_Initialized = value; }
		}
		#endregion

		#region IInteractive Members

		MouseClickAction m_leftClickAction;
		public MouseClickAction LeftClickAction
		{
			get { return m_leftClickAction; }
			set { m_leftClickAction = value; }
		}	

		MouseClickAction m_rightClickAction;
		public MouseClickAction RightClickAction
		{
			get { return m_rightClickAction; }
			set { m_rightClickAction = value; }
		}	

		public bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnMouseDown(e);
					if(handled)
						return handled;
				}
			}

			return handled;
		}

		public bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnMouseUp(e);
					if(handled)
						return handled;
				}
			}

			return handled;
		}

		public bool OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnKeyDown(e);
					if(handled)
						return handled;
				}
			}

			return handled;
		}

		public bool OnKeyUp(System.Windows.Forms.KeyEventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnKeyUp(e);
					if(handled)
						return handled;
				}
			}

			return handled;
		}

		public bool OnMouseEnter(EventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnMouseEnter(e);
					if(handled)
						return handled;
				}
			}

			return handled;
		}

		public bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnMouseMove(e);
					if(handled)
						return handled;
				}
			}

			return handled;
		}

		public bool OnMouseLeave(EventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnMouseLeave(e);
					if(handled)
						return handled;
				}
			}
			
			return handled;
		}

		public bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;

				if(currentWidget != null && currentWidget is jhuapl.util.IInteractive)
				{
					jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[index] as jhuapl.util.IInteractive;

					handled = currentInteractive.OnMouseWheel(e);
					if(handled)
						return handled;
				}
			}
			
			return handled;
		}

		#endregion

		new public void Add(jhuapl.util.IWidget widget)
		{
			m_ChildWidgets.Add(widget);
			widget.ParentWidget = this;
		}		
		
		new public void Remove(jhuapl.util.IWidget widget)
		{
			m_ChildWidgets.Remove(widget);
		}
	}
}
