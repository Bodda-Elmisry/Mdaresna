# سجل التطوير والنقاشات

## 2026-10-15 — حفظ الخطة التقنية للأنظمة والباقات والسعات في ملف Markdown

- بطلب المستخدم أُنشئ `docs/implementation/SYSTEM_SUBSCRIPTIONS_AND_CAPACITY_TECHNICAL_PLAN.md` من523سطرًا و15قسمًا، مبنيًا على جرد محدود للمصدر الحالي. يفصل المتطلبات المحسومة عن P01–P12، ويحدد الكتالوج وإصداراته والعقود والعروض ومراجعة الدفع وAPI والأحداث والتهيئة وملكية الاستخدام والسعة الذرية والواجهات والترحيل والتقاعد والاختبارات وحزم W0–W8.
- يعالج ترتيب الباقات المستقل عن السعر، ومنع تكرار المنح والتخصيص، وتزامن تغيير الحد مع التفعيل، وتجديد الفترات ونقل الاستخدام وفق السياسة، وفصل حساب المدرس عن HR. إزالةالوحدات تشمل schema/API/الموافقة/provisioning والأحداث مع محفوظات مالية كاملة وخطة expand/contract وpilot/rollback؛ لا حاجة لترحيلR2 أوFamily كاعتماد سابق.
- رُبط المرجع بـCURRENT_STATE وADR-058، واختُصر تكرارالخطة فيNEXT_STEPS إلى روابط وحزم عمل حتى يبقى التفصيل في مصدر واحد. لا تنفيذAPI/واجهات/migrations ولا قيم أسرار أوتغييرأسعار فعلي.
- التحقق: قراءةالسياق بالترتيب ثمrg/Get-Content لمواضعالتأثير ومرجعخطةالواجبات؛ فحص15عنوانًا و12سياجMarkdown متوازنًا وتحليلJSON المثال وحساب425×87.5=37187.50 و12×115=1380.00. git diff --check ناجح معتحذيراتLF/CRLF فقط. لاbuild أوtests تشغيلية لتغييرتوثيق فقط؛ أوامرالتحقق فيالخطة مستقبلية وموسومةكذلك.
## 2026-10-15 — تأكيد إنهاء نموذج الوحدات القديم

- أكد المستخدم أن اعتماد الأنظمة والباقات يتضمن إزالةالوحدات. عُدّلADR-058 والحالة والخطة لتشمل إزالةالواجهات وAPI والشراءوالأرصدة واعتمادالوحدةبالموافقةوالتهيئةوالعقود/الأحداث؛ليس مجردتغييرمسمى أوالإبقاءعلىنموذجينتشغيليين.
- الإزالةتتبعترحيلالمدارسوالطلباتالمعلقة وتصريفالرسائلالقديمة وحفظتاريخالمشترياتوالمدفوعات،ثمتنظيفالكودوالجداولالزائدةبمهاجرةلاحقة. لاكودأوبياناتحُذفتالآن؛الطلبتوضيحلخطةالتنفيذ. تحقققراءةالسياقبترتيبهوgit diff --check للتوثيقفقط.
## 2026-10-15 — توضيح كتالوج الأنظمة والباقات والأسعار والترتيب القابل للتعريف

- صحح المستخدم أنالأسعاروالسعاتلايجبأنتكونثابتةبالكود؛يريدإدارةالأنظمةوإتاحاتها منPlatform،وتعريفcustom plan بسعتهاوسعرالفردداخلها وسعرالفردبالإضافات وترتيبالباقةلتحديدالباقةالتالية. الأرقامالسابقةأمثلةقابلةللتغيير؛ثباتالسعةالمتعاقدعليهامستقلعنتغييرالكتالوج.
- عُدّلتخطةNEXT_STEPS وحالةالمشروع وأضيفتوضيحADR-058: SystemDefinition/إتاحاتمدعومة وPlan بسعةوBaseUnitPrice وAddOnUnitPrice وSortOrder/version؛حسابالإجماليفيالخادم وإدارةترتيبداخلالنظام واختيارباقاتنشطةلاحقةصالحة. الترقيةتحتاجسياسةفرقسعرونقلالإضافاتووقتسريان؛لاافتراضترقيةآليةأوفقدالمقاعد. إتاحاتاللوحةتربطوظائفمدعومةلاوظائفمنشأةديناميكيًا.
- التحقق: قراءةملفاتالسياقبترتيبهاوتحديثالتوثيق وgit diff --check؛لاAPI/DB/أسعارفعليةأوواجهاتتنفيذيةتغيرت،ولااختباراتتشغيليةلازمةلتعديلالخطةفقط.
## 2026-10-15 — خطة تطبيق التعاقد بالسعات بعد مراجعة المصدر

- طلبالمستخدم خطة،فراجعتسياقالمشروع ثمBilling/Units وReviewSchoolPlatformPaymentCommandHandler وموافقةSchoolsController وعقودProvisionSchoolV1 ومستهلكprovisioning وSchoolStudentsController وواجهاتالمدارسفيPlatform Web/Flutter. الوحدةمستخدمةبالموافقةوالتهيئةوالمدفوعات وليستمجردقائمة؛إنشاءالطالبوقبولالطلبينشئانقيدActive،ويحتاجانفحصسعةموحدًا. لم أعثرعلىاستهلاكSchoolUnitsGrantedV1 داخلSchools فيالبحثالمحدد؛يلزممستهلكعقدجديد وليسافتراضتعديلهائم.
- وُضعتخطةتفصيليةمن8مراحلفيNEXT_STEPS: سياساتغيرمحسومة،كتالوجوعقودمؤرخةوسعات،إعادةاستخداممراجعةالدفعوالـledger،Outbox/RabbitMQ وsnapshot محليversioned،حجزذريداخلSchool DB،تحديثWeb/Flutter،mapping/dry-run وترحيلمتدرجواختباراتمزوديSQL. السجلالقديممحفوظ؛تجهيزHR/Finance لايعنيتنفيذوظائفهماالمؤجلة. سعةالفصلوصلاحياتالمستخدممستقلتانعنترخيصالنظام،وRedis routing لايصبحعدادسعة.
- لا قرارجديدبسنويةالأسعار أوإرجاعالمقعد أوترحيلتلقائي،ولاكود/DB/واجهات/أسرار تغيرت. التحقققراءةمحددةللمصدر وgit diff --check للتوثيق؛لاbuild/tests مطلوبةلتغييرالخطةفقط. بقيترحيلR2 مهمةمستقلة،وتمالتنبيهلتوجيهmigration worker قبلترحيلقواعدموزعةعلىHosts مختلفة.
## 2026-10-15 — حسم سعة الباقات والإضافات وحدود الأنظمة

- أوضح المستخدم أن الباقات سعات ثابتة، وأن إضافات الطالب100 جنيه والموظف50 جنيه بالسعر الأصلي مهما كان خصم الباقة، وأن شراء النظام يفعّل وظائفه كاملة دون اختيار وحدات فرعية. التفعيل يستهلك من سعة المتعاقد عليها. ثُبّتADR-058 وحالة المنتج؛ لا تنفيذ أوحذف لوحدات تقنية أوتغييرعقود.
- أكد أن الأكاديمي يضيف المدرسين والموظفين دونإداري ويديرتكاليف الصف وطرق السداد دونمالي؛ التحصيل يتطلبماليًا. الإداري يحسب الرواتب لكن الصرف يحتاجماليًا وخزنة. نوقش الفصل بينوجودالشخص الأكاديمي واستهلاك اشتراكHR، والحد الإجمالي/المستخدم/المتاح والتفعيل الذري غيرالمكرر كمتطلبات تصميم؛ لا حسمضمني لإرجاع المقاعد أوفترةالفوترة.
- التحقق: قراءةCURRENT_STATE ثمDECISIONS وأحدثDEVELOPMENT_LOG ثمNEXT_STEPS؛ تحديثالتوثيق فقط وgit diff --check. لااختبارات تشغيلية أوتعديلات مصدر/DB/أسرار فيدفعة المناقشة.

## 2026-10-15 — مناقشة التعاقد على ثلاثة أنظمة ومقاييس سعة

- اقترح المستخدم استبدال التسعير بالوحدات بثلاثة أنظمة تجارية: أكاديمي بحسب الطلاب (حتى300 بسعر100،600 بسعر80،800/1000 بسعر60، وأكثر بسعر50 جنيه للفرد)، إداري بحسب الموظفين (حتى50 بسعر50،100 بسعر40،150 بسعر30، وأكثر بسعر20)، ومالي بحسب الخزن (5000 جنيه للخزنة). هذه أرقام مقترحة من المستخدم؛ فترة الفوترة ومعنى الباقة كسعة ثابتة أوعدد فعلي لم يُحسما، ولا تعديل أوتسعيرمعتمد أوترحيل عقود.
- التقييم: الفصل التجاري مناسب مع بقاء التقسيم البرمجي الداخلي. يوصى بتثبيت نظام/حدود/مدة/سعر العقد وإصدار التسعير عند التوقيع؛ الأكاديمي يشمل دليل المدرسين وإسناداتهم دون اشتراط HR، والإداري يحسب استحقاقات الرواتب والمالي يسجل الصرف. تعريف رسوم الصف وطريقة السداد أكاديمي، والتحصيل والتسويات والحركات المالية نطاق مالي يحتاج حدودًا متفقًا عليها.
- حساب مقترحات المستخدم يبيّن ضرورة التمييز بين سعر جميع الأفراد وسعر باقة السعة:300×100=30000 مقابل301×80=24080، و1000×60=60000 مقابل1001×50=50050؛ للموظفين150×30=4500 مقابل151×20=3020. إن كانت سعات ثابتة فـ600 و800 لهما سعر48000 نفسه، ويحتاج تجاوز السعة سياسة صريحة. مقترح شرائح تصاعدية على الزيادة أو باقات ثابتة دون تخفيض إجمالي الفاتورة عند زيادة العدد، لا اعتماد لأرقام بديلة.
- المتبقي قبل التصميم: شهرية/سنوية، سعة متعاقد عليها أم عدد فعلي، تعريف الطالب والموظف المحتسبين وعدم ازدواج الشخص، الفروع والعام والتاريخ والترقية/التجاوز، وتعريف الخزنة والحساب البنكي/المحفظة. تحقق قراءة وثائق الحالة والقرارات والسجل والخطوات وrg لنطاق التعاقد/الوحدات فقط؛ لاكود أواختبارات تنفيذية لأن الطلب مناقشة رأي.
## 2026-10-15 — نجاح اختبار R2 الحقيقي بعد حفظ بيانات الوصول محليًا

- أكد المستخدم بصورة نجاح سكربت حفظ الأسرار؛ شُغّل الاختبار الحي على مخزنdev دون طباعة المفاتيح. المحاولة الأولى فشلت قبلالاتصال لغيابIConfiguration فيServiceCollection الخاصةبالاختبار؛ أُضيف تسجيلها وأضيف تحقق بناءمزودDI فياختبارالإعداد. التطبيق الفعلي يسجلIConfiguration منWebApplication أصلًا؛ لا تغييرسلوكproduction بسببالإصلاح.
- التحقق النهائي: MDARESNA_R2_LIVE_TEST=1 مع dotnet test tests/Mdaresna.Schools.UnitTests --filter 'FullyQualifiedName~R2LiveStorageTests|FullyQualifiedName~R2SchoolDocumentStorageTests.Provider_and_credentials' -v quiet نجح2/2، بمافيه اتصالR2 الفعلي وlist/Put/Head/GetRange/seek وفكالتشفير وSHA256/Delete ثمقراءة404. ملفاختبارعشوائي وحيد حُذف، ومفتاحالتشفيرالمؤقت لميمس مفاتيحالمستندات. متغيرopt-in أزيلفيfinally وgit diff --check ناجح بتحذيراتLF/CRLF فقط.
- لم يتغيرprovider الفعلي أوDB أوملفاتالمدارس، ولم يُنشرالتطبيق. حدّثCURRENT_STATE/NEXT_STEPS بنجاحالاتصال معبقاءbackup/manifest وترحيلciphertext والتحققوالقطع وتجربةPDF/MP4 علىالواجهات قبلالتفعيل. لاAccountId أوendpoint أوقيمsecrets مسجلةبالتوثيق.
## 2026-10-15 — تنفيذ تكامل R2 لمستندات المدارس

- بطلب المستخدم أضيف R2SchoolDocumentStorage وR2RangeStream واختيارProvider معAWSSDK.S3 مثبت، وتشفيرAES-GCM الحالي وبصمة/حجم plaintext. رفع محدودالحجم/التوازي دونplaintext temp وconditional create؛ HEAD/ranged GET محكوم بـETag وContent-Range وصفحة256KiB. فشل الإعداد/الشبكة لا يرجع إلىLocal، و404 وحده يعني عدموجودالملف. cipher async يدعمI/O async معإبطال cache chunk قبل إعادة فكها.
- readiness يقرأR2 list محدودًا بدل كتابةvolume محلي حينR2، معبقاء فحصالتشفير/DB. أضيفت إعداداتCompose/قوالب/preflight وسكربتPowerShell7 لإدخالAccessKeyId/SecretAccessKey مخفيين عبرstdin إلىuser-secrets، ودليل SCHOOL_DOCUMENT_R2.md للتفعيل والترحيل والرجوع وحدودالذاكرة. لا نقل للملفات أوتنفيذFamily أوتغييرأسرارأو.env حقيقي أوتفعيلR2؛ الاختبارالحي ينتظر إعدادالمستخدم المحلي.
- التحقق: dotnet build src/Schools/Mdaresna.Schools.Api --no-restore -v quiet نجح بلاwarnings/errors؛ dotnet test tests/Mdaresna.Schools.UnitTests -v quiet نجح232 ومتخطى5 من237 (منها اختبارR2 الحي دونopt-in). اختباراتR2 وحدها14 ناجحة و1live متخطى: fakeS3 وتدفقAWS SDK الفعلي عبرHttpMessageHandler محاكى، ciphertext/seek/hash/ETag/IfNoneMatch/403/503/حدود/cancellation؛ ليست اتصالCloudflare حقيقيًا.
- deploy/tests/Test-ComposeFixtures.ps1 نجح للمحلي/الإنتاج ورفضTLS المفقود وR2 الناقص وقبولfixtureR2؛ parser لسكربتالأسرار نجح وdotnet user-secrets JSON stdin تحقق بproject/id مؤقتين وقيمsynthetic ثمأزيلت. git diff --check ناجح معتحذيراتLF/CRLF فقط. أصلح أثناءالتنفيذ توقيعAmazonS3Exception فيfixtures وتحميلuser-secrets بالـAssembly قبلالنتيجةالنهائية. لم تُطبقmigrations أوتُرفعملفاتحقيقية؛ يلزمlive smoke ثمترحيلوتجربةPDF/MP4 فعلية قبلالإنتاج.

## 2026-10-14 — تجهيز متطلبات تكاملR2 وحسم مسار Family Server

- أكد المستخدم أنFamily Server يقرأR2 ويفكالتشفير ويرسلللمستخدم، لاclient direct؛ عُدّلADR-054 والخطواتالمقبلة. طلب المستخدمتحديداحتياجاتالتكامل: حسابCloudflare معR2 مفعّل، bucket خاصStandard واختبارمنفصل، endpoint الصحيح منالحساب (region auto)، AccessKeyId/SecretAccessKey لتوكنSchools محدودبـbucket بصلاحيةObject Read & Write؛ Family يحصلعلىtoken مستقلRead only عندتنفيذقراءته. لاdomainعام أوCORS لمسارserver-to-server المتفقعليه، ولاWorkerCloudflare مطلوببذاته.
- مفاتيحR2 فيsecret manager/إعدادنشر محمي لاالشات أوMarkdown؛ مفاتيحUploadEncryption الحالية منفصلةوتبقىثابتةومحفوظة، وترحيلالمستندات يحافظعلىstorageKey والنسخوالبصمة قبلتغييرالمزود. خطةالتنفيذ: R2 provider لـISchoolDocumentStorage، seek/range متوافقمعcipher، readiness مناسب، إعداداتوقوالب واختباراتencrypted roundtrip وPDF/MP4 وفشل/network، معبقاءLocal لخيارالتطويروالترحيل. صورالحسابات المخزنةفيDB ليستتحويلًا تلقائيًا ضمنDocumentCore.
- تحققGet-Content/rg لمسارLocal/Program/UploadEncryptionConfiguration ومراجعةالمراجعالرسمية https://developers.cloudflare.com/r2/api/tokens/ وhttps://developers.cloudflare.com/r2/examples/aws/aws-sdk-net/ وhttps://developers.cloudflare.com/r2/buckets/create-buckets/ . لاكودأوبكتأوkeysأواتصالR2 فعلي أوbuild فيدفعةتحديدالمتطلبات، ولا قيمأسرار سُجلت.

## 2026-10-14 — توضيح قراءة Family لمرفقات الواجب منR2

- حدد المستخدم أنتعامل المدرسةمعالملفات عبرSchools Backend، والواجبومراجعالمرفقات يصلان إلىFamily عبرالأحداث ويحفظان فيFamily DB؛ عندالعرض يقرأFamily منR2 دونطلبSchools Backend. أُضيف التوضيح إلىADR-054 والخطواتالتالية، دونتنفيذR2 أوFamily أوتغييرالكود.
- التوصية: حدث يحفظمعرفمرفق/إصدار ومفتاحتخزين وبياناتنوع/حجم/بصمة وسياسةإتاحة مناسبة، لا بايتات أوaccess keys أوsigned URL طويلالعمر. Family يملكتحققالوصول محليًا، ومزامنةالتعديل/الحذف/سحبالإتاحة ضرورية. الملفاتمشتركة فيR2 والمراجع محليةفيكلتطبيق؛ R2 ليسمصدرصلاحياتالعلاقةبالطالب.
- تنزيلالجهاز مباشرةأوعبرFamily Backend يحتاجتمييزًا فيتصميمالتسليم؛ cipher الحالي يحفظciphertext فلايكفيpresigned URL لإظهارالمحتوى. Family Backend يمكنهقراءةR2 وفكالتشفيربصلاحيةمفوضة دونSchools runtime؛ لااعتمادضمني لتعطيلالتشفير أوتوزيعالمفتاحالرئيسي علىالعملاء. تحقققراءةالسياق ومعطياتمراجعةالتشفير السابقة فقط؛ لاbuild أوتجربةR2 فعلية، وgit diff --check للتوثيق.

## 2026-10-14 — تقييم Cloudflare R2 لتخزين ملفات المدارس

- اقترح المستخدم R2 وطلب الرأي؛ التوصية المبدئية استخدامR2 Standard كObject Storage خارجي للصور والمرفقات والفيديو والكتب، معmetadata والصلاحيات فيSchool DB وbucket خاص ومفاتيحأسرار خارجالمستودع. ISchoolDocumentStorage موجود والمزود الحاليLocalSchoolDocumentStorage؛ لا تكاملR2 أو نقلملفات منفذ بهذهالمناقشة.
- فحصSchoolDocumentStorage وUploadedFileCipher يؤكدAES-GCM chunked وتشفير مرتبط بـstorageKey، ومطلبseekable للرفع والقراءة. مزودR2 يحتاجrange/seek adapter أوbuffering محدود لبايتاتمشفرة، مععدم تحميلملفكامل لكلseek/request أوكسرRange للفيديو، وتحديثreadiness الحالي الذييفحصfilesystem. الترحيل يحافظعلىstorage keys ومفاتيحالتشفير، معhash/قراءة/استعادة قبلحذفالأصل؛ صورالحسابات داخلDB ليستجزءًا تلقائيًا مناستبدالDocument Core.
- نقل الملفات يحل نمو القرص ومشاركة المحتوى بينBackend replicas؛ المرور عبرSchools للمصادقة وفكالتشفير يبقيحملالشبكة/CPU علىالسيرفر، ولايحلrenderingPDF تلقائيًا. Presigned URL للبايتاتالمشفرة لايعرضمحتوىمقروءًا، والتسليم المباشر يحتاجتصميمًا مستقلًا يحافظعلىالصلاحيات والتشفير وسياسةمنعالتنزيل. R2 ليسنسخةاحتياطية مستقلةبذاته.
- راجعناالمصادرالرسمية https://developers.cloudflare.com/r2/pricing/ وhttps://developers.cloudflare.com/r2/api/s3/api/ وhttps://developers.cloudflare.com/r2/api/s3/presigned-urls/ . السعرStandard0.015USD/GB-month معClassA4.50USD/million وClassB0.36USD/million وegressمنR2 مجاني؛ أمثلةالتخزين قبلfree tier والعمليات/الضرائب ولا تشملشبكةسيرفرالتطبيق. GetObject Range مدعوم، ولايفترضتوافقكلS3features. تحققGet-Content/rg فقط؛ لاكود/إعدادات/bucket أوsecrets تغيرت، ولاbuild مطلوب لتقييمالرأي.

## 2026-10-14 — تنفيذ تحديث كاش وجهات المدارس بالأحداث والحذف عند الإيقاف

- بطلب المستخدم: SchoolRoutingChangedV1 وRoutingVersion مستقل/concurrency token علىSchoolRegistration؛ PlatformDbContext يلتقط تغييرات المدرسة وحقولoperational endpoint ويضيفsnapshot كاملًا أوtombstone إلىOutbox في نفسSaveChanges. يشملAPI/Worker provisioning وتبديلprimary، ويستبعدmigration metadata والحفظغيرالمتغير. أضيفت مهاجرتاPlatform PG20261014190417 وSQL20261014190458_AddSchoolRoutingVersion، عمودbigint افتراضي0؛ لمتطبقا علىDB فعلية ولاmigrations لمدارس.
- Worker يعلنrouting queue ويرسل عبرناشرOutbox الحالي؛ Schools consumer يحققproducer/schema/scope/payload، ويؤكد بعدكتابةRedis فقط معretry وDLQ. قبلتطبيقكلحدث يتأكد منsnapshot حالي منPlatform لمنعreplay بعدفقدguards والحالاتالوسيطةلتبديلprimary داخلtransaction؛ القراءةوقتالتغيير فقط لاكلطلب. endpoint داخلي جديدschool-routing-targets يوفرrevision والوجهة فيRepeatableRead و404مميزةلمدرسةمحذوفة حتىلايُفسّرendpoint قديمغيرمنشور كحذفمدارس.
- Redis raw Lua بدلIDistributedCache للكتابة الذرية: guard version وتombstone بلاTTL وdata30يومًا؛ الأحدثيحدثالإدخال كاملاً، الإيقاف/الحذف يزيلdata، التكرار/القديم لايمددTTL. epoch يتغيرعندفقدguard ويرفضfill الذيبدأقبلrestart/eviction. مفاتيحv2 لا تقرأv1، hash slots مشتركةللـguard/data، وfallback الطلبات/حدودها/مقاييسها محفوظة. التسجيلفيDI وCompose وAMQPS وقوالبconsumerEnabled وpreflight محدثة؛ لا.env فعلي أوخدمةتطبيقعاملةعُدّلت.
- التحقق: Platform API build ناجح بتحذيرCS0168 قائم فيSchoolsController، Worker build ناجح0warnings/errors. suitePlatform نجح179/179 ثمrouting outbox tests الأربعة (بمافيهاconcurrency الجديد)4/4؛ suiteSchools نجح221/221 و1skip لـPoppler، ثمالمسارالمستهدف21/21 بعدآخرمراجعة، بRedis7.4 وRabbitMQ3 حقيقيين معزولين. broker smoke يغطيالتعديل والإيقاف وإعادةالمحاولة بعد503 وreplay قديم بعدفقدguard؛ Lua tests تغطيالإصداراتوepoch، والوحدةتغطيduplicate/TTL وفشلRedis وfill متزامن. اختبارOutbox مصدرهEF InMemory، لا اختبارtransaction SQL فعلية؛ أُصلحتfixtures RowVersion وحالةendpointActive وخطأconstructor configuration السابق فيPlatformAccountProfileControllerTests دونتغييروظيفته.
- dotnet-ef migrations has-pending-model-changes بلا تغييرات للمزودين؛ توليدالمهاجرات مجردإضافةعمود، معتحذيراتdecimal القديمة لدىSQLServer. deploy/tests/Test-ComposeFixtures.ps1 ناجح للمحليوالإنتاج ورفضTLSالمفقود؛ git diff --check بلاwhitespace errors معCRLFwarnings القائمة. حاويتاQA أوقفتاوأزيلتا بـ--rm دونمساسRabbitMQ المستخدمالحالي. لا نشر أوترحيل بيانات فعلية؛ الدليلSCHOOL_ROUTING_REDIS.md وADR-056/CURRENT_STATE/NEXT_STEPS محدثة.
- الحدود الباقية: اتساقلاحق وزمنتأخرالرسائل، staging بمعاملاتوتزامنحقيقيين علىالمزودالمستخدم وTLS/ACL وlag/DLQ monitoring؛ SQLمباشر يتجاوزالالتقاط، وإعادةاستخدامكودمحذوف لهويةجديدة تحتاجترحيلguard. نسخنفسالمنصة تحتاجنفسRedis/namespace/authority وbroker/vhost منفصل للبيئات. لاbulk bootstrap جديد؛ cold misses وإعادةتأكيدالحدثتعيدانالأحدث.

## 2026-10-14 — مقترح تحديث كاش وجهة المدرسة بدل حذفه عند التعديل

- اقترح المستخدم تحديث إدخال Redis مباشرة عند تغيير البيانات، وحذفه عند إيقاف المدرسة أو حذفها. التوصية: حفظ مصدر الحقيقة في Platform مع Outbox في معاملة واحدة، ثم حدث عبر RabbitMQ إلى مستهلك كاش يكتب snapshot كاملًا لبيانات التوجيه المسموح بها دون أسرار، أو يزيل إدخال التوجيه عند الإيقاف/الحذف. التحديث يحدد صلاحية30 يومًا جديدة، بينما hit لا يمددها. المقترح غير منفذ ولا يغير السلوك الحالي.
- يلزم إصدار متزايد لوجهة المدرسة ككل، لا إصدار endpoint منفرد عند تبديلprimary، وتطبيق مقارنة الإصدار والكتابة بشكل ذري داخلRedis. الاحتفاظ بعلامة إصدار الإيقاف/الحذف لمنع إعادة إنشاء الإدخال برسالة قديمة أو lookup جارٍ؛ إزالة routing entry وحدها لا تكفي. reconciliation/rebuild عندrestart أوeviction، retry وتأكيد الاستلام بعد نجاح التطبيق، ومراقبة التأخر. التحديث عبرالرسائل غيرمتزامن وليسفوريًا مضمونًا.
- التحقق: قراءة السياق الحالي ومرجعRedis الرسمي https://redis.io/docs/latest/develop/programmability/eval-intro/ لعمليات المقارنة/الكتابة الذرية؛ توثيق فقط، لا كود أو DB أو broker أو اختبارات تشغيل تغيرت. git diff --check لفحص التوثيق.

## 2026-10-14 — تعديل مدة كاش وجهات قواعد المدارس إلى30 يومًا

- بطلب المستخدم لتقليل ضغط Platform DB: الافتراضي وحد التحقق الأعلى أصبحا2592000 ثانية (30 يومًا ثابتًا)، بدل15 ثانية وحد60. التعديل يشملSchoolRoutingCache وDI وCompose المحلي/الإنتاج بقوالب.env وpreflight؛ absolute TTL بلا تمديد علىhit ولاstale fallback. لا ملفات.env فعلية أو إدخالاتRedis قائمة أو نشر تغيرت؛ الإدخالات القديمة لا تمتد تلقائيًا، وrestart/eviction قد يسببانmiss قبل الشهر.
- أضيف اختبار قبول مدة الشهر، واختبار الافتراضي30 يومًا وثبات الموعد علىhit، وعدّل رفض الحد إلى2592001. حدّثCURRENT_STATE وADR-055 وNEXT_STEPS ودليليRedis/Docker. إبطال أحداثPlatform ما زال غير منفذ؛ الأولوية قبل إنتاج المدة الشهرية هي الإبطال عندإيقاف/نقلالمدرسة وتغييرالوجهة، معoutbox/version وحمايةسباقfill. الصيانة المؤقتة تعطّل الكاش وتزيلالقديم أوتغيّرnamespace لجميعالنسخ قبلإعادةالتفعيل؛ لاادعاءأن زيادةTTL وحدها تحققذلك.
- التحقق: dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore --filter FullyQualifiedName~SchoolRoutingCacheTests نجح13/13 و1skip لاختبارRedis الحقيقي الاختياري (غيرمشغل في هذهالدفعة)، معbuild ناجح. deploy/tests/Test-ComposeFixtures.ps1 نجح للمحلي والإنتاج ورفضTLS المفقود. git diff --check بلا أخطاءwhitespace مع تحذيراتCRLF القائمة. لا إعادة اختبارحمل أو broker أوDB أوRedis حقيقي مطلوب لهذا التغيير فيالافتراضي والحدود فقط.

## 2026-10-14 — تنفيذ أول مرحلة Redis لكاش وجهات قواعد المدارس

- بطلب المستخدم: أضيف Microsoft.Extensions.Caching.StackExchangeRedis8.0.31 وSchoolRoutingCache إلى Schools resolver؛ cache-aside لبياناتالتوجيه قبلPlatformHTTP، بلاsecret أوconnection string محلول، وحلcredential reference لكلhit، وكودمدرسةموحد وnamespace/authority لعزلالبيئات. TTL مطلق15ثانية (1–60) يبدأقبلupstream؛ Active فقط، بلاnegativecache أوstale fallback. سجلADR-055 وحالةالمشروع والخطواتوالدليلSCHOOL_ROUTING_REDIS.md.
- ضوابطالأداء: تجميعmiss داخلprocess بأقفالعددها64، وحد16lookup للمنصة وtimeout5s؛ Redis500ms وcooldown5s عندالفشل، معcancellation ومقاييسhit/load/failure ورسائلتحذير بلاsecret/exception content. لاإبطال فوري موصول بأحداثPlatform ولاstampede lock موزع؛ تقادمإيقاف/نقلالمدرسة حتىTTL موثق، وتلكالمرحلة ضمنNEXT_STEPS.
- Compose يضيفRedis7.4-alpine داخليًا بلاhost port، non-root/read-only/cap_drop/memory/log limits، password منsecret source و128MiB allkeys-lru دونpersistence. مفاتيحRedis/namespace/TTL أضيفتلقوالب.env فقط وpreflight/fixtures ودليلDocker؛ اتصالخارجي يدعمTLS عبرإعدادمزودRedis. لا.env فعلي أوAPI يعمل أوDB/migrations أوmigration worker عُدّل.
- التحقق: dotnet build Schools API نجح0warnings/errors؛ dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore معRedisfixture حقيقي نجح212/212 و1skip لاختبارPoppler السابق، منها12حالةكاش تغطيhit/عزلالمدارسوالمنصات/TTL/تزامن/فشل/cancellation/عدمcache404/تغييرsecret/disabled/config validation وRedisحقيقيمننسختين. أخطاءfixture الأول (duplicate configuration keys) صُححت، واختبارRedis قصير1ثانية عُدل5ثوان لتجنبانتهاءdeadline أثناءwarm-up تحتالحمل؛ النتيجةالنهائيةنجاح.
- deploy/tests/Test-ComposeFixtures.ps1 نجحللمحليوالإنتاج ورفضTLSالمفقود؛ خدمةRedis فعليةبإعدادCompose التجريبيnon-root/read-only رفضتping بلاauth وأعادتPONG بالمصادقة. حاويتانQA معزولتان فقط ثمأوقفتاوأزيلتا بـ--rm؛ لا نشرإنتاج أوتفعيلمدارسفعلية. git diff --check بلاwhitespace errors معتحذيراتCRLF القائمة.

## 2026-10-14 — التحقق من وجود Redis في النظام الحالي

- بناءً على سؤال المستخدم قبل التنفيذ: Redis غير مدمج في Backend أو compose الحالي؛ بحث محدد عن Redis/StackExchange.Redis/IDistributedCache/ConnectionMultiplexer/HybridCache فيsrc/deploy/tests وcompose لم يرجع نتائج. PlatformSchoolLoginTenantResolver يستدعيPlatform عبرHTTP مباشرة، ومسجل بـAddHttpClient دون طبقةكاش. Redis السابق مقترح لا تنفيذ.
- التحقق: rg بنمطكلماتدقيق وGet-Content للـresolver/DI؛ docker ps --format يعرض RabbitMQ فقط ولاRedis ضمنالحاوياتالمحليةالعاملة وقتالفحص. لايؤكدذلك غيابخدمةRedis خارجDocker أوعلىاستضافةبعيدة، لكنه يؤكدعدموجودتكاملها فيالمسارالمفحوص. البحثالأول شملDirectory.Packages.props غيرموجود وأعيدبدونه بنجاح؛ لاكود/إعدادات/نشر تغيرت ولاbuild مطلوب.

## 2026-10-14 — توضيح توجيه تحديث قواعد المدارس مع تعدد Backend وDB servers

- سؤال المستخدم عن ربط المدرسة بـdomain عند تعددBackend: نسخSchools API المتكافئة خلفdomain مشترك لا تحتاجربطمدرسةبنسخةAPI؛ المدرسةتحتاجوجهةDB مسجلة (Host/Port/Database/secret reference)، وقدتحتاجDbClusterId لتوجيهworker مقيدبمجموعةDB. مقترحBackendمشترك وسيرفراتDB لمجموعاتحتى500مدرسة لم يثبتباختبارسعة.
- فحصMigrateSchoolDatabaseV1 وSchoolDatabaseMigrationHandlers وSchoolDatabaseMigrationConsumerService: command يحملOperationId/TenantId/SchoolId/DatabaseName فقط؛ consumer يستخدمSchoolProvisioningDatabase:AdminConnectionString المحلي ويغيرDatabase فقط، معqueue مشتركةschools.database-migrate.v1. لذلك ليسجاهزًالتوزيعقواعدعبرHosts متعددة، وتشغيلنسخباعداداتDB مختلفةعلىqueue نفسهاقديوجهالتحديثللـHost الخطأ.
- اقتراحانقبلالتنفيذ: worker مركزييحلوجهةDB الموثوقة منregistry حسبالمدرسة ولديهصلاحياتصيانةلكلمجموعة، أوworkers بحسبDbClusterId ورسائل/queues موجهةإليها. تقييدتوازيالمهاجرات ومنعتحديثنفسDB بالتزامن وتوافقإصدارAPI معschema قبلالنشر؛ لا تعديلكودأوعقودرسائلأونشرتم. تحققGet-Content/rg للملفاتالمحددة وgit diff --check للتوثيق، بلااختبارbroker/DB فعلي.

## 2026-10-14 — اعتماد الفصل الكامل بين Family وSchools عبر RabbitMQ

- صحح المستخدم افتراض اعتماد Family مباشرة على Schools: كل تطبيق Backend/DB مستقل، والبيانات في الاتجاهين تمر عبر RabbitMQ وتحفظ محليًا لدى المستقبل. اعتمد ADR-054 وحدث CURRENT_STATE/NEXT_STEPS؛ Family يقرأ محليًا، وSchools ينشر أحداث بياناته ويستقبل أوامر الأسرة ويعيد النتائج دون اعتبار قبول الطلب نجاحًا نهائيًا.
- نميز استقلال القراءة عن ملكية الحقيقة والاتساق اللاحق: يحتاج التنفيذ Outbox/Inbox وEventId/version وack بعد الحفظ وretry/DLQ وتوجيه مجموعة المدارس وinitial sync/reconciliation، مع سياسة تحديث/حذف/سحب الإتاحة. حمل Family على Schools يأتي من أوامره وإنتاج الأحداث لا استعلامات Family المباشرة. لا بدء تنفيذ Family أو تعديل كود/DB/نشر.
- التحقق: Get-Content وrg للسياق وقرارات Family، ومراجعة https://www.rabbitmq.com/docs/reliability ؛ توثيق فقط بلا build أو اختبار broker فعلي. فحص git diff --check للتوثيق دون أخطاء whitespace، مع تحذير CRLF القائم.

## 2026-10-14 — تقسيم الاستضافة المقترح إلىRedis/Schools/Platform/Family

- لخّص المستخدم التصور فيRedis server وسيرفراتSchools وPlatform server وFamily server. التقسيم المقصود مجموعات مسؤوليات استضافة؛ ليس إثباتنشر أو تحديدعددنسخالتوافر. هدف500 مدرسة لكلSchools server يبقى مشروطًاباختبارالحمل وتوجيهHTTP/provisioning/migrations، وRedis كاشوجهاتالمدارس لامصدرالحقيقة.
- يجب إدراج مكانDB والملفات فيميزانيةالسعة: قواعدالمدارس يمكن أنتبقى علىسيرفراتSchools فيتصورالبداية ويشملهاالاختبار؛ Platform databases/Worker/RabbitMQ يحتاجونتوزيعًاصريحًا. Object Storage والنسخالاحتياطية الخارجية خدماتإضافية. Family استضافةمستقلة مستقبلًا، لكنقراءة/كتابةعملياتالطلاب قدتظل علىSchools backend حسبتصميمالتكامل، فلايعني فصلFamily إزالةحملهعنSchools. لا كود/نشر/إعدادات تغيرت، تحقق سياقالمناقشة فقط ولاbuild مطلوب.

## 2026-10-14 — مناقشةRedis لتقليل طلباتSchools إلىPlatform

