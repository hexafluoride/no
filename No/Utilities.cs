#define COLOR // Linux might have some color issues, so comment this line if you don't want color.
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

        /// <summary>
        /// Prints a color-coded string. Color codes are as follows:
        /// %0: black
        /// %7: gray
        /// %a: green
        /// %c: red
        /// %f: white(for emphasis)
        /// %e: yellow
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="format">List of objects to apply to a formatted string</param>
        public static void PrintColoredLine(string message, params object[] format)
        {
            PrintColored(message, format);
            Console.WriteLine();
        }

#if COLOR
        /// <summary>
        /// Prints a color-coded string. Color codes are as follows:
        /// %0: black
        /// %7: gray
        /// %a: green
        /// %c: red
        /// %f: white(for emphasis)
        /// %e: yellow
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="format">List of objects to apply to a formatted string</param>
        public static void PrintColored(string message, params object[] format)
        {
            if (!message.Contains('%'))
            {
                Console.WriteLine(message, format);
                return;
            }

            message = string.Format(message, format);

            ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            string[] fragments = message.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string str in fragments)
            {
                string fragment = str.Substring(1);
                string color_code = str[0].ToString();

                try
                {
                    int color = Convert.ToInt32(color_code, 16);
                    Console.ForegroundColor = colors[color];
                }
                catch
                {
                    fragment = str;
                }

                Console.Write(fragment);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }
#endif

#if !COLOR
        public static void PrintColored(string message, params object[] format)
        {
            Console.WriteLine(message, format);
        }
#endif

        public static void LogMessage(string message, params object[] format)
        {
#if DEBUG
            Console.WriteLine(message, format);
#endif
        }
    }
}
