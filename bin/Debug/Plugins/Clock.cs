//----------------------------------------------------------------------------
// NAME: On-Screen Clock 
// VERSION: 1.1
// DESCRIPTION: Clock sample, adds itself in layer manager - demonstrates adding a renderable object, drawing on screen (C#)
// DEVELOPER: Bjorn Reppen aka "Mashi"
// WEBSITE: http://www.mashiharu.com
//----------------------------------------------------------------------------
//
// This file is in the Public Domain, and comes with no warranty. 
//
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using WorldWind;
using WorldWind;
using WorldWind.Renderable;

namespace Mashiharu.PluginSample
{
	public class ClockPlugin : WorldWind.PluginEngine.Plugin
	{
		Clock clock;
		
		/// <summary>
		/// Plugin entry point - All plugins must implement this function
		/// </summary>
		public override void Load()
		{
			// Add us to the list of renderable objects - this puts us in the render loop
			clock = new Clock(Application);
			Application.WorldWindow.CurrentWorld.RenderableObjects.Add(clock);
		}

		/// <summary>
		/// Unloads our plugin
		/// </summary>
		public override void Unload()
		{
			Application.WorldWindow.CurrentWorld.RenderableObjects.Remove(clock.Name);
		}
	}

	/// <summary>
	/// Clock Example : Display the current time 
	/// </summary>
	public class Clock : RenderableObject
	{
        int color2 = Color.Yellow.ToArgb();
		int color = Color.Yellow.ToArgb();
		const int distanceFromCorner = 5;
		MainApplication ww;

		public Clock(MainApplication app) : base("Clock", Vector3.Empty, Quaternion.Identity)
		{
			this.ww = app;
			
			// We want to be drawn on top of everything else
			this.RenderPriority = RenderPriority.Icons;

			// true to make this layer active on startup, this is equal to the checked state in layer manager
			this.IsOn = true;
		}

		/// <summary>
		/// Plugin entry point - All plugins must implement this function
		/// </summary>
		public void Load()
		{
			// Add us to the list of renderable objects - this puts us in the render loop
			ww.WorldWindow.CurrentWorld.RenderableObjects.Add(this);
		}

		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			// Draw the current time using default font in lower right corner
			string text = DateTime.Now.ToString();
			Rectangle bounds = drawArgs.defaultDrawingFont.MeasureString(null, text, DrawTextFormat.None, 0);
			drawArgs.defaultDrawingFont.DrawText(null, text, 
				drawArgs.screenWidth-bounds.Width-distanceFromCorner, drawArgs.screenHeight-bounds.Height-distanceFromCorner,
				color );
		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}
	}
}