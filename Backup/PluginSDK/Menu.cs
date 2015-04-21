using System;
using System.Collections;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using WorldWind.Renderable;
using Utility;

namespace WorldWind.Menu
{
	public interface IMenu
	{
		void OnKeyUp(KeyEventArgs keyEvent);
		void OnKeyDown(KeyEventArgs keyEvent);
		bool OnMouseUp(MouseEventArgs e);
		bool OnMouseDown(MouseEventArgs e);
		bool OnMouseMove(MouseEventArgs e);
		bool OnMouseWheel(MouseEventArgs e);
		void Render(DrawArgs drawArgs);
		void Dispose();
	}

	public abstract class MenuButton : IMenu
	{
		#region Private Members

		private string _iconTexturePath;
		private Texture m_iconTexture;
		private System.Drawing.Size _iconTextureSize;
		string _description;
		float curSize;
		static int white = System.Drawing.Color.White.ToArgb();
		static int black = System.Drawing.Color.Black.ToArgb();
		static int transparent = Color.FromArgb(140,255,255,255).ToArgb();
		int alpha;
		const int alphaStep = 30;
		const float zoomSpeed = 1.2f;

		public static float NormalSize;
		public static float SelectedSize;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.MenuButton"/> class.
		/// </summary>
		protected MenuButton() 
		{
		}
		
		#region Properties
		public string Description
		{
			get
			{
				if(this._description == null)
					return "N/A";
				else
					return this._description;
			}
			set
			{
				this._description = value;
			}
		}
		
		public Texture IconTexture
		{
			get
			{
				return m_iconTexture;
			}
		}

		public System.Drawing.Size IconTextureSize
		{
			get
			{
				return this._iconTextureSize;
			}
		}
		#endregion

		public MenuButton(string iconTexturePath)
		{
			this._iconTexturePath = iconTexturePath;
		}

		public abstract bool IsPushed();
		public abstract void SetPushed(bool isPushed);

		public void InitializeTexture(Device device)
		{
			try
			{
				m_iconTexture = ImageHelper.LoadIconTexture( this._iconTexturePath);

				using(Surface s = m_iconTexture.GetSurfaceLevel(0))
				{
					SurfaceDescription desc = s.Description;
					this._iconTextureSize = new Size(desc.Width, desc.Height);
				}
			}
			catch
			{
			}
		}

		public void RenderLabel( DrawArgs drawArgs, int x, int y, int buttonHeight, bool selected )
		{
			if (selected)
			{
				if(buttonHeight == curSize)
				{
					alpha += alphaStep; 
					if(alpha>255)
						alpha = 255;
				}
			}
			else
			{
				alpha -= alphaStep;
				if (alpha<0)
				{
					alpha=0;
					return;
				}
			}

			DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.Center | DrawTextFormat.WordBreak;
			int halfWidth = (int)(SelectedSize*0.75);
			Rectangle rect = new System.Drawing.Rectangle(
				x-halfWidth+1,
				(int)(y+SelectedSize)+1,
				(int)halfWidth*2,
				200);

			if(rect.Right>drawArgs.screenWidth)
			{
				rect = Rectangle.FromLTRB(rect.Left, rect.Top, drawArgs.screenWidth, rect.Bottom);
			}

			drawArgs.toolbarFont.DrawText(
				null,
				Description,
				rect,
				format,
				black & 0xffffff + (alpha<<24));
					
			rect.Offset(2,0);

			drawArgs.toolbarFont.DrawText(
				null,
				Description,
				rect,
				format,
				black & 0xffffff + (alpha<<24));
					
			rect.Offset(0,2);

			drawArgs.toolbarFont.DrawText(
				null,
				Description,
				rect,
				format,
				black & 0xffffff + (alpha<<24));
					
			rect.Offset(-2,0);

			drawArgs.toolbarFont.DrawText(
				null,
				Description,
				rect,
				format,
				black & 0xffffff + (alpha<<24));
					
			rect.Offset(1,-1);

			drawArgs.toolbarFont.DrawText(
				null,
				Description,
				rect,
				format,
				white & 0xffffff + (alpha<<24));
		}

		public float CurrentSize
		{
			get { return curSize; }
		}

		public void RenderEnabledIcon(Sprite sprite, DrawArgs drawArgs, float centerX, float topY, 
			bool selected )
		{
			float width = selected ? MenuButton.SelectedSize : width = MenuButton.NormalSize;
			RenderLabel( drawArgs, (int)centerX, (int)topY, (int)width, selected );

			int color = selected ? white : transparent;

			float centerY = topY + curSize*0.5f;
			this.RenderIcon(sprite, (int)centerX, (int)centerY, (int)curSize, (int)curSize, color, m_iconTexture);

			if(curSize==0)
				curSize=width;
			if(width>curSize)
			{
				curSize=(int)(curSize*zoomSpeed);
				if(width<curSize)
					curSize = width;
			}
			else if(width<curSize)
			{
				curSize=(int)(curSize/zoomSpeed);
				if(width>curSize)
					curSize = width;
			}
		}

		private void RenderIcon(Sprite sprite, float centerX, float centerY, 
			int buttonWidth, int buttonHeight, int color, Texture t)
		{
			int halfIconWidth = (int)(0.5f * buttonWidth);
			int halfIconHeight = (int)(0.5f * buttonHeight);

			float scaleWidth = (float)buttonWidth / this._iconTextureSize.Width;
			float scaleHeight = (float)buttonHeight / this._iconTextureSize.Height;

			sprite.Transform = Matrix.Transformation2D(
				Vector2.Empty, 0.0f, 
				new Vector2(scaleWidth, scaleHeight),
				Vector2.Empty,
				0.0f, 
				new Vector2(centerX, centerY));

			sprite.Draw( t, 
				new Vector3(this._iconTextureSize.Width / 2.0f, this._iconTextureSize.Height / 2.0f, 0), 
				Vector3.Empty, 
				color);
		}

		public abstract void Update(DrawArgs drawArgs);
		public abstract void Render(DrawArgs drawArgs);
		public abstract bool OnMouseUp(MouseEventArgs e);
		public abstract bool OnMouseMove(MouseEventArgs e);
		public abstract bool OnMouseDown(MouseEventArgs e);
		public abstract bool OnMouseWheel(MouseEventArgs e);
		public abstract void OnKeyUp(KeyEventArgs keyEvent);
		public abstract void OnKeyDown(KeyEventArgs keyEvent);
		
		public virtual void Dispose()
		{
			if(m_iconTexture != null)
			{
				m_iconTexture.Dispose();
				m_iconTexture = null;
			}
		}

	}

	public class MenuCollection : IMenu
	{
		System.Collections.ArrayList _menus = new System.Collections.ArrayList();

		#region IMenu Members

		public void OnKeyUp(KeyEventArgs keyEvent)
		{
			foreach(IMenu m in this._menus)
				m.OnKeyUp(keyEvent);
		}

		public void OnKeyDown(KeyEventArgs keyEvent)
		{
			foreach(IMenu m in this._menus)
				m.OnKeyDown(keyEvent);
		}

		public bool OnMouseUp(MouseEventArgs e)
		{
			foreach(IMenu m in this._menus)
			{
				if(m.OnMouseUp(e))
					return true;
			}
			return false;
		}

		public bool OnMouseDown(MouseEventArgs e)
		{
			foreach(IMenu m in this._menus)
			{
				if(m.OnMouseDown(e))
					return true;
			}
			return false;
		}

		public bool OnMouseMove(MouseEventArgs e)
		{
			foreach(IMenu m in this._menus)
			{
				if(m.OnMouseMove(e))
					return true;
			}
			return false;
		}

