using System;
namespace mandelbrot.net
{
    public class PreCalculator : ICalculator
    {
        public ICalculator.Result Calculate(double real, double imag)
        {
            if (Math.Pow(real + 1.0, 2.0) + Math.Pow(imag, 2.0) <= 0.0625)
            {
                return new ICalculator.Result { Escaped = false, Iterations = int.MaxValue, HasResult = true };
            }
            return new ICalculator.Result();
        }
    }
}
