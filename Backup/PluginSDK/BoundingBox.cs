using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Camera;

namespace WorldWind
{
	/// <summary>
	/// The closed volume that completely contains a set of objects.
	/// </summary>
	public class BoundingBox
	{
		public Vector3[] corners;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class.
		/// </summary>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <param name="v5"></param>
		/// <param name="v6"></param>
		/// <param name="v7"></param>
		public BoundingBox(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7)
		{
			this.corners = new Vector3[8];
			this.corners[0] = v0;
			this.corners[1] = v1;
			this.corners[2] = v2;
			this.corners[3] = v3;
			this.corners[4] = v4;
			this.corners[5] = v5;
			this.corners[6] = v6;
			this.corners[7] = v7;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class
		/// from a set of lat/lon values (degrees)
		/// </summary>
		/// <param name="south"></param>
		/// <param name="north"></param>
		/// <param name="west"></param>
		/// <param name="east"></param>
		/// <param name="radius1"></param>
		/// <param name="radius2"></param>
		public BoundingBox( float south, float north, float west, float east, float radius1, float radius2)
		{
			float scale = radius2 / radius1;
			this.corners = new Vector3[8];
			this.corners[0] = MathEngine.SphericalToCartesian(south, west, radius1);
			this.corners[1] = Vector3.Scale(this.corners[0], scale);
			this.corners[2] = MathEngine.SphericalToCartesian(south, east, radius1);
			this.corners[3] = Vector3.Scale(this.corners[2], scale);
			this.corners[4] = MathEngine.SphericalToCartesian(north, west, radius1);
			this.corners[5] = Vector3.Scale(this.corners[4], scale);
			this.corners[6] = MathEngine.SphericalToCartesian(north, east, radius1);
			this.corners[7] = Vector3.Scale(this.corners[6], scale);
		}

		public Vector3 CalculateCenter()
		{
			Vector3 res = new Vector3();
			foreach(Vector3 corner in corners)
			{
				res += corner;
			}

			res.Scale(1.0f / corners.Length);
			return res;
		}

		/// <summary>
		/// Calculate the screen area (pixels) covered by the bottom of the bounding box.
		/// </summary>
		public float CalcRelativeScreenArea(CameraBase camera)
		{
			Vector3 a = camera.Project(corners[0]);
			Vector3 b = camera.Project(corners[2]);
			Vector3 c = camera.Project(corners[6]);
			Vector3 d = camera.Project(corners[4]);

			Vector3 ab = Vector3.Subtract(b,a);
			Vector3 ac = Vector3.Subtract(c,a);
			Vector3 ad = Vector3.Subtract(d,a);

			float tri1SqArea = Vector3.Cross(ab,ac).LengthSq(); 
			float tri2SqArea = Vector3.Cross(ad,ac).LengthSq(); 
			// Real area = (sqrt(tri1SqArea)+sqrt(tri2SqArea))/2 but we're only interested in relative size
			return tri1SqArea + tri2SqArea; 
		}
	}
}
