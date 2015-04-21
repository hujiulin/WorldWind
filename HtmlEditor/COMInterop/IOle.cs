using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.CustomMarshalers;

namespace onlyconnect
{
    [ComVisible(true), ComImport(),
    Guid("00000119-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleInPlaceSite
    {
        //IOleWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindow([In, Out] ref IntPtr phwnd);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ContextSensitiveHelp([In, MarshalAs(UnmanagedType.Bool)] bool
            fEnterMode);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int CanInPlaceActivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnInPlaceActivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnUIActivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindowContext([Out, MarshalAs(UnmanagedType.Interface)] out IOleInPlaceFrame ppFrame,
            [Out, MarshalAs(UnmanagedType.Interface)] out IOleInPlaceUIWindow
            ppDoc, [Out] RECT lprcPosRect, [Out] RECT lprcClipRect, [In, Out] tagOIFI
            lpFrameInfo);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Scroll([In, MarshalAs(UnmanagedType.U4)] tagSIZE scrollExtent); //tagSIZE
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnUIDeactivate([In, MarshalAs(UnmanagedType.I4)] int fUndoable);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnInPlaceDeactivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int DiscardUndoState();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int DeactivateAndUndo();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnPosRectChange([In] RECT lprcPosRect);
    }


    [ComVisible(true), Guid("00000118-0000-0000-C000-000000000046"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleClientSite
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SaveObject();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetMoniker([In, MarshalAs(UnmanagedType.U4)] uint dwAssign,
            [In, MarshalAs(UnmanagedType.U4)] uint dwWhichMoniker,
            [Out, MarshalAs(UnmanagedType.Interface)] out Object ppmk);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetContainer([Out] out IOleContainer ppContainer);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ShowObject();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnShowWindow([In, MarshalAs(UnmanagedType.I4)] int fShow);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RequestNewObjectLayout();
    }

    [ComVisible(true), Guid("0000011B-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleContainer
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ParseDisplayName([In, MarshalAs(UnmanagedType.Interface)] Object pbc,
            [In, MarshalAs(UnmanagedType.LPWStr)] String pszDisplayName, [Out,
            MarshalAs(UnmanagedType.LPArray)] int[] pchEaten, [Out,
            MarshalAs(UnmanagedType.LPArray)] Object[] ppmkOut);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int EnumObjects([In, MarshalAs(UnmanagedType.U4)] uint grfFlags, [Out,
            MarshalAs(UnmanagedType.LPArray)] Object[] ppenum);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int LockContainer([In, MarshalAs(UnmanagedType.Bool)] Boolean fLock);
    }

    [ComVisible(true), ComImport(),
    Guid("00000115-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleInPlaceUIWindow
    {
        //IOleWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindow([In, Out] ref IntPtr phwnd);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ContextSensitiveHelp([In, MarshalAs(UnmanagedType.Bool)] bool
            fEnterMode);

        //IOleInPlaceUIWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetBorder([Out] RECT lprectBorder);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RequestBorderSpace([In] RECT pborderwidths);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetBorderSpace([In] RECT pborderwidths);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetActiveObject([In, MarshalAs(UnmanagedType.Interface)]
			IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.LPWStr)]
			String pszObjName);
    }

    [ComVisible(true), ComImport(),
    Guid("00000116-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleInPlaceFrame
    {
        //IOleWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindow([In, Out] ref IntPtr phwnd);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ContextSensitiveHelp([In, MarshalAs(UnmanagedType.Bool)] bool
            fEnterMode);

        //IOleInPlaceUIWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetBorder([Out] RECT lprectBorder);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RequestBorderSpace([In] RECT pborderwidths);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetBorderSpace([In] RECT pborderwidths);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetActiveObject([In, MarshalAs(UnmanagedType.Interface)]
			IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.LPWStr)]
			String pszObjName);

