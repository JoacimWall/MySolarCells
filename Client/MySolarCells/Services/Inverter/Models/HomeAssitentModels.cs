// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace MySolarCells.Services.Inverter.Models;
#nullable disable
#region Requests and Responses models
//Response
class AuthMessage
{
    public string type { get; set; } = "";
    public string access_token { get; set; } = "";
}
class GetPrefsRequest
{
    public int id { get; set; }
    public string type { get; set; } = "";
}
//used to filter incoming messages
class GenericFilterResponse
{
    public int id { get; set; }
    public string type { get; set; } = "";
    public string ha_version { get; set; } = "";
}



class EnergyPrefsResult
{
    public int id { get; set; }
    public string type { get; set; } = "";
    public bool success { get; set; }
    public EnergyPrefs result { get; set; } = new();
}
class EnergyPrefs
{
    public List<EnergySource> energy_sources { get; set; } = new();
    //public List<DeviceConsumption> device_consumption { get; set; }
}

class EnergySource
{
    public string type { get; set; } = "";
    //public List<FlowFrom> flow_from { get; set; }
    //public List<FlowTo> flow_to { get; set; }
    public double cost_adjustment_day { get; set; }
    public string stat_energy_from { get; set; } = "";
    public object config_entry_solar_forecast { get; set; } = new();
    public string stat_cost { get; set; } = "";
    public object entity_energy_price { get; set; } = new();
    public object number_energy_price { get; set; } = new();
}
//public class DeviceConsumption
//{
//    public string stat_consumption { get; set; }
//}



//public class FlowFrom
//{
//    public string stat_energy_from { get; set; }
//    public object entity_energy_price { get; set; }
//    public object number_energy_price { get; set; }
//    public string stat_cost { get; set; }
//}

//public class FlowTo
//{
//    public string stat_energy_to { get; set; }
//    public string stat_compensation { get; set; }
//    public object entity_energy_price { get; set; }
//    public object number_energy_price { get; set; }
//}
class EnergyProductionRequest
{
    public int id { get; set; }
    public string type { get; set; } = "";
    public string start_time { get; set; } = "";
    public string end_time { get; set; } = "";
    public List<string> energy_statistic_ids { get; set; } = new();
    public string period { get; set; } = "";
    public string co2_statistic_id { get; set; } = "";
}
class EnergyProductionResult
{
    public int id { get; set; }
    public string type { get; set; } = "";
    public bool success { get; set; }
    //public List<object> result { get; set; }
    public Dictionary<string, double> result { get; set; } = new();

}
#endregion