		public bool OnMouseWheel(MouseEventArgs e)
		{
			foreach(IMenu m in this._menus)
			{
				if(m.OnMouseWheel(e))
					return true;
			}
			return false;
		}

		public void Render(DrawArgs drawArgs)
		{
			foreach(IMenu m in this._menus)
				m.Render(drawArgs);
		}

		public void Dispose()
		{
			foreach(IMenu m in this._menus)
				m.Dispose();
		}

		#endregion

		public void AddMenu(IMenu menu)
		{
			lock(this._menus.SyncRoot)
			{
				this._menus.Add(menu);
			}
		}

		public void RemoveMenu(IMenu menu)
		{
			lock(this._menus.SyncRoot)
			{
				this._menus.Remove(menu);
			}
		}
	}

	public abstract class SideBarMenu : IMenu
	{
		public long Id;

		public readonly int Left;
		public readonly int Top = 120;
		public int Right = World.Settings.layerManagerWidth;
		public int Bottom;
		public readonly float HeightPercent = 0.9f;
		private Vector2[] outlineVerts = new Vector2[5];

		public int Width
		{
			get { return Right - Left; }
			set { Right = Left + value; }
		}

		public int Height
		{
			get { return Bottom - Top; }
			set { Bottom = Top + value; }
		}

		#region IMenu Members

		public abstract void OnKeyUp(KeyEventArgs keyEvent);
		public abstract void OnKeyDown(KeyEventArgs keyEvent);
		public abstract bool OnMouseUp(MouseEventArgs e);
		public abstract bool OnMouseDown(MouseEventArgs e);
		public abstract bool OnMouseMove(MouseEventArgs e);
		public abstract bool OnMouseWheel(MouseEventArgs e);
		public void Render(DrawArgs drawArgs)
		{
			this.Bottom = drawArgs.screenHeight-1;

			MenuUtils.DrawBox(Left, Top, Right-Left, Bottom-Top, 0.0f,
				World.Settings.menuBackColor, drawArgs.device);

			RenderContents(drawArgs);

			outlineVerts[0].X = Left;
			outlineVerts[0].Y = Top;

			outlineVerts[1].X = Right;
			outlineVerts[1].Y = Top;

			outlineVerts[2].X = Right;
			outlineVerts[2].Y = Bottom;
		
			outlineVerts[3].X = Left;
			outlineVerts[3].Y = Bottom;

			outlineVerts[4].X = Left;
			outlineVerts[4].Y = Top;

			MenuUtils.DrawLine(outlineVerts, World.Settings.menuOutlineColor, drawArgs.device);
		}

		public abstract void RenderContents(DrawArgs drawArgs);
		public abstract void Dispose();

		#endregion

	}

	public class LayerManagerButton : MenuButton
	{
		World _parentWorld;
		LayerManagerMenu lmm;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.LayerManagerButton"/> class.
		/// </summary>
		/// <param name="iconImagePath"></param>
		/// <param name="parentWorld"></param>
		public LayerManagerButton(
			string iconImagePath,
			World parentWorld)
			: base(iconImagePath)
		{
			this._parentWorld = parentWorld;
			this.Description = "Layer Manager";
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		public override bool IsPushed()
		{
			return World.Settings.showLayerManager;
		}

		public override void Update(DrawArgs drawArgs)
		{
		}

		public override void OnKeyDown(KeyEventArgs keyEvent)
		{
		}

		public override void OnKeyUp(KeyEventArgs keyEvent)
		{
		}

		public override bool OnMouseDown(MouseEventArgs e)
		{
			if(IsPushed())
				return this.lmm.OnMouseDown(e);
			else 
				return false;
		}

		public override bool OnMouseMove(MouseEventArgs e)
		{
			if(lmm!=null && IsPushed())
				return this.lmm.OnMouseMove(e);
			else	
				return false;
		}

		public override bool OnMouseUp(MouseEventArgs e)
		{
			if(this.IsPushed())
				return this.lmm.OnMouseUp(e);
			else
				return false;
		}

		public override bool OnMouseWheel(MouseEventArgs e)
		{
			if(this.IsPushed())
				return this.lmm.OnMouseWheel(e);
			else
				return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(IsPushed())
			{
				if(lmm == null)
					lmm = new LayerManagerMenu(_parentWorld, this);

				lmm.Render(drawArgs);
			}
		}

		public override void SetPushed(bool isPushed)
		{
			World.Settings.showLayerManager = isPushed;
		}
	}

	public class LayerManagerMenu : SideBarMenu
	{
		public int DialogColor = System.Drawing.Color.Gray.ToArgb();
		public int TextColor = System.Drawing.Color.White.ToArgb();
		public LayerMenuItem MouseOverItem;
		public int ScrollBarSize = 20;
		public int ItemHeight = 20;
		World _parentWorld;
		MenuButton _parentButton;
		bool showScrollbar;
		int scrollBarPosition;
		float scrollSmoothPosition; // Current position of scroll when smooth scrolling (scrollBarPosition=target)
		int scrollGrabPositionY; // Location mouse grabbed scroll
		bool isResizing;
		bool isScrolling;
		int leftBorder=2;
		int rightBorder=1;
		int topBorder=25;
		int bottomBorder=1;
		Microsoft.DirectX.Direct3D.Font headerFont;
		Microsoft.DirectX.Direct3D.Font itemFont;
		Microsoft.DirectX.Direct3D.Font wingdingsFont;
		Microsoft.DirectX.Direct3D.Font worldwinddingsFont;
		ArrayList _itemList = new ArrayList();
		Microsoft.DirectX.Vector2[] scrollbarLine = new Vector2[2];	
		public ContextMenu ContextMenu;

		/// <summary>
		/// Client area X position of left side
		/// </summary>
		public int ClientLeft
		{
			get
			{
				return Left + leftBorder;
			}
		}

		/// <summary>
		/// Client area X position of right side
		/// </summary>
		public int ClientRight
		{
			get
			{
				int res = Right - rightBorder;
				if(showScrollbar)
					res -= ScrollBarSize;
				return res;
			}
		}

		/// <summary>
		/// Client area Y position of top side
		/// </summary>
		public int ClientTop
		{
			get
			{
				return Top + topBorder + 1;
			}
		}

		/// <summary>
		/// Client area Y position of bottom side
		/// </summary>
		public int ClientBottom
		{
			get
			{
				return Bottom - bottomBorder;
			}
		}

		/// <summary>
		/// Client area width
		/// </summary>
		public int ClientWidth
		{
			get
			{
				int res = Right - rightBorder - Left - leftBorder;
				if(showScrollbar)
					res -= ScrollBarSize;
				return res;
			}
		}

