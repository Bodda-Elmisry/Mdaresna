using System.Text.Json.Serialization;

namespace Mdaresna.Api.Contracts;

/// <summary>
/// The response envelope shared by new Platform, Schools, and Family APIs.
/// The HTTP response status must always equal <see cref="StatusCode"/>.
/// </summary>
public class ApiResponse<T>
{
    protected ApiResponse(
        int statusCode,
        T? data,
        string? message,
        string? code,
        IReadOnlyDictionary<string, string[]>? errors,
        string? correlationId)
    {
        StatusCode = statusCode;
        Data = data;
        Message = message;
        Code = code;
        Errors = errors;
        CorrelationId = correlationId;
    }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess => StatusCode is >= 200 and <= 299;

    [JsonPropertyName("message")]
    public string? Message { get; }

    [JsonPropertyName("data")]
    public T? Data { get; }

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    [JsonPropertyName("correlationId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; }

    public static ApiResponse<T> Success(
        T data,
        int statusCode = 200,
        string? message = null,
        string? correlationId = null)
    {
        if (statusCode is < 200 or > 299 or 204 or 205)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), "A body-bearing success requires a 2xx status other than 204 or 205.");
        }

        return new ApiResponse<T>(statusCode, data, message, null, null, correlationId);
    }

    /// <summary>
    /// Failures have no payload type. Returning an object response ensures that
    /// even ApiResponse&lt;int&gt;.Failure serializes data as null, not 0.
    /// </summary>
    public static ApiResponse<object?> Failure(
        int statusCode,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null,
        string? correlationId = null)
    {
        if (statusCode is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), "An error response requires a 4xx or 5xx status.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new ApiResponse<object?>(statusCode, null, message, code, errors, correlationId);
    }
}
