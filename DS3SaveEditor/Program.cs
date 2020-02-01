using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

namespace DS3SaveEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Choose action:\n    1) Patch SL2 save file linked account\n    2) Dump encrypted SL2 save file");
            ConsoleKeyInfo key;
            do { key = Console.ReadKey(true); } while (key.KeyChar != '1' && key.KeyChar != '2');

            // Get default DS3 Save location & prompt open save file
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var diagOpen = new OpenFileDialog()
            {
                Title = (key.KeyChar == '1') ? "Select save file to patch" : "Select save file to dump",
                DefaultExt = "sl2",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                InitialDirectory = Path.Combine(appData, "DarkSoulsIII\\")
            };
            if (diagOpen.ShowDialog() != DialogResult.OK) { return; }

            var save = new BND4File(diagOpen.FileName);

            if (key.KeyChar == '2')
            {
                save.DumpEntries(true, "UserData");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Proceed carefully. Entering the wrong ID will make the save unreadable.");

            ulong steamID = 0;
            while (steamID == 0)
            {
                Console.Write("Enter your 64-bit Steam ID (number in profile URL): ");
                if (!ulong.TryParse(Console.ReadLine(), out steamID))
                {
                    Console.WriteLine("Please input a valid number.");
                }
            }

            save.PatchAccountFlag(steamID);

            var diagSave = new SaveFileDialog()
            {
                Title = "Save patched file at...",
                DefaultExt = "sl2",
                FileName = "DS30000.sl2",
                InitialDirectory = Path.Combine(appData, "DarkSoulsIII\\")
            };
            if (diagSave.ShowDialog() != DialogResult.OK) { return; }

            save.Save(diagSave.FileName);
            Console.ReadKey();
        }
    }
}
