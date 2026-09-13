namespace Mdaresna.Platform.Application.Errors;

public abstract class PlatformApplicationException : Exception
{
    protected PlatformApplicationException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
