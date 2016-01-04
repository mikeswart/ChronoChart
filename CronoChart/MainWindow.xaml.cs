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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
            "CurrentValue", typeof (int), typeof (MainWindow), new PropertyMetadata(default(int)));

        public int CurrentValue
        {
            get { return (int) GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            var random = new Random((int)DateTime.Now.Ticks);

            Observable
                .Timer(DateTimeOffset.Now.AddSeconds(1), TimeSpan.FromMilliseconds(100))
                .ObserveOnDispatcher()
                .Subscribe(l =>
                {
                    if (!IsLoaded)
                    {
                        return;
                    }

                    CurrentValue = random.Next(100);
                });
        }
    }
}
