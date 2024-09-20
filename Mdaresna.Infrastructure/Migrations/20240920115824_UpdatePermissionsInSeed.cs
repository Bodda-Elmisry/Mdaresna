using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePermissionsInSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Key", "LastModifyDate", "Name", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("056ed8bb-3ffc-459f-831e-c8a462db313b"), false, null, "View Classes Setting List", "ViewClassesSettingList", null, "View Classes Setting List", true },
                    { new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"), false, null, "Add Multiple Exams", "AddMultipleExams", null, "Add Multiple Exams", true },
                    { new Guid("072c7a82-b62d-45d8-a3ed-48a70319abf3"), true, null, "Change School Activation", "ChangeSchoolActivation", null, "Change School Activation", false },
                    { new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"), false, null, "Add Exam", "AddExam", null, "Add Exam", true },
                    { new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"), false, null, "Add Multiple Grades", "AddMultipleGrades", null, "Add Multiple Grades", true },
                    { new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"), false, null, "Change Month Activation", "ChangeMonthActivation", null, "Change Month Activation", true },
                    { new Guid("122d8e7a-f5a7-48ea-b2a6-f86964a2c3e7"), true, null, "Change School Type", "ChangeSchoolType", null, "Change School Type", false },
                    { new Guid("16983650-564e-4331-97d2-c1b5d67fef40"), false, null, "Update School Contact", "UpdateSchoolContact", null, "Update School Contact", true },
                    { new Guid("21040e37-d949-42ea-9a77-96aed0289209"), false, null, "Update School Data", "UpdateSchoolData", null, "Update School Data", true },
                    { new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"), false, null, "Add School Contact", "AddSchoolContact", null, "Add School Contact", true },
                    { new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"), false, null, "Update Student Info", "UpdateStudentInfo", null, "Update Student Info", true },
                    { new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"), false, null, "Add Homework", "AddHomework", null, "Add Homework", true },
                    { new Guid("32d821bb-0c50-4721-9034-097019632c05"), false, null, "Add Month", "AddMonth", null, "Add Month", true },
                    { new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"), false, null, "Attendance Action", "AttendanceAction", null, "Attendance Action", true },
                    { new Guid("494d9c56-558a-427c-9854-878520fcdec8"), false, null, "View Exams Setting", "ViewExamsSetting", null, "View Exams Setting", true },
                    { new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"), false, null, "View Activities", "ViewActivities", null, "View Activities", true },
                    { new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"), false, null, "Add Grade", "AddGrade", null, "Add Grade", true },
                    { new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"), false, null, "Update Homework", "UpdateHomework", null, "Update Homework", true },
                    { new Guid("68713df9-ed1b-449c-8060-d919a2592b02"), true, null, "Create Language", "CreateLanguage", null, "Create Language", false },
                    { new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"), true, null, "View Year Monthes", "ViewYearMonthes", null, "View Year Monthes", false },
                    { new Guid("6de52188-1c80-46e3-a436-ff36469e2976"), false, null, "Link Teacher To School", "LinkTeacherToSchool", null, "Link Teacher To School", true },
                    { new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"), false, null, "Add Multiple Courses", "AddMultipleCourses", null, "Add Multiple Courses", true },
                    { new Guid("71b6f158-82f2-4a48-9950-56dbb00eec4e"), true, null, "View Languages", "ViewLanguages", null, "View Languages", false },
                    { new Guid("75162229-2536-4232-b081-feabe20c318d"), false, null, "View Grades Setting", "ViewGradesSetting", null, "View Grades Setting", true },
                    { new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"), true, null, "Change School Coin Type", "ChangeSchoolCoinType", null, "Change School Coin Type", false },
                    { new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"), false, null, "Update Exam", "UpdateExam", null, "Update Exam", true },
                    { new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"), false, null, "Update Class", "UpdateClass", null, "Update Class", true },
                    { new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"), false, null, "View Teachers List", "ViewTeachersList", null, "View Teachers List", true },
                    { new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"), false, null, "Add Multiple Classes", "AddMultipleClasses", null, "Add Multiple Classes", true },
                    { new Guid("861ad498-a214-4b7d-bde7-815abf63a587"), false, null, "Add Exam", "AddExam", null, "Add Exam", true },
                    { new Guid("869a6521-2730-4f0b-973b-60fb8093c769"), false, null, "Unlink Teacher To Course & Class", "UnlinkTeacherToCourse&Class", null, "Unlink Teacher To Course & Class", true },
                    { new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"), true, null, "Assign Coins To School", "AssignCoinsToSchool", null, "Assign Coins To School", false },
                    { new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"), false, null, "Update Month", "UpdateMonth", null, "Update Month", true },
                    { new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"), false, null, "Link Teacher To Course & Class", "LinkTeacherToCourse&Class", null, "Link Teacher To Course & Class", true },
                    { new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"), false, null, "Rate Activity", "RateActivity", null, "Rate Activity", true },
                    { new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"), false, null, "Change Student Activation", "ChangeStudentActivation", null, "Change Student Activation", true },
                    { new Guid("a1134102-7021-4770-8808-fdc376190691"), false, null, "Update Grade", "UpdateGrade", null, "Update Grade", true },
                    { new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"), false, null, "Update Course", "UpdateCourse", null, "Update Course", true },
                    { new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"), false, null, "Update Activity", "UpdateActivity", null, "Update Activity", true },
                    { new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"), false, null, "Assign Student To School", "AssignStudentToSchool", null, "Assign Student To School", true },
                    { new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"), false, null, "Assign Multiple Students To School", "AssignMultipleStudentsToSchool", null, "Assign Multiple Students To School", true },
                    { new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"), false, null, "Link Teacher To School", "LinkTeacherToSchool", null, "Link Teacher To School", true },
                    { new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"), false, null, "Change Year Activation", "ChangeYearActivation", null, "Change Year Activation", true },
                    { new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"), false, null, "Edit Note", "EditNote", null, "Edit Note", true },
                    { new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"), false, null, "Uploade School Image", "UploadeSchoolImage", null, "Uploade School Image", true },
                    { new Guid("baa954fd-8ba9-4529-834d-e640057998af"), false, null, "View Homework", "ViewHomework", null, "View Homework", true },
                    { new Guid("be86c901-37f9-4f63-b7b3-03653e75fea5"), false, null, "Add Language To School", "AddLanguageToSchool", null, "Add Language To School", true },
                    { new Guid("c1856505-7c6b-4932-8034-cf4fbc0efb67"), false, null, "Rate Homework", "RateHomework", null, "Rate Homework", true },
                    { new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"), false, null, "Add Current Year", "AddCurrentYear", null, "Add Current Year", true },
                    { new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"), false, null, "Add Class", "AddClass", null, "Add Class", true },
                    { new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"), false, null, "View School Current Year", "ViewSchoolCurrentYear", null, "View School Current Year", true },
                    { new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"), false, null, "View Shool Languages Setting", "ViewSchoolLanguagesSetting", null, "View School Languages Setting", true },
                    { new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"), false, null, "Update Year", "UpdateYear", null, "Update Year", true },
                    { new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"), false, null, "View Notes", "ViewNotes", null, "View Notes", true },
                    { new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"), false, null, "Update Exam", "UpdateExam", null, "Update Exam", true },
                    { new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"), false, null, "Add Course", "AddCourse", null, "Add Course", true },
                    { new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"), false, null, "View Attendance List", "ViewAttendanceList", null, "View Attendance List", true },
                    { new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"), false, null, "Add Activity", "AddActivity", null, "Add Activity", true },
                    { new Guid("e50a5830-e741-4260-820c-19c2cab1b419"), false, null, "View School Data", "ViewSchoolData", null, "View School Data", true },
                    { new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"), false, null, "View Classes Setting Section", "ViewClassesSettingSection", null, "View Classes Setting Section", true },
                    { new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"), false, null, "View Years List", "ViewYearsList", null, "View Years List", true },
                    { new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"), false, null, "Rate Exam", "RateExam", null, "Rate Exam", true },
                    { new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"), false, null, "Complete Yeare", "CompleteYeare", null, "Complete Yeare", true },
                    { new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"), false, null, "View School Students List", "ViewSchoolStudentsList", null, "View School Students List", true },
                    { new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"), false, null, "Add Note", "AddNote", null, "Add Note", true },
                    { new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"), false, null, "Change Class Activation", "ChangeClassActivation", null, "Change Class Activation", true },
                    { new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"), false, null, "View Exams", "ViewExams", null, "View Exams", true },
                    { new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"), true, null, "Filter Schools By Activity", "FilterSchoolsByActivity", null, "Filter Schools By Activity", true },
                    { new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"), false, null, "Unlink Teacher To School", "UnlinkTeacherToSchool", null, "Unlink Teacher To School", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("056ed8bb-3ffc-459f-831e-c8a462db313b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("062ff041-c775-4b30-8a49-202db7d9dd28"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("072c7a82-b62d-45d8-a3ed-48a70319abf3"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09e73f16-bb7a-403b-a8f9-fdc1c3228b0d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0d9bfad2-0762-47ba-8046-3aa92b2d6222"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("11f0aa92-e7af-4f94-bd50-96f389deee2a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("122d8e7a-f5a7-48ea-b2a6-f86964a2c3e7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16983650-564e-4331-97d2-c1b5d67fef40"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21040e37-d949-42ea-9a77-96aed0289209"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26de4395-48fb-4684-b88d-5c9e6081f9ac"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f1290b1-f439-4810-ba25-5c3e9cc56efe"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30aa5979-36f6-4a63-98d2-a06f96178176"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("32d821bb-0c50-4721-9034-097019632c05"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3b49fc12-d345-41c4-b5d0-97a388aaafa3"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("494d9c56-558a-427c-9854-878520fcdec8"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("49869e47-2209-4fd8-aa08-2e99ba211ff1"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5c8d7610-bfaa-4deb-bd84-e54ed4351611"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("65b57f13-161f-43b2-9e8d-daafb50fbbbb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("68713df9-ed1b-449c-8060-d919a2592b02"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("69a0778b-a7a8-4e47-b8fd-d061428dbb95"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6de52188-1c80-46e3-a436-ff36469e2976"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6ed86695-9ea7-4820-bf89-0de16dfd6ea9"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("71b6f158-82f2-4a48-9950-56dbb00eec4e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("75162229-2536-4232-b081-feabe20c318d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("76befb0e-5ef7-4fe3-8e06-02d50c17c38b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7a382b52-95f3-442f-abff-9c1c5c9607cc"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b4478c8-2526-493a-a34d-9fffa5786f85"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84956c26-e46f-432a-9b44-c8fb17e95552"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("861ad498-a214-4b7d-bde7-815abf63a587"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("869a6521-2730-4f0b-973b-60fb8093c769"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a48610d-c9ff-4fce-95eb-3a4d8d633a9c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("93fd0aa6-852f-4afe-8cf4-9cdeb365d498"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("98d37ade-fdc0-4a0b-8e00-402a708c9ddd"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99fdfba4-ef5b-44d1-b9bf-b4faf68a7b60"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9dd22e15-9701-492b-ae20-985b8927f3bf"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1134102-7021-4770-8808-fdc376190691"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a4a93ceb-21f9-4400-9444-0d94cf895beb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a992e74b-4ce4-4d18-bcc5-c079d116d1f5"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ad53e402-c2ab-4941-861b-a19c82dca0cc"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b65324a3-35b9-4579-8c85-c9938391dfa6"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6858fe8-dcc3-4c0f-b1a9-6a8ab7331924"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b6d6c7e2-e2de-4b95-9789-a61c69f27765"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7bcf498-f7e9-48b4-9e99-49f6d0a9a956"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b7f4d3f3-b0b9-4e2f-8884-e2d61e0b6f5e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("baa954fd-8ba9-4529-834d-e640057998af"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("be86c901-37f9-4f63-b7b3-03653e75fea5"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c1856505-7c6b-4932-8034-cf4fbc0efb67"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c50d6574-648b-4f6e-8ed5-d27f8b5a34f1"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cb37bbd0-40ad-4ca8-ae19-1ed87fea0b97"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cc4a8f98-fbb7-49dd-8b5f-cc6d02bccab0"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d512c1b3-dde0-4646-98cc-2be5c56c7150"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9b2c724-c5cc-497a-a11b-3e48b7dbfd97"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bdcf84-d193-4775-8f9b-bacb1bd22915"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("da3e8df0-4d2b-412a-82d5-3b8bcf62d827"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dd0f56ad-6039-4a8b-9d26-ebd5abe87e7d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de53cf00-e53d-4be0-8f67-71f8c7248df4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e09a3a9f-0ff1-46b7-8c80-b7f07ccb4542"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e50a5830-e741-4260-820c-19c2cab1b419"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5b8454f-a193-4d63-b791-3a02aa9d71d3"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e831e41d-1090-4ad0-bbaf-e4ca823ecd9d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebd73c8f-2ef9-4a67-99e6-bafe71c9e83c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ecd2236f-6fa1-43ec-92e9-aa36ae2358b4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("eec01fe0-4399-44b0-b8e7-afd417a6a93c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f19ccd22-35ac-4778-8a7f-fb0e56822385"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fba69a66-4e31-44f2-93ac-9602705a7f98"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fdebaade-4d2c-447b-ba52-11e3e2a622f4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe286e0a-193b-4f12-ae88-df5808fb2eb7"));
        }
    }
}
