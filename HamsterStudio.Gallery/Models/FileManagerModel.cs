using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Algorithm.Random;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Gallery.Models;

public class FileManagerModel : ViewModel
{
    public ObservableCollection<FileGroupModel> FileGroups { get; } = [];

    public int FileCount => FileGroups.Sum(x => x.Files.Count);

    public List<IFileManagerFilter> Filters { get; } = [];

    public IFileManagerGrouper Grouper { get; } = new DirFileManagerGrouper();

    public ICommand PlayCommand { get; }
    public ICommand OpenCommand { get; }

    public FileManagerModel()
    {
        OpenCommand = new RelayCommand(() =>
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

        });
        PlayCommand = new AsyncRelayCommand(async () =>
        {
            if (FileCount <= 0)
            {
                Logger.Shared.Warning("No file to be shown.");
            return;
            }

            Window wnd = new();
            bool pause = false;
            wnd.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Space)
                {
                    pause = !pause;
                }
            };
            wnd.Show();
            await Task.Run(async () =>
             {
                 CancellationTokenSource source = new();
                 int times = 9999;
                 while (times-- > 0 && !source.Token.IsCancellationRequested)
                 {
                     if (pause)
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

                         var filename = FileGroups.SelectMany(x => x.Files).Choice().Filename;
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
             });
        });
    }

    public void AddFile(string filename)
    {
        if (!Filters.Any(x => x.Test(filename)))
        {
            return;
        }

        var groupName = Grouper.Group(filename);
        var group = FileGroups.FirstOrDefault(x => x.GroupName == groupName);
        if (group == null)
        {
            group = new FileGroupModel(groupName);
            group.Files.Add(new(filename));
            FileGroups.Add(group);
        }
        else
        {
            group.Files.Add(new(filename));
        }

    }

    public void ReadFolder(string folder)
    {
        if(!Directory.Exists(folder))
        {
            return;
        }

        var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            AddFile(file);
        }
    }

}
