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

        public IObservable<int> Smooth(IObservable<int> source)
        {
            return source.SlidingWindow(windowSize).Select(values => (int) values.Average());
        }

        public IEnumerable<int> Smooth(IEnumerable<int> source)
        {
            return MovingAverage(windowSize, source.ToArray());
        }

        int[] MovingAverage(int window, int[] source)
        {
            var average = new int[source.Length];

            double sum = 0;
            for (int bar = 0; bar < window; bar++)
            {
                sum += source[bar];
            }

            average[window - 1] = (int) (sum / window);

            for (int bar = window; bar < source.Length; bar++)
            {
                average[bar] = average[bar - 1] + source[bar] / window
                               - source[bar - window] / window;
            }

            return average;
        }
    }
}