using System;

namespace Timeline
{
	internal sealed class Timelines
	{
		private double endTime = 0;
		private System.Collections.Hashtable timelines =
			new System.Collections.Hashtable();

		public Timelines(System.Collections.IList atElements)
		{
			foreach (At at in atElements)
			{
				if (at.Time > this.endTime)
				{
					this.endTime = at.Time;
				}

				System.Collections.IDictionaryEnumerator paramsIter =
					at.Parameters.GetEnumerator();
				while (paramsIter.MoveNext())
				{
					Timeline timeline = (Timeline)this.timelines[paramsIter.Key];
					if (timeline == null)
					{
						timeline = this.createTimeline(paramsIter.Key);
					}
					timeline.Add((TimelineElement)paramsIter.Value);
				}
			}
		}

		public double EndTime
		{
			get {return this.endTime;}
		}

		public System.Collections.ICollection Keys
		{
			get {return this.timelines.Keys;}
		}

		internal void SetTimelineHandler(Object key, Timeline.Handler handler)
		{
			Timeline t = (Timeline)this.timelines[key];
			if (t != null)
			{
				t.TimelineHandler = handler;
			}
		}

		private Timeline createTimeline(Object key)
		{
			Timeline timeline = new Timeline();
			this.timelines.Add(key, timeline);
			return timeline;
		}

		internal void pulse(double time)
		{
			foreach (Timeline te in this.timelines.Values)
			{
				te.Pulse(time);
			}
		}
	}
}