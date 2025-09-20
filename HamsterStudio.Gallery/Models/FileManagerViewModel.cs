using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Algorithm.Random;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Gallery.Models;

public class FileManagerViewModel : ViewModel
{
    public ICommand PlayCommand { get; }
    public ICommand OpenCommand { get; }

    public ObservableCollection<FileGroupViewModel> FileGroups { get; } = [];

    public int FileCount => FileGroups.Sum(x => x.Files.Count);

    public List<IFileManagerFilter> Filters { get; } = [];

    public IFileManagerGrouper Grouper { get; } = new DirFileManagerGrouper();

    public void ReadFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Where(x => Filters.Any(filter => filter.Test(x)));
        var gfiles = files.GroupBy(x => Grouper.Group(x));
        foreach (var gfile in gfiles)
        {
            var filelist = new ReadOnlyCollection<HamstertFileInfo>([.. gfile.Select(x => new HamstertFileInfo(x))]);
            var group = FileGroups.FirstOrDefault(x => x.GroupName == gfile.Key);
            if (group == null)
            {
                group = new(gfile.Key)
                {
                    Files = filelist
                };
                FileGroups.Add(group);
            }
            else
            {
                group.Files = new ReadOnlyCollection<HamstertFileInfo>([.. group.Files, .. filelist]);
                Logger.Shared.Trace($"Reload dir {gfile.Key}");
            }
        }
    }

    private void OnOpenCmd()
    {
        OpenFolderDialog dialog = new();
        dialog.Title = "选择文件夹";
        dialog.Multiselect = false;
        if (!(dialog.ShowDialog() ?? false))
        {
            return;
        }

        ReadFolder(dialog.FolderName);
        Logger.Shared.Trace($"现在一共有{FileGroups.Count}个分组，。");
    }

    private async Task OnPlayCmd()
    {
        if (FileCount <= 0)
        {
            Logger.Shared.Warning("No file to be shown.");
            return;
        }

        var wnd = new Window();
        var pause = new ManualResetEventSlim(false);
        wnd.PreviewKeyDown += (sender, e) =>
        {
            if (e.Key == Key.Space)
            {
                pause.Set();
            }
            if (e.Key == Key.Enter)
            {
                pause.Reset();
            }
        };
        wnd.Show();

        await OnPlayImpl(pause, wnd);
    }

    public async Task OnPlayImpl(ManualResetEventSlim pause, Window wnd)
    {
        CancellationTokenSource source = new();
        int times = 9999;
        while (times-- > 0 && !source.Token.IsCancellationRequested)
        {
            if (pause.IsSet)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                continue;
            }

            try
            {
                (wnd.Dispatcher ?? Application.Current.Dispatcher).Invoke(() =>
                {
                    if (!wnd.IsActive)
                    {
                        source.Cancel();
                    }
                });

                var filename = FileGroups.SelectMany(x => x.Files).Choice().FullName;
                ImageSource imgSource = new BitmapImage(new Uri(filename)) { CacheOption = BitmapCacheOption.OnLoad };
                imgSource.Freeze();

                (wnd.Dispatcher ?? Application.Current.Dispatcher).Invoke(() =>
                {
                    wnd.Content = new Image { Source = imgSource };
                });

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                Logger.Shared.Critical(ex);
            }
        }

        Logger.Shared.Information("Play done.");
    }

    public FileManagerViewModel()
    {
        OpenCommand = new RelayCommand(OnOpenCmd);
        PlayCommand = new AsyncRelayCommand(OnPlayCmd);
    }

}
