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
// Copyright (c) 2005 The Johns Hopkins University. 
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
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.IO;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;

namespace WorldWind.NewWidgets
{
    public delegate void CheckStateChangedHandler(object o, bool state);

	/// <summary>
	/// Summary description for TextLabel.
	/// </summary>
	public abstract class TreeNodeWidget : WidgetCollection, IWidget, IInteractive
	{
		public const int NODE_OFFSET = 5;

		public const int NODE_HEIGHT = 20;

		public const int NODE_INDENT = 15;

		public const int NODE_ARROW_SIZE = 15;

		public const int NODE_CHECKBOX_SIZE = 15;

		protected const int DEFAULT_OPACITY = 150;

		#region icon members

		// sprite to use for render of icon
		protected Sprite m_sprite;

		// icon texture to use
		protected IconTexture m_iconTexture;

		// image name to load
		protected string m_imageName = "";

		// whether or not this node has an icon
		protected bool m_hasIcon = false;

		// Icon X scaling computed by dividing icon area width by texture width
		protected float XScale;
 
		// Icon Y scaling computed by dividing icon area height by texture height 
		protected float YScale;

		#endregion icon members

		#region color members

		protected int m_itemOnColor = Color.White.ToArgb();
		protected int m_itemOffColor = Color.Gray.ToArgb();

		protected int m_mouseOverColor = Color.FromArgb(DEFAULT_OPACITY,160,160,160).ToArgb();
		protected int m_mouseOverOnColor = Color.White.ToArgb(); 
		protected int m_mouseOverOffColor = Color.Black.ToArgb(); 

		#endregion color members

		#region iWidget members

		protected IWidget m_parentWidget = null;

		protected System.Drawing.Point m_location = new System.Drawing.Point(0,0);

		// size is used within this class for most size calculations
		protected System.Drawing.Size m_size = new System.Drawing.Size(0,0);

		// consumed size is passed back to parent for the widget size.  Usually same as m_size except if top level tree widget
		protected System.Drawing.Size m_ConsumedSize = new System.Drawing.Size(0,0);

		protected IWidgetCollection m_subNodes = new WidgetCollection();

		protected bool m_enabled = true;

		protected bool m_visible = true;

		// Menu tree items always count height and width wise.
		protected bool m_countHeight = true;
		protected bool m_countWidth = true;

		protected object m_tag = null;
		protected string m_name = "";

		#endregion IWidget members

		// true if we've initialized this widget for proper rendering
		protected bool m_isInitialized = false;

		// true if this widget is expanded
		protected bool m_isExpanded = false;

		// true if the mouse is over this widget (detected from move events)
		protected bool m_isMouseOver = false;

		// true if we saw the mousedown event for this widget
		protected bool m_isMouseDown = false;

		// the associated renderable object (used in layer manager)
		protected WorldWind.Renderable.RenderableObject m_renderableObject;
		
		// true if we are using radio style checkbox
		protected bool m_isRadioButton = false;

		// true if check box should be checked
		protected bool m_isChecked = true;

		// true if check box should be mapped to if this tree node is enabled or not
		protected bool m_enableCheck = true;

		// fonts to use in render
		protected static Microsoft.DirectX.Direct3D.Font m_drawingFont;
		protected static Microsoft.DirectX.Direct3D.Font m_wingdingsFont;
		protected static Microsoft.DirectX.Direct3D.Font m_worldwinddingsFont;

		protected int m_xOffset = 0;

		#region Properties

		public string ImageName
		{
			get { return m_imageName; }
			set 
			{ 
				m_imageName = value; 
				m_isInitialized = false;
			}
		}


		public bool Expanded
		{
			get { return m_isExpanded; }
			set { m_isExpanded = value; }
		}

		public bool IsRadioButton
		{
			get { return m_isRadioButton; }
			set { m_isRadioButton = value; }
		}

		public bool EnableCheck
		{
			get { return m_enableCheck; }
			set { m_enableCheck = value; }
		}

