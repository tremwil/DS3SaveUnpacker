using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DS3SaveEditor
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
    struct BND4Header
    {
        /// <summary>
        /// BND version header. This should be the ANSI for "BND4".
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] BNDVers;
        /// <summary>
        /// Don't know what's here, its 8 bytes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] unknown1;
        /// <summary>
        /// Number of subfiles contained in the BND4 file.
        /// </summary>
        public uint fileCnt;
        /// <summary>
        /// Don't know what's here, its 8 bytes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] unknown2;
        /// <summary>
        /// Signature. Should be 0x 3130 3030 3030 3030
        /// </summary>
        public ulong signature;
        /// <summary>
        /// Size of the BND entry headers. Should be 32.
        /// </summary>
        public ulong entrySize;
        /// <summary>
        /// Position in the file at which entry data begins.
        /// </summary>
        public ulong dataOffset;
        /// <summary>
        /// If the entry names are Unicode wide chars
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool isUnicode;
        /// <summary>
        /// I don't know what's here
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] unknown3;
    }
}
