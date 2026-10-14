# تشغيل Mdaresna باستخدام Docker

- Redis لكاش وجهات المدارس بمدة30 يومًا، مع تحديث الإدخال بالأحداث وإزالة بيانات التوجيه عند الإيقاف/الحذف، وإصدار وguard لمنع القديم. يلزم backup ومهاجرةPlatform AddSchoolRoutingVersion ثم نشرPlatform API/Worker وSchools وتفعيلconsumer وAMQPS وnamespace موحد لنسخ نفسالمنصة. تفاصيل النشر والحدود في [SCHOOL_ROUTING_REDIS.md](SCHOOL_ROUTING_REDIS.md). الأحداث غيرمتزامنة ويجب مراقبةتأخرها؛ لا يتغير.env الفعلي تلقائيًا.

## تحديث تجهيز النشر — 2026-10-14

هناك مساران الآن: `docker-compose.yml` محلي مع منافذ loopback فقط، و`docker-compose.production.yml` overlay للإنتاج. لا تشغّل overlay منفردًا. يلزم Docker Compose 2.24.4 أو أحدث بسبب `!reset`. أسماء المشروع والـvolumes السابقة لم تتغير؛ إعادة البناء ليست ترحيل بيانات أو تنفيذ migrations.

- التطبيقات الخمسة تعمل non-root وبـread-only root filesystem وcap_drop ALL وno-new-privileges، مع tmpfs لمسارات التشغيل، وحدود CPU/RAM قابلة للتعديل ودوران logs. الويب داخل الحاوية أصبح منفذ 8080، لكن منافذ الجهاز المحلية 8082/8083 بقيت كما هي. لا تستخدم ربطًا قديمًا إلى منفذ 80 داخل صورتي الويب.
- `env.js` يولد في `/tmp/runtime` عند كل تشغيل ويعرضه Nginx من هناك؛ الصورة نفسها قابلة لإعادة الاستخدام بين البيئات. المتغيرات المنشورة فيه بيانات متصفح عامة فقط. مخزن المرفقات هو volume دائم `/data/school-documents`؛ image تهيئ ملكيته للـAPP_UID عند إنشاء volume جديد. volume قديم لا تعاد ملكيته تلقائيًا: إن فشل health، راجع ملكية المسار والمجلدات الداخلية في صيانة بعد backup، ولا تشغل API كـroot كحل.
- استُبعدت App_Data والملفات المؤقتة والسجلات وبيانات البرامج القديمة والشهادات وappsettings.Development من build context. لا تضع أسرارًا فعلية في appsettings.json المضمن؛ dockerignore ليس فاحص أسرار.
- مفاتيح الرفع إلزامية للخدمتين من مصدر الأسرار، وأضيفت أسماؤها إلى `.env.docker.example`. Default ActiveKeyId هو primary؛ عند تدوير المفتاح أضف مفاتيح key ring القديمة والجديدة باستخدام Compose override/environment مخصص قبل تغيير ActiveKeyId. لا تغيّر معرف مفتاح ملفات قائمة أو تحذف القديم. إعدادات AllowLegacyRead مستقلة وتبقى true إلى اكتمال الترحيل والمراجعة، دون السماح برفع جديد غير مشفر.
- readiness: Platform يفحص اتصال قاعدتي Platform/Identity وصلاحية مفتاح التشفير؛ Schools يفحص قاعدة provisioning ومفتاح التشفير وكتابة probe فارغ مؤقت في التخزين، وDocker يفحص Poppler. Worker يفحص حالته واتصال القاعدتين. هذه ليست مراجعة schema أو اتصال جميع قواعد المدارس أو إثبات تسليم كل consumer؛ `/health/live` منفصل، وتجربة الرسائل والمigrations تبقى إلزامية.

### تشغيل محلي

احتفظ بملفك الفعلي ومفاتيحك؛ أضف الإعدادات الجديدة يدويًا ولا تستبدله بقالب. PowerShell من مجلد المستودع:

```powershell
./deploy/Test-DockerConfiguration.ps1 -EnvFile .env.docker
docker compose --env-file .env.docker build --pull --parallel
docker compose --env-file .env.docker up -d
```

