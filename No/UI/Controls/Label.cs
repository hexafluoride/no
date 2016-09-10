using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI.Controls
{
    public class Label : Control
    {
        public string Text { get; set; }

        public Label()
        {

        }

        public override void Draw()
        {
            ResetBuffer();

            Buffer.Write(Text);
        }
    }
}
