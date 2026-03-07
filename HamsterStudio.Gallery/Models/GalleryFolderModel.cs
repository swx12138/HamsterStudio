using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Comparers;
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
    private ObservableCollection<FileInfo> _files = [];

    [ObservableProperty]
    private ObservableCollection<GalleryFolderModel> _folders = [];

    [ObservableProperty]
    private DirectoryInfo _dirInfo;

    public ICommand OpenFolderCommand { get; }

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
            folder.Files.Add(file);
        }

        return folder;
    }

}
