using System;
using System.IO;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;
using WorldWind;

namespace WorldWind.Widgets
{
    public delegate void VisibleChangedHandler(object o, bool state);

	public class Form : IWidget, IInteractive
	{
		System.Drawing.Point m_Location = new System.Drawing.Point(0, 0);
		System.Drawing.Size m_Size = new System.Drawing.Size(300, 200);
		IWidget m_ParentWidget = null;
		IWidgetCollection m_ChildWidgets = new WidgetCollection();
		string m_Name = "";
        Alignment m_alignment = Alignment.None;

		System.Drawing.Color m_BackgroundColor = System.Drawing.Color.FromArgb(
			100, 0, 0, 0);

		bool m_HideBorder = false;
		System.Drawing.Color m_BorderColor = System.Drawing.Color.GhostWhite;
		System.Drawing.Color m_HeaderColor =  System.Drawing.Color.FromArgb(
			120,
			System.Drawing.Color.Coral.R,
			System.Drawing.Color.Coral.G,
			System.Drawing.Color.Coral.B);
		
		int m_HeaderHeight = 20;

		System.Drawing.Color m_TextColor = System.Drawing.Color.GhostWhite;
		Microsoft.DirectX.Direct3D.Font m_WorldWindDingsFont = null;
		Microsoft.DirectX.Direct3D.Font m_TextFont = null;

        bool m_HideHeader = false;
		bool m_AutoHideHeader = false;
		bool m_Visible = true;
		bool m_Enabled = true;
		object m_Tag = null;
		string m_Text = "";
		
		public Form()
		{
		}

		#region Properties
        public Alignment Alignment
        {
            get { return m_alignment; }
            set { m_alignment = value; }
        }
		public bool HideBorder
		{
			get{ return m_HideBorder; }
			set{ m_HideBorder = value; }
		}
        public bool HideHeader
        {
            get { return m_HideHeader; }
            set { m_HideHeader = value; }
        }

		public Microsoft.DirectX.Direct3D.Font TextFont
		{
			get
			{
				return m_TextFont;
			}
			set
			{
				m_TextFont = value;
			}
		}
		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}
		public bool AutoHideHeader
		{
			get
			{
				return m_AutoHideHeader;
			}
			set
			{
				m_AutoHideHeader = value;
			}
		}
		public System.Drawing.Color HeaderColor
		{
			get
			{
				return m_HeaderColor;
			}
			set
			{
				m_HeaderColor = value;
			}
		}
		public int HeaderHeight
		{
			get
			{
				return m_HeaderHeight;
			}
			set
			{
				m_HeaderHeight = value;
			}
		}
		public System.Drawing.Color BorderColor
		{
			get
			{
				return m_BorderColor;
			}
			set
			{
				m_BorderColor = value;
			}
		}
		public System.Drawing.Color BackgroundColor
		{
			get
			{
				return m_BackgroundColor;
			}
			set
			{
				m_BackgroundColor = value;
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
			}
		}
		#endregion

		#region IWidget Members

		public IWidget ParentWidget
		{
			get
			{
				return m_ParentWidget;
			}
			set
			{
				m_ParentWidget = value;
			}
		}

		public bool Visible
		{
			get
			{
				return m_Visible;
			}
			set
			{
				if(m_Visible != value)
				{
					m_Visible = value;
					if(OnVisibleChanged != null)
						OnVisibleChanged(this, value);
				}
			}
		}

		public object Tag
		{
			get
			{
				return m_Tag;
			}
			set
			{
				m_Tag = value;
			}
		}

		public IWidgetCollection ChildWidgets
		{
			get
			{
				return m_ChildWidgets;
			}
			set
			{
				m_ChildWidgets = value;
			}
		}

		public System.Drawing.Size ClientSize
		{
			get
			{
				return m_Size;
			}
			set
			{
				m_Size = value;
			}
		}

		public bool Enabled
		{
			get
			{
				return m_Enabled;
			}
			set
			{
				m_Enabled = value;
			}
		}

