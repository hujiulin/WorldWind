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

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;

namespace jhuapl.util
{
	/// <summary>
	/// JHU_Globals - Singleton class that holds all the global 
	/// variables needed by the JHUAPL World Wind classes.
	/// </summary>
	public class JHU_Globals // :IDisposable
	{

		// Flag indicating if the globals have been initialized.  Typically 
		// initialized at the start of the plug-in.
		protected static bool m_initialized;

		/// <summary>
		/// The getInstance method from the singleton pattern
		/// </summary>
		/// <returns></returns>
		protected static JHU_Globals m_instance;
		public static JHU_Globals getInstance()
		{
			lock (typeof(JHU_Globals))
			{
				if (m_instance == null)
				{
					m_instance = new JHU_Globals();
				}
			}

			return m_instance;
		}

		/// <summary>
		/// The WorldWind main application that holds the WorldWindow
		/// </summary>
		protected static WorldWindow m_worldWindow;
		public WorldWindow WorldWindow
		{
			get { return m_worldWindow; }
		}

		/// <summary>
		/// Table of all icon textures
		/// </summary>
		protected static Hashtable m_textures;
		public Hashtable Textures
		{
			get { return m_textures; }
		}

		/// <summary>
		/// The root widget for all ui widgets
		/// </summary>
		protected static JHU_RootWidget m_rootWidget;
		public JHU_RootWidget RootWidget
		{
			get { return m_rootWidget; }
		}

		/// <summary>
		/// The description form that contains the description info for mouseovers
		/// </summary>
		protected static JHU_FormWidget m_infoForm;		
		public JHU_FormWidget InfoForm
		{
			get { return m_infoForm; }
		}		

		/// <summary>
		/// The widgets that holds information about an object sorted by general type.
		/// </summary>
		protected static JHU_SimpleTreeNodeWidget m_infoTree;
		public JHU_SimpleTreeNodeWidget InfoTree
		{
			get { return m_infoTree; }
		}

		protected static JHU_SimpleTreeNodeWidget m_generalInfoTreeNode;

		protected static JHU_SimpleTreeNodeWidget m_detailedInfoTreeNode;

		protected static JHU_SimpleTreeNodeWidget m_descriptionTreeNode;

		/// <summary>
		/// The general info label that shows the current mouseover target's general info
		/// </summary>
		protected static JHU_LabelWidget m_generalInfoLabel;
		public JHU_LabelWidget GeneralInfoLabel
		{
			get { return m_generalInfoLabel; }
		}

		/// <summary>
		/// The description label that shows the current mouseover target
		/// </summary>
		protected static JHU_LabelWidget m_detailedInfoLabel;
		public JHU_LabelWidget DetailedInfoLabel
		{
			get { return m_detailedInfoLabel; }
		}

		/// <summary>
		/// The description label that shows the current mouseover target
		/// </summary>
		protected static JHU_LabelWidget m_descriptionLabel;
		public JHU_LabelWidget DescriptionLabel
		{
			get { return m_descriptionLabel; }
		}

		protected static JHU_FormWidget m_controlForm;
		protected static JHU_ControlWidget m_overviewWidget;
		protected static JHU_CompassWidget m_compassWidget;
		protected static JHU_ButtonWidget m_zoomInWidget;
		protected static JHU_ButtonWidget m_zoomOutWidget;
		protected static JHU_ButtonWidget m_zoomWorldWidget;		
		protected static JHU_ButtonWidget m_zoomCountryWidget;		
		protected static JHU_ButtonWidget m_zoomStateWidget;
		protected static JHU_ButtonWidget m_zoomCityWidget;
		protected static JHU_ButtonWidget m_zoomBldgWidget;
		protected static JHU_ButtonWidget m_resetWidget;

		public JHU_FormWidget NavigatorForm
		{
			get { return m_controlForm; }
		}

		/// <summary>
		/// The base directory to look for files
		/// </summary>
		protected static string m_basePath = ".";
		public string BasePath
		{
			get { return m_basePath; }
			set { m_basePath = value; }
		}


		/// <summary>
		/// Private Constructor
		/// </summary>
		protected JHU_Globals()
		{
			m_textures = new Hashtable();
			m_initialized = false;
		}

		/// <summary> 
		/// Clean up any resources being used.  
		/// Maybe this is a dumb thing for a singleton?
		/// </summary>
		protected static void Dispose()
		{
			foreach(IconTexture iconTexture in m_textures.Values)
				iconTexture.Texture.Dispose();

			m_textures.Clear();
			m_textures = null;

			m_instance = null;
			m_initialized = false;
		}


