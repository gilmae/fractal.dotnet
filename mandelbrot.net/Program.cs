using System;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Threading.Channels;
using System.Threading.Tasks;

using CommandLine;
using System.Threading;

namespace mandelbrot.net
{
    class Program
    {


        static ICalculator preCalc = new PreCalculator();
        static ICalculator escapeCalc = new EscapeTimeCalculator(1000);

        static Channel<Point> points = Channel.CreateUnbounded<Point>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = false });
        static Channel<Point> channel = Channel.CreateUnbounded<Point>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = false });


        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static Options GetOptions(string[] args)
        {
            Options options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       options.Colouring = o.Colouring;
                       options.Gradient = o.Gradient;
                       options.Height = o.Height;
                       options.Imaginary = o.Imaginary;
                       options.MaxIterations = o.MaxIterations;
                       options.Real = o.Real;
                       options.Width = o.Width;
                       options.Zoom = o.Zoom;
                   });
            return options;
        }

        static async Task MainAsync(string[] args)
        {
            Options o = GetOptions(args);
            var image = new Image<Rgba32>(o.Width, o.Height);
            FileStream fs = new FileStream(@"/Users/gilmae/mb.jpg", FileMode.OpenOrCreate, FileAccess.Write);

            double realStep = 3.0 / (o.Width * o.Zoom);
            double imagStep = 3.0 / (o.Height * o.Zoom);
            double step = Math.Min(realStep, imagStep);

            IColouring colouring = new NoColour();
            if (o.Colouring == ColourMode.True)
            {
                colouring = new InterpolatedColour(o);
            }


            IEnumerable<int> imagRange = Enumerable.Range(0, o.Height - 1).Select(x => x);
            IEnumerable<int> realRange = Enumerable.Range(0, o.Width - 1).Select(x => x);

            foreach (int y in imagRange.Select(v => v))
            {
                double imag = o.Imaginary + step * (-1 * y + o.Height / 2);
                foreach (int x in realRange)
                {
                    double real = o.Real + step * (x - o.Width / 2);

                    points.Writer.TryWrite(new Point { x = x, y = y, real = real, imag = imag });
                }
            }
            points.Writer.Complete();


            Plotter plotter = new Plotter(points.Reader, channel.Writer, new[] { preCalc, escapeCalc });
            Drawer drawer = new Drawer(channel.Reader, image, colouring);

            Task.WhenAll(plotter.GetPlotter(), plotter.GetPlotter(), plotter.GetPlotter(), plotter.GetPlotter()).ContinueWith(_=>channel.Writer.Complete());
            await Task.WhenAll(drawer.GetDrawer(), drawer.GetDrawer(), drawer.GetDrawer(), drawer.GetDrawer()).ContinueWith(_ =>
            {

                image.SaveAsJpeg(fs);
                fs.Close();
                fs.Dispose();
                image.Dispose();
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
            private IList<ICalculator> _calculators;

            public Plotter(ChannelReader<Point> points, ChannelWriter<Point> channelWriter, IList<ICalculator> calculators)
            {
                _points = points;
                _channelWriter = channelWriter;
                _calculators = calculators;
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
                foreach(ICalculator calculator in _calculators)
                {
                    var result = calculator.Calculate(p.real, p.imag);
                    if (result.HasResult)
                    {
                        p.escaped = result.Escaped.Value;
                        p.iterations = result.Iterations;
                        return p;
                    }
                }
                p.escaped = false;
                p.iterations = int.MaxValue;
                return p;
            }
        }


        
    }
}

