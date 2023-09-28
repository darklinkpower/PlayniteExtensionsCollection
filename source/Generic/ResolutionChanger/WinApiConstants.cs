using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper
{
    public static class WinApiConstants
    {
        // Display methods
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;
        public const int DISP_CHANGE_BADMODE = -2;
        public const int DISP_CHANGE_NOTUPDATED = -3;
        public const int DISP_CHANGE_BADFLAGS = -4;
        public const int DISP_CHANGE_BADPARAM = -5;

        // DevMode DmFields
        public const int DM_ORIENTATION = 0x00000001;
        public const int DM_PAPERSIZE = 0x00000002;
        public const int DM_PAPERLENGTH = 0x00000004;
        public const int DM_PAPERWIDTH = 0x00000008;
        public const int DM_SCALE = 0x00000010;
        public const int DM_POSITION = 0x00000020;
        public const int DM_NUP = 0x00000040;
        public const int DM_DISPLAYORIENTATION = 0x00000080;
        public const int DM_COPIES = 0x00000100;
        public const int DM_DEFAULTSOURCE = 0x00000200;
        public const int DM_PRINTQUALITY = 0x00000400;
        public const int DM_COLOR = 0x00000800;
        public const int DM_DUPLEX = 0x00001000;
        public const int DM_YRESOLUTION = 0x00002000;
        public const int DM_TTOPTION = 0x00004000;
        public const int DM_COLLATE = 0x00008000;
        public const int DM_FORMNAME = 0x00010000;
        public const int DM_LOGPIXELS = 0x00020000;
        public const int DM_BITSPERPEL = 0x00040000;
        public const int DM_PELSWIDTH = 0x00080000;
        public const int DM_PELSHEIGHT = 0x00100000;
        public const int DM_DISPLAYFLAGS = 0x00200000;
        public const int DM_DISPLAYFREQUENCY = 0x00400000;
        public const int DM_ICMMETHOD = 0x00800000;
        public const int DM_ICMINTENT = 0x01000000;
        public const int DM_MEDIATYPE = 0x02000000;
        public const int DM_DITHERTYPE = 0x04000000;
        public const int DM_PANNINGWIDTH = 0x08000000;
        public const int DM_PANNINGHEIGHT = 0x10000000;
    }
}