		/// <summary>
		/// Creates the instance if it doesn't exist and initializes passed in data
		/// </summary>
		/// <param name="application"></param>
		public static void Initialize(WorldWindow worldWindow)
		{
			lock (typeof(JHU_Globals))
			{
				if (m_instance == null)
				{
					m_instance = new JHU_Globals();
				}

				m_worldWindow = worldWindow;

				if (m_rootWidget == null)
				{
					m_rootWidget = new JHU_RootWidget(m_worldWindow);
					m_rootWidget.Visible = true;
					m_rootWidget.Enabled = true;
				}

				if (m_infoForm == null)
				{
					m_infoForm = new JHU_FormWidget("Info");

					m_infoForm.WidgetSize = new System.Drawing.Size(200, 250);
					m_infoForm.Location = new System.Drawing.Point(m_rootWidget.ClientSize.Width - 201, m_rootWidget.ClientSize.Height - 271);
					m_infoForm.Anchor = JHU_Enums.AnchorStyles.Right;

					m_infoTree = new JHU_SimpleTreeNodeWidget("Info");
					m_infoTree.IsRadioButton = true;
					m_infoTree.Expanded = true;
					m_infoTree.EnableCheck = false;

					// general node

					m_generalInfoLabel = new JHU_LabelWidget("");
					m_generalInfoLabel.ClearOnRender = true;
					m_generalInfoLabel.Format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak ;
					m_generalInfoLabel.Location = new System.Drawing.Point(0, 0);
					m_generalInfoLabel.AutoSize = true;
					m_generalInfoLabel.UseParentWidth = false;

					m_generalInfoTreeNode = new JHU_SimpleTreeNodeWidget("General");
					m_generalInfoTreeNode.IsRadioButton = true;
					m_generalInfoTreeNode.Expanded = true;
					m_generalInfoTreeNode.EnableCheck = false;

					m_generalInfoTreeNode.Add(m_generalInfoLabel);
					m_infoTree.Add(m_generalInfoTreeNode);					
					
					// Detail node

					m_detailedInfoTreeNode = new JHU_SimpleTreeNodeWidget("Detailed");
					m_detailedInfoTreeNode.IsRadioButton = true;
					m_detailedInfoTreeNode.Expanded = false;
					m_detailedInfoTreeNode.EnableCheck = false;

					m_detailedInfoLabel = new JHU_LabelWidget("");
					m_detailedInfoLabel.ClearOnRender = true;
					m_detailedInfoLabel.Format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak ;
					m_detailedInfoLabel.Location = new System.Drawing.Point(0, 0);
					m_detailedInfoLabel.AutoSize = true;
					m_detailedInfoLabel.UseParentWidth = false;
					
					m_detailedInfoTreeNode.Add(m_detailedInfoLabel);
					m_infoTree.Add(m_detailedInfoTreeNode);

					// Description node 

					m_descriptionTreeNode = new JHU_SimpleTreeNodeWidget("Description");
					m_descriptionTreeNode.IsRadioButton = true;
					m_descriptionTreeNode.Expanded = false;
					m_descriptionTreeNode.EnableCheck = false;

					m_descriptionLabel = new JHU_LabelWidget("");
					m_descriptionLabel.ClearOnRender = true;
					m_descriptionLabel.Format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak ;
					m_descriptionLabel.Location = new System.Drawing.Point(0, 0);
					m_descriptionLabel.AutoSize = true;
					m_descriptionLabel.UseParentWidth = true;

					m_descriptionTreeNode.Add(m_descriptionLabel);
					m_infoTree.Add(m_descriptionTreeNode);

					// Add the tree to the info form
					m_infoForm.Add(m_infoTree);

					//m_infoForm.Add(m_descriptionLabel);
					m_rootWidget.Add(m_infoForm);
				}

				if (m_controlForm == null)
				{
					m_controlForm = new JHU_FormWidget("Navigator");
					m_controlForm.Location = new System.Drawing.Point(m_rootWidget.ClientSize.Width - 201, 0);
					m_controlForm.WidgetSize = new System.Drawing.Size(200, 242);
					m_controlForm.HorizontalScrollbarEnabled = false;
					m_controlForm.HorizontalResizeEnabled = false;
					m_controlForm.Anchor = JHU_Enums.AnchorStyles.Right;

					m_overviewWidget = new JHU_ControlWidget();
					m_controlForm.Add(m_overviewWidget);

					m_compassWidget = new JHU_CompassWidget();
					m_controlForm.Add(m_compassWidget);

					m_zoomInWidget = new JHU_ButtonWidget();
					m_zoomInWidget.Location = new System.Drawing.Point(84,100);
					m_zoomInWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformZoomIn);
					m_controlForm.Add(m_zoomInWidget);

					m_zoomOutWidget = new JHU_ButtonWidget();
					m_zoomOutWidget.ImageName = "button_out.png";
					m_zoomOutWidget.Location = new System.Drawing.Point(84,137);
					m_zoomOutWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformZoomOut);
					m_controlForm.Add(m_zoomOutWidget);

					m_zoomWorldWidget = new JHU_ButtonWidget();
					m_zoomWorldWidget.ImageName = "button_world.png";
					m_zoomWorldWidget.Location = new System.Drawing.Point(10,174);
					m_zoomWorldWidget.CountHeight = true;
					m_zoomWorldWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformWorldZoom);
					m_controlForm.Add(m_zoomWorldWidget);

