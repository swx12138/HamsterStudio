using HamsterStudioGUI.ViewModels.SpacialDownloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudioGUI.ViewModels;

public class SpacialDownloadsViewModel
{
    public DirectLinkDownloadViewModel DirectLinkDownload { get; } = new();
    public BilibiliSpDownloadsViewModel BilibiliSpDownloadsViewModel { get; } = new();
    public XiaoHongshuSpDownloadViewModel XiaoHongshuSpDownloadViewModel { get; } = new();

    public SpacialDownloadsViewModel()
    {
    }

}
