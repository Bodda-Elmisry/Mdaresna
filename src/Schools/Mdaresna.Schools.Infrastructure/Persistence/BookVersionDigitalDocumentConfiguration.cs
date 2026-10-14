using Mdaresna.Schools.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Mdaresna.Schools.Infrastructure.Persistence;
internal sealed class BookVersionDigitalDocumentConfiguration : IEntityTypeConfiguration<BookVersionDigitalDocument>
{
    public void Configure(EntityTypeBuilder<BookVersionDigitalDocument> b)
    {
        b.ToTable("book_version_digital_documents"); b.HasKey(x => x.BookVersionId);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasOne(x => x.BookVersion).WithMany().HasForeignKey(x => x.BookVersionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
