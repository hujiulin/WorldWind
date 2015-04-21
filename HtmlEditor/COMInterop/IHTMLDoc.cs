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
    [ComVisible(true), ComImport(), Guid("25336920-03F9-11CF-8FD0-00AA00686F13")]
    public class HTMLDocument
    {

    }
    
    [ComVisible(true), Guid("626FC520-A41E-11CF-A731-00A0C9082637"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLDocument
    {

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetScript();
    }

    [ComVisible(true), Guid("332C4425-26CB-11D0-B483-00C04FD90119"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLDocument2
    {

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetScript();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetAll();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement GetBody();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement GetActiveElement();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetImages();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetApplets();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetLinks();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetForms();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetAnchors();


        void SetTitle(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTitle();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetScripts();


        void SetDesignMode(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDesignMode();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLSelectionObject GetSelection();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetReadyState();

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetFrames();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetEmbeds();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetPlugins();

        void SetAlinkColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetAlinkColor();

        void SetBgColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetBgColor();

        void SetFgColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetFgColor();

        void SetLinkColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetLinkColor();

        void SetVlinkColor(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetVlinkColor();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetReferrer();

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetLocation();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetLastModified();

        void SetURL(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetURL();

        void SetDomain(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDomain();

        void SetCookie(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCookie();

        void SetExpando(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool p);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetExpando();

        void SetCharset(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCharset();

        void SetDefaultCharset(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDefaultCharset();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetMimeType();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFileSize();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFileCreatedDate();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFileModifiedDate();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetFileUpdatedDate();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetSecurity();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetProtocol();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetNameProp();

        void DummyWrite(
            [In, MarshalAs(UnmanagedType.I4)]
			int psarray);

        void DummyWriteln(
            [In, MarshalAs(UnmanagedType.I4)]
			int psarray);

        [return: MarshalAs(UnmanagedType.Interface)]
        object Open(
            [In, MarshalAs(UnmanagedType.BStr)]
			string URL,
            [In, MarshalAs(UnmanagedType.Struct)]
			Object name,
            [In, MarshalAs(UnmanagedType.Struct)]
			Object features,
            [In, MarshalAs(UnmanagedType.Struct)]
			Object replace);

        void Close();

        void Clear();

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
        Object QueryCommandValue(
            [In, MarshalAs(UnmanagedType.BStr)]
			string cmdID);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ExecCommand(
            [In, MarshalAs(UnmanagedType.BStr)]
			string cmdID,
            [In, MarshalAs(UnmanagedType.Bool)]
			bool showUI,
            [In, MarshalAs(UnmanagedType.Struct)]
			Object value);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ExecCommandShowHelp(
            [In, MarshalAs(UnmanagedType.BStr)]
			string cmdID);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement CreateElement(
            [In, MarshalAs(UnmanagedType.BStr)]
			string eTag);

        void SetOnhelp(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnhelp();

        void SetOnclick(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnclick();

        void SetOndblclick(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndblclick();


        void SetOnkeyup(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnkeyup();

        void SetOnkeydown(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnkeydown();

        void SetOnkeypress(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnkeypress();

        void SetOnmouseup(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnmouseup();

        void SetOnmousedown(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnmousedown();

        void SetOnmousemove(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnmousemove();

        void SetOnmouseout(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnmouseout();

        void SetOnmouseover(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnmouseover();

        void SetOnreadystatechange(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnreadystatechange();

        void SetOnafterupdate(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnafterupdate();

        void SetOnrowexit(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnrowexit();

        void SetOnrowenter(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnrowenter();

        int SetOndragstart(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndragstart();

        void SetOnselectstart(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnselectstart();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement ElementFromPoint(
            [In, MarshalAs(UnmanagedType.I4)]
			int x,
            [In, MarshalAs(UnmanagedType.I4)]
			int y);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLWindow2 GetParentWindow();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyleSheetsCollection GetStyleSheets();

        void SetOnbeforeupdate(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnbeforeupdate();

        void SetOnerrorupdate(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnerrorupdate();

        [return: MarshalAs(UnmanagedType.BStr)]
        string toString();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyleSheet CreateStyleSheet(
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrHref,
            [In, MarshalAs(UnmanagedType.I4)]
			int lIndex);
    }

    [ComVisible(true), Guid("3050f485-98b5-11cf-bb82-00aa00bdce0b"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
    TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
   ]
    public interface IHTMLDocument3
    {
        [DispId(dispids.DISPID_IHTMLDOCUMENT3_RELEASECAPTURE)]
        void releaseCapture();

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_RECALC)]
        void recalc([In, MarshalAs(UnmanagedType.VariantBool)] bool fForce);

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_CREATETEXTNODE)]
        IHTMLDOMNode createTextNode([In] String text);

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_DOCUMENTELEMENT)]
        IHTMLElement documentElement();

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_UNIQUEID),
           TypeLibFunc(TypeLibFuncFlags.FHidden)]
        String uniqueID();

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ATTACHEVENT)]
        bool attachEvent([In] String sEvent, [In, MarshalAs(UnmanagedType.IDispatch)] object pDisp);

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_DETACHEVENT)]
        void detachEvent([In] String sEvent, [In, MarshalAs(UnmanagedType.IDispatch)] object pDisp);

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONROWSDELETE)]
        object onrowsdelete { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONROWSINSERTED)]
        object onrowsinserted { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONCELLCHANGE)]
        object oncellchange { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONDATASETCHANGED)]
        object ondatasetchanged { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONDATAAVAILABLE)]
        object ondataavailable { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONDATASETCOMPLETE)]
        object ondatasetcomplete { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONPROPERTYCHANGE)]
        object onpropertychange { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_DIR)]
        object dir { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONCONTEXTMENU)]
        object oncontextmenu { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONSTOP)]
        object onstop { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_CREATEDOCUMENTFRAGMENT)]
        IHTMLDocument2 createDocumentFragment();

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_PARENTDOCUMENT)] //hidden, restricted
        IHTMLDocument2 parentDocument();

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ENABLEDOWNLOAD)] //hidden, restricted
        bool enableDownload { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_BASEURL)] //hidden, restricted
        String baseUrl { set;get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_CHILDNODES)]
        object childNodes(); //IDispatch retval

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_INHERITSTYLESHEETS)] //hidden,restricted
        bool inheritStyleSheets { set;get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_ONBEFOREEDITFOCUS)]
        object onbeforeeditfocus { set;get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_GETELEMENTSBYNAME)]
        IHTMLElementCollection getElementsByName([In] String v);

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_GETELEMENTBYID)]
        IHTMLElement getElementById([In] String v);

        [DispId(dispids.DISPID_IHTMLDOCUMENT3_GETELEMENTSBYTAGNAME)]
        IHTMLElementCollection getElementsByTagName([In] String v);

    }



    [ComVisible(true), Guid("3050f69a-98b5-11cf-bb82-00aa00bdce0b"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
    TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
    ]
    interface IHTMLDocument4
    {
        [DispId(dispids.DISPID_IHTMLDOCUMENT4_FOCUS)]
        int focus();

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_HASFOCUS)]
        int hasFocus([Out] out bool pfFocus);

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_ONSELECTIONCHANGE)]
        object onselectionchange { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_NAMESPACES)]
        object namespaces { get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_CREATEDOCUMENTFROMURL)]
        IHTMLDocument2 createDocumentFromUrl([In] String bstrUrl, [In] String bstrOptions);

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_MEDIA)]
        String media { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_CREATEEVENTOBJECT)]
        IHTMLEventObj createEventObject();

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_FIREEVENT)]
        int fireEvent([In] String bstrEventName, [In] object pvarEventObject, [Out] out bool pfCancelled);

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_CREATERENDERSTYLE)]
        int createRenderStyle([In] String v, [Out, MarshalAs(UnmanagedType.IUnknown)] /*IHTMLRenderStyle*/ object ppIHTMLRenderStyle);

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_ONCONTROLSELECT)]
        object oncontrolselect { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT4_URLUNENCODED)]
        String URLUnencoded { get;}
    }

    [ComImport, ComVisible(true), Guid("3050f80c-98b5-11cf-bb82-00aa00bdce0b"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
    TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
    ]
    interface IHTMLDocument5
    {
        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONMOUSEWHEEL)]
        object onmousewheel { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_DOCTYPE)]
        void getDoctype([Out] out /*IHTMLDOMNode*/ Object p);

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_IMPLEMENTATION)]
        void getImplementation([Out] out /*IHTMLDOMImplementation*/ object p);

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_CREATEATTRIBUTE)]
        object /*IHTMLDOMAttribute*/ createAttribute([In] String bstrattrName);

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_CREATECOMMENT)]
        object /*IHTMLDOMNode*/ createComment([In] String bstrdata);

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONFOCUSIN)]
        object onfocusin { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONFOCUSOUT)]
        object onfocusout { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONACTIVATE)]
        object onactivate { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONDEACTIVATE)]
        object ondeactivate { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONBEFOREACTIVATE)]
        object onbeforeactivate { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_ONBEFOREDEACTIVATE)]
        object onbeforedeactivate { set; get;}

        [DispId(dispids.DISPID_IHTMLDOCUMENT5_COMPATMODE)]
        String compatMode { get;}
    }

    [ComImport, Guid("3050f613-98b5-11cf-bb82-00aa00bdce0b"),
   InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
   TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
   ]
    public interface HTMLDocumentEvents2
    {
        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONHELP)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onhelp([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONCLICK)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onclick([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONDBLCLICK)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool ondblclick([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONKEYDOWN)]
        [PreserveSig]
        void onkeydown([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONKEYUP)]
        [PreserveSig]
        void onkeyup([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONKEYPRESS)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onkeypress([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONMOUSEDOWN)]
        [PreserveSig]
        void onmousedown([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONMOUSEMOVE)]
        [PreserveSig]
        void onmousemove([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONMOUSEUP)]
        [PreserveSig]
        void onmouseup([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONMOUSEOUT)]
        [PreserveSig]
        void onmouseout([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONMOUSEOVER)]
        [PreserveSig]
        void onmouseover([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONREADYSTATECHANGE)]
        [PreserveSig]
        void onreadystatechange([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONBEFOREUPDATE)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onbeforeupdate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONAFTERUPDATE)]
        [PreserveSig]
        void onafterupdate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONROWEXIT)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onrowexit([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONROWENTER)]
        [PreserveSig]
        void onrowenter([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONDRAGSTART)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool ondragstart([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONSELECTSTART)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onselectstart([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONERRORUPDATE)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onerrorupdate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONCONTEXTMENU)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool oncontextmenu([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONSTOP)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onstop([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONROWSDELETE)]
        [PreserveSig]
        void onrowsdelete([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONROWSINSERTED)]
        [PreserveSig]
        void onrowsinserted([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONCELLCHANGE)]
        [PreserveSig]
        void oncellchange([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONPROPERTYCHANGE)]
        [PreserveSig]
        void onpropertychange([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONDATASETCHANGED)]
        [PreserveSig]
        void ondatasetchanged([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONDATAAVAILABLE)]
        [PreserveSig]
        void ondataavailable([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONDATASETCOMPLETE)]
        [PreserveSig]
        void ondatasetcomplete([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONBEFOREEDITFOCUS)]
        [PreserveSig]
        void onbeforeeditfocus([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONSELECTIONCHANGE)]
        [PreserveSig]
        void onselectionchange([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONCONTROLSELECT)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool oncontrolselect([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONMOUSEWHEEL)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onmousewheel([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONFOCUSIN)]
        [PreserveSig]
        void onfocusin([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONFOCUSOUT)]
        [PreserveSig]
        void onfocusout([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONACTIVATE)]
        [PreserveSig]
        void onactivate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONDEACTIVATE)]
        [PreserveSig]
        void ondeactivate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONBEFOREACTIVATE)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onbeforeactivate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

        [DispId(dispids.DISPID_HTMLDOCUMENTEVENTS2_ONBEFOREDEACTIVATE)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        [PreserveSig]
        bool onbeforedeactivate([In] [MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEventObj);

    }

}
