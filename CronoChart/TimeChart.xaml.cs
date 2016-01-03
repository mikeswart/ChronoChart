using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            timedValues = new Queue<Tuple<TimeSpan, int>>(1000);

            var random = new Random((int)DateTime.Now.Ticks);

            Observable
                .Timer(DateTimeOffset.Now.AddSeconds(1), TimeSpan.FromMilliseconds(10))
                .ObserveOnDispatcher()
                .Subscribe(l =>
                {
                    if (!IsLoaded)
                    {
                        return;
                    }

                    CurrentValue = random.Next(100);
                });

            Observable
                .Interval(TimeSpan.FromMilliseconds(1000))
                .ObserveOnDispatcher()
                .Subscribe(l =>
                {
                    if (timedValues.Count == 999)
                    {
                        timedValues.Dequeue();
                    }

                    timedValues.Enqueue(new Tuple<TimeSpan, int>(ElapsedTime, CurrentValue));
                    CreateGraph();
                });
        }

        private void CreateGraph()
        {
            Canvas.Children.Clear();

            var tresholdTime = ElapsedTime.Subtract(DisplayTimeSpan);
            var polyLine = new Polyline
            {
                Stroke = Brushes.Fuchsia,
                StrokeThickness = 1,
                Points = new PointCollection(
                    timedValues
                    .ToList()
                    .Where(tuple => tuple.Item1 > tresholdTime)
                    .Select((t, index) => new Point(TimeOffsetToLocation(t.Item1), t.Item2))
                    .ToList())
            };

            Canvas.Children.Add(polyLine);

            Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(5)));
            Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(7)));
            Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(9)));
            Canvas.Children.Add(CreateTimeTick(TimeSpan.FromSeconds(10)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //CreateGraph();

            base.OnRender(drawingContext);
        }

        private double TimeToLocation(TimeSpan timeOffset)
        {
            return ActualWidth - ((ActualWidth / DisplayTimeSpan.Ticks) * timeOffset.Ticks);
        }

        private double TimeOffsetToLocation(TimeSpan timeOffset)
        {
            return ActualWidth - ((ActualWidth / DisplayTimeSpan.Ticks) * (ElapsedTime.Subtract(timeOffset)) .Ticks);
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
