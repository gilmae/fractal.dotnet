using SixLabors.ImageSharp.PixelFormats;

namespace mandelbrot.net
{
    public interface IColouring
    {
        Rgba32 GetColour(Point p);
    }
}