        //IOleInPlaceFrame
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int InsertMenus([In] IntPtr hmenuShared, [In, Out] tagOleMenuGroupWidths
            lpMenuWidths);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetMenu([In] IntPtr hmenuShared, [In] IntPtr holemenu, [In] IntPtr
            hwndActiveObject);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RemoveMenus([In] IntPtr hmenuShared);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetStatusText([In, MarshalAs(UnmanagedType.LPWStr)] String
            pszStatusText);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int EnableModeless([In, MarshalAs(UnmanagedType.Bool)] Boolean fEnable);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TranslateAccelerator([In, MarshalAs(UnmanagedType.LPStruct)] MSG
            lpmsg, [In, MarshalAs(UnmanagedType.U2)] short wID);
    }






    [ComVisible(true), ComImport(),
    Guid("9C2CAD80-3424-11CF-B670-00AA004CD6D8"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleInPlaceSiteEx
    {
        //IOleWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindow([In, Out] ref IntPtr phwnd);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ContextSensitiveHelp([In, MarshalAs(UnmanagedType.Bool)] bool
            fEnterMode);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int CanInPlaceActivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnInPlaceActivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnUIActivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindowContext([Out, MarshalAs(UnmanagedType.Interface)] out IOleInPlaceFrame ppFrame,
            [Out, MarshalAs(UnmanagedType.Interface)] out IOleInPlaceUIWindow
            ppDoc, [Out] RECT lprcPosRect, [Out] RECT lprcClipRect, [In, Out] tagOIFI
            lpFrameInfo);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Scroll([In, MarshalAs(UnmanagedType.U4)] tagSIZE scrollExtent); //tagSIZE


        //void OnUIDeactivate(
        //	[In, MarshalAs(UnmanagedType.I4)]
        //	int fUndoable);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnUIDeactivate([In, MarshalAs(UnmanagedType.I4)] int fUndoable);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnInPlaceDeactivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int DiscardUndoState();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int DeactivateAndUndo();

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnPosRectChange([In] RECT lprcPosRect);


        //IOleInPlaceSiteEx
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnInPlaceActivateEx(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pfNoRedraw,
            [In, MarshalAs(UnmanagedType.U4)]  int dwFlags
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnInPlaceDeactivateEx(
            [In, MarshalAs(UnmanagedType.Bool)] bool fNoRedraw
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RequestUIActivate();
    }


    [ComVisible(true), ComImport(),
    Guid("B722BCC7-4E68-101B-A2BC-00AA00404770"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleDocumentSite
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ActivateMe([In, MarshalAs(UnmanagedType.Interface)] IOleDocumentView
            pViewToActivate);
    }

    [ComVisible(true), Guid("B722BCC6-4E68-101B-A2BC-00AA00404770"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleDocumentView
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetInPlaceSite([In, MarshalAs(UnmanagedType.Interface)]
			IOleInPlaceSite pIPSite);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetInPlaceSite([Out, MarshalAs(UnmanagedType.Interface)] IOleInPlaceSite ppIPSite);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetDocument([Out] Object ppunk);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetRect([In] RECT prcView);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetRect([Out] RECT prcView);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetRectComplex([In] RECT prcView, [In] RECT prcHScroll, [In] RECT
            prcVScroll, [In] RECT prcSizeBox);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Show([In, MarshalAs(UnmanagedType.I4)] int fShow);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int UIActivate([In, MarshalAs(UnmanagedType.I4)] int fUIActivate);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Open();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int CloseView([In, MarshalAs(UnmanagedType.I4)]int dwReserved);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SaveViewState([In, MarshalAs(UnmanagedType.Interface)] IStream pstm);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ApplyViewState([In, MarshalAs(UnmanagedType.Interface)] IStream
            pstm);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Clone([In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceSite
            pIPSiteNew, [Out, MarshalAs(UnmanagedType.LPArray)] IOleDocumentView[]
            ppViewNew);
    }

    [ComVisible(true), ComImport(), Guid("00000122-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleDropTarget
    {

        [PreserveSig]
        int OleDragEnter(
            //[In, MarshalAs(UnmanagedType.Interface)]
            IntPtr pDataObj,
            [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
            [In, MarshalAs(UnmanagedType.U8)]
                long pt,
            [In, Out]
                ref int pdwEffect);

        [PreserveSig]
        int OleDragOver(
            [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
            [In, MarshalAs(UnmanagedType.U8)]
                long pt,
            [In, Out]
                ref int pdwEffect);

        [PreserveSig]
        int OleDragLeave();

        [PreserveSig]
        int OleDrop(
            //[In, MarshalAs(UnmanagedType.Interface)]
            IntPtr pDataObj,
            [In, MarshalAs(UnmanagedType.U4)]
                int grfKeyState,
            [In, MarshalAs(UnmanagedType.U8)]
                long pt,
            [In, Out]
                ref int pdwEffect);
    }

    [ComImport,
    Guid("00000113-0000-0000-C000-000000000046")]
    public interface IOleInPlaceObject
    {

        //IOleWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindow([In, Out] ref IntPtr phwnd);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ContextSensitiveHelp([In, MarshalAs(UnmanagedType.Bool)] bool
            fEnterMode);

        void InPlaceDeactivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int UIDeactivate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetObjectRects([In] RECT lprcPosRect,
           [In] RECT lprcClipRect);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ReactivateAndUndo();
    }


    [ComVisible(true), ComImport(),
    Guid("00000117-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleInPlaceActiveObject
    {

        //IOleWindow
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetWindow([In, Out] ref IntPtr phwnd);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ContextSensitiveHelp([In, MarshalAs(UnmanagedType.Bool)] bool
            fEnterMode);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TranslateAccelerator([In, MarshalAs(UnmanagedType.LPStruct)] MSG
            lpmsg);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnFrameWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool
            fActivate);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnDocWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ResizeBorder([In] RECT prcBorder, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pUIWindow, [In,
            MarshalAs(UnmanagedType.Bool)] Boolean fFrameWindow);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int EnableModeless([In, MarshalAs(UnmanagedType.Bool)] Boolean fEnable);
    }

    [ComVisible(true), ComImport(),
    Guid("00000112-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleObject
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetClientSite([In, MarshalAs(UnmanagedType.Interface)] IOleClientSite
            pClientSite);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetClientSite([Out, MarshalAs(UnmanagedType.Interface)] out IOleClientSite site);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetHostNames([In, MarshalAs(UnmanagedType.LPWStr)] String
            szContainerApp, [In, MarshalAs(UnmanagedType.LPWStr)] String
            szContainerObj);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Close([In, MarshalAs(UnmanagedType.U4)] uint dwSaveOption);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetMoniker([In, MarshalAs(UnmanagedType.U4)] uint dwWhichMoniker, [In,
            MarshalAs(UnmanagedType.Interface)] Object pmk);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetMoniker([In, MarshalAs(UnmanagedType.U4)] uint dwAssign, [In,
            MarshalAs(UnmanagedType.U4)] uint dwWhichMoniker, [Out, MarshalAs(UnmanagedType.Interface)] out Object moniker);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int InitFromData([In, MarshalAs(UnmanagedType.Interface)] Object
            pDataObject, [In, MarshalAs(UnmanagedType.Bool)] Boolean fCreation, [In,
            MarshalAs(UnmanagedType.U4)] uint dwReserved);
        int GetClipboardData([In, MarshalAs(UnmanagedType.U4)] uint dwReserved, out
			Object data);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int DoVerb([In, MarshalAs(UnmanagedType.I4)] int iVerb, [In] IntPtr lpmsg,
            [In, MarshalAs(UnmanagedType.Interface)] IOleClientSite pActiveSite, [In,
            MarshalAs(UnmanagedType.I4)] int lindex, [In] IntPtr hwndParent, [In] RECT
            lprcPosRect);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int EnumVerbs(out Object e); // IEnumOLEVERB
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OleUpdate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int IsUpToDate();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetUserClassID([In, Out] ref Guid pClsid);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetUserType([In, MarshalAs(UnmanagedType.U4)] uint dwFormOfType, [Out,
            MarshalAs(UnmanagedType.LPWStr)] out String userType);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetExtent([In, MarshalAs(UnmanagedType.U4)] uint dwDrawAspect, [In]
			Object pSizel); // tagSIZEL
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetExtent([In, MarshalAs(UnmanagedType.U4)] uint dwDrawAspect, [Out]
			Object pSizel); // tagSIZEL
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Advise([In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink, out
			int cookie);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Unadvise([In, MarshalAs(UnmanagedType.U4)] int dwConnection);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int EnumAdvise(out Object e);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetMiscStatus([In, MarshalAs(UnmanagedType.U4)] uint dwAspect, out int
            misc);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetColorScheme([In] Object pLogpal); // tagLOGPALETTE
    }


    [ComVisible(true), ComImport(), Guid("0000010E-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleDataObject
    {

        int OleGetData(
            FORMATETC pFormatetc,
            [Out]
			STGMEDIUM pMedium);


        int OleGetDataHere(
            FORMATETC pFormatetc,
            [In, Out]
			STGMEDIUM pMedium);


        int OleQueryGetData(
            FORMATETC pFormatetc);


        int OleGetCanonicalFormatEtc(
            FORMATETC pformatectIn,
            [Out]
			FORMATETC pformatetcOut);


        int OleSetData(
            FORMATETC pFormatectIn,
            STGMEDIUM pmedium,

            int fRelease);

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumFORMATETC OleEnumFormatEtc(
            [In, MarshalAs(UnmanagedType.U4)]
			int dwDirection);

        int OleDAdvise(
            FORMATETC pFormatetc,
            [In, MarshalAs(UnmanagedType.U4)]
			int advf,
            [In, MarshalAs(UnmanagedType.Interface)]
			object pAdvSink,
            [Out, MarshalAs(UnmanagedType.LPArray)]
			int[] pdwConnection);

        int OleDUnadvise(
            [In, MarshalAs(UnmanagedType.U4)]
			int dwConnection);

        int OleEnumDAdvise(
            [Out, MarshalAs(UnmanagedType.LPArray)]
			object[] ppenumAdvise);
    }

    [ComVisible(true), ComImport(),
   Guid("b722bccb-4e68-101b-a2bc-00aa00404770"),
   InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleCommandTarget
    {


        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int QueryStatus(
            [In, MarshalAs(UnmanagedType.Struct)]
			ref Guid pguidCmdGroup,
            UInt32 cCmds2,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] OLECMD prgCmds,
            ref OLECMDTEXT pCmdText);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Exec(
            [In, MarshalAs(UnmanagedType.Struct)]
			ref Guid pguidCmdGroup,
            [MarshalAs(UnmanagedType.U4)]
			uint nCmdId,
            [MarshalAs(UnmanagedType.U4)]
			uint nCmdExecOpt,
            ref Object pvaIn,
            ref Object pvaOut);
    }


}
