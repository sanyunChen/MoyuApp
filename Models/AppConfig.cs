using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace MoyuApp.Models
{
    public class AppConfig : INotifyPropertyChanged
    {
        private string _startTime = "09:00";
        private string _endTime = "18:00";
        private DateTime _hireDate = new DateTime(2000, 1, 1);
        private string _gender = "male";
        private int _refreshInterval = 60;
        private bool _darkMode = false;
        private string _activeStartTime = "09:00";
        private string _activeEndTime = "11:00";
        private int _salaryDay = 10;
        private string _weekdays = "1-5";
        private BindingList<CustomCountdown> _customCountdowns = new();

        public string StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); }
        }

        public string EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); }
        }

        public DateTime HireDate
        {
            get => _hireDate;
            set { _hireDate = value; OnPropertyChanged(); }
        }

        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
        }

        public int RefreshInterval
        {
            get => _refreshInterval;
            set { _refreshInterval = Math.Max(5, value); OnPropertyChanged(); }
        }

        public bool DarkMode
        {
            get => _darkMode;
            set { _darkMode = value; OnPropertyChanged(); }
        }

        public string ActiveStartTime
        {
            get => _activeStartTime;
            set { _activeStartTime = value; OnPropertyChanged(); }
        }

        public string ActiveEndTime
        {
            get => _activeEndTime;
            set { _activeEndTime = value; OnPropertyChanged(); }
        }

        public int SalaryDay
        {
            get => _salaryDay;
            set { _salaryDay = Math.Clamp(value, 1, 31); OnPropertyChanged(); }
        }

        public string Weekdays
        {
            get => _weekdays;
            set { _weekdays = value; OnPropertyChanged(); }
        }

        public BindingList<CustomCountdown> CustomCountdowns
        {
            get => _customCountdowns;
            set { _customCountdowns = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static AppConfig LoadDefault()
        {
            return new AppConfig
            {
                StartTime = "09:00",
                EndTime = "18:00",
                HireDate = new DateTime(2000, 1, 1),
                Gender = "male",
                RefreshInterval = 60,
                DarkMode = false,
                ActiveStartTime = "09:00",
                ActiveEndTime = "11:00",
                SalaryDay = 10,
                Weekdays = "1-5",
                CustomCountdowns = new BindingList<CustomCountdown>()
            };
        }
    }

    public class CustomCountdown : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private DateTime _date = DateTime.Now.AddDays(30);

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public int DaysRemaining => (int)Math.Ceiling((Date - DateTime.Now).TotalDays);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}