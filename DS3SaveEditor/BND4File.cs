using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace DS3SaveEditor
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

        public BND4File(string filePath)
        {
            using (BinReader bin = new BinReader(File.OpenRead(filePath)))
            {
                if (DEBUG) Console.WriteLine("[DEBUG] READ BND4 HEADERS");

                header = bin.ReadStruct<BND4Header>();
                isUnicode = header.isUnicode;

                entries = new BND4Entry[header.fileCnt];

                for (int i = 0; i < header.fileCnt; i++)
                {
                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] READ ENTRY HEADER {0}/{1}", i+1, header.fileCnt));

                    entries[i] = new BND4Entry();
                    entries[i].header = bin.ReadStruct<BND4EntryHeader>();

                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] READ ENTRY NAME {0}/{1}", i+1, header.fileCnt));

                    bin.StepInto(entries[i].header.entryNameOffset);
                    entries[i].name = header.isUnicode ? bin.ReadWideString() : bin.ReadShiftJIS();
                    bin.StepOut();

                    if (DEBUG) Console.WriteLine(string.Format("[DEBUG] READ ENTRY DATA {0}/{1}", i+1, header.fileCnt));

                    bin.StepInto(entries[i].header.entryDataOffset);
                    entries[i].data = bin.ReadBytes((int)entries[i].header.entrySize);
                    bin.StepOut();
                }

                if (DEBUG) Console.WriteLine("[DEBUG] READ COMPLETE");
            }
        }

        /// <summary>
        /// Recalculate the offset and sizes present in 
        /// </summary>
        public void FixHeaders()
        {
            if (DEBUG) Console.WriteLine("[DEBUG] RECALCULATING HEADERS");

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

            if (DEBUG) Console.WriteLine("[DEBUG] HEADERS RECALCULATED");
        }

        /// <summary>
        /// Update BND4 headers and save the archive.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            if (DEBUG) Console.WriteLine("[DEBUG] SAVING");

            FixHeaders();

            using (BinWriter bin = new BinWriter(File.OpenWrite(path)))
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
        /// Dump unencrypted entry data to files.
        /// </summary>
        public void DumpEntries(bool decrypt = false, string folder = "")
        {
            if (DEBUG) Console.WriteLine("[DEBUG] DUMPING ENTRY DATA");

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
        }

        /// <summary>
        /// Patch the account flag that controls if a save can be opened by a user.
        /// </summary>
        /// <param name="steamID"></param>
        public void PatchAccountFlag(ulong steamID)
        {
            if (DEBUG) Console.WriteLine("[DEBUG] PATCHING ACCOUNT FLAG");

            uint accNum = (uint)(steamID & 0xffff_ffffUL);
            BND4Entry data10 = findEntryByName("USER_DATA010");

            byte[] buff = data10.DecryptData();
            Array.Copy(BitConverter.GetBytes(accNum), 0, buff, 0x8, 4);
            data10.SetEncryptedData(buff);

            if (DEBUG) Console.WriteLine("[DEBUG] ACCOUNT FLAG PATCHED");
        }
    }
}
