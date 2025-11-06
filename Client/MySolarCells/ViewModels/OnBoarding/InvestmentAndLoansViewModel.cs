namespace MySolarCells.ViewModels.OnBoarding;

public class InvestmentAndLoanViewModel : BaseViewModel
{
    private readonly MscDbContext mscDbContext;
    public InvestmentAndLoanViewModel(MscDbContext mscDbContext, IDialogService dialogService,
        IAnalyticsService analyticsService, IInternetConnectionService internetConnectionService, ILogService logService, ISettingsService settingsService, IHomeService homeService) : base(dialogService, analyticsService, internetConnectionService,
        logService, settingsService, homeService)
    {

        this.mscDbContext = mscDbContext;
        var result = this.mscDbContext.InvestmentAndLon.Include(i => i.Interest).Where(x => x.HomeId == HomeService.CurrentHome().HomeId).ToList();
        foreach (var item in result)
        {
            InvestmentAndLoans.Add(item);
        }
        if (InvestmentAndLoans.Count > 0)
        {
            selectedInvestmentAndLoan = InvestmentAndLoans.Last();
        }
        else
        {
            AddInvestLon();
        }

    }

    public ICommand AddInvestmentLonCommand => new Command(AddInvestLon);
    public ICommand AddInterestCommand => new Command(AddInterest);
    public ICommand SaveCommand => new Command(async () => await Save());


    private async Task Save()
    {

        if (selectedInvestmentAndLoan.InvestmentAndLoanId == 0)
            mscDbContext.InvestmentAndLon.Add(selectedInvestmentAndLoan);
        else //update
        {
            var dbEntity = mscDbContext.InvestmentAndLon.Include(i => i.Interest).First(x => x.InvestmentAndLoanId == selectedInvestmentAndLoan.InvestmentAndLoanId);
            dbEntity.FromDate = selectedInvestmentAndLoan.FromDate;
            dbEntity.Investment = selectedInvestmentAndLoan.Investment;
            dbEntity.Description = selectedInvestmentAndLoan.Description;
            dbEntity.Loan = selectedInvestmentAndLoan.Loan;

            //dbEntity.Interest.First().Interest = SelectedInterest.Interest;

        }
        await mscDbContext.SaveChangesAsync();
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

    private void AddInterest()
    {
        SelectedInvestmentAndLoan.Interest.Add(new InvestmentAndLoanInterest { Description = AppResources.My_Description, FromDate = DateTime.Today });
        SelectedInterest = SelectedInvestmentAndLoan.Interest.Last();
    }

    private void AddInvestLon()
    {
        InvestmentAndLoans.Add(new InvestmentAndLoan { Description = AppResources.Investment_And_Loan, HomeId = HomeService.CurrentHome().HomeId });
        SelectedInvestmentAndLoan = InvestmentAndLoans.Last();

    }

    private ObservableCollection<InvestmentAndLoan> investmentAndLoans = new ObservableCollection<InvestmentAndLoan>();
    public ObservableCollection<InvestmentAndLoan> InvestmentAndLoans
    {
        get => investmentAndLoans;
        set => SetProperty(ref investmentAndLoans, value);
    }

    private InvestmentAndLoan selectedInvestmentAndLoan = new() { Description = "" };
    public InvestmentAndLoan SelectedInvestmentAndLoan
    {
        get => selectedInvestmentAndLoan;
        set
        {
            SetProperty(ref selectedInvestmentAndLoan, value);
            if (selectedInvestmentAndLoan.Interest.Count > 0)
                SelectedInterest = selectedInvestmentAndLoan.Interest.Last();
        }
    }
    private InvestmentAndLoanInterest? selectedInterest;
    public InvestmentAndLoanInterest? SelectedInterest
    {
        get => selectedInterest;
        set => SetProperty(ref selectedInterest, value);
    }
    private bool isOnboardingMode;
    public bool IsOnbordingMode
    {
        get => isOnboardingMode;
        set => SetProperty(ref isOnboardingMode, value);
    }


}

