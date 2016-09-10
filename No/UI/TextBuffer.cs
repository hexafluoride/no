using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI
{
    public class TextBuffer
    {
        /// <summary>
        /// The text contained within this TextBuffer.
        /// </summary>
        public char[,] Text { get; set; }
        public ColorInfo[,] Colors { get; set; }

        /// <summary>
        /// The size of this TextBuffer.
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// The width of this TextBuffer.
        /// </summary>
        public int Width
        {
            get
            {
                return Size.Width;
            }
        }

        /// <summary>
        /// The height of this TextBuffer.
        /// </summary>
        public int Height
        {
            get
            {
                return Size.Height;
            }
        }

        /// <summary>
        /// The location of this buffer's cursor.
        /// </summary>
        public Point Cursor { get; set; }

        public ColorInfo CurrentColor { get; set; }

        public ConsoleColor ForegroundColor
        {
            get
            {
                return CurrentColor.ForegroundColor;
            }
            set
            {
                CurrentColor.ForegroundColor = value;
            }
        }
        public ConsoleColor BackgroundColor
        {
            get
            {
                return CurrentColor.BackgroundColor;
            }
            set
            {
                CurrentColor.BackgroundColor = value;
            }
        }

        public TextBuffer(Size size)
        {
            SetSize(size);
            CurrentColor = new ColorInfo(ConsoleColor.DarkRed, ConsoleColor.White);
        }

        public TextBuffer(int width, int height) :
            this(new Size(width, height))
        {

        }

        public void SetSize(Size size)
        {
            Size = size;

            Text = new char[Height, Width];
            Colors = new ColorInfo[Height, Width];
            Cursor = new Point(0, 0);
        }

        public void PrintToConsole(Point position, bool clear = false)
        {
            ColorInfo color = ColorInfo.FromConsole();

            Rectangle drawarea = new Rectangle(position, Size);

            if (clear)
                Console.Clear();

            for (int y = drawarea.Top; y < drawarea.Bottom; y++)
            {
                Console.SetCursorPosition(position.X, y);

                for(int x = drawarea.Left; x < drawarea.Right; x++)
                {
                    int local_x = x - drawarea.Left;
                    int local_y = y - drawarea.Top;

                    Colors[local_y, local_x].SetConsoleColor();
                    Console.Write(Text[local_y, local_x]);
                }
                Console.WriteLine();
            }

            color.SetConsoleColor();
        }

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Text[y, x] = ' ';
                    Colors[y, x] = CurrentColor;
                }
            }
        }

        public void SetCursor(int x, int y)
        {
            Cursor.X = x;
            Cursor.Y = y;
        }

        public void Write(string str, params object[] format)
        {
            str = string.Format(str, format);

            foreach (char c in str)
                WriteCharacter(c);
        }

        public void WriteNoWrap(string str, params object[] format)
        {
            str = string.Format(str, format);

            foreach (char c in str)
                WriteCharacter(c, false);
        }

        public void WriteCharacter(char c, bool wrap = true)
        {
            if (!CanPrint())
                return;

            switch (c)
            {
                case '\n':
                    Cursor.Y++;
                    break;
                case '\r':
                    Cursor.X = 0;
                    break;
                default:
                    Text[Cursor.Y, Cursor.X] = c;
                    Colors[Cursor.Y, Cursor.X] = CurrentColor;
                    IncrementCursor(wrap);
                    break;
            }
        }

        private void IncrementCursor(bool wrap)
        {
            Cursor.X++;

            if (Cursor.X >= Width && wrap)
            {
                Cursor.Y++;
                Cursor.X = 0;
            }
        }

        private bool CanPrint()
        {
            return
                Cursor.Y < Height && Cursor.X < Width &&
                Cursor.Y >= 0 && Cursor.X >= 0;
        }
    }
}