- اقترح المستخدمRedis لكاش بيانات المدارس؛ التوصية المقترحة cache-aside داخل مسارSchools resolver لبيانات التوجيه فقط (هوية/كود المدرسة، وجهةDB، مزود/TLS ومرجعsecret)، لا كلمات مرور أوconnection strings محلولة أو صلاحيات المدرس. Platform مصدر الحقيقة؛ hit لا يستدعيPlatform وmiss يجلب منه ويحفظ بTTL. وضع الكاش فيPlatform وحده لا يزيلHTTP منSchools.
- القراءة الموجهة لـSchoolLoginTargetsController وPlatformSchoolLoginTenantResolver تؤكد أنPlatform لا يعيد إلا مدرسةActive وأنresolver يستدعيHTTP كلResolve، بما في ذلكCreateAsync لقاعدة المدرسة وليسlogin وحده. يجب تغطية إيقاف/حذف/نقل المدرسة وتغييرprimary endpoint/مرجعsecret بأحداث موثوقة معoutbox/retry/version؛ TTL حد لتقادم البيانات وليس ضمان تعطيل فوري، والكاش المحلي الاختياري لا يتجاوز انتهاءالكاش الأصلي. حماية تعبئة متزامنة وسباقإبطال/fill ومراقبةhit/miss/fallback.
- عند تعطلRedis: fallback مضبوط إلىPlatform بtimeouts وحدود توازي، دون عاصفةطلبات؛ عند تعطل الاثنين وغيابنسخةصحيحة رفض مؤقت، دون تمديدبياناتقديمة افتراضيًا. لا نشرRedis أو تعديلكود أو اعتمادTTL نهائي. المرجع https://redis.io/docs/latest/develop/use-cases/cache-aside/ ؛ أوامرGet-Content وrg لملفاتresolver/controller والسياق نجحت، ولاbenchmark أوbuild لهذهالمناقشة.

## 2026-10-14 — توضيح المستخدم لنموذج حمل Schools

- أكد المستخدم أن الحسابات النشطة المقصودة في تقدير السعة هي حسابات المدرسين، لا حسابات الطلاب؛ الطلاب سجلات ترتبط بها حركات الحضور/الغياب والتقييمات والأنشطة والخطة اليومية وتقييم الحصة. كثافة العمل اليومية في هذه المسارات هي أساس التقدير، بينما الاختبارات والخطط السنوية/التيرم/الشهرية/الأسبوعية وبقية إعدادات الحصة أعمال أقل تكرارًا، مع اختبار ذرواتها الدورية منفصلًا.
- لذلك لا تحوّل أعداد الطلاب إلى مستخدمين متزامنين؛ تستخدم لتقدير حجم الحفظ الجماعي والتاريخ والتدقيق. اختبار هدف500 مدرسة يبنى على المدرسين النشطين/المتزامنين والفصول والطلاب لكل فصل وعدد الحصص وتزامن الحفظ عند حدود الحصص. أي أرقام تزامن أو معدلات طلبات تظل سيناريوهات افتراضية حتى قياس الاستخدام الفعلي؛ لا اعتماد سعة أو تغيير معمارية/إعدادات في هذه المناقشة.
- التحقق: مراجعة توضيح المستخدم مقابل سياق الحضور وتقييم الحصة والتخطيط الموجود؛ توثيق فقط، بلا أوامر build أو اختبار حمل أو تغيير قاعدة بيانات.

## 2026-10-14 — تقييم مبدئي لاستضافة500 مدرسة على Contabo Cloud VDS XXL

- مناقشة سعة مقترحة فقط: مواصفات الصورة والمصدر الرسمي24vCores/96GB RAM/720GB NVMe/1Gbps تجعل500 مدرسة هدفًا ممكنًا للاختبار في Schools فقط؛ لا benchmark أو حد مضمون. متوسط441 طالب يعني نحو220.5ألف سجل طالب، لا مستخدمين متزامنين؛ الحمل الفعلي من staff وعمليات الحصص والوسائط. مثال تخزين افتراضي2GB/مدرسة يصل1TB قبل DB/backup، لذلك فصل محتوى الملفات عن القرص المحلي ضمن الخطة.
- فحص موجه كشف: SchoolDbContextFactory يحل وجهة المدرسة عبرPlatformSchoolLoginTenantResolver دونcache في هذا المسار؛ اتصال Platform يظل مؤثرًا رغم فصل الاستضافة. compose يحدSchools API افتراضيًا2CPU/2GB؛ BookPdfRenderer يسمح بعمليتين متوازيتين مع30s timeout. provisioning/migration consumers تستعمل queues مشتركة وإعداد admin محلي، فلا يعد تكرارها توزيعًا آمنًا لمجموعات مدارس؛ يلزم توجيه HTTP ورسائل الإنشاء/التحديث وميزانية اتصالات ومهام تذكير حسب المجموعة. مزودDocument Core الحاليfilesystem؛ S3/MinIO مقترح غير منفذ.
- التحقق: Get-Content للسياق وملفاتfactory/resolver/renderer/consumers وrg لحدودcompose وpooling نجحت في القراءة؛ راجعنا https://docs.contabo.com/docs/servers-hosting/max-performance-vps/ وhttps://www.npgsql.org/doc/connection-string-parameters.html . أضيف عمل تقييم الحمل إلىNEXT_STEPS؛ لا تعديلات كود/إعدادات/DB أو اختبارات أداء فعلية، ولا قرار اعتماد500 مدرسة ثابتًا.

## 2026-10-14 — تأكيد عدم توافق Android Studio مع AGP الحالي

- صورة Gradle Sync الجديدة تؤكد المانع: المشروع AGP9.0.1 بينما IDE يدعم حتى8.7.2. هذا يفسر عدم اكتمال استيراد Gradle وأخطاء MainActivity داخل IDE؛ تعريف .iml القديم كان قرينة سابقة وليس السبب النهائي المؤكد. البناء بـFlutter نجح في التشخيص السابق، فلا تعديل imports أو downgrade عشوائي.
- التوجيه: تحديث Android Studio إلى إصدار يدعم AGP9.0، مثل Otter3 Feature Drop2025.2.3 أو إصدار أحدث متوافق وفق جدول Google الرسمي، ثم فتح android وإعادة Sync. لم يُثبت تحديث أو يُغير AGP/Kotlin/Gradle/ملفات IDE؛ التحقق من نص الصورة وجدول Android Developers فقط. تحذيرات KGP/file_picker تُراجع لاحقًا ولا تساوي خطأ عدم توافق IDE.

## 2026-10-14 — تشخيص مراجع MainActivity غير المحلولة في IDE

- فحص Android MainActivity/settings/app Gradle/local SDK: imports وFlutterActivity/MethodChannel صحيحة وManifest يربط .MainActivity. تعريف android/mdaresna_schools_android.iml يشير Android API24 غير مثبت وإلى Flutter for Android، بينما .idea/libraries لا يحتوي هذه المكتبة؛ تعريف المشروع الأعلى يشير API34 الموجود. يرجح أن unresolved android/embedding داخل IDE من عدم استيراد Gradle/مزامنته، لا خطأ في مصدر Activity. لم تعدل .idea أو SDK أو plugins أو imports؛ إرشاد فتح مجلد android كمشروع Gradle ثم Sync وضبط JDK وFlutter SDK.
- تحقق البناء: Flutter3.44.5/AGP9.0.1؛ build العادي فشل في Kotlin incremental caches لإضافتي file_picker/video_player_android لا unresolved references للـActivity. retry لـ`flutter build apk --debug --no-pub` نجح في46.1s مع GRADLE_OPTS مؤقت يعطل kotlin.incremental ويجعل compiler execution in-process، واستعيد المتغير بعد الأمر دون تنظيف/حذف caches أو تغيير إعداد دائم. APK محلي محدث في build/app/outputs/flutter-apk/app-debug.apk؛ لا تشغيل جهاز أو اختبار FLAG_SECURE فعلي. تحذير file_picker/انتقال KGP قائم؛ Flutter الرسمي يؤكد عدم تفعيل built-in Kotlin على3.44 كحل عشوائي. لا تعديل إصدارات أو ترحيل Kotlin بهذه الدفعة.

## 2026-10-14 — دليل Word عربي لنشر التطبيقات الخمسة

- بناءً على طلب المستخدم: `docs/deployment/Mdaresna_Five_Applications_DevOps_Deployment_Manual_AR.docx` مرجع DevOps من22 صفحة عربية RTL مع أوامر Bash LTR وجداول وأرقام صفحات وقائمة تسليم. يشرح المشاريع ومساراتها ومتطلبات البيئة وCompose المحلي/الإنتاج والدومينات والأسرار وTLS/AMQPS والصور وDB migrations داخل الشبكة وحساب الإدارة الأول وتحديث المدارس والقبول وترحيل الملفات والنسخ/الاستعادة والتحديث/الرجوع والمراقبة والأعطال. لا قيم أسرار أو اتصال فعلي ولا تشغيل أوامر نشر.
- المصدر ملفات النشر/ADR-053 ودليل التشفير وفحص factories PostgreSQL وأداة bootstrap؛ تم الرجوع إلى وثائق Docker الرسمية وMicrosoft EF لتأكيد إرشادات التثبيت/الإنتاج/migrations. لا يدعي guide أن Healthy تثبت schema أو كل consumers، وprivate CA تحتاج trust صريح وrunner داخلي بدل فتح منفذDB.
- التحقق: توليد python-docx من runtime المرفق وartifact marker مرة واحدة؛ packaged render_docx تعذر لغياب LibreOffice على Windows، فاستُخدم Microsoft Word COM read-only لتصدير PDF داخلي وPoppler لإنتاج PNG22. راجعت جميع الصفحات وأصلحت border قالب Title واتجاه الفقرات والمحاذاة start ومسارات LTR وإعادة التصيير؛ ZIP/OOXML والعدد والصفحات جرى فحصها. PNG/PDF داخل .codex_tmp للـQA فقط وليست تسليمات؛ الملف النهائي Word فقط. لا تعديل Docker أو مفاتيح/بيانات فعلية بهذه الدفعة.

## 2026-10-14 — تنفيذ تجهيز Docker للتطبيقات الخمسة

- بعد طلب التنفيذ: تحديث الصور الخمس وcompose المحلي بnon-root/read-only/no-new-privileges/cap_drop ALL/tmpfs/resources/log rotation ومنافذ loopback؛ إعداد Web8080 وenv.js في tmpfs، وSchools directory ملكيته APP_UID للمخزن الجديد وhealth Poppler. COPY Backend محصور src/global.json، واستبعاد App_Data/بيانات legacy/artifacts/شهادات/appsettings.Development، وLF attributes للـshell/config. إضافة مفاتيح الرفع/legacy إلى القالب دون تغيير القيم الفعلية.
- production overlay منفصل2.24.4+ يغلق host ports باستثناء gateway80/443، Production وAMQPS إلزامي وbroker TLS5671/shut plaintext وشهاداتread-only خارجية، alias لاسم SAN، gateway same-origin API/SignalR و54MiB/no upload disk buffering/access logs بلا query. إضافة Hosting:KnownProxies في Platform بعنوان gateway فقط. preflight يقرأ resolved config بالذاكرة دون طباعة الأسرار ويتحقق من المفاتيح/المنافذ/TLS files؛ لا يتجاوز certificate trust. Existing broker users/volume ownership/key ring تحتاج صيانة مستقلة لا overwrite تلقائي.
- readiness جديدة: Platform DBs/encryption، Schools provisioning DB/encryption/empty ephemeral storage probe، Worker service/DBs، بدون معلومات سرية في failure. لا schema أو tenant sweep أو إثبات كل consumer. دليل DOCKER_DEPLOYMENT وقالب production واختبارات fixture/web/gateway مضافة؛ build --pull موصى به في دليل الإصدار لتحديث base images، لا نشر تلقائي.
- التحقق: Docker build للصور الخمس نجح بعلامة docker-qa (تحذير CS0168 سابق وVite/SignalR annotations؛ Docker warnings لأسماء Firebase web public الفارغة لا أسرار server). config --no-interpolate --quiet نجح للbase/overlay، وفاحص fixtures نجح بما فيه رفض غيابTLS. nginx readonly smoke اكتشف pid path /run وأصلح قبل إعادة الاختبار؛ HTTP للويب الحقيقي non-root/readonly مع128m و0.5CPU وSPA fallback/env.js نجح، gateway nginx -t نجح بشهادة اختبار مؤقتة disposable، وBackend non-root runtime/Schools fresh anonymous volume/Poppler نجحت. Schools suite200passed/1skip=201؛ اختبارا readiness جديدان ناجحان. لا اختبارTLStrust/DB/رسائل/staging كامل أو composeup/تطبيق migrations/تعديل ملفات الأسرار. حاويات الاختبار --rm أزيلت، الصور المحلية بقيت للاختبار؛ لم تلمس الحاوية rabbitmq الحالية.

## 2026-10-14 — مراجعة Docker للتطبيقات الخمسة (دون تنفيذ نشر)

- راجعت Dockerfiles لـPlatform API/Worker/Web وSchools API/Web وcompose وقوالب env/nginx وdockerignore ونقاط health. الأساس موجود: multi-stage، الصور منفصلة، backend net8 متوافق مع runtime8 (SDK9 للبناء ليس تعارضًا بذاته)، non-root للـBackend، runtime env.js للويب وvolumes وتبعيات وhealth. `docker version --format` أكد daemon متاح؛ `docker compose -f Mdaresna/docker-compose.yml config --no-interpolate --quiet` نجح لفحص البنية فقط؛ لم تعرض إعدادات env الفعلية أو تشغل build/up أو تغيّر أي سر/volume/DB.
- فجوات قبل الإنتاج: compose/template افتراضيهما Development وbroker مثال amqp بينما الكود يرفضه خارج Development، وRabbitMQ بلا TLS مهيأ؛ يلزم AMQPS وشهادات أو broker خارجي موثوق لا تعطيل الحماية. volume المستندات جديد بلا mkdir/chown في image أو init service رغم USER غير root؛ يلزم تهيئة ملكية واختبار كتابة. backend dockerignore لا يستبعد **/App_Data رغم وجوده محليًا وCOPY . .؛ يلزم استبعاد بيانات الرفع/logs/artifacts من build context. قالب env ينقص مفتاحَي تشفير الرفع وخيار legacy صريح. منافذ DB/broker/management/worker تنشر لكل الواجهات افتراضيًا؛ الإنتاج يحتاج عدم نشرها أو bind داخلي مقيد.
- فجوات تشغيلية: APIs readiness=self فقط وWorker readiness لخدمته الخلفية لا تحقق شامل DB/queues/storage/keys؛ لا يضمن health الحالي اكتمال الخدمة. gateway HTTPS/forwarded-proxy trust وعناوين API/CORS الحقيقية وحد رفع54MiB وعدم buffering/SignalR لازمة خارج nginx الويب static؛ Platform init.sql ينشئ قواعد فارغة فقط والمigrations منفصلة كما بالدليل. مراجعة reminders SchoolCodes عند نشرها، resource limits/backup ومراقبة وتحقق staging لكل الصور باقية. لا ادعاء اختبار بناء/تشغيل فعلي لهذه المراجعة.

## 2026-10-14 — مراجعة متطلبات Docker للنشر (قراءة فقط)

- تصحيح اختيار خطة مادة الصف لا يحتاج Dockerfile/compose/migration جديدة، بل إعادة بناء Web وتحديث Flutter خارج Docker. نشر كامل التحديثات السابقة يحتاج Schools API وPlatform API وWorker حديثة وتحديث قواعد المدارس المركزي بعد backup.
- فحص Dockerfile Schools أكد poppler-utils وUSER غير root؛ compose يمرر مفاتيح UploadEncryption الإلزامية للخدمتين ويربط StorageRoot بvolume دائم. user-secrets المحلي لا ينتقل للحاويات؛ عند نقل بيانات مشفرة يجب نقل key ring المطابق والاحتفاظ بالقديم، لا توليد بديل يفقد القراءة. تحقق صلاحية كتابة volume ومصدر أسرار دائم وProduction وHTTPS وحد رفع gateway وعدم buffering على القرص؛ nginx Web الحالي static وليس بوابة API. AllowLegacyRead يبقى أثناء ترحيل القديم ثم يعطل صراحة بعد المراجعة.
- نقص إعداد موثق: `.env.docker.example` لا يسرد PLATFORM_UPLOAD_ENCRYPTION_KEY/SCHOOLS_UPLOAD_ENCRYPTION_KEY رغم إلزامهما في compose. لم تعدل ملفات نشر أو أسرار أو تشغل Docker/build/deploy؛ التحقق قراءة الملفات وrg فقط.

## 2026-10-14 — إظهار خطة مادة الصف المختارة عند التعديل

- السبب: Web/Flutter يصفيان خيارات خطة المنهج إلى Draft، فيسقط parentId الحالي من القائمة بعد تفعيل/أرشفة الخطة. أضيفت الخطة الحالية ضمن خيارات التعديل فقط، مع إبقاء الإضافة للمسودات وحماية immutability في API؛ لا تغيير Backend أو DB أو مهاجرة.
- التحقق: `npm run build` نجح؛ `npm run lint` بلا أخطاء مع 3 تحذيرات سابقة. `dart format` و`flutter analyze lib/src/features/settings/school_curriculum_page.dart` ناجحان. fixture المتصفح `subject-total-browser.html?planStatus=Active/Archived` أكد اختيار الخطة الحالية، وحفظ parentId=plan دون تغييره (طلب mock لا كتابة فعلية)، واستبعاد المفعلة من الإضافة مع وجود مسودة أخرى. لم يبن APK أو ينشر أو تختبر قاعدة فعلية.

## 2026-10-14 — تنفيذ فلاج إضافة مادة الصف إلى المجموع

- بعد موافقة المستخدم: CountsTowardTotal nullable على CurriculumGradeSubject فقط، وحفظ true/false والـDTO/Swagger والقيمة الموروثة للـoffering عبر Include. لا عمود تشغيل مستقل؛ API يرفض override خارج مادة الصف، ويرفض إنشاء جديد بلا قيمة، ويحافظ على قيمة تحديث العميل القديم عند الإغفال، ويطبق قفل الخطة الحالي. السابق null للمراجعة لا تصنيف شامل؛ لا تغيير IsRequired أو PassScore أو نتيجة المادة، ولا Gradebook/مجموع شامل منفذ حاليًا.
- Web checkbox في محرر مادة الصف (new=true، القديم null=indeterminate مع توضيح)، hidden field يرسل false صراحة ولا يرسل null غير المراجع؛ عمود حالة في مواد الصفوف والتشغيل، ومحرر التشغيل read-only يتغير مع اختيار مادة الخطة. Flutter checkbox مستقلة وtri-state للقديم وموروثة disabled في التشغيل، وحالة بالقائمة، وترجمات عربية/إنجليزية.
- مهاجرات PG `20261013232014_AddGradeSubjectTotalPolicy` وSQL `20261013232122_AddGradeSubjectTotalPolicy` مولدة فقط، تضيف nullable column دون default/backfill؛ has-pending-model-changes بلا تغييرات للمزودين (تحذيرات filters السابقة). لم تطبق أو تنشر أو تغير بيانات مدرسة فعلية، ولا بُني APK جديد بهذه الدفعة.
- التحقق: Schools unit tests نجحت 198 مع skip واحد لاختبار Poppler السابق opt-in (199 إجمالًا)، منها 5 حالات جديدة true/false/legacy/حماية الخطة ورفض override/read-only. Web build ناجح/تحذير bundle سابق، lint بلا أخطاء/3 تحذيرات سابقة؛ Flutter suite41/41 وتحليل مستهدف نظيف، واختبارا الفلاج أعيدا ونجحا بعد تحسين وصف inherited. Browser على المكون الحقيقي بfixture أكد mixed للقديم، payload القديم بدون تصنيف، payload الإنشاء غير المضاف false مع IsRequired=true، والتشغيل read-only؛ لقطة `.codex-artifacts/subject-total-offering.png` فحصت ثم حدثت لفصل سطور الوصف. fixture احتاج sortOrder=0 ليمر native form validation؛ ليس خللًا في الحفظ الفعلي. diff checks ناجحة، وتحديث ملفات السياق الأربعة.

## 2026-10-14 — مناقشة المادة المضافة للمجموع (مقترح غير منفذ)

- المستخدم سأل عن وضع الفلاج في مادة الصف بالخطة أو التشغيل السنوي. التوصية: CountsTowardTotal على CurriculumGradeSubject، لا Subject العام ولا فلاج تشغيل مستقل؛ التشغيل يرث ويعرض القيمة من مادة الخطة المثبتة على عامه. ADR-014 والخطة المفعلة الثابتة يحميان تاريخ السنوات دون تكرار مصدر الحقيقة؛ اختلاف السياسة مستقبلًا عبر إصدار خطة جديد، وتجاوز سنوي لا يضاف إلا لو طلب صراحة مع تصميم تدقيق/تثبيت.
- الفلاج مستقل عن IsRequired وعن اشتراط النجاح/الرسوب: مادة غير مضافة للمجموع يمكن أن تتطلب النجاح. يلزم عند التنفيذ ربط احتساب مجموع الطالب ودرجته القصوى والمئوية/الترتيب بالفلاج في نطاق وحدات النتائج التي تدعمه، ومراجعة القيم للمواد القديمة دون افتراض ترحيل شامل. فحص كيانَي CurriculumGradeSubject/GradeSubjectOffering أكد عدم وجوده حاليًا؛ لا تغيير كود/DB أو قرار معتمد في هذا النقاش.

## 2026-10-14 — تفاصيل إعداد Windows المحلي للرفع (دون تفعيل)

- فحص أسماء إعدادات user-secrets فقط أكد غياب UploadEncryption في Schools.Api وPlatform.Api وقت الفحص؛ لم تعرض قيم الأسرار. إعداد Schools appsettings الحالي StorageRoot نسبي `App_Data/school-documents`، وlaunch profile https للخدمتين Development (Schools7251 وPlatform7241). أوضح الشرح مكان Manage User Secrets ومساراته، وتوليد 32-byte عشوائي وJSON stdin إلى CLI مع guard عدم استبدال إعداد تشفير موجود والحفاظ على بقية الأسرار، وإعداد مسارات Poppler المحلية ثم إعادة تشغيل الخدمات وWorker وتحديث مدرسة تجريبية.
- `dotnet user-secrets set --help` أكد دعم piped JSON (help يعيد exit1)، وتوثيق Microsoft الرسمي يؤكد أدوات التطوير/طريقة Visual Studio. ترحيل القديم بعد backup وصيانة فقط، وتعطيل legacy reads بعد جميع المدارس وIdentity ومراجعة الـvolume، لا بعد مدرسة واحدة. السكربت المشروح لم يُشغّل، ولم تتغير إعدادات/مفاتيح/DB أو ملفات فعلية في هذا الشرح.

## 2026-10-14 — شرح تجهيز رفع الملفات محليًا (إرشادات فقط)

- ترتيب التفعيل الموضح للمستخدم: backup، مفاتيح UploadEncryption مستقلة وثابتة للخدمتين داخل user-secrets للتطوير فقط مع Development، الحفاظ على StorageRoot الحالي إن وجدت ملفات وصلاحية كتابة الخدمة، تحديث المدارس المركزي بالمهاجرات، إعداد Poppler للكتب وتحديد مساراته إن لم يظهر لـAPI، إعادة تشغيل واختبار رفع/قراءة، ثم صيانة لترحيل القديم وتعطيل legacy reads بعد المراجعة. لا استبدال مفتاح مستخدم أو تعديل مسار تخزين بلا نقل مدروس.
- فحص csproj أكد UserSecretsId للخدمتين؛ Get-Command أكد pdfinfo/pdftoppm داخل runtime أدوات Codex الحالي فقط، ولا يفترض PATH نفسه في Visual Studio أو الإنتاج. راجع توثيق Microsoft user-secrets لتمييز التطوير عن مخزن أسرار الإنتاج. لم تُولّد مفاتيح أو تُعرض قيم أسرار، ولم تُعدّل إعدادات/بيانات أو تُنفّذ migrations/ترحيل في هذا الشرح.

## 2026-10-14 — تنفيذ التشفير الشامل للرفع والنسخ الرقمية للكتب

- نتيجة الإقفال: Android debug APK بُني بنجاح بعد retry بخيارات GRADLE_OPTS المؤقتة `org.gradle.project.kotlin.incremental=false` و`org.gradle.project.kotlin.compiler.execution.strategy=in-process` (تحذيرات KGP/file_picker وJava8 سابقة)، artifact `Mdaresna_Schools/build/app/outputs/flutter-apk/app-debug.apk`. لا تعديل Gradle دائم أو حذف caches. Flutter39/39 وتحليل نظيف، واختبار التشفير11/11 أعيد بعد إضافة رفض legacy reads عند تعطيلها؛ server suite194/194 أعلاه. لا اختبار screenshot على جهاز أو iOS build، ولا تفعيل مفاتيح/ترحيل/نشر. جميع diff checks ناجحة.

- متابعة التحقق النهائي: Flutter suite نجحت 39/39، والتحليل المستهدف للصفحة والاختبار نظيف بعد إصلاح lint الاختبار؛ اختبار القارئ أعيد ونجح بعد فحص capture قبل تحميل أول صفحة. EF has-pending-model-changes بلا تغييرات للمزودين. محاولة APK الأولى فشلت بكاش Kotlin incremental لدى file_picker (بيئة Windows/مسارات الكاش)، وجارٍ retry بخيارات عملية البناء فقط دون حذف caches أو تعديل pubspec؛ native iOS غير مبني على Windows.

- حسب موافقة المستخدم على حدود Screenshot وتأكيد تشفير كل الرفع: UploadedFileCipher مركزي في SharedKernel بغلاف AES-256-GCM مجزأ/HKDF وأثر سياق، key id وتدوير/رفض مفتاح ناقص أو تلاعب/Seek. LocalSchoolDocumentStorage يشفر كل حفظ جديد؛ صور حسابات Schools وPlatform داخل DB تشفر كذلك، مع فك جميع مداخل قراءتها ومنها صور Platform Staff. multipart لا يكتب temp plaintext للحدود المدعومة؛ legacy read انتقالية يمكن تعطيلها.
- API نسخ الكتب typed مع metadata ورفع PDF50MiB/2000 صفحة والسياسة/تنزيل الأصل/pages، ExpectedRevision وقيود الإصدار/الكتاب والصلاحيات/audit؛ إضافة EF تبين في اختبار الرفع أن نسخة المستند تحتاج Add صريحًا قبل ربطها، وصُحح. قارئ Poppler فعلي بstdin/stdout وعدد/timeout/output/concurrency limits، وعلامة مائية server PNG بلا overlay قابل للإزالة؛ لا antivirus/Family أو ضمان DRM مطلق.
- Web/Flutter مدخل من إصدارات الكتب، رفع/استبدال والسياسات والقارئ/السابق/التالي وتنزيل مشروط. Android channel لـFLAG_SECURE عند منع الأصل ويزال عند الخروج؛ iOS capture detection/hiding مع دعم implicit engine، بلا تجربة جهاز أو ضمان منع screenshot. endpoints صيانة للملفات/صور الحسابات السابقة بدفعات وchecksum قبل تبديل DB/حذف الأصل؛ لم تستدع على بيانات فعلية، وcrash يتطلب مراجعة يتامى/أصول متبقية قبل الإقرار باكتمال تشفير الـvolume.
- migrations generated فقط: PG `20261013223025_AddBookVersionDigitalDocuments` وSQL `20261013223149_AddBookVersionDigitalDocuments`؛ فشل أول توليد لغياب design-time env عولج بقيم fixture بلا اتصال فعلي. Docker Schools API يضيف poppler-utils وcompose يطلب مفاتيح الخدمات من secret source؛ دليل التشغيل الجديد `docs/deployment/UPLOAD_ENCRYPTION_AND_DIGITAL_BOOKS.md`، لا مفتاح generated/محفوظ بالتوثيق أو تغيير أسرار/قواعد أو نشر.
- التحقق: Schools tests مع `MDARESNA_TEST_POPPLER=1` نجحت 194/194، بما فيها قراءة تشفير/حدود chunks/Range/rotation/tamper/scope/legacy وضبط التنزيل/revision/read-only وتعطيل الإصدار وPoppler فعلي، وصورة `.codex-artifacts/native-book-page.png` فحصت بصريًا. Platform API build ناجح/تحذير CS0168 سابق. Web build ناجح/تحذير bundle سابق وlint بلا أخطاء/3 تحذيرات سابقة؛ browser mock أكد صفحات1/2 و2/2 وغياب التنزيل ثم ظهوره بعد حفظ السياسة، لقطة `.codex-artifacts/digital-book-reader.png` (صفحة fixture SVG، لا upload فعلي). Flutter اختبار جديد نجح لطلب صفحات فقط وauth وإزالة secure flag مع channel mock؛ suite السابقة 38/38 ناجحة، والفحص المستهدف نظيف قبل إضافة الاختبار. فحص نهائي شامل/Android build مسجل في متابعة أدناه عند اكتماله. diff checks ناجحة؛ لا native iOS/Docker/SQL concurrency أو ترحيل/استعادة مدرسة فعلية.

## 2026-10-14 — طلب النسخة الرقمية لإصدار الكتاب وحمايتها (لم ينفذ)

- المستخدم طلب رفع نسخة رقمية مرتبطة بـBookVersion، مشاركة الأسرة اختيارية، السماح بالتنزيل اختياري ومنع Screenshot عند منعه، وتخزين مشفر بالسيرفر. فحص ADR-014 وSchoolCurriculumController وSchoolDocumentStorage أكد فصل الإصدار وغياب رفع الكتب؛ التخزين المحلي الحالي يكتب bytes الأصلية ولا ينفذ تشفيرًا في التطبيق.
- المقترح: PDF مرتبط typed بـDocument Core، إعدادات مشاركة/تنزيل مستقلة وتحقق Server-side وصلاحيات ونطاق المنهج وسجل أثر، تشفير authenticated عند التخزين بمفتاح من secret source مستقل مع version/key id، وHTTPS للنقل. العرض المقيد لا يعيد PDF الأصلي للعميل، بل صفحات محمية بعلامة مائية شخصية؛ لا ضمان منع نسخ الصور المعروضة.
- قيد يحتاج اتفاق المستخدم: منع Screenshot مطلق غير مضمون على Web/iOS أو أمام كاميرا خارجية؛ Android FLAG_SECURE حماية للنظام المدعوم لا DRM كامل. توثيق Android https://developer.android.com/security/fraud-prevention/activities وApple https://developer.apple.com/documentation/swiftui/protecting-sensitive-content-when-screen-sharing راجعا فقط. Family App يبقى مؤجلًا؛ المشاركة metadata جاهزة فقط حتى بدء مرحلته. لا كود/migration/نشر/اختبارات تنفيذ في هذه المناقشة.

## 2026-10-14 — تبسيط القائمة الجانبية في Schools Web

- إزالة academicSetup وnotifications من navigation المشترك للقائمة الجانبية والمتجاوبة فقط؛ إبقاء الإعدادات الأكاديمية وجرس الهيدر وصفحة الإشعارات ومسارات أهدافها كما هي، دون تغيير الصلاحيات/API/Flutter.
- التحقق: `npm run build` ناجح مع تحذير bundle size السابق؛ `npm run lint` بلا أخطاء/3 تحذيرات سابقة؛ `git diff --check` ناجح. فحص المصدر أكد استمرار جرس الهيدر والتاب الأكاديمي تحت الإعدادات؛ لا اختبار متصفح/نشر في هذه الدفعة.

## 2026-10-14 — صف التقييم والمتابعة والرؤية في الملاحظة

- استخراج LessonNoteOptions: التقييم والcheckbox للمتابعة وradio list للرؤية بجانب بعض في Web مع fieldsets/labels ومعرف مجموعة مستقل وaria-pressed للتقييم؛ صف متجاوب يتكدس تحت 720px. خيارات وقيم الحفظ نفسها، لا تغيير API/Flutter/DB/Family.
- التحقق: `npm run build` ناجح مع تحذير حجم bundle السابق؛ `npm run lint` بلا أخطاء و3 تحذيرات سابقة؛ browser mock أكد أن المجموعات الثلاثة على y=530.6 نفسه، وأن اختيار الرؤية والمتابعة والتقييم يستمر بعد تبديل التابين. لقطة `.codex-artifacts/note-options-row.png` فُحصت؛ لا حفظ فعلي أو اختبار جهاز/عرض ضيق. `git diff --check` ناجح.

## 2026-10-14 — تابات التفاصيل والمرفقات وتوسيع محرر الملاحظة

- Schools Web: إضافة/تعديل ملاحظة بتابين وإطار أعرض 820px، هيدر وفوتر ثابتين وscroll؛ بيانات التصنيف والنص والتقييم والمتابعة والرؤية محفوظة بين التابات وكذلك الملفات المختارة ومفاتيح retry. ملفات المحفوظ لها مدخل من تاب المرفقات؛ تابات الواجب reused بمعرفات مستقلة دون تغيير عقدها الافتراضي. لا تعديل API/Flutter/DB.
- التحقق: `npm run build` ناجح مع تحذير bundle size السابق؛ `npm run lint` بلا أخطاء مع 3 تحذيرات سابقة؛ browser fixture على المكون الحقيقي أكد 820x680 وtop20 في التابين، footer ضمن الشاشة ونص محفوظ عند الرجوع. لقطتا `.codex-artifacts/note-details-tabs.png` و`note-attachments-tabs.png`؛ لا رفع/حفظ حقيقي أو جهاز موبايل. `git diff --check` ناجح.

## 2026-10-14 — تحديث رفع المرفقات المحفوظة للتصميم الجديد

- استبدال زر الرفع الصغير بمنطقة dashed كاملة بأيقونة وتعليمات عربية/إنجليزية للسحب أو الاختيار في WorkAttachments المشترك للواجبات والأنشطة والملاحظات. توضيح الرفع الفوري، مع المعرض والصلاحيات الحالية؛ منع تجاوز عدد 10 وبداية رفع متزامن، وحالة hover/focus/drop وتعطيل أثناء الطلب/بلوغ الحد.
- التحقق: Schools Web `npm run build` ناجح (تحذير حجم bundle سابق)، و`npm run lint` بلا أخطاء مع 3 تحذيرات سابقة، و`git diff --check` ناجح مع تنبيهات line endings فقط. لا فحص متصفح/منتقي ملفات فعلي أو نشر أو تغيير API/Flutter/migrations في هذه الدفعة.

## 2026-10-14 — تثبيت حجم محرر الواجب ورفع شبيه بالخطط ومعرض مرفقات

- Web: height responsive ثابت لمحرر الواجب، ومنطقة drop/click مشتركة مثل الخطط مع قواعد العدد/النوع/الحجم السابقة. Gallery للمختار بصور مصغرة وأيقونات MP4/PDF ونافذة سابق/تالي وإزالة قبل الحفظ؛ روابط مؤقتة بتنظيف وإطار PDF MIME ثابت وsandbox بلا scripts. المحفوظ gallery grid ومصغرات صور fetch مصادق/Abort/revoke، ومعرض سابق/تالي ينزل المحتوى عند الاختيار فقط، وEscape يغلق المعاينة لا نافذة المرفقات. Flutter منتقي أوضح وكروت صور/أنواع للمختار والمحفوظ؛ PageView للمختار مع صور، وأيقونة للفيديو/PDF قبل الحفظ تنبه أنهما يتاحان بعده، بينما تشغيل/تنزيل المحفوظ كما سابقًا. لا توليد thumbnail فيديو/API/DB/migration أو تغيير uploadId/retry. تحسينات المكونات المشتركة تنطبق أيضًا على مرفقات الأنشطة/الملاحظات دون تغيير سيرها.
- التحقق: Web build ناجح وlint بلا أخطاء/3 تحذيرات سابقة بعد capture node في cleanup، diff checks ناجحة. Flutter tests 38/38 (اختبار swipe/فتح/إغلاق gallery وعدم تغيير القائمة جديد)، dart analyze بلا مشاكل. Browser على المكون الحقيقي مع mock: التفاصيل والمرفقات كلاهما 900x680/top20 في viewport720؛ dropzone ظاهرة، وثلاث بطاقات صورة/MP4/PDF، ومعرض 1/3 ثم 2/3 ثم 3/3 وiframe PDF، وإغلاق/حفظ القائمة. الفيديو/PDF fixtures رمزية لا تأكيد decode حقيقي؛ PNG معاينة صغيرة. صور `.codex-artifacts/homework-dropzone-fixed.png` و`homework-attachment-gallery.png` فُحصت. لا رفع OS/جهاز أو مدرسة فعلية أو نشر؛ تحديث ملفات السياق الأربعة.

## 2026-10-14 — تبويبا تفاصيل ومرفقات وتصميم محرر الواجب

- نُقل منتقي المرفقات ومرفقات المسودة المحفوظة إلى تاب مستقل في إنشاء/تعديل الواجب Web/Flutter؛ Details افتراضي وعداد للقائمة، وإبقاء الحقول والملفات وId المسودة ورفع retry كما هي. Web بهيدر وفوتر ثابتين وscroll داخلي وبطاقات ومسافات وأزرار وRTL وARIA/تنقل أسهم للتابات، وFlutter SegmentedButton وVisibility maintainState وجسم scroll وحفظ/إلغاء ثابتين. أضيف onChanged للنصين في Flutter لتحديث حالة زر الحفظ فور الإدخال. النشاط والملاحظة دون تبديل هيكلهما؛ لا API/DB/migration أو بيانات فعلية/نشر.
- التحقق: Web npm run build ناجح وlint بلا أخطاء/3 تحذيرات سابقة، diff check ناجح. Flutter tests 37/37 (اختبار تبويبين جديد يحفظ النص ويعرض عدد ملف واحد بعد التبديل)، dart analyze بلا مشاكل. Browser على محرر الواجب الحقيقي بmock أكد ظهور التابين وعزل المرفقات، حفظ عنوان/وصف بعد التبديل، وأزرار الفوتر داخل viewport720. الصور `.codex-artifacts/homework-details-tab.png` و`homework-attachments-tab.png`؛ معاينة تبويب المرفقات فُحصت بصريًا. لا جهاز/مدرسة فعلية، ولا رفع OS جديد أو تغيير سياسات السيرفر. تحديث ملفات السياق الأربعة؛ تفعيل الواجهة بعد نشرها.

## 2026-10-13 — اختيار المرفقات داخل إنشاء الواجب والنشاط والملاحظة

