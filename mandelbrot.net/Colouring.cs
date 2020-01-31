using System;
using System.Linq;
using System.Collections.Generic;

using SixLabors.ImageSharp.PixelFormats;
namespace mandelbrot.net
{
    public interface IColouring
    {
        Rgba32 GetColour(Point p);
    }

    public class InterpolatedColour : IColouring
    {
        private CubicInterpolation redInterpolator;
        private CubicInterpolation blueInterpolator;
        private CubicInterpolation greenInterpolator;
        private int _maxIterations;
        private string _setColour;

        public InterpolatedColour(Options options)
        {
            var gradient = Newtonsoft.Json.JsonConvert.DeserializeObject<string[][]>(options.Gradient);
            _maxIterations = options.MaxIterations;
            _setColour = options.SetColour;

            double[] xPoints = gradient.Select(x => double.Parse(x[0])).ToArray();
            var hexes = gradient.Select(x => new[] { x[1].Substring(0, 2), x[1].Substring(2, 2), x[1].Substring(4, 2) });

            double[] redPoints = hexes.Select(x => Convert.ToInt32(x[0], 16)*1.0).ToArray();
            double[] greenPoints = hexes.Select(x => Convert.ToInt32(x[1], 16) * 1.0).ToArray();
            double[] bluePoints = hexes.Select(x => Convert.ToInt32(x[2], 16) * 1.0).ToArray();


            redInterpolator = new CubicInterpolation(xPoints, redPoints);
            greenInterpolator = new CubicInterpolation(xPoints, greenPoints);
            blueInterpolator = new CubicInterpolation(xPoints, bluePoints);
        }

        public Rgba32 GetColour(Point p)
        {
            if (p.escaped)
            {
                double escapePercentage = p.iterations * 1.0 / _maxIterations;

                int r = (int)Math.Abs(redInterpolator.GetYPoint(escapePercentage));
                int g = (int)Math.Abs(greenInterpolator.GetYPoint(escapePercentage));
                int b = (int)Math.Abs(blueInterpolator.GetYPoint(escapePercentage));

                try
                {
                    return Rgba32.FromHex($"{r:X2}{g:X2}{b:X2}");
                }
                catch (Exception)
                {
                    Console.WriteLine($"{r}|{g}|{b}");
                    Console.WriteLine($"{r:X2}{g:X2}{b:X2}");
                    throw;
                }
            }
            else
            {
                return Rgba32.FromHex(_setColour);
            }
        }
    }
}
