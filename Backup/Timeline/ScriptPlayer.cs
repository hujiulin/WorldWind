using System;
using WorldWind;

namespace Timeline
{
	public class ScriptPlayer
	{
		public enum StatusChange {ScriptStarted, ScriptStopped, ScriptEnded};
		public delegate void StatusChangeHandler(ScriptPlayer player, StatusChange change);

		private int timesToRepeat = 0;
		private int timesRepeated = 0;
		private bool isRunning = false;
		private Timelines timelines;
		System.Timers.Timer timer;
		private System.DateTime startTime;
		private IGlobe globe;
		public event StatusChangeHandler StatusChanged;

		public ScriptPlayer(String scriptFile, IGlobe globe)
		{
			System.Diagnostics.Debug.Assert(globe != null);
			System.Diagnostics.Debug.Assert(scriptFile != null);
			System.Diagnostics.Debug.Assert(!scriptFile.Equals(String.Empty));

			System.IO.FileInfo fi = new System.IO.FileInfo(scriptFile);
			if (!fi.Exists)
				throw new TimelineException("Specified script does not exist.");

			Script script = new Script(fi);
			System.Collections.IList atElements = script.AtElements;
			if (atElements == null || atElements.Count < 1)
				throw new TimelineException("Script file " + scriptFile
					+ " has no At elements.");

			this.globe = globe;
			this.timesToRepeat = script.RepeatCount;
			this.timelines = new Timelines(atElements);

			this.timer = new System.Timers.Timer(15);
			this.timer.AutoReset = true;
			this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timerTickHandler);

			this.createPlayers();
		}

		public void Start()
		{
			if (this.isRunning)
				this.Stop();

			this.isRunning = true;
			this.startTime = System.DateTime.Now;
			this.timelines.pulse(0);
			this.timer.Start();
			if (this.StatusChanged != null)
				this.StatusChanged(this, StatusChange.ScriptStarted);
		}

		public void Stop()
		{
			this.isRunning = false;
			this.timer.Stop();
			if (this.StatusChanged != null)
				this.StatusChanged(this, StatusChange.ScriptStopped);
		}

		public void EndScript()
		{
			this.Stop();
			if (this.StatusChanged != null)
				this.StatusChanged(this, StatusChange.ScriptEnded);
		}

		public void Repeat()
		{
			this.Start();
		}

		private void timerTickHandler(Object obj, System.Timers.ElapsedEventArgs eventArgs)
		{
			// Since the timer may call the handler even after it's turned off, the
			// state of the player must be checked before sending a pulse.
			if (!this.isRunning)
				return;

			try
			{
				System.TimeSpan elapsedTime = System.DateTime.Now.Subtract(this.startTime);
				double time = elapsedTime.TotalSeconds;
				if (time <= this.timelines.EndTime)
				{
					this.timelines.pulse(time);
				}
				else
				{
					++this.timesRepeated;
					if (this.timesRepeated < this.timesToRepeat
						|| this.timesToRepeat == 0)
					{
						this.Repeat();
					}
					else
					{
						this.EndScript();
					}
				}
			}
			catch (Exception e)
			{
				this.EndScript();
				throw e;
			}
		}

		private const double EPSILON = 0.00001; // smallest distinguishable time delta

		static internal double ComputeAlpha(double time,
			TimelineElement previous, TimelineElement next)
		{
			if (previous == null || next == null)
				return 1;
			
			double delta = next.Time - previous.Time;
			if ((delta - EPSILON) <= 0)
				return 1;

			return time / delta;
		}

		private void createPlayers()
		{
			System.Collections.ICollection timelineTypes = this.timelines.Keys;
			foreach (String tt in timelineTypes)
			{
				Timeline.Handler th = (Timeline.Handler)this.instancePlayerForTimeline(tt);
				if (th != null)
				{
					this.timelines.SetTimelineHandler(tt, th);
				}
			}
		}

		// Constant arguments for object instancing below.
		private static String ns = typeof(ScriptElement).Namespace + ".";
		private static Type[] argTypes = new Type[] {typeof(IGlobe)};

		private Object instancePlayerForTimeline(String timelineType)
		{
			try
			{
				String typeName = ns + timelineType + "Player";
				Type t = Type.GetType(typeName);
				if (t == null)
					return null;

				System.Reflection.ConstructorInfo ctor = t.GetConstructor(argTypes);
				if (ctor == null)
					return null;

				Object[] argValues = new Object[] {this.globe};
				return ctor.Invoke(argValues);
			}
			catch (Exception e)
			{
				throw new TimelineException("Error constructing script player for "
					+ timelineType + ".", e);
			}
		}
	}
}