لا تنفذ up على قواعد Platform/Identity فارغة قبل تطبيق schema كما بالقسم أدناه. مفاتيح user-secrets لا تنتقل إلى Docker. الاتصال من جهاز آخر لن يعمل على منافذ loopback عمدًا؛ استخدم gateway أو إعداد نشر واضح بدل فتح خدمات البنية التحتية للعامة.

### تشغيل الإنتاج

1. جهز ملف إعداد محمي خارج Git يجمع إعدادات القالب المحلي وإعدادات `.env.docker.production.example`. لا تنسخ placeholders كأسرار؛ ضع IMAGE_TAG لإصدار محدد والدومينين الحقيقيين بدون scheme/path/port، واختر subnet لا يتعارض مع شبكة السيرفر. لا تنفذ `config` بدون `--quiet` على ملف حقيقي لأنه يكشف القيم.
2. Gateway يحتاج دليل TLS خارجي يحتوي `fullchain.pem` و`privkey.pem` لشهادة تغطي دوميني Platform وSchools، مع DNS يشير للسيرفر. شهادات الإنتاج يجب أن تكون موثوقة وغير منتهية، وتجدد خارج الصور مع إعادة تحميل gateway بعد التجديد. الدليل يركب read-only ولا ينشأ تلقائيًا إن كان مفقودًا.
3. RabbitMQ يحتاج دليل خارجي يحتوي `ca.pem` و`cert.pem` (سلسلة السيرفر) و`key.pem`. SAN يطابق RABBITMQ_TLS_HOST، وسلسلة الشهادة موثوقة من trust store داخل صور .NET. شبكة Compose تعطي RabbitMQ alias بهذا الاسم. RABBITMQ_AMQPS_URI من مخزن الأسرار يستخدم amqps ومنفذ5671 واسم الشهادة نفسه. private CA تحتاج صورًا مشتقة تثبت الجذر الموثوق وتعيد بناء API/Worker؛ مجرد وجود ca.pem على broker لا يجعل العملاء يثقون به. لا تستخدم تعطيل تحقق الشهادة أو self-signed إنتاجية بلا trust صريح.
4. ملف شهادة broker ومفتاحه يجب أن يكونا قابلين للقراءة بواسطة مستخدم rabbitmq داخل الحاوية، وليس world-writable. تظل بيانات RabbitMQ محفوظة؛ تغيير RABBITMQ_DEFAULT_USER/PASS لا يغيّر تلقائيًا مستخدمًا موجودًا في volume. محليًا AMQP مسموح في Development فقط؛ overlay يغلق TCP plaintext ويستخدم AMQPS لجميع publishers/consumers. الإدارة لا تنشر خارجيًا.
5. Production overlay لا ينشر منافذ أي تطبيق أو DB أو broker؛ gateway فقط ينشر80/443. Platform وSchools على دومينين، والـAPI لكل منهما same-origin تحت `/api/`. `/hubs/` للـPlatform يدعم websocket. Gateway يمرر headers الموثوقة ويستبدل forwarding المرسل من الخارج؛ Platform يثق بعنوان gateway المحدد فقط، لا بجميع proxies. حد الرفع54MiB وrequest buffering مغلق؛ logs gateway لا تحفظ query strings التي قد تحمل SignalR token.
6. خذ backup متسقًا للبيانات والمفاتيح، طبّق migrations منفصلة، ثم افحص وانشر:

```powershell
./deploy/Test-DockerConfiguration.ps1 -EnvFile C:/secure/deployment.env -Production
docker compose --env-file C:/secure/deployment.env -f docker-compose.yml -f docker-compose.production.yml build --pull --parallel
docker compose --env-file C:/secure/deployment.env -f docker-compose.yml -f docker-compose.production.yml up -d
docker compose --env-file C:/secure/deployment.env -f docker-compose.yml -f docker-compose.production.yml ps
```

مسار المثال يستبدل بالمسار الحقيقي المحمي؛ على Linux يمكن استخدام pwsh للفاحص، أو config --quiet ثم فحص الشهادات/المفاتيح يدويًا. الفاحص يتحقق من تركيب الإعداد والمفاتيح والمنافذ ووجود ملفات TLS دون عرض القيم، وليس صلاحية الشهادات/trust/تفويض DB أو اختبار تحميل. لا تستخدم down -v. لا تترك ملف الأسرار في build context، ولا تضع server secrets في env.js. للمراقبة استخدم docker exec داخل الشبكة لـhealth بدل جعل endpoints داخلية عامة.

