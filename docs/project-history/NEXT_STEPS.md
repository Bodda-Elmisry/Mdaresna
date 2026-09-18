# الخطوات التالية

آخر تحديث: 2026-09-18

## مكتمل مؤخرًا

- اكتملت الهيكلة المادية في Backend وReact وFlutter مع الصلاحيات وsoft delete والاستعادة وmigrations للمزودين.
- اكتملت الهيكلة الأكاديمية والتقويم والتشغيل في Backend وReact وFlutter مع الصلاحيات وCRUD والتفعيل والإيقاف وsoft delete والاستعادة وmigrations للمزودين.
- اكتمل ربط جهات الاتصال داخل البيانات الشخصية: الهاتف الرئيسي يُقرأ من Identity، والجهات الإضافية تُدار محليًا داخل قاعدة المدرسة، بدون migration جديدة.

## قبل تجربة الإدارة المركزية للمigrations

- تطبيق أحدث Platform PostgreSQL migration على قاعدة Platform.
- تشغيل RabbitMQ وPlatform API وPlatform Worker وSchools API.
- تم التحقق محليًا من اكتمال Action المدرسة وتحول `MigrationStatus` إلى `Succeeded` وتحديث `SchemaVersion` إلى `20260917214728_AddSchoolFacilities` بعد تشغيل Worker وconsumer.
- تجربة حالة فشل آمنة والتأكد من تسجيل `LastMigrationError` وإمكانية إعادة المحاولة.

## تحسينات قريبة

- تطبيق أحدث Schools migration على قواعد المدارس من Action التحديث المركزي حتى تظهر أقسام الهيكلة الأكاديمية والتقويم والتشغيل وصلاحياتها في المدارس الموجودة.
- إضافة School Notifications API والتخزين والعداد وربط Firebase/Realtime بقائمة Schools Web الحالية.
- عرض حالة آخر migration وإصدار قاعدة المدرسة داخل شاشة المدارس بدل الاكتفاء برسالة قبول الطلب.
- إضافة polling أو notification للواجهة عند اكتمال عملية migration asynchronous.
- إضافة معالجة إدارية للعمليات التي تبقى `Pending` مدة طويلة، مع retry آمن ومراقب.
- إضافة integration tests تشمل RabbitMQ ومسار command/result كاملًا.

## خطة النشر المستقبلية

- توثيق Docker Compose/Cloud deployment للـPlatform API والWorker وSchools API.
- تمرير الأسرار من secret manager وعدم استخدام user-secrets في الإنتاج.
- تضمين health checks، database migrations، النسخ الاحتياطي، rollback، ومراقبة queues وdead-letter queues.