- Web/Flutter: منتقي صور/PDF/MP4 داخل الإضافة والتعديل مع قائمة محلية وحذف قبل الحفظ؛ حفظ الأصل ثم رفع القائمة، وعدم نشر النشاط قبل اكتمالها. إعادة المحاولة تبقي Id الأصل المؤكد وتستبعد الملفات الناجحة؛ الخطأ يظهر داخل النافذة. نافذة الملاحظة Flutter تحافظ على المحرر أثناء الحفظ/الرفع، وWeb جسم قابل للتمرير وfooter داخل حدود الشاشة. زر الملفات مخصص عربي/إنجليزي، والحدود السابقة ثابتة. أضيف uploadId اختياري لكل ملف إلى APIs الثلاثة وId الرابط الموجود؛ retry نفس SHA256 والحجم يعيد النتيجة دون تكرار، واختلاف المالك/المحتوى أو حذف/رفض الرابط يرفض؛ query retry AsNoTracking كي لا يمس cleanup ملفًا محفوظًا سابقًا. لا migration جديدة أو نشر أو تعديل بيانات فعلية، ولا ادعاء حفظ ذري للمحتوى+الملفات أو idempotency عام لإنشاء الأصل دون تأكيده.
- التحقق: Schools UnitTests 179/179 (اختبار retry ثابت/bytes/owner جديد) وFlutter 36/36 (اختباران جديدان للقائمة المتبقية/ثبات key والإزالة قبل الحفظ)، dart analyze بلا مشاكل. Web build ناجح وlint بلا أخطاء/3 تحذيرات سابقة بعد فصل uploader عن component وإصلاح ref effect. فحص uploader الفعلي في Node/mock: success ثم fail ثم retry يرسل keys one/two/two ويحتفظ بالمتبقي. Browser الحقيقي مع mock أكد ظهور الاختيار في شاشات الإضافة الثلاثة؛ لاحظ overflow في الملاحظة ثم صحح إلى top20/bottom700/footer680 داخل viewport720 مع جسم scroll. لقطة `.codex-artifacts/create-work-attachments-preview.png`. diff checks ناجحة؛ لا منتقي OS/جهاز فعلي أو SQL transaction smoke/تفعيل migrations.
- تحديث ملفات السياق الأربعة؛ التفعيل مع API/Web/Flutter الجديدة وجداول المرفقات السابقة، وتحديث الدليل المكتبي قبل توزيعه. تغييرات worktrees الأخرى محفوظة.

## 2026-10-13 — مرفقات الواجبات والأنشطة والملاحظات في الواجهتين

- أضيفت روابط typed للكيانات الثلاثة إلى DocumentVersion، وتكوينات EF ومهاجرتا PostgreSQL `20261013185358_AddClassWorkAttachments` وSQL Server `20261013185411_AddClassWorkAttachments` (ثلاثة جداول فقط). API رفع/قائمة/محتوى Range/حذف منطقي مع قيود 10/8MiB/25MiB/50MiB وكشف MIME بالترويسة وتنظيف الاسم والأثر، وSerializability للرفع/الحذف؛ يعيد استخدام نطاق كل Controller وصلاحيات view/manage، Draft للواجب/النشاط وCanModify للملاحظة. رفع بعد حفظ الأصل؛ لا تسليم طلاب/Family/notifications جديدة. Storage يزيل الملف الجزئي عند الفشل وتضاف DeleteAsync لتنظيف الرفع غير المكتمل أفضل جهد؛ حذف المستخدم يبقي البايتات للتدقيق. فحص malware ما زال مؤجلًا وحالة Pending.
- Web نافذة مرفقات lazy بمنتقي عربي مخصص ومعاينات وتنزيل وتأكيد حذف وإلغاء وfocus/Escape وتنظيف blob؛ النجاحات الجزئية لا تُخفى. Flutter مداخل التفاصيل والملاحظة وصفحة الرفع/التنزيل ومعاينة الصور/تشغيل MP4؛ PDF تنزيل فقط. الملفات تضبط بعد حفظ الكيان، وتظهر من تفاصيله؛ لا ادعاء رفع ذري أثناء إنشاء الواجب/النشاط/الملاحظة. المرفقات ليست paginated لأن حدها 10؛ قوائم الكيانات تبقى كما هي.
- التحقق: dotnet build Schools API بلا warnings/errors؛ Schools UnitTests 178/178 (5 جديدة: أنواع/حدود، scope/رفض/حذف/Range، روابط domains/حد العدد، رفض ملف مزيف قبل الكتابة، ورفض 3 controllers لمستخدم غير مسند). أُكملت بيانات fixture المطلوبة بعد ظهور NotFound بدل Forbid في اختبار النشاط، ثم نجح الاختبار والمجموعة كاملة. Flutter 34/34 (4 جديدة للتحميل lazy/Auth/قراءة فقط/رفع)، dart analyze بلا مشاكل. Web build ناجح وlint بلا أخطاء/3 تحذيرات سابقة؛ diff checks ناجحة، وEF has-pending-model-changes بلا تغييرات للمزودين. فحص المتصفح بالمكون الحقيقي وmock: العربية/الحدود/تساوي الأزرار 40x44، إلغاء وتأكيد الحذف، منع أزرار الكتابة في read-only، طلب معاينة مصادق وإطار PDF، modal focus وعدم overflow؛ الصورة `.codex-artifacts/work-attachments-preview.png`. لا فحص منتقي/حفظ جهاز حقيقي أو SQL transaction smoke أو قاعدة فعلية أو نشر.
- تحديث CURRENT_STATE/ADR-042/NEXT_STEPS؛ تعليمات التفعيل مركزية بعد backup مع نسخ الواجهتين وAPI، وvolume للتخزين. تحديث Word لاحقًا قبل توزيعه. تغييرات worktrees السابقة محفوظة.

## 2026-10-13 — تنسيق أزرار حضور الفصل وشرح وظائفها

- Schools Web: مجموعة attendance-actions بـflex/wrap وgap 12px وهامش صفر وأبعاد متكافئة وأيقونات. حفظ المسودة والتحديث ثانويان outlined، والحفظ والاعتماد primary؛ tooltip مترجم لكل وظيفة وتحذير صريح بأن التحديث يستبدل الاختيارات غير المحفوظة ولا يحفظها. لا تغيير save(false)/save(true)/load أو الصلاحيات أو API/Flutter أو بيانات فعلية.
- التحقق: npm run build ناجح، npm run lint بلا أخطاء/3 تحذيرات سابقة، git diff --check ناجح (تنبيهات CRLF فقط). راجعنا handlers وAPI: Draft يبقى قابلًا للتحرير، Finalized يقفل التسجيل إلى إعادة فتح مخولة. لا smoke بصري أو نشر؛ تحديث CURRENT_STATE/NEXT_STEPS، ولا قرار معماري جديد.

## 2026-10-13 — ربط أشهر خطط التدريس بفترات التيرم المسجلة

- options في Schools API يحمل فترات التيرم المفعلة، وWeb/Flutter يختاران أسماءها وتواريخها للشهرية والأسبوعية دون توليد شهور ميلادية أو fallback. الفترة الشهرية كما في الإعدادات، وأسابيع السبت–الجمعة تقص بحدودها؛ حالة واضحة عند غياب الفترات. API يتحقق من الفترة، والأسماء من اسم فترة المدرسة واسم العام، مع إبقاء فترة الخطط القديمة عند تحريرها بلا تغيير. لا migration جديدة أو قاعدة فعلية/نشر؛ الربط بالتيرم والتواريخ لا FK جديد.
- التحقق: dotnet test Schools UnitTests 173/173 ناجح؛ Flutter test --no-pub 30/30 ناجح وdart analyze بلا مشاكل؛ npm run build ناجح وlint بلا أخطاء/3 تحذيرات سابقة. فحص Web helper الحقيقي يثبت الاسم المخصص وحدود 5 أكتوبر–3 نوفمبر والأسابيع المقصوصة وعدم توليد أشهر عند غياب الإعدادات. اختبار عنوان الفترة المخصصة ناجح. أُصلحت اختبارات reflection لبارامترات العنوان الجديدة بعد فشلها الأول، ثم أعيدت المجموعة كاملة بنجاح. تحديث ملفات السياق الأربعة؛ لم يُجر اختبار مدرسة فعلية.

## 2026-10-13 — تصحيح بداية أسبوع الخطة وإخفاء الإجازات

- Web/Flutter: الأسبوع السبت–الجمعة، وقص الحدود داخل التيرم، والافتراضي الأسبوع المحتوي على تاريخ المدرسة حتى عبر الشهر. محرر الأيام يخفي الإغلاقات وغير الدراسة بدل عرض حقول معطلة، ولا يطبق نصوصها؛ حالة واضحة للأسبوع بلا دراسة. API يصحح تحقق الفترة وترقيمها مع السماح بفترة خطة قديمة محفوظة دون تغييرها. لا migration جديدة أو تعديل قاعدة/نشر؛ تخصيص الفرع ما زال مؤجلًا.
- التحقق: dotnet test Schools UnitTests ناجح 167/167، Flutter test --no-pub ناجح 29/29، dart analyze بلا مشاكل، npm run build ناجح وnpm run lint بلا أخطاء/3 تحذيرات سابقة. اختبارات بداية السبت وحدود التيرم وإخفاء أيام الإجازة ناجحة؛ تحديث ملفات السياق الأربعة. لا اختبار مدرسة فعلية.

السجل Append-only: تُضاف الإدخالات الجديدة في الأعلى بعد المقدمة مباشرة.

## 2026-10-13 — تنفيذ تفاصيل أيام الخطة الأسبوعية

- نُفذ جدول TeachingPlanDayDetail وتفرد Plan/Date ويوم مشتق من التاريخ، وAPI القراءة/الإنشاء/تعديل المسودة والتحقق والحفظ الذري للأيام مع الأب، وحماية ExpectedRevision/Revision. النص القديم يبقى دون توزيع أو إلغاء، وDaily مستقل. محرر Web/Flutter يعرض الأيام ويفتح popup بحقولها، يطبق محليًا ثم يحفظ الكل، مع إلغاء وتأكيد قبل استبدال أسبوع آخر، وعدم إرسال مراحل الاعتماد أثناء تعديل غير محفوظ. Web التفاصيل والإشعارات وFlutter الإشعارات وبطاقات الخطط تتيح التحرير للمخول في Draft فقط. اختيار الأسبوع في Flutter أصبح مقاطع 7 أيام من بداية التيرم مع حساب UTC للتواريخ المدنية، ومنع بقاء مؤشر تحميل متحرك خلال التحرير.
- تقويم الأيام يشمل مواعيد نوع التعليم العامة وإغلاقات المدرسة/البرنامج/العام، لا تخصيصات الفرع؛ مع غياب المواعيد العامة لا يُفترض يوم إجازة. القيود: يوم مدخل على الأقل، 7 كحد أقصى، 4000 حرف لكل يوم، وعدم تكرار أو خروج التاريخ عن الأسبوع. لا رفع جديد أو Family أو نشر أو تغيير بيانات قديمة تلقائيًا. migrations `20261013170601_AddWeeklyTeachingPlanDays` PostgreSQL و`20261013170612_AddWeeklyTeachingPlanDays` SQL Server تضيف جدول الأيام فقط؛ inspected Up لا يمس جداول أخرى، وRevision تغيير metadata.
- التحقق: dotnet build Schools API بلا warnings/errors؛ Schools tests 162/162 (11 جديدة للحدود/التكرار/الفراغ/الطول/العبور الشهري والأسبوع الجزئي والحفظ/التعديل/بقاء النص القديم والتزامن وفهرس التفرد). Flutter tests 28/28 منها اختباران للمحرر/الإلغاء والقراءة فقط، وdart analyze كامل بلا مشاكل. Web build ناجح وlint بلا أخطاء/3 تحذيرات سابقة، وdiff check للمستودعات ناجح. المتصفح استخدم محررًا حقيقيًا مع mock: الأيام والإجازات، غياب إرسال قبل حفظ الخطة، payload منفصل ليومين، التعديل والإلغاء وEscape وتأكيد تغيير الأسبوع، وتطابق أزرار popup عند ارتفاع 42px؛ معاينة داخلية محفوظة. فحص EF has-pending-model-changes بلا تغييرات لكلا المزودين. لا SQL transaction smoke أو جهاز فعلي أو تطبيق migrations/نشر.
- تحديث CURRENT_STATE/ADR-042/NEXT_STEPS. التفعيل عبر تحديث المدارس المركزي بعد backup مع نشر API/Web/Flutter معًا؛ عقد إنشاء الأسبوع الجديد يحتاج الأيام. ملفات Word القديمة لم تعدل، وتحتاج تحديث شرح الأسبوع قبل إعادة توزيعها. تغييرات worktrees غير المتعلقة محفوظة.

## 2026-10-13 — مقترح تفاصيل أيام داخل الخطة الأسبوعية

- ناقش المستخدم استبدال حقل تفاصيل الأسبوع الواحد بمنطقة عرض وزر يفتح محرر الأيام ونص لكل يوم، مع نفس العرض عند تعديل الخطة. المقترح: سجلات أيام typed تابعة للخطة الأسبوعية (PlanId، التاريخ الفعلي، يوم الأسبوع/مرجعه، Details)، فريدة للخطة والتاريخ وبحدود الأسبوع والتيرم. التاريخ يمنع لبس أسابيع تعبر الشهر، والأيام تتولد من الفترة لا تُضاف عشوائيًا. تفاصيل الأيام ليست خططًا يومية ولا تخلقها تلقائيًا.
- عرض مقروء بالأيام وزر إضافة/تعديل واحد يفتح كل الأيام؛ حفظ المحرر يحدّث محتوى النموذج محليًا، وحفظ الخطة يثبت الكل بصورة ذرية مع صلاحيات الحالة وتزامن الحفظ والتدقيق. مراعاة أيام الدراسة والإجازات، وتغيير الأسبوع يتطلب تأكيد قبل إسقاط نصوصه، والخطط القديمة تحتفظ بنصها دون توزيع مختلق. اقتراح السماح بترك بعض الأيام فارغة مع اشتراط يوم مدخل على الأقل للخطة الجديدة. النقاش لم يعتمد بعد، فلا تغيير API/DB/واجهات أو قرار منفذ.
- التحقق: قراءة سياق المشروع وTeachingPlanEntities فقط؛ يوجد TeachingPlanItem عام بعنوان ونص وفترة اختيارية، فلا يُفترض أنه يفرض تفرد التاريخ أو تفاصيل الأيام تلقائيًا. لا اختبارات كود لهذا النقاش. أضيف العمل المقترح إلى NEXT_STEPS.

## 2026-10-13 — ترتيب العام قبل التيرم في محرر الخطط

- صححت ملاحظة المستخدم: Web يعرض العام أولًا ثم التيرم في الصف نفسه لخطة التيرم، وFlutter يعرض العام قبل التيرم. حافظت على مسح اختيار التيرم وإعادة حصره في العام عند تغييره، والفترات المخفية وقواعد API والأسماء دون تغيير. الأنواع التشغيلية في Web تحتفظ بالفصل والمادة قبل التيرم لأن العام مأخوذ من الفصل؛ لا تغيير Backend أو بيانات.
- التحقق: Web npm run build ناجح، وفحص Dart للملف المعدل بلا مشاكل بعد format؛ ترتيب عناصر النموذج راجع من المصدر. لا اختبار متصفح أو جهاز جديد لهذه الدفعة. تحديث CURRENT_STATE/NEXT_STEPS؛ لا قرار معماري جديد.

## 2026-10-13 — تنفيذ الفترة المخفية وأسماء الخطط المعتمدة

- اعتماد المستخدم صحح المقترح السابق: التواريخ مخفية وليست readonly. نُفذ اختيار العام وإخفاء تواريخ السنوية والتيرم في Web/Flutter، وتحديث داخلي للفترة حسب اختيار العام/الفصل المرجعي/التيرم. API يشتق كامل العام أو التيرم قبل التحقق والحفظ في create/update ولا يثق بتاريخ العميل؛ يحصر التيرم في عام مادة الصف. عناوين السنوية/التيرم/الشهرية تستخدم اسم تعريف العام دون لاحقة المادة أو سنة الشهر الميلادية، مع ترجمات؛ لا تغيير الأسبوعية/اليومية أو بيانات قديمة تلقائيًا ولا migrations.
- التحقق: Schools tests 151/151 (منها 5 اختبارات جديدة للأسماء، تصحيح تاريخ العميل، وإبطال تيرم لا ينتمي للعام)؛ Web build ناجح وlint بلا أخطاء/3 تحذيرات سابقة، وgit diff --check في Backend/Web ناجح. اختبار متصفح معزول للمحرر الحقيقي ببيانات mock أكد صفر input date للسنوية والتيرم، وحفظ فترة 2026/2027 ثم تغييرها إلى 2027/2028 مع تحديث المادة، وحفظ فترة التيرم بدل العام. معاينة محفوظة داخليًا، لا مدرسة فعلية أو نشر. Dart format ناجح وdart analyze للملف المعدل بلا مشاكل؛ لا اختبار جهاز فعلي.
- تحديث CURRENT_STATE وADR-042 وNEXT_STEPS. الأدلة المكتبية القديمة لم تُعدل في هذه الدفعة وتحتاج تصحيح تعليمات الفترة اليدوية ونمط الأسماء قبل إعادة توزيعها؛ التغييرات غير المرتبطة في worktrees محفوظة.

## 2026-10-13 — مقترح اشتقاق فترة الخطة السنوية وخطة التيرم

- طلب المستخدم رأيًا في اختيار السنة للخطة السنوية والتيرم لخطته وجلب تاريخي البداية والنهاية مباشرة بدل إدخالهما يدويًا. التصور المقترح: السنة هي عام البرنامج الخاص بنوع التعليم والصف، والتيرم يتبع العام نفسه؛ اشتقاق الفترة كاملة وعرضها للقراءة فقط، وتحديثها عند تغيير الاختيار، مع تحقق API وعدم الاكتفاء بالواجهة. نقص تعريف التواريخ يمنع الإنشاء برسالة توجيه لإعداد التقويم، لا استخدام تواريخ مخمنة.
- الحالة: نقاش ومقترح بانتظار طلب التنفيذ؛ لم يُعدّل التطبيق أو أدلة المستخدم أو بيانات المدرسة، ولم تُجر اختبارات كود. راجعت سياق المشروع وقرارات خطط التدريس؛ لا قرار منفذ جديد ولا تحديث CURRENT_STATE لسلوك غير منفذ.

## 2026-10-13 — اسم المدرسة وشعارها في هيدر Schools Web

- حسب الصورة والطلب استبدلت هوية مدارسنا في الهيدر المسجل باسم المدرسة وشعارها، مع fallback للشعار المفقود/التحميل الفاشل واختصار الاسم الطويل. مكون SchoolHeaderBrand يحمل metadata ومحتوى الشعار بمصادقة، مع AbortController وتنظيف blob وحماية العرض عند تغيير token أو refreshKey؛ لا API جديد أو صلاحيات أو public URLs. تحديث الشعار من SchoolPresentationPage ينعكس مباشرة. حافظت على التغييرات القائمة غير المرتبطة في worktree؛ لم يتغير الهيدر العام قبل الدخول أو Platform أو Flutter.
- التحقق: npm run build ناجح (تحذير حجم bundle)، npm run lint بلا أخطاء وبـ3 تحذيرات سابقة، git diff --check ناجح. اختبار متصفح للمكون الحقيقي وCSS الحالي ببيانات mock عبر tests/header-brand-browser: شعار مصادق loaded وcontain، لا شعار وفشل المحتوى ثم العودة للشعار، واسم طويل بعرض 260px/ellipsis. صورة معاينة داخلية محفوظة؛ لا اختبار حساب/مدرسة فعلية أو نشر. تحديث CURRENT_STATE/NEXT_STEPS؛ لا قرار معماري جديد.

## 2026-10-07 — تشخيص رسائل اتصال PostgreSQL المحلي

- أرسل المستخدم EF Core Connection[20004] لقاعدتي Identity وPlatform على localhost:5432 دون الاستثناء الداخلي. فحوص read-only: خدمة PostgreSQL 17 Running، وGet-NetTCPConnection يؤكد listeners IPv4/IPv6، وTcpClient يتصل بنجاح، وpg_isready يعيد accepting connections. توقف Docker Desktop لا يثبت سببًا؛ يوجد PostgreSQL محلي فعال.
- سجل PostgreSQL الحالي يتضمن بتاريخ 2026-10-07 22:29:32–33 إغلاق اتصالات من جهة العميل (forcibly closed by remote host)، دون إثبات ارتباطه بالرسائل المرسلة. الجزء المفحوص لا يحسم خطأ مصادقة أو عدم وجود قاعدة. لا يمكن تحديد سبب EF النهائي دون NpgsqlException/InnerException والتوقيت الموافق.
- لم تُغيّر خدمات أو إعدادات أو بيانات أو كود ولم تُطبق migrations. الخطوة التالية طلب الاستثناء الكامل المنقح من الأسرار؛ لا تغيير معماري أو سلوك يستدعي CURRENT_STATE/DECISIONS.

## 2026-10-05 — دليل مستخدم موحد من ملف مراحل إعداد المدرسة

- أُنشئ `docs/guides/School-Setup-User-Manual-AR.docx` مستقلًا، 23 صفحة RTL/Tahoma تغطي المراحل الست عشرة بنفس ترتيب وعناوين ملف المراحل الأصلي، مع خطوات ومواقع الشاشات وحقول أساسية ونتائج تحقق ومشكلات ونموذج متابعة وتسليم. استند إلى ملف المراحل وسياق المشروع وأدلة المستخدم السابقة، مع فحص محدود لنموذج طلب المدرسة والتفعيل وتعريفها لاستكمال المرحلتين الأولى والثانية. يتضمن التكليفات التشغيلية المحدثة وحدود الصلاحيات والحضور، والتمييز بين المنهج وخطة التدريس وبين حفظ الاختبار/النتائج واعتمادها ونشرها. لا Family أو Online أو جاهزية فعلية مفترضة.
- التحقق: `build_school_setup_user_manual.py` نجح؛ `render_docx.py` تعذر لغياب soffice.exe، ثم `verify_school_setup_manual_word.ps1` فتح وحفظ المستند في Microsoft Word وصدر 23 صفحة. فُحصت صور جميع الصفحات عبر Poppler ثم PDFium للتأكد من العرض العربي، وأعيد التوليد بعد ضبط ترتيب bookmark وإظهار تنسيق العناوين صراحة. النسخة النهائية راجعت كاملة دون قص جداول أو صفحات فارغة؛ فهرس 21 رابطًا/bookmark مطابق للصفحات، 20 جدول RTL، ZIP/XML ونص PDF لكل صفحة ناجحة. PDF/PNG للمعاينة الداخلية فقط.
- حُفظ الملف الأصلي والأدلة السابقة دون تعديل، ولم يتغير التطبيق أو النشر أو بيانات المدرسة. لا يُعد نجاح فتح الملف الجديد حسمًا لبلاغ الشاشة البيضاء في الملف الأصلي. تحديث CURRENT_STATE/NEXT_STEPS؛ لا قرار معماري جديد.

## 2026-10-05 — دليل مستخدم المرحلة السادسة عشرة مراجعة الجاهزية وبدء التشغيل

- أُنشئ `docs/guides/School-Setup-Phase-16-User-Manual-AR.docx`، 11 صفحة RTL/Tahoma بفهرس وقوائم مراجعة وتجارب وخطة بدء ومحضر جاهزية قابل للتعبئة في Word والطباعة. يغطي نتائج إعداد الحسابات والمرافق والهيكل الأكاديمي والتقويم والمواد والفصول والإسنادات والطلاب والتكليفات، تجارب الحضور المشترك وهويته وتزامنه والأعمال اليومية والاختبارات والخطط والإشعارات، تصنيف النواقص وقرار البدء وخطة اليوم الأول والأسبوع الأول والدعم والنسخ الاحتياطية.
- استند إلى CURRENT_STATE/DECISIONS/NEXT_STEPS وأدلة المراحل السابقة، مع فحص AcademicSettings للإجراءات الفعلية دون مسح عام للكود. التصنيف والقرار والمحضر إجراءات إدارية مقترحة لا حالات أو شاشة إطلاق جديدة؛ لا يدعي الدليل جاهزية مدرسة أو تنفيذ تجارب عليها. يوضح حدود تصحيح الحضور التاريخي والطلاب، الفرق بين حضور الفصل والاختبار وبين الحفظ والاعتماد والنشر، snapshots القوائم والسياسات، وحدود Family/Online/Push. لا تغيير تطبيق أو بيئة أو بيانات أو أدلة سابقة ولا قرار معماري جديد.
- التحقق: `build_phase16_user_manual.py` نجح مع ZIP/XML؛ `render_docx.py` تعذر لغياب soffice.exe؛ `verify_phase16_manual_word.ps1` فتح وحفظ في Microsoft Word. صُححت كثافة المقدمة بعد انتقال الفهرس، ثم أعيد التوليد والتصدير إلى 11 صفحة وفحص جميع صور النسخة النهائية؛ الفهرس مطابق ومحضر التسليم صفحة واحدة دون قص أو صفحات فارغة. ZIP/XML بعد Word و8 جداول RTL ونص PDF لكل صفحة ناجحة؛ PDF/PNG للمعاينة الداخلية فقط. تحديث CURRENT_STATE/NEXT_STEPS.

## 2026-10-05 — دليل مستخدم المرحلة الخامسة عشرة تجهيز خطط التدريس

- أُنشئ `docs/guides/School-Setup-Phase-15-User-Manual-AR.docx`، 11 صفحة RTL/Tahoma بفهرس وخطوات وحقول وأمثلة ومشكلات وقائمة تسليم. يغطي الأنواع الخمسة ونطاق الصف/الفصول، التمييز عن خطة المنهج، الأشهر داخل التيرم والأسابيع السباعية من بدايته واليومية المحلية، توثيق المصدر والبند الاختياري والمرفقات، المراجعة والاعتماد والنشر والإعادة والأرشفة والإشعارات ومنع النشر المتداخل.
- المطابقة: سياق المشروع وADR-042/048 ثم AcademicSettings وOperationalClassesPage وSchoolNotificationTargetPage وSchoolTeachingPlansController. يوضح أن المرجع فصل لتحديد سياق الصف للسنوية والتيرم، وأن الأسبوع العابر للشهر يظهر تحت شهر بدايته، واليومية خارج التيرم تبدأ ببدايته. لا استيراد وزارة آلي أو اختيار خطة أم أو توليد خطط أصغر؛ بند واحد في المحرر. الملخصات 5/6 خطط دون pagination، صفحة الإشعار تعدل التفاصيل فقط، ولا مدير مرفقات كامل بعد الإنشاء؛ فشل الرفع قد يترك مسودة وملفات جزئية. لا تطبيق أسرة أو تغيير تطبيق أو قرار معماري جديد.
- التحقق: `build_phase15_user_manual.py` نجح مع ZIP/XML؛ `render_docx.py` تعذر لغياب soffice.exe؛ `verify_phase15_manual_word.ps1` فتح وحفظ في Microsoft Word وصدر 11 صفحة. فُحصت صور جميع الصفحات والفهرس دون قص أو تداخل أو صفحات فارغة. ZIP/XML بعد Word و10 جداول RTL ونص PDF لكل صفحة ناجحة؛ PDF/PNG للمعاينة الداخلية فقط. تحديث CURRENT_STATE/NEXT_STEPS.

## 2026-10-05 — دليل مستخدم المرحلة الرابعة عشرة إعداد سياسات التشغيل والاختبارات

- أُنشئ `docs/guides/School-Setup-Phase-14-User-Manual-AR.docx`، 10 صفحات RTL/Tahoma بفهرس وخطوات وحقول ومثال ومشكلات وقائمة تسليم. يغطي المنطقة الزمنية والحضور اليومي/لكل حصة وتخصيص نوع التعليم والمرحلة، صلاحيات وتكليفات منفذ الحضور، نطاقات وإصدارات سياسات الاختبارات وفترات السريان ومفاتيح الإنشاء والاعتماد والنشر والنتائج والتحذيرات ومسؤولي المراحل وتسليم العمل والإشعارات.
- استند إلى سياق المشروع وADR-036 وAttendanceSettings وAcademicSettings وExamsManagementPage وExamWorkflowPanel وSchoolStudentAttendanceController واختيار السياسة في SchoolExamsController. يوضح أن إعداد الحضور لا يضم فترة سريان، وأن سياسة الاختبار كاملة وليست دمج مفاتيح، وأولوية المرحلة ثم نوع التعليم ثم النوع ثم الإصدار، وsnapshot الاختبارات القائمة، وأن إصدارًا جديدًا يوقف المنافس ولو كان مؤرخًا للمستقبل، وأن الصلاحية والنطاق لا تمنحهما السياسة. لا تطبيق أسرة أو اختبارات إلكترونية أو تغيير تطبيق أو قرار معماري جديد.
- التحقق: `build_phase14_user_manual.py` نجح مع ZIP/XML؛ `render_docx.py` تعذر لغياب soffice.exe؛ `verify_phase14_manual_word.ps1` فتح وحفظ في Microsoft Word وصدر 10 صفحات. فُحصت صور جميع الصفحات والفهرس دون قص أو صفحات فارغة. ZIP/XML بعد Word و8 جداول RTL ونص PDF لكل صفحة ناجحة؛ PDF/PNG للمعاينة الداخلية فقط.

## 2026-10-05 — دليل مستخدم المرحلة الثالثة عشرة تسجيل الطلاب وربطهم بالفصول

- أُنشئ `docs/guides/School-Setup-Phase-13-User-Manual-AR.docx`، 10 صفحات RTL/Tahoma بفهرس وخطوات وحقول ومثال ومشكلات وقائمة تسليم. يغطي التسجيل المباشر للمقيدين وطلبات مقر المدرسة والانتظار/الرفض والقبول والتسكين، هوية الطالب والكود العالمي وأولياء الأمور والسعة وتاريخ القيد والبحث والتفاصيل وسجل القيود والإشعارات الداخلية وأثر snapshots أعمال الفصل.
- استند إلى ADR-027 وStudentsPage وSchoolStudentsController. يوضح أن قوائم الطلاب صفحات 20 سجلًا بأسهم وليست scrolling، وأن الإضافة المباشرة لا تضيف قيدًا جديدًا لملف موجود؛ قبول طلب مطابق يمكن أن يستخدم ملفًا موجودًا دون استبدال أوليائه تلقائيًا. لا تعديل/نقل/انسحاب/ترقية/حذف طالب في الشاشة الحالية، وبعض حالات الطلب في التصفية ليست أزرار عمل. يوضح أن محرر أولياء الأمور لا يخصص CanPickup/IsFinancialResponsible/IsEmergencyContact ويبدأها true؛ لا تعتبر تفويضًا رسميًا. تواريخ القيود ومطابقة المستندات تراجع يدويًا. لا Family جديد ولا تغيير تطبيق أو قرار معماري.
- التحقق: `build_phase13_user_manual.py` نجح مع ZIP/XML؛ `render_docx.py` تعذر لغياب soffice.exe؛ `verify_phase13_manual_word.ps1` فتح وحفظ في Microsoft Word وصدر 10 صفحات. فُحصت صور جميع الصفحات والفهرس دون قص أو صفحات فارغة. ZIP/XML بعد Word و6 جداول RTL ونص PDF لكل صفحة ناجحة؛ PDF/PNG للمعاينة الداخلية فقط.

## 2026-10-05 — دليل مستخدم المرحلة الثانية عشرة إعداد جدول الحصص

- أُنشئ `docs/guides/School-Setup-Phase-12-User-Manual-AR.docx`، 11 صفحة RTL/Tahoma كمرجع عملي بفهرس وخطوات وحقول ومثال ومشكلات وقائمة تسليم. يغطي حدود المرحلة وأيام الدراسة، فتح الفصل بحسب نوع التعليم والمرحلة والصف والعام، كل خانة ومادتها ومدرسها الأساسي وبدلاءها وأوقاتها، الفسح والنصاب، الغرف والمشاركة وجدول الغرفة، المراجعة والإشعارات والتعديل والحذف والدمج المؤرخ.
- استند إلى ADR الخاص بالنطاق والغرف والدمج وTeachingPlanningSettings وSchoolTeachingPlanningController. يوضح أن الحفظ مباشر لكل خانة بلا نشر جماعي، وعدد المادة عند التعديل يستبعد الخانة الحالية، وأن الاختيار الفارغ يرث الغرفة الأساسية، وفحص تداخل المعلم يخص الأساسي؛ اكتمال النصاب وتوزيع الحصص/الفسح واحتواء الأوقات وتوافر البدلاء مراجعة يدوية. يميز الدمج المؤقت والتغطية اليومية عن تغيير القالب. لا تعديل تطبيق أو مدرسة فعلية أو أدلة سابقة ولا قرار معماري جديد.
- التحقق: `build_phase12_user_manual.py` نجح مع ZIP/XML؛ `render_docx.py` تعذر لغياب soffice.exe؛ `verify_phase12_manual_word.ps1` نجح بفتح/حفظ Microsoft Word وتصدير 11 صفحة. روجعت صور جميع الصفحات دون قص أو تداخل، والفهرس مطابق. إعادة فحص ZIP/XML بعد Word و6 جداول RTL ونص PDF لكل صفحة ناجحة. PDF/PNG للمعاينة الداخلية فقط.

## 2026-10-05 — دليل مستخدم المرحلة الحادية عشرة ربط المعلمين بالصفوف والفصول

- أُنشئ `docs/guides/School-Setup-Phase-11-User-Manual-AR.docx`، 8 صفحات RTL/Tahoma كمرجع مستخدم بفهرس وخطوات وحقول ومشكلات ومثال وقائمة تسليم. يغطي «المواد والصفوف» ثم «مدرسو الفصل»، تعدد المعلمين والتصفية والبحث والاختيارات المخفية والحفظ الشامل، المراجعة من تكليفاتي وإشعارات الإضافة والإنهاء، وفك الاستخدامات قبل إزالة النطاق. يميز ربط التدريس عن تحديد المدرس الأساسي والبدلاء في الجدول وعن RBAC والتكليفات التشغيلية.
- استند إلى ADR-018 والواجهتين TeacherScopeDialog وClassTeacherScopeDialog والتحقق والإشعارات في SchoolTeachingPlanningController. لم تُعدل التطبيقات أو الأدلة السابقة، ولا قرار معماري جديد.
- التحقق: `build_phase11_user_manual.py` نجح مع ZIP/XML؛ `render_docx.py` تعذر لغياب soffice.exe؛ `verify_phase11_manual_word.ps1` نجح بفتح/حفظ Microsoft Word وتصدير معاينة 8 صفحات. فُحصت صور الصفحات الثماني بصريًا؛ إعادة فحص ZIP/XML بعد Word و4 جداول RTL ونص PDF لكل صفحة ناجحة. ملفات PDF/PNG للمعاينة الداخلية فقط.

## 2026-10-05 — دليل مستخدم المرحلة العاشرة تشغيل مواد الصفوف

- أُنشئ `docs/guides/School-Setup-Phase-10-User-Manual-AR.docx`، 8 صفحات RTL/Tahoma كمرجع عملي بفهرس وخطوات وحقول ومشكلات ومثال وقائمة تسليم. يغطي مراجعة الصف المفتوح وخطة العام ومادة المنهج، الإضافة والحفظ كمسودة والتفعيل وحالات Draft/Active/Closed، التصحيح والإغلاق والحذف والاستعادة. يفصل التشغيل على مستوى الصف السنوي عن تعريف المادة ومادة الفصل وتوزيع المدرسين، ويحذر من تكراره بسبب تعدد الفصول.
- المطابقة: سياق المشروع وADR-014 ثم CurriculumSettings وSchoolCurriculumController وlocalization ومؤشر العلاقة الفريدة في SchoolsDbContext. وثقت شروط تطابق الصف والخطة الفعالة المعينة للعام وحالة مادة الصف، وأن اختيار المواد غير مفلتر تلقائيًا حسب الصف/الخطة وقد لا يظهر التيرم/الإصدار، وأن Web لا يعيد فتح التشغيل المغلق. الصيانة توصيات حذر للمستخدم وليست ادعاء قفل تلقائي للتعديل/الحذف على كل تشغيل مستخدم. لا تغيير تطبيق أو بيانات أو أدلة سابقة ولا قرار معماري جديد.
- التحقق: runtime المرفق وmarker create مرة واحدة، builder `build_phase10_user_manual.py` ناجح. تعذر render_docx.py لغياب soffice.exe؛ البديل `verify_phase10_manual_word.ps1` فتح وحفظ معزول في Microsoft Word وPDF/A ثم Poppler نجح `WORD_OPEN_SAVE_OK Pages=8`. روجعت صور جميع الصفحات النهائية، الفهرس مطابق دون قص أو تداخل أو صفحات فارغة. ZIP CRC وكل XML سليمة، 6 جداول و6 bidiVisual، PDF ثماني صفحات نصية. تحديث CURRENT_STATE/NEXT_STEPS؛ QA داخلية وDOCX فقط للتسليم.

## 2026-10-05 — دليل مستخدم المرحلة التاسعة فتح الصفوف وإنشاء الفصول

- أُنشئ `docs/guides/School-Setup-Phase-09-User-Manual-AR.docx`، 9 صفحات RTL/Tahoma كمرجع عملي بفهرس وحقول وخطوات وأمثلة وصيانة وحل مشكلات وقائمة تسليم. يغطي تشغيل الصف بحسب عام البرنامج، فلترة المرحلة والمسار، الفصول والسعات والفترات والأسماء والأكواد التلقائية، إنشاء فصل فعال قبل تفعيل الصف، وإسناد الغرف بالتواريخ والساعات والسعة والتعارض. يوضح أن إيقاف الصف يعيده مسودة وليس إغلاقًا للعام، وأن تشغيل المواد وإسناد المدرسين والقيد والجدول إجراءات مستقلة.
- المطابقة: سياق المشروع وADR-013 ثم ClassStructureSettings وSchoolClassStructureController وApp/localization. وثقت سعة الصف الاختيارية 1–100000 والفصل الإلزامية 1–10000، الترقيم الذي يشمل المحذوفات، منع تغيير الصف/العام عند الاستخدام وحماية آخر فصل فعال، والغرفة الفعالة القابلة للجدولة من نوع فصل وبسعة كافية. مراجعة مجموع السعات والتواريخ والغرف الأساسية المتداخلة مسؤولية مستخدم الإعداد، دون ادعاء منع تلقائي غير موجود. لا تغيير تطبيق أو بيانات أو أدلة سابقة ولا قرار معماري جديد.
- التحقق: runtime المرفق وmarker create مرة واحدة، builder `build_phase09_user_manual.py` ناجح. تعذر render_docx.py لغياب soffice.exe؛ البديل `verify_phase09_manual_word.ps1` فتح وحفظ معزول في Microsoft Word وPDF/A ثم Poppler نجح `WORD_OPEN_SAVE_OK Pages=9`. روجعت صور جميع الصفحات النهائية، الفهرس مطابق دون قص أو تداخل أو صفحات فارغة. ZIP CRC وكل XML سليمة، 8 جداول و8 bidiVisual، PDF تسع صفحات نصية. تحديث CURRENT_STATE/NEXT_STEPS؛ QA داخلية وDOCX فقط للتسليم.

