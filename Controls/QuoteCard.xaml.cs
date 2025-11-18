using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoyuApp.Controls
{
    public partial class QuoteCard : UserControl
    {
        public static readonly DependencyProperty QuoteTextProperty =
            DependencyProperty.Register("QuoteText", typeof(string), typeof(QuoteCard), 
                new PropertyMetadata("", OnQuoteTextChanged));

        public event EventHandler<TextEditEventArgs>? TextEditRequested;

        public string QuoteText
        {
            get => (string)GetValue(QuoteTextProperty);
            set => SetValue(QuoteTextProperty, value);
        }

        public QuoteCard()
        {
            InitializeComponent();
        }

        private static void OnQuoteTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is QuoteCard card)
            {
                card.QuoteTextBlock.Text = e.NewValue?.ToString() ?? "";
            }
        }

        private void QuoteText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // 双击编辑
            {
                TextEditRequested?.Invoke(this, new TextEditEventArgs(QuoteText));
                e.Handled = true;
            }
        }
    }
}