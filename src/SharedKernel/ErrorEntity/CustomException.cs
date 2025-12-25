namespace SharedKernel.ErrorEntity;

public class CustomException : Exception
{
    public CustomException()
        : base()
    {
    }

    public CustomException(string message)
        : base(message)
    {

    }

    public CustomException(string message, string responseCode)
       : base(message)
    {
        Data.Add("ResponseCode", responseCode);
    }

    public CustomException(string message, string responseCode, string entityId)
      : base(message)
    {
        Data.Add("ResponseCode", responseCode);
        Data.Add("EntityId", entityId);
    }

    public CustomException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public CustomException(string name, object key) : base($"{name}")
    {
    }

    public CustomException(string message, Exception innerException, string responseCode)
        : base(message, innerException)
    {
        Data.Add("ResponseCode", responseCode);
    }

    public bool Status { get; set; }
    public string? ResponseCode { get; set; }
}

public class CustomResponseException<T> : Exception
{
    public CustomResponseException()
        : base()
    {
    }

    public CustomResponseException(string message)
        : base(message)
    {
    }

    public CustomResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public bool Status { get; set; }
    public string? ResponseCode { get; set; }

    public T? Response { get; set; }
    public string? Description { get; set; }


    public CustomResponseException(bool status, string responseCode, string message, T? data) : base(message)

    {
        Status = status;
        ResponseCode = responseCode;
        Response = data;
        Description = message;
        Data.Add("ResponseCode", responseCode);
        Data.Add("Response", data);
        Data.Add("Message", message);

    }
    
    public static CustomResponseException<T> PartialSuccess(bool status, string responseCode, T? data, string message)
    {
        return new CustomResponseException<T>(status, responseCode, message, data);
    }

}