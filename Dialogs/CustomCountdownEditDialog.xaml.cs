using System;
using System.Windows;
using MoyuApp.Models;

namespace MoyuApp.Dialogs
{
    public partial class CustomCountdownEditDialog : Window
    {
        private readonly CustomCountdown _originalCountdown;

        public string CountdownName => NameTextBox.Text;
        public DateTime CountdownDate => DatePicker.SelectedDate ?? DateTime.Now;

        public CustomCountdownEditDialog(CustomCountdown countdown)
        {
            InitializeComponent();
            _originalCountdown = countdown;
            
            // 初始化数据
            NameTextBox.Text = countdown.Name;
            DatePicker.SelectedDate = countdown.Date;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("请输入倒计时名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("请选择目标日期", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}