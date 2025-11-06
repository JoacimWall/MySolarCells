// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace MySolarCells.Services.Inverter.Models;
#nullable disable


public class SolarEdgeEnergyDetails
{
    public string timeUnit { get; set; } = "";
    public string unit { get; set; } = "";
    public List<SolarEdgeEnergyMeter> meters { get; set; } = new();
}

public class SolarEdgeEnergyMeter
{
    public string type { get; set; } = "";
    public List<SolarEdgeEnergyValue> values { get; set; } = new();
}

public class SolarEdgeEnegyDetialsResponse
{
    public SolarEdgeEnergyDetails energyDetails { get; set; } = new();
}

public class SolarEdgeEnergyValue
{
    public string date { get; set; } = "";
    public double? value { get; set; }
}

public class SolarEdgeBattery
{
    public double nameplate { get; set; }
    public string serialNumber { get; set; } = "";
    public string modelNumber { get; set; } = "";
    public int telemetryCount { get; set; }
    public List<SolarEdgeTelemetry> telemetries { get; set; } = new();
}

public class SolarEdgeStorageDataResponse
{
    public SolarEdgeStorageData storageData { get; set; } = new();
}

public class SolarEdgeStorageData
{
    public int batteryCount { get; set; }
    public List<SolarEdgeBattery> batteries { get; set; } = new();
}

public class SolarEdgeTelemetry
{
    public string timeStamp { get; set; } = "";
    public double? power { get; set; }
    public int batteryState { get; set; }
    public int lifeTimeEnergyDischarged { get; set; }
    public int lifeTimeEnergyCharged { get; set; }
    public double batteryPercentageState { get; set; }
    public double fullPackEnergyAvailable { get; set; }
    public double internalTemp { get; set; }
    public double? ACGridCharging { get; set; }
}

public class SolarEdgeSumHour
{
    public DateTime TimeStamp { get; set; }
    public double SelfConsumption { get; set; }
    public double FeedIn { get; set; }
    public double Purchased { get; set; }
    public double batteryCharge { get; set; }
    public double batteryOutput { get; set; }

}

//Response
public class SolarEdgeSiteListResponse
{
    public SolarEdgeSites sites { get; set; } = new();

}

public class SolarEdgeSite
{
    public int id { get; set; }
    public string name { get; set; } = "";
    public int accountId { get; set; }
    public string status { get; set; } = "";
    public double peakPower { get; set; }
    public string lastUpdateTime { get; set; } = "";
    public string installationDate { get; set; } = "";
    public object ptoDate { get; set; } = new();
    public string notes { get; set; } = "";
    public string type { get; set; } = "";
    public SolarEdgePrimaryModule primaryModule { get; set; } = new();

}

public class SolarEdgeSites
{
    public int count { get; set; }
    public List<SolarEdgeSite> site { get; set; } = new();
}

public class SolarEdgePrimaryModule
{
    public string manufacturerName { get; set; } = "";
    public string modelName { get; set; } = "";
    public double maximumPower { get; set; }
    public double temperatureCoef { get; set; }
}

public class SolarEdgeMeter
{
    public string type { get; set; } = "";
    public List<SolarEdgeValue> values { get; set; } = new();
}

public class SolarEdgePowerDetails
{
    public string timeUnit { get; set; } = "";
    public string unit { get; set; } = "";
    public List<SolarEdgeMeter> meters { get; set; } = new();
}

public class SolarEdgePowerDetailsResults
{
    public SolarEdgePowerDetails powerDetails { get; set; } = new();
}

public class SolarEdgeValue
{
    public string date { get; set; } = "";
    public double value { get; set; }
}