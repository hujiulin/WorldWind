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

    [ComImport, ComVisible(true), Guid("3050f5da-98b5-11cf-bb82-00aa00bdce0b"),
  InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
  TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
  ]
    public interface IHTMLDOMNode
    {
        [DispId(dispids.DISPID_IHTMLDOMNODE_NODETYPE)]
        int nodeType { get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_PARENTNODE)]
        IHTMLDOMNode parentNode { get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_HASCHILDNODES)]
        bool hasChildNodes();

        [DispId(dispids.DISPID_IHTMLDOMNODE_CHILDNODES)]
        IHTMLDOMChildrenCollection childNodes { [return: MarshalAs(UnmanagedType.Interface)] get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_ATTRIBUTES)]
        IHTMLAttributeCollection  attributes { [return: MarshalAs(UnmanagedType.Interface)] get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_INSERTBEFORE)]
        IHTMLDOMNode insertBefore([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode newChild);

        [DispId(dispids.DISPID_IHTMLDOMNODE_REMOVECHILD)]
        IHTMLDOMNode removeChild([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode oldChild);

        [DispId(dispids.DISPID_IHTMLDOMNODE_REPLACECHILD)]
        IHTMLDOMNode replaceChild([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode newChild, [In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode oldChild);

        [DispId(dispids.DISPID_IHTMLDOMNODE_CLONENODE)]
        IHTMLDOMNode cloneNode([In] bool fDeep);

        [DispId(dispids.DISPID_IHTMLDOMNODE_REMOVENODE)]
        IHTMLDOMNode removeNode([In] bool fDeep);

        [DispId(dispids.DISPID_IHTMLDOMNODE_SWAPNODE)]
        IHTMLDOMNode swapNode([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode otherNode);

        [DispId(dispids.DISPID_IHTMLDOMNODE_REPLACENODE)]
        IHTMLDOMNode replaceNode([In] IHTMLDOMNode replacement);

        [DispId(dispids.DISPID_IHTMLDOMNODE_APPENDCHILD)]
        IHTMLDOMNode appendChild([In, MarshalAs(UnmanagedType.Interface)] IHTMLDOMNode newChild);

        [DispId(dispids.DISPID_IHTMLDOMNODE_NODENAME)]
        String nodeName { get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_NODEVALUE)]
        Object nodeValue { set; get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_FIRSTCHILD)]
        IHTMLDOMNode firstChild { get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_LASTCHILD)]
        IHTMLDOMNode lastChild { get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_PREVIOUSSIBLING)]
        IHTMLDOMNode previousSibling { get;}

        [DispId(dispids.DISPID_IHTMLDOMNODE_NEXTSIBLING)]
        IHTMLDOMNode nextSibling { get;}
    };

    [ComImport, Guid("3050f5ab-98b5-11cf-bb82-00aa00bdce0b")//, 
        //InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
        //TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
    ]
    public interface IHTMLDOMChildrenCollection : IEnumerable
    {
        [
        DispId(dispids.DISPID_IHTMLDOMCHILDRENCOLLECTION_LENGTH)
        ]
        int length
        { get;}

        [DispId(dispids.DISPID_IHTMLDOMCHILDRENCOLLECTION__NEWENUM),
        ]
        [return: MarshalAs(UnmanagedType.CustomMarshaler,
        MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        new IEnumerator GetEnumerator();

        [DispId(dispids.DISPID_IHTMLDOMCHILDRENCOLLECTION_ITEM)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object item([In] int lIndex);
   
    };


    [ComImport, ComVisible(true), Guid("3050f4c3-98b5-11cf-bb82-00aa00bdce0b"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
]
    public interface IHTMLAttributeCollection : IEnumerable 
    {
        [DispId(dispids.DISPID_IHTMLATTRIBUTECOLLECTION_LENGTH)]
        int length { get;}

    [DispId(dispids.DISPID_IHTMLATTRIBUTECOLLECTION__NEWENUM),
        TypeLibFuncAttribute(TypeLibFuncFlags.FRestricted),
        MethodImpl(MethodImplOptions.InternalCall,
            MethodCodeType=MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler,
            MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler))]
        new IEnumerator GetEnumerator();

     [DispId(dispids.DISPID_IHTMLATTRIBUTECOLLECTION_ITEM)]
    [return: MarshalAs(UnmanagedType.IDispatch)]
        object item([Optional, In] object name);

    }




    [ComImport, ComVisible(true), Guid("3050f4b0-98b5-11cf-bb82-00aa00bdce0b"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual),
TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)
]
   public interface IHTMLDOMAttribute
    {
       [DispId(dispids.DISPID_IHTMLDOMATTRIBUTE_NODENAME)]
       string nodeName { get;}

       [DispId(dispids.DISPID_IHTMLDOMATTRIBUTE_NODEVALUE)]
       object nodeValue { set; get;}

       [DispId(dispids.DISPID_IHTMLDOMATTRIBUTE_SPECIFIED)]
       bool specified {[return: MarshalAs(UnmanagedType.VariantBool)] get;}
       
    }

}
