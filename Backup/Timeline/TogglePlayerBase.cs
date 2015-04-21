using System;
using WorldWind;

namespace Timeline
{
	internal abstract class TogglePlayerBase : PlayerBase
	{
		public TogglePlayerBase(IGlobe globe) : base(globe)
		{
		}

		override public void OnPulse(Timeline timeline, double time)
		{
			ToggleBase toggle = timeline.GetMostPreviousInTime(time) as ToggleBase;
			if (toggle != null)
			{
				this.DoPulse(toggle.ToggleValue);
			}
		}

		abstract protected void DoPulse(bool toggleValue);
	}
}