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

namespace jhuapl.util
{
	/// <summary>
	/// JHU_FormWidget - This class implements a basic form with no layout management whatsoever but
	/// will resize and generate scrollbars.  JHU_FormWidget can take any widget as a child including
	/// other form widgets.  Typically all other widgets reside in a form widget.
	/// 
	/// Note:  You can nest form widgets (maybe) but not put form widgets inside other widgets 
	/// because each form widget creates a new ViewPort.  Typically a JHU_PanelWidget should work
	/// fine instead.
	/// </summary>
	public class JHU_FormWidget : JHU_WidgetCollection, jhuapl.util.IWidget, jhuapl.util.IInteractive
	{
		/// <summary>
		/// Possible Resize Directions
		/// </summary>
		[Flags]
		public enum ResizeDirection : ushort
		{
			None = 0x00,
			Left = 0x01,
			Right = 0x02,
			Up = 0x04,
			Down = 0x08,
			UL = 0x05,
			UR = 0x06,
			DL = 0x09,
			DR = 0x0A
		}

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
		protected jhuapl.util.IWidget m_parentWidget = null;

		/// <summary>
		/// ChildWidget property value
		/// </summary>
		protected IWidgetCollection m_ChildWidgets = new JHU_WidgetCollection();

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
//		96,
//		32,
//		32,
//		32);

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

		#region vertical scroll bar members

		/// <summary>
		/// Width of vertical scrollbar
		/// </summary>
		protected int m_scrollbarWidth = 20;

		protected int m_vScrollbarPos = 0;
		protected int m_vScrollbarHeight = 0;
		protected double m_vScrollbarPercent = 0.0;

		// where we grabbed the scroll bar in a drag
		protected int m_vScrollbarGrabPosition = 0;

		// True if the vertical scroll bar must be visible because the client height is too small
		protected bool m_showVScrollbar = false;

		// True if we are currently dragging the scroll bar
		protected bool m_isVScrolling = false;

		#endregion

		#region Horizontal scroll bar members

		/// <summary>
		/// Height of horizontal scrollbar
		/// </summary>
		protected int m_scrollbarHeight = 20;

		protected int m_hScrollbarPos = 0;
		protected int m_hScrollbarWidth = 0;
		protected double m_hScrollbarPercent = 0.0;

		// where we grabbed the scroll bar in a drag
		protected int m_hScrollbarGrabPosition = 0;

		// True if the horizontal scroll bar must be visible because the client width is too small
		protected bool m_showHScrollbar = false;

		// True if we are currently dragging the scroll bar
		protected bool m_isHScrolling = false;

		#endregion

		/// <summary>
		/// Whether or not to render the body.
		/// </summary>
		protected bool m_renderBody = true;

		protected Microsoft.DirectX.Direct3D.Font m_TextFont; 
		protected Microsoft.DirectX.Direct3D.Font m_TitleFont; 
		protected Microsoft.DirectX.Direct3D.Font m_wingdingsFont;
		protected Microsoft.DirectX.Direct3D.Font m_worldwinddingsFont;

		/// <summary>
		/// Region around widget that counts for grabbing when trying to resize the widget.
		/// </summary>
		protected int resizeBuffer = 5;

		protected Vector2[] m_OutlineVertsHeader = new Vector2[5];
		protected Vector2[] m_OutlineVerts = new Vector2[5];

		/// <summary>
		/// True if we're dragging the form around
		/// </summary>
		protected bool m_isDragging = false;

		/// <summary>
		/// Last point where the mouse was clicked (mousedown).
		/// </summary>
		protected System.Drawing.Point m_LastMousePosition = new System.Drawing.Point(0,0);

		/// <summary>
		/// Last time the mouse clicked on this widget (header area mostly) - used to implement double click
		/// </summary>
		protected DateTime m_LastClickTime;

		/// <summary>
		/// Current resizing direction
		/// </summary>
		protected ResizeDirection m_resize = ResizeDirection.None;

		protected int m_distanceFromTop = 0;
		protected int m_distanceFromBottom = 0;
		protected int m_distanceFromLeft = 0;
		protected int m_distanceFromRight = 0;

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

		/// <summary>
		/// Whether or not to hide the header when form doesn't have focus..
		/// </summary>
		public bool AutoHideHeader = false;

		/// <summary>
		/// Minimum drawing size
		/// </summary>
		public System.Drawing.Size MinSize = new System.Drawing.Size(20, 100);

