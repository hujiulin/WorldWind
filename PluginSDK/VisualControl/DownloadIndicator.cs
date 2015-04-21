using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.IO;
using System.Drawing;
using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;

namespace WorldWind.VisualControl
{
	/// <summary>
	/// Renders download progress and rectangle
	/// </summary>
	public class DownloadIndicator : IDisposable
	{
		protected ProgressBar m_progressBar;
		protected CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];
		protected Vector2 m_renderPosition;
		public static int HalfWidth = 24;
		protected Sprite m_sprite;

		public void Render(DrawArgs drawArgs)
		{
			// Render from bottom and up
			const int screenMargin = 10;
			m_renderPosition = new Vector2(drawArgs.screenWidth - HalfWidth - screenMargin, drawArgs.screenHeight - screenMargin);

			ImageAccessor logoAccessor = null;

			// Render download progress and rectangles
			for(int i=0; i < DrawArgs.DownloadQueue.ActiveDownloads.Count; i++)
			{
				DownloadRequest request = (DownloadRequest)DrawArgs.DownloadQueue.ActiveDownloads[i];
				GeoSpatialDownloadRequest geoRequest = request as GeoSpatialDownloadRequest;
				if(geoRequest == null)
					continue;

				RenderProgress(drawArgs, geoRequest);
				RenderRectangle(drawArgs, geoRequest);

				ImageTileRequest imageRequest = geoRequest as ImageTileRequest;
				if(imageRequest == null)
					continue;

				QuadTile qt = imageRequest.QuadTile;
				if(qt.QuadTileArgs.ImageAccessor.ServerLogoPath != null)
					logoAccessor = qt.QuadTileArgs.ImageAccessor;
			}

			if(logoAccessor != null)
				RenderLogo( drawArgs, logoAccessor );
		}

		/// <summary>
		/// Renders the server logo
		/// </summary>
		/// <param name="logoAccessor"></param>
		protected void RenderLogo( DrawArgs drawArgs, ImageAccessor logoAccessor )
		{
			if(logoAccessor.ServerLogoPath == null)
				return;

			if(logoAccessor.ServerLogo == null)
			{
				if(!File.Exists(logoAccessor.ServerLogoPath))
					return;

				logoAccessor.ServerLogo = ImageHelper.LoadTexture(drawArgs.device, logoAccessor.ServerLogoPath);

				using(Surface s = logoAccessor.ServerLogo.GetSurfaceLevel(0))
				{
					SurfaceDescription desc = s.Description;
					logoAccessor.ServerLogoSize = new Rectangle(0, 0, desc.Width, desc.Height);
				}
			}

			if(m_sprite == null)
				m_sprite = new Sprite(drawArgs.device);

			float xScale = 2f * HalfWidth / logoAccessor.ServerLogoSize.Width;
			float yScale = 2f * HalfWidth / logoAccessor.ServerLogoSize.Height;

			m_renderPosition.Y -= HalfWidth;
			m_sprite.Begin(SpriteFlags.AlphaBlend);

			m_sprite.Transform = Matrix.Scaling(xScale,yScale,0);
			m_sprite.Transform *= Matrix.Translation(m_renderPosition.X, m_renderPosition.Y, 0);
			m_sprite.Draw( logoAccessor.ServerLogo,
				new Vector3(logoAccessor.ServerLogoSize.Width>>1, logoAccessor.ServerLogoSize.Height>>1, 0),
				Vector3.Empty,
				World.Settings.downloadLogoColor );

			m_sprite.End();
		}

		/// <summary>
		/// Render download indicator rectangles
		/// </summary>
		protected virtual void RenderProgress(DrawArgs drawArgs, GeoSpatialDownloadRequest request)
		{
			const int height = 4;
			const int spacing = 2;

			// Render progress bar
			if(m_progressBar==null)
				m_progressBar = new ProgressBar(HalfWidth*2 * 4/5, height); // 4/5 of icon width
			m_progressBar.Draw(drawArgs, m_renderPosition.X, m_renderPosition.Y-height, request.Progress, request.Color);
			m_renderPosition.Y -= height+spacing;
		}

		/// <summary>
		/// Render a rectangle around an image tile in the specified color
		/// </summary>
		public void RenderRectangle(DrawArgs drawArgs, GeoSpatialDownloadRequest request)
		{
			int color = request.Color;
			// Render terrain download rectangle
			float radius = (float)drawArgs.WorldCamera.WorldRadius;
			Vector3 northWestV = MathEngine.SphericalToCartesian(request.North, request.West, radius);
			Vector3 southWestV = MathEngine.SphericalToCartesian(request.South, request.West, radius);
			Vector3 northEastV = MathEngine.SphericalToCartesian(request.North, request.East, radius);
			Vector3 southEastV = MathEngine.SphericalToCartesian(request.South, request.East, radius);

			downloadRectangle[0].X = northWestV.X;
			downloadRectangle[0].Y = northWestV.Y;
			downloadRectangle[0].Z = northWestV.Z;
			downloadRectangle[0].Color = color;

			downloadRectangle[1].X = southWestV.X;
			downloadRectangle[1].Y = southWestV.Y;
			downloadRectangle[1].Z = southWestV.Z;
			downloadRectangle[1].Color = color;

			downloadRectangle[2].X = southEastV.X;
			downloadRectangle[2].Y = southEastV.Y;
			downloadRectangle[2].Z = southEastV.Z;
			downloadRectangle[2].Color = color;

			downloadRectangle[3].X = northEastV.X;
			downloadRectangle[3].Y = northEastV.Y;
			downloadRectangle[3].Z = northEastV.Z;
			downloadRectangle[3].Color = color;

			downloadRectangle[4].X = downloadRectangle[0].X;
			downloadRectangle[4].Y = downloadRectangle[0].Y;
			downloadRectangle[4].Z = downloadRectangle[0].Z;
			downloadRectangle[4].Color = color;

			drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
			drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, downloadRectangle);
		}
		#region IDisposable Members

		public void Dispose()
		{
			if(m_sprite != null)
			{
				m_sprite.Dispose();
				m_sprite = null;
			}

			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
