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
using System.Collections;
using System.Windows.Forms;

using WorldWind;

namespace WorldWind.NewWidgets
{
	/// <summary>
	/// Delegate for mouse click events
	/// </summary>
	public delegate void MouseClickAction(System.Windows.Forms.MouseEventArgs e);

	/// <summary>
	/// Interface must be implemented in order to recieve user input.
	/// </summary>
	public interface IInteractive
	{
		#region Properties

		/// <summary>
		/// Action to perform when the left mouse button is clicked
		/// </summary>
		MouseClickAction LeftClickAction{set; get;}

		/// <summary>
		/// Action to perform when the right mouse button is clicked
		/// </summary>
		MouseClickAction RightClickAction{set; get;}

		#endregion

		#region Methods

		/// <summary>
		/// Mouse down event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnMouseDown(MouseEventArgs e);

		/// <summary>
		/// Mouse up event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnMouseUp(MouseEventArgs e);

		/// <summary>
		/// Mouse move event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnMouseMove(MouseEventArgs e);

		/// <summary>
		/// Mouse wheel event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>				
		bool OnMouseWheel(MouseEventArgs e);
		
		/// <summary>
		/// Mouse entered this widget event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnMouseEnter(EventArgs e);
		
		/// <summary>
		/// Mouse left this widget event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnMouseLeave(EventArgs e);

		/// <summary>
		/// Key down event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnKeyDown(KeyEventArgs e);
		
		/// <summary>
		/// Key up event handler.
		/// </summary>
		/// <param name="e">Event args</param>
		/// <returns>If this widget handled this event</returns>
		bool OnKeyUp(KeyEventArgs e);		

		#endregion
	}

	
	/// <summary>
	/// Base Interface for DirectX GUI Widgets
	/// </summary>
	public interface IWidget
	{
		#region Properties

		/// <summary>
		/// Name of this widget
		/// </summary>
		string Name{get;set;}

		/// <summary>
		/// Location of this widget relative to the client area of the parent
		/// </summary>
		System.Drawing.Point Location {get;set;}

		/// <summary>
		/// Where this widget is on the window
		/// </summary>
		System.Drawing.Point AbsoluteLocation{get;}

		/// <summary>
		/// The top left corner of this widget's client area
		/// </summary>
		System.Drawing.Point ClientLocation{get;}

		/// <summary>
		/// Size of widget in pixels
		/// </summary>
		System.Drawing.Size WidgetSize{get;set;}

		/// <summary>
		/// Size of the client area in pixels
		/// </summary>
		System.Drawing.Size ClientSize{get;}

		/// <summary>
		/// Whether this widget is enabled
		/// </summary>
		bool Enabled{get;set;}

		/// <summary>
		/// Whether this widget is visible
		/// </summary>
		bool Visible{get;set;}

		/// <summary>
		/// Whether this widget should count for height calculations - HACK until we do real layout
		/// </summary>
		bool CountHeight{get; set;}

		/// <summary>
		/// Whether this widget should count for width calculations - HACK until we do real layout
		/// </summary>
		bool CountWidth{get; set;}

		/// <summary>
		/// The parent widget of this widget
		/// </summary>
		IWidget ParentWidget{get;set;}

		/// <summary>
		/// List of children widgets
		/// </summary>
		IWidgetCollection ChildWidgets{get;set;}

		/// <summary>
		/// A link to an object.
		/// </summary>
		object Tag{get;set;}
		#endregion

		#region Methods

		/// <summary>
		/// The render method to draw this widget on the screen.
		/// 
		/// Called on the GUI thread.
		/// </summary>
		/// <param name="drawArgs">The drawing arguments passed from the WW GUI thread.</param>
		void Render(DrawArgs drawArgs);


		/// <summary>
		/// Initializes the button by loading the texture, creating the sprite and figure out the scaling.
		/// 
		/// Called on the GUI thread.
		/// </summary>
		/// <param name="drawArgs">The drawing arguments passed from the WW GUI thread.</param>
		void Initialize(DrawArgs drawArgs);

		#endregion
	}


	/// <summary>
	/// Collection of IWidgets
	/// </summary>
	public interface IWidgetCollection
	{
		#region Methods
		void BringToFront(int index);
		void BringToFront(IWidget widget);
		void Add(IWidget widget);
		void Clear();
		void Insert(IWidget widget, int index);
		IWidget RemoveAt(int index);
		void Remove (IWidget widget);
		#endregion

		#region Properties
		int Count{get;}
		#endregion

		#region Indexers
		IWidget this[int index] {get;set;}
		#endregion
	}

}