					m_zoomCountryWidget = new JHU_ButtonWidget();
					m_zoomCountryWidget.ImageName = "button_country.png";
					m_zoomCountryWidget.Location = new System.Drawing.Point(47,174);
					m_zoomCountryWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformCountryZoom);
					m_controlForm.Add(m_zoomCountryWidget);

					m_zoomStateWidget = new JHU_ButtonWidget();
					m_zoomStateWidget.ImageName = "button_state.png";
					m_zoomStateWidget.Location = new System.Drawing.Point(84,174);
					m_zoomStateWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformStateZoom);
					m_controlForm.Add(m_zoomStateWidget);

					m_zoomCityWidget = new JHU_ButtonWidget();
					m_zoomCityWidget.ImageName = "button_city.png";
					m_zoomCityWidget.Location = new System.Drawing.Point(121,174);
					m_zoomCityWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformCityZoom);
					m_controlForm.Add(m_zoomCityWidget);

					m_zoomBldgWidget = new JHU_ButtonWidget();
					m_zoomBldgWidget.ImageName = "button_building.png";
					m_zoomBldgWidget.Location = new System.Drawing.Point(158,174);
					m_zoomBldgWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformBuildingZoom);
					m_controlForm.Add(m_zoomBldgWidget);

					m_resetWidget = new JHU_ButtonWidget();
					m_resetWidget.ImageName = "button_reset.png";
					m_resetWidget.Location = new System.Drawing.Point(158,100);
					m_resetWidget.LeftClickAction = new MouseClickAction(JHU_Globals.PerformReset);
					m_controlForm.Add(m_resetWidget);

					m_rootWidget.Add(m_controlForm);
				}

				m_initialized = true;
			}
		}

		public static void PerformZoomOut(System.Windows.Forms.MouseEventArgs e)
		{
			DrawArgs drawArgs = JHU_Globals.getInstance().WorldWindow.DrawArgs;

			double alt = System.Math.Round(drawArgs.WorldCamera.Altitude);
			alt = alt * 1.2;
			if (alt <= 0)
				return;
			drawArgs.WorldCamera.Altitude = alt;

			JHU_Log.Write(1, "NAV", drawArgs.WorldCamera.Latitude.Degrees, drawArgs.WorldCamera.Longitude.Degrees, alt, "", "Zoom Out Button Pressed");
		}

		public static void PerformZoomIn(System.Windows.Forms.MouseEventArgs e)
		{
			DrawArgs drawArgs = JHU_Globals.getInstance().WorldWindow.DrawArgs;

			double alt = System.Math.Round(drawArgs.WorldCamera.Altitude);
			alt = alt * 0.8;
			if (alt <= 0)
				return;
			drawArgs.WorldCamera.Altitude = alt;

			JHU_Log.Write(1, "NAV", drawArgs.WorldCamera.Latitude.Degrees, drawArgs.WorldCamera.Longitude.Degrees, alt, "", "Zoom In Button Pressed");
		}

		public static void PerformWorldZoom(System.Windows.Forms.MouseEventArgs e)
		{
			JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude = 12500000;

			JHU_Log.Write(1, "NAV", JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude, "", "World Zoom Button Pressed");
		}

		public static void PerformCountryZoom(System.Windows.Forms.MouseEventArgs e)
		{
			JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude = 3500000;

			JHU_Log.Write(1, "NAV", JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude, "", "Country Zoom Button Pressed");
		}		

		public static void PerformStateZoom(System.Windows.Forms.MouseEventArgs e)
		{
			JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude = 300000;

			JHU_Log.Write(1, "NAV", JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude, "", "State Zoom Button Pressed");
		}

		public static void PerformCityZoom(System.Windows.Forms.MouseEventArgs e)
		{
			JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude = 30000;

			JHU_Log.Write(1, "NAV", JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude, "", "City Zoom Button Pressed");
		}

		public static void PerformBuildingZoom(System.Windows.Forms.MouseEventArgs e)
		{
			JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude = 1000;

			JHU_Log.Write(1, "NAV", JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees, JHU_Globals.getInstance().WorldWindow.DrawArgs.WorldCamera.Altitude, "", "Building Zoom Button Pressed");
		}

		public static void PerformReset(System.Windows.Forms.MouseEventArgs e)
		{
			DrawArgs drawArgs = JHU_Globals.getInstance().WorldWindow.DrawArgs;

			double lat = drawArgs.WorldCamera.Latitude.Degrees;
			double lon = drawArgs.WorldCamera.Longitude.Degrees;           
			double alt = drawArgs.WorldCamera.Altitude;
			double fov = drawArgs.WorldCamera.ViewRange.Degrees;           

			JHU_Globals.getInstance().WorldWindow.GotoLatLon(lat, lon, 0, alt, fov, 0);

			JHU_Log.Write(1, "NAV", drawArgs.WorldCamera.Latitude.Degrees, drawArgs.WorldCamera.Longitude.Degrees, drawArgs.WorldCamera.Altitude, "", "Reset Button Pressed");
		}
	}
}
