namespace Mdaresna.Schools.Application.Identity;

public sealed class SchoolRoutingUnavailableException(Exception innerException)
    : Exception("School routing is temporarily unavailable.", innerException);
