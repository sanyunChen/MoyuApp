using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MoyuApp.Models;

namespace MoyuApp.Services
{
    public class ConfigService : IDisposable
    {
        private readonly string _configPath;
        private AppConfig? _currentConfig;

        public ConfigService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "摸鱼办");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "config.json");
        }

        public async Task<AppConfig> LoadConfigAsync()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = await File.ReadAllTextAsync(_configPath);
                    _currentConfig = JsonConvert.DeserializeObject<AppConfig>(json) ?? AppConfig.LoadDefault();
                }
                else
                {
                    _currentConfig = AppConfig.LoadDefault();
                    await SaveConfigAsync(_currentConfig);
                }
            }
            catch (Exception)
            {
                _currentConfig = AppConfig.LoadDefault();
            }

            return _currentConfig;
        }

        public async Task SaveConfigAsync(AppConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
                _currentConfig = config;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存配置失败: {ex.Message}", ex);
            }
        }

        public AppConfig GetCurrentConfig()
        {
            return _currentConfig ?? AppConfig.LoadDefault();
        }

        public void Dispose()
        {
            // 清理资源（如果需要）
            _currentConfig = null;
        }
    }
}