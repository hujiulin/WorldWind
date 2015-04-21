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
using System.Diagnostics;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;
using WorldWind.Menu;
using System.Windows.Forms;

namespace jhuapl.util
{
	/// <summary>
	/// Holds a collection of icons
	/// </summary>
	public class JHU_Icons : WorldWind.Renderable.RenderableObjectList
	{
		protected Sprite m_sprite;

		protected bool m_mouseOver;

		protected bool m_needToInitChildren = true;

		protected ArrayList m_childrenToInit;

		/// <summary>
		/// The closest icon the mouse is currently over
		/// </summary>
		protected JHU_Icon mouseOverIcon;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icons"/> class 
		/// </summary>
		/// <param name="name"></param>
		public JHU_Icons(string name) : base(name) 
		{
			m_mouseOver = true;
			isInitialized = false;
			m_needToInitChildren = true;
			m_childrenToInit = new ArrayList();
		}

		/// <summary>
		/// Adds an icon to this layer. Deprecated.
		/// </summary>
		public void AddIcon(JHU_Icon icon)
		{
			this.Add(icon);
		}

		#region RenderableObject methods

		/// <summary>
		/// Add a child object to this layer.
		/// </summary>
		public override void Add(RenderableObject ro)
		{
			m_children.Add(ro);
			m_childrenToInit.Add(ro);

			// force an initialize on all the children
			m_needToInitChildren = true;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(!isInitialized)
			{
				JHU_Globals.getInstance().WorldWindow.MouseUp += new MouseEventHandler(OnMouseUp);
				m_sprite = new Sprite(drawArgs.device);

				// force the init of all children
				m_needToInitChildren = true;
				m_childrenToInit.Clear();
			}

			InitializeChildren(drawArgs);

			isInitialized = true;
		}

		protected void InitializeChildren(DrawArgs drawArgs)
		{
			if (m_needToInitChildren)
			{
				if (m_childrenToInit.Count != 0)
				{
					foreach(RenderableObject ro in m_childrenToInit)
					{
						if(ro.IsOn)
							ro.Initialize(drawArgs);
					}
					m_childrenToInit.Clear();
				}
				else
				{
					// initialize all children
					foreach(RenderableObject ro in m_children)
					{
						if(ro.IsOn)
							ro.Initialize(drawArgs);
						continue;
					}
				}
				m_needToInitChildren = false;
			}
		}

