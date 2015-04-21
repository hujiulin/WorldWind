//----------------------------------------------------------------------------
// NAME: Time Controller
// VERSION: 1.0
// DESCRIPTION: Time Controller 
// DEVELOPER: Chris Maxwell - NASA World Wind Project
// WEBSITE: http://worldwind.arc.nasa.gov
// REFERENCES: 
//----------------------------------------------------------------------------
//
// 
//

using System;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using WorldWind.Renderable;
using System.Windows.Forms;
using Utility;

namespace NASA.Plugins
{
    public class TimeController : WorldWind.PluginEngine.Plugin
    {
        string basePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        WorldWind.NewWidgets.FormWidget m_window = null;
        WorldWind.NewWidgets.PictureBox time = null;
        WorldWind.NewWidgets.PictureBox play = null;
        WorldWind.NewWidgets.PictureBox close = null;
        WorldWind.NewWidgets.PictureBox pause = null;
        WorldWind.NewWidgets.PictureBox slow = null;
        WorldWind.NewWidgets.PictureBox fast = null;
        WorldWind.NewWidgets.PictureBox rewind = null;
        WorldWind.NewWidgets.TextLabel timeLabel = null;
        WorldWind.NewWidgets.WidgetMenuButton m_toolbarItem = null;

        public override void Load()
        {
            try
            {
                m_window = new WorldWind.NewWidgets.FormWidget("Time Control");
                m_window.Name = "Time Control";
                m_window.ClientSize = new System.Drawing.Size(300, 183);

                // Bug in FormWidget required anchor to be set before Location and parent widget
                m_window.Anchor = WorldWind.NewWidgets.WidgetEnums.AnchorStyles.Left | WorldWind.NewWidgets.WidgetEnums.AnchorStyles.Bottom;
                m_window.ParentWidget = DrawArgs.NewRootWidget;
                m_window.Location = new System.Drawing.Point(0, DrawArgs.NewRootWidget.ClientSize.Height - 183);
                m_window.Text = "Double Click to Re-Open";
                m_window.BorderEnabled = false;
                m_window.AutoHideHeader = true;
                m_window.BackgroundColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
                m_window.HeaderEnabled = true;
                m_window.Visible = false;

                time = new WorldWind.NewWidgets.PictureBox();
                time.Name = "Time";
                time.ImageUri = basePath + "\\Data\\Icons\\Time\\time off.png";
                time.ClientLocation = new System.Drawing.Point(12, 59);
                time.ClientSize = new System.Drawing.Size(42, 42);
                time.Visible = true;
                time.ParentWidget = m_window;
                time.OnMouseEnterEvent += new EventHandler(time_OnMouseEnterEvent);
                time.OnMouseLeaveEvent += new EventHandler(time_OnMouseLeaveEvent);
                time.OnMouseUpEvent += new MouseEventHandler(time_OnMouseUpEvent);
                time.CountHeight = false;
                time.CountWidth = true;
                m_window.ChildWidgets.Add(time);

                play = new WorldWind.NewWidgets.PictureBox();
                play.Name = "Play";
                if (TimeKeeper.Enabled)
                {
                    play.ImageUri = basePath + "\\Data\\Icons\\Time\\play on.png";
                }
                else
                {
                    play.ImageUri = basePath + "\\Data\\Icons\\Time\\play off.png";
                }
                play.ClientLocation = new System.Drawing.Point(50, 1);
                play.ClientSize = new System.Drawing.Size(82, 82);
                play.Visible = true;
                play.ParentWidget = m_window;
                play.OnMouseEnterEvent += new EventHandler(play_OnMouseEnterEvent);
                play.OnMouseLeaveEvent += new EventHandler(play_OnMouseLeaveEvent);
                play.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(play_OnMouseUpEvent);
                play.CountHeight = true;
                play.CountWidth = true;
                m_window.ChildWidgets.Add(play);

                close = new WorldWind.NewWidgets.PictureBox();
                close.Name = "Close";
                close.ImageUri = basePath + "\\Data\\Icons\\Time\\close off.png";
                close.ClientLocation = new System.Drawing.Point(29, 3);
                close.ClientSize = new System.Drawing.Size(22, 22);
                close.Visible = true;
                close.ParentWidget = m_window;
                close.OnMouseEnterEvent += new EventHandler(close_OnMouseEnterEvent);
                close.OnMouseLeaveEvent += new EventHandler(close_OnMouseLeaveEvent);
                close.OnMouseUpEvent += new MouseEventHandler(close_OnMouseUpEvent);
                close.CountHeight = false;
                close.CountWidth = false;
                m_window.ChildWidgets.Add(close);

                rewind = new WorldWind.NewWidgets.PictureBox();
                rewind.Name = "Rewind";
                rewind.ImageUri = basePath + "\\Data\\Icons\\Time\\repeat off.png";
                rewind.ClientLocation = new System.Drawing.Point(16, 26);
                rewind.ClientSize = new System.Drawing.Size(32, 32);
                rewind.Visible = true;
                rewind.ParentWidget = m_window;
                rewind.OnMouseEnterEvent += new EventHandler(rewind_OnMouseEnterEvent);
                rewind.OnMouseLeaveEvent += new EventHandler(rewind_OnMouseLeaveEvent);
                rewind.OnMouseUpEvent += new MouseEventHandler(rewind_OnMouseUpEvent);
                rewind.CountHeight = false;
                rewind.CountWidth = false;
                m_window.ChildWidgets.Add(rewind);

                pause = new WorldWind.NewWidgets.PictureBox();
                pause.Name = "Pause";
                if (TimeKeeper.Enabled)
                {
                    pause.ImageUri = basePath + "\\Data\\Icons\\Time\\pause off.png";
                }
                else
                {
                    pause.ImageUri = basePath + "\\Data\\Icons\\Time\\pause on.png";
                }
                pause.ClientLocation = new System.Drawing.Point(35, 88);
                pause.ClientSize = new System.Drawing.Size(64, 64);
                pause.Visible = true;
                pause.ParentWidget = m_window;
                pause.OnMouseEnterEvent += new EventHandler(pause_OnMouseEnterEvent);
                pause.OnMouseLeaveEvent += new EventHandler(pause_OnMouseLeaveEvent);
                pause.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(pause_OnMouseUpEvent);
                pause.CountHeight = true;
                pause.CountWidth = false;
                m_window.ChildWidgets.Add(pause);

                slow = new WorldWind.NewWidgets.PictureBox();
                slow.Name = "Slow";
                slow.ImageUri = basePath + "\\Data\\Icons\\Time\\slow off.png";
                slow.ClientLocation = new System.Drawing.Point(97, 88);
                slow.ClientSize = new System.Drawing.Size(64, 64);
                slow.Visible = true;
                slow.ParentWidget = m_window;
                slow.OnMouseEnterEvent += new EventHandler(slow_OnMouseEnterEvent);
                slow.OnMouseLeaveEvent += new EventHandler(slow_OnMouseLeaveEvent);
                slow.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(slow_OnMouseUpEvent);
                slow.CountHeight = false;
                slow.CountWidth = false;
                m_window.ChildWidgets.Add(slow);

                fast = new WorldWind.NewWidgets.PictureBox();
                fast.Name = "Fast";
                fast.ImageUri = basePath + "\\Data\\Icons\\Time\\fast off.png";
                fast.ClientLocation = new System.Drawing.Point(158, 88);
                fast.ClientSize = new System.Drawing.Size(64, 64);
                fast.Visible = true;
                fast.ParentWidget = m_window;
                fast.OnMouseEnterEvent += new EventHandler(fast_OnMouseEnterEvent);
                fast.OnMouseLeaveEvent += new EventHandler(fast_OnMouseLeaveEvent);
                fast.OnMouseUpEvent += new System.Windows.Forms.MouseEventHandler(fast_OnMouseUpEvent);
                fast.CountHeight = false;
                fast.CountWidth = false;
                m_window.ChildWidgets.Add(fast);

                timeLabel = new WorldWind.NewWidgets.TextLabel();
                timeLabel.Name = "Current Time";
                timeLabel.ClientLocation = new System.Drawing.Point(134, 65);
                timeLabel.ClientSize = new System.Drawing.Size(140, 60);
                timeLabel.Visible = true;
                timeLabel.ParentWidget = m_window;
                timeLabel.Text = GetCurrentTimeString();
                TimeKeeper.Elapsed += new System.Timers.ElapsedEventHandler(TimeKeeper_Elapsed);
                timeLabel.CountHeight = false;
                timeLabel.CountWidth = true;
                timeLabel.WordBreak = true;
                m_window.ChildWidgets.Add(timeLabel);

                DrawArgs.NewRootWidget.ChildWidgets.Add(m_window);

                m_toolbarItem = new WorldWind.NewWidgets.WidgetMenuButton(
                    "Time Controller",
                    basePath + "\\Data\\Icons\\Time\\time off.png",
                    m_window);

                ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_toolbarItem);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            base.Load();
        }

