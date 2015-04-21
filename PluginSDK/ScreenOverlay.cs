using System;

namespace WorldWind.Renderable
{
	public enum ScreenAlignment
	{
		Left,
		Right
	}

    public enum ScreenUnits
    {
        Pixels,
        Fraction
    }

	/// <summary>
	/// Summary description for ScreenOverlay.
	/// </summary>
	public class ScreenOverlay : WorldWind.Renderable.RenderableObject
	{
		WorldWind.Widgets.Form overlay = null;

		ScreenAlignment alignment = ScreenAlignment.Left;

		string clickableUrl = null;
        float m_Width = 0;
        float m_Height = 0;
		bool m_ShowHeader = true;
        public ScreenUnits OffsetXUnits = ScreenUnits.Pixels;
        public ScreenUnits OffsetYUnits = ScreenUnits.Pixels;
        public ScreenUnits SizeXUnits = ScreenUnits.Pixels;
        public ScreenUnits SizeYUnits = ScreenUnits.Pixels;
        public ScreenUnits PositionXUnits = ScreenUnits.Pixels;
        public ScreenUnits PositionYUnits = ScreenUnits.Pixels;

        float m_StartX = 0;
        float m_StartY = 0;
		string m_ImageUri = null;
		string m_SaveFilePath = null;
		double m_RefreshTimeSec = 0;
		bool m_HideBorder = false;
		System.Drawing.Color m_BorderColor = System.Drawing.Color.White;
        float m_offsetX = 0;
        float m_offsetY = 0;
		public System.Drawing.Color BorderColor
		{
			get{ return (overlay == null ? m_BorderColor : overlay.BorderColor); }
			set
			{
				m_BorderColor = value;
				if(overlay != null)
					overlay.BorderColor = value;
			}
		}

        public float OffsetX
        {
            get { return m_offsetX; }
            set { m_offsetX = value; }
        }

        public float OffsetY
        {
            get { return m_offsetY; }
            set { m_offsetY = value; }
        }
		
		public bool HideBorder
		{
			get{ return (overlay == null ? m_HideBorder : overlay.HideBorder); }
			set
			{ 
				m_HideBorder = value; 
				if(overlay != null)
					overlay.HideBorder = value;
			}
		}

		public string ClickableUrl
		{
			get
			{
				return clickableUrl;
			}
			set
			{
				clickableUrl = value;
				if(pBox != null)
					pBox.ClickableUrl = value;
			}
		}
		
		public ScreenAlignment Alignment
		{
			get{ return alignment; }
			set{ alignment = value; }
		}

		public double RefreshTimeSec
		{
			get
			{
				return m_RefreshTimeSec;
			}
			set
			{
				m_RefreshTimeSec = value;
			}
		}

		public bool ShowHeader
		{
			get{ return m_ShowHeader; }
			set{ m_ShowHeader = value; }
		}

		public string SaveFilePath
		{
			get
			{
				return m_SaveFilePath;
			}
			set
			{
				m_SaveFilePath = value;
			}
		}

        public float Width
		{
			get
			{
				return m_Width;
			}
			set
			{
				m_Width = value;
			}
		}

        public float Height
		{
			get
			{
				return m_Height;
			}
			set
			{
				m_Height = value;
			}
		}

        public ScreenOverlay(string name, float startX, float startY, string imageUri)
            : base(name)
		{
			m_StartX = startX;
			m_StartY = startY;
			m_ImageUri = imageUri;

            if(DrawArgs.ParentControl != null)
                DrawArgs.ParentControl.Resize += new EventHandler(ParentControl_Resize);
		}

