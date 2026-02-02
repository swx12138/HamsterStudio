using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace HamsterStudio.Toolkits.Logging
{
    // WPF 日志条目模型
    public partial class LogEntry : ObservableObject
    {
        [ObservableProperty]
        private DateTime _timestamp;

        [ObservableProperty]
        private LogLevel _level;

        [ObservableProperty]
        private string _category;

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private Exception _exception;

        public string LevelColor
        {
            get
            {
                return Level switch
                {
                    LogLevel.Trace => "Gray",
                    LogLevel.Debug => "LightBlue",
                    LogLevel.Information => "Green",
                    LogLevel.Warning => "Orange",
                    LogLevel.Error => "Red",
                    LogLevel.Critical => "DarkRed",
                    _ => "Black"
                };
            }
        }
    }

    // WPF 日志视图模型
    public class LogViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<LogEntry> _logs;
        private readonly object _syncLock = new object();
        private int _maxLogCount = 1000;
        private bool _autoScroll = true;
        private string _filterText = string.Empty;
        private LogLevel _minLogLevel = LogLevel.Information;

        public ObservableCollection<LogEntry> Logs => _logs;

        public int MaxLogCount
        {
            get => _maxLogCount;
            set { _maxLogCount = value; OnPropertyChanged(); }
        }

        public bool AutoScroll
        {
            get => _autoScroll;
            set { _autoScroll = value; OnPropertyChanged(); }
        }

        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public LogLevel MinLogLevel
        {
            get => _minLogLevel;
            set { _minLogLevel = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ICollectionView LogsView { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public LogViewModel()
        {
            _logs = new ObservableCollection<LogEntry>();
            LogsView = CollectionViewSource.GetDefaultView(_logs);
            LogsView.Filter = FilterLogEntry;
        }

        public void AddLog(LogEntry logEntry)
        {
            // 确保在 UI 线程上执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_syncLock)
                {
                    _logs.Add(logEntry);

                    // 限制日志数量
                    while (_logs.Count > MaxLogCount)
                    {
                        _logs.RemoveAt(0);
                    }
                }
            });
        }

        public void ClearLogs()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_syncLock)
                {
                    _logs.Clear();
                }
            });
        }

        public void ExportToFile(string filePath)
        {
            lock (_syncLock)
            {
                var logsToExport = _logs.ToList();
                var lines = logsToExport.Select(log =>
                    $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level}] {log.Category}: {log.Message}");

                File.WriteAllLines(filePath, lines);
            }
        }

        private bool FilterLogEntry(object obj)
        {
            if (obj is not LogEntry log) return false;

            // 按日志级别过滤
            if (log.Level < MinLogLevel) return false;

            // 按关键字过滤
            if (!string.IsNullOrEmpty(FilterText))
            {
                var searchText = FilterText.ToLower();
                if (!log.Category.ToLower().Contains(searchText) &&
                    !log.Message.ToLower().Contains(searchText))
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyFilter()
        {
            LogsView.Refresh();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // WPF 日志提供程序
    public class WpfLoggerProvider : ILoggerProvider
    {
        private readonly LogViewModel _logViewModel;
        private readonly ConcurrentDictionary<string, WpfLogger> _loggers = new();

        public WpfLoggerProvider(LogViewModel logViewModel)
        {
            _logViewModel = logViewModel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new WpfLogger(name, _logViewModel));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        private class WpfLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly LogViewModel _logViewModel;

            public WpfLogger(string categoryName, LogViewModel logViewModel)
            {
                _categoryName = categoryName;
                _logViewModel = logViewModel;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                var message = formatter(state, exception);

                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = logLevel,
                    Category = _categoryName,
                    Message = message,
                    Exception = exception
                };

                _logViewModel.AddLog(logEntry);
            }
        }
    }
}
