using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.FileSystem;

namespace HamsterStudio.Gallery.Models;

[ObservableObject]
internal partial class HamstertFileInfoDisplayModel(string filename) : HamstertFileInfo(filename)
{
    [ObservableProperty]
    private bool _selected = false;

    [RelayCommand]
    public void OpenLargeImageView()
    {

    }


}
