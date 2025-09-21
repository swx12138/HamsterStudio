﻿using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.SysCall;
using System.Collections.ObjectModel;
using System.IO;

namespace HamsterStudio.Gallery.Models;

public class FileManagerViewModel : ViewModel
{
    public ObservableCollection<FileGroupViewModel> FileGroups { get; } = [];

    public int FileCount => FileGroups.Sum(x => x.Files.Count);

    public List<IFileManagerFilter> Filters { get; } = [];

    public IFileManagerGrouper Grouper { get; } = new DirFileManagerGrouper();

    public void ReadFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Where(x => Filters.Any(filter => filter.Test(x)));
        var gfiles = files.GroupBy(x => Grouper.Group(x));
        foreach (var gfile in gfiles)
        {
            var group = FileGroups.FirstOrDefault(x => x.GroupName == gfile.Key);
            if (group == null)
            {
                group = new(gfile.Key);
                FileGroups.Add(group);
            }
            group.UpdateFiles([.. gfile.Select(x=>x)]);
        }
    }

}
