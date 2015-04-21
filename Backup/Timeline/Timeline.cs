using System;

namespace Timeline
{
	internal sealed class Timeline
	{
		public interface Handler
		{
			void OnPulse(Timeline timeline, double time);
		}

		private Handler handler = null;
		private System.Collections.ArrayList elements =
			new System.Collections.ArrayList();

		public Handler TimelineHandler
		{
			get {return this.handler;}
			set {this.handler = value;}
		}

		public void Add(TimelineElement param)
		{
			this.elements.Add(param);
			this.elements.Sort(); // keep the list sorted by time
		}

		public void Pulse(double time)
		{
			if (this.handler == null)
				return;

			this.handler.OnPulse(this, time);
		}

		public TimelineElement GetMostPreviousInTime(double time)
		{
			TimelineElement previous = null;

			foreach (TimelineElement te in this.elements)
			{
				if (te.Time <= time)
					previous = te;
				else
					break; // we're now beyond entries prior to the input time
			}

			return previous;
		}

		public TimelineElement GetNextInTime(double time)
		{
			TimelineElement next = null;

			foreach (TimelineElement te in this.elements)
			{
				if (te.Time > time)
				{
					next = te;
					break;
				}
			}

			return next;
		}
	}
}