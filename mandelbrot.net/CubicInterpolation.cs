using System;
using System.Collections.Generic;
using MathNet.Numerics.Interpolation;

namespace mandelbrot.net
{

    public class CubicInterpolation
    {
        private CubicSpline _interpolator;

        public CubicInterpolation(double[] xPoints, double[] yPoints)
        {
            _interpolator = CubicSpline.InterpolateNaturalSorted(xPoints, yPoints);
        }

        public double GetYPoint(double xPoint)
        {
            double y = _interpolator.Interpolate(xPoint);
            if (y > 255)
            {
                return 255;
            }
            if (y < 0)
            {
                return 0;
            }
            return y;
        }
    }
}

