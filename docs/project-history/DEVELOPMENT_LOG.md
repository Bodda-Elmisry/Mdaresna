# سجل التطوير والنقاشات

السجل Append-only: تُضاف الإدخالات الجديدة في الأعلى بعد المقدمة مباشرة.

## 2026-09-18 — جعل Identity مصدر الهاتف الرئيسي في ملف المدرسة

- النوع: تصحيح معماري وتكامل Backend.
- النطاق: Platform API وSchools API.
- النتيجة: إضافة Platform internal endpoint يعيد الهاتف الرئيسي الحالي من Identity، واستخدامه داخل Schools عند قراءة جهات الاتصال ودمجه مع الجهات الإضافية المحلية فقط.
- استجابة Schools تتجاهل أي نسخة `IsPrimary` قديمة في `person_contacts`، كما يتحقق إنشاء أو تعديل هاتف إضافي من عدم مطابقته للهاتف الرئيسي في Identity.
- لا توجد migration جديدة.
- التحقق: Platform API build ناجح مع warning قديم غير متعلق في `SchoolsController`، وSchools API build ناجح بدون warnings، و9 Schools tests ناجحة.

## 2026-09-18 — جهات الاتصال في البيانات الشخصية للمدرسة

- النوع: ميزة Backend وواجهة.
- النطاق: Schools API وSchools Web.
- النتيجة: إضافة عرض وإضافة وتعديل وحذف جهات الهاتف والبريد والعنوان للمستخدم الحالي بنفس تجربة Platform، مع إبقاء جهة الاتصال الرئيسية للهوية للقراءة فقط ومنع التكرار والتحقق من صحة القيم.
- استُخدم جدول `school.person_contacts` الحالي، لذلك لا توجد migration جديدة.
- التحقق: Schools API build ناجح بدون warnings، و9 Schools tests ناجحة، وSchools Web lint وproduction build ناجحان.

## 2026-09-18 — توحيد Data Grid المستخدمين وإضافة الفلاتر

- النوع: تحسين واجهة + ميزة بحث.
- النطاق: Schools API وSchools Web.
- النتيجة: إعادة تصميم جدولي المدرسين والموظفين بنفس نمط موظفي Platform، مع الهوية والصورة وrole chips والحالات والإجراءات وpage size ونطاق النتائج والتنقل الكامل.
- أضيفت فلاتر Server-side للاسم/اسم المستخدم/الهاتف والحالة والدور، ولوحة فلاتر قابلة للفتح والمسح والتطبيق.
- أضيف endpoint محمي لقراءة صورة مستخدم المدرسة من جدول المستخدمين.
- التحقق: Schools API build ناجح بدون warnings، و9 Schools tests ناجحة، وSchools Web lint/build ناجحان.

## 2026-09-17 — حماية الحساب الحالي وآخر مدير مدرسة

- النوع: إصلاح صلاحيات وقواعد عمل.
- النطاق: Schools API وSchools Web.
- النتيجة: إخفاء إجراءات الإدارة للحساب الحالي ورفض تعديل الحساب نفسه أو تعطيله أو حذفه من Users API.
- أصبحت قاعدة آخر مدير تشترط وجود مستخدم فعال من نوع `Employee` يحمل دور `School Admin`، وتمنع إزالة الدور أو تحويل آخر مدير إلى `Teacher` أو تعطيله أو حذفه.
- التحقق: Schools API build ناجح بدون warnings، و9 Schools tests ناجحة، وSchools Web lint/build ناجحان.

## 2026-09-17 — إدارة المدرسين والموظفين وربطهم بـIdentity

