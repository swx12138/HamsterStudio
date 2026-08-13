using HamsterStudioMaui.ViewModels;

namespace HamsterStudioMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = MauiProgram.App?.Services.GetRequiredService<MainPageModel>();
        }
    }

}
