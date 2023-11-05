using Shiny.Jobs;
namespace MySolarCells.Jobs;

public class JobDalySync : IJob
{
    public JobDalySync()
    {
    }

    public async Task Run(JobInfo jobInfo, CancellationToken cancelToken)
    {
        for (var i = 0; i < 10; i++)
        {
            if (cancelToken.IsCancellationRequested)
                break;

            await Task.Delay(1000, cancelToken).ConfigureAwait(false);
        }
    }
    
}

