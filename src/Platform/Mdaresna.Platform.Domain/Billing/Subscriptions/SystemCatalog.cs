using System.Text.RegularExpressions;

namespace Mdaresna.Platform.Domain.Billing.Subscriptions;

public enum CapacityMetric { Student = 1, Employee = 2, Cashbox = 3 }

public sealed record SupportedSystemCapability(string Code, string NameAr, string NameEn, CapacityMetric Metric, bool Implemented);

public static class SystemCapabilityCatalog
{
    // Stable integration codes, not saleable modules. Pricing and system composition belong to the database.
    public static IReadOnlyList<SupportedSystemCapability> All { get; } = Array.AsReadOnly(new[] {
        new SupportedSystemCapability("academic.students", "الطلاب والقيد", "Students and enrollment", CapacityMetric.Student, true),
        new("academic.admissions", "طلبات الالتحاق", "Admissions", CapacityMetric.Student, true),
        new("academic.attendance", "حضور الطلاب", "Student attendance", CapacityMetric.Student, true),
        new("academic.curriculum", "المناهج والكتب", "Curriculum and books", CapacityMetric.Student, true),
        new("academic.teaching-plans", "خطط التدريس", "Teaching plans", CapacityMetric.Student, true),
        new("academic.homework", "الواجبات", "Homework", CapacityMetric.Student, true),
        new("academic.activities", "الأنشطة", "Activities", CapacityMetric.Student, true),
        new("academic.assessments", "التقييمات والاختبارات", "Assessments and exams", CapacityMetric.Student, true),
        new("academic.social", "التواصل والمنشورات", "School communication", CapacityMetric.Student, true),
        new("academic.staff", "مدرسو وموظفو التشغيل", "Academic operational staff", CapacityMetric.Student, true),
        new("academic.fees", "رسوم الصفوف وطرق السداد", "Grade fees and payment schedules", CapacityMetric.Student, false),
        new("administration.employees", "شؤون الموظفين", "Employee administration", CapacityMetric.Employee, false),
        new("administration.attendance", "الحضور والإجازات", "Staff attendance and leave", CapacityMetric.Employee, false),
        new("administration.payroll", "احتساب المرتبات", "Payroll calculation", CapacityMetric.Employee, false),
        new("finance.cashboxes", "الخزن", "Cashboxes", CapacityMetric.Cashbox, false),
        new("finance.collections", "التحصيلات", "Collections", CapacityMetric.Cashbox, false),
        new("finance.disbursements", "الصرف والتسويات", "Disbursements and settlements", CapacityMetric.Cashbox, false)
    });

