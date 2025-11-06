using CommunityToolkit.Mvvm.Input;

namespace MySolarCells.ViewModels;

public partial class BaseViewModel : ObservableObject, IQueryAttributable
{
    protected IDialogService DialogService { get; set; }
    protected IAnalyticsService AnalyticsService { get; set; }
    protected ILogService LogService { get; set; }
    protected ISettingsService SettingsService { get; set; }
    protected IHomeService HomeService { get; set; }
    protected IInternetConnectionService InternetConnectionService { get; set; }
    public BaseViewModel(IDialogService dialogService, IAnalyticsService analyticsService,
        IInternetConnectionService internetConnectionService, ILogService logService, ISettingsService settingsService, IHomeService homeService)
    {
        DialogService = dialogService;
        AnalyticsService = analyticsService;
        InternetConnectionService = internetConnectionService;
        LogService = logService;
        SettingsService = settingsService;
        HomeService = homeService;
        FirstTimeAppearing = true;
        FirstTimeNavigatingTo = true;
        stateEmptyMessage = "";
        stateSavingMessage = "";
    }
    #region Propertys
    public virtual ICommand NavBackCommand => new RelayCommand(async () => await GoBack());
    public virtual ICommand RefreshCommand => new RelayCommand(async () => await RefreshAsync());

    private ChartDataRequest chartDataRequest = new ChartDataRequest();
    public ChartDataRequest ChartDataRequest
    {
        get => chartDataRequest;
        set
        {
            SetProperty(ref chartDataRequest, value);
            HomeService.SetCurrentChartDataRequest(value);
        }
    }
    public Home CurrentHome
    {
        get => HomeService.CurrentHome();
    }
    private HistorySimulate roiSimulate = new HistorySimulate();
    public HistorySimulate RoiSimulate
    {
        get => roiSimulate;
        set => SetProperty(ref roiSimulate, value);
    }

    [ObservableProperty]
    bool showNavbarBackButton;

    [ObservableProperty]
    bool firstTimeAppearing;

    [ObservableProperty]
    bool firstTimeNavigatingTo;

    [ObservableProperty]
    bool reloadNeededOnAppearing;

    [ObservableProperty]
    string title = string.Empty;

    //[ObservableProperty]
    //bool IsLoading;

    [ObservableProperty]
    bool isValid;
    [ObservableProperty]
    bool isSuccess;
    [ObservableProperty]
    bool isRefreshing;
    [ObservableProperty]
    bool isLoading;
    [ObservableProperty]
    bool isError;
    [ObservableProperty]
    bool isEmpty;
    [ObservableProperty]
    bool isSaving;

    [ObservableProperty]
    string stateEmptyMessage;
    [ObservableProperty]
    string stateSavingMessage;
    [ObservableProperty]
    private bool cleanupPerformed;
    #endregion

    #region Virutal functions
    public virtual async Task OnDisappearingAsync() => await Task.FromResult(true);
    public virtual async Task OnAppearingAsync() => await Task.FromResult(true);

    public virtual async Task RefreshAsync(TsViewState layoutState = TsViewState.Refreshing, bool showProgress = true)
    {
        await Task.FromResult(true);
    }

    public virtual void Refresh(TsViewState layoutState = TsViewState.Refreshing, bool showProgress = true)
    {
    }

    protected virtual async Task<bool> NavBackIsAllowed() => await Task.FromResult(true);

