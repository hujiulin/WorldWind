using System;
using WorldWind;

namespace Timeline
{
	internal abstract class PlayerBase : Timeline.Handler
	{
		protected IGlobe globe;

		protected PlayerBase(IGlobe globe)
		{
			this.globe = globe;
		}

		abstract public void OnPulse(Timeline timeline, double time);
	}
}