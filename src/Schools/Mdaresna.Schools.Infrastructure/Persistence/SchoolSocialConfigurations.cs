using Mdaresna.Schools.Domain.Social;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

public sealed class SocialPostConfiguration : IEntityTypeConfiguration<SocialPost>
{
    public void Configure(EntityTypeBuilder<SocialPost> b)
    {
        b.ToTable("social_posts"); b.HasKey(x => x.Id);
        b.Property(x => x.Text).HasMaxLength(10000);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasIndex(x => new { x.Status, x.Id }); b.HasIndex(x => new { x.AuthorUserId, x.Id });
        b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SocialMediaConfiguration : IEntityTypeConfiguration<SocialMedia>
{
    public void Configure(EntityTypeBuilder<SocialMedia> b)
    {
        b.ToTable("social_media"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.PostId, x.SortOrder });
        b.HasOne<SocialPost>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SchoolDocumentVersion>().WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SocialLikeConfiguration : IEntityTypeConfiguration<SocialLike>
{
    public void Configure(EntityTypeBuilder<SocialLike> b)
    {
        b.ToTable("social_likes"); b.HasKey(x => new { x.PostId, x.UserId });
        b.HasOne<SocialPost>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SocialCommentConfiguration : IEntityTypeConfiguration<SocialComment>
{
    public void Configure(EntityTypeBuilder<SocialComment> b)
    {
        b.ToTable("social_comments"); b.HasKey(x => x.Id); b.Property(x => x.Text).HasMaxLength(2000);
        b.Property(x => x.Revision).IsConcurrencyToken(); b.HasIndex(x => new { x.PostId, x.Id });
        b.HasOne<SocialPost>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SocialReportConfiguration : IEntityTypeConfiguration<SocialReport>
{
    public void Configure(EntityTypeBuilder<SocialReport> b)
    {
        b.ToTable("social_reports"); b.HasKey(x => x.Id);
        b.Property(x => x.TargetKey).HasMaxLength(40); b.Property(x => x.Reason).HasMaxLength(30);
        b.Property(x => x.Details).HasMaxLength(1000); b.Property(x => x.Resolution).HasMaxLength(1100);
        b.HasIndex(x => new { x.PostId, x.UserId, x.TargetKey }).IsUnique(); b.HasIndex(x => new { x.PostId, x.ResolvedAtUtc });
        b.HasOne<SocialPost>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SocialComment>().WithMany().HasForeignKey(x => x.CommentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SocialAuditConfiguration : IEntityTypeConfiguration<SocialAudit>
{
    public void Configure(EntityTypeBuilder<SocialAudit> b)
    {
        b.ToTable("social_audits"); b.HasKey(x => x.Id); b.Property(x => x.Action).HasMaxLength(50);
        b.HasIndex(x => new { x.PostId, x.Id });
        b.HasOne<SocialPost>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SocialSettingsConfiguration : IEntityTypeConfiguration<SocialSettings>
{
    public void Configure(EntityTypeBuilder<SocialSettings> b)
    { b.ToTable("social_settings"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever(); b.Property(x => x.Revision).IsConcurrencyToken(); b.HasData(new SocialSettings()); }
}
public sealed class SocialReviewerConfiguration : IEntityTypeConfiguration<SocialReviewer>
{
    public void Configure(EntityTypeBuilder<SocialReviewer> b)
    { b.ToTable("social_reviewers"); b.HasKey(x => x.UserId); b.HasOne<LocalUserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); }
}
