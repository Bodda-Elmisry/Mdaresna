# Redis لكاش وجهات قواعد المدارس

## السلوك الحالي

Schools يقرأ بيانات التوجيه من Redis قبل طلب Platform. إذا لم توجد نسخة صالحة، يطلب snapshot من المنصة. الكاش يحتوي الهوية والكود وعنوان واسم قاعدة البيانات والمزود وإعداد TLS ومرجع السر فقط. كلمة المرور وconnection string المحلول لا يكتبان في الكاش، وصلاحيات المدرس تظل في قاعدة المدرسة.

عند تغيير وجهة المدرسة أو حالتها، يحفظ Platform إصدار RoutingVersion ورسالة SchoolRoutingChangedV1 في Outbox ضمن نفس SaveChanges. الحفظ يغطي مسارات Platform API والـWorker التي تستخدم PlatformDbContext، بما فيها تبديل primary والإيقاف والإغلاق والحذف. تعديل SQL مباشر أو ExecuteUpdate خارج هذا المسار لا ينتج رسالة تلقائيًا؛ لا تستخدمه للتغييرات التشغيلية دون إعادة مزامنة مدروسة.

Platform Worker ينشر الرسالة على RabbitMQ. مستهلك Schools يحدث الإدخال الكامل للمدرسة النشطة بدل حذفه، ويزيل بيانات التوجيه للمدرسة الموقوفة أو المحذوفة أو التي ليس لها operational primary نشط. الإصدار يخص المدرسة كلها؛ تغيير endpoint مختلف لا يعيد عداده للصفر.

قبل تطبيق الحدث، يقرأ المستهلك أحدث snapshot من Platform مرة لكل رسالة تغيير. هذا يمنع حدثًا قديمًا من إعادة بيانات بعد فقد Redis، ويجمع الحالات الوسيطة عند تبديل primary في معاملة واحدة. ليست هذه قراءة لكل طلب مدرس أو قراءة دورية لكل مدرسة. إذا تعطل Platform أو Redis تبقى الرسالة لإعادة المحاولة ولا يؤكد استلامها كنجاح.

## ترتيب الرسائل والتزامن

مقارنة الإصدار والكتابة تتمان ذريًا بسكربت Redis. التحديث الأقدم أو المكرر لا يستبدل الأحدث ولا يمدد TTL. التحديث الجديد يحدد صلاحية30 يومًا جديدة؛ قراءة hit لا تمددها. عند الإيقاف تحذف data ويحتفظ guard بالإصدار وعلامة المنع دون TTL. هذه ليست نسخة من بيانات المدرسة الموقوفة.

كل guard له epoch؛ فقده بإعادة التشغيل أو eviction يغير epoch ويحذف data المرتبطة به. طلب بدأ قبل الفقد لا يمكنه إعادة تعبئة الكاش من snapshot قديم؛ يعيد lookup بدلًا من ذلك. guard وdata في hash slot واحد لمواءمة Redis Cluster. مفاتيح v2 الجديدة لا تقرأ إدخالات v1 السابقة.

المنع يبدأ بعد معالجة الحدث، لا بمجرد ضغط زر الإيقاف. يجب مراقبة زمن تأخر الرسائل وOutbox وDLQ؛ RabbitMQ المتعطل لا يحقق إيقافًا لحظيًا. لا تعد استخدام كود مدرسة محذوفة لهوية جديدة دون إجراءات ترحيل للـguard؛ هوية المدرسة والكود ثابتان في هذا التصميم.

## الإعداد

- SchoolRoutingCache:Enabled: معطل افتراضيًا خارج Compose؛ مفعّل في Compose.
- SchoolRoutingCache:RedisConnection: من secret manager أو متغير نشر محمي. Redis بين السيرفرات يستخدم شبكة خاصة وTLS موثوقًا؛ لا تعطل تحقق الشهادة.
- SchoolRoutingCache:Namespace: اسم مستقل لكل منصة/بيئة. جميع نسخ Schools التي تستهلك نفس queue يجب أن تستخدم نفس Redis وnamespace وعنوان Platform المعرّف في المفاتيح. استخدم broker أو vhost منفصلًا لكل منصة/بيئة؛ queue مشتركة لا توزع الحدث على عدة Redis مستقلة.
- SchoolRoutingCache:TtlSeconds: افتراضيًا2592000 (30 يومًا ثابتًا)، والنطاق1–2592000. مدة مطلقة وليست sliding. restart/eviction قد يسببان miss مبكرًا.
- SchoolRoutingCacheConsumer:Enabled: مفعّل افتراضيًا عند تفعيل الكاش؛ قالب Compose يستخدم SCHOOL_ROUTING_CACHE_CONSUMER_ENABLED=true. preflight يرفض مدة الكاش التشغيلية مع مستهلك معطل.
- SchoolRoutingCacheConsumer:BrokerUri: من مصدر أسرار. AMQPS في الإنتاج؛ AMQP مسموح في Development فقط. Compose الإنتاج يمرر RABBITMQ_AMQPS_URI.
- REDIS_PASSWORD وكلمة مرور العميل في SCHOOL_ROUTING_REDIS_CONNECTION يجب أن يتطابقا عند استخدام Redis المرفق. القوالب لا تحتوي أسرارًا فعلية.

