using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChronoChart
{
    public partial class CollectingChart : UserControl
    {
        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
            "CurrentValue", typeof (int), typeof (CollectingChart), new PropertyMetadata(default(int)));

        public int CurrentValue
        {
            get { return (int) GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        private readonly BlockingCollection<Tuple<TimeSpan, int>> smoothedValues;

        private readonly DateTime startTime = DateTime.Now;

        private TimeSpan ElapsedTime
        {
            get { return DateTime.Now.Subtract(startTime); }
        }

        private double ticksPerUnit = TimeSpan.FromSeconds(1).Ticks;

        public CollectingChart()
        {
            InitializeComponent();
            smoothedValues = new BlockingCollection<Tuple<TimeSpan, int>>(10000);

            Subject<TimeSpan> valueSubject = new Subject<TimeSpan>();

            //Observable
            //    .Interval(TimeSpan.FromMilliseconds(50))
            //    .ObserveOnDispatcher()
            //    .Subscribe(l =>
            //    {
            //        if (timedValues.Count == 9999)
            //        {
            //            timedValues.Dequeue();
            //        }

            //        var time = ElapsedTime;
            //        timedValues.Enqueue(new Tuple<TimeSpan, int>(time, CurrentValue));
            //        valueSubject.OnNext(time);
            //    });


            var valueStream = Observable
                .Interval(TimeSpan.FromMilliseconds(50))
                .ObserveOnDispatcher()
                .Select(_ => new Tuple<TimeSpan, int>(ElapsedTime, CurrentValue));

            //valueStream
            //    .Publish(observable =>
            //        observable.Buffer(
            //            observable.DistinctUntilChanged(tuple =>
            //                GetDisplayOffset(tuple.Item1))))
            //    .Subscribe(list => Debug.WriteLine("Received {0} items. Values: {1}", list.Count, String.Join(",", list.Select(tuple => GetDisplayOffset(tuple.Item1)).Distinct().ToArray())));

            valueStream.Subscribe(tuple =>
            {
                var offset = GetDisplayOffset(tuple.Item1);

                if (displayValues.ContainsKey(offset))
                {
                    displayValues[offset] = (displayValues[offset] + tuple.Item2)/2;
                }
                else
                {
                    displayValues[offset] = tuple.Item2;
                    valueSubject.OnNext(tuple.Item1);
                    
                    UpdateBitmap();
                }

                smoothedValues.Add(tuple);
            });

            //valueSubject.Subscribe(_ => UpdateBitmap());
        }

        Dictionary<int, int> displayValues = new Dictionary<int, int>();

        private int GetDisplayOffset(TimeSpan timeSpan)
        {
            return Convert.ToInt32(timeSpan.Ticks / ticksPerUnit);
        }

        private WriteableBitmap bitmap;
        private void CreateBitmap()
        {
            int width = (int) ActualWidth;
            int height = (int) ActualHeight;
            ticksPerUnit = ((DisplayTimeSpan.Ticks) / ActualWidth);

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            RenderSurface.Source = bitmap;
        }

        public TimeSpan DisplayTimeSpan = TimeSpan.FromSeconds(10);

        private void TimeChart_OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateBitmap();
        }

        private void UpdateBitmap()
        {
            if (bitmap == null)
            {
                return;
            }

            UpdateViewPort(TimeSpan.FromTicks(Math.Max(ElapsedTime.Subtract(DisplayTimeSpan).Ticks, 0)), DisplayTimeSpan);
 
            var times = smoothedValues
                .Select(tuple => new Point(GetXLocation(tuple.Item1), ActualHeight - ((ActualHeight / 100) * tuple.Item2)))
                .Where(point => point.X > 0 && point.X < ActualWidth);


            bitmap.Clear();
            bitmap.DrawPolylineAa(times.ToArray(), Colors.Fuchsia);
        }

        Rect viewPort = new Rect(0, 0, 100, 100);

        private void UpdateViewPort(TimeSpan startTimeOffset, TimeSpan duration)
        {
            ticksPerUnit = duration.Ticks/ActualWidth;
            viewPort = new Rect(startTimeOffset.Ticks / ticksPerUnit, 0, duration.Ticks / ticksPerUnit, ActualHeight);
        }

        private sealed class ViewPort
        {
            public Rect Rect { get; set; }
            public int TicksPerUnit { get; set; }

            public void Update(TimeSpan startTimeOffset, TimeSpan duration)
            {
            }
        }

        private double GetXLocation(TimeSpan offset)
        {
            var rawOffsetDisplayUnits = offset.Ticks/ticksPerUnit;
            return (rawOffsetDisplayUnits - viewPort.Left);
        }

        private void TimeChart_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateBitmap();
        }
    }

    public static class BitmapExtensions
    {
        public static void DrawPolylineAa(this WriteableBitmap bmp, Point[] points, Color color)
        {
            if (points.Length == 0)
            {
                return;
            }

            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!;
                var point = points[0];

                for (var i = 1; i < points.Length; i ++)
                {
                    var nextPoint = points[i];

                    bmp.DrawLine((int) point.X, (int) point.Y, (int) nextPoint.X, (int) nextPoint.Y, color);
                    bmp.FillEllipseCentered((int)nextPoint.X, (int)nextPoint.Y, 1, 1, color);
                    //bmp.DrawLine((int) nextPoint.X,0,(int) nextPoint.X, 100, color);
                    point = nextPoint;
                }
            }
        }

        public static void DrawPolylineAa(this WriteableBitmap bmp, int[] points, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                var w = context.Width;
                var h = context.Height;
                var x1 = points[0];
                var y1 = points[1];

                for (var i = 2; i < points.Length; i += 2)
                {
                    var x2 = points[i];
                    var y2 = points[i + 1];

                    bmp.DrawLine(x1, y1, x2, y2, color);
                    x1 = x2;
                    y1 = y2;
                }
            }
        }
    }

    public sealed class ChartingDataSet<T>
    {
        private readonly Subject<T> changedSubject;

        public IObservable<T> Changed
        {
            get { return changedSubject.AsObservable(); }
        }

        public ChartingDataSet()
        {
            changedSubject = new Subject<T>();
            BlockingCollection<T> dataset = new BlockingCollection<T>();
        }
    }
}
