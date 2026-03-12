using HamsterStudio.Gallery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudio.Gallery.DataTemplateSelectors
{
    public class GalleryFolderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FullImageTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is GalleryFolderModel)
            {
                return FolderTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
