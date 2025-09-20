using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.MVVM;

public class ViewModel : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        LogPropertyChanged(e.PropertyName ?? "Unknown");
        base.OnPropertyChanged(e);
    }

    protected virtual void LogPropertyChanged(string propertyName)
    {
        Logger.Shared.Debug($"Property {propertyName} has changed.");
    }

}

public partial class KnownViewModel : ViewModel
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
    }

    override protected void LogPropertyChanged(string propertyName)
    {
        Logger.Shared.Debug($"Property {propertyName} of {DisplayName} has changed.");
    }
}
