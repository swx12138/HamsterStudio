using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.ViewModels;
using HamsterStudio.Gallery.Views;
using Microsoft.Win32;
using System.Windows.Input;

namespace HamsterStudio.Gallery;

partial class MainWindowModel :ViewModel
{
    public GalleryView MainView { get; } = new();
    public GalleryViewModel ViewModel => (MainView.DataContext as GalleryViewModel)!;

    [ObservableProperty]
    private bool _topmost = false;

    public ICommand OpenCommand { get; }

    public MainWindowModel()
    {
#if DEBUG
        Topmost = true;
#endif
        OpenCommand = new RelayCommand(() =>
        {
            OpenFolderDialog dialog = new();
            dialog.Title = "选择文件夹";
            dialog.Multiselect = false;
            if (!(dialog.ShowDialog() ?? false))
            {
                return;
            }

            ViewModel.FileManager.ReadFolder(dialog.FolderName);
            Logger.Shared.Trace($"现在一共有{ViewModel.FileManager.FileGroups.Count}个分组，。");

        });
    }

}
