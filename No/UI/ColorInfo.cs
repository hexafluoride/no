using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI
{
    /// <summary>
    /// A class containing two ConsoleColors, one foreground and one background.
    /// </summary>
    public class ColorInfo
    {
        public ConsoleColor ForegroundColor;
        public ConsoleColor BackgroundColor;

        public ColorInfo(ConsoleColor foreground, ConsoleColor background)
        {
            ForegroundColor = foreground;
            BackgroundColor = background;
        }

        public ColorInfo(byte b)
        {
            ForegroundColor = (ConsoleColor)((b & 0xf0) >> 4);
            BackgroundColor = (ConsoleColor)(b & 0x0f);
        }

        public byte Pack()
        {
            return (byte)((((int)ForegroundColor) << 4) | ((int)BackgroundColor));
        }

        public void SetConsoleColor()
        {
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
        }

        public static ColorInfo FromConsole()
        {
            return new ColorInfo(Console.ForegroundColor, Console.BackgroundColor);
        }
    }
}
