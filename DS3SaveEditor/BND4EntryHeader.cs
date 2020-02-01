using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DS3SaveEditor
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

        public BND4EntryHeader(ulong entrySize, uint entryDataOffset, uint entryNameOffset)
        {
            padding = 0xFFFFFFFF00000050UL;
            unused = 0;

            this.entrySize = entrySize;
            this.entryDataOffset = entryDataOffset;
            this.entryNameOffset = entryNameOffset;
        }
    }
}
