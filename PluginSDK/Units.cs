using System;
using System.Collections.Generic;
using System.Text;

namespace WorldWind
{
    public enum Units
    {
        English,
        Metric
    }

    public static class ConvertUnits
    {
        public static string GetDisplayString(double distance)
        {
            if (World.Settings.DisplayUnits == Units.Metric)
            {
                if (distance >= 1000)
                {
                    return string.Format("{0:,.0} km", distance / 1000);
                }
                else
                {
                    return string.Format("{0:f0} m", distance);
                }
            }
            else
            {
                double feetPerMeter = 3.2808399;
                double feetPerMile = 5280;

                distance *= feetPerMeter;

                if (distance >= feetPerMile)
                {
                    return string.Format("{0:,.0} miles", distance / feetPerMile);
                }
                else
                {
                    return string.Format("{0:f0} ft", distance);
                }
            }
        }
    }
}
