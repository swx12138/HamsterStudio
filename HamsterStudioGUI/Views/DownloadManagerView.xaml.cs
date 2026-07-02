using System.Windows.Controls;
using HamsterStudio.Web.Models;

namespace HamsterStudioGUI.Views;

public partial class DownloadManagerView : UserControl
{
    public DownloadManagerView()
    {
        InitializeComponent();
    }

    private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ViewModels.DownloadManagerViewModel vm &&
            sender is ComboBox combo &&
            combo.SelectedItem is ComboBoxItem item &&
            item.Tag is string tag)
        {
            vm.FilterStatus = tag switch
            {
                "Downloading" => DownloadPackageStatus.Downloading,
                "Queued" => DownloadPackageStatus.Queued,
                "Completed" => DownloadPackageStatus.Completed,
                "Failed" => DownloadPackageStatus.Failed,
                _ => null,
            };
        }
    }
}
