using System.Text.Json;
using Mdaresna.Api.Contracts;

namespace Mdaresna.Api.Contracts.UnitTests;

public sealed class PagedApiResponseTests
{
    [Fact]
    public void Paged_success_inherits_the_base_response_and_computes_total_pages()
    {
        var response = PagedApiResponse<int>.Success([1, 2], totalCount: 21, pageNumber: 2, pageSize: 10);
        ApiResponse<IReadOnlyList<int>> asBase = response;

        Assert.True(asBase.IsSuccess);
        Assert.Equal([1, 2], asBase.Data);
        Assert.Equal(21, response.TotalCount);
        Assert.Equal(2, response.PageNumber);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response));
        var json = document.RootElement;

        Assert.Equal(200, json.GetProperty("statusCode").GetInt32());
        Assert.True(json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(2, json.GetProperty("data").GetArrayLength());
        Assert.Equal(21, json.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, json.GetProperty("pageNumber").GetInt32());
        Assert.Equal(10, json.GetProperty("pageSize").GetInt32());
        Assert.Equal(3, json.GetProperty("totalPages").GetInt32());
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(20, 10, 2)]
    [InlineData(21, 10, 3)]
    [InlineData(int.MaxValue, 1, int.MaxValue)]
    public void Total_pages_is_correct_without_integer_overflow(int totalCount, int pageSize, int expected)
    {
        var response = PagedApiResponse<int>.Success([], totalCount, 1, pageSize);

        Assert.Equal(expected, response.TotalPages);
    }

    [Fact]
    public void Paged_success_rejects_invalid_pagination()
    {
        Assert.Throws<ArgumentNullException>(() => PagedApiResponse<int>.Success(null!, 0, 1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => PagedApiResponse<int>.Success([], -1, 1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => PagedApiResponse<int>.Success([], 0, 0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => PagedApiResponse<int>.Success([], 0, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => PagedApiResponse<int>.Success([], 0, 1, 10, statusCode: 204));
        Assert.Throws<ArgumentOutOfRangeException>(() => PagedApiResponse<int>.Success([], 0, 1, 10, statusCode: 205));
    }
}
