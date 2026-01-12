using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace HamsterStudio.Barefeet.MVVM;

public abstract class BindableBase(ILogger? logger) : ObservableObject
{
    protected ILogger? logger = logger;
}

public class ViewModel(ILogger? logger) : BindableBase(logger)
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        LogPropertyChanged(e.PropertyName ?? "Unknown");
        base.OnPropertyChanged(e);
    }

    protected virtual void LogPropertyChanged(string propertyName)
    {
        logger?.LogDebug($"Property {propertyName} has changed.");
    }

}

public partial class KnownViewModel(ILogger? logger) : ViewModel(logger)
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
    }

    override protected void LogPropertyChanged(string propertyName)
    {
        logger?.LogDebug($"Property {propertyName} of {DisplayName} has changed.");
    }

}