## 2026-10-05 — دليل مستخدم المرحلة الثامنة المواد والمناهج والكتب

- أُنشئ `docs/guides/School-Setup-Phase-08-User-Manual-AR.docx`، 12 صفحة RTL/Tahoma كمرجع مستخدم عملي بفهرس وخطوات وحقول وأمثلة وحل مشكلات وقائمة تسليم. يغطي تعريف المواد وخطة المنهج وإصدارها ومواد الصفوف والتيرم والحصص، الكتب والطبعات والأدوار والروابط، مراجعة التفعيل وتعيين الخطة لعام البرنامج وتشغيل المواد. يفرق المنهج عن خطة التدريس والتعريف عن التشغيل السنوي، ويوضح تأجيل التشغيل إلى فتح الصفوف وعدم وجود رفع ملفات الكتب في الشاشة الحالية.
- المطابقة: سياق تاريخ المشروع أولًا ثم CurriculumSettings وlocalization وSchoolCurriculumController. وثقت حدود الأسماء والأكواد والحصص وسنة النشر، حماية النسخ المفعلة والطبعات المستخدمة، اختيار خطة العام المحفوظ مباشرة وقفل التغيير بعد بدء العام مع استثناء أول تعيين للعام القديم، وتحذيرات التفعيل القابلة للتجاوز بتأكيد صريح. لا تغيير تطبيق أو صلاحيات أو قواعد بيانات أو أدلة سابقة، ولا قرار معماري جديد.
- التحقق: runtime المرفق وmarker create مرة واحدة؛ `build_phase08_user_manual.py` نجح، وrender_docx.py تعذر لغياب soffice.exe. البديل `verify_phase08_manual_word.ps1` فتح وحفظ DOCX في Microsoft Word معزول وصدّر PDF/A ثم Poppler؛ `WORD_OPEN_SAVE_OK Pages=12`. روجعت صور الصفحات الاثنتي عشرة دون قص أو تداخل أو صفحات فارغة والفهرس مطابق. ZIP CRC وكل XML سليمة، 13 جدولًا و13 bidiVisual، PDF 12 صفحة نصية. تحديث CURRENT_STATE/NEXT_STEPS؛ QA داخلية وDOCX فقط للتسليم.

## 2026-10-05 — دليل مستخدم المرحلة السابعة العام والتيرمات والتقويم

- أُنشئ `docs/guides/School-Setup-Phase-07-User-Manual-AR.docx`، 11 صفحة RTL/Tahoma كمرجع مستخدم عملي: تعريف العام دون تواريخ، ربطه بأنواع التعليم مع التواريخ، التيرمات وترتيبها والفترات، أيام وساعات الدراسة والتوقيت، الأحداث ونطاقها والإغلاق، الصيانة والمشكلات وقائمة التسليم. أمثلة تواريخ غير رسمية بوضوح، لا ادعاء بمنع التداخل/احتواء التواريخ تلقائيًا، ولا خلط التقويم بجدول الحصص أو إنشاء الاختبارات أو المناهج. مصدر المطابقة تاريخ المشروع ثم AcademicSettings/AttendanceSettings/localization وSchoolAcademicSettingsController/SchoolStudentAttendanceController والمؤشرات الأكاديمية.
- نقص مؤكد موثق دون إصلاح: فحص الإغلاق في ResolveContext لا يستخدم BranchId، وفحص يوم الحضور Daily لا يستخدم الفرع. تحذير واضح في الدليل من اعتماد إغلاق فرع منفرد أو أيام فروع مختلفة دون مراجعة النطاق؛ NEXT_STEPS يسجل الاستكمال بانتظار طلب تنفيذ. لا تغير تطبيق أو حسابات أو بيانات مدرسة فعلية.
- مهارة documents: runtime المرفق وmarker create مرة واحدة، builder `build_phase07_user_manual.py` وverifier `verify_phase07_manual_word.ps1` داخل `.codex-artifacts`. تعذر render_docx.py لغياب soffice.exe؛ Word معزول فتح وحفظ وصدّر PDF/A ثم Poppler، `WORD_OPEN_SAVE_OK Pages=11`. صُحح اختفاء ترويسة جدول حقول الأحداث في Word بإلغاء repeat لذلك الجدول الأحادي الصفحة؛ أُعيد render وفحص صور الصفحات 1–11 النهائية دون قص أو تداخل أو صفحات فارغة، والفهرس مطابق. ZIP CRC وكل XML سليمة، 11 جدولًا عربيًا و11 bidiVisual وPDF 11 صفحة نصية؛ ملفات QA داخلية وDOCX فقط للتسليم. تحديث CURRENT_STATE/NEXT_STEPS دون تعديل الأدلة السابقة.

## 2026-10-05 — دليل مستخدم المرحلة السادسة أنواع التعليم والمراحل والصفوف

- أُنشئ `docs/guides/School-Setup-Phase-06-User-Manual-AR.docx` كمرجع عملي للمستخدم ومسؤول إعداد المدرسة، 9 صفحات RTL/Tahoma بفهرس وحقول وخطوات وأمثلة وصيانة وحل مشكلات وقائمة تسليم. يغطي أنواع التعليم ونظامها، المراحل، المسارات الاختيارية، الصفوف الثابتة وحدود الحصص 1–20 والفسح 0–10 والمجموع 24، والأكواد ونطاق عدم التكرار. لا يخلط التعريف بتشغيل الصف السنوي والفصل الفعلي أو القسم، ويؤجل الأعوام والتيرمات والمناهج والفصول إلى مراحلها.
- المطابقة: تاريخ المشروع ثم AcademicSettings/ClassStructureSettings/App/localization وSchoolAcademicSettingsController/SchoolClassStructureController ومؤشرات الكود في SchoolsDbContext. وثق الدليل أن مسارات مراحل أخرى قد تظهر في اختيار مسار الصف مع وجوب التطابق ورفض الربط الخاطئ، وأن تقليل خانات المرحلة المستخدمة قد يتطلب مراجعة الجدول. لا تغيير تطبيق أو صلاحيات أو بيانات فعلية، ولا تعديل أدلة سابقة.
- مهارة documents: runtime المرفق وmarker create مرة واحدة. builder `build_phase06_user_manual.py` وverifier `verify_phase06_manual_word.ps1` داخل `.codex-artifacts`. تعذر render_docx.py لغياب soffice.exe؛ بديل Word معزول فتح وحفظ وصدّر PDF/A ثم Poppler. عولج انتقال خاتمة المسارات إلى صفحة منفردة؛ النهائي `WORD_OPEN_SAVE_OK Pages=9` وفهرس مطابق، وفحص صور الصفحات التسع دون قص أو تداخل أو صفحات فارغة. ZIP CRC وكل XML سليمة، 10 جداول و10 bidiVisual، PDF تسع صفحات نصية. تحديث CURRENT_STATE/NEXT_STEPS؛ ملفات QA داخلية وDOCX فقط للتسليم.

## 2026-10-05 — دليل مستخدم المرحلة الخامسة إعداد الأقسام ورؤسائها

- أُنشئ `docs/guides/School-Setup-Phase-05-User-Manual-AR.docx` كمرجع عملي لمستخدم المدرسة: 9 صفحات RTL/Tahoma، فهرس، إنشاء القسم التعليمي/الإداري والنطاق والتبعية، الأعضاء والقسم الأساسي، تعيين/تغيير/إنهاء الرئيس، مواد القسم ومنسقيها عند جاهزيتها، تعديل/إيقاف/حذف/استعادة، مشكلات وسيناريو وقائمة تسليم. تمت المطابقة مع تاريخ المشروع أولًا ثم DepartmentSettings وSchoolDepartmentsController؛ لا واجهات مختلقة أو API أو أكواد صلاحيات. الرئاسة والعضوية لا تمنح RBAC أو إسناد التدريس، ومنسق المادة معلم عضو، وخيارات نطاق التنسيق التفصيلية ليست في Web الحالي.
- مهارة documents: runtime المرفق وmarker create مرة واحدة، builder `build_phase05_user_manual.py` وverifier `verify_phase05_manual_word.ps1` داخل `.codex-artifacts`. تعذر `render_docx.py` لغياب soffice.exe؛ فتح وحفظ معزول في Microsoft Word وتصدير PDF/A ثم Poppler نجح `WORD_OPEN_SAVE_OK Pages=9`. روجعت صور الصفحات 1–9 دون قص أو تداخل أو صفحات فارغة؛ PDF/PNG داخليان للمراجعة وليس التسليم.
- التحقق: بناء DOCX وWord ناجحان؛ ZIP CRC وكل XML سليمة، 7 جداول و7 bidiVisual و261 bidi، وPDF تسع صفحات كلها نصية. تحديث CURRENT_STATE وNEXT_STEPS؛ لا تغيير التطبيقات أو الحسابات أو قواعد البيانات، ولا تعديل أدلة المراحل السابقة.

## 2026-10-05 — دليل مستخدم المرحلة الرابعة إعداد المباني والغرف

- أُنشئ `docs/guides/School-Setup-Phase-04-User-Manual-AR.docx` كمرجع لمستخدم المدرسة ومسؤول setup، وليس شرحًا تقنيًا: 10 صفحات RTL/Tahoma، مسار الهيكلة المادية الحقيقي، ترتيب الفروع/المباني/الأدوار/الأنواع/القدرات/الغرف، شرح الحقول وأمثلة واقعية معنونة كأمثلة، خصائص المعمل والفصل والسعة والجدولة، الفرق بين الغرفة والفصل، الصيانة والاستعادة وتكليف مسؤولي الأدوار عند وجود فصول مرتبطة، مشكلات وقائمة جاهزية. مصدر المطابقة docs أولًا ثم FacilitiesSettings/localization/SchoolFacilitiesController في النطاق المحدد. لا تعديل التطبيق أو صلاحيات أو بيانات فعلية.
- مهارة documents: runtime المرفق وpython-docx، marker create نجح مرة واحدة، builder `.codex-artifacts/build_phase04_user_manual.py` وverifier Word مستقل. فشل render_docx.py بسبب غياب soffice.exe؛ بديل Word.Application معزول فتح وحفظ الملف النهائي وصدّر PDF/A وPoppler صور PNG داخلية. صُححت فواصل الصفحات وفراغات الجداول بعد مسودة 15 ثم 11 صفحة؛ النسخة النهائية 10 صفحات وفهرس مطابق، وفُحصت جميع الصور 1–10 دون قص/تداخل/صفحات فارغة، وأُغلقت نسخة Word المنشأة فقط.
- التحقق النهائي: builder ناجح؛ verify_phase04_manual_word.ps1 نجح WORD_OPEN_SAVE_OK Pages=10؛ فحص ZIP CRC/XML/RTL والجداول (11) وعدد صفحات PDF (10) ناجح. DOCX وحده للتسليم؛ ملفات المعاينة PDF/PNG داخلية. لا build للتطبيق لأن العمل توثيقي فقط.

## 2026-10-05 — إصلاح انحراف محاذاة أزرار التكليفات

- صور المستخدم أظهرت استمرار انخفاض التحديث والإلغاء بعد التنسيق السابق. السبب .inline-action العام به margin-top:16px لم يلغَه التنسيق المحلي. ضبط margin:0 وheight/min-height:44px وbox-sizing:border-box وline-height وحدود شفافة للزر الأساسي، داخل assignment-panel فقط.
- Browser QA للمكون الحقيقي مع App.css وبيانات fetch mock معزولة: الإضافة والتحديث y=163 وheight=44 والهامش=0؛ الحفظ والإلغاء y=623 وheight=44 والهامش=0. npm run build نجح مع تحذير حجم bundle السابق. لا اتصال API فعلي أو تغيير بيانات؛ أزيل ملفا fixture وأغلق تبويب وخادم Vite الاختباريان فقط بعد الفحص.

## 2026-10-05 — تنسيق أزرار شاشة التكليفات في Web

- أبلغ المستخدم عن التصاق إضافة التكليف والتحديث. أضيفت مجموعة إجراءات flex بفراغ 12px ولف متجاوب، الإضافة إجراء أساسي والتحديث والإغلاق ثانويان outlined، مع أيقونات وارتفاع 42px وحالات focus/disabled. جمع حفظ/إلغاء النموذج في footer بعرض كامل بدل خلايا grid منفصلة؛ لا تغيير API/صلاحيات/Mobile.
- التحقق: npm run build ناجح مع تحذير حجم bundle السابق؛ npm run lint بلا أخطاء مع التحذيرات الثلاثة السابقة. لم تُجر معاينة متصفح فعلية في هذه الدفعة؛ لا نشر أو تعديل بيانات مدرسة.

## 2026-10-05 — تشخيص رفض اتصال RabbitMQ المحلي

- اللوج المرفق يؤكد بدء Schools API على 7251/5251، مع رفض اتصال 127.0.0.1:5672 (SocketException 10061) لخدمتي provisioning وdatabase migration consumers وإعادة المحاولة؛ لا دليل في اللوج على توقف API أو فشل مصادقة RabbitMQ.
- التحقق المحلي read-only: Get-NetTCPConnection للمنفذ 5672 لم يرجع listener، وGet-Service للنمط Rabbit لم يرجع خدمة. لا يحسم ذلك هل RabbitMQ مثبت عبر Docker أو عنوانه المقصود مختلف. لم تُشغل خدمات أو تُعدل إعدادات أو migrations؛ المطلوب إتاحة broker على العنوان المقصود ثم إعادة التحقق من اتصال المستهلكين.

## 2026-10-05 — نقل زر مسؤولي الدور إلى إجراءات الصف في Web

- بناء على طلب المستخدم، أزيل شريط أزرار مسؤولي الدور المستقل من FacilitiesSettings وأضيف زر عربي/إنجليزي بأيقونة بجانب تعديل كل دور، مع accessible label يتضمن اسم الدور؛ يفتح نفس ClassAssignments بـfloorId الصحيح.
- احتفظ بشرط school.operations.manage والدور النشط وغير المحذوف وخارج سلة المحذوفات؛ لا تعديل API أو Mobile أو RBAC أو قواعد فعلية. التحقق: npm run build ناجح مع تحذير حجم bundle السابق، وnpm run lint بلا أخطاء مع التحذيرات الثلاثة السابقة، وgit diff --check للويب والتوثيق ناجح. لا اختبار متصفح أو نشر في هذه الدفعة.

## 2026-10-05 — تشخيص غياب مدخل مسؤولي الدور في Web

- أرفق المستخدم قائمة الأدوار ونموذج تعديل الدور دون مدخل المسؤولين. مراجعة FacilitiesSettings داخل App.tsx تؤكد أن المدخل الحالي شريط أزرار منفصل فوق القائمة، وليس حقلًا في نموذج التعديل أو إجراءً داخل صف الدور؛ يشترط الدور النشط وخروج سلة المحذوفات وصلاحية الجلسة school.operations.manage.
- لا تحسم اللقطات هل غياب المدخل بسبب نسخة Web قديمة أم صلاحية الجلسة؛ لم تُفحص جلسة المستخدم أو تُغير صلاحيات أو migrations. يجب التمييز بين التنفيذ المحلي وظهوره في التطبيق الجاري. التحقق: قراءة المصدر وgit status فقط، دون تعديل التطبيق أو ادعاء إصلاح/نشر.

## 2026-10-05 — تنفيذ تكليفات الفصول والأدوار والحضور المشترك

- بعد موافقة المستخدم، نُفذ ADR-051 في Schools API/Web/Flutter: جداول assignments/grants/audits، إسناد موظف أو معلم لفصول مباشرة أو دور وقائمة فصول صريحة، إجراءات الحضور اليومية/بالحصة/الاعتماد/إعادة الفتح ونتائج المادة، وفترة داخل سنة واحدة. عدة مستخدمين للإجراء نفسه دون تغيير RBAC أو إسناد التدريس؛ مدير التكليف لا يفوض ما لا يملك، وRBAC الحالي والنطاق يتحققان من DB لكل إجراء مفوض. نقل الفصل خارج الدور أو إلغاء المادة/التكليف يسحب الوصول؛ إنهاء تكليف قديم ممكن حتى بعد نقل الفصل.
- سجل حضور واحد بمسودات مشتركة وrevision وUnitKey وحماية من نسخة قديمة/حصة تغيرت، وتحديث entries في مكانها. snapshots للمعلم الأساسي والفعلي/البديل وهوية المسجل منفصلة عن آخر محرر ومعتمد، audit للقيم قبل/بعد والتوقيت، API وسجل عمليات في الواجهتين. أضيف اعتماد مستقل للمسودة دون صلاحية تعديلها، وإعادة فتح بسبب. نتائج الورقة محدودة بمواد وفصول المرشحين، والإرسال يحتاج تغطية الورقة كلها، مع ExpectedUpdatedAtUtc وconcurrency token مستقل عن revision النشر؛ لا اعتماد أو نشر ضمني من تكليف الرصد.
- مداخل من قائمة الفصول وكارتها/مساحتها والشخص والدور في الواجهتين، وإنشاء وتعديل وإنهاء التكليف وتحذير عند نقص صلاحيات الحساب. تعديل الوصف أو الفترة يحفظ توزيع grants الأصلي دون توسيع إجراءات فصل لآخر في تكليف غير متجانس. إشعارات الإنشاء والتعديل والإنهاء الإداري تفتح صفحة التكليفات المسموحة؛ لا إضافة Family أو Push أو job لإشعار انتهاء التاريخ. انتهاء التاريخ نفسه يسحب النطاق تلقائيًا.
- migrations أُنشئت ولم تُطبق: PostgreSQL `20261004235056_AddClassOperationalAssignments` و`20261004235959_ProtectExamEntryConcurrency`؛ SQL Server `20261004235214_AddClassOperationalAssignments` و`20261005001212_ProtectExamEntryConcurrency`. الثانية metadata فقط لتزامن الرصد. فحص `dotnet ef migrations has-pending-model-changes --no-build` لكل context نجح بلا تغيرات model معلقة باستخدام design-time placeholders محلية؛ لا اتصال بمدرسة فعلية أو نشر. يلزم تفعيل مركزي بعد backup ونشر API/الواجهتين معًا بسبب تغير عقود الحفظ.
- التحقق: `dotnet test tests/Mdaresna.Schools.UnitTests --no-restore` نجح `146/146` (15 حالة إضافية تشمل تعدد المسؤولين، الصلاحية AND النطاق، سحب الوصول، نقل الفصل، إشعار/audit دون تغيير أدوار، موظف يسجل حصة معلم، منع stale overwrite، اعتماد فقط دون تعديل، تضييق المادة، وترجمة الاستعلامات للمزودين). `npm run build` ناجح مع تحذير حجم bundle السابق، و`npm run lint` بلا أخطاء مع 3 تحذيرات سابقة فقط. Flutter `analyze --no-pub` نظيف و`test --no-pub` `26/26`. `git diff --check` ناجح في المستودعات الثلاثة؛ لا اختبار تزامن/transactions أو تطبيق migrations على SQL حقيقي ولا تجربة جهاز موبايل في هذه الدفعة.
- Browser QA معزول على Vite خاص بالاختبار: واجهة عربية RTL، اختيار موظف وفصلين بالدور وحضور الحصص والاعتماد، ومراجعة JSON طلب الحفظ للتأكد من النطاق والإجراءات؛ no live API/accounts. أزيل ملفا fixture المؤقتان وأوقف خادم الاختبار الذي أنشأته فقط؛ خدمات المستخدم الأخرى محفوظة.
- دليل المستخدم المحدث مستقل `docs/guides/School-Setup-Phase-03-User-Manual-Operational-Assignments-AR.docx`: 9 صفحات وفهرس محدث وتكليف المسؤولين والحضور المشترك، دون أكواد API. الأصل مفتوح/مقفل في Word فحُفظ دون تعديل. `render_docx.py` تعذر لغياب soffice.exe؛ التحقق البديل بفتح وحفظ النسخة الجديدة وحدها في Microsoft Word معزول وتصدير PDF للمراجعة نجح، وفُحصت PNG للصفحات 1–9 الحالية عبر Poppler دون قص أو صفحات فارغة. PDF/PNG داخليان للمراجعة؛ تحديث CURRENT_STATE/DECISIONS/NEXT_STEPS.

## 2026-10-05 — تعدد مسجلي الحضور وهوية صاحب الحصة

- أكد المستخدم ضرورة السماح لعدة أشخاص مخولين بنفس الإجراء في الفصل/الدور، وتمييز الموظف الذي سجّل الحضور عن المعلم صاحب الحصة عند نمط PerSession. توسيع مقترح التكليف: لا حصرية للمسجل؛ سجل واحد للفصل والتاريخ ووحدة الحصة، به سياق المادة والموعد والمعلم الأساسي والمعلم الفعلي/البديل وقت التنفيذ، وهوية منفذ كل إجراء مستقلة عن تلك الهويات.
- مقترح بانتظار التنفيذ: منشئ وآخر محرر ومعتمد للسجل، وتدقيق لكل تعديل طالب بالقيمة السابقة والجديدة والمنفذ والتوقيت؛ لا إعادة نسبة السجل للمعلم لمجرد أن الحصة تخصه. صلاحية التكليف بالحصة تتحقق لكل طلب، ولا تعني مسؤولية الدور حق تسجيل كل الحصص تلقائيًا. تعدد المسؤولين لا ينشئ سجلات حضور موازية؛ حماية التزامن تمنع الكتابة فوق نسخة قديمة، والاعتماد يقفل التحرير إلا بإعادة فتح مخولة ومدققة. حفظ المسودة لا يعني اكتمال حضور بقية الطلاب.
- لا تغيير تطبيق أو قاعدة أو Word في هذا النقاش؛ قراءة مقدمات ملفات التاريخ بالترتيب وفحص diff للتوثيق فقط.

## 2026-10-05 — تصور تكليف الموظفين والمعلمين بالفصول والأدوار المكانية

- طلب المستخدم خطة لربط الموظف أو المعلم بالفصل أو بالدور داخل المبنى ومنحه أعمالًا محددة كالحضور ورصد الدرجات. الفحص المحدود: BuildingFloor يمثل المكان دون مسؤول، وحضور الطلاب يحسم النطاق عبر ClassSectionTeacherScopes والمدير؛ منح صلاحية لموظف وحده لا ينفذ هذا الربط. عبارة «المسؤول المحدد للفصل» في ADR-029 ليست دليلًا على وجود إسناد إداري مستقل حاليًا.
- مقترح بانتظار الاعتماد: تكليف تشغيلي مؤرخ للمستخدم المحلي، مستقل عن إسناد التدريس وRBAC، للفصل أو الدور مع قائمة فصول صريحة في العام الدراسي، وصف مسؤولية وإجراءات محددة. الإجراء يتطلب صلاحية المستخدم وتكليفًا جاريًا يبيح نفس الإجراء في نفس الهدف؛ الرصد يُضيَّق بالمادة/ورقة الاختبار ولا يمنح اعتمادًا أو نشرًا. الحضور اليومي منفصل عن تفويض حضور الحصة، ونقل الفصل بين الأدوار لا يوسع الوصول تلقائيًا.
- التنفيذ المقترح يشمل Backend وWeb وFlutter وإشعارات التكليف/التغيير/الإنهاء والتدقيق واختبارات سحب الوصول وتعدد التكليفات. لا تعديل تطبيق أو قاعدة أو دليل Word في هذا النقاش. التحقق: rg وقراءة SchoolStudentAttendanceController وSchoolFacilityEntities في نطاق السؤال فقط؛ فحص diff للتوثيق.

## 2026-10-05 — مرجع عملي للمدارس في إعداد المستخدمين والأدوار

- بناءً على رفض تقديم المرجع السابق للمستخدمين، أُنشئت نسخة مستقلة `docs/guides/School-Setup-Phase-03-User-Manual-AR.docx` من 8 صفحات: بدء الإعداد، اختيار المسؤوليات، إنشاء/تعديل الأدوار، إضافة المعلمين والموظفين، التفعيل والتسليم، مراجعة الحسابات، المشكلات، وقائمة الجاهزية. شرح بلغة المستخدم مع أمثلة ورسالة تسليم جاهزة، دون API أو أكواد صلاحيات داخلية؛ لا تغيير للدليلين السابقين أو التطبيقات أو حسابات فعلية.
- التحقق: Python المرفق شغّل `.codex-artifacts/build_phase03_user_manual.py` بنجاح؛ `render_docx.py` تعذر لغياب soffice.exe. بديل `.codex-artifacts/verify_phase03_manual_word.ps1` فتح وحفظ المستند الجديد فقط في نسخة Microsoft Word معزولة وصدر PDF للمراجعة، وأكد 8 صفحات. فُحصت صور الصفحات 1–8 الحالية من Poppler: RTL ومحاذاة عربية، عناوين وجداول واضحة، دون قص أو صفحات فارغة. ملفات PDF/PNG للمراجعة فقط وليست مخرجات التسليم. فحص ZIP/XML وفحص `git diff --check -- docs/project-history` ضمن التحقق النهائي؛ تحديث CURRENT_STATE/DECISIONS/NEXT_STEPS.

## 2026-10-05 — دليل تفصيلي للمرحلة الثالثة المستخدمون والأدوار والصلاحيات

- أُنشئ `docs/guides/School-Setup-Phase-03-Users-Roles-Permissions-AR.docx` من 12 فصلًا وملحق 76 صلاحية أساسية في SchoolIdentitySeed؛ شرح الحقول والخطوات والتفعيل والبحث والحالات وإدارة الأدوار واتحاد الصلاحيات وقيود آخر مدير/الحساب الحالي/المعلم المرتبط، والنطاقات والتوصيات واختبارات القبول والتسليم. فصل واضح بين المنفذ والتوصية، وإسنادات الأقسام والتدريس اللاحقة ليست جزءًا من إنشاء الحساب، والملحق ليس قائمة كاملة لصلاحيات كل migrations.
- مطابقة محدودة للتوثيق ثم App.tsx وSchoolUsersController وSchoolRolesController وSchoolAuthController وSchoolIdentityEntities؛ لا تغيير Backend/Web/Flutter أو نشر أو صلاحيات فعلية، ولا قرار معماري جديد. المصدر القابل للتوليد `.codex-artifacts/build_phase03_guide.py` والتحقق `.codex-artifacts/verify_phase03_word.ps1`.
- التحقق: Python المرفق أنشأ DOCX وفحص RTL والجداول والنص؛ `render_docx.py` فشل لغياب soffice.exe. بديل محلي: Word.Application جديد بلا مستندات سابقة فتح وحفظ الملف الجديد فقط، ثم Repaginate/ComputeStatistics/ExportAsFixedFormat نجحت: 20 صفحة و23240 محرفًا. أُغلقت نسخة Word التي أنشأها التحقق فقط، دون تعديل المستند السابق أو إعدادات Office أو ملفات المستخدم المفتوحة. صُححت المحاذاة العربية باستخدام jc=start، وأزيل border/italic الموروثان، وضُبطت مجموعات الملحق لمنع انقسام الجداول الصغيرة.
- مراجعة صور الصفحات العشرين من تصدير Word عبر Poppler، بما فيها المحاذاة والصفحات والجداول. ظهرت عيوب glyph متغيرة في بعض صور تصدير PDF/Poppler؛ PDF/A والتصيير المنفصل حسّناها لكنها ليست تأكيدًا لخلو معاينة PDF من العيوب، لذا لا يُسلَّم PDF/PNG كمنتج. DOCX فُتح وحُفظ بنجاح في Word؛ نجاحه لا يحسم بلاغ البياض في ملف المراحل القديم. تحديث CURRENT_STATE/NEXT_STEPS؛ فحص `git diff --check -- docs/project-history` وفحص ZIP/XML النهائي موثقان في نتيجة التنفيذ.

## 2026-10-05 — تشخيص ظهور دليل إعداد المدرسة أبيض في Word

- أكد المستخدم فتح School-Setup-Stages-AR.docx في Microsoft Word وأرفق لقطة الملف في Explorer؛ لقطة البياض السابقة لا تُظهر شريط Word أو اسم المستند. فحصنا الملف دون تعديله: حجم 40814 بايت، ZIP CRC سليم، 14 XML قابلة للقراءة، و59 مقطع نص عربي منها العنوان، ولا تنسيق vanish مباشر. ذلك لا يثبت سلامة العرض في Word ولا يحسم سبب البياض.
- تصحيح لفحص البيئة السابق: Microsoft Word مثبت في Office16، وWord.Application مسجل عند استخدام مسار Registry الصحيح بشرطات خلفية؛ استنتاج عدم وجود Word في إدخال إنشاء الدليل كان مبنيًا على مسار غير صحيح. لا جلسة Word متصلة بأداة document control؛ معاينة render_docx ما زالت متعذرة لغياب LibreOffice.
- التحقق: فحص python المرفق بـ`-X utf8` للـZIP/XML ناجح؛ Get-Process/Get-CimInstance وقراءة Registry checks فقط. لم يُغلق Word أو تُغير إعداداته أو يُعاد حفظ المستند. الخطوة التشخيصية التالية فتح المستند من داخل Word عبر File/Open، ومقارنته بملف Word آخر لتحديد إن كانت المشكلة عامة أم خاصة بالمستند.

## 2026-10-05 — إعداد دليل Word عربي لمراحل إعداد المدرسة

- طلب المستخدم دليل setup بمراحل وعناوين عربية وRTL. أُنشئ `docs/guides/School-Setup-Stages-AR.docx` عبر python-docx المرفق: 16 مرحلة في أربع مجموعات، ملخص المطلوب ومعيار اكتمال كل مرحلة، أنماط Title/Heading، Arial واتجاه الفقرات RTL ومحاذاة يمين. الدليل للمدير ومسؤول الإعداد، وليس دليل نشر تقنيًا أو wizard جديدًا؛ Family مؤجل. اعتمد على CURRENT_STATE/DECISIONS دون مراجعة كود التطبيق.
- المصدر القابل لإعادة التوليد `.codex-artifacts/build_school_setup_doc.py`. نجح إنشاء الملف والفحص البنيوي لعدد المراحل واتجاه جميع فقرات المحتوى ومحاذاتها وقراءة ZIP. أمر `render_docx.py ... --output_dir .codex-artifacts/school-setup-render --verbose` فشل بـFileNotFoundError لغياب soffice.exe؛ لا Word.Application مسجل، ولا LibreOffice مرفق ضمن runtime Windows. لم يثبت render بصري أو عدد الصفحات النهائي ولم نستخدم تثبيتًا خارجيًا أو upload لخدمة أخرى.
- طُلب فتح DOCX في Codex ونتيجته queued فقط وليست تأكيد معاينة. أُضيف مرجع الدليل إلى CURRENT_STATE ومهمة استكمال الفحص البصري إلى NEXT_STEPS. لا تغيير تطبيقات أو صلاحيات أو نشر، ولا قرار معماري جديد.

## 2026-10-05 — تأكيد التوثيق أولًا وتسجيل نقاشات Family وpagination

- النوع: نقاش وتوثيق؛ لا تغيير تطبيق أو نشر. أكد المستخدم تسجيل العمل والتعديلات والنقاشات باستمرار لتقليل إعادة قراءة الكود والـtokens. وُضحت القاعدة في README/DECISIONS: التوثيق أولًا، فحص المصدر عند الحاجة فقط، وتسجيل الحالة والقرار والتأجيل والتحقق والنواقص دون أسرار أو تكرار التاريخ.
- خلاصة النقاشات الأخيرة: النشر إلى Family غير متاح حاليًا طبقًا لـADR-049؛ يظل ربطه وعرضه وتفاعلاته إلى مرحلة إنشاء Family. المنشورات والتعليقات paginated فعليًا من API، 15 منشورًا و20 تعليقًا افتراضيًا (حد أقصى 50)، cursor بالمعرف والأحدث أولًا، scrolling وزر تحميل المزيد في Web/Flutter مع منع تكرار الصفوف.
- التحقق من pagination في النقاش السابق: `rg -n` على SchoolSocialController وSchoolSocialPage وschool_social_page أكد before/nextCursor وTake(limit + 1) ومستمعات التمرير؛ دون اختبارات أو تغييرات تنفيذ جديدة. أهم الملفات: docs/project-history/README وCURRENT_STATE وDECISIONS وNEXT_STEPS وهذا السجل. فحص `git diff --check -- docs/project-history` للتوثيق؛ لا حاجة لإعادة بناء التطبيقات لدفعة Markdown.

## 2026-10-05 — نموذج التعليق أعلى القائمة وإجراءات كارت مختصرة

- أُعيد ترتيب التعليقات في Schools Web/Flutter ليكون الإنشاء/التعديل أولًا، ثم الكروت وpagination الحالي. استبدلت أزرار الكارت الدائمة بثلاث نقاط رأسية وقائمة أيقونات مع تسميات مترجمة وإجراءات وفق الملكية/الصلاحيات الحالية؛ أضيف إلغاء التعديل. قائمة Web تغلق بالنقر خارجها/Escape واختيار الإجراء، وتفتح لأعلى لمنع قصها عند آخر كارت؛ اختيار تعديل يركز النموذج، وFlutter يعيد التمرير إليه. لا تغيير Backend/Family/migrations.
- التحقق: Web `npm run build` ناجح و`npm run lint` بلا أخطاء مع 3 تحذيرات سابقة وتحذير حجم bundle. Flutter `flutter analyze` نظيف و`flutter test` 26/26؛ اختبار جديد يثبت ترتيب النموذج وإخفاء الإجراءات قبل فتح القائمة، ومنع تعديل/حذف تعليق غير مملوك، وإلغاء التعديل. browser mock بالعربية أكد النموذج فوق الكارت وأيقونات القائمة واختيار التعديل وإغلاق القائمة وتركيز النص؛ لا اختبار API مدرسة فعلية. تحديث CURRENT_STATE/ADR-050/NEXT_STEPS.

## 2026-10-05 — إصلاح تباين زر تحميل الفيديو داخل المعرض

- كان زر تحميل الفيديو يرث لون النص الأبيض من المعرض الداكن مع خلفية بيضاء من تنسيق الأزرار العام، فأخفى النص والأيقونة. أضيفت قواعد محددة لأزرار الوسائط في lightbox: خلفية زرقاء ونص/أيقونة أبيض، hover/focus واضحان، وتنزيل ثانوي؛ تغطي أيضًا التحميل وإعادة المحاولة دون تغيير API/Flutter/Family.
- التحقق: `npm run build` ناجح و`npm run lint` بلا أخطاء (3 تحذيرات سابقة وتحذير حجم bundle). فحص المتصفح بfixture معزول أكد ظهور «تحميل الفيديو للعرض» وأيقونة التشغيل، واللون المحسوب أبيض فوق rgb(36,84,214)، والتنزيل أبيض فوق خلفية داكنة شبه شفافة. تحديث CURRENT_STATE/NEXT_STEPS؛ لا قرار معماري جديد.

## 2026-09-26 — تنفيذ النطاق المدرسي الأساسي لوحدة الواجبات

- النوع: Domain/Persistence وSchool API وSchool Web وSchool Flutter وmigrations.
- أضيف نموذج الواجب الكامل داخل قاعدة المدرسة: الواجب والأسئلة والاختيارات وفراغات الإكمال والإجابات المقبولة وجمهور الطلاب والمحاولات والإجابات والتدقيق، مع migrations PostgreSQL `20260926132602_AddSchoolHomework` وSQL Server `20260926132640_AddSchoolHomework`.
- أضيفت الصلاحيات `school.homework.view/manage/publish/grade`، ونطاق الفصل/المادة مفروض في الـBackend للمدرس بينما يحتفظ School Admin بنطاق المدرسة.
- أضيفت School APIs لاختيارات المواد والكتب، قائمة الواجبات والتفاصيل، إنشاء/تعديل المسودة، النشر مع snapshot الطلاب، الإغلاق والإلغاء، مزامنة الطلاب الجدد، عرض حالات الطلاب، الإعفاء، وتسجيل استلام ودرجة الواجب الأوفلاين.
- استبدل Placeholder الواجبات داخل «فتح الفصل» في Web وFlutter بواجهة فعلية لاختيار المادة وإنشاء Online/Offline وبناء أسئلة Single/Multiple choice والنص والإكمال والنشر والإغلاق ومتابعة الطلاب وتقييم الأوفلاين، مع نصوص عربية وإنجليزية.
- التحقق: `dotnet build ...Schools.Api -c Debug --no-restore` نجح بلا أخطاء أو تحذيرات؛ `dotnet test ...Schools.UnitTests -c Debug --no-restore` نجح 9/9؛ `npm run build` في Schools Web نجح؛ `flutter analyze` نجح بلا ملاحظات قبل إضافة شاشة تقييم الأوفلاين، وتبعته مراجعة تحليل موجهة بعد الإضافة.
- ملاحظة تشغيل: لم تُطبق migrations على قاعدة مدرسة فعلية في هذه الدفعة؛ يجب تشغيل Action التحديث المركزي ثم smoke test. Family API/UI لم تنفذ التزامًا بالنطاق الحالي.

## 2026-09-26 — حصر التنفيذ الحالي للواجبات في تطبيق المدرسة

