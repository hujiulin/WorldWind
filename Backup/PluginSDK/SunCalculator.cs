using System;
using Microsoft.DirectX;
using WorldWind;

namespace WorldWind
{
	/// <summary>
	/// Summary description for SunCalculator.
	/// </summary>
	public class SunCalculator
	{
		public static Point3d GetGeocentricPosition(System.DateTime utcDateTime)
		{
            if (World.Settings.SunSynchedWithTime)
            {
                // Sun synched with time and date
                double JD = getJulianDay(utcDateTime);
                double T = (JD - 2451545.0) / 36525; // number of Julian centuries since Jan 1, 2000, 12 UT

                double k = Math.PI / 180.0;
                double M = 357.52910 + 35999.05030 * T - 0.0001559 * T * T - 0.00000048 * T * T * T; // mean anomaly, degree
                double L0 = 280.46645 + 36000.76983 * T + 0.0003032 * T * T; // mean longitude, degree
                double DL = (1.914600 - 0.004817 * T - 0.000014 * T * T) * Math.Sin(k * M) + (0.019993 - 0.000101 * T) * Math.Sin(k * 2 * M) + 0.000290 * Math.Sin(k * 3 * M);
                double L = L0 + DL; // true longitude, degree
                L = L % 360;
                //		obliquity eps of ecliptic:
                double eps = 23.0 + 26.0 / 60.0 + 21.448 / 3600.0 - (46.8150 * T + 0.00059 * T * T - 0.001813 * T * T * T) / 3600;

                double X = Math.Cos(k * L);
                double Y = Math.Cos(k * eps) * Math.Sin(k * L);
                double Z = Math.Sin(k * eps) * Math.Sin(k * L);

                double R = Math.Sqrt(1.0 - Z * Z);

                double dec = (180 / Math.PI) * Math.Atan(Z / R); // in degrees
                double RA = (24 / Math.PI) * Math.Atan(Y / (X + R)); // in hours

                double theta0 = 280.46061837 + 360.98564736629 * (JD - 2451545.0) + 0.000387933 * T * T - T * T * T / 38710000.0; // Greenwich Sidereal Time

                theta0 = theta0 % 360;
                RA *= 15; // convert from hours to degrees
                double tau = theta0 - RA;

                Point3d pos = MathEngine.SphericalToCartesianD(
                    Angle.FromDegrees(-dec),
                    Angle.FromDegrees(-(tau - 180)),
                    1);

                return pos;
            }
            else
            {
                // Fixed sun heading and elevation
                double worldRadius = 6378137;   // Earth meter
                Vector3 position = MathEngine.SphericalToCartesian(World.Settings.CameraLatitude, World.Settings.CameraLongitude, worldRadius);
                return GetGeocentricPosition(position, Angle.FromRadians(World.Settings.SunHeading), Angle.FromRadians(World.Settings.SunElevation), World.Settings.SunDistance);
            }
		}

		private static double getJulianDay(System.DateTime gregorianDate)
		{
			double d = gregorianDate.Day + 
				(double)gregorianDate.Hour / 24.0 + 
				(double)gregorianDate.Minute / (24.0 * 60.0) + 
				(double)gregorianDate.Second / (24.0 * 60.0 * 60.0) + 
				(double)gregorianDate.Millisecond / (24.0 * 60.0 * 60.0 * 1000.0);
			
			int m = (gregorianDate.Month < 3 ? gregorianDate.Month + 12 : gregorianDate.Month);
			int j = (gregorianDate.Month < 3 ? gregorianDate.Year - 1 : gregorianDate.Year);

			int c = 2 - (int)Math.Floor((double)(j / 100)) + (int)Math.Floor((double)(j / 400));
			return (int)Math.Floor((double)(1461 * (j + 4716)) / 4) + (int)Math.Floor((double)(153 * (m + 1) / 5)) + d + c - 1524.5;
		}

        // Compute Sun geocentric cartesian position given actual position on the globe, 
        // heading, elevation and sun distance
        public static Point3d GetGeocentricPosition(Vector3 position, Angle heading, Angle elevation, double sunDistance)
        {
            // Untransformed sun pos from globe center
            Vector3 sun = MathEngine.SphericalToCartesian(elevation, Angle.FromRadians(Math.PI - heading.Radians), sunDistance);
            // Now transform it to current location
            Vector3 pos = MathEngine.CartesianToSpherical(position.X, position.Y, position.Z);
            Matrix sunTrans = Matrix.Identity;
            sunTrans *= Matrix.Translation(0, 0, pos.X);                // radius pos
            sunTrans *= Matrix.RotationY((float)Math.PI / 2 - pos.Y);   // lat pos
            sunTrans *= Matrix.RotationZ(pos.Z);                        // lon pos
            sun.TransformCoordinate(sunTrans);
            return new Point3d(-sun.X, -sun.Y, -sun.Z);
        }
	}
}