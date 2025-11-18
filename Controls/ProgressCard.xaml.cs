using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoyuApp.Controls
{
    public partial class ProgressCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ProgressCard), 
                new PropertyMetadata("", OnTitleChanged));

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double), typeof(ProgressCard), 
                new PropertyMetadata(0.0, OnProgressValueChanged));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(ProgressCard), 
                new PropertyMetadata("", OnStatusTextChanged));

        public static readonly DependencyProperty ProgressBrushProperty =
            DependencyProperty.Register("ProgressBrush", typeof(System.Windows.Media.Brush), typeof(ProgressCard), 
                new PropertyMetadata(null, OnProgressBrushChanged));

        public event EventHandler<TextEditEventArgs>? TextEditRequested;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public System.Windows.Media.Brush ProgressBrush
        {
            get => (System.Windows.Media.Brush)GetValue(ProgressBrushProperty);
            set => SetValue(ProgressBrushProperty, value);
        }

        public ProgressCard()
        {
            InitializeComponent();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressCard card)
            {
                card.TitleText.Text = e.NewValue?.ToString() ?? "";
            }
        }

        private static void OnProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressCard card)
            {
                card.ProgressBar.Value = (double)e.NewValue;
            }
        }

        private static void OnStatusTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressCard card)
            {
                card.StatusTextBlock.Text = e.NewValue?.ToString() ?? "";
            }
        }

        private static void OnProgressBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressCard card && e.NewValue is System.Windows.Media.Brush brush)
            {
                card.ProgressBar.Foreground = brush;
            }
        }

        private void StatusText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // 双击编辑
            {
                TextEditRequested?.Invoke(this, new TextEditEventArgs(StatusText));
                e.Handled = true;
            }
        }
    }

    public class TextEditEventArgs : EventArgs
    {
        public string OriginalText { get; }
        public string ModuleName { get; }

        public TextEditEventArgs(string originalText, string moduleName = "")
        {
            OriginalText = originalText;
            ModuleName = moduleName;
        }
    }
}