using System.Security.Claims;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Mdaresna.Schools.UnitTests;

public sealed class SubjectTotalPolicyTests
{
    private sealed class Factory : ISchoolDbContextFactory
    {
        public readonly DbContextOptions<SchoolsDbContext> Options = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public Task<SchoolLoginTarget?> ResolveAsync(string code,CancellationToken ct=default)=>Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string code,CancellationToken ct=default)=>Task.FromResult<SchoolsDbContext?>(code=="school"?new(Options):null);
    }
    private static SchoolCurriculumController Controller(Factory factory,bool manage=true)=>new(factory){ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(SchoolClaimTypes.SchoolCode,"school"),new Claim(SchoolClaimTypes.Permission,"school.academics.view"),new Claim(SchoolClaimTypes.Permission,manage?"school.academics.manage":"none")},"test"))}}};
    private static async Task<SaveCurriculumRequest> Seed(Factory f,bool? policy=null,bool existing=false)
    {
        await using var db=new SchoolsDbContext(f.Options);
        var program=new EducationProgram{Id=Guid.NewGuid(),Code="PROGRAM"};var stage=new EducationStage{Id=Guid.NewGuid(),EducationProgramId=program.Id,EducationProgram=program,Code="STAGE"};
        var grade=new GradeLevel{Id=Guid.NewGuid(),EducationStageId=stage.Id,EducationStage=stage,Code="GRADE"};var subject=new Subject{Id=Guid.NewGuid(),Code="SUBJECT"};
        var plan=new CurriculumPlan{Id=Guid.NewGuid(),EducationProgramId=program.Id,EducationProgram=program,Code="PLAN",VersionLabel="ONE"};db.AddRange(grade,subject,plan);
        if(existing)db.CurriculumGradeSubjects.Add(new(){Id=Guid.NewGuid(),CurriculumPlanId=plan.Id,GradeLevelId=grade.Id,SubjectId=subject.Id,WeeklyPeriods=3,IsRequired=true,CountsTowardTotal=policy});
        await db.SaveChangesAsync();return new(ParentId:plan.Id,SecondaryParentId:grade.Id,ThirdParentId:subject.Id,WeeklyPeriods:3,IsRequired:true,CountsTowardTotal:policy);
    }
    [Theory][InlineData(true)][InlineData(false)] public async Task New_subject_requires_explicit_policy_and_required_flag_is_independent(bool policy)
    {
        var f=new Factory();var request=await Seed(f,policy);var c=Controller(f);
        Assert.IsType<BadRequestObjectResult>(await c.Create("grade-subjects",request with{CountsTowardTotal=null},default));
        Assert.Equal(201,Assert.IsType<ObjectResult>(await c.Create("grade-subjects",request,default)).StatusCode);
        await using var db=new SchoolsDbContext(f.Options);var item=await db.CurriculumGradeSubjects.SingleAsync();Assert.Equal(policy,item.CountsTowardTotal);Assert.True(item.IsRequired);
    }
    [Fact] public async Task Existing_policy_is_preserved_for_older_clients_and_legacy_unknown_is_not_assigned_implicitly()
    {
        var f=new Factory();var request=await Seed(f,false,true);var c=Controller(f);Guid id;
        await using(var db=new SchoolsDbContext(f.Options)){id=(await db.CurriculumGradeSubjects.SingleAsync()).Id;}
        Assert.IsType<OkObjectResult>(await c.Update("grade-subjects",id,request with{CountsTowardTotal=null},default));
        await using(var db=new SchoolsDbContext(f.Options)){Assert.False((await db.CurriculumGradeSubjects.SingleAsync()).CountsTowardTotal);}
        var legacy=new Factory();var oldRequest=await Seed(legacy,null,true);var old=Controller(legacy);Guid oldId;
        await using(var db=new SchoolsDbContext(legacy.Options)){oldId=(await db.CurriculumGradeSubjects.SingleAsync()).Id;}
        await old.Update("grade-subjects",oldId,oldRequest,default);
        await using(var db=new SchoolsDbContext(legacy.Options)){Assert.Null((await db.CurriculumGradeSubjects.SingleAsync()).CountsTowardTotal);}
        Assert.IsType<OkObjectResult>(await old.Update("grade-subjects",oldId,oldRequest with{CountsTowardTotal=true},default));
    }
    [Fact] public async Task Offering_exposes_inherited_false_and_cannot_override_a_frozen_plan()
    {
        var f=new Factory();var request=await Seed(f,false,true);Guid id;
        await using(var db=new SchoolsDbContext(f.Options)){
            var item=await db.CurriculumGradeSubjects.SingleAsync();id=item.Id;(await db.CurriculumPlans.SingleAsync()).Status=CurriculumPlanStatus.Active;
            db.GradeSubjectOfferings.Add(new(){Id=Guid.NewGuid(),GradeOfferingId=Guid.NewGuid(),CurriculumGradeSubjectId=id});await db.SaveChangesAsync();
        }
        var c=Controller(f);var result=Assert.IsType<OkObjectResult>(await c.Get(false,default));var response=Assert.IsType<ApiResponse<SchoolCurriculumResponse>>(result.Value).Data!;
        Assert.False(response.Offerings.Single().CountsTowardTotal);Assert.False(response.GradeSubjects.Single().CountsTowardTotal);
        Assert.Equal(409,Assert.IsType<ConflictObjectResult>(await c.Update("grade-subjects",id,request with{CountsTowardTotal=true},default)).StatusCode);
        Assert.IsType<BadRequestObjectResult>(await c.Create("offerings",new(CountsTowardTotal:true),default));
    }
    [Fact] public async Task Read_only_actor_cannot_edit_total_policy()
    {
        var f=new Factory();var request=await Seed(f,true);Assert.IsType<ForbidResult>(await Controller(f,false).Create("grade-subjects",request,default));
    }
}
