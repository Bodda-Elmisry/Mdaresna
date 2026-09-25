namespace Mdaresna.Platform.Domain.Registry;

public sealed class GlobalStudentRegistry
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? BirthCertificateNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
