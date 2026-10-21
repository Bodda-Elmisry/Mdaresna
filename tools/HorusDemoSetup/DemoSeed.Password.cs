using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal static partial class DemoSeed
{
    public static async Task ResetStaffPasswordAsync(SchoolsDbContext db)
    {
        var password = Environment.GetEnvironmentVariable("MDARESNA_DEMO_STAFF_PASSWORD");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new InvalidOperationException("MDARESNA_DEMO_STAFF_PASSWORD must contain at least eight characters.");
        var users = await db.LocalUsers.Where(x => x.UserName.StartsWith("demo-")).ToArrayAsync();
        if (users.Length != 105 || users.Any(x => x.Status != LocalUserStatus.Active))
            throw new InvalidOperationException("Expected exactly 105 active demo staff accounts.");
        var ids = users.Select(x => x.Id).ToArray();
        var credentials = await db.Set<LocalUserCredential>().Where(x => ids.Contains(x.UserId)).ToDictionaryAsync(x => x.UserId);
        if (credentials.Count != users.Length) throw new InvalidOperationException("Missing demo credentials.");
        var hasher = new PasswordHasher<LocalUserAccount>();
        await using var tx = await db.Database.BeginTransactionAsync();
        foreach (var user in users)
        {
            var credential = credentials[user.Id];
            credential.PasswordHash = hasher.HashPassword(user, password);
            credential.SecurityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            credential.ChangedAtUtc = DateTimeOffset.UtcNow;
            credential.FailedSignInCount = 0;
            credential.LockoutEndUtc = null;
        }
        foreach (var session in await db.Set<LocalUserSession>().Where(x => ids.Contains(x.UserId) && x.RevokedAtUtc == null).ToArrayAsync())
        {
            session.RevokedAtUtc = DateTimeOffset.UtcNow;
            session.RevocationReason = "Demo staff password reset";
        }
        await db.SaveChangesAsync();
        foreach (var user in users)
        {
            var stored = await db.Set<LocalUserCredential>().AsNoTracking().SingleAsync(x => x.UserId == user.Id);
            if (hasher.VerifyHashedPassword(user, stored.PasswordHash, password) == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("Credential verification failed.");
        }
        await tx.CommitAsync();
        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", "Mdaresna.Schools.Api", "secrets.json");
        var json = JsonNode.Parse(await File.ReadAllTextAsync(file))!.AsObject();
        json["SchoolDemo:HorusStaffPassword"] = password;
        await File.WriteAllTextAsync(file, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Updated and verified 105 active demo staff credentials; owner unchanged. User Secrets synchronized. Password not printed.");
    }
}
