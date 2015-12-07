#define CLIPBOARD // Same, disable with Linux if you encounter issues

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace No
{
    public class Utilities
    {
        public static void ProperMove(string source, string dest, string root = "")
        {
            // this function doesn't actually *move* files, it copies them non-destructively

            if (root == "")
                root = source;

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            DirectoryInfo dir = new DirectoryInfo(source);

            foreach(var file in dir.GetFiles())
            {
                LogMessage("Moving file {0} to {1}", file.FullName, Path.Combine(dest, GetRelativePath(root, file.FullName)));

                file.CopyTo(Path.Combine(dest, GetRelativePath(root, file.FullName)));
            }
            foreach(var child_dir in dir.GetDirectories())
            {
                LogMessage("Moving directory {0} to {1}", child_dir.FullName, Path.Combine(dest, GetRelativePath(root, child_dir.FullName)));

                ProperMove(child_dir.FullName, Path.Combine(dest, GetRelativePath(root, child_dir.FullName), root));
            }
        }

        public static string GetRelativePath(string absolute, string child)
        {
            absolute = Path.GetFullPath(absolute);
            child = Path.GetFullPath(child);

            Uri absolute_uri = new Uri(absolute);
            Uri child_uri = new Uri(child);

            return Uri.UnescapeDataString(absolute_uri.MakeRelativeUri(child_uri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static string GetDataRoot()
        {
            if (IsRunningOnMono())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nodata" + Path.DirectorySeparatorChar);
            else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "No" + Path.DirectorySeparatorChar);
        }

        public static string GetString(byte[] data)
        {
            return "0x" + BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static void SetClipboard(string text)
        {
#if CLIPBOARD
            if (IsRunningOnMono())
            {
                RunAndPipe("xclip", "", text);
            }
            else
            { 
                Thread thr = new Thread(new ThreadStart(delegate
                {
                    Clipboard.SetText(text);
                }));

                thr.SetApartmentState(ApartmentState.STA);
                thr.Start();
            }
#endif
        }

        public static void RunAndPipe(string file, string args, string data)
        {
            ProcessStartInfo psi = new ProcessStartInfo(file, args);
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;

            Process proc = Process.Start(psi);
            proc.StandardInput.Write(data);
            proc.StandardInput.Close();

        }

        public static void LogMessage(string message, params object[] format)
        {
#if DEBUG
            Console.WriteLine(message, format);
#endif
        }
    }
}