- النوع: ميزة + قرار تكامل.
- النطاق: Platform API وSchools API وقاعدة المدرسة وSchools Web.
- النتيجة: إضافة تبويبي المدرسين والموظفين داخل الإعدادات مع pagination والإضافة والتعديل والتفعيل/الإيقاف والحذف ورسالة تأكيد الحذف.
- الإضافة تطلب الاسم المعروض واسم المستخدم ورقم الهاتف الرئيسي والأدوار. Platform يبحث عن Identity account بالهاتف، ينشئه عند عدم وجوده، ويتأكد أن الرقم هو الـprimary phone قبل ربطه بالمستخدم المحلي.
- المستخدم المحلي يبدأ `PendingActivation` وتُرسل له رسالة باسم الدخول الكامل `username@schoolCode`؛ ثم يستخدم مسار OTP الحالي لتعيين كلمة المرور وتفعيل الحساب.
- بعد نجاح OTP المحلي يُحدّث Platform الهاتف الرئيسي إلى verified ويحوّل Identity account الجديد من `PendingVerification` إلى `Active` بشكل idempotent.
- تمت حماية الحساب الحالي وآخر School Admin من التعطيل أو الحذف، ومنع التفعيل الإداري قبل إنشاء كلمة المرور.
- migrations المضافة: PostgreSQL `20260917182119_AddSchoolUserKind` وSQL Server `20260917182128_AddSchoolUserKind`.
- التحقق: Platform API وSchools API build ناجحان، 176 Platform tests و9 Schools tests ناجحة، Schools Web lint/build ناجحان، ولا توجد pending model changes لمزودي Schools.

## 2026-09-17 — تعديل Display Name لمستخدم المدرسة

- النوع: إصلاح واكتمال ميزة.
- النطاق: Schools API وSchools Web.
- النتيجة: إضافة `DisplayName` كحقل قابل للتعديل في البيانات الشخصية، مع validation وتحديث `Person.DisplayName` المحلي وإعادة تحميل الملف بعد الحفظ.
- التحقق: Schools API build ناجح بدون warnings، وSchools Web lint/build ناجحان.

## 2026-09-17 — إصلاح تحميل البيانات الشخصية

- النوع: إصلاح واجهة.
- النطاق: Schools Web.
- النتيجة: فتح الإعدادات يعيد طلب `GET /api/schools/v1/me/profile`، وأصبح للفشل state وزر retry بدل التحميل اللانهائي.
- شريط تبويبات الإعدادات أصبح full width والزران يقتسمان العرض.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-17 — إعدادات Schools Web والملف الشخصي والأدوار

- النوع: ميزة + ربط واجهة وBackend.
- النطاق: Schools API، قاعدة المدرسة، Schools Web.
- النتيجة: تقسيم الإعدادات إلى تبويب البيانات الشخصية وتبويب الأدوار.
- البيانات الشخصية تدعم تاريخ الميلاد والنوع ورفع/عرض/حذف صورة JPEG أو PNG أو WebP حتى 2 MB، وتُحفظ الصورة في جدول `school.person_profile_images` داخل قاعدة المدرسة.
- تبويب الأدوار يستخدم School Roles/Permissions APIs الحالية ويحترم `school.roles.view` و`school.roles.manage`، مع إنشاء الأدوار وتعديل صلاحياتها وحذف غير النظامي منها.
- migrations المضافة: PostgreSQL `20260917175418_AddSchoolPersonProfileImage`، SQL Server `20260917175429_AddSchoolPersonProfileImage`.
- التحقق: Schools API build ناجح، 9 Schools tests ناجحة، Schools Web lint/build ناجحان، ولا توجد pending model changes للمزودين.

## 2026-09-17 — مطابقة قوائم الحساب والإشعارات بين Schools وPlatform Web

- النوع: تحسين واجهة + إصلاح تفاعل.
- النطاق: Schools Web.
- النتيجة: تحويل أيقونتي الحساب والإشعارات إلى dropdown panels بنفس نمط Platform Web، مع الإغلاق عند الضغط خارج القائمة أو Escape وإغلاق القائمة المقابلة تلقائيًا.
- لوحة الحساب تعرض هوية المستخدم والأدوار، تفتح صفحة الملف الشخصي، تحتوي إعداد اللغة، وتنفذ تسجيل الخروج.
- لوحة الإشعارات تعرض empty state صحيحة بدون unread badge وهمي؛ ربط قائمة الإشعارات الفعلية ينتظر إضافة School Notifications API.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-17 — ربط بيانات وهوية Schools Web

- النوع: إصلاح + ربط واجهة.
- النطاق: Schools API وSchools Web.
- النتيجة: إضافة endpoint محمي لقراءة `SchoolInformation` من قاعدة المدرسة، وحفظ جلسة تسجيل الدخول في الويب، وعرض اسم وبيانات المدرسة الفعلية.
- أيقونة الإشعارات تنقل إلى قسم الإشعارات، وأيقونة الحساب تفتح بيانات المستخدم وتوفر تسجيل الخروج مع إبطال refresh token.
- التحقق:
  - Schools API build ناجح بدون warnings أو errors.
  - Schools unit tests: 9 ناجحة.
  - Schools Web build ناجح.
  - Schools Web lint ناجح.

