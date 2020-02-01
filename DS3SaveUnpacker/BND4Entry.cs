using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace DS3SaveUnpacker
{
    class BND4Entry
    {
        private const bool DEBUG = true;

        /// <summary>
        /// Encryption key for DS3 USERDATA subfiles.
        /// </summary>
        public static readonly byte[] DS3_KEY = "FD464D695E69A39A10E319A7ACE8B7FA".hexToBytes();

        /// <summary>
        /// Header of this BND4 entry.
        /// </summary>
        public BND4EntryHeader header;
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
            if (DEBUG) Console.WriteLine(string.Format("[BND4Entry] DECRYPT ENTRY DATA OF '{0}'", name));

            Aes AES = Aes.Create();
            AES.Mode = CipherMode.CBC;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            byte[] IV = new byte[16];
            Array.Copy(this.data, 16, IV, 0, 16);
            AES.IV = IV;

            AES.Key = DS3_KEY;

            byte[] buff = new byte[data.Length - 32];
            using (MemoryStream m = new MemoryStream(this.data, 32, data.Length - 32))
            {
                var crypto = new CryptoStream(m, AES.CreateDecryptor(), CryptoStreamMode.Read);
                var decryptLen = crypto.Read(buff, 0, data.Length - 32);
                Array.Resize(ref buff, decryptLen);
            }
            return buff;
        }

        public void SetEncryptedData(byte[] newData, bool usePreviousIV = true)
        {
            if (DEBUG) Console.WriteLine(string.Format("[BND4Entry] SET ENCRYPTED DATA OF '{0}'", name));

            var AES = Aes.Create();
            AES.Mode = CipherMode.CBC;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            if (usePreviousIV) 
            {
                byte[] IV = new byte[16];
                Array.Copy(this.data, 16, IV, 0, 16);
                AES.IV = IV;
            }
            else
            {
                AES.GenerateIV();
            }

            AES.Key = DS3_KEY;

            int padLen = 16 - newData.Length % 16;
            data = new byte[32 + newData.Length + padLen];
            Array.Copy(AES.IV, 0, data, 16, 16);

            using (MemoryStream m = new MemoryStream(data, 32, data.Length - 32, true))
            {
                var crypto = new CryptoStream(m, AES.CreateEncryptor(), CryptoStreamMode.Write);
                crypto.Write(newData, 0, newData.Length);
                crypto.FlushFinalBlock();
            }

            if (DEBUG) Console.WriteLine(string.Format("[BND4Entry] SIGN ENCRYPTED ENTRY DATA OF '{0}'", name));

            var md5 = MD5.Create();
            byte[] checksum = md5.ComputeHash(data, 16, data.Length - 16);
            Array.Copy(checksum, 0, data, 0, 16);
        }
    }
}
