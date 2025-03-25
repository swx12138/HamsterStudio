using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.BraveShine.ViewModels;
using HamsterStudio.BraveShine.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.BraveShine
{
    class MainWindowModel : ObservableObject
    {
        public MainView MainView { get;  } = new();

        public MainViewModel MainViewModel => (MainView.DataContext as MainViewModel)!;

    }
}
