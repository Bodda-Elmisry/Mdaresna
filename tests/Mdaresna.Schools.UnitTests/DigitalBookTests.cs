using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Mdaresna.Schools.UnitTests;
public sealed class DigitalBookTests
{
    private sealed class Factory : ISchoolDbContextFactory
    {
        public DbContextOptions<SchoolsDbContext> Options { get; } = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public Task<SchoolLoginTarget?> ResolveAsync(string code, CancellationToken ct = default) => Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string code, CancellationToken ct = default) => Task.FromResult<SchoolsDbContext?>(code == "school" ? new(Options) : null);
    }
    private sealed class Storage : ISchoolDocumentStorage
    {
        public int Reads, Writes;
        public Task<StoredSchoolDocument> SaveAsync(string s, Guid d, Guid v, Stream input, string extension, CancellationToken ct) { Writes++; return Task.FromResult(new StoredSchoolDocument($"{v}.enc", "hash", input.Length)); }
        public Task<Stream?> OpenReadAsync(string key, CancellationToken ct) { Reads++; return Task.FromResult<Stream?>(new MemoryStream("%PDF-fixture"u8.ToArray())); }
        public Task DeleteAsync(string key, CancellationToken ct) => Task.CompletedTask;
    }
    private sealed class Renderer : IBookPdfRenderer
    {
        public Task<int> PageCountAsync(Stream pdf, CancellationToken ct) => Task.FromResult(3);
        public Task<byte[]> RenderAsync(Stream pdf, int page, string watermark, CancellationToken ct) => Task.FromResult("watermarked PNG"u8.ToArray());
    }
    private static SchoolCurriculumController Controller(Factory factory, bool manage = true) => new(factory) { ControllerContext = new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(SchoolClaimTypes.SchoolCode, "school"),new Claim(SchoolClaimTypes.Permission, "school.academics.view"), new Claim(SchoolClaimTypes.Permission, manage ? "school.academics.manage" : "none"),new Claim(JwtRegisteredClaimNames.Sub,Guid.NewGuid().ToString()) }, "test")) } } };
    private static async Task<Guid> Seed(Factory factory)
    {
        await using var db = new SchoolsDbContext(factory.Options); var id = Guid.NewGuid();
        db.BookVersions.Add(new BookVersion { Id = id, BookId = Guid.NewGuid(), EditionCode = "ONE", Book = new Book { Id = Guid.NewGuid(), Code = "BOOK", NameAr = "كتاب", NameEn = "Book" } }); await db.SaveChangesAsync(); return id;
    }
    private static FormFile Pdf() => new(new MemoryStream("%PDF-fixture"u8.ToArray()), 0, 12, "file", "../book.pdf");
    [Fact] public async Task Upload_policy_and_reader_do_not_expose_original_when_download_is_disabled()
    {
        var factory = new Factory(); var id = await Seed(factory); var c = Controller(factory); var storage = new Storage();
        var result = await c.UploadDigitalBook(id,Pdf(),0,true,false,storage,new Renderer(),default);
        Assert.True(result is OkObjectResult, System.Text.Json.JsonSerializer.Serialize((result as ObjectResult)?.Value));
        var upload = Assert.IsType<OkObjectResult>(result);
        var copy = Assert.IsType<ApiResponse<DigitalBookResponse>>(upload.Value).Data!; Assert.Equal("book.pdf", copy.FileName); Assert.True(copy.ShareWithFamily); Assert.False(copy.AllowDownload); Assert.Equal(3,copy.PageCount);
        Assert.IsType<ForbidResult>(await c.DownloadDigitalBook(id,storage,default)); Assert.Equal(0,storage.Reads);
        var page=Assert.IsType<FileContentResult>(await c.DigitalBookPage(id,1,1,storage,new Renderer(),default)); Assert.Equal("image/png",page.ContentType);
        Assert.Equal(1,storage.Reads); Assert.Equal(409,Assert.IsType<ObjectResult>(await c.DigitalBookPolicy(id,new(0,false,true),default)).StatusCode);
        Assert.IsType<OkObjectResult>(await c.DigitalBookPolicy(id,new(1,false,true),default)); var download=Assert.IsType<FileStreamResult>(await c.DownloadDigitalBook(id,storage,default)); await download.FileStream.DisposeAsync();
        Assert.Equal(409,Assert.IsType<ObjectResult>(await c.DigitalBookPage(id,1,1,storage,new Renderer(),default)).StatusCode);
    }
    [Fact] public async Task Read_only_permissions_and_invalid_files_cannot_upload_or_change_policy()
    {
        var f = new Factory(); var id=await Seed(f);var c=Controller(f,false);var storage=new Storage();
        Assert.IsType<ForbidResult>(await c.UploadDigitalBook(id,Pdf(),0,false,false,storage,new Renderer(),default)); Assert.IsType<ForbidResult>(await c.DigitalBookPolicy(id,new(0,true,true),default)); Assert.Equal(0,storage.Writes);
        var admin=Controller(f); var bad=new FormFile(new MemoryStream("not-pdf!"u8.ToArray()),0,8,"file","fake.pdf");Assert.Equal(400,Assert.IsType<ObjectResult>(await admin.UploadDigitalBook(id,bad,0,false,false,storage,new Renderer(),default)).StatusCode);
    }
    [Fact] public async Task Disabled_editions_and_out_of_range_pages_are_not_readable()
    {
        var f=new Factory();var id=await Seed(f);var c=Controller(f);var storage=new Storage();await c.UploadDigitalBook(id,Pdf(),0,false,false,storage,new Renderer(),default);
        Assert.IsType<NotFoundObjectResult>(await c.DigitalBookPage(id,4,1,storage,new Renderer(),default));
        await using(var db=new SchoolsDbContext(f.Options)){(await db.BookVersions.FindAsync(id))!.IsActive=false;await db.SaveChangesAsync();}
        Assert.IsType<NotFoundObjectResult>(await c.DigitalBook(id,default));Assert.IsType<NotFoundObjectResult>(await c.DownloadDigitalBook(id,storage,default));Assert.Equal(0,storage.Reads);
    }
}

