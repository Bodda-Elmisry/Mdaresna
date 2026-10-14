using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Documents;

public sealed record WorkAttachmentResponse(Guid Id,string FileName,string ContentType,long SizeBytes,DateTimeOffset CreatedAtUtc);

public static class WorkAttachments
{
    public static async Task<IActionResult> Execute<T>(ControllerBase controller, SchoolsDbContext db, ISchoolDocumentStorage storage,
        string schoolCode, Guid actor, Guid ownerId, string operation, Guid? attachmentId, IFormFile? file, CancellationToken ct, Guid? uploadId = null)
        where T : class, IWorkAttachment, new()
    {
        var query = db.Set<T>().Where(x => EF.Property<Guid>(x,"OwnerId") == ownerId && !EF.Property<bool>(x,"IsDeleted"));
        IActionResult Fail(string code,string message,int status=400) => controller.StatusCode(status,ApiResponse<object?>.Failure(status,code,message));
        if (operation == "list")
        {
            var rows = await query.Where(x => x.Version.Document.Status == SchoolDocumentStatus.Active && x.Version.ValidationStatus != SchoolDocumentValidationStatus.Rejected)
                .OrderBy(x => x.Version.CreatedAtUtc).Select(x => new WorkAttachmentResponse(x.Id,x.Version.OriginalFileName,x.Version.ContentType,x.Version.SizeBytes,x.Version.CreatedAtUtc)).ToArrayAsync(ct);
            return controller.Ok(ApiResponse<IReadOnlyList<WorkAttachmentResponse>>.Success(rows));
        }
        if (operation == "upload")
        {
            if (file is null || file.Length <= 0 || file.Length > WorkAttachmentRules.MaxVideoBytes) return Fail("attachments.invalid","Invalid file or size.");
            await using var input = file.OpenReadStream();
            var header = new byte[32]; var count = await input.ReadAtLeastAsync(header,Math.Min(32,(int)Math.Min(file.Length,32)),false,ct);
            var type = WorkAttachmentRules.FileType(header.AsSpan(0,count));
            if (type is null || !WorkAttachmentRules.ValidSize(file.Length,type.Value.ContentType) || !input.CanSeek) return Fail("attachments.invalid","Use an image up to 8 MiB, PDF up to 25 MiB, or MP4 up to 50 MiB.");
            if(uploadId==Guid.Empty)return Fail("attachments.invalid","Invalid upload identifier.");
            if(uploadId.HasValue) {
                var existing=await db.Set<T>().AsNoTracking().Include(x=>x.Version).ThenInclude(x=>x.Document).SingleOrDefaultAsync(x=>EF.Property<Guid>(x,"Id")==uploadId.Value,ct);
                if(existing is not null) {
                    if(existing.OwnerId!=ownerId || existing.IsDeleted || existing.Version.Document.Status!=SchoolDocumentStatus.Active || existing.Version.ValidationStatus==SchoolDocumentValidationStatus.Rejected)return Fail("attachments.changed","Upload identifier is unavailable.",409);
                    input.Position=0;var sha=Convert.ToHexString(await System.Security.Cryptography.SHA256.HashDataAsync(input,ct)).ToLowerInvariant();
                    if(sha!=existing.Version.Sha256 || file.Length!=existing.Version.SizeBytes)return Fail("attachments.changed","Upload identifier belongs to another file.",409);
                    return controller.Ok(ApiResponse<WorkAttachmentResponse>.Success(new(existing.Id,existing.Version.OriginalFileName,existing.Version.ContentType,existing.Version.SizeBytes,existing.Version.CreatedAtUtc)));
                }
            }
            if (await query.CountAsync(ct) >= WorkAttachmentRules.MaxFiles) return Fail("attachments.limit","Maximum ten attachments per item.");
            input.Position=0; var now=DateTimeOffset.UtcNow; var documentId=Guid.NewGuid(); var versionId=Guid.NewGuid();
            var stored=await storage.SaveAsync(schoolCode,documentId,versionId,input,type.Value.Extension,ct);
            var name=Path.GetFileName(file.FileName.Replace('\\','/')); name=new string(name.Where(c=>!char.IsControl(c)).Take(180).ToArray());
            if(string.IsNullOrWhiteSpace(name))name=$"attachment{type.Value.Extension}";
            name=Path.GetFileNameWithoutExtension(name)+type.Value.Extension;
            var document=new SchoolDocument { Id=documentId,Title=name,CreatedByUserId=actor,CreatedAtUtc=now,UpdatedAtUtc=now };
            var version=new SchoolDocumentVersion { Id=versionId,VersionNumber=1,OriginalFileName=name,ContentType=type.Value.ContentType,SizeBytes=stored.SizeBytes,
                Sha256=stored.Sha256,StorageKey=stored.StorageKey,ValidationStatus=SchoolDocumentValidationStatus.Pending,CreatedByUserId=actor,CreatedAtUtc=now,Document=document };
            var link=new T { Id=uploadId??Guid.NewGuid(),OwnerId=ownerId,DocumentVersionId=versionId,Version=version };
            db.Set<T>().Add(link); db.DocumentAudits.Add(new() { Id=Guid.NewGuid(),DocumentId=documentId,Action=$"{typeof(T).Name}Uploaded",ActorUserId=actor,CreatedAtUtc=now,Details=ownerId.ToString() });
            try { await db.SaveChangesAsync(ct); }
            catch { await storage.DeleteAsync(stored.StorageKey,CancellationToken.None); throw; }
            return controller.Ok(ApiResponse<WorkAttachmentResponse>.Success(new(link.Id,name,version.ContentType,version.SizeBytes,now)));
        }
        var attachment=await query.Include(x=>x.Version).ThenInclude(x=>x.Document).SingleOrDefaultAsync(x=>EF.Property<Guid>(x,"Id")==attachmentId,ct);
        if(attachment is null || attachment.Version.Document.Status!=SchoolDocumentStatus.Active) return Fail("attachments.not_found","Attachment not found.",404);
        if(operation=="remove")
        {
            attachment.IsDeleted=true; attachment.Version.Document.Status=SchoolDocumentStatus.Deleted; attachment.Version.Document.UpdatedAtUtc=DateTimeOffset.UtcNow;
            db.DocumentAudits.Add(new() { Id=Guid.NewGuid(),DocumentId=attachment.Version.DocumentId,Action=$"{typeof(T).Name}Removed",ActorUserId=actor,CreatedAtUtc=DateTimeOffset.UtcNow,Details=ownerId.ToString() });
            await db.SaveChangesAsync(ct); return controller.Ok(ApiResponse<object?>.Success(null));
        }
        if(attachment.Version.ValidationStatus==SchoolDocumentValidationStatus.Rejected)return Fail("attachments.not_found","Attachment unavailable.",404);
        var stream=await storage.OpenReadAsync(attachment.Version.StorageKey,ct);if(stream is null)return Fail("attachments.not_found","Attachment missing.",404);
        controller.Response.Headers.CacheControl="private, no-store"; controller.Response.Headers["X-Content-Type-Options"]="nosniff";
        return controller.Request.Query["download"]=="true" ? controller.File(stream,attachment.Version.ContentType,attachment.Version.OriginalFileName,true) : controller.File(stream,attachment.Version.ContentType,enableRangeProcessing:true);
    }

    public static async Task CleanupUpload(SchoolsDbContext db, ISchoolDocumentStorage storage)
    {
        foreach(var entry in db.ChangeTracker.Entries<SchoolDocumentVersion>())
            await storage.DeleteAsync(entry.Entity.StorageKey,CancellationToken.None);
    }
}
