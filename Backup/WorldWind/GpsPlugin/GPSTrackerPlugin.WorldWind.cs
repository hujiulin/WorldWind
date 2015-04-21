//----------------------------------------------------------------------------
// NAME: GPSTracker
// DEVELOPER: Javier Santoro
// WEBSITE: http://www.worldwindcentral.com/wiki/Add-on:GPS_Tracker_(plugin)
// VERSION: V04R00 (February 09, 2007)
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
//using System.Diagnostics;

namespace GpsTrackerPlugin
{
    //
    // Here are all the classes for rendering the icons, tracks, etc on WorldWind
    // and for interfacing with WorldWind Plugin system

	public class GpsTrackerPlugin : WorldWind.PluginEngine.Plugin
	{
		public string m_sVersion = "V04R00";
        public static string m_sPluginDirectory;
        

		public GpsTracker gpsTracker=null;
		public GPSTrackerOverlay gpsOverlay;
		public bool m_fGpsTrackerRunning=false;
		WindowsControlMenuButton m_MenuButton;

		public override void Load()
		{
            //set plugin directory
            if (PluginDirectory.ToLower().EndsWith("gpstracker"))
                m_sPluginDirectory = PluginDirectory;
            else
                m_sPluginDirectory = PluginDirectory + "\\plugins\\gpstracker";

            try
            {
                Directory.CreateDirectory(m_sPluginDirectory + "\\NMEAExport");
            }
            catch (Exception)
            {
            }

			gpsOverlay=null;
			pluginAddOverlay();

			//create new instance of gpstracker
			gpsTracker = new GpsTracker(this);
			// Add the GPSTracker plugin to the World Wind tool bar 
            m_MenuButton = new WindowsControlMenuButton("Gps Tracker", m_sPluginDirectory + "\\gpstracker.png", this.gpsTracker);
			Application.WorldWindow.MenuBar.AddToolsMenuButton( m_MenuButton );
			
			base.Load();
		}
  
		public override void Unload()
		{
			// Clean up
			gpsTracker.Close();
			
			//remove button...
			Application.WorldWindow.MenuBar.RemoveToolsMenuButton(m_MenuButton);

			pluginRemoveOverlay();

			m_MenuButton=null;
			gpsTracker=null;
			gpsOverlay=null;
			base.Unload ();
		}

		public void pluginAddOverlay()
		{
			gpsOverlay = new GPSTrackerOverlay(this);
			gpsOverlay.Initialize(Application.WorldWindow.DrawArgs);
			Application.WorldWindow.CurrentWorld.RenderableObjects.Add(gpsOverlay);
			gpsOverlay.Update(Application.WorldWindow.DrawArgs);
		}

		public void pluginRemoveOverlay()
		{
			if (gpsOverlay!=null)
			{
				Application.WorldWindow.CurrentWorld.RenderableObjects.Remove(gpsOverlay.Name);
				gpsOverlay=null;
			}
		}

		//
		//Functions to access WorldWindow used by the GPS tracker class
		//GotoLatLon
		public void pluginWorldWindowGotoLatLonHeading(double fLat, double fLon, float fHeading, int iAltitud)
		{
			if (fHeading==-1F || gpsTracker.m_bTrackHeading==false || gpsOverlay==null)
			{
				if (iAltitud==0)
					ParentApplication.WorldWindow.GotoLatLon(fLat,fLon);
				else
					ParentApplication.WorldWindow.GotoLatLonAltitude(fLat,fLon,(double)iAltitud*(double)1000);
			}
			else
			{
				if (gpsOverlay!=null && gpsTracker.m_bTrackHeading)
				{
					if (iAltitud==0)
						ParentApplication.WorldWindow.GotoLatLonHeadingAltitude(fLat, fLon, fHeading, gpsOverlay.drawArgs.WorldCamera.Altitude);
					else
						ParentApplication.WorldWindow.GotoLatLonHeadingAltitude(fLat,fLon,fHeading,(double)iAltitud*(double)1000);
				}
			}
		}

		//WorldWindow focus
		public void pluginWorldWindowFocus()
		{
			ParentApplication.WorldWindow.Focus();
		}

		//WorldWindow invalidate
		public void pluginWorldWindowInvalidate()
		{
			ParentApplication.WorldWindow.Invalidate();
		}

		//Call by the gpstracker class to check (locked) |  uncheck (not locked)
		//the GPSTracker menuitem
        //Not used in this version
		public void pluginLocked(bool fLocked)
		{
			//m_MenuButton.SetPushed(fLocked);
            //m_MenuItem.Checked = fLocked;
            
		}

		//Show the user selected icon for the gps device on the WorldView
		public void pluginShowOverlay(GPSRenderInformation renderInformation)
		{
			renderInformation.iActiveTrack=gpsTracker.GetActiveTrack();
			gpsOverlay.ShowOverlay(renderInformation);
		}

        public void pluginAddGeoFence(GPSGeoFenceData geoFence)
        {
            gpsOverlay.AddGeoFence(geoFence);
        }

		public void pluginShowFixInfo(string sText)
		{
			gpsOverlay.ShowFixInfo(sText);
		}

		public void pluginRemoveAllOverlay()
		{
			gpsOverlay.RemoveAllOverlay();
		}

		public void pluginSetActiveTrack(int iIndex, bool bTrack)
		{
			gpsTracker.SetActiveTrack(iIndex,bTrack);
		}

		public void pluginToggleTrackHeading(bool bTrackHeading)
		{
            gpsTracker.m_bTrackHeading = bTrackHeading;
            gpsTracker.SetTrackHeading(bTrackHeading);
		}

		public void pluginToggleTrackLine(bool bTrackLine)
		{
            gpsTracker.m_bTrackLine = bTrackLine;
            gpsTracker.SetTrackLine(bTrackLine);
		}

		public void pluginAddPOI(string sPOIName, float fLat, float fLon)
		{
			gpsTracker.AddPOI(sPOIName, fLat, fLon, true, null);
		}

        public void pluginResetPOIs()
        {
            try
            {
                for (int i = 0; i < gpsTracker.m_gpsSourceList.Count; i++)
                {
                    GPSSource gpsSource = (GPSSource)gpsTracker.m_gpsSourceList[i];
                    if (gpsSource.sType == "POI")
                        gpsSource.bPOISet = false;
                }
            }
            catch(Exception)
            {
            }
        }

        public void pluginAddGeoFenceToGpsTracker(GPSGeoFenceData geoFence)
        {
            gpsTracker.AddGeoFence(geoFence);
        }
	}

	//
	// Add the GPSTracker overlay (GPS icons will be added under this overlay)
	public class GPSTrackerOverlay : RenderableObjectList
	{
		public static MainApplication ParentApplication;
		public GpsTrackerPlugin Plugin;
		public DrawArgs drawArgs;
		GPSTrackerFixInfo gpsTrackerInfo;

		public GPSIcon [] m_gpsIcons;
		public GPSIcon [] m_gpsPOI;
		public GPSTrackLine [] m_gpsTrack;
        public GPSGeoFence[] m_gpsGeoFence;
        private uint m_uIconResize;
        private uint m_uPOIResize;
        private uint m_uTrackResize;
        private uint m_uGeoFenceResize;
        
		public int m_iGpsPOIIndex;
		public int m_iGpsIconIndex;
        public int m_iGpsGeoFenceIndex;

		public string m_sPOIName;

		[DllImport("user32")] public static extern int GetKeyboardState(byte [] pbKeyState);
		static int VK_LSHIFT = 0xA0;
        static int VK_LALT = 0xA4;

		public GPSTrackerOverlay(GpsTrackerPlugin plugin) : base("GPSTracker")
		{
			this.Plugin = plugin;
            ParentApplication = plugin.ParentApplication;

            //
            //Set an original size for the arrays
            m_uIconResize=1;
            m_uPOIResize=1;
            m_uTrackResize=1;
            m_uGeoFenceResize = 1;
            m_gpsIcons = new GPSIcon[m_uIconResize];
            m_gpsPOI = new GPSIcon[m_uPOIResize];
            m_gpsTrack = new GPSTrackLine[m_uTrackResize];
            m_gpsGeoFence = new GPSGeoFence[m_uGeoFenceResize];

			gpsTrackerInfo=null;
			m_iGpsPOIIndex=0;
			m_iGpsIconIndex=0;
            m_iGpsGeoFenceIndex = 0;

			// true to make this layer active on startup, this is equal to the checked state in layer manager
			IsOn = false;
		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
			Dispose();
			this.drawArgs = ParentApplication.WorldWindow.DrawArgs;
			isInitialized = true;
		}

		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			if(!IsOn)
				return;
			if(!isInitialized)
				return;

			base.Render(drawArgs);
		}

		/// <summary>
		/// Handle mouse click
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
            Point pLastMousePoint = DrawArgs.LastMousePosition;

			foreach(RenderableObject ro in ChildObjects)
			{
				if(!ro.IsOn || !ro.isSelectable)
					continue;

				if (ro.PerformSelectionAction(drawArgs))
					return true;
			}

