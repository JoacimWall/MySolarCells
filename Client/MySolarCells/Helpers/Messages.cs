namespace MySolarCells.Helpers;

public class AppSleepMessage : ValueChangedMessage<bool>
{
    public AppSleepMessage(bool isRunning) : base(isRunning)
    {
    }
}