    public static string Normalize(CapacityMetric metric, IEnumerable<string> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);
        var items = codes.ToArray();
        if (items.Length is < 1 or > 50 || items.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Select supported system capabilities.");
        var normalized = items.Select(x => x.Trim()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (normalized.Any(code => !All.Any(x => x.Code == code && x.Metric == metric)))
            throw new ArgumentException("A capability does not belong to this capacity metric.");
        return string.Join('\n', normalized);
    }
}

public sealed class SchoolSystemDefinition
{
    private SchoolSystemDefinition() { }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = "";
    public string NameAr { get; private set; } = "";
    public string NameEn { get; private set; } = "";
    public CapacityMetric CapacityMetric { get; private set; }
    public string CapabilityCodes { get; private set; } = "";
    public bool IsActive { get; private set; }
    public long Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SchoolSystemDefinition Create(Guid id, string code, string nameAr, string nameEn, CapacityMetric metric, string[] capabilities, DateTimeOffset now)
    {
        if (id == Guid.Empty || !Enum.IsDefined(metric)) throw new ArgumentException("Invalid system identity or capacity metric.");
        var system = new SchoolSystemDefinition { Id = id, Code = CatalogValidation.Code(code), CapacityMetric = metric, CreatedAtUtc = now };
        system.Update(nameAr, nameEn, capabilities, true, now);
        return system;
    }
    public void Update(string nameAr, string nameEn, string[] capabilities, bool active, DateTimeOffset now)
    {
        var ar = CatalogValidation.Name(nameAr); var en = CatalogValidation.Name(nameEn);
        var codes = SystemCapabilityCatalog.Normalize(CapacityMetric, capabilities);
        NameAr = ar; NameEn = en; CapabilityCodes = codes; IsActive = active; Touch(now);
    }
    public void Touch(DateTimeOffset now)
    {
        if (now.Offset != TimeSpan.Zero) throw new ArgumentException("UTC timestamp required.");
        UpdatedAtUtc = now; Version = checked(Version + 1);
    }
}

public sealed class SystemPlan
{
    private SystemPlan() { }
    public Guid Id { get; private set; }
    public Guid SystemId { get; private set; }
    public string Code { get; private set; } = "";
    public string NameAr { get; private set; } = "";
    public string NameEn { get; private set; } = "";
    public int SortOrder { get; private set; }
    public bool IsArchived { get; private set; }
    public long Version { get; private set; }
    public int LatestRevision { get; private set; }
    public int? PublishedRevision { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static SystemPlan Create(Guid id, Guid systemId, string code, string nameAr, string nameEn, int sortOrder, DateTimeOffset now)
    {
        if (id == Guid.Empty || systemId == Guid.Empty) throw new ArgumentException("Invalid plan identity.");
        var plan = new SystemPlan { Id = id, SystemId = systemId, Code = CatalogValidation.Code(code), CreatedAtUtc = now };
        plan.Rename(nameAr, nameEn); plan.Move(sortOrder); return plan;
    }
    public void Rename(string ar, string en) { var nameAr = CatalogValidation.Name(ar); var nameEn = CatalogValidation.Name(en); NameAr = nameAr; NameEn = nameEn; Version++; }
    public void Move(int order) { if (order < 1) throw new ArgumentOutOfRangeException(nameof(order)); SortOrder = order; Version++; }
    public int NextRevision() { if (IsArchived) throw new ArgumentException("Archived plans cannot receive new offers."); LatestRevision = checked(LatestRevision + 1); Version++; return LatestRevision; }
    public void Publish(int revision) { if (IsArchived || revision < 1 || revision > LatestRevision) throw new ArgumentException("Invalid publication."); PublishedRevision = revision; Version++; }
    public void Archive() { IsArchived = true; Version++; }
}

/// <summary>Immutable offer. Publication is a pointer on the plan; changing a price always creates another revision.</summary>
public sealed class SystemPlanRevision
{
    private SystemPlanRevision() { }
    public Guid PlanId { get; private set; }
    public int Revision { get; private set; }
    public int Capacity { get; private set; }
    public decimal BaseUnitPrice { get; private set; }
    public decimal AddOnUnitPrice { get; private set; }
    public string Currency { get; private set; } = "";
    public decimal BaseAmount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public static SystemPlanRevision Create(Guid planId, int revision, int capacity, decimal basePrice, decimal addOnPrice, string currency, DateTimeOffset now)
    {
        if (planId == Guid.Empty || revision < 1 || capacity is < 1 or > 1_000_000) throw new ArgumentException("Invalid plan offer.");
        CatalogValidation.Price(basePrice); CatalogValidation.Price(addOnPrice);
        var code = currency?.Trim().ToUpperInvariant() ?? "";
        if (!Regex.IsMatch(code, "^[A-Z]{3}$", RegexOptions.CultureInvariant)) throw new ArgumentException("Invalid currency code.");
        var amount = decimal.Round(checked(capacity * basePrice), 2, MidpointRounding.AwayFromZero);
        if (amount <= 0 || amount > 9999999999999999.99m) throw new ArgumentException("Plan amount exceeds the supported range.");
        return new() { PlanId = planId, Revision = revision, Capacity = capacity, BaseUnitPrice = basePrice, AddOnUnitPrice = addOnPrice,
            Currency = code, BaseAmount = amount, CreatedAtUtc = now };
    }
}

internal static class CatalogValidation
{
    internal static string Code(string value)
    {
        var code = value?.Trim().ToUpperInvariant() ?? "";
        if (!Regex.IsMatch(code, "^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$", RegexOptions.CultureInvariant)) throw new ArgumentException("Invalid catalog code.");
        return code;
    }
    internal static string Name(string value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 200 ? value.Trim() : throw new ArgumentException("Catalog names are required and limited to 200 characters.");
    internal static void Price(decimal value) { if (value <= 0 || value > 99999999999999.9999m || decimal.Round(value, 4) != value) throw new ArgumentException("Prices must be positive decimal(18,4) values."); }
}
