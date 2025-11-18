using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using MoyuApp.Models;

namespace MoyuApp.Services
{
    public class DataBackupService
    {
        private readonly string _backupFolder;
        private readonly ConfigService _configService;

        public DataBackupService(ConfigService configService)
        {
            _configService = configService;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _backupFolder = Path.Combine(appData, "摸鱼办", "Backups");
            Directory.CreateDirectory(_backupFolder);
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"moyu_backup_{timestamp}.json";
                var backupPath = Path.Combine(_backupFolder, backupFileName);

                var config = _configService.GetCurrentConfig();
                var backupData = new
                {
                    Version = "1.0",
                    BackupTime = DateTime.Now,
                    Config = config,
                    ExportTime = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(backupData, Formatting.Indented);
                await File.WriteAllTextAsync(backupPath, json);

                return backupPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"创建备份失败: {ex.Message}", ex);
            }
        }

        public async Task RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("备份文件不存在");
                }

                var json = await File.ReadAllTextAsync(backupPath);
                var backupData = JsonConvert.DeserializeObject<BackupData>(json);

                if (backupData?.Config != null)
                {
                    await _configService.SaveConfigAsync(backupData.Config);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"恢复备份失败: {ex.Message}", ex);
            }
        }

        public List<BackupInfo> GetAvailableBackups()
        {
            try
            {
                var backups = new List<BackupInfo>();
                var files = Directory.GetFiles(_backupFolder, "moyu_backup_*.json");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    backups.Add(new BackupInfo
                    {
                        FilePath = file,
                        FileName = Path.GetFileName(file),
                        CreatedTime = fileInfo.CreationTime,
                        FileSize = fileInfo.Length
                    });
                }

                return backups.OrderByDescending(b => b.CreatedTime).ToList();
            }
            catch (Exception)
            {
                return new List<BackupInfo>();
            }
        }

        public async Task<string> ExportConfigAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"moyu_config_{timestamp}.json";
                
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    DefaultExt = ".json",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    var config = _configService.GetCurrentConfig();
                    var exportData = new
                    {
                        Version = "1.0",
                        ExportTime = DateTime.Now,
                        Config = config
                    };

                    var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                    await File.WriteAllTextAsync(dialog.FileName, json);
                    
                    return dialog.FileName;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"导出配置失败: {ex.Message}", ex);
            }
        }

        public async Task<bool> ImportConfigAsync()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".json",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    var json = await File.ReadAllTextAsync(dialog.FileName);
                    var importData = JsonConvert.DeserializeObject<ExportData>(json);

                    if (importData?.Config != null)
                    {
                        await _configService.SaveConfigAsync(importData.Config);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"导入配置失败: {ex.Message}", ex);
            }
        }

        public void CleanupOldBackups(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var files = Directory.GetFiles(_backupFolder, "moyu_backup_*.json");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception)
            {
                // 清理失败不影响主要功能
            }
        }
    }

    public class BackupData
    {
        public string Version { get; set; } = string.Empty;
        public DateTime BackupTime { get; set; }
        public AppConfig? Config { get; set; }
        public DateTime ExportTime { get; set; }
    }

    public class ExportData
    {
        public string Version { get; set; } = string.Empty;
        public DateTime ExportTime { get; set; }
        public AppConfig? Config { get; set; }
    }

    public class BackupInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public long FileSize { get; set; }

        public string FileSizeFormatted
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = FileSize;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }
}