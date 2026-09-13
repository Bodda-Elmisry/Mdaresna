using System.Text.Json.Serialization;

namespace Mdaresna.Api.Contracts;

public sealed class PagedApiResponse<T> : ApiResponse<IReadOnlyList<T>>
{
    private PagedApiResponse(
        IReadOnlyList<T> data,
        int totalCount,
        int pageNumber,
        int pageSize,
        int statusCode,
        string? message,
        string? correlationId)
        : base(statusCode, data, message, null, null, correlationId)
    {
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (totalCount / pageSize) + (totalCount % pageSize == 0 ? 0 : 1);
    }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; }

    public static PagedApiResponse<T> Success(
        IReadOnlyList<T> data,
        int totalCount,
        int pageNumber,
        int pageSize,
        int statusCode = 200,
        string? message = null,
        string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (statusCode is < 200 or > 299 or 204 or 205)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), "A paged response requires a body-bearing 2xx status.");
        }

        return new PagedApiResponse<T>(data, totalCount, pageNumber, pageSize, statusCode, message, correlationId);
    }
}
