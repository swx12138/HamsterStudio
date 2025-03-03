using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Models;
using HamsterStudio.Toolkits;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

    partial class PreviewImageProperties : ObservableObject
    {
        [ObservableProperty]
        private int _colums = 3;

        [ObservableProperty]
        private bool _uniform = true;

        [ObservableProperty]
        private string _savingFilename = string.Empty;

        public event EventHandler? NotifyRedraw;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Colums) || e.PropertyName == nameof(Uniform))
            {
                NotifyRedraw?.Invoke(this, EventArgs.Empty);
            }
            base.OnPropertyChanged(e);
        }
    }

    partial class MainWindowModel : ObservableObject
    {
        [ObservableProperty]
        private bool _topmost;

        private ObservableCollection<ImageInfo> _imagePaths = [];
        public ObservableCollection<ImageInfo> ImagePaths
        {
            get => _imagePaths;
            set
            {
                foreach (var item in _imagePaths)
                {
                    item.PropertyChanged -= ImageInfo_PropertyChanged;
                }

                SetProperty(ref _imagePaths, value);

                foreach (var item in _imagePaths)
                {
                    item.PropertyChanged += ImageInfo_PropertyChanged;
                }
            }
        }

        [ObservableProperty]
        private ImageSource? _previewImage;

        [ObservableProperty]
        private PreviewImageProperties _previewImageProps = new();

        public MainWindowModelCommands Commands { get; }

        private const string SavingPath = @"D:\Publish\bizhi";

        [ObservableProperty]
        private int _imageWidthLimit = 3840;

        [ObservableProperty]
        private int savingCount = 0;

        public MainWindowModel()
        {
            Commands = new(this);

            ImagePaths.CollectionChanged += ImagePaths_CollectionChanged;

            OnPropertyChanged(nameof(PreviewImageProps));
        }

        private void ImagePaths_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ImageInfo newItem in e.NewItems)
                {
                    newItem.PropertyChanged += ImageInfo_PropertyChanged;
                }

                if (PreviewImageProps.SavingFilename == string.Empty)
                {
                    PreviewImageProps.SavingFilename = Path.Combine(
                        SavingPath,
                        "Concat_" + string.Join("_", Path.GetFileName(ImagePaths[0].Filename).Split("_").Where(static x => !x.StartsWith('p'))));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ImageInfo newItem in e.OldItems)
                {
                    newItem.PropertyChanged -= ImageInfo_PropertyChanged;
                }
            }
        }

        private void ImageInfo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Commands.RefreshPreviewCommand.Execute(null);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PreviewImageProps))
            {
                PreviewImageProps.NotifyRedraw += (s, e) => { Commands.RefreshPreviewCommand.Execute(null); };
                Commands.RefreshPreviewCommand.Execute(null);
            }
            base.OnPropertyChanged(e);
        }

        public bool LoadFiles(IReadOnlyCollection<string> files)
        {
            foreach (var filename in files)
            {
                ImagePaths.Add(new(filename)
                {
                    RepeatCommand = Commands.RepeatItemCommand,
                    DestroyCommand = Commands.DestroyItemCommand,
                });
            }
            Commands.RefreshPreviewCommand.Execute(null);
            return true;
        }

        public void ShowException(Exception ex)
        {
            Trace.TraceError(ex.Message);
            Trace.TraceError(ex.StackTrace);
        }

    }
}
