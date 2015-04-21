using System;
using WorldWind;

namespace Timeline
{
	internal sealed class WmsImagePlayer : PlayerBase
	{
		public WmsImagePlayer(IGlobe globe) : base(globe) {}
	
		override public void OnPulse(Timeline timeline, double time)
		{
			WmsImage wi = timeline.GetMostPreviousInTime(time) as WmsImage;
			if (wi == null)
				return;

			this.globe.SetWmsImage(wi.Descriptor, null, 0);
		}
	}
}