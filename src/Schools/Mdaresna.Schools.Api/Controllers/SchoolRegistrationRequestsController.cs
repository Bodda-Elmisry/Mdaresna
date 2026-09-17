using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Contracts.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/schools/v1/registration-requests")]
public sealed class SchoolRegistrationRequestsController(
    SubmitSchoolRegistrationRequestHandler handler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubmitSchoolRegistrationResult>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitSchoolRegistrationRequestBody body,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(new SubmitSchoolRegistrationRequest(
                body.SchoolName, body.Address, body.SchoolType, body.SchoolPrimaryPhone,
                body.OwnerName, body.OwnerPhone), Request.Headers.TraceParent.FirstOrDefault(),
                cancellationToken);
            return Accepted(ApiResponse<SubmitSchoolRegistrationResult>.Success(
                result, statusCode: StatusCodes.Status202Accepted,
                message: "School registration request accepted.",
                correlationId: HttpContext.TraceIdentifier));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object?>.Failure(
                StatusCodes.Status400BadRequest, "request.invalid", exception.Message,
                correlationId: HttpContext.TraceIdentifier));
        }
    }
}

public sealed record SubmitSchoolRegistrationRequestBody(
    string SchoolName,
    string Address,
    RequestedSchoolTypeV2 SchoolType,
    string SchoolPrimaryPhone,
    string OwnerName,
    string OwnerPhone);
