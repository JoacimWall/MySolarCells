namespace MySolarCells.Services;

public interface ISettingsService
{
    OnboardingStatusEnum OnboardingStatus { get; set; }
    int SelectedHomeId { get; set; }
    //DateTime LastDataSync { get; set; }
}

public class SettingsService : ISettingsService
{
    
    private readonly MscDbContext mscDbContext;
    public SettingsService(MscDbContext mscDbContext)
    {
        this.mscDbContext = mscDbContext;
    }
    
    public OnboardingStatusEnum OnboardingStatus
    {
        get
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                return OnboardingStatusEnum.Unknown;
            else
                return (OnboardingStatusEnum)exist.IntValue;
        }
        set
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(OnboardingStatus));
            if (exist == null)
                this.mscDbContext.Preferences.Add(new Sqlite.Models.Preferences { Name = nameof(OnboardingStatus), IntValue = (int)value });
            else
                exist.IntValue = (int)value;
            this.mscDbContext.SaveChanges();
        }
    }
    public int SelectedHomeId
    {
        get
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null)
                return 0;
            else
                return exist.IntValue;
        }
        set
        {
            var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(SelectedHomeId));
            if (exist == null && value != 0)
            {
                this.mscDbContext.Preferences.Add(new Sqlite.Models.Preferences { Name = nameof(SelectedHomeId), IntValue = (int)value });
                this.mscDbContext.SaveChanges();
            }
            else if (exist != null)
            {
                exist.IntValue = (int)value;
                this.mscDbContext.SaveChanges();
            }

            
        }
    }
    //public DateTime LastDataSync
    //{
    //    get
    //    {
    //        var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastDataSync));
    //        if (exist == null)
    //            return DateTime.Now.AddHours(-1);
    //        else
    //            return exist.DateValue;
    //    }
    //    set
    //    {
    //        var exist = this.mscDbContext.Preferences.FirstOrDefault(x => x.Name == nameof(LastDataSync));
    //        if (exist == null)
    //            this.mscDbContext.Preferences.Add(new Sqlite.Models.Preferences { Name = nameof(LastDataSync), DateValue =value });
    //        else
    //            exist.DateValue = value;
    //        this.mscDbContext.SaveChanges();
    //    }
    //}
    
}