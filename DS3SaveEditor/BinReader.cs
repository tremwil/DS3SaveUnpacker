using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS3SaveEditor
{
    class BinReader: BinaryReader
    {
        /// <summary>
        /// LIFO stack to keep track of the positions across StepInto and StepOut calls
        /// </summary>
        protected Stack<long> positions;

        public BinReader(Stream stream) : base(stream) 
        {
            positions = new Stack<long>();
        }

        /// <summary>
        /// Step into an offset. Call <see cref="StepOut"/> to come back to the last position.
        /// </summary>
        /// <param name="offset"></param>
        public void StepInto(long offset)
        {
            positions.Push(this.BaseStream.Position);
            this.BaseStream.Position = offset;
        }

        /// <summary>
        /// Step out and back to the position before the last <see cref="StepInto"/> call.
        /// </summary>
        /// <param name="offset"></param>
        public void StepOut()
        {
            this.BaseStream.Position = positions.Pop();
        }
        
        /// <summary>
        /// Read a 2-byte wide null-terminated string.
        /// </summary>
        /// <returns></returns>
        public string ReadWideString()
        {
            StringBuilder sb = new StringBuilder();
            ushort chr = ReadUInt16();
            while (chr != 0)
            {
                sb.Append(char.ConvertFromUtf32(chr));
                chr = ReadUInt16();
            }
            return sb.ToString();
        }
    }

    class BinWriter: BinaryWriter
    {
        /// <summary>
        /// LIFO stack to keep track of the positions across StepInto and StepOut calls
        /// </summary>
        protected Stack<long> positions;

        public BinWriter(Stream stream) : base(stream)
        {
            positions = new Stack<long>();
        }

        /// <summary>
        /// Step into an offset. Call <see cref="StepOut"/> to come back to the last position.
        /// </summary>
        /// <param name="offset"></param>
        public void StepInto(long offset)
        {
            positions.Push(this.BaseStream.Position);
            this.BaseStream.Position = offset;
        }

        /// <summary>
        /// Step out and back to the position before the last <see cref="StepInto"/> call.
        /// </summary>
        /// <param name="offset"></param>
        public void StepOut()
        {
            this.BaseStream.Position = positions.Pop();
        }

        /// <summary>
        /// Write a 2-byte wide null-terminated string.
        /// </summary>
        /// <returns></returns>
        public void WriteWideString(string s)
        {
            foreach (char chr in s)
            {
                Write((ushort)chr);
            }
            Write((ushort)0);
        }
    }
}