		/// <summary>
		/// Flag that indicates whether the user can resize vertically
		/// </summary>
		public bool VerticalResizeEnabled = true;

		/// <summary>
		/// Flag that indicates whether the user can resize horizontally
		/// </summary>
		public bool HorizontalResizeEnabled = true;

		/// <summary>
		/// True if we allow the showing of the vertical scroll bar (clips otherwise)
		/// </summary>
		public bool VerticalScrollbarEnabled = true;

		/// <summary>
		/// True if we allow the showing of the horizontal scroll bar (clips otherwise)
		/// </summary>
		public bool HorizontalScrollbarEnabled = true;

		/// <summary>
		/// Flag that indicates this form should get deleted on close
		/// </summary>
		public bool DestroyOnClose = false;

		public JHU_Enums.AnchorStyles Anchor = JHU_Enums.AnchorStyles.None;

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
		public JHU_FormWidget(string name)
		{
			m_name = name;
		}

		/// <summary>
		/// Adds a new child widget
		/// </summary>
		/// <param name="widget">The widget to be added</param>
		new public void Add(jhuapl.util.IWidget widget)
		{
			m_ChildWidgets.Add(widget);
			widget.ParentWidget = this;
		}

		/// <summary>
		/// Removes a child widget
		/// </summary>
		/// <param name="widget">The widget to be removed</param>
		new public void Remove(jhuapl.util.IWidget widget)
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
			get 
			{
				// multiple anchors not supported.
				// ignore top and left anchors
				if ( (Anchor == JHU_Enums.AnchorStyles.Bottom) && (m_parentWidget != null))
				{
					// if the distance has changed then reset the location.
					if (m_location.Y - m_parentWidget.ClientSize.Height != m_distanceFromBottom)
					{
						m_location.Y = m_parentWidget.ClientSize.Height - m_distanceFromBottom;
					}
				}
				if ( (Anchor == JHU_Enums.AnchorStyles.Right) && (m_parentWidget != null))
				{
					// if the distance has changed then reset the location.
					if (m_location.X - m_parentWidget.ClientSize.Width != m_distanceFromRight)
					{
						m_location.X = m_parentWidget.ClientSize.Width - m_distanceFromRight;
					}
				}
				return m_location; 
			}
			set 
			{ 
				m_location = value; 
				UpdateLocation();
			}
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
						Location.X + m_parentWidget.ClientLocation.X,
						Location.Y + m_parentWidget.ClientLocation.Y);
				}
				else
				{
					return this.Location;
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
				if (m_showVScrollbar)
				{
					if (m_vScrollbarPercent < .01)
						m_vScrollbarPercent = .01;
					m_clientLocation.Y -= (int) (m_vScrollbarPos / m_vScrollbarPercent);
				}
				if (m_showHScrollbar)
				{
					if (m_hScrollbarPercent < .01)
						m_hScrollbarPercent = .01;
					m_clientLocation.X -= (int) (m_hScrollbarPos / m_hScrollbarPercent);
				}
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

				// if scroll bars deduct those sizes
				if (m_showHScrollbar)
					m_clientSize.Height -= m_scrollbarHeight;

				if (m_showVScrollbar)
					m_clientSize.Width -= m_scrollbarWidth;

				return m_clientSize; 
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
		public jhuapl.util.IWidget ParentWidget
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

			UpdateLocation();

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

			#region Resize Crosshair rendering

			if(WorldWind.DrawArgs.LastMousePosition.X > widgetLeft - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetLeft + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetTop - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetTop + resizeBuffer)
			{
				WorldWind.DrawArgs.MouseCursor = CursorType.Cross;
			}
			else if(WorldWind.DrawArgs.LastMousePosition.X > widgetRight - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetRight + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetTop - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetTop + resizeBuffer)
			{
				WorldWind.DrawArgs.MouseCursor = CursorType.Cross;
			}
			else if(WorldWind.DrawArgs.LastMousePosition.X > widgetLeft - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetLeft + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetBottom - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetBottom + resizeBuffer)
			{
				WorldWind.DrawArgs.MouseCursor = CursorType.Cross;
			}
			else if(WorldWind.DrawArgs.LastMousePosition.X > widgetRight - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetRight + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetBottom - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetBottom + resizeBuffer)
			{
				WorldWind.DrawArgs.MouseCursor = CursorType.Cross;
			}
			else if(
				(WorldWind.DrawArgs.LastMousePosition.X > widgetLeft - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetLeft + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetTop - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetBottom + resizeBuffer) ||
				(WorldWind.DrawArgs.LastMousePosition.X > widgetRight - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetRight + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetTop - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetBottom + resizeBuffer))
			{
				WorldWind.DrawArgs.MouseCursor = CursorType.SizeWE;
			}
			else if(
				(WorldWind.DrawArgs.LastMousePosition.X > widgetLeft - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetRight + resizeBuffer&&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetTop - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetTop + resizeBuffer) ||
				(WorldWind.DrawArgs.LastMousePosition.X > widgetLeft- resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.X < widgetRight + resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y > widgetBottom - resizeBuffer &&
				WorldWind.DrawArgs.LastMousePosition.Y < widgetBottom + resizeBuffer))
			{
				WorldWind.DrawArgs.MouseCursor = CursorType.SizeWE;
			}

			#endregion

			m_currHeaderHeight = 0;

			#region Header Rendering

			// If we should render the header or if we're in the body (when autohide enabled) then render the header
			if(HeaderEnabled || (AutoHideHeader &&
				WorldWind.DrawArgs.LastMousePosition.X >= widgetLeft &&
				WorldWind.DrawArgs.LastMousePosition.X <= widgetRight &&
				WorldWind.DrawArgs.LastMousePosition.Y >= widgetTop &&
				WorldWind.DrawArgs.LastMousePosition.Y <= widgetBottom))
			{
				m_currHeaderHeight = m_headerHeight;

				JHU_Utilities.DrawBox(
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

				int widthLeft = m_size.Width - 20;  // account for close box

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

				m_worldwinddingsFont.DrawText(
					null,
					"E",
					new System.Drawing.Rectangle(this.AbsoluteLocation.X + m_size.Width - 18, this.AbsoluteLocation.Y+2, 20, m_currHeaderHeight),
					DrawTextFormat.None,
					m_TextColor.ToArgb());

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

				JHU_Utilities.DrawLine(m_OutlineVertsHeader, m_BorderColor.ToArgb(), drawArgs.device);
				
			}

			#endregion
			
			#region Body Rendering

			if (m_renderBody)
			{
				
				// Draw the interior background
				JHU_Utilities.DrawBox(
					this.AbsoluteLocation.X,
					this.AbsoluteLocation.Y + m_currHeaderHeight,
					m_size.Width,
					m_size.Height - m_currHeaderHeight,
					0.0f,
					m_BackgroundColor.ToArgb(),
					drawArgs.device);

				// Render scrollbars
				int childrenHeight = 0;
				int childrenWidth = 0;

				int bodyHeight = m_size.Height - m_currHeaderHeight;
				int bodyWidth = m_size.Width;
				
				getChildrenSize(out childrenHeight, out childrenWidth);

				// reset the scroll bars flag so we can retest if we need them
				m_showVScrollbar = false;
				m_showHScrollbar = false;

				// if the children are too high turn on the verticle scrollbar
				if ( (childrenHeight > bodyHeight) && (VerticalScrollbarEnabled) )
				{
					// deduct the vertical scrollbar width
					m_showVScrollbar = true;
					bodyWidth -= m_scrollbarWidth;

					// if the children are too wide turn on the horizontal
					if ( (childrenWidth > bodyWidth) && (HorizontalScrollbarEnabled) )
					{
						m_showHScrollbar = true;
						bodyHeight -= m_scrollbarHeight;
					}
				}
				else
				{
					// if children are too wide turn on horizontal scrollbar
					if ( (childrenWidth > m_size.Width) && (HorizontalScrollbarEnabled) )
					{
						m_showHScrollbar = true;
						bodyHeight -= m_scrollbarHeight;

						// if the horizontal scrollbar takes up too much room turn on the verticle too
						if ( (childrenHeight > bodyHeight) && (VerticalScrollbarEnabled) )
						{
							m_showVScrollbar = true;
							bodyWidth -= m_scrollbarWidth;
						}
					}
				}

				// Render verticle scrollbar if there is one
				if (m_showVScrollbar)
				{
					m_vScrollbarPercent = (double)bodyHeight/(double)childrenHeight;
					m_vScrollbarHeight = (int)(bodyHeight * m_vScrollbarPercent);

					if (m_vScrollbarPos < 0)
					{
						m_vScrollbarPos = 0;
					} 
					else if (m_vScrollbarPos > bodyHeight - m_vScrollbarHeight)
					{
						m_vScrollbarPos = bodyHeight - m_vScrollbarHeight;
					}

					int color = (m_isVScrolling ? System.Drawing.Color.White.ToArgb() : System.Drawing.Color.Gray.ToArgb());
					JHU_Utilities.DrawBox(
						BodyLocation.X + bodyWidth + 2,
						BodyLocation.Y + m_vScrollbarPos + 1,
						m_scrollbarWidth - 3,
						m_vScrollbarHeight - 2,
						0.0f,
						color,
						drawArgs.device);

					m_OutlineVerts[0].X = AbsoluteLocation.X + m_size.Width - m_scrollbarWidth;
					m_OutlineVerts[0].Y = AbsoluteLocation.Y + m_currHeaderHeight;

					m_OutlineVerts[1].X = AbsoluteLocation.X + m_size.Width;
					m_OutlineVerts[1].Y = AbsoluteLocation.Y + m_currHeaderHeight;

					m_OutlineVerts[2].X = AbsoluteLocation.X + m_size.Width ;
					m_OutlineVerts[2].Y = AbsoluteLocation.Y + m_size.Height;
		
					m_OutlineVerts[3].X = AbsoluteLocation.X + m_size.Width - m_scrollbarWidth;
					m_OutlineVerts[3].Y = AbsoluteLocation.Y + m_size.Height;

					m_OutlineVerts[4].X = AbsoluteLocation.X + m_size.Width - m_scrollbarWidth;
					m_OutlineVerts[4].Y = AbsoluteLocation.Y + m_currHeaderHeight;

					JHU_Utilities.DrawLine(m_OutlineVerts, m_BorderColor.ToArgb(), drawArgs.device);

				}
				else
				{
					m_vScrollbarPos = 0;
				}

				if (m_showHScrollbar)
				{
					m_hScrollbarPercent = (double)bodyWidth/(double)childrenWidth;
					m_hScrollbarWidth = (int)(bodyWidth * m_hScrollbarPercent);

					if (m_hScrollbarPos < 0)
					{
						m_hScrollbarPos = 0;
					} 
					else if (m_hScrollbarPos > bodyWidth - m_hScrollbarWidth)
					{
						m_hScrollbarPos = bodyWidth - m_hScrollbarWidth;
					}

					int color = (m_isHScrolling ? System.Drawing.Color.White.ToArgb() : System.Drawing.Color.Gray.ToArgb());
					JHU_Utilities.DrawBox(
						BodyLocation.X + m_hScrollbarPos + 1,
						BodyLocation.Y + bodyHeight + 2,
						m_hScrollbarWidth - 3,
						m_scrollbarHeight - 2,
						0.0f,
						color,
						drawArgs.device);

					m_OutlineVerts[0].X = AbsoluteLocation.X;
					m_OutlineVerts[0].Y = AbsoluteLocation.Y + bodyHeight + m_currHeaderHeight;

					m_OutlineVerts[1].X = AbsoluteLocation.X + m_size.Width;
					m_OutlineVerts[1].Y = AbsoluteLocation.Y + bodyHeight + m_currHeaderHeight;

					m_OutlineVerts[2].X = AbsoluteLocation.X + m_size.Width;
					m_OutlineVerts[2].Y = AbsoluteLocation.Y + m_size.Height;
		
					m_OutlineVerts[3].X = AbsoluteLocation.X;
					m_OutlineVerts[3].Y = AbsoluteLocation.Y + m_size.Height;

					m_OutlineVerts[4].X = AbsoluteLocation.X ;
					m_OutlineVerts[4].Y = AbsoluteLocation.Y + bodyHeight + m_currHeaderHeight;

					JHU_Utilities.DrawLine(m_OutlineVerts, m_BorderColor.ToArgb(), drawArgs.device);
				}
				else
				{
					m_hScrollbarPos = 0;
				}

				// Render each child widget

				// create the client viewport to clip child objects
				Viewport clientViewPort = new Viewport();

				clientViewPort.X = BodyLocation.X;
				clientViewPort.Y = BodyLocation.Y;

				clientViewPort.Width = ClientSize.Width;
				clientViewPort.Height = ClientSize.Height;

				if (this.m_parentWidget != null)
				{
					if (BodyLocation.X + ClientSize.Width > m_parentWidget.ClientSize.Width + m_parentWidget.ClientLocation.X)
						clientViewPort.Width = (m_parentWidget.ClientSize.Width + m_parentWidget.ClientLocation.X) - BodyLocation.X;

					if (BodyLocation.Y + ClientSize.Height > m_parentWidget.ClientSize.Height + m_parentWidget.ClientLocation.Y)
						clientViewPort.Height = (m_parentWidget.ClientSize.Height + m_parentWidget.ClientLocation.Y) - BodyLocation.Y;
				}

				// save the original viewport
				Viewport defaultViewPort = drawArgs.device.Viewport;

				// replace with client viewport
				drawArgs.device.Viewport = clientViewPort;

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
					jhuapl.util.IWidget currentChildWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;
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

				// restore normal viewport
				drawArgs.device.Viewport = defaultViewPort;

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

				JHU_Utilities.DrawLine(m_OutlineVerts, m_BorderColor.ToArgb(), drawArgs.device);
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
				// Reset dragging and resizing
				m_isDragging = false;
				m_resize = ResizeDirection.None;

				// Reset scrolling
				m_isVScrolling = false;
				m_isHScrolling = false;

				#region resize and dragging

				// Check for resize (pointer is just outside the form)
				if(e.X > widgetLeft - resizeBuffer &&
					e.X < widgetLeft + resizeBuffer &&
					e.Y > widgetTop - resizeBuffer &&
					e.Y < widgetTop + resizeBuffer)
				{
					if ((HorizontalResizeEnabled) && (VerticalResizeEnabled))
						m_resize = ResizeDirection.UL;
					else if (HorizontalResizeEnabled)
						m_resize = ResizeDirection.Left;
					else if (VerticalResizeEnabled)
						m_resize = ResizeDirection.Up;
				}
				else if(e.X > widgetRight - resizeBuffer &&
					e.X < widgetRight + resizeBuffer &&
					e.Y > widgetTop - resizeBuffer &&
					e.Y < widgetTop + resizeBuffer)
				{
					if ((HorizontalResizeEnabled) && (VerticalResizeEnabled))
						m_resize = ResizeDirection.UR;
					else if (HorizontalResizeEnabled)
						m_resize = ResizeDirection.Right;
					else if (VerticalResizeEnabled)
						m_resize = ResizeDirection.Up;
				}
				else if(e.X > widgetLeft - resizeBuffer &&
					e.X < widgetLeft + resizeBuffer &&
					e.Y > widgetBottom - resizeBuffer &&
					e.Y < widgetBottom + resizeBuffer)
				{
					if ((HorizontalResizeEnabled) && (VerticalResizeEnabled))
						m_resize = ResizeDirection.DL;
					else if (HorizontalResizeEnabled)
						m_resize = ResizeDirection.Left;
					else if (VerticalResizeEnabled)
						m_resize = ResizeDirection.Down;
				}
				else if(e.X > widgetRight - resizeBuffer &&
					e.X < widgetRight + resizeBuffer &&
					e.Y > widgetBottom - resizeBuffer &&
					e.Y < widgetBottom + resizeBuffer )
				{
					if ((HorizontalResizeEnabled) && (VerticalResizeEnabled))
						m_resize = ResizeDirection.DR;
					else if (HorizontalResizeEnabled)
						m_resize = ResizeDirection.Right;
					else if (VerticalResizeEnabled)
						m_resize = ResizeDirection.Down;
				}
				else if(e.X > AbsoluteLocation.X - resizeBuffer &&
					e.X < AbsoluteLocation.X + resizeBuffer &&
					e.Y > AbsoluteLocation.Y - resizeBuffer &&
					e.Y < AbsoluteLocation.Y + resizeBuffer + m_size.Height &&
					HorizontalResizeEnabled)
				{
					m_resize = ResizeDirection.Left;
				}
				else if(e.X > AbsoluteLocation.X - resizeBuffer + m_size.Width &&
					e.X < AbsoluteLocation.X + resizeBuffer + m_size.Width &&
					e.Y > AbsoluteLocation.Y - resizeBuffer &&
					e.Y < AbsoluteLocation.Y + resizeBuffer + m_size.Height &&
					HorizontalResizeEnabled)
				{
					m_resize = ResizeDirection.Right;
				}
				else if(e.X > AbsoluteLocation.X - resizeBuffer &&
					e.X < AbsoluteLocation.X + resizeBuffer + m_size.Width &&
					e.Y > AbsoluteLocation.Y - resizeBuffer &&
					e.Y < AbsoluteLocation.Y + resizeBuffer &&
					VerticalResizeEnabled)
				{
					m_resize = ResizeDirection.Up;
				}
				else if(e.X > AbsoluteLocation.X - resizeBuffer &&
					e.X < AbsoluteLocation.X + resizeBuffer + m_size.Width &&
					e.Y > AbsoluteLocation.Y - resizeBuffer + m_size.Height &&
					e.Y < AbsoluteLocation.Y + resizeBuffer + m_size.Height &&
					VerticalResizeEnabled)
				{
					m_resize = ResizeDirection.Down;
				}
					// Check for close if header is rendered
				else if(HeaderEnabled &&
					e.X >= Location.X + m_size.Width - 18 &&
					e.X <= AbsoluteLocation.X + m_size.Width &&
					e.Y >= AbsoluteLocation.Y + 2 &&
					e.Y <= AbsoluteLocation.Y + m_currHeaderHeight - 2)
				{
					Visible = false;
					if (DestroyOnClose)
					{
						Enabled = false;

						JHU_WidgetCollection parentCollection = (JHU_WidgetCollection) m_parentWidget;
						if (parentCollection != null)
							parentCollection.Remove(this);

						this.Dispose();
					}
				}
					// Check for header double click (if its shown)
				else if(HeaderEnabled &&
					e.X >= Location.X &&
					e.X <= AbsoluteLocation.X + m_size.Width &&
					e.Y >= AbsoluteLocation.Y &&
					e.Y <= AbsoluteLocation.Y + m_currHeaderHeight)
				{
					if (DateTime.Now > m_LastClickTime.AddSeconds(0.5))
					{
						m_isDragging = true;
						handled = true;
					}
					else
					{
						m_renderBody = !m_renderBody;
						if (AutoHideHeader && m_renderBody)
						{
							HeaderEnabled = false;
						}
						else
						{
							HeaderEnabled = true;
						}
					}
					m_LastClickTime = DateTime.Now;

				}

				#endregion

				#region scrolling

				if (inClientArea && m_renderBody)
				{
					// Check to see if we're in vertical scroll region
					if( m_showVScrollbar &&
						e.X > this.Right - m_scrollbarWidth &&
						(!m_showHScrollbar || e.Y < this.Bottom - m_scrollbarWidth) )
					{
						// set scroll position to e.Y offset from top of client area
						if (e.Y < this.BodyLocation.Y + m_vScrollbarPos)
						{
							m_vScrollbarPos -= m_clientSize.Height / 10;
						}
						else if (e.Y > this.BodyLocation.Y + m_vScrollbarPos + m_vScrollbarHeight)
						{
							m_vScrollbarPos += m_clientSize.Height / 10;
						}
						else
						{
							m_vScrollbarGrabPosition = e.Y - this.BodyLocation.Y;
							m_isVScrolling = true;
						}
						handled = true;
					}
					else if( m_showHScrollbar &&
						e.Y > this.Bottom - m_scrollbarWidth &&
						(!m_showVScrollbar || e.X < this.Right - m_scrollbarWidth) )
					{
						// set scroll position to e.Y offset from top of client area
						if (e.X < this.BodyLocation.X + m_hScrollbarPos)
						{
							m_hScrollbarPos -= m_clientSize.Width / 10;
						}
						else if (e.X > this.BodyLocation.X + m_hScrollbarPos + m_hScrollbarWidth)
						{
							m_hScrollbarPos += m_clientSize.Width / 10;
						}
						else
						{
							m_hScrollbarGrabPosition = e.X - this.BodyLocation.X;
							m_isHScrolling = true;
						}
						handled = true;
					}
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
						if(m_ChildWidgets[i] is jhuapl.util.IInteractive)
						{
							jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[i] as jhuapl.util.IInteractive;
							handled = currentInteractive.OnMouseDown(e);
						}
					}
				}
			}

			// If we resized or inside the form then consider it handled anyway.
			if(inClientArea || (m_resize != ResizeDirection.None))
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

			// reset scrolling flags (don't care what up button event it is)
			m_isVScrolling = false;
			m_isHScrolling = false;

			// if its the left mouse button then reset dragging and resizing
			if (((m_isDragging) || (m_resize != ResizeDirection.None)) && 
				(e.Button == System.Windows.Forms.MouseButtons.Left))
			{
				// reset dragging flags
				m_isDragging = false;
				m_resize = ResizeDirection.None;

				return true;
			}

			// if we're in the client area handle let the children try 
			if(e.X >= widgetLeft &&
				e.X <= widgetRight &&
				e.Y >= widgetTop &&
				e.Y <= widgetBottom)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					if(m_ChildWidgets[i] is jhuapl.util.IInteractive)
					{
						jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[i] as jhuapl.util.IInteractive;
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

			if(m_isDragging)
			{
				m_location.X += deltaX;
				m_location.Y += deltaY;

				// Since we've modified m_location update the anchor calcs.
				UpdateLocation();

				return true;
			}

			// If we're resizing handle it
			if (m_resize != ResizeDirection.None)
			{
				if ((ResizeDirection.Up & m_resize) > 0)
				{
					m_location.Y += deltaY;
					m_size.Height -= deltaY;
				}
				if ((ResizeDirection.Down & m_resize) > 0)
				{
					m_size.Height += deltaY;
				}
				if ((ResizeDirection.Right & m_resize) > 0)
				{
					m_size.Width += deltaX;
				}
				if ((ResizeDirection.Left & m_resize) > 0)
				{
					m_location.X += deltaX;
					m_size.Width -= deltaX;
				}

				if(m_size.Width < MinSize.Width)
				{
					m_size.Width = MinSize.Width;
				}

				if(m_size.Height < MinSize.Height)
				{
					m_size.Height = MinSize.Height;
				}

				// TODO - Resize all the child widgets

				// Since we've modified m_location update the anchor calcs.
				UpdateLocation();

				return true;
			}

			if (m_isVScrolling)
			{
				m_vScrollbarPos = e.Y - m_vScrollbarGrabPosition - this.BodyLocation.Y;
				return true;
			}

			if (m_isHScrolling)
			{
				m_hScrollbarPos = e.X - m_hScrollbarGrabPosition - this.BodyLocation.X;
				return true;
			}

			// Handle each child if we're in the client area
			if(e.X >= widgetLeft &&
				e.X <= widgetRight &&
				e.Y >= widgetTop &&
				e.Y <= widgetBottom)
			{
				for(int i = 0; i < m_ChildWidgets.Count; i++)
				{
					if(m_ChildWidgets[i] is jhuapl.util.IInteractive)
					{
						jhuapl.util.IInteractive currentInteractive = m_ChildWidgets[i] as jhuapl.util.IInteractive;
						
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

			// if we're not in the client area
			if(e.X > this.Right || e.X < this.Left  || e.Y < this.Top || e.Y > this.Bottom)
				return false;

			// Mouse wheel scroll
			this.m_vScrollbarPos -= (e.Delta/10);

			return true;
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


		protected void UpdateLocation()
		{
			int height;
			int width;

			if (m_parentWidget != null)
			{
				height =  m_parentWidget.ClientSize.Height;
				width = m_parentWidget.ClientSize.Width;
			}
			else
			{
				height = JHU_Globals.getInstance().WorldWindow.DrawArgs.screenHeight;
				width = JHU_Globals.getInstance().WorldWindow.DrawArgs.screenWidth;
			}

			// if anchors are active recompute distances from anchor location
			if (Anchor != JHU_Enums.AnchorStyles.None)
			{
				// Compute our distance from the edges
				m_distanceFromTop = m_location.Y;
				m_distanceFromLeft = m_location.X;
				m_distanceFromBottom = height - m_location.Y;
				m_distanceFromRight = width - m_location.X;

				// Make sure the distance makes sense
				if (m_distanceFromTop < 0) m_distanceFromTop = 0;
				if (m_distanceFromBottom < m_currHeaderHeight) m_distanceFromBottom = m_currHeaderHeight;
				if (m_distanceFromLeft < 0) m_distanceFromLeft = 0;
				if (m_distanceFromRight < m_currHeaderHeight) m_distanceFromRight = m_currHeaderHeight;
			}

			// If we're off the top or left edge
			if(m_location.X < 0)
				m_location.X = 0;
			if(m_location.Y < 0)
				m_location.Y = 0;

			// If we're off the bottom or right edge
			if(m_location.Y + m_headerHeight > height)
				m_location.Y = height - m_currHeaderHeight;
			if(m_location.X + m_headerHeight > width)
				m_location.X = width - m_currHeaderHeight;
		}

		#endregion

		#endregion
	}
}
