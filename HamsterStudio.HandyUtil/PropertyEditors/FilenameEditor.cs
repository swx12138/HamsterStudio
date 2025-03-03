using HamsterStudio.Controls;
using HandyControl.Controls;
using System.Windows;

namespace HamsterStudio.HandyUtil.PropertyEditors
{
    public class FilenameEditor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            return new PathSelector() { PathOnly = false };
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return PathSelector.PathProperty;
        }
    }
}
