using System.Diagnostics.CodeAnalysis;

namespace Core.All;

/// <summary>
///     Represents the result of an operation.
/// </summary>
public readonly struct Result
{
    /// <summary>
    ///     Operation result enum value.
    /// </summary>
    public ResultEnum ResultEnum { get; }

    /// <summary>
    ///     Gets the operation result message.
    /// </summary>
    public readonly string Message;

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => ResultEnum is ResultEnum.Success;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Result" /> struct.
    /// </summary>
    /// <param name="resultEnum">Operation result enum value.</param>
    /// <param name="message">Operation result message.</param>
    public Result(
        ResultEnum resultEnum,
        string message
        )
    {
        ResultEnum = resultEnum;
        Message = message;
    }
}


/// <summary>
///     Represents the result of an operation with a return value.
/// </summary>
/// <typeparam name="T">Type of the result object.</typeparam>
public readonly struct Result<T>
{
    /// <summary>
    ///     Operation result enum value.
    /// </summary>
    private readonly ResultEnum _resultEnum;

    /// <summary>
    ///     Gets the operation result message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the operation result object.
    /// </summary>
    public T? ResultObject { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(returnValue: true, nameof(ResultObject))]
    public bool IsSuccess => _resultEnum is ResultEnum.Success;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{T}" /> struct.
    /// </summary>
    /// <param name="resultEnum">Operation result enum value.</param>
    /// <param name="resultObj">Operation result object.</param>
    /// <param name="message">Operation result message.</param>
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


/// <summary>
///     Defines possible operation result states.
/// </summary>
public enum ResultEnum : byte
{
    /// <summary>
    ///     Successful operation.
    /// </summary>
    Success,
    /// <summary>
    ///     Error while validating hash.
    /// </summary>
    HashError,
    /// <summary>
    ///     Something not found.
    /// </summary>
    NotFound,
    /// <summary>
    ///     Connection to online resource failed.
    /// </summary>
    ConnectionError,
    /// <summary>
    ///     Access to file failed.
    /// </summary>
    FileAccessError,
    /// <summary>
    ///     Task canceled.
    /// </summary>
    Cancelled,
    /// <summary>
    ///     General error.
    /// </summary>
    Error
}
