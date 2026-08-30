using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Photogrammetry.Models;
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

    public ICollectionView FilteredReferences { get; }

    public ReferenceProjectViewModel(ILogger<ReferenceProjectViewModel> logger) : base(logger)
    {
        DisplayName = "参考图";

        FilteredReferences = CollectionViewSource.GetDefaultView(References);
        FilteredReferences.Filter = FilterPredicate;
    }

    partial void OnSearchTextChanged(string value) => FilteredReferences.Refresh();

    partial void OnDemandFilterIndexChanged(int value) => FilteredReferences.Refresh();

    partial void OnShotFilterIndexChanged(int value) => FilteredReferences.Refresh();

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

        if (!string.IsNullOrWhiteSpace(SearchText)
            && !item.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".webp",
    };

    [RelayCommand]
    private void AddReferences()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择参考图",
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.webp|所有文件|*.*",
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
        AddFiles(files.Where(f => ImageExtensions.Contains(System.IO.Path.GetExtension(f))));
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
            Filter = "JSON 文件|*.json|所有文件|*.*",
            FileName = $"{Name}.json",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(ToModel(), JsonOptions);
            File.WriteAllText(dialog.FileName, json);
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
            Filter = "JSON 文件|*.json|所有文件|*.*",
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
            logger?.LogInformation("已加载参考图项目 {Path}", dialog.FileName);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "加载参考图项目失败");
            MessageBox.Show($"加载失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
        References = References.Select(x => x.ToModel()).ToList(),
    };

    public void ApplyModel(ReferenceProjectModel model)
    {
        Name = model.Name;
        Description = model.Description;
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
