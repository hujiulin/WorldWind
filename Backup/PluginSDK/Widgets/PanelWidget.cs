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
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;

namespace WorldWind.NewWidgets
{
	/// <summary>
	/// PanelWidget - This class implements a basic panel with no layout management whatsoever.  
	/// PanelWidget can take any widget as a child.  This widget is used to group other widgets
	/// as a single widget.  
	/// 
	/// No scroll bars are created as this widget doesn't clip subwidgets.  If
	/// you specify a child widget to be outside the panel space it renders outside the panel.
	/// 
	/// Panels cannot be closed, resized or moved (dragged).
	/// </summary>
	public class PanelWidget : WidgetCollection, IWidget, IInteractive
	{
		#region Protected Members

		#region IWidget support variables

		/// <summary>
		/// Name property value
		/// </summary>
		protected string m_name = "";

		/// <summary>
		/// Location property value
		/// </summary>
		protected System.Drawing.Point m_location = new System.Drawing.Point(0,0);

		/// <summary>
		/// ClientLocation property value
		/// </summary>
		protected System.Drawing.Point m_clientLocation = new System.Drawing.Point(0,23);

		/// <summary>
		/// WidgetSize property value
		/// </summary>
		protected System.Drawing.Size m_size = new System.Drawing.Size(200, 300);

		/// <summary>
		/// ClientSize property value
		/// </summary>
		protected System.Drawing.Size m_clientSize = new System.Drawing.Size(300,177);

		/// <summary>
		/// Visible property value
		/// </summary>
		protected bool m_visible = true;

		/// <summary>
		/// Enabled property value
		/// </summary>
		protected bool m_enabled = true;

		/// <summary>
		/// CountHeight property value
		/// </summary>
		protected bool m_countHeight = false;

		/// <summary>
		/// CountWidth property value
		/// </summary>
		protected bool m_countWidth = false;

		/// <summary>
		/// Parent widget property value
		/// </summary>
		protected IWidget m_parentWidget = null;

		/// <summary>
		/// ChildWidget property value
		/// </summary>
		protected IWidgetCollection m_ChildWidgets = new WidgetCollection();

		/// <summary>
		/// Tag property
		/// </summary>
		protected object m_tag = null;

		/// <summary>
		/// Flag indicating if initialization is required
		/// </summary>
		protected bool m_isInitialized = false;

		#endregion

		#region IInteractive support variables

		/// <summary>
		/// LeftClickAction value - holds method to call on left mouse click
		/// </summary>
		protected MouseClickAction m_leftClickAction = null;

		/// <summary>
		/// RightClickAction value - holds method to call on right mouse click
		/// </summary>
		protected MouseClickAction m_rightClickAction = null;

		#endregion

		#region Color Values

		/// <summary>
		/// Background color
		/// </summary>
		protected System.Drawing.Color m_BackgroundColor = System.Drawing.Color.FromArgb(
			170,
			40,
			40,
			40);

		/// <summary>
		/// Border Color
		/// </summary>
		protected System.Drawing.Color m_BorderColor = System.Drawing.Color.GhostWhite;

		/// <summary>
		/// Header Background Color
		/// </summary>
		protected System.Drawing.Color m_HeaderColor = System.Drawing.Color.FromArgb(
			170,
//		96,
			System.Drawing.Color.DarkKhaki.R,
			System.Drawing.Color.DarkKhaki.G,
			System.Drawing.Color.DarkKhaki.B);

		/// <summary>
		/// Text color
		/// </summary>
		protected System.Drawing.Color m_TextColor = System.Drawing.Color.GhostWhite;

		#endregion

		/// <summary>
		/// Height of title bar
		/// </summary>
		protected int m_headerHeight = 23;
		protected int m_currHeaderHeight = 0;

		protected int m_leftPadding = 2;
		protected int m_rightPadding = 1;
		protected int m_topPadding = 2;
		protected int m_bottomPadding = 1;

		/// <summary>
		/// Whether or not to render the body.
		/// </summary>
		protected bool m_renderBody = true;

		protected Microsoft.DirectX.Direct3D.Font m_TextFont; 
		protected Microsoft.DirectX.Direct3D.Font m_TitleFont; 
		protected Microsoft.DirectX.Direct3D.Font m_wingdingsFont;
		protected Microsoft.DirectX.Direct3D.Font m_worldwinddingsFont;

