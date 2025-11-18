using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MoyuApp.Models;

namespace MoyuApp.Services
{
    public class EditService
    {
        private readonly Window _parentWindow;
        private readonly Grid _overlayGrid;
        private readonly Border _editPanel;
        private readonly TextBox _editTextBox;
        private readonly TextBlock _titleTextBlock;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public event EventHandler<EditCompletedEventArgs>? EditCompleted;

        public EditService(Window parentWindow)
        {
            _parentWindow = parentWindow;
            
            // 创建编辑覆盖层
            _overlayGrid = new Grid
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0)),
                Visibility = Visibility.Collapsed
            };

            // 创建编辑面板
            _editPanel = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20),
                Width = 400,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    ShadowDepth = 10,
                    BlurRadius = 20,
                    Color = System.Windows.Media.Color.FromArgb(100, 0, 0, 0)
                }
            };

            var stackPanel = new StackPanel();
            
            _titleTextBlock = new TextBlock
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            };

            _editTextBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                MaxHeight = 100,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            _saveButton = new Button
            {
                Content = "保存",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(15, 8, 15, 8),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 126, 179)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };

            _cancelButton = new Button
            {
                Content = "取消",
                Padding = new Thickness(15, 8, 15, 8),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 126, 179)),
                BorderThickness = new Thickness(1)
            };

            buttonPanel.Children.Add(_saveButton);
            buttonPanel.Children.Add(_cancelButton);

            stackPanel.Children.Add(_titleTextBlock);
            stackPanel.Children.Add(_editTextBox);
            stackPanel.Children.Add(buttonPanel);

            _editPanel.Child = stackPanel;
            _overlayGrid.Children.Add(_editPanel);

            // 添加事件处理
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
            _overlayGrid.MouseLeftButtonDown += OverlayGrid_MouseLeftButtonDown;
            _editPanel.MouseLeftButtonDown += EditPanel_MouseLeftButtonDown;
            _editTextBox.KeyDown += EditTextBox_KeyDown;

            // 添加到父窗口
            var mainGrid = _parentWindow.Content as Grid;
            if (mainGrid != null)
            {
                mainGrid.Children.Add(_overlayGrid);
                Grid.SetRowSpan(_overlayGrid, int.MaxValue);
                Grid.SetColumnSpan(_overlayGrid, int.MaxValue);
            }
        }

        public void ShowEditDialog(string title, string originalText, string moduleType = "")
        {
            _titleTextBlock.Text = title;
            _editTextBox.Text = originalText;
            _overlayGrid.Visibility = Visibility.Visible;
            
            // 动画效果
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            _overlayGrid.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            
            _editTextBox.Focus();
            _editTextBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            CommitEdit();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
        }

        private void OverlayGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 点击遮罩层关闭编辑
            CancelEdit();
        }

        private void EditPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 阻止事件冒泡
            e.Handled = true;
        }

        private void EditTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    if (Keyboard.Modifiers != System.Windows.Input.ModifierKeys.Control)
                    {
                        CommitEdit();
                        e.Handled = true;
                    }
                    break;
                case System.Windows.Input.Key.Escape:
                    CancelEdit();
                    e.Handled = true;
                    break;
            }
        }

        private void CommitEdit()
        {
            var newText = _editTextBox.Text;
            HideEditDialog();
            EditCompleted?.Invoke(this, new EditCompletedEventArgs(newText, true));
        }

        private void CancelEdit()
        {
            HideEditDialog();
            EditCompleted?.Invoke(this, new EditCompletedEventArgs(string.Empty, false));
        }

        private void HideEditDialog()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) => _overlayGrid.Visibility = Visibility.Collapsed;
            _overlayGrid.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    public class EditCompletedEventArgs : EventArgs
    {
        public string NewText { get; }
        public bool IsCommitted { get; }

        public EditCompletedEventArgs(string newText, bool isCommitted)
        {
            NewText = newText;
            IsCommitted = isCommitted;
        }
    }
}