		/// <summary>
		/// Client area height
		/// </summary>
		public int ClientHeight
		{
			get
			{
				int res = Bottom - bottomBorder - Top - topBorder - 1;
				return res;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.LayerManagerMenu"/> class.
		/// </summary>
		/// <param name="parentWorld"></param>
		/// <param name="parentButton"></param>
		public LayerManagerMenu(World parentWorld, MenuButton parentButton)
		{
			this._parentWorld = parentWorld;
			this._parentButton = parentButton;			
		}

		public override void OnKeyDown(KeyEventArgs keyEvent)
		{
		}

		public override void Dispose()
		{
		}

		public override void OnKeyUp(KeyEventArgs keyEvent)
		{
		}

		public override bool OnMouseWheel(MouseEventArgs e)
		{
			if(e.X > this.Right || e.X < this.Left  || e.Y < this.Top || e.Y > this.Bottom)
				// Outside
				return false;

			// Mouse wheel scroll
			this.scrollBarPosition -= (e.Delta/6);
			return true;
		}

		public override bool OnMouseDown(MouseEventArgs e)
		{
			if(e.X > Right || e.X < Left  || e.Y < Top || e.Y > Bottom)
				// Outside
				return false;
			
			if(e.X > this.Right - 5 && e.X < this.Right + 5)
			{
				this.isResizing = true;
				return true;
			}

			if(e.Y < ClientTop)
				return false;

			if(e.X > this.Right - ScrollBarSize)
			{
				int numItems = GetNumberOfUncollapsedItems();
				int totalHeight = GetItemsHeight(m_DrawArgs);
				if(totalHeight > ClientHeight)
				{
					//int totalHeight = numItems * ItemHeight;
					double percentHeight = (double)ClientHeight / totalHeight;
					if(percentHeight > 1)
						percentHeight = 1;

					double scrollItemHeight = (double)percentHeight * ClientHeight;
					int scrollPosition = ClientTop + (int)(scrollBarPosition * percentHeight);
					if(e.Y < scrollPosition)
						scrollBarPosition -= ClientHeight;
					else if(e.Y > scrollPosition + scrollItemHeight)
						scrollBarPosition += ClientHeight;
					else
					{
						scrollGrabPositionY = e.Y - scrollPosition;
						isScrolling = true;
					}
				}
			}

			return true;
		}

		DrawArgs m_DrawArgs = null;

		public override bool OnMouseMove(MouseEventArgs e)
		{
			// Reset mouse over effect since mouse moved.
			MouseOverItem = null;

			if(this.isResizing)
			{
				if(e.X > 140 && e.X < 800)
					this.Width = e.X;
				
				return true;
			}

			if(this.isScrolling)
			{
				int totalHeight = GetItemsHeight(m_DrawArgs);//GetNumberOfUncollapsedItems() * ItemHeight;
				double percent = (double)totalHeight/ClientHeight;
				scrollBarPosition = (int)((e.Y - scrollGrabPositionY - ClientTop) * percent);
				return true;
			}
			
			if(e.X > this.Right || e.X < this.Left  || e.Y < this.Top || e.Y > this.Bottom)
				// Outside
				return false;
			
			if(Math.Abs(e.X - this.Right ) < 5 )
			{
				DrawArgs.MouseCursor = CursorType.SizeWE;
				return true;
			}

			if(e.X > ClientRight)
				return true;

			foreach(LayerMenuItem lmi in this._itemList)
				if(lmi.OnMouseMove(e))
					return true;
			
			// Handled
			return true;
		}

		public override bool OnMouseUp(MouseEventArgs e)
		{
			if(this.isResizing)
			{
				this.isResizing = false;
				return true;
			}

			if(this.isScrolling)
			{
				this.isScrolling = false;
				return true;
			}

			foreach(LayerMenuItem lmi in this._itemList)
			{
				if(lmi.OnMouseUp(e))
					return true;
			}
			
			if(e.X > this.Right - 20 && e.X < this.Right &&
				e.Y > this.Top && e.Y < this.Top + topBorder)
			{
				this._parentButton.SetPushed(false);
				return true;
			}
			else if(e.X > 0 && e.X < this.Right && e.Y > 0 && e.Y < this.Bottom)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Displays the layer manager context menu for an item.
		/// </summary>
		/// <param name="ro"></param>
		public void ShowContextMenu( int x, int y, LayerMenuItem item )
		{
			if(ContextMenu!=null)
			{
				ContextMenu.Dispose();
				ContextMenu = null;
			}
			ContextMenu = new ContextMenu();
			item.RenderableObject.BuildContextMenu(ContextMenu);
			ContextMenu.Show(item.ParentControl, new System.Drawing.Point(x,y));
		}

		/// <summary>
		/// Calculate the number of un-collapsed items in the tree.
		/// </summary>
		public int GetNumberOfUncollapsedItems()
		{
			int numItems = 1;
			foreach(LayerMenuItem subItem in _itemList)
				numItems += subItem.GetNumberOfUncollapsedItems();

			return numItems;
		}

		public int GetItemsHeight(DrawArgs drawArgs)
		{
			int height = 20;
			foreach(LayerMenuItem subItem in _itemList)
				height += subItem.GetItemsHeight(drawArgs);

			return height;
		}

		private void updateList()
		{
			if(this._parentWorld != null && this._parentWorld.RenderableObjects != null)
			{
				for(int i = 0; i < this._parentWorld.RenderableObjects.ChildObjects.Count; i++)
				{
					RenderableObject curObject = (RenderableObject)this._parentWorld.RenderableObjects.ChildObjects[i];
				
					if(i >= this._itemList.Count)
					{
						LayerMenuItem newItem = new LayerMenuItem(this, curObject);
						this._itemList.Add(newItem);
					}
					else
					{
						LayerMenuItem curItem = (LayerMenuItem)this._itemList[i];
						if(!curItem.RenderableObject.Name.Equals(curObject.Name))
						{
							this._itemList.Insert(i, new LayerMenuItem(this, curObject));
						}
					}
				}

				int extraItems = this._itemList.Count - this._parentWorld.RenderableObjects.ChildObjects.Count;
				this._itemList.RemoveRange(this._parentWorld.RenderableObjects.ChildObjects.Count, extraItems);
			}
			else
			{
				this._itemList.Clear();
			}
		}

		public override void RenderContents(DrawArgs drawArgs)
		{
			m_DrawArgs = drawArgs;
			try
			{
				if(itemFont == null)
				{
					itemFont = drawArgs.CreateFont( World.Settings.LayerManagerFontName, 
						World.Settings.LayerManagerFontSize, World.Settings.LayerManagerFontStyle );

					// TODO: Fix wingdings menu problems
					System.Drawing.Font localHeaderFont = new System.Drawing.Font("Arial", 12.0f, FontStyle.Italic | FontStyle.Bold);
					headerFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, localHeaderFont);

					System.Drawing.Font wingdings = new System.Drawing.Font("Wingdings", 12.0f);
					wingdingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, wingdings);

					AddFontResource(Path.Combine(Application.StartupPath, "World Wind Dings 1.04.ttf"));
					System.Drawing.Text.PrivateFontCollection fpc = new System.Drawing.Text.PrivateFontCollection();
					fpc.AddFontFile(Path.Combine(Application.StartupPath, "World Wind Dings 1.04.ttf"));
					System.Drawing.Font worldwinddings = new System.Drawing.Font(fpc.Families[0], 12.0f);
					worldwinddingsFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, worldwinddings);
				}

				this.updateList();
				
				this.worldwinddingsFont.DrawText(
					null,
					"E",
					new System.Drawing.Rectangle(this.Right - 16, this.Top + 2, 20, topBorder),
					DrawTextFormat.None,
					TextColor);

				int numItems = GetNumberOfUncollapsedItems();
				int totalHeight = GetItemsHeight(drawArgs);//numItems * ItemHeight;
				showScrollbar = totalHeight > ClientHeight;
				if(showScrollbar)
				{
					double percentHeight = (double)ClientHeight / totalHeight;
					int scrollbarHeight = (int)(ClientHeight * percentHeight);

					int maxScroll = totalHeight-ClientHeight;

					if(scrollBarPosition < 0)
						scrollBarPosition = 0;
					else if(scrollBarPosition > maxScroll)
						scrollBarPosition = maxScroll;

					// Smooth scroll
					const float scrollSpeed = 0.3f;
					float smoothScrollDelta = (scrollBarPosition - scrollSmoothPosition)*scrollSpeed;
					float absDelta = Math.Abs(smoothScrollDelta);
					if(absDelta > 100f || absDelta < 3f) 
						// Scroll > 100 pixels and < 1.5 pixels faster
						smoothScrollDelta = (scrollBarPosition - scrollSmoothPosition)*(float)Math.Sqrt(scrollSpeed);
					
					scrollSmoothPosition += smoothScrollDelta;

					if(scrollSmoothPosition > maxScroll)
						scrollSmoothPosition = maxScroll;

					int scrollPos = (int)((float)percentHeight * scrollBarPosition );

					int color = isScrolling ? World.Settings.scrollbarHotColor : World.Settings.scrollbarColor;
					MenuUtils.DrawBox(
						Right - ScrollBarSize + 2,
						ClientTop + scrollPos,
						ScrollBarSize - 3,
						scrollbarHeight + 1,
						0.0f,
						color,
						drawArgs.device);

					scrollbarLine[0].X = this.Right - ScrollBarSize;
					scrollbarLine[0].Y = this.ClientTop;
					scrollbarLine[1].X = this.Right - ScrollBarSize;
					scrollbarLine[1].Y = this.Bottom;
					MenuUtils.DrawLine(scrollbarLine, 
						DialogColor,
						drawArgs.device);
				}

				this.headerFont.DrawText(
					null, "Layer Manager",
					new System.Drawing.Rectangle( Left+5, Top+1, Width, topBorder-2 ),
					DrawTextFormat.VerticalCenter, TextColor);

				Microsoft.DirectX.Vector2[] headerLinePoints = new Microsoft.DirectX.Vector2[2];
				headerLinePoints[0].X = this.Left;
				headerLinePoints[0].Y = this.Top + topBorder - 1;

				headerLinePoints[1].X = this.Right;
				headerLinePoints[1].Y = this.Top + topBorder - 1;

				MenuUtils.DrawLine(headerLinePoints, DialogColor, drawArgs.device);

				int runningItemHeight = 0;
				if( showScrollbar )
					runningItemHeight = -(int)Math.Round(scrollSmoothPosition);

				// Set the Direct3D viewport to match the layer manager client area
				// to clip the text to the window when scrolling
				Viewport lmClientAreaViewPort = new Viewport();
				lmClientAreaViewPort.X = ClientLeft;
				lmClientAreaViewPort.Y = ClientTop;
				lmClientAreaViewPort.Width = ClientWidth;
				lmClientAreaViewPort.Height = ClientHeight;
				Viewport defaultViewPort = drawArgs.device.Viewport;
				drawArgs.device.Viewport = lmClientAreaViewPort;
				for(int i = 0; i < _itemList.Count; i++)
				{
					if(runningItemHeight > ClientHeight)
						// No more space for items
						break;
					LayerMenuItem lmi = (LayerMenuItem)_itemList[i];
					runningItemHeight += lmi.Render(
						drawArgs, 
						ClientLeft,
						ClientTop,
						runningItemHeight,
						ClientWidth,
						ClientBottom,
						itemFont,
						wingdingsFont,
						worldwinddingsFont, 
						MouseOverItem);
				}
				drawArgs.device.Viewport = defaultViewPort;
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
		}

		[DllImport("gdi32.dll")]
		static extern int AddFontResource(string lpszFilename);
	}

