using System;
using System.Drawing;

namespace WorldWind
{
	/// <summary>
	/// Summary description for LineString.
	/// </summary>
	public class LineString
	{
		public Point3d[] Coordinates = null;
		public Color Color = Color.Yellow;
		public float LineWidth = 1.0f;
		public bool Visible = true;
		public bool Remove = false;
		public WorldWind.Renderable.RenderableObject ParentRenderable = null;

		public GeographicBoundingBox GetGeographicBoundingBox()
		{
			if(Coordinates == null || 
				Coordinates.Length == 0)
				return null;

			double minX = Coordinates[0].X;
			double maxX = Coordinates[0].X;
			double minY = Coordinates[0].Y;
			double maxY = Coordinates[0].Y;
			double minZ = Coordinates[0].Z;
			double maxZ = Coordinates[0].Z;

			for(int i = 1; i < Coordinates.Length; i++)
			{
				if( Coordinates[i].X < minX)
					minX =  Coordinates[i].X;
				if( Coordinates[i].X > maxX)
					maxX =  Coordinates[i].X;

				if( Coordinates[i].Y < minY)
					minY =  Coordinates[i].Y;
				if( Coordinates[i].Y > maxY)
					maxY =  Coordinates[i].Y;

				if( Coordinates[i].Z < minZ)
					minZ =  Coordinates[i].Z;
				if( Coordinates[i].Z > maxZ)
					maxZ =  Coordinates[i].Z;
			}
			
			return new GeographicBoundingBox(
				maxY, minY, minX, maxX, minZ, maxZ);
		}
	}
}
