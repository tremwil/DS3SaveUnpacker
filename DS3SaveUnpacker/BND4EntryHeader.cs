using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DS3SaveUnpacker
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
    struct BND4EntryHeader
    {
        /// <summary>
        /// Padding (50 00 00 00 FF FF FF FF in little-endian)
        /// </summary>
        public ulong padding;
        /// <summary>
        /// Size (in bytes) of the entry data.
        /// </summary>
        public ulong entrySize;
        /// <summary>
        /// Offset of the entry data in the BND4 file.
        /// </summary>
        public uint entryDataOffset;
        /// <summary>
        /// Offset of the entry name in the BND4 file.
        /// </summary>
        public uint entryNameOffset;
        /// <summary>
        /// Unused. Should be 0
        /// </summary>
        public ulong unused;

        /// <summary>
        /// Create a BND4 entry header with given parameters.
        /// </summary>
        /// <param name="entrySize"></param>
        /// <param name="entryDataOffset"></param>
        /// <param name="entryNameOffset"></param>
        public BND4EntryHeader(ulong entrySize, uint entryDataOffset, uint entryNameOffset)
        {
            padding = 0xFFFFFFFF00000050UL;
            unused = 0;

            this.entrySize = entrySize;
            this.entryDataOffset = entryDataOffset;
            this.entryNameOffset = entryNameOffset;
        }

        /// <summary>
        /// Throw an exception if this header does not make sense.
        /// </summary>
        public void AssertIntegrity()
        {
            Utils.Assert(padding == 0xFFFFFFFF00000050UL, "Invalid BND4 entry header");
        }
    }
}
