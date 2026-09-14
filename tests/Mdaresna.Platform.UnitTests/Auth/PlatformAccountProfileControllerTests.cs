using Mdaresna.Platform.Api.Controllers.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.UnitTests.Auth;

public sealed class PlatformAccountProfileControllerTests
{
    [Theory]
    [InlineData("1899-12-31", "male")]
    [InlineData("2100-01-01", "female")]
    [InlineData("2000-01-01", "unsupported")]
    public async Task Invalid_person_details_are_rejected_before_database_access(
        string date, string gender)
    {
        var controller = CreateController();

        var result = await controller.Update(
            new UpdatePersonProfileRequest(DateOnly.Parse(date), gender), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_rejects_disguised_image_without_database_access()
    {
        var controller = CreateController();
        var bytes = "<svg xmlns='http://www.w3.org/2000/svg'/>"u8.ToArray();
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "profile.png")
        {
            Headers = new HeaderDictionary(), ContentType = "image/png"
        };

        var result = await controller.UploadImage(file, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static PlatformAccountProfileController CreateController() => new(null!)
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };
}
