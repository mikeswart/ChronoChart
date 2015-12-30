using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CronoChart
{
    /// <summary>
    /// Interaction logic for TimeChart.xaml
    /// </summary>
    public partial class TimeChart : UserControl
    {

        public TimeChart()
        {
            InitializeComponent();

            var valueQueue = new Queue<int>(100);
            valueQueue.Enqueue(100);

            Observable.Timer(DateTimeOffset.Now.AddSeconds(1), TimeSpan.FromMilliseconds(10)).ObserveOnDispatcher().Subscribe(
                l =>
                {
                    if (!IsLoaded)
                    {
                        return;
                    }

                    var random = new Random((int)DateTime.Now.Ticks);

                    if (valueQueue.Count == 99)
                    {
                        valueQueue.Dequeue();
                    }

                    var item = valueQueue.Last() + (random.Next(10) - 5);
                    item = (int) Math.Max(item, 0);
                    item = (int) Math.Min(item, ActualHeight);
                    valueQueue.Enqueue(item);
                    var points = valueQueue.ToList().Select((t, index) => new Point(100- index, t)).ToList();

                    Polyline.Points = new PointCollection(points);
                });
        }
    }
}
