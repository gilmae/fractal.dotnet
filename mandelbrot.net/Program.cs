using System;

using System.IO;
using System.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Threading.Channels;
using System.Threading.Tasks;

using CommandLine;

namespace mandelbrot.net
{
    class Program
    {
        static ICalculator escapeCalc;

        static Channel<Point> points = Channel.CreateUnbounded<Point>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = false });
        static Channel<Point> channel = Channel.CreateUnbounded<Point>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = false });

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var o = ((Parsed<Options>)Parser.Default.ParseArguments<Options>(args)).Value;

            escapeCalc = new EscapeTimeCalculator(o);

            double realStep = 3.0 / (o.Width - 1) * o.Zoom;
            double imagStep = 3.0 / (o.Height - 1) * o.Zoom;
            double step = Math.Min(realStep, imagStep);

            IColouring colouring = new NoColour();
            if (o.Colouring == ColourMode.True)
            {
                colouring = new InterpolatedColour(o);
            }

            var xRange = Enumerable.Range(0, o.Width - 1);
            var realRange = xRange.Select(i => o.Real + step * (i - (o.Width - 1) / 2)).ToArray();

            var yRange = Enumerable.Range(0, o.Height - 1);
            var imagRange = yRange.Select(i => o.Imaginary + step * (-1 * i + (o.Height - 1) / 2)).ToArray();

            foreach (int y in yRange)
            {
                foreach (int x in xRange)
                {
                    points.Writer.TryWrite(new Point { x = x, y = y, real = realRange[x], imag = imagRange[y] });
                }
            }
            points.Writer.Complete();

            var image = new Image<Rgba32>(o.Width, o.Height);

            Plotter plotter = new Plotter(points.Reader, channel.Writer, escapeCalc);
            Drawer drawer = new Drawer(channel.Reader, image, colouring);

            _ = Task.WhenAll(plotter.GetPlotter(), plotter.GetPlotter(), plotter.GetPlotter(), plotter.GetPlotter())
                .ContinueWith(_ => channel.Writer.Complete());

            await Task.WhenAll(drawer.GetDrawer(), drawer.GetDrawer(), drawer.GetDrawer(), drawer.GetDrawer()).ContinueWith(_ =>
            {
                using (FileStream fs = new FileStream(o.Output, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    image.SaveAsJpeg(fs);
                    image.Dispose();
                }
            });
        }

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

        public class Plotter
        {
            private ChannelReader<Point> _points;
            private ChannelWriter<Point> _channelWriter;
            private ICalculator _calculator;

            public Plotter(ChannelReader<Point> points, ChannelWriter<Point> channelWriter, ICalculator calculator)
            {
                _points = points;
                _channelWriter = channelWriter;
                _calculator = calculator;
            }


            public async Task GetPlotter()
            {
                while (await _points.WaitToReadAsync())
                {
                    while (_points.TryRead(out Point p))
                    {
                        _channelWriter.TryWrite(Plot(p));
                    }
                }
            }

            public Point Plot(Point p)
            {
                var result = _calculator.Calculate(p.real, p.imag);
                p.escaped = result.Escaped.Value;
                p.iterations = result.Iterations;
                return p;
                
            }
        }
    }
}

