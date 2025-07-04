﻿using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Algorithm.Random;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Gallery.Models;

public class FileManagerViewModel : ViewModel
{
    public ICommand PlayCommand { get; }
    public ICommand OpenCommand { get; }

    public FileManagerModel Model { get; }

    private void OnOpenCmd()
    {
        OpenFolderDialog dialog = new();
        dialog.Title = "选择文件夹";
        dialog.Multiselect = false;
        if (!(dialog.ShowDialog() ?? false))
        {
            return;
        }

        Model.ReadFolder(dialog.FolderName);
        Logger.Shared.Trace($"现在一共有{Model.FileGroups.Count}个分组，。");

    }

    private async Task OnPlayCmd()
    {
        if (Model.FileCount <= 0)
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

                var filename = Model.FileGroups.SelectMany(x => x.Files).Choice().FullName;
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

    public FileManagerViewModel(FileManagerModel fileManager)
    {
        Model = fileManager;
        OpenCommand = new RelayCommand(OnOpenCmd);
        PlayCommand = new AsyncRelayCommand(OnPlayCmd);
    }

}
