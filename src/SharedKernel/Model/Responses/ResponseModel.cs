namespace SharedKernel.Model.Responses;

public class ResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = default!;
    public string ResponseCode { get; set; } = default!;

    public static ResponseModel Success(string message = "")
    {
        return new ResponseModel()
        {
            IsSuccess = true,
            Message = message,
            ResponseCode = ResponseStatusCode.Successful.ResponseCode
        };
    }

    public static ResponseModel Failure(string message = "", string responseCode = "")
    {
        return new ResponseModel()
        {
            IsSuccess = false,
            Message = message,
            ResponseCode = string.IsNullOrEmpty(responseCode) ? ResponseStatusCode.Failed.ResponseCode : responseCode
        };
    }

    public static ResponseModel ValidationError(string message = "")
    {
        return new ResponseModel()
        {
            IsSuccess = false,
            Message = message ?? "One or more validation error occurred",
            ResponseCode = ResponseStatusCode.ValidationError.ResponseCode
        };
    }
}

public class ResponseModel<T> : ResponseModel
{
    public T? Data { get; set; }

    public static ResponseModel<T> Success(T data, string message = "")
    {
        return new ResponseModel<T>()
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            ResponseCode = ResponseStatusCode.Successful.ResponseCode
        };
    }

    public static ResponseModel<T> Failure(T data, string message = "", string responseCode = "")
    {
        return new ResponseModel<T>()
        {
            IsSuccess = false,
            Data = data,
            Message = message,
            ResponseCode = string.IsNullOrEmpty(responseCode) ? ResponseStatusCode.Failed.ResponseCode : responseCode
        };
    }

    public static new ResponseModel<T> Failure(string message, string responseCode = "")
    {
        return new ResponseModel<T>()
        {
            IsSuccess = false,
            Data = default,
            Message = message,
            ResponseCode = string.IsNullOrEmpty(responseCode) ? ResponseStatusCode.Failed.ResponseCode : responseCode
        };
    }
}