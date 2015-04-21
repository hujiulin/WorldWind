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

namespace GpsTrackerPlugin
{
    public partial class GpsSetup : Form
    {
        GPSIcon m_gpsIcon;
        GPSTrackLine m_gpsTrackLine;
        GPSGeoFence m_gpsFence;

        public GpsSetup(GPSIcon gpsIcon, GPSTrackLine gpsTrackLine, GPSGeoFence geoFence)
        {
            m_gpsIcon = gpsIcon;
            m_gpsTrackLine = gpsTrackLine;
            m_gpsFence = geoFence;

            InitializeComponent();

            if (m_gpsIcon!=null)
            {
                if (m_gpsIcon.m_RenderInfo.bPOI)
                {
                    checkBoxDistanceFromPOI.Enabled = true;
                    checkBoxDistanceFromPOI.Text = "Distance && bearing to " + m_gpsIcon.m_RenderInfo.sDescription + " from all Gps";
                    checkBoxDistanceFromPOI.Checked = m_gpsIcon.m_bSignalDistance;
                    checkBoxDistanceToPOI.Enabled = false;
                    checkBoxDistanceToPOI.Text = "Distance from " + m_gpsIcon.m_RenderInfo.sDescription + " to all POIs";
                    checkBoxTrackHeading.Enabled = false;
                    checkBoxTrackLine.Enabled = false;
                }
                else
                {
                    checkBoxDistanceFromPOI.Enabled = false;
                    checkBoxDistanceFromPOI.Text = "Distance && bearing to " + m_gpsIcon.m_RenderInfo.sDescription + " from all Gps";
                    checkBoxDistanceToPOI.Enabled = true;
                    checkBoxDistanceToPOI.Text = "Distance from " + m_gpsIcon.m_RenderInfo.sDescription + " to all POIs";
                    checkBoxDistanceToPOI.Checked = m_gpsIcon.m_bSignalDistance;
                }

                checkBoxInformationText.Checked = m_gpsIcon.m_bShowInfo;
                checkBoxTrackHeading.Checked = m_gpsIcon.m_bTrackHeading;
                checkBoxTrackLine.Checked = m_gpsIcon.m_bTrackLine;
                labelTitle.Text = "Set options for " + m_gpsIcon.m_RenderInfo.sDescription;
            }
            else
            if (m_gpsTrackLine != null)
            {
                checkBoxInformationText.Checked = m_gpsTrackLine.m_bShowInfo;
                checkBoxTrackHeading.Enabled = false;
                checkBoxTrackLine.Enabled = false;
                checkBoxDistanceFromPOI.Text = "Distance && bearing to " + m_gpsTrackLine.m_sDescription + " from all Gps";
                checkBoxDistanceFromPOI.Enabled = false;
                checkBoxDistanceToPOI.Text = "Distance from " + m_gpsTrackLine.m_sDescription + " to all POIs";
                checkBoxDistanceToPOI.Enabled = false;
                labelTitle.Text = "Set options for " + m_gpsIcon.m_RenderInfo.sDescription;
            }
            else
                if (m_gpsFence != null)
            {
                checkBoxInformationText.Checked = m_gpsFence.m_bShowInfo;
                checkBoxTrackHeading.Enabled = false;
                checkBoxTrackLine.Enabled = false;
                checkBoxDistanceFromPOI.Text = "Distance && bearing to " + m_gpsFence.m_sDescription + " from all Gps";
                checkBoxDistanceFromPOI.Enabled = false;
                checkBoxDistanceToPOI.Text = "Distance from " + m_gpsFence.m_sDescription + " to all POIs";
                checkBoxDistanceToPOI.Enabled = false;
                labelTitle.Text = "Set options for " + m_gpsFence.m_sDescription;
            }

            
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            if (m_gpsIcon!=null)
            {
                if (m_gpsIcon.m_RenderInfo.bPOI)
                    m_gpsIcon.m_bSignalDistance = checkBoxDistanceFromPOI.Checked;
                else
                    m_gpsIcon.m_bSignalDistance = checkBoxDistanceToPOI.Checked;

                m_gpsIcon.m_bShowInfo = checkBoxInformationText.Checked;
                m_gpsIcon.m_bTrackHeading = checkBoxTrackHeading.Checked;
                m_gpsIcon.m_bTrackLine = checkBoxTrackLine.Checked;
            }
            else
            if (m_gpsTrackLine != null)
            {
                m_gpsTrackLine.m_bShowInfo = checkBoxInformationText.Checked;
            }
            else
            if (m_gpsFence != null)
            {
                m_gpsFence.m_bShowInfo = checkBoxInformationText.Checked;
            }
            Close();
        }

        private void GpsSetup_Load(object sender, EventArgs e)
        {
            this.Top = Control.MousePosition.Y;
            this.Left = Control.MousePosition.X;

            if (this.Top + this.Height > Screen.PrimaryScreen.WorkingArea.Bottom)
                this.Top = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - 5;

            if (this.Left + this.Width > Screen.PrimaryScreen.WorkingArea.Right)
                this.Left = Screen.PrimaryScreen.WorkingArea.Right - this.Width - 5;
        }
    }
}