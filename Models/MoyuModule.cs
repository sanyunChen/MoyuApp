using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MoyuApp.Models
{
    public class MoyuModule : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _displayText = string.Empty;
        private double _progressValue;
        private string _progressText = string.Empty;
        private bool _isEditing;
        private string _editValue = string.Empty;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string DisplayText
        {
            get => _displayText;
            set { _displayText = value; OnPropertyChanged(); }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(); }
        }

        public string ProgressText
        {
            get => _progressText;
            set { _progressText = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public string EditValue
        {
            get => _editValue;
            set { _editValue = value; OnPropertyChanged(); }
        }

        public ModuleType Type { get; set; }
        public DateTime? TargetDate { get; set; }
        public TimeSpan? TargetTime { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartEditing()
        {
            IsEditing = true;
            EditValue = DisplayText;
        }

        public void CancelEditing()
        {
            IsEditing = false;
            EditValue = string.Empty;
        }

        public void SaveEdit()
        {
            DisplayText = EditValue;
            IsEditing = false;
        }
    }

    public enum ModuleType
    {
        TodayProgress,
        WeekProgress,
        WeekendCountdown,
        SalaryCountdown,
        RetireCountdown,
        CustomCountdown,
        HolidayCountdown,
        Quote
    }

    public class Holiday
    {
        public string Name { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Day { get; set; }

        public static Holiday[] ChineseHolidays = new[]
        {
            new Holiday { Name = "元旦", Month = 1, Day = 1 },
            new Holiday { Name = "春节", Month = 2, Day = 1 },
            new Holiday { Name = "清明节", Month = 4, Day = 4 },
            new Holiday { Name = "劳动节", Month = 5, Day = 1 },
            new Holiday { Name = "国庆节", Month = 10, Day = 1 }
        };
    }
}