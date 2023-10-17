using System;
namespace MySolarCells.Messages;

public class PickerUpdateDisplayNameMessage : ValueChangedMessage<string>
{
    public PickerUpdateDisplayNameMessage(string status) : base(status)
    {
    }
}

