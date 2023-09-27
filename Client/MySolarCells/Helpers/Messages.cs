namespace MySolarCells.Helpers.Messages;

public class AppResumeMessage : ValueChangedMessage<bool>
{
    public AppResumeMessage(bool isrunning) : base(isrunning)
    {
    }
}
public class AppSleepMessage : ValueChangedMessage<bool>
{
    public AppSleepMessage(bool isrunning) : base(isrunning)
    {
    }
}

