namespace MySolarCells;
public static class MySolarCellsGlobals
{
    public static DateTime LastOnSleep { get; set; }
    public static DateTime LastOnStart { get; set; }
    public static DateTime LastOnResume { get; set; }
    public static bool ImportErrorValidateEvrryRow { get; set; }
    public static Application? App { get; set; }
    public static ApplicationState ApplicationState { get; set; }

}
public enum ApplicationState
{
    NotSet,
    Active,
    InActive,
}