- النوع: توضيح نطاق وترتيب تنفيذ، دون تغيير كود.
- حُصر الإصدار الحالي في Domain/Persistence وSchool API وSchool Web وSchool Flutter، وأُجل Family API وFamily App وإشعارات الأسرة إلى مرحلة مستقلة.
- سيظل نموذج البيانات من البداية مستوعبًا للمحاولات والتسليم الأونلاين وقناة `FamilyApp` حتى تضاف لاحقًا دون إعادة تصميم وحدة المدرسة؛ ويعمل الأوفلاين كاملًا الآن بينما يجهز جانب المدرسة إنشاء الواجب الأونلاين وأسئلته وإدارته.

## 2026-09-26 — خطة تنفيذ وحدة الواجبات الكاملة

- النوع: خطة تنفيذ موثقة، دون تغيير كود.
- أضيفت `docs/implementation/HOMEWORK_MODULE_IMPLEMENTATION_PLAN.md` لتغطية واجبات Online/Offline، Question Builder للاختيار والنص والإكمال، snapshot الطلاب، محاولات Family App، التصحيح الآلي واليدوي، School/Family APIs، الصلاحيات، الأمان، التوقيت، الإشعارات، migrations، مراحل التنفيذ والاختبارات ومعايير القبول.
- الخطة تفصل Teacher DTO عن Student DTO لمنع كشف الحلول، وتجعل مصدر العملية مشتقًا من القناة، وتحفظ كل محاولة تاريخيًا، وتؤجل تكامل Gradebook دون تأجيل مصدر Family App.

## 2026-09-26 — إضافة أسئلة إكمال الجملة للواجب الأونلاين

- النوع: توسيع تصميم محفوظ للتنفيذ لاحقًا، دون تغيير كود.
- أضيف `FillInBlank` كنوع مستقل عن الإجابة النصية، ويدعم سؤالًا يحتوي فراغًا واحدًا أو أكثر. لكل فراغ ترتيب ودرجة وقائمة إجابات مقبولة، وتحفظ إجابة الطالب لكل فراغ منفصلة.
- التصحيح الآلي يعتمد التطابق الكامل بعد normalization مضبوط، ولا يستخدم fuzzy matching حتى لا يقبل إجابة مختلفة المعنى. يستطيع المعلم إدخال صيغ صحيحة متعددة، والتحكم في تجاهل المسافات وحالة الأحرف والتشكيل، وتحويل الإجابة غير المطابقة إلى مراجعة يدوية.
- مجموع درجات الفراغات يساوي درجة سؤال الإكمال، وتبقى الإجابات المقبولة ضمن Teacher DTO فقط ولا ترسل للطالب قبل سياسة إظهار الإجابات.

## 2026-09-26 — تصحيح تصور الواجب الأونلاين إلى Question Builder

- النوع: تصحيح نقاش وتصميم محفوظ للتنفيذ لاحقًا، دون تغيير كود.
- المقصود بالـFlow الشبيه بـGoogle هو نموذج أسئلة وإجابة نموذجية قريب من Google Forms، وليس رفع ملفات كمسار التسليم الأساسي.
- ينشئ المعلم أسئلة اختيارية أحادية/متعددة أو نصية، ويحدد الترتيب والإلزامية ودرجة كل سؤال. في الاختياري يحدد الخيارات الصحيحة فيصححها الـBackend تلقائيًا، وفي النصي يحفظ إجابة نموذجية مرجعية مخفية ويصحح المعلم إجابة الطالب يدويًا.
- تحفظ محاولة الطالب وإجابة كل سؤال والخيارات المحددة والدرجة الممنوحة وحالة التصحيح. إذا احتوت المحاولة أسئلة نصية تصبح `PendingManualReview` حتى يكمل المعلم تصحيحها، ثم يحسب النظام المجموع من درجات الأسئلة.
- الإجابات الصحيحة والنموذجية لا تدخل Student DTO قبل سياسة الإظهار، وتثبت بنية الأسئلة بعد النشر حتى لا يرى الطلاب نسخًا مختلفة؛ ويمكن فك النشر فقط إن لم توجد أي محاولة.

## 2026-09-26 — اعتماد الواجب الأونلاين والأوفلاين وقناة التسليم

- النوع: تعديل تصميم محفوظ للتنفيذ مع وحدة الواجبات، دون تغيير كود.
- يحدد الواجب نوع `Online/Offline`. الأونلاين يظهر ويُسلَّم رقميًا من Family App، والأوفلاين يظهر كتوجيه للطالب لكن يسجل المعلم استلامه يدويًا داخل المدرسة من School App.
- مصدر العملية يعمل من أول إصدار ولا يؤجل: `FamilyApp` للأونلاين و`SchoolApp` للأوفلاين، ويشتقه الـBackend بدل قبول قيمة حرة من العميل.
- لدعم Flow قريب من Google Classroom أضيف مفهوم محاولات التسليم: مسودة، تسليم، متأخر، سحب التسليم، إعادة تسليم، إرجاع من المعلم، وتقييم؛ وتحفظ كل محاولة ومرفقاتها تاريخيًا مع هوية الطالب أو ولي الأمر الذي نفذها.
- لا تحفظ الملفات الثنائية داخل قاعدة المدرسة؛ تحفظ metadata وروابط object storage بعد الفحص، مع بقاء `StudentHomework` ملخص الحالة والدرجة الحالية و`HomeworkSubmissionAttempt` تاريخ المحاولات.

## 2026-09-26 — تصور واجبات الفصل حسب المادة

- النوع: نقاش وتصميم محفوظ للتنفيذ لاحقًا، دون تغيير كود.
- يبدأ تبويب الواجبات داخل «فتح الفصل» بالمواد التي يدرسها المستخدم فعليًا في الفصل؛ School Admin يرى كل مواد الفصل، والمعلم يرى نطاقه فقط. اختيار المادة يعرض واجباتها مع زر إضافة واجب للمصرح له.
- يفصل التصميم `HomeworkAssignment` كرأس للواجب عن `StudentHomework` كسجل لكل قيد طالب. الرأس يحفظ الفصل ومادة الفصل والمنشئ ووقت الإنشاء والتعليمات والدرجة القصوى وموعد التسليم وحالة دورة العمل ومرجع كتاب/إصدار اختياري من كتب منهج المادة.
- عند نشر الواجب تنشأ سجلات الطلاب كـsnapshot للقيود الفعالة في الفصل، وتستخدم حالة تسليم متعددة القيم بدل boolean فقط لدعم غير مسلم/مسلم/متأخر/معفى، مع وقت التسليم والدرجة والتغذية الراجعة وهوية ووقت المقيم.
- الكتاب اختياري لأن بعض الواجبات خارج الكتاب، وعند اختياره يجب أن يكون `BookVersion` مرتبطًا بمادة الصف في الخطة الفعالة، مع صفحات أو مرجع اختياري. الواجب المنشور لا يحذف ماديًا بل يغلق أو يلغى لحماية التاريخ.
- درجات الواجب تشغيلية داخل الوحدة، ويجهز التصميم مرجع مصدر لربطها مستقبلًا بالـGradebook دون جعل جدول الواجبات نفسه دفتر الدرجات الرسمي.

## 2026-09-26 — إظهار Action فتح الفصل مستقلًا عن صلاحية الحضور

- النوع: إصلاح واجهة في Schools Web وSchools Flutter.
- كان زر «فتح الفصل» مخفيًا بالكامل ما لم يحمل الـtoken صلاحية `school.attendance.view`، رغم أن مساحة الفصل مدخل عام لتبويبات متعددة. أصبح الزر ظاهرًا لكل مستخدم أعادت له شاشة الفصول فصلًا مسموحًا، بينما تظل كل وظيفة داخل المساحة محمية بصلاحيتها والـBackend الخاص بها.
- التحقق: Schools Web lint/build وSchools Flutter analyze ناجحة بعد التعديل.

## 2026-09-26 — تنفيذ مساحة الفصل ومحرك حضور الطلاب

- النوع: ميزة كاملة في Schools API وSchools Web وSchools Flutter مع migrations.
- أضيفت منطقة زمنية IANA للمدرسة وإعداد `Daily/PerSession` افتراضي مع override لنوع التعليم ثم المرحلة. تستخدم خدمة `SchoolClock` و`TimeProvider` لحساب التاريخ والوقت المحليين من UTC، وتحفظ سجلات الحضور snapshot للمنطقة.
- أضيف Action «فتح الفصل» ومساحة بتبويبات الحضور والواجبات والأنشطة والاختبارات والملاحظات وتقييم الحصة. تبويب الحضور فعلي والباقي مداخل جاهزة للمراحل التالية.
- يسجل النمط اليومي مرة واحدة في يوم دراسة داخل العام، ويسجل نمط الحصة للحصة الجارية غير الفسحة فقط. عند وجود تغطية فعلية يصبح البديل المؤرخ هو صاحب التسجيل بدل المدرس الأساسي، ويظل School Admin قادرًا على التشغيل.
- قائمة الطلاب تبدأ حاضرة افتراضيًا، ويعتمد الحفظ snapshot لكل القيود الفعالة. يمنع unique index تكرار الفصل/التاريخ/الوحدة، ويصبح السجل للقراءة فقط حتى تعيد صلاحية `school.attendance.reopen` فتحه مع Audit trail.
- أضيفت الصلاحيات `school.attendance.view/record/reopen`، ومنحت تلقائيًا لدور School Admin. migrations: PostgreSQL `20260925213520_AddStudentAttendanceOperations` وSQL Server `20260925213557_AddStudentAttendanceOperations`.
- التحقق: Schools API Release build بلا warnings أو errors، واختبارات Schools `9/9`؛ لا توجد pending model changes للمزودين؛ Schools Web lint وproduction build ناجحان؛ Schools Flutter analyze بلا مشاكل واختباراته `4/4` ناجحة.

## 2026-09-26 — اعتماد المنطقة الزمنية للمدرسة في التشغيل

- النوع: قرار تصميم محفوظ للتنفيذ قبل الحضور، دون تغيير كود.
- ستملك كل مدرسة `TimeZoneId` إلزاميًا بصيغة IANA مثل `Africa/Cairo`. لا تستخدم قواعد الحضور أو الحصص أو الإشعارات توقيت جهاز العميل أو التوقيت المحلي للسيرفر.
- تحفظ timestamps التقنية كـUTC، بينما تحفظ تواريخ العمل المدرسي كتواريخ محلية وتحسب الحصة الجارية من UTC الحالي بعد تحويله إلى منطقة المدرسة، مع دعم تغييرات التوقيت الصيفي من قاعدة بيانات المناطق الزمنية.
- تضيف سجلات التشغيل الحساسة مثل الحضور snapshot للمنطقة الزمنية المستخدمة، وتؤثر تغييرات المنطقة على السجلات المستقبلية فقط. المدارس الحالية يجب أن تكمل الإعداد قبل تفعيل وظائف تعتمد على الوقت.

## 2026-09-26 — اعتماد مساحة تشغيل الفصل وتجربة تسجيل الحضور

- النوع: تعديل تصميم محفوظ للتنفيذ لاحقًا، دون تغيير كود.
- القرار: يضاف Action «فتح الفصل» إلى شاشة الفصول، وينقل المستخدم إلى مساحة عمل للفصل تحتوي تبويبات الحضور والواجبات والأنشطة والاختبارات والملاحظات وتقييم الحصة، مع إظهار التبويبات حسب الصلاحيات.
- في `Daily` يعرض تبويب الحضور طلاب الفصل محددين حاضرين افتراضيًا؛ يزيل المسجل التحديد عن الغائبين ثم يحفظ سجلًا واحدًا للفصل والتاريخ. بعد الحفظ يصبح السجل للقراءة فقط، مع تصحيح لاحق عبر صلاحية وإعادة فتح مدققة.
- في `PerSession` يعمل التدفق نفسه للحصة الجارية حسب توقيت السيرفر والجدول، ولا يقبل الفسح. يستطيع مدرس الحصة أو البديل الفعلي التسجيل داخل نافذة زمنية مضبوطة، وتظهر الحصص المسجلة للقراءة فقط.

## 2026-09-26 — تصور حضور الطلاب اليومي أو حسب الحصة

- النوع: نقاش وتصميم محفوظ للتنفيذ لاحقًا، دون تغيير كود.
- الاتجاه: محرك حضور موحد بسياسة `Daily` أو `PerSession`، مع إعداد افتراضي للمدرسة وإمكانية override لنوع التعليم ثم المرحلة؛ الأخص هو الذي يسري.
- سجل الحضور يكون Header للفصل والتاريخ ووحدة الحضور (`DAY` أو حصة جدول محددة) وتحته سجل لكل قيد طالب، مع منع التكرار وقفل السجل بعد الاعتماد وتسجيل من أنشأ وعدّل واعتمد.
- في نمط الحصص يُشتق ملخص اليوم من سجلات الحصص ولا يُنشأ سجل يومي مكرر. الحصة تستبعد الفسح، ويرث المدرس البديل المؤرخ حق التسجيل للحصة التي يغطيها.
- الحضور التاريخي يثبت قائمة قيود الطلاب وقت اعتماد السجل، ولا يعاد احتسابه من القائمة الحالية بعد نقل طالب أو انسحابه.

## 2026-09-26 — شاشة الفصول التشغيلية حسب المستخدم

- النوع: ميزة تشغيلية في Schools API وSchools Web وSchools Flutter.
- أضيفت شاشة «الفصول» إلى القائمة الجانبية؛ تعرض بيانات الفصل الأكاديمية والفترة والسعة وعدد الطلاب المقيدين والغرفة الأساسية والمواد المرتبطة.
- يحسب الـBackend النطاق من هوية المستخدم: `School Admin` يرى جميع الفصول النشطة، والمدرس يرى فقط الفصول التي اختير فيها ضمن مواد الفصل، وغير المرتبط لا يستقبل فصولًا. لا تعتمد الحماية على فلترة الواجهة.
- تعرض مواد المدرس المرتبطة فقط في نطاقه، بينما يرى المدير كل مواد الفصل. لا توجد migration جديدة لأن التنفيذ يستخدم إسنادات الفصول والمواد الحالية.
- التحقق: Schools API Release build ناجح بلا warnings أو errors واختبارات Schools `9/9`؛ Schools Web production build وlint ناجحان؛ Schools Flutter analyze بلا مشاكل واختباراته `4/4` ناجحة.

## 2026-09-25 — معالجة إلغاء الطلب أثناء فحص Platform JWT

- النوع: إصلاح Backend في Platform API.
- السبب: كان `OnTokenValidated` يمرر `RequestAborted` إلى فحص الحساب داخل PostgreSQL دون معالجة الإلغاء؛ فإذا ألغى المتصفح الطلب أو توقف التطبيق أثناء فتح اتصال بارد، تخرج `TaskCanceledException` من Npgsql ويسجلها JwtBearer كاستثناء مصادقة.
- الإصلاح: التقاط `OperationCanceledException` فقط عندما يكون طلب HTTP نفسه قد أُلغي، وإنهاء المصادقة بـ`NoResult` دون إخفاء أخطاء الاتصال أو قاعدة البيانات الحقيقية.
- التحقق: PostgreSQL service يعمل و`/health/ready` أعاد `200` خمس مرات؛ Platform API Release build ناجح (مع warning سابق غير مرتبط)، واختبارات Platform `176/176` ناجحة.

## 2026-09-25 — إظهار انتظار تسجيل الدخول في Schools

- النوع: إصلاح تجربة استخدام في Schools Web وSchools Flutter.
- يعرض زر تسجيل الدخول بعد الضغط مؤشر تحميل ونص «جارٍ تسجيل الدخول...»، ويتعطل حتى اكتمال الطلب لمنع الضغط المتكرر؛ وتعرض إجراءات التفعيل «جارٍ المعالجة...» بالحالة نفسها.
- أضيفت النصوص بالعربية والإنجليزية، مع `aria-busy` في Web لدعم الوصول.
- التحقق: Schools Web production build وlint ناجحان؛ Schools Flutter analyze بلا مشاكل واختباراته `4/4` ناجحة.

## 2026-09-25 — تذكرني وتجديد جلسات Platform وSchools

- النوع: ميزة مصادقة وأمان في Platform API وSchools API وWeb وFlutter.
- أُضيف `rememberMe` لتسجيل الدخول، وPlatform refresh/logout endpoints وجلسات refresh دوّارة باستخدام `identity.sessions` الحالية؛ أصبحت جلسات Schools تحترم نفس مدة اليوم/30 يومًا وتحافظ على تاريخ الانتهاء عند التدوير.
- يستعيد Web وFlutter الجلسة عند التشغيل ويجدد access token قبل انتهائه بدقيقة. Web يفرق بين `sessionStorage` و`localStorage`، وFlutter يستخدم `flutter_secure_storage` للجلسة الدائمة فقط، ولا يحفظ أي عميل كلمة المرور.
- أُضيف تسجيل الخروج المبطل للجلسة في العملاء، وتجديد عند استئناف تطبيقات Flutter، مع نصوص عربية وإنجليزية وخيار مفعّل افتراضيًا.
- لا توجد migration جديدة؛ استُخدمت جداول الجلسات الموجودة.
- التحقق: Platform API وSchools API build ناجحان؛ Platform tests `176/176` وSchools tests `9/9`؛ Platform Web build و`32/32` اختبارًا؛ Schools Web build/lint؛ Platform Flutter analyze و`28/28` اختبارًا؛ Schools Flutter analyze و`4/4` اختبارات.

## 2026-09-25 — عرض التفاصيل الكاملة للطالب

- النوع: ميزة في Schools API وSchools Web وSchools Flutter.
- أضيف Action «عرض التفاصيل» إلى كل طالب، ونافذة قراءة فقط تعرض البيانات العربية والإنجليزية والكود والمعرف العالمي والميلاد والنوع والأرقام الرسمية والحالة.
- تعرض النافذة جميع أولياء الأمور مع الأساسي وصلة القرابة والهاتف والبريد والرقم القومي وصلاحيات الاستلام والمسؤولية المالية والطوارئ، كما تعرض سجل القيود بالعام ونوع التعليم والمرحلة والصف والفصل وتاريخ القيد والحالة.
- تم توسيع استجابة تفاصيل الطالب بالرقم القومي لولي الأمر ونوع التعليم والمرحلة داخل القيد، دون أي تغيير في قاعدة البيانات.
- التحقق: Schools API build ناجح بلا warnings أو errors وSchools tests `9/9`، وSchools Web lint/build ناجحان، وSchools Flutter analyze بلا مشاكل واختباراته `4/4` ناجحة.

## 2026-09-25 — إصلاح ظهور نص زر حفظ الطالب

- النوع: إصلاح واجهة في Schools Web.
- السبب: قاعدة أزرار نافذة الطالب كانت تتغلب على خلفية `.settings-primary` وتجعل زر الحفظ أبيض مع نص أبيض.
- الإصلاح: تثبيت لون وخلفية وحد زر الحفظ الأساسي داخل نافذة الطالب بأولوية CSS صحيحة.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-25 — إصلاح إغلاق لوحة فلاتر الطلاب

- النوع: إصلاح واجهة في Schools Web.
- السبب: قاعدة `display: grid` الخاصة بنموذج الفلاتر كانت تتغلب على خاصية `hidden`، ولذلك ظل المحتوى ظاهرًا بعد الضغط على شريط اللوحة.
- الإصلاح: إضافة قاعدة صريحة لـ`.student-filters[hidden]` لإخفاء المحتوى مع إبقاء حالة الفتح والسهم كما هي.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-25 — توحيد لوحة فلاتر الطلاب مع Platform Web

- النوع: تحسين واجهة في Schools Web.
- تحولت عوامل تصفية تبويبي الطلاب وطلبات التقديم إلى لوحة collapsible مغلقة افتراضيًا بنفس نمط Platform: شريط كامل بعنون «عوامل التصفية» وسهم متحرك، وتظهر الحقول وأزرار التطبيق والمسح داخله عند الفتح.
- انتقل زر إضافة الطالب أو الطلب إلى شريط الأدوات بجانب لوحة التصفية، مع التفاف responsive يجعل اللوحة بعرض كامل على الشاشات الأصغر.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-25 — تنفيذ Student Core وطلبات التقديم والقيد

- النوع: ميزة كاملة + قرار معماري + migrations + ربط Web وFlutter.
- أضيف سجل الطالب العالمي داخل Platform مع `GlobalStudentId` وكود `STU-*` عشوائي فريد ومفهرس، وinternal API محمية لحل الهوية بالكود أو الرقم القومي أو شهادة الميلاد أو إنشائها. لا تحفظ Platform عضويات المدارس أو بيانات القيد.
- أضيفت داخل كل قاعدة مدرسة كيانات الطالب وولي الأمر وربط عدة أولياء مع أساسي واحد والقيد وطلب التقديم وأولياء الطلب، مع الفهارس والقيود وmigrations لـSQL Server وPostgreSQL.
- أضيفت صلاحيات مستقلة للطلاب والقبول، وAPIs للقوائم المصفاة والصفحات، والتسجيل المباشر، وإنشاء طلب من مقر المدرسة، وتغيير حالته، وقبوله وتحويله إلى طالب وقيد داخل فصل مع فحص السعة.
- أضيفت شاشة الطلاب في Schools Web وSchools Flutter بتبويبي «طلاب المدرسة» و«الطلبات المقدمة»، والفلاتر الأكاديمية والاسم والكود وتاريخ الميلاد، وفلاتر الحالة/المصدر/تاريخ التقديم، ونماذج أولياء الأمور المتعددين، وقبول الطلب باختيار الفصل. لا يظهر فلتر الفصل للطلبات قبل القبول.
- يرتبط كل `Student` محليًا بـ`Person` واحد إلزامي وفريد، تمهيدًا لإضافة حساب الطالب لاحقًا دون ازدواج هوية الشخص داخل المدرسة.
- migrations: Platform SQL Server `20260925191826_AddGlobalStudentRegistry`، Platform PostgreSQL `20260925191837_AddGlobalStudentRegistry`، Schools PostgreSQL `20260925193625_AddStudentCoreAdmissions`، Schools SQL Server `20260925193637_AddStudentCoreAdmissions`.
- التحقق: solution build ناجح بلا warnings أو errors، وجميع اختبارات Backend ناجحة `230/230`، ولا توجد pending model changes للسياقات الأربعة؛ Schools Web lint وproduction build ناجحان؛ Schools Flutter analyze بلا مشاكل واختباراته `4/4` ناجحة.

## 2026-09-25 — تصحيح نطاق فلاتر طلبات التقديم

- النوع: تصحيح قرار تصميمي، دون تغيير كود.
- القرار: فلتر الفصل الدراسي يخص تبويب «طلاب المدرسة» فقط لأنه يعتمد على `StudentEnrollment` الفعلي، ولا يظهر في «الطلبات المقدمة» قبل القبول والقيد.
- فلاتر الطلبات المشتركة مع الطلاب هي العام المستهدف ونوع التعليم والمرحلة والصف والاسم والكود العالمي الاختياري وتاريخ الميلاد؛ وتضيف الطلبات الحالة ونطاق تاريخ التقديم ومصدر الطلب مثل `SchoolDesk` أو `FamilyApp`.

## 2026-09-25 — تثبيت كود الطالب العالمي وفلاتر شاشة الطلاب

- النوع: استكمال تصميم Student Core وAdmissions، دون تنفيذ كود بعد.
- القرار: يولد الـPlatform كود طالب عالميًا فريدًا ويحفظه مع `GlobalStudentId` في قاعدة Platform، ثم تحفظ كل مدرسة `GlobalStudentId` والكود نفسيهما داخل ملف الطالب المحلي كنسخة مفهرسة للعرض والبحث؛ لا تولد المدرسة الكود ولا تستخدمه لمنح صلاحية.
- الفلاتر المشتركة لتبويبي «طلاب المدرسة» و«الطلبات المقدمة»: المرحلة والصف والفصل واسم الطالب والكود وتاريخ الميلاد. ويضاف للطلبات حالة الطلب ونطاق تاريخ التقديم.
- معنى الفصل في الطلاب هو الفصل الحالي من القيد الفعال، أما في الطلب فهو فصل مطلوب/مقترح اختياري ولا يصبح قيدًا فعليًا قبل القبول. يمكن لطلب طالب معروف مسبقًا حمل الكود العالمي، بينما يظل الكود فارغًا للمتقدم الجديد حتى القبول.
- جميع الفلاتر Server-side وتدخل في الإجمالي والـpagination، مع قوائم صف وفصل تابعة للاختيار الأعلى وفهارس قاعدة بيانات مناسبة.

## 2026-09-25 — تثبيت مساري طلب التقديم والتسجيل المباشر للطالب

- النوع: نقاش وتصميم محفوظ للتنفيذ في Student Core وAdmissions، دون تغيير كود بعد.
- القرار: شاشة الطلاب تبدأ بتبويبين رئيسيين: «طلاب المدرسة» للملفات المقيدة فعليًا، و«الطلبات المقدمة» لطلبات الالتحاق قبل التحول إلى طالب.
- تدعم المدرسة إدخال طلب يدويًا نيابة عن ولي أمر حضر إلى مقرها، مع حفظ مصدر الطلب `SchoolDesk` تمهيدًا لاستقبال المصدر `FamilyApp` لاحقًا عبر الكيان والمسار نفسيهما.
- تدعم المدرسة أيضًا تسجيل طالب مقيد حاليًا مباشرة وإنشاء ملفه وقيده الدراسي دون إجباره على المرور بدورة طلب القبول؛ ويظل قبول طلب التقديم مسارًا آخر ينشئ الطالب والقيد ويربطهما بالطلب الأصلي.

## 2026-09-25 — تثبيت تصور خطط التدريس متعددة المستويات

- النوع: نقاش وتصميم محفوظ للتنفيذ لاحقًا، دون تغيير كود.
- الاتجاه: خطط التدريس تكون `Yearly`, `Term`, `Monthly`, `Weekly`, `Daily`، وكلها في النهاية ضمن نطاق عام دراسي وفصل دراسي ومادة محددة، مع تسلسل اختياري من الخطة السنوية حتى اليومية.
- الفصل المفاهيمي: `CurriculumPlan` يظل المنهج الرسمي، وكيان مستقل باسم `TeachingPlan` أو `InstructionPlan` يمثل توزيع وتحضير المدرس، بينما يسجل `LessonDelivery` ما نُفذ فعليًا.
- الخطة اليومية يمكن ربطها بتاريخ وحصة فعلية من الجدول، مع مراجع ومرفقات وأنشطة وواجبات، وإتاحة القراءة للبديل وتسجيل التنفيذ دون تغيير أصل الخطة.

## 2026-09-25 — استكمال تغطية الحصص وإشعارات البديل في Web وFlutter

- أضيفت إشعارات مدرسية داخلية دائمة للمستخدم، مع قائمة آخر 50 إشعارًا وعدّاد غير المقروء وتعليم إشعار واحد أو الجميع كمقروء. ينشأ إشعار للمدرس البديل عند إسناد تغطية حصة، وإشعار إلغاء عند تغيير التكليف أو إلغائه أو حذف سجل الغياب المرتبط.
- أصبح جدول المدرس يعرض، بجانب حصصه الأساسية، التغطيات الفعلية المؤرخة التي أُسندت إليه مع المادة والفصل وموعد الحصة واسم المدرس الغائب؛ ويمكن للمدرس قراءة جدوله الشخصي دون منحه صلاحية عرض جميع المستخدمين.
- نُفذت شاشتا المعلمين والموظفين التشغيليتان في Schools Flutter بالفلاتر وجدول المدرس وتسجيل الغياب/الإذن/الإجازة وتغطية حصص الغياب، ونُفذت شاشة الإشعارات والعداد وحالات القراءة. نُفذت التجربة نفسها في Schools Web مع قائمة إشعارات في شريط التطبيق وصفحة كاملة.
- migrations: PostgreSQL `20260925154727_AddSchoolUserNotifications`، SQL Server `20260925154756_AddSchoolUserNotifications`.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ لا توجد pending model changes للمزودين؛ Schools Web lint وproduction build ناجحان؛ Schools Flutter analyze ناجح و4 اختبارات ناجحة.

## 2026-09-24 — إصلاح تسجيل غياب محدد بالساعات

- السبب: حقلا وقت الغياب كانا يستقبلان `TimeOnly?` بالمحول الافتراضي، بينما حقل HTML يرسل صيغة `HH:mm`؛ فكان Model Binding يرفض الطلب قبل الوصول إلى منطق التسجيل.
- الإصلاح: استخدام `FlexibleNullableTimeOnlyJsonConverter` المعتمد بالفعل في إعدادات المواعيد والغرف لقبول الوقت المرن، مع تنسيق عرض الوقت إلى الساعات والدقائق في نافذة الغياب والتغطية.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان.

## 2026-09-24 — إصلاح ظهور خطأ بعد نجاح تسجيل الغياب

- السبب: معالج إرسال نموذج الغياب كان يرجع إلى `event.currentTarget` بعد انتهاء طلب `await`؛ في هذه اللحظة يفقد React مرجع `currentTarget`، فينجح الحفظ في API ثم يرمي الـWeb خطأ قبل تصفير النموذج وإعادة تحميل السجلات.
- الإصلاح: حفظ مرجع عنصر النموذج قبل أول `await` واستخدامه بعد نجاح الطلب، ثم إعادة تحميل قائمة الغيابات فورًا.
- لا توجد migration مطلوبة.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-24 — توضيح تعارض سجلات الغياب في الواجهة

- السبب: كانت Schools API تعيد الكود الصحيح `people.absence_overlap` وحالة 409 عند محاولة تسجيل فترة متداخلة، لكن نافذة الغياب كانت تعرض رسالة عامة ولا تفرق بين سبب التعارض وباقي أخطاء التحقق.
- الإصلاح: ربط أكواد أخطاء الغياب وتغطية الحصص برسائل عربية وإنجليزية حسب لغة المستخدم، بما يشمل الفترة غير الصحيحة والتداخل وغياب البديل وتعارض جدوله.
- لا توجد migration مطلوبة.
- التحقق: Schools Web lint وproduction build ناجحان.

## 2026-09-24 — تنفيذ الغياب الأولي وتغطية حصص المعلم

- النطاق: Schools API وقاعدة المدرسة وSchools Web، داخل شاشتي المعلمين والموظفين التشغيليتين مع بقاء إعداد الحساب داخل الإعدادات.
- النتيجة: إضافة `StaffAbsence` لتسجيل الغياب والإذن والإجازات يدويًا بفترة تاريخية ووقت اختياري وملاحظات، مع منع السجلات المتداخلة وتطبيق صلاحيات الأشخاص ونطاق الأقسام.
- التغطية: يعرض سجل غياب المعلم حصصه الفعلية المتأثرة ويتيح إسناد بديل من مرشحي الحصة؛ تحفظ التغطية في `TeacherSubstitution` بتاريخ محدد دون تعديل الجدول الأسبوعي، مع فحص تعارض البديل وعدم غيابه وإلغاء التغطيات عند حذف الغياب.
- الاستدامة: إضافة `SourceType` و`SourceReferenceId` للغياب والتغطية ليصبح HR لاحقًا مصدر الحقيقة دون إعادة تصميم الجزء الأكاديمي.
- migrations: PostgreSQL `20260924155809_AddStaffAbsencesAndCoverageSources`، SQL Server `20260924155823_AddStaffAbsencesAndCoverageSources`.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ الحل الكامل build ناجح مع warnings قديمة وبدون errors؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان؛ ولا توجد pending model changes للمزودين.

## 2026-09-24 — إصلاح خطأ 500 عند تحميل شاشتي المعلمين والموظفين

- السبب: EF Core لم يستطع ترجمة `Distinct + OrderBy` بعد إنشاء response record يحتوي `Department.Type.ToString()` داخل استعلام خيارات الأقسام، فأعاد endpoint خطأ 500 وأفشل تحميل الشاشة.
- الإصلاح: إرجاع الحقول الخام القابلة للترجمة من SQL أولًا، ثم إزالة التكرار والترتيب بعد الجلب، وإخراج نوع القسم enum مباشرة عبر `JsonStringEnumConverter` بدل `ToString()` داخل LINQ.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ Schools Web production build ناجح؛ وsmoke test مباشر على API جديد أكد نجاح endpoint خيارات الأقسام وقائمتي Teacher وEmployee.
- لا توجد migration مطلوبة.

## 2026-09-24 — تصحيح الفصل بين إعدادات الأفراد وشاشاتهم التشغيلية

- التصحيح: إعادة تبويبي المعلمين والموظفين إلى الإعدادات للإضافة والتعديل والحالة والحذف وإسناد مواد المعلم.
- التشغيل: الشاشتان خارج الإعدادات مستقلتان ولا تعرضان عمليات إدارة الحساب؛ وتعرض شاشة المعلمين التفاصيل والجدول الأسبوعي حاليًا، مع تجهيزها للحضور وتغطية الحصص لاحقًا.
- التصفية: إضافة فلاتر Server-side مستقلة للقسم والاسم ورقم الهاتف، مع endpoint يعيد الأقسام المتاحة فقط ضمن نطاق المستخدم.
- الحماية: استمر تطبيق نطاق القسم في Schools API على القائمة والتفاصيل والصورة وجدول المعلم وعمليات الإدارة.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان؛ ولا توجد pending model changes للمزودين.

## 2026-09-24 — تنفيذ شاشتي المعلمين والموظفين بنطاق الأقسام

- النوع: Schools API + Schools Web + قرار نطاق بيانات.
- النتيجة: نقل «المعلمين» و«الموظفين» إلى شاشتين مستقلتين في القائمة الرئيسية وإزالة تبويبيهما من الإعدادات، مع إظهار الأقسام الحالية في كل قائمة.
- الحماية: أصبح `School Admin` يرى المدرسة كلها، وغيره يرى نفسه وأعضاء الأقسام التي يقودها وفروعها، مع تطبيق النطاق على القائمة والصورة والتفاصيل وعمليات التعديل/الحالة/الحذف ونطاق المواد وجدول المعلم.
- القاعدة: الصلاحيات RBAC تظل شرطًا مستقلًا؛ العضوية أو الرئاسة لا تمنح صلاحية تقنية بمفردها.
- لا توجد migration مطلوبة؛ أُضيفت navigation فقط للعلاقة الموجودة.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان؛ و`dotnet ef migrations has-pending-model-changes` يؤكد عدم وجود تغييرات معلقة للمزودين.

## 2026-09-24 — تصور شاشتي المعلمين والموظفين التشغيليتين ونطاق الأقسام

- النوع: نقاش معماري قيد الاعتماد، بدون تنفيذ.
- الاتجاه: تبقى إدارة الحسابات والأدوار داخل الإعدادات، بينما تنشأ خارجها شاشتان رئيسيتان مستقلتان: «المعلمون» للأعمال الأكاديمية والجداول والتغطية، و«الموظفون» للأعمال الإدارية والتشغيلية. تشترك الشاشتان في منطق نطاق الأقسام فقط ولا تدمجان في دليل واحد.
- الوصول: الصلاحية تحدد العملية المسموحة، ونطاق الإسناد يحدد الأقسام التي تظهر بياناتها. عضوية القسم أو رئاسته لا تمنح صلاحية تقنية تلقائيًا، و`School Admin` يرى جميع الأقسام.
- التكامل المستقبلي: سجلات الإذن والغياب والإجازة يملكها النظام الإداري، بينما الأكاديمي يملك حالة تغطية الحصص فقط ويرتبط بالسجل الإداري من خلال مصدر ومعرف مرجعي دون FK مباشر بين الوحدتين.

## 2026-09-24 — حجوزات الغرف المشتركة والدمج المؤقت للفصول

- النوع: ميزة أكاديمية + قرار تشغيلي + migrations.
- النطاق: Schools API وقاعدة المدرسة وSchools Web.
- النتيجة: إضافة غرفة اختيارية لكل حصة، وخيار حجز مشترك يسمح باستخدام المعمل لأكثر من فصل مع فحص موافقة كل الحجوزات والسعة، وشاشة جدول أسبوعي للغرفة تعرض الفصل والمادة والمدرس.
- الإصلاح: الغرفة الأساسية المسندة للفصل أصبحت تُورث تلقائيًا لكل حصصه وتظهر في جدول الغرفة حتى للحصص القديمة التي لا تحتوي `RoomId`؛ واختيار غرفة مختلفة داخل محرر الحصة أصبح override للمعمل أو النقل بدل أن يكون حجزًا مطلوبًا لكل حصة.
- النتيجة: إضافة دمج مؤقت مؤرخ لفصلين أو أكثر في غرفة ووقت محددين، مع الحضور المتوقع والملاحظات وفحص تعارض الغرفة والفصول وsoft delete، بدون تعديل تعريف الفصول أو الجدول الأسبوعي.
- migrations: PostgreSQL `20260924114735_AddRoomTimetableAndTemporaryMerges`، SQL Server `20260924114803_AddRoomTimetableAndTemporaryMerges`.
- التحقق: Schools API build ناجح بدون warnings أو errors؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان؛ ولا توجد pending model changes للمزودين.

## 2026-09-24 — عرض الجدول الأسبوعي للمدرس

- النوع: Schools API + Schools Web.
- النتيجة: إضافة endpoint يجمع حصص المدرس الأساسية من كل الفصول، وAction «عرض الجدول» في قائمة المدرسين يفتح مصفوفة أسبوعية تعرض المادة والفصل والصف ونوع التعليم والعام ووقت الحصة.
- يستخدم العرض فترات الوقت الفعلية كأعمدة بدل رقم الخانة، لدعم اختلاف مواعيد المراحل وأنواع التعليم داخل جدول مدرس واحد.
- البدلاء المسجلون كمرشحين في قالب الحصة لا يظهرون في جدول المدرس كحصص فعلية؛ سيظهر الإسناد البديل عند تنفيذ التشغيل اليومي لـ`TeacherSubstitution`.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بلا warnings أو errors، و9 اختبارات Schools ناجحة، وSchools Web lint وproduction build ناجحان.

## 2026-09-24 — إصلاح إنشاء قسم رئيسي بلا أب

