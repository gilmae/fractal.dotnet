using System;
namespace mandelbrot.net
{
    public class EscapeTimeCalculator : ICalculator
    {
        private int _maxIterations;
        public EscapeTimeCalculator(int maxIterations)
        {
            _maxIterations = maxIterations;
        }

        public ICalculator.Result Calculate(double real, double imag)
        {
            int iteration = 1;
            double rsquare = 0.0;
            double isquare = 0.0;
            double zsquare = 0.0;
            double x;
            double y;

            while (rsquare + isquare <= 4.0 && iteration < _maxIterations)
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
