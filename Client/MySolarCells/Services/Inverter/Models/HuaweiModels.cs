#nullable disable
// // ReSharper disable InconsistentNaming
// // ReSharper disable CollectionNeverUpdated.Global
// // ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace MySolarCells.Services.Inverter.Models;
public class HuaweiLoginResponse
{
    public object data { get; set; } = new();
    public bool success { get; set; }
    public int failCode { get; set; }
    public object message { get; set; } = new();
}

public class HuaweiSiteData
{
    public List<HuaweiSiteList> list { get; set; } = new();
    public int pageCount { get; set; }
    public int pageNo { get; set; }
    public int pageSize { get; set; }
    public int total { get; set; }
}

public class HuaweiSiteList
{
    public double capacity { get; set; }
    public string contactMethod { get; set; } = "";
    public string contactPerson { get; set; } = "";
    public DateTime gridConnectionDate { get; set; }
    public string latitude { get; set; } = "";
    public string longitude { get; set; } = "";
    public string plantAddress { get; set; } = "";
    public string plantCode { get; set; } = "";
    public string plantName { get; set; } = "";
}

public class HuaweiSiteResponse
{
    public HuaweiSiteData data { get; set; } = new();
    public int failCode { get; set; }
    public string message { get; set; } = "";
    public bool success { get; set; }
}

public class HuaweiDevicesData
{
    public string devName { get; set; } = "";
    public int devTypeId { get; set; }
    public string esnCode { get; set; } = "";
    public object id { get; set; } = new();
    public string invType { get; set; } = "";
    public double latitude { get; set; }
    public double longitude { get; set; }
    public int? optimizerNumber { get; set; }
    public string softwareVersion { get; set; } = "";
    public string stationCode { get; set; } = "";
}

public class HuaweiDevicesParams
{
    public long currentTime { get; set; }
    public string stationCodes { get; set; } = "";
}

public class HuaweiDevicesResponse
{
    public List<HuaweiDevicesData> data { get; set; } = new();
    public int failCode { get; set; }
    public object message { get; set; } = new();
    //public HuaweiDevicesParams params { get; set; }
    public bool success { get; set; }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class HuaweiProductioItemMap
{
    public double? radiation_intensity { get; set; }
    public double? inverter_power { get; set; }
    public double? power_profit { get; set; }
    public double? theory_power { get; set; }
    public double? ongrid_power { get; set; }
}

public class HuaweiProductionData
{
    public long? collectTime { get; set; }
    public string stationCode { get; set; } = "";
    public HuaweiProductioItemMap dataItemMap { get; set; } = new();
}

//public class Params
//{
//    public long currentTime { get; set; }
//    public long collectTime { get; set; }
//    public string stationCodes { get; set; }
//}

public class HuaweiProductionResponse
{
    [JsonPropertyName("data")]
    [JsonConverter(typeof(SingleOrArrayConverter<HuaweiProductionData>))]
    public List<HuaweiProductionData> data { get; set; } = new();
    public int failCode { get; set; }
    public object message { get; set; } = new();
    //public Params @params { get; set; }
    public bool success { get; set; }
}

////Request
public class HuaweiLoginRequest
{
    public string userName { get; set; } = "";
    public string systemCode { get; set; } = "";

}
public class SiteListRequest
{
    public int pageNo { get; set; }
    public int pageSize { get; set; }

}
public class HuaweiDevListRequest
{
    public string stationCodes { get; set; } = "";


}
public class HuaweiProductioRequest
{
    public string stationCodes { get; set; } = "";
    public long collectTime { get; set; }

}


public class HuaweiPoint
{
    public DateTime timestamp { get; set; }
    public double? value { get; set; }
    public string __typename { get; set; } = "";
}
