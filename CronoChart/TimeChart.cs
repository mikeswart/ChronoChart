using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ChronoChart.DataAnalysis;

namespace ChronoChart
{
    public partial class TimeChart : UserControl
    {
        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
            "CurrentValue", typeof (int), typeof (TimeChart), new PropertyMetadata(default(int)));

        public int CurrentValue
        {
            get { return (int) GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        private readonly Queue<Tuple<TimeSpan, int>> timedValues;
        private readonly Queue<Tuple<TimeSpan, int>> smoothedValues;

        private readonly DateTime startTime = DateTime.Now;

        private TimeSpan ElapsedTime
        {
            get { return DateTime.Now.Subtract(startTime); }
        }

        public TimeChart()
        {
            InitializeComponent();

            timedValues = new Queue<Tuple<TimeSpan, int>>(10000);
            smoothedValues = new Queue<Tuple<TimeSpan, int>>(10000);

            Observable.Interval(TimeSpan.FromMilliseconds(250))
                .ObserveOnDispatcher()
                .Select(_ => CurrentValue)
                .SavitzyGolaySmoothing(SavitzyGolaySmoothing.FilterParametersSize5)
                .Select(smoothedValue => new
                {
                    ElapsedTime,
                    Value = smoothedValue
                })
                .Subscribe(l =>
                {
                    if (smoothedValues.Count == 9999)
                    {
                        smoothedValues.Dequeue();
                    }

                    smoothedValues.Enqueue(new Tuple<TimeSpan, int>(l.ElapsedTime, l.Value));
                });

            /// Moving Average
            //Observable.Interval(TimeSpan.FromMilliseconds(250))
            //    .Select(_ => CurrentValue)
            //    .SlidingWindow(4)
            //    .Select(windowValues => new
            //    {
            //        ElapsedTime,
            //        Value = (int)windowValues.Average()
            //    })
            //    .Subscribe(l =>
            //    {
            //        if (smoothedValues.Count == 9999)
            //        {
            //            smoothedValues.Dequeue();
            //        }

            //        smoothedValues.Enqueue(new Tuple<TimeSpan, int>(l.ElapsedTime, l.Value));
            //    });

            Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .ObserveOnDispatcher()
                .Subscribe(l =>
                {
                    if (timedValues.Count == 9999)
                    {
                        timedValues.Dequeue();
                    }

                    timedValues.Enqueue(new Tuple<TimeSpan, int>(ElapsedTime, CurrentValue));
                    InvalidateVisual();
                });

            rawPen = new Pen(Brushes.LightGray, 1);
            rawPen.Freeze();

            smoothPen = new Pen(Brushes.Black, 2);
            smoothPen.Freeze();
        }



        private Pen rawPen;
        private Pen smoothPen;

        protected override void OnRender(DrawingContext drawingContext)
        {
            var sw = new Stopwatch();
            sw.Start();

            var minimumShownTime = ElapsedTime.Subtract(DisplayTimeSpan);
            var ticksPerUnit = (DisplayTimeSpan.Ticks/ActualWidth);

            var times = timedValues
                .Where(tuple => tuple.Item1 > minimumShownTime)
                .GroupBy(tuple => Math.Floor(tuple.Item1.Ticks / ticksPerUnit))
                .Select(tuples => new
                {
                    Unit = ActualWidth - tuples.Key,
                    Value = tuples.Average(tuple => tuple.Item2)
                })
                .Select(t => new Point(t.Unit, ActualHeight - ((ActualHeight/100)*t.Value)))
                .ToList();

            for (var i = 1; i < times.Count; i++)
            {
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(times[i - 1], false, false);
                    context.LineTo(times[i], true, false);
                }

                geometry.Freeze();
                drawingContext.DrawGeometry(null, rawPen, geometry);
            }

            var smoothedTimes = smoothedValues
                .Where(tuple => tuple.Item1 > minimumShownTime)
                .GroupBy(tuple => Math.Floor(tuple.Item1.Ticks / ticksPerUnit))
                .Select(tuples => new
                {
                    Unit = ActualWidth - tuples.Key,
                    Value = tuples.Average(tuple => tuple.Item2)
                })
                .Select(t => new Point(t.Unit, ActualHeight - ((ActualHeight / 100) * t.Value)))
                .ToList();

            for (var i = 1; i < smoothedTimes.Count; i++)
            {
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(smoothedTimes[i - 1], false, false);
                    context.LineTo(smoothedTimes[i], true, false);
                }

                geometry.Freeze();
                drawingContext.DrawGeometry(null, smoothPen, geometry);
            }
            sw.Stop();

            Debug.WriteLine("Reduced " + timedValues .Count + " Rendering " + times.Count + " lines took: " + sw.ElapsedMilliseconds);
        }

        private double TimeToLocation(TimeSpan timeOffset)
        {
            return ActualWidth - ((ActualWidth / DisplayTimeSpan.Ticks) * timeOffset.Ticks);
        }

        private double TimeOffsetToLocation(TimeSpan timeOffset)
        {
            var timeOffsetToLocation = ActualWidth - ((ActualWidth / DisplayTimeSpan.Ticks) * (ElapsedTime.Subtract(timeOffset)).Ticks);
            return timeOffsetToLocation;
        }

        public TimeSpan DisplayTimeSpan = TimeSpan.FromMinutes(5);

        private Line CreateTimeTick(TimeSpan timeOffset)
        {
            var x = TimeToLocation(timeOffset);
            return new Line
            {
                X1 = x,
                X2 = x,
                Y1 = ActualHeight,
                Y2 = ActualHeight - 10,
                Stroke = Brushes.Fuchsia,
                StrokeThickness = 5
            };
        }


    }
}
