using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HamsterStudio.Gallery.Models;

public partial class FileModel : KnownViewModel
{
    public string Filename { get; }

    public ICommand ShowImageCommand { get; }

    public FileModel(string filename)
    {
        Filename = filename;
        DisplayName = Path.GetFileName(filename);
        ShowImageCommand = new RelayCommand(() =>
        {
            Window wnd = new();
            wnd.Content = new Image
            {
                Source = new BitmapImage(new Uri(filename))
            };
            wnd.ShowDialog();
        });
    }

}

public partial class FileGroupModel : KnownViewModel
{
    public string GroupName { get; }

    public ObservableCollection<FileModel> Files { get; } = [];

    public ICommand ViewCommand { get; }

    public FileGroupModel(string groupName)
    {
        DisplayName = Path.GetFileName(groupName);
        GroupName = groupName;
        ViewCommand = new RelayCommand(() =>
        {
            FileGroupWindow window = new();
            window.DataContext = this;
            _ = window.ShowDialog();
        });
    }

}
