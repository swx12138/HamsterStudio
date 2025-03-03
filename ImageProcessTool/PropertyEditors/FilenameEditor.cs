using HandyControl.Controls;
using System.Windows;
using HamsterStudio.Controls;
using System.Windows.Controls;

namespace ImageProcessTool.PropertyEditors
{
    class FilenameEditor : PropertyEditorBase
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
