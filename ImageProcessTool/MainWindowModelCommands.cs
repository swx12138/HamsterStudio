﻿using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Models;
using HamsterStudio.Toolkits;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageProcessTool
{
    class MainWindowModelCommands
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

        public MainWindowModelCommands(MainWindowModel mainWindowModel)
        {
            SaveCommand = new RelayCommand(() =>
            {
                if (File.Exists(mainWindowModel.PreviewImageProps.SavingFilename))
                {
                    if (MessageBox.Show("File already exists. overwrite it?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                mainWindowModel!.PreviewImage?.SaveImageSource(mainWindowModel.PreviewImageProps.SavingFilename);
                mainWindowModel.SavingCount++;
                Trace.TraceInformation($"File saved @ {mainWindowModel.PreviewImageProps.SavingFilename}");
            });
            RepeatItemCommand = new RelayCommand<ImageInfo>(item =>
            {
                ImageInfo imageInfo = new(item!.Filename) { RepeatCommand = RepeatItemCommand!, DestroyCommand = DestroyItemCommand! };
                mainWindowModel.ImagePaths.Add(imageInfo);
                RefreshPreviewCommand?.Execute(null);
            });
            DestroyItemCommand = new RelayCommand<ImageInfo>(item =>
            {
                mainWindowModel.ImagePaths.Remove(item!);
                RefreshPreviewCommand?.Execute(null);
            });

            OpenFilesCommand = new RelayCommand(() =>
            {
                var diag = new OpenFileDialog();
                diag.Multiselect = true;
                if (diag.ShowDialog() ?? false)
                {
                    mainWindowModel.LoadFiles(diag.FileNames);
                }
            });
            CloseFilesCommand = new RelayCommand(() =>
            {
                mainWindowModel.ImagePaths.Clear();
                mainWindowModel.SavingCount = 0;
                mainWindowModel.PreviewImageProps.SavingFilename = string.Empty;
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
                    mainWindowModel.PreviewImage = mainWindowModel.ImagePaths.CreateImageSource(mainWindowModel.PreviewImageProps.Colums, mainWindowModel.PreviewImageProps.Uniform);
                }
                catch (Exception ex)
                {
                    mainWindowModel.ShowException(ex);
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
                    mainWindowModel.ShowException(ex);
                }
            });
            SetImageWidthLimitCommand = new RelayCommand<int>(newWidth =>
            {
                Trace.TraceInformation($"Set image width limit {mainWindowModel.ImageWidthLimit} => {newWidth}");
                mainWindowModel.ImageWidthLimit = newWidth;
            });
        }
    }
}