    public virtual void CleanUp() { }
    public virtual void OnAppearing() => OnAppearingLocal();
    public virtual void OnDisappearing() { }
    public virtual void OnNavigatedTo(NavigatedToEventArgs args) => OnNavigatedToLocal();
    public virtual void RefreshTranslations() => RefreshTranslationsLocal();
    public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.Clear();
    }

    protected virtual async Task PushModal(Page page)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.Navigation.PushModalAsync(page);
        });
    }

    protected virtual async Task GoToAsync(string page)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync(page);

        });

    }
    public virtual async Task GoToAsync(string page, Dictionary<string, object> dic)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync(page, dic);

        });

    }
    public virtual async Task GoBack()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current != null)
                {
                    if (Application.Current.MainPage is Shell { Navigation: not null } shellRoot)
                    {
                        var result = await RunNavBackIsAllowed(shellRoot.CurrentPage);
                        if (!result)
                            return;
                        RunCleanUp(shellRoot.CurrentPage);
                        await Shell.Current.Navigation.PopAsync();
                        //await navigationPage.Navigation.PopAsync();
                    }
                    else
                    {
                        var navigationPage = Application.Current.MainPage as NavigationPage;
                        var result = navigationPage != null && await RunNavBackIsAllowed(navigationPage.CurrentPage);
                        if (!result)
                            return;

                        if (navigationPage != null)
                        {
                            RunCleanUp(navigationPage.CurrentPage);
                            await navigationPage.Navigation.PopAsync();
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            var logic = new Dictionary<string, string>
            {
                { "Info", "Error in GoBack Function" },
                { "Error", ex.StackTrace ?? ex.Message }
            };
            LogService.ReportErrorToAppCenter(ex, logic);
        }
    }
    public virtual async Task RemoveModalPageAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current != null)
                {
                    if (Application.Current.MainPage is Shell { Navigation: not null } shellRoot)
                    {
                        RunCleanUp(shellRoot.CurrentPage);
                        await Shell.Current.Navigation.PopModalAsync();
                    }
                    else
                    {
                        if (Application.Current.MainPage is NavigationPage navigationPage)
                        {
                            RunCleanUp(navigationPage.CurrentPage);
                            await navigationPage.Navigation.PopAsync();
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            var logic = new Dictionary<string, string>
            {
                { "Info", "Error in RemoveModalPageAsync Function" },
                { "Error", ex.StackTrace ?? ex.Message }
            };
            LogService.ReportErrorToAppCenter(ex, logic);
        }
    }
    public virtual void RemovePreviousPage()
    {
        try
        {

            if (Application.Current != null && Application.Current.MainPage is Shell mainPage)
            {
                // We cannot remove root page so lets check the count before we remove
                var indexToRemove = mainPage.Navigation.NavigationStack.Count - 2;
                if (indexToRemove > 0)
                {
                    RunCleanUp(mainPage.Navigation.NavigationStack[indexToRemove]);
                    mainPage.Navigation.RemovePage(mainPage.Navigation.NavigationStack[indexToRemove]);
                }
            }

        }
        catch (Exception ex)
        {
            var logic = new Dictionary<string, string>
            {
                { "Info", "Error in RemovePageAsync Function" },
                { "Error", ex.StackTrace ?? ex.Message }
            };
            LogService.ReportErrorToAppCenter(ex, logic);
        }
    }
    #endregion

    #region Public functions
    public void ObservableCollectionCallback(IEnumerable collection, object context, Action? accessMethod, bool writeAccess)
    {
        // `lock` ensures that only one thread access the collection at a time
        lock (collection)
        {
            accessMethod?.Invoke();
        }
    }

    public bool PromptToConfirmExit =>
        //bool promptToConfirmExit = false;
        //if (App.Current.MainPage is ContentPage)
        //{
        //    return true;
        //}
        ////else if (App.Current.MainPage is MasterDetailPage masterDetailPage
        ////    && masterDetailPage.Detail is NavigationPage detailNavigationPage)
        ////{
        ////    return detailNavigationPage.Navigation.NavigationStack.Count <= 1;
        ////}
        //else if (App.Current.MainPage is NavigationPage mainPage)
        //{
        //    if (mainPage.CurrentPage is TabbedPage tabbedPage
        //        && tabbedPage.CurrentPage is NavigationPage navigationPage)
        //    {
        //        return navigationPage.Navigation.NavigationStack.Count <= 1;
        //    }
        //    else
        //    {
        //        return mainPage.Navigation.NavigationStack.Count <= 1;
        //    }
        //}
        //else if (App.Current.MainPage is TabbedPage tabbedPage && tabbedPage.CurrentPage is NavigationPage navigationPage)
        //{
        //    return navigationPage.Navigation.NavigationStack.Count <= 1;
        //}
        false; // promptToConfirmExit;
    string stateErrorMessage = string.Empty;
    public string StateErrorMessage
    {
        get => stateErrorMessage;
        set => SetProperty(ref stateErrorMessage, value);
    }


    public ViewState CurrentViewState { get; set; }
    public virtual void SetCurrentState(TsViewState currentState)
    {

        switch (currentState)
        {
            case TsViewState.Refreshing:
                IsRefreshing = true;
                IsLoading = false;
                IsError = false;
                IsSuccess = false;
                IsEmpty = false;
                IsSaving = false;
                break;

            case TsViewState.Loading:
                IsRefreshing = false;
                IsLoading = true;
                IsError = false;
                IsSuccess = false;
                IsEmpty = false;
                IsSaving = false;
                break;
            case TsViewState.Success:
                IsRefreshing = false;
                IsLoading = false;
                IsError = false;
                IsSuccess = true;
                IsEmpty = false;
                IsSaving = false;
                break;
            case TsViewState.Empty:
                IsRefreshing = false;
                IsLoading = false;
                IsError = false;
                IsSuccess = false;
                IsEmpty = true;
                IsSaving = false;
                break;
            case TsViewState.Saving:
                IsRefreshing = false;
                IsLoading = false;
                IsError = false;
                IsSuccess = false;
                IsEmpty = false;
                IsSaving = true;
                break;
            default:
                IsRefreshing = false;
                IsLoading = false;
                IsError = false;
                IsSuccess = false;
                IsError = true;
                IsSaving = false;
                break;
        }

        if ((ViewState)currentState != CurrentViewState)
            CurrentViewState = (ViewState)currentState;

    }




    #endregion

    #region Private functions
    private void RunCleanUp(Page view)
    {
        if (view.BindingContext is BaseViewModel { CleanupPerformed: false } viewModelBase) viewModelBase.CleanUp();
    }
    private void RefreshTranslationsLocal()
    {

        OnPropertyChanged(nameof(Title));

    }
    private void OnAppearingLocal()
    {
        if (FirstTimeAppearing)
        {
            AnalyticsService.SetCurrentScreen(GetType().Name, GetType().Name);
        }
        else if (AnalyticsService.IsTabbedPageViewModel(GetType().Name))
        {
            AnalyticsService.SetCurrentScreen(GetType().Name, GetType().Name);
        }

        FirstTimeAppearing = false;

    }

    private void OnNavigatedToLocal()
    {
        FirstTimeNavigatingTo = false;
    }

    private async Task<bool> RunNavBackIsAllowed(Page view)
    {
        if (view.BindingContext is BaseViewModel viewModelBase)
        {
            return await viewModelBase.NavBackIsAllowed();
        }
        return true;
    }
    #endregion
}
public enum TsViewState
{
    Loading = 0,
    Refreshing = 1,
    Empty = 2,
    Success = 3,
    Error = 4,
    Saving = 5
}