using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WorldWind.PluginEngine
{
	/// <summary>
	/// The list view in the plugin dialog.
	/// </summary>
	public class PluginListView : ListView
	{
		ImageList imageList;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.PluginEngine.PluginListView"/> class.
		/// </summary>
		public PluginListView()
		{
			this.View = View.Details;
			this.ResizeRedraw = true;		
		}

		protected override CreateParams CreateParams
		{
			get
			{
				// Override and set owner draw (for checkbox + bitmaps)
				const int LVS_OWNERDRAWFIXED = 0x0400;

				CreateParams cp=base.CreateParams;
				cp.Style|=(int)LVS_OWNERDRAWFIXED;
				return cp;
			}
		}		

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true), SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode=true)]
		protected override void WndProc(ref Message m)
		{
			const int WM_DRAWITEM				=  0x002B;
			const int WM_REFLECT				=  0x2000;
			const int ODS_SELECTED			=  0x0001;

			switch(m.Msg)
			{
				case WM_REFLECT | WM_DRAWITEM:
				{
					// Get the DRAWITEMSTRUCT from the LParam of the message
					NativeMethods.DRAWITEMSTRUCT dis = (NativeMethods.DRAWITEMSTRUCT)Marshal.PtrToStructure(
						m.LParam,typeof(NativeMethods.DRAWITEMSTRUCT));
					// Create a rectangle from the RECT struct
					Rectangle r = new Rectangle(dis.rcItem.left, dis.rcItem.top, 
						dis.rcItem.right - dis.rcItem.left, dis.rcItem.bottom - dis.rcItem.top);

					// Get the graphics from the hdc field of the DRAWITEMSTRUCT
					using( Graphics g = Graphics.FromHdc(dis.hdc) )
					{
						// Create new DrawItemState in its default state					
						DrawItemState d = DrawItemState.Default;
						// Set the correct state for drawing
						if((dis.itemState & ODS_SELECTED) > 0)
							d = DrawItemState.Selected;
						// Create the DrawItemEventArgs object
						PluginListItem item = (PluginListItem)Items[dis.itemID];
						DrawItemEventArgs e = new DrawItemEventArgs(g,this.Font,r,dis.itemID,d);
						OnDrawItem(e, item);
						// We processed the message
						m.Result = (IntPtr)1;
					}
					break;
				}

				default:
					base.WndProc(ref m);
					break;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			// Detect which column was clicked and handle accordingly
			const int LVM_FIRST = 0x1000;
			const int LVM_SUBITEMHITTEST = LVM_FIRST + 57;

			NativeMethods.LVHITTESTINFO hitInfo = new NativeMethods.LVHITTESTINFO();
			hitInfo.pt = new Point(e.X, e.Y);
			IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.LVHITTESTINFO)));
			Marshal.StructureToPtr(hitInfo, pointer, true);
			Message message = Message.Create(Handle, LVM_SUBITEMHITTEST, IntPtr.Zero, pointer);
			DefWndProc(ref message);
			hitInfo = (NativeMethods.LVHITTESTINFO)Marshal.PtrToStructure(
				pointer, typeof(NativeMethods.LVHITTESTINFO));
			Marshal.FreeHGlobal(pointer);

			if(hitInfo.iItem >=0 && hitInfo.iSubItem == 1)
			{
				// Flip the load on startup flag
				PluginListItem item = (PluginListItem)Items[hitInfo.iItem];
				item.PluginInfo.IsLoadedAtStartup = !item.PluginInfo.IsLoadedAtStartup;
				Invalidate();
				return;
			}

			base.OnMouseUp(e);
		}

		/// <summary>
		/// Owner drawing listview item.
		/// </summary>
		protected void OnDrawItem( DrawItemEventArgs e, PluginListItem item )
		{
			e.DrawBackground();

			// IsRunnning bitmap
			const int imageWidth = 16+3;
			if(imageList==null)
				imageList = ((PluginDialog)Parent).ImageList;
			if(imageList!=null)
			{
				int imageIndex = item.PluginInfo.IsCurrentlyLoaded ? 0 : 1;
				imageList.Draw(e.Graphics, e.Bounds.Left+2, e.Bounds.Top+1, imageIndex);
			}

			// Name
			Rectangle bounds = Rectangle.FromLTRB(e.Bounds.Left+imageWidth, 
				e.Bounds.Top, e.Bounds.Left+Columns[0].Width, e.Bounds.Bottom);
			using(Brush brush = new SolidBrush(e.ForeColor))
				e.Graphics.DrawString(item.Name, e.Font, brush, bounds);
			
			// Check box (Load at startup)
			bounds = Rectangle.FromLTRB(bounds.Right+1, 
				bounds.Top, bounds.Right+Columns[1].Width+1, bounds.Bottom-1);
			ButtonState state = item.PluginInfo.IsLoadedAtStartup ? ButtonState.Checked : ButtonState.Normal;
			ControlPaint.DrawCheckBox(e.Graphics, bounds, state);
		}
	}
}
