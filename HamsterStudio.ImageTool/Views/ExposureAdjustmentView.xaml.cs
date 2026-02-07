using HamsterStudio.ImageTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace HamsterStudio.ImageTool.Views
{
    /// <summary>
    /// ExposureAdjustmentView.xaml 的交互逻辑
    /// </summary>
    public partial class ExposureAdjustmentView : UserControl
    {
        public ExposureAdjustmentView()
        {
            InitializeComponent();
        }

        private void Image_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ExposureAdjustmentViewModel vm)
            {
                vm.AutoFitImage();
            }
        }

        bool startMouseDrag = false;
        Point startPosition;
        private void Image_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            startMouseDrag = true;
            startPosition = e.GetPosition(sender as FrameworkElement);
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            startMouseDrag = false;
        }

        private void Image_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (startMouseDrag && DataContext is ExposureAdjustmentViewModel vm)
            {
                var pos = e.GetPosition(sender as FrameworkElement);
                vm.ImagePositionX += (pos.X - startPosition.X);
                vm.ImagePositionY += (pos.Y - startPosition.Y);
                startPosition = pos;
            }
        }

        private void ImageCanvas_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ExposureAdjustmentViewModel vm)
            {
                var pos = e.GetPosition(sender as FrameworkElement);
                vm.UpdateMousePosition(pos);
            }
        }

        private void AdjustImage_Loaded(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation(nameof(AdjustImage_Loaded) + " called");
            if (DataContext is ExposureAdjustmentViewModel vm)
            {
                vm.ImageCanvasReadonlyWidth = ImageCanvas.ActualWidth;
                vm.ImageCanvasReadonlyHeight = ImageCanvas.ActualHeight;
                vm.AutoFitImage();
            }
        }

        private void ImageCanvas_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Trace.TraceInformation(nameof(ImageCanvas_PreviewMouseWheel) + " called");
            if (DataContext is ExposureAdjustmentViewModel vm)
            {
                var pos = e.GetPosition(sender as FrameworkElement);
                vm.UpdateMousePosition(pos);
                vm.UpdateImagePosition(e.Delta);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExposureAdjustmentViewModel vm)
            {
                vm.LoadImageCommand.Execute(@"E:\HamsterStudioHome\xiaohongshu\小记噜噜\老师…_2_xhs_小记噜噜_1040g2sg31s8k5idplme04b36r575g6afe5ib2pg.png");
            }
        }
    }

}
