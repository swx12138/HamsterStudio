using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HamsterStudioGUI
{
    class TabPageModel
    {
        public required string Title { get; set; }
        public required string Desc { get; set; }
        public required UserControl Element { get; set; }

        public static TabPageModel Incubator<T>(string title, string desc) where T : UserControl, new()
        {
            return new TabPageModel() { Title = title, Desc = desc, Element = new T() };
        }
    }

    class MainWindowModel
    {
        public ObservableCollection<TabPageModel> TabPages { get; } = [];

        public MainWindowModel()
        {
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.ImageTool.Views.MainView>("ImageTool", "ImageTool"));
            TabPages.Add(TabPageModel.Incubator<HamsterStudio.BraveShine.Views.MainView>("BraveShine", "BraveShine"));

        }

    }

}