		protected Vector2[] m_OutlineVertsHeader = new Vector2[5];
		protected Vector2[] m_OutlineVerts = new Vector2[5];

		/// <summary>
		/// Last point where the mouse was clicked (mousedown).
		/// </summary>
		protected System.Drawing.Point m_LastMousePosition = new System.Drawing.Point(0,0);

		/// <summary>
		/// Last time the mouse clicked on this widget (header area mostly) - used to implement double click
		/// </summary>
		protected DateTime m_LastClickTime;

		#endregion

		#region Public Members

		/// <summary>
		/// The text to render when the body is hidden
		/// </summary>
		public string Text = "";

		/// <summary>
		/// Whether or not to ever render the header
		/// </summary>
		public bool HeaderEnabled = true;

		#endregion 

		#region Properties

		public Microsoft.DirectX.Direct3D.Font TextFont
		{
			get { return m_TextFont; }
			set { m_TextFont = value; }
		}

		public System.Drawing.Color HeaderColor
		{
			get { return m_HeaderColor; }
			set { m_HeaderColor = value; }
		}

		public int HeaderHeight
		{
			get { return m_headerHeight; }
			set { m_headerHeight = value; }
		}

		public System.Drawing.Color BorderColor
		{
			get { return m_BorderColor; }
			set { m_BorderColor = value; }
		}

		public System.Drawing.Color BackgroundColor
		{
			get { return m_BackgroundColor; }
			set { m_BackgroundColor = value; }
		}


		/// <summary>
		/// The top edge of this widget.
		/// </summary>
		public int Top
		{
			get
			{
				if (HeaderEnabled)
					return this.AbsoluteLocation.Y;
				else
					return this.AbsoluteLocation.Y + this.m_currHeaderHeight;
			}
		}


		/// <summary>
		/// The bottom edge of this widget
		/// </summary>
		public int Bottom
		{
			get 
			{
				if (m_renderBody)
					return this.AbsoluteLocation.Y + this.m_size.Height;
				else
					return this.AbsoluteLocation.Y + this.m_currHeaderHeight;
			}
		}


		/// <summary>
		/// The left edge of this widget
		/// </summary>
		public int Left
		{
			get
			{
				return this.AbsoluteLocation.X;
			}
		}


		/// <summary>
		/// The right edge of this widget
		/// </summary>
		public int Right
		{
			get
			{
				return this.AbsoluteLocation.X + this.m_size.Width;
			}
		}


		/// <summary>
		/// Location within the form of where the client area is
		/// </summary>
		public System.Drawing.Point BodyLocation
		{
			get
			{
				System.Drawing.Point bodyLocation;
				bodyLocation = this.AbsoluteLocation;
				if (this.HeaderEnabled)
					bodyLocation.Y += m_currHeaderHeight;
				return bodyLocation;
			}
		}


		#endregion


		/// <summary>
		/// Form Widget Constructor
		/// </summary>
		/// <param name="name">Name of this form.  Name is displayed in header.</param>
		public PanelWidget(string name)
		{
			m_name = name;
		}

		/// <summary>
		/// Adds a new child widget
		/// </summary>
		/// <param name="widget">The widget to be added</param>
		new public void Add(IWidget widget)
		{
			m_ChildWidgets.Add(widget);
			widget.ParentWidget = this;
		}

		/// <summary>
		/// Removes a child widget
		/// </summary>
		/// <param name="widget">The widget to be removed</param>
		new public void Remove(IWidget widget)
		{
			m_ChildWidgets.Remove(widget);
		}

