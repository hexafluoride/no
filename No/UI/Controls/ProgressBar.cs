using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI.Controls
{
    public class ProgressBar : Control
    {
        public char LeftBoundary = '[';
        public char RightBoundary = ']';
        public char Bar = '=';
        public char Tip = '>';
        public char Space = ' ';

        public double Min { get; set; }
        public double Max { get; set; }
        public double Value { get; set; }

        public ProgressBar()
        {

        }

        public override void Draw()
        {
            double fraction = (Value - Min) / (Max - Min);
            int length = (int)(fraction * (Width - 2)); // subtract 2 because of boundaries

            Buffer.WriteCharacter(LeftBoundary);
            Buffer.Write(new string(Bar, length));

            if (length < Width - 2)
            {
                Buffer.WriteCharacter(Tip);
                length++;
            }

            Buffer.Write(new string(Space, (Width - 2) - length));
            Buffer.WriteCharacter(RightBoundary);
        }
    }
}
