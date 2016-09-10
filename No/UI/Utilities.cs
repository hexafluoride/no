using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI
{
    class Utilities
    {
        public static void ClearArea(Rectangle rect)
        {
            int x_t = Console.CursorLeft;
            int y_t = Console.CursorTop;

            for(int x = rect.Left; x < rect.Right; x++)
            {
                for(int y = rect.Top; y < rect.Bottom; y++)
                {
                    Console.SetCursorPosition(x, y);

                    Console.Write(" ");
                }
            }

            Console.SetCursorPosition(x_t, y_t);
        }
    }
}
