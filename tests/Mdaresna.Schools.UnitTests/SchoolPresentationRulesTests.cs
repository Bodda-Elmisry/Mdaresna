using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.School;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolPresentationRulesTests
{
    [Theory]
    [InlineData("89504E470D0A1A0A00000000000000000000000000000000", ".png", "image/png")]
    [InlineData("FFD8FF000000000000000000", ".jpg", "image/jpeg")]
    [InlineData("52494646000000005745425000000000", ".webp", "image/webp")]
    public void Image_type_is_derived_from_bytes_not_client_mime(string hex, string extension, string mime)
    {
        var value = SchoolPresentationRules.ImageType(Convert.FromHexString(hex));
        Assert.NotNull(value);
        Assert.Equal(extension, value.Value.Extension);
        Assert.Equal(mime, value.Value.ContentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("89504E47")]
    [InlineData("FFD8FF")]
    [InlineData("3C7376673E3C7363726970743E3C2F7363726970743E3C2F7376673E")]
    public void Empty_truncated_and_svg_content_is_rejected(string hex) =>
        Assert.Null(SchoolPresentationRules.ImageType(Convert.FromHexString(hex)));

    [Fact]
    public void Presentation_management_has_a_dedicated_permission() =>
        Assert.Single(SchoolIdentitySeed.Permissions, x => x.Code == "school.profile.manage");
}
