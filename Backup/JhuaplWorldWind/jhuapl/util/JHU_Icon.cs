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
// Copyright (c) 2006 The Johns Hopkins University/Applied Physics Laboratory
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
using System.Text;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;
using WorldWind.Menu;
using System.Windows.Forms;

namespace jhuapl.util
{
	/// <summary>
	/// One icon in an icon layer
	/// </summary>
	public class JHU_Icon : RenderableObject
	{
		#region private members

		/// <summary>
		/// Latitude (North/South) in decimal degrees
		/// </summary>
		protected float m_latitude = 0.0F;

		/// <summary>
		/// Longitude (East/West) in decimal degrees
		/// </summary>
		protected float m_longitude = 0.0F;

		/// <summary>
		/// The altitude of this object (ASL)
		/// </summary>	
		protected float m_altitude = 0.0F;

		/// <summary>
		/// On-Click browse to location
		/// </summary>
		protected string m_url;

		protected Hashtable m_textures;

		protected JHU_FormWidget m_hookForm = null;

		protected JHU_SimpleTreeNodeWidget m_hookTreeNode = null;

		protected JHU_SimpleTreeNodeWidget m_hookGeneralTreeNode = null;

		protected JHU_SimpleTreeNodeWidget m_hookDetailTreeNode = null;

		protected JHU_SimpleTreeNodeWidget m_hookDescTreeNode = null;

		protected JHU_LabelWidget m_hookGeneralLabel = null;

		protected JHU_LabelWidget m_hookDetailLabel = null;

		protected JHU_LabelWidget m_hookDescLabel = null;

		protected ContextMenu m_contextMenu = null;

		protected Vector3 m_groundPoint;
		protected Line m_groundStick;

		protected System.Drawing.Color m_groundStickColor = System.Drawing.Color.FromArgb(
			192,
			255,
			255,
			255);

		protected bool m_isUpdated = false;

		protected static JHU_Globals m_globals;

		#endregion

		/// <summary>
		/// Latitude (North/South) in decimal degrees
		/// </summary>
		public double Latitude
		{
			get { return m_latitude; }
			set 
			{
				m_latitude = (float)value;
				m_isUpdated = false;
			}
		}

		/// <summary>
		/// Longitude (East/West) in decimal degrees
		/// </summary>
		public double Longitude
		{
			get { return m_longitude; }
			set 
			{
				m_longitude = (float)value;
				m_isUpdated = false;
			}
		}
		
		/// <summary>
		/// The icon altitude above sea level
		/// </summary>
		public double Altitude
		{
			get { return m_altitude; }
			set 
			{ 
				m_altitude = (float)value; 
				m_isUpdated = false;
			}
		}

		/// <summary>
		/// Longer description of icon (addition to name)
		/// </summary>
        // inherited from RenderableObject.
		// public new string Description;

		/// <summary>
		/// Icon bitmap path. (Overrides Image)
		/// </summary>
		public string TextureFileName;

		public JHU_IconTexture m_iconTexture;

		protected bool m_iconTexture2Show = false;
		protected string m_iconTexture2Name;
		protected JHU_IconTexture m_iconTexture2 = null;

		protected bool m_iconTexture3Show = false;
		protected string m_iconTexture3Name;
		protected JHU_IconTexture m_iconTexture3 = null;

		/// <summary>
		/// Icon image.  Leave TextureFileName=null if using Image.  
		/// Caller is responsible for disposing the Bitmap when the layer is removed, 
		/// either by calling Dispose on Icon or on the Image directly.
		/// </summary>
		public Bitmap Image;

		/// <summary>
		/// Icon on-screen rendered width (pixels).  Defaults to icon image width.  
		/// If source image file is not a valid GDI+ image format, width may be increased to closest power of 2.
		/// </summary>
		public int Width;

		/// <summary>
		/// Icon on-screen rendered height (pixels).  Defaults to icon image height.  
		/// If source image file is not a valid GDI+ image format, height may be increased to closest power of 2.
		/// </summary>
		public int Height;

		/// <summary>
		///  Icon X scaling computed by dividing icon width by texture width
		/// </summary>
		public float XScale;
 
		/// <summary>
		///  Icon Y scaling computed by dividing icon height by texture height 
		/// </summary>
		public float YScale;

