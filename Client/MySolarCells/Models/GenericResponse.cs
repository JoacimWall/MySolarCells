namespace MySolarCells.Models;

public class GenericResponse
{
    #region api properties
    //gamla backend var string, nya int The return Successcode as inte not string generate error in convert to object               
    public int Successcode { get; set; }
    public int Statuscode { get; set; }
    public string ErrorMessage { get; set; }
    public string UserMessage { get; set; }
    public string InternalMessage { get; set; }
    public string Status { get; set; }
    public string Details { get; set; }

    //this is used to know if there was a server error not  a server message like epost format error meessage 
    public bool ServerError { get; set; }
    //public string Errorcode { get; set; }

    //BackendServerErrorResponse
    public string ErrorCode { get; set; }
    public string Description { get; set; }
    //mvc Problem
    public string Type { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public string Instance { get; set; }

    private string _message;
    public string Message
    {
        get
        {
            if (string.IsNullOrEmpty(_message) && !string.IsNullOrEmpty(UserMessage))
                return UserMessage;

            if (string.IsNullOrEmpty(_message) && !string.IsNullOrEmpty(InternalMessage))
                return InternalMessage;

            if (string.IsNullOrEmpty(_message) && !string.IsNullOrEmpty(ErrorMessage))
                return ErrorMessage;

            if (!string.IsNullOrEmpty(_message))
                return "A_Server_Error_Occurred_Please_Try_Again_Later";

            return _message;
        }
        set
        {
            _message = value;

        }

    }
    public GenericResponse()
    {

    }
    #endregion
}
