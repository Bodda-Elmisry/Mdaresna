namespace Mdaresna.Platform.Application.Billing.Read;

public sealed record PlatformPaymentPage(
    IReadOnlyList<PlatformPaymentDetail> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
