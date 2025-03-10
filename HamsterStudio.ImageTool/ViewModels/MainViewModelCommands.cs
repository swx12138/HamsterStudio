using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Models;
using HamsterStudio.Toolkits;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace HamsterStudio.ImageTool.ViewModels
{
    class MainViewModelCommands
    {
        public ICommand SaveCommand { get; }

        public ICommand RepeatItemCommand { get; }
        public ICommand DestroyItemCommand { get; }

        public ICommand OpenFilesCommand { get; }
        public ICommand CloseFilesCommand { get; }
        public ICommand ReselectFilesCommand { get; }

        public ICommand RefreshPreviewCommand { get; }

        public ICommand ScaleImagesCommand { get; }
        public ICommand SetImageWidthLimitCommand { get; }

        public ICommand LoadCurrentWallpaperCommand { get; }

        public MainViewModelCommands(MainViewModel mainViewModel)
        {
            SaveCommand = new RelayCommand(() =>
            {
                if (File.Exists(mainViewModel.PreviewImageProps.SavingFilename))
                {
                    if (MessageBox.Show("File already exists. overwrite it?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                mainViewModel!.PreviewImage?.SaveImageSource(mainViewModel.PreviewImageProps.SavingFilename);
                mainViewModel.SavingCount++;
                Trace.TraceInformation($"File saved @ {mainViewModel.PreviewImageProps.SavingFilename}");
            });
            RepeatItemCommand = new RelayCommand<ImageInfo>(item =>
            {
                ImageInfo imageInfo = new(item!.Filename) { RepeatCommand = RepeatItemCommand!, DestroyCommand = DestroyItemCommand! };
                mainViewModel.ImagePaths.Add(imageInfo);
                RefreshPreviewCommand?.Execute(null);
            });
            DestroyItemCommand = new RelayCommand<ImageInfo>(item =>
            {
                mainViewModel.ImagePaths.Remove(item!);
                RefreshPreviewCommand?.Execute(null);
            });

            OpenFilesCommand = new RelayCommand(() =>
            {
                var diag = new OpenFileDialog();
                diag.Multiselect = true;
                if (diag.ShowDialog() ?? false)
                {
                    mainViewModel.LoadFiles(diag.FileNames);
                }
            });
            CloseFilesCommand = new RelayCommand(() =>
            {
                mainViewModel.ImagePaths.Clear();
                mainViewModel.SavingCount = 0;
                mainViewModel.PreviewImageProps.SavingFilename = string.Empty;
            });
            ReselectFilesCommand = new RelayCommand(() =>
            {
                CloseFilesCommand.Execute(null);
                OpenFilesCommand.Execute(null);
            });

            RefreshPreviewCommand = new RelayCommand(() =>
            {
                try
                {
                    mainViewModel.PreviewImage = mainViewModel.ImagePaths.CreateImageSource(mainViewModel.PreviewImageProps.Colums, mainViewModel.PreviewImageProps.Uniform);
                }
                catch (Exception ex)
                {
                    mainViewModel.ShowException(ex);
                }
            });
            ScaleImagesCommand = new RelayCommand(() =>
            {
                try
                {
                    int n = 0;
                    if (!Directory.Exists(@"D:\Publish\bizhi\Abandoned"))
                    {
                        Directory.CreateDirectory(@"D:\Publish\bizhi\Abandoned");
                    }

                    var filenames = Directory.GetFiles(@"D:\Publish\bizhi");
                    filenames.AsParallel().ForAll(filename =>
                    {
                        ImageUtils.ScaleImage(filename, x => Math.Floor(38400.0 / x.Width) / 10);
                        Trace.TraceInformation($"[{n++}/{filenames.Length}] {filename}");
                    });
                }
                catch (Exception ex)
                {
                    mainViewModel.ShowException(ex);
                }
            });
            SetImageWidthLimitCommand = new RelayCommand<int>(newWidth =>
            {
                Trace.TraceInformation($"Set image width limit {mainViewModel.ImageWidthLimit} => {newWidth}");
                mainViewModel.ImageWidthLimit = newWidth;
            });

            LoadCurrentWallpaperCommand = new RelayCommand(() =>
            {
                if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers")?.GetValue("BackgroundHistoryPath0") is not string path)
                {
                    return;
                }
                CloseFilesCommand.Execute(null);
                mainViewModel.LoadFiles([path]);
            });
        }
    }
}
