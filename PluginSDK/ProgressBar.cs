using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;

namespace WorldWind.VisualControl
{
	/// <summary>
	/// Render a progress bar.
	/// </summary>
	public class ProgressBar
	{
		CustomVertex.TransformedColored[] progressBarOutline = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[5];
		CustomVertex.TransformedColored[] progressBarBackground = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[4];
		CustomVertex.TransformedColored[] progressBarShadow = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[4];
		CustomVertex.TransformedColored[] progressBar = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[4];
		CustomVertex.TransformedColored[] progressRight = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[4];
		float x;
		float y;
		float width;
		float height;
		float halfWidth;
		float halfHeight;
		static int backColor = Color.FromArgb(98,200,200,200).ToArgb();
		static int shadowColor = Color.FromArgb(98,50,50,50).ToArgb();
		static int outlineColor = 80<<24;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.VisualControl.ProgressBar"/> class.
		/// </summary>
		/// <param name="width">Width in pixels of progress bar</param>
		/// <param name="height">Height in pixels of progress bar</param>
		/// <param name="color">Color of the progress bar</param>
		public ProgressBar( float width, float height )
		{
			this.width = width;
			this.height = height;
			this.halfWidth = width/2;
			this.halfHeight = height/2;
		}

		/// <summary>
		/// Sets up the data for rendering
		/// </summary>
		/// <param name="x">Center X position of progress.</param>
		/// <param name="y">Center Y position of progress.</param>
		public void Initalize( float x, float y)
		{
			this.x = x;
			this.y = y;

			// Background
			progressBarBackground[0].X = x - halfWidth;
			progressBarBackground[0].Y = y - halfHeight;
			progressBarBackground[0].Z = 0.5f;
			progressBarBackground[0].Color = backColor;

			progressBarBackground[1].X = x - halfWidth;
			progressBarBackground[1].Y = y + halfHeight;
			progressBarBackground[1].Z = 0.5f;
			progressBarBackground[1].Color = backColor;
					
			progressBarBackground[2].X = x + halfWidth;
			progressBarBackground[2].Y = y - halfHeight;
			progressBarBackground[2].Z = 0.5f;
			progressBarBackground[2].Color = backColor;
					
			progressBarBackground[3].X = x + halfWidth;
			progressBarBackground[3].Y = y + halfHeight;
			progressBarBackground[3].Z = 0.5f;
			progressBarBackground[3].Color = backColor;

			// Shadow
			const float shadowOffsetX = 2.5f;
			const float shadowOffsetY = 2.5f;

			progressBarShadow[0].X = x - halfWidth + shadowOffsetX;
			progressBarShadow[0].Y = y - halfHeight + shadowOffsetY;
			progressBarShadow[0].Z = 0.5f;
			progressBarShadow[0].Color = shadowColor;

			progressBarShadow[1].X = x - halfWidth + shadowOffsetX;
			progressBarShadow[1].Y = y + halfHeight + shadowOffsetY;
			progressBarShadow[1].Z = 0.5f;
			progressBarShadow[1].Color = shadowColor;
					
			progressBarShadow[2].X = x + halfWidth + shadowOffsetX;
			progressBarShadow[2].Y = y - halfHeight + shadowOffsetY;
			progressBarShadow[2].Z = 0.5f;
			progressBarShadow[2].Color = shadowColor;
					
			progressBarShadow[3].X = x + halfWidth + shadowOffsetX;
			progressBarShadow[3].Y = y + halfHeight + shadowOffsetY;
			progressBarShadow[3].Z = 0.5f;
			progressBarShadow[3].Color = shadowColor;

			// Outline
			progressBarOutline[0].X = x - halfWidth;
			progressBarOutline[0].Y = y - halfHeight;
			progressBarOutline[0].Z = 0.5f;
			progressBarOutline[0].Color = outlineColor;

			progressBarOutline[1].X = x - halfWidth;
			progressBarOutline[1].Y = y + halfHeight;
			progressBarOutline[1].Z = 0.5f;
			progressBarOutline[1].Color = outlineColor;
					
			progressBarOutline[2].X = x + halfWidth;
			progressBarOutline[2].Y = y + halfHeight;
			progressBarOutline[2].Z = 0.5f;
			progressBarOutline[2].Color = outlineColor;
					
			progressBarOutline[3].X = x + halfWidth;
			progressBarOutline[3].Y = y - halfHeight;
			progressBarOutline[3].Z = 0.5f;
			progressBarOutline[3].Color = outlineColor;
					
			progressBarOutline[4].X = x - halfWidth;
			progressBarOutline[4].Y = y - halfHeight;
			progressBarOutline[4].Z = 0.5f;
			progressBarOutline[4].Color = outlineColor;

			// Progress bar progress
			progressBar[0].Z = 0.5f;
			progressBar[1].Z = 0.5f;
			progressBar[2].Z = 0.5f;
			progressBar[3].Z = 0.5f;

			int rightColor = 0x70808080;
			progressRight[0].Z = 0.5f;
			progressRight[0].Color = rightColor;
			progressRight[1].Z = 0.5f;
			progressRight[1].Color = rightColor;
			progressRight[2].Z = 0.5f;
			progressRight[2].Color = rightColor;
			progressRight[3].Z = 0.5f;
			progressRight[3].Color = rightColor;
		}

		/// <summary>
		/// Draws the progress bar
		/// </summary>
		/// <param name="x">Center X position of progress.</param>
		/// <param name="y">Center Y position of progress.</param>
		/// <param name="progress">Progress vale, in the range 0..1</param>
		public void Draw(DrawArgs drawArgs, float x, float y, float progress, int color)
		{
			if(x!=this.x || y!=this.y)
				Initalize(x,y);
			int barlength = (int)(progress * 2 * halfWidth);

			progressBar[0].X = x - halfWidth;
			progressBar[0].Y = y - halfHeight;
			progressBar[0].Color = color;
			progressBar[1].X = x - halfWidth;
			progressBar[1].Y = y + halfHeight;
			progressBar[1].Color = color;
			progressBar[2].X = x - halfWidth + barlength;
			progressBar[2].Y = y - halfHeight;
			progressBar[2].Color = color;
			progressBar[3].Y = y + halfHeight;
			progressBar[3].X = x - halfWidth + barlength;
			progressBar[3].Color = color;

			progressRight[0].X = x - halfWidth +barlength;
			progressRight[0].Y = y - halfHeight;
			progressRight[1].X = x - halfWidth + barlength;
			progressRight[1].Y = y + halfHeight;
			progressRight[2].X = x + halfWidth;
			progressRight[2].Y = y - halfHeight;
			progressRight[3].X = x + halfWidth;
			progressRight[3].Y = y + halfHeight;

			drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
			drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, progressBar);
			drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, progressRight);
			drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, progressBarOutline);
		}
	}
}
