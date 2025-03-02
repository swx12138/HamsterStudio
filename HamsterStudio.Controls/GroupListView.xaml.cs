using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HamsterStudio.Controls
{
    /// <summary>
    /// GroupListView.xaml 的交互逻辑
    /// </summary>
    public partial class GroupListView : UserControl
    {
        public CollectionViewSource ItemsSource
        {
            get { return (CollectionViewSource)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(CollectionViewSource), typeof(GroupBox), new PropertyMetadata(null, ItemsSourceMetadta_OnPropertyChanged) );


        public GroupListView()
        {
            InitializeComponent();
        }

        private static void ItemsSourceMetadta_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is GroupListView listView)
            {
                //listView.lbx1.ItemsSource = listView.ItemsSource;
            }
        }
    }
}
