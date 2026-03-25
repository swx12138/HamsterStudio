using CommunityToolkit.Mvvm.ComponentModel;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace HamsterStudio.Gallery.ViewModels;

public partial class LargeImageViewModel(ILogger<LargeImageViewModel> logger) : KnownViewModel(logger)
{
    [ObservableProperty]
    private string _currentImage = @"E:\HamsterStudioHome\xiaohongshu\樱\小千_3_xhs_樱._1040g3k031u4alka9ig005o7u94bgbimbsoliir8.png";

}
