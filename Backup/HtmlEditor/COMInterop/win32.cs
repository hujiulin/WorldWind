using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.CustomMarshalers;

namespace onlyconnect
{
    class win32
    {
        //win32 functions

        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;
        public const int GW_CHILD = 5;

public const int  GWL_STYLE  =         -16;
        public const int GWL_EXSTYLE = -20;

        public const int WM_SETFOCUS = 0x7;
        public const int WM_MOUSEACTIVATE = 0x21;
        public const int WM_PARENTNOTIFY = 0x210;
        public const int WM_ACTIVATE = 0x6;
        public const int WM_KILLFOCUS = 0x8;
        public const int WM_CLOSE = 0x10;
        public const int WM_DESTROY = 0x2;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYLAST = 0x0109;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_NEXTDLGCTL = 0x0028; // see also GetNextDlgTabItem
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_MOUSELEAVE = 0x02A3;
        public const int WM_MOUSEHOVER = 0x02A1;

        public const int WS_TABSTOP  =  0x00010000;
        public const int MK_LBUTTON = 0x0001;
        public const int MK_RBUTTON = 0x0002;
        public const int MK_SHIFT = 0x0004;
        public const int MK_CONTROL = 0x0008;
        public const int MK_MBUTTON = 0x0010;
        public const int MK_XBUTTON1 = 0x0020;
        public const int MK_XBUTTON2 = 0x0040;

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern Boolean GetClientRect(IntPtr hWnd, [In, Out] RECT rect);

        [DllImport("User32.dll")]
        public static extern int GetMessageTime();

        [DllImport("User32.dll")]
        public static extern int GetMessagePos();

        [DllImport("User32.dll")]
        public static extern int GetWindowLong([In] IntPtr hWnd, [In] int nIndex);

