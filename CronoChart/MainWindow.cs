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

        public static readonly DependencyProperty RunningProperty = DependencyProperty.Register(
            "Running", typeof (bool), typeof (MainWindow), new PropertyMetadata(true));

        public bool Running
        {
            get { return (bool) GetValue(RunningProperty); }
            set { SetValue(RunningProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            Observable
                .Timer(DateTimeOffset.Now.AddSeconds(1), TimeSpan.FromMilliseconds(50))
                .ObserveOnDispatcher()
                .Where(l => IsLoaded)
                .Subscribe(l =>
                {
                    CurrentValue = (int) Math.Floor(((ActualHeight - Mouse.GetPosition(this).Y) / ActualHeight)*100);
                });
        }

        private void PauseClicked(object sender, RoutedEventArgs e)
        {
            Running = !Running;
        }
    }
}
