using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Gallery.Views;
using System.IO;
using System.Windows.Input;

namespace HamsterStudio.Gallery.Models;

public partial class FileGroupViewModel : KnownViewModel
{
    public ICommand ViewCommand { get; }

    public FileGroupViewModel(FileGroupModel fileGroup)
    {
        DisplayName = Path.GetFileName(fileGroup.GroupName);
        ViewCommand = new RelayCommand(() =>
        {
            FileGroupWindow window = new();
            window.DataContext = this;
            _ = window.ShowDialog();
        });
    }
}