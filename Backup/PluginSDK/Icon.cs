using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Contains one texture for our icon texture cache
	/// </summary>
	public class IconTexture : IDisposable
	{
		public Texture Texture;
		public int Width;
		public int Height;
        public int ReferenceCount;

        /// <summary>
        /// Base Save path for any downloaded images.  Set to CachePath\IconTextures by default.
        /// TODO: Need to get the real CachePath rather than faking it.
        /// TODO: Should make this directory settable.
        /// TODO: Should have some mechanism to clear the cache out.
        /// </summary>
        public static string BaseSavePath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Cache\IconTextures";

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.IconTexture"/> class 
		/// from a texture file on disk.
		/// </summary>
		public IconTexture(Device device, string textureFileName)
		{
            if ((textureFileName != null) && textureFileName.Length > 0)
            {
                if (textureFileName.ToLower().StartsWith("http://") && BaseSavePath != null)
                {
                    // download it
                    try
                    {
                        Uri uri = new Uri(textureFileName);

                        // Set the subdirectory path to the hostname and replace . with _
                        string savePath = uri.Host;
                        savePath = savePath.Replace('.', '_');

                        // build the save file name from the component pieces
                        savePath = BaseSavePath + @"\" + savePath + uri.AbsolutePath;
                        savePath = savePath.Replace('/', '\\');

                        WorldWind.Net.WebDownload webDownload = new WorldWind.Net.WebDownload(textureFileName);
                        webDownload.DownloadType = WorldWind.Net.DownloadType.Unspecified;
                        webDownload.DownloadFile(savePath);

                        // reset the texture file name for later use.
                        textureFileName = savePath;
                    }
                    catch { }
                }
            }

            if(ImageHelper.IsGdiSupportedImageFormat(textureFileName))
			{
				// Load without rescaling source bitmap
				using(Image image = ImageHelper.LoadImage(textureFileName))
					LoadImage(device, image);
			}
			else
			{
				// Only DirectX can read this file, might get upscaled depending on input dimensions.
				Texture = ImageHelper.LoadIconTexture( textureFileName );
				// Read texture level 0 size
				using(Surface s = Texture.GetSurfaceLevel(0))
				{
					SurfaceDescription desc = s.Description;
					Width = desc.Width;
					Height = desc.Height;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.IconTexture"/> class 
		/// from a bitmap.
		/// </summary>
		public IconTexture(Device device, Bitmap image)
		{
			LoadImage(device, image);
		}

		protected void LoadImage(Device device, Image image)
		{
			Width = (int)Math.Round(Math.Pow(2, (int)(Math.Ceiling(Math.Log(image.Width)/Math.Log(2)))));
			if(Width>device.DeviceCaps.MaxTextureWidth)
				Width = device.DeviceCaps.MaxTextureWidth;

			Height = (int)Math.Round(Math.Pow(2, (int)(Math.Ceiling(Math.Log(image.Height)/Math.Log(2)))));
			if(Height>device.DeviceCaps.MaxTextureHeight)
				Height = device.DeviceCaps.MaxTextureHeight;

			using(Bitmap textureSource = new Bitmap(Width, Height))
			using(Graphics g = Graphics.FromImage(textureSource))
			{
				g.DrawImage(image, 0,0,Width,Height);
				if(Texture!=null)
					Texture.Dispose();
				Texture = new Texture(device, textureSource, Usage.None, Pool.Managed);
			}
		}


		#region IDisposable Members

		public void Dispose()
		{
			if(Texture!=null)
			{
				Texture.Dispose();
				Texture = null;
			}
			
			GC.SuppressFinalize(this);
		}

		#endregion
	}
	
	/// <summary>
	/// Holds a collection of icons
	/// </summary>
	public class Icons : RenderableObjectList
	{
		/// <summary>
		/// Texture cache
		/// </summary>
		protected Hashtable m_textures = new Hashtable();
		
		protected Sprite m_sprite;

		static int hotColor = Color.White.ToArgb();
		static int normalColor = Color.FromArgb(150,255,255,255).ToArgb();
		static int nameColor = Color.White.ToArgb();
		static int descriptionColor = Color.White.ToArgb();

		System.Timers.Timer refreshTimer;

		/// <summary>
		/// The closest icon the mouse is currently over
		/// </summary>
		protected Icon mouseOverIcon;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icons"/> class 
		/// </summary>
		/// <param name="name"></param>
		public Icons(string name) : base(name) 
		{
		}

		public Icons(string name, 
			string dataSource, 
			TimeSpan refreshInterval,
			World parentWorld,
			Cache cache) : base(name, dataSource, refreshInterval, parentWorld, cache) 
		{
		}

		/// <summary>
		/// Adds an icon to this layer. Deprecated.
		/// </summary>
		public void AddIcon(Icon icon)
		{
			Add(icon);
		}

		#region RenderableObject methods

		/// <summary>
		/// Add a child object to this layer.
		/// </summary>
		public override void Add(RenderableObject ro)
		{
			m_children.Add(ro);
			isInitialized = false;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(m_sprite != null)
			{
				m_sprite.Dispose();
				m_sprite = null;
			}

			m_sprite = new Sprite(drawArgs.device);

			System.TimeSpan smallestRefreshInterval = System.TimeSpan.MaxValue;

			// Load all textures
			foreach(RenderableObject ro in m_children)
			{
				Icon icon = ro as Icon;
				if(icon==null)
				{
					// Child is not an icon
					if(ro.IsOn)
						ro.Initialize(drawArgs);
					continue;
				}

				if(icon.RefreshInterval.TotalMilliseconds != 0 && icon.RefreshInterval != TimeSpan.MaxValue && icon.RefreshInterval < smallestRefreshInterval)
					smallestRefreshInterval = icon.RefreshInterval;

				// Child is an icon
				icon.Initialize(drawArgs);

				object key = null;
				IconTexture iconTexture = null;

				if(icon.TextureFileName != null && icon.TextureFileName.Length > 0)
				{
					if(icon.TextureFileName.ToLower().StartsWith("http://") && icon.SaveFilePath != null)
					{
						//download it
						try
						{
							WorldWind.Net.WebDownload webDownload = new WorldWind.Net.WebDownload(icon.TextureFileName);
							webDownload.DownloadType = WorldWind.Net.DownloadType.Unspecified;
	
							System.IO.FileInfo saveFile = new System.IO.FileInfo(icon.SaveFilePath);
							if(!saveFile.Directory.Exists)
								saveFile.Directory.Create();

							webDownload.DownloadFile(saveFile.FullName);
						}
						catch{}
						
						iconTexture = (IconTexture)m_textures[icon.SaveFilePath];
						if(iconTexture==null)
						{
							key = icon.SaveFilePath;
							iconTexture = new IconTexture( drawArgs.device, icon.SaveFilePath );
						}
					}
					else
					{
						// Icon image from file
						iconTexture = (IconTexture)m_textures[icon.TextureFileName];
						if(iconTexture==null)
						{
							key = icon.TextureFileName;
							iconTexture = new IconTexture( drawArgs.device, icon.TextureFileName );
						}
					}
				}
				else
				{
					// Icon image from bitmap
					if(icon.Image != null)
					{
						iconTexture = (IconTexture)m_textures[icon.Image];
						if(iconTexture==null)
						{
							// Create new texture from image
							key = icon.Image;
							iconTexture = new IconTexture( drawArgs.device, icon.Image);
						}
					}
				}

				if(iconTexture==null)
					// No texture set
					continue;

				if(key!=null)
				{
					// New texture, cache it
					m_textures.Add(key, iconTexture);

					// Use default dimensions if not set
					if(icon.Width==0)
						icon.Width = iconTexture.Width;
					if(icon.Height==0)
						icon.Height = iconTexture.Height;
				}
			}

			// Compute mouse over bounding boxes
			foreach(RenderableObject ro in m_children)
			{
				Icon icon = ro as Icon;
				if(icon==null)
					// Child is not an icon
					continue;

				if(GetTexture(icon)==null)
				{
					// Label only 
					icon.SelectionRectangle = drawArgs.defaultDrawingFont.MeasureString(null, icon.Name, DrawTextFormat.None, 0);
				}
				else
				{
					// Icon only
					icon.SelectionRectangle = new Rectangle( 0,0,icon.Width, icon.Height );
				}

				// Center the box at (0,0)
				icon.SelectionRectangle.Offset(-icon.SelectionRectangle.Width/2, -icon.SelectionRectangle.Height/2 );
			}

			if(refreshTimer == null && smallestRefreshInterval != TimeSpan.MaxValue)
			{
				refreshTimer = new System.Timers.Timer(smallestRefreshInterval.TotalMilliseconds);
				refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(refreshTimer_Elapsed);
				refreshTimer.Start();
			}

			isInitialized = true;
		}

		public override void Dispose()
		{
			base.Dispose();

			if(m_textures != null)
			{
				foreach(IconTexture iconTexture in m_textures.Values)
					iconTexture.Texture.Dispose();
				m_textures.Clear();
			}

			if(m_sprite != null)
			{
				m_sprite.Dispose();
				m_sprite = null;
			}

			if(refreshTimer != null)
			{
				refreshTimer.Stop();
				refreshTimer.Dispose();
				refreshTimer = null;
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            foreach(RenderableObject ro in m_children)
			{
				if(!ro.IsOn)
					continue;
				if(!ro.isSelectable)
					continue;

				Icon icon = ro as Icon;
				if(icon==null)
				{
					// Child is not an icon
					if (ro.PerformSelectionAction(drawArgs))
						return true;
					continue;
				}
                
                if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
					continue;

                Vector3 referenceCenter = new Vector3(
                    (float)drawArgs.WorldCamera.ReferenceCenter.X,
                    (float)drawArgs.WorldCamera.ReferenceCenter.Y,
                    (float)drawArgs.WorldCamera.ReferenceCenter.Z);

				Vector3 projectedPoint = drawArgs.WorldCamera.Project(icon.Position - referenceCenter);
				if(!icon.SelectionRectangle.Contains(
					DrawArgs.LastMousePosition.X - (int)projectedPoint.X, 
					DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y ) )
					continue;

                try
				{
					if(DrawArgs.IsLeftMouseButtonDown && !DrawArgs.IsRightMouseButtonDown)
					{
						if(icon.OnClickZoomAltitude != double.NaN || icon.OnClickZoomHeading != double.NaN || icon.OnClickZoomTilt != double.NaN)
						{
							drawArgs.WorldCamera.SetPosition(
								icon.Latitude,
								icon.Longitude,
								icon.OnClickZoomHeading,
								icon.OnClickZoomAltitude, 
								icon.OnClickZoomTilt);
						}


						if (!icon.ClickableActionURL.Contains(@"worldwind://"))
						{
							if (World.Settings.UseInternalBrowser)
							{
								SplitContainer sc = (SplitContainer)drawArgs.parentControl.Parent.Parent;
								InternalWebBrowserPanel browser = (InternalWebBrowserPanel)sc.Panel1.Controls[0];
								browser.NavigateTo(icon.ClickableActionURL);
							}
							else
							{
								ProcessStartInfo psi = new ProcessStartInfo();
								psi.FileName = icon.ClickableActionURL;
								psi.Verb = "open";
								psi.UseShellExecute = true;

								psi.CreateNoWindow = true;
								Process.Start(psi);
							}
						}
					}
					else if(!DrawArgs.IsLeftMouseButtonDown && DrawArgs.IsRightMouseButtonDown)
					{
						ScreenOverlay[] overlays = icon.Overlays;
						if(overlays != null && overlays.Length > 0)
						{
							System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
							foreach(ScreenOverlay curOverlay in overlays)
							{
								contextMenu.MenuItems.Add(curOverlay.Name, new System.EventHandler(icon.OverlayOnOpen));
							}
							contextMenu.Show(DrawArgs.ParentControl, DrawArgs.LastMousePosition);
						}
					}
					return true;
				}
				catch
				{
				}
			}
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(!isInitialized)
				return;

			// First render everything except icons
			foreach(RenderableObject ro in m_children)
			{
			//	if(ro is Icon)
			//		continue;

				if(!ro.IsOn)
					continue;

				// Child is not an icon
				ro.Render(drawArgs);
			}

			int closestIconDistanceSquared = int.MaxValue;
			Icon closestIcon = null;

			// Now render just the icons
			m_sprite.Begin(SpriteFlags.AlphaBlend);
			foreach(RenderableObject ro in m_children)
			{
				if(!ro.IsOn)
					continue;

				Icon icon = ro as Icon;
				if(icon==null)
					continue;

				Vector3 translationVector = new Vector3(
				(float)(icon.PositionD.X - drawArgs.WorldCamera.ReferenceCenter.X),
				(float)(icon.PositionD.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
				(float)(icon.PositionD.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

				// Find closest mouse-over icon
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

				if(icon != mouseOverIcon)
					Render(drawArgs, icon, projectedPoint);
			}

			// Render the mouse over icon last (on top)
			if(mouseOverIcon != null)
			{
				Vector3 translationVector = new Vector3(
					(float)(mouseOverIcon.PositionD.X - drawArgs.WorldCamera.ReferenceCenter.X),
					(float)(mouseOverIcon.PositionD.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
					(float)(mouseOverIcon.PositionD.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

				Render(drawArgs, mouseOverIcon, drawArgs.WorldCamera.Project(translationVector));
			}

			mouseOverIcon = closestIcon;

			m_sprite.End();
		}

		#endregion

		/// <summary>
		/// Draw the icon
		/// </summary>
		protected virtual void Render(DrawArgs drawArgs, Icon icon, Vector3 projectedPoint)
		{
			if (!icon.isInitialized)
				icon.Initialize(drawArgs);

			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
				return;

			// Check icons for within "visual" range
			double distanceToIcon = Vector3.Length(icon.Position - drawArgs.WorldCamera.Position);
			if(distanceToIcon > icon.MaximumDisplayDistance)
				return;
			if(distanceToIcon < icon.MinimumDisplayDistance)
				return;

			IconTexture iconTexture = GetTexture(icon);
			bool isMouseOver = icon == mouseOverIcon;
			if(isMouseOver)
			{
				// Mouse is over
				isMouseOver = true;

				if(icon.isSelectable)
					DrawArgs.MouseCursor = CursorType.Hand;

				string description = icon.Description;
				if(description==null)
					description = icon.ClickableActionURL;
				if(description!=null)
				{
					// Render description field
					DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak | DrawTextFormat.Bottom;
					int left = 10;
					if(World.Settings.showLayerManager)
						left += World.Settings.layerManagerWidth;
					Rectangle rect = Rectangle.FromLTRB(left, 10, drawArgs.screenWidth - 10, drawArgs.screenHeight - 10 );

					// Draw outline
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );
					
					rect.Offset(2,0);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					rect.Offset(0,2);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					rect.Offset(-2,0);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					// Draw description
					rect.Offset(1,-1);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect, 
						format, descriptionColor );
				}
			}

			int color = isMouseOver ? hotColor : normalColor;
			if(iconTexture==null || isMouseOver || icon.NameAlwaysVisible)
			{
				// Render label
				if(icon.Name != null)
				{
					// Render name field
					const int labelWidth = 1000; // Dummy value needed for centering the text
					if(iconTexture==null)
					{
						// Center over target as we have no bitmap
						Rectangle rect = new Rectangle(
							(int)projectedPoint.X - (labelWidth>>1), 
							(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
							labelWidth, 
							drawArgs.screenHeight );

						drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, rect, DrawTextFormat.Center, color);
					}
					else
					{
						// Adjust text to make room for icon
						int spacing = (int)(icon.Width * 0.3f);
						if(spacing>10)
							spacing = 10;
						int offsetForIcon = (icon.Width>>1) + spacing;

						Rectangle rect = new Rectangle(
							(int)projectedPoint.X + offsetForIcon, 
							(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
							labelWidth, 
							drawArgs.screenHeight );

						drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, rect, DrawTextFormat.WordBreak, color);
					}
				}
			}

			if(iconTexture!=null)
			{
				// Render icon
				float xscale = (float)icon.Width / iconTexture.Width;
				float yscale = (float)icon.Height / iconTexture.Height;
				m_sprite.Transform = Matrix.Scaling(xscale,yscale,0);

				if(icon.IsRotated)
					m_sprite.Transform *= Matrix.RotationZ((float)icon.Rotation.Radians - (float)drawArgs.WorldCamera.Heading.Radians);

				m_sprite.Transform *= Matrix.Translation(projectedPoint.X, projectedPoint.Y, 0);
				m_sprite.Draw( iconTexture.Texture,
					new Vector3(iconTexture.Width>>1, iconTexture.Height>>1,0),
					Vector3.Empty,
					color );
				
				// Reset transform to prepare for text rendering later
				m_sprite.Transform = Matrix.Identity;
			}
		}

		/// <summary>
		/// Retrieve an icon's texture
		/// </summary>
		protected IconTexture GetTexture(Icon icon)
		{
			object key = null;
			
			if(icon.Image == null)
			{
				key = (icon.TextureFileName.ToLower().StartsWith("http://") ? icon.SaveFilePath : icon.TextureFileName);
			}
			else
			{
				key = icon.Image;
			}
			if(key==null)
				return null;

			IconTexture res = (IconTexture)m_textures[key];
			return res;
		}

		bool isUpdating = false;
		private void refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(isUpdating)
				return;
			isUpdating = true;
			try
			{
				for(int i = 0; i < this.ChildObjects.Count; i++)
				{
					RenderableObject ro = (RenderableObject)this.ChildObjects[i];
					if(ro != null && ro.IsOn && ro is Icon)
					{
						Icon icon = (Icon)ro;

						if(icon.RefreshInterval == TimeSpan.MaxValue || icon.LastRefresh > System.DateTime.Now - icon.RefreshInterval)
							continue;

						object key = null;
						IconTexture iconTexture = null;

						if(icon.TextureFileName != null && icon.TextureFileName.Length > 0)
						{
							if(icon.TextureFileName.ToLower().StartsWith("http://") && icon.SaveFilePath != null)
							{
								//download it
								try
								{
									WorldWind.Net.WebDownload webDownload = new WorldWind.Net.WebDownload(icon.TextureFileName);
									webDownload.DownloadType = WorldWind.Net.DownloadType.Unspecified;
	
									System.IO.FileInfo saveFile = new System.IO.FileInfo(icon.SaveFilePath);
									if(!saveFile.Directory.Exists)
										saveFile.Directory.Create();

									webDownload.DownloadFile(saveFile.FullName);
								}
								catch{}
						
								iconTexture = (IconTexture)m_textures[icon.SaveFilePath];
								if(iconTexture != null)
								{
									IconTexture tempTexture = iconTexture;
									m_textures[icon.SaveFilePath] = new IconTexture( DrawArgs.Device, icon.SaveFilePath );
									tempTexture.Dispose();
								}
								else
								{
									key = icon.SaveFilePath;
									iconTexture = new IconTexture( DrawArgs.Device, icon.SaveFilePath );
									
									// New texture, cache it
									m_textures.Add(key, iconTexture);

									// Use default dimensions if not set
									if(icon.Width==0)
										icon.Width = iconTexture.Width;
									if(icon.Height==0)
										icon.Height = iconTexture.Height;
								}
								
							}
							else
							{
								// Icon image from file
								iconTexture = (IconTexture)m_textures[icon.TextureFileName];
								if(iconTexture != null)
								{
									IconTexture tempTexture = iconTexture;
									m_textures[icon.SaveFilePath] = new IconTexture( DrawArgs.Device, icon.TextureFileName );
									tempTexture.Dispose();
								}
								else
								{
									key = icon.SaveFilePath;
									iconTexture = new IconTexture( DrawArgs.Device, icon.TextureFileName );
									
									// New texture, cache it
									m_textures.Add(key, iconTexture);

									// Use default dimensions if not set
									if(icon.Width==0)
										icon.Width = iconTexture.Width;
									if(icon.Height==0)
										icon.Height = iconTexture.Height;
								}
							}
						}
						else
						{
							// Icon image from bitmap
							if(icon.Image != null)
							{
								iconTexture = (IconTexture)m_textures[icon.Image];
								if(iconTexture != null)
								{
									IconTexture tempTexture = iconTexture;
									m_textures[icon.SaveFilePath] = new IconTexture( DrawArgs.Device, icon.Image );
									tempTexture.Dispose();
								}
								else
								{
									key = icon.SaveFilePath;
									iconTexture = new IconTexture( DrawArgs.Device, icon.Image );
									
									// New texture, cache it
									m_textures.Add(key, iconTexture);

									// Use default dimensions if not set
									if(icon.Width==0)
										icon.Width = iconTexture.Width;
									if(icon.Height==0)
										icon.Height = iconTexture.Height;
								}
							}
						}

						icon.LastRefresh = System.DateTime.Now;
					}
				}
			}
			catch{}
			finally
			{
				isUpdating = false;
			}
		}
	}

	/// <summary>
	/// One icon in an icon layer
	/// </summary>
	public class Icon : RenderableObject
	{
		public double OnClickZoomAltitude = double.NaN;
		public double OnClickZoomHeading = double.NaN;
		public double OnClickZoomTilt = double.NaN;
		public string SaveFilePath = null;
		public System.DateTime LastRefresh = System.DateTime.MinValue;
		public System.TimeSpan RefreshInterval = System.TimeSpan.MaxValue;

		private Angle m_rotation = Angle.Zero;
		private bool m_isRotated = false;
		private Point3d m_positionD = new Point3d();

		bool m_nameAlwaysVisible = false;
		
		public bool NameAlwaysVisible
		{
			get{ return m_nameAlwaysVisible; }
			set{ m_nameAlwaysVisible = value; }
		}

		public bool IsRotated
		{
			get
			{
				return m_isRotated;
			}
			set
			{
				m_isRotated = value;
			}
		}
	
		public Angle Rotation
		{
			get
			{
				return m_rotation;
			}
			set
			{
				m_rotation = value;
			}
		}

		System.Collections.ArrayList overlays = new ArrayList();
		
		//not a good way to handle this
		public void OverlayOnOpen(object o, EventArgs e)
		{
			System.Windows.Forms.MenuItem mi = (System.Windows.Forms.MenuItem)o;

			foreach(ScreenOverlay overlay in overlays)
			{
				if(overlay == null)
					continue;

				if(overlay.Name.Equals(mi.Text))
				{
					if(!overlay.IsOn)
						overlay.IsOn = true;
				}
			}
		}

		public ScreenOverlay[] Overlays
		{
			get
			{
				if(overlays == null)
				{
					return null;
				}
				else
				{
					return (ScreenOverlay[])overlays.ToArray(typeof(ScreenOverlay));
				}
			}
		}

		public void AddOverlay(ScreenOverlay overlay)
		{
			if(overlay != null)
				overlays.Add(overlay);
		}

		public void RemoveOverlay(ScreenOverlay overlay)
		{
			for(int i = 0; i < overlays.Count; i++)
			{
				ScreenOverlay curOverlay = (ScreenOverlay)overlays[i];
				if(curOverlay.IconImagePath == overlay.IconImagePath && overlay.Name == curOverlay.Name)
				{
					overlays.RemoveAt(i);
				}
			}
		}

		#region private members

		/// <summary>
		/// On-Click browse to location
		/// </summary>
		protected string m_clickableActionURL;

		/// <summary>
		/// Latitude (North/South) in decimal degrees
		/// </summary>
		protected double m_latitude;

		/// <summary>
		/// Longitude (East/West) in decimal degrees
		/// </summary>
		protected double m_longitude;

		#endregion

		/// <summary>
		/// Longer description of icon (addition to name)
		/// </summary>
		//public string Description;

		/// <summary>
		/// The icon altitude above sea level
		/// </summary>
		public double Altitude;

		/// <summary>
		/// Icon bitmap path. (Overrides Image)
		/// </summary>
		public string TextureFileName;

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
		/// On-Click browse to location
		/// </summary>
		public string ClickableActionURL
		{
			get
			{
				return m_clickableActionURL;
			}
			set 
			{
				isSelectable = value != null;
				m_clickableActionURL = value;
			}
		}

		public Point3d PositionD
		{
			get{ return m_positionD; }
			set{ m_positionD = value; }
		}

		/// <summary>
		/// The maximum distance (meters) the icon will be visible from
		/// </summary>
		public double MaximumDisplayDistance = double.MaxValue;

		/// <summary>
		/// The minimum distance (meters) the icon will be visible from
		/// </summary>
		public double MinimumDisplayDistance;

		/// <summary>
		/// Bounding box centered at (0,0) used to calculate whether mouse is over icon/label
		/// </summary>
		public Rectangle SelectionRectangle;

		/// <summary>
		/// Latitude (North/South) in decimal degrees
		/// </summary>
		public double Latitude
		{
			get { return m_latitude; }
		}

		/// <summary>
		/// Longitude (East/West) in decimal degrees
		/// </summary>
		public double Longitude
		{
			get { return m_longitude; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		public Icon(string name,
			double latitude, 
			double longitude) : base( name )
		{
			m_latitude = latitude;
			m_longitude = longitude;
			this.RenderPriority = RenderPriority.Icons;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		public Icon(string name,
			double latitude, 
			double longitude,
			double heightAboveSurface) : base( name )
		{
			m_latitude = latitude;
			m_longitude = longitude;
			Altitude = heightAboveSurface;
			this.RenderPriority = RenderPriority.Icons;
		}

		#region Obsolete

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		[Obsolete]
		public Icon(string name,
			double latitude, 
			double longitude,
			double heightAboveSurface, 
			World parentWorld ) : base( name )
		{
			m_latitude = latitude;
			m_longitude = longitude;
			this.Altitude = heightAboveSurface;
			this.RenderPriority = RenderPriority.Icons;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		[Obsolete]
		public Icon(string name, 
			string description,
			double latitude, 
			double longitude, 
			double heightAboveSurface,
			World parentWorld, 
			Bitmap image,
			int width,
			int height,
			string actionURL) : base( name )
		{
			this.Description = description;
			m_latitude = latitude;
			m_longitude = longitude;
			this.Altitude = heightAboveSurface;
			this.Image = image;
			this.Width = width;
			this.Height = height;
			ClickableActionURL = actionURL;
			this.RenderPriority = RenderPriority.Icons;
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
		[Obsolete]
		public Icon(string name, 
			string description,
			double latitude, 
			double longitude, 
			double heightAboveSurface,
			World parentWorld, 
			string TextureFileName,
			int width,
			int height,
			string actionURL) : base( name )
		{
			this.Description = description;
			m_latitude = latitude;
			m_longitude = longitude;
			this.Altitude = heightAboveSurface;
			this.TextureFileName = TextureFileName;
			this.Width = width;
			this.Height = height;
			ClickableActionURL = actionURL;
			this.RenderPriority = RenderPriority.Icons;
		}

		#endregion

		/// <summary>
		/// Sets the geographic position of the icon.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		public void SetPosition(double latitude, double longitude)
		{
			m_latitude = latitude;
			m_longitude = longitude;

			// Recalculate XYZ coordinates
			isInitialized = false;
		}

		/// <summary>
		/// Sets the geographic position of the icon.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="altitude">The icon altitude above sea level.</param>
		public void SetPosition(double latitude, double longitude, double altitude)
		{
			m_latitude = latitude;
			m_longitude = longitude;
			Altitude = altitude;

			// Recalculate XYZ coordinates
			isInitialized = false;
		}

		#region RenderableObject methods

		public override void Initialize(DrawArgs drawArgs)
		{
			double samplesPerDegree = 50.0 / (drawArgs.WorldCamera.ViewRange.Degrees);
			double elevation = drawArgs.CurrentWorld.TerrainAccessor.GetElevationAt(m_latitude, m_longitude, samplesPerDegree);
			double altitude = (World.Settings.VerticalExaggeration * Altitude + World.Settings.VerticalExaggeration * elevation);
			Position = MathEngine.SphericalToCartesian(m_latitude, m_longitude, 
				altitude + drawArgs.WorldCamera.WorldRadius);

			m_positionD = MathEngine.SphericalToCartesianD(
				Angle.FromDegrees(m_latitude),
				Angle.FromDegrees(m_longitude),
				altitude + drawArgs.WorldCamera.WorldRadius);

			isInitialized = true;
		}

		/// <summary>
		/// Disposes the icon (when disabled)
		/// </summary>
		public override void Dispose()
		{
			// Nothing to dispose
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			// Handled by parent
			return false;
		}

		Matrix lastView = Matrix.Identity;

		public override void Update(DrawArgs drawArgs)
		{
			if(drawArgs.WorldCamera.ViewMatrix != lastView && drawArgs.CurrentWorld.TerrainAccessor != null && drawArgs.WorldCamera.Altitude < 300000)
			{
				double samplesPerDegree = 50.0 / drawArgs.WorldCamera.ViewRange.Degrees;
				double elevation = drawArgs.CurrentWorld.TerrainAccessor.GetElevationAt(m_latitude, m_longitude, samplesPerDegree);
				double altitude = World.Settings.VerticalExaggeration * Altitude + World.Settings.VerticalExaggeration * elevation;
				Position = MathEngine.SphericalToCartesian(m_latitude, m_longitude, 
					altitude + drawArgs.WorldCamera.WorldRadius);

				lastView = drawArgs.WorldCamera.ViewMatrix;
			}

			if(overlays != null)
			{
				for(int i = 0; i < overlays.Count; i++)
				{
					ScreenOverlay curOverlay = (ScreenOverlay)overlays[i];
					if(curOverlay != null)
					{
						curOverlay.Update(drawArgs);
					}
				}
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(overlays != null)
			{
				for(int i = 0; i < overlays.Count; i++)
				{
					ScreenOverlay curOverlay = (ScreenOverlay)overlays[i];
					if(curOverlay != null && curOverlay.IsOn)
					{
						curOverlay.Render(drawArgs);
					}
				}
			}
		}

		#endregion

		private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{

		}
	}
}
