using System;
using System.Windows;
using MoyuApp.Services;

namespace MoyuApp
{
    public partial class App : Application
    {
        private ConfigService? _configService;
        private AutoSaveService? _autoSaveService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 配置Material Design主题
            ConfigureMaterialDesign();
            
            // 初始化服务
            InitializeServices();
            
            // 设置异常处理
            SetupExceptionHandling();
        }

        private void ConfigureMaterialDesign()
        {
            // 默认主题文件（Material Design 3）
            var mdDefaults = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml")
            };
            Resources.MergedDictionaries.Add(mdDefaults);

            // 主色
            var primaryColor = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.DeepPurple.xaml")
            };

            // 辅色
            var secondaryColor = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Secondary/MaterialDesignColor.Lime.xaml")
            };

            Resources.MergedDictionaries.Add(primaryColor);
            Resources.MergedDictionaries.Add(secondaryColor);
        }


        private void InitializeServices()
        {
            _configService = new ConfigService();
            _autoSaveService = new AutoSaveService(_configService, 5); // 5分钟自动保存
            _autoSaveService.Start();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                MessageBox.Show(
                    $"发生未处理的异常:\n{exception?.Message}\n\n应用程序将关闭。",
                    "严重错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Shutdown(1);
            };

            DispatcherUnhandledException += (sender, e) =>
            {
                MessageBox.Show(
                    $"发生UI异常:\n{e.Exception.Message}",
                    "UI错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                
                e.Handled = true;
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            // 清理资源
            _autoSaveService?.Dispose();
            _configService?.Dispose();
        }
    }
}