using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ChronoChart
{
    public static class ObservableExtensions
    {
        public static IObservable<IList<T>> SlidingWindow<T>(this IObservable<T> source, int windowSize)
        {
            var feed = source.Publish().RefCount();
            return Observable
                .Zip(Enumerable
                    .Range(0, windowSize)
                    .Select(i => feed.Skip(i))
                    .ToArray());
        }
    }
}