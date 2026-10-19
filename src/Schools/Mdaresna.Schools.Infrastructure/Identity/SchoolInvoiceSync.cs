using System.Text.Json;
using System.Text.RegularExpressions;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public static class SchoolInvoiceSync
{
    public static void Validate(SchoolInvoiceSnapshotChangedV1 x)
    {
        if (x is null || x.SchoolId == Guid.Empty || x.InvoiceId == Guid.Empty || x.Revision < 1 || x.Amount is <= 0 or > 9999999999999999.99m ||
            string.IsNullOrWhiteSpace(x.Number) || x.Number.Length > 64 ||
            !Regex.IsMatch(x.SchoolCode ?? "", "^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$") ||
            !Regex.IsMatch(x.Currency ?? "", "^[A-Z]{3}$") ||
            x.PaymentStatus is not ("Unpaid" or "PendingReview" or "Paid" or "Rejected") ||
            x.CreatedAtUtc.Offset != TimeSpan.Zero || x.PaidAtUtc?.Offset is { } offset && offset != TimeSpan.Zero ||
            (x.PaymentStatus == "Paid") != x.PaidAtUtc.HasValue ||
            x.Lines is null || x.Lines.Length is < 1 or > 30 || x.Receipts is null || x.Receipts.Length > 1000)
            throw new ArgumentException("Invalid invoice snapshot.");
        if (x.Lines.Any(l => l is null || l.QuoteId == Guid.Empty || l.PlanId == Guid.Empty || l.Capacity < 1 || l.Amount is < 0 or > 9999999999999999.99m ||
            l.Kind is not ("New" or "AddOn" or "Upgrade" or "Renew") ||
            new[] { l.SystemNameAr, l.SystemNameEn, l.PlanNameAr, l.PlanNameEn }.Any(n => string.IsNullOrWhiteSpace(n) || n.Length > 250)) ||
            x.Lines.Select(l => l.QuoteId).Distinct().Count() != x.Lines.Length || x.Lines.Sum(l => l.Amount) != x.Amount ||
            x.Receipts.Any(r => r is null || r.PaymentRequestId == Guid.Empty || r.Method is not ("BankTransfer" or "CashRepresentative") ||
                string.IsNullOrWhiteSpace(r.Reference) || r.Reference.Length > 200 || r.RepresentativeName?.Length > 200 ||
                r.ReceivedAtUtc.Offset != TimeSpan.Zero || r.Status is not ("Pending" or "Approved" or "Rejected")))
            throw new ArgumentException("Invalid invoice details.");
    }

    public static async Task ApplyAsync(SchoolsDbContext db, SchoolInvoiceSnapshotChangedV1 snapshot, CancellationToken ct)
    {
        Validate(snapshot);
        var school = await db.SchoolInformation.AsNoTracking().SingleAsync(ct);
        if (school.PlatformSchoolReferenceId != snapshot.SchoolId || school.Code != snapshot.SchoolCode)
            throw new ArgumentException("Invoice school scope mismatch.");
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var query = db.Database.IsNpgsql()
            ? db.InvoiceCopies.FromSqlInterpolated($"SELECT * FROM school.invoice_copies WHERE \"Id\" = {snapshot.InvoiceId} FOR UPDATE")
            : db.Database.IsSqlServer()
                ? db.InvoiceCopies.FromSqlInterpolated($"SELECT * FROM school.invoice_copies WITH (UPDLOCK, HOLDLOCK) WHERE Id = {snapshot.InvoiceId}")
                : db.InvoiceCopies;
        var copy = await query.SingleOrDefaultAsync(x => x.Id == snapshot.InvoiceId, ct);
        if (copy != null && copy.Revision >= snapshot.Revision) return;
        if (copy != null)
        {
            var old = JsonSerializer.Deserialize<SchoolInvoiceSnapshotChangedV1>(copy.PayloadJson)!;
            if (old.Number != snapshot.Number || old.Amount != snapshot.Amount || old.Currency != snapshot.Currency ||
                old.CreatedAtUtc != snapshot.CreatedAtUtc || !old.Lines.SequenceEqual(snapshot.Lines))
                throw new ArgumentException("Issued invoice terms cannot change.");
        }
        else { copy = new() { Id = snapshot.InvoiceId, SchoolId = snapshot.SchoolId, CreatedAtUtc = snapshot.CreatedAtUtc }; db.InvoiceCopies.Add(copy); }
        copy.Revision = snapshot.Revision; copy.PayloadJson = JsonSerializer.Serialize(snapshot); copy.SyncedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
    }
}
