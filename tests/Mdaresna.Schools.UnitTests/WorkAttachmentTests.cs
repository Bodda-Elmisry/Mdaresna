using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Application.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Mdaresna.Schools.UnitTests;

public sealed class WorkAttachmentTests
{
    private sealed class Factory : ISchoolDbContextFactory
    {
        public readonly DbContextOptions<SchoolsDbContext> Options=new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(x=>x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        public Task<SchoolLoginTarget?> ResolveAsync(string schoolCode,CancellationToken ct=default)=>Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string schoolCode,CancellationToken ct=default)=>Task.FromResult<SchoolsDbContext?>(schoolCode=="fixture-school"?new(Options):null);
    }
    private sealed class TestController : ControllerBase { }
    private sealed class Storage : ISchoolDocumentStorage
    {
        public int Writes,Reads;
        public Task<StoredSchoolDocument> SaveAsync(string s,Guid d,Guid v,Stream stream,string e,CancellationToken ct){Writes++;return Task.FromResult(new StoredSchoolDocument("fixture",Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant(),stream.Length));}
        public Task<Stream?> OpenReadAsync(string key,CancellationToken ct){Reads++;return Task.FromResult<Stream?>(new MemoryStream("%PDF-fixture"u8.ToArray()));}
        public Task DeleteAsync(string key,CancellationToken ct)=>Task.CompletedTask;
    }
    private static SchoolsDbContext Db()=>new(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static TestController Controller()=>new(){ControllerContext=new(){HttpContext=new DefaultHttpContext()}};
    private static IFormFile File(byte[] content,string name="file.pdf")=>new FormFile(new MemoryStream(content),0,content.Length,"file",name);
    [Fact] public void Types_are_detected_from_bytes_not_supplied_MIME()
    {
        Assert.Equal("application/pdf",WorkAttachmentRules.FileType("%PDF-fixture"u8)!.Value.ContentType);
        Assert.Null(WorkAttachmentRules.FileType("<script>not an image"u8));
        Assert.False(WorkAttachmentRules.ValidSize(8*1024*1024+1,"image/png"));
        Assert.True(WorkAttachmentRules.ValidSize(50*1024*1024,"video/mp4"));
        Assert.False(WorkAttachmentRules.ValidSize(25*1024*1024+1,"application/pdf"));
    }
    [Fact] public async Task Owner_scope_rejected_versions_and_removed_links_are_not_downloadable()
    {
        await using var db=Db();var storage=new Storage();var controller=Controller();var owner=Guid.NewGuid();
        var upload=Assert.IsType<OkObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"upload",null,File("%PDF-fixture"u8.ToArray(),"../fixture.pdf"),default));
        var data=Assert.IsType<ApiResponse<WorkAttachmentResponse>>(upload.Value).Data!;
        Assert.Equal("fixture.pdf",data.FileName);Assert.Equal(1,storage.Writes);
        var denied=Assert.IsType<ObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),Guid.NewGuid(),"content",data.Id,null,default));
        Assert.Equal(404,denied.StatusCode);Assert.Equal(0,storage.Reads);
        var content=Assert.IsType<FileStreamResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"content",data.Id,null,default));Assert.True(content.EnableRangeProcessing);await content.FileStream.DisposeAsync();
        var version=await db.DocumentVersions.SingleAsync();version.ValidationStatus=SchoolDocumentValidationStatus.Rejected;await db.SaveChangesAsync();
        Assert.Equal(404,Assert.IsType<ObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"content",data.Id,null,default)).StatusCode);
        Assert.IsType<OkObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"remove",data.Id,null,default));
        Assert.True((await db.Set<HomeworkAttachment>().SingleAsync()).IsDeleted);Assert.Equal(SchoolDocumentStatus.Deleted,(await db.Documents.SingleAsync()).Status);
        Assert.Equal(2,await db.DocumentAudits.CountAsync());
    }
    [Fact] public async Task Each_domain_has_independent_typed_links_and_limit()
    {
        await using var db=Db();var storage=new Storage();var owner=Guid.NewGuid();var controller=Controller();
        for(var i=0;i<10;i++)Assert.IsType<OkObjectResult>(await WorkAttachments.Execute<ClassActivityAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"upload",null,File("%PDF-fixture"u8.ToArray()),default));
        Assert.Equal(400,Assert.IsType<ObjectResult>(await WorkAttachments.Execute<ClassActivityAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"upload",null,File("%PDF-fixture"u8.ToArray()),default)).StatusCode);
        Assert.IsType<OkObjectResult>(await WorkAttachments.Execute<LessonNoteAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"upload",null,File("%PDF-fixture"u8.ToArray()),default));
        Assert.Equal(10,await db.Set<ClassActivityAttachment>().CountAsync());Assert.Equal(1,await db.Set<LessonNoteAttachment>().CountAsync());
        foreach(var type in new[]{typeof(HomeworkAttachment),typeof(ClassActivityAttachment),typeof(LessonNoteAttachment)}){
            var model=db.Model.FindEntityType(type)!;Assert.Equal(2,model.GetForeignKeys().Count());Assert.Contains(model.GetIndexes(),x=>x.IsUnique);
        }
    }
    [Fact] public async Task Invalid_file_is_rejected_before_storage_write()
    {
        await using var db=Db();var storage=new Storage();
        Assert.Equal(400,Assert.IsType<ObjectResult>(await WorkAttachments.Execute<LessonNoteAttachment>(Controller(),db,storage,"school",Guid.NewGuid(),Guid.NewGuid(),"upload",null,File("malicious bytes"u8.ToArray(),"image.png"),default)).StatusCode);
        Assert.Equal(0,storage.Writes);Assert.Empty(await db.Documents.ToArrayAsync());
    }

    [Fact] public async Task Upload_retry_with_same_key_is_idempotent_and_rejects_changed_bytes_or_owner()
    {
        await using var db=Db();var storage=new Storage();var owner=Guid.NewGuid();var key=Guid.NewGuid();var controller=Controller();
        for(var i=0;i<2;i++)Assert.IsType<OkObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"upload",null,File("%PDF-fixture"u8.ToArray()),default,key));
        Assert.Equal(1,storage.Writes);Assert.Equal(1,await db.Set<HomeworkAttachment>().CountAsync());
        Assert.Equal(409,Assert.IsType<ObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),owner,"upload",null,File("%PDF-changed"u8.ToArray()),default,key)).StatusCode);
        Assert.Equal(409,Assert.IsType<ObjectResult>(await WorkAttachments.Execute<HomeworkAttachment>(controller,db,storage,"school",Guid.NewGuid(),Guid.NewGuid(),"upload",null,File("%PDF-fixture"u8.ToArray()),default,key)).StatusCode);
    }

    [Fact] public async Task All_three_controllers_reject_unassigned_actor_before_upload()
    {
        var factory=new Factory();var actor=Guid.NewGuid();var storage=new Storage();
        var subject=new ClassSectionSubject {Id=Guid.NewGuid(),ClassSectionId=Guid.NewGuid(),GradeSubjectOfferingId=Guid.NewGuid(),IsActive=true};
        subject.GradeSubjectOffering=new GradeSubjectOffering {Id=subject.GradeSubjectOfferingId,CurriculumGradeSubject=new CurriculumGradeSubject {Id=Guid.NewGuid(),Subject=new Subject {Id=Guid.NewGuid(),NameAr="مادة",NameEn="Subject"}}};
        var homework=new HomeworkAssignment {Id=Guid.NewGuid(),ClassSectionSubject=subject};
        var activity=new ClassActivity {Id=Guid.NewGuid(),ClassSectionId=subject.ClassSectionId,Scope=ActivityScope.Subject,ClassSectionSubject=subject};
        activity.CreatedByUser=new LocalUserAccount {Id=actor,Person=new Person {Id=Guid.NewGuid(),DisplayName="Fixture"}};
        var note=new StudentLessonNote {Id=Guid.NewGuid(),ClassSectionId=subject.ClassSectionId,CreatedByUserId=actor,WeeklyTimetableSlotId=Guid.NewGuid(),LessonDate=new(2026,10,13)};
        await using(var db=new SchoolsDbContext(factory.Options)){db.HomeworkAssignments.Add(homework);db.ClassActivities.Add(activity);db.StudentLessonNotes.Add(note);await db.SaveChangesAsync();}
        ControllerContext Context()=>new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub,actor.ToString()),new Claim(SchoolClaimTypes.SchoolCode,"fixture-school")],"test"))}};
        var h=new SchoolHomeworkController(factory){ControllerContext=Context()};
        var a=new SchoolClassActivitiesController(factory,new SchoolClock(TimeProvider.System)){ControllerContext=Context()};
        var n=new SchoolStudentLessonNotesController(factory,new SchoolClock(TimeProvider.System)){ControllerContext=Context()};
        Assert.IsType<ForbidResult>(await h.UploadAttachment(homework.Id,File("%PDF-fixture"u8.ToArray()),storage,default));
        Assert.IsType<ForbidResult>(await a.UploadAttachment(activity.Id,File("%PDF-fixture"u8.ToArray()),storage,default));
        Assert.IsType<ForbidResult>(await n.UploadAttachment(note.Id,File("%PDF-fixture"u8.ToArray()),storage,default));
        Assert.Equal(0,storage.Writes);
    }
}