		public System.Drawing.Point ClientLocation
		{
			get
			{
                System.Drawing.Point location = m_Location;

                if (m_ParentWidget != null)
                {
                    if (m_alignment == Alignment.Right || m_alignment == (Alignment.Top | Alignment.Right) || m_alignment == (Alignment.Bottom | Alignment.Right))
                    {
                        location.X = m_ParentWidget.ClientSize.Width - ClientSize.Width - location.X;
                    }
                    if (m_alignment == Alignment.Bottom || m_alignment == (Alignment.Left | Alignment.Bottom) || m_alignment == (Alignment.Right | Alignment.Bottom))
                    {
                        location.Y = m_ParentWidget.ClientSize.Height - ClientSize.Height - location.Y;
                    }
                }
                return location;
			}
			set
			{
				m_Location = value;
			}
		}

		public System.Drawing.Point AbsoluteLocation
		{
			get
			{
				if(m_ParentWidget != null)
				{
					return new System.Drawing.Point(
						ClientLocation.X + m_ParentWidget.AbsoluteLocation.X,
                        ClientLocation.Y + m_ParentWidget.AbsoluteLocation.Y);
				}
				else
				{
					return ClientLocation;		
				}
			}
		}

		[DllImport("gdi32.dll")]
		static extern int AddFontResource(string lpszFilename);

		int resizeBuffer = 5;

		public virtual void Render(DrawArgs drawArgs)
		{
			if(!Visible)
				return;

			if(m_TextFont == null)
			{
				System.Drawing.Font localHeaderFont = new System.Drawing.Font("Arial", 12.0f, System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Bold);
				m_TextFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, localHeaderFont);
			}

