using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	/// <summary>
	/// The region of space in the modeled world that may appear on the screen; it is the field of view of the notional camera.
	/// Used to perform culling of invisible object (prior to rendering) to increase speed.
	/// See: http://en.wikipedia.org/wiki/Viewing_frustum
	/// </summary>
	public class Frustum
	{
		public Plane[] planes = new Plane[6];

		public void Update(Matrix m)
		{
			//bottom (down) plane
			this.planes[0] = new Plane(
				m.M14 + m.M12, //a
				m.M24 + m.M22, //b
				m.M34 + m.M32, //c
				m.M44 + m.M42 //d
				);
			
			//far plane
			this.planes[1] = new Plane(
				m.M14 - m.M13,
				m.M24 - m.M23,
				m.M34 - m.M33,
				m.M44 - m.M43
				);

			//right side plane
			this.planes[2] = new Plane(
				m.M14 - m.M11, //a
				m.M24 - m.M21, //b
				m.M34 - m.M31, //c
				m.M44 - m.M41 //d
				);

			//left side plane
			this.planes[3] = new Plane(
				m.M14 + m.M11,	//a
				m.M24 + m.M21,	//b
				m.M34 + m.M31,	//c
				m.M44 + m.M41	//d
				);

			//near plane
			this.planes[4] = new Plane(
				m.M13,
				m.M23,
				m.M33,
				m.M43);

			//top (up) plane
			this.planes[5] = new Plane(
				m.M14 - m.M12, //a
				m.M24 - m.M22, //b
				m.M34 - m.M32, //c
				m.M44 - m.M42 //d
				);

			foreach(Plane p in this.planes)
				p.Normalize();
		}

		/// <summary>
		/// Test if a sphere intersects or is completely inside the frustum.
		/// </summary>
		/// <returns>true when the sphere intersects.</returns>
		public bool Intersects(BoundingSphere c)
		{
			foreach(Plane p in this.planes)
			{
				float distancePlaneToPoint = p.A * c.Center.X + p.B * c.Center.Y + p.C * c.Center.Z + p.D;
				if(distancePlaneToPoint < -c.Radius)
					// More than 1 radius outside the plane = outside
					return false;
			}

			//else it's in view
			return true;
		}

		/// <summary>
		/// Test if a point is inside the frustum.
		/// </summary>
		/// <returns>true when the point is inside.</returns>
		/// <param name="v">XYZ in world coordinates of the point to test.</param>
		public bool ContainsPoint(Vector3 v)
		{
			foreach(Plane p in this.planes)
				if(Vector3.Dot(new Vector3(p.A, p.B, p.C), v) + p.D < 0)
					return false;

			return true;
		}

		/// <summary>
		/// Tests if the view frustum fully contains the bounding box.
		/// </summary>
		/// <returns>true when the box is complete enclosed by the frustum.</returns>
		public bool Contains(BoundingBox bb)
		{
			//Code taken from Flip Code Article:
			// http://www.flipcode.com/articles/article_frustumculling.shtml
			int iTotalIn = 0;
			foreach(Plane p in this.planes)
			{
				int iInCount = 8;
				int iPtIn = 1;
				// TODO: Modify bounding box and only check 2 corners.
				for(int i = 0; i < 8; i++)
				{
					if(Vector3.Dot(new Vector3(p.A,p.B,p.C), bb.corners[i]) + p.D < 0)
					{
						iPtIn = 0;
						--iInCount;
					}
				}

				if(iInCount == 0)
					return false;

				iTotalIn += iPtIn;
			}

			if(iTotalIn == 6)
				return true;

			return false;
		}

		/// <summary>
		/// Tests if the bounding box specified intersects with or is fully contained in the frustum.
		/// </summary>
		/// <returns>true when the box intersects with the frustum.</returns>
		public bool Intersects(BoundingBox bb)
		{
			foreach(Plane p in this.planes)
			{
				Vector3 v = new Vector3(p.A,p.B,p.C);
				bool isInside = false;
				// TODO: Modify bounding box and only check 2 corners.
				for(int i = 0; i < 8; i++)
				{
					if(Vector3.Dot(v, bb.corners[i]) + p.D >= 0)
					{
						isInside = true;
						break;
					}
				}

				if(!isInside)
					return false;
			}

			return true;
		}
	
		public override string ToString()
		{
			string res = string.Format("Near:\n{0}Far:\n{1}", planes[4], planes[1] );
			return res;
		}
	}
}
