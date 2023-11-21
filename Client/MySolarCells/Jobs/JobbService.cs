using Shiny.Jobs;
namespace MySolarCells.Jobs;

public class JobDalySync : IJob
{
    private readonly IDataSyncService dataSyncService;
    private readonly MscDbContext mscDbContext;
    private readonly ISettingsService settingsService;
    public JobDalySync(IDataSyncService dataSyncService,ISettingsService settingsService, MscDbContext mscDbContext)
    {
        this.dataSyncService = dataSyncService;
        this.settingsService = settingsService;
        this.mscDbContext = mscDbContext;
    }

    public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
    {
        mscDbContext.Log.Add(new Log
        {
            LogTitle = "Sync stated from bakgrunds job",
            CreateDate = DateTime.Now,
            LogDetails = "",
            LogTyp = (int)LogTyp.Info 
        });
        if (this.settingsService.OnboardingStatus != OnboardingStatusEnum.OnboardingDone)
            return;

       
            var result = await this.dataSyncService.Sync();

       



    }
    
}

