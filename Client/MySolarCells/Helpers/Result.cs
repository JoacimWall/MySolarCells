namespace MySolarCells.Helpers;

public class Result<T>
{
    //God Response
    public Result(T model, bool wasSuccessful = true)
    {
        Model = model;
        WasSuccessful = wasSuccessful;
    }

    public Result(bool wasSuccessful)
    {
        WasSuccessful = wasSuccessful;
    }

    //Bad response
    public Result(GenericResponse? response, bool wasSuccessful = false)
    {
        ErrorMessage = response != null ? response.Message : "";
        WasSuccessful = wasSuccessful;
    }

    public Result(string error)
    {
        ErrorMessage = error;
        WasSuccessful = false;
    }

    public Result(string error, ErrorCode errorCode)
    {
        ErrorMessage = error;
        ErrorCode = errorCode;
        WasSuccessful = false;
    }
    public Result(string error, string errorType = "")
    {
        ErrorMessage = error;

        WasSuccessful = false;
    }

    public T? Model { get; set; }

    public bool WasSuccessful { get; set; }

    public string ErrorMessage { get; set; } = "";

    public ErrorCode ErrorCode { get; set; }
}

public class ResultModelBool
{
    public bool Result { get; set; }
}
public class GenericResponse
{
    #region api properties
    public string ErrorMessage { get; set; } = "";
    public string UserMessage { get; set; } = "";
    public string InternalMessage { get; set; } = "";
    public string Status { get; set; } = "";
    public string Details { get; set; } = "";

    //this is used to know if there was a server error not a server message like email format error message
    public bool ServerError { get; set; }


    //BackendServerErrorResponse
    public string ErrorCode { get; set; } = "";

    public string Description { get; set; } = "";

    //mvc Problem
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Instance { get; set; } = "";

    private string? message;

    public string Message
    {
        get
        {
            if (string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(UserMessage))
                return UserMessage;

            if (string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(InternalMessage))
                return InternalMessage;

            if (string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(ErrorMessage))
                return ErrorMessage;

            if (!string.IsNullOrEmpty(message))
                return
                    "A server error occurred please try again later";

            return message ?? "";
        }
        set => message = value;
    }

    #endregion
}

public enum ErrorCode
{
    Unknown = 0,
    NoEnergyEntryOnCurrentDate = 100
}