        TimeSetterDialog m_timeSetterDialog = null;
        void time_OnMouseUpEvent(object sender, MouseEventArgs e)
        {
            if (m_timeSetterDialog == null)
            {
                m_timeSetterDialog = new TimeSetterDialog();
            }
            m_timeSetterDialog.DateTimeUtc = TimeKeeper.CurrentTimeUtc;
            m_timeSetterDialog.ShowDialog();

        }

        void time_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            time.ImageUri = basePath + "\\Data\\Icons\\Time\\time off.png";
        }

        void time_OnMouseEnterEvent(object sender, EventArgs e)
        {
            time.ImageUri = basePath + "\\Data\\Icons\\Time\\time on.png";
        }

        void TimeKeeper_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timeLabel.Text = GetCurrentTimeString();
        }

        string GetCurrentTimeString()
        {
            System.DateTime localTime = TimeKeeper.CurrentTimeUtc.ToLocalTime();

            return string.Format("{0:D4}-{1:D2}-{2:D2}\n{3:D2}:{4:D2}:{5:D2} (x{6})",
                localTime.Year,
                localTime.Month,
                localTime.Day,
                localTime.Hour,
                localTime.Minute,
                localTime.Second,
                TimeKeeper.TimeMultiplier);
        }

        void play_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!TimeKeeper.Enabled)
            {
                TimeKeeper.Enabled = true;
                pause.ImageUri = basePath + "\\Data\\Icons\\Time\\pause off.png";
            }
        }

        void pause_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            TimeKeeper.Enabled = !TimeKeeper.Enabled;
            if (TimeKeeper.Enabled)
            {
                play.ImageUri = basePath + "\\Data\\Icons\\Time\\play on.png";
            }
            else
            {
                play.ImageUri = basePath + "\\Data\\Icons\\Time\\play off.png";
            }
        }

        void slow_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (TimeKeeper.TimeMultiplier == 0 || (TimeKeeper.TimeMultiplier <= 1 && TimeKeeper.TimeMultiplier > 0))
            {
                TimeKeeper.TimeMultiplier = -1;
            }
            else if (TimeKeeper.TimeMultiplier > 0)
            {
                TimeKeeper.TimeMultiplier *= 0.1f;
            }
            else
            {
                TimeKeeper.TimeMultiplier *= 10f;
            }
            timeLabel.Text = GetCurrentTimeString();
        }

        void fast_OnMouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (TimeKeeper.TimeMultiplier == 0 || (TimeKeeper.TimeMultiplier >= -1 && TimeKeeper.TimeMultiplier < 0))
            {
                TimeKeeper.TimeMultiplier = 1;
            }
            else if (TimeKeeper.TimeMultiplier > 0)
            {
                TimeKeeper.TimeMultiplier *= 10;
            }
            else
            {
                TimeKeeper.TimeMultiplier *= 0.1f;
            }
            timeLabel.Text = GetCurrentTimeString();
        }

        void fast_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            fast.ImageUri = basePath + "\\Data\\Icons\\Time\\fast off.png";
        }

        void fast_OnMouseEnterEvent(object sender, EventArgs e)
        {
            fast.ImageUri = basePath + "\\Data\\Icons\\Time\\fast on.png";
        }

        void slow_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            slow.ImageUri = basePath + "\\Data\\Icons\\Time\\slow off.png";
        }

        void slow_OnMouseEnterEvent(object sender, EventArgs e)
        {
            slow.ImageUri = basePath + "\\Data\\Icons\\Time\\slow on.png";
        }

        void pause_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            if (TimeKeeper.Enabled)
            {
                pause.ImageUri = basePath + "\\Data\\Icons\\Time\\pause off.png";
            }
        }

        void pause_OnMouseEnterEvent(object sender, EventArgs e)
        {
            pause.ImageUri = basePath + "\\Data\\Icons\\Time\\pause on.png";
        }

        void close_OnMouseEnterEvent(object sender, EventArgs e)
        {
            close.ImageUri = basePath + "\\Data\\Icons\\Time\\close on.png";
        }

        void close_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            close.ImageUri = basePath + "\\Data\\Icons\\Time\\close off.png";
        }

        void close_OnMouseUpEvent(object sender, EventArgs e)
        {
            m_window.Visible = false;
        }

        void rewind_OnMouseEnterEvent(object sender, EventArgs e)
        {
            rewind.ImageUri = basePath + "\\Data\\Icons\\Time\\repeat on.png";
        }

        void rewind_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            rewind.ImageUri = basePath + "\\Data\\Icons\\Time\\repeat off.png";
        }

        void rewind_OnMouseUpEvent(object sender, EventArgs e)
        {
            TimeKeeper.CurrentTimeUtc = DateTime.UtcNow;
            timeLabel.Text = GetCurrentTimeString();
        }


        void play_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            if (!TimeKeeper.Enabled)
            {
                play.ImageUri = basePath + "\\Data\\Icons\\Time\\play off.png";
            }
        }

        void play_OnMouseEnterEvent(object sender, EventArgs e)
        {
            play.ImageUri = basePath + "\\Data\\Icons\\Time\\play on.png";
        }

        public override void Unload()
        {
            if (m_window != null)
            {
                for (int i = 0; i < DrawArgs.RootWidget.ChildWidgets.Count; i++)
                {
                    if (DrawArgs.RootWidget.ChildWidgets[i].Name == m_window.Name)
                    {
                        DrawArgs.RootWidget.ChildWidgets.RemoveAt(i);
                        m_window = null;
                        break;
                    }
                }
            }

            if (m_toolbarItem != null)
            {
                ParentApplication.WorldWindow.MenuBar.RemoveToolsMenuButton(m_toolbarItem);
                m_toolbarItem.Dispose();
                m_toolbarItem = null;
            }
            base.Unload();
        }
    }
}