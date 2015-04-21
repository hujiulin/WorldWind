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
    [ComImport, ComVisible(true), Guid("3050f240-98b5-11cf-bb82-00aa00bdce0b"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
]
    public interface IHTMLImgElement
    {


    //     [propput, id(DISPID_IHTMLIMGELEMENT_ISMAP), displaybind, bindable] HRESULT isMap([in] VARIANT_BOOL v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_ISMAP), displaybind, bindable] HRESULT isMap([retval, out] VARIANT_BOOL * p);
        object temp { set; get;}

        //[propput, id(DISPID_IHTMLIMGELEMENT_USEMAP), displaybind, bindable] HRESULT useMap([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_USEMAP), displaybind, bindable] HRESULT useMap([retval, out] BSTR * p);
        object temp1 { set; get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_MIMETYPE)] HRESULT mimeType([retval, out] BSTR * p);
        object temp2 {  get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_FILESIZE)] HRESULT fileSize([retval, out] BSTR * p);
        object temp3 {  get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_FILECREATEDDATE)] HRESULT fileCreatedDate([retval, out] BSTR * p);
        object temp4 {  get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_FILEMODIFIEDDATE)] HRESULT fileModifiedDate([retval, out] BSTR * p);
    //[propget, id(DISPID_IHTMLIMGELEMENT_FILEUPDATEDDATE)] HRESULT fileUpdatedDate([retval, out] BSTR * p);
        object temp5 { get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_PROTOCOL)] HRESULT protocol([retval, out] BSTR * p);
        object temp6 {get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_HREF)] HRESULT href([retval, out] BSTR * p);
        object temp7 {  get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_NAMEPROP)] HRESULT nameProp([retval, out] BSTR * p);
        object temp8 { get;}
        //[propput, id(DISPID_IHTMLIMGELEMENT_BORDER), displaybind, bindable] HRESULT border([in] VARIANT v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_BORDER), displaybind, bindable] HRESULT border([retval, out] VARIANT * p);
        object temp9 { set; get;}
        //[propput, id(DISPID_IHTMLIMGELEMENT_VSPACE), displaybind, bindable] HRESULT vspace([in] long v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_VSPACE), displaybind, bindable] HRESULT vspace([retval, out] long * p);
        object temp10 { set; get;}
        //[propput, id(DISPID_IHTMLIMGELEMENT_HSPACE), displaybind, bindable] HRESULT hspace([in] long v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_HSPACE), displaybind, bindable] HRESULT hspace([retval, out] long * p);
        object temp11 { set; get;}
        //[propput, id(DISPID_IHTMLIMGELEMENT_ALT), displaybind, bindable] HRESULT alt([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_ALT), displaybind, bindable] HRESULT alt([retval, out] BSTR * p);
        object temp12 { set; get;}

        [DispId(dispids.DISPID_IHTMLIMGELEMENT_SRC)]
        string src {[param: MarshalAs(UnmanagedType.BStr)] set; [return: MarshalAs(UnmanagedType.BStr)] get;}

        //[propput, id(DISPID_IHTMLIMGELEMENT_LOWSRC), displaybind, bindable] HRESULT lowsrc([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_LOWSRC), displaybind, bindable] HRESULT lowsrc([retval, out] BSTR * p);
        object temp14 { set; get;}
        //[propput, id(DISPID_IHTMLIMGELEMENT_VRML), displaybind, bindable] HRESULT vrml([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_VRML), displaybind, bindable] HRESULT vrml([retval, out] BSTR * p);

        object temp15 { set; get;}
        //[propput, id(DISPID_IHTMLIMGELEMENT_DYNSRC), displaybind, bindable] HRESULT dynsrc([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_DYNSRC), displaybind, bindable] HRESULT dynsrc([retval, out] BSTR * p);
        object temp16 { set; get;}

        //[propget, id(DISPID_IHTMLIMGELEMENT_READYSTATE)] HRESULT readyState([retval, out] BSTR * p);
        object temp17 {  get;}
        //[propget, id(DISPID_IHTMLIMGELEMENT_COMPLETE)] HRESULT complete([retval, out] VARIANT_BOOL * p);
        object temp18 {  get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_LOOP), displaybind, bindable] HRESULT loop([in] VARIANT v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_LOOP), displaybind, bindable] HRESULT loop([retval, out] VARIANT * p);
        object temp19 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_ALIGN), displaybind, bindable] HRESULT align([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_ALIGN), displaybind, bindable] HRESULT align([retval, out] BSTR * p);
        object temp20 { set; get;}

        //[propput, id(DISPID_IHTMLIMGELEMENT_ONLOAD), displaybind, bindable] HRESULT onload([in] VARIANT v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_ONLOAD), displaybind, bindable] HRESULT onload([retval, out] VARIANT * p);
        object temp21 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_ONERROR), displaybind, bindable] HRESULT onerror([in] VARIANT v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_ONERROR), displaybind, bindable] HRESULT onerror([retval, out] VARIANT * p);
        object temp22 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_ONABORT), displaybind, bindable] HRESULT onabort([in] VARIANT v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_ONABORT), displaybind, bindable] HRESULT onabort([retval, out] VARIANT * p);
        object temp23 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_NAME), displaybind, bindable] HRESULT name([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_NAME), displaybind, bindable] HRESULT name([retval, out] BSTR * p);
        object temp24 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_WIDTH)] HRESULT width([in] long v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_WIDTH)] HRESULT width([retval, out] long * p);
        object temp25 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_HEIGHT)] HRESULT height([in] long v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_HEIGHT)] HRESULT height([retval, out] long * p);
        object temp26 { set; get;}
        
        //[propput, id(DISPID_IHTMLIMGELEMENT_START), displaybind, bindable] HRESULT start([in] BSTR v);
    //[propget, id(DISPID_IHTMLIMGELEMENT_START), displaybind, bindable] HRESULT start([retval, out] BSTR * p);
        object temp27 { set; get;}

    }
}
