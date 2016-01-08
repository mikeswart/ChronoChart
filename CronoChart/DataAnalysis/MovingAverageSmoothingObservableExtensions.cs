using System;

namespace ChronoChart.DataAnalysis
{
    internal static class MovingAverageSmoothingObservableExtensions
    {
        public static IObservable<int> MovingAverageSmoothing(this IObservable<int> source, int windowSize)
        {
            return new MovingAverageSmoothing(windowSize).Smooth(source);
        }
    }
}