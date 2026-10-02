using Mdaresna.Schools.Domain.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

public sealed class SchoolPresentationConfiguration : IEntityTypeConfiguration<SchoolPresentation>
{
    public void Configure(EntityTypeBuilder<SchoolPresentation> b)
    {
        b.ToTable("school_presentations", "school");
        b.HasKey(x => x.SchoolInformationId);
        b.HasOne(x => x.SchoolInformation).WithOne().HasForeignKey<SchoolPresentation>(x => x.SchoolInformationId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.TaglineAr).HasMaxLength(250);
        b.Property(x => x.TaglineEn).HasMaxLength(250);
        b.Property(x => x.AboutAr).HasMaxLength(10000);
        b.Property(x => x.AboutEn).HasMaxLength(10000);
        b.Property(x => x.VisionAr).HasMaxLength(5000);
        b.Property(x => x.VisionEn).HasMaxLength(5000);
        b.Property(x => x.MissionAr).HasMaxLength(5000);
        b.Property(x => x.MissionEn).HasMaxLength(5000);
        b.Property(x => x.Email).HasMaxLength(250);
        b.Property(x => x.Website).HasMaxLength(1000);
        b.Property(x => x.ContactAddress).HasMaxLength(500);
        b.Property(x => x.ContactPhone).HasMaxLength(50);
        b.Property(x => x.CoverPosition).HasPrecision(5, 2);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasOne<SchoolProfileImage>().WithMany().HasForeignKey(x => x.CoverImageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SchoolProfileImageConfiguration : IEntityTypeConfiguration<SchoolProfileImage>
{
    public void Configure(EntityTypeBuilder<SchoolProfileImage> b)
    {
        b.ToTable("school_profile_images", "school");
        b.HasKey(x => x.Id);
        b.Property(x => x.CaptionAr).HasMaxLength(500);
        b.Property(x => x.CaptionEn).HasMaxLength(500);
        b.HasIndex(x => new { x.SchoolInformationId, x.SortOrder });
        b.HasIndex(x => x.DocumentId).IsUnique();
        b.HasOne(x => x.SchoolInformation).WithMany().HasForeignKey(x => x.SchoolInformationId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
    }
}
