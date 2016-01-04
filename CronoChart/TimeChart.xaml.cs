using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CronoChart
{
    /// <summary>
    /// Interaction logic for TimeChart.xaml
    /// </summary>
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

        private readonly DateTime startTime = DateTime.Now;

        private TimeSpan ElapsedTime
        {
            get { return DateTime.Now.Subtract(startTime); }
        }

        public TimeChart()
        {
            InitializeComponent();

            timedValues = new Queue<Tuple<TimeSpan, int>>(10000);

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
                    CreateGraph();
                    InvalidateVisual();
                });

            pen = new Pen(Brushes.Fuchsia, 1);
            pen.Freeze();
        }

        private Pen pen;
        private void CreateGraph()
        {
            //Canvas.Children.Clear();

            var tresholdTime = ElapsedTime.Subtract(DisplayTimeSpan);

            //var ticksPerUnit = DisplayTimeSpan.Ticks/ActualWidth;
            //var times = timedValues
            //    .Where(tuple => tuple.Item1 > tresholdTime)
            //    //.GroupBy(tuple => Math.Floor(TimeOffsetToLocation(tuple.Item1))) 
            //    .GroupBy(tuple => Math.Floor(tuple.Item1.TotalSeconds)) 
            //    .Select(tuples => new {Unit = TimeOffsetToLocation(TimeSpan.FromSeconds(tuples.Key)), Value = tuples.Average(tuple => tuple.Item2)});

            //Polyline.Points = new PointCollection(
            //    times
            //        .Select(t => new Point(t.Unit, ActualHeight - ((ActualHeight / 100) * t.Value)))
            //        .ToList());



            //var random = new Random();
            //Polyline.Points = new PointCollection(Enumerable.Range(0, (int) ActualWidth).Select(t => new Point(t,random.Next((int) ActualHeight))));



            //Canvas.Children.Add(polyLine);

            //Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(5)));
            //Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(7)));
            //Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(9)));
            //Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(10)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //var random = new Random();
            //var points = Enumerable.Range(0, (int)ActualWidth).Select(t => new Point(t, random.Next((int)ActualHeight))).ToList();

            //for (int i = 1; i < points.Count; i++)
            //{
            //    var geometry = new StreamGeometry();
            //    using (var context = geometry.Open())
            //    {
            //        context.BeginFigure(points[i - 1], false, false);
            //        context.LineTo(points[i], true, false);
            //    }

            //    geometry.Freeze();
            //    drawingContext.DrawGeometry(null, pen, geometry);
            //}

            var tresholdTime = ElapsedTime.Subtract(DisplayTimeSpan);
            var times = timedValues
                .Where(tuple => tuple.Item1 > tresholdTime)
                .GroupBy(tuple => Math.Floor(tuple.Item1.TotalSeconds))
                .Select(tuples => new { Unit = TimeOffsetToLocation(TimeSpan.FromSeconds(tuples.Key)), Value = tuples.Average(tuple => tuple.Item2) })
                .Select(t => new Point(t.Unit, ActualHeight - ((ActualHeight / 100) * t.Value)))
                .ToList();

            for (int i = 1; i < times.Count; i++)
            {
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(times[i - 1], false, false);
                    context.LineTo(times[i], true, false);
                }

                geometry.Freeze();
                drawingContext.DrawGeometry(null, pen, geometry);
            }


            //if (times.Count == 0)
            //{
            //    return;
            //}

            //var geometry = new StreamGeometry();
            //using (var context = geometry.Open())
            //{
            //    context.BeginFigure(times[0], false, false);
            //    times.Skip(1).ToList().ForEach(point => context.LineTo(point, true, false));
            //}

            //geometry.Freeze();
            //drawingContext.DrawGeometry(null, pen, geometry);
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
