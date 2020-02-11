using System;
namespace mandelbrot.net
{
    public class EscapeTimeCalculator : ICalculator
    {
        private const double MandelbrotBailout = 4.0;
        private const double MainCardioidBounds = 0.0625;

        private int _maxIterations;
        public EscapeTimeCalculator(Options o)
        {
            _maxIterations = o.MaxIterations;
        }

        public ICalculator.Result Calculate(double real, double imag)
        {
            if (Math.Pow(real + 1.0, 2.0) + Math.Pow(imag, 2.0) <= MainCardioidBounds)
            {
                return new ICalculator.Result { Escaped = false, Iterations = int.MaxValue };
            }
            
            int iteration = 1;
            double rsquare = 0.0;
            double isquare = 0.0;
            double zsquare = 0.0;
            double x;
            double y;

            while (rsquare + isquare <= MandelbrotBailout && iteration < _maxIterations)
            {
                x = rsquare - isquare + real;
                y = zsquare - rsquare - isquare + imag;
                rsquare = x * x;
                isquare = y * y;
                zsquare = (x + y) * (x + y);
                iteration += 1;
            }

            return new ICalculator.Result { Escaped = iteration < _maxIterations, Iterations = iteration };
        }
    }
}
