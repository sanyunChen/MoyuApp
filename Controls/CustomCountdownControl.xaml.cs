using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MoyuApp.Models;

namespace MoyuApp.Controls
{
    public partial class CustomCountdownControl : UserControl
    {
        public static readonly DependencyProperty CountdownsProperty =
            DependencyProperty.Register("Countdowns", typeof(ObservableCollection<CustomCountdown>), 
                typeof(CustomCountdownControl), new PropertyMetadata(null, OnCountdownsChanged));

        public ObservableCollection<CustomCountdown> Countdowns
        {
            get => (ObservableCollection<CustomCountdown>)GetValue(CountdownsProperty);
            set => SetValue(CountdownsProperty, value);
        }

        public event EventHandler<CustomCountdownEventArgs>? CountdownEditRequested;
        public event EventHandler? AddCountdownRequested;

        public CustomCountdownControl()
        {
            InitializeComponent();
        }

        private static void OnCountdownsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomCountdownControl control)
            {
                control.CountdownListBox.ItemsSource = e.NewValue as ObservableCollection<CustomCountdown>;
            }
        }

        private void CountdownListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CountdownListBox.SelectedItem is CustomCountdown countdown)
            {
                CountdownEditRequested?.Invoke(this, new CustomCountdownEventArgs(countdown));
                e.Handled = true;
            }
        }

        private void AddCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            AddCountdownRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CustomCountdownEventArgs : EventArgs
    {
        public CustomCountdown Countdown { get; }

        public CustomCountdownEventArgs(CustomCountdown countdown)
        {
            Countdown = countdown;
        }
    }
}