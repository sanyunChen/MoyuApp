using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MoyuApp.Dialogs
{
    public partial class BackupRestoreDialog : Window
    {
        public string SelectedBackupPath { get; private set; } = string.Empty;
        private readonly List<BackupInfo> _backups;

        public BackupRestoreDialog(List<Services.BackupInfo> backupFiles)
        {
            InitializeComponent();
            
            _backups = backupFiles.Select(backup => new BackupInfo
            {
                Path = backup.FilePath,
                Name = backup.FileName,
                Date = backup.CreatedTime,
                Size = backup.FileSizeFormatted
            }).OrderByDescending(b => b.Date).ToList();
            
            BackupListBox.ItemsSource = _backups;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }

        private void DeleteBackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is BackupInfo backup)
            {
                var result = MessageBox.Show(
                    $"确定要删除备份 \"{backup.Name}\" 吗？\n此操作不可撤销。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(backup.Path);
                        _backups.Remove(backup);
                        BackupListBox.ItemsSource = null;
                        BackupListBox.ItemsSource = _backups;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除备份失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackupListBox.SelectedItem is BackupInfo selectedBackup)
            {
                SelectedBackupPath = selectedBackup.Path;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("请选择一个备份文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class BackupInfo
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Size { get; set; } = string.Empty;
    }
}