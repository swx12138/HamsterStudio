using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.MVVM;

public class ViewModel : ObservableObject
{

}

public partial class KnownViewModel : ViewModel
{
    [ObservableProperty]
    private string _displayName = string.Empty;

}
