using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using NLog.Targets;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace HamsterStudio.Toolkits.Logging;

[ObservableObject]
public partial class DiagnosticsTraceTarget : Target
{
    public DiagnosticsTraceTarget(string name)
    {
        Name = name;
    }

    protected override void Write(LogEventInfo logEvent)
    {
        if (logEvent.Level < LogLevel.Info)
        {
            Debug.WriteLine(logEvent.FormattedMessage);
        }
        else if (logEvent.Level < LogLevel.Warn)
        {
            Trace.TraceInformation(logEvent.FormattedMessage);
        }
        else if (logEvent.Level < LogLevel.Error)
        {
            Trace.TraceWarning(logEvent.FormattedMessage);
        }
        else
        {
            Trace.TraceError(logEvent.FormattedMessage);
        }
    }
}