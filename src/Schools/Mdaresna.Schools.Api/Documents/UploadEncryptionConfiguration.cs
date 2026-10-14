using Mdaresna.SharedKernel;
namespace Mdaresna.Schools.Api.Documents;
public static class UploadEncryptionConfiguration
{
    public static UploadedFileCipher Create(IConfiguration config) => new(config["UploadEncryption:ActiveKeyId"],
        config.GetSection("UploadEncryption:Keys").GetChildren().ToDictionary(x => x.Key, x => x.Value ?? ""));
}
