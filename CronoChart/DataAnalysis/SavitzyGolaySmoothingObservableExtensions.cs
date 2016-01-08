using System;

namespace ChronoChart.DataAnalysis
{
    internal static class SavitzyGolaySmoothingObservableExtensions
    {
        public static IObservable<int> SavitzyGolaySmoothing(this IObservable<int> source, SavitzyGolaySmoothing.IFilterParameters _filterParameters)
        {
            return new SavitzyGolaySmoothing(_filterParameters).Smooth(source);
        }
    }
}