        [DllImport("User32.dll")]
        public static extern IntPtr GetTopWindow([In] IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern Boolean IsWindowVisible([In] IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern Boolean IsWindowEnabled([In] IntPtr hWnd);

        [DllImport("ole32.dll", PreserveSig = false)]
        public static extern void CreateStreamOnHGlobal([In] IntPtr hGlobal,
            [In] int fDeleteOnRelease, [Out] out IStream pStream);

        [DllImport("ole32.dll", PreserveSig = false)]
        public static extern void GetHGlobalFromStream(IStream pStream, [Out] out IntPtr pHGlobal);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool GlobalUnlock(IntPtr handle);


        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int CreateBindCtx(int dwReserved, [Out] out IBindCtx ppbc);

        [DllImport("urlmon.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int CreateURLMoniker(IMoniker pmkContext, String szURL, [Out]
			out IMoniker ppmk);

        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int OleRun(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int OleLockRunning(
        [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown,
        [In, MarshalAs(UnmanagedType.Bool)] bool flock,
        [In, MarshalAs(UnmanagedType.Bool)] bool fLastUnlockCloses
        );

        [DllImport("user32.dll")]
        public static extern int SendMessage(
            IntPtr hWnd,      // handle to destination window
            uint Msg,     // message
            int wParam,  // first message parameter
            int lParam   // second message parameter
            );

        [DllImport("user32.Dll")]
        public static extern IntPtr PostMessage(
            IntPtr hWnd,
            int msg,
            int wParam,
            int lParam);

        [DllImport("user32.Dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.Dll")]
        public static extern IntPtr GetWindow([In] IntPtr hWnd, [In] uint wCmd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

    }

    public enum tagOLECLOSE
    {
        OLECLOSE_SAVEIFDIRTY = 0,
        OLECLOSE_NOSAVE = 1,
        OLECLOSE_PROMPTSAVE = 2
    }

    public struct HRESULT
    {
        public const int S_OK = 0;
        public const int S_FALSE = 1;
        public const int E_NOTIMPL = unchecked((int)0x80004001);
        public const int E_INVALIDARG = unchecked((int)0x80070057);
        public const int E_NOINTERFACE = unchecked((int)0x80004002);
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
    }

    struct DOCHOSTUIDBLCLICK
    {
        public const int DEFAULT = 0x0;
        public const int SHOWPROPERTIES = 0x1;
        public const int SHOWCODE = 0x2;
    }

    public enum DOCHOSTUIFLAG
    {
        DIALOG = 0x00000001,
        DISABLE_HELP_MENU = 0x00000002,
        NO3DBORDER = 0x00000004,
        SCROLL_NO = 0x00000008,
        DISABLE_SCRIPT_INACTIVE = 0x00000010,
        OPENNEWWIN = 0x00000020,
        DISABLE_OFFSCREEN = 0x00000040,
        FLAT_SCROLLBAR = 0x00000080,
        DIV_BLOCKDEFAULT = 0x00000100,
        ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
        OVERRIDEBEHAVIORFACTORY = 0x00000400,
        CODEPAGELINKEDFONTS = 0x00000800,
        URL_ENCODING_DISABLE_UTF8 = 0x00001000,
        URL_ENCODING_ENABLE_UTF8 = 0x00002000,
        ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
        ENABLE_INPLACE_NAVIGATION = 0x00010000,
        IME_ENABLE_RECONVERSION = 0x00020000,
        THEME = 0x00040000,
        NOTHEME = 0x00080000,
        NOPICS = 0x00100000,
        NO3DOUTERBORDER = 0x00200000,
        //DELEGATESIDOFDISPATCH = 0x00400000,
        DISABLE_EDIT_NS_FIXUP = 0x00400000,
        LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
        DISABLE_UNTRUSTEDPROTOCOL = 0x01000000
    }

    public enum ELEMENT_TAG_ID
    {	TAGID_NULL	= 0,
	TAGID_UNKNOWN	= 1,
	TAGID_A	= 2,
	TAGID_ACRONYM	= 3,
	TAGID_ADDRESS	= 4,
	TAGID_APPLET	= 5,
	TAGID_AREA	= 6,
	TAGID_B	= 7,
	TAGID_BASE	= 8,
	TAGID_BASEFONT	= 9,
	TAGID_BDO	= 10,
	TAGID_BGSOUND	= 11,
	TAGID_BIG	= 12,
	TAGID_BLINK	= 13,
	TAGID_BLOCKQUOTE	= 14,
	TAGID_BODY	= 15,
	TAGID_BR	= 16,
	TAGID_BUTTON	= 17,
	TAGID_CAPTION	= 18,
	TAGID_CENTER	= 19,
	TAGID_CITE	= 20,
	TAGID_CODE	= 21,
	TAGID_COL	= 22,
	TAGID_COLGROUP	= 23,
	TAGID_COMMENT	= 24,
	TAGID_COMMENT_RAW	= 25,
	TAGID_DD	= 26,
	TAGID_DEL	= 27,
	TAGID_DFN	= 28,
	TAGID_DIR	= 29,
	TAGID_DIV	= 30,
	TAGID_DL	= 31,
	TAGID_DT	= 32,
	TAGID_EM	= 33,
	TAGID_EMBED	= 34,
	TAGID_FIELDSET	= 35,
	TAGID_FONT	= 36,
	TAGID_FORM	= 37,
	TAGID_FRAME	= 38,
	TAGID_FRAMESET	= 39,
	TAGID_GENERIC	= 40,
	TAGID_H1	= 41,
	TAGID_H2	= 42,
	TAGID_H3	= 43,
	TAGID_H4	= 44,
	TAGID_H5	= 45,
	TAGID_H6	= 46,
	TAGID_HEAD	= 47,
	TAGID_HR	= 48,
	TAGID_HTML	= 49,
	TAGID_I	= 50,
	TAGID_IFRAME	= 51,
	TAGID_IMG	= 52,
	TAGID_INPUT	= 53,
	TAGID_INS	= 54,
	TAGID_KBD	= 55,
	TAGID_LABEL	= 56,
	TAGID_LEGEND	= 57,
	TAGID_LI	= 58,
	TAGID_LINK	= 59,
	TAGID_LISTING	= 60,
	TAGID_MAP	= 61,
	TAGID_MARQUEE	= 62,
	TAGID_MENU	= 63,
	TAGID_META	= 64,
	TAGID_NEXTID	= 65,
	TAGID_NOBR	= 66,
	TAGID_NOEMBED	= 67,
	TAGID_NOFRAMES	= 68,
	TAGID_NOSCRIPT	= 69,
	TAGID_OBJECT	= 70,
	TAGID_OL	= 71,
	TAGID_OPTION	= 72,
	TAGID_P	= 73,
	TAGID_PARAM	= 74,
	TAGID_PLAINTEXT	= 75,
	TAGID_PRE	= 76,
	TAGID_Q	= 77,
	TAGID_RP	= 78,
	TAGID_RT	= 79,
	TAGID_RUBY	= 80,
	TAGID_S	= 81,
	TAGID_SAMP	= 82,
	TAGID_SCRIPT	= 83,
	TAGID_SELECT	= 84,
	TAGID_SMALL	= 85,
	TAGID_SPAN	= 86,
	TAGID_STRIKE	= 87,
	TAGID_STRONG	= 88,
	TAGID_STYLE	= 89,
	TAGID_SUB	= 90,
	TAGID_SUP	= 91,
	TAGID_TABLE	= 92,
	TAGID_TBODY	= 93,
	TAGID_TC	= 94,
	TAGID_TD	= 95,
	TAGID_TEXTAREA	= 96,
	TAGID_TFOOT	= 97,
	TAGID_TH	= 98,
	TAGID_THEAD	= 99,
	TAGID_TITLE	= 100,
	TAGID_TR	= 101,
	TAGID_TT	= 102,
	TAGID_U	= 103,
	TAGID_UL	= 104,
	TAGID_VAR	= 105,
	TAGID_WBR	= 106,
	TAGID_XMP	= 107,
	TAGID_ROOT	= 108,
	TAGID_OPTGROUP	= 109,
	TAGID_COUNT	= 110,
	TAGID_LAST_PREDEFINED	= 10000,
	ELEMENT_TAG_ID_Max	= 2147483647
    } 	
    
    public enum SELECTION_TYPE
    {
        SELECTION_TYPE_None = 0,
        SELECTION_TYPE_Caret = 1,
        SELECTION_TYPE_Text = 2,
        SELECTION_TYPE_Control = 3,
        SELECTION_TYPE_Max = 2147483647
    }

    public enum CARET_DIRECTION
    {
        CARET_DIRECTION_INDETERMINATE = 0,
        CARET_DIRECTION_SAME = 1,
        CARET_DIRECTION_BACKWARD = 2,
        CARET_DIRECTION_FORWARD = 3,
        CARET_DIRECTION_Max = 2147483647
    }

    public enum COORD_SYSTEM
    {
        COORD_SYSTEM_GLOBAL = 0,
        COORD_SYSTEM_PARENT = 1,
        COORD_SYSTEM_CONTAINER = 2,
        COORD_SYSTEM_CONTENT = 3,
        COORD_SYSTEM_FRAME = 4,
        COORD_SYSTEM_Max = 2147483647
    }

    public enum ELEMENT_CORNER
    {
        ELEMENT_CORNER_NONE = 0,
        ELEMENT_CORNER_TOP = 1,
        ELEMENT_CORNER_LEFT = 2,
        ELEMENT_CORNER_BOTTOM = 3,
        ELEMENT_CORNER_RIGHT = 4,
        ELEMENT_CORNER_TOPLEFT = 5,
        ELEMENT_CORNER_TOPRIGHT = 6,
        ELEMENT_CORNER_BOTTOMLEFT = 7,
        ELEMENT_CORNER_BOTTOMRIGHT = 8,
        ELEMENT_CORNER_Max = 2147483647
    }


    public enum OLECMDF : int
    {
        OLECMDF_SUPPORTED = 1,
        OLECMDF_ENABLED = 2,
        OLECMDF_LATCHED = 4,
        OLECMDF_NINCHED = 8
    }

    struct WM
    {
        public const int KEYFIRST = 0x0100;
        public const int KEYLAST = 0x0108;
        public const int KEYDOWN = 0x0100;
        public const int KEYUP = 0x0101;
    }

    struct OLEIVERB
    {
        public const int PRIMARY = 0;
        public const int SHOW = -1;
        public const int OPEN = -2;
        public const int HIDE = -3;
        public const int UIACTIVATE = -4;
        public const int INPLACEACTIVATE = -5;
        public const int DISCARDUNDOSTATE = -6;
        public const int PROPERTIES = -7;
    }


    [ComVisible(true), StructLayout(LayoutKind.Sequential)]
    public class DOCHOSTUIINFO
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize = 0;
        [MarshalAs(UnmanagedType.I4)]
        public int dwFlags = 0;
        [MarshalAs(UnmanagedType.I4)]
        public int dwDoubleClick = 0;
        [MarshalAs(UnmanagedType.I4)]
        public int dwReserved1 = 0;
        [MarshalAs(UnmanagedType.I4)]
        public int dwReserved2 = 0;
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public class MSG
    {
        public IntPtr hwnd = IntPtr.Zero;
        public int message = 0;
        public IntPtr wParam = IntPtr.Zero;
        public IntPtr lParam = IntPtr.Zero;
        public int time = 0;
        public int pt_x = 0;
        public int pt_y = 0;
    }

    [ComVisible(true), StructLayout(LayoutKind.Sequential)]
    public class RECT
    {
        public int left = 0;
        public int top = 0;
        public int right = 0;
        public int bottom = 0;
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public class tagOIFI
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.I4)]
        public int fMDIApp;
        public IntPtr hwndFrame;
        public IntPtr hAccel;
        [MarshalAs(UnmanagedType.U4)]
        public int cAccelEntries;
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public class tagOleMenuGroupWidths
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public int[] widths = new int[6];
    }


    [ComVisible(true), StructLayout(LayoutKind.Sequential)]
    public struct OLECMD
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cmdID;
        [MarshalAs(UnmanagedType.U4)]
        public int cmdf;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OLECMDTEXT
    {
        public UInt32 cmdtextf;
        public UInt32 cwActual;
        public UInt32 cwBuf;
        public Char rgwz;
    }

    [ComVisible(true), StructLayout(LayoutKind.Sequential)]
    public struct win32POINT
    {
        public int x;
        public int y;
    }

 }
