//----------------------------------------------------------------------------
// NAME: GPSTracker
// DEVELOPER: Javier Santoro
// WEBSITE: http://www.worldwindcentral.com/wiki/Add-on:GPS_Tracker_(plugin)
// VERSION: V04R00 (January 28, 2007)
//----------------------------------------------------------------------------

using System.Globalization;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.PluginEngine;
using System.Net;
using System.Net.Sockets;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Xml;
using System.Data;
using System.Collections;

namespace GpsTrackerPlugin
{
    //
    // This class holds all the information for rendering a GPS source
	public class GPSPositionVariables
	{
		public string	 m_sName;
		public string	 m_sComment;
		public int		 m_iAPRSIconTable;
		public int		 m_iAPRSIconCode;
		public GPSTrack  m_gpsTrack;
		public double  m_fLat;
		public double  m_fLon;
		public float  m_fSpeed;
		public float  m_fAlt;
		public float  m_fRoll;
		public float  m_fPitch;
		public float  m_fESpeed;
		public float  m_fNSpeed;
		public float  m_fVSpeed;
		public int  m_iHour;
		public int  m_iMin;
		public float  m_iSec;
		public int  m_iDay;
		public int  m_iMonth;
		public int  m_iYear;
		public string  m_sAltUnit;
		public string  m_sSpeedUnit;
		public float  m_fHeading;
		public float  m_fDepth;

		public GPSPositionVariables()
		{
		}
	}


}


