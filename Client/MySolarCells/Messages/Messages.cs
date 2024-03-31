namespace MySolarCells.Messages;
public class RefreshRoiViewMessage(bool status) : ValueChangedMessage<bool>(status);
public class AppSleepMessage(bool isRunning) : ValueChangedMessage<bool>(isRunning);
