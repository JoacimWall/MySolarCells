type TranslationKey =
  // Panel header & tabs
  | "panel.title"
  | "tab.overview"
  | "tab.roi"
  | "tab.fakta"
  | "tab.sensors"
  | "tab.yearlyParams"
  | "tab.hourlyEnergy"
  // Common
  | "common.year"
  | "common.yes"
  | "common.no"
  | "common.loading"
  | "common.error"
  | "common.noData"
  | "common.save"
  | "common.cancel"
  | "common.delete"
  | "common.refresh"
  | "common.add"
  | "common.na"
  | "common.never"
  // Chart
  | "chart.ownUse"
  | "chart.sold"
  | "chart.today"
  | "chart.thisWeek"
  | "chart.thisMonth"
  | "chart.thisYear"
  // Overview
  | "overview.loadingOverview"
  | "overview.noData"
  | "overview.dbSummary"
  | "overview.lastTibberSync"
  | "overview.hourlyRecords"
  | "overview.firstRecord"
  | "overview.lastRecord"
  | "overview.energySummary"
  | "overview.yearlyParams"
  | "overview.taxReduction"
  | "overview.gridComp"
  | "overview.transferFee"
  | "overview.energyTax"
  | "overview.installedKw"
  // Hourly
  | "hourly.title"
  | "hourly.startDate"
  | "hourly.endDate"
  | "hourly.loadBtn"
  | "hourly.loadingBtn"
  | "hourly.selectDateRange"
  | "hourly.timestamp"
  | "hourly.purchasedKwh"
  | "hourly.costSek"
  | "hourly.soldKwh"
  | "hourly.profitSek"
  | "hourly.ownUseKwh"
  | "hourly.savedSek"
  | "hourly.priceLevel"
  | "hourly.synced"
  | "hourly.enriched"
  | "hourly.prev"
  | "hourly.next"
  | "hourly.pageInfo"
  // Sensors
  | "sensors.loadingSensors"
  | "sensors.title"
  | "sensors.infoBox"
  | "sensors.status"
  | "sensors.role"
  | "sensors.description"
  | "sensors.entityId"
  | "sensors.currentState"
  | "sensors.lastStoredReading"
  | "sensors.required"
  | "sensors.optional"
  | "sensors.requiredMissing"
  | "sensors.requiredMissingText"
  | "sensors.usingTibberApi"
  | "sensors.notConfigured"
  // Yearly params
  | "params.loadingParams"
  | "params.waitingForData"
  | "params.title"
  | "params.addYear"
  | "params.selectYear"
  | "params.noParams"
  | "params.taxReduction"
  | "params.gridComp"
  | "params.transferFee"
  | "params.energyTax"
  | "params.installedKw"
  | "params.editYear"
  | "params.taxReductionLabel"
  | "params.gridCompLabel"
  | "params.transferFeeLabel"
  | "params.energyTaxLabel"
  | "params.installedKwLabel"
  | "params.saving"
  | "params.deleteConfirm"
  // ROI
  | "roi.loadingRoi"
  | "roi.noData"
  | "roi.title"
  | "roi.investment"
  | "roi.howCalculated"
  | "roi.explainHistorical"
  | "roi.explainSoldPrice"
  | "roi.explainOwnUsePrice"
  | "roi.explainFuture"
  | "roi.explainPriceDev"
  | "roi.explainPanelDeg"
  | "roi.explainSavingsSold"
  | "roi.explainSavingsOwnUse"
  | "roi.explainRemaining"
  | "roi.explainRoiYear"
  | "roi.explainTaxReduction"
  | "roi.explainNote"
  | "roi.priceDevLabel"
  | "roi.panelDegLabel"
  | "roi.calculating"
  | "roi.calculate"
  | "roi.colIndex"
  | "roi.colYear"
  | "roi.colAvgPriceSold"
  | "roi.colAvgPriceOwnUse"
  | "roi.colProdSold"
  | "roi.colProdOwnUse"
  | "roi.colSavingsSold"
  | "roi.colSavingsOwnUse"
  | "roi.colReturnPct"
  | "roi.colRemaining"
  // Fakta
  | "fakta.periodToday"
  | "fakta.periodDay"
  | "fakta.periodWeek"
  | "fakta.periodMonth"
  | "fakta.periodYear"
  | "fakta.todayLabel"
  | "fakta.productionTitle"
  | "fakta.sold"
  | "fakta.ownUse"
  | "fakta.batteryCharge"
  | "fakta.batteryUse"
  | "fakta.purchased"
  | "fakta.production"
  | "fakta.consumption"
  | "fakta.balance"
  | "fakta.costTitle"
  | "fakta.prodSold"
  | "fakta.prodGridComp"
  | "fakta.prodTaxReduction"
  | "fakta.ownUseSpot"
  | "fakta.ownUseTransfer"
  | "fakta.ownUseEnergyTax"
  | "fakta.battSpot"
  | "fakta.battTransfer"
  | "fakta.battEnergyTax"
  | "fakta.purchasedCost"
  | "fakta.purchasedTransfer"
  | "fakta.purchasedTax"
  | "fakta.costProduction"
  | "fakta.costConsumption"
  | "fakta.costBalance"
  | "fakta.simTitle"
  | "fakta.simTitleWithBattery"
  | "fakta.simTitleWithoutBattery"
  | "fakta.simAddBattery"
  | "fakta.simRemoveBattery"
  | "fakta.simCalculating"
  | "fakta.simCalculate"
  | "fakta.factsTitle"
  | "fakta.factsProductionIndex"
  | "fakta.factsAvgPriceSold"
  | "fakta.factsAvgPricePurchased"
  | "fakta.factsAvgPriceOwnUse"
  | "fakta.factsPeakReduction";

