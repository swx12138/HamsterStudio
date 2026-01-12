using HamsterStudio.Barefeet.MVVM;
using HamsterStudioGUI.ViewModels.SpacialDownloads;
using Microsoft.Extensions.Logging;

namespace HamsterStudioGUI.ViewModels;

public class SpacialDownloadsViewModel(ILoggerFactory loggerFactory) : KnownViewModel(loggerFactory.CreateLogger<SpacialDownloadsViewModel>())
{
    public DirectLinkDownloadViewModel DirectLinkDownload { get; } = new(loggerFactory.CreateLogger<DirectLinkDownloadViewModel>());
    public BilibiliSpDownloadsViewModel BilibiliSpDownloadsViewModel { get; } = new(loggerFactory.CreateLogger<BilibiliSpDownloadsViewModel>());
    public XiaoHongshuSpDownloadViewModel XiaoHongshuSpDownloadViewModel { get; } = new(loggerFactory.CreateLogger<XiaoHongshuSpDownloadViewModel>());

}
