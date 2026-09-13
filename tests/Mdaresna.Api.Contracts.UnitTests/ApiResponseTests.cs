using System.Text.Json;
using Mdaresna.Api.Contracts;

namespace Mdaresna.Api.Contracts.UnitTests;

public sealed class ApiResponseTests
{
    [Fact]
    public void Success_has_a_consistent_status_and_serializes_the_shared_shape()
    {
        var response = ApiResponse<SchoolDto>.Success(
            new SchoolDto("SCH-1"),
            statusCode: 201,
            message: "Created",
            correlationId: "request-123");

        Assert.True(response.IsSuccess);
        Assert.Equal(201, response.StatusCode);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var json = document.RootElement;

        Assert.Equal(201, json.GetProperty("statusCode").GetInt32());
        Assert.True(json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Created", json.GetProperty("message").GetString());
        Assert.Equal("SCH-1", json.GetProperty("data").GetProperty("code").GetString());
        Assert.Equal("request-123", json.GetProperty("correlationId").GetString());
        Assert.False(json.TryGetProperty("errors", out _));
        Assert.False(json.TryGetProperty("Code", out _));
    }

    [Fact]
    public void Failure_includes_machine_code_and_field_errors_without_pagination()
    {
        var response = ApiResponse<object?>.Failure(
            400,
            "validation.failed",
            "Invalid input",
            new Dictionary<string, string[]> { ["schoolCode"] = ["required"] },
            "request-456");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response));
        var json = document.RootElement;

        Assert.Equal(400, json.GetProperty("statusCode").GetInt32());
        Assert.False(json.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("validation.failed", json.GetProperty("code").GetString());
        Assert.Equal("Invalid input", json.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("data").ValueKind);
        Assert.Equal("required", json.GetProperty("errors").GetProperty("schoolCode")[0].GetString());
        Assert.Equal("request-456", json.GetProperty("correlationId").GetString());
        Assert.False(json.TryGetProperty("totalCount", out _));
    }

    [Fact]
    public void Failure_requested_from_a_value_type_response_still_serializes_null_data()
    {
        var response = ApiResponse<int>.Failure(404, "school.not_found", "School not found");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }

    [Theory]
    [InlineData(199)]
    [InlineData(204)]
    [InlineData(205)]
    [InlineData(300)]
    [InlineData(400)]
    public void Success_rejects_non_body_success_statuses(int statusCode)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ApiResponse<string>.Success("value", statusCode));
    }

    [Theory]
    [InlineData(200)]
    [InlineData(302)]
    [InlineData(600)]
    public void Failure_rejects_non_error_statuses(int statusCode)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ApiResponse<object?>.Failure(statusCode, "request.failed", "Failed"));
    }

    [Fact]
    public void Failure_requires_stable_code_and_message()
    {
        Assert.Throws<ArgumentException>(() => ApiResponse<object?>.Failure(400, " ", "Failed"));
        Assert.Throws<ArgumentException>(() => ApiResponse<object?>.Failure(400, "request.failed", " "));
    }

    private sealed record SchoolDto(string Code);
}