		/// <summary>
		/// On-Click browse to location
		/// </summary>
		public string URL
		{
			get { return m_url; }
			set 
			{
				isSelectable = value != null;
				m_url = value;
			}
		}

		/// <summary>
		/// The maximum distance (meters) the icon will be visible from
		/// </summary>
		public float MaximumDisplayDistance = float.MaxValue;

		/// <summary>
		/// The minimum distance (meters) the icon will be visible from
		/// </summary>
		public float MinimumDisplayDistance;

		/// <summary>
		/// Bounding box centered at (0,0) used to calculate whether mouse is over icon/label
		/// </summary>
		public Rectangle SelectionRectangle;

		static int hotColor = Color.White.ToArgb();
		static int normalColor = Color.FromArgb(200,255,255,255).ToArgb();
		static int nameColor = Color.White.ToArgb();
		static int descriptionColor = Color.White.ToArgb();

		// Render name field
		const int labelWidth = 1000; // Dummy value needed for centering the text

		protected Angle m_rotation = Angle.Zero;
		public Angle Rotation
		{
			get { return m_rotation; }
			set 
			{ 
				m_rotation = value; 
				if (value == Angle.Zero)
				{
					m_isRotated = true;
				}
				else
				{
					m_isRotated = false;
				}
			}
		}

		protected bool m_isRotated;
		public bool IsRotated
		{
			get { return m_isRotated; }
			set 
			{
				m_isRotated = value;
				if (!m_isRotated)
					m_rotation = Angle.Zero;
			}
		}

