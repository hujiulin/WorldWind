//----------------------------------------------------------------------------
// NAME: GPSTracker
// DEVELOPER: Javier Santoro
// WEBSITE: http://www.worldwindcentral.com/wiki/Add-on:GPS_Tracker_(plugin)
// VERSION: V04R00 (January 28, 2007)
//----------------------------------------------------------------------------

using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using System.Collections;

namespace GpsTrackerPlugin
{
    //
    //This class holds all the user interface entered information about a GPS source
    public class GPSGeoFenceData
    {
        public string sSource;
        public string sName;
        public ArrayList arrayLat;
        public ArrayList arrayLon;
        public string sEmail;
        public bool bEmailIn;
        public bool bEmailOut;
        public string sSound;
        public string sSoundOut;
        public bool bSoundIn;
        public bool bSoundOut;
        public bool bMsgBoxIn;
        public bool bMsgBoxOut;
        public bool bShowInfo;
        public ArrayList SourcesIn;
        public ArrayList SourcesOut;

    }

	public class GPSSource
	{
		//Common
		public int					iStartAltitud;
		public int					iNameIndex;
		public int					iIndex;
		public string				sType; //COM, UDP, TCP, FILE, APRS, POI
		public string				sDescription;
		public string				sComment;
		public string				sIconPath;
		public System.Drawing.Color colorTrack;
		public double				fLat;
		public double				fLon;
		public bool					bTrack;
		public bool					bSetup;
		public TreeNode				treeNode;
		public bool					bNeedApply;
		public bool					bSave;
        public GPSPositionVariables GpsPos;
        public AutoResetEvent       eventThreadSync;

		//USB
		public string	sUSBDevice;
		public Thread   usbThread;
		//COM
		public int		iCOMPort;
		public int		iBaudRate;
		public int		iByteSize;
		public int		iSelectedItem;
		public int		iParity;
		public int		iStopBits;
		public int		iFlowControl;
        public SerialPort GpsCOM;
        public string sCOMData;
		
		//UDP
		public int		iUDPPort;

		//TCP
		public string	sTCPAddress;
		public int		iTCPPort;
		public bool		bSecureSocket;
        public TCPSockets tcpSockets;

		//File
		public string	sFileName;
		public bool		bNoDelay;
		public bool		bTrackAtOnce;
		public int		iPlaySpeed;
		public int		iReload;
		public bool		bSession;
		public bool		bForcePreprocessing;
		public bool		bWaypoints;
        public Thread   fileThread;
		public string[]	saGpsBabelFormat;
		public bool		bBabelNMEA;
		public DateTime datePosition;

		//Session
		public int		iFilePlaySpeed;
		public string	sFileNameSession;

		//APRS
		public int		iServerIndex;
		public string	sAPRSServerURL;
		public string	sCallSign;
		public int		iRefreshRate;
		public string   sCallSignFilter;
		public string   [] sCallSignFilterLines;
        public Thread   aprsThread;

		//POI
		public bool		bPOISet;

        //NMEA export
        public StreamWriter swExport;
        public bool bNMEAExport;

        //GeoFence
        public GPSGeoFenceData GeoFence;


	    public GPSSource()
	    {
            GpsPos = new GPSPositionVariables();
            GeoFence = new GPSGeoFenceData();
            GeoFence.arrayLat = new ArrayList();
            GeoFence.arrayLon = new ArrayList();
            GeoFence.SourcesIn = new ArrayList();
            GeoFence.SourcesOut = new ArrayList();
            GeoFence.sEmail = "";
            GeoFence.sName = "";
            GeoFence.sSound = GpsTrackerPlugin.m_sPluginDirectory + "\\GeoFence.wav";
            GeoFence.sSoundOut = GpsTrackerPlugin.m_sPluginDirectory + "\\GeoFenceOut.wav";
            GeoFence.bEmailIn=false;
            GeoFence.bEmailOut=false;
            GeoFence.bMsgBoxIn=false;
            GeoFence.bMsgBoxOut=false;
            GeoFence.bSoundIn=true;
            GeoFence.bSoundOut=true;
	    }
	}


}