			byte [] pbKeyState = new byte[256];
			GetKeyboardState(pbKeyState);
			if ((pbKeyState[VK_LSHIFT] & 0x80)==0x80)
			{
				Angle StartLatitude;
				Angle StartLongitude;
				drawArgs.WorldCamera.PickingRayIntersection(
                    pLastMousePoint.X,
                    pLastMousePoint.Y,
					out StartLatitude,
					out StartLongitude);

				if (!double.IsNaN(StartLatitude.Degrees) && !double.IsNaN(StartLongitude.Degrees))
				{
					POIName poiName = new POIName(this);
					m_sPOIName="";
					poiName.ShowDialog();
					poiName.Dispose();

					Plugin.pluginAddPOI(m_sPOIName, (float)StartLatitude.Degrees, (float)StartLongitude.Degrees);
				}
			}
            else
            if ((pbKeyState[VK_LALT] & 0x80) == 0x80)
            {
                lock ("RenderAccess")
                {
                    //Resize array if necessary
                    if (m_iGpsGeoFenceIndex >= m_uGeoFenceResize)
                    {
                        m_uGeoFenceResize += 5;
                        Array.Resize(ref m_gpsGeoFence, (int)m_uGeoFenceResize);
                    }

                    if (m_gpsGeoFence[m_iGpsGeoFenceIndex]==null)
                    {
                        m_gpsGeoFence[m_iGpsGeoFenceIndex] = new GPSGeoFence(ParentApplication.WorldWindow.CurrentWorld, "New Geo Fence", Plugin);
                        m_gpsGeoFence[m_iGpsGeoFenceIndex].Initialize(this.drawArgs);
                        m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bDone = false;
                        m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bShowInfo = true;
                        Add(m_gpsGeoFence[m_iGpsGeoFenceIndex]);
                    }
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].AddGeoFence(drawArgs, pLastMousePoint);
                    if (m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bDone && m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bCancel == false)
                    {
                        GPSGeoFenceData geoFence = new GPSGeoFenceData();
                        geoFence.sName = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sDescription;
                        geoFence.sSource = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sSource;
                        geoFence.arrayLat = m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLat;
                        geoFence.arrayLon = m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLon;
                        geoFence.sEmail = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sEmail;
                        geoFence.sSound = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sSound;
                        geoFence.sSoundOut = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sSoundOut;
                        geoFence.bEmailIn = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bEmailIn;
                        geoFence.bEmailOut = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bEmailOut;
                        geoFence.bSoundIn = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bSoundIn;
                        geoFence.bSoundOut = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bSoundOut;
                        geoFence.bMsgBoxIn = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bMsgBoxIn;
                        geoFence.bMsgBoxOut = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bMsgBoxOut;
                        geoFence.bShowInfo = m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bShowInfo;
                        geoFence.SourcesIn = new ArrayList();
                        geoFence.SourcesOut = new ArrayList();
                        Plugin.pluginAddGeoFenceToGpsTracker(geoFence);

                        m_iGpsGeoFenceIndex++;
                    }
                    else
                    if (m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bCancel)
                    {
                        Remove(m_gpsGeoFence[m_iGpsGeoFenceIndex]);
                        m_gpsGeoFence[m_iGpsGeoFenceIndex].Dispose();
                        m_gpsGeoFence[m_iGpsGeoFenceIndex] = null;
                    }
                }
            }
			return false;
		}

        public void AddGeoFence(GPSGeoFenceData geoFence)
        {
            lock ("RenderAccess")
            {
                //Resize array if necessary
                if (m_iGpsGeoFenceIndex >= m_uGeoFenceResize)
                {
                    m_uGeoFenceResize += 5;
                    Array.Resize(ref m_gpsGeoFence, (int)m_uGeoFenceResize);
                }

                if (m_gpsGeoFence[m_iGpsGeoFenceIndex] == null)
                {
                    m_gpsGeoFence[m_iGpsGeoFenceIndex] = new GPSGeoFence(ParentApplication.WorldWindow.CurrentWorld, "New Geo Fence", Plugin);
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bDone = false;
                    
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sDescription = geoFence.sName;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLat = (ArrayList)geoFence.arrayLat.Clone();
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLat.Add(m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLat[0]);
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLon = (ArrayList)geoFence.arrayLon.Clone();
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLon.Add(m_gpsGeoFence[m_iGpsGeoFenceIndex].arrayLon[0]);
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sEmail = geoFence.sEmail;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bEmailIn = geoFence.bEmailIn;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bEmailOut = geoFence.bEmailOut;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sSound = geoFence.sSound;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_sSoundOut = geoFence.sSoundOut;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bSoundIn = geoFence.bSoundIn;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bSoundOut = geoFence.bSoundOut;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bMsgBoxIn = geoFence.bMsgBoxIn;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bMsgBoxOut = geoFence.bMsgBoxOut;
                    m_gpsGeoFence[m_iGpsGeoFenceIndex].m_bShowInfo = geoFence.bShowInfo;

                    m_gpsGeoFence[m_iGpsGeoFenceIndex].Initialize(this.drawArgs);
                    Add(m_gpsGeoFence[m_iGpsGeoFenceIndex]);
                    m_iGpsGeoFenceIndex++;
                }
            }
        }

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
			isInitialized = false;

			//RemoveAllOverlay();

			base.Dispose();
		}

		public bool GetPOI(int iIndex, out double fLat, out double fLon, out string sName, out string sIconFile)
		{
			bool bRet=false;
			
			fLat=0F;
			fLon=0F;
			sName="";
			sIconFile="";
			
			if (m_gpsPOI[iIndex]!=null)
			{
				fLat=m_gpsPOI[iIndex].m_RenderInfo.fLat;
				fLon=m_gpsPOI[iIndex].m_RenderInfo.fLon;
				sName=m_gpsPOI[iIndex].m_RenderInfo.sDescription;
				sIconFile=m_gpsPOI[iIndex].m_textureFileName;
				bRet=true;
			}
			return bRet;
		}

		//
		//Show the user selected icon for the gps device on the WorldView
		public void ShowOverlay(GPSRenderInformation renderInformation)
		{
            lock ("RenderAccess")
			{
				if (renderInformation.gpsTrack!=null)
				{
                    //Resize array if necessary
                    if (renderInformation.iIndex >= m_uTrackResize)
                    {
                        m_uTrackResize = (uint)renderInformation.iIndex+1;
                        Array.Resize(ref m_gpsTrack, (int)m_uTrackResize);
                    }

					if (m_gpsTrack[renderInformation.iIndex]==null)
                        m_gpsTrack[renderInformation.iIndex] = new GPSTrackLine(ParentApplication.WorldWindow.CurrentWorld, renderInformation.sDescription);
                    m_gpsTrack[renderInformation.iIndex].Initialize(this.drawArgs);
                    m_gpsTrack[renderInformation.iIndex].SetTrack(this, renderInformation.gpsTrack, renderInformation.sDescription, renderInformation.colorTrack, renderInformation.sIcon, renderInformation.bShowInfo);
					Add(m_gpsTrack[renderInformation.iIndex]);
				}
				else
				{
					if (renderInformation.bPOI==false)
					{
						
						int iIndex;
						for (iIndex=0; iIndex<m_iGpsIconIndex; iIndex++)
							if (renderInformation.sDescription==m_gpsIcons[iIndex].m_RenderInfo.sDescription)
								break;

						if (iIndex==m_iGpsIconIndex)
						{
                            //Resize array if necessary
                            if (m_iGpsIconIndex>=m_uIconResize)
                            {
                                m_uIconResize+=5;
                                Array.Resize(ref m_gpsIcons, (int)m_uIconResize);
                            }

							m_gpsIcons[iIndex] = new GPSIcon(this, renderInformation, ParentApplication.WorldWindow.CurrentWorld);
							m_gpsIcons[iIndex].Initialize(this.drawArgs);
							Add(m_gpsIcons[iIndex]);
							m_iGpsIconIndex++;
						}

						if (m_gpsIcons[iIndex].m_bTrack)
							Plugin.pluginWorldWindowGotoLatLonHeading(renderInformation.fLat,renderInformation.fLon,renderInformation.fHeading,renderInformation.iStartAltitud);

						m_gpsIcons[iIndex].SetGpsData(drawArgs, renderInformation);
					}
					else
					{
                        //Resize array if necessary
                        if (m_iGpsPOIIndex >= m_uPOIResize)
                        {
                            m_uPOIResize += 5;
                            Array.Resize(ref m_gpsPOI, (int)m_uPOIResize);
                        }

						if (m_gpsPOI[m_iGpsPOIIndex]==null)
						{
							m_gpsPOI[m_iGpsPOIIndex] = new GPSIcon(this, renderInformation.iIndex,renderInformation, ParentApplication.WorldWindow.CurrentWorld);
							m_gpsPOI[m_iGpsPOIIndex].Initialize(this.drawArgs);
							Add(m_gpsPOI[m_iGpsPOIIndex]);
						}
						m_iGpsPOIIndex++;
					}
				}
			}
		}

		public void RemoveAllOverlay()
		{
            lock ("RenderAccess")
            {
                for (int i = 0; i < m_iGpsPOIIndex; i++)
                {
                    if (m_gpsPOI[i] != null)
                    {
                        Remove(m_gpsPOI[i]);
                        m_gpsPOI[i].Dispose();
                        m_gpsPOI[i] = null;
                    }
                }
                Plugin.pluginResetPOIs();

                for (int i = 0; i < m_iGpsIconIndex; i++)
                {
                    if (m_gpsIcons[i] != null)
                    {
                        Remove(m_gpsIcons[i]);
                        m_gpsIcons[i].Dispose();
                        m_gpsIcons[i] = null;
                    }
                }

                for (int i = 0; i < m_uTrackResize; i++)
                {
                    if (m_gpsTrack[i] != null)
                    {
                        Remove(m_gpsTrack[i]);
                        m_gpsTrack[i].Dispose();
                        m_gpsTrack[i] = null;
                    }
                }
                m_iGpsPOIIndex = 0;
                m_iGpsIconIndex = 0;

                for (int i = 0; i < m_iGpsGeoFenceIndex; i++)
                {
                    if (m_gpsGeoFence[i] != null)
                    {
                        Remove(m_gpsGeoFence[i]);
                        m_gpsGeoFence[i].Dispose();
                        m_gpsGeoFence[i] = null;
                    }
                }
                m_iGpsGeoFenceIndex = 0;

                if (gpsTrackerInfo != null)
                {
                    Remove(gpsTrackerInfo);
                    gpsTrackerInfo.Dispose();
                    gpsTrackerInfo = null;
                }
            }
		}

		public void ShowFixInfo(string sText)
		{
			if (gpsTrackerInfo==null)
			{
				gpsTrackerInfo = new GPSTrackerFixInfo();
				gpsTrackerInfo.Initialize(this.drawArgs);
				Add(gpsTrackerInfo);
			}
			gpsTrackerInfo.ShowFixInfo(sText);
			Update(this.drawArgs);
		}

		public void SetActiveTrack(int iIndex, bool bTrack)
		{
			Plugin.pluginSetActiveTrack(iIndex,bTrack);
		}

        public void ToggleTrackHeading(bool bTrackHeading)
		{
            Plugin.pluginToggleTrackHeading(bTrackHeading);
		}

        public void ToggleTrackLine(bool bTrackLine)
		{
            Plugin.pluginToggleTrackLine(bTrackLine);
		}
	}

	public class GPSTrackerFixInfo : RenderableObject
	{
		public string m_sText;

		public GPSTrackerFixInfo() : base("Fix Status", Vector3.Empty, Quaternion.Identity)
		{
			m_sText="GPSTracker: Active";
			
			// We want to be drawn on top of everything else
			this.RenderPriority = RenderPriority.Icons;

			// true to make this layer active on startup, this is equal to the checked state in layer manager
			this.IsOn = true;
		}


		public void ShowFixInfo(string sText)
		{
			if (sText=="" || m_sText=="" ||
				!(m_sText!="GPSTracker: Active" &&
				m_sText!="GPSTracker: Fix" &&
				m_sText!="GPSTracker: No Fix" &&
				(
					sText=="GPSTracker: Active" ||
					sText=="GPSTracker: Fix" ||
					sText=="GPSTracker: No Fix" 
				)))
				m_sText=sText;
		}

		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			String sInfo=m_sText;

			Rectangle bounds = drawArgs.toolbarFont.MeasureString(null, sInfo, DrawTextFormat.None, 0);
			int color = Color.Black.ToArgb();
			drawArgs.toolbarFont.DrawText(null, sInfo,drawArgs.screenWidth-bounds.Width-5+1, drawArgs.screenHeight-bounds.Height-5,color );
			drawArgs.toolbarFont.DrawText(null, sInfo,drawArgs.screenWidth-bounds.Width-5+1, drawArgs.screenHeight-bounds.Height-5+1,color );
			color = Color.Yellow.ToArgb();
			drawArgs.toolbarFont.DrawText(null, sInfo,drawArgs.screenWidth-bounds.Width-5, drawArgs.screenHeight-bounds.Height-5,color );
		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

	}

	public class GPSTrackLine : RenderableObject
	{
        DrawArgs m_drawArgs;
		GPSTrack m_gpsTrack;
		World m_parentWorld;
		uint m_uVerticesCount;
		float m_fTotalDistance=0F;
        double m_fStartAlt;
        double m_fStartLatitud;
        double m_fStartLongitude;
        double m_fLatitudFrom;
        double m_fLatitudTo;
        double m_fLongitudeFrom;
        double m_fLongitudeTo;
        public string m_sDescription;
        string m_sIconFileName;
		private CustomVertex.PositionColored[][] vertices = new CustomVertex.PositionColored[1000][];
		Vector3 v;
		Color m_colorTrack;
		float m_fCurrentVerticalExaggeration;
		GPSTrackerOverlay m_gpsTrackerOverlay;
		bool m_bCalculateDistance;

        int m_iTextureWidth;
        int m_iTextureHeight;
        int m_iIconWidth;
        int m_iIconHeight;
        int m_iIconWidthHalf;
        int m_iIconHeightHalf;
        Vector3 xyzPosition;

        Texture texture;
        Sprite sprite;
        Rectangle spriteSize;

        public bool m_bShowInfo;

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        static int VK_LCONTROL = 0xA2;

        public GPSTrackLine(World parentWorld, string sDescription)
			: base(sDescription, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			m_parentWorld=parentWorld;
		}

		public void SetTrack(GPSTrackerOverlay gpsTrackerOverlay,GPSTrack gpsTrack,string sDescription,Color colorTrack, string sIcon, bool bShowInfo)
		{
			m_gpsTrack=gpsTrack;
			m_gpsTrackerOverlay = gpsTrackerOverlay;
            m_bShowInfo = bShowInfo;

			m_uVerticesCount=(gpsTrack.m_uPointCount/50000)+1;
			m_fTotalDistance=0;
			m_colorTrack=colorTrack;
			m_fCurrentVerticalExaggeration = -1F;
			m_bCalculateDistance = true;

            m_fStartAlt = gpsTrack.m_fAlt[0];
            m_fStartLatitud = gpsTrack.m_fLat[0];
            m_fStartLongitude = gpsTrack.m_fLon[0];
            m_sDescription = sDescription;
            m_sIconFileName = sIcon;
			if (m_sIconFileName=="")
				m_sIconFileName = GpsTrackerPlugin.m_sPluginDirectory + "\\Gpsx.png";
			
					
#if !DEBUG
            try
#endif
            {
                this.texture = TextureLoader.FromFile(m_drawArgs.device, this.m_sIconFileName, 0, 0, 1, 0, Format.Unknown, Pool.Managed, Filter.Box, Filter.Box, 0);
            }
#if !DEBUG
            catch (Microsoft.DirectX.Direct3D.InvalidDataException)
            {
                this.texture = TextureLoader.FromFile(m_drawArgs.device, GpsTrackerPlugin.m_sPluginDirectory + "\\gpsx.png", 0, 0, 1, 0, Format.Unknown, Pool.Managed, Filter.Box, Filter.Box, 0);
            }
#endif

            using (Surface s = this.texture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                this.m_iTextureWidth = desc.Width;
                this.m_iTextureHeight = desc.Height;
                this.m_iIconWidth = desc.Width;
                this.m_iIconHeight = desc.Height;
                this.m_iIconWidthHalf = desc.Width / 2;
                this.m_iIconHeightHalf = desc.Height / 2;

                this.spriteSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }

            this.isSelectable = true;

            this.sprite = new Sprite(m_drawArgs.device);


			this.isInitialized = true;
			this.RenderPriority = RenderPriority.Icons;
		}

        public override void Initialize(DrawArgs drawArgs)
        {
            this.isInitialized = true;
            m_drawArgs = drawArgs;


        }

		public override void Update(DrawArgs drawArgs)
		{
			if(!this.isInitialized)
				this.Initialize(drawArgs);
		}

		public override void Dispose()
		{
			this.isInitialized = false;
		}

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            bool bRet = false;

			float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(m_fStartLatitud,m_fStartLongitude, (double)241));
			if (m_fStartAlt < elevation)
				m_fStartAlt = elevation;
			xyzPosition=MathEngine.SphericalToCartesian(m_fStartLatitud, m_fStartLongitude, this.m_parentWorld.EquatorialRadius + (m_fStartAlt * World.Settings.VerticalExaggeration));

            if (!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(xyzPosition))
                return false;

            Vector3 translationVector = new Vector3(
                (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));
            Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);
