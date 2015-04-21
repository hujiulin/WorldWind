using System;
using WorldWind;

namespace Timeline
{
	internal sealed class LatLonGridPlayer : TogglePlayerBase
	{
		public LatLonGridPlayer(IGlobe globe) : base(globe) {}

		protected override void DoPulse(bool toggleValue)
		{
			this.globe.SetLatLonGridShow(toggleValue);
		}
	}
}