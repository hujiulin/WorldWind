using System;
using WorldWind;

namespace Timeline
{
	internal class LayersPlayer : PlayerBase
	{
		public LayersPlayer(IGlobe globe) : base(globe)
		{
		}

		override public void OnPulse(Timeline timeline, double time)
		{
			Layers prev = timeline.GetMostPreviousInTime(time) as Layers;
			if (prev != null)
			{
//				System.Console.Write(time + " : ");
				this.globe.SetLayers(prev.LayerList);
			}
		}
	}	
}