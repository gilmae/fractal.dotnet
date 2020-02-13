using System;
namespace mandelbrot.net
{
    public class EscapeTimeCalculator : ICalculator
    {
        private const double MandelbrotBailout = 4.0;
        private const double MainCardioidBounds = 0.0625;

        private int _maxIterations;
        private bool _verbose;

        public EscapeTimeCalculator(Options o)
        {
            _maxIterations = o.MaxIterations;
            _verbose = o.Verbose;
        }

        public ICalculator.Result Calculate(double real, double imag)
        {
            // Check that the point isn't in the main cardioid or the period-2 bulb
            if (Math.Pow(real + 1.0, 2.0) + Math.Pow(imag, 2.0) <= MainCardioidBounds)
            {
                if (_verbose)
                {
                    Console.WriteLine($"{real} + {imag}i is in the main cardioid.");
                }
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

            /* The Mandelbrot function is to iterate the function z(n+1) = z(n)**2 + c, where z(0) = 0 + 0i,
             * for each complex number c in the plane. The following is a slightly modified version of that
             * function intended to avoid as many floating point multiplications as possible.
             *
             * see https://en.wikipedia.org/wiki/Mandelbrot_set#Escape_time_algorithm for details,
             * and https://en.wikipedia.org/wiki/Mandelbrot_set#Optimizations for details on optimizations
             * 
             * We're also using doubles rather than complex numbers for two reasons:
             * 
             * 1. It makes the cardioid pre-calc check at the beginning of this function easier.
             * 2. It will faciliate later adoption of BigFloat
             * 
             */
            while (rsquare + isquare <= MandelbrotBailout && iteration < _maxIterations)
            {
                x = rsquare - isquare + real;
                y = zsquare - rsquare - isquare + imag;

                // If the function has encountered these coordinates before, then we will encounter them again.
                // Which means we will be in a loop, i.e. there is no escape, it's in the mandelbrot set.
                if (x == rOld && y == iOld)
                {
                    if (_verbose)
                    {
                        Console.WriteLine($"{real} + {imag}i has a detected loop.");
                    }

                    return new ICalculator.Result { Escaped = false, Iterations = _maxIterations };
                }
                
                rsquare = x * x;
                isquare = y * y;
                zsquare = (x + y) * (x + y);
                iteration += 1;

                // At the end of the length of the periodocity check, set new co-ordinates
                if (iteration%periodocity == 0)
                {
                    rOld = x;
                    iOld = y;
                }
            }

            if (_verbose)
            {
                if (iteration >= _maxIterations)
                {
                    Console.WriteLine($"{real} + {imag}i does not escape.");
                }
                else
                {
                    Console.WriteLine($"{real} + {imag}i escapes after {iteration} iterations.");
                }
            }

            return new ICalculator.Result { Escaped = iteration < _maxIterations, Iterations = iteration };
        }
    }
}
