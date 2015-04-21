using System;

namespace Timeline
{
	internal abstract class TimelineElement : ScriptElement, IComparable
	{
		private double time = 0;

		public Double Time
		{
			get {return this.time;}
			set {this.time = value;}
		}

		public int CompareTo(object obj)
		{
			TimelineElement other = (TimelineElement)obj;
			return this.time.CompareTo(other.time);
		}
	}
}