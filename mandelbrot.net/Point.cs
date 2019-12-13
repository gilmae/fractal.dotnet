using System;
namespace mandelbrot.net
{
    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }
        public double real { get; set; }
        public double imag { get; set; }
        public int iterations { get; set; }
        public bool escaped { get; set; }
    }
}
