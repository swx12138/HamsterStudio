using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.FileSystem.Filters;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Gallery.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Gallery.Services;

public partial class GalleriaFileMgmt : ObservableObject
{
    private readonly DataStorageMgmt _dataStorageMgmt;

    [ObservableProperty]
    private ObservableCollection<string> _includePaths = [];

    [ObservableProperty]
    private ObservableCollection<string> _excludePaths = [];

    public FileManagerViewModel FileManager { get; }

    public GalleriaFileMgmt(DataStorageMgmt dataStorageMgmt)
    {
        _dataStorageMgmt = dataStorageMgmt;
        Load();

        var fileMgr = new FileManagerViewModel();
        fileMgr.Filters.Add(new ImageFileFilter());
        foreach (var path in IncludePaths)
        {
            fileMgr.ReadFolder(path);
        }
        foreach (var path in ExcludePaths)
        {

        }
        FileManager = fileMgr;

    }

    public void Save()
    {
        try
        {
            _dataStorageMgmt.Set("Galleria.IncludePaths", IncludePaths.ToArray());
            _dataStorageMgmt.Set("Galleria.ExcludePaths", ExcludePaths.ToArray());
            Logger.Shared.Information($"Save Galleria data success.");
        }
        catch (Exception ex)
        {
            Logger.Shared.Error($"Save Galleria.IncludePaths failed.");
            Logger.Shared.Critical(ex);
        }
    }

    public void Load()
    {
        try
        {
            var includePaths = _dataStorageMgmt.Get<string[]>("Galleria.IncludePaths");
            if (includePaths != null)
            {
                IncludePaths = new ObservableCollection<string>(includePaths);
            }
            var excludePaths = _dataStorageMgmt.Get<string[]>("Galleria.ExcludePaths");
            if (excludePaths != null)
            {
                ExcludePaths = new ObservableCollection<string>(excludePaths);
            }
            Logger.Shared.Information($"Load Galleria data success.");
        }
        catch (Exception ex)
        {
            Logger.Shared.Error($"Load Galleria data failed.");
            Logger.Shared.Critical(ex);
        }
    }
}
