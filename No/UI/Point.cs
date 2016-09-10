using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI
{
    /// <summary>
    /// Represents a point within the Cartesian coordinate space.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// The X axis of this Point.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y axis of this Point.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// A Point with the coordinates (0,0).
        /// </summary>
        public static readonly Point Empty = new Point(0, 0);

        /// <summary>
        /// Creates a Point with the given coordinates.
        /// </summary>
        /// <param name="x">The X axis of the Point.</param>
        /// <param name="y">The Y axis of the Point.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
    }
}
