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
using System.Text;



namespace GpsTrackerPlugin
{
    //
    //This class will parse APRS messages from all sources
	public class GpsTrackerAPRS
	{
		GpsTracker m_GpsTracker;

		public GpsTrackerAPRS(GpsTracker gpsTracker)
		{
			m_GpsTracker=gpsTracker;
			//Ensure we load the APRS parser dll from the correct place
            LoadLibrary(GpsTrackerPlugin.m_sPluginDirectory + "\\APRS\\APRSParserDLL.GPSTracker");
		}

		public bool Parse(string sMsg, int iIndex)
		{
			bool bRet=false;

			//Init APRS structures
			APRSPOSITION aprsPosition = new APRSPOSITION();
			APRSSTATUS aprsStatus = new APRSSTATUS();
     		aprsPosition.source = new string(' ',10);
			aprsPosition.sentence = new string(' ',6);
            aprsPosition.name = new string(' ', 10);
			aprsStatus.source = new string(' ',10);
			aprsStatus.comment = new string(' ',256);

			if (APRSParse(sMsg , ref aprsPosition, ref aprsStatus)>=0)
			{
				bRet=false;

				GPSSource gpsSource=(GPSSource)m_GpsTracker.m_gpsSourceList[iIndex];
				for (int i=0; i<gpsSource.sCallSignFilterLines.Length-1; i++)
				{
					string source=aprsPosition.source;
					string sourceFilter=gpsSource.sCallSignFilterLines[i];

					int iWildIndex=sourceFilter.LastIndexOf('*');
					if (iWildIndex==0)
					{
						bRet=true;
						break;
					}
					else
						if (iWildIndex>=1)
					{
						sourceFilter=sourceFilter.Substring(0,iWildIndex);
						if (source.ToUpper().StartsWith(sourceFilter.ToUpper()))
						{
							bRet=true;
							break;
						}
					}
					else
					if (iWildIndex==-1 && source.ToUpper()==sourceFilter.ToUpper())
					{
						bRet=true;
						break;
					}
				}


				if (gpsSource.sCallSignFilterLines.Length<=1)
					bRet=true;

				if (bRet)
				{
                    gpsSource.GpsPos.m_fLat = Convert.ToSingle(aprsPosition.latitude);
                    gpsSource.GpsPos.m_fLon = Convert.ToSingle(aprsPosition.longitude);
                    gpsSource.GpsPos.m_fAlt = aprsPosition.altitude;
                    gpsSource.GpsPos.m_fSpeed = aprsPosition.speed_over_ground;

                    gpsSource.GpsPos.m_sName = aprsPosition.source;
                    gpsSource.GpsPos.m_sComment = aprsStatus.comment;
                    gpsSource.GpsPos.m_iAPRSIconCode = Convert.ToInt32(aprsStatus.symbol_code);
                    gpsSource.GpsPos.m_iAPRSIconTable = Convert.ToInt32(aprsStatus.symbol_table);
				}
			}

			return bRet;

		}


