//----------------------------------------------------------------------------
// NAME: Navigator
// VERSION: 0.9
// DESCRIPTION: iWidget example code:  Navigator
// DEVELOPER: Nigel Tzeng, Osbaldo Cantu, Nathan Koterba
// WEBSITE: http://www.jhuapl.edu
// REFERENCES: System.Data, JhuaplWorldWind.dll
//----------------------------------------------------------------------------
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
using System.Threading;
using System.Timers;
using System.Collections;
using System.Windows;
using System.Windows.Forms;

using WorldWind;
using WorldWind.Menu;
using WorldWind.Renderable;

using jhuapl.util;

namespace jhuapl.collab.CollabSpace
{
	/// <summary>
	/// 
	/// </summary>
	public class Navigator : WorldWind.PluginEngine.Plugin
	{
		protected NavigatorMenuButton m_menuButton;

		protected System.Windows.Forms.MenuItem m_navMenuItem;

		protected System.Windows.Forms.MenuItem m_infoMenuItem;

		public System.Windows.Forms.MenuItem NavMenu
		{
			get { return m_navMenuItem; }
		}

		public System.Windows.Forms.MenuItem InfoMenu
		{
			get { return m_infoMenuItem; }
		}

		public Navigator()
		{
		}
	
		public override void Load()
		{
			JHU_Globals.Initialize(ParentApplication.WorldWindow);
			JHU_Globals.getInstance().BasePath = this.PluginDirectory + @"\Plugins\Navigator\";

			// Add our menu button
            m_menuButton = new NavigatorMenuButton(this.PluginDirectory + @"\Plugins\Navigator\Data\Icons\Interface\Navigator.png", this);
			ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_menuButton);

			// Add our navigation menu item
			m_navMenuItem = new System.Windows.Forms.MenuItem();
			m_navMenuItem.Text = "Hide Navigator\tN";
			m_navMenuItem.Click += new System.EventHandler(navMenuItem_Click);
			ParentApplication.ToolsMenu.MenuItems.Add(m_navMenuItem);

			// Add our info menu item
			m_infoMenuItem = new System.Windows.Forms.MenuItem();
			m_infoMenuItem.Text = "Hide Info\tI";
			m_infoMenuItem.Click += new System.EventHandler(infoMenuItem_Click);
			ParentApplication.ToolsMenu.MenuItems.Add(m_infoMenuItem);

			ParentApplication.WorldWindow.KeyUp += new KeyEventHandler(keyUp);

			JHU_Globals.getInstance().NavigatorForm.Enabled = true;
			JHU_Globals.getInstance().NavigatorForm.Visible = true;

			JHU_Globals.getInstance().InfoForm.Enabled = true;
			JHU_Globals.getInstance().InfoForm.Visible = true;

			base.Load ();
		}
	
		public override void Unload()
		{
			// Reset the bottom for the Layer Manager
			m_menuButton.SetPushed(false);

			// Clean up and remove menu item
			//ParentApplication.WorldWindow.MenuBar.RemoveToolsMenuButton(m_menuButton);

			base.Unload ();
		}

		protected void navMenuItem_Click(object sender, EventArgs s)
		{
			if (JHU_Globals.getInstance().NavigatorForm.Enabled)
			{
				JHU_Globals.getInstance().NavigatorForm.Enabled = false;
				m_navMenuItem.Text = "Show Navigator\tN";
			}
			else
			{
				JHU_Globals.getInstance().NavigatorForm.Enabled = true;
				JHU_Globals.getInstance().NavigatorForm.Visible = true;
				m_navMenuItem.Text = "Hide Navigator\tN";
			}
		}

		protected void infoMenuItem_Click(object sender, EventArgs s)
		{
			if (JHU_Globals.getInstance().InfoForm.Enabled)
			{
				JHU_Globals.getInstance().InfoForm.Enabled = false;
				m_infoMenuItem.Text = "Show Info\tI";
			}
			else
			{
				JHU_Globals.getInstance().InfoForm.Enabled = true;
				JHU_Globals.getInstance().InfoForm.Visible = true;
				m_infoMenuItem.Text = "Hide Info\tI";
			}
		}

		protected void keyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData==Keys.N)
			{
				navMenuItem_Click(sender, e);
			} 
			else if (e.KeyData==Keys.I)
			{
				infoMenuItem_Click(sender, e);
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class NavigatorMenuButton : MenuButton
	{

		#region Private Members

		// The plugin associated with this button object
		internal static Navigator m_plugin;

		protected JHU_RootWidget m_rootWidget;

		protected bool m_setFlag = true;

		#endregion

		public NavigatorMenuButton(string buttonIconPath, Navigator plugin) : base(buttonIconPath)
		{
			m_plugin = plugin;
			m_rootWidget = JHU_Globals.getInstance().RootWidget;
			this.Description = "Navigator";
			this.SetPushed(true);
		}

		public override void Dispose()
		{
			base.Dispose ();
		}

		public override void Update(DrawArgs drawArgs)
		{
		}

		public override bool IsPushed()
		{
			return m_setFlag;
		}

		public override void SetPushed(bool isPushed)
		{
			m_setFlag = isPushed;
		}

		public override void OnKeyDown(KeyEventArgs keyEvent)
		{
			m_rootWidget.OnKeyDown(keyEvent);
		}

		public override void OnKeyUp(KeyEventArgs keyEvent)
		{
			m_rootWidget.OnKeyUp(keyEvent);
		}

		public override bool OnMouseDown(MouseEventArgs e)
		{
			if(this.IsPushed())	
				return m_rootWidget.OnMouseDown(e);
			else
				return false;
		}

		public override bool OnMouseMove(MouseEventArgs e)
		{
			if(this.IsPushed())	
				return m_rootWidget.OnMouseMove(e);
			else
				return false;
		}

		public override bool OnMouseUp(MouseEventArgs e)
		{
			if(this.IsPushed())	
				return m_rootWidget.OnMouseUp(e);
			else
				return false;
		}

		public override bool OnMouseWheel(MouseEventArgs e)
		{
			if(this.IsPushed())
				return m_rootWidget.OnMouseWheel(e);
			else
				return false;
		}


		public override void Render(DrawArgs drawArgs)
		{
			// HACK - check form state to set menu button correcly
			if (JHU_Globals.getInstance().NavigatorForm.Visible)
				m_plugin.NavMenu.Text = "Hide Navigator\tN";
			else
				m_plugin.NavMenu.Text = "Show Navigator\tN";

			// HACK - check form state to set menu button correcly
			if (JHU_Globals.getInstance().InfoForm.Visible)
				m_plugin.InfoMenu.Text = "Hide Info\tI";
			else
				m_plugin.InfoMenu.Text = "Show Info\tI";

			// Render all widgets
			m_rootWidget.Render(drawArgs);
		}
	}
}