		public class LayerMenuItem
		{
			RenderableObject m_renderableObject;
			ArrayList m_subItems = new ArrayList();
			private int _x;
			private int _y;
			private int _width;

			private int _itemXOffset = 5;
			private int _expandArrowXSize = 15;
			private int _checkBoxXOffset = 15;
			private int _subItemXIndent = 15;

			int itemOnColor = Color.White.ToArgb();
			int itemOffColor = Color.Gray.ToArgb();

			private bool isExpanded;
			public Control ParentControl;
			LayerManagerMenu m_parent; // menu this item belongs in

			public RenderableObject RenderableObject
			{
				get
				{
					return m_renderableObject;
				}
			}

			/// <summary>
			/// Calculate the number of un-collapsed items in the tree.
			/// </summary>
			public int GetNumberOfUncollapsedItems()
			{
				int numItems = 1;
				if(this.isExpanded)
				{
					foreach(LayerMenuItem subItem in m_subItems)
						numItems += subItem.GetNumberOfUncollapsedItems();
				}

				return numItems;
			}

			public int GetItemsHeight(DrawArgs drawArgs)
			{
				System.Drawing.Rectangle rect = drawArgs.defaultDrawingFont.MeasureString(
					null,
					this.m_renderableObject.Name, DrawTextFormat.None, System.Drawing.Color.White.ToArgb());

				int height = rect.Height;
				
				if(m_renderableObject.Description != null && m_renderableObject.Description.Length > 0)
				{
					System.Drawing.SizeF rectF = DrawArgs.Graphics.MeasureString(
						m_renderableObject.Description,
						drawArgs.defaultSubTitleFont,
						_width - (this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset)
						);

					height += (int)rectF.Height + 15;
				}

				if(height < lastConsumedHeight)
					height = lastConsumedHeight;

				if(this.isExpanded)
				{
					foreach(LayerMenuItem subItem in m_subItems)
						height += subItem.GetItemsHeight(drawArgs);
				}
				
				return height;
			}


			private string getFullRenderableObjectName(RenderableObject ro, string name)
			{
				if(ro.ParentList == null)
					return "/" + name;
				else
				{
					if(name == null)
						return getFullRenderableObjectName(ro.ParentList, ro.Name);
					else
						return getFullRenderableObjectName(ro.ParentList, ro.Name + "/" + name);
				}
			}


			/// <summary>
			/// Detect expand arrow mouse over
			/// </summary>
			public bool OnMouseMove(MouseEventArgs e)
			{
				if(e.Y < this._y)
					// Over 
					return false;

				if(e.X < m_parent.Left || e.X > m_parent.Right)
					return false;

				if(e.Y < this._y + 20)
				{
					// Mouse is on item
					m_parent.MouseOverItem = this;

					if(e.X > this._x + this._itemXOffset && 
						e.X < this._x + (this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset))
					{
						if(m_renderableObject is WorldWind.Renderable.RenderableObjectList)
							DrawArgs.MouseCursor = CursorType.Hand;
						return true;
					}
					return false;
				}

				foreach(LayerMenuItem lmi in m_subItems)
				{
					if(lmi.OnMouseMove(e))
					{
						// Mouse is on current item
						m_parent.MouseOverItem = lmi;
						return true;
					}
				}

				return false;
			}

