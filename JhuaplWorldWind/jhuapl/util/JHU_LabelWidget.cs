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

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;

namespace jhuapl.util
{
	/// <summary>
	/// Summary description for TextLabel.
	/// </summary>
	public class JHU_LabelWidget : jhuapl.util.IWidget
	{
		string m_Text = "";
		System.Drawing.Point m_location = new System.Drawing.Point(0,0);
		System.Drawing.Size m_size = new System.Drawing.Size(0,20);
		bool m_visible = true;
		bool m_enabled = true;
		jhuapl.util.IWidget m_parentWidget = null;
		object m_tag = null;
		System.Drawing.Color m_ForeColor = System.Drawing.Color.White;
		string m_name = "";
		DrawTextFormat m_Format = DrawTextFormat.NoClip;

		protected int m_borderWidth = 5;

		protected bool m_clearOnRender = false;

		protected bool m_autoSize = true;

		protected bool m_useParentWidth = false;

		protected bool m_useParentHeight = false;

		protected bool m_isInitialized = false;

		public JHU_LabelWidget()
		{
			m_location.X = m_borderWidth;
			m_location.Y = m_borderWidth;
		}

		public JHU_LabelWidget(string text)
		{
			Text = text;
			m_location.X = m_borderWidth;
			m_location.Y = m_borderWidth;
		}
		
		#region Properties
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}
		public System.Drawing.Color ForeColor
		{
			get
			{
				return m_ForeColor;
			}
			set
			{
				m_ForeColor = value;
			}
		}
		public string Text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
				m_isInitialized = false;
			}
		}

		public DrawTextFormat Format
		{
			get { return m_Format; }
			set { m_Format = value; }
		}

		public bool ClearOnRender
		{
			get { return m_clearOnRender; }
			set { m_clearOnRender = value; }
		}

		public bool AutoSize
		{
			get { return m_autoSize; }
			set { m_autoSize = value; }
		}

		public bool UseParentWidth
		{
			get { return m_useParentWidth; }
			set { m_useParentWidth = value; }
		}

		public bool UseParentHeight
		{
			get { return m_useParentHeight; }
			set { m_useParentHeight = value; }
		}

		#endregion

		#region IWidget Members

		public jhuapl.util.IWidget ParentWidget
		{
			get
			{
				return m_parentWidget;
			}
			set
			{
				m_parentWidget = value;
			}
		}

		public bool Visible
		{
			get
			{
				return m_visible;
			}
			set
			{
				m_visible = value;
			}
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

		public object Tag
		{
			get
			{
				return m_tag;
			}
			set
			{
				m_tag = value;
			}
		}

		public jhuapl.util.IWidgetCollection ChildWidgets
		{
			get
			{
				// TODO:  Add TextLabel.ChildWidgets getter implementation
				return null;
			}
			set
			{
				// TODO:  Add TextLabel.ChildWidgets setter implementation
			}
		}

		public System.Drawing.Size ClientSize
		{
			get { return m_size; }
			set { m_size = value; }
		}

		public System.Drawing.Size WidgetSize
		{
			get 
			{ 
				if (m_parentWidget != null)
				{
					if (m_useParentWidth)
						m_size.Width = m_parentWidget.ClientSize.Width - (m_borderWidth + m_location.X);
					if (m_useParentHeight)
						m_size.Height = m_parentWidget.ClientSize.Height - (m_borderWidth + m_location.Y);	
				}
				return m_size; 
			}
			set { m_size = value; }
		}

		public bool Enabled
		{
			get { return m_enabled; }
			set { m_enabled = value; }
		}

		public System.Drawing.Point Location
		{
			get { return m_location; }
			set { m_location = value; }
		}

		public System.Drawing.Point ClientLocation
		{
			get { return this.AbsoluteLocation; }
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

		public void ComputeAutoSize (DrawArgs drawArgs)
		{
			Microsoft.DirectX.Direct3D.Font font = drawArgs.defaultDrawingFont;
			if(font==null)
				font = drawArgs.CreateFont( "", 10 );
			Rectangle bounds = font.MeasureString(null, m_Text, m_Format, 0);
			if(m_useParentWidth)
			{
				m_size.Width = this.WidgetSize.Width - m_location.X;
				m_size.Height = bounds.Height * ( (int)(bounds.Width/m_size.Width) + 1);
			}
			else
			{
				m_size.Width = bounds.Width + m_borderWidth;
				m_size.Height = bounds.Height + m_borderWidth;
			}

			if(m_useParentHeight)
				m_size.Height = this.WidgetSize.Height - m_location.Y;

			// This code is iffy - no idea why Y is offset by more than specified.
			if (m_location.X == 0)
			{
				m_location.X = m_borderWidth;
				m_size.Width += m_borderWidth;
			}
			if (m_location.Y == 0)
			{
				m_location.Y = m_borderWidth;
				m_size.Height += m_borderWidth;
			}
		}
			
		public void Initialize(DrawArgs drawArgs)
		{
			if (m_autoSize)
				ComputeAutoSize (drawArgs);
			m_isInitialized = true;
		}

		public void Render(DrawArgs drawArgs)
		{
			// if we aren't active do nothing.
			if ((!m_visible) || (!m_enabled))
				return;			

			if (!m_isInitialized)
				Initialize(drawArgs);

			drawArgs.defaultDrawingFont.DrawText(
				null,
				m_Text,
				new System.Drawing.Rectangle(AbsoluteLocation.X, AbsoluteLocation.Y, m_size.Width, m_size.Height),
				m_Format,
				m_ForeColor);

			if (m_clearOnRender)
			{
				m_Text = "";
				m_isInitialized = false;
			}
		}

		#endregion
	}
}
