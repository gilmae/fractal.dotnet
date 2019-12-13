using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using CommandLine;

namespace mandelbrot.net
{
    class Program
    {


        static ICalculator preCalc = new PreCalculator();
        static ICalculator escapeCalc = new EscapeTimeCalculator(1000);

        static ConcurrentQueue<Point> points = new ConcurrentQueue<Point>();
        static ConcurrentQueue<Point> plottedPoints = new ConcurrentQueue<Point>();

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {

                       double realStep = 3.0 / (o.Width * o.Zoom);
                       double imagStep = 3.0 / (o.Height * o.Zoom);
                       double step = Math.Min(realStep, imagStep);

                       IColouring colouring;
                       if (o.Colouring == ColourMode.True)
                       {
                           colouring = new InterpolatedColour(o);
                       }
                       else if (o.Colouring == ColourMode.None)
                       {
                           colouring = new NoColour();
                       }


                       IEnumerable<int> imagRange = Enumerable.Range(0, o.Height-1).Select(x => x);
                       IEnumerable<int> realRange = Enumerable.Range(0, o.Width-1).Select(x => x);

                       foreach (int y in imagRange.Select(v => v))
                       {
                           double imag = o.Imaginary + step * (-1 * y + o.Height/2);
                           foreach (int x in realRange)
                           {
                               double real = o.Real + step * (x - o.Width/2);

                               points.Enqueue(new Point { x = x, y = y, real = real, imag = imag });
                           }
                       }

                       Action action = () =>
                       {
                           Point localValue;
                           while (points.TryDequeue(out localValue))
                           {
                               plottedPoints.Enqueue(Plot(localValue));
                           }

                       };

                       Parallel.Invoke(action, action, action, action);

                       using (var image = new Image<Rgba32>(o.Width, o.Height))
                       {
                           using (FileStream fs = new FileStream(@"/Users/gilmae/mb.jpg", FileMode.OpenOrCreate, FileAccess.Write))
                           {
                               Point p;
                               while (plottedPoints.TryDequeue(out p))
                               {
                                   image[p.x, p.y] = colouring.GetColour(p);
                               }
                               image.SaveAsJpeg(fs);
                           }
                       }

                   });


        }


        public static Point Plot(Point p)
        {
            var preCalcResult = preCalc.Calculate(p.real, p.imag);
            if (preCalcResult.Escaped.HasValue && !preCalcResult.Escaped.Value)
            {
                p.escaped = false;
                p.iterations = int.MaxValue;
                return p;
            }

            var escapeCalcResult = escapeCalc.Calculate(p.real, p.imag);

            if (escapeCalcResult.Escaped.HasValue)
            {
                if (escapeCalcResult.Escaped == true)
                {
                    p.escaped = true;
                    p.iterations = escapeCalcResult.Iterations;

                }
                else
                {
                    p.escaped = false;
                    p.iterations = int.MaxValue;
                }

            }

            return p;


        }
    }
}

