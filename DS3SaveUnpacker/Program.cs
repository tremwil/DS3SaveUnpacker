using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DS3SaveUnpacker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(@"Choose action:
  [1] Unpack encrypted SL2 save file
  [2] Pack encrypted SL2 save file
  [3] Unpack unencrypted BND4 archive
  [4] Pack unencrypted BND4 archive
  [5] Patch SL2 save file linked account
  [6] Recursively patch all saves in a folder
  [7] Exit
");

                char key;
                do { key = Console.ReadKey(true).KeyChar; } while (key < '1' || key > '7');
                if (key == '7') { return; }

                // Get default DS3 Save location & prompt open save file
                var ds3Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkSoulsIII\\");

                if (key == '2' || key == '4')
                {
                    var diagFolder = new CommonOpenFileDialog()
                    {
                        Title = "Select unpack directory",
                        IsFolderPicker = true,
                        InitialDirectory = ds3Folder
                    };
                    if (diagFolder.ShowDialog() != CommonFileDialogResult.Ok) { continue; }
                    BND4File.Pack(diagFolder.FileName, key == '2');

                    Console.Write("\nDone, press any key to return to menu");
                    Console.ReadKey(true);
                    continue;
                }

                if (key == '6')
                {
                    var diagFolder = new CommonOpenFileDialog()
                    {
                        Title = "Select root directory",
                        IsFolderPicker = true,
                        InitialDirectory = ds3Folder
                    };
                    if (diagFolder.ShowDialog() != CommonFileDialogResult.Ok) { continue; }

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

                    foreach (string saveFile in Directory.EnumerateFiles(diagFolder.FileName, "*.sl2", SearchOption.AllDirectories))
                    {
                        Console.WriteLine("  Patching " + saveFile + "...");

                        BND4File BND = new BND4File(saveFile);

                        BND.PatchDS3AccountFlag(steamID);
                        BND.Save(saveFile);
                    }

                    Console.Write("\nDone, press any key to return to menu");
                    Console.ReadKey(true);
                    continue;
                }

                var diagOpen = new OpenFileDialog()
                {
                    Title = "Select BND4/SL2 file",
                    DefaultExt = "sl2",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = false,
                    InitialDirectory = ds3Folder
                };
                if (diagOpen.ShowDialog() != DialogResult.OK) { continue; }

                if (key == '1' || key == '3')
                {
                    var diagFolder = new CommonOpenFileDialog()
                    {
                        Title = "Select unpack directory",
                        IsFolderPicker = true,
                        InitialDirectory = ds3Folder
                    };
                    if (diagFolder.ShowDialog() != CommonFileDialogResult.Ok) { continue; }
                    BND4File.Unpack(diagOpen.FileName, diagFolder.FileName, key == '1');
                }
                if (key == '5')
                {
                    var save = new BND4File(diagOpen.FileName);
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

                    save.PatchDS3AccountFlag(steamID);

                    var diagSave = new SaveFileDialog()
                    {
                        Title = "Save patched file at...",
                        DefaultExt = "sl2",
                        FileName = "DS30000.sl2",
                        InitialDirectory = ds3Folder
                    };
                    if (diagSave.ShowDialog() != DialogResult.OK) { continue; }

                    save.Save(diagSave.FileName);
                }

                Console.Write("\nDone, press any key to return to menu");
                Console.ReadKey(true);
            }
        }
    }
}