#if !DEBUG
			try
#endif
            {
                if (Math.Abs(DrawArgs.LastMousePosition.X - projectedPoint.X) < m_iIconWidthHalf &&
                    Math.Abs(DrawArgs.LastMousePosition.Y - projectedPoint.Y) < m_iIconHeightHalf)
                {
                    byte[] pbKeyState = new byte[256];
                    GetKeyboardState(pbKeyState);
                    if ((pbKeyState[VK_LCONTROL] & 0x80) == 0x80) //Toggle Information text
                    {
                        GpsSetup gpsSetup = new GpsSetup(null, this, null);
                        gpsSetup.ShowDialog();
                        gpsSetup.Dispose();
                    }

                    bRet = true;
                    Update(drawArgs);
                }
            }
#if !DEBUG
			catch
			{
			}
#endif

            return bRet; //false
        }
		//Draw the icon and information text
		public override void Render(DrawArgs drawArgs)
		{

			if(this.isInitialized)
			{
				if (m_fCurrentVerticalExaggeration != World.Settings.VerticalExaggeration)
				{
					uint uCount=0;
					uint uLength=0;
					for (int i=0; i<m_uVerticesCount; i++)
					{
						if (((i+1)*50000)>m_gpsTrack.m_uPointCount)
							uLength=m_gpsTrack.m_uPointCount-uCount;
						else
							uLength=50000;
						this.vertices[i] = new CustomVertex.PositionColored[uLength];
						for(uint ii = 0; ii < uLength; ii++) 
						{
							float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(m_gpsTrack.m_fLat[uCount], m_gpsTrack.m_fLon[uCount], (double)241));
							if (m_gpsTrack.m_fAlt[uCount] < elevation)
								m_gpsTrack.m_fAlt[uCount] = elevation;
							v=MathEngine.SphericalToCartesian(m_gpsTrack.m_fLat[uCount], m_gpsTrack.m_fLon[uCount], this.m_parentWorld.EquatorialRadius + (m_gpsTrack.m_fAlt[uCount] * World.Settings.VerticalExaggeration));
					
							this.vertices[i][ii].X = v.X;
							this.vertices[i][ii].Y = v.Y;
							this.vertices[i][ii].Z = v.Z;
							this.vertices[i][ii].Color =m_colorTrack.ToArgb();

							if (m_bCalculateDistance==true)
							{
								v=m_gpsTrackerOverlay.drawArgs.WorldCamera.Project(v);
								if (uCount==0)
								{
									m_fLatitudFrom = m_gpsTrack.m_fLat[uCount];
									m_fLongitudeFrom = m_gpsTrack.m_fLon[uCount];
								}
								else
								{
									m_fLatitudTo = m_gpsTrack.m_fLat[uCount];
									m_fLongitudeTo = m_gpsTrack.m_fLon[uCount];

									double dDistance, dLatTo, dLatFrom, dLonFrom, dLonTo, deltaLon, deltaLat;
									dLatTo = m_fLatitudTo;
									dLatFrom = m_fLatitudFrom;
									dLonTo = m_fLongitudeTo;
									dLonFrom = m_fLongitudeFrom;

									dLatFrom = dLatFrom * (Math.PI / 180);
									dLatTo = dLatTo * (Math.PI / 180);
									dLonFrom = dLonFrom * (Math.PI / 180);
									dLonTo = dLonTo * (Math.PI / 180);
									deltaLon = dLonTo - dLonFrom;
									deltaLat = dLatTo - dLatFrom;
									if (deltaLon == 0 && deltaLat == 0)
										dDistance = (double)0;
									else
										dDistance = Math.Acos(Math.Sin(dLatFrom) * Math.Sin(dLatTo) + Math.Cos(dLatFrom) * Math.Cos(dLatTo) * Math.Cos(deltaLon)) * 6371.0;
									if (double.IsNaN(dDistance) == false)
										m_fTotalDistance = m_fTotalDistance + (float)dDistance;
									m_fLatitudFrom = m_fLatitudTo;
									m_fLongitudeFrom = m_fLongitudeTo;

								}
							}
							uCount++;
						}
					}
					if (m_bCalculateDistance==true)
						m_bCalculateDistance=false;

					m_fCurrentVerticalExaggeration = World.Settings.VerticalExaggeration;
				}

				drawArgs.device.RenderState.ZBufferEnable = false;
				drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
                drawArgs.device.Transform.World = Matrix.Translation(
                    (float)-drawArgs.WorldCamera.ReferenceCenter.X,
                    (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
                    (float)-drawArgs.WorldCamera.ReferenceCenter.Z
                    );
				for (uint i=0; i<m_uVerticesCount; i++)
					drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.vertices[i].Length - 1, this.vertices[i]);

				float elev = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(m_fStartLatitud,m_fStartLongitude, (double)241));
				if (m_fStartAlt < elev)
					m_fStartAlt = elev;
				xyzPosition=MathEngine.SphericalToCartesian(m_fStartLatitud, m_fStartLongitude, this.m_parentWorld.EquatorialRadius + (m_fStartAlt * World.Settings.VerticalExaggeration));

			    if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(xyzPosition))
				    return;
                

                Vector3 translationVector = new Vector3(
                    (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                    (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                    (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

                Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);

			    this.sprite.Begin(SpriteFlags.AlphaBlend);
			    this.sprite.Transform = Matrix.Transformation2D(
				    new Vector2(0.0f, 0.0f), 
				    0.0f, 
				    //new Vector2(scaleWidth, scaleHeight),
				    new Vector2((float)1,(float)1),
				    new Vector2(0,0),
				    0.0f, 
				    new Vector2(projectedPoint.X + (m_iIconWidthHalf), projectedPoint.Y + (m_iIconHeightHalf)));

			    this.sprite.Draw(this.texture, this.spriteSize,
				    new Vector3(this.m_iIconWidth,this.m_iIconHeight,0), 
				    new Vector3(0,0,0),
				    Color.FromArgb(180,255,255,255).ToArgb());

			    this.sprite.End();

                bool sShowInfo = false;
                if (Math.Abs(DrawArgs.LastMousePosition.X - projectedPoint.X) < m_iIconWidthHalf &&
                    Math.Abs(DrawArgs.LastMousePosition.Y - projectedPoint.Y) < m_iIconHeightHalf)
                {
                    DrawArgs.MouseCursor = CursorType.Hand;
                    sShowInfo = true;
                }

                if (m_bShowInfo || sShowInfo)
                {
                    string sInfo = "Track: " + m_sDescription + "\n";
                    string sNS;
                    if (m_fStartLatitud >= (float)0)
                        sNS = "N";
                    else
                        sNS = "S";
                    double dLat = Math.Abs(m_fStartLatitud);
                    double dWhole = Math.Floor(dLat);
                    double dFraction = dLat - dWhole;
                    double dMin = dFraction * (double)60;
                    double dMinWhole = Math.Floor(dMin);
                    double dSeconds = (dMin - dMinWhole) * (double)60;
                    int iDegrees = Convert.ToInt32(dWhole);
                    int iMinutes = Convert.ToInt32(dMinWhole);
                    float fSeconds = Convert.ToSingle(dSeconds);
                    sInfo += "Lat: " + Convert.ToString(iDegrees) + "°" + Convert.ToString(iMinutes) + "'" + Convert.ToString(fSeconds) + "\" " + sNS + "\n";

                    string sEW;
                    if (m_fStartLongitude >= (float)0)
                        sEW = "E";
                    else
                        sEW = "W";
                    double dLon = Math.Abs(m_fStartLongitude);
                    dWhole = Math.Floor(dLon);
                    dFraction = dLon - dWhole;
                    dMin = dFraction * (double)60;
                    dMinWhole = Math.Floor(dMin);
                    dSeconds = (dMin - dMinWhole) * (double)60;
                    iDegrees = Convert.ToInt32(dWhole);
                    iMinutes = Convert.ToInt32(dMinWhole);
                    fSeconds = Convert.ToSingle(dSeconds);
                    sInfo += "Lon: " + Convert.ToString(iDegrees) + "°" + Convert.ToString(iMinutes) + "'" + Convert.ToString(fSeconds) + "\" " + sEW + "\n";

                    sInfo += "Length: " + m_fTotalDistance + "km.";

                    //Draw some black shadow around the yellow text
                    int color = Color.Black.ToArgb();
                    drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) + 11, (int)(projectedPoint.Y), color);
                    drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) + 11, (int)(projectedPoint.Y) + 1, color);
                    color = Color.Yellow.ToArgb();
                    drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) + 10, (int)(projectedPoint.Y), color);
                }

		}
		}




	}

	public class GPSIcon : RenderableObject
	{
		GPSTrackerOverlay m_gpsTrackerOverlay;
		public GPSRenderInformation m_RenderInfo;
		public string m_sDescriptionFrom;
		public bool  m_bSignalDistance;
		public double m_fLatitudeFrom;
		public double m_fLongitudeFrom;
		public bool  m_bShowInfo;
		public bool  m_bTrackLine;
		float m_fLastAlt;
		float m_fLastRoll;
		float m_fLastDepth;
		float m_fLastPitch;
		float m_fLastSpeed;
		float m_fLastESpeed;
		float m_fLastNSpeed;
		float m_fLastVSpeed;
		float m_fLastHeading;
		int m_iLastHour;
		int m_iLastMin;
		float m_fLastSec;
		int m_iLastDay;
		int m_iLastMonth;
		int m_iLastYear;
		World m_parentWorld;
		public string m_textureFileName;
		int m_iTextureWidth; 
		int m_iTextureHeight;
		int m_iIconWidth;
		int m_iIconHeight;
		int m_iIconWidthHalf;
		int m_iIconHeightHalf;
		Vector3 xyzPosition;

		static int hotColor = System.Drawing.Color.White.ToArgb();
		static int normalColor = Color.FromArgb(180,255,255,255).ToArgb();

		uint m_uVerticesCount=0;
		uint m_uPointCount=0;
		float m_fTotalDistance=0;
		double m_fLatFrom;
		double m_fLonFrom;
		uint m_uTotalPointCount=0;
		GPSTrackVertices[] vertices = new GPSTrackVertices[1000];
		float m_fVerticalExaggeration;

		public bool m_bTrack;
        public bool m_bTrackHeading;

		Texture texture;
		Sprite sprite;
		Rectangle spriteSize;


        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        //static int VK_LSHIFT = 0xA0;
        //static int VK_RSHIFT = 0xA1;
        static int VK_LCONTROL = 0xA2;
        //static int VK_RCONTROL = 0xA3;
        //static int VK_LALT = 0xA4;
        //static int VK_RALT = 0xA5;

		//float fFeetToMeter = 31F/100F;

		public GPSIcon(
			GPSTrackerOverlay gpsTrackerOverlay,
			int iIndex, 
			GPSRenderInformation renderInformation,
			World parentWorld)
			: base(renderInformation.sDescription, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			m_fVerticalExaggeration = World.Settings.VerticalExaggeration;
			m_RenderInfo=renderInformation;
			m_RenderInfo.bPOI=true;
			this.m_gpsTrackerOverlay=gpsTrackerOverlay;
			m_RenderInfo.iIndex=iIndex;
			this.m_parentWorld = parentWorld;
			this.m_textureFileName = m_RenderInfo.sIcon;
			this.m_iTextureWidth = 32;
			this.m_iTextureHeight = 32;
			this.m_iIconWidth = 32;
			this.m_iIconHeight = 32;
			this.m_iIconWidthHalf = m_iIconWidth/2;
			this.m_iIconHeightHalf = m_iIconHeight/2;
			this.m_bTrack=renderInformation.fTrack;
			m_bShowInfo = m_RenderInfo.bShowInfo;

			m_fLatitudeFrom=1000F;
			m_fLongitudeFrom=1000F;
			m_sDescriptionFrom="";
			m_bSignalDistance=false;

			m_uVerticesCount=0;
			m_uPointCount=0;
			m_fTotalDistance=0;
			m_uTotalPointCount=0;

			this.RenderPriority = RenderPriority.Icons;
		}

		public GPSIcon(
			GPSTrackerOverlay gpsTrackerOverlay,
			GPSRenderInformation renderInformation,
			World parentWorld)
			: base(renderInformation.sDescription, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			m_fVerticalExaggeration = World.Settings.VerticalExaggeration;
			m_RenderInfo=renderInformation;
			m_RenderInfo.bPOI=false;
			this.m_gpsTrackerOverlay=gpsTrackerOverlay;
			this.m_parentWorld = parentWorld;
			this.m_textureFileName = m_RenderInfo.sIcon;
			this.m_iTextureWidth = 32;
			this.m_iTextureHeight = 32;
			this.m_iIconWidth = 32;
			this.m_iIconHeight = 32;
			this.m_iIconWidthHalf = m_iIconWidth/2;
			this.m_iIconHeightHalf = m_iIconHeight/2;
			m_bSignalDistance=false;
			this.m_bTrack=renderInformation.fTrack;
			m_bShowInfo = m_RenderInfo.bShowInfo;
            m_bTrackLine = m_RenderInfo.bTrackLine;
            m_bTrackHeading = gpsTrackerOverlay.Plugin.gpsTracker.m_bTrackHeading;

			m_fLastAlt=-1000000F;
			m_fLastRoll=-1000F;
			m_fLastDepth=-1000000F;
			m_fLastPitch=-1000F;
			m_fLastSpeed=-1F;
			m_fLastESpeed=-1000000F;
			m_fLastNSpeed=-1000000F;
			m_fLastVSpeed=-1000000F;
			m_fLastHeading=-1F;
			m_iLastHour=-1;
			m_iLastMin=-1;
			m_fLastSec=(float)-1;
			m_iLastDay=-1;
			m_iLastMonth=-1;
			m_iLastYear=-1;
			m_fLatitudeFrom=1000F;
			m_fLongitudeFrom=1000F;
			m_sDescriptionFrom="";

			m_uVerticesCount=0;
			m_uPointCount=0;
			m_fTotalDistance=0;
			m_uTotalPointCount=0;

			for (int i=0; i<m_gpsTrackerOverlay.m_iGpsIconIndex; i++)
				if (m_gpsTrackerOverlay.m_gpsIcons[i].m_RenderInfo.iIndex==m_RenderInfo.iIndex && 
					m_gpsTrackerOverlay.m_gpsIcons[i].m_bTrack &&
					m_bTrack)
					m_bTrack=false;

			this.RenderPriority = RenderPriority.Icons;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.isInitialized = true;

			#if !DEBUG
			try
			#endif
			{
				this.texture = TextureLoader.FromFile(drawArgs.device, this.m_textureFileName, 0, 0, 1, 0, Format.Unknown, Pool.Managed, Filter.Box, Filter.Box, 0);
			}
			#if !DEBUG
			catch(Microsoft.DirectX.Direct3D.InvalidDataException)
			{
				this.texture = TextureLoader.FromFile(drawArgs.device, GpsTrackerPlugin.m_sPluginDirectory + "\\gpsx.png", 0, 0, 1, 0, Format.Unknown, Pool.Managed, Filter.Box, Filter.Box, 0);
			}
			#endif

			using(Surface s = this.texture.GetSurfaceLevel(0))
			{
				SurfaceDescription desc = s.Description;
				this.m_iTextureWidth = desc.Width;
				this.m_iTextureHeight = desc.Height;
				this.m_iIconWidth = desc.Width;
				this.m_iIconHeight = desc.Height;
				this.m_iIconWidthHalf = desc.Width/2;
				this.m_iIconHeightHalf = desc.Height/2;

				this.spriteSize = new Rectangle(0,0, desc.Width, desc.Height);
			}

			this.isSelectable = true;

			this.sprite = new Sprite(drawArgs.device);

			float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(m_RenderInfo.fLat,m_RenderInfo.fLon, (double)241));
			if (m_RenderInfo.fAlt < elevation)
				m_RenderInfo.fAlt = elevation;
			xyzPosition=MathEngine.SphericalToCartesian(m_RenderInfo.fLat, m_RenderInfo.fLon, this.m_parentWorld.EquatorialRadius + (m_RenderInfo.fAlt * World.Settings.VerticalExaggeration));

		}

		public override void Update(DrawArgs drawArgs)
		{
			if(!this.isInitialized)
				this.Initialize(drawArgs);
		}

		public override void Dispose()
		{
			this.isInitialized = false;

			if(this.texture != null)
				this.texture.Dispose();

			if(this.sprite != null)
				this.sprite.Dispose();
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			bool bRet=false;

			float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(m_RenderInfo.fLat,m_RenderInfo.fLon, (double)241));
			if (m_RenderInfo.fAlt < elevation)
				m_RenderInfo.fAlt = elevation;
			xyzPosition=MathEngine.SphericalToCartesian(m_RenderInfo.fLat, m_RenderInfo.fLon, this.m_parentWorld.EquatorialRadius + (m_RenderInfo.fAlt * World.Settings.VerticalExaggeration));

			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(xyzPosition))
				return false;

            Vector3 translationVector = new Vector3(
                (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));
            Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);
			#if !DEBUG
			try
			#endif
			{
				if(Math.Abs(DrawArgs.LastMousePosition.X-projectedPoint.X)<m_iIconWidthHalf &&
					Math.Abs(DrawArgs.LastMousePosition.Y-projectedPoint.Y)<m_iIconHeightHalf)
				{
					byte [] pbKeyState = new byte[256];
					GetKeyboardState(pbKeyState);
					if ((pbKeyState[VK_LCONTROL] & 0x80)==0x80) //Toggle Information text
					{
                        GpsSetup gpsSetup = new GpsSetup(this, null, null);
                        gpsSetup.ShowDialog();
                        gpsSetup.Dispose();

                        m_gpsTrackerOverlay.ToggleTrackHeading(m_bTrackHeading);
                        m_gpsTrackerOverlay.ToggleTrackLine(m_bTrackLine);

                        if (m_RenderInfo.bPOI==false)
                        {
						    if (m_bSignalDistance==false)
						    {
							    for (int i=0; i<m_gpsTrackerOverlay.m_iGpsPOIIndex; i++)
							    {
								    if (m_gpsTrackerOverlay.m_gpsPOI[i]!=null)
								    {

									    m_gpsTrackerOverlay.m_gpsPOI[i].m_fLatitudeFrom=1000F;
									    m_gpsTrackerOverlay.m_gpsPOI[i].m_fLongitudeFrom=1000F;
									    m_gpsTrackerOverlay.m_gpsPOI[i].m_sDescriptionFrom="";
								    }
							    }
						    }
						    else
						    {
							    for (int i=0; i<m_gpsTrackerOverlay.m_iGpsIconIndex; i++)
								    if (m_gpsTrackerOverlay.m_gpsIcons[i]!=null)
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_bSignalDistance=false;
    							
							    for (int i=0; i<m_gpsTrackerOverlay.m_iGpsPOIIndex; i++)
							    {
								    if (m_gpsTrackerOverlay.m_gpsPOI[i]!=null)
								    {
                                        m_bSignalDistance = true;
									    m_gpsTrackerOverlay.m_gpsPOI[i].m_fLatitudeFrom=m_RenderInfo.fLat;
									    m_gpsTrackerOverlay.m_gpsPOI[i].m_fLongitudeFrom=m_RenderInfo.fLon;
									    m_gpsTrackerOverlay.m_gpsPOI[i].m_sDescriptionFrom=m_RenderInfo.sDescription;
								    }
							    }
						    }
                        }
                        else
                        {
						    if (m_bSignalDistance==false)
						    {
							    for (int i=0; i<m_gpsTrackerOverlay.m_iGpsIconIndex; i++)
							    {
								    if (m_gpsTrackerOverlay.m_gpsIcons[i]!=null)
								    {
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_fLatitudeFrom=1000F;
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_fLongitudeFrom=1000F;
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_sDescriptionFrom="";
								    }
							    }
						    }
						    else
						    {
							    for (int i=0; i<m_gpsTrackerOverlay.m_iGpsPOIIndex; i++)
								    if (m_gpsTrackerOverlay.m_gpsPOI[i]!=null)
									    m_gpsTrackerOverlay.m_gpsPOI[i].m_bSignalDistance=false;
    							
							    for (int i=0; i<m_gpsTrackerOverlay.m_iGpsIconIndex; i++)
							    {
								    if (m_gpsTrackerOverlay.m_gpsIcons[i]!=null)
								    {
                                        m_bSignalDistance = true;
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_fLatitudeFrom=m_RenderInfo.fLat;
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_fLongitudeFrom=m_RenderInfo.fLon;
									    m_gpsTrackerOverlay.m_gpsIcons[i].m_sDescriptionFrom=m_RenderInfo.sDescription;
								    }
							    }
						    }

                        }
					}
					else
					{
						bool bOn=true;
						if (m_bTrack)
							bOn=false;
						for (int i=0; i<m_gpsTrackerOverlay.m_iGpsIconIndex; i++)
							m_gpsTrackerOverlay.m_gpsIcons[i].m_bTrack=false;
						for (int i=0; i<m_gpsTrackerOverlay.m_iGpsPOIIndex; i++)
							m_gpsTrackerOverlay.m_gpsPOI[i].m_bTrack=false;
						m_bTrack=bOn;

						m_gpsTrackerOverlay.SetActiveTrack(m_RenderInfo.iIndex,bOn);
					}

					bRet=true;
				}
			}
			#if !DEBUG
			catch
			{
			}
			#endif

			return bRet; //false
		}

		//Draw the icon and information text
		public override void Render(DrawArgs drawArgs)
		{
			if(!this.isInitialized || this.texture == null)
				return;
			int color = normalColor;
			float fDistance=-1F;

			if (m_RenderInfo.bPOI)
			{
				if (m_bTrack)
					color = hotColor;

				//calculate current position
				float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(this.m_RenderInfo.fLat, this.m_RenderInfo.fLon, (double)241));
				if (this.m_RenderInfo.fAlt < elevation)
					this.m_RenderInfo.fAlt = elevation;
				xyzPosition=MathEngine.SphericalToCartesian(this.m_RenderInfo.fLat, this.m_RenderInfo.fLon, this.m_parentWorld.EquatorialRadius + (this.m_RenderInfo.fAlt * World.Settings.VerticalExaggeration));

			}
			else
			{
				if(m_bTrackLine && m_uPointCount>1)
				{
					drawArgs.device.RenderState.ZBufferEnable = false;
					drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
					drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
                    drawArgs.device.Transform.World = Matrix.Translation(
                        (float)-drawArgs.WorldCamera.ReferenceCenter.X,
                        (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
                        (float)-drawArgs.WorldCamera.ReferenceCenter.Z
                        );
					for (uint i=0; i<=m_uVerticesCount; i++)
					{
						if (m_uPointCount<50000)
							drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, (int)m_uPointCount - 1, this.vertices[i].vertices);
						else
							drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.vertices[i].vertices.Length - 1, this.vertices[i].vertices);
						
					}
				}
				if (m_bTrack)
					color = hotColor;

				//calculate current position
				float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(m_RenderInfo.fLat,m_RenderInfo.fLon, (double)241));
				if (m_RenderInfo.fAlt < elevation)
					m_RenderInfo.fAlt = elevation;
				xyzPosition=MathEngine.SphericalToCartesian(m_RenderInfo.fLat, m_RenderInfo.fLon, this.m_parentWorld.EquatorialRadius + (m_RenderInfo.fAlt * World.Settings.VerticalExaggeration));
			}

			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(xyzPosition))
				return;

            Vector3 translationVector = new Vector3(
                (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

            Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);
			if(Math.Abs(DrawArgs.LastMousePosition.X-projectedPoint.X)<m_iIconWidthHalf &&
				Math.Abs(DrawArgs.LastMousePosition.Y-projectedPoint.Y)<m_iIconHeightHalf)
			{
				if (color==normalColor)
					color = hotColor;
				else
					color=normalColor;
				DrawArgs.MouseCursor = CursorType.Hand;
				ShowInfo(drawArgs,  projectedPoint, fDistance );
							
			}
			if (m_bShowInfo)
				ShowInfo(drawArgs,  projectedPoint , fDistance);

			this.sprite.Begin(SpriteFlags.AlphaBlend);
			this.sprite.Transform = Matrix.Transformation2D(
				new Vector2(0.0f, 0.0f), 
				0.0f, 
				//new Vector2(scaleWidth, scaleHeight),
				new Vector2((float)1,(float)1),
				new Vector2(0,0),
				0.0f, 
				new Vector2(projectedPoint.X + (m_iIconWidthHalf), projectedPoint.Y + (m_iIconHeightHalf)));

			this.sprite.Draw(this.texture, this.spriteSize,
				new Vector3(this.m_iIconWidth,this.m_iIconHeight,0), 
				new Vector3(0,0,0),
				color);

			this.sprite.End();

		}

		//Draw the GPS icon information text
		void ShowInfo(DrawArgs drawArgs,  Vector3 projectedPoint, float fDistance )
		{
			// Draw the information text
			int color = Color.Black.ToArgb();
			string sInfo = m_RenderInfo.sDescription + "\n";

			string sNS;
			if (m_RenderInfo.fLat>=(float)0)
				sNS="N";
			else
				sNS="S";
			double dLat=Math.Abs(m_RenderInfo.fLat);
			double dWhole = Math.Floor(dLat);
			double dFraction = dLat- dWhole;
			double dMin = dFraction * (double)60;
			double dMinWhole = Math.Floor(dMin);
			double dSeconds = (dMin - dMinWhole) * (double)60;
			int iDegrees = Convert.ToInt32(dWhole);
			int iMinutes = Convert.ToInt32(dMinWhole);
			float fSeconds = Convert.ToSingle(dSeconds);
			sInfo += "Lat: " + Convert.ToString(iDegrees) + "°" + Convert.ToString(iMinutes) + "'" + Convert.ToString(fSeconds) + "\" " + sNS + "\n";

			string sEW;
			if (m_RenderInfo.fLon>=(float)0)
				sEW="E";
			else
				sEW="W";
			double dLon=Math.Abs(m_RenderInfo.fLon);
			dWhole = Math.Floor(dLon);
			dFraction = dLon- dWhole;
			dMin = dFraction * (double)60;
			dMinWhole = Math.Floor(dMin);
			dSeconds = (dMin - dMinWhole) * (double)60;
			iDegrees = Convert.ToInt32(dWhole);
			iMinutes = Convert.ToInt32(dMinWhole);
			fSeconds = Convert.ToSingle(dSeconds);
			sInfo += "Lon: " + Convert.ToString(iDegrees) + "°" + Convert.ToString(iMinutes) + "'" + Convert.ToString(fSeconds) + "\" " + sEW + "\n";
			if (m_RenderInfo.bPOI==true && m_sDescriptionFrom!="")
			{
                double dDistance = CalculateDistance(m_fLatitudeFrom, m_fLongitudeFrom, m_RenderInfo.fLat, m_RenderInfo.fLon);
                if (double.IsNaN(dDistance) == false)
                {
                    string sUnit = (dDistance >= 1F) ? "km" : "m";
                    dDistance = (dDistance < 1F) ? (dDistance * 1000F) : dDistance;
                    sInfo += "Distance From " + m_sDescriptionFrom + ": " + Convert.ToString(decimal.Round(Convert.ToDecimal(dDistance), 3)) +
                        sUnit +
                        "\n";
                }


			}
			else
			if (m_RenderInfo.bPOI==false)
			{


				if (m_RenderInfo.fAlt==-1000000F)
				{
					if (m_fLastAlt!=-1000000F)
						sInfo += "Altitud: " + Convert.ToString(m_fLastAlt) + " " + m_RenderInfo.sAltUnit + " (Last Known)\n";
				}
				else
				{
					sInfo += "Altitud: " + Convert.ToString(m_RenderInfo.fAlt) + " " + m_RenderInfo.sAltUnit + "\n";
					m_fLastAlt=m_RenderInfo.fAlt;
				}

				if (m_RenderInfo.fDepth==-1000000F)
				{
					if (m_fLastDepth!=-1000000F)
						sInfo += "Depth: " + Convert.ToString(m_fLastDepth) + " meters (Last Known)\n";
				}
				else
				{
					sInfo += "Depth: " + Convert.ToString(m_RenderInfo.fDepth) + " meters\n";
					m_fLastDepth=m_RenderInfo.fDepth;
				}

				if (m_RenderInfo.fRoll==-1000F)
				{
					if (m_fLastRoll!=-1000F)
						sInfo += "Roll: " + Convert.ToString(m_fLastRoll) + "° (Last Known)\n";
				}
				else
				{
					sInfo += "Roll: " + Convert.ToString(m_RenderInfo.fRoll) + "°\n";
					m_fLastRoll=m_RenderInfo.fRoll;
				}

				if (m_RenderInfo.fPitch==-1000F)
				{
					if (m_fLastPitch!=-1000F)
						sInfo += "Pitch: " + Convert.ToString(m_fLastPitch) + "° (Last Known)\n";

				}
				else
				{
					sInfo += "Pitch: " + Convert.ToString(m_RenderInfo.fPitch) + "°\n";
					m_fLastPitch=m_RenderInfo.fPitch;
				}

				if (m_RenderInfo.fESpeed==-1000000F)
				{
					if (m_fLastESpeed!=-1000000F)
						sInfo += "Speed: E " + Convert.ToString(m_fLastESpeed) + "m/s, N " + Convert.ToString(m_fLastNSpeed)+ "m/s, V " +Convert.ToString(m_fLastVSpeed) +"m/s\n";
				}
				else
				{
					sInfo += "Speed: E " + Convert.ToString(m_RenderInfo.fESpeed) + "m/s, N " + Convert.ToString(m_RenderInfo.fNSpeed)+ "m/s, V " +Convert.ToString(m_RenderInfo.fVSpeed) +"m/s\n";
					m_fLastESpeed=m_RenderInfo.fESpeed;
					m_fLastNSpeed=m_RenderInfo.fNSpeed;
					m_fLastVSpeed=m_RenderInfo.fVSpeed;
				}

				if (m_RenderInfo.fESpeed==-1000000F && m_fLastESpeed==-1000000F)
				{
					if (m_RenderInfo.fSpeed==-1F)
					{
						if (m_fLastSpeed!=-1F)
							sInfo += "Speed: " + Convert.ToString(m_fLastSpeed) + " " + m_RenderInfo.sSpeedUnit + " (Last Known)\n";
					}
					else
					{
						sInfo += "Speed: " + Convert.ToString(m_RenderInfo.fSpeed) + " " + m_RenderInfo.sSpeedUnit + "\n";
						m_fLastSpeed=m_RenderInfo.fSpeed;
					}
				}

				if (m_RenderInfo.fHeading==-1F)
				{
					if (m_fLastHeading!=-1F)
						sInfo += "Heading: " + Convert.ToString(m_fLastHeading) + "° (Last Known)\n";
				}
				else
				{
					sInfo += "Heading: " + Convert.ToString(m_RenderInfo.fHeading) + "°\n";
					m_fLastHeading=m_RenderInfo.fHeading;
				}

				if (m_RenderInfo.iHour==-1 || m_RenderInfo.iMin==-1 || m_RenderInfo.fSec==-1F)
				{
					if (m_iLastHour!=-1)
						sInfo += "UTC: " +	String.Format("{0:00}",m_iLastHour) + ":"  + 
							String.Format("{0:00}",m_iLastMin) + ":" + 
							String.Format("{0:00.000}",m_fLastSec) + 
							" (Last Known)\n";
				}
				else
				{
					sInfo += "UTC: " +	String.Format("{0:00}",m_RenderInfo.iHour) + ":"  + 
						String.Format("{0:00}",m_RenderInfo.iMin) + ":" + 
						String.Format("{0:00.000}",m_RenderInfo.fSec) + 
						"\n";
					m_iLastHour=m_RenderInfo.iHour;
					m_iLastMin=m_RenderInfo.iMin;
					m_fLastSec=m_RenderInfo.fSec;
				}

				if (m_RenderInfo.iDay==-1 || m_RenderInfo.iMonth==-1 || m_RenderInfo.iYear==-1)
				{
					if (m_iLastDay!=-1)
						sInfo += "Date: " +	String.Format("{0:00}",m_iLastDay) + "/"  + 
							String.Format("{0:00}",m_iLastMonth) + "/" + 
							String.Format("{0:00}",m_iLastYear) + 
							" (Last Known)\n";
				}
				else
				{
					sInfo += "Date: " +	String.Format("{0:00}",m_RenderInfo.iDay) + "/"  + 
						String.Format("{0:00}",m_RenderInfo.iMonth) + "/" + 
						String.Format("{0:00}",m_RenderInfo.iYear) + 
						"\n";
					m_iLastDay=m_RenderInfo.iDay;
					m_iLastMonth=m_RenderInfo.iMonth;
					m_iLastYear=m_RenderInfo.iYear;
				}

				if (m_fTotalDistance>=0F)
				{
					string sUnit=(m_fTotalDistance>=1F)?"km":"m";
                    fDistance = (m_fTotalDistance < 1F) ? (m_fTotalDistance * 1000F) : m_fTotalDistance;
					sInfo += "Track Distance: " + Convert.ToString(decimal.Round(Convert.ToDecimal(fDistance),3)) +  
						sUnit + 
						"\n";
				}

				if (m_sDescriptionFrom!="")
				{
                    double dDistance = CalculateDistance(m_fLatitudeFrom, m_fLongitudeFrom, m_RenderInfo.fLat, m_RenderInfo.fLon);
                    if (double.IsNaN(dDistance) == false)
                    {
                        string sUnit = (dDistance >= 1F) ? "km" : "m";
                        dDistance = (dDistance < 1F) ? (dDistance * 1000F) : dDistance;
                        sInfo += "Distance To " + m_sDescriptionFrom + ": " + Convert.ToString(decimal.Round(Convert.ToDecimal(dDistance), 3)) +
                            sUnit +
                            "\n";
                    }

					double lat1, lat2, lon1, lon2, deltaLon, dBearing;
					lat1= Convert.ToDouble(m_RenderInfo.fLat)*(Math.PI/180);
					lat2= Convert.ToDouble(m_fLatitudeFrom)*(Math.PI/180);
					lon1= Convert.ToDouble(m_RenderInfo.fLon)*(Math.PI/180);
					lon2= Convert.ToDouble(m_fLongitudeFrom)*(Math.PI/180);
					deltaLon=lon2-lon1;
					dBearing = Math.Atan2(Math.Sin(deltaLon)*Math.Cos(lat2),Math.Cos(lat1)*Math.Sin(lat2)-Math.Sin(lat1)*Math.Cos(lat2)*Math.Cos(deltaLon));
					dBearing = dBearing * (180 / Math.PI);
					if (dBearing<0)
						dBearing=(180+dBearing)+180;
					sInfo += "Bearing To " + m_sDescriptionFrom + ": " + Convert.ToString(decimal.Round(Convert.ToDecimal(dBearing),3)) + "°\n";

				}

				if (m_RenderInfo.sComment!="")
					sInfo += "Comment: " + m_RenderInfo.sComment + "\n";

				sInfo += m_RenderInfo.sPortInfo;
			}
			//Draw some black shadow around the yellow text
			drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) +11, (int)(projectedPoint.Y),color );
			drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) +11, (int)(projectedPoint.Y)+1,color );
			color = Color.Yellow.ToArgb();
			drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) +10, (int)(projectedPoint.Y),color );
		}

		public class GPSTrackVertices
		{			
			public CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[1];
			public double [] fLat = new double[1];
			public double [] fLon = new double[1];
			public double [] fAlt = new double[1];

			public GPSTrackVertices()
			{
				vertices = new CustomVertex.PositionColored[1];
				fLat = new double[1];
				fLon = new double[1];
				fAlt = new double[1];
			}
		}

		//Update the icon position from the latest gps data
		public void SetGpsData(DrawArgs drawArgs, GPSRenderInformation renderInformation)
		{

			this.m_RenderInfo=renderInformation;
			if (m_bSignalDistance)
			{
				for (int i=0; i<m_gpsTrackerOverlay.m_iGpsPOIIndex; i++)
				{
					m_gpsTrackerOverlay.m_gpsPOI[i].m_fLatitudeFrom=m_RenderInfo.fLat;
					m_gpsTrackerOverlay.m_gpsPOI[i].m_fLongitudeFrom=m_RenderInfo.fLon;
					m_gpsTrackerOverlay.m_gpsPOI[i].m_sDescriptionFrom=m_RenderInfo.sDescription;
				}
			}
		
			if (renderInformation.bRestartTrack)
			{
				m_uVerticesCount=0;
				m_uPointCount=0;
				m_uTotalPointCount=0;
				m_fTotalDistance=0;
			}
			else
			if (m_fVerticalExaggeration != World.Settings.VerticalExaggeration)
			{
				//There was a change in vertical exaggeration, recalculate track
				uint uPoint;
				if (m_uVerticesCount==0)
					uPoint=m_uPointCount;
				else
					uPoint=50000;
				for (uint uVertices=0; uVertices<=m_uVerticesCount; uVertices++)
				{
					for (uint uP=0; uP<uPoint; uP++)
					{
						float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(this.vertices[uVertices].fLat[uP], this.vertices[uVertices].fLon[uP], (double)241));
						if (this.vertices[m_uVerticesCount].fAlt[uP] < elevation)
							this.vertices[m_uVerticesCount].fAlt[uP] = elevation;
						Vector3 v=MathEngine.SphericalToCartesian(this.vertices[uVertices].fLat[uP], this.vertices[uVertices].fLon[uP], this.m_parentWorld.EquatorialRadius + (this.vertices[uVertices].fAlt[uP] * World.Settings.VerticalExaggeration));

						this.vertices[uVertices].vertices[uP].X = v.X;
						this.vertices[uVertices].vertices[uP].Y = v.Y;
						this.vertices[uVertices].vertices[uP].Z = v.Z;
					}
					if (uVertices == m_uVerticesCount)
						uPoint = m_uPointCount;
				}
				m_fVerticalExaggeration = World.Settings.VerticalExaggeration;
			}

			if (m_uPointCount>=50000)
			{
				m_uVerticesCount++;
				m_uPointCount=0;
			}

			if (m_uPointCount==0)
			{
				this.vertices[m_uVerticesCount] = new GPSTrackVertices();
				this.vertices[m_uVerticesCount].vertices = new CustomVertex.PositionColored[50000];
				this.vertices[m_uVerticesCount].fLat = new double[50000];
				this.vertices[m_uVerticesCount].fLon = new double[50000];
				this.vertices[m_uVerticesCount].fAlt = new double[50000];
			}

			float elev = (float)(m_parentWorld.TerrainAccessor.GetElevationAt(renderInformation.fLat, renderInformation.fLon, (double)241));
			if (renderInformation.fAlt < elev)
				renderInformation.fAlt = elev;
			Vector3 vTrack=MathEngine.SphericalToCartesian(renderInformation.fLat, renderInformation.fLon, this.m_parentWorld.EquatorialRadius + (renderInformation.fAlt * World.Settings.VerticalExaggeration));

			this.vertices[m_uVerticesCount].vertices[m_uPointCount].X = vTrack.X;
			this.vertices[m_uVerticesCount].vertices[m_uPointCount].Y = vTrack.Y;
			this.vertices[m_uVerticesCount].vertices[m_uPointCount].Z = vTrack.Z;
			this.vertices[m_uVerticesCount].vertices[m_uPointCount].Color =m_RenderInfo.colorTrack.ToArgb();
			this.vertices[m_uVerticesCount].fLat[m_uPointCount] = renderInformation.fLat;
			this.vertices[m_uVerticesCount].fLon[m_uPointCount] = renderInformation.fLon;
			this.vertices[m_uVerticesCount].fAlt[m_uPointCount] = renderInformation.fAlt;
			m_uPointCount++;
			if (m_uTotalPointCount>0)
			{
                double dDistance = CalculateDistance(m_fLatFrom, m_fLonFrom, renderInformation.fLat, renderInformation.fLon);
				if (double.IsNaN(dDistance)==false)
					m_fTotalDistance=m_fTotalDistance+(float)dDistance;
				}

			m_fLatFrom=renderInformation.fLat;
			m_fLonFrom=renderInformation.fLon;
			m_uTotalPointCount++;
			
		}

        private double CalculateDistance(double dLatFrom, double dLonFrom, double dLatTo, double dLonTo)
        {
            double deltaLon, deltaLat, dDistance;
            dLatFrom = dLatFrom * (Math.PI / 180);
            dLatTo = dLatTo * (Math.PI / 180);
            dLonFrom = dLonFrom * (Math.PI / 180);
            dLonTo = dLonTo * (Math.PI / 180);
            deltaLon = dLonTo - dLonFrom;
            deltaLat = dLatTo - dLatFrom;
            if (deltaLon == 0 && deltaLat == 0)
                dDistance = (double)0;
            else
                dDistance = Math.Acos(Math.Sin(dLatFrom) * Math.Sin(dLatTo) + Math.Cos(dLatFrom) * Math.Cos(dLatTo) * Math.Cos(deltaLon)) * 6371.0;
            return dDistance;
        }
	}

    public class GPSGeoFence : RenderableObject
    {
        DrawArgs m_drawArgs;
        World m_parentWorld;
        public string m_sDescription;
        private CustomVertex.PositionColored [] vertices = new CustomVertex.PositionColored[2];
        static int hotColor = System.Drawing.Color.White.ToArgb();
        static int normalColor = Color.FromArgb(180, 255, 255, 255).ToArgb();

        int m_iTextureWidth;
        int m_iTextureHeight;
        int m_iIconWidth;
        int m_iIconHeight;
        int m_iIconWidthHalf;
        int m_iIconHeightHalf;
        Vector3 xyzPosition;

        Texture texture;
        Sprite sprite;
        Rectangle spriteSize;

        public string m_sSource="";
        public bool m_bShowInfo;
        public bool m_bDone;
        public bool m_bCancel;
        public string m_sEmail="";
        public bool m_bEmailIn;
        public bool m_bEmailOut;
        public string m_sSound="";
        public string m_sSoundOut = "";
        public bool m_bSoundIn;
        public bool m_bSoundOut;
        public bool m_bMsgBoxIn;
        public bool m_bMsgBoxOut;

        public ArrayList arrayLat = new ArrayList();
        public ArrayList arrayLon = new ArrayList();

        public GpsTrackerPlugin m_Plugin;


        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        static int VK_LCONTROL = 0xA2;

        public GPSGeoFence(World parentWorld, string sDescription, GpsTrackerPlugin Plugin)
            : base(sDescription, parentWorld.Position, Quaternion.RotationYawPitchRoll(0, 0, 0))
        {
            m_parentWorld = parentWorld;
            m_Plugin = Plugin;
            m_sDescription = sDescription;

            m_bDone = false;
            m_bCancel = false;
        }

        public override void Initialize(DrawArgs drawArgs)
        {
            m_drawArgs = drawArgs;

#if !DEBUG
			try
#endif
            {
                this.texture = TextureLoader.FromFile(drawArgs.device, "D:\\NASA\\SVN 1.4 trunk\\WorldWind\\bin\\Debug\\Plugins\\GpsTracker\\GeoFenceVertice.png", 0, 0, 1, 0, Format.Unknown, Pool.Managed, Filter.Box, Filter.Box, 0);
            }
#if !DEBUG
			catch(Microsoft.DirectX.Direct3D.InvalidDataException)
			{
				this.texture = TextureLoader.FromFile(drawArgs.device, GpsTrackerPlugin.m_sPluginDirectory + "\\gpsx.png", 0, 0, 1, 0, Format.Unknown, Pool.Managed, Filter.Box, Filter.Box, 0);
			}
#endif

            using (Surface s = this.texture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                this.m_iTextureWidth = desc.Width;
                this.m_iTextureHeight = desc.Height;
                this.m_iIconWidth = desc.Width;
                this.m_iIconHeight = desc.Height;
                this.m_iIconWidthHalf = desc.Width / 2;
                this.m_iIconHeightHalf = desc.Height / 2;

                this.spriteSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }

            this.isSelectable = true;

            this.sprite = new Sprite(drawArgs.device);

            this.isInitialized = true;

        }

        public override void Update(DrawArgs drawArgs)
        {
            if (!this.isInitialized)
                this.Initialize(drawArgs);
        }

        public override void Dispose()
        {
            this.isInitialized = false;
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            bool bRet = false;

            if (arrayLat.Count > 0)
            {
                float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt((float)arrayLat[0], (float)arrayLon[0], (double)241));
                xyzPosition = MathEngine.SphericalToCartesian((float)arrayLat[0], (float)arrayLon[0], this.m_parentWorld.EquatorialRadius + (elevation * World.Settings.VerticalExaggeration));

                if (!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(xyzPosition))
                    return false;

                Vector3 translationVector = new Vector3(
                    (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                    (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                    (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));
                Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);
#if !DEBUG
			try
#endif
                {
                    if (Math.Abs(DrawArgs.LastMousePosition.X - projectedPoint.X) < m_iIconWidthHalf &&
                        Math.Abs(DrawArgs.LastMousePosition.Y - projectedPoint.Y) < m_iIconHeightHalf)
                    {
                        byte[] pbKeyState = new byte[256];
                        GetKeyboardState(pbKeyState);
                        if ((pbKeyState[VK_LCONTROL] & 0x80) == 0x80) //Toggle Information text
                        {
                            GpsSetup gpsSetup = new GpsSetup(null, null, this);
                            gpsSetup.ShowDialog();
                            gpsSetup.Dispose();
                            bRet = true;
                        }
                    }
                }
#if !DEBUG
			catch
			{
			}
#endif
            }

            return bRet; //false
        }

        public void AddGeoFence(DrawArgs drawArgs, Point pLastMousePosition)
        {
            Angle StartLatitude;
            Angle StartLongitude;
            drawArgs.WorldCamera.PickingRayIntersection(
                pLastMousePosition.X,
                pLastMousePosition.Y,
                out StartLatitude,
                out StartLongitude);

            if (!double.IsNaN(StartLatitude.Degrees) && !double.IsNaN(StartLongitude.Degrees))
            {
                if (arrayLat.Count > 0)
                {
                    for (int i = 0; i < arrayLat.Count; i++)
                    {
                        float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt((float)arrayLat[i], (float)arrayLon[i], (double)241));
                        xyzPosition = MathEngine.SphericalToCartesian((float)arrayLat[i], (float)arrayLon[i], this.m_parentWorld.EquatorialRadius + (elevation * World.Settings.VerticalExaggeration));
                        Vector3 translationVector = new Vector3(
                            (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                            (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                            (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));
                        Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);
                        if (Math.Abs(DrawArgs.LastMousePosition.X - projectedPoint.X) < m_iIconWidthHalf &&
                                Math.Abs(DrawArgs.LastMousePosition.Y - projectedPoint.Y) < m_iIconHeightHalf)
                        {
                            if ((i == 0 && arrayLat.Count == 2) || i > 0)
                            {
                                m_bCancel = true;
                                arrayLat.Clear();
                                arrayLon.Clear();
                                break;
                            }
                        }
                    }
                }

                if (arrayLat.Count > 2 && m_bCancel==false)
                {
                    float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt((float)arrayLat[0], (float)arrayLon[0], (double)241));
                    xyzPosition = MathEngine.SphericalToCartesian((float)arrayLat[0], (float)arrayLon[0], this.m_parentWorld.EquatorialRadius + (elevation * World.Settings.VerticalExaggeration));
                    Vector3 translationVectorStart = new Vector3(
                        (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                        (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                        (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));
                    Vector3 projectedPointStart = drawArgs.WorldCamera.Project(translationVectorStart);
                    if (Math.Abs(DrawArgs.LastMousePosition.X - projectedPointStart.X) < m_iIconWidthHalf &&
                            Math.Abs(DrawArgs.LastMousePosition.Y - projectedPointStart.Y) < m_iIconHeightHalf)
                        {
                            GeoFenceSetup gpsSetup = new GeoFenceSetup(this, m_Plugin);
                            gpsSetup.ShowDialog();
                            gpsSetup.Dispose();
                            m_bDone = true;
                        }
                 }

                 if (m_bDone == false && m_bCancel == false)
                 {
                     arrayLat.Add((float)StartLatitude.Degrees);
                     arrayLon.Add((float)StartLongitude.Degrees);
                 }
            }

        }

        //Draw the icon and information text
        public override void Render(DrawArgs drawArgs)
        {
            if (!this.isInitialized || this.texture == null)
                return;
            int color;

            for (int iList = 0; iList < arrayLat.Count; iList++)
            {
                if (iList==0)
                    color = hotColor;
                else
                    color = normalColor;

                //calculate current position
                float elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt((float)arrayLat[iList], (float)arrayLon[iList], (double)241));
                xyzPosition = MathEngine.SphericalToCartesian((float)arrayLat[iList], (float)arrayLon[iList], this.m_parentWorld.EquatorialRadius + (elevation * World.Settings.VerticalExaggeration));

                if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(xyzPosition))
                {
                    Vector3 translationVector = new Vector3(
                        (float)(xyzPosition.X - drawArgs.WorldCamera.ReferenceCenter.X),
                        (float)(xyzPosition.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                        (float)(xyzPosition.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

                    Vector3 projectedPoint = drawArgs.WorldCamera.Project(translationVector);

                    this.sprite.Begin(SpriteFlags.AlphaBlend);
                    this.sprite.Transform = Matrix.Transformation2D(
                        new Vector2(0.0f, 0.0f),
                        0.0f,
                        //new Vector2(scaleWidth, scaleHeight),
                        new Vector2((float)1, (float)1),
                        new Vector2(0, 0),
                        0.0f,
                        new Vector2(projectedPoint.X + (m_iIconWidthHalf), projectedPoint.Y + (m_iIconHeightHalf)));

                    this.sprite.Draw(this.texture, this.spriteSize,
                        new Vector3(this.m_iIconWidth, this.m_iIconHeight, 0),
                        new Vector3(0, 0, 0),
                        color);

                    this.sprite.End();

                    if (iList == 0 && m_bShowInfo)
                    {
                        string sInfo = "GeoFence:\r\n" + m_sDescription;
                        //Draw some black shadow around the yellow text
                        color = Color.Black.ToArgb();
                        drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) + 1, (int)(projectedPoint.Y), color );
                        drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) + 1, (int)(projectedPoint.Y) + 1, color);
                        color = Color.Yellow.ToArgb();
                        drawArgs.toolbarFont.DrawText(null, sInfo, (int)projectedPoint.X + (m_iIconWidthHalf) + 0, (int)(projectedPoint.Y), color);
                    }
                }

                if (iList > 0)
                {
                    elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt((float)arrayLat[iList - 1], (float)arrayLon[iList - 1], (double)241));
                    Vector3 xyzPrev = MathEngine.SphericalToCartesian((float)arrayLat[iList - 1], (float)arrayLon[iList - 1], this.m_parentWorld.EquatorialRadius + (elevation * World.Settings.VerticalExaggeration));
                    
                    this.vertices[0].X = xyzPrev.X;
                    this.vertices[0].Y = xyzPrev.Y;
                    this.vertices[0].Z = xyzPrev.Z;
                    this.vertices[0].Color = Color.Red.ToArgb();

                    this.vertices[1].X = xyzPosition.X;
                    this.vertices[1].Y = xyzPosition.Y;
                    this.vertices[1].Z = xyzPosition.Z;
                    this.vertices[1].Color = Color.Red.ToArgb();

                    drawArgs.device.RenderState.ZBufferEnable = false;
                    drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
                    drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
                    drawArgs.device.Transform.World = Matrix.Translation(
                        (float)-drawArgs.WorldCamera.ReferenceCenter.X,
                        (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
                        (float)-drawArgs.WorldCamera.ReferenceCenter.Z
                        );
                    drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.vertices.Length - 1, this.vertices);

                    if (m_bDone && iList == arrayLat.Count - 1)
                    {
                        elevation = (float)(m_parentWorld.TerrainAccessor.GetElevationAt((float)arrayLat[0], (float)arrayLon[0], (double)241));
                        xyzPrev = MathEngine.SphericalToCartesian((float)arrayLat[0], (float)arrayLon[0], this.m_parentWorld.EquatorialRadius + (elevation * World.Settings.VerticalExaggeration));

                        this.vertices[0].X = xyzPrev.X;
                        this.vertices[0].Y = xyzPrev.Y;
                        this.vertices[0].Z = xyzPrev.Z;
                        this.vertices[0].Color = Color.Red.ToArgb();

                        this.vertices[1].X = xyzPosition.X;
                        this.vertices[1].Y = xyzPosition.Y;
                        this.vertices[1].Z = xyzPosition.Z;
                        this.vertices[1].Color = Color.Red.ToArgb();

                        drawArgs.device.RenderState.ZBufferEnable = false;
                        drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
                        drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
                        drawArgs.device.Transform.World = Matrix.Translation(
                            (float)-drawArgs.WorldCamera.ReferenceCenter.X,
                            (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
                            (float)-drawArgs.WorldCamera.ReferenceCenter.Z
                            );
                        drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.vertices.Length - 1, this.vertices);

                    }
                }
            }
        }

    }
	[Serializable]
	public class GPSTrack 
	{
		public double [] m_fLat;
		public double [] m_fLon;
		public float [] m_fAlt;
		public uint  m_uPointCount;

		public GPSTrack()
		{
			m_uPointCount=0;
		}

        // Resize track array size
		public void Resize(uint uDesiredSize)
		{
            double[] m_fLatResize = new double[uDesiredSize];
            Array.Copy(m_fLat, m_fLatResize, Math.Min(m_fLat.Length,uDesiredSize)); 
			m_fLat=m_fLatResize;

            double[] m_fLonResize = new double[uDesiredSize];
            Array.Copy(m_fLon, m_fLonResize, Math.Min(m_fLon.Length, uDesiredSize)); 
			m_fLon=m_fLonResize;

            float[] m_fAltResize = new float[uDesiredSize];
            Array.Copy(m_fAlt, m_fAltResize, Math.Min(m_fAlt.Length, uDesiredSize)); 
			m_fAlt=m_fAltResize;
		}

        //Set track array size
        public void SetSize(uint uDesiredSize)
        {
            m_fLat = new double[uDesiredSize];
            m_fLon = new double[uDesiredSize];
            m_fAlt = new float[uDesiredSize];
        }



		public void AddPoint(double fLat, double fLon, float fAlt)
		{
			bool bAdd=false;
			uint iCount;

			if (fAlt==-1000000F)
				fAlt=0F;

			if (m_uPointCount==1000000) //up to a million track points
			{
				for (iCount=0; iCount<m_uPointCount; iCount++)
				{
					m_fLat[iCount-1]=m_fLat[iCount];
					m_fLon[iCount-1]=m_fLon[iCount];
					m_fAlt[iCount-1]=m_fAlt[iCount];
				}
				m_uPointCount--;
				bAdd=true;
			}
			else
			if (m_uPointCount>0)
			{
				if (m_fLat[m_uPointCount-1]!=fLat ||
					m_fLon[m_uPointCount-1]!=fLon ||
					m_fAlt[m_uPointCount-1]!=fAlt)
					bAdd=true;
			}
			else
				bAdd=true;

			if (bAdd==true )
			{
				m_fLat[m_uPointCount]=fLat;
				m_fLon[m_uPointCount]=fLon;
				m_fAlt[m_uPointCount]=fAlt;
				m_uPointCount++;
			}
		}
	}
}