		//
		//sCallSign is the call sign get data from
		//use * as wild card (only at the end of the call sign):
		//eg: sCallSign=N9UMJ-15
		//eg: sCallSign=N9UMJ*
		//eg: sCallSign=* - get data from all available call signs
		public void threadAPRSIS()
		{
			int iIndex= Int32.Parse(Thread.CurrentThread.Name); 
			GPSSource gpsSource=(GPSSource)m_GpsTracker.m_gpsSourceList[iIndex];
			string sCallSign=gpsSource.sCallSign;
			int iRefresh=gpsSource.iRefreshRate;

			while (sCallSign != null && sCallSign.Length > 0 && m_GpsTracker.m_fCloseThreads==false)
			{
				try
				{
					HttpWebRequest request=null;
					request = (HttpWebRequest)WebRequest.Create(gpsSource.sAPRSServerURL.Trim() + sCallSign);
					
					HttpWebResponse response=null;
					// execute the request
					response = (HttpWebResponse)request.GetResponse();
	 
					// we will read data via the response stream
					Stream resStream = response.GetResponseStream();
                    resStream.ReadTimeout = 60000;
					int iRead;
					byte[] byChar = new byte[1];
					StringBuilder sbMsg = new StringBuilder("");

					do
					{
						iRead = resStream.ReadByte();
						if (iRead >= 0)
						{
							byChar[0] = Convert.ToByte(iRead);
							if (byChar[0] != '\r' && byChar[0] != '\n')
								sbMsg.Append(Encoding.ASCII.GetString(byChar));
							else
							{
								string sMsg = sbMsg.ToString();


								try
								{
									if (m_GpsTracker.m_MessageMonitor!=null)
										m_GpsTracker.m_MessageMonitor.AddMessageUnfilteredAPRS(sMsg);
								}
								catch (Exception)
								{
									m_GpsTracker.m_MessageMonitor=null;
								}


								char [] cMessage =  sMsg.ToCharArray();
								if (!m_GpsTracker.ShowGPSIcon(cMessage,sMsg.Length,false,iIndex,false,true))
								{
									if (sMsg.StartsWith("\"packet_id\"") == false)
									{
										CSVReader csvReader = new CSVReader();
										string [] sMsgField = csvReader.GetCSVLine(sMsg);
										if (sMsgField!=null && sMsgField.Length==14)
										{
											//Display icon
											gpsSource.GpsPos.m_sName=sMsgField[1];
											if (sMsgField[2]!="")
												gpsSource.GpsPos.m_fLat=Convert.ToSingle(sMsgField[2]);
											if (sMsgField[3]!="")
												gpsSource.GpsPos.m_fLon=Convert.ToSingle(sMsgField[3]);
											if (sMsgField[4]!="")
												gpsSource.GpsPos.m_fHeading=Convert.ToSingle(sMsgField[4]);
											if (sMsgField[5]!="")
												gpsSource.GpsPos.m_fSpeed=Convert.ToSingle(sMsgField[5]);
											if (sMsgField[6]!="")
												gpsSource.GpsPos.m_fAlt=Convert.ToSingle(sMsgField[6]);
											if (sMsgField[7]!="")
												gpsSource.GpsPos.m_iAPRSIconTable=Convert.ToInt32(Convert.ToChar(sMsgField[7]));
											if (sMsgField[8]!="")
												gpsSource.GpsPos.m_iAPRSIconCode=Convert.ToInt32(Convert.ToChar(sMsgField[8]));
											gpsSource.GpsPos.m_sComment=sMsgField[9];
											if (sMsgField[13]!="")
											{
												DateTime dateAPRS = new DateTime(0);
												dateAPRS = DateTime.Parse(sMsgField[13]);
												gpsSource.GpsPos.m_iYear=dateAPRS.Year;
												gpsSource.GpsPos.m_iMonth=dateAPRS.Month;
												gpsSource.GpsPos.m_iDay=dateAPRS.Day;
												gpsSource.GpsPos.m_iHour=dateAPRS.Hour;
												gpsSource.GpsPos.m_iMin=dateAPRS.Minute;
												gpsSource.GpsPos.m_iSec=Convert.ToSingle(dateAPRS.Second);

											}

											gpsSource.GpsPos.m_fRoll=-1000F;
											gpsSource.GpsPos.m_fPitch=-1000F;
											gpsSource.GpsPos.m_fESpeed=-1000000;
											gpsSource.GpsPos.m_fNSpeed=-1000000;
											gpsSource.GpsPos.m_fVSpeed=-1000000;
											gpsSource.GpsPos.m_fDepth = -1000000;

											gpsSource.GpsPos.m_sAltUnit="m";
											gpsSource.GpsPos.m_sSpeedUnit="km\\h";
										
										
											sMsg="APRS:"+sMsg;
											cMessage =  sMsg.ToCharArray();
											m_GpsTracker.ShowGPSIcon(cMessage,sMsg.Length,false,iIndex,false,false);
										}
									}
								}
								sbMsg = new StringBuilder("");
							}
						}
					} while (iRead >= 0 && m_GpsTracker.m_fCloseThreads==false);
				}
				catch (Exception)
				{
				}

				for (int iDelay=0; iDelay<(iRefresh*2); iDelay++)
				{
					if (m_GpsTracker.m_fCloseThreads==true)
						break;
					Thread.Sleep(500);
				}
			}
            gpsSource.eventThreadSync.Set();
		}

		

		#region APRSParser DLL access

		private int APRSParse(string sString, ref APRSPOSITION aprsPosition, ref APRSSTATUS aprsStatus)
		{
			int iRet=-1;
			try
			{
				lock("ExternDllAccess")
				{
					//Call the APRS DLL
					iRet=APRSParseLinePosStat(sString , ref aprsPosition, ref aprsStatus);
				}
			}
			catch(Exception)
			{
				iRet=-1;
			}
			return iRet;
		}

		[StructLayout(LayoutKind.Sequential)]
			private struct APRSSTATUS
		{
			public string source; //10
			public string comment; //256
			public int power;		// Output power in Watts
			public int height;		// Height above terain in meters
			public float gain;		// antenna gain
			public int directivity;	// antenna directivity
			public int rate;		// station transmit rate
			public byte symbol_table;	// Symbol table
			public byte symbol_code;	// Symbol code  
		} 

		[StructLayout(LayoutKind.Sequential)]
			private struct APRSPOSITION
		{
			public string source;  //10
			public string sentence;  //6
		
			public long time_of_fix;	// Time of Fix (seconds since Epoch)
			public double latitude;	// Latitude (North=postive,South=negative)
			public double longitude;	// Longitude (West=negative,East=positive)
			public float speed_over_ground;	// Speed over ground (km/hr)
			public float course_made_good;	// Course made good (true)
			public float altitude;	// Antenna altitude above / below sea level
			// APRS stuff
			// Less useful stuff (specific to instrument) below:
			public int checksum;		// Checksum (Valid=1,Invalid=0)
			// BUG: Should be enumerated
			public byte rx_warning;	// Navigation receiver warning
			public float magnetic_variance;	// Magnetic variance
			public int quality;		// 0 = fix not available
			// 1 = GPS SPS mode
			// 2 = diffrential GPS, SPS mode
			// 3 = GPS PPS mode
			public int sats;		// Satellites in use
			public float hor_dilution;	// Horizontal dillution of precision
			public float geoidal_separation;	// difference between teh WGS84 earth
			// ellipsoid and mean sea level
			public float diff_last_update;	// seconds since last DGPS update
			public int diff_station;	// diffrential station ID (0000-1023) 

			public string name;  //10
		} 

		//Necessary imports for com port access
		[DllImport("APRSParserDLL.gpsTracker")]
		private static extern int APRSParseLinePosStat(string sString, ref APRSPOSITION aprsPosition, ref APRSSTATUS aprsStatus);

		//Imports for loading and unloading the GpsBabel dll
		[DllImport ("kernel32.dll")]
		private extern static IntPtr LoadLibrary(string fileName);
		[DllImport ("kernel32.dll")]
		private extern static bool FreeLibrary(IntPtr lib);
		

		#endregion
	}
}


