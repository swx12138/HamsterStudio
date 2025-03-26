using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.Algorithm.Random;
using HamsterStudio.Barefeet.MVVM;
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

    public List<IFileManagerFilter> Filters { get; } = [];

    public IFileManagerGrouper Grouper { get; } = new DirFileManagerGrouper();

    public ICommand PlayCommand { get; }

    public FileManagerModel()
    {
        PlayCommand = new AsyncRelayCommand(async () =>
        {
            Window wnd = new();
            wnd.Show();
            await Task.Run(async () =>
             {

                 int times = 9999;
                 while (times-- > 0)
                 {
                     (wnd.Dispatcher ?? Application.Current.Dispatcher).Invoke(() =>
                     {
                         var filename = FileGroups.Choice().Files.Choice().Filename;
                         ImageSource source = new BitmapImage(new Uri(filename)) { CacheOption = BitmapCacheOption.OnLoad };
                         source.Freeze();
                         wnd.Content = new Image { Source = source };
                     });

                     await Task.Delay(1000);
                 }
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
