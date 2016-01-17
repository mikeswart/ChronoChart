using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public static readonly DependencyProperty RunningProperty = DependencyProperty.Register(
            "Running", typeof (bool), typeof (TimeChart), new PropertyMetadata(true));

        public bool Running
        {
            get { return (bool) GetValue(RunningProperty); }
            set { SetValue(RunningProperty, value); }
        }

        private readonly Pen rawPen;
        private readonly Queue<Tuple<TimeSpan, int>> timedValues;

        public TimeSpan DisplayPeriod = TimeSpan.FromMinutes(5);

        public TimeChart()
        {
            InitializeComponent();

            timedValues = new Queue<Tuple<TimeSpan, int>>();

            TimeSpan interval = TimeSpan.FromMilliseconds(250);
            long observedTicks = 0;
            Observable
                .Interval(interval)
                .ObserveOnDispatcher()
                .Where(l => Running)
                .Subscribe(l =>
                {
                    if (timedValues.Count == 9999)
                    {
                        timedValues.Dequeue();
                    }

                    timedValues.Enqueue(new Tuple<TimeSpan, int>(TimeSpan.FromTicks(observedTicks++ * interval.Ticks), CurrentValue));
                    InvalidateVisual();
                });

            rawPen = new Pen(Brushes.Tomato, 1);
            rawPen.Freeze();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (timedValues.Count == 0)
            {
                return;
            }

            var minimumShownTime = timedValues.Last().Item1.Subtract(DisplayPeriod);
            
            var ticksPerUnit = (DisplayPeriod.Ticks / ActualWidth);
            var maximumShownTime = timedValues.Last().Item1.Ticks/ticksPerUnit;
            // Reduce the collected points into one data point per pixel
            var times = timedValues
                .Where(value => value.Item1 > minimumShownTime)
                .Select(t => new Point(ActualWidth - ((( t.Item1.Ticks) / ticksPerUnit) -maximumShownTime) , ActualHeight - ((ActualHeight / 100) * t.Item2)))
                .GroupBy(point => point.X)
                .Select(points => new Point(points.Key, points.Select(point => point.Y).Average()))
                .ToList();

            if (times.Count == 0)
            {
                return;
            }
            sw.Stop();

            Debug.WriteLine("Reducing - " + sw.ElapsedMilliseconds);
            sw.Restart();
            //var smoothedItems = new MovingAverageSmoothing(5).Smooth(times.Select(point => (int)point.Y));
            //var smoothedTimeItems = times.Select(point => point.X).Zip(smoothedItems, (d, i) => new Point(d, i)).ToList();

            var smoothedTimeItems = times;
            for (var i = 1; i < times.Count; i++)
            {
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(smoothedTimeItems[i - 1], false, false);
                    context.LineTo(smoothedTimeItems[i], true, false);
                }

                geometry.Freeze();
                drawingContext.DrawGeometry(null, rawPen, geometry);
            }
            sw.Stop();
            Debug.WriteLine("Drawing - " + sw.ElapsedMilliseconds);
        } 
    }
}
