# ربط ملفات المدارس بـ Cloudflare R2

تاريخ التنفيذ: 2026-10-15. المزود مضاف في الكود؛ لا يعني ذلك تفعيل حساب أو اتصال فعلي. التخزين الافتراضي Local حتى التبديل الصريح. لا تغيير لقواعد البيانات أو لمفاتيح التشفير.

## إعداد التطوير

أنشئ مخزن Standard خاصًا مثل `mdaresna-schools-dev` ومفتاح Object Read & Write مقيدًا به. يبقى public access معطلًا؛ لا CORS أو public domain مطلوبان لاتصال الخوادم. إعداد SDK وفق https://developers.cloudflare.com/r2/examples/aws/aws-sdk-net/؛ تعطيل payload signing/checksum الخاصين بـ AWS streaming لا يعطل HTTPS أو AES-GCM التطبيقي.

من جذر مستودع Mdaresna وفي PowerShell 7:

```powershell
./deploy/Set-SchoolR2Secrets.ps1
```

أدخل عنوان S3 للحساب فقط (بلا اسم المخزن)، واسم المخزن وAccess Key ID وSecret Access Key عند الطلب. الحقلان الأخيران مخفيان ويمران إلى dotnet عبر stdin، لا command line أو ملف داخل المستودع. Token Value وAccount ID ليسا إعدادين مطلوبين منفصلين؛ المعرف موجود في endpoint. السكربت لا يغير provider افتراضيًا. user-secrets خارج Git لكنها ليست مخزنًا مشفرًا ولا تصلح للإنتاج؛ احمِ حساب Windows والجهاز ولا تطبع محتواها.

اختبار حقيقي اختياري، **ينشئ ويحذف ملفًا عشوائيًا واحدًا في مخزن dev/test** ويستخدم مفتاح تشفير مؤقتًا خاصًا بالاختبار:

```powershell
$env:MDARESNA_R2_LIVE_TEST = '1'
try {
    dotnet test tests/Mdaresna.Schools.UnitTests --filter FullyQualifiedName~R2LiveStorageTests -v minimal
} finally {
    Remove-Item Env:MDARESNA_R2_LIVE_TEST -ErrorAction SilentlyContinue
}
```

يعيد الاختبار تحقق list/read/write/delete وencrypted roundtrip وseek/hash. لا يختبر فك ترميز PDF/MP4 أو صلاحيات HTTP للمستخدمين. إذا فشل cleanup راجع بادئة المدرسة التي تبدأ بـr2smoke في مخزن التجربة واحذف ملف الاختبار فقط. لا يقرأ ملفات مدرسية فعلية ولا يغير مفاتيحها.

بعد نجاح الاختبار وترحيل أي ملفات موجودة وفق القسم التالي، تفعيل التطوير:

```powershell
dotnet user-secrets set 'SchoolDocuments:Provider' 'R2' --project src/Schools/Mdaresna.Schools.Api
```

أعد تشغيل Schools API ببيئة Development. اختبر رفع مستند فعلي وصورة وPDF وMP4 من الواجهات الحالية، وRange والتنزيل ومنع مستخدم/مدرسة غير مصرحين. صور الحسابات داخل DB لا تنتقل بهذا المزود.

## إعدادات النشر

| المفتاح | المصدر/القيمة |
|---|---|
| SchoolDocuments:Provider | Local أو R2؛ الافتراضي Local |
| SchoolDocuments:R2:Endpoint | عنوان HTTPS S3 للحساب، وendpoint الخاص بالـjurisdiction عند استخدامه |
| SchoolDocuments:R2:Bucket | اسم المخزن الخاص بالبيئة |
| SchoolDocuments:R2:AccessKeyId | مصدر أسرار النشر |
| SchoolDocuments:R2:SecretAccessKey | مصدر أسرار النشر |
| SchoolDocuments:R2:MaxUploadMiB | 64 افتراضيًا، 1–256، حد بايتات ciphertext للرفع |
| SchoolDocuments:R2:ConcurrentUploads | 2 افتراضيًا، 1–8 لكل process |
| UploadEncryption:ActiveKeyId / Keys | مفاتيح التشفير الحالية من مصدر الأسرار، بلا استبدال أثناء التبديل |

