using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI.Controls
{
    public abstract class Control
    {
        private Size _size;

        public Size Size { get; set; }
        public Point Location { get; set; }
        public string Name { get; set; }
        public TextBuffer Buffer { get; set; }

        public ColorInfo Color { get; set; }

        #region Boundary helpers
        public int X
        {
            get
            {
                return Location.X;
            }
            set
            {
                Location.X = value;
            }
        }

        public int Y
        {
            get
            {
                return Location.Y;
            }
            set
            {
                Location.Y = value;
            }
        }

        public int Width
        {
            get
            {
                return Size.Width;
            }
            set
            {
                Size.Width = value;
            }
        }

        public int Height
        {
            get
            {
                return Size.Height;
            }
            set
            {
                Size.Height = value;
            }
        }

        public int Left
        {
            get
            {
                return X;
            }
        }

        public int Top
        {
            get
            {
                return Y;
            }
        }

        public int Right
        {
            get
            {
                return X + Width;
            }
        }

        public int Bottom
        {
            get
            {
                return Y + Height;
            }
        }
        #endregion

        public Control()
        {

        }

        public void Initialize()
        {
            Buffer = new TextBuffer(Size);
        }

        public void ResetBuffer()
        {
            Buffer.Clear();
            Buffer.SetCursor(0, 0);
            Buffer.CurrentColor = Color;
        }

        public void Resize(Size size)
        {
            Size = size;
            Buffer.SetSize(size);
        }

        public abstract void Draw();
    }
}
