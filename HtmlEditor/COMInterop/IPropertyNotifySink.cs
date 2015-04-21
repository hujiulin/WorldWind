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
    Guid("9BFBBC02-EFF1-101A-84ED-00AA00341D07"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyNotifySink
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnChanged(
            [In, MarshalAs(UnmanagedType.I4)] int DispId
            );

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int OnRequestEdit(
            [In, MarshalAs(UnmanagedType.I4)] int DispId
            );
    }

}
