using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace DS3SaveUnpacker
{
    class BND4File
    {
        private const bool DEBUG = true;

        /// <summary>
        /// BND4 file header.
        /// </summary>
        public BND4Header header;
        /// <summary>
        /// The entries of the BND4 file.
        /// </summary>
        public BND4Entry[] entries;

        /// <summary>
        /// True if the encoding used for the save names is UTF-16.
        /// </summary>
        public bool isUnicode;

        /// <summary>
        /// Create empty, non-initialized BND4 file.
        /// </summary>
        public BND4File()
        {

        }

        /// <summary>
        /// Load a BND4 archive from a file.
        /// </summary>
        /// <param name="filePath"></param>
        public BND4File(string filePath)
        {
            LoadInPlace(filePath);
        }

        /// <summary>
        /// Recalculate the BND4 headers from entry data.
        /// </summary>
        public void CalculateHeaders()
        {
            if (DEBUG) Console.WriteLine("[DEBUG] RECALCULATE HEADERS");

            // 1st pass, do names
            uint cDataOffset = (uint)(Marshal.SizeOf<BND4Header>() + entries.Length * Marshal.SizeOf<BND4EntryHeader>());
            foreach (BND4Entry entry in entries)
            {
                entry.header = new BND4EntryHeader((ulong)entry.data.Length, 0, cDataOffset);
                cDataOffset += (isUnicode ? 2U : 1U) * (uint)(entry.name.Length + 1);
            }
            // Pad cDataOffset to a multiple of 16 bytes
            cDataOffset += (16U - cDataOffset % 16U) % 16U;
            // Fix main header
            header = new BND4Header((uint)entries.Length, cDataOffset, isUnicode);
            // 2nd pass, do file offsets
            foreach (BND4Entry entry in entries)
            {
                entry.header.entryDataOffset = cDataOffset;
                cDataOffset += (uint)entry.header.entrySize;
                // Pad to a multiple of 16 bytes
                cDataOffset += (16U - cDataOffset % 16U) % 16U;
            }
        }

        /// <summary>
        /// Load a BND4 file into the existing object, overwriting existing data.
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadInPlace(string filePath)
        {
            Utils.Assert(File.Exists(filePath), "File does not exist");

            using (BinReader bin = new BinReader(File.OpenRead(filePath)))
            {
                if (DEBUG) Console.WriteLine("[DEBUG] READ BND4 HEADERS");

                header = bin.ReadStruct<BND4Header>();
                header.AssertIntegrity();
                isUnicode = header.isUnicode;

                entries = new BND4Entry[header.fileCnt];

                for (int i = 0; i < header.fileCnt; i++)
                {
                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] READ ENTRY HEADER {0}/{1}", i + 1, header.fileCnt));

                    entries[i] = new BND4Entry();
                    entries[i].header = bin.ReadStruct<BND4EntryHeader>();
                    entries[i].header.AssertIntegrity();

                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] READ ENTRY NAME {0}/{1}", i + 1, header.fileCnt));

                    bin.StepInto(entries[i].header.entryNameOffset);
                    entries[i].name = header.isUnicode ? bin.ReadWideString() : bin.ReadShiftJIS();
                    bin.StepOut();

                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] READ ENTRY DATA {0}/{1}", i + 1, header.fileCnt));

                    bin.StepInto(entries[i].header.entryDataOffset);
                    entries[i].data = bin.ReadBytes((int)entries[i].header.entrySize);
                    bin.StepOut();
                }

                if (DEBUG) Console.WriteLine("[DEBUG] READ COMPLETE");
            }
        }

        /// <summary>
        /// Load a BND4 archive from a file and return a <see cref="BND4File"></see> object.
        /// </summary>
        /// <param name="filePath"></param>
        public static BND4File Load(string filePath)
        {
            var BND = new BND4File();
            BND.LoadInPlace(filePath);
            return BND;
        }

        /// <summary>
        /// Update BND4 headers and save the archive.
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath)
        {
            if (DEBUG) Console.WriteLine("[DEBUG] SAVING BND4 ARCHIVE...");

            CalculateHeaders();

            using (BinWriter bin = new BinWriter(File.OpenWrite(filePath)))
            {
                if (DEBUG) Console.WriteLine("[DEBUG] WRITE BND4 HEADERS");

                bin.WriteStruct(header);

                foreach (BND4Entry entry in entries)
                {
                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] WRITE ENTRY HEADER '{0}'", entry.name));

                    bin.WriteStruct(entry.header);

                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] WRITE ENTRY HEADER NAME '{0}'", entry.name));

                    bin.StepInto(entry.header.entryNameOffset);
                    if (header.isUnicode) { bin.WriteWideString(entry.name); }
                    else { bin.WriteShiftJIS(entry.name); }
                    bin.StepOut();

                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] WRITE ENTRY HEADER DATA '{0}'", entry.name));

                    bin.StepInto(entry.header.entryDataOffset);
                    bin.Write(entry.data);
                    bin.StepOut();
                }
            };

            if (DEBUG) Console.WriteLine("[DEBUG] SAVING COMPLETED");
        }

        public BND4Entry findEntryByName(string name)
        {
            foreach (BND4Entry entry in entries)
            {
                if (entry.name == name) { return entry; }
            }
            return null;
        }

        /// <summary>
        /// Unpack the archive files into a folder.
        /// </summary>
        public void Unpack(string folder = "", bool decrypt = true)
        {
            if (DEBUG) Console.WriteLine("[DEBUG] UNPACKING ENTRY DATA...");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (BND4Entry entry in entries)
            {
                if (decrypt)
                {
                    byte[] decrypted = entry.DecryptData();
                    File.WriteAllBytes(Path.Combine(folder, entry.name), decrypted);
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(folder, entry.name), entry.data);
                }
            }

            if (DEBUG) Console.WriteLine("[DEBUG] ENTRY DATA UNPACKED");
        }

        /// <summary>
        /// Unpack a BND4 file into a folder.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="folder"></param>
        /// <param name="decrypt"></param>
        public static void Unpack(string filePath, string folder, bool decrypt = true)
        {
            var BND = new BND4File(filePath);
            BND.Unpack(folder, decrypt);
        }

        /// <summary>
        /// Create a BND4File object from subfiles stored in a folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="encrypt"></param>
        /// <returns></returns>
        public static BND4File Pack(string folder, bool encrypt = true)
        {
            if (DEBUG) Console.WriteLine("[DEBUG] PACKING FOLDER INTO BND4 ARCHIVE...");

            Utils.Assert(Directory.Exists(folder), "Folder does not exist");

            string[] files = Directory.GetFiles(folder);
            Utils.Assert(files.Length != 0, "No subfiles to add to archive");

            var BND = new BND4File();
            BND.isUnicode = true;
            BND.entries = new BND4Entry[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                BND.entries[i].name = Path.GetFileName(files[i]);

                byte[] data = File.ReadAllBytes(files[i]);
                if (encrypt) { BND.entries[i].SetEncryptedData(data, false); }
                else { BND.entries[i].data = data; }
            }

            if (DEBUG) Console.WriteLine("[DEBUG] PACKING COMPLETED");

            return BND;
        }

        /// <summary>
        /// Pack a BND4 file from subfiles stored in a folder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filePath"></param>
        /// <param name="encrypt"></param>
        public static void Pack(string folder, string filePath, bool encrypt = true)
        {
            var BND = Pack(folder, encrypt);
            BND.Save(filePath);
        }

        /// <summary>
        /// Patch the DS3 account flag that controls if a save can be opened by a user.
        /// </summary>
        /// <param name="steamID"></param>
        public void PatchDS3AccountFlag(ulong steamID)
        {
            if (DEBUG) Console.WriteLine("[DEBUG] PATCHING ACCOUNT FLAG...");

            uint accNum = (uint)(steamID & 0xffff_ffffUL);
            BND4Entry data10 = findEntryByName("USER_DATA010");

            byte[] buff = data10.DecryptData();
            Array.Copy(BitConverter.GetBytes(accNum), 0, buff, 0x8, 4);
            data10.SetEncryptedData(buff);

            if (DEBUG) Console.WriteLine("[DEBUG] ACCOUNT FLAG PATCHED");
        }
    }
}
