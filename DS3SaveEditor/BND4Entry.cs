using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS3SaveEditor
{
    class BND4Entry
    {
        /// <summary>
        /// Header of this BND4 entry.
        /// </summary>
        public BND4EntryHeader32 header;
        /// <summary>
        /// Name of this BND4 entry.
        /// </summary>
        public string name;
        /// <summary>
        /// True if this entry is encrypted.
        /// </summary>
        public bool encrypted;
        /// <summary>
        /// IV of the AES encryption.
        /// </summary>
        public byte[] AES_IV;
        /// <summary>
        /// Unencrypted raw data stored in the entry.
        /// </summary>
        public byte[] data;
        
        /// <summary>
        /// Initialize a BND4Entry with default properties, specifying the use of encryption.
        /// </summary>
        /// <param name="encrypted"></param>
        public BND4Entry(bool encrypted)
        {
            this.encrypted = encrypted;
        }
    }
}
