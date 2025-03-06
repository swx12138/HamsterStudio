using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using NLog.Targets;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace HamsterStudio.Toolkits.Logging;

[ObservableObject]
public partial class ObservableCollectionTarget : Target
{
    [ObservableProperty]
    private ObservableCollection<LogEventInfo> _logEntries = [];

    public ObservableCollectionTarget(string name)
    {
        Name = name;
    }

    protected override void Write(LogEventInfo logEvent)
    {
        // 确保在 UI 线程更新集合
        Application.Current.Dispatcher.Invoke(() =>
        {
            _logEntries.Add(logEvent);

            // 可选：限制日志条目数量（例如保留最后100条）
            if (_logEntries.Count > 1000)
                _logEntries.RemoveAt(0);
        });
    }
}