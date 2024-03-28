using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace MySolarCells.Views;

public class BaseContentPage : ContentPage
{
    protected BaseContentPage()
    {
        NavigationPage.SetBackButtonTitle(this, "");
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
        
        //hides warnings as we require IOS 12.4 so it's easy
        HideSoftInputOnTapped = true;
      
        On<iOS>().SetUseSafeArea(true);
        
    }
    public Color StatusbarBackgroundColor { get; set; } = AppColors.StatusbarBackgroundColor;

    //public void SubscribeToScrollToTopMessage()
    //{  
    //    WeakReferenceMessenger.Default.Register<TmScrollTabbarPageToTopMessage>(this, (r, m) =>
    //    {
    //        if (m.Value == this.GetType().ToString())
    //            ScrollToTopAction();
    //    });
    //}

    public virtual void ScrollToTopAction() { }
    protected override bool OnBackButtonPressed()
    {
        if (BindingContext != null)
            (BindingContext as BaseViewModel)?.GoBack();


        return base.OnBackButtonPressed();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        bool darkStatusBarBackground = !(StatusbarBackgroundColor == AppColors.WhiteColor
                                         || StatusbarBackgroundColor == AppColors.Gray100Color
                                         || StatusbarBackgroundColor == AppColors.Primary200Color);


#pragma warning disable CA1416
        if (darkStatusBarBackground)
            Behaviors.Add(new CommunityToolkit.Maui.Behaviors.StatusBarBehavior { StatusBarColor = StatusbarBackgroundColor, StatusBarStyle = CommunityToolkit.Maui.Core.StatusBarStyle.LightContent });
        else
            Behaviors.Add(new CommunityToolkit.Maui.Behaviors.StatusBarBehavior { StatusBarColor = StatusbarBackgroundColor, StatusBarStyle = CommunityToolkit.Maui.Core.StatusBarStyle.DarkContent });
#pragma warning restore CA1416



        if (BindingContext != null)
        {
            await ((BaseViewModel)BindingContext).OnAppearingAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                (BindingContext as BaseViewModel)?.OnAppearing();
            });

        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext != null)
        {
            await ((BaseViewModel)BindingContext).OnDisappearingAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (BindingContext as BaseViewModel)?.OnDisappearing();
            });
        }
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext != null)
        {
            (BindingContext as BaseViewModel)?.OnNavigatedTo(args);
           
        }
    }
}