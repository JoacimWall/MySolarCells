using Color = System.Drawing.Color;

namespace MySolarCells.Services;
public interface IDialogService
{
    Task ShowAlertAsync(string? message, string? title, string? buttonLabel);
    Task<bool> ShowAlertAsync(string? message, string? title, string? buttonAccept, string? buttonCancel);
    Task<string> ShowPromptAsync(string? message, string? title, string? ok, string? cancel, string initValue = "");
    Task<string> ShowActionSheetAsync(string? title, string? cancel, string? destructive, params string[] buttons);

    Task<bool> ConfirmAsync(string? message, string? title, string? ok, string? cancel);
    object GetProgress(string title);
    void ShowToast(string message, int durationSecs = 3);
}
public class DialogService : IDialogService
{
    public async Task ShowAlertAsync(string? message, string? title, string? buttonLabel)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current != null && Application.Current.MainPage != null)
                await Application.Current.MainPage.DisplayAlert(title, message, buttonLabel);
        });
    }

    public async Task<bool> ShowAlertAsync(string? message, string? title, string? buttonAccept, string? buttonCancel)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current != null && Application.Current.MainPage != null)
                return await Application.Current.MainPage.DisplayAlert(title, message, buttonAccept, buttonCancel);
            return false;
        });
    }

    public async Task<string> ShowActionSheetAsync(string? title, string? cancel, string? destructive,
        params string[] buttons)
    {
        if (string.IsNullOrEmpty(title))
            title = "";

        buttons = buttons.Where(c => c != string.Empty).ToArray();

        var result = string.Empty;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current != null && Application.Current.MainPage != null)
                result = await Application.Current.MainPage.DisplayActionSheet(title, cancel, destructive, buttons);
        });
        return result;
    }

    public async Task<string> ShowPromptAsync(string? message, string? title, string? ok, string? cancel,
        string initValue = "")
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current != null && Application.Current.MainPage != null)
                return await Application.Current.MainPage.DisplayPromptAsync(title, message, ok, cancel,
                    initialValue: initValue);
            return string.Empty;
        });
    }

    public async Task<bool> ConfirmAsync(string? message, string? title, string? ok, string? cancel)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current != null && Application.Current.MainPage != null)
                return await Application.Current.MainPage.DisplayAlert(title, message, ok, cancel);
            return false;
        });
    }

    public object GetProgress(string title)
    {
        var v = new ProgressDialogConfig();
        v.SetTitle(title);
        //v.MaskType = MaskType.Gradient;
        v.SetMaskType(MaskType.Gradient);
#if IOS || ANDROID || MACCATALYST
        return UserDialogs.Instance.Progress(v);
#endif
#if WINDOWS
        //TODO:WINDOWS
        return null;
#endif
    }

    public void ShowToast(string message, int durationSecs = 3)
    {
        // Add top and bottom space to iOS
        if (DeviceInfo.Platform == DevicePlatform.iOS) message = "\n" + message + "\n";
        MainThread.BeginInvokeOnMainThread(() =>
        {
#if IOS || ANDROID || MACCATALYST
            UserDialogs.Instance.Toast(new ToastConfig(message)
                .SetBackgroundColor(Color.Gray)
                .SetMessageTextColor(Color.White)
                .SetDuration(TimeSpan.FromSeconds(durationSecs))
                .SetPosition(ToastPosition.Top));
#endif
#if WINDOWS
          //TODO:WINDOWS
#endif
        });
    }
}