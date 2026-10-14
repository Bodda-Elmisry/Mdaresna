using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.UnitTests;

public sealed class ClassOperationalAssignmentTests
{
    private sealed class Factory : ISchoolDbContextFactory
    {
        private readonly DbContextOptions<SchoolsDbContext> options = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public Task<SchoolLoginTarget?> ResolveAsync(string c,CancellationToken ct=default)=>Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string c,CancellationToken ct=default)=>Task.FromResult<SchoolsDbContext?>(new SchoolsDbContext(options));
        public SchoolsDbContext Open()=>new(options);
    }
    private static SchoolsDbContext Provider(bool sqlServer)=>sqlServer?new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer("Server=localhost;Database=design_only;Integrated Security=true;TrustServerCertificate=true").Options):new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql("Host=localhost;Database=design_only;Username=design_only").Options);
    private static async Task<(Guid User,Guid Class,Guid Assignment)> Seed(Factory factory, params string[] permissions)
    {
        await using var db=factory.Open();var today=DateOnly.FromDateTime(DateTime.UtcNow);
        var user=new LocalUserAccount{Id=Guid.NewGuid(),Status=LocalUserStatus.Active,Person=new Person{Id=Guid.NewGuid(),DisplayName="Staff"}};
        var year=new ProgramAcademicYear{Id=Guid.NewGuid(),StartDate=today.AddDays(-30),EndDate=today.AddDays(30)};
        var section=new ClassSection{Id=Guid.NewGuid(),IsActive=true,GradeOffering=new GradeOffering{Id=Guid.NewGuid(),IsActive=true,ProgramAcademicYear=year}};
        var assignment=new ClassOperationalAssignment{Id=Guid.NewGuid(),User=user,StartsOn=today.AddDays(-1),EndsOn=today.AddDays(1),Responsibility="Attendance"};
        assignment.Grants.Add(new ClassOperationalGrant{Id=Guid.NewGuid(),ClassSection=section,Action="attendance.session"});
        db.ClassOperationalAssignments.Add(assignment);
        var role=new LocalRole{Id=Guid.NewGuid(),IsActive=true};
        foreach(var code in permissions)role.Permissions.Add(new LocalRolePermission{Permission=new LocalPermission{Id=Guid.NewGuid(),Code=code}});
        db.LocalUserRoles.Add(new LocalUserRole{User=user,Role=role});await db.SaveChangesAsync();
        return(user.Id,section.Id,assignment.Id);
    }
    [Fact]
    public async Task Employee_permission_and_assignment_are_both_required_and_class_is_limited()
    {
        var f=new Factory();var ids=await Seed(f,"school.attendance.record");await using var db=f.Open();
        Assert.True(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"attendance.session",default));
        Assert.False(await ClassOperationalScope.Allows(db,ids.User,Guid.NewGuid(),"attendance.session",default));
        Assert.False(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"attendance.daily",default));
        Assert.False(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"attendance.finalize",default));
    }
    [Fact]
    public async Task Assignment_does_not_grant_a_missing_permission()
    {
        var f=new Factory();var ids=await Seed(f);await using var db=f.Open();
        Assert.False(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"attendance.session",default));
        Assert.Empty(await ClassOperationalScope.VisibleClasses(db,ids.User,default));
    }
    [Theory]
    [InlineData("disabled")] [InlineData("expired")] [InlineData("future")] [InlineData("user")] [InlineData("role")]
    public async Task Revocation_is_applied_from_database_without_waiting_for_token_expiry(string mode)
    {
        var f=new Factory();var ids=await Seed(f,"school.attendance.record");await using(var db=f.Open()){
            var a=await db.ClassOperationalAssignments.SingleAsync();var today=DateOnly.FromDateTime(DateTime.UtcNow);
            if(mode=="disabled")a.IsActive=false;if(mode=="expired")a.EndsOn=today.AddDays(-1);if(mode=="future")a.StartsOn=today.AddDays(1);
            if(mode=="user")(await db.LocalUsers.SingleAsync()).Status=LocalUserStatus.Disabled;
            if(mode=="role")(await db.LocalRoles.SingleAsync()).IsActive=false;await db.SaveChangesAsync();}
        await using var read=f.Open();Assert.False(await ClassOperationalScope.Allows(read,ids.User,ids.Class,"attendance.session",default));
    }
    [Fact]
    public async Task Multiple_people_can_hold_the_same_class_action()
    {
        var f=new Factory();var first=await Seed(f,"school.attendance.record");var second=await Seed(f,"school.attendance.record");
        await using(var db=f.Open()){var g=await db.ClassOperationalGrants.SingleAsync(x=>x.AssignmentId==second.Assignment);g.ClassSectionId=first.Class;await db.SaveChangesAsync();}
        await using var read=f.Open();Assert.True(await ClassOperationalScope.Allows(read,first.User,first.Class,"attendance.session",default));Assert.True(await ClassOperationalScope.Allows(read,second.User,first.Class,"attendance.session",default));
    }
    [Fact]
    public async Task Stale_assignment_update_has_no_notification_or_mutation()
    {
        var f=new Factory();var ids=await Seed(f,"school.operations.manage","school.attendance.record");
        var c=new SchoolClassAssignmentsController(f){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub,ids.User.ToString())],"test"))}}};
        var day=DateOnly.FromDateTime(DateTime.UtcNow);
        var result=await c.Update(ids.Assignment,new(ids.User,null,"Changed",day,day,true,999,[new(ids.Class,"attendance.session",null)]),default);
        Assert.Equal(409,Assert.IsType<ObjectResult>(result).StatusCode);await using var read=f.Open();Assert.Empty(read.SchoolUserNotifications);Assert.Equal("Attendance",(await read.ClassOperationalAssignments.SingleAsync()).Responsibility);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void Both_provider_models_translate_scope_and_protect_attendance_revision(bool sqlServer)
    {
        using var db=Provider(sqlServer);var query=ClassOperationalScope.Current(db,Guid.NewGuid(),new DateOnly(2026,10,5));
        Assert.Contains("class_operational_grants",query.ToQueryString());
        Assert.True(db.Model.FindEntityType(typeof(StudentAttendanceRegister))!.FindProperty(nameof(StudentAttendanceRegister.Revision))!.IsConcurrencyToken);
        Assert.True(db.Model.FindEntityType(typeof(ClassOperationalAssignment))!.FindProperty(nameof(ClassOperationalAssignment.Revision))!.IsConcurrencyToken);
        Assert.True(db.Model.FindEntityType(typeof(Mdaresna.Schools.Domain.Exams.ExamPaper))!.FindProperty("UpdatedAtUtc")!.IsConcurrencyToken);
        var index=Assert.Single(db.Model.FindEntityType(typeof(StudentAttendanceRegister))!.GetIndexes(),x=>x.Properties.Select(p=>p.Name).SequenceEqual(new[]{"ClassSectionId","AttendanceDate","UnitKey"}));Assert.True(index.IsUnique);
    }
    [Fact]
    public async Task Floor_assignment_stops_when_the_class_moves_and_can_still_be_ended()
    {
        var f=new Factory();var ids=await Seed(f,"school.operations.manage","school.attendance.record");
        Guid roomId;
        await using(var db=f.Open())
        {
            var floor=new BuildingFloor{Id=Guid.NewGuid(),Building=new SchoolBuilding{Id=Guid.NewGuid(),Branch=new SchoolBranch{Id=Guid.NewGuid()}}};
            var room=new SchoolRoom{Id=Guid.NewGuid(),Floor=floor};roomId=room.Id;
            var day=DateOnly.FromDateTime(DateTime.UtcNow);
            db.ClassRoomAssignments.Add(new(){Id=Guid.NewGuid(),ClassSectionId=ids.Class,Room=room,EffectiveFrom=day.AddDays(-1),EffectiveTo=day.AddDays(1)});
            (await db.ClassOperationalAssignments.SingleAsync()).BuildingFloor=floor;await db.SaveChangesAsync();
            Assert.True(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"attendance.session",default));
            var nextFloor=new BuildingFloor{Id=Guid.NewGuid(),Building=floor.Building};db.BuildingFloors.Add(nextFloor);room.Floor=nextFloor;await db.SaveChangesAsync();
            Assert.False(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"attendance.session",default));
        }
        var c=new SchoolClassAssignmentsController(f){ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub,ids.User.ToString())],"test"))}}};
        var result=await c.Update(ids.Assignment,new(ids.User,null,"Ignored",default,default,false,1,[]),default);
        Assert.IsType<OkObjectResult>(result);
        await using var read=f.Open();Assert.False((await read.ClassOperationalAssignments.SingleAsync()).IsActive);
        Assert.Single(read.ClassOperationalAssignmentAudits);Assert.Single(read.SchoolUserNotifications);
    }
    [Fact]
    public async Task Creating_assignment_audits_and_notifies_without_changing_roles()
    {
        var f=new Factory();var ids=await Seed(f,"school.operations.manage","school.attendance.record");
        var c=new SchoolClassAssignmentsController(f){ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub,ids.User.ToString())],"test"))}}};
        var day=DateOnly.FromDateTime(DateTime.UtcNow);
        var result=await c.Create(new(ids.User,null,"Assistant",day,day,true,0,[new(ids.Class,"attendance.session",null)]),default);
        Assert.IsType<OkObjectResult>(result);await using var read=f.Open();
        Assert.Single(read.ClassOperationalAssignmentAudits);Assert.Single(read.SchoolUserNotifications);Assert.Single(read.LocalUserRoles);
        Assert.Equal("ClassOperationalAssignment",(await read.SchoolUserNotifications.SingleAsync()).RelatedEntityType);
    }
    [Fact]
    public async Task Employee_records_teachers_lesson_and_stale_writer_cannot_overwrite_it()
    {
        var f=new Factory();var ids=await Seed(f,"school.attendance.record","school.attendance.view");var day=DateOnly.FromDateTime(DateTime.UtcNow);
        Guid slotId,enrollmentId,teacherId;
        await using(var db=f.Open())
        {
            var schoolRef=Guid.NewGuid();var now=DateTimeOffset.UtcNow;
            var school=SchoolInformation.Create(schoolRef,schoolRef,Guid.NewGuid(),"TEST","Test school","Private","Shared",null,null,Guid.NewGuid(),"UNIT","Unit",0,"EGP",Guid.NewGuid(),now,now);
            school.ConfigureStudentAttendance("UTC",StudentAttendanceMode.PerSession,now);db.SchoolInformation.Add(school);
            var section=await db.ClassSections.Include(x=>x.GradeOffering).ThenInclude(x=>x.ProgramAcademicYear).SingleAsync();
            var program=new EducationProgram{Id=Guid.NewGuid()};db.EducationPrograms.Add(program);
            var stage=new EducationStage{Id=Guid.NewGuid(),EducationProgram=program};db.EducationStages.Add(stage);
            var grade=new GradeLevel{Id=Guid.NewGuid(),EducationStage=stage};db.GradeLevels.Add(grade);
            section.GradeOffering.GradeLevel=grade;section.GradeOffering.ProgramAcademicYear.EducationProgram=program;
            var teacher=new LocalUserAccount{Id=Guid.NewGuid(),Kind=SchoolUserKind.Teacher,Status=LocalUserStatus.Active,Person=new Person{Id=Guid.NewGuid(),DisplayName="Lesson teacher"}};
            db.LocalUsers.Add(teacher);teacherId=teacher.Id;
            var scope=new TeacherGradeSubjectScope{Id=Guid.NewGuid(),TeacherUser=teacher};db.TeacherGradeSubjectScopes.Add(scope);
            var slot=new WeeklyTimetableSlot{Id=Guid.NewGuid(),ClassSectionId=ids.Class,PrimaryTeacherScope=scope,DayOfWeek=DateTime.UtcNow.DayOfWeek,StartsAt=TimeOnly.MinValue,EndsAt=new(23,59,59),SlotNumber=3};
            db.WeeklyTimetableSlots.Add(slot);slotId=slot.Id;
            var enrollment=new StudentEnrollment{Id=Guid.NewGuid(),ClassSectionId=ids.Class,GradeOffering=section.GradeOffering,EnrollmentDate=day.AddDays(-1),Student=new Student{Id=Guid.NewGuid(),Person=new Person{Id=Guid.NewGuid(),DisplayName="Student"}}};
            db.StudentEnrollments.Add(enrollment);enrollmentId=enrollment.Id;await db.SaveChangesAsync();
        }
        var c=new SchoolStudentAttendanceController(f,new SchoolClock(TimeProvider.System)){ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub,ids.User.ToString())],"test"))}}};
        var entries=new RecordAttendanceEntryRequest[]{new(enrollmentId,StudentAttendanceStatus.Present,null,null,null)};
        Assert.IsType<OkObjectResult>(await c.Record(ids.Class,new(entries,$"SLOT:{slotId:D}",null,false),default));
        var conflict=await c.Record(ids.Class,new(entries,$"SLOT:{slotId:D}",null,false),default);
        Assert.Equal(409,Assert.IsType<ConflictObjectResult>(conflict).StatusCode);
        Assert.IsType<OkObjectResult>(await c.Record(ids.Class,new([new(enrollmentId,StudentAttendanceStatus.Absent,null,null,null)],$"SLOT:{slotId:D}",1,false),default));
        Assert.IsType<ForbidResult>(await c.Record(ids.Class,new(entries,$"SLOT:{slotId:D}",2,true),default));
        await using var read=f.Open();var register=await read.StudentAttendanceRegisters.Include(x=>x.Entries).SingleAsync();
        Assert.Equal(ids.User,register.RecordedByUserId);Assert.Equal(ids.User,register.LastEditedByUserId);
        Assert.Equal(teacherId,register.PrimaryTeacherUserIdSnapshot);Assert.Equal(teacherId,register.ActualTeacherUserIdSnapshot);
        Assert.Equal("Lesson teacher",register.ActualTeacherNameSnapshot);Assert.Equal(2,register.Revision);
        Assert.Equal(StudentAttendanceStatus.Absent,Assert.Single(register.Entries).Status);Assert.Equal(2,await read.StudentAttendanceAudits.CountAsync());
        var grant=await read.ClassOperationalGrants.SingleAsync();grant.Action="attendance.finalize";await read.SaveChangesAsync();
        var response=Assert.IsType<OkObjectResult>(await c.GetClassAttendance(ids.Class,default));
        var state=Assert.IsType<Mdaresna.Api.Contracts.ApiResponse<ClassAttendanceResponse>>(response.Value).Data!;
        Assert.False(state.CanRecord);Assert.True(state.CanFinalize);
        Assert.IsType<OkObjectResult>(await c.FinalizeRegister(register.Id,new(2),default));
        await using var finalRead=f.Open();Assert.Equal(StudentAttendanceRegisterStatus.Finalized,(await finalRead.StudentAttendanceRegisters.SingleAsync()).Status);
        Assert.Equal(3,await finalRead.StudentAttendanceAudits.CountAsync());
    }
    [Fact]
    public async Task Results_assignment_is_subject_scoped_and_unlinking_subject_revokes_it()
    {
        var f=new Factory();var ids=await Seed(f,"school.exams.results.enter");Guid subjectId;
        await using(var db=f.Open())
        {
            var offering=new GradeSubjectOffering{Id=Guid.NewGuid(),IsActive=true};db.GradeSubjectOfferings.Add(offering);subjectId=offering.Id;
            db.ClassSectionSubjects.Add(new(){Id=Guid.NewGuid(),ClassSectionId=ids.Class,GradeSubjectOffering=offering});
            var grant=await db.ClassOperationalGrants.SingleAsync();grant.Action="exams.results.enter";grant.GradeSubjectOfferingId=subjectId;
            await db.SaveChangesAsync();
            Assert.True(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"exams.results.enter",default,subjectId));
            Assert.False(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"exams.results.enter",default,Guid.NewGuid()));
            Assert.False(await ClassOperationalScope.Allows(db,ids.User,Guid.NewGuid(),"exams.results.enter",default,subjectId));
            (await db.ClassSectionSubjects.SingleAsync()).IsActive=false;await db.SaveChangesAsync();
            Assert.False(await ClassOperationalScope.Allows(db,ids.User,ids.Class,"exams.results.enter",default,subjectId));
        }
    }
}
