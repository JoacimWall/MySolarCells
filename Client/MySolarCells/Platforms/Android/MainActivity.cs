using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MySolarCells;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize  | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle bundle)
    {

        base.OnCreate(bundle);
        

        Acr.UserDialogs.UserDialogs.Init(this);



    }
}

