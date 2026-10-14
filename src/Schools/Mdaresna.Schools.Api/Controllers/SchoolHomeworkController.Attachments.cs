using System.Data;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Domain.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Mdaresna.Schools.Api.Controllers;

public sealed partial class SchoolHomeworkController
{
    [HttpGet("homework/{id:guid}/attachments"),Authorize(Policy=SchoolPermissionPolicies.HomeworkView)]
    public Task<IActionResult> Attachments(Guid id,[FromServices] ISchoolDocumentStorage storage,CancellationToken ct) => WorkDocuments(id,"list",null,null,storage,ct);
    [HttpGet("homework/{id:guid}/attachments/{attachmentId:guid}/content"),Authorize(Policy=SchoolPermissionPolicies.HomeworkView)]
    public Task<IActionResult> AttachmentContent(Guid id,Guid attachmentId,[FromServices] ISchoolDocumentStorage storage,CancellationToken ct) => WorkDocuments(id,"content",attachmentId,null,storage,ct);
    [HttpPost("homework/{id:guid}/attachments"),Authorize(Policy=SchoolPermissionPolicies.HomeworkManage),RequestSizeLimit(54_000_000),RequestFormLimits(MultipartBodyLengthLimit=54_000_000)]
    public Task<IActionResult> UploadAttachment(Guid id,IFormFile file,[FromServices] ISchoolDocumentStorage storage,CancellationToken ct,[FromForm] Guid? uploadId=null) => WorkDocuments(id,"upload",null,file,storage,ct,uploadId);
    [HttpDelete("homework/{id:guid}/attachments/{attachmentId:guid}"),Authorize(Policy=SchoolPermissionPolicies.HomeworkManage)]
    public Task<IActionResult> RemoveAttachment(Guid id,Guid attachmentId,[FromServices] ISchoolDocumentStorage storage,CancellationToken ct) => WorkDocuments(id,"remove",attachmentId,null,storage,ct);
    private async Task<IActionResult> WorkDocuments(Guid id,string operation,Guid? attachmentId,IFormFile? file,ISchoolDocumentStorage storage,CancellationToken ct,Guid? uploadId=null)
    {
        await using var db=await RequireDb(ct);if(db is null)return Unauthorized();
        var write=operation is "upload" or "remove";
        await using var transaction=write?await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct):null;
        var entity=await Load(db,id,ct);if(entity is null)return NotFound();
        if(!await CanAccessSubject(db,entity.ClassSectionSubject.ClassSectionId,entity.ClassSectionSubjectId,ct))return Forbid();
        if(write && entity.Status!=HomeworkStatus.Draft)return Conflict(Failure(409,"attachments.draft_only","Only draft homework attachments can be changed."));
        try {
        var result=await WorkAttachments.Execute<HomeworkAttachment>(this,db,storage,User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value??"",CurrentUserId(),id,operation,attachmentId,file,ct,uploadId);
        if(transaction is not null && result is OkObjectResult)await transaction.CommitAsync(ct);
        return result;
        }
        catch(Exception ex) when(write) {
            if(operation=="upload")await WorkAttachments.CleanupUpload(db,storage);
            if(ex is DbUpdateException or System.Data.Common.DbException)return Conflict(Failure(409,"attachments.changed","The item changed. Reload and retry."));
            throw;
        }
    }
}
