using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.FileSystem;
using System.Windows.Input;

namespace HamsterStudio.Gallery.Models;

[ObservableObject]
internal partial class HamstertFileInfoDisplayModel(string filename) : HamstertFileInfo(filename)
{
    [ObservableProperty]
    private bool _selected = false;

    public ICommand OpenLargeImageViewCommand { get; }


}
