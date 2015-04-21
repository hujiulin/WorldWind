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
    [ComImport, ComVisible(true), Guid("3050f1d8-98b5-11cf-bb82-00aa00bdce0b"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
]
    public interface IHTMLBodyElement
    {
        //[propput, id(DISPID_IHTMLBODYELEMENT_BACKGROUND), displaybind, bindable] HRESULT background([in] BSTR v);
        void temp1();
        //[propget, id(DISPID_IHTMLBODYELEMENT_BACKGROUND), displaybind, bindable] HRESULT background([retval, out] BSTR * p);
        void temp2();
        //[propput, id(DISPID_IHTMLBODYELEMENT_BGPROPERTIES), displaybind, bindable] HRESULT bgProperties([in] BSTR v);
        void temp3();
        //[propget, id(DISPID_IHTMLBODYELEMENT_BGPROPERTIES), displaybind, bindable] HRESULT bgProperties([retval, out] BSTR * p);
        void temp4();
        //[propput, id(DISPID_IHTMLBODYELEMENT_LEFTMARGIN), displaybind, bindable] HRESULT leftMargin([in] VARIANT v);
        void temp5();
        //[propget, id(DISPID_IHTMLBODYELEMENT_LEFTMARGIN), displaybind, bindable] HRESULT leftMargin([retval, out] VARIANT * p);
        void temp6();
        //[propput, id(DISPID_IHTMLBODYELEMENT_TOPMARGIN), displaybind, bindable] HRESULT topMargin([in] VARIANT v);
        void temp7();
        //[propget, id(DISPID_IHTMLBODYELEMENT_TOPMARGIN), displaybind, bindable] HRESULT topMargin([retval, out] VARIANT * p);
        void temp8();
        //[propput, id(DISPID_IHTMLBODYELEMENT_RIGHTMARGIN), displaybind, bindable] HRESULT rightMargin([in] VARIANT v);
        void temp9();
        //[propget, id(DISPID_IHTMLBODYELEMENT_RIGHTMARGIN), displaybind, bindable] HRESULT rightMargin([retval, out] VARIANT * p);
        void temp10();
        //[propput, id(DISPID_IHTMLBODYELEMENT_BOTTOMMARGIN), displaybind, bindable] HRESULT bottomMargin([in] VARIANT v);
        void temp11();
        //[propget, id(DISPID_IHTMLBODYELEMENT_BOTTOMMARGIN), displaybind, bindable] HRESULT bottomMargin([retval, out] VARIANT * p);
        void temp12();
        //[propput, id(DISPID_IHTMLBODYELEMENT_NOWRAP), displaybind, bindable] HRESULT noWrap([in] VARIANT_BOOL v);
        void temp13();
        //[propget, id(DISPID_IHTMLBODYELEMENT_NOWRAP), displaybind, bindable] HRESULT noWrap([retval, out] VARIANT_BOOL * p);
        void temp14();
        //[propput, id(DISPID_IHTMLBODYELEMENT_BGCOLOR), displaybind, bindable] HRESULT bgColor([in] VARIANT v);
        void temp15();
        //[propget, id(DISPID_IHTMLBODYELEMENT_BGCOLOR), displaybind, bindable] HRESULT bgColor([retval, out] VARIANT * p);
        void temp16();
        //[propput, id(DISPID_IHTMLBODYELEMENT_TEXT), displaybind, bindable] HRESULT text([in] VARIANT v);
        void temp17();
        //[propget, id(DISPID_IHTMLBODYELEMENT_TEXT), displaybind, bindable] HRESULT text([retval, out] VARIANT * p);
        void temp18();
        //[propput, id(DISPID_IHTMLBODYELEMENT_LINK), displaybind, bindable] HRESULT link([in] VARIANT v);
        void temp19();
        //[propget, id(DISPID_IHTMLBODYELEMENT_LINK), displaybind, bindable] HRESULT link([retval, out] VARIANT * p);
        void temp20();
        //[propput, id(DISPID_IHTMLBODYELEMENT_VLINK), displaybind, bindable] HRESULT vLink([in] VARIANT v);
        void temp21();
        //[propget, id(DISPID_IHTMLBODYELEMENT_VLINK), displaybind, bindable] HRESULT vLink([retval, out] VARIANT * p);
        void temp22();
        //[propput, id(DISPID_IHTMLBODYELEMENT_ALINK), displaybind, bindable] HRESULT aLink([in] VARIANT v);
        void temp23();
        //[propget, id(DISPID_IHTMLBODYELEMENT_ALINK), displaybind, bindable] HRESULT aLink([retval, out] VARIANT * p);
        void temp24();
        //[propput, id(DISPID_IHTMLBODYELEMENT_ONLOAD), displaybind, bindable] HRESULT onload([in] VARIANT v);
        void temp25();
        //[propget, id(DISPID_IHTMLBODYELEMENT_ONLOAD), displaybind, bindable] HRESULT onload([retval, out] VARIANT * p);
        void temp26();
        //[propput, id(DISPID_IHTMLBODYELEMENT_ONUNLOAD), displaybind, bindable] HRESULT onunload([in] VARIANT v);
        void temp27();
        //[propget, id(DISPID_IHTMLBODYELEMENT_ONUNLOAD), displaybind, bindable] HRESULT onunload([retval, out] VARIANT * p);
        void temp28();
        //[propput, id(DISPID_IHTMLBODYELEMENT_SCROLL), displaybind, bindable] HRESULT scroll([in] BSTR v);
        void temp29();
        //[propget, id(DISPID_IHTMLBODYELEMENT_SCROLL), displaybind, bindable] HRESULT scroll([retval, out] BSTR * p);
        void temp30();
        //[propput, id(DISPID_IHTMLBODYELEMENT_ONSELECT), displaybind, bindable] HRESULT onselect([in] VARIANT v);
        void temp31();
        //[propget, id(DISPID_IHTMLBODYELEMENT_ONSELECT), displaybind, bindable] HRESULT onselect([retval, out] VARIANT * p);
        void temp32();
        //[propput, id(DISPID_IHTMLBODYELEMENT_ONBEFOREUNLOAD), displaybind, bindable] HRESULT onbeforeunload([in] VARIANT v);
        void temp33();
        //[propget, id(DISPID_IHTMLBODYELEMENT_ONBEFOREUNLOAD), displaybind, bindable] HRESULT onbeforeunload([retval, out] VARIANT * p);
        void temp34();

        [DispId(dispids.DISPID_IHTMLBODYELEMENT_CREATETEXTRANGE)]
        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLTxtRange createTextRange();
    }


}