## 2026-09-17 — الإدارة المركزية لـSchools migrations

- النوع: قرار + ميزة.
- النطاق: Platform API، Platform Worker، Schools API، Platform Web، Platform Flutter.
- النتيجة: إضافة تحديث قاعدة مدرسة واحدة أو كل المدارس الفعالة من Platform، مع تنفيذ asynchronous عبر Outbox وRabbitMQ وتسجيل النتيجة في `school_database_endpoints`.
- تم إلغاء تشغيل migrations من مسار طلب OTP لضمان وجود نقطة تشغيل واحدة فقط للمدارس الموجودة.
- تمت إضافة الصلاحية `platform.schools.migrations.execute` ومنحها لدور `app-manager` في migration قاعدة Platform.
- migrations المضافة:
  - SQL Server: `20260917171834_AddSchoolDatabaseMigrationControl`.
  - PostgreSQL: `20260917171848_AddPostgreSqlSchoolDatabaseMigrationControl`.
- التحقق:
  - Platform unit tests: 176 ناجحة.
  - Schools unit tests: 9 ناجحة.
  - Platform Web: build ناجح و32 اختبارًا ناجحًا.
  - Platform Flutter: analyze بدون مشاكل و28 اختبارًا ناجحًا.
  - Platform API وWorker وSchools API: build ناجح.
  - EF: لا توجد pending model changes في Platform SQL Server أو PostgreSQL.

## 2026-09-17 — تعديل تفعيل مالك المدرسة

- النوع: قرار + ميزة + إصلاح.
- النطاق: Platform وSchools backend، Schools Web، Schools Flutter.
- النتيجة: provisioning يرسل اسم الدخول الكامل بدون OTP. المستخدم يطلب OTP عند التفعيل، ثم يرسل OTP وكلمة مرور جديدة.
- تمت إضافة cooldown لمدة دقيقة، صلاحية 10 دقائق، حد 5 إرساليات خلال 24 ساعة، وحد 5 محاولات خاطئة.
- يدعم Platform Worker استهلاك provisioning schema 2 القديم بدون استخدام OTP القديم، بينما الإصدار الحالي schema 3.
- التحقق: builds واختبارات Backend وReact وFlutter ناجحة وقت التنفيذ.

## 2026-09-17 — Provisioning قواعد المدارس وdatabase registry

- النوع: نقاش + إصلاح.
- النتيجة: Schools consumer ينشئ PostgreSQL database، يشغل migrations، ينشئ `SchoolInformation` والمستخدم المحلي، ثم يعيد النتيجة. Platform يسجل endpoint في `school_database_endpoints` ويحوّل المدرسة إلى Active.
- مصدر إعداد الاتصال الحالي: `SchoolProvisioningDatabase:AdminConnectionString` في Schools runtime، وليس connection string داخل Platform table.
- Registry يخزن host/port/database/secret reference ولا يخزن credentials.

## 2026-09-17 — RowVersion مع PostgreSQL

- النوع: إصلاح.
- النتيجة: PostgreSQL Schools contexts تستخدم `xmin` كـconcurrency token بدل SQL Server `rowversion` byte array.
- SQL Server يحتفظ بـ`IsRowVersion()`.
- أضيفت migration لإزالة أعمدة RowVersion القديمة من PostgreSQL.

## 2026-09-17 — Firebase والإشعارات الفورية

- النوع: إصلاح + نقاش.
- النتيجة: إصلاح WebPush link غير الصالح، وربط استقبال Firebase المباشر بتحديث عداد الإشعارات وrefresh signals في Web وFlutter دون انتظار click داخل التطبيق.
- طلب notification permission أصبح عند بدء التطبيق، وتم تفعيل browser notifications للويب.

## 2026-09-17 — إعدادات Docker والنشر

- النوع: نقاش + إعداد.
- النتيجة: Platform API وPlatform Worker قابلان للتشغيل على Linux containers.
- قرار نشر: تمرير الأسرار كـenvironment variables أو secret manager، ومنها نفس `PlatformSms__EncryptionKey` للـAPI والـWorker.
- متابعة: تضمين PostgreSQL وRabbitMQ وFirebase وSMS وinternal keys وhealth checks والمigrations والrollback في خطة النشر النهائية.