public sealed class NativePdfFactAttribute : FactAttribute
{
    public NativePdfFactAttribute() { if (Environment.GetEnvironmentVariable("MDARESNA_TEST_POPPLER") != "1") Skip = "Opt-in renderer integration requires Poppler on PATH."; }
}
public sealed class NativeBookRendererTests
{
    [NativePdfFact] public async Task Real_pdf_is_counted_and_rendered_to_watermarked_png_without_plaintext_temp_files()
    {
        using var pdf=new MemoryStream(); var offsets=new List<long>(); void Write(string value)=>pdf.Write(Encoding.ASCII.GetBytes(value));
        Write("%PDF-1.4\n");
        var objects=new[]{"<< /Type /Catalog /Pages 2 0 R >>","<< /Type /Pages /Kids [3 0 R] /Count 1 >>","<< /Type /Page /Parent 2 0 R /MediaBox [0 0 300 200] /Contents 4 0 R >>","<< /Length 0 >>\nstream\n\nendstream"};
        for(var i=0;i<objects.Length;i++){offsets.Add(pdf.Position);Write($"{i+1} 0 obj\n{objects[i]}\nendobj\n");}
        var xref=pdf.Position;Write("xref\n0 5\n0000000000 65535 f \n");foreach(var offset in offsets)Write($"{offset:0000000000} 00000 n \n");Write($"trailer\n<< /Size 5 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");pdf.Position=0;
        var renderer=new BookPdfRenderer(new ConfigurationBuilder().Build());Assert.Equal(1,await renderer.PageCountAsync(pdf,default));pdf.Position=0;
        var bytes=await renderer.RenderAsync(pdf,1,"SCHOOL USER-01",default);Assert.Equal(new byte[]{137,80,78,71,13,10,26,10},bytes[..8]);Assert.True(bytes.Length>100);
        if (Environment.GetEnvironmentVariable("MDARESNA_TEST_ARTIFACT_DIR") is { Length: > 0 } directory) { Directory.CreateDirectory(directory); await File.WriteAllBytesAsync(Path.Combine(directory,"native-book-page.png"),bytes); }
    }
}