			public bool OnMouseUp(MouseEventArgs e)
			{
				if(e.Y < this._y)
					// Above 
					return false;

				if(e.Y <= this._y + 20)
				{
					if(e.X > this._x + this._itemXOffset &&
						e.X < this._x + (this._itemXOffset + this._width) &&
						e.Button == MouseButtons.Right)
					{
						m_parent.ShowContextMenu( e.X, e.Y, this );
					}

					if(e.X > this._x + this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset &&
						e.X < this._x + (this._itemXOffset + this._width) &&
						e.Button == MouseButtons.Left &&
						m_renderableObject != null &&
						m_renderableObject.MetaData.Contains("InfoUri"))
					{
						string infoUri = (string)m_renderableObject.MetaData["InfoUri"];

						if (World.Settings.UseInternalBrowser)
						{
							MessageBox.Show(this.ParentControl.Parent.Parent.ToString());
							SplitContainer sc = (SplitContainer)this.ParentControl.Parent.Parent;
							InternalWebBrowserPanel browser = (InternalWebBrowserPanel)sc.Panel1.Controls[0];
							browser.NavigateTo(infoUri);
						}
						else
						{
							ProcessStartInfo psi = new ProcessStartInfo();
							psi.FileName = infoUri;
							psi.Verb = "open";
							psi.UseShellExecute = true;
							psi.CreateNoWindow = true;
							Process.Start(psi);
						}
					}

					if(e.X > this._x + this._itemXOffset && 
						e.X < this._x + (this._itemXOffset + this._expandArrowXSize) &&
						m_renderableObject is WorldWind.Renderable.RenderableObjectList)
					{
						WorldWind.Renderable.RenderableObjectList rol = (WorldWind.Renderable.RenderableObjectList)m_renderableObject;
						if(!rol.DisableExpansion)
						{
							this.isExpanded = !this.isExpanded;
							return true;
						}
					}
				
					if(e.X > this._x + this._itemXOffset + this._expandArrowXSize && 
						e.X < this._x + (this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset) )
					{
						if(!m_renderableObject.IsOn && m_renderableObject.ParentList != null && 
							m_renderableObject.ParentList.ShowOnlyOneLayer)
							m_renderableObject.ParentList.TurnOffAllChildren();

						m_renderableObject.IsOn = !m_renderableObject.IsOn;
						return true;
					}
				}

				if(isExpanded)
				{
					foreach(LayerMenuItem lmi in m_subItems)
					{
						if(lmi.OnMouseUp(e))
							return true;
					}
				}
				
				return false;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.LayerMenuItem"/> class.
			/// </summary>
			/// <param name="parent"></param>
			/// <param name="renderableObject"></param>
			public LayerMenuItem(LayerManagerMenu parent, RenderableObject renderableObject)
			{
				m_renderableObject = renderableObject;
				m_parent = parent;
			}

			private void updateList()
			{
				if(this.isExpanded)
				{
					RenderableObjectList rol = (RenderableObjectList)m_renderableObject;
					for(int i = 0; i < rol.ChildObjects.Count; i++)
					{
						RenderableObject childObject = (RenderableObject)rol.ChildObjects[i];
						if(i >= m_subItems.Count)
						{
							LayerMenuItem newItem = new LayerMenuItem(m_parent, childObject);
							m_subItems.Add(newItem);
						}
						else
						{
							LayerMenuItem curItem = (LayerMenuItem)m_subItems[i];

							if(curItem != null && curItem.RenderableObject != null && 
								childObject != null &&
								!curItem.RenderableObject.Name.Equals(childObject.Name))
							{
								m_subItems.Insert(i, new LayerMenuItem(m_parent, childObject));
							}
						}
					}

					int extraItems = m_subItems.Count - rol.ChildObjects.Count;
					if(extraItems > 0)
						m_subItems.RemoveRange(rol.ChildObjects.Count, extraItems);
				}
			}

			int lastConsumedHeight = 20;
			
			public int Render(DrawArgs drawArgs, int x, int y, int yOffset, int width, int height, 
				Microsoft.DirectX.Direct3D.Font drawingFont,
				Microsoft.DirectX.Direct3D.Font wingdingsFont,
				Microsoft.DirectX.Direct3D.Font worldwinddingsFont, 
				LayerMenuItem mouseOverItem)
			{
				if(ParentControl == null)
					ParentControl = drawArgs.parentControl;

				this._x = x;
				this._y = y + yOffset;
				this._width = width;

				int consumedHeight = 20;
				
				System.Drawing.Rectangle textRect = drawingFont.MeasureString(null,
					m_renderableObject.Name,
					DrawTextFormat.None,
					System.Drawing.Color.White.ToArgb());

				consumedHeight = textRect.Height;

				if (m_renderableObject.Description != null && m_renderableObject.Description.Length > 0 && !(m_renderableObject is WorldWind.Renderable.Icon))
				{
					System.Drawing.SizeF rectF = DrawArgs.Graphics.MeasureString(
						m_renderableObject.Description,
						drawArgs.defaultSubTitleFont,
						width - (this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset)
						);
				
					consumedHeight += (int)rectF.Height + 15;
				}

				lastConsumedHeight = consumedHeight;
				// Layer manager client area height
				int totalHeight = height - y;

				updateList();

				if(yOffset >= -consumedHeight)
				{
					// Part of item or whole item visible
					int color = m_renderableObject.IsOn ? itemOnColor : itemOffColor;
					if(mouseOverItem==this)
					{
						if(!m_renderableObject.IsOn)
							// mouseover + inactive color (black)
							color = 0xff << 24;
						MenuUtils.DrawBox(m_parent.ClientLeft,_y,m_parent.ClientWidth,consumedHeight,0,
							World.Settings.menuOutlineColor, drawArgs.device);
					}

					if(m_renderableObject is WorldWind.Renderable.RenderableObjectList)
					{
						RenderableObjectList rol = (RenderableObjectList)m_renderableObject;
						if(!rol.DisableExpansion)
						{
							worldwinddingsFont.DrawText(
								null,
								(this.isExpanded ? "L" : "A"),
								new System.Drawing.Rectangle(x + this._itemXOffset, _y, this._expandArrowXSize, height),
								DrawTextFormat.None,
								color );
						}
					}

					string checkSymbol = null;
					if(m_renderableObject.ParentList != null && m_renderableObject.ParentList.ShowOnlyOneLayer)
						// Radio check
						checkSymbol = m_renderableObject.IsOn ? "O" : "P";
					else				
						// Normal check
						checkSymbol = m_renderableObject.IsOn ? "N" : "F";

					worldwinddingsFont.DrawText(
							null,
							checkSymbol,
							new System.Drawing.Rectangle(
							x + this._itemXOffset + this._expandArrowXSize,
							_y,
							this._checkBoxXOffset,
							height),
							DrawTextFormat.NoClip,
							color );


					drawingFont.DrawText(
						null,
						m_renderableObject.Name,
						new System.Drawing.Rectangle(
						x + this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset,
						_y,
						width - (this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset),
						height),
						DrawTextFormat.None,
						color );

					if(m_renderableObject.Description != null && m_renderableObject.Description.Length > 0 && !(m_renderableObject is WorldWind.Renderable.Icon))
					{
						drawArgs.defaultSubTitleDrawingFont.DrawText(
							null,
							m_renderableObject.Description,
							new System.Drawing.Rectangle(
								x + this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset,
								_y + textRect.Height,
								width - (_itemXOffset + _expandArrowXSize + _checkBoxXOffset),
								height),
							DrawTextFormat.WordBreak,
							System.Drawing.Color.Gray.ToArgb());
					}

					if(m_renderableObject.MetaData.Contains("InfoUri"))
					{
						Vector2[] underlineVerts = new Vector2[2];
						underlineVerts[0].X = x + this._itemXOffset + this._expandArrowXSize + this._checkBoxXOffset;
						underlineVerts[0].Y = _y + textRect.Height;
						underlineVerts[1].X = underlineVerts[0].X + textRect.Width;
						underlineVerts[1].Y = _y + textRect.Height;

						MenuUtils.DrawLine(underlineVerts, color, drawArgs.device);
					}
				}
				
				if(isExpanded)
				{
					for(int i = 0; i < m_subItems.Count; i++)
					{
						int yRealOffset = yOffset + consumedHeight;
						if(yRealOffset > totalHeight)
							// No more space for items
							break;
						LayerMenuItem lmi = (LayerMenuItem)m_subItems[i];
						consumedHeight += lmi.Render(
							drawArgs,
							x + _subItemXIndent,
							y,
							yRealOffset,
							width - _subItemXIndent,
							height,
							drawingFont,
							wingdingsFont,
							worldwinddingsFont,
							mouseOverItem );
					}
				}

				return consumedHeight;
			}
		}

