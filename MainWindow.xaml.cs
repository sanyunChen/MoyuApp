using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using MoyuApp.Models;
using MoyuApp.Services;

namespace MoyuApp
{
    public partial class MainWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly HolidayService _holidayService;
        private readonly DispatcherTimer _refreshTimer;
        private readonly DispatcherTimer _clockTimer;
        private AppConfig _config;
        private MoyuModule? _currentEditingModule;
        private readonly Random _random = new();
        private List<Holiday> _currentHolidays = new();

        public MainWindow()
        {
            InitializeComponent();
            _configService = new ConfigService();
            _holidayService = new HolidayService();
            _refreshTimer = new DispatcherTimer();
            _clockTimer = new DispatcherTimer();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _config = await _configService.LoadConfigAsync();
                
                // 加载网络节日数据
                try
                {
                    _currentHolidays = await _holidayService.GetCurrentYearHolidaysAsync();
                    System.Diagnostics.Debug.WriteLine($"成功加载 {_currentHolidays.Count} 个网络节日");
                }
                catch (Exception holidayEx)
                {
                    System.Diagnostics.Debug.WriteLine($"加载网络节日失败，使用默认节日: {holidayEx.Message}");
                    _currentHolidays = new List<Holiday>(Holiday.ChineseHolidays);
                }
                
                InitializeTimer();
                InitializeClockTimer();
                UpdateAllModules();
                ApplyTheme();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _config = AppConfig.LoadDefault();
            }
        }

        private void InitializeTimer()
        {
            _refreshTimer.Interval = TimeSpan.FromSeconds(_config.RefreshInterval);
            _refreshTimer.Tick += (s, e) => UpdateAllModules();
            _refreshTimer.Start();
        }

        private void InitializeClockTimer()
        {
            _clockTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            _clockTimer.Tick += (s, e) => UpdateCurrentTime();
            _clockTimer.Start();
        }

        private void ApplyTheme()
        {
            // 清除现有主题
            var themesToRemove = new List<ResourceDictionary>();
            foreach (var dict in Resources.MergedDictionaries)
            {
                if (dict.Source != null && (dict.Source.ToString().Contains("DarkTheme.xaml") || 
                    dict.Source.ToString().Contains("LightTheme.xaml")))
                {
                    themesToRemove.Add(dict);
                }
            }
            
            foreach (var theme in themesToRemove)
            {
                Resources.MergedDictionaries.Remove(theme);
            }
            
            // 应用新主题
            if (_config.DarkMode)
            {
                // 应用深色主题
                var darkTheme = new ResourceDictionary
                {
                    Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                };
                Resources.MergedDictionaries.Add(darkTheme);
                System.Diagnostics.Debug.WriteLine("已应用深色主题");
            }
            else
            {
                // 应用浅色主题
                var lightTheme = new ResourceDictionary
                {
                    Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
                };
                Resources.MergedDictionaries.Add(lightTheme);
                System.Diagnostics.Debug.WriteLine("已应用浅色主题");
            }
        }

        private void UpdateAllModules()
        {
            UpdateCurrentTime();
            UpdateTodayProgress();
            UpdateWeekProgress();
            UpdateWeekendCountdown();
            UpdateSalaryCountdown();
            UpdateCustomCountdowns();
            UpdateHolidayCountdowns();
            UpdateRetireCountdown();
            UpdateQuote();
        }

        private void UpdateCurrentTime()
        {
            var now = DateTime.Now;
            CurrentTimeText.Text = now.ToString("yyyy-MM-dd HH:mm:ss");
            TimeZoneText.Text = TimeZoneInfo.Local.DisplayName;
        }

        private void UpdateTodayProgress()
        {
            var now = DateTime.Now;
            var startTime = TimeSpan.Parse(_config.StartTime);
            var endTime = TimeSpan.Parse(_config.EndTime);
            
            var startDateTime = now.Date.Add(startTime);
            var endDateTime = now.Date.Add(endTime);
            
            if (now < startDateTime)
            {
                TodayProgressBar.Value = 0;
                TodayProgressText.Text = "还未开始上班";
            }
            else if (now > endDateTime)
            {
                TodayProgressBar.Value = 100;
                TodayProgressText.Text = "今日工作已完成";
            }
            else
            {
                var totalMinutes = (endDateTime - startDateTime).TotalMinutes;
                var passedMinutes = (now - startDateTime).TotalMinutes;
                var percentage = Math.Min(100, Math.Max(0, (passedMinutes / totalMinutes) * 100));
                
                TodayProgressBar.Value = percentage;
                var remaining = endDateTime - now;
                TodayProgressText.Text = $"已过 {percentage:F1}%，离下班 {FormatTimeSpan(remaining)}";
            }
        }

        private void UpdateWeekProgress()
        {
            var now = DateTime.Now;
            var weekRange = _config.Weekdays.Split('-');
            var startDay = int.Parse(weekRange[0]);
            var endDay = int.Parse(weekRange[1]);
            
            var currentDayOfWeek = ((int)now.DayOfWeek == 0) ? 7 : (int)now.DayOfWeek;
            
            var weekStart = now.AddDays(-(currentDayOfWeek - startDay)).Date;
            var weekEnd = now.AddDays(endDay - currentDayOfWeek).Date.AddDays(1).AddSeconds(-1);
            
            var totalTicks = weekEnd.Ticks - weekStart.Ticks;
            var passedTicks = now.Ticks - weekStart.Ticks;
            var percentage = Math.Min(100, Math.Max(0, (double)passedTicks / totalTicks * 100));
            
            WeekProgressBar.Value = percentage;
            WeekProgressText.Text = $"已过 {percentage:F1}%，剩余 {100 - percentage:F1}%";
        }

        private void UpdateWeekendCountdown()
        {
            var now = DateTime.Now;
            var daysUntilSaturday = (6 - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            
            var nextSaturday = now.AddDays(daysUntilSaturday).Date;
            var timeUntilWeekend = nextSaturday - now;
            
            WeekendText.Text = FormatTimeSpan(timeUntilWeekend);
        }

        private void UpdateSalaryCountdown()
        {
            var now = DateTime.Now;
            var salaryDate = new DateTime(now.Year, now.Month, _config.SalaryDay);
            
            if (now > salaryDate)
            {
                salaryDate = salaryDate.AddMonths(1);
            }
            
            var timeUntilSalary = salaryDate - now;
            SalaryText.Text = FormatTimeSpan(timeUntilSalary);
            SalaryTitleText.Text = $"距离发工资（{_config.SalaryDay}号）";
        }

        private void UpdateCustomCountdowns()
        {
            var now = DateTime.Now;
            
            // 直接绑定到CustomCountdowns集合
            CustomCountdownList.ItemsSource = null;
            CustomCountdownList.ItemsSource = _config.CustomCountdowns;
            
            System.Diagnostics.Debug.WriteLine($"自定义倒计时列表已更新，共{_config.CustomCountdowns.Count}个项目");
        }

        private void UpdateHolidayCountdowns()
        {
            var now = DateTime.Now;
            var items = new List<string>();
            
            // 使用网络节日数据，如果没有则使用默认数据
            var holidays = _currentHolidays.Count > 0 ? _currentHolidays : new List<Holiday>(Holiday.ChineseHolidays);
            
            foreach (var holiday in holidays)
            {
                var holidayDate = new DateTime(now.Year, holiday.Month, holiday.Day);
                if (holidayDate < now)
                {
                    holidayDate = holidayDate.AddYears(1);
                }
                
                var daysRemaining = (int)Math.Ceiling((holidayDate - now).TotalDays);
                items.Add($"{holiday.Name}：还有 {daysRemaining} 天 ({holidayDate:yyyy-MM-dd})");
            }
            
            HolidayCountdownList.ItemsSource = items;
            
            // 显示数据来源
            var sourceText = _currentHolidays.Count > 0 ? "网络节日数据" : "默认节日数据";
            System.Diagnostics.Debug.WriteLine($"节日倒计时已更新，使用{sourceText}，共{holidays.Count}个节日");
        }

        private void UpdateRetireCountdown()
        {
            var now = DateTime.Now;
            var retireAge = _config.Gender == "female" ? 55 : 60;
            var retireDate = _config.HireDate.AddYears(retireAge);
            
            var totalMonths = (retireDate.Year - _config.HireDate.Year) * 12 + 
                             (retireDate.Month - _config.HireDate.Month);
            var passedMonths = (now.Year - _config.HireDate.Year) * 12 + 
                               (now.Month - _config.HireDate.Month);
            
            var percentage = Math.Min(100, Math.Max(0, (double)passedMonths / totalMonths * 100));
            
            RetireProgressBar.Value = percentage;
            RetireInfoText.Text = $"生日：{_config.HireDate:yyyy-MM-dd} · 退休年龄：{retireAge}岁";
            RetireRemainingText.Text = $"已工作 {passedMonths} 个月 · 剩余 {totalMonths - passedMonths} 个月";
            
            // 更新生日倒计时
            UpdateBirthdayCountdown();
        }

        private void UpdateBirthdayCountdown()
        {
            var now = DateTime.Now;
            var thisYearBirthday = new DateTime(now.Year, _config.HireDate.Month, _config.HireDate.Day);
            var nextBirthday = thisYearBirthday;
            
            // 如果今年的生日已经过了，计算明年的生日
            if (now > thisYearBirthday)
            {
                nextBirthday = thisYearBirthday.AddYears(1);
            }
            
            var daysUntilBirthday = (nextBirthday - now).Days;
            var totalDaysInYear = DateTime.IsLeapYear(now.Year) ? 366 : 365;
            var daysSinceLastBirthday = now.DayOfYear > _config.HireDate.DayOfYear 
                ? now.DayOfYear - _config.HireDate.DayOfYear 
                : totalDaysInYear - (_config.HireDate.DayOfYear - now.DayOfYear);
            
            var percentage = Math.Min(100, Math.Max(0, (double)daysSinceLastBirthday / totalDaysInYear * 100));
            
            BirthdayProgressBar.Value = percentage;
            BirthdayInfoText.Text = $"下次生日：{nextBirthday:yyyy-MM-dd} · 年龄：{now.Year - _config.HireDate.Year}岁";
            
            if (daysUntilBirthday == 0)
            {
                BirthdayRemainingText.Text = "🎂 今天是生日！";
            }
            else
            {
                BirthdayRemainingText.Text = $"距离生日还有 {daysUntilBirthday} 天";
            }
            
            System.Diagnostics.Debug.WriteLine($"生日倒计时已更新：下次生日 {nextBirthday:yyyy-MM-dd}，还有 {daysUntilBirthday} 天");
        }

        private void UpdateQuote()
        {
            var now = DateTime.Now;
            var quotes = GetSmartQuotes(now);
            QuoteText.Text = quotes[_random.Next(quotes.Count)];
        }

        private List<string> GetSmartQuotes(DateTime now)
        {
            var activeQuotes = new List<string>
            {
                "摸鱼是门艺术，要讲节奏 🎨",
                "认真摸鱼，快乐加倍 🐠",
                "效率摸鱼两不误 💼",
                "放松片刻，更好出发 ☕",
                "摸鱼不误正业，反而助力效率 ⚡"
            };

            var relaxQuotes = new List<string>
            {
                "摸鱼使我快乐 😎",
                "摸鱼是对工作的尊重 ✨",
                "你摸，我摸，大家都摸 🐟",
                "一摸解千愁 🍃",
                "今天也要摸得自然 🧘"
            };

            var weekendQuotes = new List<string>
            {
                "工作才开始，忍着！💪",
                "胜利在望，摸鱼蓄力！🚀",
                "今天就是周五！恭喜下班！🎉"
            };

            var salaryQuotes = new List<string>
            {
                "钱包正在路上，请注意查收 💰",
                "发薪倒计时，信心满满 💵"
            };

            // 计算距离发薪天数
            var salaryDate = new DateTime(now.Year, now.Month, _config.SalaryDay);
            if (now > salaryDate) salaryDate = salaryDate.AddMonths(1);
            var daysToSalary = (int)Math.Ceiling((salaryDate - now).TotalDays);

            // 计算距离周末天数
            var daysToWeekend = (6 - (int)now.DayOfWeek + 7) % 7;
            if (daysToWeekend == 0) daysToWeekend = 7;

            // 判断积极时间段
            var currentTime = now.TimeOfDay;
            var activeStart = TimeSpan.Parse(_config.ActiveStartTime);
            var activeEnd = TimeSpan.Parse(_config.ActiveEndTime);
            var isActiveTime = currentTime >= activeStart && currentTime <= activeEnd;

            // 优先级：发薪 → 周末 → 普通时间段
            if (daysToSalary < 5)
            {
                return salaryQuotes;
            }
            else if (daysToWeekend > 3)
            {
                return new List<string> { weekendQuotes[0] };
            }
            else if (daysToWeekend == 1)
            {
                return new List<string> { weekendQuotes[1] };
            }
            else if (daysToWeekend == 0)
            {
                return new List<string> { weekendQuotes[2] };
            }
            else
            {
                return isActiveTime ? activeQuotes : relaxQuotes;
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan <= TimeSpan.Zero)
                return "已到";
            
            var days = (int)timeSpan.TotalDays;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            
            return $"{days}天{hours}小时{minutes}分钟";
        }

        // 实时编辑功能
        private void ModuleText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // 双击进入编辑模式
            {
                var textBlock = sender as TextBlock;
                if (textBlock != null)
                {
                    StartModuleEdit(textBlock);
                }
            }
        }

        private void StartModuleEdit(TextBlock textBlock)
        {
            _currentEditingModule = GetModuleFromTextBlock(textBlock);
            if (_currentEditingModule != null)
            {
                EditTitle.Text = $"编辑 {_currentEditingModule.Title}";
                EditTextBox.Text = textBlock.Text;
                EditModeOverlay.Visibility = Visibility.Visible;
                EditTextBox.Focus();
                EditTextBox.SelectAll();
            }
        }

        private MoyuModule? GetModuleFromTextBlock(TextBlock textBlock)
        {
            // 根据TextBlock的名称返回对应的模块
            if (textBlock == TodayProgressText)
                return new MoyuModule { Title = "今日摸鱼进度", Type = ModuleType.TodayProgress };
            if (textBlock == WeekProgressText)
                return new MoyuModule { Title = "本周摸鱼进度", Type = ModuleType.WeekProgress };
            if (textBlock == WeekendText)
                return new MoyuModule { Title = "距离周末", Type = ModuleType.WeekendCountdown };
            if (textBlock == SalaryText)
                return new MoyuModule { Title = "距离发工资", Type = ModuleType.SalaryCountdown };
            if (textBlock == RetireRemainingText)
                return new MoyuModule { Title = "退休倒计时", Type = ModuleType.RetireCountdown };
            if (textBlock == QuoteText)
                return new MoyuModule { Title = "摸鱼语录", Type = ModuleType.Quote };
            
            return null;
        }

        private void SaveEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEditingModule != null)
            {
                // 这里可以添加保存逻辑，比如更新配置
                EditModeOverlay.Visibility = Visibility.Collapsed;
                _currentEditingModule = null;
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            EditModeOverlay.Visibility = Visibility.Collapsed;
            _currentEditingModule = null;
        }

        private void EditModeOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 点击遮罩层关闭编辑模式
            EditModeOverlay.Visibility = Visibility.Collapsed;
            _currentEditingModule = null;
        }

        private void EditPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 阻止事件冒泡，避免点击编辑面板时关闭编辑模式
            e.Handled = true;
        }

        // 设置面板功能
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsToPanel();
            SettingsPanel.Visibility = Visibility.Visible;
            
            // 确保设置面板中的事件处理程序已正确连接
            SaveSettingsButton.Click -= SaveSettingsButton_Click;
            SaveSettingsButton.Click += SaveSettingsButton_Click;
        }

        private void LoadSettingsToPanel()
        {
            StartTimeTextBox.Text = _config.StartTime;
            EndTimeTextBox.Text = _config.EndTime;
            HireDatePicker.Text = _config.HireDate.ToString("yyyy-MM-dd");
            GenderComboBox.SelectedIndex = _config.Gender == "male" ? 0 : 1;
            SalaryDayTextBox.Text = _config.SalaryDay.ToString();
            WeekdaysTextBox.Text = _config.Weekdays;
            RefreshIntervalTextBox.Text = _config.RefreshInterval.ToString();
            
            System.Diagnostics.Debug.WriteLine($"设置面板已加载");
        }

        private async void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 验证输入
                if (!TimeSpan.TryParse(StartTimeTextBox.Text, out _))
                {
                    MessageBox.Show("上班时间格式不正确，请使用 HH:mm 格式", "验证错误", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(EndTimeTextBox.Text, out _))
                {
                    MessageBox.Show("下班时间格式不正确，请使用 HH:mm 格式", "验证错误", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(SalaryDayTextBox.Text, out var salaryDay) || salaryDay < 1 || salaryDay > 31)
                {
                    MessageBox.Show("发薪日必须是1-31之间的数字", "验证错误", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(RefreshIntervalTextBox.Text, out var refreshInterval) || refreshInterval < 5)
                {
                    MessageBox.Show("刷新间隔必须至少为5秒", "验证错误", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 保存设置
                _config.StartTime = StartTimeTextBox.Text;
                _config.EndTime = EndTimeTextBox.Text;
                _config.HireDate = DateTime.Parse(HireDatePicker.Text);
                _config.Gender = (string)((ComboBoxItem)GenderComboBox.SelectedItem).Tag;
                _config.SalaryDay = salaryDay;
                _config.Weekdays = WeekdaysTextBox.Text;
                _config.RefreshInterval = refreshInterval;

                await _configService.SaveConfigAsync(_config);
                
                // 重新初始化定时器
                _refreshTimer.Interval = TimeSpan.FromSeconds(_config.RefreshInterval);
                
                SettingsPanel.Visibility = Visibility.Collapsed;
                
                // 立即更新所有模块以反映设置变化
                UpdateAllModules();
                
                MessageBox.Show("设置已保存！界面将立即更新。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;
        }

        private void AddCustomCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            // 添加自定义倒计时的逻辑
            if (_config.CustomCountdowns.Count < 5)
            {
                _config.CustomCountdowns.Add(new CustomCountdown 
                { 
                    Name = "新事件", 
                    Date = DateTime.Now.AddDays(30) 
                });
                UpdateCustomCountdowns();
            }
            else
            {
                MessageBox.Show("最多只能添加5个自定义倒计时", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteCustomCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button? button = null;
                if (sender is Button btn)
                {
                    button = btn;
                    if (button.Tag is CustomCountdown countdown)
                    {
                        System.Diagnostics.Debug.WriteLine($"删除按钮点击，倒计时对象: {countdown.Name}, 日期: {countdown.Date}");
                        
                        var result = MessageBox.Show($"确定要删除\"{countdown.Name}\"吗？", "确认删除", 
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            // 直接尝试从集合中删除
                            _config.CustomCountdowns.Remove(countdown);
                            UpdateCustomCountdowns();
                            
                            System.Diagnostics.Debug.WriteLine($"已删除倒计时: {countdown.Name}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"删除按钮点击但未找到倒计时对象: sender={sender?.GetType()}, Tag类型={button?.Tag?.GetType()}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"删除按钮点击但sender不是Button: sender={sender?.GetType()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除异常: {ex.Message}");
                MessageBox.Show($"删除失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditCustomCountdownName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is CustomCountdown countdown)
                {
                    var inputDialog = Microsoft.VisualBasic.Interaction.InputBox(
                        "请输入新的倒计时名称:",
                        "编辑倒计时名称",
                        countdown.Name);

                    if (!string.IsNullOrWhiteSpace(inputDialog))
                    {
                        countdown.Name = inputDialog.Trim();
                        UpdateCustomCountdowns();
                        System.Diagnostics.Debug.WriteLine($"已更新倒计时名称: {countdown.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"编辑倒计时名称异常: {ex.Message}");
                MessageBox.Show($"编辑失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditCustomCountdownDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is CustomCountdown countdown)
                {
                    // 使用简单的输入框来获取日期
                    var inputDialog = Microsoft.VisualBasic.Interaction.InputBox(
                        "请输入新的倒计时日期 (格式: yyyy-MM-dd):",
                        "编辑倒计时日期",
                        countdown.Date.ToString("yyyy-MM-dd"));

                    if (DateTime.TryParse(inputDialog, out DateTime newDate))
                    {
                        countdown.Date = newDate;
                        UpdateCustomCountdowns();
                        System.Diagnostics.Debug.WriteLine($"已更新倒计时日期: {countdown.Date:yyyy-MM-dd}");
                    }
                    else if (!string.IsNullOrWhiteSpace(inputDialog))
                    {
                        MessageBox.Show("日期格式不正确，请使用 yyyy-MM-dd 格式", "错误", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"编辑倒计时日期异常: {ex.Message}");
                MessageBox.Show($"编辑失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditCustomCountdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is CustomCountdown countdown)
                {
                    // 先编辑名称
                    var nameInput = Microsoft.VisualBasic.Interaction.InputBox(
                        "请输入新的倒计时名称:",
                        "编辑倒计时名称",
                        countdown.Name);

                    if (!string.IsNullOrWhiteSpace(nameInput))
                    {
                        countdown.Name = nameInput.Trim();
                        
                        // 再编辑日期
                        var dateInput = Microsoft.VisualBasic.Interaction.InputBox(
                            "请输入新的倒计时日期 (格式: yyyy-MM-dd):",
                            "编辑倒计时日期",
                            countdown.Date.ToString("yyyy-MM-dd"));

                        if (DateTime.TryParse(dateInput, out DateTime newDate))
                        {
                            countdown.Date = newDate;
                            UpdateCustomCountdowns();
                            System.Diagnostics.Debug.WriteLine($"已更新倒计时: {countdown.Name} - {countdown.Date:yyyy-MM-dd}");
                        }
                        else if (!string.IsNullOrWhiteSpace(dateInput))
                        {
                            MessageBox.Show("日期格式不正确，请使用 yyyy-MM-dd 格式", "错误", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"编辑倒计时异常: {ex.Message}");
                MessageBox.Show($"编辑失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _refreshTimer?.Stop();
            _clockTimer?.Stop();
        }
    }
}