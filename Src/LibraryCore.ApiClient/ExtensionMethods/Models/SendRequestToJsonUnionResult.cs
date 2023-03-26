using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.ApiClient.ExtensionMethods.Models;

public class SendRequestToJsonUnionResult<T1Ok, T2BadRequest>
{
    private SendRequestToJsonUnionResult(bool isSuccessful, T1Ok? okResult, T2BadRequest? badRequest)
    {
        IsSuccessful = isSuccessful;
        OkResult = okResult;
        BadRequest = badRequest;
    }

    public bool IsSuccessful { get; }
    private T1Ok? OkResult { get; }
    private T2BadRequest? BadRequest { get; }

    public static SendRequestToJsonUnionResult<T1Ok, T2BadRequest> CreateSuccess(T1Ok? okResult) =>
        new(true, okResult, default);

    public static SendRequestToJsonUnionResult<T1Ok, T2BadRequest> CreateBadRequest(T2BadRequest? badRequest) =>
        new(false, default, badRequest);

    /// <summary>
    /// Returns true is the call is 200 ok
    /// </summary>
    public bool TryGetIsSuccessful([NotNullWhen(true)] out T1Ok? result)
    {
        result = OkResult;
        return IsSuccessful;
    }

    /// <summary>
    /// Returns true is the call is 400 bad request
    /// </summary>
    public bool TryGetIsBadRequest([NotNullWhen(true)] out T2BadRequest? result)
    {
        result = BadRequest;
        return !IsSuccessful;
    }
}
