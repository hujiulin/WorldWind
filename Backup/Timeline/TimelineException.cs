using System;

namespace Timeline
{
	public class TimelineException : Exception
	{
		public TimelineException(String msg) : base(msg)
		{
		}

		public TimelineException(String msg, Exception e) : base(msg, e)
		{
		}
	}
}