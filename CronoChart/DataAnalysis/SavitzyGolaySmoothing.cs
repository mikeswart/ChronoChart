using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ChronoChart.DataAnalysis
{
    internal sealed class SavitzyGolaySmoothing
    {
        // For more coeffecients, see http://www.vias.org/tmdatanaleng/cc_savgol_coeff.html
        
        public static readonly IFilterParameters FilterParametersSize5 = new FilterParameters(5, 35, new[]
        {
            17,
            12,
            -3
        });

        public static readonly IFilterParameters FilterParametersSize7 = new FilterParameters(7, 21, new[]
        {
            7,
            6,
            3,
            -2
        });

        public static readonly IFilterParameters FilterParametersSize9 = new FilterParameters(9, 231, new[]
        {
            59,
            54,
            39,
            14,
            -21
        });

        private readonly IFilterParameters filterParameters;
        private readonly IEnumerable<int> computedCoefficients;

        public SavitzyGolaySmoothing(IFilterParameters filterParameters)
        {
            this.filterParameters = filterParameters;
            computedCoefficients = this.filterParameters.Coefficients.Reverse().Concat(this.filterParameters.Coefficients.Skip(1)).ToArray();
        }

        public int Smooth(IReadOnlyList<int> input)
        {
            return 
                (int) (computedCoefficients
                    .Select((coefficient, index) => input[index]*coefficient)
                    .Sum() / (double)filterParameters.Normalization);
        }

        public IObservable<int> Smooth(IObservable<int> source)
        {
            return source
                .SlidingWindow(filterParameters.WindowSize)
                .Select(values => Smooth(values.ToArray()));
        }

        public interface IFilterParameters
        {
            int WindowSize { get; }
            int Normalization { get; }
            int[] Coefficients { get; }
        }

        private class FilterParameters : IFilterParameters
        {
            public int WindowSize { get; private set; }
            public int Normalization { get; private set; }
            public int[] Coefficients { get; private set; }

            public FilterParameters(int windowSize, int normalization, int[] coefficients)
            {
                WindowSize = windowSize;
                Normalization = normalization;
                Coefficients = coefficients;
            }
        }
    }
}