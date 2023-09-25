namespace MySolarCells.Helpers;

public class Result<T>
{
    //God Response
    public Result(T model, bool wasSuccessful = false)
    {
        Model = model;
        WasSuccessful = true;
    }

    public Result(bool wasSuccessful)
    {

        WasSuccessful = wasSuccessful;
    }
    //Bad response
    public Result(GenericResponse response, bool wasSuccessful = false)
    {
        ErrorMessage = response != null ? response.Message : "";
        this.GenericResponse = response;
        WasSuccessful = wasSuccessful;
    }
    public Result(string error)
    {
        ErrorMessage = error;
        WasSuccessful = false;
    }
    public Result(string error, string errorType = "")
    {
        ErrorMessage = error;
        
        this.GenericResponse = new GenericResponse { ErrorMessage = error, ErrorCode = errorType };
        WasSuccessful = false;
    }

    public T Model { get; set; }

    public GenericResponse GenericResponse { get; set; }

    public bool WasSuccessful { get; set; }

    public string ErrorMessage { get; set; }
}
public class ResultModelBool
{
    public bool Result { get; set; }
}