		public bool IsChecked
		{
			get
			{
				if (m_enableCheck)
					m_isChecked = this.Enabled;

				return m_isChecked;
			}
			set
			{
				if (m_enableCheck)
					this.Enabled = value;

				m_isChecked = value;
			}
		}

		public WorldWind.Renderable.RenderableObject RenderableObject
		{
			get { return m_renderableObject; }
			set 
			{ 
				m_renderableObject = value;
				if (m_renderableObject != null)
				{
					// Use radio check
					if(m_renderableObject.ParentList != null && m_renderableObject.ParentList.ShowOnlyOneLayer)
						m_isRadioButton = true;
				}

			}
		}

		#endregion

		#region IWidget Properties

		public IWidget ParentWidget
		{
			get { return m_parentWidget; }
			set { m_parentWidget = value; }
		}


		public System.Drawing.Point Location
		{
			get { return m_location; }
			set { m_location = value; }
		}


		public System.Drawing.Point AbsoluteLocation
		{
			get
			{
				if(m_parentWidget != null)
				{
					return new System.Drawing.Point(
						m_location.X + m_parentWidget.ClientLocation.X,
						m_location.Y + m_parentWidget.ClientLocation.Y);
					
				}
				else
				{
					return m_location;
				}
			}
		}

		
		public System.Drawing.Point ClientLocation
		{
			get { return this.AbsoluteLocation; }
		}


		public System.Drawing.Size ClientSize
		{
			get 
			{
				System.Drawing.Size clientSize = new Size();
				if (m_parentWidget != null)
				{
					m_size.Width = m_parentWidget.ClientSize.Width;
				}

				clientSize.Width = m_size.Width;
				clientSize.Height = m_ConsumedSize.Height;

				return clientSize; 
			}
			set { m_size = value; }
		}


		public System.Drawing.Size WidgetSize
		{
			get { return m_ConsumedSize; }
			set { m_ConsumedSize = value; }
		}


		public IWidgetCollection ChildWidgets
		{
			get { return m_subNodes; }
			set { m_subNodes = value; }
		}


		public bool Enabled
		{
			get 
			{ 
				if (m_renderableObject != null)
					m_enabled = m_renderableObject.IsOn;

				return m_enabled; 
			}
			set 
			{
				m_enabled = value; 
				if (m_renderableObject != null)
					m_renderableObject.IsOn = value;

                if (OnCheckStateChanged != null)
                    OnCheckStateChanged(this, value);
			}
		}


		public bool Visible
		{
			get { return m_visible; }
			set { m_visible = value; }
		}


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


		public object Tag
		{
			get { return m_tag; }
			set { m_tag = value; }
		}


		public string Name
		{
			get
			{
				// if the name isn't set return the RO's name if it exists
				if ( (m_name.Length <= 0) && (m_renderableObject != null) )
				{
					return m_renderableObject.Name;
				} 

				return m_name;
			}
			set
			{
				m_name = value;
			}
		}


		#endregion IWidget Properties

        public event CheckStateChangedHandler OnCheckStateChanged;

		public TreeNodeWidget()
		{
			m_location.X = 0;
			m_location.Y = 0;
			m_size.Height = NODE_HEIGHT;
			m_size.Width = 100;
			m_ConsumedSize = m_size;
			m_isInitialized = false;
		}

		public TreeNodeWidget(string name) : this()
		{
			m_name = name;
		}

		public void Initialize (DrawArgs drawArgs)
		{
			// Initialize fonts
			if (m_drawingFont == null)
			{
				System.Drawing.Font localHeaderFont = new System.Drawing.Font("Arial", 12.0f, FontStyle.Italic | FontStyle.Bold);
				m_drawingFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, localHeaderFont);

				System.Drawing.Font wingdings = new System.Drawing.Font("Wingdings", 12.0f);
				m_wingdingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, wingdings);

