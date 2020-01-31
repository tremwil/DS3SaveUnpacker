using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace DS3SaveEditor
{
    class BND4Entry
    {
        public static readonly byte[] DS3_KEY = "FD464D695E69A39A10E319A7ACE8B7FA".hexToBytes();

        /// <summary>
        /// Header of this BND4 entry.
        /// </summary>
        public BND4EntryHeader32 header;
        /// <summary>
        /// Name of this BND4 entry.
        /// </summary>
        public string name;

        /// <summary>
        /// Raw data stored in the entry.
        /// </summary>
        public byte[] data;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BND4Entry() { }
        
        /// <summary>
        /// Initialize a BND4Entry with default properties, specifying the use of encryption.
        /// </summary>
        public BND4Entry(string name, byte[] data)
        {
            this.name = name;
            this.data = data;
        }

        /// <summary>
        /// Decrypt the entry data, assuming 128bit AES-CBC for DS3 saves.
        /// </summary>
        /// <returns></returns>
        public byte[] DecryptData()
        {
            Aes AES = Aes.Create();
            AES.Mode = CipherMode.CBC;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.None;

            Array.Copy(this.data, AES.IV, AES.BlockSize);
            AES.Key = DS3_KEY;

            byte[] buff = new byte[data.Length];
            using (MemoryStream m = new MemoryStream(this.data, AES.BlockSize, data.Length - AES.BlockSize))
            {
                var crypto = new CryptoStream(m, AES.CreateDecryptor(), CryptoStreamMode.Read);
                var decryptLen = crypto.Read(buff, 0, data.Length);
                Array.Resize(ref buff, decryptLen);
            }
            return buff;
        }

        public void SetEncryptedData(byte[] data)
        {
            Aes AES = Aes.Create();
            AES.Mode = CipherMode.CBC;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.None;


        }
    }
}
