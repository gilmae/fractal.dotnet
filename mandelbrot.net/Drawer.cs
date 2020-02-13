using System.Threading.Channels;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace mandelbrot.net
{
    public class Drawer
    {
        private ChannelReader<Point> _reader;
        private Image<Rgba32> _image;
        private IColouring _colourer;

        public Drawer(ChannelReader<Point> reader, Image<Rgba32> image, IColouring colourer)
        {
            _reader = reader;
            _image = image;
            _colourer = colourer;
        }

        public async Task GetDrawer()
        {
            while (await _reader.WaitToReadAsync())
            {
                while (_reader.TryRead(out Point p))
                {
                    _image[p.x, p.y] = _colourer.GetColour(p);
                }
            }
        }
    }
}
