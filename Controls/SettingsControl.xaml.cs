using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MoyuApp.Models;
using MoyuApp.Services;
using MoyuApp.Dialogs;

namespace MoyuApp.Controls
{
    public partial class SettingsControl : UserControl
    {
        private AppConfig _originalConfig;
        private AppConfig _currentConfig;
        private readonly ConfigService _configService;
        private readonly DataBackupService _backupService;

        public event EventHandler? SettingsApplied;
        public event EventHandler? SettingsSaved;
        public event EventHandler? SettingsCancelled;

        public SettingsControl()
        {
            InitializeComponent();
            _configService = new ConfigService();
            _backupService = new DataBackupService(_configService);
            LoadSettings();
        }

        private async void LoadSettings()
        {
            try
            {
                _originalConfig = await _configService.LoadConfigAsync();
                _currentConfig = CloneConfig(_originalConfig);
                
                BindConfigToUI(_currentConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载设置失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindConfigToUI(AppConfig config)
        {
            // 工作时间
            StartTimeTextBox.Text = config.StartTime;
            EndTimeTextBox.Text = config.EndTime;
            
            // 个人信息
            HireDatePicker.SelectedDate = config.HireDate;
            GenderComboBox.SelectedIndex = config.Gender == "male" ? 0 : 1;
            
            // 发薪设置
            SalaryDayTextBox.Text = config.SalaryDay.ToString();
            WeekdaysTextBox.Text = config.Weekdays;
            
            // 语录设置
            ActiveStartTimeTextBox.Text = config.ActiveStartTime;
            ActiveEndTimeTextBox.Text = config.ActiveEndTime;
            
            // 外观设置
            RefreshIntervalTextBox.Text = config.RefreshInterval.ToString();
            DarkModeToggle.IsChecked = config.DarkMode;
            
            // 自定义倒计时
            RefreshCustomCountdownList();
        }

        private void RefreshCustomCountdownList()
        {
            CustomCountdownListBox.ItemsSource = _currentConfig.CustomCountdowns;
        }

        private AppConfig CloneConfig(AppConfig original)
        {
            return new AppConfig
            {
                StartTime = original.StartTime,
                EndTime = original.EndTime,
                HireDate = original.HireDate,
                Gender = original.Gender,
                RefreshInterval = original.RefreshInterval,
                DarkMode = original.DarkMode,
                ActiveStartTime = original.ActiveStartTime,
                ActiveEndTime = original.ActiveEndTime,
                SalaryDay = original.SalaryDay,
                Weekdays = original.Weekdays,
                CustomCountdowns = new System.ComponentModel.BindingList<CustomCountdown>(
                    original.CustomCountdowns.ToList())
            };
        }

        private bool ValidateConfig()
        {
            // 验证时间格式
            if (!TimeSpan.TryParse(StartTimeTextBox.Text, out _))
            {
                MessageBox.Show("上班时间格式不正确，请使用 HH:mm 格式", "验证错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!TimeSpan.TryParse(EndTimeTextBox.Text, out _))
            {
                MessageBox.Show("下班时间格式不正确，请使用 HH:mm 格式", "验证错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 验证日期
            if (HireDatePicker.SelectedDate == null)
            {
                MessageBox.Show("请选择入职日期", "验证错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 验证发薪日
            if (!int.TryParse(SalaryDayTextBox.Text, out var salaryDay) || salaryDay < 1 || salaryDay > 31)
            {
                MessageBox.Show("发薪日必须是1-31之间的数字", "验证错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 验证刷新间隔
            if (!int.TryParse(RefreshIntervalTextBox.Text, out var refreshInterval) || refreshInterval < 5)
            {
                MessageBox.Show("刷新间隔必须至少为5秒", "验证错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ApplySettingsToConfig()
        {
            _currentConfig.StartTime = StartTimeTextBox.Text;
            _currentConfig.EndTime = EndTimeTextBox.Text;
            _currentConfig.HireDate = HireDatePicker.SelectedDate ?? DateTime.Now;
            _currentConfig.Gender = GenderComboBox.SelectedIndex == 0 ? "male" : "female";
            _currentConfig.SalaryDay = int.Parse(SalaryDayTextBox.Text);
            _currentConfig.Weekdays = WeekdaysTextBox.Text;
            _currentConfig.ActiveStartTime = ActiveStartTimeTextBox.Text;
            _currentConfig.ActiveEndTime = ActiveEndTimeTextBox.Text;
            _currentConfig.RefreshInterval = int.Parse(RefreshIntervalTextBox.Text);
            _currentConfig.DarkMode = DarkModeToggle.IsChecked ?? false;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateConfig())
            {
                ApplySettingsToConfig();
                
                // 处理夜间模式切换
                var newDarkMode = DarkModeToggle.IsChecked ?? false;
                if (_currentConfig.DarkMode != newDarkMode)
                {
                    _currentConfig.DarkMode = newDarkMode;
                    // 触发主题变更事件
                    SettingsApplied?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    SettingsApplied?.Invoke(this, EventArgs.Empty);
                }
                
                // 显示应用成功的反馈
                MessageBox.Show("设置已应用！点击保存可永久保存设置。", "应用成功", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateConfig())
            {
                try
                {
                    ApplySettingsToConfig();
                    await _configService.SaveConfigAsync(_currentConfig);
                    SettingsSaved?.Invoke(this, EventArgs.Empty);
                    
                    MessageBox.Show("设置已保存！", "提示", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存设置失败: {ex.Message}", "错误", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void AddCustomCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentConfig.CustomCountdowns.Count < 5)
            {
                _currentConfig.CustomCountdowns.Add(new CustomCountdown
                {
                    Name = "新事件",
                    Date = DateTime.Now.AddDays(30)
                });
                RefreshCustomCountdownList();
            }
            else
            {
                MessageBox.Show("最多只能添加5个自定义倒计时", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CustomCountdownListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CustomCountdownListBox.SelectedItem is CustomCountdown countdown)
            {
                // 这里可以打开编辑对话框
                var dialog = new CustomCountdownEditDialog(countdown);
                if (dialog.ShowDialog() == true)
                {
                    countdown.Name = dialog.CountdownName;
                    countdown.Date = dialog.CountdownDate;
                    RefreshCustomCountdownList();
                }
            }
        }

        private void DeleteCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is CustomCountdown countdown)
                {
                    System.Diagnostics.Debug.WriteLine($"删除按钮点击，倒计时对象: {countdown.Name}, 日期: {countdown.Date}");
                    
                    var result = MessageBox.Show($"确定要删除\"{countdown.Name}\"吗？", "确认删除", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // 直接尝试从集合中删除
                        _currentConfig.CustomCountdowns.Remove(countdown);
                        RefreshCustomCountdownList();
                        
                        System.Diagnostics.Debug.WriteLine($"已删除倒计时: {countdown.Name}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"删除按钮点击但未找到倒计时对象: sender={sender?.GetType()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除异常: {ex.Message}");
                MessageBox.Show($"删除失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePath = await _backupService.ExportConfigAsync();
                if (!string.IsNullOrEmpty(filePath))
                {
                    MessageBox.Show($"配置已导出到:\n{filePath}", "导出成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出配置失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ImportConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("导入配置将覆盖当前所有设置，确定继续吗？", "确认导入", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    if (await _backupService.ImportConfigAsync())
                    {
                        _currentConfig = await _configService.LoadConfigAsync();
                        BindConfigToUI(_currentConfig);
                        SettingsApplied?.Invoke(this, EventArgs.Empty);
                        
                        MessageBox.Show("配置导入成功！", "导入成功", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入配置失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backupPath = await _backupService.CreateBackupAsync();
                MessageBox.Show($"备份已创建:\n{backupPath}", "备份成功", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建备份失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RestoreBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backups = _backupService.GetAvailableBackups();
                if (!backups.Any())
                {
                    MessageBox.Show("没有找到可用的备份", "提示", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new BackupRestoreDialog(backups);
                if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedBackupPath))
                {
                    await _backupService.RestoreFromBackupAsync(dialog.SelectedBackupPath);
                    _currentConfig = await _configService.LoadConfigAsync();
                    BindConfigToUI(_currentConfig);
                    SettingsApplied?.Invoke(this, EventArgs.Empty);
                    
                    MessageBox.Show("备份恢复成功！", "恢复成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"恢复备份失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}