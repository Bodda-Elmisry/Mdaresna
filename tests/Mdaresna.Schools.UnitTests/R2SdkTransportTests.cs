using Amazon.Runtime;
using Amazon.S3;
using Mdaresna.Schools.Api.Documents;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class R2SdkTransportTests
{
    [Fact]
    public async Task Real_sdk_serializes_conditional_put_head_range_and_delete_without_live_credentials()
    {
        using var handler = new S3Handler();
        using var client = new AmazonS3Client(new BasicAWSCredentials("fixture", "fixture"), new AmazonS3Config {
            ServiceURL = "https://fixture.r2.cloudflarestorage.com", AuthenticationRegion = "auto", ForcePathStyle = true,
            HttpClientFactory = new StubClientFactory(handler), MaxErrorRetry = 0 });
        var settings = new R2StorageSettings { Bucket = "schools-test", Endpoint = "https://fixture.r2.cloudflarestorage.com", AccessKeyId = "fixture", SecretAccessKey = "fixture" };
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["UploadEncryption:ActiveKeyId"] = "fixture", ["UploadEncryption:Keys:fixture"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }).Build();
        using var storage = new R2SchoolDocumentStorage(client, settings, config);
        var bytes = RandomNumberGenerator.GetBytes(600000);
        var id = Guid.NewGuid(); var version = Guid.NewGuid();
        var saved = await storage.SaveAsync("school", id, version, new MemoryStream(bytes), ".pdf", default);
        Assert.NotEqual(bytes, handler.Bytes);
        Assert.True(handler.Signed); Assert.True(handler.CreateOnly);
        using (var read = (await storage.OpenReadAsync(saved.StorageKey, default))!)
        {
            read.Position = 530000; var part = new byte[100]; await read.ReadExactlyAsync(part);
            Assert.Equal(bytes[530000..530100], part);
            read.Position = 0; using var all = new MemoryStream(); await read.CopyToAsync(all);
            Assert.Equal(bytes, all.ToArray());
        }
        Assert.True(handler.ConditionalRange);
        await Assert.ThrowsAsync<AmazonS3Exception>(() => storage.SaveAsync("school", id, version, new MemoryStream(bytes), ".pdf", default));
        await storage.CheckReadinessAsync(default);
        await storage.DeleteAsync(saved.StorageKey, default);
        Assert.Null(await storage.OpenReadAsync(saved.StorageKey, default));
    }

    private sealed class StubClientFactory(HttpMessageHandler handler) : Amazon.Runtime.HttpClientFactory
    {
        public override HttpClient CreateHttpClient(IClientConfig clientConfig) => new(handler, disposeHandler: false);
    }

    private sealed class S3Handler : HttpMessageHandler
    {
        public byte[]? Bytes { get; private set; }
        public bool Signed { get; private set; }
        public bool CreateOnly { get; private set; }
        public bool ConditionalRange { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Signed |= request.Headers.Contains("Authorization");
            if (request.Method == HttpMethod.Put)
            {
                CreateOnly = request.Headers.IfNoneMatch.Any(tag => tag.Tag == "*");
                if (Bytes != null) return new(HttpStatusCode.PreconditionFailed) { Content = new StringContent("<Error><Code>PreconditionFailed</Code></Error>") };
                Bytes = await request.Content!.ReadAsByteArrayAsync(ct);
                return Response(HttpStatusCode.OK, []);
            }
            if (request.Method == HttpMethod.Head)
            {
                if (Bytes == null) return Response(HttpStatusCode.NotFound, []);
                var head = Response(HttpStatusCode.OK, []); head.Content.Headers.ContentLength = Bytes.Length; return head;
            }
            if (request.Method == HttpMethod.Delete) { Bytes = null; return Response(HttpStatusCode.NoContent, []); }
            if (request.RequestUri!.Query.Contains("list-type"))
                return new(HttpStatusCode.OK) { Content = new StringContent("<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"><Name>schools-test</Name><KeyCount>0</KeyCount><MaxKeys>1</MaxKeys><IsTruncated>false</IsTruncated></ListBucketResult>") };
            var range = request.Headers.Range!.Ranges.Single();
            ConditionalRange = request.Headers.IfMatch.Any(tag => tag.Tag == "\"revision\"");
            var from = (int)range.From!; var to = (int)range.To!;
            var result = Response(HttpStatusCode.PartialContent, Bytes![from..(to + 1)]);
            result.Content.Headers.ContentRange = new ContentRangeHeaderValue(from, to, Bytes.Length);
            return result;
        }
        private static HttpResponseMessage Response(HttpStatusCode status, byte[] body)
        {
            var response = new HttpResponseMessage(status) { Content = new ByteArrayContent(body) };
            response.Headers.ETag = new EntityTagHeaderValue("\"revision\""); return response;
        }
    }
}