	public class LayerShortcutMenuButton : MenuButton
	{
		#region Private Members
		bool _isPushed = false;
		WorldWind.Renderable.RenderableObject _ro;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.LayerShortcutMenuButton"/> class.
		/// </summary>
		/// <param name="imageFilePath"></param>
		/// <param name="ro"></param>
		public LayerShortcutMenuButton(
			string imageFilePath, WorldWind.Renderable.RenderableObject ro)
			: base(imageFilePath)
		{
			this.Description = ro.Name;
			this._ro = ro;
			this._isPushed = ro.IsOn;
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		public override bool IsPushed()
		{
			return this._isPushed;
		}

		public override void SetPushed(bool isPushed)
		{
			this._isPushed = isPushed;
			if(!this._ro.IsOn && this._ro.ParentList != null && this._ro.ParentList.ShowOnlyOneLayer)
				this._ro.ParentList.TurnOffAllChildren();

			this._ro.IsOn = this._isPushed;

			//HACK: Temporary fix
			if( _ro.Name=="Placenames" )
				World.Settings.showPlacenames = isPushed;
			else if( _ro.Name=="Boundaries" )
				World.Settings.showBoundaries = isPushed;
		}

		public override void OnKeyDown(KeyEventArgs keyEvent)
		{
		}

		public override void OnKeyUp(KeyEventArgs keyEvent)
		{

		}
		public override void Update(DrawArgs drawArgs)
		{
			if(this._ro.IsOn != this._isPushed)
				this._isPushed = this._ro.IsOn;
		}

		public override void Render(DrawArgs drawArgs)
		{
		}

		public override bool OnMouseDown(MouseEventArgs e)
		{
			return false;
		}

		public override bool OnMouseMove(MouseEventArgs e)
		{
			return false;
		}

		public override bool OnMouseUp(MouseEventArgs e)
		{
			return false;
		}

		public override bool OnMouseWheel(MouseEventArgs e)
		{
			return false;
		}
	}

	/// <summary>
	/// Toolbar
	/// </summary>
	public class MenuBar : IMenu
	{
		#region Private Members
		protected ArrayList m_toolsMenuButtons = new ArrayList();
		protected ArrayList m_layersMenuButtons = new ArrayList();
		protected VisibleState _visibleState = VisibleState.Visible;
		protected DateTime _lastVisibleChange = DateTime.Now;
		protected float _outerPadding = 5;
		protected int x;
		protected int y;
		protected int hideTimeMilliseconds = 100;
		protected MenuAnchor m_anchor = MenuAnchor.Left;
		protected bool _isHideable;
		protected const float padRatio = 1/9.0f;
		protected CursorType mouseCursor;
		protected int chevronColor =  Color.Black.ToArgb();
		protected CustomVertex.TransformedColored[] enabledChevron = new CustomVertex.TransformedColored[3];
		protected Sprite m_sprite;


		#endregion

		#region Properties

		/// <summary>
		/// Indicates whether the menu is "open". (user activity)
		/// </summary>
		public bool IsActive
		{
			get
			{
				return (this._curSelection>=0);
			}
		}

		public System.Collections.ArrayList LayersMenuButtons
		{
			get
			{
				return m_layersMenuButtons;
			}
			set
			{
				m_layersMenuButtons = value;
			}
		}

