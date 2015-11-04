using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace No
{
    public class Utilities
    {
        public static string GetString(byte[] data)
        {
            return "0x" + BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        public static void SetClipboard(string text)
        {
            Thread thr = new Thread(new ThreadStart(delegate 
            {
                Clipboard.SetText(text);
            }));

            thr.SetApartmentState(ApartmentState.STA);
            thr.Start();
        }

        public static void LogMessage(string message, params object[] format)
        {
#if DEBUG
            Console.WriteLine(message, format);
#endif
        }
    }
}
