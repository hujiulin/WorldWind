using System;

namespace WorldWind.Menu
{
	/// <summary>
	/// Summary description for PositionMenuButton.
	/// </summary>
	public class PositionMenuButton : MenuButton
	{
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.PositionMenuButton"/> class.
		/// </summary>
		/// <param name="buttonIconPath"></param>
		public PositionMenuButton(string buttonIconPath) : base(buttonIconPath)
		{
			this.Description = "Position Information";
		}

		public override void Dispose()
		{
			base.Dispose ();
		}
		public override bool IsPushed()
		{
			return World.Settings.showPosition;
		}
		public override void SetPushed(bool isPushed)
		{
			World.Settings.showPosition = isPushed;
			World.Settings.showCrosshairs = isPushed;
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
		public override void Update(DrawArgs drawArgs)
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

		}
	}
}
