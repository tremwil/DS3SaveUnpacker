using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DS3SaveUnpacker
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
    struct BND4Header
    {
        /// <summary>
        /// BND version header. This should be the ANSI for "BND4", or 0x3444_4E42.
        /// </summary>
        public uint BNDVers;
        /// <summary>
        /// Don't know what's here, its 8 bytes (0x0001_0000_0000_0000).
        /// </summary>
        public ulong unknown1;
        /// <summary>
        /// Number of subfiles contained in the BND4 file.
        /// </summary>
        public uint fileCnt;
        /// <summary>
        /// Don't know what's here, its 8 bytes (0x0000_0000_0000_0040).
        /// </summary>
        public ulong unknown2;
        /// <summary>
        /// Signature. Should be 0x_3130_3030_3030_3030.
        /// </summary>
        public ulong signature;
        /// <summary>
        /// Size of the BND entry headers. Should be 32.
        /// </summary>
        public ulong entryHeaderSize;
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

        /// <summary>
        /// Create a BND4Header with given parameters.
        /// </summary>
        /// <param name="fileCnt"></param>
        /// <param name="dataOffset"></param>
        /// <param name="isUnicode"></param>
        public BND4Header(uint fileCnt, ulong dataOffset, bool isUnicode)
        {
            BNDVers = 0x3444_4E42;
            unknown1 = 0x0001_0000_0000_0000;
            unknown2 = 0x0000_0000_0000_0040;
            signature = 0x3130_3030_3030_3030;
            unknown3 = "200000000000000000000000000000".hexToBytes();
            entryHeaderSize = 0x20;

            this.fileCnt = fileCnt;
            this.dataOffset = dataOffset;
            this.isUnicode = isUnicode;
        }

        /// <summary>
        /// Throw an exception if this header does not make sense.
        /// </summary>
        public void AssertIntegrity()
        {
            Utils.Assert(BNDVers == 0x3444_4E42 && signature == 0x3130_3030_3030_3030, "Invalid BND4 header");
            Utils.Assert(entryHeaderSize == 0x20, "Unsupported entry header size");
        }
    }
}
