using System;

namespace WorldWind
{
	public struct Quaternion4d
	{
		public double X;
		public double Y;
		public double Z;
		public double W;

		public Quaternion4d(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public override int GetHashCode() 
		{
			return (int)(X / Y / Z / W);
		}

		public override bool Equals(object obj)
		{
			if(obj is Quaternion4d)
			{
				Quaternion4d q = (Quaternion4d)obj;
				return q == this;
			}
			else
				return false;
		}

		public static Quaternion4d EulerToQuaternion(double yaw, double pitch, double roll)
		{
			double cy = Math.Cos(yaw * 0.5);
			double cp = Math.Cos(pitch * 0.5);
			double cr = Math.Cos(roll * 0.5);
			double sy = Math.Sin(yaw * 0.5);
			double sp = Math.Sin(pitch * 0.5);
			double sr = Math.Sin(roll * 0.5);

			double qw = cy*cp*cr + sy*sp*sr;
			double qx = sy*cp*cr - cy*sp*sr;
			double qy = cy*sp*cr + sy*cp*sr;
			double qz = cy*cp*sr - sy*sp*cr;

			return new Quaternion4d(qx, qy, qz, qw);
		}

		/// <summary>
		/// Transforms a rotation in quaternion form to a set of Euler angles 
		/// </summary>
		/// <returns>The rotation transformed to Euler angles, X=Yaw, Y=Pitch, Z=Roll (radians)</returns>
		public static Point3d QuaternionToEuler(Quaternion4d q)
		{
			double q0 = q.W;
			double q1 = q.X;
			double q2 = q.Y;
			double q3 = q.Z;

			double x = Math.Atan2( 2 * (q2*q3 + q0*q1), (q0*q0 - q1*q1 - q2*q2 + q3*q3));
			double y = Math.Asin( -2 * (q1*q3 - q0*q2));
			double z = Math.Atan2( 2 * (q1*q2 + q0*q3), (q0*q0 + q1*q1 - q2*q2 - q3*q3));

			return new Point3d(x, y, z);
		}

		public static Quaternion4d operator+(Quaternion4d a, Quaternion4d b)
		{
			return new Quaternion4d(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
		}

		public static Quaternion4d operator-(Quaternion4d a, Quaternion4d b)
		{
			return new Quaternion4d( a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
		}

		public static Quaternion4d operator*(Quaternion4d a, Quaternion4d b)
		{
			return new Quaternion4d(
					a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
					a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
					a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
					a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
		}

		public static Quaternion4d operator*(double s, Quaternion4d q)
		{
			return new Quaternion4d(s * q.X, s * q.Y, s * q.Z, s * q.W);
		}

		public static Quaternion4d operator*(Quaternion4d q, double s)
		{
			return new Quaternion4d(s * q.X, s * q.Y, s * q.Z, s * q.W);
		}

		// equivalent to multiplying by the quaternion (0, v)
		public static Quaternion4d operator*(Point3d v, Quaternion4d q)
		{
			return new Quaternion4d(
					 v.X * q.W + v.Y * q.Z - v.Z * q.Y,
					 v.Y * q.W + v.Z * q.X - v.X * q.Z,
					 v.Z * q.W + v.X * q.Y - v.Y * q.X,
					-v.X * q.X - v.Y * q.Y - v.Z * q.Z);
		}

		public static Quaternion4d operator/(Quaternion4d q, double s)
		{
			return q * (1 / s);
		}

		// conjugate operator
		public Quaternion4d Conjugate()
		{
			return new Quaternion4d( -X, -Y, -Z, W);
		}

		public static double Norm2(Quaternion4d q)
		{
			return q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
		}

		public static double Abs(Quaternion4d q)
		{
			return Math.Sqrt(Norm2(q));
		}

		public static Quaternion4d operator/(Quaternion4d a, Quaternion4d b)
		{
			return a * (b.Conjugate() / Abs(b));
		}

		public static bool operator==(Quaternion4d a, Quaternion4d b)
		{
			return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
		}

		public static bool operator!=(Quaternion4d a, Quaternion4d b)
		{
			return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
		}

		public static double Dot(Quaternion4d a, Quaternion4d b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
		}

		public void Normalize()
		{
			double L = Length();

			X = X / L;
			Y = Y / L;
			Z = Z / L;
			W = W / L;
		}

		public double Length()
		{
			return Math.Sqrt(X * X + Y * Y +
				Z * Z + W * W);
		}
		
		public static Quaternion4d Slerp(Quaternion4d q0, Quaternion4d q1, double t)
		{
			double cosom = q0.X * q1.X + q0.Y * q1.Y + q0.Z * q1.Z + q0.W * q1.W;
			double tmp0, tmp1, tmp2, tmp3;
			if (cosom < 0.0)
			{
				cosom = -cosom;
				tmp0 = -q1.X;
				tmp1 = -q1.Y;
				tmp2 = -q1.Z;
				tmp3 = -q1.W;
			}
			else
			{
				tmp0 = q1.X;
				tmp1 = q1.Y;
				tmp2 = q1.Z;
				tmp3 = q1.W;
			}

			/* calc coeffs */
			double scale0, scale1;

			if ((1.0 - cosom) > double.Epsilon)
			{
				// standard case (slerp)
				double omega =  Math.Acos (cosom);
				double sinom = Math.Sin (omega);
				scale0 =  Math.Sin ((1.0 - t) * omega) / sinom;
				scale1 =  Math.Sin (t * omega) / sinom;
			}
			else
			{
				/* just lerp */
				scale0 = 1.0 - t;
				scale1 = t;
			}

			Quaternion4d q = new Quaternion4d();

			q.X = scale0 * q0.X + scale1 * tmp0;
			q.Y = scale0 * q0.Y + scale1 * tmp1;
			q.Z = scale0 * q0.Z + scale1 * tmp2;
			q.W = scale0 * q0.W + scale1 * tmp3;

			return q;
		}

		public Quaternion4d Ln() 
		{
			return Ln(this);
		}

		public static Quaternion4d Ln(Quaternion4d q)
		{
			double t = 0;
 
			double s = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z); 
			double om = Math.Atan2(s, q.W); 
			
			if (Math.Abs(s) < double.Epsilon) 
				t = 0.0f; 
			else 
				t = om/s; 
			
			q.X = q.X * t;
			q.Y = q.Y * t;
			q.Z = q.Z * t;
			q.W = 0.0f;

			return q;
		}

		//the below functions have not been certified to work properly
		public static Quaternion4d Exp(Quaternion4d q)
		{
			double sinom;
			double om = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
			
			if (Math.Abs(om) < double.Epsilon) 
				sinom = 1.0; 
			else 
				sinom = Math.Sin(om)/om; 
			
			q.X = q.X * sinom;
			q.Y = q.Y * sinom;
			q.Z = q.Z * sinom;
			q.W = Math.Cos(om);
			
			return q;
		}
		
		public Quaternion4d Exp()
		{
			return Ln(this);
		}
		
		public static Quaternion4d Squad(
			Quaternion4d q1,
			Quaternion4d a,
			Quaternion4d b,
			Quaternion4d c,
			double t)
		{
			return Slerp(
				Slerp(q1, c, t), Slerp(a, b, t), 2 * t * (1.0 - t));
		}

		public static void SquadSetup(
			ref Quaternion4d outA,
			ref Quaternion4d outB,
			ref Quaternion4d outC,
			Quaternion4d q0,
			Quaternion4d q1,
			Quaternion4d q2,
			Quaternion4d q3)
		{
			q0 = q0 + q1;
			q0.Normalize();

			q2 = q2 + q1;
			q2.Normalize();

			q3 = q3 + q1;
			q3.Normalize();
			
			q1.Normalize();

			outA = q1 * Exp(-0.25 * (Ln(Exp(q1) * q2) + Ln(Exp(q1) * q0)));
			outB = q2 * Exp(-0.25 * (Ln(Exp(q2) * q3) + Ln(Exp(q2) * q1)));
			outC = q2;

		}
	}
}
