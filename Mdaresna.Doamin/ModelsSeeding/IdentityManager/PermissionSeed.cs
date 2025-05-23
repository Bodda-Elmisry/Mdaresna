﻿using Mdaresna.Doamin.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.IdentityManager
{
    public class PermissionSeed : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasData(
                new Permission
                {
                    Id = Guid.Parse("0225DBD5-9675-438C-87F2-63FB6921841C"),
                    Key = "AssignScoolManagerToUser",
                    Name = "Assign Scool Manager To User",
                    Description = "Assign School Manager Permission To User",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E"),
                    Key = "CreateSchool",
                    Name = "Add School",
                    Description = "Create new shool",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B15E32EB-092E-437D-9D4E-B9ED583C23B0"),
                    Key = "ViewUserSchoolsDropDown",
                    Name = "View User Schools Drop Down",
                    Description = "View User Schools Drop Down",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("97AAD235-16FA-496B-88D2-ADCEEFBD8D5C"),
                    Key = "schoolAdmin",
                    Name = "school Admin",
                    Description = "school Admin",
                    SchoolPermission = true,
                    AppPermission = false
                },

                new Permission
                {
                    Id = Guid.Parse("8AD5E47C-5EC4-49C7-A0AB-0D37E576961F"),
                    Key = "ViewYearSettingSection",
                    Name = "View Year Setting Section",
                    Description = "View Year Setting Section",
                    SchoolPermission = true,
                    AppPermission = false
                },

                new Permission
                {
                    Id = Guid.Parse("1DFBDB17-BA97-4539-B887-E81FC0E72B47"),
                    Key = "ViewSchoolSettingSection",
                    Name = "View School Setting Section",
                    Description = "View School Setting Section",
                    SchoolPermission = true,
                    AppPermission = false
                },

                new Permission
                {
                    Id = Guid.Parse("893E8A43-0DA7-4149-ABDB-E2469239896F"),
                    Key = "ViewClassesSettingSection",
                    Name = "View Classes Setting Section",
                    Description = "View Classes Setting Section",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("4838711A-4139-465E-A34F-A4B6756AE475"),
                    Key = "ViewYearsSetting",
                    Name = "View Years Setting",
                    Description = "View Years Setting",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("072C7A82-B62D-45D8-A3ED-48A70319ABF3"),
                    Key = "ChangeSchoolActivation",
                    Name = "Change School Activation",
                    Description = "Change School Activation",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("FDEBAADE-4D2C-447B-BA52-11E3E2A622F4"),
                    Key = "FilterSchoolsByActivity",
                    Name = "Filter Schools By Activity",
                    Description = "Filter Schools By Activity",
                    SchoolPermission = true,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("8A48610D-C9FF-4FCE-95EB-3A4D8D633A9C"),
                    Key = "AssignCoinsToSchool",
                    Name = "Assign Coins To School",
                    Description = "Assign Coins To School",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("122D8E7A-F5A7-48EA-B2A6-F86964A2C3E7"),
                    Key = "ChangeSchoolType",
                    Name = "Change School Type",
                    Description = "Change School Type",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("76BEFB0E-5EF7-4FE3-8E06-02D50C17C38B"),
                    Key = "ChangeSchoolCoinType",
                    Name = "Change School Coin Type",
                    Description = "Change School Coin Type",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("E50A5830-E741-4260-820C-19C2CAB1B419"),
                    Key = "ViewSchoolData",
                    Name = "View School Data",
                    Description = "View School Data",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("21040E37-D949-42EA-9A77-96AED0289209"),
                    Key = "UpdateSchoolData",
                    Name = "Update School Data",
                    Description = "Update School Data",
                    SchoolPermission = true,
                    AppPermission =false
                },
                new Permission
                {
                    Id = Guid.Parse("B7F4D3F3-B0B9-4E2F-8884-E2D61E0B6F5E"),
                    Key = "UploadeSchoolImage",
                    Name = "Uploade School Image",
                    Description = "Uploade School Image",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("26DE4395-48FB-4684-B88D-5C9E6081F9AC"),
                    Key = "AddSchoolContact",
                    Name = "Add School Contact",
                    Description = "Add School Contact",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("16983650-564E-4331-97D2-C1B5D67FEF40"),
                    Key = "UpdateSchoolContact",
                    Name = "Update School Contact",
                    Description = "Update School Contact",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D512C1B3-DDE0-4646-98CC-2BE5C56C7150"),
                    Key = "ViewSchoolLanguagesSetting",
                    Name = "View School Languages Setting",
                    Description = "View Shool Languages Setting",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("BE86C901-37F9-4F63-B7B3-03653E75FEA5"),
                    Key = "AddLanguageToSchool",
                    Name = "Add Language To School",
                    Description = "Add Language To School",
                    SchoolPermission = true,
                    AppPermission =false
                },
                new Permission
                {
                    Id = Guid.Parse("71B6F158-82F2-4A48-9950-56DBB00EEC4E"),
                    Key = "ViewLanguages",
                    Name = "View Languages",
                    Description = "View Languages",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("68713DF9-ED1B-449C-8060-D919A2592B02"),
                    Key = "CreateLanguage",
                    Name = "Create Language",
                    Description = "Create Language",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("75162229-2536-4232-B081-FEABE20C318D"),
                    Key = "ViewGradesSetting",
                    Name = "View Grades Setting",
                    Description = "View Grades Setting",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("5C8D7610-BFAA-4DEB-BD84-E54ED4351611"),
                    Key = "AddGrade",
                    Name = "Add Grade",
                    Description = "Add Grade",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("0D9BFAD2-0762-47BA-8046-3AA92B2D6222"),
                    Key = "AddMultipleGrades",
                    Name = "Add Multiple Grades",
                    Description = "Add Multiple Grades",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("A1134102-7021-4770-8808-FDC376190691"),
                    Key = "UpdateGrade",
                    Name = "Update Grade",
                    Description = "Update Grade",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("494D9C56-558A-427C-9854-878520FCDEC8"),
                    Key = "ViewExamsSetting",
                    Name = "View Exams Setting",
                    Description = "View Exams Setting",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("861AD498-A214-4B7D-BDE7-815ABF63A587"),
                    Key = "AddExam",
                    Name = "Add Exam",
                    Description = "Add Exam",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("062FF041-C775-4B30-8A49-202DB7D9DD28"),
                    Key = "AddMultipleExams",
                    Name = "Add Multiple Exams",
                    Description = "Add Multiple Exams",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E5B8454F-A193-4D63-B791-3A02AA9D71D3"),
                    Key = "ViewClassesSettingSection",
                    Name = "View Classes Setting Section",
                    Description = "View Classes Setting Section",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("056ED8BB-3FFC-459F-831E-C8A462DB313B"),
                    Key = "ViewClassesSettingList",
                    Name = "View Classes Setting List",
                    Description = "View Classes Setting List",
                    SchoolPermission = true,
                    AppPermission =false
                },
                new Permission
                {
                    Id = Guid.Parse("CB37BBD0-40AD-4CA8-AE19-1ED87FEA0B97"),
                    Key = "AddClass",
                    Name = "Add Class",
                    Description = "Add Class",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("84956C26-E46F-432A-9B44-C8FB17E95552"),
                    Key = "AddMultipleClasses",
                    Name = "Add Multiple Classes",
                    Description = "Add Multiple Classes",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("F19CCD22-35AC-4778-8A7F-FB0E56822385"),
                    Key = "ChangeClassActivation",
                    Name = "Change Class Activation",
                    Description = "Change Class Activation",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("7A382B52-95F3-442F-ABFF-9C1C5C9607CC"),
                    Key = "UpdateClass",
                    Name = "Update Class",
                    Description = "Update Class",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B6858FE8-DCC3-4C0F-B1A9-6A8AB7331924"),
                    Key = "LinkTeacherToSchool",
                    Name = "Link Teacher To School",
                    Description = "Link Teacher To School",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("DD0F56AD-6039-4A8B-9D26-EBD5ABE87E7D"),
                    Key = "AddCourse",
                    Name = "Add Course",
                    Description = "Add Course",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("6ED86695-9EA7-4820-BF89-0DE16DFD6EA9"),
                    Key = "AddMultipleCourses",
                    Name = "Add Multiple Courses",
                    Description = "Add Multiple Courses",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("A4A93CEB-21F9-4400-9444-0D94CF895BEB"),
                    Key = "UpdateCourse",
                    Name = "Update Course",
                    Description = "Update Course",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("7B4478C8-2526-493A-A34D-9FFFA5786F85"),
                    Key = "ViewTeachersList",
                    Name = "View Teachers List",
                    Description = "View Teachers List",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("6DE52188-1C80-46E3-A436-FF36469E2976"),
                    Key = "ViewAdminSettings",
                    Name = "View Admin Aettings",
                    Description = "View Admin Aettings",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("FE286E0A-193B-4F12-AE88-DF5808FB2EB7"),
                    Key = "UnlinkTeacherToSchool",
                    Name = "Unlink Teacher To School",
                    Description = "Unlink Teacher To School",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("98D37ADE-FDC0-4A0B-8E00-402A708C9DDD"),
                    Key = "LinkTeacherToCourse&Class",
                    Name = "Link Teacher To Course & Class",
                    Description = "Link Teacher To Course & Class",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("869A6521-2730-4F0B-973B-60FB8093C769"),
                    Key = "UnlinkTeacherToCourse&Class",
                    Name = "Unlink Teacher To Course & Class",
                    Description = "Unlink Teacher To Course & Class",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("EE6325A4-4BC4-4AA7-9944-F82C3B7A305A"),
                    Key = "ViewSchoolStudentsList",
                    Name = "View School Students List",
                    Description = "View School Students List",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("AD53E402-C2AB-4941-861B-A19C82DCA0CC"),
                    Key = "AssignStudentToSchool",
                    Name = "Assign Student To School",
                    Description = "Assign Student To School",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B65324A3-35B9-4579-8C85-C9938391DFA6"),
                    Key = "AssignMultipleStudentsToSchool",
                    Name = "Assign Multiple Students To School",
                    Description = "Assign Multiple Students To School",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("2F1290B1-F439-4810-BA25-5C3E9CC56EFE"),
                    Key = "UpdateStudentInfo",
                    Name = "Update Student Info",
                    Description = "Update Student Info",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("9DD22E15-9701-492B-AE20-985B8927F3BF"),
                    Key = "ChangeStudentActivation",
                    Name = "Change Student Activation",
                    Description = "Change Student Activation",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("CC4A8F98-FBB7-49DD-8B5F-CC6D02BCCAB0"),
                    Key = "ViewSchoolCurrentYear",
                    Name = "View School Current Year",
                    Description = "View School Current Year",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("C50D6574-648B-4F6E-8ED5-D27F8B5A34F1"),
                    Key = "AddCurrentYear",
                    Name = "Add Current Year",
                    Description = "Add Current Year",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("32D821BB-0C50-4721-9034-097019632C05"),
                    Key = "AddMonth",
                    Name = "Add Month",
                    Description = "Add Month",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("69A0778B-A7A8-4E47-B8FD-D061428DBB95"),
                    Key = "ViewYearMonthes",
                    Name = "View Year Monthes",
                    Description = "View Year Monthes",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D9B2C724-C5CC-497A-A11B-3E48B7DBFD97"),
                    Key = "UpdateYear",
                    Name = "Update Year",
                    Description = "Update Year",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("93FD0AA6-852F-4AFE-8CF4-9CDEB365D498"),
                    Key = "UpdateMonth",
                    Name = "Update Month",
                    Description = "Update Month",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B6D6C7E2-E2DE-4B95-9789-A61C69F27765"),
                    Key = "ChangeYearActivation",
                    Name = "Change Year Activation",
                    Description = "Change Year Activation",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("11F0AA92-E7AF-4F94-BD50-96F389DEEE2A"),
                    Key = "ChangeMonthActivation",
                    Name = "Change Month Activation",
                    Description = "Change Month Activation",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("ECD2236F-6FA1-43EC-92E9-AA36AE2358B4"),
                    Key = "CompleteYeare",
                    Name = "Complete Yeare",
                    Description = "Complete Yeare",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E831E41D-1090-4AD0-BBAF-E4CA823ECD9D"),
                    Key = "ViewYearsList",
                    Name = "View Years List",
                    Description = "View Years List",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("219007EA-620E-4D96-8292-2D015EF68DB1"),
                    Key = "ViewChildernsList",
                    Name = "View Childerns",
                    Description = "View Childerns List",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("3B49FC12-D345-41C4-B5D0-97A388AAAFA3"),
                    Key = "AttendanceAction",
                    Name = "Attendance Action",
                    Description = "Attendance Action",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("DE53CF00-E53D-4BE0-8F67-71F8C7248DF4"),
                    Key = "ViewAttendanceList",
                    Name = "View Attendance List",
                    Description = "View Attendance List",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("30AA5979-36F6-4A63-98D2-A06F96178176"),
                    Key = "AddHomework",
                    Name = "Add Homework",
                    Description = "Add Homework",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("BAA954FD-8BA9-4529-834D-E640057998AF"),
                    Key = "ViewHomework",
                    Name = "View Homework",
                    Description = "View Homework",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("65B57F13-161F-43B2-9E8D-DAAFB50FBBBB"),
                    Key = "UpdateHomework",
                    Name = "Update Homework",
                    Description = "Update Homework",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("C1856505-7C6B-4932-8034-CF4FBC0EFB67"),
                    Key = "RateHomework",
                    Name = "Rate Homework",
                    Description = "Rate Homework",
                    SchoolPermission = true,
                    AppPermission =false
                },
                new Permission
                {
                    Id = Guid.Parse("09E73F16-BB7A-403B-A8F9-FDC1C3228B0D"),
                    Key = "ViewSchoolSettings",
                    Name = "View School Settings",
                    Description = "View School Settings",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("FBA69A66-4E31-44F2-93AC-9602705A7F98"),
                    Key = "ViewExams",
                    Name = "View Exams",
                    Description = "View Exams",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("77127635-8FE8-4451-AC9C-59C38EE02D4D"),
                    Key = "UpdateExam",
                    Name = "Update Exam",
                    Description = "Update Exam",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("EBD73C8F-2EF9-4A67-99E6-BAFE71C9E83C"),
                    Key = "RateExam",
                    Name = "Rate Exam",
                    Description = "Rate Exam",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("49869E47-2209-4FD8-AA08-2E99BA211FF1"),
                    Key = "ViewActivities",
                    Name = "View Activities",
                    Description = "View Activities",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E09A3A9F-0FF1-46B7-8C80-B7F07CCB4542"),
                    Key = "AddActivity",
                    Name = "Add Activity",
                    Description = "Add Activity",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("A992E74B-4CE4-4D18-BCC5-C079D116D1F5"),
                    Key = "UpdateActivity",
                    Name = "Update Activity",
                    Description = "Update Activity",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("99FDFBA4-EF5B-44D1-B9BF-B4FAF68A7B60"),
                    Key = "RateActivity",
                    Name = "Rate Activity",
                    Description = "Rate Activity",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("EEC01FE0-4399-44B0-B8E7-AFD417A6A93C"),
                    Key = "AddNote",
                    Name = "Add Note",
                    Description = "Add Note",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D9BDCF84-D193-4775-8F9B-BACB1BD22915"),
                    Key = "ViewNotes",
                    Name = "View Notes",
                    Description = "View Notes",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B7BCF498-F7E9-48B4-9E99-49F6D0A9A956"),
                    Key = "EditNote",
                    Name = "Edit Note",
                    Description = "Edit Note",
                    SchoolPermission = true,
                    AppPermission = false
                }
                );
        }
    }
}