        void ParentControl_Resize(object sender, EventArgs e)
        {
            if (overlay != null)
            {
                if (alignment == ScreenAlignment.Left)
                {
                    int x = (int)m_StartX;
                    int y = (int)m_StartY;

                    int offsetX = (int)m_offsetX;
                    int offsetY = (int)m_offsetY;

                    

                    if (PositionXUnits == ScreenUnits.Fraction)
                        x = (int)(DrawArgs.ParentControl.Width * m_StartX) - offsetX;
                    if (PositionYUnits == ScreenUnits.Fraction)
                        y = (int)(DrawArgs.ParentControl.Height * m_StartY) - offsetY;

                    overlay.ClientLocation = new System.Drawing.Point(x, y);
                }
                else
                {
                    int x = (int)(DrawArgs.ParentControl.Width - m_StartX);
                    int y = (int)m_StartY;

                    if (PositionXUnits == ScreenUnits.Fraction)
                        x = (int)(DrawArgs.ParentControl.Width - DrawArgs.ParentControl.Width * m_StartX);
                    if (PositionYUnits == ScreenUnits.Fraction)
                        y = (int)(DrawArgs.ParentControl.Height * m_StartY);
                    
                    overlay.ClientLocation = new System.Drawing.Point(x, y);
                    
                }

                int width = (int)m_Width;
                int height = (int)m_Height;

                if (SizeXUnits == ScreenUnits.Fraction)
                    width = (int)(DrawArgs.ParentControl.Width * m_Width);
                if (SizeYUnits == ScreenUnits.Fraction)
                    height = (int)(DrawArgs.ParentControl.Height * m_Height);

                overlay.ClientSize = new System.Drawing.Size(width, height);
            }	
        }

		public override void Dispose()
		{
			if(overlay != null)
			{
				overlay.Visible = false;
			}

			isInitialized = false;
		}

		Widgets.PictureBox pBox;

		public override void Initialize(DrawArgs drawArgs)
		{
			if(overlay == null)
			{
				overlay = new WorldWind.Widgets.Form();
				overlay.Text = name;
                ParentControl_Resize(null, null);
                overlay.OnVisibleChanged += new WorldWind.Widgets.VisibleChangedHandler(overlay_OnVisibleChanged);

                overlay.AutoHideHeader = !m_ShowHeader;
				overlay.HideBorder = m_HideBorder;
				overlay.BorderColor = m_BorderColor;

				pBox = new WorldWind.Widgets.PictureBox();
				pBox.ClickableUrl = clickableUrl;
				pBox.RefreshTime = m_RefreshTimeSec * 1000;
				pBox.Opacity = Opacity;
				pBox.ParentWidget = overlay;
				pBox.ImageUri = m_ImageUri;
				pBox.SaveFilePath = m_SaveFilePath;
				pBox.ClientLocation = new System.Drawing.Point(0,overlay.HeaderHeight);
                pBox.ClientSize = overlay.ClientSize;
				
                pBox.Visible = true;
                if (m_Width == 0 && m_Height == 0)
                {
                    pBox.SizeParentToImage = true;
                }
				overlay.ChildWidgets.Add(pBox);
				DrawArgs.RootWidget.ChildWidgets.Add(overlay);
			}

			if(!overlay.Visible)
				overlay.Visible = true;

			isInitialized = true;
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(overlay != null && overlay.Visible && pBox != null && pBox.Visible && pBox.IsLoaded)
			{
				if(pBox.ClientSize.Width != overlay.ClientSize.Width)
				{
					pBox.ClientSize = new System.Drawing.Size(overlay.ClientSize.Width, pBox.ClientSize.Height);
				}

				if(pBox.ClientSize.Height != overlay.ClientSize.Height - overlay.HeaderHeight)
				{
					pBox.ClientSize = new System.Drawing.Size(pBox.ClientSize.Width, overlay.ClientSize.Height - overlay.HeaderHeight);
				}
			}
		}

		public override void Update(DrawArgs drawArgs)
		{
			if(IsOn && !isInitialized)
			{
				Initialize(drawArgs);
			}
			else if(!IsOn && isInitialized)
			{
				Dispose();
			}
		}

		public override byte Opacity
		{
			get
			{
				return base.Opacity;
			}
			set
			{
				base.Opacity = value;
				if(pBox != null)
				{
					pBox.Opacity = value;
				}
			}
		}


		private void overlay_OnVisibleChanged(object o, bool state)
		{
			if(!state)
			{
				IsOn = false;
			}
		}
	}
}
