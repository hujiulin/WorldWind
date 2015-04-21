using System;
using WorldWind;

namespace Timeline
{
	internal sealed class DisplayMessagesPlayer : PlayerBase
	{
		public DisplayMessagesPlayer(IGlobe globe) : base(globe) {}
	
		override public void OnPulse(Timeline timeline, double time)
		{
			DisplayMessages dms = timeline.GetMostPreviousInTime(time) as DisplayMessages;
			if (dms == null)
				return;

			this.globe.SetDisplayMessages(dms.Messages);
		}
	}
}