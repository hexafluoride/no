using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No.UI
{
    /// <summary>
    /// Represents a size.
    /// </summary>
    public class Size
    {
        /// <summary>
        /// The width of this Size.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of this Size.
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// Creates a Size object with the given width and height.
        /// </summary>
        /// <param name="width">The width of the Size.</param>
        /// <param name="height">The height of the Size.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
