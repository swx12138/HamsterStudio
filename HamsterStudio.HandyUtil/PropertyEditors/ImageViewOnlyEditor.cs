using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudio.HandyUtil.PropertyEditors
{
    public class ImageViewOnlyEditor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            var img = new Image();
            img.SetBinding(Image.SourceProperty, propertyItem.Name);
            img.MaxHeight = 128;
            return img;
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return FrameworkElement.DataContextProperty;
        }
    }
}
