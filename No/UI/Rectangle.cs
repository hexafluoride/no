using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace No.UI
{
    /// <summary>
    /// Represents a rectangle within a Cartesian coordinate space.
    /// </summary>
    public class Rectangle
    {
        public Point Location { get; set; }
        public Size Size { get; set; }

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

        public Rectangle(Point location, Size size)
        {
            Location = location;
            Size = size;
        }

        public Rectangle(int x, int y, Size size) :
            this(new Point(x, y), size)
        {

        }

        public Rectangle(Point location, int width, int height) :
            this(location, new Size(width, height))
        {

        }

        public Rectangle(int x, int y, int width, int height) :
            this(new Point(x, y), new Size(width, height))
        {

        }

        public bool Intersects(Rectangle rect)
        {
            return IntersectCheck(this, rect) || IntersectCheck(rect, this);
        }

        public bool Contains(Point point)
        {
            return
                point.X < this.Right && point.X > this.Left &&
                point.Y < this.Bottom && point.Y > this.Top;
        }

        private bool IntersectCheck(Rectangle first, Rectangle second)
        {
            return
                first.Top < second.Bottom && first.Top > second.Top &&
                first.Left < second.Right && first.Left > second.Left;
        }

        public static void Test()
        {
            Debug.Assert(new Rectangle(0, 0, 100, 100).Intersects(new Rectangle(50, 50, 10, 10)));
            Debug.Assert(new Rectangle(20, 40, 30, 30).Intersects(new Rectangle(25, 50, 100, 100)));
            Debug.Assert(!new Rectangle(0, 0, 100, 100).Intersects(new Rectangle(100, 100, 10, 10)));
            Debug.Assert(!new Rectangle(20, 40, 30, 30).Intersects(new Rectangle(30, 20, 2, 2)));
        }
    }
}
