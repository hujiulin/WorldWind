using System;
using WorldWind;

namespace Timeline
{
	internal class ViewPositionPlayer : PlayerBase
	{
		public ViewPositionPlayer(IGlobe globe) : base(globe)
		{
		}

		override public void OnPulse(Timeline timeline, double time)
		{
			ViewPosition pvp = timeline.GetMostPreviousInTime(time) as ViewPosition;
			ViewPosition nvp = timeline.GetNextInTime(time) as ViewPosition;

			if (pvp != null && nvp != null)
			{
				double alpha = ScriptPlayer.ComputeAlpha(time - pvp.Time, pvp, nvp);
				double span = (nvp.Longitude - pvp.Longitude) % 360;
				if (span >= 180)
					span -= 360;
				else if (span < -180)
					span += 360;

				double lon = pvp.Longitude + alpha * span;
				double lat = pvp.Latitude + alpha * (nvp.Latitude - pvp.Latitude);
				double ele = pvp.Elevation + alpha * (nvp.Elevation - pvp.Elevation);

				this.globe.SetViewPosition(lat, lon, ele);
			}
			else if (pvp != null)
			{
				this.globe.SetViewPosition(pvp.Latitude, pvp.Longitude, pvp.Elevation);
			}
		}
	}	
}