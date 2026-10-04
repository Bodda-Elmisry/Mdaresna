using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Social;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolSocialTests
{
    private sealed class Factory : ISchoolDbContextFactory
    {
        private readonly DbContextOptions<SchoolsDbContext> options = new DbContextOptionsBuilder<SchoolsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        public Task<SchoolLoginTarget?> ResolveAsync(string code, CancellationToken ct = default) => Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string code, CancellationToken ct = default) => Task.FromResult<SchoolsDbContext?>(code == "test-school" ? new SchoolsDbContext(options) : null);
        public SchoolsDbContext Open() => new(options);
    }
    private sealed class Storage : ISchoolDocumentStorage
    {
        public int Reads;
        public Task<StoredSchoolDocument> SaveAsync(string s, Guid d, Guid v, Stream content, string e, CancellationToken ct) => throw new NotSupportedException();
        public Task<Stream?> OpenReadAsync(string key, CancellationToken ct) { Reads++; return Task.FromResult<Stream?>(new MemoryStream([1, 2])); }
    }
    private static SchoolSocialController Controller(Factory f, Guid actor, params string[] permissions)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, actor.ToString()), new Claim(SchoolClaimTypes.SchoolCode, "test-school") }
            .Concat(permissions.Select(p => new Claim(SchoolClaimTypes.Permission, "school.social." + p)));
        return new SchoolSocialController(f, new Storage()) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } } };
    }
    private static int Status(IActionResult r) => r is ObjectResult o ? o.StatusCode ?? 200 : r is ForbidResult ? 403 : r is UnauthorizedResult ? 401 : 0;
    private static async Task Seed(Factory f, SocialPost p)
    { await using var db = f.Open(); db.SocialPosts.Add(p); db.SocialSettings.Add(new SocialSettings()); await db.SaveChangesAsync(); }

    [Theory]
    [InlineData(SocialPostStatus.Draft, false)] [InlineData(SocialPostStatus.PendingReview, false)]
    [InlineData(SocialPostStatus.Published, true)] [InlineData(SocialPostStatus.Hidden, false)] [InlineData(SocialPostStatus.Deleted, false)]
    public void Only_published_posts_are_visible_to_unrelated_readers(SocialPostStatus status, bool expected) =>
        Assert.Equal(expected, SocialRules.CanRead(new SocialPost { AuthorUserId = Guid.NewGuid(), Status = status }, Guid.NewGuid(), false));

    [Fact]
    public void Owner_can_read_draft_but_not_deleted_and_moderator_can_review_deleted()
    {
        var owner = Guid.NewGuid(); var p = new SocialPost { AuthorUserId = owner };
        Assert.True(SocialRules.CanRead(p, owner, false)); p.Status = SocialPostStatus.Deleted;
        Assert.False(SocialRules.CanRead(p, owner, false)); Assert.True(SocialRules.CanRead(p, Guid.NewGuid(), true));
    }
    [Theory]
    [InlineData("000000186674797069736F6D00000200", "video/mp4")]
    [InlineData("89504E470D0A1A0A00000000000000000000000000000000", "image/png")]
    public void Media_mime_is_derived_from_signature(string hex, string expected) => Assert.Equal(expected, SocialRules.MediaType(Convert.FromHexString(hex))!.Value.ContentType);
    [Theory]
    [InlineData("3C7376673E3C7363726970743E3C2F7363726970743E3C2F7376673E")]
    [InlineData("00000018667479707174202000000200")]
    [InlineData("0000001866747970")]
    public void Svg_unknown_video_and_truncated_content_are_rejected(string hex) => Assert.Null(SocialRules.MediaType(Convert.FromHexString(hex)));

    [Fact]
    public async Task Cannot_publish_another_authors_draft_or_delete_without_delete_permission()
    {
        var f = new Factory(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = Guid.NewGuid(), Text = "Draft" });
        var c = Controller(f, Guid.NewGuid(), "publish");
        Assert.Equal(404, Status(await c.Action(1, new SocialAction("submit", 1), default)));
        var manager = Controller(f, Guid.NewGuid(), "manage");
        Assert.Equal(403, Status(await manager.Action(1, new SocialAction("delete", 1), default)));
    }
    [Fact]
    public async Task Author_without_publish_permission_submits_for_review_not_direct_publication()
    {
        var f = new Factory(); var owner = Guid.NewGuid(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = owner, Text = "Draft" });
        Assert.Equal(200, Status(await Controller(f, owner, "create").Action(1, new SocialAction("submit", 1), default)));
        await using var db = f.Open(); Assert.Equal(SocialPostStatus.PendingReview, (await db.SocialPosts.SingleAsync()).Status);
        Assert.Equal("submit", (await db.SocialAudits.SingleAsync()).Action);
    }
    [Fact]
    public async Task Empty_post_and_stale_revision_cannot_be_published()
    {
        var f = new Factory(); var owner = Guid.NewGuid(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = owner });
        var c = Controller(f, owner, "create", "publish");
        Assert.Equal(409, Status(await c.Action(1, new SocialAction("submit", 0), default)));
        Assert.Equal(400, Status(await c.Action(1, new SocialAction("submit", 1), default)));
    }
    [Fact]
    public async Task Like_is_idempotent_and_unlike_preserves_actor_timestamps()
    {
        var f = new Factory(); var owner = Guid.NewGuid(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = owner, Status = SocialPostStatus.Published });
        var c = Controller(f, owner, "interact");
        Assert.Equal(200, Status(await c.Like(1, new(true), default)));
        Assert.Equal(200, Status(await c.Like(1, new(true), default)));
        Assert.Equal(200, Status(await c.Like(1, new(false), default)));
        await using var db = f.Open(); var like = Assert.Single(await db.SocialLikes.ToArrayAsync());
        Assert.False(like.IsActive); Assert.Equal(owner, like.UserId); Assert.True(like.CreatedAtUtc <= like.UpdatedAtUtc);
    }
    [Fact]
    public async Task Comment_closed_and_other_users_comment_edit_are_rejected()
    {
        var f = new Factory(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = Guid.NewGuid(), Status = SocialPostStatus.Published, CommentsEnabled = false });
        await using (var db = f.Open()) { db.SocialComments.Add(new SocialComment { Id = 1, PostId = 1, UserId = Guid.NewGuid(), Text = "original" }); await db.SaveChangesAsync(); }
        var c = Controller(f, Guid.NewGuid(), "interact", "edit_own");
        Assert.Equal(409, Status(await c.AddComment(1, new("comment"), default)));
        Assert.Equal(403, Status(await c.EditComment(1, 1, new("edited", 1), default)));
    }
    [Fact]
    public async Task Report_is_deduplicated_and_does_not_hide_the_post()
    {
        var f = new Factory(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = Guid.NewGuid(), Status = SocialPostStatus.Published, Text = "original" });
        var user = Guid.NewGuid(); var c = Controller(f, user);
        Assert.Equal(200, Status(await c.Report(1, new("privacy", "details"), default)));
        Assert.Equal(200, Status(await c.Report(1, new("privacy", "details"), default)));
        await using var db = f.Open(); var r = Assert.Single(await db.SocialReports.ToArrayAsync());
        Assert.Contains("original", r.EvidenceJson); Assert.Equal(user, r.UserId); Assert.Equal(SocialPostStatus.Published, (await db.SocialPosts.SingleAsync()).Status);
    }
    [Fact]
    public async Task Reviewer_cannot_delete_post_without_delete_any_permission()
    {
        var f = new Factory(); await Seed(f, new SocialPost { Id = 1, AuthorUserId = Guid.NewGuid(), Status = SocialPostStatus.Published });
        Assert.Equal(403, Status(await Controller(f, Guid.NewGuid(), "moderate").Resolve(1, new("delete", 1, "reason"), default)));
    }
    [Fact]
    public async Task Feed_projects_cards_and_private_posts_are_not_readable_by_others()
    {
        var f = new Factory(); var owner = Guid.NewGuid();
        await Seed(f, new SocialPost { Id = 1, AuthorUserId = owner, Text = "private" });
        await using (var db = f.Open()) {
            db.SocialPosts.Add(new SocialPost { Id = 2, AuthorUserId = owner, Text = "public", Status = SocialPostStatus.Published });
            await db.SaveChangesAsync();
        }
        var c = Controller(f, Guid.NewGuid());
        Assert.Equal(200, Status(await c.Posts(null, "feed", 1)));
        Assert.Equal(200, Status(await c.Post(2, default)));
        Assert.Equal(404, Status(await c.Post(1, default)));
        Assert.Equal(403, Status(await c.Posts(null, "reports")));
    }
    [Fact]
    public async Task Resolved_report_can_reopen_while_preserving_previous_evidence_in_audit()
    {
        var f = new Factory(); var owner = Guid.NewGuid(); var user = Guid.NewGuid();
        await Seed(f, new SocialPost { Id = 1, AuthorUserId = owner, Status = SocialPostStatus.Published, Text = "original" });
        var c = Controller(f, user);
        await c.Report(1, new("privacy", "first"), default);
        await using (var db = f.Open()) {
            var r = await db.SocialReports.SingleAsync(); r.ResolvedAtUtc = DateTimeOffset.UtcNow; r.Resolution = "dismiss";
            await db.SaveChangesAsync();
        }
        Assert.Equal(200, Status(await c.Report(1, new("abuse", "second"), default)));
        await using var check = f.Open(); var report = Assert.Single(await check.SocialReports.ToArrayAsync());
        Assert.Null(report.ResolvedAtUtc); Assert.Equal("second", report.Details);
        Assert.Contains(await check.SocialAudits.ToArrayAsync(), x => x.Action == "ReportReopened" && x.SnapshotJson.Contains("first"));
    }
    [Fact]
    public void Unique_collision_translation_does_not_mask_unrelated_database_errors()
    {
        Assert.True(SchoolSocialController.KnownCollision(new DbUpdateException("duplicate", new Npgsql.PostgresException("duplicate", "ERROR", "ERROR", "23505", constraintName: "PK_social_likes"))));
        Assert.False(SchoolSocialController.KnownCollision(new DbUpdateException("duplicate", new Npgsql.PostgresException("duplicate", "ERROR", "ERROR", "23505", constraintName: "IX_students_StudentCode"))));
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void Both_database_models_have_concurrency_unique_likes_reports_and_typed_media(bool sqlServer)
    {
        using SchoolsDbContext db = sqlServer ? new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer("Server=localhost;Database=design_only;Integrated Security=true;TrustServerCertificate=true").Options)
            : new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql("Host=localhost;Database=design_only;Username=design_only").Options);
        Assert.True(db.Model.FindEntityType(typeof(SocialPost))!.FindProperty("Revision")!.IsConcurrencyToken);
        Assert.Equal(new[] { "PostId", "UserId" }, db.Model.FindEntityType(typeof(SocialLike))!.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.Contains(db.Model.FindEntityType(typeof(SocialReport))!.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(new[] { "PostId", "UserId", "TargetKey" }));
        Assert.Contains(db.Model.FindEntityType(typeof(SocialMedia))!.GetForeignKeys(), x => x.Properties.Single().Name == "DocumentVersionId");
        var sql = db.SocialPosts.Where(p => p.Id < 100 && p.Status == SocialPostStatus.Published).OrderByDescending(p => p.Id).Take(16)
            .Select(p => new { p.Id, likes = db.SocialLikes.Count(x => x.PostId == p.Id && x.IsActive), comments = db.SocialComments.Count(x => x.PostId == p.Id && !x.IsDeleted) }).ToQueryString();
        Assert.Contains("ORDER BY", sql); Assert.Contains(sqlServer ? "TOP" : "LIMIT", sql);
    }
}
