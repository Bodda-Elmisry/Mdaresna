namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

internal static class PlatformStaffSmsMessages
{
    public const string InvitationType = "staff-invitation";
    public const string ActivationType = "otp";

    public static string Invitation(string userName) =>
        $"تمت إضافتك كموظف في إدارة منصة مدارسنا. اسم الدخول الخاص بك: {userName}. افتح تطبيق Mdaresna Platform واختر تفعيل حساب الموظف عند أول دخول للحصول على رمز التفعيل.";

    public static string ActivationCode(string userName, string code) =>
        $"رمز تفعيل حسابك كموظف في إدارة منصة مدارسنا: {code}. اسم الدخول: {userName}. الرمز صالح لمدة 10 دقائق.";
}