const en: Record<TranslationKey, string> = {
  // Panel header & tabs
  "panel.title": "Solar Data",
  "tab.overview": "Overview",
  "tab.roi": "ROI",
  "tab.fakta": "Facts",
  "tab.sensors": "Sensors",
  "tab.yearlyParams": "Yearly Params",
  "tab.hourlyEnergy": "Hourly Energy",
  // Common
  "common.year": "Year",
  "common.yes": "Yes",
  "common.no": "No",
  "common.loading": "Loading...",
  "common.error": "Error",
  "common.noData": "No data available",
  "common.save": "Save",
  "common.cancel": "Cancel",
  "common.delete": "Delete",
  "common.refresh": "Refresh",
  "common.add": "Add",
  "common.na": "N/A",
  "common.never": "Never",
  // Chart
  "chart.ownUse": "Own Use",
  "chart.sold": "Sold",
  "chart.today": "Today",
  "chart.thisWeek": "This Week",
  "chart.thisMonth": "This Month",
  "chart.thisYear": "This Year",
  // Overview
  "overview.loadingOverview": "Loading overview...",
  "overview.noData": "No data available",
  "overview.dbSummary": "Database Summary",
  "overview.lastTibberSync": "Last Tibber Sync",
  "overview.hourlyRecords": "Hourly Records",
  "overview.firstRecord": "First Record",
  "overview.lastRecord": "Last Record",
  "overview.energySummary": "Energy Summary",
  "overview.yearlyParams": "Yearly Financial Parameters",
  "overview.taxReduction": "Tax Reduction",
  "overview.gridComp": "Grid Comp.",
  "overview.transferFee": "Transfer Fee",
  "overview.energyTax": "Energy Tax",
  "overview.installedKw": "Installed kW",
  // Hourly
  "hourly.title": "Hourly Energy Records",
  "hourly.startDate": "Start Date",
  "hourly.endDate": "End Date",
  "hourly.loadBtn": "Load",
  "hourly.loadingBtn": "Loading...",
  "hourly.selectDateRange": "Select a date range and click Load",
  "hourly.timestamp": "Timestamp",
  "hourly.purchasedKwh": "Purchased kWh",
  "hourly.costSek": "Cost SEK",
  "hourly.soldKwh": "Sold kWh",
  "hourly.profitSek": "Profit SEK",
  "hourly.ownUseKwh": "Own Use kWh",
  "hourly.savedSek": "Saved SEK",
  "hourly.priceLevel": "Price Level",
  "hourly.synced": "Synced",
  "hourly.enriched": "Enriched",
  "hourly.prev": "Prev",
  "hourly.next": "Next",
  "hourly.pageInfo": "Page {0} of {1} ({2} records)",
  // Sensors
  "sensors.loadingSensors": "Loading sensor configuration...",
  "sensors.title": "Sensor Configuration",
  "sensors.infoBox":
    "Only the <strong>production</strong> sensor is required \u2014 it is used to calculate <strong>production_own_use</strong> (total production minus grid export). All other data (grid export, grid import) comes from the <strong>Tibber API</strong> by default. You can optionally override grid export/import with HA sensors for higher accuracy, and add battery sensors if you have a battery. Sensors are configured in the integration setup flow.",
  "sensors.status": "Status",
  "sensors.role": "Role",
  "sensors.description": "Description",
  "sensors.entityId": "Entity ID",
  "sensors.currentState": "Current State",
  "sensors.lastStoredReading": "Last Stored Reading",
  "sensors.required": "Required",
  "sensors.optional": "Optional",
  "sensors.requiredMissing": "Required Sensor Missing",
  "sensors.requiredMissingText":
    "The <strong>production</strong> sensor is not configured. Without it, <strong>production_own_use</strong> cannot be calculated. Please configure it in the integration setup flow.",
  "sensors.usingTibberApi": "Using Tibber API",
  "sensors.notConfigured": "Not configured",
  // Yearly params
  "params.loadingParams": "Loading yearly parameters...",
  "params.waitingForData": "Waiting for data...",
  "params.title": "Yearly Financial Parameters",
  "params.addYear": "Add Year",
  "params.selectYear": "Select year...",
  "params.noParams": "No yearly parameters configured yet. Add a year above.",
  "params.taxReduction": "Tax Reduction",
  "params.gridComp": "Grid Comp.",
  "params.transferFee": "Transfer Fee",
  "params.energyTax": "Energy Tax",
  "params.installedKw": "Installed kW",
  "params.editYear": "Edit {0}",
  "params.taxReductionLabel": "Tax Reduction (SEK/kWh)",
  "params.gridCompLabel": "Grid Compensation (SEK/kWh)",
  "params.transferFeeLabel": "Transfer Fee (SEK/kWh)",
  "params.energyTaxLabel": "Energy Tax (SEK/kWh)",
  "params.installedKwLabel": "Installed kW",
  "params.saving": "Saving...",
  "params.deleteConfirm": "Delete parameters for {0}?",
  // ROI
  "roi.loadingRoi": "Loading ROI projection...",
  "roi.noData": "No ROI projection data available.",
  "roi.title": "ROI Projection",
  "roi.investment": "Investment: <strong>{0} SEK</strong>",
  "roi.howCalculated": "How is the ROI calculated?",
  "roi.explainHistorical":
    "Historical years use actual production and price data from Tibber. If the current year is incomplete, each missing month is filled using the average price and production for that specific month from up to 3 prior years. If no prior data exists for a month, the previous month\u2019s price is used as fallback.",
  "roi.explainSoldPrice":
    "Sold price = spot price + grid compensation (n\u00e4tnytta) + tax reduction (skattereduktion, until 2026).",
  "roi.explainOwnUsePrice":
    "Own use price = avoided purchase cost (spot price + transfer fee + energy tax). If a battery is present, it is the average of own-use and battery savings per kWh.",
  "roi.explainFuture":
    "Future years are projected using monthly average prices and production from up to 3 of the most recent historical years. Each calendar month is averaged across however many years have data for it. Price development and panel degradation are then applied per month and summed to yearly totals. This captures seasonal variation \u2014 summer has high production but typically lower prices, winter the opposite.",
  "roi.explainPriceDev":
    "Prices increase each year by the price development percentage. E.g. 5% means next year\u2019s price = this year\u2019s price \u00d7 1.05.",
  "roi.explainPanelDeg":
    "Production decreases each year by the panel degradation percentage. E.g. 0.25% means next year\u2019s production = this year\u2019s \u00d7 0.9975.",
  "roi.explainSavingsSold":
    "Savings sold = production sold \u00d7 average sold price",
  "roi.explainSavingsOwnUse":
    "Savings own use = own use production \u00d7 average own use price",
  "roi.explainRemaining":
    "Remaining = investment \u2212 cumulative total savings",
  "roi.explainRoiYear":
    "The ROI year (green row) is when cumulative savings exceed the investment.",
  "roi.explainTaxReduction":
    "Tax reduction (skattereduktion) is removed from sold price starting 2026.",
  "roi.explainNote":
    "Note: The table shows yearly weighted averages, but all savings calculations use monthly granularity for accuracy.",
  "roi.priceDevLabel": "Price development (%/year)",
  "roi.panelDegLabel": "Panel degradation (%/year)",
  "roi.calculating": "Calculating...",
  "roi.calculate": "Calculate",
  "roi.colIndex": "#",
  "roi.colYear": "Year",
  "roi.colAvgPriceSold": "Avg price sold",
  "roi.colAvgPriceOwnUse": "Avg price own use",
  "roi.colProdSold": "Prod. sold (kWh)",
  "roi.colProdOwnUse": "Prod. own use (kWh)",
  "roi.colSavingsSold": "Savings sold (SEK)",
  "roi.colSavingsOwnUse": "Savings own use (SEK)",
  "roi.colReturnPct": "Return %",
  "roi.colRemaining": "Remaining (SEK)",
  // Fakta
  "fakta.periodToday": "Today",
  "fakta.periodDay": "Day",
  "fakta.periodWeek": "Week",
  "fakta.periodMonth": "Month",
  "fakta.periodYear": "Year",
  "fakta.todayLabel": "TODAY",
  "fakta.productionTitle": "Production and consumption",
  "fakta.sold": "Sold",
  "fakta.ownUse": "Own use",
  "fakta.batteryCharge": "Battery charge",
  "fakta.batteryUse": "Battery use",
  "fakta.purchased": "Purchased",
  "fakta.production": "Production",
  "fakta.consumption": "Consumption",
  "fakta.balance": "Balance (prod. \u2212 purchased)",
  "fakta.costTitle": "Costs and revenue",
  "fakta.prodSold": "Prod. sold",
  "fakta.prodGridComp": "Prod. grid comp.",
  "fakta.prodTaxReduction": "Prod. tax reduction",
  "fakta.ownUseSpot": "Own use spot price",
  "fakta.ownUseTransfer": "Own use transfer",
  "fakta.ownUseEnergyTax": "Own use energy tax",
  "fakta.battSpot": "Batt. use spot price",
  "fakta.battTransfer": "Batt. use transfer",
  "fakta.battEnergyTax": "Batt. use energy tax",
  "fakta.purchasedCost": "Purchased elec. cost",
  "fakta.purchasedTransfer": "Purchased transfer fee",
  "fakta.purchasedTax": "Purchased energy tax",
  "fakta.costProduction": "Production",
  "fakta.costConsumption": "Consumption",
  "fakta.costBalance": "Balance (prod. \u2212 purchased)",
  "fakta.simTitle": "Simulation",
  "fakta.simTitleWithBattery": "Simulation (with {0} kWh battery)",
  "fakta.simTitleWithoutBattery": "Simulation (without battery)",
  "fakta.simAddBattery": "With battery",
  "fakta.simRemoveBattery": "Without battery",
  "fakta.simCalculating": "Calculating...",
  "fakta.simCalculate": "Calculate",
  "fakta.factsTitle": "Facts",
  "fakta.factsProductionIndex": "Production index (prod/day)",
  "fakta.factsAvgPriceSold": "Avg price sold",
  "fakta.factsAvgPricePurchased": "Avg price purchased",
  "fakta.factsAvgPriceOwnUse": "Avg price own use",
  "fakta.factsPeakReduction": "Own use peak consumption reduction",
};

