using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace DS3SaveUnpacker
{
    static class GenBitConverter
    {
        /// <summary>
        /// Convert structure to bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToBytes<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr<T>(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        /// <summary>
        /// Convert bytes to structure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T ToStruct<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(buffer, 0, ptr, size);
            T obj = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

        /// <summary>
        /// Read a structure from a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static T ReadStruct<T>(this Stream fs) where T : struct
        {
            int size = Marshal.SizeOf<T>();

            byte[] buff = new byte[size];
            fs.Read(buff, 0, size);

            return ToStruct<T>(buff);
        }

        /// <summary>
        /// Read a structure from a BinaryReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bin"></param>
        /// <returns></returns>
        public static T ReadStruct<T>(this BinaryReader bin) where T : struct
        {
            int size = Marshal.SizeOf<T>();

            byte[] buff = new byte[size];
            bin.Read(buff, 0, size);

            return ToStruct<T>(buff);
        }

        /// <summary>
        /// Write a structure to a stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fs"></param>
        /// <param name="obj"></param>
        public static void WriteStruct<T>(this Stream fs, T obj) where T : struct
        {
            byte[] buff = ToBytes(obj);
            fs.Write(buff, 0, buff.Length);
        }

        /// <summary>
        /// Write a structure to a BinaryWriter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bin"></param>
        /// <param name="obj"></param>
        public static void WriteStruct<T>(this BinaryWriter bin, T obj) where T : struct
        {
            byte[] buff = ToBytes(obj);
            bin.Write(buff, 0, buff.Length);
        }

        public static byte[] hexToBytes(this string hex)
        {
            byte[] data = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i+=2)
            {
                data[i/2] = byte.Parse(hex.Substring(i,2), System.Globalization.NumberStyles.HexNumber);
            }
            return data;
        }
    }
}
