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

    [ComImport, ComVisible(true), Guid("3050f220-98b5-11cf-bb82-00aa00bdce0b"),
      InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
      TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
      ]
    public interface IHTMLTxtRange
    {
        [DispId(dispids.DISPID_IHTMLTXTRANGE_HTMLTEXT)]
        string htmlText
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        [DispId(dispids.DISPID_IHTMLTXTRANGE_TEXT)]
        string text
        {
            set;
            get;
        }

        [DispId(dispids.DISPID_IHTMLTXTRANGE_PARENTELEMENT)]
        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement parentElement();

        //[id(DISPID_IHTMLTXTRANGE_DUPLICATE)] HRESULT duplicate([retval, out] IHTMLTxtRange** Duplicate);
        //void temp4();
        [DispId(dispids.DISPID_IHTMLTXTRANGE_DUPLICATE)]
        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLTxtRange duplicate();

        //[id(DISPID_IHTMLTXTRANGE_INRANGE)] HRESULT inRange([in] IHTMLTxtRange* Range,[retval, out] VARIANT_BOOL* InRange);
        void temp5();

        //[id(DISPID_IHTMLTXTRANGE_ISEQUAL)] HRESULT isEqual([in] IHTMLTxtRange* Range,[retval, out] VARIANT_BOOL* IsEqual);
        void temp6();

        //[id(DISPID_IHTMLTXTRANGE_SCROLLINTOVIEW)] HRESULT scrollIntoView([defaultvalue(-1), in] VARIANT_BOOL fStart);
        void temp7();

        //[id(DISPID_IHTMLTXTRANGE_COLLAPSE)] HRESULT collapse([defaultvalue(-1), in] VARIANT_BOOL Start);
        //void temp8();
        [DispId(dispids.DISPID_IHTMLTXTRANGE_COLLAPSE)]
        HRESULT collapse(
        [In, MarshalAs(UnmanagedType.Bool)]
        bool start);

        //[id(DISPID_IHTMLTXTRANGE_EXPAND)] HRESULT expand([in] BSTR Unit,[retval, out] VARIANT_BOOL* Success);
        void temp9();

        [DispId(dispids.DISPID_IHTMLTXTRANGE_MOVE)]
        [return: MarshalAs(UnmanagedType.I4)]
        int move([In] string Unit, [In] int Count);

        [DispId(dispids.DISPID_IHTMLTXTRANGE_MOVESTART)]
        [return: MarshalAs(UnmanagedType.I4)]
        int moveStart([In] string Unit, [In] int Count);

        [DispId(dispids.DISPID_IHTMLTXTRANGE_MOVEEND)]
        [return: MarshalAs(UnmanagedType.I4)]
        int moveEnd([In] string Unit, [In] int Count);


        //[id(DISPID_IHTMLTXTRANGE_SELECT)] HRESULT select();
        //void temp13();
        [DispId(dispids.DISPID_IHTMLTXTRANGE_SELECT)]
        void select();

        //[id(DISPID_IHTMLTXTRANGE_PASTEHTML)] HRESULT pasteHTML([in] BSTR html);
        void pasteHTML([In, MarshalAs(UnmanagedType.BStr)] string html);

        //[id(DISPID_IHTMLTXTRANGE_MOVETOELEMENTTEXT)] HRESULT moveToElementText([in] IHTMLElement* element);
        //void temp15();
        [DispId(dispids.DISPID_IHTMLTXTRANGE_MOVETOELEMENTTEXT)]
        void moveToElementText(
        [In, MarshalAs(UnmanagedType.Interface)]
        IHTMLElement element);

        [DispId(dispids.DISPID_IHTMLTXTRANGE_SETENDPOINT)]
        void setEndPoint(
        [In, MarshalAs(UnmanagedType.BStr)]
        string how,
        [In, MarshalAs(UnmanagedType.Interface)]
        IHTMLTxtRange sourceRange);

        [DispId(dispids.DISPID_IHTMLTXTRANGE_COMPAREENDPOINTS)]
        [return: MarshalAs(UnmanagedType.I4)]
        int compareEndPoints(
        [In, MarshalAs(UnmanagedType.BStr)]
        string how,
        [In, MarshalAs(UnmanagedType.Interface)]
        IHTMLTxtRange sourceRange);

        //[id(DISPID_IHTMLTXTRANGE_FINDTEXT)] HRESULT findText([in] BSTR String,[defaultvalue(1073741823), in] long count,[defaultvalue(0), in] long Flags,[retval, out] VARIANT_BOOL* Success);
        void temp18();

        //[id(DISPID_IHTMLTXTRANGE_MOVETOPOINT)] HRESULT moveToPoint([in] long x,[in] long y);
        void temp19();

        //[id(DISPID_IHTMLTXTRANGE_GETBOOKMARK)] HRESULT getBookmark([retval, out] BSTR* Boolmark);
        void temp20();

        //[id(DISPID_IHTMLTXTRANGE_MOVETOBOOKMARK)] HRESULT moveToBookmark([in] BSTR Bookmark,[retval, out] VARIANT_BOOL* Success);
        void temp21();

        //[id(DISPID_IHTMLTXTRANGE_QUERYCOMMANDSUPPORTED)] HRESULT queryCommandSupported([in] BSTR cmdID,[retval, out] VARIANT_BOOL* pfRet);
        void temp22();

        //[id(DISPID_IHTMLTXTRANGE_QUERYCOMMANDENABLED)] HRESULT queryCommandEnabled([in] BSTR cmdID,[retval, out] VARIANT_BOOL* pfRet);
        void temp23();

        //[id(DISPID_IHTMLTXTRANGE_QUERYCOMMANDSTATE)] HRESULT queryCommandState([in] BSTR cmdID,[retval, out] VARIANT_BOOL* pfRet);
        void temp24();

        //[id(DISPID_IHTMLTXTRANGE_QUERYCOMMANDINDETERM)] HRESULT queryCommandIndeterm([in] BSTR cmdID,[retval, out] VARIANT_BOOL* pfRet);
        void temp25();

        //[id(DISPID_IHTMLTXTRANGE_QUERYCOMMANDTEXT)] HRESULT queryCommandText([in] BSTR cmdID,[retval, out] BSTR* pcmdText);
        void temp26();

        //[id(DISPID_IHTMLTXTRANGE_QUERYCOMMANDVALUE)] HRESULT queryCommandValue([in] BSTR cmdID,[retval, out] VARIANT* pcmdValue);
        void temp27();

        //HRESULT execCommand([in] BSTR cmdID,[defaultvalue(0), in] VARIANT_BOOL showUI,[optional, in] VARIANT value,[retval, out] VARIANT_BOOL* pfRet);
        // void temp28();
        [DispId(dispids.DISPID_IHTMLTXTRANGE_EXECCOMMAND)]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool execCommand(
        [In, MarshalAs(UnmanagedType.BStr)]
        string cmdID,
        [In, MarshalAs(UnmanagedType.Bool)]
        bool showUI,
        [In, MarshalAs(UnmanagedType.Struct)]
        Object value);

        //[id(DISPID_IHTMLTXTRANGE_EXECCOMMANDSHOWHELP)] HRESULT execCommandShowHelp([in] BSTR cmdID,[retval, out] VARIANT_BOOL* pfRet);
        void temp29();
    }
}