		public override void Dispose()
		{
			if (isInitialized)
			{
				m_sprite.Dispose();
				m_sprite = null;

				JHU_Globals.getInstance().WorldWindow.MouseUp -= new MouseEventHandler(OnMouseUp);

				isInitialized = false;
				m_needToInitChildren = true;
				m_childrenToInit.Clear();
			}

			// base dispose calls dispose of all children
			base.Dispose();
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			int closestIconDistanceSquared = int.MaxValue;
			JHU_Icon closestIcon = null;

			foreach(RenderableObject ro in m_children)
			{
				// If renderable object can handle the selection
				if(!ro.IsOn)
					continue;
				if(!ro.isSelectable)
					continue;

				JHU_Icon icon = ro as JHU_Icon;

				// if its not an icon just perform the action else do the selection on the closest icon
				if(icon == null)
				{
					if (ro.PerformSelectionAction(drawArgs))
						return true;
				}
				else
				{
					// don't check if we aren't even in view
					if(drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
					{
						// check if inside current icon's selection rectangle
						Vector3 projectedPoint = drawArgs.WorldCamera.Project(icon.Position);

						int dx = DrawArgs.LastMousePosition.X - (int)projectedPoint.X;
						int dy = DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y;

						if( icon.SelectionRectangle.Contains( dx, dy ) )
						{
							// Mouse is over, check whether this icon is closest
							int distanceSquared = dx*dx + dy*dy;
							if(distanceSquared < closestIconDistanceSquared)
							{
								closestIconDistanceSquared = distanceSquared;
								closestIcon = icon;
							}
						}
					}
				}
			}

			// if no other object has handled the selection let the closest icon try
			if (closestIcon != null)
				if (closestIcon.PerformSelectionAction(drawArgs))
					return true;

			return false;
		}

		public bool PerformRMBAction(MouseEventArgs e)
		{
			int closestIconDistanceSquared = int.MaxValue;
			JHU_Icon closestIcon = null;
			DrawArgs drawArgs = JHU_Globals.getInstance().WorldWindow.DrawArgs;

			foreach(RenderableObject ro in m_children)
			{
				// If renderable object can handle the selection
				if(!ro.IsOn)
					continue;
				if(!ro.isSelectable)
					continue;

				JHU_Icon icon = ro as JHU_Icon;

				// if its a JHU_Icon check to see if we're on top
				if(icon != null)
				{
					// don't check if we aren't even in view
					if(drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
					{
						// check if inside current icon's selection rectangle
						Vector3 projectedPoint = drawArgs.WorldCamera.Project(icon.Position);

						int dx = e.X - (int)projectedPoint.X;
						int dy = e.Y - (int)projectedPoint.Y;

						if( icon.SelectionRectangle.Contains( dx, dy ) )
						{
							// Mouse is over, check whether this icon is closest
							int distanceSquared = dx*dx + dy*dy;
							if(distanceSquared < closestIconDistanceSquared)
							{
								closestIconDistanceSquared = distanceSquared;
								closestIcon = icon;
							}
						}
					}
				}
			}

			// if no other object has handled the selection let the closest icon try
			if (closestIcon != null)
			{
				if (closestIcon.PerformRMBAction(e))
				{
					return true;
				}
			}

			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(!isInitialized)
				this.Initialize(drawArgs);

			if(m_needToInitChildren)
				this.InitializeChildren(drawArgs);

			int closestIconDistanceSquared = int.MaxValue;
			JHU_Icon closestIcon = null;

			// Begin sprite rendering
			try
			{
				m_sprite.Begin(SpriteFlags.AlphaBlend);

				if (m_mouseOver)
				{
					// For each ro
					foreach(RenderableObject ro in m_children)
					{
						if(!ro.IsOn)
							continue;

						// most objects should be icons
						JHU_Icon icon = ro as JHU_Icon;

						// if its not an icon call its render
						if(icon==null)
						{
							ro.Render(drawArgs);
						}
							// else its an icon - render here to batch up the sprite renders
						else
						{
							// don't bother if we aren't even in view
							if(drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
							{

                                Vector3 translationVector = new Vector3(
                                (float)(icon.Position.X - drawArgs.WorldCamera.ReferenceCenter.X),
                                (float)(icon.Position.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                                (float)(icon.Position.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

								// check if inside current icon's selection rectangle
                                Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);

								int dx = DrawArgs.LastMousePosition.X - (int)projectedPoint.X;
								int dy = DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y;

								if( icon.SelectionRectangle.Contains( dx, dy ) )
								{
									// Mouse is over, check whether this icon is closest
									int distanceSquared = dx*dx + dy*dy;
									if(distanceSquared < closestIconDistanceSquared)
									{
										closestIconDistanceSquared = distanceSquared;
										closestIcon = icon;
									}
								}

								// render the icon if it wasn't the last mouseover
								if(icon != mouseOverIcon)
									icon.FastRender(drawArgs, m_sprite, projectedPoint);
							}

							// Update here or it doesn't update when the icon is out of view
							icon.UpdateHookForm();
						}
					}

					// Render the mouse over icon last (on top)
					if(mouseOverIcon != null)
					{
						mouseOverIcon.MouseOverRender(drawArgs, m_sprite, drawArgs.WorldCamera.Project(mouseOverIcon.Position));
					}

					// set new mouseover icon
					mouseOverIcon = closestIcon;
				}
				else
				{
					// For each ro
					foreach(RenderableObject ro in m_children)
					{
						if(!ro.IsOn)
							continue;

						// most objects should be icons
						JHU_Icon icon = ro as JHU_Icon;

						// if its not an icon call its render
						if(icon==null)
						{
							ro.Render(drawArgs);
						}
							// else its an icon - render here to batch up the sprite renders
						else
						{
							// don't bother if we aren't even in view
							if(drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
							{
								icon.FastRender(drawArgs, m_sprite, drawArgs.WorldCamera.Project(icon.Position));
							}

							// Update here or it doesn't update when the icon is out of view
							icon.UpdateHookForm();
						}
					}
				}
			}
			catch(Exception ex)
			{
				System.Console.WriteLine(ex.Message.ToString());			
			}
			finally
			{
				m_sprite.End();
			}
		}

		#endregion


		/// <summary>
		/// Check for RMB events
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				this.PerformRMBAction(e);
			}

			return;
		}

		#region Context Menu Methods

		public override void BuildContextMenu(ContextMenu menu)
		{
			// initialize context menu
			MenuItem mouseoverMenuItem = new MenuItem("Disable Mouse Over", new EventHandler(IconMouseOverMenuItem_Click));

			if (!m_mouseOver)
				mouseoverMenuItem.Text = "Enable Mouse Over";

			menu.MenuItems.Add(mouseoverMenuItem);
		}

		void IconMouseOverMenuItem_Click(object sender, EventArgs s)
		{
			m_mouseOver = ! m_mouseOver;
		}

		#endregion
	}
}
