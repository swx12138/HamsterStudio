using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Photogrammetry.Models;
using HamsterStudio.Toolkits;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HamsterStudio.Photogrammetry.ViewModels;

/// <summary>
/// 参考图项目视图模型，用于组建 <see cref="ReferenceProjectModel"/>。
/// </summary>
public partial class ReferenceProjectViewModel : KnownViewModel
{
    private const string AllScenesFilter = "全部";

    private const string LastProjectPathKey = "Photogrammetry.LastProjectPath";

    private readonly DataStorageMgmt _dataStorageMgmt;

    [ObservableProperty]
    private string _name = ReferenceProjectModel.EmptyProjectName;

    [ObservableProperty]
    private string _description = string.Empty;

    public ObservableCollection<ReferenceItemViewModel> References { get; } = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// 需求程度筛选：0=全部，1=普通，2=必须。
    /// </summary>
    [ObservableProperty]
    private int _demandFilterIndex;

    /// <summary>
    /// 拍摄状态筛选：0=全部，1=未拍摄，2=已拍摄。
    /// </summary>
    [ObservableProperty]
    private int _shotFilterIndex;

    [ObservableProperty]
    private ReferenceItemViewModel? _selectedReference;

    /// <summary>
    /// 项目场景列表。
    /// </summary>
    public ObservableCollection<string> Scenes { get; } = [];

    /// <summary>
    /// 场景筛选下拉项（“全部”+各场景）。
    /// </summary>
    public ObservableCollection<string> SceneFilterOptions { get; } = [AllScenesFilter];

    /// <summary>
    /// 当前场景筛选，等于 AllScenesFilter 表示不筛选。
    /// </summary>
    [ObservableProperty]
    private string _selectedSceneFilter = AllScenesFilter;

    [ObservableProperty]
    private string _newSceneName = string.Empty;

    public ICollectionView FilteredReferences { get; }

    public ReferenceProjectViewModel(DataStorageMgmt dataStorageMgmt, ILogger<ReferenceProjectViewModel> logger) : base(logger)
    {
        _dataStorageMgmt = dataStorageMgmt;

        DisplayName = "参考图";

        FilteredReferences = CollectionViewSource.GetDefaultView(References);
        FilteredReferences.Filter = FilterPredicate;

        Scenes.CollectionChanged += (_, _) => SyncSceneFilterOptions();

        LoadLastProject();
    }

    partial void OnSearchTextChanged(string value) => FilteredReferences.Refresh();

    partial void OnDemandFilterIndexChanged(int value) => FilteredReferences.Refresh();

    partial void OnShotFilterIndexChanged(int value) => FilteredReferences.Refresh();

    partial void OnSelectedSceneFilterChanged(string value) => FilteredReferences.Refresh();

