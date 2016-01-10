using System;
using System.Collections.Generic;

namespace ChronoChart.DataAnalysis
{
    internal static class MovingAverageSmoothingObservableExtensions
    {
        public static IObservable<int> MovingAverageSmoothing(this IObservable<int> source, int windowSize)
        {
            return new MovingAverageSmoothing(windowSize).Smooth(source);
        }

        public static IEnumerable<int> MovingAverageSmoothing(this IEnumerable<int> source, int windowSize)
        {
            return new MovingAverageSmoothing(windowSize).Smooth(source);
        }
    }
}