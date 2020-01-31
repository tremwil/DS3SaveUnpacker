using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;


namespace DS3SaveEditor
{
    static class Program
    {
        static void Main()
        {
            var save = new BND4File("C:\\Users\\William\\AppData\\Roaming\\DarkSoulsIII\\011000010b7474d4\\DS30000.sl2", true);

        }
    }
}
