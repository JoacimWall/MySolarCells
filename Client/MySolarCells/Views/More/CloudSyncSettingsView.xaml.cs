namespace MySolarCells.Views.More;

public partial class CloudSyncSettingsView : BaseContentPage
{
    public CloudSyncSettingsView()
    {
        InitializeComponent();
        BindingContext = App.Current!.Handler!.MauiContext!.Services.GetService<CloudSyncSettingsViewModel>();
    }
}
