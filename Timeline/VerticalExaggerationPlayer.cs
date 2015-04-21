using System;
using WorldWind;

namespace Timeline
{
	internal sealed class VerticalExaggerationPlayer : PlayerBase
	{
		public VerticalExaggerationPlayer(IGlobe globe) : base(globe) {}

		override public void OnPulse(Timeline timeline, double time)
		{
			VerticalExaggeration pve =
				timeline.GetMostPreviousInTime(time) as VerticalExaggeration;
			VerticalExaggeration nve =
				timeline.GetNextInTime(time) as VerticalExaggeration;

			if (pve != null && nve != null)
			{
				double alpha = ScriptPlayer.ComputeAlpha(time - pve.Time, pve, nve);
				double span = nve.Exaggeration - pve.Exaggeration;
				double ve = pve.Exaggeration + alpha * span;

				this.globe.SetVerticalExaggeration(ve);
			}
			else if (pve != null)
			{
				this.globe.SetVerticalExaggeration(pve.Exaggeration);
			}
		}	
	}
}