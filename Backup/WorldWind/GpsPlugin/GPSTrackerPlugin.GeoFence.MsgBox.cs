//----------------------------------------------------------------------------
// NAME: GPSTracker
// DEVELOPER: Javier Santoro
// WEBSITE: http://www.worldwindcentral.com/wiki/Add-on:GPS_Tracker_(plugin)
// VERSION: V04R00 (January 28, 2007)
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace GpsTrackerPlugin
{
    public partial class GPSGeoFenceMsgBox : Form
    {
        bool m_bIn;
        GPSGeoFenceData m_GeoFence;
        GPSSource m_GpsSource;

        public GPSGeoFenceMsgBox(bool bIn, GPSGeoFenceData geoFence, GPSSource gpsSource)
        {
            m_bIn = bIn;
            m_GeoFence = geoFence;
            m_GpsSource = gpsSource;
            InitializeComponent();

            pictureBoxGeoFence.ImageLocation = m_GpsSource.sIconPath;
            labelMessage.Text = m_GpsSource.sDescription + " is ";
            if (bIn)
                labelMessage.Text += "inside ";
            else
                labelMessage.Text += "outside ";
            labelMessage.Text += "GeoFence Zone " + geoFence.sName;
            labelLatitude.Text = "";
            labelLongitude.Text = "";

            string sLocation = "";
            string sNS;
            if (m_GpsSource.GpsPos.m_fLat >= (float)0)
                sNS = "N";
            else
                sNS = "S";
            double dLat = Math.Abs(m_GpsSource.GpsPos.m_fLat);
            double dWhole = Math.Floor(dLat);
            double dFraction = dLat - dWhole;
            double dMin = dFraction * (double)60;
            double dMinWhole = Math.Floor(dMin);
            double dSeconds = (dMin - dMinWhole) * (double)60;
            int iDegrees = Convert.ToInt32(dWhole);
            int iMinutes = Convert.ToInt32(dMinWhole);
            float fSeconds = Convert.ToSingle(dSeconds);
            sLocation = Convert.ToString(iDegrees) + "°" + Convert.ToString(iMinutes) + "'" + Convert.ToString(fSeconds) + "\" " + sNS;
            labelLatitude.Text = sLocation;

            string sEW;
            if (m_GpsSource.GpsPos.m_fLon >= (float)0)
                sEW = "E";
            else
                sEW = "W";
            double dLon = Math.Abs(m_GpsSource.GpsPos.m_fLon);
            dWhole = Math.Floor(dLon);
            dFraction = dLon - dWhole;
            dMin = dFraction * (double)60;
            dMinWhole = Math.Floor(dMin);
            dSeconds = (dMin - dMinWhole) * (double)60;
            iDegrees = Convert.ToInt32(dWhole);
            iMinutes = Convert.ToInt32(dMinWhole);
            fSeconds = Convert.ToSingle(dSeconds);
            sLocation = Convert.ToString(iDegrees) + "°" + Convert.ToString(iMinutes) + "'" + Convert.ToString(fSeconds) + "\" " + sEW;
            labelLongitude.Text = sLocation;

            labelTime.Text = DateTime.Now.ToString();

            Thread threadMsg = new Thread(new ThreadStart(this.threadShowMsg));
            threadMsg.IsBackground = true;
            threadMsg.Priority = System.Threading.ThreadPriority.Normal;
            threadMsg.Start();
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        private void threadShowMsg()
        {
            ShowDialog();
        }

    }
}

/*
Form form = new Form();
form.ShowInTaskbar = false;
ShowWindow( form.Handle, SW_SHOWNOACTIVATE );

And you will need the following definitions:

using System.Runtime.InteropServices;
private const int SW_SHOWNOACTIVATE = 0x04;
[DllImport ("user32.dll")]
private static extern bool ShowWindow( IntPtr hWnd, int flags );
*/