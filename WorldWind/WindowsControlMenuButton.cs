using System;
using WorldWind.Menu;

namespace WorldWind
{
	
	public class WindowsControlMenuButton : MenuButton
	{
		System.Windows.Forms.Control _control;
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.WindowsControlMenuButton"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="iconFilePath"></param>
		/// <param name="control"></param>
		public WindowsControlMenuButton(
			string name,
			string iconFilePath,
			System.Windows.Forms.Control control) : base(iconFilePath)
		{
			this.Description = name;
			this._control = control;
		}

		public override void Update(DrawArgs drawArgs)
		{
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		public override bool IsPushed()
		{	
			return this._control.Visible;
		}

		public override void SetPushed(bool isPushed)
		{
			this._control.Visible = isPushed;
		}

		public override bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
			return false;
		}

		public override void OnKeyDown(System.Windows.Forms.KeyEventArgs keyEvent)
		{
		}

		public override void OnKeyUp(System.Windows.Forms.KeyEventArgs keyEvent)
		{
		}

		public override bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			return false;
		}

		public override bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			return false;
		}

		public override bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!this._control.Visible)
			{
				this.SetPushed(false);
			}
		}
	}
}
