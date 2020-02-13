using System.Threading.Channels;
using System.Threading.Tasks;

namespace mandelbrot.net
{
    public class Plotter
    {
        private ChannelReader<Point> _points;
        private ChannelWriter<Point> _channelWriter;
        private ICalculator _calculator;

        public Plotter(ChannelReader<Point> points, ChannelWriter<Point> channelWriter, ICalculator calculator)
        {
            _points = points;
            _channelWriter = channelWriter;
            _calculator = calculator;
        }

        public async Task GetPlotter()
        {
            while (await _points.WaitToReadAsync())
            {
                while (_points.TryRead(out Point p))
                {
                    _channelWriter.TryWrite(Plot(p));
                }
            }
        }

        public Point Plot(Point p)
        {
            var result = _calculator.Calculate(p.real, p.imag);
            p.escaped = result.Escaped.Value;
            p.iterations = result.Iterations;
            return p;

        }
    }
}