    private bool FilterPredicate(object obj)
    {
        if (obj is not ReferenceItemViewModel item)
        {
            return false;
        }

        switch (DemandFilterIndex)
        {
            case 1 when item.DemandDegree != ReferenceDemandDegree.Normal:
                return false;
            case 2 when item.DemandDegree != ReferenceDemandDegree.Must:
                return false;
        }

        switch (ShotFilterIndex)
        {
            case 1 when item.IsShot:
                return false;
            case 2 when !item.IsShot:
                return false;
        }

        if (SelectedSceneFilter != AllScenesFilter
            && !item.Scenes.Contains(SelectedSceneFilter))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SearchText)
            && !item.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void AddReferences()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择参考图",
            Filter = ImageUtils.KnownImageExtsFilter,
            Multiselect = true,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AddFiles(dialog.FileNames);
    }

    /// <summary>
    /// 处理拖放进来的文件，仅保留图片。
    /// </summary>
    public void DropFiles(string[] files)
    {
        AddFiles(files.Where(f => ImageUtils.KnownImageExts.Contains(Path.GetExtension(f))));
    }

    private void AddFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            References.Add(new ReferenceItemViewModel { Path = file });
        }

        SelectedReference ??= References.LastOrDefault();
    }

    [RelayCommand]
    private void AddScene()
    {
        var scene = NewSceneName?.Trim();
        if (string.IsNullOrWhiteSpace(scene) || Scenes.Contains(scene))
        {
            return;
        }

        Scenes.Add(scene);
        NewSceneName = string.Empty;
    }

    [RelayCommand]
    private void RemoveScene(string? scene)
    {
        if (scene is null)
        {
            return;
        }

        foreach (var item in References)
        {
            item.Scenes.Remove(scene);
        }

        Scenes.Remove(scene);
    }

    private void SyncSceneFilterOptions()
    {
        var selected = SelectedSceneFilter;

        SceneFilterOptions.Clear();
        SceneFilterOptions.Add(AllScenesFilter);
        foreach (var scene in Scenes)
        {
            SceneFilterOptions.Add(scene);
        }

        SelectedSceneFilter = SceneFilterOptions.Contains(selected) ? selected : AllScenesFilter;

        FilteredReferences.Refresh();
    }

    [RelayCommand]
    private void RemoveReference(ReferenceItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        References.Remove(item);
    }

    [RelayCommand]
    private void MoveUp(ReferenceItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var index = References.IndexOf(item);
        if (index <= 0)
        {
            return;
        }

        References.Move(index, index - 1);
        FilteredReferences.Refresh();
    }

    [RelayCommand]
    private void MoveDown(ReferenceItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var index = References.IndexOf(item);
        if (index < 0 || index >= References.Count - 1)
        {
            return;
        }

        References.Move(index, index + 1);
        FilteredReferences.Refresh();
    }

    [RelayCommand]
    private void MoveToFirst(ReferenceItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var index = References.IndexOf(item);
        if (index <= 0)
        {
            return;
        }

        References.Move(index, 0);
        FilteredReferences.Refresh();
    }

    [RelayCommand]
    private void Clear()
    {
        References.Clear();
        SelectedReference = null;
    }

    [RelayCommand]
    private void Save()
    {
        var dialog = new SaveFileDialog
        {
            Title = "保存参考图项目",
            Filter = "参考项目文件|*.hpck_proj|所有文件|*.*",
            FileName = $"{Name}.hpck_proj",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(ToModel(), JsonOptions);
            File.WriteAllText(dialog.FileName, json);
            RememberLastProjectPath(dialog.FileName);
            logger?.LogInformation("已保存参考图项目到 {Path}", dialog.FileName);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "保存参考图项目失败");
            MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Load()
    {
        var dialog = new OpenFileDialog
        {
            Title = "打开参考图项目",
            Filter = "参考项目文件|*.hpck_proj;*.json|所有文件|*.*",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(dialog.FileName);
            var project = JsonSerializer.Deserialize<ReferenceProjectModel>(json, JsonOptions);
            if (project is null)
            {
                return;
            }

            ApplyModel(project);
            RememberLastProjectPath(dialog.FileName);
            logger?.LogInformation("已加载参考图项目 {Path}", dialog.FileName);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "加载参考图项目失败");
            MessageBox.Show($"加载失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadLastProject()
    {
        try
        {
            var path = _dataStorageMgmt.Get<string>(LastProjectPathKey);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            var project = JsonSerializer.Deserialize<ReferenceProjectModel>(File.ReadAllText(path), JsonOptions);
            if (project is null)
            {
                return;
            }

            ApplyModel(project);
            logger?.LogInformation("已自动打开上次工程 {Path}", path);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("自动打开上次工程失败：{Message}", ex.Message);
        }
    }

    private void RememberLastProjectPath(string path)
    {
        try
        {
            _dataStorageMgmt.Set(LastProjectPathKey, path);
            _dataStorageMgmt.Persist();
        }
        catch (Exception ex)
        {
            logger?.LogWarning("记录最近工程路径失败：{Message}", ex.Message);
        }
    }

    [RelayCommand]
    private void OpenWithSystem(ReferenceItemViewModel? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(item.Path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "无法用系统默认程序打开 {Path}", item.Path);
        }
    }

    [RelayCommand]
    private void ShowFullscreen(ReferenceItemViewModel? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Path) || !File.Exists(item.Path))
        {
            return;
        }

        var viewer = new HandyControl.Controls.ImageViewer
        {
            Uri = new Uri(item.Path, UriKind.Absolute),
            ShowToolBar = true,
        };

        var window = new Window
        {
            Content = viewer,
            Title = item.FileName,
            Width = 1280,
            Height = 720,
            WindowState = WindowState.Maximized,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = Brushes.Black,
        };

        window.Show();
    }

    public ReferenceProjectModel ToModel() => new()
    {
        Name = Name,
        Description = Description,
        Scenes = Scenes.ToList(),
        References = References.Select(x => x.ToModel()).ToList(),
    };

    public void ApplyModel(ReferenceProjectModel model)
    {
        Name = model.Name;
        Description = model.Description;

        Scenes.Clear();
        foreach (var scene in model.Scenes)
        {
            Scenes.Add(scene);
        }

        References.Clear();
        foreach (var reference in model.References)
        {
            References.Add(ReferenceItemViewModel.FromModel(reference));
        }

        SelectedReference = References.FirstOrDefault();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
