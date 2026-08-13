using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace HamsterStudioMaui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static MainActivity? Instance { get; private set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Instance = this;
        RequestManageStorageIfNeeded();
    }

    public void RequestManageStorageIfNeeded()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            if (!Android.OS.Environment.IsExternalStorageManager)
            {
                var intent = new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission);
                intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
                StartActivity(intent);
            }
        }
    }
}
