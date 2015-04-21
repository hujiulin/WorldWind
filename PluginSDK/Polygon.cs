using System;
using System.Drawing;

namespace WorldWind
{
	/// <summary>
	/// Summary description for Polygon.
	/// </summary>
	public class Polygon
	{
		public LinearRing outerBoundary = null;
		public LinearRing[] innerBoundaries = null;

		public Color PolgonColor = Color.Red;
		public Color OutlineColor = Color.Black;
		public float LineWidth = 1.0f;
		public bool Outline = true;
		public bool Fill = true;
		public bool Visible = true;
		public bool Remove = false;
		public WorldWind.Renderable.RenderableObject ParentRenderable = null;

		public GeographicBoundingBox GetGeographicBoundingBox()
		{
			if(outerBoundary == null || 
				outerBoundary.Points == null ||
				outerBoundary.Points.Length == 0)
				return null;

			double minX = outerBoundary.Points[0].X;
			double maxX = outerBoundary.Points[0].X;
			double minY = outerBoundary.Points[0].Y;
			double maxY = outerBoundary.Points[0].Y;
			double minZ = outerBoundary.Points[0].Z;
			double maxZ = outerBoundary.Points[0].Z;

			for(int i = 1; i < outerBoundary.Points.Length; i++)
			{
				if( outerBoundary.Points[i].X < minX)
					minX =  outerBoundary.Points[i].X;
				if( outerBoundary.Points[i].X > maxX)
					maxX =  outerBoundary.Points[i].X;
				if( outerBoundary.Points[i].Y < minY)
					minY =  outerBoundary.Points[i].Y;
				if( outerBoundary.Points[i].Y > maxY)
					maxY =  outerBoundary.Points[i].Y;
				if( outerBoundary.Points[i].Z < minZ)
					minZ =  outerBoundary.Points[i].Y;
				if( outerBoundary.Points[i].Z > maxZ)
					maxZ =  outerBoundary.Points[i].Y;
			}
			
			return new GeographicBoundingBox(
				maxY, minY, minX, maxX, minZ, maxZ);
		}
	}
}
