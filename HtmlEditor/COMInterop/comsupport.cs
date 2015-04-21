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
    /// <summary>
    /// Constants and interface definitions used by HtmlEditor
    /// </summary>
    ///     
    [
    System.Security.SuppressUnmanagedCodeSecurityAttribute()
    ]
    public sealed class ComSupport
    {


        public ComSupport()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

    [ComVisible(true), ComImport(),
    Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDocHostUIHandler
    {

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ShowContextMenu(uint dwID, ref win32POINT ppt,
            [MarshalAs(UnmanagedType.IUnknown)] object pcmdtReserved,
            [MarshalAs(UnmanagedType.IDispatch)]object pdispReserved);


        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetHostInfo([In, Out] DOCHOSTUIINFO info);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ShowUI([In, MarshalAs(UnmanagedType.I4)] int dwID, [In,
            MarshalAs(UnmanagedType.Interface)] IOleInPlaceActiveObject activeObject,
            [In, MarshalAs(UnmanagedType.Interface)] IOleCommandTarget
            commandTarget, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceFrame
            frame, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceUIWindow doc);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int HideUI();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int UpdateUI();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int EnableModeless([In, MarshalAs(UnmanagedType.Bool)] Boolean fEnable);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnDocWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool
            fActivate);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnFrameWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool
            fActivate);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ResizeBorder([In] RECT prcBorder, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceUIWindow pUIWindow, [In,
            MarshalAs(UnmanagedType.Bool)] Boolean fFrameWindow);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TranslateAccelerator([In] MSG msg, [In] ref Guid group, [In,
            MarshalAs(UnmanagedType.I4)] int nCmdID);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetOptionKeyPath(out IntPtr pbstrKey,
            [In, MarshalAs(UnmanagedType.U4)] uint dw);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] IOleDropTarget
            pDropTarget, [Out, MarshalAs(UnmanagedType.Interface)] out IOleDropTarget
            ppDropTarget);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetExternal([Out, MarshalAs(UnmanagedType.Interface)] out Object
            ppDispatch);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TranslateUrl([In, MarshalAs(UnmanagedType.U4)] int dwTranslate, [In,
            MarshalAs(UnmanagedType.LPWStr)] String strURLIn, [Out,
             MarshalAs(UnmanagedType.LPWStr)] out String pstrURLOut);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int FilterDataObject([In, MarshalAs(UnmanagedType.Interface)] IOleDataObject pDO,
            [Out, MarshalAs(UnmanagedType.Interface)] out IOleDataObject ppDORet);
    }


    [ComVisible(true), ComImport(), Guid("00000103-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumFORMATETC
    {

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Next(
            [In, MarshalAs(UnmanagedType.U4)]
			int celt,
            [Out]
			FORMATETC rgelt,
            [In, Out, MarshalAs(UnmanagedType.LPArray)]
			int[] pceltFetched);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Skip(
            [In, MarshalAs(UnmanagedType.U4)]
			int celt);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Reset();

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Clone(
            [Out, MarshalAs(UnmanagedType.LPArray)]
			IEnumFORMATETC[] ppenum);
    }


    [ComVisible(true), ComImport(), Guid("00000104-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumOLEVERB
    {

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Next(
            [MarshalAs(UnmanagedType.U4)]
			int celt,
            [Out]
			tagOLEVERB rgelt,
            [Out, MarshalAs(UnmanagedType.LPArray)]
			int[] pceltFetched);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Skip(
            [In, MarshalAs(UnmanagedType.U4)]
			int celt);


        void Reset();


        void Clone(
            out IEnumOLEVERB ppenum);


    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public sealed class tagOLEVERB
    {
        [MarshalAs(UnmanagedType.I4)]
        public int lVerb;

        [MarshalAs(UnmanagedType.LPWStr)]
        public String lpszVerbName;

        [MarshalAs(UnmanagedType.U4)]
        public int fuFlags;

        [MarshalAs(UnmanagedType.U4)]
        public int grfAttribs;

    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public sealed class STATDATA
    {

        [MarshalAs(UnmanagedType.U4)]
        public int advf;
        [MarshalAs(UnmanagedType.U4)]
        public int dwConnection;

    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public class STGMEDIUM
    {

        [MarshalAs(UnmanagedType.I4)]
        public int tymed;
        public IntPtr unionmember;
        public IntPtr pUnkForRelease;

    }


    [ComVisible(true), Guid("79eac9c9-baf9-11ce-8c82-00aa004ba90b"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistMoniker
    {
        void GetClassID([In, Out] ref Guid pClassID);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int IsDirty();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Load([In] int fFullyAvailable, [In,
            MarshalAs(UnmanagedType.Interface)] IMoniker pmk,
            [In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc,
            [In] int grfMode);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Save([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmk,
            [In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc,
            [In] int fRemember);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SaveCompleted([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmk,
            [In, MarshalAs(UnmanagedType.Interface)] Object pbc);
        [return: MarshalAs(UnmanagedType.Interface)]
        IMoniker GetCurMoniker();
    }


    [ComVisible(true), Guid("0000010f-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAdviseSink
    {
        void OnDataChange(
            [In] object pFormatetc,
            [In] object pStgmed
            );
        void OnViewChange(
            [In, MarshalAs(UnmanagedType.U4)] int dwAspect,
            [In, MarshalAs(UnmanagedType.I4)] int lindex
            );
        void OnRename(
            [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmk
            );
        void OnSave();
        void OnClose();
    }

    [ComVisible(true), ComImport(),
    Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStreamInit
    {
        void GetClassID([In, Out] ref Guid pClassID);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int IsDirty();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Load([In] IStream pstm);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Save([In] IStream pstm, [In,
            MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
        void GetSizeMax([Out] long pcbSize);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int InitNew();
    }



    [ComVisible(true), ComImport(),
    Guid("3050f6a0-98b5-11cf-bb82-00aa00bdce0b"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHTMLEditHost
    {

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SnapRect(
            [In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement,
            [In, Out] RECT rect,
            [In] ELEMENT_CORNER ehandle
            );
    }

    [ComVisible(true), ComImport(),
    Guid("3050f663-98b5-11cf-bb82-00aa00bdce0b"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHTMLEditServices
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int AddDesigner(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLEditDesigner pIDesigner
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RemoveDesigner(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLEditDesigner pIDesigner
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetSelectionServices(
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupContainer pIContainer,
            out IntPtr ppSelSvc
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int MoveToSelectionAnchor(
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pIStartAnchor);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int MoveToSelectionEnd(
            [In, MarshalAs(UnmanagedType.Interface)]
			/*IMarkupPointer*/ object pIEndAnchor);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SelectRange(
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pStart,
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pEnd,
            [In, MarshalAs(UnmanagedType.U4)]
			SELECTION_TYPE eType);
    };


    [ComVisible(true), ComImport(),
    Guid("6d5140c1-7436-11ce-8034-00aa006009fa"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int QueryService(
            ref System.Guid guidService,
            ref System.Guid riid,
            out IntPtr ppvObject);
    }

    [ComVisible(true), ComImport(),
    Guid("3050f662-98b5-11cf-bb82-00aa00bdce0b"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHTMLEditDesigner
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int PreHandleEvent(
            [In, MarshalAs(UnmanagedType.I4)] int inEvtDispId,
            [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int PostHandleEvent(
            [In, MarshalAs(UnmanagedType.I4)] int inEvtDispId,
            [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TranslateAccelerator(
            [In, MarshalAs(UnmanagedType.I4)] int inEvtDispId,
            [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int PostEditorEventNotify(
            [In, MarshalAs(UnmanagedType.I4)] int inEvtDispId,
            [In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pIEventObj
            );
    }

    [ComVisible(true), ComImport(),
    Guid("C4D244B0-D43E-11CF-893B-00AA00BDCE1A"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDocHostShowUI
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ShowMessage(
            [In] IntPtr hwnd, [In][MarshalAs(UnmanagedType.LPWStr)] String lpStrText,
             [In][MarshalAs(UnmanagedType.LPWStr)] String lpstrCaption,
             [In] uint dwType, [In][MarshalAs(UnmanagedType.LPWStr)] String
              lpStrHelpFile, [In] uint dwHelpContext,
               [Out] IntPtr lpresult);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ShowHelp(
            [In] IntPtr hwnd,
            [In][MarshalAs(UnmanagedType.LPWStr)] String lpHelpFile,
            [In] uint uCommand,
            [In] uint dwData,
            [In] win32POINT ptMouse,
            [Out][MarshalAs(UnmanagedType.IDispatch)] Object pDispatchObjectHit
            );
    }

    [ComImport, Guid("3050F32D-98B5-11CF-BB82-00AA00BDCE0B"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
    TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
    ]
    public interface IHTMLEventObj
    {
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_SRCELEMENT)]
        IHTMLElement SrcElement { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_ALTKEY)]
        bool AltKey { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_CTRLKEY)]
        bool CtrlKey { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_SHIFTKEY)]
        bool ShiftKey { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_RETURNVALUE)]
        Object ReturnValue { set; get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_CANCELBUBBLE)]
        bool CancelBubble { set; get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_FROMELEMENT)]
        IHTMLElement FromElement { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_TOELEMENT)]
        IHTMLElement ToElement { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_KEYCODE)]
        int keyCode { set; get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_BUTTON)]
        int Button { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_TYPE)]
        string EventType { get;}

        [DispId(dispids.DISPID_IHTMLEVENTOBJ_QUALIFIER)]
        string Qualifier { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_REASON)]
        int Reason { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_X)]
        int X { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_Y)]
        int Y { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_CLIENTX)]
        int ClientX { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_CLIENTY)]
        int ClientY { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_OFFSETX)]
        int OffsetX { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_OFFSETY)]
        int OffsetY { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_SCREENX)]
        int ScreenX { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_SCREENY)]
        int ScreenY { get;}
        [DispId(dispids.DISPID_IHTMLEVENTOBJ_SRCFILTER)]
        object SrcFilter { get;}

    }

    [ComVisible(true), Guid("3050F25E-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLStyle
    {

        void SetFontFamily(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontFamily();


        void SetFontStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontStyle();


        void SetFontObject(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontObject();


        void SetFontWeight(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontWeight();


        void SetFontSize(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetFontSize();


        void SetFont(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFont();


        void SetColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetColor();


        void SetBackground(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackground();


        void SetBackgroundColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundColor();


        void SetBackgroundImage(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundImage();


        void SetBackgroundRepeat(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundRepeat();


        void SetBackgroundAttachment(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundAttachment();


        void SetBackgroundPosition(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundPosition();


        void SetBackgroundPositionX(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundPositionX();


        void SetBackgroundPositionY(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundPositionY();


        void SetWordSpacing(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetWordSpacing();


        void SetLetterSpacing(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLetterSpacing();


        void SetTextDecoration(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextDecoration();


        void SetTextDecorationNone(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationNone();


        void SetTextDecorationUnderline(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationUnderline();


        void SetTextDecorationOverline(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationOverline();


        void SetTextDecorationLineThrough(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationLineThrough();


        void SetTextDecorationBlink(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationBlink();


        void SetVerticalAlign(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetVerticalAlign();


        void SetTextTransform(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextTransform();


        void SetTextAlign(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextAlign();


        void SetTextIndent(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetTextIndent();


        void SetLineHeight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLineHeight();


        void SetMarginTop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginTop();


        void SetMarginRight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginRight();


        void SetMarginBottom(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginBottom();


        void SetMarginLeft(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginLeft();


        void SetMargin(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetMargin();


        void SetPaddingTop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingTop();


        void SetPaddingRight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingRight();


        void SetPaddingBottom(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingBottom();


        void SetPaddingLeft(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingLeft();


        void SetPadding(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPadding();


        void SetBorder(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorder();


        void SetBorderTop(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderTop();


        void SetBorderRight(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderRight();


        void SetBorderBottom(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderBottom();


        void SetBorderLeft(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderLeft();


        void SetBorderColor(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderColor();


        void SetBorderTopColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderTopColor();


        void SetBorderRightColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderRightColor();


        void SetBorderBottomColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderBottomColor();


        void SetBorderLeftColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderLeftColor();


        void SetBorderWidth(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderWidth();


        void SetBorderTopWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderTopWidth();


        void SetBorderRightWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderRightWidth();


        void SetBorderBottomWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderBottomWidth();


        void SetBorderLeftWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderLeftWidth();


        void SetBorderStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderStyle();


        void SetBorderTopStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderTopStyle();


        void SetBorderRightStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderRightStyle();


        void SetBorderBottomStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderBottomStyle();


        void SetBorderLeftStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderLeftStyle();


        void SetWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetWidth();


        void SetHeight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetHeight();


        void SetStyleFloat(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetStyleFloat();


        void SetClear(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetClear();


        void SetDisplay(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDisplay();


        void SetVisibility(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetVisibility();


        void SetListStyleType(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyleType();


        void SetListStylePosition(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStylePosition();


        void SetListStyleImage(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyleImage();


        void SetListStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyle();


        void SetWhiteSpace(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetWhiteSpace();


        void SetTop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetTop();


        void SetLeft(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLeft();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPosition();


        void SetZIndex(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetZIndex();


        void SetOverflow(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetOverflow();


        void SetPageBreakBefore(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPageBreakBefore();


        void SetPageBreakAfter(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPageBreakAfter();


        void SetCssText(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCssText();


        void SetPixelTop(
            [In, MarshalAs(UnmanagedType.I4)]
			int p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetPixelTop();


        void SetPixelLeft(
            [In, MarshalAs(UnmanagedType.I4)]
			int p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetPixelLeft();


        void SetPixelWidth(
            [In, MarshalAs(UnmanagedType.I4)]
			int p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetPixelWidth();


        void SetPixelHeight(
            [In, MarshalAs(UnmanagedType.I4)]
			int p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetPixelHeight();


        void SetPosTop(
            [In, MarshalAs(UnmanagedType.R4)]
			float p);

        [return: MarshalAs(UnmanagedType.R4)]
        float GetPosTop();


        void SetPosLeft(
            [In, MarshalAs(UnmanagedType.R4)]
			float p);

        [return: MarshalAs(UnmanagedType.R4)]
        float GetPosLeft();


        void SetPosWidth(
            [In, MarshalAs(UnmanagedType.R4)]
			float p);

        [return: MarshalAs(UnmanagedType.R4)]
        float GetPosWidth();


        void SetPosHeight(
            [In, MarshalAs(UnmanagedType.R4)]
			float p);

        [return: MarshalAs(UnmanagedType.R4)]
        float GetPosHeight();


        void SetCursor(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCursor();


        void SetClip(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetClip();


        void SetFilter(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFilter();


        void SetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.Struct)]
			Object AttributeValue,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

    }

    [ComVisible(true), Guid("3050F2E3-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLStyleSheet
    {


        void SetTitle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTitle();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyleSheet GetParentStyleSheet();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement GetOwningElement();


        void SetDisabled(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetDisabled();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetReadOnly();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyleSheetsCollection GetImports();


        void SetHref(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetHref();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetStyleSheetType();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetId();

        [return: MarshalAs(UnmanagedType.I4)]
        int AddImport(
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrURL,
            [In, MarshalAs(UnmanagedType.I4)]
			int lIndex);

        [return: MarshalAs(UnmanagedType.I4)]
        int AddRule(
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrSelector,
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrStyle,
            [In, MarshalAs(UnmanagedType.I4)]
			int lIndex);


        void RemoveImport(
            [In, MarshalAs(UnmanagedType.I4)]
			int lIndex);


        void RemoveRule(
            [In, MarshalAs(UnmanagedType.I4)]
			int lIndex);


        void SetMedia(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetMedia();


        void SetCssText(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCssText();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyleSheetRulesCollection GetRules();

    }

    [ComVisible(true), Guid("3050F37E-98B5-11CF-BB82-00AA00BDCE0B"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
    TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    public interface IHTMLStyleSheetsCollection : IEnumerable
    {

        [DispId(dispids.DISPID_IHTMLSTYLESHEETSCOLLECTION_LENGTH)]
        int length { get;}

        [DispId(dispids.DISPID_IHTMLSTYLESHEETSCOLLECTION__NEWENUM),
        TypeLibFuncAttribute(TypeLibFuncFlags.FRestricted),
        MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler,
            MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        new IEnumerator GetEnumerator();

        [DispId(dispids.DISPID_IHTMLSTYLESHEETSCOLLECTION_ITEM)]
        object item([In] object pvarIndex);

    }

    [ComVisible(true), Guid("3050F357-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLStyleSheetRule
    {


        void SetSelectorText(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetSelectorText();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLRuleStyle GetStyle();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetReadOnly();

    }


    [ComVisible(true), Guid("3050F2E5-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLStyleSheetRulesCollection
    {

        [return: MarshalAs(UnmanagedType.I4)]
        int GetLength();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyleSheetRule Item(
            [In, MarshalAs(UnmanagedType.I4)]
			int index);

    }



    [ComVisible(true), Guid("3050F25A-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLSelectionObject
    {
        [DispId(dispids.DISPID_IHTMLSELECTIONOBJECT_CREATERANGE)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object createRange();

        [DispId(dispids.DISPID_IHTMLSELECTIONOBJECT_EMPTY)]
        void Empty();

        [DispId(dispids.DISPID_IHTMLSELECTIONOBJECT_CLEAR)]
        void Clear();

        [DispId(dispids.DISPID_IHTMLSELECTIONOBJECT_TYPE)]
        string SelectionType { [return: MarshalAs(UnmanagedType.BStr)] get;}
    }

    [ComVisible(true), Guid("3050F3CF-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLRuleStyle
    {


        void SetFontFamily(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontFamily();


        void SetFontStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontStyle();


        void SetFontObject(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontObject();


        void SetFontWeight(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontWeight();


        void SetFontSize(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetFontSize();


        void SetFont(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFont();


        void SetColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetColor();


        void SetBackground(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackground();


        void SetBackgroundColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundColor();


        void SetBackgroundImage(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundImage();


        void SetBackgroundRepeat(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundRepeat();


        void SetBackgroundAttachment(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundAttachment();


        void SetBackgroundPosition(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundPosition();


        void SetBackgroundPositionX(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundPositionX();


        void SetBackgroundPositionY(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundPositionY();


        void SetWordSpacing(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetWordSpacing();


        void SetLetterSpacing(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLetterSpacing();


        void SetTextDecoration(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextDecoration();


        void SetTextDecorationNone(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationNone();


        void SetTextDecorationUnderline(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationUnderline();


        void SetTextDecorationOverline(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationOverline();


        void SetTextDecorationLineThrough(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationLineThrough();


        void SetTextDecorationBlink(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTextDecorationBlink();


        void SetVerticalAlign(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetVerticalAlign();


        void SetTextTransform(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextTransform();


        void SetTextAlign(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextAlign();


        void SetTextIndent(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetTextIndent();


        void SetLineHeight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLineHeight();


        void SetMarginTop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginTop();


        void SetMarginRight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginRight();


        void SetMarginBottom(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginBottom();


        void SetMarginLeft(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginLeft();


        void SetMargin(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetMargin();


        void SetPaddingTop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingTop();


        void SetPaddingRight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingRight();


        void SetPaddingBottom(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingBottom();


        void SetPaddingLeft(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingLeft();


        void SetPadding(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPadding();


        void SetBorder(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorder();


        void SetBorderTop(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderTop();


        void SetBorderRight(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderRight();


        void SetBorderBottom(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderBottom();


        void SetBorderLeft(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderLeft();


        void SetBorderColor(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderColor();


        void SetBorderTopColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderTopColor();


        void SetBorderRightColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderRightColor();


        void SetBorderBottomColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderBottomColor();


        void SetBorderLeftColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderLeftColor();


        void SetBorderWidth(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderWidth();


        void SetBorderTopWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderTopWidth();


        void SetBorderRightWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderRightWidth();


        void SetBorderBottomWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderBottomWidth();


        void SetBorderLeftWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderLeftWidth();


        void SetBorderStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderStyle();


        void SetBorderTopStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderTopStyle();


        void SetBorderRightStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderRightStyle();


        void SetBorderBottomStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderBottomStyle();


        void SetBorderLeftStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderLeftStyle();


        void SetWidth(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetWidth();


        void SetHeight(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetHeight();


        void SetStyleFloat(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetStyleFloat();


        void SetClear(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetClear();


        void SetDisplay(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDisplay();


        void SetVisibility(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetVisibility();


        void SetListStyleType(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyleType();


        void SetListStylePosition(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStylePosition();


        void SetListStyleImage(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyleImage();


        void SetListStyle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyle();


        void SetWhiteSpace(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetWhiteSpace();


        void SetTop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetTop();


        void SetLeft(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLeft();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPosition();


        void SetZIndex(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetZIndex();


        void SetOverflow(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetOverflow();


        void SetPageBreakBefore(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPageBreakBefore();


        void SetPageBreakAfter(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPageBreakAfter();


        void SetCssText(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCssText();


        void SetCursor(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCursor();


        void SetClip(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetClip();


        void SetFilter(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFilter();


        void SetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.Struct)]
			Object AttributeValue,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

    }



    [ComVisible(true), Guid("332c4427-26cb-11d0-b483-00c04fd90119"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLWindow2
    {
        [DispId(dispids.DISPID_IHTMLFRAMESCOLLECTION2_ITEM)]
        object item([In] object pvarIndex);

        [DispId(dispids.DISPID_IHTMLFRAMESCOLLECTION2_LENGTH)]
        int length { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_FRAMES)]
        IHTMLFramesCollection2 frames { [return: MarshalAs(UnmanagedType.Interface)] get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_DEFAULTSTATUS)]
        string defaultStatus { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_STATUS)]
        string status { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_SETTIMEOUT)]
        int setTimeout([In] string expression, [In] int msec, [Optional, In] object language);

        [DispId(dispids.DISPID_IHTMLWINDOW2_CLEARTIMEOUT)]
        void clearTimeout([In] int timerID);

        [DispId(dispids.DISPID_IHTMLWINDOW2_ALERT)]
        void alert([In] string message); //default value ""

        [DispId(dispids.DISPID_IHTMLWINDOW2_CONFIRM)]
        bool confirm([In] string message);

        [DispId(dispids.DISPID_IHTMLWINDOW2_PROMPT)]
        object prompt([In] string message, [In] string defstr);

        [DispId(dispids.DISPID_IHTMLWINDOW2_IMAGE)]
        object Image { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_LOCATION)]
        object location { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_HISTORY)]
        object history { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_CLOSE)]
        void close();

        [DispId(dispids.DISPID_IHTMLWINDOW2_OPENER)]
        object opener { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_NAVIGATOR)]
        object navigator { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_NAME)]
        string name { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_PARENT)]
        IHTMLWindow2 parent { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_OPEN)]
        IHTMLWindow2 open([In] string url, [In] string name, [In] string features, [In, MarshalAs(UnmanagedType.VariantBool)] bool replace);

        [DispId(dispids.DISPID_IHTMLWINDOW2_SELF)]
        IHTMLWindow2 self { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_TOP)]
        IHTMLWindow2 top { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_WINDOW)]
        IHTMLWindow2 window { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_NAVIGATE)]
        void navigate([In] string url);

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONFOCUS)]
        object onfocus { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONBLUR)]
        object onblur { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONLOAD)]
        object onload { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONBEFOREUNLOAD)]
        object onbeforeunload { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONUNLOAD)]
        object onunload { set;get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONHELP)]
        object onhelp { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONERROR)]
        object onerror { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONRESIZE)]
        object onresize { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_ONSCROLL)]
        object onscroll { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_DOCUMENT)]
        IHTMLDocument2 document { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_EVENT)]
        IHTMLEventObj eventobj { get;} //event

        [DispId(dispids.DISPID_IHTMLWINDOW2__NEWENUM)]
        object _newEnum { [return: MarshalAs(UnmanagedType.IUnknown)] get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_SHOWMODALDIALOG)]
        object showModalDialog([In] string dialog, [Optional, In] object varArgIn, [Optional, In] object varOptions);

        [DispId(dispids.DISPID_IHTMLWINDOW2_SHOWHELP)]
        void showHelp([In] string helpURL, [Optional, In] object helpArg, [In] string features);

        [DispId(dispids.DISPID_IHTMLWINDOW2_SCREEN)]
        object screen { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_OPTION)]
        object Option { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_FOCUS)]
        void focus();

        [DispId(dispids.DISPID_IHTMLWINDOW2_CLOSED)]
        bool closed { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_BLUR)]
        void blur();

        [DispId(dispids.DISPID_IHTMLWINDOW2_SCROLL)]
        void scroll([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_CLIENTINFORMATION)]
        object clientInformation { get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_SETINTERVAL)]
        int setInterval([In] string expression, [In] int msec, [Optional, In] object language);

        [DispId(dispids.DISPID_IHTMLWINDOW2_CLEARINTERVAL)]
        void clearInterval([In] int timerID);

        [DispId(dispids.DISPID_IHTMLWINDOW2_OFFSCREENBUFFERING)]
        object offscreenBuffering { set; get;}

        [DispId(dispids.DISPID_IHTMLWINDOW2_EXECSCRIPT)]
        object execScript([In] string code, [In] string language); //default language JScript

        [DispId(dispids.DISPID_IHTMLWINDOW2_TOSTRING)]
        string toString();

        [DispId(dispids.DISPID_IHTMLWINDOW2_SCROLLBY)]
        void scrollBy([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_SCROLLTO)]
        void scrollTo([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_MOVETO)]
        void moveTo([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_MOVEBY)]
        void moveBy([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_RESIZETO)]
        void resizeTo([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_RESIZEBY)]
        void resizeBy([In] int x, [In] int y);

        [DispId(dispids.DISPID_IHTMLWINDOW2_EXTERNAL)]
        object external { [return: MarshalAs(UnmanagedType.IDispatch)] get;}

    }

    [ComImport, Guid("332c4426-26cb-11d0-b483-00c04fd90119"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
        TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    public interface IHTMLFramesCollection2
    {
        [DispId(dispids.DISPID_IHTMLFRAMESCOLLECTION2_ITEM)]
        object item([In] object pvarIndex);

        [DispId(dispids.DISPID_IHTMLFRAMESCOLLECTION2_LENGTH)]
        int length
        { get;}
    }

    [ComVisible(true), Guid("3050F3DB-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLCurrentStyle
    {

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPosition();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetStyleFloat();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetColor();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundColor();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontFamily();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontStyle();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFontObject();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetFontWeight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetFontSize();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundImage();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundPositionX();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBackgroundPositionY();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundRepeat();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderLeftColor();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderTopColor();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderRightColor();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderBottomColor();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderTopStyle();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderRightStyle();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderBottomStyle();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderLeftStyle();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderTopWidth();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderRightWidth();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderBottomWidth();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBorderLeftWidth();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLeft();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetTop();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetWidth();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetHeight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingLeft();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingTop();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingRight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetPaddingBottom();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextAlign();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTextDecoration();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDisplay();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetVisibility();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetZIndex();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLetterSpacing();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLineHeight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetTextIndent();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetVerticalAlign();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBackgroundAttachment();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginTop();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginRight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginBottom();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetMarginLeft();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetClear();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyleType();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStylePosition();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetListStyleImage();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetClipTop();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetClipRight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetClipBottom();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetClipLeft();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetOverflow();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPageBreakBefore();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetPageBreakAfter();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCursor();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTableLayout();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBorderCollapse();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDirection();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetBehavior();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetUnicodeBidi();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetRight();

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBottom();
    }

    [ComVisible(true), Guid("3050f604-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHTMLCaret
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int MoveCaretToPointer(
            /* [in] */ [In] [MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer,
            /* [in] */ [In] bool fScrollIntoView,
            /* [in] */ [In] CARET_DIRECTION eDir);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int MoveCaretToPointerEx(
            /* [in] */ [In] [MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer,
            /* [in] */ [In] bool fVisible,
            /* [in] */ [In] bool fScrollIntoView,
            /* [in] */ [In] CARET_DIRECTION eDir);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int MoveMarkupPointerToCaret(
            /* [in] */ [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIMarkupPointer);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int MoveDisplayPointerToCaret(
            /* [in] */ [In] [MarshalAs(UnmanagedType.Interface)] IDisplayPointer pDispPointer);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int IsVisible(
            /* [out] */ [Out] out bool pIsVisible);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Show(
            /* [in] */ [In] bool fScrollIntoView);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int Hide();

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int InsertText(
            /* [in] */ [In] [MarshalAs(UnmanagedType.LPWStr)] String pText,
            /* [in] */ [In] int lLen);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ScrollIntoView();

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetLocation(
            /* [out] */ [Out] out win32POINT pPoint,
            /* [in] */ [In, MarshalAs(UnmanagedType.Bool)] bool fTranslate);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetCaretDirection(
            /* [out] */ [Out] out CARET_DIRECTION peDir);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int SetCaretDirection(
            /* [in] */ [In] CARET_DIRECTION eDir);
    }

    [ComVisible(true), Guid("3050f69e-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDisplayPointer
    {
        //		[] HRESULT MoveToPoint([in] POINT ptPoint,[in] COORD_SYSTEM eCoordSystem,[in] IHTMLElement* pElementContext,[in] DWORD dwHitTestOptions,[out] DWORD* pdwHitTestResults);
        //		[] HRESULT MoveUnit([in] DISPLAY_MOVEUNIT eMoveUnit,[in] LONG lXPos);
        //		[] HRESULT PositionMarkupPointer([in] IMarkupPointer* pMarkupPointer);
        //		[] HRESULT MoveToPointer([in] IDisplayPointer* pDispPointer);
        //		[] HRESULT SetPointerGravity([in] POINTER_GRAVITY eGravity);
        //		[] HRESULT GetPointerGravity([out] POINTER_GRAVITY* peGravity);
        //		[] HRESULT SetDisplayGravity([in] DISPLAY_GRAVITY eGravity);
        //		[] HRESULT GetDisplayGravity([out] DISPLAY_GRAVITY* peGravity);
        //		[] HRESULT IsPositioned([out] BOOL* pfPositioned);
        //		[] HRESULT Unposition();
        //		[] HRESULT IsEqualTo([in] IDisplayPointer* pDispPointer,[out] BOOL* pfIsEqual);
        //		[] HRESULT IsLeftOf([in] IDisplayPointer* pDispPointer,[out] BOOL* pfIsLeftOf);
        //		[] HRESULT IsRightOf([in] IDisplayPointer* pDispPointer,[out] BOOL* pfIsRightOf);
        //		[] HRESULT IsAtBOL([out] BOOL* pfBOL);
        //		[] HRESULT MoveToMarkupPointer([in] IMarkupPointer* pPointer,[in] IDisplayPointer* pDispLineContext);
        //		[] HRESULT ScrollIntoView();
        //		[] HRESULT GetLineInfo([out] ILineInfo** ppLineInfo);
        //		[] HRESULT GetFlowElement([out] IHTMLElement** ppLayoutElement);
        //		[] HRESULT QueryBreaks([out] DWORD* pdwBreaks);
    }

    [ComVisible(true), Guid("3050f69d-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDisplayServices
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int CreateDisplayPointer([Out] out IDisplayPointer ppDispPointer);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TransformRect([In, Out] RECT pRect,
            [In] COORD_SYSTEM eSource,
            [In] COORD_SYSTEM eDestination,
            [In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int TransformPoint([In, Out] win32POINT pPoint, [In] COORD_SYSTEM eSource,
             [In] COORD_SYSTEM eDestination,
             [In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetCaret([Out, MarshalAs(UnmanagedType.Interface)] out IHTMLCaret ppCaret);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetComputedStyle([In] IMarkupPointer pPointer,
           [Out, MarshalAs(UnmanagedType.Interface)] out /*IHTMLComputedStyle*/ object ppComputedStyle);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int ScrollRectIntoView(
            [In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement,
            [In] RECT rect);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int HasFlowLayout([In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pIElement,
            [Out] out bool pfHasFlowLayout);
    }


    [ComVisible(true), Guid("3050F29C-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLControlRange
    {
        void Select();

        void Add(
        [In, MarshalAs(UnmanagedType.Interface)]
        /// IHTMLControlElement
        object item);

        void Remove(
        [In, MarshalAs(UnmanagedType.I4)]
        int index);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement Item(
        [In, MarshalAs(UnmanagedType.I4)]
        int index);

        void ScrollIntoView(
        [In, MarshalAs(UnmanagedType.Struct)]
        object varargStart);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryCommandSupported(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryCommandEnabled(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryCommandState(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryCommandIndeterm(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.BStr)]
        string QueryCommandText(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.Struct)]
        object QueryCommandValue(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ExecCommand(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID,
        [In, MarshalAs(UnmanagedType.Bool)]
        bool showUI,
        [In, MarshalAs(UnmanagedType.Struct)]
        object value);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ExecCommandShowHelp(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement CommonParentElement();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetLength();
    }

    [ComVisible(true), Guid("3050f65e-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHtmlControlRange2
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int addElement(
        [In, MarshalAs(UnmanagedType.Interface)]
        IHTMLElement element);
    }


    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public sealed class tagSIZE
    {
        [MarshalAs(UnmanagedType.I4)]
        public int cx;

        [MarshalAs(UnmanagedType.I4)]
        public int cy;

    }

    [ComVisible(true), StructLayout(LayoutKind.Sequential)]
    public sealed class tagSIZEL
    {
        [MarshalAs(UnmanagedType.I4)]
        public int cx;

        [MarshalAs(UnmanagedType.I4)]
        public int cy;

    }

    [ComVisible(true), Guid("00000105-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATDATA
    {

        //C#r: UNDONE (Field in interface) public static readonly    Guid iid;

        void Next(
            [In, MarshalAs(UnmanagedType.U4)]
			int celt,
            [Out]
			STATDATA rgelt,
            [Out, MarshalAs(UnmanagedType.LPArray)]
			int[] pceltFetched);


        void Skip(
            [In, MarshalAs(UnmanagedType.U4)]
			int celt);


        void Reset();


        void Clone(
            [Out, MarshalAs(UnmanagedType.LPArray)]
			IEnumSTATDATA[] ppenum);


    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public sealed class tagLOGPALETTE
    {
        [MarshalAs(UnmanagedType.U2)/*leftover(offset=0, palVersion)*/]
        public short palVersion;

        [MarshalAs(UnmanagedType.U2)/*leftover(offset=2, palNumEntries)*/]
        public short palNumEntries;

        // UNMAPPABLE: palPalEntry: Cannot be used as a structure field.
        //   /** @com.structmap(UNMAPPABLE palPalEntry) */
        //  public UNMAPPABLE palPalEntry;

    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public sealed class FORMATETC
    {

        [MarshalAs(UnmanagedType.I4)]
        public int cfFormat;
        public IntPtr ptd;
        [MarshalAs(UnmanagedType.I4)]
        public int dwAspect;
        [MarshalAs(UnmanagedType.I4)]
        public int lindex;
        [MarshalAs(UnmanagedType.I4)]
        public int tymed;

    }

}
