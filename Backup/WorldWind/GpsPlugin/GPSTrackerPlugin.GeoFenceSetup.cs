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
    public partial class GeoFenceSetup : Form
    {
        GPSGeoFence m_GpsGeoFence;
        GpsTrackerPlugin m_Plugin;

        public GeoFenceSetup(GPSGeoFence gpsGeoFence, GpsTrackerPlugin Plugin)
        {
            m_GpsGeoFence = gpsGeoFence;
            m_Plugin = Plugin;
            InitializeComponent();

            textBoxSoundFile.Text = GpsTrackerPlugin.m_sPluginDirectory + "\\GeoFence.wav";
            textBoxSoundFileOut.Text = GpsTrackerPlugin.m_sPluginDirectory + "\\GeoFenceOut.wav";

            comboBoxGeoFenceSource.Items.Add("All Gps Sources");
            for (int i = 0; i < m_Plugin.gpsTracker.m_gpsSourceList.Count; i++)
            {
                GPSSource gpsSource = (GPSSource)m_Plugin.gpsTracker.m_gpsSourceList[i];
                if (gpsSource.sType != "POI" &&
                    gpsSource.sType != "GeoFence" &&
                    gpsSource.bWaypoints == false &&
                    gpsSource.bSetup)
                    comboBoxGeoFenceSource.Items.Add(gpsSource.sDescription);
            }
            comboBoxGeoFenceSource.Text = "All Gps Sources";
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text == "")
            {
                MessageBox.Show("Please, enter a name for this Geo Fence...", "GpsTracker :: Geo Fence Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

            if ((checkBoxGeoFenceEmailIn.Checked || checkBoxGeoFenceEmailOut.Checked) &&
                textBoxEmailAddress.Text=="")
            {
                MessageBox.Show("Please, enter a valid Email Address for this Geo Fence...", "GpsTracker :: Geo Fence Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

            if (checkBoxGeoFenceSoundIn.Checked && textBoxSoundFile.Text == "")
            {
                MessageBox.Show("Please, enter a Sound File In path for this Geo Fence...", "GpsTracker :: Geo Fence Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

            if (checkBoxGeoFenceSoundOut.Checked && textBoxSoundFileOut.Text == "")
            {
                MessageBox.Show("Please, enter a Sound File Out path for this Geo Fence...", "GpsTracker :: Geo Fence Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

            m_GpsGeoFence.m_sDescription = textBoxName.Text;
            m_GpsGeoFence.m_sSource = comboBoxGeoFenceSource.Text;
            m_GpsGeoFence.m_sEmail = textBoxEmailAddress.Text;
            m_GpsGeoFence.m_sSound = textBoxSoundFile.Text;
            m_GpsGeoFence.m_sSoundOut = textBoxSoundFileOut.Text;
            m_GpsGeoFence.m_bEmailIn = checkBoxGeoFenceEmailIn.Checked;
            m_GpsGeoFence.m_bEmailOut = checkBoxGeoFenceEmailOut.Checked;
            m_GpsGeoFence.m_bSoundIn = checkBoxGeoFenceSoundIn.Checked;
            m_GpsGeoFence.m_bSoundOut = checkBoxGeoFenceSoundOut.Checked;
            m_GpsGeoFence.m_bMsgBoxIn = checkBoxGeoFenceMsgBoxIn.Checked;
            m_GpsGeoFence.m_bMsgBoxOut = checkBoxGeoFenceMsgBoxOut.Checked;
            Close();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
			OpenFileDialog dlgOpenFile = new OpenFileDialog();
			dlgOpenFile.Title = "Select Audio File (wav)" ;
			string sFilter;
			sFilter="Wav files (*.wav)|*.wav";
			dlgOpenFile.Filter = sFilter;
			dlgOpenFile.FilterIndex = 1 ;
			dlgOpenFile.RestoreDirectory = true ;
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                textBoxSoundFile.Text = dlgOpenFile.FileName;
            }
        }

        private void buttonBrowseOut_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Title = "Select Audio File (wav)";
            string sFilter;
            sFilter = "Wav files (*.wav)|*.wav";
            dlgOpenFile.Filter = sFilter;
            dlgOpenFile.FilterIndex = 1;
            dlgOpenFile.RestoreDirectory = true;
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                textBoxSoundFileOut.Text = dlgOpenFile.FileName;
            }
        }
    }
}