using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChronoChart
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

            Observable
                .Timer(DateTimeOffset.Now.AddSeconds(1), TimeSpan.FromMilliseconds(100))
                .ObserveOnDispatcher()
                .Where(l => IsLoaded)
                .Subscribe(l =>
                {
                    CurrentValue = (int) Math.Floor(((ActualHeight - Mouse.GetPosition(this).Y) / ActualHeight)*100);
                });
        }
    }
}
