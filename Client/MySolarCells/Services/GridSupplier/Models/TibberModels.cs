namespace MySolarCells.Services.GridSupplier.Models;
#nullable disable
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
#region Requests and Responses models


//Requests
public class GraphQlRequestTibber
{
    //public string operationName { get; set; }
    public Object variables { get; set; } = new();
    public string query { get; set; } = "";
}

public class TibberConsumptionProductionRequest
{
    public string homeid { get; set; } = "";
    public string from { get; set; } = "";
    public int first { get; set; }
}

//Responses
public class TibberResponse
{
    public TibberData data { get; set; } = new TibberData();
}

public class TibberData
{
    public TibberViewer viewer { get; set; } = new TibberViewer();
}

public class TibberViewer
{
    public TibberHome home { get; set; } = new();
    public List<string> accountType { get; set; } = new();
    public List<TibberHome> homes { get; set; } = new();
}

public class TibberAddress
{
    public string address1 { get; set; } = "";
    public object address2 { get; set; } = new();
    public object address3 { get; set; } = new();
    public string postalCode { get; set; } = "";
    public string city { get; set; } = "";
    public string country { get; set; } = "";
    public string latitude { get; set; } = "";
    public string longitude { get; set; } = "";
}

public class TibberHome
{
    public string id { get; set; } = "";
    public TibberConsumption consumption { get; set; } = new();
    public TibberProduction production { get; set; } = new();
    public TibberAddress address { get; set; } = new();
    public TibberCurrentSubscription currentSubscription { get; set; } = new();
}

public class TibberCurrentSubscription
{
    public TibberPriceInfo priceInfo { get; set; } = new();
}

public class TibberPriceInfo
{
    public List<TibberPrice> today { get; set; } = new();
    public List<TibberPrice> tomorrow { get; set; } = new();
}

public class TibberPrice
{
    public double total { get; set; }
    public double energy { get; set; }
    public double tax { get; set; }
    public string level { get; set; } = "";
    public DateTime startsAt { get; set; }
    public string currency { get; set; } = "";
}

public class TibberConsumption
{
    public List<TibberConsumptionNode> nodes { get; set; } = new();
}

public class TibberProduction
{
    public List<TibberProductionNode> nodes { get; set; } = new();
}

public class TibberConsumptionNode
{
    public DateTime? from { get; set; }
    public DateTime? to { get; set; }
    public decimal? cost { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? unitPriceVAT { get; set; }
    public decimal? consumption { get; set; }
    public string consumptionUnit { get; set; } = "";
    public string currency { get; set; } = "";
}

public class TibberProductionNode
{
    public DateTime? from { get; set; }
    public DateTime? to { get; set; }
    public decimal? profit { get; set; }
    public decimal? unitPrice { get; set; }
    public decimal? unitPriceVAT { get; set; }
    public decimal? production { get; set; }
    public string productionUnit { get; set; } = "";
    public string currency { get; set; } = "";
}
#endregion