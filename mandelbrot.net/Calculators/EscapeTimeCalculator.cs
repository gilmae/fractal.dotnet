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
            // Check that the point isn't in the main cardioid or the period-2 bulb
            if (Math.Pow(real + 1.0, 2.0) + Math.Pow(imag, 2.0) <= MainCardioidBounds)
            {
                return new ICalculator.Result { Escaped = false, Iterations = int.MaxValue };
            }

            double rOld = double.MaxValue;
            double iOld = double.MaxValue;
            double periodocity = 20.0;
            
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

                // If the function has encountered these coordinates before, then we will encounter them again.
                // Which means we will be in a loop, i.e. there is no escape, it's in the mandelbrot set.
                if (x == rOld && y == iOld)
                {
                    return new ICalculator.Result { Escaped = false, Iterations = _maxIterations };
                }
                rsquare = x * x;
                isquare = y * y;
                zsquare = (x + y) * (x + y);
                iteration += 1;

                if (iteration%periodocity == 0)
                {
                    rOld = rsquare;
                    iOld = isquare;
                }
            }

            return new ICalculator.Result { Escaped = iteration < _maxIterations, Iterations = iteration };
        }
    }
}