				System.Drawing.Font worldwinddings = new System.Drawing.Font("World Wind dings", 12.0f);
				m_worldwinddingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, worldwinddings);
			}

			// Initialize icon if any
			if (m_imageName.Trim() != string.Empty)
			{
				object key = null;

				// Icon image from file
				m_iconTexture = (IconTexture)DrawArgs.Textures[m_imageName];
				if(m_iconTexture==null)
				{
					key = m_imageName;
					m_iconTexture = new IconTexture( drawArgs.device, m_imageName );
				}

				if(m_iconTexture!=null)
				{
					m_iconTexture.ReferenceCount++;

					if(key!=null)
					{
						// New texture, cache it
						DrawArgs.Textures.Add(key, m_iconTexture);
					}

					if (m_size.Width == 0)
						m_size.Width = m_iconTexture.Width;

					if (m_size.Height == 0)
						m_size.Height = m_iconTexture.Height;

					this.XScale = (float)m_size.Width / m_iconTexture.Width;
					this.YScale = (float)m_size.Height / m_iconTexture.Height;
				}

				if (m_sprite == null)
					m_sprite = new Sprite(drawArgs.device);
			}

			m_isInitialized = true;
		}

		protected MouseClickAction m_leftClickAction;
		public MouseClickAction LeftClickAction
		{
			get { return m_leftClickAction; }
			set { m_leftClickAction = value; }
		}	

		protected MouseClickAction m_rightClickAction;
		public MouseClickAction RightClickAction
		{
			get { return m_rightClickAction; }
			set { m_rightClickAction = value; }
		}	

		protected MouseClickAction m_checkClickAction;
		public MouseClickAction CheckClickAction
		{
			get { return m_checkClickAction; }
			set { m_checkClickAction = value; }
		}

		protected MouseClickAction m_expandClickAction;
		public MouseClickAction ExpandClickAction
		{
			get { return m_expandClickAction; }
			set { m_expandClickAction = value; }
		}

		#region IInteractive Methods

		public bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;
			if (m_visible)
			{
				// if we're in the client area and lmb
				if( e.X >= this.AbsoluteLocation.X &&
					e.X <= this.AbsoluteLocation.X + ClientSize.Width &&
					e.Y >= this.AbsoluteLocation.Y &&
					e.Y <= this.AbsoluteLocation.Y + NODE_HEIGHT)
				{
					m_isMouseDown = true;
					handled = true;
				}
				else
				{
					m_isMouseDown = false;
				}
			}

			// if mouse didn't come down on us, let the children try as we should be inside the top form client area
			if (!handled)
			{
				for(int i = 0; i < m_subNodes.Count; i++)
				{
					if(m_subNodes[i] is IInteractive)
					{
						IInteractive currentInteractive = m_subNodes[i] as IInteractive;
						handled = currentInteractive.OnMouseDown(e);
					}

					// if anyone has handled this, we're done
					if (handled)
						continue;	
				}
			}

			return handled;			 
		}


		public bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;
			if (m_visible)
			{
				// if we're in the client area
				if( e.X >= this.AbsoluteLocation.X &&
					e.X <= this.AbsoluteLocation.X + ClientSize.Width &&
					e.Y >= this.AbsoluteLocation.Y &&
					e.Y <= this.AbsoluteLocation.Y + NODE_HEIGHT)
				{
					if (m_isMouseDown)
					{
						// if we're in the expand arrow region
						if ((e.X > this.AbsoluteLocation.X + m_xOffset) &&
							(e.X < this.AbsoluteLocation.X + m_xOffset + NODE_ARROW_SIZE))
						{
							this.Expanded = !this.Expanded;

							// call helper routine if it exists
							if (m_expandClickAction != null)
								m_expandClickAction(e);
						}
						// else if we're supposed to use checkmark and we're in the activate/deactivate region
						else if (m_enableCheck &&
								 (e.X > this.AbsoluteLocation.X + m_xOffset + NODE_ARROW_SIZE) &&
								 (e.X < this.AbsoluteLocation.X + m_xOffset + NODE_ARROW_SIZE + NODE_CHECKBOX_SIZE))
						{
							this.Enabled = !this.Enabled;

							// call helper routine if it exists
							if (m_checkClickAction != null)
								m_checkClickAction(e);
						}
							// Otherwise perform general LMB, RMB action
						else if ((e.Button == System.Windows.Forms.MouseButtons.Left) && (m_leftClickAction != null))
						{
							m_leftClickAction(e);
						} 
						else if ((e.Button == System.Windows.Forms.MouseButtons.Right) && (m_rightClickAction != null))
						{
							m_rightClickAction(e);
						}

						handled = true;
					}
				}
			}

			// if mouse isn't over us, let the children try as we should be inside the top form client area
			if (!handled)
			{
				for(int i = 0; i < m_subNodes.Count; i++)
				{
					if(m_subNodes[i] is IInteractive)
					{
						IInteractive currentInteractive = m_subNodes[i] as IInteractive;
						bool subHandled = currentInteractive.OnMouseUp(e);
                        if (subHandled)
                            handled = true;
                    }

					// if anyone has handled this, we're done
					//if (handled)
					//	break;	
				}
			}

			// regardless reset mousedown point 
			m_isMouseDown = false;
            
            return handled;
		}


		public bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			bool handled = false;
			if (m_visible)
			{
				// if we're in the client area
				if( e.X >= this.AbsoluteLocation.X &&
					e.X <= this.AbsoluteLocation.X + ClientSize.Width &&
					e.Y >= this.AbsoluteLocation.Y &&
					e.Y <= this.AbsoluteLocation.Y + NODE_HEIGHT)
				{
					if (!m_isMouseOver)
						this.OnMouseEnter(e);

//					m_isMouseOver = true;
					handled = true;
				}
				else
				{
					if (m_isMouseOver)
						this.OnMouseLeave(e);

//					m_isMouseOver = false;
				}
			}
			else
			{
				m_isMouseOver = false;
			}

			// call all the children because they need to have a chance to detect mouse leaving.
			for(int i = 0; i < m_subNodes.Count; i++)
			{
				if(m_subNodes[i] is IInteractive)
				{
					IInteractive currentInteractive = m_subNodes[i] as IInteractive;
					if (currentInteractive.OnMouseMove(e))
						handled = true;
				}
			}

			return handled;
		}


		public bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
			return false;
		}

		public bool OnMouseEnter(EventArgs e)
		{
            if (this.m_parentWidget != null && m_parentWidget is TreeNodeWidget)
            {
                TreeNodeWidget parentNode = (TreeNodeWidget)m_parentWidget;
                if (!parentNode.Expanded)
                {
                    m_isMouseOver = false;
                }
                else
                {
                    m_isMouseOver = true;
                }
            }
            else
            {
                m_isMouseOver = true;
            }
            return false;
		}


		public bool OnMouseLeave(EventArgs e)
		{
			m_isMouseOver = false;
			return false;
		}


		public bool OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			return false;
		}


		public bool OnKeyUp(System.Windows.Forms.KeyEventArgs e)
		{
			return false;
		}

		/// <summary>
		/// Key press event handler.
		/// This widget does nothing with key presses.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public bool OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
		{
			return false;
		}


		#endregion

		#region IWidget Methods

		public void Render(DrawArgs drawArgs)
		{
			int consumedPixels = 0;

			consumedPixels = this.Render(drawArgs, NODE_OFFSET, 0);

			// TODO: set size to consumed pixels?
			m_ConsumedSize.Height = consumedPixels;
		}

		#endregion

		#region IWidgetCollection methods

		new public void Add(IWidget widget)
		{
			m_subNodes.Add(widget);
			widget.ParentWidget = this;
		}

		new public void Remove(IWidget widget)
		{
			m_subNodes.Remove(widget);
		}

		#endregion IWidgetCollection methods

		/// <summary>
		/// Specialized render for tree nodes
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="xOffset">The offset from the left based on how deep this node is nested</param>
		/// <param name="yOffset">The offset from the top based on how many treenodes are above this one</param>
		/// <returns>Total pixels consumed by this widget and its children</returns>
		public abstract int Render(DrawArgs drawArgs, int xOffset, int yOffset);
	}
}
