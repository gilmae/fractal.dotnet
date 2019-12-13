using System;
using CommandLine;

namespace mandelbrot.net
{
    public class Options
    {
        private const string defaultGradient = "[['0.0', '000764'],['0.16', '026bcb'],['0.42', 'edffff'],['0.6425', 'ffaa00'],['0.8675', '000200'],['1.0', '000764']]";

        [Option('r', "real", Required = false, Default=-0.75, HelpText = "Set output to verbose messages.")]
        public double Real { get; set; }

        [Option('i', "imaginary", Default =0.0, Required = false, HelpText = "Set output to verbose messages.")]
        public double Imaginary { get; set; }

        [Option('z', "zoom", Default = 1.0, Required = false, HelpText = "Set output to verbose messages.")]
        public double Zoom { get; set; }

        [Option('g', "gradient", Default=defaultGradient, Required =false)]
        public string Gradient { get; set; }

        [Option('h', "height", Default =1600, Required =false)]
        public int Height { get; set; }

        [Option('w', "width", Default = 1600, Required = false)]
        public int Width { get; set; }

        [Option('m', "max", Default = 1000, Required = false)]
        public int MaxIterations { get; set; }

        [Option('c', "colourmode", Default="none", Required=false]
        public ColourMode Colouring { get; set; }

    }

    public enum ColourMode
    {
        None,
        True
    }
}
