namespace MySolarCells.Services;

public interface ISettingsService
{
    OnboardingStatusEnum OnboardingStatus { get; set; }
    int SelectedHomeId { get; set; }
}

public class SettingsService : ISettingsService
{
    public OnboardingStatusEnum OnboardingStatus
    {
        get
        {
            var countryEnum = (OnboardingStatusEnum)Preferences.Get(nameof(OnboardingStatus).ToLower(), (int)OnboardingStatusEnum.Unknown);
            return countryEnum;
        }
        set
        {
            Preferences.Set(nameof(OnboardingStatus).ToLower(), (int)value);
           
        }
    }
    public int SelectedHomeId
    {
        get => Preferences.Get(nameof(SelectedHomeId).ToLower(), 0);
        set => Preferences.Set(nameof(SelectedHomeId).ToLower(), value);
    }
   
}