		public System.Collections.ArrayList ToolsMenuButtons
		{
			get
			{
				return m_toolsMenuButtons;
			}
			set
			{
				m_toolsMenuButtons = value;
			}
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.MenuBar"/> class.
		/// </summary>
		/// <param name="anchor"></param>
		/// <param name="iconSize"></param>
		public MenuBar(MenuAnchor anchor, int iconSize)
		{
			m_anchor = anchor;
			MenuButton.SelectedSize = iconSize;
		}

		/// <summary>
		/// Adds a tool button to the bar
		/// </summary>
		public void AddToolsMenuButton(MenuButton button)
		{
			lock(m_toolsMenuButtons.SyncRoot)
			{
				m_toolsMenuButtons.Add(button);
			}
		}

		/// <summary>
		/// Adds a tool button to the bar
		/// </summary>
		public void AddToolsMenuButton(MenuButton button, int index)
		{
			lock(m_toolsMenuButtons.SyncRoot)
			{
				if(index < 0)
					m_toolsMenuButtons.Insert(0, button);
				else if(index >= m_toolsMenuButtons.Count)
					m_toolsMenuButtons.Add(button);
				else
					m_toolsMenuButtons.Insert(index, button);
			}
		}

		/// <summary>
		/// Removes a layer button from the bar if it is found.
		/// </summary>
		public void RemoveToolsMenuButton(MenuButton button)
		{
			lock(m_toolsMenuButtons.SyncRoot)
			{
				m_toolsMenuButtons.Remove(button);
			}
		}

		/// <summary>
		/// Adds a layer button to the bar
		/// </summary>
		public void AddLayersMenuButton(MenuButton button)
		{
			lock(m_layersMenuButtons.SyncRoot)
			{
				m_layersMenuButtons.Add(button);
			}
		}

		/// <summary>
		/// Adds a layer button to the bar
		/// </summary>
		public void AddLayersMenuButton(MenuButton button, int index)
		{
			lock(m_layersMenuButtons.SyncRoot)
			{
				if(index < m_layersMenuButtons.Count)
					m_layersMenuButtons.Insert(0, button);
				else if(index >= m_layersMenuButtons.Count)
					m_layersMenuButtons.Add(button);
				else
					m_layersMenuButtons.Insert(index, button);
			}
		}

		/// <summary>
		/// Removes a layer button from the bar if it is found.
		/// </summary>
		public void RemoveLayersMenuButton(MenuButton button)
		{
			lock(m_layersMenuButtons.SyncRoot)
			{
				m_layersMenuButtons.Remove(button);
			}
		}

		#region IMenu Members

		public void OnKeyUp(KeyEventArgs keyEvent)
		{
			// TODO:  Add ToolsMenuBar.OnKeyUp implementation
		}

		public void OnKeyDown(KeyEventArgs keyEvent)
		{
			// TODO:  Add ToolsMenuBar.OnKeyDown implementation
		}

		public bool OnMouseUp(MouseEventArgs e)
		{
			if(World.Settings.showToolbar)
			{
				if(this._curSelection != -1 && e.Button == MouseButtons.Left)
				{
					if(this._curSelection < m_toolsMenuButtons.Count)
					{
						MenuButton button = (MenuButton)m_toolsMenuButtons[this._curSelection];
						button.SetPushed(!button.IsPushed());
					}
					else
					{
						MenuButton button = (MenuButton)m_layersMenuButtons[this._curSelection - m_toolsMenuButtons.Count];
						button.SetPushed(!button.IsPushed());
					}

					return true;
				}
			}

			// Pass message on to the "tools"
			foreach(MenuButton button in m_toolsMenuButtons)
				if(button.IsPushed())
					if(button.OnMouseUp(e))
						return true;

			return false;
		}

		public bool OnMouseDown(MouseEventArgs e)
		{
			// Trigger "tool" update
			foreach(MenuButton button in m_toolsMenuButtons)
				if(button.IsPushed())
					if(button.OnMouseDown(e))
						return true;
			
			return false;
		}

		int _curSelection = -1;

		public bool OnMouseMove(MouseEventArgs e)
		{
			// Default to arrow cursor every time mouse moves
			mouseCursor = CursorType.Arrow;

			// Trigger "tools" update
			foreach(MenuButton button in m_toolsMenuButtons)
				if(button.IsPushed())
					if(button.OnMouseMove(e))
						return true;

			if(!World.Settings.showToolbar)
				return false;

			if(this._visibleState == VisibleState.Visible)
			{
				float width, height;

				switch(m_anchor)
				{
					case MenuAnchor.Top:
						int buttonCount = m_toolsMenuButtons.Count + m_layersMenuButtons.Count;
						width =  buttonCount * (_outerPadding+MenuButton.NormalSize)+_outerPadding;
						height = _outerPadding * 2 + MenuButton.NormalSize;

						int sel=-1;
						if(e.Y >= y && e.Y <= y + height + 2*_outerPadding)
						{
							sel = (int) ((e.X-_outerPadding) / (MenuButton.NormalSize+_outerPadding));
							if(sel<buttonCount)
								mouseCursor = CursorType.Hand;
							else
								sel = -1;
						}
						_curSelection = sel;

						break;

					case MenuAnchor.Bottom:
						width = _outerPadding * 2 + (m_toolsMenuButtons.Count * m_layersMenuButtons.Count) * MenuButton.SelectedSize;
						height = _outerPadding * 2 + MenuButton.SelectedSize;

						if(e.X >= x + _outerPadding && e.X <=  x + width + _outerPadding &&
							e.Y >= y + _outerPadding && e.Y <= y + height + _outerPadding)
						{
							int dx = (int)(e.X - (x + _outerPadding));
							_curSelection = (int)(dx / MenuButton.SelectedSize);
						}
						else
							_curSelection = -1;
						break;
					
					case MenuAnchor.Right:
						width = _outerPadding * 2 + MenuButton.SelectedSize;
						height = _outerPadding * 2 + (m_toolsMenuButtons.Count * m_layersMenuButtons.Count) * MenuButton.SelectedSize;

						if(e.X >= x + _outerPadding && e.X <= x + width + _outerPadding &&
							e.Y >= y + _outerPadding && e.Y <= y + height + _outerPadding)
						{
							int dx = (int)(e.Y - (y + _outerPadding));
							_curSelection = (int)(dx / MenuButton.SelectedSize);
						}
						else
						{
							_curSelection = -1;
						}
						break;
				}
			}

			return false;
		}

		public bool OnMouseWheel(MouseEventArgs e)
		{
			// Trigger "tool" update
			foreach(MenuButton button in m_toolsMenuButtons)
				if(button.IsPushed())
					if(button.OnMouseWheel(e))
						return true;

			return false;
		}

		public void Render(DrawArgs drawArgs)
		{
			if(m_sprite==null)
				m_sprite = new Sprite(drawArgs.device);

			if(mouseCursor!=CursorType.Arrow)
				DrawArgs.MouseCursor = mouseCursor;


			foreach(MenuButton button in m_toolsMenuButtons)
				if(button.IsPushed())
					// Does not render the button, but the functionality behind the button
					button.Render(drawArgs);

			foreach(MenuButton button in m_toolsMenuButtons)
				button.Update(drawArgs);

			foreach(MenuButton button in m_layersMenuButtons)
				button.Update(drawArgs);

			if(!World.Settings.showToolbar)
				return;

			if(this._isHideable)
			{
				if(this._visibleState == VisibleState.NotVisible)
				{
					if(
						(m_anchor == MenuAnchor.Top && DrawArgs.LastMousePosition.Y < MenuButton.NormalSize) ||
						(m_anchor == MenuAnchor.Bottom && DrawArgs.LastMousePosition.Y > drawArgs.screenHeight - MenuButton.NormalSize) ||
						(m_anchor == MenuAnchor.Right && DrawArgs.LastMousePosition.X > drawArgs.screenWidth - MenuButton.NormalSize)
						)
					{
						this._visibleState = VisibleState.Ascending;
						this._lastVisibleChange = System.DateTime.Now;
					}
				}
				else if(
					(m_anchor == MenuAnchor.Top && DrawArgs.LastMousePosition.Y > 2 * this._outerPadding + MenuButton.NormalSize) ||
					(m_anchor == MenuAnchor.Bottom && DrawArgs.LastMousePosition.Y < drawArgs.screenHeight - 2 * this._outerPadding - MenuButton.NormalSize) ||
					(m_anchor == MenuAnchor.Right && DrawArgs.LastMousePosition.X < drawArgs.screenWidth - MenuButton.NormalSize)
					)
				{
					if(this._visibleState == VisibleState.Visible)
					{
						this._visibleState = VisibleState.Descending;
						this._lastVisibleChange = System.DateTime.Now;
					}
					else if(this._visibleState == VisibleState.Descending)
					{
						if(System.DateTime.Now.Subtract(this._lastVisibleChange) > System.TimeSpan.FromMilliseconds(hideTimeMilliseconds))
						{
							this._visibleState = VisibleState.NotVisible;
							this._lastVisibleChange = System.DateTime.Now;
						}
					}
				}
				else if(this._visibleState == VisibleState.Ascending)
				{
					if(System.DateTime.Now.Subtract(this._lastVisibleChange) > System.TimeSpan.FromMilliseconds(hideTimeMilliseconds))
					{
						this._visibleState = VisibleState.Visible;
						this._lastVisibleChange = System.DateTime.Now;
					}
				}
				else if(this._visibleState == VisibleState.Descending)
				{
					if(System.DateTime.Now.Subtract(this._lastVisibleChange) > System.TimeSpan.FromMilliseconds(hideTimeMilliseconds))
					{
						this._visibleState = VisibleState.NotVisible;
						this._lastVisibleChange = System.DateTime.Now;
					}
				}
			}
			else
			{
				this._visibleState = VisibleState.Visible;
			}
		
			int totalNumberButtons = m_toolsMenuButtons.Count + m_layersMenuButtons.Count;
			MenuButton.NormalSize  = MenuButton.SelectedSize/2;
			_outerPadding = MenuButton.NormalSize * padRatio;
			
			if(m_anchor == MenuAnchor.Left)
			{
				x = 0;
				y = (int)MenuButton.NormalSize;
			}
			else if(m_anchor == MenuAnchor.Right)
			{
				x = (int)(drawArgs.screenWidth - 2 * _outerPadding - MenuButton.NormalSize);
				y = (int)MenuButton.NormalSize;
			}
			else if(m_anchor == MenuAnchor.Top)
			{
				x = (int)(drawArgs.screenWidth / 2 - totalNumberButtons * MenuButton.NormalSize / 2 - _outerPadding);
				y = 0;
			}
			else if(m_anchor == MenuAnchor.Bottom)
			{
				x = (int)(drawArgs.screenWidth / 2 - totalNumberButtons * MenuButton.NormalSize / 2 - _outerPadding);
				y = (int)(drawArgs.screenHeight - 2 * _outerPadding - MenuButton.SelectedSize);
			}

			if(this._visibleState == VisibleState.Ascending)
			{
				TimeSpan t = System.DateTime.Now.Subtract(this._lastVisibleChange);
				if(t.Milliseconds < hideTimeMilliseconds)
				{
					double percent = (double)t.Milliseconds / hideTimeMilliseconds;
					int dx = (int)((MenuButton.NormalSize + 5) - (percent * (MenuButton.NormalSize + 5)));
					
					if(m_anchor == MenuAnchor.Left)
					{
						x -= dx;
					}
					else if(m_anchor == MenuAnchor.Right)
					{
						x += dx;
					}
					else if(m_anchor == MenuAnchor.Top)
					{
						y -= dx;
					
					}
					else if(m_anchor == MenuAnchor.Bottom)
					{
						y += dx;
					}
				}
			}
			else if(this._visibleState == VisibleState.Descending)
			{
				TimeSpan t = System.DateTime.Now.Subtract(this._lastVisibleChange);
				if(t.Milliseconds < hideTimeMilliseconds)
				{
					double percent = (double)t.Milliseconds / hideTimeMilliseconds;
					int dx = (int)((percent * (MenuButton.NormalSize + 5)));
					
					if(m_anchor == MenuAnchor.Left)
					{
						x -= dx;
					}
					else if(m_anchor == MenuAnchor.Right)
					{
						x += dx;
					}
					else if(m_anchor == MenuAnchor.Top)
					{
						y -= dx;
					
					}
					else if(m_anchor == MenuAnchor.Bottom)
					{
						y += dx;
					}
				}
			}

			lock(m_toolsMenuButtons.SyncRoot)
			{
				MenuButton	selectedButton = null;
				if(_curSelection>=0 & _curSelection < totalNumberButtons)
				{
					if(_curSelection < m_toolsMenuButtons.Count)
						selectedButton = (MenuButton)m_toolsMenuButtons[_curSelection];
					else
						selectedButton = (MenuButton)m_layersMenuButtons[_curSelection - m_toolsMenuButtons.Count];
				}

				_outerPadding = MenuButton.NormalSize*padRatio;
				float menuWidth = (MenuButton.NormalSize+_outerPadding)*totalNumberButtons+_outerPadding;
				if(menuWidth>drawArgs.screenWidth)
				{
					//MessageBox.Show(drawArgs.screenWidth.ToString());
					MenuButton.NormalSize = (drawArgs.screenWidth)/((padRatio+1)*totalNumberButtons+padRatio);
					//MessageBox.Show(MenuButton.NormalSize.ToString());
					_outerPadding = MenuButton.NormalSize*padRatio;
				}

				if(this._visibleState != VisibleState.NotVisible)
				{
					MenuUtils.DrawBox(0, 0, drawArgs.screenWidth, (int)(MenuButton.NormalSize + 2* _outerPadding), 0.0f, 
						World.Settings.toolBarBackColor, drawArgs.device);
				}

				float total = 0;
				float extra = 0;
				for(int i = 0; i < totalNumberButtons; i++)
				{
					MenuButton button;
					if(i < m_toolsMenuButtons.Count)
						button = (MenuButton)m_toolsMenuButtons[i];
					else
						button = (MenuButton)m_layersMenuButtons[i - m_toolsMenuButtons.Count];
					total += button.CurrentSize;
					extra += button.CurrentSize-MenuButton.NormalSize;
				}

				float pad = ((float)_outerPadding*(totalNumberButtons+1) - extra)/(totalNumberButtons+1);
				float buttonX = pad;

				m_sprite.Begin(SpriteFlags.AlphaBlend);
				for(int i = 0; i < totalNumberButtons; i++)
				{
					MenuButton button;
					if(i < m_toolsMenuButtons.Count)
						button = (MenuButton)m_toolsMenuButtons[i];
					else
						button = (MenuButton)m_layersMenuButtons[i - m_toolsMenuButtons.Count];
				
					if(button.IconTexture == null)
						button.InitializeTexture(drawArgs.device);

					if(this._visibleState != VisibleState.NotVisible)
					{
						int centerX = (int)(buttonX+button.CurrentSize*0.5f);
						buttonX += button.CurrentSize + pad;
						float buttonTopY = y + _outerPadding;

						if(button.IsPushed())
						{
							// Draw the chevron
							float chevronSize = button.CurrentSize*padRatio;

							enabledChevron[0].Color = chevronColor;
							enabledChevron[1].Color = chevronColor;
							enabledChevron[2].Color = chevronColor;

							enabledChevron[2].X = centerX - chevronSize;
							enabledChevron[2].Y = y + 2;
							enabledChevron[2].Z = 0.0f;
							
							enabledChevron[0].X = centerX;
							enabledChevron[0].Y = y + 2 + chevronSize;
							enabledChevron[0].Z = 0.0f;
							
							enabledChevron[1].X = centerX + chevronSize;
							enabledChevron[1].Y = y + 2;
							enabledChevron[1].Z = 0.0f;
							
							drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
							drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, enabledChevron);
							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
						}

						button.RenderEnabledIcon(
							m_sprite,
							drawArgs,
							centerX,
							buttonTopY,
							i == this._curSelection);
					}
				}
				m_sprite.End();

			}
		}

