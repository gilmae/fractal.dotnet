using System;
using SixLabors.ImageSharp.PixelFormats;

namespace mandelbrot.net
{
    public class NoColour : IColouring
    {
        public Rgba32 GetColour(Point p)
        {
            if (p.escaped)
            {
                return Rgba32.White;
            }
            return Rgba32.Black;
        }
    }
}
