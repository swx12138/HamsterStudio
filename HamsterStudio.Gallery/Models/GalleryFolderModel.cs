using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Comparers;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.SysCall;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace HamsterStudio.Gallery.Models;

public partial class GalleryFolderModel : BindableBase
{
    [ObservableProperty]
    private ObservableCollection<HamstertFileInfo> _files = [];

    [ObservableProperty]
    private ObservableCollection<GalleryFolderModel> _folders = [];

    [ObservableProperty]
    private DirectoryInfo _dirInfo;

    public ICommand OpenFolderCommand { get; }

    static readonly string[] AcceptExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp" };

    public GalleryFolderModel(DirectoryInfo di, ILogger? logger = null) : base(logger)
    {
        _dirInfo = di;
        OpenFolderCommand = new RelayCommand(() =>
        {
            ShellApi.OpenFolder(DirInfo.FullName);
        });
    }

    public static GalleryFolderModel LoadFolder(DirectoryInfo dirInfo)
    {
        var folder = new GalleryFolderModel(dirInfo);

        foreach (var dir in dirInfo.EnumerateDirectories().Order(new DirectoryInfoComparer()))
        {
            folder.Folders.Add(LoadFolder(dir));
        }

        foreach (var file in dirInfo.EnumerateFiles().Order(new FileInfoComparer()))
        {
            if (!AcceptExtensions.Contains(file.Extension))
            {
                continue;
            }
            folder.Files.Add(new HamstertFileInfo(file.FullName) { RemoveCommand = new RelayCommand(() => { }) }); // 待优化
        }

        return folder;
    }

}
