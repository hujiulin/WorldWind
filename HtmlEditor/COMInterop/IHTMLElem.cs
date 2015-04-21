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


    [ComImport, ComVisible(true), Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
]
   public interface IHTMLElement
    {
       [DispId(dispids.DISPID_IHTMLELEMENT_SETATTRIBUTE)]
       void SetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In]
			Object AttributeValue,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

         [DispId(dispids.DISPID_IHTMLELEMENT_GETATTRIBUTE)]
        object GetAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

        [DispId(dispids.DISPID_IHTMLELEMENT_REMOVEATTRIBUTE)]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveAttribute(
            [In, MarshalAs(UnmanagedType.BStr)]
			string strAttributeName,
            [In, MarshalAs(UnmanagedType.I4)]
			int lFlags);

       [DispId(dispids.DISPID_IHTMLELEMENT_CLASSNAME)]
       string className { set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ID)]
       string id { set; get;}

      [DispId(dispids.DISPID_IHTMLELEMENT_TAGNAME)] 
        string tagName{get;}
       
        [DispId(dispids.DISPID_IHTMLELEMENT_PARENTELEMENT)] 
        IHTMLElement parentElement{  [return: MarshalAs(UnmanagedType.Interface)] get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_STYLE)]
        IHTMLStyle style{   [return: MarshalAs(UnmanagedType.Interface)] get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONHELP)]
        object onhelp{set; get;}
         
        [DispId(dispids.DISPID_IHTMLELEMENT_ONCLICK)]
        object onclick{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONDBLCLICK)]
        object ondblclick{set; get;}

         [DispId(dispids.DISPID_IHTMLELEMENT_ONKEYDOWN)]
        object onkeydown{set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONKEYUP)]
        object onkeyup{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONKEYPRESS)]
        object onkeypress{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONMOUSEOUT)]
        object onmouseout{set; get;}
        
        [DispId(dispids.DISPID_IHTMLELEMENT_ONMOUSEOVER)]
        object onmouseover{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONMOUSEMOVE)]
        object onmousemove{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONMOUSEDOWN)]
        object onmousedown{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONMOUSEUP)]
        object onmouseup{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_DOCUMENT)]
        object document{[return: MarshalAs(UnmanagedType.IDispatch)]get;}
  
        [DispId(dispids.DISPID_IHTMLELEMENT_TITLE)]
        string title{set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_LANGUAGE)]
        string language{set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONSELECTSTART)]
       object onselectstart { set; get;}
   
         [DispId(dispids.DISPID_IHTMLELEMENT_SCROLLINTOVIEW)] 
        void scrollIntoView([Optional, In] object varargStart);

         [DispId(dispids.DISPID_IHTMLELEMENT_CONTAINS)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        bool Contains(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLElement pChild);

       [DispId(dispids.DISPID_IHTMLELEMENT_SOURCEINDEX)]
       int sourceIndex { get;}
  

        [DispId(dispids.DISPID_IHTMLELEMENT_RECORDNUMBER)] 
        object recordNumber{get;}
        
       [DispId(dispids.DISPID_IHTMLELEMENT_LANG)]
       string lang { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_OFFSETLEFT)]
       int offsetLeft { get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_OFFSETTOP)]
       int offsetTop { get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_OFFSETWIDTH)]
       int offsetWidth { get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_OFFSETHEIGHT)]
       int offsetHeight { get;}
  
        [DispId(dispids.DISPID_IHTMLELEMENT_OFFSETPARENT)]
        IHTMLElement  offsetParent{[return: MarshalAs(UnmanagedType.Interface)]get;}
  
       [DispId(dispids.DISPID_IHTMLELEMENT_INNERHTML)]
      string innerHTML { [param: MarshalAs(UnmanagedType.BStr)] set;   [return: MarshalAs(UnmanagedType.BStr)] get;}
    
       [DispId(dispids.DISPID_IHTMLELEMENT_INNERTEXT)]
       string innerText { [param: MarshalAs(UnmanagedType.BStr)] set;   [return: MarshalAs(UnmanagedType.BStr)] get;}


       [DispId(dispids.DISPID_IHTMLELEMENT_OUTERHTML)]
        string outerHTML{ [param: MarshalAs(UnmanagedType.BStr)] set;   [return: MarshalAs(UnmanagedType.BStr)] get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_OUTERTEXT)]
       string outerText { set; get;}
   
       [DispId(dispids.DISPID_IHTMLELEMENT_INSERTADJACENTHTML)]
       void insertAdjacentHTML([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string html);
    

        [DispId(dispids.DISPID_IHTMLELEMENT_INSERTADJACENTTEXT)]
       void insertAdjacentText([In, MarshalAs(UnmanagedType.BStr)] string where, [In, MarshalAs(UnmanagedType.BStr)] string text);
    
       [DispId(dispids.DISPID_IHTMLELEMENT_PARENTTEXTEDIT)]
        IHTMLElement parentTextEdit{[return: MarshalAs(UnmanagedType.Interface)]get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ISTEXTEDIT)]
       bool isTextEdit { [return: MarshalAs(UnmanagedType.Bool)] get;}
  
       [DispId(dispids.DISPID_IHTMLELEMENT_CLICK)]
       void Click();

        [DispId(dispids.DISPID_IHTMLELEMENT_FILTERS)]
        object filters{[return: MarshalAs(UnmanagedType.Interface)] get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONDRAGSTART)]
       object ondragstart { set; get;}
             
       [DispId(dispids.DISPID_IHTMLELEMENT_TOSTRING)]
        [return: MarshalAs(UnmanagedType.BStr)]
        string toString();

       [DispId(dispids.DISPID_IHTMLELEMENT_ONBEFOREUPDATE)]
       object onbeforeupdate { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONAFTERUPDATE)]
       object onafterupdate { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONERRORUPDATE)]
       object onerrorupdate { set; get;}
        

       [DispId(dispids.DISPID_IHTMLELEMENT_ONROWEXIT)]
       object onrowexit { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONROWENTER)]
       object onrowenter { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONDATASETCHANGED)]
       object ondatasetchanged { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONDATAAVAILABLE)]
       object ondataavailable { set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENT_ONDATASETCOMPLETE)]
       object ondatasetcomplete { set; get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ONFILTERCHANGE)]
       object onfilterchange{set; get;}
 
       [DispId(dispids.DISPID_IHTMLELEMENT_CHILDREN)]
       object children { [return: MarshalAs(UnmanagedType.IDispatch)] get;}

       [DispId(dispids.DISPID_IHTMLELEMENT_ALL)]
       object all { [return: MarshalAs(UnmanagedType.IDispatch)] get;}
            
    //[propput, id(DISPID_IHTMLELEMENT_CLASSNAME), bindable] HRESULT className([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_CLASSNAME), bindable] HRESULT className([retval, out] BSTR * p);
    //[propput, id(DISPID_IHTMLELEMENT_ID), bindable] HRESULT id([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_ID), bindable] HRESULT id([retval, out] BSTR * p);
    //[propget, id(DISPID_IHTMLELEMENT_TAGNAME)] HRESULT tagName([retval, out] BSTR * p);
    //[propget, id(DISPID_IHTMLELEMENT_PARENTELEMENT)] HRESULT parentElement([retval, out] IHTMLElement* * p);
    //[propget, id(DISPID_IHTMLELEMENT_STYLE), nonbrowsable] HRESULT style([retval, out] IHTMLStyle* * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONHELP), displaybind, bindable] HRESULT onhelp([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONHELP), displaybind, bindable] HRESULT onhelp([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONCLICK), displaybind, bindable] HRESULT onclick([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONCLICK), displaybind, bindable] HRESULT onclick([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONDBLCLICK), displaybind, bindable] HRESULT ondblclick([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONDBLCLICK), displaybind, bindable] HRESULT ondblclick([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONKEYDOWN), displaybind, bindable] HRESULT onkeydown([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONKEYDOWN), displaybind, bindable] HRESULT onkeydown([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONKEYUP), displaybind, bindable] HRESULT onkeyup([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONKEYUP), displaybind, bindable] HRESULT onkeyup([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONKEYPRESS), displaybind, bindable] HRESULT onkeypress([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONKEYPRESS), displaybind, bindable] HRESULT onkeypress([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONMOUSEOUT), displaybind, bindable] HRESULT onmouseout([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONMOUSEOUT), displaybind, bindable] HRESULT onmouseout([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONMOUSEOVER), displaybind, bindable] HRESULT onmouseover([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONMOUSEOVER), displaybind, bindable] HRESULT onmouseover([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONMOUSEMOVE), displaybind, bindable] HRESULT onmousemove([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONMOUSEMOVE), displaybind, bindable] HRESULT onmousemove([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONMOUSEDOWN), displaybind, bindable] HRESULT onmousedown([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONMOUSEDOWN), displaybind, bindable] HRESULT onmousedown([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONMOUSEUP), displaybind, bindable] HRESULT onmouseup([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONMOUSEUP), displaybind, bindable] HRESULT onmouseup([retval, out] VARIANT * p);
    //[propget, id(DISPID_IHTMLELEMENT_DOCUMENT)] HRESULT document([retval, out] IDispatch* * p);
    //[propput, id(DISPID_IHTMLELEMENT_TITLE), displaybind, bindable] HRESULT title([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_TITLE), displaybind, bindable] HRESULT title([retval, out] BSTR * p);
    //[propput, id(DISPID_IHTMLELEMENT_LANGUAGE), displaybind, bindable] HRESULT language([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_LANGUAGE), displaybind, bindable] HRESULT language([retval, out] BSTR * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONSELECTSTART), displaybind, bindable] HRESULT onselectstart([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONSELECTSTART), displaybind, bindable] HRESULT onselectstart([retval, out] VARIANT * p);
    //[id(DISPID_IHTMLELEMENT_SCROLLINTOVIEW)] HRESULT scrollIntoView([optional, in] VARIANT varargStart);
    //[id(DISPID_IHTMLELEMENT_CONTAINS)] HRESULT contains([in] IHTMLElement* pChild,[retval, out] VARIANT_BOOL* pfResult);
    //[propget, id(DISPID_IHTMLELEMENT_SOURCEINDEX), bindable] HRESULT sourceIndex([retval, out] long * p);
    //[propget, id(DISPID_IHTMLELEMENT_RECORDNUMBER)] HRESULT recordNumber([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_LANG)] HRESULT lang([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_LANG)] HRESULT lang([retval, out] BSTR * p);
    //[propget, id(DISPID_IHTMLELEMENT_OFFSETLEFT)] HRESULT offsetLeft([retval, out] long * p);
    //[propget, id(DISPID_IHTMLELEMENT_OFFSETTOP)] HRESULT offsetTop([retval, out] long * p);
    //[propget, id(DISPID_IHTMLELEMENT_OFFSETWIDTH)] HRESULT offsetWidth([retval, out] long * p);
    //[propget, id(DISPID_IHTMLELEMENT_OFFSETHEIGHT)] HRESULT offsetHeight([retval, out] long * p);
    //[propget, id(DISPID_IHTMLELEMENT_OFFSETPARENT)] HRESULT offsetParent([retval, out] IHTMLElement* * p);
    //[propput, id(DISPID_IHTMLELEMENT_INNERHTML)] HRESULT innerHTML([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_INNERHTML)] HRESULT innerHTML([retval, out] BSTR * p);
    //[propput, id(DISPID_IHTMLELEMENT_INNERTEXT)] HRESULT innerText([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_INNERTEXT)] HRESULT innerText([retval, out] BSTR * p);
      //[propput, id(DISPID_IHTMLELEMENT_OUTERTEXT)] HRESULT outerText([in] BSTR v);
    //[propget, id(DISPID_IHTMLELEMENT_OUTERTEXT)] HRESULT outerText([retval, out] BSTR * p);
    //[id(DISPID_IHTMLELEMENT_INSERTADJACENTHTML)] HRESULT insertAdjacentHTML([in] BSTR where,[in] BSTR html);
    //[id(DISPID_IHTMLELEMENT_INSERTADJACENTTEXT)] HRESULT insertAdjacentText([in] BSTR where,[in] BSTR text);
    //[propget, id(DISPID_IHTMLELEMENT_PARENTTEXTEDIT)] HRESULT parentTextEdit([retval, out] IHTMLElement* * p);
    //[propget, id(DISPID_IHTMLELEMENT_ISTEXTEDIT)] HRESULT isTextEdit([retval, out] VARIANT_BOOL * p);
    //[id(DISPID_IHTMLELEMENT_CLICK)] HRESULT click();
    //[propget, id(DISPID_IHTMLELEMENT_FILTERS)] HRESULT filters([retval, out] IHTMLFiltersCollection* * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONDRAGSTART), displaybind, bindable] HRESULT ondragstart([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONDRAGSTART), displaybind, bindable] HRESULT ondragstart([retval, out] VARIANT * p);
    //[id(DISPID_IHTMLELEMENT_TOSTRING)] HRESULT toString([retval, out] BSTR* String);
    //[propput, id(DISPID_IHTMLELEMENT_ONBEFOREUPDATE), displaybind, bindable] HRESULT onbeforeupdate([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONBEFOREUPDATE), displaybind, bindable] HRESULT onbeforeupdate([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONAFTERUPDATE), displaybind, bindable] HRESULT onafterupdate([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONAFTERUPDATE), displaybind, bindable] HRESULT onafterupdate([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONERRORUPDATE), displaybind, bindable] HRESULT onerrorupdate([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONERRORUPDATE), displaybind, bindable] HRESULT onerrorupdate([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONROWEXIT), displaybind, bindable] HRESULT onrowexit([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONROWEXIT), displaybind, bindable] HRESULT onrowexit([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONROWENTER), displaybind, bindable] HRESULT onrowenter([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONROWENTER), displaybind, bindable] HRESULT onrowenter([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONDATASETCHANGED), displaybind, bindable] HRESULT ondatasetchanged([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONDATASETCHANGED), displaybind, bindable] HRESULT ondatasetchanged([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONDATAAVAILABLE), displaybind, bindable] HRESULT ondataavailable([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONDATAAVAILABLE), displaybind, bindable] HRESULT ondataavailable([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONDATASETCOMPLETE), displaybind, bindable] HRESULT ondatasetcomplete([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONDATASETCOMPLETE), displaybind, bindable] HRESULT ondatasetcomplete([retval, out] VARIANT * p);
    //[propput, id(DISPID_IHTMLELEMENT_ONFILTERCHANGE), displaybind, bindable] HRESULT onfilterchange([in] VARIANT v);
    //[propget, id(DISPID_IHTMLELEMENT_ONFILTERCHANGE), displaybind, bindable] HRESULT onfilterchange([retval, out] VARIANT * p);
    //[propget, id(DISPID_IHTMLELEMENT_CHILDREN)] HRESULT children([retval, out] IDispatch* * p);
    //[propget, id(DISPID_IHTMLELEMENT_ALL)] HRESULT all([retval, out] IDispatch* * p);


    }

    [ComVisible(true), Guid("3050F434-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLElement2
    {

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetScopeName();


        void SetCapture(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool containerCapture);


        void ReleaseCapture();


        void SetOnlosecapture(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnlosecapture();

        [return: MarshalAs(UnmanagedType.BStr)]
        string ComponentFromPoint(
            [In, MarshalAs(UnmanagedType.I4)]
			int x,
            [In, MarshalAs(UnmanagedType.I4)]
			int y);


        void DoScroll(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object component);


        void SetOnscroll(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnscroll();


        void SetOndrag(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndrag();


        void SetOndragend(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndragend();


        void SetOndragenter(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndragenter();


        void SetOndragover(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndragover();


        void SetOndragleave(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndragleave();


        void SetOndrop(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOndrop();


        void SetOnbeforecut(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnbeforecut();


        void SetOncut(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOncut();


        void SetOnbeforecopy(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnbeforecopy();


        void SetOncopy(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOncopy();


        void SetOnbeforepaste(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnbeforepaste();


        void SetOnpaste(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnpaste();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLCurrentStyle GetCurrentStyle();


        void SetOnpropertychange(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnpropertychange();

        [return: MarshalAs(UnmanagedType.Interface)]
        object /*IHTMLRectCollection*/ GetClientRects();

        [return: MarshalAs(UnmanagedType.Interface)]
        object /*IHTMLRect*/ GetBoundingClientRect();


        void SetExpression(
            [In, MarshalAs(UnmanagedType.BStr)]
			string propname,
            [In, MarshalAs(UnmanagedType.BStr)]
			string expression,
            [In, MarshalAs(UnmanagedType.BStr)]
			string language);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetExpression(
            [In, MarshalAs(UnmanagedType.BStr)]
			Object propname);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveExpression(
            [In, MarshalAs(UnmanagedType.BStr)]
			string propname);


        void SetTabIndex(
            [In, MarshalAs(UnmanagedType.I2)]
			short p);

        [return: MarshalAs(UnmanagedType.I2)]
        short GetTabIndex();


        void Focus();


        void SetAccessKey(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetAccessKey();


        void SetOnblur(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnblur();


        void SetOnfocus(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnfocus();


        void SetOnresize(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnresize();


        void Blur();


        void AddFilter(
            [In, MarshalAs(UnmanagedType.Interface)]
			object pUnk);


        void RemoveFilter(
            [In, MarshalAs(UnmanagedType.Interface)]
			object pUnk);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetClientHeight();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetClientWidth();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetClientTop();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetClientLeft();

        [return: MarshalAs(UnmanagedType.Bool)]
        bool AttachEvent(
            [In, MarshalAs(UnmanagedType.BStr)]
			string ev,
            [In, MarshalAs(UnmanagedType.Interface)]
			object pdisp);


        void DetachEvent(
            [In, MarshalAs(UnmanagedType.BStr)]
			string ev,
            [In, MarshalAs(UnmanagedType.Interface)]
			object pdisp);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetReadyState();


        void SetOnreadystatechange(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnreadystatechange();


        void SetOnrowsdelete(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnrowsdelete();


        void SetOnrowsinserted(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnrowsinserted();


        void SetOncellchange(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOncellchange();


        void SetDir(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDir();

        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateControlRange();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetScrollHeight();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetScrollWidth();


        void SetScrollTop(
            [In, MarshalAs(UnmanagedType.I4)]
			int p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetScrollTop();


        void SetScrollLeft(
            [In, MarshalAs(UnmanagedType.I4)]
			int p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetScrollLeft();


        void ClearAttributes();


        void MergeAttributes(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLElement mergeThis);


        void SetOncontextmenu(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOncontextmenu();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement InsertAdjacentElement(
            [In, MarshalAs(UnmanagedType.BStr)]
			string where,
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLElement insertedElement);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElement ApplyElement(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLElement apply,
            [In, MarshalAs(UnmanagedType.BStr)]
			string where);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetAdjacentText(
            [In, MarshalAs(UnmanagedType.BStr)]
			string where);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ReplaceAdjacentText(
            [In, MarshalAs(UnmanagedType.BStr)]
			string where,
            [In, MarshalAs(UnmanagedType.BStr)]
			string newText);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetCanHaveChildren();

        [return: MarshalAs(UnmanagedType.I4)]
        int AddBehavior(
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrUrl,
            [In]
			ref Object pvarFactory);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveBehavior(
            [In, MarshalAs(UnmanagedType.I4)]
			int cookie);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyle GetRuntimeStyle();

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetBehaviorUrns();


        void SetTagUrn(
            [In, MarshalAs(UnmanagedType.BStr)]
			string p);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetTagUrn();


        void SetOnbeforeeditfocus(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnbeforeeditfocus();

        [return: MarshalAs(UnmanagedType.I4)]
        int GetReadyStateValue();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLElementCollection GetElementsByTagName(
            [In, MarshalAs(UnmanagedType.BStr)]
			string v);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyle GetBaseStyle();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLCurrentStyle GetBaseCurrentStyle();

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyle GetBaseRuntimeStyle();


        void SetOnmousehover(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnmousehover();


        void SetOnkeydownpreview(
            [In, MarshalAs(UnmanagedType.Struct)]
			Object p);

        [return: MarshalAs(UnmanagedType.Struct)]
        Object GetOnkeydownpreview();

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetBehavior(
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrName,
            [In, MarshalAs(UnmanagedType.BStr)]
			string bstrUrn);
    }

    [ComImport, ComVisible(true), Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
]

    public interface IHTMLElementCollection : IEnumerable
    {

        [DispId(dispids.DISPID_IHTMLELEMENTCOLLECTION_TOSTRING)]
        [return: MarshalAs(UnmanagedType.BStr)]
        string toString();

        [DispId(dispids.DISPID_IHTMLELEMENTCOLLECTION_LENGTH)]
        int length { set; get;}

        [DispId(dispids.DISPID_IHTMLELEMENTCOLLECTION__NEWENUM),
        TypeLibFuncAttribute(TypeLibFuncFlags.FRestricted),
        MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler,
            MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        new IEnumerator GetEnumerator();

        [return: MarshalAs(UnmanagedType.Interface)]
        object Item(
            [In, MarshalAs(UnmanagedType.Struct)]
			object name,
            [In, MarshalAs(UnmanagedType.Struct)]
			object index);

        [DispId(dispids.DISPID_IHTMLELEMENTCOLLECTION_ITEM)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object item([Optional, In] object name, [Optional, In] object index);

        [DispId(dispids.DISPID_IHTMLELEMENTCOLLECTION_TAGS)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object tags([In] object tagName);
    }

    [ComVisible(true), Guid("3050F6C9-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    public interface IHTMLElementDefaults
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLStyle GetStyle();

        void SetTabStop(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool v);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetTabStop();


        void SetViewInheritStyle(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool v);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetViewInheritStyle();


        void SetViewMasterTab(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool v);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetViewMasterTab();


        void SetScrollSegmentX(
            [In, MarshalAs(UnmanagedType.I4)]
			int v);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetScrollSegmentX();


        void SetScrollSegmentY(
            [In, MarshalAs(UnmanagedType.I4)]
			object p);

        [return: MarshalAs(UnmanagedType.I4)]
        int GetScrollSegmentY();


        void SetIsMultiLine(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool v);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetIsMultiLine();


        void SetContentEditable(
            [In, MarshalAs(UnmanagedType.BStr)]
			string v);

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetContentEditable();


        void SetCanHaveHTML(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool v);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetCanHaveHTML();

        void SetViewLink(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLDocument viewLink);

        [return: MarshalAs(UnmanagedType.Interface)]
        IHTMLDocument GetViewLink();

        void SetFrozen(
            [In, MarshalAs(UnmanagedType.Bool)]
			bool v);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetFrozen();
    }
}
