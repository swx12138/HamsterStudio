using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Algorithm.Random;
using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Models;
using HamsterStudio.Toolkits.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Gallery.ViewModels;

public partial class GalleryViewModel : KnownViewModel
{
    public FileManagerViewModel FileManager { get; }

    [ObservableProperty]
    private Visibility _galleriesVisible = Visibility.Visible;

    [ObservableProperty]
    private Visibility _galleryVisible = Visibility.Visible;

    [ObservableProperty]
    private ThemeMgmt _themeMgmt;

    public ICommand OpenCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand PlayCommand { get; }

    public GalleryViewModel(ILogger<GalleryViewModel> logger) : base(logger)
    {
    }

    public GalleryViewModel(FileManagerViewModel fileManager, ThemeMgmt themeMgmt, ILogger<GalleryViewModel> logger) : base(logger)
    {
        DisplayName = "图库";

        ThemeMgmt = themeMgmt;
        FileManager = fileManager;

        OpenCommand = new RelayCommand(OnOpenCmd);
        ClearCommand = new RelayCommand(OnClearCommand);
        PlayCommand = new AsyncRelayCommand(OnPlayCmd);
    }

    private void OnClearCommand()
    {
        FileManager.FileGroups.Clear();
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

        FileManager.ReadFolder(dialog.FolderName);
        logger?.LogTrace($"现在一共有{FileManager.FileGroups.Count}个分组，。");
    }

    private async Task OnPlayCmd()
    {
        if (FileManager.FileCount <= 0)
        {
            logger?.LogWarning("No file to be shown.");
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

                var filename = FileManager.FileGroups.SelectMany(x => x.Files).Choice().FullName;
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
                logger?.LogCritical(ex.ToFullString());
            }
        }

        logger?.LogInformation("Play done.");
    }

}
