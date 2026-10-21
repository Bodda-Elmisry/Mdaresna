using System.Security.Claims;
using System.Text.Json;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Controllers;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolSettingsPaginationTests
{
    [Theory]
    [InlineData(TeachingPlanType.Yearly)] [InlineData(TeachingPlanType.Term)] [InlineData(TeachingPlanType.Monthly)] [InlineData(TeachingPlanType.Weekly)] [InlineData(TeachingPlanType.Daily)]
    public async Task Class_plan_tabs_do_not_include_other_class_targets_or_other_grades(TeachingPlanType tab)
    {
        var factory=new Factory();var classId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            var program=new EducationProgram{Id=Guid.NewGuid()};
            var year=new ProgramAcademicYear{Id=Guid.NewGuid(),EducationProgram=program,AcademicYearDefinition=new AcademicYearDefinition{Id=Guid.NewGuid()}};
            var offering=new GradeOffering{Id=Guid.NewGuid(),IsActive=true,ProgramAcademicYear=year,GradeLevel=new GradeLevel{Id=Guid.NewGuid(),EducationStage=new EducationStage{Id=Guid.NewGuid(),EducationProgram=program}}};
            var a=new ClassSection{Id=classId,GradeOffering=offering};var b=new ClassSection{Id=Guid.NewGuid(),GradeOffering=offering};
            var subject=new GradeSubjectOffering{Id=Guid.NewGuid(),GradeOffering=offering,CurriculumGradeSubject=new CurriculumGradeSubject{Id=Guid.NewGuid(),GradeLevel=offering.GradeLevel,CurriculumPlan=new CurriculumPlan{Id=Guid.NewGuid(),EducationProgram=program},Subject=new Subject{Id=Guid.NewGuid(),NameAr="عربي",NameEn="Arabic"}}};
            db.ClassSections.AddRange(a,b);
            foreach(var kind in Enum.GetValues<TeachingPlanType>())
            {
                TeachingPlan Plan(string title,ClassSection? target)
                {
                    var plan=new TeachingPlan{Id=Guid.NewGuid(),Type=kind,TitleAr=title,TitleEn=title,ProgramAcademicYear=year,GradeOffering=offering,GradeSubjectOffering=subject};
                    if(target!=null)plan.Targets.Add(new TeachingPlanTarget{Id=Guid.NewGuid(),ClassSection=target});
                    db.TeachingPlans.Add(plan);return plan;
                }
                Plan("belongs-to-a",kind is TeachingPlanType.Yearly or TeachingPlanType.Term?null:a);
                Plan("only-b",b);
                var other=Plan("other-grade",null);
                other.GradeOffering=new GradeOffering{Id=Guid.NewGuid(),IsActive=true,ProgramAcademicYear=year,GradeLevel=offering.GradeLevel};
                db.GradeOfferings.Add(other.GradeOffering);
                db.ClassSections.Add(new ClassSection{Id=Guid.NewGuid(),GradeOffering=other.GradeOffering});
                if(kind is not (TeachingPlanType.Yearly or TeachingPlanType.Term))Plan("untargeted-operational-plan",null);
            }
            await db.SaveChangesAsync();
        }
        var identity=new ClaimsIdentity(new[]{new Claim(SchoolClaimTypes.SchoolCode,"PAGING-QA"),new Claim(ClaimTypes.Role,Mdaresna.Schools.Domain.Identity.SchoolIdentitySeed.SchoolAdminRoleCode)},"test");
        var controller=new SchoolTeachingPlansController(factory,null!,null!,null!){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(identity)}}};
        var page=Data(await controller.List(classId,null,tab,default,1,10));
        Assert.Equal(1,page.GetProperty("totalCount").GetInt32());
        Assert.Equal("belongs-to-a",page.GetProperty("items")[0].GetProperty("titleEn").GetString());
        controller.HttpContext.User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(SchoolClaimTypes.SchoolCode,"PAGING-QA")],"test"));
        Assert.IsType<NotFoundObjectResult>(await controller.List(classId,null,tab,default,1,10));
    }
    [Theory]
    [InlineData("year",1)] [InlineData("term",1)] [InlineData("definition",2)] [InlineData("unknown",0)]
    public async Task Plan_list_filters_context_before_count_and_pagination(string context,int expected)
    {
        var factory=new Factory();var definition=new AcademicYearDefinition{Id=Guid.NewGuid(),NameAr="2026/27",NameEn="2026/27"};
        var yearId=Guid.NewGuid();var termId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var i=0;i<2;i++)
            {
                var program=new EducationProgram{Id=Guid.NewGuid(),Code="P"+i,NameAr="برنامج"+i,NameEn="Programme"+i};
                var year=new ProgramAcademicYear{Id=i==0?yearId:Guid.NewGuid(),EducationProgram=program,AcademicYearDefinition=definition};
                var grade=new GradeLevel{Id=Guid.NewGuid(),EducationStage=new EducationStage{Id=Guid.NewGuid(),EducationProgram=program}};
                var offering=new GradeOffering{Id=Guid.NewGuid(),ProgramAcademicYear=year,GradeLevel=grade,IsActive=true,Status=GradeOfferingStatus.Active};
                var subject=new GradeSubjectOffering{Id=Guid.NewGuid(),GradeOffering=offering,CurriculumGradeSubject=new CurriculumGradeSubject{Id=Guid.NewGuid(),GradeLevel=grade,CurriculumPlan=new CurriculumPlan{Id=Guid.NewGuid(),EducationProgram=program},Subject=new Subject{Id=Guid.NewGuid(),NameAr="عربي",NameEn="Arabic"}}};
                var term=new AcademicTerm{Id=i==0?termId:Guid.NewGuid(),ProgramAcademicYear=year};
                db.ClassSections.Add(new ClassSection{Id=Guid.NewGuid(),GradeOffering=offering});
                foreach(var type in new[]{TeachingPlanType.Yearly,TeachingPlanType.Term}) db.TeachingPlans.Add(new TeachingPlan{Id=Guid.NewGuid(),Type=type,ProgramAcademicYear=year,GradeOffering=offering,GradeSubjectOffering=subject,AcademicTerm=type==TeachingPlanType.Term?term:null,TitleAr="خطة",TitleEn="Plan",FromDate=new(2026,9,1),ToDate=new(2027,6,30)});
            }
            await db.SaveChangesAsync();
        }
        var claims=new[]{new Claim(SchoolClaimTypes.SchoolCode,"PAGING-QA"),new Claim(ClaimTypes.Role,Mdaresna.Schools.Domain.Identity.SchoolIdentitySeed.SchoolAdminRoleCode)};
        var controller=new SchoolTeachingPlansController(factory,null!,null!,null!){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(claims,"test"))}}};
        var page=Data(await controller.List(null,null,context=="term"?TeachingPlanType.Term:TeachingPlanType.Yearly,default,1,1,false,
            context=="year"?yearId:context=="unknown"?Guid.NewGuid():null,context=="term"?termId:null,context=="definition"?definition.Id:null));
        Assert.Equal(expected,page.GetProperty("totalCount").GetInt32());
        Assert.Equal(Math.Min(1,expected),page.GetProperty("items").GetArrayLength());
    }
    [Fact]
    public async Task Term_groups_page_years_by_identity_and_keep_all_term_counts()
    {
        var factory = new Factory();
        await using (var db = factory.Open())
        {
            var definition = new AcademicYearDefinition { Id = Guid.NewGuid(), NameAr = "2026/27", NameEn = "2026/27" };
            for (var yearIndex = 0; yearIndex < 3; yearIndex++)
            {
                var year = new ProgramAcademicYear { Id = Guid.NewGuid(), NameAr = "نفس الاسم", NameEn = "Same name", StartDate = new(2026,9,1),
                    EducationProgram = new EducationProgram { Id = Guid.NewGuid(), Code = $"P{yearIndex}", NameAr = $"نظام {yearIndex}", NameEn = $"Programme {yearIndex}" }, AcademicYearDefinition = definition, IsDeleted = yearIndex == 2 };
                for (var termIndex = 0; termIndex < 23; termIndex++) db.AcademicTerms.Add(new AcademicTerm { Id = Guid.NewGuid(), ProgramAcademicYear = year, SortOrder = termIndex, Code = $"T{termIndex:00}", IsDeleted = termIndex == 22 });
            }
            await db.SaveChangesAsync();
        }
        var controller = Controller(factory, "academics");
        var first = Data(await controller.Get("academic", "term-groups", 1, 1));
        var second = Data(await controller.Get("academic", "term-groups", 2, 1));
        Assert.Equal(3, first.GetProperty("totalCount").GetInt32());
        var group = first.GetProperty("items")[0];
        var yearId = group.GetProperty("year").GetProperty("id").GetGuid();
        Assert.NotEqual(yearId, second.GetProperty("items")[0].GetProperty("year").GetProperty("id").GetGuid());
        Assert.Contains("Programme 0", group.GetProperty("nameEn").GetString());
        Assert.Equal(22, group.GetProperty("termCount").GetInt32());
        var terms = Data(await controller.Get("academic", "terms", 2, 20, programAcademicYearId: yearId));
        Assert.Equal(22, terms.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, terms.GetProperty("items").GetArrayLength());
        Assert.All(terms.GetProperty("items").EnumerateArray(), t => Assert.Equal(yearId, t.GetProperty("parentId").GetGuid()));
        var deletedParentGroup = Data(await controller.Get("academic", "term-groups", 3, 1));
        Assert.True(deletedParentGroup.GetProperty("items")[0].GetProperty("year").GetProperty("isDeleted").GetBoolean());
        var trash = Data(await controller.Get("academic", "term-groups", deleted: true));
        Assert.Equal(3, trash.GetProperty("totalCount").GetInt32());
        Assert.All(trash.GetProperty("items").EnumerateArray(), g => Assert.Equal(1, g.GetProperty("termCount").GetInt32()));
        Assert.Equal(66, Data(await controller.Get("academic", "terms")).GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Term_groups_require_academic_permission_and_validate_windows()
    {
        var factory = new Factory();
        Assert.IsType<ForbidResult>(await Controller(factory, "operations").Get("academic", "term-groups"));
        Assert.Null(factory.RequestedSchool);
        Assert.IsType<BadRequestResult>(await Controller(factory, "academics").Get("academic", "term-groups", pageSize: 101));
        Assert.Null(factory.RequestedSchool);
        Assert.Equal(0, Data(await Controller(factory, "academics").Get("academic", "term-groups")).GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Period_scope_is_applied_before_count_and_pagination()
    {
        var factory = new Factory(); var termId = Guid.NewGuid();
        await using (var db = factory.Open())
        {
            for (var i = 0; i < 2; i++)
            {
                var term = new AcademicTerm { Id = i == 0 ? termId : Guid.NewGuid(), NameAr = "تيرم", NameEn = "Term" };
                for (var j = 0; j < 4; j++) db.AcademicPeriods.Add(new AcademicPeriod { Id = Guid.NewGuid(), AcademicTerm = term, SortOrder = j, IsDeleted = j == 3 });
            }
            await db.SaveChangesAsync();
        }
        var controller = Controller(factory, "academics");
        var page = Data(await controller.Get("academic", "periods", 2, 2, academicTermId: termId));
        Assert.Equal(3, page.GetProperty("totalCount").GetInt32());
        Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal(termId, page.GetProperty("items")[0].GetProperty("parentId").GetGuid());
        var trash = Data(await controller.Get("academic", "periods", deleted: true, academicTermId: termId));
        Assert.Equal(1, trash.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, Data(await controller.Get("academic", "periods", academicTermId: Guid.NewGuid())).GetProperty("totalCount").GetInt32());
    }

    [Theory]
    [InlineData(false, "2026-08-31", "2026-09-30", false)]
    [InlineData(false, "2026-09-01", "2027-01-02", false)]
    [InlineData(true, "2026-08-31", "2026-09-30", false)]
    [InlineData(true, "2026-09-01", "2027-01-02", false)]
    [InlineData(false, "2026-09-01", "2026-12-31", true)]
    [InlineData(true, "2026-09-01", "2026-12-31", true)]
    public async Task Period_create_and_update_enforce_term_date_boundaries(bool update, string start, string end, bool valid)
    {
        var factory = new Factory(); var termId = Guid.NewGuid(); var periodId = Guid.NewGuid();
        await using (var db = factory.Open())
        {
            db.AcademicTerms.Add(new AcademicTerm { Id = termId, IsActive = true, StartDate = new(2026,9,1), EndDate = new(2026,12,31) });
            if (update) db.AcademicPeriods.Add(new AcademicPeriod { Id = periodId, AcademicTermId = termId, Code = "OLD", StartDate = new(2026,9,1), EndDate = new(2026,9,30) });
            await db.SaveChangesAsync();
        }
        var identity = new ClaimsIdentity([new Claim(SchoolClaimTypes.SchoolCode, "PAGING-QA"), new Claim(SchoolClaimTypes.Permission, "school.academics.manage")], "test");
        var controller = new SchoolAcademicSettingsController(factory) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } } };
        var request = new SaveAcademicSettingRequest("P1", "فترة", "Period", ParentId: termId, StartDate: DateOnly.Parse(start), EndDate: DateOnly.Parse(end));
        var result = update ? await controller.Update("periods", periodId, request, default) : await controller.Create("periods", request, default);
        if (valid) Assert.True(result is OkObjectResult || result is ObjectResult { StatusCode: 201 });
        else
        {
            var failure = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("academic_settings.period_dates_outside_term", JsonSerializer.SerializeToElement(failure.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web)).GetProperty("code").GetString());
        }
        await using var saved = factory.Open();
        Assert.Equal(valid || update ? 1 : 0, await saved.AcademicPeriods.CountAsync());
        if (update && !valid) Assert.Equal("OLD", (await saved.AcademicPeriods.SingleAsync()).Code);
    }

    [Theory]
    [InlineData("grades",false)] [InlineData("offerings",false)] [InlineData("sections",false)] [InlineData("room-assignments",false)]
    [InlineData("grades",true)] [InlineData("offerings",true)] [InlineData("sections",true)] [InlineData("room-assignments",true)]
    public async Task Class_groups_page_parent_identities_and_filter_children_before_count(string kind,bool deleted)
    {
        var factory=new Factory();
        await using(var db=factory.Open())
        {
            for(var i=0;i<12;i++)
            {
                var program=new EducationProgram{Id=Guid.NewGuid(),Code=$"P{i:00}",NameAr="نظام",NameEn="Programme"};
                var stage=new EducationStage{Id=Guid.NewGuid(),Code=$"S{i:00}",NameAr="مرحلة مكررة",NameEn="Same stage",EducationProgram=program,IsDeleted=i==11};
                var year=new ProgramAcademicYear{Id=Guid.NewGuid(),Code=$"Y{i:00}",NameAr="عام مكرر",NameEn="Same year",EducationProgram=program,AcademicYearDefinition=new AcademicYearDefinition{Id=Guid.NewGuid(),NameAr="2026/27",NameEn="2026/27"},IsDeleted=i==11};
                var grade=new GradeLevel{Id=Guid.NewGuid(),Code=$"G{i:00}",NameAr="صف مكرر",NameEn="Same grade",EducationStage=stage};
                var offering=new GradeOffering{Id=Guid.NewGuid(),Code=$"O{i:00}",GradeLevel=grade,ProgramAcademicYear=year,IsDeleted=i==11};
                var section=new ClassSection{Id=Guid.NewGuid(),Code=$"C{i:00}",NameAr="فصل مكرر",NameEn="Same class",GradeOffering=offering,IsDeleted=i==11};
                for(var j=0;j<3;j++)
                {
                    var id=Guid.NewGuid();var code=$"CH{j}";
                    switch(kind)
                    {
                        case "grades": db.GradeLevels.Add(new GradeLevel{Id=id,Code=code,EducationStage=stage,IsDeleted=j==2});break;
                        case "offerings": db.GradeOfferings.Add(new GradeOffering{Id=id,Code=code,ProgramAcademicYear=year,GradeLevel=grade,IsDeleted=j==2});break;
                        case "sections": db.ClassSections.Add(new ClassSection{Id=id,Code=code,GradeOffering=offering,IsDeleted=j==2});break;
                        default: db.ClassRoomAssignments.Add(new ClassRoomAssignment{Id=id,ClassSection=section,Room=new SchoolRoom{Id=Guid.NewGuid(),Code="ROOM",NameAr="غرفة",NameEn="Room"},IsDeleted=j==2});break;
                    }
                }
            }
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var first=Data(await controller.Get("classes",kind+"-groups",1,10,deleted));
        var second=Data(await controller.Get("classes",kind+"-groups",2,10,deleted));
        Assert.Equal(12,first.GetProperty("totalCount").GetInt32());
        Assert.Equal(10,first.GetProperty("items").GetArrayLength());
        Assert.Equal(2,second.GetProperty("items").GetArrayLength());
        var parent=first.GetProperty("items")[0].GetProperty("id").GetGuid();
        Assert.All(first.GetProperty("items").EnumerateArray(),g=>Assert.Equal(deleted?1:2,g.GetProperty("itemCount").GetInt32()));
        Assert.Empty(first.GetProperty("items").EnumerateArray().Select(g=>g.GetProperty("id").GetGuid()).Intersect(second.GetProperty("items").EnumerateArray().Select(g=>g.GetProperty("id").GetGuid())));
        var child=Data(await controller.Get("classes",kind,2,1,deleted,parentId:parent));
        Assert.Equal(deleted?1:2,child.GetProperty("totalCount").GetInt32());
        Assert.Single(child.GetProperty("items").EnumerateArray());
        Assert.Equal(parent,child.GetProperty("items")[0].GetProperty("parentId").GetGuid());
        Assert.Equal(0,Data(await controller.Get("classes",kind,parentId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
        Assert.Equal(12,Data(await controller.Get("classes",kind+"-groups",999,10,deleted)).GetProperty("totalCount").GetInt32());
        Assert.Equal("PAGING-QA",factory.RequestedSchool);
    }

    [Theory]
    [InlineData("grades-groups")] [InlineData("offerings-groups")] [InlineData("sections-groups")] [InlineData("room-assignments-groups")]
    public async Task Class_groups_enforce_permissions_and_empty_windows(string kind)
    {
        var factory=new Factory();
        Assert.IsType<ForbidResult>(await Controller(factory,"facilities").Get("classes",kind));
        Assert.Null(factory.RequestedSchool);
        Assert.IsType<BadRequestResult>(await Controller(factory,"academics").Get("classes",kind,pageSize:101));
        Assert.Null(factory.RequestedSchool);
        Assert.Equal(0,Data(await Controller(factory,"academics").Get("classes",kind)).GetProperty("totalCount").GetInt32());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Grade_program_groups_and_stage_pages_keep_education_programs_separate(bool deleted)
    {
        var factory=new Factory();var programId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var i=0;i<2;i++)
            {
                var program=new EducationProgram{Id=i==0?programId:Guid.NewGuid(),Code=$"P{i}",NameAr="نظام"+i,NameEn="Programme"+i,IsDeleted=i==1};
                for(var j=0;j<(i==0?2:1);j++)
                {
                    var stage=new EducationStage{Id=Guid.NewGuid(),Code=$"S{i}{j}",NameAr="مرحلة متشابهة",NameEn="Same stage",EducationProgram=program};
                    db.GradeLevels.AddRange(new GradeLevel{Id=Guid.NewGuid(),EducationStage=stage},new GradeLevel{Id=Guid.NewGuid(),EducationStage=stage,IsDeleted=true});
                }
            }
            db.EducationPrograms.Add(new EducationProgram{Id=Guid.NewGuid(),Code="EMPTY",NameAr="دون صفوف",NameEn="No grades"});
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var programs=Data(await controller.Get("classes","grade-program-groups",1,1,deleted));
        Assert.Equal(2,programs.GetProperty("totalCount").GetInt32());
        var first=programs.GetProperty("items")[0];
        Assert.Equal(programId,first.GetProperty("id").GetGuid());
        Assert.Equal(2,first.GetProperty("itemCount").GetInt32());
        var stages=Data(await controller.Get("classes","grades-groups",2,1,deleted,educationProgramId:programId));
        Assert.Equal(2,stages.GetProperty("totalCount").GetInt32());
        Assert.Equal("Same stage",stages.GetProperty("items")[0].GetProperty("nameEn").GetString());
        var stageId=stages.GetProperty("items")[0].GetProperty("id").GetGuid();
        Assert.Equal(1,Data(await controller.Get("classes","grades",deleted:deleted,parentId:stageId)).GetProperty("totalCount").GetInt32());
        Assert.Equal(0,Data(await controller.Get("classes","grades-groups",educationProgramId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
    }

    [Theory]
    [InlineData("sections",false)] [InlineData("sections",true)]
    [InlineData("room-assignments",false)] [InlineData("room-assignments",true)]
    public async Task Nested_class_groups_scope_each_level_and_follow_leaf_deletion(string kind,bool deleted)
    {
        var factory=new Factory();var programId=Guid.NewGuid();var yearId=Guid.NewGuid();var offeringId=Guid.NewGuid();var sectionId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var p=0;p<2;p++)
            {
                var program=new EducationProgram{Id=p==0?programId:Guid.NewGuid(),Code=$"P{p}",NameAr="نظام",NameEn="Programme",IsDeleted=p==1};
                var stage=new EducationStage{Id=Guid.NewGuid(),Code="S",NameAr="مرحلة",NameEn="Stage",EducationProgram=program};
                var grade=new GradeLevel{Id=Guid.NewGuid(),Code="G",NameAr="صف",NameEn="Grade",EducationStage=stage};
                for(var y=0;y<2;y++)
                {
                    var year=new ProgramAcademicYear{Id=p==0&&y==0?yearId:Guid.NewGuid(),Code=$"Y{y}",EducationProgram=program,AcademicYearDefinition=new AcademicYearDefinition{Id=Guid.NewGuid(),NameAr=$"عام{y}",NameEn=$"Year{y}"}};
                    for(var g=0;g<2;g++)
                    {
                        var offering=new GradeOffering{Id=p==0&&y==0&&g==0?offeringId:Guid.NewGuid(),Code=$"O{g}",GradeLevel=grade,ProgramAcademicYear=year};
                        for(var s=0;s<2;s++)
                        {
                            var section=new ClassSection{Id=p==0&&y==0&&g==0&&s==0?sectionId:Guid.NewGuid(),Code=$"C{s}",NameAr="فصل",NameEn="Class",GradeOffering=offering,IsDeleted=kind=="sections"&&s==1};
                            db.ClassRoomAssignments.Add(new ClassRoomAssignment{Id=Guid.NewGuid(),ClassSection=section,IsDeleted=s==1,Room=new SchoolRoom{Id=Guid.NewGuid(),Code="R",NameAr="غرفة",NameEn="Room"}});
                        }
                    }
                }
            }
            // Empty ancestors must not appear; intermediate deletion state must not hide matching descendants.
            db.EducationPrograms.Add(new EducationProgram{Id=Guid.NewGuid(),Code="EMPTY"});
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var programs=Data(await controller.Get("classes",kind+"-program-groups",1,1,deleted));
        Assert.Equal(2,programs.GetProperty("totalCount").GetInt32());
        Assert.Equal(programId,programs.GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.Equal(2,programs.GetProperty("items")[0].GetProperty("itemCount").GetInt32());
        var years=Data(await controller.Get("classes",kind+"-year-groups",2,1,deleted,educationProgramId:programId));
        Assert.Equal(2,years.GetProperty("totalCount").GetInt32());
        Assert.Equal("Year1",years.GetProperty("items")[0].GetProperty("nameEn").GetString());
        Assert.Equal(0,Data(await controller.Get("classes",kind+"-year-groups",deleted:deleted,educationProgramId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
        var grades=Data(await controller.Get("classes",kind=="sections"?"sections-groups":"room-assignments-grade-groups",1,1,deleted,programAcademicYearId:yearId));
        Assert.Equal(2,grades.GetProperty("totalCount").GetInt32());
        Assert.Equal(offeringId,grades.GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.Equal("Grade",grades.GetProperty("items")[0].GetProperty("nameEn").GetString());
        if(kind=="room-assignments")
        {
            var sections=Data(await controller.Get("classes","room-assignments-groups",deleted:deleted,gradeOfferingId:offeringId));
            Assert.Single(sections.GetProperty("items").EnumerateArray());
            sectionId=sections.GetProperty("items")[0].GetProperty("id").GetGuid();
            Assert.Equal("Class",sections.GetProperty("items")[0].GetProperty("nameEn").GetString());
        }
        var leaves=Data(await controller.Get("classes",kind,deleted:deleted,parentId:kind=="sections"?offeringId:sectionId));
        Assert.Single(leaves.GetProperty("items").EnumerateArray());
        Assert.Equal(deleted,leaves.GetProperty("items")[0].GetProperty("isDeleted").GetBoolean());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Stage_groups_page_programs_and_only_return_the_selected_program_stages(bool deleted)
    {
        var factory=new Factory();var programId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var p=0;p<2;p++)
            {
                var program=new EducationProgram{Id=p==0?programId:Guid.NewGuid(),Code=$"P{p}"};
                db.EducationStages.AddRange(new EducationStage{Id=Guid.NewGuid(),EducationProgram=program,NameEn="Same stage"},new EducationStage{Id=Guid.NewGuid(),EducationProgram=program,NameEn="Same stage",IsDeleted=true});
            }
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var groups=Data(await controller.Get("classes","stages-groups",1,1,deleted));
        Assert.Equal(2,groups.GetProperty("totalCount").GetInt32());
        Assert.Equal(programId,groups.GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.Equal(1,groups.GetProperty("items")[0].GetProperty("itemCount").GetInt32());
        var stages=Data(await controller.Get("classes","stages",deleted:deleted,parentId:programId));
        Assert.Single(stages.GetProperty("items").EnumerateArray());
        Assert.Equal(programId,stages.GetProperty("items")[0].GetProperty("parentId").GetGuid());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Plan_subject_pages_filter_before_paging_and_keep_same_named_plans_separate(bool deleted)
    {
        var factory=new Factory();var planId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var p=0;p<2;p++)
            {
                var program=new EducationProgram{Id=Guid.NewGuid(),Code=$"P{p}"};
                var plan=new CurriculumPlan{Id=p==0?planId:Guid.NewGuid(),Code=$"PLAN{p}",NameAr="خطة متكررة",NameEn="Same plan",EducationProgram=program};
                var grade=new GradeLevel{Id=Guid.NewGuid(),NameAr="الصف الأول",NameEn="Grade 1",EducationStage=new EducationStage{Id=Guid.NewGuid(),EducationProgram=program}};
                var subject=new Subject{Id=Guid.NewGuid(),NameAr="رياضيات",NameEn="Math"};
                for(var i=0;i<4;i++)db.CurriculumGradeSubjects.Add(new CurriculumGradeSubject{Id=Guid.NewGuid(),CurriculumPlan=plan,GradeLevel=grade,Subject=subject,SortOrder=i,IsDeleted=i>=2});
            }
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var page=Data(await controller.Get("curriculum","grade-subjects",2,1,deleted,curriculumPlanId:planId));
        Assert.Equal(2,page.GetProperty("totalCount").GetInt32());
        var row=Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal(planId,row.GetProperty("parentId").GetGuid());
        Assert.Equal(deleted,row.GetProperty("isDeleted").GetBoolean());
        Assert.Equal("Grade 1 / Math",page.GetProperty("parents").GetProperty(row.GetProperty("id").GetString()!).GetProperty("en").GetString());
        Assert.Equal(4,Data(await controller.Get("curriculum","grade-subjects",deleted:deleted)).GetProperty("totalCount").GetInt32());
        Assert.Equal(0,Data(await controller.Get("curriculum","grade-subjects",deleted:deleted,curriculumPlanId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
        Assert.IsType<ForbidResult>(await Controller(factory,"facilities").Get("curriculum","grade-subjects",curriculumPlanId:planId));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Book_edition_pages_filter_before_paging_and_keep_same_named_books_separate(bool deleted)
    {
        var factory=new Factory();var bookId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var b=0;b<2;b++)
            {
                var book=new Book{Id=b==0?bookId:Guid.NewGuid(),Code=$"B{b}",NameAr="كتاب متكرر",NameEn="Same book"};
                for(var i=0;i<4;i++)db.BookVersions.Add(new BookVersion{Id=Guid.NewGuid(),Book=book,EditionCode=$"ED{i}",VersionLabel="2026",PublicationYear=2026,Language="ar",IsDeleted=i>=2});
            }
            db.Books.Add(new Book{Id=Guid.NewGuid(),Code="EMPTY",NameAr="كتاب جديد",NameEn="New book"});
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var page=Data(await controller.Get("curriculum","book-versions",2,1,deleted,bookId:bookId));
        Assert.Equal(2,page.GetProperty("totalCount").GetInt32());
        var row=Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal(bookId,row.GetProperty("parentId").GetGuid());
        Assert.Equal(deleted,row.GetProperty("isDeleted").GetBoolean());
        Assert.Equal(deleted?"ED3":"ED1",row.GetProperty("editionCode").GetString());
        Assert.Equal(4,Data(await controller.Get("curriculum","book-versions",deleted:deleted)).GetProperty("totalCount").GetInt32());
        Assert.Equal(0,Data(await controller.Get("curriculum","book-versions",deleted:deleted,bookId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
        Assert.Equal(3,Data(await controller.Get("curriculum","books")).GetProperty("totalCount").GetInt32());
        Assert.IsType<ForbidResult>(await Controller(factory,"facilities").Get("curriculum","book-versions",bookId:bookId));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Subject_offering_program_groups_page_programs_and_scope_rows_by_identity(bool deleted)
    {
        var factory=new Factory();var programId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            for(var p=0;p<2;p++)
            {
                var program=new EducationProgram{Id=p==0?programId:Guid.NewGuid(),Code=$"P{p}",NameAr="نظام",NameEn="Same programme",IsDeleted=p==1};
                var grade=new GradeLevel{Id=Guid.NewGuid(),NameEn="Grade 1",EducationStage=new EducationStage{Id=Guid.NewGuid(),EducationProgram=program}};
                var subject=new CurriculumGradeSubject{Id=Guid.NewGuid(),GradeLevel=grade,Subject=new Subject{Id=Guid.NewGuid(),NameEn="Math"},CurriculumPlan=new CurriculumPlan{Id=Guid.NewGuid(),EducationProgram=program}};
                for(var y=0;y<2;y++)
                {
                    var offering=new GradeOffering{Id=Guid.NewGuid(),GradeLevel=grade,ProgramAcademicYear=new ProgramAcademicYear{Id=Guid.NewGuid(),EducationProgram=program,AcademicYearDefinition=new AcademicYearDefinition{Id=Guid.NewGuid(),NameEn="2026/27"}}};
                    db.GradeSubjectOfferings.AddRange(new GradeSubjectOffering{Id=Guid.NewGuid(),GradeOffering=offering,CurriculumGradeSubject=subject},new GradeSubjectOffering{Id=Guid.NewGuid(),GradeOffering=offering,CurriculumGradeSubject=subject,IsDeleted=true});
                }
            }
            db.EducationPrograms.Add(new EducationProgram{Id=Guid.NewGuid(),Code="EMPTY"});
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var groups=Data(await controller.Get("curriculum","offering-program-groups",2,1,deleted));
        Assert.Equal(2,groups.GetProperty("totalCount").GetInt32());
        Assert.Equal(2,groups.GetProperty("items")[0].GetProperty("itemCount").GetInt32());
        Assert.NotEqual(programId,groups.GetProperty("items")[0].GetProperty("id").GetGuid());
        var page=Data(await controller.Get("curriculum","offerings",2,1,deleted,educationProgramId:programId));
        Assert.Equal(2,page.GetProperty("totalCount").GetInt32());
        var row=Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal(deleted,row.GetProperty("isDeleted").GetBoolean());
        Assert.Equal("2026/27 / Grade 1 / Math",page.GetProperty("parents").GetProperty(row.GetProperty("id").GetString()!).GetProperty("en").GetString());
        Assert.Equal(4,Data(await controller.Get("curriculum","offerings",deleted:deleted)).GetProperty("totalCount").GetInt32());
        Assert.Equal(0,Data(await controller.Get("curriculum","offerings",deleted:deleted,educationProgramId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
        var unauthorizedFactory=new Factory();
        Assert.IsType<ForbidResult>(await Controller(unauthorizedFactory,"facilities").Get("curriculum","offering-program-groups"));
        Assert.Null(unauthorizedFactory.RequestedSchool);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Facility_tree_keeps_ancestors_and_scopes_each_level_including_shared_capabilities(bool deleted)
    {
        var factory=new Factory();var branchId=Guid.NewGuid();var buildingId=Guid.NewGuid();var floorId=Guid.NewGuid();var roomId=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            var shared=new RoomCapability{Id=Guid.NewGuid(),Code="SHARED"};var old=new RoomCapability{Id=Guid.NewGuid(),Code="OLD",IsDeleted=true};
            for(var i=0;i<2;i++)
            {
                var branch=new SchoolBranch{Id=i==0?branchId:Guid.NewGuid(),Code=$"B{i}",NameEn="Same branch"};
                var building=new SchoolBuilding{Id=i==0?buildingId:Guid.NewGuid(),Code="BLD",Branch=branch};
                var floor=new BuildingFloor{Id=i==0?floorId:Guid.NewGuid(),Code="F",Building=building};
                var room=new SchoolRoom{Id=i==0?roomId:Guid.NewGuid(),Code="R",Floor=floor};
                room.Capabilities.Add(new SchoolRoomCapability{Room=room,Capability=shared});room.Capabilities.Add(new SchoolRoomCapability{Room=room,Capability=old});
                db.SchoolRooms.AddRange(room,new SchoolRoom{Id=Guid.NewGuid(),Code="TRASH",Floor=floor,IsDeleted=true});
            }
            db.SchoolBranches.Add(new SchoolBranch{Id=Guid.NewGuid(),Code="EMPTY"});await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"facilities");
        Assert.Equal(deleted?2:3,Data(await controller.Get("facilities","tree-branches",deleted:deleted)).GetProperty("totalCount").GetInt32());
        Assert.Equal(buildingId,Data(await controller.Get("facilities","tree-buildings",deleted:deleted,parentId:branchId)).GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.Equal(floorId,Data(await controller.Get("facilities","tree-floors",deleted:deleted,parentId:buildingId)).GetProperty("items")[0].GetProperty("id").GetGuid());
        var rooms=Data(await controller.Get("facilities","tree-rooms",1,1,deleted,parentId:floorId));
        Assert.Equal(deleted?2:1,rooms.GetProperty("totalCount").GetInt32());
        Assert.Single(rooms.GetProperty("items").EnumerateArray());
        Assert.Equal(floorId,rooms.GetProperty("items")[0].GetProperty("parentId").GetGuid());
        Assert.Equal(1,Data(await controller.Get("facilities","room-capabilities",deleted:deleted,parentId:roomId)).GetProperty("totalCount").GetInt32());
        Assert.Equal(0,Data(await controller.Get("facilities","tree-rooms",deleted:deleted,parentId:Guid.NewGuid())).GetProperty("totalCount").GetInt32());
        Assert.Equal(0,Data(await controller.Get("facilities","tree-buildings",deleted:deleted)).GetProperty("totalCount").GetInt32());
        Assert.IsType<ForbidResult>(await Controller(factory,"academics").Get("facilities","tree-branches"));
    }

    [Fact]
    public async Task Room_capability_links_preserve_room_data_and_other_rooms_and_reject_unavailable_new_definitions()
    {
        var factory=new Factory();var roomId=Guid.NewGuid();var otherRoomId=Guid.NewGuid();var retained=Guid.NewGuid();var added=Guid.NewGuid();var inactive=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            db.RoomCapabilities.AddRange(new RoomCapability{Id=retained,Code="OLD",IsActive=false},new RoomCapability{Id=added,Code="NEW"},new RoomCapability{Id=inactive,Code="OFF",IsActive=false});
            db.SchoolRooms.AddRange(new SchoolRoom{Id=roomId,Code="ROOM",Capacity=37,IsSchedulable=false},new SchoolRoom{Id=otherRoomId,Code="OTHER"});
            db.SchoolRoomCapabilities.AddRange(new SchoolRoomCapability{RoomId=roomId,CapabilityId=retained},new SchoolRoomCapability{RoomId=otherRoomId,CapabilityId=retained});await db.SaveChangesAsync();
        }
        var controller=new SchoolFacilitiesController(factory){ControllerContext=Controller(factory,"facilities").ControllerContext};
        Assert.IsType<BadRequestObjectResult>(await controller.SetRoomCapabilities(roomId,new([inactive]),default));
        Assert.IsType<BadRequestObjectResult>(await controller.SetRoomCapabilities(roomId,new(null),default));
        Assert.IsType<OkObjectResult>(await controller.SetRoomCapabilities(roomId,new([retained,added,added]),default));
        Assert.IsType<OkObjectResult>(await controller.SetRoomCapabilities(roomId,new([added]),default));
        await using var saved=factory.Open();
        Assert.Equal(added,(await saved.SchoolRoomCapabilities.SingleAsync(x=>x.RoomId==roomId)).CapabilityId);
        Assert.Equal(retained,(await saved.SchoolRoomCapabilities.SingleAsync(x=>x.RoomId==otherRoomId)).CapabilityId);
        Assert.Equal(37,(await saved.SchoolRooms.SingleAsync(x=>x.Id==roomId)).Capacity);
        Assert.False((await saved.SchoolRooms.SingleAsync(x=>x.Id==roomId)).IsSchedulable);
        Assert.Equal(3,await saved.RoomCapabilities.CountAsync());
        var policy=Assert.Single(typeof(SchoolFacilitiesController).GetMethod(nameof(SchoolFacilitiesController.SetRoomCapabilities))!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),false).Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Equal(SchoolPermissionPolicies.FacilitiesManage,policy.Policy);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Capability_creation_can_link_to_room_in_one_save_and_invalid_room_leaves_no_definition(bool valid)
    {
        var factory=new Factory();var roomId=Guid.NewGuid();
        await using(var db=factory.Open()){db.SchoolRooms.Add(new SchoolRoom{Id=roomId,Code="ROOM",IsActive=valid});await db.SaveChangesAsync();}
        var controller=new SchoolFacilitiesController(factory){ControllerContext=Controller(factory,"facilities").ControllerContext};
        var result=await controller.Create("capabilities",new("PROJECTOR","بروجكتور","Projector",ParentId:roomId),default);
        if(valid)Assert.Equal(201,Assert.IsType<ObjectResult>(result).StatusCode);else Assert.IsType<BadRequestObjectResult>(result);
        await using var saved=factory.Open();Assert.Equal(valid?1:0,await saved.RoomCapabilities.CountAsync());Assert.Equal(valid?1:0,await saved.SchoolRoomCapabilities.CountAsync());
    }

    private sealed class Factory : ISchoolDbContextFactory
    {
        private readonly DbContextOptions<SchoolsDbContext> options = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public string? RequestedSchool { get; private set; }
        public Task<SchoolLoginTarget?> ResolveAsync(string code,CancellationToken ct=default)=>Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string code,CancellationToken ct=default){RequestedSchool=code;return Task.FromResult<SchoolsDbContext?>(new SchoolsDbContext(options));}
        public SchoolsDbContext Open()=>new(options);
    }
    private static SchoolSettingsListsController Controller(Factory factory,params string[] modules)
    {
        var claims=modules.Select(m=>new Claim(SchoolClaimTypes.Permission,$"school.{m}.view")).Append(new Claim(SchoolClaimTypes.SchoolCode,"PAGING-QA"));
        return new(factory){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity(claims,"test"))}}};
    }
    private static JsonElement Data(IActionResult result)
    {
        var value=Assert.IsType<OkObjectResult>(result).Value;
        return JsonSerializer.SerializeToElement(value,new JsonSerializerOptions(JsonSerializerDefaults.Web)).GetProperty("data");
    }
    [Fact]
    public async Task Pages_are_disjoint_ordered_and_deleted_rows_have_their_own_total()
    {
        var factory=new Factory();
        await using(var db=factory.Open())
        {
            for(var i=12;i>=0;i--)db.Subjects.Add(new Subject{Id=Guid.NewGuid(),Code=$"S{i:00}",NameAr="مادة",NameEn="Subject",IsDeleted=i>=11});
            await db.SaveChangesAsync();
        }
        var controller=Controller(factory,"academics");
        var first=Data(await controller.Get("curriculum","subjects",1,10));
        var second=Data(await controller.Get("curriculum","subjects",2,10));
        Assert.Equal(11,first.GetProperty("totalCount").GetInt32());
        Assert.Equal(10,first.GetProperty("items").GetArrayLength());
        Assert.Single(second.GetProperty("items").EnumerateArray());
        Assert.Equal("S00",first.GetProperty("items")[0].GetProperty("code").GetString());
        Assert.Equal("S10",second.GetProperty("items")[0].GetProperty("code").GetString());
        Assert.Equal("PAGING-QA",factory.RequestedSchool);
        var trash=Data(await controller.Get("curriculum","subjects",1,10,true));
        Assert.Equal(2,trash.GetProperty("totalCount").GetInt32());
        var beyond=Data(await controller.Get("curriculum","subjects",999,10));
        Assert.Equal(2,beyond.GetProperty("pageNumber").GetInt32());
    }
    [Fact]
    public async Task Parent_labels_survive_soft_deleted_parent_without_including_other_lists()
    {
        var factory=new Factory();var id=Guid.NewGuid();
        await using(var db=factory.Open())
        {
            db.SchoolBuildings.Add(new SchoolBuilding{Id=id,Code="B",NameAr="مبنى",NameEn="Building",Branch=new SchoolBranch{Id=Guid.NewGuid(),Code="OLD",NameAr="فرع قديم",NameEn="Old branch",IsDeleted=true}});
            await db.SaveChangesAsync();
        }
        var page=Data(await Controller(factory,"facilities").Get("facilities","buildings"));
        Assert.Equal("Old branch",page.GetProperty("parents").GetProperty(id.ToString()).GetProperty("en").GetString());
        Assert.False(page.TryGetProperty("branches",out _));
    }
    [Theory]
    [InlineData("academic","schedules","academics")]
    [InlineData("academic","calendar-events","operations")]
    [InlineData("curriculum","books","facilities")]
    public async Task Permission_for_each_area_is_enforced_before_resolving_school(string area,string kind,string wrongPermission)
    {
        var factory=new Factory();Assert.IsType<ForbidResult>(await Controller(factory,wrongPermission).Get(area,kind));Assert.Null(factory.RequestedSchool);
    }
    [Theory]
    [InlineData(0,20)] [InlineData(1,0)] [InlineData(1,101)] [InlineData(int.MaxValue,20)]
    public async Task Invalid_windows_are_rejected_without_opening_database(int page,int size)
    {
        var factory=new Factory();Assert.IsType<BadRequestResult>(await Controller(factory,"academics").Get("curriculum","subjects",page,size));Assert.Null(factory.RequestedSchool);
    }
    [Theory]
    [InlineData("facilities","branches")] [InlineData("facilities","buildings")] [InlineData("facilities","floors")] [InlineData("facilities","rooms")] [InlineData("facilities","room-types")] [InlineData("facilities","capabilities")]
    [InlineData("academic","programs")] [InlineData("academic","year-definitions")] [InlineData("academic","program-years")] [InlineData("academic","terms")] [InlineData("academic","periods")] [InlineData("academic","schedules")] [InlineData("academic","calendar-events")]
    [InlineData("classes","stages")] [InlineData("classes","tracks")] [InlineData("classes","grades")] [InlineData("classes","offerings")] [InlineData("classes","sections")] [InlineData("classes","room-assignments")]
    [InlineData("curriculum","subjects")] [InlineData("curriculum","plans")] [InlineData("curriculum","grade-subjects")] [InlineData("curriculum","books")] [InlineData("curriculum","book-versions")] [InlineData("curriculum","book-roles")] [InlineData("curriculum","subject-books")] [InlineData("curriculum","offerings")]
    public async Task Every_catalog_supports_an_empty_page(string area,string kind)
    {
        var page=Data(await Controller(new Factory(),"academics","operations","calendar","facilities").Get(area,kind));
        Assert.Equal(0,page.GetProperty("items").GetArrayLength());Assert.Equal(1,page.GetProperty("pageNumber").GetInt32());
    }
}
