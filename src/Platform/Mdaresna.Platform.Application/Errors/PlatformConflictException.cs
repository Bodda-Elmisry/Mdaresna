namespace Mdaresna.Platform.Application.Errors;

public sealed class PlatformConflictException : PlatformApplicationException
{
    public PlatformConflictException(string code, string message)
        : base(code, message)
    {
    }
}
