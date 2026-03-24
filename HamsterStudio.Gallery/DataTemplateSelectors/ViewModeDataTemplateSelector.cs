using System.Windows;
using System.Windows.Controls;

namespace HamsterStudio.Gallery.DataTemplateSelectors;

internal class ViewModeDataTemplateSelector : DataTemplateSelector
{
    public bool IsThumbnailView { get; set; }

    public required DataTemplate ThumbnailView { get; set; }
    public required DataTemplate LargeImageView { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        try
        {
            if (IsThumbnailView)
            {
                return ThumbnailView;
            }
            else
            {
                return LargeImageView;
            }
        }
        catch
        {
            // Fallback to default template if any error occurs
        }
        return base.SelectTemplate(item, container);
    }

}
