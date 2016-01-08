using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ChronoChart.DataAnalysis
{
    internal sealed class MovingAverageSmoothing
    {
        private readonly int windowSize;

        public MovingAverageSmoothing(int windowSize)
        {
            this.windowSize = windowSize;
        }

        public int Smooth(IReadOnlyList<int> input)
        {
            return (int) input.Average();
        }

        public IObservable<int> Smooth(IObservable<int> source)
        {
            return source.SlidingWindow(windowSize).Select(values => Smooth(values.ToArray()));
        }
    }
}