using System;
using WorldWind;

namespace Timeline
{
	internal sealed class ViewDirectionPlayer : PlayerBase
	{
		public ViewDirectionPlayer(IGlobe globe) : base(globe) {}
	
		override public void OnPulse(Timeline timeline, double time)
		{
			ViewDirection pvd = timeline.GetMostPreviousInTime(time) as ViewDirection;
			ViewDirection nvd = timeline.GetNextInTime(time) as ViewDirection;

			if (pvd != null)
			{
				if (pvd.SpecForm.Equals("angles"))
					this.playFromAngles(timeline, time, pvd, nvd);
				else if (pvd.SpecForm.Equals("position"))
					this.playFromPosition(timeline, time, pvd, nvd);
			}

		}

		private void playFromAngles(Timeline timeline, double time,
			ViewDirection pvd, ViewDirection nvd)
		{
			if (pvd != null && nvd != null && pvd.SpecForm.Equals(nvd.SpecForm))
			{
				double alpha = ScriptPlayer.ComputeAlpha(time - pvd.Time, pvd, nvd);
				double hSpan = nvd.HorizontalAngle - pvd.HorizontalAngle;
				double vSpan = nvd.VerticalAngle - pvd.VerticalAngle;

				double horizontal = pvd.HorizontalAngle + alpha * hSpan;
				double vertical = pvd.VerticalAngle + alpha * vSpan;

				this.globe.SetViewDirection(pvd.SpecForm, horizontal, vertical, 0);
			}
			else if (pvd != null)
			{
				this.globe.SetViewDirection(pvd.SpecForm, pvd.HorizontalAngle,
					pvd.VerticalAngle, 0);
			}
		}
		
		private void playFromPosition(Timeline timeline, double time,
			ViewDirection pvd, ViewDirection nvd)
		{
			if (pvd != null && nvd != null)
			{
				double alpha = ScriptPlayer.ComputeAlpha(time - pvd.Time, pvd, nvd);
				double span = (nvd.Longitude - pvd.Longitude) % 360;
				if (span >= 180)
					span -= 360;
				else if (span < -180)
					span += 360;

				double lon = pvd.Longitude + alpha * span;
				double lat = pvd.Latitude + alpha * (nvd.Latitude - pvd.Latitude);
				double ele = pvd.Elevation + alpha * (nvd.Elevation - pvd.Elevation);

				this.globe.SetViewDirection(pvd.SpecForm, lat, lon, ele);
			}
			else if (pvd != null)
			{
				this.globe.SetViewDirection(pvd.SpecForm, pvd.Latitude,
					pvd.Longitude, pvd.Elevation);
			}		
		}
	}
}