using System.Diagnostics.CodeAnalysis;

namespace Core.All;

/// <summary>
/// Operation result
/// </summary>
public readonly struct Result
{
    /// <summary>
    /// Operation result enum
    /// </summary>
    private readonly ResultEnum _resultEnum;

    /// <summary>
    /// Operation result message
    /// </summary>
    public readonly string Message;

    /// <summary>
    /// Is operation successful
    /// </summary>
    public bool IsSuccess => _resultEnum is ResultEnum.Success;


    public Result(
        ResultEnum resultEnum,
        string message
        )
    {
        _resultEnum = resultEnum;
        Message = message;
    }
}


/// <summary>
/// Operation result with return object
/// </summary>
public readonly struct Result<T>
{
    /// <summary>
    /// Operation result enum
    /// </summary>
    private ResultEnum _resultEnum { get; }

    /// <summary>
    /// Operation result message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Operation result object
    /// </summary>
    public T? ResultObject { get; }

    /// <summary>
    /// Is operation successful
    /// </summary>
    [MemberNotNullWhen(returnValue: true, nameof(ResultObject))]
    public bool IsSuccess => _resultEnum is ResultEnum.Success;


    public Result(
        ResultEnum resultEnum,
        T? resultObj,
        string message
        )
    {
        _resultEnum = resultEnum;
        Message = message;
        ResultObject = resultObj;
    }
}


public enum ResultEnum : byte
{
    /// <summary>
    /// Successful operation
    /// </summary>
    Success,
    /// <summary>
    /// Error while validating hash
    /// </summary>
    HashError,
    /// <summary>
    /// Something not found
    /// </summary>
    NotFound,
    /// <summary>
    /// Connection to online resource failed
    /// </summary>
    ConnectionError,
    /// <summary>
    /// Access to file failed
    /// </summary>
    FileAccessError,
    /// <summary>
    /// Task canceled
    /// </summary>
    Cancelled,
    /// <summary>
    /// General error
    /// </summary>
    Error
}