		public void Dispose()
		{
			foreach(MenuButton button in m_toolsMenuButtons)
				button.Dispose();

			if(m_sprite!=null)
			{
				m_sprite.Dispose();
				m_sprite = null;
			}
		}

		#endregion

		protected enum VisibleState
		{
			NotVisible,
			Descending,
			Ascending,
			Visible
		}
	}

	public enum MenuAnchor
	{
		Top,
		Bottom,
		Left,
		Right
	}

	public sealed class MenuUtils
	{
		private MenuUtils(){}

		public static void DrawLine(Vector2[] linePoints, int color, Device device)
		{
			CustomVertex.TransformedColored[] lineVerts = new CustomVertex.TransformedColored[linePoints.Length];

			for(int i = 0; i < linePoints.Length; i++)
			{
				lineVerts[i].X = linePoints[i].X;
				lineVerts[i].Y = linePoints[i].Y;
				lineVerts[i].Z = 0.0f;

				lineVerts[i].Color = color;
			}

			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.VertexFormat = CustomVertex.TransformedColored.Format;

			device.DrawUserPrimitives(PrimitiveType.LineStrip, lineVerts.Length - 1, lineVerts);
		}

		public static void DrawBox(int ulx, int uly, int width, int height, float z, int color, Device device)
		{
			CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[4];
			verts[0].X = (float)ulx;
			verts[0].Y = (float)uly;
			verts[0].Z = z;
			verts[0].Color = color;

			verts[1].X = (float)ulx;
			verts[1].Y = (float)uly + height;
			verts[1].Z = z;
			verts[1].Color = color;

			verts[2].X = (float)ulx + width;
			verts[2].Y = (float)uly;
			verts[2].Z = z;
			verts[2].Color = color;

			verts[3].X = (float)ulx + width;
			verts[3].Y = (float)uly + height;
			verts[3].Z = z;
			verts[3].Color = color;

			device.VertexFormat = CustomVertex.TransformedColored.Format;
			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts.Length - 2, verts);
		}

		public static void DrawSector(double startAngle, double endAngle, int centerX, int centerY, int radius, float z, int color, Device device)
		{
			int prec = 7;

			CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[prec + 2];
			verts[0].X = centerX;
			verts[0].Y = centerY;
			verts[0].Z = z;
			verts[0].Color = color;
			double angleInc = (double)(endAngle - startAngle) / prec;

			for(int i = 0; i <= prec; i++)
			{
				verts[i + 1].X = (float)Math.Cos((double)(startAngle + angleInc * i))*radius + centerX;
				verts[i + 1].Y = (float)Math.Sin((double)(startAngle + angleInc * i))*radius*(-1.0f) + centerY;
				verts[i + 1].Z = z;
				verts[i + 1].Color = color;
			}

			device.VertexFormat = CustomVertex.TransformedColored.Format;
			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.DrawUserPrimitives(PrimitiveType.TriangleFan, verts.Length - 2, verts);
		}
	}
}
