using System;
using CommandLine;

namespace mandelbrot.net
{
    public class Options
    {
        private const string defaultGradient = "[['0.0', 'ff0000'],['0.16', 'ff7f00'],['0.33', 'ffff00'],['0.5', '00ff00'],['0.66', '0000ff'],['0.83', '4b0082'],['1.0','9400d3']]";

        [Option('r', "real", Required = false, Default = -0.75, HelpText = "Set output to verbose messages.")]
        public double Real { get; set; }

        [Option('i', "imaginary", Default = 0.0, Required = false, HelpText = "Set output to verbose messages.")]
        public double Imaginary { get; set; }

        [Option('z', "zoom", Default = 1.0, Required = false, HelpText = "Set output to verbose messages.")]
        public double Zoom { get; set; }

        [Option('g', "gradient", Default = defaultGradient, Required = false)]
        public string Gradient { get; set; }

        [Option('h', "height", Default = 1600, Required = false)]
        public int Height { get; set; }

        [Option('w', "width", Default = 1600, Required = false)]
        public int Width { get; set; }

        [Option('m', "max", Default = 1000, Required = false)]
        public int MaxIterations { get; set; }

        [Option('c', "colourmode", Default = ColourMode.True, Required = false)]
        public ColourMode Colouring { get; set; }

        [Option('s', "setcolour", Default ="000000", Required=false)]
        public string SetColour {get;set;}

    }

    public enum ColourMode
    {
        None,
        True
    }
}
