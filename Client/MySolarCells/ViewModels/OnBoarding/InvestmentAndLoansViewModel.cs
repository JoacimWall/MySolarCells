using System.Linq;
using MySolarCells.Services.Sqlite.Models;

namespace MySolarCells.ViewModels.OnBoarding;

public class InvestmentAndLoanViewModel : BaseViewModel
{
   
    public InvestmentAndLoanViewModel()
    {
       
        using var dbContext = new MscDbContext();
        var result =  dbContext.InvestmentAndLon.Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
        foreach (var item in result)
        {
            InvestmentAndLons.Add(item);
        }
        if (InvestmentAndLons != null && InvestmentAndLons.Count > 0)
        {
            selectedInvestmentAndlon = InvestmentAndLons.Last();
            if (selectedInvestmentAndlon.Interest != null && selectedInvestmentAndlon.Interest.Count > 0)
                SelectedInterest = selectedInvestmentAndlon.Interest.Last();
        }
    }

    public ICommand AddInestlonCommand => new Command(async () => await AddInvestLon());
    public ICommand AddInterestCommand => new Command(async () => await AddInterest());
    public ICommand SaveCommand => new Command(async () => await Save());
    public ICommand SkipCommand => new Command(async () => await Skip());

    private Task Skip()
    {
        throw new NotImplementedException();
    }

    private async Task Save()
    {
        using var dbContext = new MscDbContext();
        if (selectedInvestmentAndlon.InvestmentAndLonId == 0)
            dbContext.InvestmentAndLon.Add(selectedInvestmentAndlon);
        else //update
        {
            var dbEntity = dbContext.InvestmentAndLon.Include(i => i.Interest).First(x => x.InvestmentAndLonId == selectedInvestmentAndlon.InvestmentAndLonId);
            dbEntity.FromDate = selectedInvestmentAndlon.FromDate;
            dbEntity.Investment = selectedInvestmentAndlon.Investment;
            dbEntity.Description = selectedInvestmentAndlon.Description;
            dbEntity.Lon = selectedInvestmentAndlon.Lon;
            //TODO:Se till att updatera ränta tabell också

        }
        await dbContext.SaveChangesAsync();
    }

    private async Task AddInterest()
    {
        if (SelectedInvestmentAndlon != null)
        {
            if (SelectedInvestmentAndlon.Interest == null)
                SelectedInvestmentAndlon.Interest = new ObservableCollection<InvestmentAndLonInterest>();

            SelectedInvestmentAndlon.Interest.Add(new Services.Sqlite.Models.InvestmentAndLonInterest {Description = "MyDescription", FromDate = DateTime.Today });
            SelectedInterest = SelectedInvestmentAndlon.Interest.Last();
        }
    }

    private async Task AddInvestLon()
    {
        InvestmentAndLons.Add(new Services.Sqlite.Models.InvestmentAndLon { Description = "My base investment", HomeId = MySolarCellsGlobals.SelectedHome.HomeId });
        SelectedInvestmentAndlon = InvestmentAndLons.Last();

    }

    private ObservableCollection<Services.Sqlite.Models.InvestmentAndLon> investmentAndLons = new ObservableCollection<Services.Sqlite.Models.InvestmentAndLon>();
    public ObservableCollection<Services.Sqlite.Models.InvestmentAndLon> InvestmentAndLons
    {
        get => investmentAndLons;
        set
        {
            SetProperty(ref investmentAndLons, value);

        }
    }
    private Services.Sqlite.Models.InvestmentAndLon selectedInvestmentAndlon;
    public Services.Sqlite.Models.InvestmentAndLon SelectedInvestmentAndlon
    {
        get => selectedInvestmentAndlon;
        set { SetProperty(ref selectedInvestmentAndlon, value); }
    }
    private Services.Sqlite.Models.InvestmentAndLonInterest selectedInterest;
    public Services.Sqlite.Models.InvestmentAndLonInterest SelectedInterest
    {
        get => selectedInterest;
        set { SetProperty(ref selectedInterest, value); }
    }
    private bool _isOnbordingMode = false;
    public bool IsOnbordingMode
    {
        get { return _isOnbordingMode; }
        set { SetProperty(ref _isOnbordingMode, value); }
    }

    //private bool _showProgressStatus;
    //public bool ShowProgressStatus
    //{
    //    get { return _showProgressStatus; }
    //    set { SetProperty(ref _showProgressStatus, value); }
    //}
    //private string _progessStatus;
    //public string ProgressStatus
    //{
    //    get { return _progessStatus; }
    //    set { SetProperty(ref _progessStatus, value); }
    //}
    //private string _progressSubStatus;
    //public string ProgressSubStatus
    //{
    //    get { return _progressSubStatus; }
    //    set { SetProperty(ref _progressSubStatus, value); }
    //}
    //private float _progressProcent;
    //public float ProgressProcent
    //{
    //    get { return _progressProcent; }
    //    set { SetProperty(ref _progressProcent, value); }
    //}
}

