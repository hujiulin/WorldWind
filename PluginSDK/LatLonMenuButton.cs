using System;

namespace WorldWind.Menu
{
	
	public class LatLonMenuButton : MenuButton
	{
		#region Private Members
		World _parentWorld;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.LatLonMenuButton"/> class.
		/// </summary>
		/// <param name="buttonIconPath"></param>
		/// <param name="parentWorld"></param>
		public LatLonMenuButton(string buttonIconPath, World parentWorld) : base(buttonIconPath)
		{
			this._parentWorld = parentWorld;
			this.Description = "Latitude & Longitude Lines";
			this.SetPushed(World.Settings.showLatLonLines);
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
			return World.Settings.showLatLonLines;
		}

		public override void SetPushed(bool isPushed)
		{
			World.Settings.showLatLonLines = isPushed;
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

		}
	}
}
