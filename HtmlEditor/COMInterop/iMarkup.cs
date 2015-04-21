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

    [ComImport, ComVisible(true), Guid("3050f64a-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHTMLChangeSink
    {
        void Notify();
    }

    [ComImport, ComVisible(true), Guid("3050F649-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IHTMLChangeLog
    {
        void GetNextChange();
    }

    [ComImport, ComVisible(true), Guid("3050F5F9-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMarkupContainer
    {
    }

    [ComImport, ComVisible(true), Guid("3050F648-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMarkupContainer2
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int owningDoc([Out, MarshalAs(UnmanagedType.Interface)] IHTMLDocument2 doc);
        
        
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int createChangeLog(
        [In, MarshalAs(UnmanagedType.Interface)]
        IHTMLChangeSink pChangeSink,
        [Out, MarshalAs(UnmanagedType.Interface)]
        out IHTMLChangeLog ppChangeLog,
        [In, MarshalAs(UnmanagedType.Bool)]
        bool fForward,
        [In, MarshalAs(UnmanagedType.Bool)]
        bool fBackward);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int RegisterForDirtyRange(
        [In, MarshalAs(UnmanagedType.Interface)]
        IHTMLChangeSink pChangeSink,
        [Out, MarshalAs(UnmanagedType.U4)]
        out uint pdwCookie);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int UnRegisterForDirtyRange([In] uint pdwCookie);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetAndClearDirtyRange([In] uint dwCookie, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIPointerBegin,
        [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIPointerEnd);

        long GetVersionNumber();

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int GetMasterElement([Out, MarshalAs(UnmanagedType.Interface)] IHTMLElement el);
    }
         
    [ComVisible(true), Guid("3050f49f-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMarkupPointer
    {
        //		[] HRESULT OwningDoc([out] IHTMLDocument2** ppDoc);
        //		[] HRESULT Gravity([out] POINTER_GRAVITY* pGravity);
        //		[] HRESULT SetGravity([in] POINTER_GRAVITY Gravity);
        //		[] HRESULT Cling([out] BOOL* pfCling);
        //		[] HRESULT SetCling([in] BOOL fCLing);
        //		[] HRESULT Unposition();
        //		[] HRESULT IsPositioned([out] BOOL* pfPositioned);
        //		[] HRESULT GetContainer([out] IMarkupContainer** ppContainer);
        //		[] HRESULT MoveAdjacentToElement([in] IHTMLElement* pElement,[in] ELEMENT_ADJACENCY eAdj);
        //		[] HRESULT MoveToPointer([in] IMarkupPointer* pPointer);
        //		[] HRESULT MoveToContainer([in] IMarkupContainer* pContainer,[in] BOOL fAtStart);
        //		[] HRESULT Left([in] BOOL fMove,[out] MARKUP_CONTEXT_TYPE* pContext,[out] IHTMLElement** ppElement,[in, out] long* pcch,[out] OLECHAR* pchText);
        //		[] HRESULT Right([in] BOOL fMove,[out] MARKUP_CONTEXT_TYPE* pContext,[out] IHTMLElement** ppElement,[in, out] long* pcch,[out] OLECHAR* pchText);
        //		[] HRESULT CurrentScope([out] IHTMLElement** ppElemCurrent);
        //		[] HRESULT IsLeftOf([in] IMarkupPointer* pPointerThat,[out] BOOL* pfResult);
        //		[] HRESULT IsLeftOfOrEqualTo([in] IMarkupPointer* pPointerThat,[out] BOOL* pfResult);
        //		[] HRESULT IsRightOf([in] IMarkupPointer* pPointerThat,[out] BOOL* pfResult);
        //		[] HRESULT IsRightOfOrEqualTo([in] IMarkupPointer* pPointerThat,[out] BOOL* pfResult);
        //		[] HRESULT IsEqualTo([in] IMarkupPointer* pPointerThat,[out] BOOL* pfAreEqual);
        //		[] HRESULT MoveUnit([in] MOVEUNIT_ACTION muAction);
        //		[] HRESULT FindText([in] OLECHAR* pchFindText,[in] DWORD dwFlags,[in] IMarkupPointer* pIEndMatch,[in] IMarkupPointer* pIEndSearch);
    }

    


    [ComImport, ComVisible(true), Guid(
"3050f4a0-98b5-11cf-bb82-00aa00bdce0b"), InterfaceTypeAttribute(
ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMarkupServices
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        HRESULT CreateMarkupPointer(
            [Out, MarshalAs(UnmanagedType.Interface)]
			out IMarkupPointer pointer);

        void PlaceHolder_CreateMarkupContainer();
        void PlaceHolder_CreateElement();
        void PlaceHolder_CloneElement();
        void PlaceHolder_InsertElement();
        void PlaceHolder_RemoveElement();
        void PlaceHolder_Remove();
        void PlaceHolder_Copy();
        void PlaceHolder_Move();
        void PlaceHolder_InsertText();
        void PlaceHolder_ParseString();
        void PlaceHolder_ParseGlobal();
        void PlaceHolder_IsScopedElement();
        void PlaceHolder_GetElementTagId();
        void PlaceHolder_GetTagIDForName();
        void PlaceHolder_GetNameForTagID();

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        HRESULT MovePointersToRange(
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLTxtRange IRange,
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pointerStart,
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pointerFinish);

        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        HRESULT MoveRangeToPointers(
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pointerStart,
            [In, MarshalAs(UnmanagedType.Interface)]
			IMarkupPointer pointerFinish,
            [In, MarshalAs(UnmanagedType.Interface)]
			IHTMLTxtRange IRange);

        void PlaceHolder_BeginUndoUnit();
        void PlaceHolder_EndUndoUnit();
    }

    [ComVisible(true), Guid("3050f682-98b5-11cf-bb82-00aa00bdce0b"),
InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMarkupServices2
    {
        void CreateMarkupPointer([Out] [MarshalAs(UnmanagedType.Interface)] out IMarkupPointer ppPointer);
        void CreateMarkupContainer([Out] [MarshalAs(UnmanagedType.Interface)] IMarkupContainer ppMarkupContainer);
        void CreateElement([In] ELEMENT_TAG_ID tagID,
        [In] [MarshalAs(UnmanagedType.LPWStr)] string pchAttributes,
        [Out] [MarshalAs(UnmanagedType.Interface)] IHTMLElement ppElement);
        void CloneElement([In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pElemCloneThis,
        [Out] [MarshalAs(UnmanagedType.Interface)] IHTMLElement ppElementTheClone);
        void InsertElement([In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pElementInsert,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        void RemoveElement([In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pElementRemove);
        void Remove([In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        void Copy([In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceFinish,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget);
        void Move([In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerSourceFinish,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget);
        void InsertText([In] [MarshalAs(UnmanagedType.LPWStr)] string pchText,
        [In] [MarshalAs(UnmanagedType.I8)] int cch, //This was LONG.
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerTarget);
        void ParseString([In] [MarshalAs(UnmanagedType.LPWStr)] string pchHTML,
        [In] uint dwFlags,
        [Out] [MarshalAs(UnmanagedType.Interface)] IMarkupContainer ppContainerResult,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer ppPointerStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer ppPointerFinish);
        void ParseGlobal([In] Int32 hglobalHTML,
        [In] uint dwFlags,
        [Out] [MarshalAs(UnmanagedType.Interface)] IMarkupContainer ppContainerResult,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        void IsScopedElement([In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement,
        [Out] bool pfScoped);
        void GetElementTagId([In] [MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement,
        [Out] ELEMENT_TAG_ID ptagId);
        void GetTagIDForName([In] [MarshalAs(UnmanagedType.BStr)] string bstrName,
        [Out] ELEMENT_TAG_ID ptagId);
        void GetNameForTagID([In] ELEMENT_TAG_ID tagId,
        [Out] [MarshalAs(UnmanagedType.BStr)] string pbstrName);
        void MovePointersToRange([In] [MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange pIRange,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish);
        void MoveRangeToPointers([In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerStart,
        [In] [MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerFinish,
        [In] [MarshalAs(UnmanagedType.Interface)] IHTMLTxtRange pIRange);
        void BeginUndoUnit([In] [MarshalAs(UnmanagedType.LPWStr)] string pchTitle);
        void EndUndoUnit();

        void ParseGlobalEx([In] object hglobalHTML,
        [In] int dwFlags,
        [In] IMarkupContainer pContext,
        [Out] out IMarkupContainer ppContainerResult,
        [In] IMarkupPointer pPointerStart,
        [In] IMarkupPointer pPointerFinish);

        void ValidateElements([In] IMarkupPointer pPointerStart,
        [In] IMarkupPointer pPointerFinish,
        [In] IMarkupPointer pPointerTarget,
        [Out][In] ref IMarkupPointer pPointerStatus,
        [Out] out IHTMLElement ppElemFailBottom,
        [Out] out IHTMLElement ppElemFailTop);

        void SaveSegmentsToClipboard([In] object pSegmentList, //ISegmentList not implimented yet.
        [In] int dwFlags);

    }


}
