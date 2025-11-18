using System;
using System.Threading.Tasks;
using System.Timers;
using MoyuApp.Models;
using MoyuApp.Services;

namespace MoyuApp.Services
{
    public class AutoSaveService : IDisposable
    {
        private readonly ConfigService _configService;
        private readonly System.Timers.Timer _saveTimer;
        private AppConfig? _lastSavedConfig;
        private bool _hasChanges;

        public event EventHandler<AutoSaveEventArgs>? AutoSaveCompleted;
        public event EventHandler<string>? AutoSaveFailed;

        public AutoSaveService(ConfigService configService, double intervalMinutes = 5)
        {
            _configService = configService;
            _saveTimer = new System.Timers.Timer(intervalMinutes * 60 * 1000); // 转换为毫秒
            _saveTimer.Elapsed += OnAutoSaveTimerElapsed;
            _saveTimer.AutoReset = true;
        }

        public void Start()
        {
            _saveTimer.Start();
        }

        public void Stop()
        {
            _saveTimer.Stop();
        }

        public void MarkAsChanged()
        {
            _hasChanges = true;
        }

        private async void OnAutoSaveTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_hasChanges)
            {
                await PerformAutoSaveAsync();
            }
        }

        private async Task PerformAutoSaveAsync()
        {
            try
            {
                var currentConfig = _configService.GetCurrentConfig();
                
                // 检查是否有实际变化
                if (!HasConfigChanged(currentConfig))
                {
                    _hasChanges = false;
                    return;
                }

                await _configService.SaveConfigAsync(currentConfig);
                _lastSavedConfig = CloneConfig(currentConfig);
                _hasChanges = false;

                AutoSaveCompleted?.Invoke(this, new AutoSaveEventArgs
                {
                    SaveTime = DateTime.Now,
                    Config = currentConfig
                });
            }
            catch (Exception ex)
            {
                AutoSaveFailed?.Invoke(this, $"自动保存失败: {ex.Message}");
            }
        }

        private bool HasConfigChanged(AppConfig currentConfig)
        {
            if (_lastSavedConfig == null) return true;

            // 这里可以添加更详细的配置比较逻辑
            return currentConfig.StartTime != _lastSavedConfig.StartTime ||
                   currentConfig.EndTime != _lastSavedConfig.EndTime ||
                   currentConfig.HireDate != _lastSavedConfig.HireDate ||
                   currentConfig.Gender != _lastSavedConfig.Gender ||
                   currentConfig.RefreshInterval != _lastSavedConfig.RefreshInterval ||
                   currentConfig.DarkMode != _lastSavedConfig.DarkMode ||
                   currentConfig.ActiveStartTime != _lastSavedConfig.ActiveStartTime ||
                   currentConfig.ActiveEndTime != _lastSavedConfig.ActiveEndTime ||
                   currentConfig.SalaryDay != _lastSavedConfig.SalaryDay ||
                   currentConfig.Weekdays != _lastSavedConfig.Weekdays;
        }

        private AppConfig CloneConfig(AppConfig config)
        {
            return new AppConfig
            {
                StartTime = config.StartTime,
                EndTime = config.EndTime,
                HireDate = config.HireDate,
                Gender = config.Gender,
                RefreshInterval = config.RefreshInterval,
                DarkMode = config.DarkMode,
                ActiveStartTime = config.ActiveStartTime,
                ActiveEndTime = config.ActiveEndTime,
                SalaryDay = config.SalaryDay,
                Weekdays = config.Weekdays,
                CustomCountdowns = config.CustomCountdowns
            };
        }

        public async Task ForceSaveAsync()
        {
            if (_hasChanges)
            {
                await PerformAutoSaveAsync();
            }
        }

        public void Dispose()
        {
            _saveTimer?.Stop();
            _saveTimer?.Dispose();
        }
    }

    public class AutoSaveEventArgs : EventArgs
    {
        public DateTime SaveTime { get; set; }
        public AppConfig? Config { get; set; }
    }
}