namespace MySolarCells.Messages;
public class RefreshRoiViewMessage : ValueChangedMessage<bool>
{
    public RefreshRoiViewMessage(bool status) : base(status)
    {
    }
}

