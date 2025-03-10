using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace HamsterStudio.ImageTool.ViewModels
{
    partial class MainViewModel : ObservableObject
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
        private PreviewImagePropertiesModel _previewImageProps = new();

        public MainViewModelCommands Commands { get; }

        private const string SavingPath = @"D:\Publish\bizhi";

        [ObservableProperty]
        private int _imageWidthLimit = 3840;

        [ObservableProperty]
        private int savingCount = 0;

        public MainViewModel()
        {
            Commands = new(this);

            ImagePaths.CollectionChanged += ImagePaths_CollectionChanged;

            OnPropertyChanged(nameof(PreviewImageProps));
        }

        private void ImagePaths_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems == null)
                {
                    return;
                }

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
                if (e.OldItems == null)
                {
                    return;
                }

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
