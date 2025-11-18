using System;
using System.Windows.Threading;
using MoyuApp.Models;

namespace MoyuApp.Services
{
    public class RefreshService : IDisposable
    {
        private readonly DispatcherTimer _refreshTimer;
        private readonly ConfigService _configService;
        private AppConfig _currentConfig;

        public event EventHandler<RefreshEventArgs>? RefreshTick;
        public event EventHandler<string>? RefreshFailed;

        public RefreshService(ConfigService configService)
        {
            _configService = configService;
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60) // 默认60秒
            };
            _refreshTimer.Tick += OnRefreshTick;
        }

        public void Start()
        {
            try
            {
                _currentConfig = _configService.GetCurrentConfig();
                UpdateRefreshInterval();
                _refreshTimer.Start();
            }
            catch (Exception ex)
            {
                RefreshFailed?.Invoke(this, $"启动刷新服务失败: {ex.Message}");
            }
        }

        public void Stop()
        {
            _refreshTimer.Stop();
        }

        public void UpdateConfig()
        {
            try
            {
                _currentConfig = _configService.GetCurrentConfig();
                UpdateRefreshInterval();
            }
            catch (Exception ex)
            {
                RefreshFailed?.Invoke(this, $"更新配置失败: {ex.Message}");
            }
        }

        private void UpdateRefreshInterval()
        {
            var newInterval = TimeSpan.FromSeconds(Math.Max(5, _currentConfig.RefreshInterval));
            if (_refreshTimer.Interval != newInterval)
            {
                _refreshTimer.Interval = newInterval;
            }
        }

        private void OnRefreshTick(object? sender, EventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                RefreshTick?.Invoke(this, new RefreshEventArgs
                {
                    RefreshTime = now,
                    Config = _currentConfig
                });
            }
            catch (Exception ex)
            {
                RefreshFailed?.Invoke(this, $"刷新失败: {ex.Message}");
            }
        }

        public void ForceRefresh()
        {
            OnRefreshTick(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
        }
    }

    public class RefreshEventArgs : EventArgs
    {
        public DateTime RefreshTime { get; set; }
        public AppConfig? Config { get; set; }
    }
}