- النوع: Bug fix في Schools API.
- السبب: فحص منع ربط القسم بنفسه كان يقارن `ParentDepartmentId` بمعرف القسم حتى أثناء الإنشاء؛ ولأن القيمتين `null` اعتبر الطلب الصحيح لقسم رئيسي self-parent وأعاد `departments.invalid`.
- النتيجة: أصبح فحص self-parent يعمل عند التعديل فقط عندما يوجد معرف للقسم، ويمكن إنشاء قسم رئيسي بـ`ParentDepartmentId = null` مع استمرار منع الدورات في الشجرة.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بلا warnings أو errors، و9 اختبارات Schools ناجحة.

## 2026-09-24 — تنفيذ الهيكل التنظيمي الموحد للأقسام

- النوع: Backend + EF Core + Schools Web + Schools Flutter.
- النتيجة: إضافة أقسام أكاديمية وإدارية موحدة، شجرة أقسام ونطاق فرع اختياري، عضويات ورئاسة مؤرختين، قسم أساسي واحد لكل مستخدم، وربط الأقسام الأكاديمية بالمواد ومنسقي المواد.
- القيود: الرئيس عضو بالقسم، ومنسق المادة مدرس وعضو فيه، ولا تقبل الأقسام الإدارية مواد أو منسقين. نوع القسم يوجه المرشحين لكنه لا يمنع استثناءات العضوية، ولا تمنح العضوية أو الرئاسة صلاحيات RBAC تلقائيًا.
- الواجهات: تبويب كامل في Schools Web وشاشة في Schools Flutter للتعريف والتعديل والحالة وsoft delete والاستعادة وإدارة الأعضاء والرئيس والمواد والمنسقين، مع رسائل خطأ عربية/إنجليزية حسب اللغة الفعالة.
- الصلاحيات: `school.departments.view`, `school.departments.manage`, `school.departments.delete`, `school.departments.restore`، وممنوحة لدور `School Admin` من seed البيانات.
- migrations: PostgreSQL `20260924100046_AddSchoolOrganizationDepartments` وSQL Server `20260924100121_AddSchoolOrganizationDepartments`.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان؛ Schools Flutter analyze ناجح و4 اختبارات ناجحة؛ و`dotnet ef migrations has-pending-model-changes` يؤكد عدم وجود تغييرات معلقة للمزودين.

## 2026-09-24 — تصور موحد للأقسام التعليمية والإدارية

- النوع: نقاش وتصميم تنظيمي، لم يُنفذ بعد.
- التصور: كيان أقسام موحد للمدرسة بنوع `Academic` أو `Administrative`، مع شجرة اختيارية للأقسام الفرعية ونطاق على مستوى المدرسة أو فرع محدد.
- القسم التعليمي يمكن ربطه بالمواد والنطاق الأكاديمي، ثم إسناد رئيس قسم ومنسقي مواد مؤرخين. القسم الإداري يمثل وحدات مثل الموارد البشرية والمالية وشؤون الطلاب والتشغيل.
- عضوية القسم علاقة مستقلة ومؤرخة بالمستخدم، مع قسم أساسي واحد وإمكان عضويات ثانوية؛ نوع القسم يحدد السلوك والمرشحين الافتراضيين في الواجهة لكنه لا يُستخدم كقيد جامد يمنع الاستثناءات الصحيحة، مثل موظف مختبر داخل قسم العلوم أو مدرس مكلف بمسؤولية إدارية.
- الهيكل التنظيمي لا يستبدل RBAC: رئاسة القسم أو عضويته لا تمنح صلاحيات تقنية تلقائيًا إلا من خلال سياسة صلاحيات نطاقية صريحة.
- التحقق: مناقشة تصميمية فقط؛ لا توجد تغييرات كود أو migrations.

## 2026-09-24 — تصحيح ترتيب أيام جدول الحصص

- النوع: Bug fix في Schools API وSchools Web وSchools Flutter.
- السبب: ترتيب `DayOfWeek` داخل استعلام قاعدة البيانات كان يعتمد على تمثيل المزود وقد يظهر أبجديًا بدل ترتيب الأسبوع المدرسي.
- النتيجة: أصبح الترتيب ثابتًا: السبت ← الأحد ← الاثنين ← الثلاثاء ← الأربعاء ← الخميس ← الجمعة. تعرض الـAPI الأيام النشطة المعرفة لنوع التعليم بهذا الترتيب، وتطبق الواجهتان الترتيب نفسه دفاعيًا.
- لا توجد migration مطلوبة لأن التغيير في ترتيب الاستجابة والعرض فقط.

## 2026-09-23 — تنفيذ مصفوفة جدول الحصص والفسح والبدلاء المتعددين

- النوع: Backend + EF Core + Schools Web + Schools Flutter.
- أضيف إلى المرحلة عدد الحصص اليومية وعدد الفسح، مع منع تقليل الحدود إذا كانت هناك خانات قائمة ستخرج عن المدى الجديد.
- أعيد تصميم `WeeklyTimetableSlot` ليرتبط مباشرة بالفصل ويحفظ رقم الخانة ونوعها، وأضيف `WeeklyTimetableSlotSubstituteTeacher` لدعم صفر أو عدة بدلاء لكل حصة.
- أضيفت endpoints قراءة أقسام الجدول وقراءة مصفوفة الفصل وحفظ/حذف خلية واحدة، مع التحقق من أيام الدراسة والوقت وتعارض الفصل والأساسي والحد الأسبوعي للمادة والحد اليومي للفسح. البدلاء لا يخضعون لفحص التعارض، ويمنع فقط تساويهم مع الأساسي أو تكرارهم.
- أعيدت واجهة Schools Web كمستويات نوع تعليم ← مرحلة ← صف/عام ← فصل، ثم نافذة جدول أيام × خانات ومحرر حصة/فسحة. أضيفت التجربة نفسها إلى تبويب جدول الحصص داخل الإعداد الأكاديمي في Schools Flutter.
- migrations: PostgreSQL `20260923203813_RedesignWeeklyTimetableMatrix` وSQL Server `20260923203828_RedesignWeeklyTimetableMatrix`، وتتضمن backfill للفصل ورقم الخانة والأساسي والبدلاء من التصميم السابق.
- التحقق: `dotnet build` ناجح بلا تحذيرات، `dotnet test tests/Mdaresna.Schools.UnitTests` ناجح 9/9، و`dotnet ef migrations has-pending-model-changes` يؤكد عدم وجود تغييرات معلقة للمزودين، وSchools Web `npm run lint` و`npm run build` ناجحان، وSchools Flutter `flutter analyze` و`flutter test` ناجحان 4/4.

## 2026-09-23 — تثبيت قاعدة التحقق من بدلاء الحصة

- النوع: تثبيت قاعدة عمل لتصميم جدول الحصص، لم تُنفذ بعد.
- المدرسون البدلاء مرشحون احتياطيون فقط، لذلك لا يخضعون لفحص تعارضات الجدول عند حفظ الحصة ولا يُعاملون كإسناد تدريس فعلي.
- التحقق الوحيد في إعداد الخلية هو منع اختيار المدرس الأساسي نفسه ضمن البدلاء، مع منع تكرار المدرس في قائمة البدلاء.
- تعارض المدرس البديل يُفحص لاحقًا فقط عند إسناده فعليًا بدل المدرس الأساسي في تاريخ وحصة محددين ضمن التشغيل اليومي.
- التحقق: مناقشة تصميمية فقط؛ لا توجد تغييرات كود أو migrations.

## 2026-09-23 — دعم صفر أو عدة مدرسين بدلاء لكل حصة

- النوع: توضيح تصميم أكاديمي، لم يُنفذ بعد.
- القرار المطلوب: خلية الحصة لها مدرس أساسي واحد، أما المدرسون البدلاء فاختيارهم غير إجباري ويمكن أن يكون العدد صفرًا أو أكثر من مدرس.
- الواجهة: بعد اختيار المادة والمدرس الأساسي تعرض النافذة multi-select اختياريًا للبدلاء من مدرسي المادة داخل الفصل، مع استبعاد الأساسي ومنع التكرار.
- أثر نموذج البيانات: الحقل المفرد الحالي للبديل لا يكفي. يحتاج التصميم الجديد علاقة متعددة مرتبطة بخانة الجدول، مثل `WeeklyTimetableSlotSubstituteTeacher`، بينما تحتفظ الخانة بالمدرس الأساسي.
- الفسحة لا تقبل مدرسًا أساسيًا أو بدلاء، وحذف مدرس من فريق الفصل يُمنع إذا كان مستخدمًا كأساسي أو بديل في خلية قائمة.
- التحقق: مناقشة تصميمية فقط؛ لا توجد تغييرات كود أو migrations.

## 2026-09-23 — تصحيح تصميم خانات جدول الحصص والفسح

- النوع: توضيح تصميم أكاديمي، لم يُنفذ بعد، وينسخ افتراض ترتيب الفسح المسبق في النقاش السابق.
- إعداد المرحلة يحفظ عدد الحصص اليومية وعدد الفسح فقط لكل نوع تعليم ومرحلة وعام برنامج؛ عدد أعمدة جدول الفصل يساوي مجموع الرقمين.
- المستخدم يحدد داخل كل خلية هل هي فسحة، ولا تُحفظ مواضع الفسح في قالب المرحلة. عند اكتمال صف يوم دراسي يجب ألا يتجاوز عدد الفسح العدد المحدد، ويجب أن تتطابق أعداد الحصص والفسح عند اعتماد الجدول.
- للفصل Action واحد «فتح الجدول». النافذة تعرض الأيام كصفوف والخانات المرقمة كأعمدة. الخلية الفارغة تعرض زر قلم، والمعبأة تعرض تفاصيلها وتكشف التعديل والحذف عند hover.
- محرر الخلية يبدأ بـ`IsBreak`. إذا كانت فسحة تُخفى المادة والمدرسون ويظل الوقت مطلوبًا، وإذا كانت حصة يختار المستخدم المادة والمدرس الأساسي والبديل ووقت البداية والنهاية.
- مدرسو الخلية يُقصرون على `ClassSectionTeacherScope` للمادة المختارة، مع منع تساوي الأساسي والبديل واستمرار فحص تعارض الفصل والمدرس والحد الأسبوعي للمادة.
- التحقق: مناقشة تصميمية فقط؛ لا توجد تغييرات كود أو migrations.

## 2026-09-23 — تصور إعادة تصميم جدول الحصص كمصفوفة يومية

- النوع: نقاش وتصميم أكاديمي، لم يُنفذ بعد.
- التصور: تعريف قالب يوم دراسي لكل نوع تعليم ومرحلة داخل عام البرنامج، يتكون من خانات مرتبة من نوع حصة أو فسحة بدل الاكتفاء بعدد الحصص والفسح فقط؛ مثال KG: ست حصص وفسحتان ينتجان ثماني خانات مرتبة.
- تجربة العرض: تبويب لكل نوع تعليم، وتحته لوحات للمراحل، ثم الصفوف والفصول، وعلى كل فصل Action لإضافة أو تعديل جدول أسبوعي في نافذة مصفوفية؛ الصفوف هي أيام الدراسة والأعمدة هي الحصص والفسح بالترتيب.
- خانات الفسح غير قابلة لإسناد مادة، وخانة الحصة تختار مادة الفصل ومدرسها المؤهل، مع استمرار قواعد الحد الأسبوعي ومنع تعارض الفصل والمدرس.
- حماية التاريخ: الإعداد التشغيلي يجب أن يُنسخ أو يُربط بعام البرنامج حتى لا يغير تعديل قالب سنة جديدة جداول السنوات السابقة.
- التحقق: مناقشة تصميمية فقط؛ لا توجد تغييرات كود أو migrations.

## 2026-09-23 — تحويل أقسام الإعداد الأكاديمي إلى Accordion

- النوع: تحسين تجربة Schools Web وأداء تحميل البيانات.
- النتيجة: وضع أقسام الهيكلة الأكاديمية والصفوف الدراسية والمواد والمناهج وجدول الحصص داخل لوحات collapse متجاوبة، مع لوحة واحدة مفتوحة فقط وفتح أي لوحة يغلق السابقة تلقائيًا.
- التحميل: اللوحة المفتوحة وحدها يُعمل لها render، فلا تُرسل الأقسام المغلقة طلبات API حتى يفتحها المستخدم.
- الوصول: أضيفت حالات `aria-expanded` وربط `aria-controls` و`region` لكل لوحة.
- التحقق: `npm run lint` ناجح، و`npm run build` ناجح مع تحويل 1880 module.

## 2026-09-23 — تنفيذ مدرسي المادة داخل الفصل

- النوع: ميزة أكاديمية كاملة في Backend وSchools Web وSchools Flutter.
- النتيجة: إضافة Action «مدرسو الفصل» على سجل الفصل، وComboBox لمواد الصف، وmulti-select بمدرسي المادة المؤهلين، مع حفظ جميع اختيارات النافذة في عملية ذرية.
- نموذج البيانات: `ClassSectionTeacherScope` يربط الفصل بـ`TeacherGradeSubjectScope`، ولا يكرر دور الأساسي أو البديل المحفوظ في `ClassSubjectTeacherAssignment`.
- التكامل: جدول حصص الفصل يعرض فقط المدرسين المختارين للمادة، وتفاصيل المدرس تعرض الفصول والمواد المختار لها منفصلة عن إسناداته الفعلية.
- الحماية: منع إزالة مدرس مستخدم في جدول الفصل، ومنع تعطيل مدرس عضو في فصل أو جدول فعال، وحماية حذف الفصل ذي العلاقات التابعة.
- الترقية: PostgreSQL migration `20260923194432_AddClassSectionTeacherScopes` وSQL Server migration `20260923194446_AddClassSectionTeacherScopes` مع backfill تلقائي لإسنادات الجداول الحالية.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ 9 اختبارات Schools ناجحة؛ لا توجد pending model changes للمزودين؛ Schools Web lint وproduction build ناجحان؛ Flutter format وanalyze ناجحان و4 اختبارات ناجحة.

## 2026-09-23 — خطة فريق مدرسي الفصل

- النوع: نقاش وتصميم أكاديمي، لم يُنفذ بعد.
- القرار المقترح: Action «مدرسو الفصل» على سجل الفصل يبدأ باختيار مادة من ComboBox ثم يعرض multi-select بمدرسي هذه المادة المؤهلين في الصف، ويحفظهم في `ClassSectionTeacherScope` مرتبط بـ`TeacherGradeSubjectScope`.
- تم تثبيت الفصل بين أهلية الصف والمادة، واختيار مرشحي كل مادة داخل الفصل، وإسناد المدرس الأساسي/البديل للمادة داخل الجدول.
- القيود: لا يُقبل مدرس خارج نطاق الصف، ولا يمكن إزالة مدرس مستخدم في جدول الفصل، وخيارات الجدول ستقتصر على أعضاء الفصل المؤهلين للمادة.
- التوثيق: `docs/implementation/CLASS_SECTION_TEACHER_ROSTER_PROPOSAL.md`.
- التحقق: مراجعة اتساق الخطة مع `ClassSection`, `TeacherGradeSubjectScope`, `ClassSubjectTeacherAssignment`, و`WeeklyTimetableSlot`؛ لا توجد تغييرات كود أو migrations في هذه المناقشة.

## 2026-09-23 — عرض التفاصيل الشامل للمدرس

- النوع: تحسين Backend وSchools Web بدون migration.
- النتيجة: إضافة Action «عرض التفاصيل» في سجل المستخدم وendpoint مجمع يعرض الصورة والهوية والحالة وتاريخ الميلاد والنوع وتواريخ الحساب وكل جهات الاتصال والأدوار، ويعرض للمدرس نطاق الصفوف والمواد وإسنادات الفصول ودوره الأساسي/البديل ومواعيد الحصص.
- مصدر البيانات: الهاتف الرئيسي يُقرأ مباشرة من Identity مع fallback للنسخة المحلية، وباقي جهات الاتصال والبيانات الأكاديمية من قاعدة المدرسة.
- التحقق: Schools API build ناجح بلا warnings أو errors؛ اختبارات Schools عددها 9 ناجحة؛ Schools Web lint وproduction build ناجحان.

## 2026-09-23 — تنفيذ نطاق المدرس وجدول الحصص الأسبوعي

- النوع: ميزة أكاديمية كاملة في Backend وSchools Web.
- النطاق: نطاق الصفوف والمواد للمدرس، مادة الفصل، المدرس الأساسي والبديل، وجدول الحصص.
- النتيجة: إضافة Action «المواد والصفوف» إلى قائمة المدرسين بفلاتر العام ونوع التعليم والمرحلة واختيار مستقل لكل صف ومادة، وإضافة شاشة جدول أسبوعي بسيطة داخل الإعداد الأكاديمي لاختيار الفصل والأساسي والبديل وأيام وأوقات الحصص.
- القيود: لا يسمح بمدرس خارج نطاق الصف والمادة، أو مادة لا تتبع الفصل، أو تجاوز العدد الأسبوعي، أو تعارض الفصل، أو تعارض المدرس الأساسي. يمنع حذف نطاق مستخدم أو تعطيل مدرس موجود في جدول فعال.
- الكيانات: `TeacherGradeSubjectScope`, `ClassSectionSubject`, `ClassSubjectTeacherAssignment`, `WeeklyTimetableSlot`, `TeacherSubstitution`.
- migrations: PostgreSQL `20260923130205_AddSchoolTeachingPlanning` وSQL Server `20260923130233_AddSchoolTeachingPlanning`.
- التحقق: Schools API build ناجح بدون warnings؛ 9 اختبارات Schools ناجحة؛ Schools Web lint وproduction build ناجحان؛ لا توجد pending model changes في PostgreSQL أو SQL Server.

## 2026-09-23 — تثبيت دقة نطاق المدرس على مستوى الصف

- النوع: توضيح قرار تصميم أكاديمي، لم يُنفذ بعد.
- القرار: نطاق المدرس لا يُحفظ على مستوى المرحلة؛ المرحلة فلتر واجهة فقط، وكل سجل نطاق يربط المدرس بمادة صف واحدة مشغلة في عام محدد عبر `GradeSubjectOffering`.
- المثال المعتمد: يمكن اختيار الصفوف الأول والثاني والثالث الابتدائي فقط لمادتي العربي والدين، والصفين الثاني والثالث الإعدادي فقط للعربي، دون أن تشمل العلاقة باقي صفوف المرحلتين.
- التوثيق: تحديث `docs/implementation/TEACHER_ACADEMIC_SCOPE_PROPOSAL.md`.
- التحقق: التصميم الحالي لـ`TeacherGradeSubjectScope` يستخدم `GradeSubjectOfferingId`، ولذلك يدعم الاختيار على مستوى الصف دون تعديل إضافي في الاتجاه المعماري.

## 2026-09-23 — تصحيح موضع إسناد المدرس إلى مادة الفصل

- النوع: قرار تصميم أكاديمي، لم يُنفذ بعد.
- القرار: قائمة المدرسين تدير نطاق الصفوف والمواد فقط؛ إسناد المدرس الأساسي والبديل لمادة داخل فصل يتم من تجربة جدول الحصص الأسبوعي وليس كـAction مستقل على المدرس.
- التصميم المقترح: `ClassSectionSubject` لمادة الفصل، و`ClassSubjectTeacherAssignment` للأساسي والبديل، و`WeeklyTimetableSlot` لقالب الحصص، و`TeacherSubstitution` للتغطية الفعلية في تاريخ محدد.
- السبب: منع تكرار المدرس في كل خانة أسبوعية، والحفاظ على جدول ثابت مع تسجيل الاستبدالات اليومية تاريخيًا.
- التوثيق: تحديث `docs/implementation/TEACHER_ACADEMIC_SCOPE_PROPOSAL.md`.
- التحقق: مراجعة اتساق التصميم مع `GradeOffering`, `ClassSection`, و`GradeSubjectOffering` الحالية؛ لا توجد تغييرات كود أو migrations.

## 2026-09-23 — مقترح نطاق تدريس المدرس قبل إسناد الفصل

- النوع: نقاش تصميم أكاديمي، لم يُنفذ بعد.
- النطاق: المدرسون، مواد الصفوف، وإسناد المدرس الأساسي والمساعد والبديل.
- النتيجة المقترحة: فصل `TeacherGradeSubjectScope` السنوي المرتبط بـ`GradeSubjectOffering` عن `TeachingAssignment` الفعلي المرتبط بـ`ClassSection` والدور وفترة السريان.
- المرحلة ونوع التعليم يستخدمان كفلاتر ولا تتكرر مفاتيحهما داخل نطاق المدرس؛ الصف والمادة والعام يُستنتجون من مادة الصف المشغلة.
- تجربة الواجهة المقترحة تبدأ من Action «المواد والصفوف» على سجل المدرس، ثم العام ونوع التعليم والمرحلة والصفوف ومواد كل صف.
- التوثيق: `docs/implementation/TEACHER_ACADEMIC_SCOPE_PROPOSAL.md`.
- التحقق: مراجعة الكيانات الحالية `GradeOffering`, `ClassSection`, `CurriculumGradeSubject`, `GradeSubjectOffering`, و`LocalUserAccount`؛ لا توجد تغييرات كود أو migrations في هذه المناقشة.

## 2026-09-23 — تثبيت نطاق المالية والإدارة الداخلية وFamily App

- النوع: توثيق نطاق + قرار معماري مستقبلي.
- النطاق: النظام المالي، إدارة الموظفين والموارد، وتجربة الطالب وولي الأمر.
- النتيجة: إنشاء `docs/reports/POST_ACADEMIC_MODULES_REFERENCE.md` ليحفظ النطاق الكامل للوحدات التالية بعد الأكاديمي، ويحدد ملكية البيانات والتكاملات والاعتماد بين الوحدات.
- تم تثبيت أن العمليات المالية والإدارية اليومية تخص المدرسة، وأن Family App قناة للتقديم والمقابلات والمحاضرات والتواصل والعرض والدفع، بينما تبقى قواعد العمل في Backend.
- التحقق: مراجعة الروابط بين المرجع الجديد ومرجع Ultimate وملفات الحالة والقرارات والخطوات التالية، مع فحص عدم وجود أسرار أو بيانات شخصية حقيقية.

## 2026-09-23 — تثبيت مرجع Ultimate Schools الدائم

- النوع: توثيق + قرار معماري/تنظيمي.
- النطاق: وثائق المشروع والمقارنة التنافسية.
- النتيجة: إنشاء `docs/reports/ULTIMATE_SCHOOLS_REFERENCE.md` ليجمع بصورة دائمة نتائج فحص Ultimate Schools ومقارنتها بـMdaresna وترتيب تنفيذ Student Core والقبول والجداول والحضور والتقييم والامتحانات والنتائج والبوابات.
- تم ربط المرجع بالحالة الحالية والقرارات والخطوات التالية، مع منع تخزين بيانات الدخول أو الأسرار.
- التحقق: مراجعة وجود الملف وروابطه الداخلية والبحث عن أي بيانات اعتماد أو أسرار؛ لا توجد قيم سرية مخزنة.

## 2026-09-22 — مقارنة النطاق الأكاديمي بمنصة Ultimate Schools

- النوع: مراجعة فجوات وظيفية ومعمارية بدون تغيير كود.
- النطاق: إعداد المدرسة، التشغيل الأكاديمي، الطلاب، الحضور، التقييمات والامتحانات.
- النتيجة: تأكد أن Mdaresna أقوى في عزل المدارس، الهيكلة المادية، السلم التعليمي والتشغيل السنوي، وفصل المنهج والكتب بإصدارات تاريخية؛ بينما ينتهي التنفيذ الحالي عند تشغيل الصفوف والمواد، ولا توجد بعد كيانات أو APIs تشغيلية للطلاب والأوصياء والقبول والتسجيل وإسناد المعلمين والجداول والحضور والترقية أو للـgradebook والاختبارات والكنترول والشهادات.
- الأولوية المقترحة: ملف الطالب والأوصياء ثم القبول/التسجيل والتوزيع على الفصول، ثم إسناد المعلمين والجداول، ثم الحضور، ثم إطار التقييم ودفتر الدرجات، وأخيرًا الامتحانات والكنترول والنتائج.
- التحقق: مراجعة `SchoolsDbContext` وكيانات Academics/Identity وControllers وSchools Web؛ صفحات الطلاب والإعداد الأكاديمي والمالية والإشعارات في التنقل العام ما زالت تعرض `comingSoon`، بينما إدارة الهيكلة الأكاديمية الفعلية متاحة داخل الإعدادات.

## 2026-09-22 — أخطاء تشغيل المواد مترجمة ومحددة السبب

- النوع: إصلاح تشخيص الأخطاء وتعريب الواجهات.
- النطاق: Schools API وSchools Web وSchools Flutter.
- النتيجة: فُصل التحقق من تشغيل المادة إلى أكواد أخطاء مستقلة لاختيار ناقص، أو صف غير متاح، أو مادة غير فعالة، أو اختلاف الصف، أو خطة غير مفعلة، أو غير مسندة للعام، أو مختلفة عن خطة العام.
- Web وFlutter يعرضان رسائل مترجمة من `error code` حسب لغة المستخدم، ولا يعرضان نص رسالة الـBackend الإنجليزية مباشرة؛ الأكواد غير المعروفة تستخدم رسالة fallback مترجمة للسياق.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بدون warnings و9 اختبارات ناجحة؛ Schools Web lint/build ناجحان؛ Flutter format/analyze ناجحان و4 اختبارات ناجحة.

## 2026-09-22 — مراجعة نواقص خطة المنهج قبل التفعيل

- النوع: تحسين دورة تفعيل الخطة + قرار قواعد عمل.
- النطاق: Schools API وSchools Web وSchools Flutter.
- النتيجة: أضيف endpoint لمراجعة جاهزية الخطة قبل التفعيل، ويكشف عدم وجود مواد صفوف فعالة، ووجود مواد غير فعالة، والمواد بلا كتب فعالة، والمواد ذات الكتب بلا كتاب أساسي.
- تعرض الواجهتان رسالة مجمعة بالنواقص وتتيح الإلغاء أو التفعيل رغمها؛ ويتطلب الـAPI موافقة صريحة `AllowIncomplete` عند وجود نواقص حتى لا تتجاوزها أي واجهة بالخطأ.
- تظل الخطة snapshot غير قابلة للتعديل بعد التفعيل سواء كانت مكتملة أو فُعّلت باستثناء صريح.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بدون warnings و9 اختبارات ناجحة؛ Schools Web lint/build ناجحان؛ Flutter format/analyze ناجحان و4 اختبارات ناجحة.

## 2026-09-18 — تبسيط إنشاء الفصل الدراسي

- النوع: تبسيط نموذج الإدخال وتثبيت هوية مولدة من الخادم.
- النطاق: Schools API وSchools Web وSchools Flutter.
- النتيجة: لم يعد إنشاء أو تعديل الفصل يطلب الكود والاسم العربي والاسم الإنجليزي؛ يختار المستخدم تشغيل الصف ويحدد السعة والفترة فقط.
- Schools API يولد كودًا فريدًا واسمًا تسلسليًا عربيًا وإنجليزيًا عند الإنشاء، ويحافظ على الهوية عند تعديل تشغيل الصف أو السعة أو الفترة.
- لا توجد migration مطلوبة لأن أعمدة الهوية الحالية ما زالت مستخدمة.
- التحقق: Schools API build ناجح بدون warnings و9 اختبارات ناجحة؛ Schools Web lint/build ناجحان؛ Flutter format/analyze ناجحان و4 اختبارات ناجحة.

## 2026-09-18 — فك الاعتماد الدائري بين تشغيل الصف والفصل

- النوع: إصلاح قواعد عمل وتجربة استخدام.
- النطاق: Schools API وSchools Web وSchools Flutter.
- النتيجة: أصبح تشغيل الصف في حالة `Draft` ظاهرًا ومتاحًا عند إنشاء أو تعديل الفصل؛ تُقبل الحالات `Draft/Active` ويُستبعد التشغيل `Closed` أو المحذوف في الواجهات والـBackend.
- تظل قاعدة التفعيل كما هي: يُنشأ الفصل ويُفعّل أولًا، ثم يمكن تفعيل تشغيل الصف؛ كما مُنع تفعيل أو استعادة فصل تحت تشغيل صف مغلق.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بدون warnings و9 اختبارات ناجحة؛ Schools Web lint/build ناجحان؛ Flutter format/analyze ناجحان و4 اختبارات ناجحة.

## 2026-09-18 — تصحيح إدخال تشغيل الصفوف

- النوع: إصلاح نموذج بيانات وتجربة استخدام.
- النطاق: Schools API وSchools Web وSchools Flutter.
- النتيجة: أزيل إدخال الكود والاسم العربي والإنجليزي من نموذج تشغيل الصف، وأصبح المستخدم يختار المرحلة والمسار اختياريًا للفلترة ثم الصف والعام المتوافق مع نوع التعليم والسعة الاختيارية.
- Schools API يتحقق من انتماء الصف والعام إلى نوع التعليم نفسه ويولد كودًا ثابتًا واسمي العرض تلقائيًا، ويمنع نقل تشغيل صف مستخدم أو فعال إلى عام أو صف آخر.
- لا توجد migration جديدة لأن مفاتيح الربط كانت موجودة بالفعل.
- التحقق: Schools API build ناجح بدون warnings و9 اختبارات ناجحة؛ Schools Web lint/build ناجحان؛ Flutter analyze بدون مشاكل.

## 2026-09-18 — المواد وخطط المناهج وإصدارات الكتب

- النوع: ميزة كاملة + قرار معماري.
- النطاق: Schools API وقواعد مدارس PostgreSQL وSQL Server وSchools Web وSchools Flutter.
- النتيجة: إضافة المواد المشتركة، خطط المناهج ذات الإصدارات، مواد الصفوف داخل الخطة، الكتب وإصداراتها، أدوار الكتب seeded، ربط الكتب بمواد الخطة، وتشغيل المواد سنويًا داخل الصف المفتوح.
- أضيف إسناد الخطة إلى `ProgramAcademicYear` مع منع تغييره بعد بدء العام، ومنع تفعيل خطة فارغة أو تعديل snapshot مفعلة/مستخدمة تاريخيًا، والتحقق من تطابق نوع التعليم والصف والخطة عند التشغيل السنوي.
- جميع الكيانات الجديدة تدعم دورة الحياة والصلاحيات الأكاديمية الحالية وsoft delete والاستعادة مع حماية العلاقات والأدوار النظامية.
- migrations: PostgreSQL `20260918195744_AddSchoolCurriculumBooks` وSQL Server `20260918195800_AddSchoolCurriculumBooks`.
- التحقق: Schools API build ناجح بدون warnings؛ 9 اختبارات Backend ناجحة؛ Schools Web lint/build ناجحان؛ Flutter analyze بدون مشاكل و4 اختبارات ناجحة؛ لا توجد pending model changes للمزودين.

## 2026-09-18 — المراحل والصفوف والفصول وإسناد الغرف

- النوع: ميزة كاملة + قرار معماري.
- النطاق: Schools API وقواعد مدارس PostgreSQL وSQL Server وSchools Web وSchools Flutter.
- النتيجة: إضافة المراحل التعليمية والمسارات الاختيارية والصفوف، وفتح الصف حسب عام البرنامج، وإنشاء الفصول السنوية وإسنادها إلى الغرف بفترات تاريخية وساعات اختيارية.
- قواعد العمل: لا يُفعّل الصف المفتوح بدون فصل فعال؛ الغرفة يجب أن تكون من نوع فصل وقابلة للجدولة وبسعة كافية؛ يمنع تعارض إسنادات الغرفة، كما تمنع تعديلات الغرفة أو نوعها من كسر الإسنادات الفعالة.
- جميع الكيانات الجديدة تدعم التفعيل والإيقاف وsoft delete والاستعادة باستخدام صلاحيات `school.academics.*` الحالية.
- migrations: PostgreSQL `20260918101807_AddAcademicStagesGradesAndClasses` وSQL Server `20260918101842_AddAcademicStagesGradesAndClasses`.
- التحقق: Schools API build ناجح بدون warnings؛ 9 اختبارات Backend ناجحة؛ Schools Web build/lint ناجحان؛ Flutter analyze ناجح و4 اختبارات ناجحة؛ لا توجد pending model changes للمزودين.

## 2026-09-18 — إصلاح حفظ مواعيد الدراسة

- النوع: إصلاح API contract.
- السبب: عناصر HTML/Flutter ترسل الوقت بصيغة `HH:mm` مثل `08:00`، بينما محول `TimeOnly` الافتراضي رفضها أثناء model binding وأعاد HTTP 400 قبل دخول الـcontroller.
- النتيجة: حقلا `StartsAt` و`EndsAt` يقبلان الآن صيغتي `HH:mm` و`HH:mm:ss` عبر JSON converter مخصص، مع الإبقاء على التحقق أن وقت النهاية بعد البداية.
- لا توجد migration مطلوبة.
- التحقق: Schools API build ناجح بدون warnings، و9 اختبارات Schools ناجحة.

## 2026-09-18 — الهيكلة الأكاديمية والتقويم والتشغيل

- النوع: ميزة كاملة + قرار بيانات.
- النطاق: Schools API، قواعد مدارس PostgreSQL وSQL Server، Schools Web، Schools Flutter.
- النتيجة: إضافة أنواع التعليم وتعريفات الأعوام والأعوام حسب البرنامج والتيرمات والفترات، وقوالب أيام/ساعات الدراسة حسب البرنامج والفرع، وأحداث التقويم والإجازات والإغلاق والأنشطة والاختبارات.
- جميع الكيانات تدعم التفعيل والإيقاف وsoft delete وسلة المحذوفات والاستعادة، مع صلاحيات مستقلة للأكاديمية والتشغيل والتقويم ومنحها افتراضيًا لـ`School Admin`.
- migrations: PostgreSQL `20260917223147_AddSchoolAcademicCalendarOperations` وSQL Server `20260917223148_AddSchoolAcademicCalendarOperations`.
- التحقق: Schools API build ناجح؛ 9 اختبارات Backend ناجحة؛ Schools Web lint/build ناجحان؛ Flutter analyze ناجح و4 اختبارات ناجحة؛ لا توجد pending model changes للمزودين.

## 2026-09-18 — ترتيب إعدادات الهيكلة المادية حسب الاعتماد

- النوع: تحسين تجربة استخدام في Schools Web وSchools Flutter.
- النتيجة: أصبح ترتيب الأقسام: الفروع، المباني، الأدوار، أنواع الغرف، قدرات الغرف، ثم الغرف؛ حتى تُعرّف الأنواع والقدرات قبل إنشاء الغرفة التي تعتمد عليها.
- التحقق: Schools Web build وFlutter analyze.

## 2026-09-18 — إصلاح تشخيص وتشغيل تحديث قواعد المدارس

- النوع: إصلاح تشغيلي + تحسين واجهة Platform Web.
- السبب: كانت عملية المدرسة عالقة بحالة `Pending` لأن Platform Worker لم يكن يعمل؛ زر المدرسة أعاد تعارضًا، بينما تحديث الكل تخطى العملية المعلقة وعرض رسالة نجاح عامة رغم أن عدد العمليات الجديدة كان صفرًا.
- النتيجة: تشغيل Platform Worker وSchools API أكمل العملية، وأصبحت حالة endpoint `Succeeded` والإصدار `20260917214728_AddSchoolFacilities`. تعرض الواجهة الآن عدد قواعد البيانات التي أضيفت فعلًا، وتحذر عند العدد صفر، وتوضح حالة العملية الجارية بدل رسالة الفشل العامة.
- تحقق قاعدة المدرسة: migration الهيكلة مطبقة، وصلاحيات `school.facilities.*` الأربع موجودة ومربوطة بدور `school-admin`.
- التحقق: Platform Web production build ناجح و32 اختبارًا ناجحة؛ المشروع لا يحتوي script باسم `lint`.

## 2026-09-18 — تنفيذ الهيكلة المادية للمدرسة

- النوع: ميزة كاملة Backend وReact وFlutter.
- النطاق: قاعدة المدرسة، Schools API، Schools Web، Schools Flutter.
- النتيجة: إضافة الفروع والمباني والأدوار والغرف وأنواع الغرف والقدرات متعددة الربط، مع CRUD والتفعيل والإيقاف وsoft delete والاستعادة وسلة محذوفات.
- أضيفت أربع صلاحيات مستقلة ومنحت لدور `School Admin`، مع query filters وpartial unique indexes وقيود تمنع حذف الأصل قبل معالجة الأبناء أو استعادة الابن قبل الأصل.
- تم إصلاح Flutter لتمرير access token والصلاحيات من نتيجة تسجيل الدخول حتى تعمل شاشة الهيكلة الفعلية.
- migrations: PostgreSQL `20260917214728_AddSchoolFacilities` وSQL Server `20260917214750_AddSchoolFacilities`.
- التحقق: Schools API build ناجح بدون warnings، و9 Schools tests ناجحة، ولا توجد pending model changes للمزودين، وSchools Web lint/build ناجحان، وFlutter analyze بدون مشاكل و4 اختبارات ناجحة.

## 2026-09-18 — تصميم الهيكلة المدرسية واعتماد soft delete

- النوع: نقاش وقرار معماري.
- النطاق: الإعدادات المادية والأكاديمية المستقبلية لتطبيق المدارس.
- النتيجة: اعتماد فصل الهيكلة المادية إلى فروع ومبانٍ وأدوار وغرف وقدرات، وفصل البرامج التعليمية عن أعوامها وتيرماتها وتقويماتها وقوالب الدوام.
- اعتماد soft delete بجانب الإيقاف، مع سلة محذوفات واستعادة وتدقيق واستبعاد افتراضي عبر query filters، ومنع الحذف المادي أو cascade غير الصريح.
- التحقق: قرار تصميمي فقط؛ لم تُجر تغييرات كود أو قاعدة بيانات بعد.

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

## 2026-09-28 — Docker للخدمات الخمس