		/// <summary>
		/// Try to clean up everything.
		/// </summary>
		public void Dispose()
		{
			if(m_ChildWidgets != null)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					// get rid of child widget
				}				
				m_ChildWidgets.Clear();
			}
			m_isInitialized = false;
		}

		/// <summary>
		/// Computes the height and width of children as laid out.  This value is
		/// used to determine if scrolling is required.
		/// 
		/// HACK - Uses the fields CountHeight and CountWidth in the child widgets 
		/// to determine if they should be counted in the total height/width.
		/// </summary>
		/// <param name="childrenHeight">The total children height.</param>
		/// <param name="childrenWidth">The total children width</param>
		protected void getChildrenSize(out int childrenHeight, out int childrenWidth)
		{
			childrenHeight = 0;
			childrenWidth = 0;

			int biggestHeight = 0;
			int biggestWidth = 0;

			for(int i = 0; i < m_ChildWidgets.Count; i++)
			{
				if (m_ChildWidgets[i].CountHeight)
					childrenHeight += m_ChildWidgets[i].WidgetSize.Height;

				if (m_ChildWidgets[i].CountWidth)
					childrenWidth += m_ChildWidgets[i].WidgetSize.Width;

				if (m_ChildWidgets[i].WidgetSize.Height > biggestHeight)
					biggestHeight = m_ChildWidgets[i].WidgetSize.Height;

				if (m_ChildWidgets[i].WidgetSize.Width > biggestWidth)
					biggestWidth = m_ChildWidgets[i].WidgetSize.Width;
			}
			if (biggestHeight > childrenHeight)
				childrenHeight = biggestHeight;

			if (biggestWidth > childrenWidth)
				childrenWidth = biggestWidth;
		}

		#region IWidget Members

		#region Properties

		/// <summary>
		/// Name of this widget
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}


		/// <summary>
		/// Location of this widget relative to the client area of the parent
		/// </summary>
		public System.Drawing.Point Location
		{
			get { return m_location; }
			set { m_location = value; }
		}


		/// <summary>
		/// Where this widget is on the window
		/// </summary>
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


		/// <summary>
		/// The top left corner of this widget's client area offset by scrolling.
		/// This area is is masked by the ViewPort so objects outside the client
		/// area is clipped and not shown.
		/// </summary>
		public System.Drawing.Point ClientLocation
		{
			get
			{
				m_clientLocation = this.BodyLocation;
				return m_clientLocation;
			}
		}


		/// <summary>
		/// Size of widget in pixels
		/// </summary>
		public System.Drawing.Size WidgetSize
		{
			get { return m_size; }
			set { m_size = value; }
		}


		/// <summary>
		/// Size of the client area in pixels.  This area is the 
		/// widget area minus header and scrollbar areas.
		/// </summary>
		public System.Drawing.Size ClientSize
		{
			get 
			{ 					
				m_clientSize = m_size;

				// deduct header height
				m_clientSize.Height -= m_currHeaderHeight;

				return m_clientSize; 
			}
			set 
			{ 
				m_size = value;
				m_size.Height += m_currHeaderHeight; 
			}
		}


		/// <summary>
		/// Whether this widget is enabled
		/// </summary>
		public bool Enabled
		{
			get { return m_enabled; }
			set { m_enabled = value; }
		}


		/// <summary>
		/// Whether this widget is visible
		/// </summary>
		public bool Visible
		{
			get { return m_visible; }
			set { m_visible = value; }
		}


		/// <summary>
		/// Whether this widget should count for height calculations - HACK until we do real layout
		/// </summary>
		public bool CountHeight
		{
			get { return m_countHeight; }
			set { m_countHeight = value; }
		}


		/// <summary>
		/// Whether this widget should count for width calculations - HACK until we do real layout
		/// </summary>
		public bool CountWidth		
		{
			get { return m_countWidth; }
			set { m_countWidth = value; }
		}


		/// <summary>
		/// The parent widget of this widget.
		/// </summary>
		public IWidget ParentWidget
		{
			get { return m_parentWidget; }
			set { m_parentWidget = value; }
		}


		/// <summary>
		/// List of children widgets - None in the case of button widgets.
		/// </summary>
		public IWidgetCollection ChildWidgets
		{
			get { return m_ChildWidgets; }
			set { m_ChildWidgets = value; }
		}


		/// <summary>
		/// A link to an object.
		/// </summary>
		public object Tag
		{
			get { return m_tag; }
			set { m_tag = value; }
		}


		#endregion

		#region Methods

		/// <summary>
		/// Initializes the button by loading the texture, creating the sprite and figure out the scaling.
		/// 
		/// Called on the GUI thread.
		/// </summary>
		/// <param name="drawArgs">The drawing arguments passed from the WW GUI thread.</param>
		public void Initialize(DrawArgs drawArgs)
		{
			if(!m_enabled)
				return;

			if (m_TitleFont == null)
			{
				System.Drawing.Font localHeaderFont = new System.Drawing.Font("Arial", 12.0f, FontStyle.Italic | FontStyle.Bold);
				m_TitleFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, localHeaderFont);

				System.Drawing.Font wingdings = new System.Drawing.Font("Wingdings", 12.0f);
				m_wingdingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, wingdings);

				System.Drawing.Font worldwinddings = new System.Drawing.Font("World Wind dings", 12.0f);
				m_worldwinddingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, worldwinddings);
			}

			m_TextFont = drawArgs.defaultDrawingFont;

			m_isInitialized = true;
		}


		/// <summary>
		/// The render method to draw this widget on the screen.
		/// 
		/// Called on the GUI thread.
		/// </summary>
		/// <param name="drawArgs">The drawing arguments passed from the WW GUI thread.</param>
		public void Render(DrawArgs drawArgs)
		{
			if ((!m_visible) || (!m_enabled))
				return;

			if (!m_isInitialized)
			{
				Initialize(drawArgs);
			}

			int widgetTop = this.Top;
			int widgetBottom = this.Bottom;
			int widgetLeft = this.Left;
			int widgetRight = this.Right;

			m_currHeaderHeight = 0;

			#region Header Rendering

			// If we should render the header
			if(HeaderEnabled)
			{
				m_currHeaderHeight = m_headerHeight;

				WidgetUtilities.DrawBox(
					this.AbsoluteLocation.X,
					this.AbsoluteLocation.Y,
					m_size.Width,
					m_currHeaderHeight,
					0.0f,
					m_HeaderColor.ToArgb(),
					drawArgs.device);

				Rectangle nameBounds = m_TitleFont.MeasureString(
					null,
					m_name,
					DrawTextFormat.None,
					0);

				int widthLeft = m_size.Width; 

				m_TitleFont.DrawText(
					null,
					m_name,
					new System.Drawing.Rectangle(this.AbsoluteLocation.X + 2, this.AbsoluteLocation.Y + 2, widthLeft, m_currHeaderHeight),
					DrawTextFormat.None,
					m_TextColor.ToArgb());


				// if we don't render the body add whatever is in the text field as annotation
				if (!m_renderBody)
				{

					widthLeft -= nameBounds.Width + 10;
					if (widthLeft > 20)
					{
						m_TextFont.DrawText(
							null,
							Text,
							new System.Drawing.Rectangle(this.AbsoluteLocation.X + 10 + nameBounds.Width, this.AbsoluteLocation.Y + 3, widthLeft, m_currHeaderHeight),
							DrawTextFormat.None,
							m_TextColor.ToArgb());
					}
				}

				// Render border
				m_OutlineVertsHeader[0].X = AbsoluteLocation.X;
				m_OutlineVertsHeader[0].Y = AbsoluteLocation.Y;

				m_OutlineVertsHeader[1].X = AbsoluteLocation.X + m_size.Width;
				m_OutlineVertsHeader[1].Y = AbsoluteLocation.Y;

				m_OutlineVertsHeader[2].X = AbsoluteLocation.X + m_size.Width;
				m_OutlineVertsHeader[2].Y = AbsoluteLocation.Y + m_currHeaderHeight;
		
				m_OutlineVertsHeader[3].X = AbsoluteLocation.X;
				m_OutlineVertsHeader[3].Y = AbsoluteLocation.Y + m_currHeaderHeight;

				m_OutlineVertsHeader[4].X = AbsoluteLocation.X;
				m_OutlineVertsHeader[4].Y = AbsoluteLocation.Y;

				WidgetUtilities.DrawLine(m_OutlineVertsHeader, m_BorderColor.ToArgb(), drawArgs.device);
				
			}

			#endregion
			
			#region Body Rendering

			if (m_renderBody)
			{
				
				// Draw the interior background
				WidgetUtilities.DrawBox(
					this.AbsoluteLocation.X,
					this.AbsoluteLocation.Y + m_currHeaderHeight,
					m_size.Width,
					m_size.Height - m_currHeaderHeight,
					0.0f,
					m_BackgroundColor.ToArgb(),
					drawArgs.device);

				int childrenHeight = 0;
				int childrenWidth = 0;

				int bodyHeight = m_size.Height - m_currHeaderHeight;
				int bodyWidth = m_size.Width;
				
				getChildrenSize(out childrenHeight, out childrenWidth);

				// Render each child widget

				int bodyLeft = this.BodyLocation.X;
				int bodyRight = this.BodyLocation.X + this.ClientSize.Width;
				int bodyTop = this.BodyLocation.Y;
				int bodyBottom = this.BodyLocation.Y + this.ClientSize.Height;
				int childLeft = 0;
				int childRight = 0;
				int childTop = 0;
				int childBottom = 0;

				for(int index = m_ChildWidgets.Count - 1; index >= 0; index--)
				{
					IWidget currentChildWidget = m_ChildWidgets[index] as IWidget;
					if(currentChildWidget != null)
					{
						if(currentChildWidget.ParentWidget == null || currentChildWidget.ParentWidget != this)
						{
							currentChildWidget.ParentWidget = this;
						}
						System.Drawing.Point childLocation = currentChildWidget.AbsoluteLocation;

						// if any portion is visible try to render
						childLeft = childLocation.X;
						childRight = childLocation.X + currentChildWidget.WidgetSize.Width;
						childTop = childLocation.Y;
						childBottom = childLocation.Y + currentChildWidget.WidgetSize.Height;

						if ( ( ( (childLeft >= bodyLeft) && (childLeft <= bodyRight) ) ||
							( (childRight >= bodyLeft) && (childRight <= bodyRight) ) ||
							( (childLeft <= bodyLeft) && (childRight >= bodyRight) ) ) 
							&&
							( ( (childTop >= bodyTop) && (childTop <= bodyBottom) ) ||
							( (childBottom >= bodyTop) && (childBottom <= bodyBottom) ) ||
							( (childTop <= bodyTop) && (childBottom >= bodyBottom) ) )  
							)
						{
							currentChildWidget.Visible = true;
							currentChildWidget.Render(drawArgs);
						}
						else
							currentChildWidget.Visible = false;
					}
				}

				m_OutlineVerts[0].X = AbsoluteLocation.X;
				m_OutlineVerts[0].Y = AbsoluteLocation.Y + m_currHeaderHeight;

				m_OutlineVerts[1].X = AbsoluteLocation.X + m_size.Width;
				m_OutlineVerts[1].Y = AbsoluteLocation.Y + m_currHeaderHeight;

				m_OutlineVerts[2].X = AbsoluteLocation.X + m_size.Width;
				m_OutlineVerts[2].Y = AbsoluteLocation.Y + m_size.Height;
		
				m_OutlineVerts[3].X = AbsoluteLocation.X;
				m_OutlineVerts[3].Y = AbsoluteLocation.Y + m_size.Height;

				m_OutlineVerts[4].X = AbsoluteLocation.X;
				m_OutlineVerts[4].Y = AbsoluteLocation.Y + m_currHeaderHeight;

				WidgetUtilities.DrawLine(m_OutlineVerts, m_BorderColor.ToArgb(), drawArgs.device);
			}

			#endregion
		}


		#endregion

		#endregion

		#region IInteractive Members

		#region Properties

		/// <summary>
		/// Action to perform when the left mouse button is clicked
		/// </summary>
		public MouseClickAction LeftClickAction
		{
			get { return m_leftClickAction; }
			set { m_leftClickAction = value; }
		}	


		/// <summary>
		/// Action to perform when the right mouse button is clicked
		/// </summary>
		public MouseClickAction RightClickAction
		{
			get { return m_rightClickAction; }
			set { m_rightClickAction = value; }
		}	


		#endregion

		#region Methods

		/// <summary>
		/// Mouse down event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			// Whether or not the event was handled
			bool handled = false;

			// Whether or not we're in the form
			bool inClientArea = false;

			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			int widgetTop = this.Top;
			int widgetBottom = this.Bottom;
			int widgetLeft = this.Left;
			int widgetRight = this.Right;

			// if we're in the client area bring to front
			if(e.X >= widgetLeft &&
				e.X <= widgetRight &&
				e.Y >= widgetTop &&
				e.Y <= widgetBottom)
			{
				if (m_parentWidget != null)
					m_parentWidget.ChildWidgets.BringToFront(this);

				inClientArea = true;
			}

			// if its the left mouse button check for UI actions (resize, drags, etc) 
			if(e.Button == System.Windows.Forms.MouseButtons.Left)
			{

				#region header dbl click

					// Check for header double click (if its shown)
				if (HeaderEnabled &&
					e.X >= m_location.X &&
					e.X <= AbsoluteLocation.X + m_size.Width &&
					e.Y >= AbsoluteLocation.Y &&
					e.Y <= AbsoluteLocation.Y + m_currHeaderHeight)
				{
					if (DateTime.Now > m_LastClickTime.AddSeconds(0.5))
					{
						handled = true;
					}
					else
					{
						m_renderBody = !m_renderBody;
					}
					m_LastClickTime = DateTime.Now;

				}

				#endregion
			}

			// Store the current position
			m_LastMousePosition = new System.Drawing.Point(e.X, e.Y);

			// If we aren't handling this then let the children try if they are rendered
			if(!handled && inClientArea && m_renderBody)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					if(!handled)
					{
						if(m_ChildWidgets[i] is IInteractive)
						{
							IInteractive currentInteractive = m_ChildWidgets[i] as IInteractive;
							handled = currentInteractive.OnMouseDown(e);
						}
					}
				}
			}

			// If we resized or inside the form then consider it handled anyway.
			if(inClientArea)
			{
				handled = true;
			}

			return handled;			 
		}


		/// <summary>
		/// Mouse up event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			int widgetTop = this.Top;
			int widgetBottom = this.Bottom;
			int widgetLeft = this.Left;
			int widgetRight = this.Right;

			// if we're in the client area handle let the children try 
			if(e.X >= widgetLeft &&
				e.X <= widgetRight &&
				e.Y >= widgetTop &&
				e.Y <= widgetBottom)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					if(m_ChildWidgets[i] is IInteractive)
					{
						IInteractive currentInteractive = m_ChildWidgets[i] as IInteractive;
						if (currentInteractive.OnMouseUp(e))
							return true;
					}
				}
			}

			return false;
		}


		/// <summary>
		/// Mouse move event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			int deltaX = e.X - m_LastMousePosition.X;
			int deltaY = e.Y - m_LastMousePosition.Y;

			m_LastMousePosition = new System.Drawing.Point(e.X, e.Y);

			int widgetTop = this.Top;
			int widgetBottom = this.Bottom;
			int widgetLeft = this.Left;
			int widgetRight = this.Right;

			// Handle each child if we're in the client area
			if(e.X >= widgetLeft &&
				e.X <= widgetRight &&
				e.Y >= widgetTop &&
				e.Y <= widgetBottom)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					if(m_ChildWidgets[i] is IInteractive)
					{
						IInteractive currentInteractive = m_ChildWidgets[i] as IInteractive;
						
						if (currentInteractive.OnMouseMove(e))
							return true;
					}
				}
			}

			return false;
		}


		/// <summary>
		/// Mouse wheel event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return false;

			int widgetTop = this.Top;
			int widgetBottom = this.Bottom;
			int widgetLeft = this.Left;
			int widgetRight = this.Right;

			// Handle each child if we're in the client area
			if(e.X >= widgetLeft &&
				e.X <= widgetRight &&
				e.Y >= widgetTop &&
				e.Y <= widgetBottom)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					if(m_ChildWidgets[i] is IInteractive)
					{
						IInteractive currentInteractive = m_ChildWidgets[i] as IInteractive;
						
						if (currentInteractive.OnMouseWheel(e))
							return true;
					}
				}
			}

			return false;
		}


		/// <summary>
		/// Mouse entered this widget event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnMouseEnter(EventArgs e)
		{
			return false;
		}


		/// <summary>
		/// Mouse left this widget event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnMouseLeave(EventArgs e)
		{
			return false;
		}


		/// <summary>
		/// Key down event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		public bool OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			return false;
		}


		/// <summary>
		/// Key up event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
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

		#endregion
	}
}