Redis المرفق داخلي بلا host port، بمصادقة، non-root/read-only وحد128MiB وسياسة allkeys-lru ودون persistence. عميل الطلب ينتظر500ms للكاش، مع cooldown5s عند الفشل ثم fallback للمنصة؛ المنصة بحد16 طلبًا متزامنًا لكل process وtimeout5s. تجميع miss محلي فقط، وليس lock موزعًا بين النسخ. الحدث له timeout محدود وإعادة محاولة؛ لا يبتلع فشل الكتابة.

العدادات تحت Meter باسم Mdaresna.Schools.RoutingCache تشمل hits وplatform.loads وcache.failures. logs لا تطبع بيانات اتصال أو أسرار. queue مدارس التوجيه schools.routing-cache.v1، وDLQ schools.routing-cache.v1.dead للرسائل غير الصالحة؛ راجعها يدويًا بعد إصلاح مصدرها.

## النشر

1. أخذ backup لقاعدة Platform وتجهيز TLS والأسرار، وإيقاف تغييرات سجل المدارس أثناء ترقية نسخ المنصة القديمة.
2. تطبيق AddSchoolRoutingVersion على قاعدة Platform: PostgreSQL 20261014190417 أو SQL Server 20261014190458. عمود bigint افتراضي0 وconcurrency token؛ لا migrations لقواعد المدارس لهذه الدفعة.
3. نشر Platform API وPlatform Worker الجديدين أولًا، ثم Schools API وكل نسخه. لا تترك نسخ Schools القديمة التي تقرأ v1 تعمل بالتوازي مدة طويلة. endpoint الداخلي الجديد school-routing-targets يعيد نسخة واحدة متسقة من المدرسة والوجهة؛ 404 خاص بمدرسة مفقودة، وليس 404 endpoint غير منشور.
4. تأكد أن مستهلك الكاش متصل وأن Outbox والqueue تتناقص، ثم جرّب تعديل عنوان/مرجع وجهة، تبديل primary، إيقاف/إعادة تفعيل، وتكرار الرسائل وانقطاع Redis وRabbitMQ وPlatform.
5. عند SQL maintenance خارج المسار أو rollback: تعطيل الكاش، إزالة بياناته القديمة وguards بصورة مدروسة أو اختيار namespace جديد لجميع النسخ، ثم إعادة التعبئة من المصدر. لا توجد أداة bulk bootstrap جديدة؛ المدارس النشطة تعيد تعبئة الكاش عند أول miss، والأحداث اللاحقة تصل عبر Outbox.

ملفات.env الفعلية لم تتغير، والمهاجرات لم تطبق على قاعدة فعلية. اختبارات مصدر Outbox هنا باستخدام EF InMemory؛ يلزم staging بمعاملات وتزامن حقيقيين على المزود المستخدم قبل الإنتاج.

## التحقق

أوامر: dotnet test tests/Mdaresna.Platform.UnitTests/Mdaresna.Platform.UnitTests.csproj وdotnet test tests/Mdaresna.Schools.UnitTests/Mdaresna.Schools.UnitTests.csproj، وdeploy/tests/Test-ComposeFixtures.ps1.

اختبارات Redis وRabbitMQ الفعلية اختيارية عبر MDARESNA_ROUTING_REDIS_TEST_CONNECTION وMDARESNA_ROUTING_BROKER_TEST_URI، من بيئة اختبار معزولة فقط. اختبار broker يتطلب مستخدم fixture على127.0.0.1 لأن أسماء queues ثابتة؛ لا توجهه إلى broker الإنتاج. لا تطبع قيم الاتصال.