- النوع: بنية نشر + إعداد + توثيق.
- النطاق: Platform API، Platform Web، Schools API، Schools Web، Platform Worker.
- النتيجة: إضافة Dockerfile متعدد المراحل لكل تطبيق؛ تستخدم تطبيقات الخادم SDK 9 المطابق لـ`global.json` للبناء وASP.NET Runtime 8 المطابق لـ`net8.0` للتشغيل غير الجذري مع health checks، وتستخدم تطبيقات React صورًا ثابتة خلف Nginx مع SPA fallback وسياسة cache مناسبة.
- أضيف `docker-compose.yml` موحد مع PostgreSQL 17 وRabbitMQ وvolumes واعتماد الخدمات على health، وملف `.env.docker.example` بلا أسرار حقيقية.
- إعدادات الويب أصبحت قابلة للحقن وقت تشغيل container عبر `/env.js`، مع بقاء `VITE_*` fallback للتطوير المحلي.
- أضيف دليل `docs/deployment/DOCKER_DEPLOYMENT.md` لتوليد الأسرار وتطبيق Platform migrations والبناء والتشغيل والإنتاج.
- التحقق المبدئي: Platform Web build ناجح و32 اختبارًا ناجحًا؛ Schools Web build ناجح؛ `docker compose --env-file .env.docker.example config --quiet` ناجح. كشف أول Docker build اعتماد `global.json` على SDK 9.0.317، فتم تصحيح build stages من SDK 8 إلى SDK 9 قبل إعادة التحقق.
- كشف smoke test أن `Platform.Worker` يحتفظ بقيمة `Urls` من `appsettings.json` على loopback والمنفذ 5081؛ أضيف override صريح `Urls=http://+:8080` في الصورة وCompose حتى يصبح health endpoint متاحًا من شبكة الحاويات.
- التحقق النهائي: بناء الصور الخمس عبر Docker Compose ناجح؛ smoke tests لـPlatform API وWorker وSchools API أعادت HTTP 200 من `/health/live` وعملت بالمستخدم غير الجذري `1654`؛ smoke tests لصورتَي الويب أعادت HTTP 200 من `/health` وأثبتت حقن عنوان الـAPI وقت التشغيل؛ وCompose config وbuilds المحلية واختبارات Platform Web الـ32 كلها ناجحة.

## 2026-09-29 — دليل تسليم النشر لفريق DevOps

- النوع: توثيق تشغيل وتسليم.
- النتيجة: إنشاء ملف Word عربي RTL في `Deliverables/Mdaresna_DevOps_Deployment_Instructions_AR.docx` يغطي الخدمات الخمس والبنية التحتية ومتغيرات البيئة والأسرار وPlatform migrations ومساري النشر الأول والتحديثات وفحوصات الصحة والمراقبة وTLS والنسخ الاحتياطي والـrollback واستكشاف الأعطال وقائمة قبول النشر.
- التحقق: فتح DOCX برمجيًا بنجاح (183 فقرة و8 جداول)، وتحويله إلى PDF عبر Microsoft Word، ثم معاينة صفحاته التسع بصريًا والتأكد من سلامة RTL والجداول والأوامر وعدم وجود قص أو تداخل. لا يحتوي المستند على كلمات مرور أو مفاتيح فعلية.

## 2026-09-29 — نشر تطبيقات Web وFlutter في GitHub

- النوع: Source control + تسليم.
- النتيجة: تهيئة ورفع `Mdaresna_Platform_Web` إلى `Bodda-Elmisry/mdaresna-platform-web`، و`Mdaresna_Schools_Web` إلى `Bodda-Elmisry/mdaresna-schools-web`، و`Mdaresna_Platform` إلى `Bodda-Elmisry/mdaresna-platform-flutter`، و`Mdaresna_Schools` إلى `Bodda-Elmisry/mdaresna-schools-flutter` على فرع `main`.
- الأمان: تشديد `.gitignore` لاستبعاد ملفات البيئة المحلية وFirebase المحلية ومواد التوقيع والمفاتيح، وإجراء فحص لأنماط الأسرار والملفات الكبيرة قبل أول commit؛ لم تُرفع ملفات حساسة مرشحة.
- التحقق: Platform Web production build و32 اختبارًا ناجحًا؛ Schools Web production build ناجح وlint بلا أخطاء مع تحذير React واحد؛ `flutter analyze` و`flutter test` ناجحان للتطبيقين؛ وتطابقت commit hashes المحلية مع `origin/main` وكانت working trees نظيفة بعد الرفع.

## 2026-09-29 — استعادة dependencies المحلية لتطبيقي Web

- النوع: إصلاح بيئة تطوير محلية.
- السبب: توقف `npm ci` أثناء تنظيف `node_modules` بسبب ملفات تنفيذية مقفولة على Windows، فاختفى أمر Vite المحلي من التطبيقين.
- الإصلاح: تشغيل `npm install` داخل `Mdaresna_Platform_Web` و`Mdaresna_Schools_Web` وإعادة binaries المحلية دون تغيير الملفات المتتبعة.
- التحقق: `npm exec vite -- --version` أعاد Vite `7.3.6` للمنصة و`8.3.0` للمدارس، وكلا working tree ظل نظيفًا ومتزامنًا مع `origin/main`.

## 2026-09-29 — إصلاح نافذة إضافة الواجب في Schools Web

- النوع: إصلاح واجهة.
- السبب: `homework-editor` كان يحدد المقاس فقط دون خلفية أو تخطيط نافذة، كما كانت قاعدة أزرار `modal-actions` تتغلب على لون زر الحفظ فتجعله أبيض بلا نص ظاهر.
- الإصلاح: إضافة container كامل بخلفية وحدود وظل ومسافات وحقول responsive، وتثبيت تصميم زر الحفظ الأساسي وأزرار النافذة على سطح المكتب والموبايل.
- التحقق: `npm run lint` بلا أخطاء (تحذير React سابق واحد)، و`npm run build` ناجح، ومعاينة المتصفح المحلي أكدت سلامة RTL وظهور الحقول وزري الحفظ والإلغاء. رُفع الإصلاح إلى `mdaresna-schools-web` على commit `3c97220`.

## 2026-09-29 — تطوير تأليف وعرض الواجبات

- النوع: تحسين تشغيل وواجهة.
- النطاق: Schools Web وSchools Flutter، بالاعتماد على عقود Schools API الموجودة دون migration جديدة.
- النتيجة: فصل نموذج أسئلة الواجب الأونلاين في نافذة مستقلة قابلة للإنشاء والتعديل، ودعم عدد اختيارات ديناميكي من 2 إلى 20 للاختيار الواحد والمتعدد، ودعم عدة فراغات للإكمال مع درجة وإجابات مقبولة لكل فراغ.
- أضيف عرض كامل لتفاصيل الواجب Online وOffline، وزر تعديل للمسودة فقط يعيد تحميل التفاصيل ويحفظ عبر `PUT /api/schools/v1/homework/{id}`؛ الواجب المنشور يظل غير قابل للتعديل من الواجهة والـAPI.
- التحقق: `npm run build` ناجح، و`npm run lint` بلا أخطاء مع تحذير React المعروف، و`flutter analyze` بلا ملاحظات، و`flutter test` نجح في 4 اختبارات. رُفعت تغييرات Web على commit `1095d33` وFlutter على commit `06319a7`.

## 2026-09-29 — إصلاح حقل عدد اختيارات الواجب

- النوع: إصلاح واجهة Schools Web.
- السبب: قاعدة حجم زر الاختيار داخل `.question-options` كانت تنطبق أيضًا على أول input داخل سطر `option-count`، فتجعل حقل العدد بعرض 18px وارتفاع 18px.
- الإصلاح: إضافة override محدد لحقل العدد بعرض عمود 110px وارتفاع 42px، مع تحويله لسطر كامل العرض على الشاشات الصغيرة.
- التحقق: `npm run build` ناجح و`npm run lint` بلا أخطاء مع التحذير المعروف، وفحص computed layout في المتصفح المحلي أكد أن الحقل أصبح `110×42px` داخل صف عرضه الكامل.

## 2026-09-29 — إضافة إجمالي درجة الواجب

- النوع: إصلاح عقد وواجهة تشغيلية.
- النطاق: Schools API وSchools Web وSchools Flutter، دون تغيير قاعدة البيانات لأن عمود `TotalScore` موجود مسبقًا.
- النتيجة: أصبح إجمالي الدرجة حقلاً مطلوبًا للواجب Online وOffline ويحفظ كما أدخله المستخدم. يتحقق الأونلاين من مساواة مجموع درجات الأسئلة للإجمالي في الواجهتين والـAPI، ويعرض الفرق ويمنع الحفظ عند عدم التطابق؛ وبذلك لم يعد الواجب الأوفلاين يحفظ بدرجة صفر.
- التحقق: Schools API build ناجح بلا تحذيرات، وSchools unit tests التسعة ناجحة، وSchools Web lint/build ناجحان مع تحذير React المعروف، وFlutter analyze بلا ملاحظات واختباراته الأربعة ناجحة.

## 2026-09-29 — تصور تشغيل أنشطة الفصل الجديدة

- النوع: نقاش وتصميم UX مقترح، دون تغيير تنفيذي بعد.
- النتيجة المقترحة: استبدال نموذج النشاط القديم بتدفق واضح من قائمة الأنشطة إلى إنشاء من ثلاث خطوات ثم صفحة مستقلة لتسجيل المشاركة والتقييم، مع حالات `Draft/Published/Completed/Cancelled` وفصل حالة مشاركة الطالب عن درجته.
- تجربة الاستخدام: اختيار كل طلاب الفصل افتراضيًا مع إتاحة طلاب محددين، تثبيت Snapshot المشاركين عند النشر، حفظ التقييم مؤقتًا ثم اعتماده، وعدم السماح بدرجة أعلى من إجمالي النشاط.
- المتابعة: اعتماد التصور النهائي مع صاحب المنتج قبل تصميم العقود والجداول وتنفيذه في Schools API وSchools Web وSchools Flutter.

## 2026-09-29 — تنفيذ وحدة أنشطة الفصل

- النوع: تنفيذ Domain وPersistence وAPI وتجربة تشغيل كاملة في Schools Web وSchools Flutter.
- أضيفت دورة `Draft/Published/Completed/Cancelled` ونطاق النشاط العام أو المرتبط بمادة وجمهور كل الفصل أو طلاب محددين، مع snapshot للمشاركين عند النشر ومزامنة إضافة فقط للطلاب الجدد في نشاط كل الفصل.
- فُصلت حالة المشاركة `NotRecorded/Participated/DidNotParticipate/Excused` عن الدرجة والملاحظة، مع حفظ جزئي ومنع الاعتماد حتى اكتمال الحالات والدرجات المطلوبة، وإعادة فتح بسبب إلزامي وسجل audits لكل التحولات.
- أضيفت الصلاحيات `school.activities.view/manage/publish/grade/reopen` ونطاق الفصل والمادة للمدرس مع وصول المدير الكامل وحماية السجلات التاريخية عند إيقاف علاقة المادة. migrations هي PostgreSQL `20260929151727_AddSchoolClassActivities` وSQL Server `20260929151742_AddSchoolClassActivities`.
- استُبدل Placeholder الأنشطة في Web وFlutter بقائمة وفلاتر ومحرر ثلاثي الخطوات وتفاصيل وصفحة مشاركة وتقييم متجاوبة. عولجت إعادة محاولة النشر على نفس المسودة، وتغير جمهور المسودة، وإظهار أخطاء النوافذ، وإخفاء التبويب دون صلاحية العرض.
- فصلت Web فشل تحميل قائمة الأنشطة عن فشل خيارات المواد والطلاب، وأضافت إعادة محاولة موحدة وحماية مدى التاريخ. وفي Web وFlutter تُمسح مادة المسودة إذا لم تعد فعالة مع إلزام المستخدم باختيار مادة حالية، دون crash أو Dropdown assertion.
- التحقق: `dotnet build ...Mdaresna.Schools.Api.csproj -c Debug --no-restore` وRelease build بمجلد خرج مستقل ناجحان دون تحذيرات أو أخطاء، و`dotnet test ...Mdaresna.Schools.UnitTests.csproj -c Release --no-restore` ناجح `18/18`، وفحص pending model changes وتوليد idempotent scripts للمزودين ناجحان. `npm run build` و`npm run lint` ناجحان مع تحذير Homework المعروف فقط وتحذير Vite لحجم chunk غير مانع، و`flutter analyze` بلا ملاحظات و`flutter test` ناجح `7/7`.

## 2026-09-29 — تصور وخطة هندسة وحدة الاختبارات

- النوع: نقاش منتج ومراجعة Read-only للوضع الحالي، دون تنفيذ أو تغيير قاعدة بيانات.
- أكد الفحص أن `SchoolCalendarEventType.Exam` مجرد حدث تقويم وأن تبويب الاختبارات في Web وFlutter ما زال Placeholder؛ لا توجد كيانات أو APIs أو صلاحيات تشغيلية للامتحانات أو اللجان أو أرقام الجلوس أو النتائج.
- التصور المقترح يفصل `ExamKind` عن جهة الإصدار وسلطة الجدولة ونمط التنفيذ داخل الفصل أو باللجان، ويستخدم `ExamSeries` ثم اختبار المادة والجلسات وSnapshot المرشحين، مع مكونات اختيارية للجان والمقاعد والمراقبين والحضور والنتائج والتدقيق.
- تجربة الاستخدام المقترحة تجمع شاشة «الاختبارات» المركزية للدورات والجداول واللجان مع تاب داخل مساحة الفصل لاختبارات الفصل وعمل المعلم، وتنفذ على مراحل تبدأ بالتشغيل والدرجات داخل الفصل ثم اللجان وأرقام الجلوس ثم التكامل المركزي وGradebook وFamily App.
- أضيفت الخطة التنفيذية الكاملة في `docs/implementation/EXAMS_MODULE_IMPLEMENTATION_PLAN.md`: السياسات والسلطات، نموذج البيانات، state machines، APIs والصلاحيات والنطاق، محرك التعارضات، Web/Flutter، migrations للمزودين، الاختبارات، النشر والمراقبة. ثُبت Candidate snapshot ذريًا عند النشر، وجدولة مشتركة تدعم أوراقًا مختلفة داخل لجنة واحدة، وفصل مسار النتيجة لكل Paper مع bulk اختياري للدورة.
- التحقق: بحث `rg` في Domain وDbContext وControllers وmigrations وWeb وFlutter أثبت عدم وجود Exam domain تشغيلي حاليًا، وحدد `SchoolCalendarEvent` والطلاب والفصول والغرف و`SchoolClock` وواجهات Snapshot الحالية كمكونات قابلة لإعادة الاستخدام. نجح `git diff --check` وفحص بنية الخطة وأقسامها الإلزامية والمصطلحات القديمة والمسافات النهائية؛ لم تُشغّل builds لأن التغيير وثائقي فقط.

## 2026-09-29 — تنفيذ المرحلة الأولى من وحدة الاختبارات

- النوع: Domain وPersistence وAPI وتجربة تشغيل في Schools Web وSchools Flutter.
- أضيف Exam Core القابل للتوسع: policies والسلاسل والأهداف وأوراق المواد والمواعيد والجلسات والأماكن وSnapshot المرشحين وتوزيعهم والحضور ومحاولات النتائج والاعتمادات والتدقيق وإسقاط أحداث التقويم. فُصل النوع وجهة الإصدار وسلطة الجدولة ونمط التنفيذ، وأضيفت optimistic concurrency وعلاقات تمنع الحذف المتسلسل للسجلات التاريخية.
- أضيفت صلاحيات `school.exams.*` ونطاق وصول مدير المدرسة والمدرس ورئيس القسم ومنسق المادة، وواجهات API للإنشاء السريع داخل الفصل والنشر والإلغاء وكشف التعارضات وتشغيل الجلسة والحضور والنتائج ودورات اعتمادها. النشر يثبت الطلاب وبيانات المادة وينشئ حدث التقويم ذريًا؛ و«إنشاء + نشر فورًا» عملية atomic كاملة.
- استُبدل Placeholder الاختبارات في Web وFlutter بتاب فعلي حسب الصلاحية يدعم إنشاء اختبار وموعده وإجمالي درجته، النشر، بدء الجلسة، حفظ واعتماد حضور الامتحان، إنهاء الجلسة، ورصد النتائج وإرسالها للاعتماد.
- migrations: PostgreSQL `20260929170306_AddSchoolExamCore` وSQL Server `20260929170332_AddSchoolExamCore`.
- التحقق: Schools API Debug/Release build ناجح، وSchools unit tests ناجحة `35/35`، ولا توجد pending model changes للمزودين. Schools Web lint بلا أخطاء مع تحذير Homework السابق وproduction build ناجح؛ وFlutter format/analyze ناجح بلا ملاحظات واختباراته `8/8` ناجحة. تبقى تحذيرات EF المعروفة الخاصة بعلاقات required مع global query filters، وتحذير Vite غير المانع عن حجم الحزمة.

## 2026-09-29 — استكمال مراحل الاختبارات على مستوى المدرسة

- النوع: Domain وPersistence وAPI وتشغيل Schools Web وSchools Flutter، مع استبعاد مقصود لأي Family/Student surface.
- أضيفت اللجان وغرفها وسعاتها وأرقام الامتحان والمقاعد وتوزيع المرشحين عند النشر، وتكليف المراقبين والبدلاء مع فحص الغياب وتداخل التكليف وإشعار المستخدم، وجدول التكليفات الشخصي.
- أضيف الإنشاء المركزي متعدد المواد والفصول، snapshot سياسة الاختبار واشتراط الاعتماد، منع تكرار إصدار الامتحان الخارجي، إعادة الجدولة، جلسات الإعادة، التظلمات التي تنتج revision جديدًا، analytics وتقرير CSV متوافق مع Excel.
- أضيفت شاشة «الاختبارات» المركزية في Web بقائمة وملخص ومحرر متعدد الأوراق والسياسات واللجان والمراقبين والتقارير، وشاشة Flutter للاختبارات وتكليفاتي والسياسات. بقي تاب الفصل محافظًا على تشغيل الحضور والرصد الحالي.
- migrations: PostgreSQL `20260929181024_AddSchoolExamOperations` وSQL Server `20260929181041_AddSchoolExamOperations`.
- التحقق: `dotnet build ...Mdaresna.Schools.Api.csproj -c Release -o .artifacts/exam-build-3` ناجح بلا تحذيرات أو أخطاء، و`dotnet test ...Mdaresna.Schools.UnitTests.csproj -c Release --no-restore` ناجح `35/35`. `npm run build` ناجح مع تحذير Vite غير المانع عن حجم chunk. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `8/8`.

## 2026-09-29 — إصلاح نافذة إنشاء سلسلة الاختبارات

- النوع: إصلاح واجهة Schools Web وتحسين قابلية الاستخدام.
- أضيف للنافذة container أبيض كامل مع padding وحدود وظل وتمرير داخلي واستجابة للشاشات الصغيرة، وأصلح موضع وحجم زر الإغلاق وإغلاق النافذة عند الضغط على الخلفية.
- أعيد تنظيم كل ورقة اختبار في حقول معنونة للمادة والفصل والعنوان والتاريخ والوقت والمدة وإجمالي الدرجة، وأضيف حقل درجة النجاح الذي كان موجودًا في عقد الحفظ دون عنصر إدخال ظاهر.
- التحقق: `npm run build` ناجح، و`npm run lint` بلا أخطاء مع تحذيري React القائمين في `HomeworkPanel` وتهيئة العام الدراسي في شاشة الاختبارات. تعذرت المعاينة داخل جلسة مصادقة لأن المتصفح المحلي لم يحتوِ على جلسة دخول، بينما صفحة Vite عملت والبناء الإنتاجي أكد سلامة JSX وCSS.

## 2026-09-29 — قرار ربط الاختبارات الختامية بالتيرم

- النوع: قرار منتج وتصميم Domain، دون تنفيذ برمجي في هذه المناقشة.
- اعتمد أن `TermFinal` يرتبط بتيرم غير أخير وبحد اختبار أساسي واحد لكل خانة، وأن `YearFinal` واحد فقط ويرتبط بآخر تيرم؛ وبذلك يملك العام ذو الثلاثة تيرمات اختبارَي نهاية ترم واختبار نهاية عام.
- فصلت اختبارات `MakeUp/Retake/Replacement` كدورات مرتبطة بالاختبار الأساسي حتى لا تكسر قيد التفرد، وسجل القرار في ADR-033 وخطة التنفيذ و`NEXT_STEPS`.
- التحقق: مراجعة اتساق المصطلحات مع `ExamKind` و`ExamSittingPurpose` في خطة الاختبارات، ثم `git diff --check` على ملفات التوثيق.

## 2026-09-29 — قرار التسمية التلقائية للاختبارات

- النوع: قرار تجربة مستخدم وتصميم بيانات، دون تنفيذ برمجي في هذه المناقشة.
- اعتمد إلغاء إدخال اسم الاختبار الأساسي يدويًا وتوليد اسم ورقة المادة من الشهر أو الترم ونوع الاختبار واسم المادة، مع اسم مستقل للسلسلة وسطر سياق للنطاق الدراسي.
- تثبت التسمية عند النشر لكنها لا تستخدم كمفتاح تفرد؛ تظل الحقول المنظمة هي مصدر الحقيقة. سجل القرار في ADR-034 وخطة تنفيذ الاختبارات.
- التحقق: `git diff --check` على ملفات التوثيق المعدلة.

## 2026-09-29 — اكتمال خطة مداخل الاختبارات الشهرية والختامية

- النوع: اكتمال تخطيط منتج وتقني، دون تنفيذ برمجي في هذه المناقشة.
- اعتمدت ثلاثة مداخل في شاشة الاختبارات للشهري والختامي والاختبارات الإضافية، وربط الشهري بـ`YearMonth` وتيرم ونطاق صريح `Stage/Grade/ClassSection`، مع منع التداخل على الأهداف الفعلية وتثبيتها عند النشر.
- اعتمد أن اختبار التيرم/نهاية العام يعمل على مستوى الصف دون فصل ويشتق نوعه من ترتيب الترم، وأن الأسماء تولد تلقائيًا من الفترة والمادة.
- سجلت التفاصيل في ADR-035 والقسم 3.7 من خطة وحدة الاختبارات، وحدثت `NEXT_STEPS` لتجميع ADR-033 إلى ADR-035 كحزمة التنفيذ التالية.
- التحقق: `git diff --check` على ملفات التوثيق المعدلة.

## 2026-09-29 — تنفيذ مداخل وفترات ونطاقات الاختبارات

- النوع: Domain وPersistence وSchools API وSchools Web وSchools Flutter.
- أضيفت حقول فترة ونطاق السلسلة `AcademicTermId/AssessmentMonth/ScopeLevel/EducationStageId/ScopeGradeOfferingId/ScopeClassSectionId` وغرض السلسلة وعلاقتها بالأصل، مع دعم `Replacement` ضمن أغراض الجلسات.
- أصبحت الـAPI تشتق `TermFinal` أو `YearFinal` من موضع التيرم، وتمنع اختبارًا ختاميًا أساسيًا ثانيًا لنفس الصف والتيرم، وتتحقق من انتماء الشهر للتيرم ومن عدم تداخل اختبار شهري على نفس الفصل والمادة بعد توسيع نطاق المرحلة أو الصف إلى الفصول الفعلية.
- أضيفت تسمية عربية وإنجليزية تلقائية للسلسلة ولكل ورقة مادة، وأصبحت شاشة الويب وFlutter تعرض مداخل مستقلة للشهري والختامي والاختبارات الإضافية مع اختيار العام والتيرم والشهر والنطاق والمواد. قُصر الإنشاء السريع داخل الفصل على الأسبوعي والميد تيرم والأنواع الإضافية.
- migrations: PostgreSQL `20260929200827_RefineSchoolExamPeriodsAndScopes` وSQL Server `20260929200846_RefineSchoolExamPeriodsAndScopes`، مع default آمن `Regular` للبيانات السابقة.
- التحقق: بناء Schools API في `Debug` ناجح بلا تحذيرات أو أخطاء؛ اختبارات Schools Unit ناجحة `42/42`؛ `npm run lint` بلا أخطاء مع تحذيري React القائمين و`npm run build` ناجح؛ `flutter analyze` بلا ملاحظات و`flutter test` ناجح `8/8`. أكد `dotnet ef migrations has-pending-model-changes` عدم وجود تغييرات معلقة، ونجح توليد script ترقية idempotent للمigration الجديدة في PostgreSQL وSQL Server.

## 2026-09-29 — إصلاح إنشاء الاختبار وثيم زر الاختبارات الإضافية

- النوع: إصلاح Schools API وSchools Web وSchools Flutter.
- كان حقل الوقت القادم من HTML/Flutter يصل بصيغة `HH:mm`، بينما محول `TimeOnly` الافتراضي يرفضها قبل دخول الـendpoint؛ أضيف محول مرن لعقود الاختبارات يقبل `HH:mm` و`HH:mm:ss`، وأصبحت الواجهتان ترسلان الصيغة الكاملة صراحة.
- أصبح زر «اختبارات إضافية» يستخدم نفس الثيم الأساسي لأزرار إنشاء الاختبارات بدل مظهر المتصفح الافتراضي غير المتسق.
- التحقق: فُك ترميز `09:00` و`09:00:00` بنجاح إلى `09:00:00`؛ بناء Schools API ناجح بلا تحذيرات أو أخطاء؛ اختبارات Schools Unit ناجحة `42/42`؛ Web lint بلا أخطاء مع التحذيرين القائمين وWeb build ناجح؛ Flutter analyze بلا ملاحظات وFlutter tests ناجحة `8/8`.

## 2026-09-29 — إدارة سياسات الاختبارات وإصداراتها

- النوع: تحديث Schools API وSchools Web وSchools Flutter دون migration جديدة؛ حقول السياسة موجودة مسبقًا.
- استُبدل زر إنشاء السياسة الافتراضية بمحرر كامل للنطاق (نوع التعليم والمرحلة ونوع الاختبار) وطريقة التنفيذ وفترة السريان وصلاحيات المدرس ومتطلبات اعتماد القسم والمدرسة والنتائج وتجاوز تحذيرات الجدول. تعرض البطاقات جميع القرارات وحالة الإصدار وتسمح بالتعديل كإصدار جديد أو التفعيل والإيقاف.
- تحفظ كل عملية تعديل إصدارًا جديدًا وتوقف السياسة النشطة المنافسة لنفس النطاق، كما توقف إعادة تفعيل إصدار قديم أي منافس. صحح اختيار السياسة ليراعي المرحلة الفعلية، وتثبت snapshot كاملة وقت إنشاء الاختبار السريع أو المركزي.
- فُعلت القواعد في دورة العمل: منع إنشاء المدرس حسب السياسة، اشتراط اعتماد القسم أو المدرسة قبل نشر المسودة، السماح لمنشئ الاختبار بالنشر المباشر عند تفعيله، والاعتماد التلقائي للنتائج عند عدم اشتراط اعتمادها. تبقى التعارضات المانعة غير قابلة للتجاوز.
- التحقق: Schools API Debug build ناجح بلا تحذيرات أو أخطاء، واختبارات Schools Unit ناجحة `42/42`. Schools Web lint بلا أخطاء مع تحذيري React القائمين وproduction build ناجح مع تحذير chunk غير مانع. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `8/8`.

## 2026-09-29 — إصلاح نشر نطاق اختبار يحتوي فصولًا فارغة وتعريب أخطائه

- النوع: إصلاح Schools API وSchools Web وSchools Flutter دون تغيير قاعدة البيانات.
- كان النشر يرفض ورقة الاختبار فور العثور على أي فصل مستهدف بلا قيود طلاب فعالة، حتى إذا احتوت فصول أخرى داخل نطاق المرحلة أو الصف على طلاب. أصبح النشر يتجاوز الفصل الفارغ ويثبت الطلاب من الفصول غير الفارغة، ولا يعيد `exams.audience_empty` إلا إذا كان إجمالي مرشحي الورقة صفرًا.
- كانت واجهات الاختبارات تعرض `message` الإنجليزية القادمة من الـAPI مباشرة رغم أن لغة التطبيق عربية. أضيف مسار موحد لترجمة أكواد `exams.*` في Schools Web وSchools Flutter، مع fallback عربي للأكواد غير المعروفة، واستُخدم في إنشاء ونشر وتشغيل الاختبارات والسياسات واللجان والحضور والنتائج.
- التحقق: Schools API Debug build ناجح بلا تحذيرات أو أخطاء واختبارات Schools Unit ناجحة `42/42`. Web lint بلا أخطاء مع تحذيري React القائمين وproduction build ناجح مع تحذير chunk غير مانع. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `8/8`.

## 2026-09-30 — تحسين القيم الافتراضية لرصد درجات الاختبار

- النوع: تحسين تجربة استخدام في Schools Web وSchools Flutter دون تغيير Backend أو قاعدة البيانات.
- عند فتح شاشة النتائج، المرشح غير المرصود `Pending` يظهر بحالة «درجة» وبإجمالي درجة الورقة كاملًا. تغيير حالته إلى أي حالة غير رقمية يمسح الدرجة، والعودة إلى «درجة» تعيد الإجمالي. النتائج المحفوظة سابقًا لا تتغير وتعرض بقيمتها الفعلية.
- التحقق: Schools Web lint بلا أخطاء مع تحذيري React القائمين وproduction build ناجح مع تحذير chunk غير مانع. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `8/8`. اختبارات Schools Unit ناجحة `42/42`.

## 2026-09-30 — استكمال دورة إنهاء اختبار الفصل

- النوع: إصلاح دورة تشغيل في Schools API وSchools Web وSchools Flutter دون تغيير قاعدة البيانات.
- أصبحت بطاقة الاختبار تخفي الحضور بعد اكتمال الجلسة، وتقصر «رصد الدرجات» على حالة `EntryOpen`، وتعرض اعتماد النتائج أو نشرها حسب الحالة والصلاحية. كما تعرض مرحلة النتائج بدل إبقاء الحالة المضللة «منشور» بعد انتهاء الجلسة.
- عند نشر آخر نتائج في السلسلة وبعد اكتمال كل جلساتها، تغلق Schools API السلسلة تلقائيًا وتسجل `SeriesClosed`. إعادة فتح نتيجة منشورة تعيد السلسلة إلى `Published` وتسجل `SeriesReopened`.
- التحقق: Schools API Debug build ناجح بلا تحذيرات أو أخطاء واختبارات Schools Unit ناجحة `42/42`. Schools Web lint بلا أخطاء مع تحذيري React القائمين وproduction build ناجح مع تحذير chunk غير مانع. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `8/8`.

## 2026-09-30 — اعتماد تصور ملاحظات الطالب داخل الحصة

- النوع: قرار منتج وتصميم وظيفي دون تنفيذ برمجي.
- ستكون الملاحظة على طالب محدد داخل الحصة، مرتبطة بالفصل والمادة والمدرس وتصنيف الملاحظة ونصها، مع المتابعة ومستوى الرؤية وتجهيز العرض المستقبلي للأسرة.
- اعتمدت خمسة وجوه فقط بمستويات `1..5`. تبقى القيمة `null` لمعنى «لم يُقيّم»، ولا يستخدم الصفر كوجه سادس أو كبديل للحالة غير المحددة.
- التحقق: مراجعة اتساق المجال وعدد الحالات فقط؛ لا توجد تغييرات كود أو قاعدة بيانات في هذه المناقشة.

## 2026-09-30 — تنفيذ ملاحظات الطالب داخل الحصة

- النوع: ميزة كاملة في Schools API وSchools Web وSchools Flutter مع migrations للمدرسة.
- أضيفت ملاحظات متعددة للطالب مرتبطة بتاريخ الحصة وخانة الجدول والفصل والمدرس، بتصنيفات أكاديمية وسلوكية ومشاركة والتزام وإيجابية وأخرى، ونص وتقييم اختياري `1..5` ومتابعة ورؤية داخلية أو مؤهلة للأسرة مستقبلًا.
- يدعم التبويب اختيار التاريخ والحصة وقائمة الطلاب، وإضافة الملاحظة وتعديلها وحذفها منطقيًا مع Audit. يطبق النطاق على المدير أو المدرس الأساسي أو البديل الفعلي، والصلاحيتين `school.lesson_notes.view/manage`.
- migrations: PostgreSQL `20260930131644_AddStudentLessonNotes` وSQL Server `20260930131759_AddStudentLessonNotes`.
- التحقق: Schools API build ناجح بلا تحذيرات، واختبارات Schools Unit ناجحة `47/47`. Schools Web lint بلا أخطاء مع تحذيري React القائمين وproduction build ناجح. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `9/9`. أكد `dotnet ef migrations has-pending-model-changes` للمزودين عدم وجود تغييرات معلقة.

## 2026-09-30 — استكمال نطاق رئيس القسم في ملاحظات الحصة

- امتدت قائمة الفصول وتبويب الملاحظات لتعرض لرئيس القسم فصول وحصص المدرسين ذوي العضوية الفعالة في قسمه والأقسام التابعة، مع اسم المدرس الفعلي وتمييز البديل.
- طبقت الحماية في Schools API؛ رئيس القسم يشاهد، بينما الإضافة تبقى لصاحب الحصة الأساسي أو البديل الفعلي أو المدير.
- لا توجد database changes. التحقق: Schools API build ناجح بلا تحذيرات، واختبارات Schools Unit ناجحة `47/47`، وSchools Web build ناجح وlint بلا أخطاء جديدة، و`flutter analyze` بلا ملاحظات و`flutter test` ناجح `9/9`.

## 2026-09-30 — تصور تقييم أداء الطلاب داخل الحصة

- النوع: خطة منتج وتقنية مقترحة دون تنفيذ برمجي.
- يقيم مدرس الحصة كل طالب مرة واحدة للحصة الفعلية وبشكل سريع دون ملاحظات نصية. التوصية هي فصل التقييم إلى بعدين مستقلين، التركيز والسلوك، ولكل منهما خمسة وجوه `1..5` حتى تكون مؤشرات التطور قابلة للتحليل ولا تختلط دلالتها.
- يستبعد الغائب تلقائيًا عند توفر حضور الحصة، وتبقى حالة «غير مقيم» مستقلة بدل اختراع درجة. بعد الاعتماد تقفل القائمة، ولا تصحح إلا بإعادة فتح مدققة وفق صلاحية مستقلة.
- لا توجد أوامر تحقق لأن المناقشة لم تغير الكود أو قاعدة البيانات.

## 2026-09-30 — تنفيذ خطط التدريس وDocument Core

- أضيفت كيانات الخطط والأهداف والعناصر والمرفقات والتدقيق، ودورة `Draft/Submitted/Approved/Published/Archived` مع توليد الاسم والتحقق من العام والتيرم ونطاق الفصل ومنع تداخل المنشور.
- أضيف Document Core ونسخ الملفات وSHA-256 وحالة التحقق وأثر التدقيق، ومزود filesystem معزول لكل مدرسة خلف `ISchoolDocumentStorage`، ورفع/تنزيل المرفقات عبر ربط `TeachingPlanDocument` typed.
- أضيفت إدارة السنوية والتيرم في الإعداد الأكاديمي على Web، وإجراءات الشهرية والأسبوعية واليومية واختيار عدة فصول من شاشة الفصول على Web وFlutter، مع رفع المرفقات في التطبيقين.
- migrations: PostgreSQL `20260930203507_AddTeachingPlansAndDocumentCore` وSQL Server `20260930205617_AddTeachingPlansAndDocumentCore`.
- التحقق: Schools API build ناجح؛ `dotnet test --no-restore` ناجح (`18+16+53+11+176`)، Web lint بلا أخطاء وbuild ناجح، Flutter analyze بلا issues واختبارات `10/10`، وEF لا يحتوي pending model changes للمزودين.

## 2026-09-30 — تنفيذ تقييم أداء الطلاب داخل الحصة

- أضيف سجل فريد لكل فصل وتاريخ وخانة جدول مع تفاصيل جميع الطلاب: تركيز وسلوك `1..5` أو غير مقيم أو غائب، واعتماد مرة واحدة وإعادة فتح ترفع المراجعة وتحفظ Audit.
- يستبعد Schools API الغائب تلقائيًا من الحضور اليومي أو حضور الحصة المعتمد، ويقصر التسجيل بعد بدء الحصة على المدرس الأساسي أو البديل الفعلي أو المدير، مع مشاهدة رئيس القسم لنطاق مدرسيه.
- أضيفت واجهتا Web وFlutter بقائمة الطلاب والوجهين لكل بعد وتطبيق جماعي سريع وقفل بعد الاعتماد. الصلاحيات: `school.lesson_evaluations.view/manage/reopen`.
- migrations: PostgreSQL `20260930142208_AddStudentLessonEvaluations` وSQL Server `20260930142302_AddStudentLessonEvaluations`.
- التحقق: Schools API build ناجح بلا تحذيرات، واختبارات Schools Unit ناجحة `53/53`. Schools Web lint بلا أخطاء جديدة وproduction build ناجح. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `10/10`. أكد `dotnet ef migrations has-pending-model-changes` للمزودين عدم وجود تغييرات معلقة.

## 2026-10-01 — تحسين تصميم محرر خطط التدريس في Schools Web

- أعيد بناء نافذة إنشاء الخطة إلى أقسام واضحة للنطاق والفترة والمحتوى والمرفقات، مع اختيار مرئي لنوع الخطة، بطاقات للفصول المستهدفة، حقول موحدة بحالات focus، فترة زمنية واضحة، ورفع ملفات مخصص يعرض الملفات المختارة وإزالتها.
- أضيفت استجابة أفضل للشاشات الصغيرة وترويسة وتذييل ثابتان داخل النافذة، مع الحفاظ على عقد الإنشاء والرفع الحالية دون تغيير Backend أو قاعدة البيانات.
- التحقق: `npm run lint` ناجح دون أخطاء مع التحذيرات الثلاثة القائمة في `AcademicSettings` و`HomeworkPanel` و`ExamsManagementPage`، و`npm run build` ناجح مع تحذير حجم الحزمة غير المانع. عمل التطبيق المحلي دون أخطاء console، لكن المعاينة الداخلية توقفت عند صفحة الدخول لعدم وجود جلسة مدرسة فيها.

## 2026-10-01 — ضبط فترات الخطط الشهرية والأسبوعية واليومية

