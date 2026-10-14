using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mdaresna.Schools.Domain.Documents;
namespace Mdaresna.Schools.Infrastructure.Persistence;

public sealed class HomeworkAttachmentConfiguration : IEntityTypeConfiguration<HomeworkAttachment>
{
    public void Configure(EntityTypeBuilder<HomeworkAttachment> b)
    {
        b.ToTable("homework_attachments"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.OwnerId,x.DocumentVersionId }).IsUnique();
        b.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ClassActivityAttachmentConfiguration : IEntityTypeConfiguration<ClassActivityAttachment>
{
    public void Configure(EntityTypeBuilder<ClassActivityAttachment> b)
    {
        b.ToTable("class_activity_attachments"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.OwnerId,x.DocumentVersionId }).IsUnique();
        b.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LessonNoteAttachmentConfiguration : IEntityTypeConfiguration<LessonNoteAttachment>
{
    public void Configure(EntityTypeBuilder<LessonNoteAttachment> b)
    {
        b.ToTable("lesson_note_attachments"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.OwnerId,x.DocumentVersionId }).IsUnique();
        b.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