Compose يمرر المقابلات SCHOOL_DOCUMENT_STORAGE_PROVIDER وSCHOOL_DOCUMENT_R2_ENDPOINT وSCHOOL_DOCUMENT_R2_BUCKET وSCHOOL_DOCUMENT_R2_ACCESS_KEY_ID وSCHOOL_DOCUMENT_R2_SECRET_ACCESS_KEY وSCHOOL_DOCUMENT_R2_MAX_UPLOAD_MIB وSCHOOL_DOCUMENT_R2_CONCURRENT_UPLOADS. القوالب لا تحوي أسرارًا حقيقية؛ ملف env النشر المحمي أو secret manager يملأ القيم. User-secrets الجهاز ليست داخل Docker. افحص الإعداد دون طباعة `docker compose config` الموسع المحتوي على الأسرار. Local volume باقٍ للرجوع المدروس ولا يُحذف تلقائيًا.

## الحدود التشغيلية

- الرفع يشفر في buffer seekable محدود، بلا ملفات plaintext مؤقتة. افتراضيًا حتى 128MiB ciphertext staging عبر رفعين، إضافةً إلى multipart المدخل وذاكرة التطبيق. اضبط حدود الرفع وذاكرة الحاوية والتوازي معًا. حجم الملف الأصلي أقل قليلًا من حد ciphertext بسبب غلاف التشفير. حد multipart الحالي 54MiB، ولا يرفعه اختيار 256MiB هنا.
- القراءة تستخدم HEAD ثم GET Range مع صفحة ciphertext واحدة حتى256KiB لكل stream وIf-Match للـETag؛ لا تنزيل كامل قبل عرض جزء. ETag لضمان ثبات النسخة لا بصمة plaintext؛ تبقى Sha256 والحجم الأصلي من التشفير. ذاكرة القارئ تزيد حسب القراءات المفتوحة؛ لا cache موزع للملفات.
- حد16 عملية HEAD/range متزامنة لكل process، timeout30s وretry محدود في SDK. القراءة async في cipher أصبحت فعلية لتجنب حجز thread أثناء الشبكة؛ مستخدمو Stream المتزامنون يظلون متزامنين. تتبع الحمل وطلبات R2 وp95 تحت اختبار واقعي.
- readiness يتحقق من التشفير واتصال provisioning DB وlist محدود بمهلة3s في R2 دون كتابة/حذف دوري. لا يثبت صلاحية الرفع والحذف؛ الاختبار الصريح أعلاه يختبرهما. فشل403/الشبكة لا يتحول إلى missing file أوLocal fallback؛ 404 للقراءة يعيد غير موجود.
- أخطاء إعداد R2 ترفض التشغيل، ولا تتضمن القيم الحساسة. لا روابط عامة أوpresigned URL للعميل؛ فك التشفير والتصريح عبر Backend الحالي.

## ترحيل الملفات والرجوع

لا تبدّل provider على قاعدة لها مرفقات محلية قبل نقلها والتحقق منها؛ اختيار R2 يقرأ R2 فقط. لا توجد أداة ترحيل تلقائي في هذه الدفعة، ولا تم نقل/حذف أي ملف موجود.

1. احتفظ بنسخة احتياطية منDB والملفات ومفاتيح UploadEncryption، واختبر الاستعادة، ثم أوقف الكتابة خلال القطع النهائي.
2. استخرج قائمة storageKey والإصدار والحجم وSha256 من قواعد المدارس. المستندات القديمة غير المشفرة تعالج أولًا بإجراء التشفير السابق في UPLOAD_ENCRYPTION_AND_DIGITAL_BOOKS.md.
3. انسخ ciphertext كما هو إلى المخزن، بنفس storageKey حرفيًا، دون إضافة مسار محلي أو تغيير مفتاح التشفير. استخدم أداة S3 موثوقة بإعداد أسرار محمي، وسجل تقدمًا قابلًا للاستئناف دون أسرار. لا تستبدل كائنًا موجودًا مختلفًا تلقائيًا.
4. تحقق عبر R2 Backend من فك كل مستند ومطابقة الحجم وSHA256 الأصليين؛ تحقق من الصور وPDF والفيديو وRange ومنع الوصول، ثم بدّل كل نسخ Schools لنفس المزود والمخزن وأعد التشغيل.
5. احتفظ بالأصل المحلي والنسخة الاحتياطية حتى قبول الترحيل. الرجوع إلى Local بعد رفع ملفات جديدة في R2 يحتاج إعادة مزامنتها أولًا؛ تبديل الإعداد وحده يفقد الوصول إليها. حذف الأصول إجراء مستقل بعد التحقق والاعتماد.

Family Backend وتفويض مفاتيح فك التشفير ومزامنة مراجع الملفات عبر RabbitMQ ليست منفذة بهذه الدفعة. المفاتيح لا تنتقل عبر الأحداث؛ Family يحصل على مفتاح R2 منفصل بصلاحيات احتياجاته عند تنفيذه.
