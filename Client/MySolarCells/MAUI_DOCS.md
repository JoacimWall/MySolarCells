# MySolarCells - .NET MAUI Application Documentation

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Core Features](#core-features)
- [Data Management](#data-management)
- [External Integrations](#external-integrations)
- [Development Guide](#development-guide)
- [Deployment](#deployment)

---

## Overview

**MySolarCells** is a cross-platform mobile application built with .NET MAUI for tracking solar energy production, consumption, and calculating return on investment (ROI). The application integrates with multiple solar inverters and electricity grid suppliers to provide comprehensive energy analytics.

### Target Platforms
- iOS 15.0+
- Android 26+ (Android 8.0)
- macOS Catalyst 15.0+

### Current Version
- **Version**: 1.0.1
- **Build**: 46
- **Package ID**: com.walltech.mysolarcells.beta

---

## Architecture

### Design Pattern: MVVM (Model-View-ViewModel)

The application follows a strict MVVM architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│              Views (XAML)                    │
│  - EnergyView, RoiView, ReportView, etc.    │
└─────────────┬───────────────────────────────┘
              │ Data Binding
┌─────────────▼───────────────────────────────┐
│         ViewModels (C#)                      │
│  - BaseViewModel, EnergyViewModel, etc.      │
└─────────────┬───────────────────────────────┘
              │ Service Calls
┌─────────────▼───────────────────────────────┐
│           Services Layer                     │
│  - DataSyncService, RoiService, etc.         │
└─────────────┬───────────────────────────────┘
              │ Data Access
┌─────────────▼───────────────────────────────┐
│      Data Layer (EF Core + SQLite)           │
│  - MscDbContext, Models                      │
└─────────────────────────────────────────────┘
```

### Key Technologies

- **.NET 9.0** - Target framework
- **MAUI 9.0.90** - Cross-platform UI framework
- **Entity Framework Core 9.0.7** - ORM with SQLite
- **CommunityToolkit.Mvvm 8.4.0** - MVVM helpers and source generators
- **Syncfusion.Maui.Charts 29.1.33** - Charting components
- **Acr.UserDialogs 9.2.2** - User interaction dialogs

---

## Project Structure

### Solution Organization

```
MySolarCells/
├── ViewModels/
│   ├── BaseViewModel.cs           # Base class for all ViewModels
│   ├── Roi/                        # ROI calculation ViewModels
│   ├── Energy/                     # Energy chart ViewModels
│   ├── More/                       # Settings ViewModels
│   └── OnBoarding/                 # First-time setup ViewModels
├── Views/
│   ├── Roi/                        # ROI screens
│   ├── Energy/                     # Energy chart screens
│   ├── More/                       # Settings screens
│   └── OnBoarding/                 # Onboarding flow screens
├── Services/
│   ├── Inverter/                   # Solar inverter integrations
│   │   ├── HuaweiService.cs
│   │   ├── HomeAssistentInverterService.cs
│   │   └── SolarEdgeService.cs
│   ├── GridSupplier/               # Grid supplier integrations
│   │   └── TibberService.cs
│   ├── DataSyncService.cs          # Data synchronization orchestration
│   ├── RoiService.cs               # ROI calculations
│   ├── EnergyChartService.cs       # Chart data preparation
│   └── HistoryDataService.cs       # Historical data aggregation
├── Models/                         # View models and DTOs
├── Helpers/                        # Utility classes
├── Converters/                     # XAML value converters
├── Controls/                       # Custom UI controls
├── Messages/                       # MVVM messenger messages
└── Resources/
    ├── Translations/               # Localization resources
    ├── Images/                     # Image assets
    ├── Fonts/                      # Custom fonts
    └── Styles/                     # XAML styles

MySolarCellsSQLite/
├── Sqlite/
│   ├── Models/                     # Database entities
│   └── MscDbContext.cs             # EF Core DbContext
└── Migrations/                     # EF Core migrations
```

### Key Files

| File | Purpose |
|------|---------|
| `MauiProgram.cs` | Application configuration and DI setup |
| `App.xaml.cs` | Application lifecycle and startup |
| `AppShell.xaml` | Shell navigation structure |
| `BaseViewModel.cs` | Base ViewModel with common functionality |
| `MscDbContext.cs` | Database context and configuration |
| `ServicesExtensions.cs` | Service registration |
| `ViewModelsExtensions.cs` | ViewModel registration |

---

## Core Features

### 1. Solar Energy Production Tracking

**Location**: `Views/Energy/EnergyView.xaml`, `ViewModels/Energy/EnergyViewModel.cs`

Tracks real-time solar panel production with breakdown:
- Energy sold to grid
- Energy used for own consumption
- Battery charging

**Time Ranges**:
- Today (hourly)
- Day (hourly)
- Week (daily aggregation)
- Month (daily aggregation)
- Year (monthly aggregation)

**Display Units**:
- kWh (energy)
- SEK (currency)

### 2. Energy Consumption Tracking

**Location**: Same as production tracking

Monitors household energy consumption:
- Energy purchased from grid
- Energy from own solar production
- Battery discharge usage

### 3. Return on Investment (ROI) Calculations

**Location**: `Services/RoiService.cs`, `Views/Roi/RoiView.xaml`

#### Historical ROI Analysis
Calculates actual savings based on production data:
```csharp
Profit from sold energy = ProductionSold × AveragePriceSold + TaxReduction
Savings from own use = ProductionOwnUse × AveragePriceOwnUse
Total savings accumulation over time
```

#### Future Projections (30-year)
Takes into account:
- Panel degradation over time
- Electricity price development trends
- Tax reduction changes (removed after 2026 in Sweden)
- Loan interest and amortization

**Swedish Market Specifics**:
- Tax reduction (skattereduktion): 0.60 SEK/kWh until 2026
- Grid compensation: 0.078 SEK/kWh
- Transfer fees: 0.30 SEK/kWh
- Energy tax: 0.49 SEK/kWh

### 4. Data Synchronization

**Location**: `Services/DataSyncService.cs`

Orchestrates data fetching from multiple sources:
1. Fetch consumption and spot prices from grid supplier
2. Fetch production data from solar inverter
3. Bulk insert/update database records
4. Handle data gaps with retry logic
5. Report progress to UI

**Performance Features**:
- Batch processing (100 records at a time)
- Async/await throughout
- EFCore.BulkExtensions for high-performance inserts
- Gap detection and automatic filling

### 5. Multi-Home Support

Users can configure:
- Multiple homes
- Multiple electricity suppliers per home
- Multiple inverters per home
- Per-home calculation parameters

### 6. Advanced Reporting

**Location**: `Views/Roi/ReportView.xaml`

Multi-page reports with:
- Savings calculation estimates (30-year projection)
- Year overview (all years summarized)
- Year details (month-by-month breakdown)
- Landscape orientation support
- Export-ready layout

---

## Data Management

### Database: SQLite with Entity Framework Core

**Database File**: `Db_v_2.db3`

**Location**:
- iOS/Android: `FileSystem.AppDataDirectory`
- macOS Catalyst: `FileSystem.AppDataDirectory/MySolarCells/`

### Core Entities

#### Energy
The primary data table storing hourly energy records:

```csharp
public class Energy
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }  // Unique index
    public int HomeId { get; set; }

    // Production
    public double ProductionSold { get; set; }
    public double ProductionOwnUse { get; set; }

    // Consumption
    public double ConsumptionPurchased { get; set; }
    public double BatteryUsed { get; set; }
    public double BatteryCharged { get; set; }

    // Pricing
    public double UnitPriceSold { get; set; }
    public double UnitPricePurchased { get; set; }
    public double Cost { get; set; }
    public double Profit { get; set; }

    // Sync status
    public bool SyncInverter { get; set; }
    public bool SyncGridSupplier { get; set; }
}
```

#### Home
Container for user configuration:
```csharp
public class Home
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string EnergyUnit { get; set; }  // "kWh"
    public string Currency { get; set; }     // "SEK"

    public List<ElectricitySupplier> ElectricitySuppliers { get; set; }
    public List<Inverter> Inverters { get; set; }
}
```

#### ElectricitySupplier
Grid supplier configuration:
```csharp
public class ElectricitySupplier
{
    public int Id { get; set; }
    public int HomeId { get; set; }
    public GridSupplierEnum SupplierType { get; set; }  // Tibber
    public string UserName { get; set; }
    public string Password { get; set; }  // Encrypted
    public string SubSystemEntityId { get; set; }  // Home ID in supplier system
}
```

#### Inverter
Solar inverter configuration:
```csharp
public class Inverter
{
    public int Id { get; set; }
    public int HomeId { get; set; }
    public InverterEnum InverterType { get; set; }  // Huawei, HomeAssistant, SolarEdge
    public string UserName { get; set; }
    public string Password { get; set; }  // Encrypted with key "jHjkhukwr4353!"
    public string SubSystemEntityId { get; set; }  // Site/device ID
}
```

#### EnergyCalculationParameter
Time-based calculation rules:
```csharp
public class EnergyCalculationParameter
{
    public int Id { get; set; }
    public int HomeId { get; set; }
    public DateTime FromDate { get; set; }  // Unique index

    public double TaxReduction { get; set; }
    public double TransferFee { get; set; }
    public double EnergyTax { get; set; }
    public double TotalInstalledPanelCapacity { get; set; }

    public bool UseSpotPrice { get; set; }
    public double FixedBuyPrice { get; set; }
    public double FixedSellPrice { get; set; }
}
```

#### InvestmentAndLoan
Investment tracking:
```csharp
public class InvestmentAndLoan
{
    public int Id { get; set; }
    public int HomeId { get; set; }
    public double TotalInvestment { get; set; }
    public double LoanAmount { get; set; }
    public double MonthlyAmortization { get; set; }

    public List<InterestRate> InterestRates { get; set; }
}
```

### Database Migration Strategy

**Auto-migration on startup**:
```csharp
// In MscDbContext.cs
Database.Migrate();
```

**Creating new migrations**:
```bash
# From MySolarCellsSQLite project directory
dotnet ef migrations add MigrationName --startup-project ../MySolarCells/MySolarCells.csproj
```

### Performance Optimizations

1. **Bulk Operations** (EFCore.BulkExtensions):
   ```csharp
   await dbContext.BulkInsertAsync(energies);
   await dbContext.BulkUpdateAsync(energies);
   ```

2. **Indexing**:
   - Unique index on `Energy.Timestamp`
   - Unique index on `EnergyCalculationParameter.FromDate`
   - Foreign key indexes automatically created

3. **No-tracking queries** for read-only operations:
   ```csharp
   dbContext.Energy.AsNoTracking().Where(...)
   ```

---

## External Integrations

### Grid Suppliers

#### Tibber
**File**: `Services/GridSupplier/TibberService.cs`

**API**: GraphQL at `https://api.tibber.com/v1-beta/gql`

**Authentication**: Bearer token in Authorization header

**Data Retrieved**:
- Hourly consumption with costs
- Hourly production sold with profit
- Spot prices (current + future up to 24h ahead)
- Account information

**Special Features**:
- 3-month batch imports
- Summer time handling (2-hour to 1-hour gap conversion)
- Spot-only import mode (for users without smart meter)

**Getting Access Token**:
1. Sign up at https://tibber.com
2. Get developer token at https://developer.tibber.com/settings/access-token

**Example GraphQL Query**:
```graphql
query {
  viewer {
    homes {
      consumption(resolution: HOURLY, first: 100) {
        nodes {
          from
          to
          cost
          consumption
        }
      }
      production(resolution: HOURLY, first: 100) {
        nodes {
          from
          to
          profit
          production
        }
      }
    }
  }
}
```

### Solar Inverters

#### 1. Huawei FusionSolar
**File**: `Services/Inverter/HuaweiService.cs`

**API**: REST at `https://eu5.fusionsolar.huawei.com`

**Authentication**: XSRF-TOKEN cookie-based

**Rate Limits**: 24 API calls per session

**Data**: Hourly production by site

**Setup**:
1. Create account at https://eu5.fusionsolar.huawei.com
2. Link inverter to account
3. Use account credentials in app

#### 2. Home Assistant
**File**: `Services/Inverter/HomeAssistentInverterService.cs`

**Protocol**: WebSocket

**Connection**: Local network

**Data**: Real-time sensor data

**Setup**:
1. Install Home Assistant on local network
2. Configure solar integration
3. Get long-lived access token
4. Enter Home Assistant URL and token in app

#### 3. SolarEdge
**File**: `Services/Inverter/SolarEdgeService.cs`

**API**: REST

**Authentication**: API key

**Data**: Production data

**Setup**:
1. Create account at monitoring.solaredge.com
2. Generate API key
3. Enter API key in app

---

## Development Guide

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 (17.8+) or Visual Studio Code
- For iOS: Xcode 15+, macOS 13+
- For Android: Android SDK 26+

### Getting Started

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd MySolarCells/Client
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Update Syncfusion license** (line 13 in `App.xaml.cs`):
   ```csharp
   Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
   ```
   Get free license at: https://www.syncfusion.com/sales/communitylicense

4. **Run the application**:
   ```bash
   # iOS
   dotnet build -t:Run -f net9.0-ios

   # Android
   dotnet build -t:Run -f net9.0-android

   # macOS
   dotnet build -t:Run -f net9.0-maccatalyst
   ```

### Dependency Injection Setup

**Services** are registered in `ServicesExtensions.cs`:
```csharp
public static IServiceCollection AddServices(this IServiceCollection services)
{
    services.AddSingleton<IDataSyncService, DataSyncService>();
    services.AddSingleton<IRoiService, RoiService>();
    // ... more services
    return services;
}
```

**ViewModels** are registered in `ViewModelsExtensions.cs`:
```csharp
public static IServiceCollection AddViewModels(this IServiceCollection services)
{
    services.AddTransient<EnergyViewModel>();
    services.AddTransient<RoiViewModel>();
    // ... more ViewModels
    return services;
}
```

**Access services** via constructor injection:
```csharp
public class EnergyViewModel : BaseViewModel
{
    private readonly IDataSyncService _dataSyncService;

    public EnergyViewModel(IDataSyncService dataSyncService)
    {
        _dataSyncService = dataSyncService;
    }
}
```

### MVVM Patterns Used

#### 1. Observable Properties (CommunityToolkit.Mvvm)
```csharp
[ObservableProperty]
private string title;

// Generates:
public string Title
{
    get => title;
    set => SetProperty(ref title, value);
}
```

#### 2. Relay Commands
```csharp
[RelayCommand]
private async Task SyncData()
{
    // Command implementation
}

// Generates: public IAsyncRelayCommand SyncDataCommand { get; }
```

#### 3. Messenger (WeakReferenceMessenger)
```csharp
// Send message
WeakReferenceMessenger.Default.Send(new DataSyncedMessage(data));

// Receive message
WeakReferenceMessenger.Default.Register<DataSyncedMessage>(this, (r, m) =>
{
    // Handle message
});
```

### BaseViewModel Features

All ViewModels inherit from `BaseViewModel` which provides:

- **View States**: Loading, Refreshing, Empty, Success, Error, Saving
- **Lifecycle Methods**: `OnAppearingAsync()`, `OnDisappearingAsync()`, `OnNavigatedTo()`
- **IsBusy Management**: Automatic busy state handling
- **CleanUp**: Memory management and messenger unregistration
- **Navigation**: Shell navigation helpers

**Example usage**:
```csharp
public class MyViewModel : BaseViewModel
{
    public override async Task OnAppearingAsync()
    {
        await base.OnAppearingAsync();

        ViewState = ViewStateEnum.Loading;

        try
        {
            // Load data
            ViewState = ViewStateEnum.Success;
        }
        catch (Exception ex)
        {
            ViewState = ViewStateEnum.Error;
        }
    }
}
```

### Navigation

**Shell-based routing**:
```csharp
// Register routes in AppShell.xaml.cs
Routing.RegisterRoute(nameof(EnergyCalculationParameterView), typeof(EnergyCalculationParameterView));

// Navigate
await Shell.Current.GoToAsync(nameof(EnergyCalculationParameterView));

// Navigate with parameters
await Shell.Current.GoToAsync($"{nameof(DetailView)}?id={itemId}");

// Navigate back
await Shell.Current.GoToAsync("..");
```

### Adding a New Feature

1. **Create Model** (if needed):
   ```csharp
   // MySolarCellsSQLite/Sqlite/Models/MyModel.cs
   public class MyModel
   {
       public int Id { get; set; }
       public string Name { get; set; }
   }
   ```

2. **Update DbContext**:
   ```csharp
   // MySolarCellsSQLite/Sqlite/MscDbContext.cs
   public DbSet<MyModel> MyModels { get; set; }
   ```

3. **Create Migration**:
   ```bash
   dotnet ef migrations add AddMyModel --startup-project ../MySolarCells/MySolarCells.csproj
   ```

4. **Create Service**:
   ```csharp
   // Services/IMyService.cs
   public interface IMyService
   {
       Task<Result<MyModel>> GetDataAsync();
   }

   // Services/MyService.cs
   public class MyService : IMyService
   {
       private readonly MscDbContext _context;

       public MyService(MscDbContext context)
       {
           _context = context;
       }

       public async Task<Result<MyModel>> GetDataAsync()
       {
           // Implementation
       }
   }
   ```

5. **Register Service**:
   ```csharp
   // ServicesExtensions.cs
   services.AddSingleton<IMyService, MyService>();
   ```

6. **Create ViewModel**:
   ```csharp
   // ViewModels/MyViewModel.cs
   public partial class MyViewModel : BaseViewModel
   {
       private readonly IMyService _myService;

       [ObservableProperty]
       private string data;

       public MyViewModel(IMyService myService)
       {
           _myService = myService;
       }

       [RelayCommand]
       private async Task LoadData()
       {
           var result = await _myService.GetDataAsync();
           if (result.IsSuccess)
           {
               Data = result.Value.Name;
           }
       }
   }
   ```

7. **Register ViewModel**:
   ```csharp
   // ViewModelsExtensions.cs
   services.AddTransient<MyViewModel>();
   ```

8. **Create View**:
   ```xml
   <!-- Views/MyView.xaml -->
   <ContentPage xmlns="..."
                x:Class="MySolarCells.Views.MyView"
                xmlns:vm="clr-namespace:MySolarCells.ViewModels"
                x:DataType="vm:MyViewModel">
       <Label Text="{Binding Data}" />
   </ContentPage>
   ```

### Testing

**Currently**: No test project exists

**Recommended Setup**:
```bash
# Create test project
dotnet new xunit -n MySolarCells.Tests
cd MySolarCells.Tests

# Add references
dotnet add reference ../MySolarCells/MySolarCells.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

**Example test**:
```csharp
public class RoiServiceTests
{
    [Fact]
    public async Task CalcSavingsEstimate_Should_Return_Correct_ROI_Year()
    {
        // Arrange
        var mockContext = new Mock<MscDbContext>();
        var service = new RoiService(mockContext.Object);

        // Act
        var result = await service.CalcSavingsEstimate(homeId: 1);

        // Assert
        result.Should().NotBeNull();
        result.RoiYear.Should().BeGreaterThan(0);
    }
}
```

---

## Deployment

### iOS Deployment

1. **Configure signing** in `MySolarCells.csproj`:
   ```xml
   <PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Release|net9.0-ios|AnyCPU'">
       <CodesignKey>iPhone Distribution</CodesignKey>
       <CodesignProvision>YourProvisioningProfile</CodesignProvision>
   </PropertyGroup>
   ```

2. **Update version** in `MySolarCells.csproj`:
   ```xml
   <ApplicationDisplayVersion>1.0.1</ApplicationDisplayVersion>
   <ApplicationVersion>46</ApplicationVersion>
   ```

3. **Build for release**:
   ```bash
   dotnet publish -f net9.0-ios -c Release
   ```

4. **Upload to App Store Connect**:
   - Use Xcode or Transporter app
   - Submit for review

### Android Deployment

1. **Configure signing** in `MySolarCells.csproj`:
   ```xml
   <PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Release|net9.0-android|AnyCPU'">
       <AndroidKeyStore>true</AndroidKeyStore>
       <AndroidSigningKeyStore>myapp.keystore</AndroidSigningKeyStore>
       <AndroidSigningKeyAlias>myapp</AndroidSigningKeyAlias>
   </PropertyGroup>
   ```

2. **Update version** in `MySolarCells.csproj`:
   ```xml
   <ApplicationDisplayVersion>1.0.1</ApplicationDisplayVersion>
   <ApplicationVersion>46</ApplicationVersion>
   ```

3. **Build release APK/AAB**:
   ```bash
   dotnet publish -f net9.0-android -c Release
   ```

4. **Upload to Google Play Console**:
   - Create release in Play Console
   - Upload AAB file
   - Submit for review

### macOS Deployment

1. **Configure signing**:
   ```xml
   <PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Release|net9.0-maccatalyst|AnyCPU'">
       <CodesignKey>Apple Distribution</CodesignKey>
       <CodesignProvision>YourMacProvisioningProfile</CodesignProvision>
       <CodesignEntitlements>Platforms/MacCatalyst/Entitlements.plist</CodesignEntitlements>
   </PropertyGroup>
   ```

2. **Build for release**:
   ```bash
   dotnet publish -f net9.0-maccatalyst -c Release
   ```

3. **Notarize** (required for distribution):
   ```bash
   xcrun notarytool submit MySolarCells.pkg --keychain-profile "AC_PASSWORD"
   ```

4. **Upload to App Store Connect**

### Environment-Specific Configuration

**Debug vs Release**:
```csharp
#if DEBUG
    builder.Logging.AddDebug();
#endif

#if RELEASE
    builder.Services.AddAppCenterCrashes();
#endif
```

### Version Management

**Semantic Versioning**: MAJOR.MINOR.PATCH (e.g., 1.0.1)

**Build Number**: Increment for each release (currently 46)

**Update locations**:
1. `MySolarCells.csproj` - ApplicationDisplayVersion and ApplicationVersion
2. App stores when submitting

---

## Common Issues and Solutions

### Issue: Database migration fails

**Solution**: Delete local database and restart app
```bash
# iOS Simulator
~/Library/Developer/CoreSimulator/Devices/[DEVICE-ID]/data/Containers/Data/Application/[APP-ID]/Library/Db_v_2.db3

# Android Emulator
adb shell run-as com.walltech.mysolarcells.beta
cd files
rm Db_v_2.db3
```

### Issue: Charts not displaying correctly

**Solution**: Check Syncfusion license registration in App.xaml.cs

### Issue: API sync failing

**Solutions**:
1. Verify API credentials are correct
2. Check network connectivity
3. Review API rate limits (Huawei: 24 calls per session)
4. Check API endpoint availability

### Issue: Memory leaks in ViewModels

**Solution**: Ensure proper cleanup in BaseViewModel:
```csharp
public override void CleanUp()
{
    base.CleanUp();
    WeakReferenceMessenger.Default.Unregister<MyMessage>(this);
    // Dispose other resources
}
```

---

## Resources

### Documentation
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Syncfusion MAUI Charts](https://help.syncfusion.com/maui/cartesian-charts/getting-started)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)

### API Documentation
- [Tibber API](https://developer.tibber.com/docs/overview)
- [Huawei FusionSolar API](https://forum.huawei.com/enterprise/en/fusionsolar-api-documentation/thread/789585-100027)
- [Home Assistant API](https://developers.home-assistant.io/docs/api/websocket)

### Community
- [.NET MAUI GitHub](https://github.com/dotnet/maui)
- [.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui)

---

## License

**Syncfusion Community License**: Required for Syncfusion components

**App License**: [Your License Here]

---

## Support

For issues and questions:
- GitHub Issues: [Your Repository]
- Email: [Your Support Email]

---

**Last Updated**: 2025-11-06
**Document Version**: 1.0