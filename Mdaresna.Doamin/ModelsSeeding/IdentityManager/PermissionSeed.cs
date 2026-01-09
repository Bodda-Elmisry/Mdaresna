using Mdaresna.Doamin.Models.Identity;
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
                    Name_AR = "تعيين مدير المدرسة للمستخدم",
                    Description = "Assign School Manager Permission To User",
                    Description_AR = "تعيين صلاحية مدير المدرسة للمستخدم",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E"),
                    Key = "CreateSchool",
                    Name = "Add School",
                    Name_AR = "إضافة مدرسة",
                    Description = "Create new shool",
                    Description_AR = "إنشاء مدرسة جديدة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B15E32EB-092E-437D-9D4E-B9ED583C23B0"),
                    Key = "ViewUserSchoolsDropDown",
                    Name = "View User Schools Drop Down",
                    Name_AR = "عرض قائمة المدارس للمستخدم",
                    Description = "View User Schools Drop Down",
                    Description_AR = "عرض قائمة المدارس للمستخدم",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("97AAD235-16FA-496B-88D2-ADCEEFBD8D5C"),
                    Key = "schoolAdmin",
                    Name = "school Admin",
                    Name_AR = "مدير المدرسة",
                    Description = "school Admin",
                    Description_AR = "مدير المدرسة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },

                new Permission
                {
                    Id = Guid.Parse("8AD5E47C-5EC4-49C7-A0AB-0D37E576961F"),
                    Key = "ViewYearSettingSection",
                    Name = "View Year Setting Section",
                    Name_AR = "عرض قسم إعدادات السنة",
                    Description = "View Year Setting Section",
                    Description_AR = "عرض قسم إعدادات السنة",
                    SchoolPermission = true,
                    AppPermission = false
                },

                new Permission
                {
                    Id = Guid.Parse("1DFBDB17-BA97-4539-B887-E81FC0E72B47"),
                    Key = "ViewSchoolSettingSection",
                    Name = "View School Setting Section",
                    Name_AR = "عرض قسم إعدادات المدرسة",
                    Description = "View School Setting Section",
                    Description_AR = "عرض قسم إعدادات المدرسة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },

                new Permission
                {
                    Id = Guid.Parse("893E8A43-0DA7-4149-ABDB-E2469239896F"),
                    Key = "ViewClassesSettingSection",
                    Name = "View Classes Setting Section",
                    Name_AR = "عرض قسم إعدادات الفصول",
                    Description = "View Classes Setting Section",
                    Description_AR = "عرض قسم إعدادات الفصول",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("4838711A-4139-465E-A34F-A4B6756AE475"),
                    Key = "ViewYearsSetting",
                    Name = "View Years Setting",
                    Name_AR = "عرض إعدادات السنوات",
                    Description = "View Years Setting",
                    Description_AR = "عرض إعدادات السنوات",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("072C7A82-B62D-45D8-A3ED-48A70319ABF3"),
                    Key = "ChangeSchoolActivation",
                    Name = "Change School Activation",
                    Name_AR = "تغيير تفعيل المدرسة",
                    Description = "Change School Activation",
                    Description_AR = "تغيير تفعيل المدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("FDEBAADE-4D2C-447B-BA52-11E3E2A622F4"),
                    Key = "FilterSchoolsByActivity",
                    Name = "Filter Schools By Activity",
                    Name_AR = "تصفية المدارس حسب النشاط",
                    Description = "Filter Schools By Activity",
                    Description_AR = "تصفية المدارس حسب النشاط",
                    SchoolPermission = true,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("8A48610D-C9FF-4FCE-95EB-3A4D8D633A9C"),
                    Key = "AssignCoinsToSchool",
                    Name = "Assign Coins To School",
                    Name_AR = "تعيين العملات للمدرسة",
                    Description = "Assign Coins To School",
                    Description_AR = "تعيين العملات للمدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("122D8E7A-F5A7-48EA-B2A6-F86964A2C3E7"),
                    Key = "ChangeSchoolType",
                    Name = "Change School Type",
                    Name_AR = "تغيير نوع المدرسة",
                    Description = "Change School Type",
                    Description_AR = "تغيير نوع المدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("76BEFB0E-5EF7-4FE3-8E06-02D50C17C38B"),
                    Key = "ChangeSchoolCoinType",
                    Name = "Change School Coin Type",
                    Name_AR = "تغيير نوع عملة المدرسة",
                    Description = "Change School Coin Type",
                    Description_AR = "تغيير نوع عملة المدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("E50A5830-E741-4260-820C-19C2CAB1B419"),
                    Key = "ViewSchoolData",
                    Name = "View School Data",
                    Name_AR = "عرض بيانات المدرسة",
                    Description = "View School Data",
                    Description_AR = "عرض بيانات المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("21040E37-D949-42EA-9A77-96AED0289209"),
                    Key = "UpdateSchoolData",
                    Name = "Update School Data",
                    Name_AR = "تحديث بيانات المدرسة",
                    Description = "Update School Data",
                    Description_AR = "تحديث بيانات المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B7F4D3F3-B0B9-4E2F-8884-E2D61E0B6F5E"),
                    Key = "UploadeSchoolImage",
                    Name = "Uploade School Image",
                    Name_AR = "رفع صورة المدرسة",
                    Description = "Uploade School Image",
                    Description_AR = "رفع صورة المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("26DE4395-48FB-4684-B88D-5C9E6081F9AC"),
                    Key = "AddSchoolContact",
                    Name = "Add School Contact",
                    Name_AR = "إضافة جهة اتصال للمدرسة",
                    Description = "Add School Contact",
                    Description_AR = "إضافة جهة اتصال للمدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("16983650-564E-4331-97D2-C1B5D67FEF40"),
                    Key = "UpdateSchoolContact",
                    Name = "Update School Contact",
                    Name_AR = "تحديث جهة اتصال المدرسة",
                    Description = "Update School Contact",
                    Description_AR = "تحديث جهة اتصال المدرسة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("D512C1B3-DDE0-4646-98CC-2BE5C56C7150"),
                    Key = "ViewSchoolLanguagesSetting",
                    Name = "View School Languages Setting",
                    Name_AR = "عرض إعدادات لغات المدرسة",
                    Description = "View Shool Languages Setting",
                    Description_AR = "عرض إعدادات لغات المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("BE86C901-37F9-4F63-B7B3-03653E75FEA5"),
                    Key = "AddLanguageToSchool",
                    Name = "Add Language To School",
                    Name_AR = "إضافة لغة للمدرسة",
                    Description = "Add Language To School",
                    Description_AR = "إضافة لغة للمدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("71B6F158-82F2-4A48-9950-56DBB00EEC4E"),
                    Key = "ViewLanguages",
                    Name = "View Languages",
                    Name_AR = "عرض اللغات",
                    Description = "View Languages",
                    Description_AR = "عرض اللغات",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("68713DF9-ED1B-449C-8060-D919A2592B02"),
                    Key = "CreateLanguage",
                    Name = "Create Language",
                    Name_AR = "إنشاء لغة",
                    Description = "Create Language",
                    Description_AR = "إنشاء لغة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("75162229-2536-4232-B081-FEABE20C318D"),
                    Key = "ViewGradesSetting",
                    Name = "View Grades Setting",
                    Name_AR = "عرض إعدادات المستويات الدراسية",
                    Description = "View Grades Setting",
                    Description_AR = "عرض إعدادات المستويات الدراسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("5C8D7610-BFAA-4DEB-BD84-E54ED4351611"),
                    Key = "AddGrade",
                    Name = "Add Grade",
                    Name_AR = "إضافة مستوى دراسي",
                    Description = "Add Grade",
                    Description_AR = "إضافة مستوى دراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("0D9BFAD2-0762-47BA-8046-3AA92B2D6222"),
                    Key = "AddMultipleGrades",
                    Name = "Add Multiple Grades",
                    Name_AR = "إضافة مستويات دراسية متعددة",
                    Description = "Add Multiple Grades",
                    Description_AR = "إضافة مستويات دراسية متعددة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("A1134102-7021-4770-8808-FDC376190691"),
                    Key = "UpdateGrade",
                    Name = "Update Grade",
                    Name_AR = "تحديث المستوى الدراسي",
                    Description = "Update Grade",
                    Description_AR = "تحديث المستوى الدراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("494D9C56-558A-427C-9854-878520FCDEC8"),
                    Key = "ViewExamsSetting",
                    Name = "View Exams Setting",
                    Name_AR = "عرض إعدادات الامتحانات",
                    Description = "View Exams Setting",
                    Description_AR = "عرض إعدادات الامتحانات",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("861AD498-A214-4B7D-BDE7-815ABF63A587"),
                    Key = "AddExam",
                    Name = "Add Exam",
                    Name_AR = "إضافة امتحان",
                    Description = "Add Exam",
                    Description_AR = "إضافة امتحان",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("062FF041-C775-4B30-8A49-202DB7D9DD28"),
                    Key = "AddMultipleExams",
                    Name = "Add Multiple Exams",
                    Name_AR = "إضافة امتحانات متعددة",
                    Description = "Add Multiple Exams",
                    Description_AR = "إضافة امتحانات متعددة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("E5B8454F-A193-4D63-B791-3A02AA9D71D3"),
                    Key = "ViewClassesSettingSection",
                    Name = "View Classes Setting Section",
                    Name_AR = "عرض قسم إعدادات الفصول",
                    Description = "View Classes Setting Section",
                    Description_AR = "عرض قسم إعدادات الفصول",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("056ED8BB-3FFC-459F-831E-C8A462DB313B"),
                    Key = "ViewClassesSettingList",
                    Name = "View Classes Setting List",
                    Name_AR = "عرض قائمة إعدادات الفصول",
                    Description = "View Classes Setting List",
                    Description_AR = "عرض قائمة إعدادات الفصول",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("CB37BBD0-40AD-4CA8-AE19-1ED87FEA0B97"),
                    Key = "AddClass",
                    Name = "Add Class",
                    Name_AR = "إضافة فصل دراسي",
                    Description = "Add Class",
                    Description_AR = "إضافة فصل دراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("84956C26-E46F-432A-9B44-C8FB17E95552"),
                    Key = "AddMultipleClasses",
                    Name = "Add Multiple Classes",
                    Name_AR = "إضافة فصول دراسية متعددة",
                    Description = "Add Multiple Classes",
                    Description_AR = "إضافة فصول دراسية متعددة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("F19CCD22-35AC-4778-8A7F-FB0E56822385"),
                    Key = "ChangeClassActivation",
                    Name = "Change Class Activation",
                    Name_AR = "تغيير تفعيل الفصل الدراسي",
                    Description = "Change Class Activation",
                    Description_AR = "تغيير تفعيل الفصل الدراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("7A382B52-95F3-442F-ABFF-9C1C5C9607CC"),
                    Key = "UpdateClass",
                    Name = "Update Class",
                    Name_AR = "تحديث الفصل الدراسي",
                    Description = "Update Class",
                    Description_AR = "تحديث الفصل الدراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B6858FE8-DCC3-4C0F-B1A9-6A8AB7331924"),
                    Key = "LinkTeacherToSchool",
                    Name = "Link Teacher To School",
                    Name_AR = "ربط المعلم بالمدرسة",
                    Description = "Link Teacher To School",
                    Description_AR = "ربط المعلم بالمدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("DD0F56AD-6039-4A8B-9D26-EBD5ABE87E7D"),
                    Key = "AddCourse",
                    Name = "Add Course",
                    Name_AR = "إضافة مادة دراسية",
                    Description = "Add Course",
                    Description_AR = "إضافة مادة دراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("6ED86695-9EA7-4820-BF89-0DE16DFD6EA9"),
                    Key = "AddMultipleCourses",
                    Name = "Add Multiple Courses",
                    Name_AR = "إضافة مواد دراسية متعددة",
                    Description = "Add Multiple Courses",
                    Description_AR = "إضافة مواد دراسية متعددة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("A4A93CEB-21F9-4400-9444-0D94CF895BEB"),
                    Key = "UpdateCourse",
                    Name = "Update Course",
                    Name_AR = "تحديث المادة الدراسية",
                    Description = "Update Course",
                    Description_AR = "تحديث المادة الدراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("7B4478C8-2526-493A-A34D-9FFFA5786F85"),
                    Key = "ViewTeachersList",
                    Name = "View Teachers List",
                    Name_AR = "عرض قائمة المعلمين",
                    Description = "View Teachers List",
                    Description_AR = "عرض قائمة المعلمين",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("6DE52188-1C80-46E3-A436-FF36469E2976"),
                    Key = "ViewAdminSettings",
                    Name = "View Admin Aettings",
                    Name_AR = "عرض إعدادات المسؤولين",
                    Description = "View Admin Aettings",
                    Description_AR = "عرض إعدادات المسؤولين",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("FE286E0A-193B-4F12-AE88-DF5808FB2EB7"),
                    Key = "UnlinkTeacherToSchool",
                    Name = "Unlink Teacher To School",
                    Name_AR = "فك ارتباط المعلم بالمدرسة",
                    Description = "Unlink Teacher To School",
                    Description_AR = "فك ارتباط المعلم بالمدرسة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("98D37ADE-FDC0-4A0B-8E00-402A708C9DDD"),
                    Key = "LinkTeacherToCourse&Class",
                    Name = "Link Teacher To Course & Class",
                    Name_AR = "ربط المعلم بالمادة الدراسية والفصل",
                    Description = "Link Teacher To Course & Class",
                    Description_AR = "ربط المعلم بالمادة الدراسية والفصل",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("869A6521-2730-4F0B-973B-60FB8093C769"),
                    Key = "UnlinkTeacherToCourse&Class",
                    Name = "Unlink Teacher To Course & Class",
                    Name_AR = "فك ارتباط المعلم بالمادة الدراسية والفصل",
                    Description = "Unlink Teacher To Course & Class",
                    Description_AR = "فك ارتباط المعلم بالمادة الدراسية والفصل",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("AD53E402-C2AB-4941-861B-A19C82DCA0CC"),
                    Key = "AssignStudentToSchool",
                    Name = "Assign Student To School",
                    Name_AR = "تعيين طالب للمدرسة",
                    Description = "Assign Student To School",
                    Description_AR = "تعيين طالب للمدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B65324A3-35B9-4579-8C85-C9938391DFA6"),
                    Key = "AssignMultipleStudentsToSchool",
                    Name = "Assign Multiple Students To School",
                    Name_AR = "تعيين طلاب متعددين للمدرسة",
                    Description = "Assign Multiple Students To School",
                    Description_AR = "تعيين طلاب متعددين للمدرسة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted =true
                },
                new Permission
                {
                    Id = Guid.Parse("2F1290B1-F439-4810-BA25-5C3E9CC56EFE"),
                    Key = "UpdateStudentInfo",
                    Name = "Update Student Info",
                    Name_AR = "تحديث معلومات الطالب",
                    Description = "Update Student Info",
                    Description_AR = "تحديث معلومات الطالب",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("9DD22E15-9701-492B-AE20-985B8927F3BF"),
                    Key = "ChangeStudentActivation",
                    Name = "Change Student Activation",
                    Name_AR = "تغيير تفعيل الطالب",
                    Description = "Change Student Activation",
                    Description_AR = "تغيير تفعيل الطالب",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("CC4A8F98-FBB7-49DD-8B5F-CC6D02BCCAB0"),
                    Key = "ViewSchoolCurrentYear",
                    Name = "View School Current Year",
                    Name_AR = "عرض السنة الدراسية الحالية للمدرسة",
                    Description = "View School Current Year",
                    Description_AR = "عرض السنة الدراسية الحالية للمدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("C50D6574-648B-4F6E-8ED5-D27F8B5A34F1"),
                    Key = "AddCurrentYear",
                    Name = "Add Current Year",
                    Name_AR = "إضافة سنة دراسية جديدة",
                    Description = "Add Current Year",
                    Description_AR = "إضافة سنة دراسية جديدة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("32D821BB-0C50-4721-9034-097019632C05"),
                    Key = "AddMonth",
                    Name = "Add Month",
                    Name_AR = "إضافة شهر دراسي",
                    Description = "Add Month",
                    Description_AR = "إضافة شهر دراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("69A0778B-A7A8-4E47-B8FD-D061428DBB95"),
                    Key = "ViewYearMonthes",
                    Name = "View Year Monthes",
                    Name_AR = "عرض أشهر السنة الدراسية",
                    Description = "View Year Monthes",
                    Description_AR = "عرض أشهر السنة الدراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D9B2C724-C5CC-497A-A11B-3E48B7DBFD97"),
                    Key = "UpdateYear",
                    Name = "Update Year",
                    Name_AR = "تحديث السنة الدراسية",
                    Description = "Update Year",
                    Description_AR = "تحديث السنة الدراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("93FD0AA6-852F-4AFE-8CF4-9CDEB365D498"),
                    Key = "UpdateMonth",
                    Name = "Update Month",
                    Name_AR = "تحديث الشهر الدراسي",
                    Description = "Update Month",
                    Description_AR = "تحديث الشهر الدراسي",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("B6D6C7E2-E2DE-4B95-9789-A61C69F27765"),
                    Key = "ChangeYearActivation",
                    Name = "Change Year Activation",
                    Name_AR = "تغيير تفعيل السنة الدراسية",
                    Description = "Change Year Activation",
                    Description_AR = "تغيير تفعيل السنة الدراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("11F0AA92-E7AF-4F94-BD50-96F389DEEE2A"),
                    Key = "ChangeMonthActivation",
                    Name = "Change Month Activation",
                    Name_AR = "تغيير تفعيل الشهر الدراسي",
                    Description = "Change Month Activation",
                    Description_AR = "تغيير تفعيل الشهر الدراسي",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("ECD2236F-6FA1-43EC-92E9-AA36AE2358B4"),
                    Key = "CompleteYeare",
                    Name = "Complete Yeare",
                    Name_AR = "إكمال السنة الدراسية",
                    Description = "Complete Yeare",
                    Description_AR = "إكمال السنة الدراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E831E41D-1090-4AD0-BBAF-E4CA823ECD9D"),
                    Key = "ViewYearsList",
                    Name = "View Years List",
                    Name_AR = "عرض قائمة السنوات الدراسية",
                    Description = "View Years List",
                    Description_AR = "عرض قائمة السنوات الدراسية",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("219007EA-620E-4D96-8292-2D015EF68DB1"),
                    Key = "ViewChildernsList",
                    Name = "View Childerns",
                    Name_AR = "عرض قائمة الأطفال",
                    Description = "View Childerns List",
                    Description_AR = "عرض قائمة الأطفال",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("3B49FC12-D345-41C4-B5D0-97A388AAAFA3"),
                    Key = "AttendanceAction",
                    Name = "Attendance Action",
                    Name_AR = "إجراء الحضور والغياب",
                    Description = "Attendance Action",
                    Description_AR = "إجراء الحضور والغياب",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("DE53CF00-E53D-4BE0-8F67-71F8C7248DF4"),
                    Key = "ViewAttendanceList",
                    Name = "View Attendance List",
                    Name_AR = "عرض قائمة الحضور والغياب",
                    Description = "View Attendance List",
                    Description_AR = "عرض قائمة الحضور والغياب",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("30AA5979-36F6-4A63-98D2-A06F96178176"),
                    Key = "AddHomework",
                    Name = "Add Homework",
                    Name_AR = "إضافة واجب منزلي",
                    Description = "Add Homework",
                    Description_AR = "إضافة واجب منزلي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("BAA954FD-8BA9-4529-834D-E640057998AF"),
                    Key = "ViewHomework",
                    Name = "View Homework",
                    Name_AR = "عرض الواجبات المنزلية",
                    Description = "View Homework",
                    Description_AR = "عرض الواجبات المنزلية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("65B57F13-161F-43B2-9E8D-DAAFB50FBBBB"),
                    Key = "UpdateHomework",
                    Name = "Update Homework",
                    Name_AR = "تحديث الواجب المنزلي",
                    Description = "Update Homework",
                    Description_AR = "تحديث الواجب المنزلي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("C1856505-7C6B-4932-8034-CF4FBC0EFB67"),
                    Key = "RateHomework",
                    Name = "Rate Homework",
                    Name_AR = "تقييم الواجب المنزلي",
                    Description = "Rate Homework",
                    Description_AR = "تقييم الواجب المنزلي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("09E73F16-BB7A-403B-A8F9-FDC1C3228B0D"),
                    Key = "ViewSchoolSettings",
                    Name = "View School Settings",
                    Name_AR = "عرض إعدادات المدرسة",
                    Description = "View School Settings",
                    Description_AR = "عرض إعدادات المدرسة",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("FBA69A66-4E31-44F2-93AC-9602705A7F98"),
                    Key = "ViewExams",
                    Name = "View Exams",
                    Name_AR = "عرض الامتحانات",
                    Description = "View Exams",
                    Description_AR = "عرض الامتحانات",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("EBD73C8F-2EF9-4A67-99E6-BAFE71C9E83C"),
                    Key = "RateExam",
                    Name = "Rate Exam",
                    Name_AR = "تقييم الامتحان",
                    Description = "Rate Exam",
                    Description_AR = "تقييم الامتحان",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("49869E47-2209-4FD8-AA08-2E99BA211FF1"),
                    Key = "ViewActivities",
                    Name = "View Activities",
                    Name_AR = "عرض الأنشطة",
                    Description = "View Activities",
                    Description_AR = "عرض الأنشطة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E09A3A9F-0FF1-46B7-8C80-B7F07CCB4542"),
                    Key = "AddActivity",
                    Name = "Add Activity",
                    Name_AR = "إضافة نشاط",
                    Description = "Add Activity",
                    Description_AR = "إضافة نشاط",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("A992E74B-4CE4-4D18-BCC5-C079D116D1F5"),
                    Key = "UpdateActivity",
                    Name = "Update Activity",
                    Name_AR = "تحديث النشاط",
                    Description = "Update Activity",
                    Description_AR = "تحديث النشاط",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("99FDFBA4-EF5B-44D1-B9BF-B4FAF68A7B60"),
                    Key = "RateActivity",
                    Name = "Rate Activity",
                    Name_AR = "تقييم النشاط",
                    Description = "Rate Activity",
                    Description_AR = "تقييم النشاط",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("EEC01FE0-4399-44B0-B8E7-AFD417A6A93C"),
                    Key = "AddNote",
                    Name = "Add Note",
                    Name_AR = "إضافة ملاحظة",
                    Description = "Add Note",
                    Description_AR = "إضافة ملاحظة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D9BDCF84-D193-4775-8F9B-BACB1BD22915"),
                    Key = "ViewNotes",
                    Name = "View Notes",
                    Name_AR = "عرض الملاحظات",
                    Description = "View Notes",
                    Description_AR = "عرض الملاحظات",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("B7BCF498-F7E9-48B4-9E99-49F6D0A9A956"),
                    Key = "EditNote",
                    Name = "Edit Note",
                    Name_AR = "تعديل الملاحظة",
                    Description = "Edit Note",
                    Description_AR = "تعديل الملاحظة",
                    SchoolPermission = true,
                    AppPermission = false
                },

                new Permission
                {
                    Id = Guid.Parse("A8704499-413D-4FF4-A3A2-122B684A0E17"),
                    Key = "ViewPermissionSettings",
                    Name = "View Permission Settings",
                    Name_AR = "عرض اعدادات الصلاحيات",
                    Description = "View Permission Settings",
                    Description_AR = "عرض اعدادات الصلاحيات",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("512D2EBF-DD4A-482B-8753-1252FE196511"),
                    Key = "ViewPaymentsTransaction",
                    Name = "View Payments Transaction",
                    Name_AR = "عرض حركات الدفع",
                    Description = "View Payments Transaction",
                    Description_AR = "عرض حركات الدفع",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("09756CB3-4363-4763-812A-13F1F8A3B693"),
                    Key = "RemoveUserRole",
                    Name = "Remove User Role",
                    Name_AR = "حذف صلاحة المستخدم",
                    Description = "Remove User Role",
                    Description_AR = "حذف صلاحة المستخدم",
                    SchoolPermission = true,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("26B6AAD5-3434-4538-867E-165D70800FB9"),
                    Key = "ManageShoolUserPermissins",
                    Name = "Manage School User Permissions",
                    Name_AR = "إدارة صلاحيات مستخدمي المدرسة",
                    Description = "Manage School User Permissions",
                    Description_AR = "إدارة صلاحيات مستخدمي المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("CF67BEBE-A01E-46E0-9B8E-1EDC0CEE2087"),
                    Key = "EditRelations",
                    Name = "Edit Relations",
                    Name_AR = "تحرير العلاقات",
                    Description = "Edit Relations",
                    Description_AR = "تحرير العلاقات",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("B357214A-296E-4999-BE38-2AF4259D3096"),
                    Key = "DeleteSchoolYear",
                    Name = "Delete School Year",
                    Name_AR = "حذف السنة الدراسية",
                    Description = "Delete School Year",
                    Description_AR = "حذف السنة الدراسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("913EEED8-4D9D-4778-A84D-3178B36FEDBB"),
                    Key = "DeleteRelationType",
                    Name = "Delete Relation Type",
                    Name_AR = "حذف نوع العلاقة",
                    Description = "Delete Relation Type",
                    Description_AR = "حذف نوع العلاقة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("687A31C3-6D21-4F8B-9860-379B2A563758"),
                    Key = "DeleteClassroom",
                    Name = "Delete Classroom",
                    Name_AR = "حذف الفصل الدراسي",
                    Description = "Delete Classroom",
                    Description_AR = "حذف الفصل الدراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("F4C86EAD-E913-4C5E-B1A5-3E17A647C216"),
                    Key = "CreateSchoolContactType",
                    Name = "Create School Contact Type",
                    Name_AR = "إنشاء نوع جهة اتصال للمدرسة",
                    Description = "Create School Contact Type",
                    Description_AR = "إنشاء نوع جهة اتصال للمدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("5E48C040-34E8-4905-9B15-3F4069E39840"),
                    Key = "DeleteSecurityGroup",
                    Name = "Delete Security Group",
                    Name_AR = "حذف مجموعة الأمان",
                    Description = "Delete Security Group",
                    Description_AR = "حذف مجموعة الأمان",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("CBCFF607-EAC1-45E8-81E7-4626A165A1B9"),
                    Key = "AddNewSchoolAdminList",
                    Name = "Add New School Admin List",
                    Name_AR = "إضافة قائمة مسؤولي المدارس",
                    Description = "Add New School Admin List",
                    Description_AR = "إضافة قائمة مسؤولي المدارس",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("889BC51F-0624-4911-988D-4A86F1A4F011"),
                    Key = "ActivateRolesList",
                    Name = "Activate Roles List",
                    Name_AR = "تفعيل قائمة الأدوار",
                    Description = "Activate Roles List",
                    Description_AR = "تفعيل قائمة الأدوار",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("21E7443F-566F-452E-A246-4E65260B16F4"),
                    Key = "AddApplicationEmployee",
                    Name = "Add Application Employee",
                    Name_AR = "إضافة موظف للتطبيق",
                    Description = "Add Application Employee",
                    Description_AR = "إضافة موظف للتطبيق",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("D65974E4-239A-4683-8B07-5110270E04F6"),
                    Key = "AddUserRole",
                    Name = "Add User Role",
                    Name_AR = "إضافة صلاحية للمستخدم",
                    Description = "Add User Role",
                    Description_AR = "إضافة صلاحية للمستخدم",
                    SchoolPermission = true,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("77127635-8FE8-4451-AC9C-59C38EE02D4D"),
                    Key = "UpdateExam",
                    Name = "Update Exam",
                    Name_AR = "تحديث الاختبار",
                    Description = "Update Exam",
                    Description_AR = "تحديث الاختبار",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("2989C158-EF37-40F8-BDF8-637C874F2F1E"),
                    Key = "AddSchoolPayment",
                    Name = "Add School Payment",
                    Name_AR = "إضافة دفعة مدرسية",
                    Description = "Add School Payment",
                    Description_AR = "إضافة دفعة مدرسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("55F6B349-57D1-40D0-BB56-64C1592C2268"),
                    Key = "CreateRole",
                    Name = "Create Role",
                    Name_AR = "إنشاء صلاحية",
                    Description = "Create Role",
                    Description_AR = "إنشاء صلاحية",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("2B656FC9-B90A-4B22-B882-672495224220"),
                    Key = "LinkEmployeeToSchool",
                    Name = "Link Employee To School",
                    Name_AR = "ربط الموظف بالمدرسة",
                    Description = "Link Employee To Schoolw",
                    Description_AR = "ربط الموظف بالمدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("666F4016-4375-4CA0-BA48-6C1CAB50F91A"),
                    Key = "DeleteTeacher",
                    Name = "Delete Teacher",
                    Name_AR = "حذف المعلم",
                    Description = "Delete Teacher",
                    Description_AR = "حذف المعلم",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D9BB6050-186D-4052-A3DC-6D33A84424AC"),
                    Key = "CreateCoinType",
                    Name = "Create Coin Type",
                    Name_AR = "إنشاء نوع عملة",
                    Description = "Create Coin Type",
                    Description_AR = "إنشاء نوع عملة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("C3DA80D1-9288-4EBE-96E7-7210CBEEB1CA"),
                    Key = "DeleteContactType",
                    Name = "Delete Contact Type",
                    Name_AR = "حذف نوع جهة الاتصال",
                    Description = "Delete Contact Type",
                    Description_AR = "حذف نوع جهة الاتصال",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("23A9E98E-BA77-48BB-8F92-73FAE4DF3245"),
                    Key = "ViewEmployeesList",
                    Name = "View Employees List",
                    Name_AR = "عرض قائمة الموظفين",
                    Description = "View Employees List",
                    Description_AR = "عرض قائمة الموظفين",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("AF2EEDF8-EE6D-464B-8551-743F6EF3D3B5"),
                    Key = "AddSchoolPost",
                    Name = "Add School Post",
                    Name_AR = "إضافة منشور مدرسي",
                    Description = "Add School Post",
                    Description_AR = "إضافة منشور مدرسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("9D7EC7F9-0341-48A6-8DB5-7486BCA97497"),
                    Key = "DeleteCourse",
                    Name = "Delete Course",
                    Name_AR = "حذف المادة الدراسة",
                    Description = "Delete Course",
                    Description_AR = "حذف المادة الدراسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E291A793-6EFA-40E1-878D-7C11095A6C65"),
                    Key = "DeleteYearMonth",
                    Name = "Delete Year Month",
                    Name_AR = "حذف شهر الدراسي",
                    Description = "Delete Year Month",
                    Description_AR = "حذف شهر الدراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E9C02932-B613-45EB-9E71-7CB6204745D9"),
                    Key = "DeleteSchool",
                    Name = "Delete School",
                    Name_AR = "حذف المدرسة",
                    Description = "Delete School",
                    Description_AR = "حذف المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("DAF8B889-EE84-45E2-9C59-8135CD5551A7"),
                    Key = "LinkEmployeeClass",
                    Name = "Link Employee Class",
                    Name_AR = "ربط الموظف بالفصل",
                    Description = "Link Employee Class",
                    Description_AR = "ربط الموظف بالفصل",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("D330DF2E-9B66-46A0-A64D-88B1D4F9519D"),
                    Key = "EditCoinType",
                    Name = "Edit Coin Type",
                    Name_AR = "تعديل نوع العملة",
                    Description = "Edit Coin Type",
                    Description_AR = "تعديل نوع العملة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("AC0C5E27-8F01-4B94-BEF0-88FEAAE2043A"),
                    Key = "ViewSchoolsAdminsList",
                    Name = "View Schools Admins List",
                    Name_AR = "عرض قائمة مسؤولي المدارس",
                    Description = "View Schools Admins List",
                    Description_AR = "عرض قائمة مسؤولي المدارس",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("5ECBB154-8466-4EA1-8B66-8AA1E7D7853A"),
                    Key = "DeleteSchoolType",
                    Name = "Delete School Type",
                    Name_AR = "حذف نوع المدرسة",
                    Description = "Delete School Type",
                    Description_AR = "حذف نوع المدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("006EDBB6-657D-41CF-9E01-8AB9CF4CEA69"),
                    Key = "RemoveSchoolPayment",
                    Name = "Remove School Payment",
                    Name_AR = "إزالة دفعة مدرسية",
                    Description = "Remove School Payment",
                    Description_AR = "إزالة دفعة مدرسية",
                    SchoolPermission = true,
                    AppPermission = false,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("F76D6CD8-4A98-4EBA-9D09-9204009D7839"),
                    Key = "UpdateSchoolPayment",
                    Name = "Update School Payment",
                    Name_AR = "تحديث دفعة مدرسية",
                    Description = "Update School Payment",
                    Description_AR = "تحديث دفعة مدرسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("3992521C-F222-4323-8969-94F23987F157"),
                    Key = "DeleteApplicationEmployee",
                    Name = "Delete Application Employee",
                    Name_AR = "حذف موظف التطبيق",
                    Description = "Delete Application Employee",
                    Description_AR = "حذف موظف التطبيق",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("19EA90DC-1ED5-4445-A238-9C95E2F37842"),
                    Key = "RemoveUserPermission",
                    Name = "Remove User Permission",
                    Name_AR = "إزالة صلاحية المستخدم",
                    Description = "Remove User Permission",
                    Description_AR = "إزالة صلاحية المستخدم",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("FB4F053D-67FA-4FF2-B85F-A4BF8F385BCE"),
                    Key = "RemoveLanguageFromSchool",
                    Name = "Remove Language From School",
                    Name_AR = "إزالة اللغة من المدرسة",
                    Description = "Remove Language From School",
                    Description_AR = "إزالة اللغة من المدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("16B49575-3825-4489-9822-A64615FE8898"),
                    Key = "AddPermissionsToRole",
                    Name = "Add Permissions To Role",
                    Name_AR = "إضافة صلاحيات إلى مستوى الامان",
                    Description = "Add Permissions To Role",
                    Description_AR = "إضافة صلاحيات إلى مستوى الامان",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("37FC9ECE-B339-446E-BEBA-A81B71302266"),
                    Key = "DeleteLanguage",
                    Name = "Delete Language",
                    Name_AR = "حذف اللغة",
                    Description = "Delete Language",
                    Description_AR = "حذف اللغة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("EC2C9457-2184-4B45-B271-A90A461A816E"),
                    Key = "ViewSchoolPaymentRequest",
                    Name = "View School Payment Request",
                    Name_AR = "عرض طلب دفعة مدرسية",
                    Description = "View School Payment Request",
                    Description_AR = "عرض طلب دفعة مدرسية",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("E4C90EA7-7F44-4C81-82B7-B1DDD979B9CD"),
                    Key = "ViewSchoolActionList",
                    Name = "View School Action List",
                    Name_AR = "عرض قائمة الإجراءات المدرسية",
                    Description = "View School Action List",
                    Description_AR = "عرض قائمة الإجراءات المدرسية",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("8E296936-5161-4B61-885C-B520FE350C9A"),
                    Key = "DeleteSchoolEmployee",
                    Name = "Delete School Employee",
                    Name_AR = "حذف موظف المدرسة",
                    Description = "Delete School Employee",
                    Description_AR = "حذف موظف المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("CDC3C1C4-4598-44F9-904F-B5D19E31F328"),
                    Key = "DeleteGrade",
                    Name = "Delete Grade",
                    Name_AR = "حذف المستوى الدراسي",
                    Description = "Delete Grade",
                    Description_AR = "حذف المستوى الدراسي",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("F6821A1F-CB86-445D-B7AA-B7224BD11B47"),
                    Key = "AddApplicationEmployees",
                    Name = "Add Application Employees",
                    Name_AR = "إضافة موظفين للتطبيق",
                    Description = "Add Application Employees",
                    Description_AR = "إضافة موظفين للتطبيق",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("D0B2932F-717A-4AD7-83A8-C11F1806237D"),
                    Key = "EditRole",
                    Name = "Edit Role",
                    Name_AR = "تعديل مستوى الامان",
                    Description = "Edit Role",
                    Description_AR = "تعديل مستوى الامان",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("C0FE2438-B76A-4BCB-9CDE-D628B5C3DEB6"),
                    Key = "AddUserPermission",
                    Name = "Add User Permission",
                    Name_AR = "إضافة صلاحية للمستخدم",
                    Description = "Add User Permission",
                    Description_AR = "إضافة صلاحية للمستخدم",
                    SchoolPermission = false,
                    AppPermission = true,
                    Deleted = true
                },
                new Permission
                {
                    Id = Guid.Parse("A1BD5EF1-6A55-4E1E-8BB8-DA39344E4412"),
                    Key = "DeleteSchoolPaymentRequest",
                    Name = "Delete School Payment Request",
                    Name_AR = "حذف طلب دفع المدرسة",
                    Description = "Delete School Payment Request",
                    Description_AR = "حذف طلب دفع المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("27DAF2FF-556D-4515-A41F-DEAD5699F5AB"),
                    Key = "CreateRelations",
                    Name = "Create Relations",
                    Name_AR = "إنشاء علاقات",
                    Description = "Create Relations",
                    Description_AR = "إنشاء علاقات",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("DE402AF3-0C36-4204-AA5B-DF56F8033580"),
                    Key = "RemovePermissionsToRole",
                    Name = "Remove Permissions From Role",
                    Name_AR = "إزالة صلاحيات من مستوى الامان",
                    Description = "Remove Permissions From Role",
                    Description_AR = "إزالة صلاحيات من مستوى الامان",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("EBDE1C53-4840-484F-AC23-DF838E6282DC"),
                    Key = "ViewPaymentsRequests",
                    Name = "View Payments Requests",
                    Name_AR = "عرض طلبات الدفع",
                    Description = "View Payments Requests",
                    Description_AR = "عرض طلبات الدفع",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("132EC233-D6EF-4187-A780-E53C85D1BABB"),
                    Key = "EditSchoolContactType",
                    Name = "Edit School Contact Type",
                    Name_AR = "تعديل نوع جهة اتصال المدرسة",
                    Description = "Edit School Contact Type",
                    Description_AR = "تعديل نوع جهة اتصال المدرسة",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("061AC24B-3828-4360-833F-EF5865712E39"),
                    Key = "CreatePaymentType",
                    Name = "Create Payment Type",
                    Name_AR = "إنشاء نوع دفع",
                    Description = "Create Payment Type",
                    Description_AR = "إنشاء نوع دفع",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("44C76680-9B72-48F9-BF3D-F113C4447331"),
                    Key = "ManageApplicationManagersPermissions",
                    Name = "Manage Application Managers Permissions",
                    Name_AR = "إدارة صلاحيات مديري التطبيق",
                    Description = "Manage Application Managers Permissions",
                    Description_AR = "إدارة صلاحيات مديري التطبيق",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("EE6325A4-4BC4-4AA7-9944-F82C3B7A305A"),
                    Key = "ViewSchoolStudentsList",
                    Name = "View School Students List",
                    Name_AR = "عرض قائمة طلاب المدرسة",
                    Description = "View School Students List",
                    Description_AR = "عرض قائمة طلاب المدرسة",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("0A0AB8A7-1D51-4B03-B10A-647A8F90EC24"),
                    Key = "DeletePaymentType",
                    Name = "Delete Payment Type",
                    Name_AR = "حذف طريقة الدفع",
                    Description = "Delete Payment Type",
                    Description_AR = "حذف طريقة الدفع",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("9C8526E9-9119-43D8-A434-4C4828C8A5D9"),
                    Key = "ViewPaymentTypesList",
                    Name = "View Payment Types List",
                    Name_AR = "عرض طرق الدفع",
                    Description = "View Payment Types List",
                    Description_AR = "عرض طرق الدفع",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("56D9A5B6-28A6-4E8D-ADE6-54AD37C846BD"),
                    Key = "UpdatePaymentType",
                    Name = "Update Payment Type",
                    Name_AR = "تعديل طريقة الدفع",
                    Description = "Update Payment Type",
                    Description_AR = "تعديل طريقة الدفع",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("B96D60AD-E4DB-4CE9-8EA8-C22D2CE8C544"),
                    Key = "DeleteCoinType",
                    Name = "Delete Coin Type",
                    Name_AR = "حذف نوع العمله",
                    Description = "Delete Coin Type",
                    Description_AR = "حذف نوع العمله",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("FBADCD2D-C9C8-4164-BAD1-667A586B54CC"),
                    Key = "ViewCoinTypesList",
                    Name = "View Coin Types List",
                    Name_AR = "عرض انواع العملات",
                    Description = "View Coin Types List",
                    Description_AR = "عرض انواع العملات",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("A44F8B6E-7B16-4497-9C4B-8E55EAF4E7D4"),
                    Key = "ShowReportedPosts",
                    Name = "Show Reported Posts",
                    Name_AR = "Show Reported Posts",
                    Description = "View reported school posts with report counts",
                    Description_AR = "View reported school posts with report counts",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("8A01A316-9151-4BB7-8B0E-A87E5EE7E367"),
                    Key = "DeletePost",
                    Name = "Delete Post",
                    Name_AR = "Delete Post",
                    Description = "Delete school post",
                    Description_AR = "Delete school post",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("5B4C93D4-22E3-4D89-BDB8-6B6EC2F7E840"),
                    Key = "DeleteSchoolPost",
                    Name = "Delete School Post",
                    Name_AR = "Delete School Post",
                    Description = "Delete school post (school manager)",
                    Description_AR = "Delete school post (school manager)",
                    SchoolPermission = true,
                    AppPermission = false
                }

                );
        }
    }
}
