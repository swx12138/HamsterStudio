using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Photogrammetry.CameraLensMath;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace HamsterStudio.Photogrammetry.ViewModels;

/// <summary>
/// 视角（AoV）计算器：可同时维护多个焦距，并在同一画布中对比绘制。
/// </summary>
public partial class AngleOfViewCalculatorViewModel : ViewModel
{
    const string CategoryName = "视角设置";

    private static readonly Brush[] Palette =
    [
        Brushes.SteelBlue,
        Brushes.IndianRed,
        Brushes.SeaGreen,
        Brushes.DarkOrange,
        Brushes.MediumPurple,
        Brushes.Teal,
        Brushes.Crimson,
        Brushes.Goldenrod,
        Brushes.SaddleBrown,
        Brushes.DeepPink,
    ];

    [ObservableProperty]
    [property: Category(CategoryName)]
    [property: DisplayName("方向")]
    private Orientation _orientation = Orientation.Horizontal;

    [ObservableProperty]
    [Browsable(false)]
    private double _newFocalLength = 50.0;

    [Browsable(false)]
    public ObservableCollection<AngleOfViewEntryViewModel> Entries { get; } = [];

    /// <summary>
    /// 按焦距大小升序排列的视图，用于列表展示。
    /// </summary>
    [Browsable(false)]
    public ICollectionView EntriesView { get; }

    [Browsable(false)]
    public ICommand AddEntryCommand { get; }

    [Browsable(false)]
    public ICommand ClearEntriesCommand { get; }

    [Browsable(false)]
    public ICommand RemoveEntryCommand { get; }

    private readonly IServiceProvider provider_;

    public AngleOfViewCalculatorViewModel(ILogger<AngleOfViewCalculatorViewModel>? logger, IServiceProvider provider) : base(logger)
    {
        provider_ = provider;

        EntriesView = CollectionViewSource.GetDefaultView(Entries);
        if (EntriesView is ListCollectionView listView)
        {
            listView.SortDescriptions.Add(new SortDescription(nameof(AngleOfViewEntryViewModel.FocalLength), ListSortDirection.Ascending));
            listView.IsLiveSorting = true;
            listView.LiveSortingProperties.Add(nameof(AngleOfViewEntryViewModel.FocalLength));
        }

        AddEntryCommand = new RelayCommand(() => AddEntry(NewFocalLength));
        ClearEntriesCommand = new RelayCommand(() => Entries.Clear());
        RemoveEntryCommand = new RelayCommand<AngleOfViewEntryViewModel>(entry => Entries.Remove(entry));

        // 预置常见焦距，便于快速对比
        foreach (var focalLength in new[] { 14.0, 24.0, 35.0, 50.0, 85.0, 135.0 })
        {
            AddEntry(focalLength);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Orientation))
        {
            foreach (var entry in Entries)
            {
                entry.Orientation = Orientation;
            }
        }
        base.OnPropertyChanged(e);
    }

    private void AddEntry(double focalLength)
    {
        if (!double.IsFinite(focalLength) || focalLength <= 0)
        {
            return;
        }

        var entry = provider_.GetRequiredService<AngleOfViewEntryViewModel>();
        entry.Orientation = Orientation;
        entry.FocalLength = focalLength;
        entry.Brush = Palette[Entries.Count % Palette.Length];
        Entries.Add(entry);
    }
}
