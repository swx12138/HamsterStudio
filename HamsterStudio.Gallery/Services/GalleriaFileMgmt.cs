using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.FileSystem.Filters;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Gallery.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace HamsterStudio.Gallery.Services;

public partial class GalleriaFileMgmt : BindableBase
{
    private readonly DataStorageMgmt _dataStorageMgmt;

    [ObservableProperty]
    private ObservableCollection<string> _includePaths = [];

    [ObservableProperty]
    private ObservableCollection<string> _excludePaths = [];

    public FileManagerViewModel FileManager { get; }

    public GalleriaFileMgmt(DataStorageMgmt dataStorageMgmt, ILogger<DataStorageMgmt> logger) : base(logger)
    {
        _dataStorageMgmt = dataStorageMgmt;
        Load();

        var fileMgr = new FileManagerViewModel(logger);
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
            logger?.LogInformation($"Save Galleria data success.");
        }
        catch (Exception ex)
        {
            logger?.LogError($"Save Galleria.IncludePaths failed.");
            logger?.LogCritical(ex.ToFullString());
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
            logger?.LogInformation($"Load Galleria data success.");
        }
        catch (Exception ex)
        {
            logger?.LogError($"Load Galleria data failed.");
            logger?.LogCritical(ex.ToFullString());
        }
    }
}
