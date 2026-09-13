namespace Mdaresna.Platform.Application.Errors;

public sealed class PlatformResourceNotFoundException : PlatformApplicationException
{
    public PlatformResourceNotFoundException(string code, string message)
        : base(code, message)
    {
    }
}
