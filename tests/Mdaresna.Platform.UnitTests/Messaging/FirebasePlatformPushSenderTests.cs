using Mdaresna.Platform.Infrastructure.Messaging;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class FirebasePlatformPushSenderTests
{
    [Theory]
    [InlineData("/staff")]
    [InlineData("/dashboard")]
    [InlineData("http://localhost:8082/staff")]
    public void Relative_or_non_https_action_url_is_kept_out_of_webpush_link(string actionUrl)
    {
        var data = new Dictionary<string, string> { ["actionUrl"] = actionUrl };

        var webpush = FirebasePlatformPushSender.CreateWebpushConfig(data);

        Assert.Null(webpush);
        Assert.Equal(actionUrl, data["actionUrl"]);
    }

    [Fact]
    public void Absolute_https_action_url_can_be_used_as_webpush_link()
    {
        var webpush = FirebasePlatformPushSender.CreateWebpushConfig(
            new Dictionary<string, string>
            {
                ["actionUrl"] = "https://platform.example.com/staff"
            });

        Assert.Equal("https://platform.example.com/staff", webpush?.FcmOptions?.Link);
    }
}
