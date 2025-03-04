using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HamsterStudio.BraveShine.PropertyEditors
{
    class ImageViewOnlyEditor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            var img = new Image();
            img.SetBinding(Image.SourceProperty, propertyItem.Name);
            img.MaxHeight = 64;
            return img;
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return Image.DataContextProperty;
        }
    }
}
