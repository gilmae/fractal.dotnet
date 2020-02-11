using System;
namespace mandelbrot.net
{
    public interface ICalculator
    {
        Result Calculate(double real, double imag);

        public class Result
        {
            public bool? Escaped { get; set; }
            public int Iterations { get; set; }
        }
    }


}