			if(m_WorldWindDingsFont == null)
			{
				AddFontResource(Path.Combine(System.Windows.Forms.Application.StartupPath, "World Wind Dings 1.04.ttf"));
				System.Drawing.Text.PrivateFontCollection fpc = new System.Drawing.Text.PrivateFontCollection();
				fpc.AddFontFile(Path.Combine(System.Windows.Forms.Application.StartupPath, "World Wind Dings 1.04.ttf"));
				System.Drawing.Font worldwinddings = new System.Drawing.Font(fpc.Families[0], 12.0f);
			
				m_WorldWindDingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, worldwinddings);
			}

			if(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer)
			{
				DrawArgs.MouseCursor = CursorType.SizeNWSE;
			}
			else if(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer)
			{
				DrawArgs.MouseCursor = CursorType.SizeNESW;
			}
			else if(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer + ClientSize.Height &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height)
			{
				DrawArgs.MouseCursor = CursorType.SizeNESW;
			}
			else if(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer + ClientSize.Height &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height)
			{
				DrawArgs.MouseCursor = CursorType.SizeNWSE;
			}
			else if(
				(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height) ||
				(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height))
			{
				DrawArgs.MouseCursor = CursorType.SizeWE;
			}
			else if(
				(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer) ||
				(DrawArgs.LastMousePosition.X > AbsoluteLocation.X - resizeBuffer &&
				DrawArgs.LastMousePosition.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				DrawArgs.LastMousePosition.Y > AbsoluteLocation.Y - resizeBuffer + ClientSize.Height &&
				DrawArgs.LastMousePosition.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height))
			{
				DrawArgs.MouseCursor = CursorType.SizeNS;
			}

			if(ClientSize.Height > drawArgs.parentControl.Height)
			{
				ClientSize = new System.Drawing.Size(ClientSize.Width, drawArgs.parentControl.Height);
			}

			if(ClientSize.Width > drawArgs.parentControl.Width)
			{
				ClientSize = new System.Drawing.Size(drawArgs.parentControl.Width, ClientSize.Height);
			}

            if (!m_HideHeader && 
                (!m_AutoHideHeader || (DrawArgs.LastMousePosition.X >= ClientLocation.X &&
                DrawArgs.LastMousePosition.X <= ClientLocation.X + m_Size.Width &&
                DrawArgs.LastMousePosition.Y >= ClientLocation.Y &&
                DrawArgs.LastMousePosition.Y <= ClientLocation.Y + m_Size.Height)))
			{
				
				Widgets.Utilities.DrawBox(
                    ClientLocation.X,
                    ClientLocation.Y,
					m_Size.Width,
					m_HeaderHeight,
					0.0f,
					m_HeaderColor.ToArgb(),
					drawArgs.device);

				m_TextFont.DrawText(
					null,
					m_Text,
                    new System.Drawing.Rectangle(ClientLocation.X + 2, ClientLocation.Y, m_Size.Width, m_HeaderHeight),
					DrawTextFormat.None,
					m_TextColor.ToArgb());

				m_WorldWindDingsFont.DrawText(
					null,
					"E",
                    new System.Drawing.Rectangle(ClientLocation.X + m_Size.Width - 15, ClientLocation.Y + 2, m_Size.Width, m_Size.Height),
					DrawTextFormat.NoClip,
					System.Drawing.Color.White.ToArgb());

				m_OutlineVertsHeader[0].X = AbsoluteLocation.X;
				m_OutlineVertsHeader[0].Y = AbsoluteLocation.Y;

				m_OutlineVertsHeader[1].X = AbsoluteLocation.X + ClientSize.Width;
				m_OutlineVertsHeader[1].Y = AbsoluteLocation.Y;

				m_OutlineVertsHeader[2].X = AbsoluteLocation.X + ClientSize.Width;
				m_OutlineVertsHeader[2].Y = AbsoluteLocation.Y + m_HeaderHeight;
		
				m_OutlineVertsHeader[3].X = AbsoluteLocation.X;
				m_OutlineVertsHeader[3].Y = AbsoluteLocation.Y + m_HeaderHeight;

				m_OutlineVertsHeader[4].X = AbsoluteLocation.X;
				m_OutlineVertsHeader[4].Y = AbsoluteLocation.Y;

				if(!m_HideBorder)
					Widgets.Utilities.DrawLine(m_OutlineVertsHeader, m_BorderColor.ToArgb(), drawArgs.device);

			}

			Widgets.Utilities.DrawBox(
                ClientLocation.X,
                ClientLocation.Y + m_HeaderHeight,
				m_Size.Width,
				m_Size.Height - m_HeaderHeight,
				0.0f,
				m_BackgroundColor.ToArgb(),
				drawArgs.device);
			
			for(int index = m_ChildWidgets.Count - 1; index >= 0; index--)
			{
				IWidget currentChildWidget = m_ChildWidgets[index] as IWidget;
				if(currentChildWidget != null)
				{
					if(currentChildWidget.ParentWidget == null || currentChildWidget.ParentWidget != this)
					{
						currentChildWidget.ParentWidget = this;
					}
					currentChildWidget.Render(drawArgs);
				}
			}

			m_OutlineVerts[0].X = AbsoluteLocation.X;
			m_OutlineVerts[0].Y = AbsoluteLocation.Y + m_HeaderHeight;

			m_OutlineVerts[1].X = AbsoluteLocation.X + ClientSize.Width;
			m_OutlineVerts[1].Y = AbsoluteLocation.Y + m_HeaderHeight;

			m_OutlineVerts[2].X = AbsoluteLocation.X + ClientSize.Width;
			m_OutlineVerts[2].Y = AbsoluteLocation.Y + ClientSize.Height;
		
			m_OutlineVerts[3].X = AbsoluteLocation.X;
			m_OutlineVerts[3].Y = AbsoluteLocation.Y + ClientSize.Height;

			m_OutlineVerts[4].X = AbsoluteLocation.X;
			m_OutlineVerts[4].Y = AbsoluteLocation.Y + m_HeaderHeight;

			if(!m_HideBorder)
				Widgets.Utilities.DrawLine(m_OutlineVerts, m_BorderColor.ToArgb(), drawArgs.device);			
		}

		private Vector2[] m_OutlineVerts = new Vector2[5];
		private Vector2[] m_OutlineVertsHeader = new Vector2[5];

		#endregion

		bool m_IsDragging = false;
		System.Drawing.Point m_LastMousePosition = new System.Drawing.Point(0,0);

		bool isResizingLeft = false;
		bool isResizingRight = false;
		bool isResizingBottom = false;
		bool isResizingTop = false;
		bool isResizingUL = false;
		bool isResizingUR = false;
		bool isResizingLL = false;
		bool isResizingLR = false;

		#region IInteractive Members

		public bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
            if (!Visible)
                return false;

			bool handled = false;

			bool inClientArea = false;

            if (e.X >= ClientLocation.X &&
                e.X <= ClientLocation.X + m_Size.Width &&
                e.Y >= ClientLocation.Y &&
                e.Y <= ClientLocation.Y + m_Size.Height)
			{
				m_ParentWidget.ChildWidgets.BringToFront(this);
				inClientArea = true;
			}

			if(e.X > AbsoluteLocation.X - resizeBuffer &&
				e.X < AbsoluteLocation.X + resizeBuffer &&
				e.Y > AbsoluteLocation.Y - resizeBuffer &&
				e.Y < AbsoluteLocation.Y + resizeBuffer)
			{
				isResizingUL = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer + ClientSize.Width &&
				e.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				e.Y > AbsoluteLocation.Y - resizeBuffer &&
				e.Y < AbsoluteLocation.Y + resizeBuffer)
			{
				isResizingUR = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer &&
				e.X < AbsoluteLocation.X + resizeBuffer &&
				e.Y > AbsoluteLocation.Y - resizeBuffer + ClientSize.Height &&
				e.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height)
			{
				isResizingLL = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer + ClientSize.Width &&
				e.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				e.Y > AbsoluteLocation.Y - resizeBuffer + ClientSize.Height &&
				e.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height)
			{
				isResizingLR = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer &&
				e.X < AbsoluteLocation.X + resizeBuffer &&
				e.Y > AbsoluteLocation.Y - resizeBuffer &&
				e.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height )
			{
				isResizingLeft = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer + ClientSize.Width &&
				e.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				e.Y > AbsoluteLocation.Y - resizeBuffer &&
				e.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height)
			{
				isResizingRight = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer &&
				e.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				e.Y > AbsoluteLocation.Y - resizeBuffer &&
				e.Y < AbsoluteLocation.Y + resizeBuffer
				)
			{
				isResizingTop = true;
			}
			else if(e.X > AbsoluteLocation.X - resizeBuffer &&
				e.X < AbsoluteLocation.X + resizeBuffer + ClientSize.Width &&
				e.Y > AbsoluteLocation.Y - resizeBuffer + ClientSize.Height &&
				e.Y < AbsoluteLocation.Y + resizeBuffer + ClientSize.Height)
			{
				isResizingBottom = true;
			}
            else if (e.X >= ClientLocation.X &&
				e.X <= AbsoluteLocation.X + ClientSize.Width &&
				e.Y >= AbsoluteLocation.Y &&
				e.Y <= AbsoluteLocation.Y + m_HeaderHeight)
			{
				m_IsDragging = true;
				handled = true;
			}
			m_LastMousePosition = new System.Drawing.Point(e.X, e.Y);

			if(!handled)
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

			if(!handled && inClientArea)
			{
				handled = true;
			}

			return handled;
			 
		}

		public bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
            if (!Visible)
                return false;

			bool handled = false;
			if(e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				if(m_IsDragging)
				{
					m_IsDragging = false;
				}
			}

			bool inClientArea = false;

            if (e.X >= ClientLocation.X &&
                e.X <= ClientLocation.X + m_Size.Width &&
                e.Y >= ClientLocation.Y &&
                e.Y <= ClientLocation.Y + m_Size.Height)
			{
				inClientArea = true;
			}

			if(inClientArea)
			{
				if(!m_HideHeader && isPointInCloseBox(new System.Drawing.Point(e.X, e.Y)))
				{
					Visible = false;
					handled = true;
				}
			}

			for(int i = 0; i < m_ChildWidgets.Count; i++)
			{
				if(m_ChildWidgets[i] is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[i] as IInteractive;
					handled = currentInteractive.OnMouseUp(e);
				}
			}

			if(!handled && inClientArea)
			{
				handled = true;
			}

			if(isResizingTop)
			{
				isResizingTop = false;
			}
			if(isResizingBottom)
			{
				isResizingBottom = false;
			}
			if(isResizingLeft)
			{
				isResizingLeft = false;
			}
			if(isResizingRight)
			{
				isResizingRight = false;
			}
			if(isResizingUL)
			{
				isResizingUL = false;
			}
			if(isResizingUR)
			{
				isResizingUR = false;
			}
			if(isResizingLL)
			{
				isResizingLL = false;
			}
			if(isResizingLR)
			{
				isResizingLR = false;
			}

			return handled;
		}

		public bool OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			return false;
		}

		public bool OnKeyUp(System.Windows.Forms.KeyEventArgs e)
		{
			return false;
		}

		public bool OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
		{
			return false;
		}

		public bool OnMouseEnter(EventArgs e)
		{
			return false;
		}

		public event VisibleChangedHandler OnVisibleChanged;

		System.Drawing.Size minSize = new System.Drawing.Size(20, 20);

		public bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
            if (!Visible)
                return false;

			bool handled = false;
			int deltaX = e.X - m_LastMousePosition.X;
			int deltaY = e.Y - m_LastMousePosition.Y;

			if(isResizingTop || isResizingUL || isResizingUR)
			{
                ClientLocation = new System.Drawing.Point(ClientLocation.X, ClientLocation.Y + deltaY);
				m_Size.Height -= deltaY;
			}
			else if(isResizingBottom || isResizingLL || isResizingLR)
			{
				m_Size.Height += deltaY;
			}
			else if(isResizingRight || isResizingUR || isResizingLR)
			{
				m_Size.Width += deltaX;
			}
			else if(isResizingLeft || isResizingUL || isResizingLL)
			{
                ClientLocation = new System.Drawing.Point(ClientLocation.X + deltaX, ClientLocation.Y);
				m_Size.Width -= deltaX;
			}
			else if(m_IsDragging)
			{
                ClientLocation = new System.Drawing.Point(ClientLocation.X + deltaX, ClientLocation.Y + deltaY);

                if (ClientLocation.X < 0)
                    ClientLocation = new System.Drawing.Point(0, ClientLocation.Y);
                if (ClientLocation.Y < 0)
                    ClientLocation = new System.Drawing.Point(ClientLocation.X, 0);
                if (ClientLocation.Y + m_Size.Height > DrawArgs.ParentControl.Height)
                    ClientLocation = new System.Drawing.Point(ClientLocation.X , DrawArgs.ParentControl.Height - m_Size.Height);
                if (ClientLocation.X + m_Size.Width > DrawArgs.ParentControl.Width)
                    ClientLocation = new System.Drawing.Point(DrawArgs.ParentControl.Width - m_Size.Width, ClientLocation.Y);

				handled = true;
			}

			if(m_Size.Width < minSize.Width)
			{
				m_Size.Width = minSize.Width;
			}

			if(m_Size.Height < minSize.Height)
			{
				m_Size.Height = minSize.Height;
			}

			m_LastMousePosition = new System.Drawing.Point(e.X, e.Y);

			for(int i = 0; i < m_ChildWidgets.Count; i++)
			{
				if(m_ChildWidgets[i] is IInteractive)
				{
					IInteractive currentInteractive = m_ChildWidgets[i] as IInteractive;
					handled = currentInteractive.OnMouseMove(e);
				}
			}

			bool inClientArea = false;
            if (e.X >= ClientLocation.X &&
                e.X <= ClientLocation.X + m_Size.Width &&
                e.Y >= ClientLocation.Y &&
                e.Y <= ClientLocation.Y + m_Size.Height)
			 {
				inClientArea = true;
			 }


			if(!handled && inClientArea)
			{
				handled = true;
			}
			return handled;
		}

		private bool isPointInCloseBox(System.Drawing.Point absolutePoint)
		{
			int closeBoxSize = 10;
			int closeBoxYOffset = 2;
			int closeBoxXOffset = m_Size.Width - 15;

			if(absolutePoint.X >= ClientLocation.X + closeBoxXOffset &&
                absolutePoint.X <= ClientLocation.X + closeBoxXOffset + closeBoxSize &&
                absolutePoint.Y >= ClientLocation.Y + closeBoxYOffset &&
                absolutePoint.Y <= ClientLocation.Y + closeBoxYOffset + closeBoxSize)
			{
				return true;
			}
			
			return false;
		}

		public bool OnMouseLeave(EventArgs e)
		{
			return false;
		}

		public bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
			return false;
		}

		#endregion
	}

    public enum Alignment
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }
}
