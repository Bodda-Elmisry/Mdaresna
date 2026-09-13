namespace Mdaresna.Platform.Domain.Common;

public sealed class PlatformDomainException : InvalidOperationException
{
    public PlatformDomainException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
