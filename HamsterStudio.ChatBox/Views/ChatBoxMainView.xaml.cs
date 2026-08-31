using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HamsterStudio.ChatBox.ViewModels;
using HamsterStudio.Toolkits;

namespace HamsterStudio.ChatBox.Views;

/// <summary>
/// ChatBoxMainView.xaml 的交互逻辑
/// </summary>
public partial class ChatBoxMainView : UserControl
{
    public ChatBoxMainView()
    {
        InitializeComponent();
    }

    private void AttachImages_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择图片",
            Multiselect = true,
            Filter = ImageUtils.KnownImageExtsFilter,
        };

        if (dialog.ShowDialog() == true && DataContext is ChatBoxMainViewModel viewModel)
            viewModel.AttachImages(dialog.FileNames);
    }

    private void ExportChat_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "导出对话",
            Filter = "ChatBox 对话文件|*.hchat",
            DefaultExt = ".hchat",
            FileName = $"chat_{DateTime.Now:yyyyMMdd_HHmmss}.hchat",
        };

        if (dialog.ShowDialog() == true && DataContext is ChatBoxMainViewModel viewModel)
            viewModel.ExportChat(dialog.FileName);
    }

    private void ImportChat_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "导入对话",
            Filter = "ChatBox 对话文件|*.hchat|所有文件|*.*",
        };

        if (dialog.ShowDialog() == true && DataContext is ChatBoxMainViewModel viewModel)
            viewModel.ImportChat(dialog.FileName);
    }

    private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 回车发送，Shift+回车换行
        if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
        {
            if (DataContext is ChatBoxMainViewModel viewModel && viewModel.SendCommand.CanExecute(null))
            {
                viewModel.SendCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void MessagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentHeightChange == 0)
            return;

        // 用户在底部附近时才自动滚动到底部，避免打断向上翻阅
        var wasAtBottom = e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - e.ExtentHeightChange - 24;
        if (wasAtBottom)
            MessagesScroll.ScrollToEnd();
    }
}