		protected bool m_drawGroundStick;
		public bool DrawGroundStick
		{
			get { return m_drawGroundStick; }
			set 
			{ 
				m_drawGroundStick = value; 
				m_isUpdated = false;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		public JHU_Icon(string name,
			double latitude, 
			double longitude) : base( name )

		{
			m_latitude = (float) latitude;
			m_longitude = (float) longitude;

			m_globals = JHU_Globals.getInstance();
			m_textures = m_globals.Textures;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		public JHU_Icon(string name,
			double latitude, 
			double longitude,
			double heightAboveSurface) : this( name, latitude, longitude )
		{
			m_altitude = (float) heightAboveSurface;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		public JHU_Icon(string name, 
			string description,
			double latitude, 
			double longitude, 
			double heightAboveSurface, 
			Bitmap image,
			int width,
			int height,
			string actionURL) : this( name, latitude, longitude, heightAboveSurface )
		{
			this.Description = description;

			this.Image = image;
			this.Width = width;
			this.Height = height;
			m_url = actionURL;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="latitude"></param>
		/// <param name="longitude"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="parentWorld"></param>
		/// <param name="TextureFileName"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="actionURL"></param>
		public JHU_Icon(string name, 
			string description,
			double latitude, 
			double longitude, 
			double heightAboveSurface,
			string TextureFileName,
			int width,
			int height,
			string actionURL) : this( name, latitude, longitude, heightAboveSurface )
		{
			this.Description = description;

			this.TextureFileName = TextureFileName;
			this.Width = width;
			this.Height = height;
			m_url = actionURL;
		}

		/// <summary>
		/// Sets the geographic position of the this.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		public void SetPosition(double latitude, double longitude)
		{
			m_latitude = (float) latitude;
			m_longitude = (float) longitude;

			// Recalculate XYZ coordinates
			m_isUpdated = false;
		}

		/// <summary>
		/// Sets the geographic position of the this.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="altitude">The icon altitude above sea level.</param>
		public void SetPosition(double latitude, double longitude, double altitude)
		{
			m_latitude = (float) latitude;
			m_longitude = (float) longitude;
			this.Altitude = (float) altitude;

			// Recalculate XYZ coordinates
			m_isUpdated = false;
		}

		protected void UpdatePosition(DrawArgs drawArgs)
		{
			// compute the ground point
			double elevation = drawArgs.WorldCamera.WorldRadius;

			if((m_globals.WorldWindow.CurrentWorld.TerrainAccessor != null) && (drawArgs.WorldCamera.Altitude < 300000))
			{
				float distanceToIcon = Vector3.Length(this.Position - drawArgs.WorldCamera.Position);

				// if we're close get high detail otherwise get the general elevation 
				if (distanceToIcon < 300000)
				{
					elevation += m_globals.WorldWindow.CurrentWorld.TerrainAccessor.GetElevationAt(
						Latitude, Longitude, 100.0) * World.Settings.VerticalExaggeration;
				}
				else
				{
					elevation += m_globals.WorldWindow.CurrentWorld.TerrainAccessor.GetElevationAt(
						Latitude, Longitude) * World.Settings.VerticalExaggeration;
				}
			}

			Position = MathEngine.SphericalToCartesian(Latitude, Longitude, 
				Altitude + elevation);

			if (m_drawGroundStick)
				m_groundPoint = MathEngine.SphericalToCartesian(Latitude, Longitude, elevation);

			m_isUpdated = true;
		}

		#region RenderableObject methods

		public override void Initialize(DrawArgs drawArgs)
		{
			UpdatePosition(drawArgs);

			// get the icon texture
			object key = null;
			m_iconTexture = null;
			if(TextureFileName.Trim() != String.Empty)
			{
				// Icon image from file
				m_iconTexture = (JHU_IconTexture)m_textures[TextureFileName];
				if(m_iconTexture==null)
				{
					key = TextureFileName;
					m_iconTexture = new JHU_IconTexture( drawArgs.device, TextureFileName );
				}

				// if secondary icon enabled
				if (m_iconTexture2Show && m_iconTexture2Name.Trim() != String.Empty)
				{
					m_iconTexture2 = (JHU_IconTexture)m_textures[m_iconTexture2Name];
					if (m_iconTexture2 == null)
					{
						m_iconTexture2 = new JHU_IconTexture( drawArgs.device, m_iconTexture2Name );
						m_textures.Add(m_iconTexture2Name, m_iconTexture2);
					}

					m_iconTexture2.ReferenceCount++;
				}

				// if teritary icon enabled
				if (m_iconTexture3Show && m_iconTexture3Name.Trim() != String.Empty)
				{
					m_iconTexture3 = (JHU_IconTexture)m_textures[m_iconTexture3Name];
					if (m_iconTexture3 == null)
					{
						m_iconTexture3 = new JHU_IconTexture( drawArgs.device, m_iconTexture3Name );
						m_textures.Add(m_iconTexture3Name, m_iconTexture3);
					}

					m_iconTexture3.ReferenceCount++;
				}
			}
			else
			{
				// Icon image from bitmap
				if(this.Image != null)
				{
					m_iconTexture = (JHU_IconTexture)m_textures[this.Image];
					if(m_iconTexture==null)
					{
						// Create new texture from image
						key = this.Image;
						m_iconTexture = new JHU_IconTexture( drawArgs.device, this.Image);
					}

				}
			}

			if(m_iconTexture!=null)
			{
				m_iconTexture.ReferenceCount++;

				if(key!=null)
				{
					// New texture, cache it
					m_textures.Add(key, m_iconTexture);
				}

				// Use default dimensions if not set
				if(this.Width==0)
					this.Width = m_iconTexture.Width;
				if(this.Height==0)
					this.Height = m_iconTexture.Height;
			}

			// Compute selection rectangle
			if(m_iconTexture == null)
			{
				// Label only 
				this.SelectionRectangle = drawArgs.defaultDrawingFont.MeasureString(null, this.Name, DrawTextFormat.None, 0);
			}
			else
			{
				// Icon only
				this.SelectionRectangle = new Rectangle( 0, 0, this.Width, this.Height );
			}

			// Center the box at (0,0)
			this.SelectionRectangle.Offset(-this.SelectionRectangle.Width/2, -this.SelectionRectangle.Height/2 );

			if (m_iconTexture != null)
			{
				this.XScale = (float)this.Width / m_iconTexture.Width;
				this.YScale = (float)this.Height / m_iconTexture.Height;

				// TODO - note: right now we assume any secondary icons scale the same as the primary
			}
			else
			{
				this.XScale = 1.0f;
				this.YScale = 1.0f;
			}

			if (m_groundStick == null)
				m_groundStick = new Line(drawArgs.device);

			isInitialized = true;
		}

		/// <summary>
		/// Disposes the icon (when disabled)
		/// </summary>
		public override void Dispose()
		{
			if (m_contextMenu != null)
			{
				m_contextMenu.Dispose();
				m_contextMenu = null;
			}

			if (m_hookForm != null)
			{
				m_hookForm.Dispose();
				m_hookForm = null;
			}

			m_iconTexture.ReferenceCount--;

			this.isInitialized = false;
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			if(drawArgs.WorldCamera.ViewFrustum.ContainsPoint(this.Position))
			{
				Vector3 projectedPoint = drawArgs.WorldCamera.Project(this.Position);

				if(this.SelectionRectangle.Contains(
					DrawArgs.LastMousePosition.X - (int)projectedPoint.X, 
					DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y ) )
				{
					try
					{
						if ((m_url != null) && (m_url.Length > 0))
						{
							Process.Start(m_url);
							return true;
						}
					}
					catch
					{
					}
				}
			}

			return false;
		}

		public bool PerformRMBAction(MouseEventArgs e)
		{
			if (m_contextMenu == null)
			{
				m_contextMenu = new ContextMenu();
				this.BuildContextMenu(m_contextMenu);
			}

			m_contextMenu.Show(m_globals.WorldWindow, new System.Drawing.Point(e.X, e.Y));

			return true;
		}

		public override void Update(DrawArgs drawArgs)
		{
			// Handled by parent
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if (!this.isInitialized)
				this.Initialize(drawArgs);

			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(this.Position))
				return;

			if ((!this.m_isUpdated) || (drawArgs.WorldCamera.Altitude < 300000))
			{
				this.UpdatePosition(drawArgs);
			}

			// check if inside current icon's selection rectangle
 
            Vector3 translationVector = new Vector3(
                (float)(this.Position.X - drawArgs.WorldCamera.ReferenceCenter.X),
                (float)(this.Position.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                (float)(this.Position.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

			Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);

			// Check icons for within "visual" range
			float distanceToIcon = Vector3.Length(this.Position - drawArgs.WorldCamera.Position);
			if(distanceToIcon > this.MaximumDisplayDistance)
				return;
			if(distanceToIcon < this.MinimumDisplayDistance)
				return;
			
			// Can't tell if mouseover so just call the fast render
			FastRender(drawArgs, null, projectedPoint);

			UpdateHookForm();
		}

		/// <summary>
		/// Draw the icon
		/// </summary>
		public void FastRender(DrawArgs drawArgs, Sprite sprite, Vector3 projectedPoint)
		{
			// Check icons for within "visual" range
			float distanceToIcon = Vector3.Length(this.Position - drawArgs.WorldCamera.Position);
			if(distanceToIcon > this.MaximumDisplayDistance)
				return;
			if(distanceToIcon < this.MinimumDisplayDistance)
				return;

			if (!this.isInitialized)
				this.Initialize(drawArgs);

			if ((!this.m_isUpdated) || (drawArgs.WorldCamera.Altitude < 300000))
			{
				this.UpdatePosition(drawArgs);
			}

			JHU_IconTexture iconTexture = this.GetTexture();

			if(iconTexture==null)
			{
				// Render label
				if(this.Name != null)
				{
					// Center over target as we have no bitmap
					Rectangle rect = new Rectangle(
						(int)projectedPoint.X - (labelWidth>>1), 
						(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
						labelWidth, 
						drawArgs.screenHeight );

					drawArgs.defaultDrawingFont.DrawText(sprite, this.Name, rect, DrawTextFormat.Center, normalColor);
				}
			}
			else
			{
				// Render icon
				sprite.Transform = Matrix.Scaling(this.XScale,this.YScale,0);
				
				if (m_isRotated)
					sprite.Transform *= Matrix.RotationZ((float)m_rotation.Radians - (float) drawArgs.WorldCamera.Heading.Radians);

				sprite.Transform *= Matrix.Translation(projectedPoint.X, projectedPoint.Y, 0);
				
				sprite.Draw( iconTexture.Texture,
					new Vector3(iconTexture.Width>>1, iconTexture.Height>>1,0),
					Vector3.Empty,
					normalColor );

				if (m_iconTexture2Show)
				{
					sprite.Draw ( m_iconTexture2.Texture,
						new Vector3(m_iconTexture2.Width>>1, m_iconTexture2.Height>>1, 0),
						Vector3.Empty,
						normalColor );
				}

				if (m_iconTexture3Show)
				{
					sprite.Draw ( m_iconTexture3.Texture,
						new Vector3(m_iconTexture3.Width>>1, m_iconTexture3.Height>>1, 0),
						Vector3.Empty,
						normalColor );
				}
				
				// Reset transform to prepare for text rendering later
				sprite.Transform = Matrix.Identity;
			}

			if (m_drawGroundStick)
			{
				Vector2[] groundStick = new Vector2[2];
				Angle testAngle = new Angle();
				testAngle.Degrees = 45.0;

				Vector3 projectedGroundPoint = drawArgs.WorldCamera.Project(m_groundPoint);

				m_groundStick.Begin();
				groundStick[0].X = projectedPoint.X;
				groundStick[0].Y = projectedPoint.Y;
				groundStick[1].X = projectedGroundPoint.X;
				groundStick[1].Y = projectedGroundPoint.Y;

				m_groundStick.Draw(groundStick, m_groundStickColor);
				m_groundStick.End();
			}
		}		

		/// <summary>
		/// Draw the icon
		/// </summary>
		public void MouseOverRender(DrawArgs drawArgs, Sprite sprite, Vector3 projectedPoint)
		{
			JHU_IconTexture iconTexture = this.GetTexture();

			if(this.isSelectable)
				DrawArgs.MouseCursor = CursorType.Hand;

			if (!this.isInitialized)
				this.Initialize(drawArgs);

			if ((!this.m_isUpdated) || (drawArgs.WorldCamera.Altitude < 300000))
			{
				this.UpdatePosition(drawArgs);
			}

			// set description to icon descrption
			m_globals.GeneralInfoLabel.Text = GeneralInfo();
			m_globals.DetailedInfoLabel.Text = DetailedInfo();
			m_globals.DescriptionLabel.Text = DescriptionInfo();


			if(iconTexture==null)
			{
				// Render label
				if(this.Name != null)
				{
					// Center over target as we have no bitmap
					Rectangle rect = new Rectangle(
						(int)projectedPoint.X - (labelWidth>>1), 
						(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
						labelWidth, 
						drawArgs.screenHeight );

					drawArgs.defaultDrawingFont.DrawText(sprite, this.Name, rect, DrawTextFormat.Center, hotColor);
				}
			}
			else			
			{
				// Render label
				if(this.Name != null)
				{
					// Adjust text to make room for icon
					int spacing = (int)(this.Width * 0.3f);
					if(spacing>10)
						spacing = 10;
					int offsetForIcon = (this.Width>>1) + spacing;

					Rectangle rect = new Rectangle(
						(int)projectedPoint.X + offsetForIcon, 
						(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
						labelWidth, 
						drawArgs.screenHeight );

					drawArgs.defaultDrawingFont.DrawText(sprite, this.Name, rect, DrawTextFormat.WordBreak, hotColor);
				}

				// Render icon
				sprite.Transform = Matrix.Scaling(this.XScale,this.YScale,0);
				
				if (m_isRotated)
					sprite.Transform *= Matrix.RotationZ((float)m_rotation.Radians - (float) drawArgs.WorldCamera.Heading.Radians);

				sprite.Transform *= Matrix.Translation(projectedPoint.X, projectedPoint.Y, 0);
				sprite.Draw( iconTexture.Texture,
					new Vector3(iconTexture.Width>>1, iconTexture.Height>>1,0),
					Vector3.Empty,
					hotColor );

				if (m_iconTexture2Show)
				{
					sprite.Draw ( m_iconTexture2.Texture,
						new Vector3(m_iconTexture2.Width>>1, m_iconTexture2.Height>>1, 0),
						Vector3.Empty,
						hotColor );
				}

				if (m_iconTexture3Show)
				{
					sprite.Draw ( m_iconTexture3.Texture,
						new Vector3(m_iconTexture3.Width>>1, m_iconTexture3.Height>>1, 0),
						Vector3.Empty,
						hotColor );
				}
				
				// Reset transform to prepare for text rendering later
				sprite.Transform = Matrix.Identity;
			}
			if (m_drawGroundStick)
			{
				Vector2[] groundStick = new Vector2[2];

				Vector3 projectedGroundPoint = drawArgs.WorldCamera.Project(m_groundPoint);

				m_groundStick.Begin();
				groundStick[0].X = projectedPoint.X;
				groundStick[0].Y = projectedPoint.Y;
				groundStick[1].X = projectedGroundPoint.X;
				groundStick[1].Y = projectedGroundPoint.Y;
				m_groundStick.Draw(groundStick, m_groundStickColor);
				m_groundStick.End();
			}
		}


		#endregion

		public virtual string GeneralInfo()
		{
			StringBuilder outString = new StringBuilder();

			outString.AppendFormat("{0:-10} {1}\n","Name:", Name);
			outString.AppendFormat("{0:-10} {1:00.00000}\n","Lat:", Latitude);
			outString.AppendFormat("{0:-10} {1:000.00000}\n","Lon:", Longitude);
			outString.AppendFormat("{0:-10} {1:F0}\n","Alt:", this.Altitude);

			return outString.ToString();

		}

		public virtual string DetailedInfo()
		{
			StringBuilder outString = new StringBuilder();

			outString.AppendFormat("{0:-10} {1}\n","URL:", m_url);

			return outString.ToString();
		}

		public virtual string DescriptionInfo()
		{
			return this.Description;
		}

		public JHU_IconTexture GetTexture()
		{
			return m_iconTexture;
		}

		public void UpdateHookForm()
		{
			// update hook form
			if (m_hookForm != null)
			{ 
				if (m_hookForm.Enabled)
				{
					m_hookForm.Text = this.DMS();
					m_hookGeneralLabel.Text = this.GeneralInfo();
					m_hookDetailLabel.Text = this.DetailedInfo();
					m_hookDescLabel.Text = this.DescriptionInfo();
				}
				else
				{
					m_hookForm.Dispose();

					m_hookTreeNode = null;
					m_hookGeneralTreeNode = null;
					m_hookDetailTreeNode = null;
					m_hookDescTreeNode = null;
					m_hookGeneralLabel = null;
					m_hookDetailLabel = null;
					m_hookDescLabel = null;

					m_hookForm = null;
				}	
			}
		}

		public string Degrees()
		{
			StringBuilder retStr = new StringBuilder();

			retStr.AppendFormat("Lat: {0:00.00000}", this.Latitude); //, (this.Latitude>=0) ? "N":"S" );
			retStr.AppendFormat(" Lon: {0:000.00000}", this.Longitude); //, (this.Longitude>=0)? "E":"W");
			retStr.AppendFormat(" Alt: {0:F0}", this.Altitude);

			return retStr.ToString();
		}

		public string DMS()
		{
			StringBuilder retStr = new StringBuilder();

			retStr.AppendFormat("Lat: {0}", JHU_Utilities.Degrees2DMS(this.Latitude, 'N', 'S'));
			retStr.AppendFormat(" Lon: {0}", JHU_Utilities.Degrees2DMS(this.Longitude, 'E', 'W'));
			retStr.AppendFormat(" Alt: {0:F0}", this.Altitude);

			return retStr.ToString();
		}

		public void GoTo()
		{
			// goto lat lon of this icon
			m_globals.WorldWindow.GotoLatLon(m_latitude, m_longitude);
		}

		#region Context Menu Methods

		public override void BuildContextMenu(ContextMenu menu)
		{
			// initialize context menu
			MenuItem gotoMenuItem = new MenuItem("Goto Location", new EventHandler(IconGotoMenuItem_Click));

			MenuItem hookMenuItem = new MenuItem("Hook " + name, new EventHandler(IconHookMenuItem_Click));

			MenuItem urlMenuItem = new MenuItem ("Open URL", new EventHandler (IconURLMenuItem_Click));

//			MenuItem adocsMenuItem = new MenuItem ("Open in ADOCS", new EventHandler (IconADOCSMenuItem_Click));

			if ((m_url == null) || (m_url.Length <= 0))
			{
				urlMenuItem.Enabled = false;
			}
			
			menu.MenuItems.Add(gotoMenuItem);
			menu.MenuItems.Add(hookMenuItem);
			menu.MenuItems.Add(urlMenuItem);
			// menu.MenuItems.Add(adocsMenuItem);
		}

		/// <summary>
		/// Adds a new context menu item to this icon.
		/// </summary>
		/// <param name="newItem">The menu item to add</param>
		public void AddContextMenuItem(MenuItem newItem)
		{
			if (m_contextMenu == null)
			{
				m_contextMenu = new ContextMenu();
				this.BuildContextMenu(m_contextMenu);
			}

			m_contextMenu.MenuItems.Add(newItem);
		}


		void IconGotoMenuItem_Click(object sender, EventArgs s)
		{
			JHU_Log.Write(1, "NAV", this.Latitude, this.Longitude, this.Altitude, this.Name, "Icon Goto called for icon " + this.Name);
			this.GoTo();
		}

		void IconHookMenuItem_Click(object sender, EventArgs s)
		{
			JHU_Log.Write(1, "ICON", this.Latitude, this.Longitude, this.Altitude, this.Name, "Icon Hook called for icon " + this.Name);
			if (m_hookForm == null)
			{
				m_hookForm = new jhuapl.util.JHU_FormWidget(" " + this.Name);

				m_hookForm.WidgetSize = new System.Drawing.Size(200, 250);
				m_hookForm.Location = new System.Drawing.Point(200,120);
				m_hookForm.DestroyOnClose = true;

				m_hookTreeNode = new JHU_SimpleTreeNodeWidget("Info");
				m_hookTreeNode.IsRadioButton = true;
				m_hookTreeNode.Expanded = true;
				m_hookTreeNode.EnableCheck = false;

				m_hookGeneralLabel = new JHU_LabelWidget("");
				m_hookGeneralLabel.ClearOnRender = true;
				m_hookGeneralLabel.Format = DrawTextFormat.WordBreak ;
				m_hookGeneralLabel.Location = new System.Drawing.Point(0, 0);
				m_hookGeneralLabel.AutoSize = true;
				m_hookGeneralLabel.UseParentWidth = false;

				m_hookGeneralTreeNode = new JHU_SimpleTreeNodeWidget("General");
				m_hookGeneralTreeNode.IsRadioButton = true;
				m_hookGeneralTreeNode.Expanded = true;
				m_hookGeneralTreeNode.EnableCheck = false;

				m_hookGeneralTreeNode.Add(m_hookGeneralLabel);
				m_hookTreeNode.Add(m_hookGeneralTreeNode);		

				m_hookDetailLabel = new JHU_LabelWidget("");
				m_hookDetailLabel.ClearOnRender = true;
				m_hookDetailLabel.Format = DrawTextFormat.WordBreak ;
				m_hookDetailLabel.Location = new System.Drawing.Point(0, 0);
				m_hookDetailLabel.AutoSize = true;
				m_hookDetailLabel.UseParentWidth = false;

				m_hookDetailTreeNode = new JHU_SimpleTreeNodeWidget("Detail");
				m_hookDetailTreeNode.IsRadioButton = true;
				m_hookDetailTreeNode.Expanded = true;
				m_hookDetailTreeNode.EnableCheck = false;

				m_hookDetailTreeNode.Add(m_hookDetailLabel);
				m_hookTreeNode.Add(m_hookDetailTreeNode);					

				m_hookDescTreeNode = new JHU_SimpleTreeNodeWidget("Description");
				m_hookDescTreeNode.IsRadioButton = true;
				m_hookDescTreeNode.Expanded = false;
				m_hookDescTreeNode.EnableCheck = false;

				m_hookDescLabel = new JHU_LabelWidget("");
				m_hookDescLabel.ClearOnRender = true;
				m_hookDescLabel.Format = DrawTextFormat.WordBreak ;
				m_hookDescLabel.Location = new System.Drawing.Point(0, 0);
				m_hookDescLabel.AutoSize = true;
				m_hookDescLabel.UseParentWidth = true;

				m_hookDescTreeNode.Add(m_hookDescLabel);
				m_hookTreeNode.Add(m_hookDescTreeNode);

				m_hookForm.Add(m_hookTreeNode);

				m_globals.RootWidget.Add(m_hookForm);
			}

			UpdateHookForm();
			m_hookForm.Enabled = true;
			m_hookForm.Visible = true;

		}

		void IconURLMenuItem_Click(object sender, EventArgs s)
		{
			try
			{
				JHU_Log.Write(1, "ICON", this.Latitude, this.Longitude, this.Altitude,  this.Name, "Icon URL called for icon " + this.Name +". URL = " + m_url);

				if ((m_url != null) && (m_url.Length > 0))
				{
					Process.Start(m_url);
				}
			}
			catch
			{
			}
		}

		void IconADOCSMenuItem_Click(object sender, EventArgs s)
		{
			try
			{
				if ((m_url != null) && (m_url.Length > 0))
				{
					Process.Start("C:\\BIN\\adocs.exe");
				}
			}
			catch
			{
			}
		}

		#endregion
	}
}