const sv: Record<TranslationKey, string> = {
  // Panel header & tabs
  "panel.title": "Soldata",
  "tab.overview": "\u00d6versikt",
  "tab.roi": "ROI",
  "tab.fakta": "Fakta",
  "tab.sensors": "Sensorer",
  "tab.yearlyParams": "\u00c5rsparametrar",
  "tab.hourlyEnergy": "Timdata",
  // Common
  "common.year": "\u00c5r",
  "common.yes": "Ja",
  "common.no": "Nej",
  "common.loading": "Laddar...",
  "common.error": "Fel",
  "common.noData": "Ingen data tillg\u00e4nglig",
  "common.save": "Spara",
  "common.cancel": "Avbryt",
  "common.delete": "Ta bort",
  "common.refresh": "Uppdatera",
  "common.add": "L\u00e4gg till",
  "common.na": "N/A",
  "common.never": "Aldrig",
  // Chart
  "chart.ownUse": "Eget anv.",
  "chart.sold": "S\u00e5ld",
  "chart.today": "Idag",
  "chart.thisWeek": "Denna vecka",
  "chart.thisMonth": "Denna m\u00e5nad",
  "chart.thisYear": "I \u00e5r",
  // Overview
  "overview.loadingOverview": "Laddar \u00f6versikt...",
  "overview.noData": "Ingen data tillg\u00e4nglig",
  "overview.dbSummary": "Databas\u00f6versikt",
  "overview.lastTibberSync": "Senaste Tibber-synk",
  "overview.hourlyRecords": "Timposter",
  "overview.firstRecord": "F\u00f6rsta post",
  "overview.lastRecord": "Senaste post",
  "overview.energySummary": "Energi\u00f6versikt",
  "overview.yearlyParams": "\u00c5rliga ekonomiska parametrar",
  "overview.taxReduction": "Skattereduktion",
  "overview.gridComp": "N\u00e4tnytta",
  "overview.transferFee": "\u00d6verf\u00f6ring",
  "overview.energyTax": "Energiskatt",
  "overview.installedKw": "Installerad kW",
  // Hourly
  "hourly.title": "Timdata f\u00f6r energi",
  "hourly.startDate": "Startdatum",
  "hourly.endDate": "Slutdatum",
  "hourly.loadBtn": "Ladda",
  "hourly.loadingBtn": "Laddar...",
  "hourly.selectDateRange": "V\u00e4lj datumintervall och klicka Ladda",
  "hourly.timestamp": "Tidst\u00e4mpel",
  "hourly.purchasedKwh": "K\u00f6pt kWh",
  "hourly.costSek": "Kostnad SEK",
  "hourly.soldKwh": "S\u00e5ld kWh",
  "hourly.profitSek": "Int\u00e4kt SEK",
  "hourly.ownUseKwh": "Eget anv. kWh",
  "hourly.savedSek": "Besparing SEK",
  "hourly.priceLevel": "Prisgrupp",
  "hourly.synced": "Synkad",
  "hourly.enriched": "Berikad",
  "hourly.prev": "F\u00f6reg.",
  "hourly.next": "N\u00e4sta",
  "hourly.pageInfo": "Sida {0} av {1} ({2} poster)",
  // Sensors
  "sensors.loadingSensors": "Laddar sensorkonfiguration...",
  "sensors.title": "Sensorkonfiguration",
  "sensors.infoBox":
    "Bara <strong>produktionssensorn</strong> kr\u00e4vs \u2014 den anv\u00e4nds f\u00f6r att ber\u00e4kna <strong>production_own_use</strong> (total produktion minus n\u00e4texport). All annan data (n\u00e4texport, n\u00e4timport) h\u00e4mtas fr\u00e5n <strong>Tibber API</strong> som standard. Du kan valfritt \u00e5sidos\u00e4tta n\u00e4texport/import med HA-sensorer f\u00f6r h\u00f6gre noggrannhet, och l\u00e4gga till batterisensorer om du har ett batteri. Sensorer konfigureras i integrationsfl\u00f6det.",
  "sensors.status": "Status",
  "sensors.role": "Roll",
  "sensors.description": "Beskrivning",
  "sensors.entityId": "Entitets-ID",
  "sensors.currentState": "Aktuellt v\u00e4rde",
  "sensors.lastStoredReading": "Senast lagrad avl\u00e4sning",
  "sensors.required": "Obligatorisk",
  "sensors.optional": "Valfri",
  "sensors.requiredMissing": "Obligatorisk sensor saknas",
  "sensors.requiredMissingText":
    "<strong>Produktionssensorn</strong> \u00e4r inte konfigurerad. Utan den kan inte <strong>production_own_use</strong> ber\u00e4knas. Konfigurera den i integrationsfl\u00f6det.",
  "sensors.usingTibberApi": "Anv\u00e4nder Tibber API",
  "sensors.notConfigured": "Ej konfigurerad",
  // Yearly params
  "params.loadingParams": "Laddar \u00e5rsparametrar...",
  "params.waitingForData": "V\u00e4ntar p\u00e5 data...",
  "params.title": "\u00c5rliga ekonomiska parametrar",
  "params.addYear": "L\u00e4gg till \u00e5r",
  "params.selectYear": "V\u00e4lj \u00e5r...",
  "params.noParams":
    "Inga \u00e5rsparametrar konfigurerade \u00e4n. L\u00e4gg till ett \u00e5r ovan.",
  "params.taxReduction": "Skattereduktion",
  "params.gridComp": "N\u00e4tnytta",
  "params.transferFee": "\u00d6verf\u00f6ring",
  "params.energyTax": "Energiskatt",
  "params.installedKw": "Installerad kW",
  "params.editYear": "Redigera {0}",
  "params.taxReductionLabel": "Skattereduktion (SEK/kWh)",
  "params.gridCompLabel": "N\u00e4tnytta (SEK/kWh)",
  "params.transferFeeLabel": "\u00d6verf\u00f6ringsavgift (SEK/kWh)",
  "params.energyTaxLabel": "Energiskatt (SEK/kWh)",
  "params.installedKwLabel": "Installerad kW",
  "params.saving": "Sparar...",
  "params.deleteConfirm": "Ta bort parametrar f\u00f6r {0}?",
  // ROI
  "roi.loadingRoi": "Laddar ROI-prognos...",
  "roi.noData": "Ingen ROI-prognosdata tillg\u00e4nglig.",
  "roi.title": "ROI-prognos",
  "roi.investment": "Investering: <strong>{0} SEK</strong>",
  "roi.howCalculated": "Hur ber\u00e4knas ROI?",
  "roi.explainHistorical":
    "Historiska \u00e5r anv\u00e4nder faktisk produktion och prisdata fr\u00e5n Tibber. Om innev\u00e4rande \u00e5r \u00e4r ofullst\u00e4ndigt fylls varje saknad m\u00e5nad med snittv\u00e4rden f\u00f6r pris och produktion f\u00f6r den specifika m\u00e5naden fr\u00e5n upp till 3 f\u00f6reg\u00e5ende \u00e5r. Om ingen data finns f\u00f6r en m\u00e5nad anv\u00e4nds f\u00f6reg\u00e5ende m\u00e5nads pris som reserv.",
  "roi.explainSoldPrice":
    "S\u00e4ljpris = spotpris + n\u00e4tnytta + skattereduktion (till 2026).",
  "roi.explainOwnUsePrice":
    "Eget anv. pris = undvikt ink\u00f6pskostnad (spotpris + \u00f6verf\u00f6ringsavgift + energiskatt). Om batteri finns \u00e4r det genomsnittet av eget anv. och batteribesparing per kWh.",
  "roi.explainFuture":
    "Framtida \u00e5r projiceras med m\u00e5nadssnitt f\u00f6r priser och produktion fr\u00e5n upp till 3 av de senaste historiska \u00e5ren. Varje kalenderm\u00e5nad medelviktas \u00f6ver de \u00e5r som har data. Prisutveckling och paneldegradation appliceras per m\u00e5nad och summeras till \u00e5rstotaler. Detta f\u00e5ngar s\u00e4songsvariationer \u2014 sommaren har h\u00f6g produktion men l\u00e4gre priser, vintern tv\u00e4rtom.",
  "roi.explainPriceDev":
    "Priserna \u00f6kar varje \u00e5r med prisutvecklingsprocenten. T.ex. 5% inneb\u00e4r att n\u00e4sta \u00e5rs pris = detta \u00e5rs pris \u00d7 1,05.",
  "roi.explainPanelDeg":
    "Produktionen minskar varje \u00e5r med paneldegraderingsprocenten. T.ex. 0,25% inneb\u00e4r att n\u00e4sta \u00e5rs produktion = detta \u00e5rs \u00d7 0,9975.",
  "roi.explainSavingsSold":
    "Besparing s\u00e5ld = produktion s\u00e5ld \u00d7 genomsnittligt s\u00e4ljpris",
  "roi.explainSavingsOwnUse":
    "Besparing eget anv. = eget anv\u00e4ndning \u00d7 genomsnittligt eget anv. pris",
  "roi.explainRemaining":
    "\u00c5terst\u00e5ende = investering \u2212 ackumulerad total besparing",
  "roi.explainRoiYear":
    "ROI-\u00e5ret (gr\u00f6n rad) \u00e4r n\u00e4r ackumulerad besparing \u00f6verstiger investeringen.",
  "roi.explainTaxReduction":
    "Skattereduktionen tas bort fr\u00e5n s\u00e4ljpriset fr\u00e5n 2026.",
  "roi.explainNote":
    "Obs: Tabellen visar \u00e5rliga viktade snittv\u00e4rden, men alla besparingsber\u00e4kningar anv\u00e4nder m\u00e5nadsgranularitet f\u00f6r noggrannhet.",
  "roi.priceDevLabel": "Prisutveckling (%/\u00e5r)",
  "roi.panelDegLabel": "Paneldegradation (%/\u00e5r)",
  "roi.calculating": "Ber\u00e4knar...",
  "roi.calculate": "Ber\u00e4kna",
  "roi.colIndex": "#",
  "roi.colYear": "\u00c5r",
  "roi.colAvgPriceSold": "Snittpris s\u00e5ld",
  "roi.colAvgPriceOwnUse": "Snittpris eget anv.",
  "roi.colProdSold": "Prod. s\u00e5ld (kWh)",
  "roi.colProdOwnUse": "Prod. eget anv. (kWh)",
  "roi.colSavingsSold": "Besparing s\u00e5ld (SEK)",
  "roi.colSavingsOwnUse": "Besparing eget anv. (SEK)",
  "roi.colReturnPct": "Avkastning %",
  "roi.colRemaining": "\u00c5terst\u00e5ende (SEK)",
  // Fakta
  "fakta.periodToday": "Idag",
  "fakta.periodDay": "Dag",
  "fakta.periodWeek": "Vecka",
  "fakta.periodMonth": "M\u00e5nad",
  "fakta.periodYear": "\u00c5r",
  "fakta.todayLabel": "IDAG",
  "fakta.productionTitle": "Produktion och konsumtion",
  "fakta.sold": "S\u00e5ld",
  "fakta.ownUse": "Eget anv.",
  "fakta.batteryCharge": "Batteriladdning",
  "fakta.batteryUse": "Batteri anv.",
  "fakta.purchased": "K\u00f6pt",
  "fakta.production": "Produktion",
  "fakta.consumption": "Konsumtion",
  "fakta.balance": "Balans (prod. \u2212 ink\u00f6p)",
  "fakta.costTitle": "Kostnader och int\u00e4kter",
  "fakta.prodSold": "Prod s\u00e5lt",
  "fakta.prodGridComp": "Prod n\u00e4tnytta",
  "fakta.prodTaxReduction": "Prod energiskatt",
  "fakta.ownUseSpot": "Eget anv. spotpris",
  "fakta.ownUseTransfer": "Eget anv. \u00f6verf\u00f6ring",
  "fakta.ownUseEnergyTax": "Eget anv. energiskatt",
  "fakta.battSpot": "Batt. anv. spotpris",
  "fakta.battTransfer": "Batt. anv. \u00f6verf\u00f6ring",
  "fakta.battEnergyTax": "Batt. anv. energiskatt",
  "fakta.purchasedCost": "K\u00f6pt el kostnad",
  "fakta.purchasedTransfer": "K\u00f6pt \u00f6verf\u00f6ringavgift",
  "fakta.purchasedTax": "K\u00f6pt energiskatt",
  "fakta.costProduction": "Produktion",
  "fakta.costConsumption": "Konsumtion",
  "fakta.costBalance": "Balans (prod. \u2212 ink\u00f6pt)",
  "fakta.simTitle": "Simulering",
  "fakta.simTitleWithBattery": "Simulering (med {0} kWh batteri i ber\u00e4kning)",
  "fakta.simTitleWithoutBattery": "Simulering (utan batteri i ber\u00e4kning)",
  "fakta.simAddBattery": "Med batteri",
  "fakta.simRemoveBattery": "Utan batteri",
  "fakta.simCalculating": "Ber\u00e4knar...",
  "fakta.simCalculate": "Ber\u00e4kna",
  "fakta.factsTitle": "Fakta",
  "fakta.factsProductionIndex": "Produktionsindex (prod/dag)",
  "fakta.factsAvgPriceSold": "Snittpris s\u00e5ld",
  "fakta.factsAvgPricePurchased": "Snittpris k\u00f6pt",
  "fakta.factsAvgPriceOwnUse": "Snittpris eget anv\u00e4ndning",
  "fakta.factsPeakReduction": "Eget anv. reducering avg. f\u00f6rb.",
};

