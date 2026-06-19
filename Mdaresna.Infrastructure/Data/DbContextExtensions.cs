using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mdaresna.Doamin.Models.Base;
using Elmasry.UtcToCountryTimeZones.Enums;
using Elmasry.UtcToCountryTimeZones.Extensions;

namespace Mdaresna.Infrastructure.Data
{
    public static class DbContextExtensions
    {
        public static void ConvertAllDatesToUtc(this DbContext context)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            // In-memory we set CreateDate and LastModifyDate to Yemen Local Time.
            // The EF Core Value Converter will automatically handle converting them to UTC when writing to DB.
            var localNow = DateTime.UtcNow.ToTimeZone(Country.Yemen);

            foreach (var entry in entries)
            {
                // Set audit properties if the entity inherits from AuditBase
                if (entry.Entity is AuditBase auditEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditEntity.CreateDate = localNow;
                    }
                    auditEntity.LastModifyDate = localNow;
                }
            }
        }

        public static void ConvertEntityDatesToUtc(object entity, bool isCreate = true)
        {
            if (entity == null) return;

            // For bulk operations, we set audit dates to Yemen Local Time in memory.
            // The EF Core Value Converter will automatically handle converting them to UTC during the bulk insert.
            var localNow = DateTime.UtcNow.ToTimeZone(Country.Yemen);

            if (entity is AuditBase auditEntity)
            {
                if (isCreate)
                {
                    auditEntity.CreateDate = localNow;
                }
                auditEntity.LastModifyDate = localNow;
            }
        }

        public static void RegisterUtcTimeZoneConverters(this ModelBuilder modelBuilder)
        {
            var yemenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Aden");

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => ConvertYemenLocalTimeToUtc(v, yemenTimeZone),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc).ToTimeZone(Country.Yemen)
            );

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v == null ? null : ConvertYemenLocalTimeToUtc(v.Value, yemenTimeZone),
                v => v == null ? null : (DateTime?)DateTime.SpecifyKind(v.Value, DateTimeKind.Utc).ToTimeZone(Country.Yemen)
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

                foreach (var property in properties)
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }

        private static DateTime ConvertYemenLocalTimeToUtc(DateTime value, TimeZoneInfo yemenTimeZone)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }

            // Request JSON and audit helpers can produce Local or Unspecified values.
            // Non-UTC dates in this application represent Yemen clock time, so remove
            // the Kind marker before converting with the Yemen timezone.
            var yemenLocalValue = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(yemenLocalValue, yemenTimeZone);
        }
    }
}
