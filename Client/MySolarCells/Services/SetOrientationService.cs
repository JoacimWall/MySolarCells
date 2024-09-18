

#if IOS || MACCATALYST
using System.Diagnostics.CodeAnalysis;
using Foundation;
using UIKit;

#elif ANDROID
using Android.Content.PM;
#endif
namespace MySolarCells.Services;

public interface ISetOrientationService
{
    void OnlyLandscape();
    void OnlyPortrait();
    void PortraitAndLandscape();
}

#if IOS|| MACCATALYST
public abstract class AppDelegateEx : MauiUIApplicationDelegate
{
    public virtual UIInterfaceOrientationMask CurrentLockedOrientation { get; set; }

    //according to the Apple docs, Application and ViewController have to agree on the supported orientation, this forces it
    //https://developer.apple.com/documentation/uikit/uiapplicationdelegate/1623107-application?language=objc
    [Export("application:supportedInterfaceOrientationsForWindow:")]
    public virtual UIInterfaceOrientationMask GetSupportedInterfaceOrientationsForWindow(UIApplication application,
        UIWindow forWindow)
    {
        return CurrentLockedOrientation;
    }
}

public class SetOrientationService : ISetOrientationService
{
    private readonly AppDelegateEx? applicationDelegate;
    private readonly ILogService logService;

    public SetOrientationService(ILogService logService)
    {
        this.logService = logService;
        if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
        {
            if (MauiUIApplicationDelegate.Current is AppDelegateEx orientationServiceDelegate)
                applicationDelegate = orientationServiceDelegate;
            else
                throw new NotImplementedException(
                    $"AppDelegate must be derived from {nameof(AppDelegateEx)} to use this implementation!");
        }
    }


    public void OnlyLandscape()
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
        {
            if (applicationDelegate != null)
            {
                applicationDelegate.CurrentLockedOrientation = UIInterfaceOrientationMask.LandscapeRight;
                SetOrientation(UIInterfaceOrientationMask.LandscapeRight);
            }
        }
        else
        {
            UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.LandscapeRight),
                new NSString("orientation"));
        }
    }

    public void OnlyPortrait()
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
        {
            if (applicationDelegate != null)
            {
                applicationDelegate.CurrentLockedOrientation = UIInterfaceOrientationMask.Portrait;

                SetOrientation(UIInterfaceOrientationMask.Portrait);
            }
        }
        else
        {
            UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Portrait),
                new NSString("orientation"));
        }
    }

    public void PortraitAndLandscape()
    {
        //throw new NotImplementedException();
    }


    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private void SetOrientation(UIInterfaceOrientationMask uiInterfaceOrientationMask)
    {
        var rootWindowScene =
            UIApplication.SharedApplication.ConnectedScenes.ToArray()?.FirstOrDefault() as UIWindowScene;

        if (rootWindowScene == null)
            return;

#pragma warning disable CA1422
        var rootViewController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
#pragma warning restore CA1422

        if (rootViewController == null)
            return;

        rootWindowScene.RequestGeometryUpdate(new UIWindowSceneGeometryPreferencesIOS(uiInterfaceOrientationMask),
            error =>
            {
                logService.ConsoleWriteLineDebug("Error while attempting to lock orientation: {Error }" +
                                                 error.LocalizedDescription);
            });

        rootViewController.SetNeedsUpdateOfSupportedInterfaceOrientations();
        rootViewController.NavigationController?.SetNeedsUpdateOfSupportedInterfaceOrientations();
    }
}
#endif


#if ANDROID
public class SetOrientationService : ISetOrientationService
{
    public void OnlyLandscape()
    {
        var activity = Platform.CurrentActivity;
        if (activity != null)
         activity.RequestedOrientation = ScreenOrientation.Landscape;
    }



    public void OnlyPortrait()
    {
        var activity = Platform.CurrentActivity;
        if (activity != null)
            activity.RequestedOrientation = ScreenOrientation.Portrait;
    }



    void PortraitAndLandscape()
    {
        var activity = Platform.CurrentActivity;
        //.RequestedOrientation = ScreenOrientation.Ori;
    }

    void ISetOrientationService.PortraitAndLandscape()
    {
        throw new NotImplementedException();
    }
    }
#endif