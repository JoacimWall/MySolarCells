using Microsoft.EntityFrameworkCore;

namespace MySolarCells.Services;

public interface ISettingsService
{
    OnboardingStatusEnum OnboardingStatus { get; set; }
    int SelectedHomeId { get; set; }
}

public class SettingsService : ISettingsService
{
    private MscDbContext dbContext = new MscDbContext();
    public SettingsService()
    {

    }
    
    public OnboardingStatusEnum OnboardingStatus
    {
        get
        {
            var exist = dbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                return OnboardingStatusEnum.Unknown;
            else
                return (OnboardingStatusEnum)exist.IntValue;
        }
        set
        {
            var exist = dbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                dbContext.Preferences.Add(new Sqlite.Models.Preferences { Name = nameof(OnboardingStatus), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            dbContext.SaveChanges();
        }
    }
    public int SelectedHomeId
    {
        get
        {
            var exist = dbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null)
                return 0;
            else
                return exist.IntValue;
        }
        set
        {
            var exist = dbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null)
                dbContext.Preferences.Add(new Sqlite.Models.Preferences { Name = nameof(SelectedHomeId), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            dbContext.SaveChanges();
        }
    }
   
}