using Mdaresna.Schools.Domain.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class SchoolInvoiceCopyConfiguration : IEntityTypeConfiguration<SchoolInvoiceCopy>
{
    public void Configure(EntityTypeBuilder<SchoolInvoiceCopy> b)
    {
        b.ToTable("invoice_copies"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Revision).IsConcurrencyToken(); b.Property(x => x.PayloadJson).IsRequired();
        b.HasIndex(x => x.CreatedAtUtc);
    }
}
