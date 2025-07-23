using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mdaresna.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "تحديث الاختبار", "تحديث الاختبار" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { "عرض قائمة طلاب المدرسة", "عرض قائمة طلاب المدرسة" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppPermission", "CreateDate", "Description", "Description_AR", "Key", "LastModifyDate", "Name", "Name_AR", "SchoolPermission" },
                values: new object[,]
                {
                    { new Guid("006edbb6-657d-41cf-9e01-8ab9cf4cea69"), false, null, "Remove School Payment", "إزالة دفعة مدرسية", "RemoveSchoolPayment", null, "Remove School Payment", "إزالة دفعة مدرسية", true },
                    { new Guid("061ac24b-3828-4360-833f-ef5865712e39"), true, null, "Create Payment Type", "إنشاء نوع دفع", "CreatePaymentType", null, "Create Payment Type", "إنشاء نوع دفع", false },
                    { new Guid("09756cb3-4363-4763-812a-13f1f8a3b693"), true, null, "Remove User Role", "حذف صلاحة المستخدم", "RemoveUserRole", null, "Remove User Role", "حذف صلاحة المستخدم", true },
                    { new Guid("132ec233-d6ef-4187-a780-e53c85d1babb"), true, null, "Edit School Contact Type", "تعديل نوع جهة اتصال المدرسة", "EditSchoolContactType", null, "Edit School Contact Type", "تعديل نوع جهة اتصال المدرسة", false },
                    { new Guid("16b49575-3825-4489-9822-a64615fe8898"), true, null, "Add Permissions To Role", "إضافة صلاحيات إلى مستوى الامان", "AddPermissionsToRole", null, "Add Permissions To Role", "إضافة صلاحيات إلى مستوى الامان", false },
                    { new Guid("19ea90dc-1ed5-4445-a238-9c95e2f37842"), true, null, "Remove User Permission", "إزالة صلاحية المستخدم", "RemoveUserPermission", null, "Remove User Permission", "إزالة صلاحية المستخدم", false },
                    { new Guid("21e7443f-566f-452e-a246-4e65260b16f4"), true, null, "Add Application Employee", "إضافة موظف للتطبيق", "AddApplicationEmployee", null, "Add Application Employee", "إضافة موظف للتطبيق", false },
                    { new Guid("23a9e98e-ba77-48bb-8f92-73fae4df3245"), false, null, "View Employees List", "عرض قائمة الموظفين", "ViewEmployeesList", null, "View Employees List", "عرض قائمة الموظفين", true },
                    { new Guid("26b6aad5-3434-4538-867e-165d70800fb9"), false, null, "Manage School User Permissions", "إدارة صلاحيات مستخدمي المدرسة", "ManageShoolUserPermissins", null, "Manage School User Permissions", "إدارة صلاحيات مستخدمي المدرسة", true },
                    { new Guid("27daf2ff-556d-4515-a41f-dead5699f5ab"), true, null, "Create Relations", "إنشاء علاقات", "CreateRelations", null, "Create Relations", "إنشاء علاقات", false },
                    { new Guid("2989c158-ef37-40f8-bdf8-637c874f2f1e"), false, null, "Add School Payment", "إضافة دفعة مدرسية", "AddSchoolPayment", null, "Add School Payment", "إضافة دفعة مدرسية", true },
                    { new Guid("2b656fc9-b90a-4b22-b882-672495224220"), false, null, "Link Employee To Schoolw", "ربط الموظف بالمدرسة", "LinkEmployeeToSchool", null, "Link Employee To School", "ربط الموظف بالمدرسة", true },
                    { new Guid("37fc9ece-b339-446e-beba-a81b71302266"), false, null, "Delete Language", "حذف اللغة", "DeleteLanguage", null, "Delete Language", "حذف اللغة", true },
                    { new Guid("3992521c-f222-4323-8969-94f23987f157"), true, null, "Delete Application Employee", "حذف موظف التطبيق", "DeleteApplicationEmployee", null, "Delete Application Employee", "حذف موظف التطبيق", false },
                    { new Guid("44c76680-9b72-48f9-bf3d-f113c4447331"), true, null, "Manage Application Managers Permissions", "إدارة صلاحيات مديري التطبيق", "ManageApplicationManagersPermissions", null, "Manage Application Managers Permissions", "إدارة صلاحيات مديري التطبيق", false },
                    { new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"), true, null, "View Payments Transaction", "", "ViewPaymentsTransaction", null, "View Payments Transaction", "", false },
                    { new Guid("55f6b349-57d1-40d0-bb56-64c1592c2268"), true, null, "Create Role", "إنشاء صلاحية", "CreateRole", null, "Create Role", "إنشاء صلاحية", false },
                    { new Guid("5e48c040-34e8-4905-9b15-3f4069e39840"), true, null, "Delete Security Group", "حذف مجموعة الأمان", "DeleteSecurityGroup", null, "Delete Security Group", "حذف مجموعة الأمان", false },
                    { new Guid("5ecbb154-8466-4ea1-8b66-8aa1e7d7853a"), true, null, "Delete School Type", "حذف نوع المدرسة", "DeleteSchoolType", null, "Delete School Type", "حذف نوع المدرسة", false },
                    { new Guid("666f4016-4375-4ca0-ba48-6c1cab50f91a"), false, null, "Delete Teacher", "حذف المعلم", "DeleteTeacher", null, "Delete Teacher", "حذف المعلم", true },
                    { new Guid("687a31c3-6d21-4f8b-9860-379b2a563758"), false, null, "Delete Classroom", "حذف الفصل الدراسي", "DeleteClassroom", null, "Delete Classroom", "حذف الفصل الدراسي", true },
                    { new Guid("889bc51f-0624-4911-988d-4a86f1a4f011"), true, null, "Activate Roles List", "تفعيل قائمة الأدوار", "ActivateRolesList", null, "Activate Roles List", "تفعيل قائمة الأدوار", false },
                    { new Guid("8e296936-5161-4b61-885c-b520fe350c9a"), false, null, "Delete School Employee", "حذف موظف المدرسة", "DeleteSchoolEmployee", null, "Delete School Employee", "حذف موظف المدرسة", true },
                    { new Guid("913eeed8-4d9d-4778-a84d-3178b36fedbb"), true, null, "Delete Relation Type", "حذف نوع العلاقة", "DeleteRelationType", null, "Delete Relation Type", "حذف نوع العلاقة", false },
                    { new Guid("9d7ec7f9-0341-48a6-8db5-7486bca97497"), false, null, "Delete Course", "حذف المادة الدراسة", "DeleteCourse", null, "Delete Course", "حذف المادة الدراسة", true },
                    { new Guid("a1bd5ef1-6a55-4e1e-8bb8-da39344e4412"), false, null, "Delete School Payment Request", "حذف طلب دفع المدرسة", "DeleteSchoolPaymentRequest", null, "Delete School Payment Request", "حذف طلب دفع المدرسة", true },
                    { new Guid("a8704499-413d-4ff4-a3a2-122b684a0e17"), true, null, "View Permission Settings", "", "ViewPermissionSettings", null, "View Permission Settings", "", false },
                    { new Guid("ac0c5e27-8f01-4b94-bef0-88feaae2043a"), true, null, "View Schools Admins List", "عرض قائمة مسؤولي المدارس", "ViewSchoolsAdminsList", null, "View Schools Admins List", "عرض قائمة مسؤولي المدارس", false },
                    { new Guid("af2eedf8-ee6d-464b-8551-743f6ef3d3b5"), false, null, "Add School Post", "إضافة منشور مدرسي", "AddSchoolPost", null, "Add School Post", "إضافة منشور مدرسي", true },
                    { new Guid("b357214a-296e-4999-be38-2af4259d3096"), false, null, "Delete School Year", "حذف السنة الدراسية", "DeleteSchoolYear", null, "Delete School Year", "حذف السنة الدراسية", true },
                    { new Guid("c0fe2438-b76a-4bcb-9cde-d628b5c3deb6"), true, null, "Add User Permission", "إضافة صلاحية للمستخدم", "AddUserPermission", null, "Add User Permission", "إضافة صلاحية للمستخدم", false },
                    { new Guid("c3da80d1-9288-4ebe-96e7-7210cbeeb1ca"), true, null, "Delete Contact Type", "حذف نوع جهة الاتصال", "DeleteContactType", null, "Delete Contact Type", "حذف نوع جهة الاتصال", false },
                    { new Guid("cbcff607-eac1-45e8-81e7-4626a165a1b9"), true, null, "Add New School Admin List", "إضافة قائمة مسؤولي المدارس", "AddNewSchoolAdminList", null, "Add New School Admin List", "إضافة قائمة مسؤولي المدارس", false },
                    { new Guid("cdc3c1c4-4598-44f9-904f-b5d19e31f328"), false, null, "Delete Grade", "حذف المستوى الدراسي", "DeleteGrade", null, "Delete Grade", "حذف المستوى الدراسي", true },
                    { new Guid("cf67bebe-a01e-46e0-9b8e-1edc0cee2087"), true, null, "Edit Relations", "تحرير العلاقات", "EditRelations", null, "Edit Relations", "تحرير العلاقات", false },
                    { new Guid("d0b2932f-717a-4ad7-83a8-c11f1806237d"), true, null, "Edit Role", "تعديل مستوى الامان", "EditRole", null, "Edit Role", "تعديل مستوى الامان", false },
                    { new Guid("d330df2e-9b66-46a0-a64d-88b1d4f9519d"), true, null, "Edit Coin Type", "تعديل نوع العملة", "EditCoinType", null, "Edit Coin Type", "تعديل نوع العملة", false },
                    { new Guid("d65974e4-239a-4683-8b07-5110270e04f6"), true, null, "Add User Role", "إضافة صلاحية للمستخدم", "AddUserRole", null, "Add User Role", "إضافة صلاحية للمستخدم", true },
                    { new Guid("d9bb6050-186d-4052-a3dc-6d33a84424ac"), true, null, "Create Coin Type", "إنشاء نوع عملة", "CreateCoinType", null, "Create Coin Type", "إنشاء نوع عملة", false },
                    { new Guid("daf8b889-ee84-45e2-9c59-8135cd5551a7"), false, null, "Link Employee Class", "ربط الموظف بالفصل", "LinkEmployeeClass", null, "Link Employee Class", "ربط الموظف بالفصل", true },
                    { new Guid("de402af3-0c36-4204-aa5b-df56f8033580"), true, null, "Remove Permissions From Role", "إزالة صلاحيات من مستوى الامان", "RemovePermissionsToRole", null, "Remove Permissions From Role", "إزالة صلاحيات من مستوى الامان", false },
                    { new Guid("e291a793-6efa-40e1-878d-7c11095a6c65"), false, null, "Delete Year Month", "حذف شهر الدراسي", "DeleteYearMonth", null, "Delete Year Month", "حذف شهر الدراسي", true },
                    { new Guid("e4c90ea7-7f44-4c81-82b7-b1ddd979b9cd"), true, null, "View School Action List", "عرض قائمة الإجراءات المدرسية", "ViewSchoolActionList", null, "View School Action List", "عرض قائمة الإجراءات المدرسية", false },
                    { new Guid("e9c02932-b613-45eb-9e71-7cb6204745d9"), false, null, "Delete School", "حذف المدرسة", "DeleteSchool", null, "Delete School", "حذف المدرسة", true },
                    { new Guid("ebde1c53-4840-484f-ac23-df838e6282dc"), true, null, "View Payments Requests", "عرض طلبات الدفع", "ViewPaymentsRequests", null, "View Payments Requests", "عرض طلبات الدفع", false },
                    { new Guid("ec2c9457-2184-4b45-b271-a90a461a816e"), false, null, "View School Payment Request", "عرض طلب دفعة مدرسية", "ViewSchoolPaymentRequest", null, "View School Payment Request", "عرض طلب دفعة مدرسية", true },
                    { new Guid("f4c86ead-e913-4c5e-b1a5-3e17a647c216"), true, null, "Create School Contact Type", "إنشاء نوع جهة اتصال للمدرسة", "CreateSchoolContactType", null, "Create School Contact Type", "إنشاء نوع جهة اتصال للمدرسة", false },
                    { new Guid("f6821a1f-cb86-445d-b7aa-b7224bd11b47"), true, null, "Add Application Employees", "إضافة موظفين للتطبيق", "AddApplicationEmployees", null, "Add Application Employees", "إضافة موظفين للتطبيق", false },
                    { new Guid("f76d6cd8-4a98-4eba-9d09-9204009d7839"), false, null, "Update School Payment", "تحديث دفعة مدرسية", "UpdateSchoolPayment", null, "Update School Payment", "تحديث دفعة مدرسية", true },
                    { new Guid("fb4f053d-67fa-4ff2-b85f-a4bf8f385bce"), true, null, "Remove Language From School", "إزالة اللغة من المدرسة", "RemoveLanguageFromSchool", null, "Remove Language From School", "إزالة اللغة من المدرسة", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("006edbb6-657d-41cf-9e01-8ab9cf4cea69"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("061ac24b-3828-4360-833f-ef5865712e39"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("09756cb3-4363-4763-812a-13f1f8a3b693"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("132ec233-d6ef-4187-a780-e53c85d1babb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16b49575-3825-4489-9822-a64615fe8898"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("19ea90dc-1ed5-4445-a238-9c95e2f37842"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21e7443f-566f-452e-a246-4e65260b16f4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("23a9e98e-ba77-48bb-8f92-73fae4df3245"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26b6aad5-3434-4538-867e-165d70800fb9"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("27daf2ff-556d-4515-a41f-dead5699f5ab"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2989c158-ef37-40f8-bdf8-637c874f2f1e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2b656fc9-b90a-4b22-b882-672495224220"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("37fc9ece-b339-446e-beba-a81b71302266"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3992521c-f222-4323-8969-94f23987f157"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("44c76680-9b72-48f9-bf3d-f113c4447331"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("512d2ebf-dd4a-482b-8753-1252fe196511"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("55f6b349-57d1-40d0-bb56-64c1592c2268"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5e48c040-34e8-4905-9b15-3f4069e39840"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5ecbb154-8466-4ea1-8b66-8aa1e7d7853a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("666f4016-4375-4ca0-ba48-6c1cab50f91a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("687a31c3-6d21-4f8b-9860-379b2a563758"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("889bc51f-0624-4911-988d-4a86f1a4f011"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8e296936-5161-4b61-885c-b520fe350c9a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("913eeed8-4d9d-4778-a84d-3178b36fedbb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9d7ec7f9-0341-48a6-8db5-7486bca97497"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a1bd5ef1-6a55-4e1e-8bb8-da39344e4412"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a8704499-413d-4ff4-a3a2-122b684a0e17"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ac0c5e27-8f01-4b94-bef0-88feaae2043a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("af2eedf8-ee6d-464b-8551-743f6ef3d3b5"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b357214a-296e-4999-be38-2af4259d3096"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c0fe2438-b76a-4bcb-9cde-d628b5c3deb6"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c3da80d1-9288-4ebe-96e7-7210cbeeb1ca"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cbcff607-eac1-45e8-81e7-4626a165a1b9"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cdc3c1c4-4598-44f9-904f-b5d19e31f328"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cf67bebe-a01e-46e0-9b8e-1edc0cee2087"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d0b2932f-717a-4ad7-83a8-c11f1806237d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d330df2e-9b66-46a0-a64d-88b1d4f9519d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d65974e4-239a-4683-8b07-5110270e04f6"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d9bb6050-186d-4052-a3dc-6d33a84424ac"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("daf8b889-ee84-45e2-9c59-8135cd5551a7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("de402af3-0c36-4204-aa5b-df56f8033580"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e291a793-6efa-40e1-878d-7c11095a6c65"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e4c90ea7-7f44-4c81-82b7-b1ddd979b9cd"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e9c02932-b613-45eb-9e71-7cb6204745d9"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ebde1c53-4840-484f-ac23-df838e6282dc"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec2c9457-2184-4b45-b271-a90a461a816e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f4c86ead-e913-4c5e-b1a5-3e17a647c216"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f6821a1f-cb86-445d-b7aa-b7224bd11b47"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f76d6cd8-4a98-4eba-9d09-9204009d7839"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fb4f053d-67fa-4ff2-b85f-a4bf8f385bce"));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("77127635-8fe8-4451-ac9c-59c38ee02d4d"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { null, "" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee6325a4-4bc4-4aa7-9944-f82c3b7a305a"),
                columns: new[] { "Description_AR", "Name_AR" },
                values: new object[] { null, "" });
        }
    }
}