### تحقق الإصدار وحدود التحقق

اختبارات fixture في `deploy/tests/Test-ComposeFixtures.ps1` تستخدم إعدادات اصطناعية فقط. اختبارات web-smoke.sh وgateway-smoke.sh تعمل داخل حاويات disposable؛ الأخير يولد شهادة اختبار مؤقتة لا تُستخدم في الإنتاج. إعادة البناء/اختبار Nginx لا يحل محل staging حقيقي: login، provisioning، تحديث مدرسة، upload/read/Range، reader PDF، SignalR، TLS client trust، إعادة تشغيل وحفظ البيانات، backup/restore وفشل اعتماد. الحدود الافتراضية نقطة بداية تحتاج load test، ولا تكفي لضمان تحمل حجم مدرسة محدد. أي تعديل subnet يحتاج إعادة إنشاء شبكة مخططة، لا حذف volumes.

يغطي هذا الملف الصور الخمس المستقلة التالية:

| الخدمة | Dockerfile | المنفذ الافتراضي على الجهاز |
|---|---|---:|
| Platform API | `src/Platform/Mdaresna.Platform.Api/Dockerfile` | 5241 |
| Platform Web | `../Mdaresna_Platform_Web/Dockerfile` | 8082 |
| Schools API | `src/Schools/Mdaresna.Schools.Api/Dockerfile` | 5251 |
| Schools Web | `../Mdaresna_Schools_Web/Dockerfile` | 8083 |
| Platform Worker | `src/Platform/Mdaresna.Platform.Worker/Dockerfile` | 5081 (health only) |

يوجد `docker-compose.yml` موحد بجانب هذا المستودع، ويضيف PostgreSQL وRabbitMQ إلى الخدمات الخمس. كل تطبيق له image وDockerfile مستقل ويمكن بناؤه أو نشره منفردًا.

## الإعداد المحلي

نفّذ الأوامر من مجلد `Mdaresna`:

```powershell
Copy-Item .env.docker.example .env.docker
```

عدّل كل قيمة تبدأ بـ`CHANGE_ME`. لا تحفظ `.env.docker` في Git. لتوليد قيم آمنة في PowerShell:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

استخدم ناتج 48 بايت لمفاتيح JWT وInternal API key، وناتج 32 بايت في `PLATFORM_ACTIVATION_CODE_HASH_KEY`. يجب أن تظل `INTERNAL_API_KEY` واحدة في Platform API وSchools API، وأن تظل `PLATFORM_SMS_ENCRYPTION_KEY` واحدة في Platform API والـWorker.

تحقق من الملف ثم شغّل البنية التحتية أولًا:

```powershell
docker compose --env-file .env.docker config --quiet
docker compose --env-file .env.docker up -d postgres rabbitmq
```

## تطبيق Platform migrations

إنشاء PostgreSQL container ينشئ قاعدتي `mdaresna_platform` و`mdaresna_identity` الفارغتين فقط؛ لا تشغّل التطبيقات على قاعدة غير مهاجرة. من نفس المجلد، وبعد استبدال كلمة المرور والمنفذ بالقيم الفعلية في `.env.docker`:

```powershell
$env:MDARESNA_PLATFORM_POSTGRES_DESIGNTIME_CONNECTION = 'Host=localhost;Port=5432;Database=mdaresna_platform;Username=postgres;Password=<secret>'
$env:MDARESNA_IDENTITY_POSTGRES_DESIGNTIME_CONNECTION = 'Host=localhost;Port=5432;Database=mdaresna_identity;Username=postgres;Password=<secret>'

dotnet tool restore
dotnet tool run dotnet-ef database update --context PostgreSqlPlatformDbContext --project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj --startup-project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj
dotnet tool run dotnet-ef database update --context PostgreSqlIdentityDbContext --project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj --startup-project src/Platform/Mdaresna.Platform.Infrastructure/Mdaresna.Platform.Infrastructure.csproj
```

قواعد المدارس تُنشأ وتُهاجر تلقائيًا بواسطة Schools API عند provisioning. المدارس الموجودة تُحدّث من عملية migrations المركزية في Platform بعد تشغيل الـWorker وSchools API.

## البناء والتشغيل

```powershell
docker compose --env-file .env.docker build --parallel
docker compose --env-file .env.docker up -d
docker compose --env-file .env.docker ps
```

