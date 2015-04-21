using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	/// <summary>
	/// Bounding sphere.  The tightest sphere that will fit the bounded object, 
	/// that is, the smallest radius sphere that all points lie within. 
	/// </summary>
	public class BoundingSphere
	{
		public Vector3 Center;
		public float Radius;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class
		/// from a center point and a radius.
		/// </summary>
		public BoundingSphere(Vector3 center, float radius)
		{
			this.Center = center;
			this.Radius = radius;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class
		/// from a set of lat/lon values (degrees)
		/// </summary>
		public BoundingSphere( float south, float north, float west, float east, float radius1, float radius2)
		{
			// Compute the points in world coordinates
			const int CornerCount = 8;
			Vector3[] corners = new Vector3[CornerCount];

			float scale = radius2 / radius1;
			corners[0] = MathEngine.SphericalToCartesian(south, west, radius1);
			corners[1] = Vector3.Scale(corners[0], scale);
			corners[2] = MathEngine.SphericalToCartesian(south, east, radius1);
			corners[3] = Vector3.Scale(corners[2], scale);
			corners[4] = MathEngine.SphericalToCartesian(north, west, radius1);
			corners[5] = Vector3.Scale(corners[4], scale);
			corners[6] = MathEngine.SphericalToCartesian(north, east, radius1);
			corners[7] = Vector3.Scale(corners[6], scale);

			//Find the center.  In this case, we'll simply average the coordinates. 
			foreach(Vector3 v in corners)
				Center += v;
			Center.Scale(1/CornerCount);

			//Loop through the coordinates and find the maximum distance from the center.  This is the radius.		
			foreach(Vector3 v in corners)
			{
				float distSq = Vector3.Subtract(v,Center).LengthSq();
				if (distSq > Radius)
					Radius = distSq;
			}

			Radius = (float)Math.Sqrt(Radius);			
		}		
	}
}
