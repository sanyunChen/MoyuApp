using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoyuApp.Controls
{
    public partial class CountdownCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CountdownCard), 
                new PropertyMetadata("", OnTitleChanged));

        public static readonly DependencyProperty CountdownTextProperty =
            DependencyProperty.Register("CountdownText", typeof(string), typeof(CountdownCard), 
                new PropertyMetadata("", OnCountdownTextChanged));

        public event EventHandler<TextEditEventArgs>? TextEditRequested;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string CountdownText
        {
            get => (string)GetValue(CountdownTextProperty);
            set => SetValue(CountdownTextProperty, value);
        }

        public CountdownCard()
        {
            InitializeComponent();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CountdownCard card)
            {
                card.TitleText.Text = e.NewValue?.ToString() ?? "";
            }
        }

        private static void OnCountdownTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CountdownCard card)
            {
                card.CountdownTextBlock.Text = e.NewValue?.ToString() ?? "";
            }
        }

        private void CountdownText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // 双击编辑
            {
                TextEditRequested?.Invoke(this, new TextEditEventArgs(CountdownText));
                e.Handled = true;
            }
        }
    }
}