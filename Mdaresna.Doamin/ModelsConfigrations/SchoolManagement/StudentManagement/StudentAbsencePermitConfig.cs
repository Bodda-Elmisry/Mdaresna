using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.StudentManagement
{
    public class StudentAbsencePermitConfig : IEntityTypeConfiguration<StudentAbsencePermit>
    {
        public void Configure(EntityTypeBuilder<StudentAbsencePermit> builder)
        {
            builder
                .HasKey(e => e.Id);

            builder
                .HasIndex(e => new { e.StudentId, e.Date })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder
                .HasOne(e => e.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.ClassRoom)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
