using System;
namespace MySolarCells.Messages;

public class PickerUpdateDisplayNameMessage : ValueChangedMessage<string>
{
    public PickerUpdateDisplayNameMessage(string status) : base(status)
    {
    }
}
public class RefreshRoiViewMessage : ValueChangedMessage<bool>
{
    public RefreshRoiViewMessage(bool status) : base(status)
    {
    }
}

