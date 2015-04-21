using System;

namespace WorldWind
{
	/// <summary>
	/// Summary description for Point3d.
	/// </summary>
	public class Point3d
	{
		public double X, Y, Z;
		// constructors

		public Point3d()
		{

		}

		public Point3d cross(Point3d p)
		{
			return new Point3d(
				Y * p.Z - Z * p.Y,
				Z * p.X - X * p.Z,
				X * p.Y - Y * p.X
				);
		}

		public double dotProduct(Point3d p)
		{
			return X * p.X + Y * p.Y + Z * p.Z;
		}

		public Point3d (double xi, double yi, double zi)	// x,y,z constructor
		{
			X = xi; Y = yi; Z = zi;
		}
		public Point3d (Point3d P) // copy constructor
		{
			X = P.X;
			Y = P.Y;
			X = P.Z;
		}

		// assignment operators
		/*Point3d operator = (Point3d P) // copy operator
		{
			x = P.x; y = P.y; z = P.z;
			return *this;
		}*/

		// other operators
		public double norm()	// L2 norm
		{
			return Math.Sqrt(norm2());
		}

		public double norm2() // squared L2 norm
		{
			return X*X + Y*Y + Z*Z;
		}

		public Point3d normalize() // normalization
		{
			double n = norm();
			return new Point3d(X / n, Y / n, Z / n);
		}

		/*Point3d operator *= (double k) // multiply by real
		{
			X *= k; Y *= k; Z *= k;
			return this;
		}*/

		/*Point3d operator /= (const double k) // divide by real
		{
			assert (k != 0.);
			x /= k; y /= k; z /= k;
			return *this;
		}*/

		/*Point3d operator += (const Point3d & P)	// addition
		{
			x += P.x; y += P.y; z += P.z;
			return *this;
		}*/

		/*inline Point3d & Point3d::operator -= (const Point3d & P) // subtraction
		{
			x -= P.x; y -= P.y; z -= P.z;
			return *this;
		}*/

		public double Length
		{
			get
			{
				return Math.Sqrt(X * X + Y * Y + Z * Z);
			}
		}

		public static Angle GetAngle(Point3d p1, Point3d p2)
		{
			Angle returnAngle = new Angle();
			returnAngle.Radians = Math.Acos(Point3d.dot(p1, p2) / (p1.Length * p2.Length));
			return returnAngle;
		}

		public static Point3d operator +(Point3d P1, Point3d P2)	// addition 2
		{
			return new Point3d (P1.X + P2.X, P1.Y + P2.Y, P1.Z + P2.Z);
		}

		public static Point3d operator -(Point3d P1, Point3d P2)	// subtraction 2
		{
			return new Point3d (P1.X - P2.X, P1.Y - P2.Y, P1.Z - P2.Z);
		}

		public static Point3d operator *(Point3d P, double k)	// multiply by real 2
		{
			return new Point3d (P.X * k, P.Y * k, P.Z * k);
		}

		public static Point3d operator *(double k, Point3d P)	// and its reverse order!
		{
			return new Point3d (P.X * k, P.Y * k, P.Z * k);
		}

		public static Point3d operator /(Point3d P, double k)	// divide by real 2
		{
			return new Point3d (P.X / k, P.Y / k, P.Z / k);
		}

		// Override the Object.Equals(object o) method:
		public override bool Equals(object o)
		{
			try
			{
				return (bool)(this == (Point3d)o);
			}
			catch
			{
				return false;
			}
		}

		// Override the Object.GetHashCode() method:
		public override int GetHashCode()
		{
			//not the best algorithm for hashing, but whatever...
			return (int)(X * Y * Z);
		}

		public static bool operator ==(Point3d P1, Point3d P2) // equal?
		{
			return (P1.X == P2.X && P1.Y == P2.Y && P1.Z == P2.Z);
		}

		public static bool operator !=(Point3d P1, Point3d P2) // equal?
		{
			return (P1.X != P2.X || P1.Y != P2.Y || P1.Z != P2.Z);
		}

		public static double dot(Point3d P1, Point3d P2) // inner product 2
		{
			return (P1.X * P2.X + P1.Y * P2.Y + P1.Z * P2.Z);
		}

		public static Point3d operator *(Point3d P1, Point3d P2)
		{
			return new Point3d (P1.Y * P2.Z - P1.Z * P2.Y,
				P1.Z * P2.X - P1.X * P2.Z, P1.X * P2.Y - P1.Y * P2.X);
		}

		/*public Point3d operator *= (const Point3d & P) // cross product
		{
			return (*this = (*this) * P);
		}*/

		public static Point3d operator - ( Point3d P)	// negation
		{
			return new Point3d (-P.X, -P.Y, -P.Z);
		}

		public static Point3d cross(Point3d P1, Point3d P2) // cross product
		{
			return P1 * P2;
		}

		// Normal direction corresponds to a right handed traverse of ordered points.
		public Point3d unit_normal (Point3d P0, Point3d P1, Point3d P2)
		{
			Point3d p = (P1 - P0) * (P2 - P0);
			double l = p.norm ();
			return new Point3d (p.X / l, p.Y / l, p.Z / l);
		}
	}
}
