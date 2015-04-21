using System;
using System.Collections.Generic;
using System.Text;

namespace onlyconnect
{
    class constants
    {
        //DISPID_AMBIENT_DLCONTROL constants
        public const uint DLCTL_DLIMAGES = 0x00000010;
        public const uint DLCTL_VIDEOS = 0x00000020;
        public const uint DLCTL_BGSOUNDS = 0x00000040;
        public const uint DLCTL_NO_SCRIPTS = 0x00000080;
        public const uint DLCTL_NO_JAVA = 0x00000100;
        public const uint DLCTL_NO_RUNACTIVEXCTLS = 0x00000200;
        public const uint DLCTL_NO_DLACTIVEXCTLS = 0x00000400;
        public const uint DLCTL_DOWNLOADONLY = 0x00000800;
        public const uint DLCTL_NO_FRAMEDOWNLOAD = 0x00001000;
        public const uint DLCTL_RESYNCHRONIZE = 0x00002000;
        public const uint DLCTL_PRAGMA_NO_CACHE = 0x00004000;
        public const uint DLCTL_NO_BEHAVIORS = 0x00008000;
        public const uint DLCTL_NO_METACHARSET = 0x00010000;
        public const uint DLCTL_URL_ENCODING_DISABLE_UTF8 = 0x00020000;
        public const uint DLCTL_URL_ENCODING_ENABLE_UTF8 = 0x00040000;
        public const uint DLCTL_NOFRAMES = 0x00080000;
        public const uint DLCTL_FORCEOFFLINE = 0x10000000;
        public const uint DLCTL_NO_CLIENTPULL = 0x20000000;
        public const uint DLCTL_SILENT = 0x40000000;
        public const uint DLCTL_OFFLINEIFNOTCONNECTED = 0x80000000;
        public const uint DLCTL_OFFLINE = 0x80000000;

        public const int OLECLOSE_NOSAVE = 1;

        public const int STGM_READ = 0x00000000;
        public const int STGM_WRITE = 0x00000001;
        public const int STGM_READWRITE = 0x00000002;
    }


}
