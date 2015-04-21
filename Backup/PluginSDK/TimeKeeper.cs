using System;

namespace WorldWind
{
	/// <summary>
	/// Summary description for TimeKeeper.
	/// </summary>
	public class TimeKeeper
	{
		static System.DateTime m_currentTimeUtc = System.DateTime.Now.ToUniversalTime();
		static float m_timeMultiplier = 1.0f;
		static bool m_enabled = false;
		static System.Timers.Timer m_timer = null;
		static float m_interval = 15;

        public static event System.Timers.ElapsedEventHandler Elapsed;

		public static System.DateTime CurrentTimeUtc
		{
			get
			{
				return m_currentTimeUtc;
			}
			set
			{
				m_currentTimeUtc = value;
			}
		}

		public static bool Enabled
		{
			get{ return m_enabled; }
			set
			{
				if(value != m_enabled)
				{
					if(value)
					{
						Start();
					}
					else
					{
						Stop();
					}
				}
			}
		}

		public static float TimeMultiplier
		{
			get{ return m_timeMultiplier; }
			set{ m_timeMultiplier = value; }
		}

		public static void Start()
		{
			m_enabled = true;
			if(m_timer == null)
			{
				m_timer = new System.Timers.Timer(m_interval);
				m_timer.Elapsed += new System.Timers.ElapsedEventHandler(m_timer_Elapsed);
			}
			m_timer.Start();
		}

		public static void Stop()
		{
			m_enabled = false;
			if(m_timer != null)
				m_timer.Stop();
		}

		private static void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			m_currentTimeUtc += System.TimeSpan.FromMilliseconds( m_interval * m_timeMultiplier );

            if (Elapsed != null)
                Elapsed(sender, e);
		}
	}
}
