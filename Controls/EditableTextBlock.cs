using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoyuApp.Controls
{
    public class EditableTextBlock : TextBox
    {
        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(EditableTextBlock),
                new PropertyMetadata(false, OnEditModeChanged));

        public static readonly DependencyProperty OriginalTextProperty =
            DependencyProperty.Register("OriginalText", typeof(string), typeof(EditableTextBlock),
                new PropertyMetadata(string.Empty));

        public bool IsInEditMode
        {
            get => (bool)GetValue(IsInEditModeProperty);
            set => SetValue(IsInEditModeProperty, value);
        }

        public string OriginalText
        {
            get => (string)GetValue(OriginalTextProperty);
            set => SetValue(OriginalTextProperty, value);
        }

        public event EventHandler<TextEditCompletedEventArgs>? EditCompleted;

        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }

        public EditableTextBlock()
        {
            BorderThickness = new Thickness(0);
            Background = System.Windows.Media.Brushes.Transparent;
            IsReadOnly = true;
            Cursor = Cursors.Hand;
            
            MouseDoubleClick += OnMouseDoubleClick;
            LostFocus += OnLostFocus;
            KeyDown += OnKeyDown;
        }

        private static void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock textBlock)
            {
                textBlock.UpdateEditMode();
            }
        }

        private void UpdateEditMode()
        {
            if (IsInEditMode)
            {
                // 进入编辑模式
                IsReadOnly = false;
                BorderThickness = new Thickness(1);
                Background = System.Windows.Media.Brushes.White;
                Cursor = Cursors.IBeam;
                Focus();
                SelectAll();
            }
            else
            {
                // 退出编辑模式
                IsReadOnly = true;
                BorderThickness = new Thickness(0);
                Background = System.Windows.Media.Brushes.Transparent;
                Cursor = Cursors.Hand;
            }
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsInEditMode)
            {
                EnterEditMode();
                e.Handled = true;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsInEditMode)
            {
                CommitEdit();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (IsInEditMode)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        CommitEdit();
                        e.Handled = true;
                        break;
                    case Key.Escape:
                        CancelEdit();
                        e.Handled = true;
                        break;
                }
            }
        }

        public void EnterEditMode()
        {
            OriginalText = Text;
            IsInEditMode = true;
        }

        public void CommitEdit()
        {
            if (IsInEditMode)
            {
                IsInEditMode = false;
                EditCompleted?.Invoke(this, new TextEditCompletedEventArgs(Text, OriginalText, true));
            }
        }

        public void CancelEdit()
        {
            if (IsInEditMode)
            {
                Text = OriginalText;
                IsInEditMode = false;
                EditCompleted?.Invoke(this, new TextEditCompletedEventArgs(Text, OriginalText, false));
            }
        }
    }

    public class TextEditCompletedEventArgs : EventArgs
    {
        public string NewText { get; }
        public string OriginalText { get; }
        public bool IsCommitted { get; }

        public TextEditCompletedEventArgs(string newText, string originalText, bool isCommitted)
        {
            NewText = newText;
            OriginalText = originalText;
            IsCommitted = isCommitted;
        }
    }
}