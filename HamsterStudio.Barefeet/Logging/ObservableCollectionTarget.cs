using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using NLog.Targets;
using System.Collections.ObjectModel;

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
        LogEntries.Add(logEvent);
        // 可选：限制日志条目数量（例如保留最后100条）
        if (LogEntries.Count > 1000)
            LogEntries.RemoveAt(0);
    }
}