using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace DS3SaveEditor
{
    class BND4File
    {
        /// <summary>
        /// BND4 file header.
        /// </summary>
        public BND4Header header;
        /// <summary>
        /// The entries of the BND4 file.
        /// </summary>
        public BND4Entry[] entries;

        public BND4File(string filePath)
        {
            using (BinReader bin = new BinReader(File.OpenRead(filePath)))
            {
                header = bin.ReadStruct<BND4Header>();
                entries = new BND4Entry[header.fileCnt];

                for (int i = 0; i < header.fileCnt; i++)
                {
                    entries[i] = new BND4Entry();
                    entries[i].header = bin.ReadStruct<BND4EntryHeader32>();

                    bin.StepInto(entries[i].header.entryNameOffset);
                    entries[i].name = bin.ReadWideString();
                    bin.StepOut();

                    bin.StepInto(entries[i].header.entryDataOffset);
                    entries[i].data = bin.ReadBytes((int)entries[i].header.entrySize);
                    bin.StepOut();
                }
            }
        }

        /// <summary>
        /// Save a BND4 archive without updating headers.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            using (BinWriter bin = new BinWriter(File.OpenWrite(path))
            {

            }
        }

        /// <summary>
        /// Dump unencrypted entry data to files.
        /// </summary>
        public void DumpEntries(bool decrypt = false, string folder = "")
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (BND4Entry entry in entries)
            {
                File.WriteAllBytes(Path.Combine(folder, entry.name), decrypt? entry.DecryptData() : entry.data);
            }
        }
    }
}
