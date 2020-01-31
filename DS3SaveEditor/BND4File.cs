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
        /// AES-128 CBC key for Dark Souls III
        /// </summary>
        public byte[] DS3_KEY = "FD464D695E69A39A10E319A7ACE8B7FA".hexToBytes();
        /// <summary>
        /// BND4 file header.
        /// </summary>
        public BND4Header header;
        /// <summary>
        /// The entries of the BND4 file.
        /// </summary>
        public BND4Entry[] entries;
        /// <summary>
        /// True if the BND4 entry data is encrypted.
        /// </summary>
        public bool encrypted;

        public BND4File(string filePath, bool encrypted)
        {
            this.encrypted = encrypted;
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.None;

            using (BinReader bin = new BinReader(File.OpenRead(filePath)))
            {
                header = bin.ReadStruct<BND4Header>();
                entries = new BND4Entry[header.fileCnt];

                for (int i = 0; i < header.fileCnt; i++)
                {
                    entries[i] = new BND4Entry(encrypted);
                    entries[i].header = bin.ReadStruct<BND4EntryHeader32>();

                    bin.StepInto(entries[i].header.entryNameOffset);
                    entries[i].name = bin.ReadWideString();
                    bin.StepOut();

                    bin.StepInto(entries[i].header.entryDataOffset);
                    if (encrypted)
                    {
                        entries[i].AES_IV = bin.ReadBytes(16);
                        entries[i].data = new byte[entries[i].header.entrySize - 16];

                        var aesDecrypt = aes.CreateDecryptor(DS3_KEY, entries[i].AES_IV);
                        var crypto = new CryptoStream(bin.BaseStream, aesDecrypt, CryptoStreamMode.Read);
                        crypto.Read(entries[i].data, 0, entries[i].data.Length);
                    }
                    else
                    {
                        entries[i].data = bin.ReadBytes((int)entries[i].header.entrySize);
                    }
                    bin.StepOut();
                }
            }
        }
    }
}