- أصبحت الشهرية تعرض أشهر التيرم فقط وتولد حدود الشهر داخله، والأسبوعية تعرض الشهر ثم أسابيع التيرم ذات السبعة أيام مع الفترة الفعلية، واليومية تعرض تاريخًا واحدًا يبدأ بتاريخ المدرسة المحلي ويمكن تغييره داخل التيرم.
- أضاف endpoint خيارات الخطط `schoolLocalDate` محسوبًا عبر `SchoolClock` والمنطقة الزمنية للمدرسة، وأضيف تحقق Server-side يمنع إرسال شهر أو أسبوع لا يطابق التقسيم المعتمد. أصبحت تسمية الأسبوع تعتمد رقمه من بداية التيرم حتى إذا عبر نهاية الشهر.
- التحقق: Schools API Debug build ناجح بلا تحذيرات، واختبارات Schools Unit ناجحة `53/53`. Schools Web lint بلا أخطاء مع التحذيرات الثلاثة القائمة، وproduction build ناجح مع تحذير حجم الحزمة غير المانع. لا توجد migration جديدة.

## 2026-10-01 — إتاحة Swagger في Schools API أثناء التطوير

- نُقل Swagger قبل middleware المصادقة والتفويض حتى لا تطبق عليه `FallbackPolicy` العامة وتعيد `401`، مع إبقائه داخل بيئة `Development` فقط.
- أضيف تعريف Bearer JWT وزر `Authorize` لتجربة endpoints المدارس المحمية، وصُحح عقد رفع مرفق الخطة بإزالة `[FromForm]` المباشر من `IFormFile` الذي كان يمنع توليد ملف OpenAPI ويرجع `500`.
- التحقق: بناء Schools API في مجلد خرج مستقل ناجح بلا تحذيرات، وsmoke test على نسخة Development مستقلة أعاد `200` لكل من `/swagger/index.html` و`/swagger/v1/swagger.json`، واحتوى المستند على `Mdaresna Schools API` وتعريف أمان `http/bearer`.

## 2026-09-30 — تصور خطط العمل الأكاديمية متعددة المدد

- النوع: خطة منتج وتقنية مقترحة دون تنفيذ برمجي.
- الخطة السنوية وخطة التيرم تكونان على تشغيل الصف والمادة داخل العام الدراسي وتداران من إعدادات الصف، مع الاحتفاظ بمصدر الوزارة ونسخته. الخطط الشهرية والأسبوعية واليومية تكون على الفصل والمادة وتدار من شاشة الفصول.
- يدعم الإجراء الجماعي اختيار عدة فصول من تشغيل الصف نفسه وإنشاء خطة مشتركة ذات عدة أهداف، بينما يبدأ إجراء كارت الفصل بنفس النموذج مع تحديد الفصل تلقائيًا. الاسم والفترة يولدهما الخادم من نوع الخطة والتقويم والمنطقة الزمنية للمدرسة.
- يلزم نموذج عناصر منظم وأثر إصدارات واعتماد، وليس حقل تفاصيل واحدًا فقط، مع منع تداخل خطة منشورة من النوع والمادة والفترة نفسها على الفصل نفسه.
- لا توجد أوامر تحقق لأن المناقشة لم تغير الكود أو قاعدة البيانات.

## 2026-09-30 — تصور إدارة المستندات والمرفقات

- النوع: تصور معماري مقترح دون تنفيذ برمجي.
- التوصية بنواة مستندات موحدة داخل نطاق كل مدرسة لبيانات الملف والنسخ ومفتاح التخزين والبصمة والحالة، مع تخزين المحتوى في Object Storage لا داخل PostgreSQL، وجداول ربط typed يملكها كل Domain مثل الخطط والواجبات والأنشطة والملاحظات والكتب.
- لا توضع ملفات المدارس الخاصة في قاعدة مركزية مشتركة؛ يعزل metadata داخل قاعدة المدرسة ومسار التخزين باسم المدرسة. المحتوى الرسمي المشترك مثل كتب الوزارة يمكن أن يدار مستقبلًا في Content Library على مستوى Platform وتحتفظ المدرسة بمرجع أو snapshot معتمد.
- لا توجد أوامر تحقق لأن المناقشة لم تغير الكود أو قاعدة البيانات.

## 2026-10-01 — Pagination وتحميل مستمر للواجبات والأنشطة والاختبارات

- أصبحت endpoints قوائم الواجبات وأنشطة الفصل واختبارات الفصل ومركز الاختبارات تعيد `PagedApiResponse` بمعاملات `pageNumber/pageSize`، تحقق `1..100`، وعد إجمالي وترتيب حتمي قبل `Skip/Take`.
- ربطت Schools Web التحميل التالي بـ`IntersectionObserver` وربطت Schools Flutter بمراقبة قرب نهاية `ScrollController`، مع دمج الصفحات دون تكرار وحالة تحميل مستقلة لا تمحو الصفحة الحالية عند الخطأ.
- أبقيت قوائم الطلاب الخاصة بالحضور والرصد والتقييم كاملة لأنها مدخلات حفظ جماعي.
- التحقق: Schools API build ناجح بلا تحذيرات؛ Schools Unit Tests ناجحة `53/53`؛ Schools Web production build ناجح وlint بلا أخطاء مع التحذيرات الثلاثة القائمة؛ Flutter analyze للملفات الأربعة المعدلة ناجح بلا ملاحظات وFlutter tests ناجحة `10/10`. لا توجد migration جديدة.

## 2026-10-02 — تنظيم مركز الاختبارات حسب النوع

- أعيد تصميم مركز الاختبارات في Schools Web وSchools Flutter إلى Accordion أحادي الفتح للأنواع الستة، مع السماح بإغلاق الجميع ورأس واضح لكل نوع وعدده وحالة فتح مرئية.
- أضيف `GET /api/schools/v1/exam-series/summary` لإرجاع إجمالي الأوراق والمنشور والمرشحين والتجميع حسب النوع، وأصبحت كل لوحة تحمل صفحات نوعها فقط عند الفتح وتستمر في التحميل بالتمرير.
- التحقق: Schools API build ناجح بلا تحذيرات؛ Schools Unit Tests ناجحة `53/53`؛ Schools Web production build ناجح؛ Flutter analyze للصفحة المعدلة ناجح بلا ملاحظات وFlutter tests ناجحة `10/10`. لا توجد migration جديدة.

## 2026-10-02 — تنظيم اختبارات الفصل حسب النوع

- طُبق نمط الـAccordion أحادي الفتح نفسه داخل تبويب اختبارات الفصل في Schools Web وSchools Flutter للأنواع الستة، مع السماح بإغلاق الجميع وإظهار العدد الحقيقي لكل نوع.
- أضيف `GET /api/schools/v1/class-workspace/{classSectionId}/exams/summary`، ودعم endpoint القائمة فلترة `kind` مع pagination مستقلة؛ تحمل الواجهة عناصر النوع المفتوح فقط وتكملها تلقائيًا بالتمرير.
- التحقق: Schools API build ناجح بلا تحذيرات؛ Schools Unit Tests ناجحة `53/53`؛ Schools Web production build ناجح وlint بلا أخطاء مع التحذيرات الثلاثة القائمة؛ Flutter analyze للوحة المعدلة ناجح بلا ملاحظات وFlutter tests ناجحة `10/10`. `git diff --check` ناجح، ولا توجد migration جديدة.

## 2026-10-02 — إضافة الاختبار الشهري من داخل الفصل

- أضيف النوع الشهري إلى محرر اختبار الفصل في Schools Web وSchools Flutter، مع خيارات تيرمات العام الخاصة بالفصل وأشهر التيرم وتقييد تاريخ الاختبار داخل الشهر المختار.
- وسعت Schools API مسار الإنشاء داخل الفصل ليستقبل `AcademicTermId/AssessmentMonth` للشهري، ويولد اسم الورقة والسلسلة تلقائيًا من الشهر والمادة، ويحفظ الشهر المنظم، ويمنع تكرار الشهري لنفس الفصل والمادة والتيرم والشهر. تبقى الاختبارات الختامية حصرًا في المركز.
- أضيف endpoint محدود بنطاق الفصل `GET /api/schools/v1/class-workspace/{classSectionId}/exams/period-options`، وعُرّب خطأ خروج الموعد عن الشهر في Web وFlutter. لا توجد migration جديدة.
- التحقق: Schools API build ناجح بلا تحذيرات؛ Schools Unit Tests ناجحة `53/53`؛ Schools Web production build ناجح وlint بلا أخطاء مع التحذيرات الثلاثة القائمة؛ Flutter analyze للملفات المعدلة ناجح بلا ملاحظات وFlutter tests ناجحة `10/10`.

## 2026-10-02 — تصور الملف التعريفي البصري للمدرسة

- النوع: مناقشة منتج وتصميم مقترح دون تنفيذ. طلب المستخدم غلافًا من صور المدرسة وشعارًا كصورة الملف، مع الرؤية والأهداف وتفاصيل المدرسة.
- المقترح: غلاف رئيسي يختاره المسؤول من معرض مرتب، شعار مستقل، نبذة ورؤية ورسالة وأهداف ووسائل تواصل، وتعديل بحسب الصلاحية ومعاينة قبل الحفظ. الصور تستخدم Document Core وروابط مدرسية typed؛ عرض Family أو العرض العام يحدد لاحقًا صراحةً.
- التحقق: مراجعة سياق المشروع و`SchoolProfileController` وشاشة `SchoolProfilePage`؛ العقد والواجهة الحاليان يعرضان البيانات الأساسية فقط دون معرض أو حقول الرؤية والرسالة. لم تتغير التطبيقات أو قاعدة البيانات ولا توجد اختبارات بناء مطلوبة للمناقشة.

## 2026-10-02 — تنفيذ الملف التعريفي البصري للمدرسة

- أضيف `SchoolPresentation` للمحتوى العربي/الإنجليزي والأهداف المرتبة والتواصل والغلاف، مع `SchoolProfileImage` كرابط typed إلى Document Core. يحافظ العقد السابق على بيانات الهوية، وتستمر الصفحة الجديدة في عرض الكود والنوع والحالة والوحدة بجانب التعريف البصري.
- أضيفت APIs للقراءة والحفظ ورفع الصور وقراءة محتواها وإزالتها، وصلاحية `school.profile.manage` لمدير المدرسة. الحفظ يحمي التزامن بـRevision ويرفض اختلاف قائمة الصور أو غلافًا ليس من المعرض؛ رفع الشعار يستبدل رابطه فقط وإزالة صورة الغلاف تصفر الغلاف، مع الاحتفاظ بالمستندات وتدقيق عمليات الصور.
- نُفذت الصفحة في Web وFlutter بغلاف وشعار ومعرض وتعليقات وترتيب ومعاينة، ونبذة ورؤية ورسالة وأهداف وتواصل. الرسائل حسب لغة الواجهة، والقراءة بمصادقة المدرسة دون نشر عام أو Family. حدود الرفع: 30 صورة للمعرض و8MB لكل صورة PNG/JPEG/WebP، مع فحص توقيع المحتوى لا فحص malware.
- جُهزت migrations PostgreSQL `20261002121024_AddSchoolPresentation` وSQL Server `20261002121052_AddSchoolPresentation` دون الاتصال بقاعدة فعلية أو تطبيق الترحيل؛ ظهرت تحذيرات EF القائمة عن العلاقات مع query filters. يلزم تحديث قاعدة المدرسة وتجديد الجلسة للحصول على الصلاحية الجديدة.
- التحقق: `dotnet build src/Schools/Mdaresna.Schools.Api/Mdaresna.Schools.Api.csproj --no-restore` ناجح `0 warnings/errors`؛ `dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore` ناجح `61/61`؛ `npm run build` ناجح مع تحذير حجم bundle، و`npm run lint` بلا أخطاء مع التحذيرات الثلاثة القائمة؛ `flutter analyze` بلا ملاحظات و`flutter test` ناجح `10/10`؛ `git diff --check` ناجح في المستودعات الثلاثة.
- مراجعة المتصفح بمهارة Browser وfixture مؤقتة ببيانات وصور هندسية تجريبية: التعديل والمعاينة والحفظ إلى mock API، شكل سطح المكتب، ومعرض موبايل بعرض 390px دون overflow أفقي. أزيلت ملفات المعاينة وأُغلق تبويب الاختبار. لم يُنفذ smoke test لرفع ملفات أو حفظ بيانات مدرسة فعلية قبل تطبيق migrations.

## 2026-10-02 — إظهار صورة غلاف المدرسة كاملة

- استُبدل عرض الغلاف المقصوص بـ`background-size: contain` دون تكرار في Web و`BoxFit.contain` في Flutter ومعاينة المحرر، مع التمركز والحفاظ على نسبة الأبعاد وخلفية هادئة للمساحة المتبقية.
- أزيل تحكم موضع الغلاف من الواجهتين، مع إبقاء الحقل المخزن للتوافق؛ لا تغيير في API أو قاعدة البيانات ولا migration جديدة.
- التحقق: `npm run build` ناجح مع تحذير حجم bundle القائم، و`flutter analyze lib/src/features/profile/school_presentation_page.dart` بلا ملاحظات. التغيير متعلق بالعرض فقط ولم يتضمن تعديل صور المدرسة الأصلية.

## 2026-10-02 — خطة إشعارات تسليم مراحل الاختبارات

- النوع: مناقشة وخطة مقترحة دون تنفيذ. الإشعار ينشأ عند انتقال ناجح وصريح للمرحلة التالية لا عند حفظ المسودة؛ تحدد السياسة خطوات تجهيز الجدول واللجان عند الحاجة ومراجعة القسم واعتماد المدرسة والنشر، بمسؤول فعلي ومهمة قابلة للإغلاق وإعادة الإسناد.
- المقترح: إعادة استخدام SchoolUserNotifications للعنوان والنص العربي/الإنجليزي، مع روابط آمنة للإجراء ومرجع خطوة/نسخة ومفتاح منع تكرار. يحفظ الانتقال والمهمة والإشعار في transaction واحدة؛ يستخدم Push/Realtime المستقبلي Outbox مع retry. يعالج الإرجاع والأسباب والتغييرات والإلغاء وتذكيرات المهام، ويجعل القراءة مستقلة عن إنجاز المهمة. لا إشعارات للطالب/الأسرة أو للإدارة التعليمية الخارجية ضمن هذه المرحلة.
- التحقق: مراجعة حالات ExamSeries وExamPolicy وExamApproval ومسارات ChangeSeriesStatus وChangeResultsStatus وPublishCore وSchoolNotificationsController. توجد إشعارات إسناد/استبدال المراقبين حاليًا، لكن تسليم المراحل والموافقات المتتابعة بمسؤولين محددين غير موصول كدورة كاملة. لا تغييرات في التطبيقات أو قاعدة البيانات ولا builds مطلوبة للمناقشة؛ أضيف المقترح إلى NEXT_STEPS بانتظار الموافقة.

## 2026-10-02 — تنفيذ تسليم مراحل الاختبار بالإشعارات والمهام

- أضيف ExamWorkflow وخطواته وتعيينات المسؤول/البديل/المشرف/الموعد، ومسار سياسة محفوظ مع الاختبار، وتسليم صريح للجدول واللجان ومراجعات الأوراق المتوازية واعتماد المدرسة والنشر. أعيد ربط إرسال/اعتماد السلسلة بمسارها، ومنع نشر مسار غير مكتمل أو بواسطة غير مسؤول النشر. السلاسل القديمة دون مسار تحتفظ بقيود نشرها السابقة.
- أضيفت APIs للتعيين والبدء والإتمام والإرجاع بسبب وإعادة الإسناد، و«مهامي» paginated تشمل المسارات المعادة لمنشئها. أعيد فحص الصلاحيات والنطاق، وحُمي التزامن بـRevision ومفاتيح حدث فريدة؛ تحفظ المهام وإشعارات العربية/الإنجليزية وOutbox في نفس الحفظ الذري. الإرجاع وتغيير الموعد قبل النشر يبطلان الموافقات؛ حذف مسودة ذات مسار نشط ممنوع وحذف غير النشط ينظف تبعياته.
- نُفذت واجهات المراحل والمهام ومسؤولي السياسة في Web وFlutter، مع فتح المسار من الإشعار وعرض السبب والإجراءات للمسؤول الفعلي، ومواعيد وبدلاء ومشرفين اختياريين. أضيفت ترجمة أكواد الأخطاء الجديدة. إشعارات النشر والإلغاء وتعديل الموعد تلتزم بالمادة والفصل والتكليف الفعلي بدل cross-product بين المواد والفصول.
- أضيفت خدمة تذكير كل دقيقة للمدارس المعرفة صراحةً في `ExamWorkflow:SchoolCodes`؛ مرة عند التأخر وتصعيد بعد 24 ساعة، مع إعادة فحص الأهلية والبديل. إعداد قائمة المدارس مطلوب للتفعيل؛ Outbox ليس مستهلك Push ولا ربط Realtime، وهما مؤجلان. لا Family أو إشعارات خارج المدرسة.
- جُهزت migrations PostgreSQL `20261002175058_AddExamWorkflowNotifications` وSQL Server `20261002175130_AddExamWorkflowNotifications` دون الاتصال بقاعدة فعلية أو تطبيقها؛ سجل EF تحذيرات query filters القائمة وتحذير العلاقة الجديدة ExamWorkflow/ExamSeries. التحقق الفعلي متعدد المستخدمين بعد الترحيل ما زال مطلوبًا.
- التحقق: Schools API build ناجح دون warnings/errors؛ `dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore` ناجح `84/84`، منها اختبارات ترتيب المراحل، توازي المراجعات، تسليم واحد، منع الإشعار الذاتي، رابط الإشعار وOutbox وقيود النموذج وتوليد SQL لاتحاد المهام النشطة والمعادة مع pagination كاملة للمزودين دون اتصال بقاعدة. `npm run build` ناجح مع تحذير bundle القائم و`npm run lint` بلا أخطاء مع التحذيرات الثلاثة القائمة. `flutter analyze` بلا ملاحظات و`flutter test` ناجح `14/14` بما فيها أربعة اختبارات واجهة جديدة. `git diff --check` ناجح للمستودعات الثلاثة.
- استُخدمت مهارة Browser لمراجعة Web بfixture مؤقتة غير متصلة بمدرسة: العرض العربي، بدء المسار، إتمام وتسليم المرحلة التالية، سبب الإرجاع الإلزامي وإعادة الإرسال، وتحميل «مهامي». أزيلت ملفا المعاينة وأغلق تبويبها وخادمها المؤقت؛ لا ادعاء smoke test لقاعدة فعلية أو Push.

## 2026-10-04 — إشعارات إسنادات المدرس وصفحة تكليفاتي

- ربطت Schools API إشعارًا عربيًا/إنجليزيًا بإضافة مدرس للقسم أو مادة صف سنوية أو مادة داخل فصل أو حصة أسبوعية، داخل نفس `SaveChanges`؛ لا إشعار عند إعادة حفظ إسناد غير متغير، وتبلغ الحصة الأساسية والمرشح الاحتياطي بنصين مختلفين، ويبلغ تغيير وقت الحصة مدرسيها الحاليين. تخزن معاينة محدودة بطول حقل الإشعار مع إبقاء السياق الكامل في عرض التكليف.
- أضيف endpoint ذاتي `GET /api/schools/v1/notifications/my-teaching-assignments` يشتق المستخدم من الجلسة فقط، وقراءات مجمعة لإسناداته؛ لا TeacherUserId من العميل ولا منح لصلاحيات الإدارة. يفتح إشعار TeacherAssignment قسمه ومعرّفه في «تكليفاتي» على Web وFlutter مع إبراز الربط وإظهار عدم إتاحته عند إزالته، ومداخل مستقلة من حساب Web وإشعارات Flutter. لا إشعارات بأثر رجعي أو Push جديد ولا migration جديدة.
- التحقق: `dotnet build src/Schools/Mdaresna.Schools.Api/Mdaresna.Schools.Api.csproj --no-restore` ناجح دون warnings/errors؛ `dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore` ناجح `97/97`، منها 13 اختبارًا جديدًا لدلتا الحصة والأدوار والنصوص والروابط والتخزين وحد طول المعاينة. `npm run build` ناجح مع تحذير حجم bundle القائم؛ `npm run lint` بلا أخطاء مع التحذيرات الثلاثة القائمة. `flutter test` ناجح `17/17`، منها ثلاثة اختبارات جديدة لفتح نوع التكليف من الهدف، endpoint الذاتي، الربط المحذوف، وإعادة محاولة التحميل؛ `flutter analyze` للصفحتين والاختبار بلا ملاحظات. `git diff --check` ناجح للمستودعات الثلاثة.
- لم تُعدّل قاعدة مدرسة فعلية ولم يُنفذ smoke test متعدد الحسابات أو اختبار Push؛ أضيف التحقق الفعلي إلى NEXT_STEPS، واستمرت إشعارات التغطية الفعلية المستقلة كما هي.

## 2026-10-04 — اقتراح توسيع إشعارات تشغيل المدرسة

- النوع: توصيات منتج دون تنفيذ أو اعتماد نهائي. بعد إشعارات إسناد المدرس والتغطية ومراحل الاختبار، الأولوية المقترحة لاستكمال إلغاء/نقل الإسنادات وتغيير المكان، رئاسة القسم وتنسيق المادة، دورة اعتماد خطط التدريس، وتذكير أعمال الحضور والتقييم والرصد غير المكتملة، ثم متابعة ملاحظات الطلاب والتغييرات في قوائم الفصول والأنشطة وطلبات التقديم.
- قواعد مقترحة: مستلم فعلي ضمن النطاق، رسالة واضحة ورابط سياقي، عدم إشعار المسودات أو تكرار الحفظ، تذكير محدود وتصعيد مرة عند الحاجة، وتجميع التغييرات الجماعية؛ تبقى إشعارات الأسرة في Family. توصيل Push/Realtime إعداد مستقل عن تعريف أحداث الإشعار ويظل مؤجلًا حتى الموافقة على التنفيذ.
- التحقق: مراجعة أقسام الإشعارات في CURRENT_STATE وDECISIONS وآخر DEVELOPMENT_LOG وNEXT_STEPS؛ لم تتغير التطبيقات أو قواعد البيانات، ولا builds مطلوبة للمناقشة.

## 2026-10-05 — تنفيذ توسيع إشعارات تشغيل المدرسة

- النوع: Schools API/Domain/Persistence وSchool Web وSchool Flutter؛ اعتماد ADR-048، بلا Family أو Push خارجي أو تغيير قاعدة مدرسة فعلية.
- رُبطت أحداث إنهاء إسنادات المدرس وتغير مكان/وقت الحصة ورئاسة القسم وتنسيق المادة والخطط والإعادة بسبب ومتابعة الملاحظات والأنشطة والقبول وإضافة الطلاب. الرسائل ثنائية اللغة، والمستلم مخول فعليًا، وملخص roster يومي، ولا يظهر نص الملاحظة أو اسم الطالب في تنبيه المتابعة. تغيير الأساسي إلى احتياطي لا يعد إلغاءً كاملًا للحصة.
- أضيف EventKey nullable وفهرس فريد للمستلم/الحدث، migrations PostgreSQL `20261004205137_AddSchoolNotificationEventKeys` وSQL Server `20261004205239_AddSchoolNotificationEventKeys`. حفظ العمل والإشعار ذري؛ retry مقيد بتصادم فهرس الإشعار يمنع فشل enrollment المتزامن بسبب ملخص يومي مكرر، ولا يتجاهل أخطاء قيود العمل.
- أضيفت خدمة تذكير opt-in بقائمة `SchoolNotifications:SchoolCodes` كل خمس دقائق: 24 ساعة تذكير، 48 ساعة تصعيد، للتقييم المعاد فتحه والنشاط السابق غير المكتمل والرصد المفتوح. دفعات keyset محدودة، إعادة فحص الحالة والصلاحيات، وعدم ملاحقة تقييم اختياري لم يبدأ. القبول يستخدم المستلمين المحددين عبر `SchoolNotifications:AdmissionsRecipients:{schoolCode}` أو المدير المؤهل.
- روابط الواجهتين تقرأ السجل الفعلي وتحترم الإلغاء الحالي للصلاحية؛ الملاحظات/التقييم يفتحان تاريخ الحصة وخانتها. `return-for-edit` للخطط يتطلب سببًا، ويحفظ audit وRevision، ويعرض السبب مع إمكانية تعديل تفاصيل المسودة ثم إعادة إرسالها. استجابة تعارض الحفظ 409 تستفيد من حماية RowVersion/xmin الموجودة.
- التحقق: `dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore` نجح `108/108`؛ `npm run build` و`npm run lint` نجحا في Schools Web دون أخطاء، مع 3 تحذيرات موجودة وتحذير bundle؛ `flutter analyze` بلا ملاحظات و`flutter test` نجح `19/19`؛ توليد migration للمزودين ناجح و`dotnet ef migrations has-pending-model-changes` لكل context يثبت تطابق snapshot. `git diff --check` نظيف في المستودعات الثلاثة. اختبارات SQL translation لا تعني إجراء اختبارات تسليم على قاعدة فعلية.
- المتبقي للتفعيل: تطبيق migrations بالتحديث المركزي وإعادة تشغيل/nشر الخدمات والواجهات وضبط القائمة الصريحة واختبار الأدوار/التذكير فعليًا. لم تنفذ عمليات نقل/انسحاب الطالب أو تعديل النشاط المنشور الغائبة، ولا إسناد متابعة الملاحظة لشخص. تذكير الحضور التاريخي مؤجل لأن الواجهة الحالية لا تتيح الإجراء خارج الحصة/اليوم الجاري؛ Push/Realtime وتفضيلات الإشعارات تظل مرحلة مستقلة.

## 2026-10-05 — مناقشة خطة School Social

- النوع: تصور منتج وتقني بانتظار الاعتماد، وليس تنفيذًا أو قرارًا معماريًا معتمدًا. Feed رسمي للمدرسة بنص ووسائط متعددة ونطاق مدرسة فقط أو مدرسة وFamily، ولايك وتعليقات وبلاغات مرتبطة بهوية موثقة وتوقيت UTC.
- المقترح: مصدر بيانات واحد في قاعدة المدرسة، actors محليون يربطون حساب المدرسة أو حساب Platform الموثق دون خلط معرفاتهما، وروابط وسائط typed إلى Document Core؛ معالجة وفحص فيديو ووصول محمي مطلوبان، وليس مجرد رفع ملف. صلاحيات مستقلة للنشر والإدارة والمراجعة، ملف مراجعة يجمع بلاغات البوست، soft-delete وتدقيق وحفظ دليل البلاغ، وإشعارات مجمعة وcursor pagination.
- Family API/App وعلاقات الدخول التشغيلية غير منفذة حاليًا؛ اكتمال التحقق من علاقة الحساب بالمدرسة شرط لإتاحة المنشورات والتفاعلات والوسائط. موافقة مسبقة لنشر Family اقتراح قابل للضبط، بلا نشر عام أو حذف آلي بسبب كثرة البلاغات. أضيف المقترح إلى NEXT_STEPS، ولم يتغير السلوك الحالي أو CURRENT_STATE أو ADR معتمد.
- التحقق: مراجعة سياق التاريخ وكيانات Document Core وGuardian/Student وعزل حسابات المدرسة؛ لا build/test للتطبيق لعدم تعديل كود. `git diff --check -- docs/project-history/DEVELOPMENT_LOG.md docs/project-history/NEXT_STEPS.md` نجح بلا أخطاء تنسيق، مع تحذير تحويل LF إلى CRLF فقط.

## 2026-10-05 — تحديد نطاق School Social وتأجيل Family

- اعتمد المستخدم قصر المرحلة الحالية على المدرسة؛ سجل ADR-049 وعدلت NEXT_STEPS لإخراج Family App/API وهوياته وتفاعلاته وإشعاراته من التنفيذ الحالي وتأجيلها إلى مرحلة إنشائه. يحتفظ التصميم بقابلية التوسع دون إظهار نشر Family فعال قبل التكامل.
- تحديث توثيق فقط؛ لم يتغير سلوك التطبيق أو CURRENT_STATE، ولم ينفذ كود أو migrations. التحقق: `git diff --check -- docs/project-history` نجح بلا أخطاء تنسيق، مع تحذيرات LF/CRLF فقط؛ لا يلزم build/test لهذا التعديل.

## 2026-10-05 — تنفيذ School Social المدرسي الأساسي

- بناءً على موافقة المستخدم نُفذت الجداول والصلاحيات وSchools Social API وReact Web وFlutter mobile، مع ملف المدرسة البصري ونص/صور/MP4 ومسودات ومراجعة ونشر وتفاعلات وبلاغات وإعدادات وإشعارات داخلية وروابط للمنشور. لا أي تنفيذ أو تعديل في Family. ADR-050 يسجل الهوية المدرسية وRevision وkeyset وروابط DocumentVersion وحفظ دليل البلاغ والحذف المنطقي.
- 10 صلاحيات seed ومigrations للمزودين (`20261004214146_AddSchoolSocial`/`20261004214352_AddSchoolSocial`)؛ لم تطبق على قاعدة فعلية. الوسائط تتحقق بالتوقيع والحدود وتُقرأ بالمصادقة وRange؛ لا ادعاء بتنفيذ malware/transcoding/thumbnails. الاستكمال والتفعيل موثقان في NEXT_STEPS.
- التحقق: `dotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj --no-restore` نجح `131/131`، بعد إصلاح تهيئة settings المتكررة في اختبار جديد؛ اختبارات EF InMemory لا تعادل integration مع قاعدة SQL. `dotnet ef migrations has-pending-model-changes --no-build` لكل PostgreSqlSchoolsDbContext/SqlServerSchoolsDbContext أكد تطابق snapshot، مع تحذيرات query filters سابقة غير خاصة بالوحدة.
- `npm run build` ناجح و`npm run lint` بلا أخطاء (3 تحذيرات قديمة وتحذير حجم bundle). `flutter pub get` و`flutter analyze` ناجحان، و`flutter test` نجح `22/22` بعد إصلاح Scaffold وحقول fixture في اختبارات Social الجديدة. أضيف video_player لعرض MP4 المحمول.
- فحص المتصفح عبر harness معزول `tests/social-browser.html` دون بيانات/توكنات حقيقية: إنشاء مسودة، نشر، لايك، تعليق، بلاغ وقائمة المراجعة، وscroll زاد 15 إلى 23 بطاقة دون تكرار. لا اختبار وسائط فعلية أو إرسال إشعارات لمدرسة إنتاج؛ يلزم smoke test بعد التفعيل.
- `git diff --check` نجح في مستودعات Backend وSchools Web وFlutter (تحذيرات LF/CRLF فقط). حُفظت معاينة mock في `.codex-artifacts/school-social-desktop.png` خارج مستودعات التطبيقات.

## 2026-10-05 — تصميم وتعريب اختيار مرفقات المنشور

- استُبدل file input المرئي في محرر School Social Web بزر مصمم «اختيار صور وفيديوهات»/Choose photos and videos، مع حالة الاختيار وعدد الملفات مترجمين، أيقونة وحالة focus بلوحة المفاتيح. يحتفظ بالمنتقي الأصلي مخفيًا وحدود الملفات/الحجم ومعاينة وأسماء الملفات وإزالتها؛ يمسح قيمة input قبل فحص الحدود لإتاحة إعادة اختيار ملف رُفض سابقًا. لا تعديل Backend/Flutter/Family أو قاعدة البيانات.
- السبب: Choose Files/No file chosen من واجهة المتصفح الأصلية وليس localization التطبيق. لغة منتقي نظام التشغيل نفسه تبقى حسب النظام.
- التحقق: `npm run build` ناجح؛ `npm run lint` بلا أخطاء وبنفس 3 تحذيرات سابقة وتحذير bundle size. فحص المتصفح على harness معزول أكد ظهور الزر والحالة بالعربية وعدم ظهور النص الأصلي، وحفظت لقطة `.codex-artifacts/school-social-upload-ar.png`؛ لم يرفع ملفًا فعليًا أو يغير بيانات مدرسة. تحديث CURRENT_STATE/ADR-050/NEXT_STEPS.

## 2026-10-05 — معرض صور المنشور داخل التطبيق

- أضيف lightbox داخل School Social Web يبدأ بالصورة المختارة ويجمع صور المنشور دون الفيديو، مع أزرار السابقة/التالية وعدّاد LTR صحيح بالعربية وأسهم لوحة المفاتيح وEscape. محرر المسودة يدعم معرض الصور المحفوظة أيضًا. تحميل الصورة الحالية فقط عبر endpoint المحمي، وتنظيف طلبات/blob URLs عند التنقل/الإغلاق؛ لا public URLs أو رفع جديد.
- Flutter يدعم فتح الصورة في Dialog fullscreen، PageView للتنقل بالسحب والأزرار وInteractiveViewer للتكبير حتى 4x وإغلاق بالزر/السحب الرأسي. الفيديو غير متأثر. لا تغيير Backend/Family أو migrations. أضيف اختبار للتنقل من الصورة المختارة وتعطيل أزرار حدود المعرض؛ الاختبار يعالج فشل تحميل الصورة بدون اتصال فعلي.
- التحقق: `flutter analyze` نظيف و`flutter test` نجح `23/23`؛ `npm run build` ناجح و`npm run lint` بلا أخطاء مع 3 تحذيرات سابقة وتحذير bundle size. `git diff --check` في الواجهتين ناجح مع تحذيرات LF/CRLF فقط. المتصفح ببيانات PNG مولدة داخل harness معزول تحقق من فتح الصورة الثانية ثم السابقة، والأسهم بعد إصلاح focus، وإغلاق Escape؛ لقطة `.codex-artifacts/school-social-gallery.png`. تحديث CURRENT_STATE/ADR-050/NEXT_STEPS.

## 2026-10-05 — توحيد معرض الصور والفيديوهات للمنشور

- صحح المستخدم المقصود إلى معرض موحد مثل Facebook. أزيل استبعاد الفيديو من معرض Web/Flutter، مع الحفاظ على ترتيب وسائط المنشور وفتح الملف المضغوط مباشرة؛ أصبح السابق/التالي للوسائط وليس الصور فقط. أضيف فتح الفيديو داخل المعرض من كارت الويب، وصار إجراء فيديو الموبايل يفتح المعرض ثم تشغيله داخله.
- Web يعرض الملف الحالي فقط ويحمّل الفيديو عبر endpoint المحمي دون autoplay، ويوقف أي inline video عند فتح المعرض ويحرر/يوقف المشغل عند التنقل أو الإغلاق. Flutter يعرض SocialMediaView للفيديو في صفحة المعرض النشطة فقط، فيُتلف controller عند مغادرتها أو إغلاق المعرض؛ الصور تحتفظ بالتكبير والسحب. ضبط ارتفاع مشغل الفيديو ضمن المساحة المتاحة. لا تعديل Backend/Family أو migrations.
- التحقق: Web `npm run build` ناجح و`npm run lint` بلا أخطاء (3 تحذيرات سابقة وتحذير bundle size)؛ Flutter analyze نظيف، و`flutter test` نجح `24/24` بعد التعديل النهائي. `git diff --check` في الواجهتين ناجح. أضيف اختبار صورة/فيديو/صورة يبدأ على الفيديو ويؤكد إزالته عند الانتقال وعودته عند الرجوع؛ لا يحاكي فك ترميز الفيديو المحمول الحقيقي.
- فحص المتصفح بوسائط مولدة داخل harness فقط: فتح الفيديو `2/3` وتشغيله (`paused=false`)، الانتقال للصورة `3/3` مع زوال مشغل الفيديو، والعودة والإغلاق بـEscape مع زوال المعرض ومشغله. الفيديو التجريبي WebM مولد محليًا وليس اختبار upload/MP4 فعلي؛ تجربة MP4 على جهاز موبايل باقية قبل الإنتاج. لقطة `.codex-artifacts/school-social-mixed-gallery.png`. عُدلت CURRENT_STATE/ADR-050/NEXT_STEPS لإلغاء قرار استثناء الفيديو السابق.

## 2026-10-05 — بطاقة فيديو وتنزيل يدوي من معرض المنشور

- حسب اختيار المستخدم أضيفت بطاقة gradient بأيقونة تشغيل وحجم الفيديو وتوجيه مترجم لفتح المعرض في Web/Flutter، بدل صورة مصغرة مولدة أو عناصر تحميل داخل قائمة المنشورات. لا تحميل bytes الفيديو في القائمة أو عند فتح المعرض؛ العرض والتنزيل بإجراء المستخدم. لا تعديل Backend/Family أو migrations أو تبعيات جديدة.
- تنزيل Web يقرأ endpoint المحمي ثم ينزّل blob ويعيد استخدام الملف المحمل؛ إغلاق العرض يلغي الطلب ويحرر URLs. Flutter mobile ينزّل بمصادقة المدرسة ويتحقق من نوع/حجم المحتوى ثم يفتح FilePicker.saveFile، مع انتظار ورسالة فشل ونجاح فقط عند حفظ فعلي وعدم اعتبار الإلغاء نجاحًا.
- التحقق: `npm run build` ناجح؛ `npm run lint` بلا أخطاء مع 3 تحذيرات سابقة وتحذير bundle size. `flutter analyze` نظيف و`flutter test` نجح 25/25، متضمنًا اختبار عدم طلب الفيديو قبل التنزيل ومصادقة الطلب وتمرير bytes/اسم الملف لمنتقي حفظ تجريبي. `git diff --check` ناجح في الواجهتين مع تحذيرات LF/CRLF فقط.
- browser harness بوسائط مولدة ومعزولة أكد 0 طلب فيديو في القائمة وبعد فتح المعرض، وطلبًا واحدًا عند التنزيل؛ تكرار التنزيل أعاد استخدام blob. التحقق من اسم ملف anchor كان دون تنزيل فعلي للنظام، ومنتقي Flutter mock دون ملف فعلي. لقطة `.codex-artifacts/school-social-video-card.png`. تجربة حفظ/تشغيل MP4 على جهاز موبايل فعلي باقية قبل الإنتاج؛ عُدلت CURRENT_STATE/ADR-050/NEXT_STEPS.