const MONTHS_EN_FULL = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];
const MONTHS_EN_SHORT = [
  "Jan", "Feb", "Mar", "Apr", "May", "Jun",
  "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
];
const MONTHS_SV_FULL = [
  "JANUARI", "FEBRUARI", "MARS", "APRIL", "MAJ", "JUNI",
  "JULI", "AUGUSTI", "SEPTEMBER", "OKTOBER", "NOVEMBER", "DECEMBER",
];
const MONTHS_SV_SHORT = [
  "JAN.", "FEB.", "MAR.", "APR.", "MAJ", "JUN.",
  "JUL.", "AUG.", "SEP.", "OKT.", "NOV.", "DEC.",
];

const dictionaries: Record<string, Record<TranslationKey, string>> = { en, sv };

export function getLang(hass: any): "sv" | "en" {
  const lang = hass?.language || hass?.locale?.language || "en";
  return lang === "sv" ? "sv" : "en";
}

export function getLocale(hass: any): string {
  return getLang(hass) === "sv" ? "sv-SE" : "en-US";
}

export function t(hass: any, key: TranslationKey, ...args: (string | number)[]): string {
  const lang = getLang(hass);
  let str = dictionaries[lang][key] || dictionaries.en[key] || key;
  for (let i = 0; i < args.length; i++) {
    str = str.replace(`{${i}}`, String(args[i]));
  }
  return str;
}

export function monthFull(hass: any, index: number): string {
  return getLang(hass) === "sv" ? MONTHS_SV_FULL[index] : MONTHS_EN_FULL[index];
}

export function monthShort(hass: any, index: number): string {
  return getLang(hass) === "sv" ? MONTHS_SV_SHORT[index] : MONTHS_EN_SHORT[index];
}
