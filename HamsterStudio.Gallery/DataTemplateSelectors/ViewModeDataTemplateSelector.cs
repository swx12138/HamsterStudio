using HamsterStudio.Gallery.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudio.Gallery.DataTemplateSelectors;

internal class ViewModeDataTemplateSelector : DataTemplateSelector
{
    public required DataTemplate ThumbnailView { get; set; }
    public required DataTemplate LargeImageView { get; set; }
    public required DataTemplate DefaultView { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        try
        {
            if (item is ThumbnailModeViewModel)
            {

                return ThumbnailView;
            }
            else if (item is LargeImageViewModel)
            {
                return LargeImageView;
            }
            else
            {
                return DefaultView;
            }
        }
        catch
        {
            // Fallback to default template if any error occurs
        }
        return base.SelectTemplate(item, container);
    }

}
