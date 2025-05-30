using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudioGUI.Models;

internal class TitleModel : ObservableObject
{
    public string DisplayText { get; set; } = string.Empty;

}