نقاط الفحص:

- Platform API: `http://localhost:5241/health/live` و`/health/ready`.
- Worker: `http://localhost:5081/health/live` و`/health/ready`.
- Schools API: `http://localhost:5251/health/live` و`/health/ready`.
- Platform Web: `http://localhost:8082/health`.
- Schools Web: `http://localhost:8083/health`.
- RabbitMQ management: `http://localhost:15672`.

إيقاف الحاويات مع الاحتفاظ بالبيانات:

```powershell
docker compose --env-file .env.docker down
```

حذف volumes يمحو قواعد البيانات ورسائل RabbitMQ، لذلك لا تستخدم `down --volumes` إلا عند طلب إعادة تهيئة محلية صريحة وبعد التأكد من عدم الحاجة للبيانات.

## Environment variables المهمة

### Platform API

- `ConnectionStrings__PlatformConnection`, `ConnectionStrings__IdentityConnection`.
- `PlatformDatabase__Provider=PostgreSql`.
- `PlatformAuth__Issuer`, `PlatformAuth__Audience`, `PlatformAuth__SigningKey`.
- `PlatformActivation__CodeHashKey`, `InternalServices__ApiKey`.
- `PlatformHost__Cors__AllowedOrigins__0` وعنوان Platform Web فقط بدون path.
- `PlatformSms__*` و`Notifications__Firebase__*` عند تفعيلهما.

### Platform Worker

- نفس اتصالات Platform ونفس `PlatformSms__EncryptionKey`.
- `SchoolRegistrationConsumer__*`, `PlatformOutboxPublisher__*`, `SchoolProvisioningResultConsumer__*`.
- `SmsLogConsumer__*` إذا كان consumer القديم مطلوبًا.

### Schools API

- `SchoolRegistrationPublisher__BrokerUri`, `SchoolProvisioningConsumer__*`.
- `SchoolProvisioningDatabase__AdminConnectionString`, `DatabaseNamePrefix`, `CredentialSecretReference`.
- `SchoolIdentity__PlatformBaseUrl`, `InternalApiKey`، و`SCHOOL_DATABASE_CREDENTIALS` المشار إليها بالمرجع `env:SCHOOL_DATABASE_CREDENTIALS`.
- `SchoolIdentity__Jwt__Issuer`, `Audience`, `SigningKey`.
- `SchoolDocuments__StorageRoot` لمسار volume دائم خارج قاعدة البيانات، و`SchoolDocuments__MaxFileSizeBytes` للحد الأقصى للملف. يستخدم Compose volume باسم `mdaresna-school-documents` ومسار `/data/school-documents`.
- `SchoolsHost__Cors__AllowedOrigins__0`.

### تطبيقات الويب

تُقرأ القيم وقت تشغيل container من `/env.js` وليست مثبتة داخل Vite bundle، ولذلك يمكن استخدام نفس image في أكثر من بيئة:

- Platform Web: `PLATFORM_API_BASE_URL` ومتغيرات `FIREBASE_*` العامة الخاصة بالويب.
- Schools Web: `SCHOOLS_API_BASE_URL`.

لا تضع أي سر في متغيرات الويب؛ كل ما داخل `/env.js` قابل للقراءة من المتصفح. لا تستخدم علامات اقتباس أو أسطرًا جديدة داخل هذه القيم لأنها تُحقن في JavaScript template.

## الإنتاج

- استخدم secret manager أو Docker/Kubernetes secrets بدل ملف env محفوظ على الخادم متى أمكن.
- ضع TLS في reverse proxy أو load balancer، واضبط عناوين الـAPI العامة وCORS على HTTPS.
- لا تنشر منافذ PostgreSQL أو RabbitMQ أو Worker health للعامة.
- عيّن `FIREBASE_CREDENTIALS_HOST_PATH` إلى ملف credentials خارج المستودع وفعّل Firebase فقط عند توفره؛ الملف الفارغ tracked مخصص فقط ليظل mount صالحًا عندما تكون الخاصية معطلة.
- شغّل migrations كخطوة إصدار منفصلة قبل تحويل traffic، وخذ backup واختبر rollback.
- راقب `/health/ready` والـqueues والـdead-letter queues، ولا تعتبر `/health/live` دليلًا على جاهزية قواعد البيانات أو consumers.
