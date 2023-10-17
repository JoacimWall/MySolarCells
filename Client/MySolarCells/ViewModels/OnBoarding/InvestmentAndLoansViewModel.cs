using System.Linq;

using MySolarCells.Services.Sqlite.Models;

namespace MySolarCells.ViewModels.OnBoarding;

public class InvestmentAndLoanViewModel : BaseViewModel
{

    public InvestmentAndLoanViewModel()
    {

        using var dbContext = new MscDbContext();
        var result = dbContext.InvestmentAndLon.Include(i => i.Interest).Where(x => x.HomeId == MySolarCellsGlobals.SelectedHome.HomeId).ToList();
        foreach (var item in result)
        {
            InvestmentAndLoans.Add(item);
        }
        if (InvestmentAndLoans != null && InvestmentAndLoans.Count > 0)
        {
            selectedInvestmentAndloan = InvestmentAndLoans.Last();
            if (selectedInvestmentAndloan.Interest != null && selectedInvestmentAndloan.Interest.Count > 0)
                SelectedInterest = selectedInvestmentAndloan.Interest.Last();
        }
        else
        {
            AddInvestLon();
        }

    }

    public ICommand AddInestlonCommand => new Command( () =>  AddInvestLon());
    public ICommand AddInterestCommand => new Command( () =>  AddInterest());
    public ICommand SaveCommand => new Command(async () => await Save());
    

    private async Task Save()
    {
        using var dbContext = new MscDbContext();
        if (selectedInvestmentAndloan.InvestmentAndLoanId == 0)
            dbContext.InvestmentAndLon.Add(selectedInvestmentAndloan);
        else //update
        {
            var dbEntity = dbContext.InvestmentAndLon.Include(i => i.Interest).First(x => x.InvestmentAndLoanId == selectedInvestmentAndloan.InvestmentAndLoanId);
            dbEntity.FromDate = selectedInvestmentAndloan.FromDate;
            dbEntity.Investment = selectedInvestmentAndloan.Investment;
            dbEntity.Description = selectedInvestmentAndloan.Description;
            dbEntity.Loan = selectedInvestmentAndloan.Loan;
            //TODO:Se till att updatera ränta tabell också
            dbEntity.Interest.First().Interest = SelectedInterest.Interest;

        }
        await dbContext.SaveChangesAsync();
        if (SettingsService.OnboardingStatus == OnboardingStatusEnum.OnboardingDone)
        {
            await GoBack();
           
        }
        else
        {
            SettingsService.OnboardingStatus = OnboardingStatusEnum.InvestmentAndLoanDone;
            await GoToAsync(nameof(FirstSyncView));
        }
    }

    private void  AddInterest()
    {
        if (SelectedInvestmentAndloan != null)
        {
            if (SelectedInvestmentAndloan.Interest == null)
                SelectedInvestmentAndloan.Interest = new ObservableCollection<InvestmentAndLoanInterest>();

            SelectedInvestmentAndloan.Interest.Add(new InvestmentAndLoanInterest { Description = AppResources.My_Description, FromDate = DateTime.Today });
            SelectedInterest = SelectedInvestmentAndloan.Interest.Last();
        }
    }

    private void AddInvestLon()
    {
        InvestmentAndLoans.Add(new InvestmentAndLoan { Description = AppResources.My_Investment_And_Loan, HomeId = MySolarCellsGlobals.SelectedHome.HomeId });
        SelectedInvestmentAndloan = InvestmentAndLoans.Last();

    }

    private ObservableCollection<InvestmentAndLoan> investmentAndLoans = new ObservableCollection<InvestmentAndLoan>();
    public ObservableCollection<InvestmentAndLoan> InvestmentAndLoans
    {
        get => investmentAndLoans;
        set
        {
            SetProperty(ref investmentAndLoans, value);

        }
    }
    private InvestmentAndLoan selectedInvestmentAndloan;
    public InvestmentAndLoan SelectedInvestmentAndloan
    {
        get => selectedInvestmentAndloan;
        set { SetProperty(ref selectedInvestmentAndloan, value); }
    }
    private InvestmentAndLoanInterest selectedInterest;
    public InvestmentAndLoanInterest SelectedInterest
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

