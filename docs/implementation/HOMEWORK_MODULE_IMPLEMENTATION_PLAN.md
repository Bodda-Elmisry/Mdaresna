# خطة تنفيذ وحدة الواجبات

آخر تحديث: 2026-09-26

حالة التنفيذ: اكتملت الدفعة المدرسية الأساسية في Domain/Persistence وSchool API وSchool Web وSchool Flutter. بقيت Family API/UI والإشعارات وتكامل Gradebook مؤجلة حسب النطاق، كما تبقى تحسينات ما بعد التشغيل مثل المرفقات والتقارير الموسعة ضمن مراحل لاحقة.

## 1. الهدف والنطاق

تنفيذ الواجبات داخل مساحة «فتح الفصل» على مستوى مادة محددة في فصل محدد، بنوعين:

- `Online`: واجب بأسئلة منظمة يحله الطالب أو ولي الأمر المصرح له من Family App، مع تصحيح آلي للاختيار والإكمال وتصحيح يدوي للإجابة النصية.
- `Offline`: واجب يظهر للطالب وولي الأمر للمشاهدة، لكن التسليم داخل المدرسة ويسجل المعلم استلامه ودرجته من School App.

الوحدة تحفظ التاريخ الكامل للنشر والمحاولات والتصحيح، ولا تصبح بديلًا عن Gradebook؛ توفر له مصدر درجات قابلًا للربط لاحقًا.

### نطاق الإصدار الحالي

- التنفيذ الحالي يقتصر على `School API` و`School Web` و`School Flutter`.
- نبني الـDomain وقاعدة البيانات من البداية بما يستوعب التسليم الأونلاين والمحاولات وقناة `FamilyApp` دون تعديل جذري لاحقًا.
- لا ننفذ حاليًا `Family API` أو واجهات `Family App` أو إشعارات أولياء الأمور.
- يكون الواجب الأوفلاين قابلًا للاستخدام الكامل من المدرسة. أما الواجب الأونلاين فتكتمل في هذه المرحلة إدارة بياناته وبناء أسئلته ونشره ومتابعته من جانب المدرسة، ويُفعّل حل الطالب وتسليمه عند بدء مرحلة Family App.

## 2. تجربة المستخدم

### 2.1 School Web وSchool Flutter

1. يفتح المستخدم شاشة «الفصول» ثم Action «فتح الفصل».
2. يختار تبويب «الواجبات».
3. يعرض النظام المواد التي يحق للمستخدم العمل عليها داخل هذا الفصل:
   - School Admin: كل مواد الفصل.
   - المعلم: مواد `ClassSectionTeacherScope` الفعالة الخاصة به في الفصل.
4. اختيار المادة يعرض قائمة واجباتها مع الحالة والنوع وموعد التسليم وعدد الطلاب وحالات التسليم والتصحيح.
5. المستخدم المصرح له يستطيع إضافة واجب، فتح التفاصيل، النشر، الإغلاق، الإلغاء، تسجيل الاستلام الأوفلاين، أو تصحيح الإجابات اليدوية.

### 2.2 Family App — مرحلة مؤجلة

- يظهر الواجب المنشور للطالب وولي الأمر الفعال المرتبط به.
- الواجب الأوفلاين يعرض التفاصيل وعبارة «التسليم داخل المدرسة» فقط.
- الواجب الأونلاين يسمح بحفظ مسودة للحل ثم التسليم، وسحب التسليم أو إعادة المحاولة وفق سياسة الواجب.
- تعرض النتيجة والتعليق والإجابات الصحيحة فقط وفق سياسة الإظهار المحددة في الواجب.

## 3. دورة حياة الواجب

### 3.1 حالة الواجب

- `Draft`: قابل لتعديل البيانات والأسئلة.
- `Published`: ظاهر للجمهور المستهدف، والأسئلة والإجابات النموذجية ثابتة.
- `Closed`: انتهى استقبال التسليمات الجديدة، ويستمر التصحيح والعرض التاريخي.
- `Cancelled`: ملغى مع الاحتفاظ بالتاريخ.

يسمح بالعودة من `Published` إلى `Draft` فقط إذا لم توجد أي محاولة طالب. لا يحذف الواجب المنشور ماديًا.

### 3.2 حالة الطالب في الواجب

- `NotSubmitted`
- `Draft`
- `Submitted`
- `Late`
- `PendingManualReview`
- `AutoGraded`
- `Graded`
- `Returned`
- `Excused`

### 3.3 مصدر العملية

- `FamilyApp`: التسليم الأونلاين.
- `SchoolApp`: تسجيل استلام الواجب الأوفلاين.

لا يقبل الـBackend المصدر كقيمة حرة؛ يشتقه من المسار وهوية العميل.

## 4. نموذج البيانات

### 4.1 `HomeworkAssignment`

- `Id`
- `ClassSectionId`
- `ClassSectionSubjectId`
- `DeliveryMode`: `Online/Offline`
- `Title`
- `Instructions`
- `CurriculumSubjectBookId?`
- `BookVersionId?`
- `BookReference?`: صفحة/وحدة/فصل من الكتاب
- `DueAtUtc`
- `SchoolDueDateLocal`
- `SchoolDueTimeLocal`
- `TimeZoneIdSnapshot`
- `AllowLateSubmission`
- `AllowMultipleAttempts`
- `MaximumAttempts`
- `AllowUnsubmitBeforeDue`
- `ShowCorrectAnswersAfter`: `Never/AfterGrading/AfterClosing`
- `ShuffleQuestions`
- `ShuffleOptions`
- `Status`: `Draft/Published/Closed/Cancelled`
- `CreatedByUserId`, `PublishedByUserId?`, `ClosedByUserId?`
- `CreatedAtUtc`, `UpdatedAtUtc`, `PublishedAtUtc?`, `ClosedAtUtc?`
- `RowVersion`

الكتاب اختياري، وعند اختياره يجب أن يكون من كتب مادة الصف في خطة المنهج المثبتة للعام.

### 4.2 `HomeworkQuestion`

- `Id`
- `HomeworkAssignmentId`
- `Type`: `SingleChoice/MultipleChoice/Text/FillInBlank`
- `Prompt`
- `SortOrder`
- `IsRequired`
- `MaxScore`
- `ModelAnswerText?`: مرجع المعلم للأسئلة النصية
- `Explanation?`: شرح يمكن إظهاره بعد التصحيح وفق السياسة
- `CreatedAtUtc`, `UpdatedAtUtc`, `RowVersion`

### 4.3 `HomeworkQuestionOption`

- `Id`
- `HomeworkQuestionId`
- `Text`
- `SortOrder`
- `IsCorrect`

قواعد الاختيار:

- `SingleChoice`: خياران على الأقل وإجابة صحيحة واحدة فقط.
- `MultipleChoice`: خياران على الأقل وإجابة صحيحة واحدة أو أكثر.
- الإصدار الأول يستخدم المطابقة الكاملة لمجموعة الاختيارات دون partial credit.

### 4.4 `HomeworkQuestionBlank`

- `Id`
- `HomeworkQuestionId`
- `Token`: معرف ثابت لموضع الفراغ داخل النص
- `SortOrder`
- `MaxScore`
- `IgnoreCase`
- `TrimWhitespace`
- `CollapseWhitespace`
- `IgnoreArabicDiacritics`
- `SendUnmatchedToManualReview`

درجة سؤال الإكمال تساوي مجموع درجات فراغاته.

### 4.5 `HomeworkBlankAcceptedAnswer`

- `Id`
- `HomeworkQuestionBlankId`
- `AnswerText`
- `NormalizedAnswer`
- `SortOrder`

يمكن للمعلم إضافة عدة صيغ صحيحة. لا يستخدم fuzzy matching، ولا تطبيع يغير المعنى مثل تحويل الهمزات أو التاء المربوطة تلقائيًا.

### 4.6 `StudentHomework`

يمثل جمهور الواجب والملخص الحالي لكل طالب:

- `Id`
- `HomeworkAssignmentId`
- `StudentEnrollmentId`
- `CurrentStatus`
- `CurrentAttemptId?`
- `FinalScore?`
- `IsExcused`
- `ExcuseReason?`
- `GradedByUserId?`
- `GradedAtUtc?`
- `TeacherFeedback?`
- `CreatedAtUtc`, `UpdatedAtUtc`, `RowVersion`

Unique index على `(HomeworkAssignmentId, StudentEnrollmentId)`.

عند النشر ينشئ النظام هذه السجلات لكل القيود الفعالة في الفصل كـsnapshot. الطالب المنضم لاحقًا لا يضاف تلقائيًا؛ يتوفر Action مدقق «إضافة الطلاب الجدد».

### 4.7 `HomeworkSubmissionAttempt`

- `Id`
- `StudentHomeworkId`
- `AttemptNumber`
- `Channel`: مشتقة `FamilyApp/SchoolApp`
- `Status`: `Draft/Submitted/Late/Unsubmitted/PendingManualReview/AutoGraded/Graded/Returned`
- `SubmittedByStudentId?`
- `SubmittedByGuardianId?`
- `RecordedByLocalUserId?`
- `StartedAtUtc`, `SubmittedAtUtc?`, `ReturnedAtUtc?`
- `AutoScore?`, `ManualScore?`, `TotalScore?`
- `RowVersion`

كل إعادة تسليم تنشئ محاولة جديدة ولا تكتب فوق السابقة.

### 4.8 `HomeworkStudentAnswer`

- `Id`
- `HomeworkSubmissionAttemptId`
- `HomeworkQuestionId`
- `TextAnswer?`
- `GradingStatus`: `Pending/AutoGraded/ManuallyGraded/Overridden`
- `AwardedScore?`
- `TeacherFeedback?`
- `GradedByUserId?`, `GradedAtUtc?`
- `RowVersion`

Unique index على `(HomeworkSubmissionAttemptId, HomeworkQuestionId)`.

### 4.9 `HomeworkStudentSelectedOption`

- `HomeworkStudentAnswerId`
- `HomeworkQuestionOptionId`

مفتاح مركب يمنع تكرار الاختيار.

### 4.10 `HomeworkStudentBlankAnswer`

- `Id`
- `HomeworkStudentAnswerId`
- `HomeworkQuestionBlankId`
- `AnswerText`
- `NormalizedAnswer`
- `IsAutoMatched`
- `MatchedAcceptedAnswerId?`
- `AwardedScore?`
- `RequiresManualReview`
- `ManuallyOverriddenByUserId?`

### 4.11 التدقيق والأحداث

- `HomeworkAudit`: نشر، فك نشر، إغلاق، إلغاء، إضافة طالب، إعفاء، تعديل درجة، وإرجاع محاولة.
- Integration events:
  - `HomeworkPublished`
  - `HomeworkSubmissionReceived`
  - `HomeworkReturned`
  - `HomeworkGraded`

## 5. خوارزمية التصحيح

### 5.1 اختيار واحد

- الدرجة كاملة إذا كان الخيار المحدد هو الصحيح الوحيد.
- صفر في غير ذلك.

### 5.2 اختيار متعدد

- تقارن مجموعة اختيارات الطالب بمجموعة الاختيارات الصحيحة exact set.
- الدرجة كاملة عند التطابق، وصفر عند وجود اختيار ناقص أو زائد.
- partial credit يؤجل لإعداد مستقل مستقبلًا.

### 5.3 إجابة نصية

- لا تصحح آليًا.
- تحفظ الإجابة وتصبح `PendingManualReview`.
- يعرض المعلم الإجابة النموذجية بجانب إجابة الطالب ويحدد الدرجة والتعليق.

### 5.4 إكمال الجملة

لكل فراغ:

1. يطبق normalization المصرح به فقط.
2. يقارن exact بأي إجابة مقبولة.
3. يمنح درجة الفراغ عند التطابق.
4. عند عدم التطابق إما يمنح صفرًا أو يرسل للمراجعة اليدوية حسب إعداد الفراغ.

### 5.5 نتيجة المحاولة

- `AutoScore`: مجموع الأسئلة والفراغات المصححة آليًا.
- إذا لم توجد عناصر يدوية تصبح المحاولة `AutoGraded`.
- إذا وجدت إجابات نصية أو فراغات تحتاج مراجعة تصبح `PendingManualReview`.
- بعد إنهاء المعلم للمراجعة يحسب `TotalScore` من درجات جميع الأسئلة وتصبح `Graded/Returned`.

## 6. الـAPIs

### 6.1 School API

- `GET /api/schools/v1/class-workspace/{classId}/homework/subjects`
- `GET /api/schools/v1/class-workspace/{classId}/homework?classSectionSubjectId=...`
- `POST /api/schools/v1/class-workspace/{classId}/homework`
- `GET /api/schools/v1/homework/{id}`: Teacher DTO كاملة بالإجابات النموذجية.
- `PUT /api/schools/v1/homework/{id}`: حفظ aggregate الواجب والأسئلة في Draft.
- `POST /api/schools/v1/homework/{id}/publish`
- `POST /api/schools/v1/homework/{id}/unpublish`
- `POST /api/schools/v1/homework/{id}/close`
- `POST /api/schools/v1/homework/{id}/cancel`
- `POST /api/schools/v1/homework/{id}/students/sync-new`
- `GET /api/schools/v1/homework/{id}/students`
- `POST /api/schools/v1/homework/{id}/students/{studentHomeworkId}/receive-offline`
- `PUT /api/schools/v1/homework/attempts/{attemptId}/answers/{answerId}/grade`
- `POST /api/schools/v1/homework/attempts/{attemptId}/return`
- `POST /api/schools/v1/homework/{id}/students/{studentHomeworkId}/excuse`

### 6.2 Family API — تصميم محفوظ وغير منفذ في المرحلة الحالية

- `GET /api/family/v1/students/{studentId}/homework`
- `GET /api/family/v1/students/{studentId}/homework/{id}`: Student DTO بلا إجابات صحيحة.
- `POST /api/family/v1/students/{studentId}/homework/{id}/attempts`
- `PUT /api/family/v1/homework/attempts/{attemptId}/draft`
- `POST /api/family/v1/homework/attempts/{attemptId}/submit`
- `POST /api/family/v1/homework/attempts/{attemptId}/unsubmit`
- `GET /api/family/v1/homework/attempts/{attemptId}/result`

كل Family request يتحقق من أن المستخدم طالب الملف نفسه أو ولي أمر فعال مرتبط به، وأن الواجب منشور وموجه لقيده داخل المدرسة.

## 7. DTOs والأمان

- Teacher DTO تشمل `IsCorrect`, الإجابات المقبولة، والإجابة النصية النموذجية.
- Student DTO لا تشمل أيًا منها قبل تحقق `ShowCorrectAnswersAfter`.
- لا يعتمد الإخفاء على الواجهة؛ توجد projections منفصلة في الـBackend.
- لا يقبل الـBackend `StudentEnrollmentId`, `Channel`, `SubmittedBy*` من العميل دون حلها من الهوية والسياق.
- تستخدم `RowVersion` وidempotency key في submit/publish لمنع التكرار بالضغط المزدوج.
- جميع قرارات الموعد والتأخير تعتمد `SchoolClock` والمنطقة الزمنية المحفوظة في الواجب.

## 8. الصلاحيات

- `school.homework.view`
- `school.homework.manage`
- `school.homework.publish`
- `school.homework.grade`
- `school.homework.override` للتعديل المدقق للدرجة أو الحالة بعد الاعتماد

School Admin يحصل عليها من seed. المعلم يحتاج الصلاحية بالإضافة إلى نطاق المادة داخل الفصل؛ الصلاحية وحدها لا توسع نطاق البيانات.

## 9. قواعد التحقق

- المادة يجب أن تكون فعالة داخل الفصل.
- المدرس يجب أن يكون ضمن نطاق المادة في الفصل.
- موعد التسليم داخل العام الدراسي وباستخدام Time Zone المدرسة.
- `Online` يجب أن يحتوي سؤالًا واحدًا على الأقل.
- `Offline` لا يحتاج Question Builder.
- مجموع درجات الأسئلة هو إجمالي الواجب.
- درجة سؤال الإكمال تساوي مجموع درجات فراغاته.
- لا تقبل درجة سالبة أو أعلى من الحد.
- لا ينشر سؤال اختيار بلا خيارات صحيحة أو سؤال إكمال بلا إجابة مقبولة.
- لا تعدل بنية الواجب بعد النشر، ولا يفك النشر عند وجود محاولة.
- لا تتجاوز المحاولات `MaximumAttempts`.
- لا يقبل التسليم بعد الموعد إن كان `AllowLateSubmission=false`.
- ولي الأمر لا يستطيع التسليم لطالب غير مرتبط به.

## 10. واجهة بناء الواجب

نافذة أو صفحة إضافة الواجب تتكون من خطوات:

1. البيانات الأساسية: النوع، العنوان، التعليمات، موعد التسليم، الكتاب والمرجع.
2. سياسة الحل: المحاولات، التأخير، سحب التسليم، إظهار الإجابات، الخلط.
3. الأسئلة للأونلاين:
   - إضافة سؤال واختيار نوعه.
   - سحب وإفلات للترتيب أو أزرار تحريك متاحة للوصول.
   - محرر اختيارات وتحديد الصحيح.
   - إدراج فراغات داخل جملة وإعداد كل فراغ.
   - إجابة نموذجية للأسئلة النصية.
4. المعاينة كما سيظهر في Family App.
5. حفظ مسودة أو نشر.

## 11. قائمة الواجبات ولوحة المتابعة

كل واجب يعرض:

- العنوان والمادة والنوع والحالة والموعد.
- منشئ الواجب ووقت النشر.
- إجمالي الطلاب.
- لم يبدأ، مسودة، مسلم، متأخر، يحتاج تصحيحًا، تم تقييمه، معفى.
- متوسط الدرجة بعد اكتمال التصحيح.

صفحة التفاصيل توفر فلاتر بالحالة والاسم والكود، وAction فتح محاولة الطالب والتصحيح أو تسجيل استلام الأوفلاين.

## 12. الإشعارات

- عند النشر: إشعار للطالب وأوليائه.
- قبل الموعد: reminder configurable لاحقًا، مع تصميم event من البداية.
- عند الإرجاع أو التقييم: إشعار بالنتيجة والتعليق.
- عند الإلغاء أو تغيير الموعد: إشعار واضح مع Audit.

## 13. قاعدة البيانات والمigrations

- إضافة الكيانات والفهارس والقيود لمزودي PostgreSQL وSQL Server.
- لا تغييرات في Platform database؛ البيانات تشغيلية داخل قاعدة المدرسة.
- تضاف permissions كـseed وتمنح لـSchool Admin.
- المدارس القائمة تستقبل التحديث من مسار migrations المركزي.

## 14. مراحل التنفيذ

### المرحلة 1 — Domain وPersistence

- enums والكيانات والعلاقات والفهارس والتدقيق.
- خدمة normalization وتصحيح الأسئلة الآلية.
- migrations للمزودين.

### المرحلة 2 — School API

- النطاق حسب الفصل والمادة.
- CRUD المسودة والنشر والطلاب والتصحيح والأوفلاين.
- Teacher DTO منفصلة وآمنة.

### المرحلة 3 — School Web

- مواد المعلم، قائمة الواجبات، Question Builder، النشر، متابعة الطلاب والتصحيح.

### المرحلة 4 — School Flutter

- نفس دورة School Web بتصميم مناسب للموبايل والتابلت.

### المرحلة 5 — Family API

- مرحلة مؤجلة إلى حين بدء العمل على Family App.
- تشمل القراءة الآمنة، المسودة، التسليم، السحب، إعادة المحاولة والنتيجة اعتمادًا على النموذج المحفوظ من المرحلة الأولى.

### المرحلة 6 — Family App UI

- مرحلة مؤجلة، وتشمل قائمة الواجبات، حل الأسئلة inline، الحفظ التلقائي للمسودة، التسليم والنتيجة.

### المرحلة 7 — الإشعارات والتقارير

- أحداث النشر والتسليم والنتيجة.
- لوحات الحالة والمتوسطات والتأخير.

### المرحلة 8 — تكامل Gradebook

- يقرأ Gradebook الدرجات المعتمدة بمصدر `Homework` ومرجع ثابت.
- لا ينقل ملكية محاولات الواجب أو تصحيحها إلى Gradebook.

## 15. الاختبارات

### Unit tests

- Single choice وmultiple choice exact set.
- normalization لكل إعداد ورفض fuzzy matching.
- Fill-in-blank متعدد الفراغات والدرجة الجزئية.
- انتقال الحالات والمحاولات والتأخير.
- مجموع الدرجات وعدم تجاوز الحد.

### Integration tests

- uniqueness للطالب داخل الواجب والمحاولة والإجابة.
- snapshot الطلاب عند النشر.
- منع تعديل الأسئلة بعد النشر وفك النشر بعد المحاولة.
- نطاق المعلم حسب المادة والفصل.
- منع كشف الإجابات الصحيحة في Family DTO.
- تحقق ولي الأمر وعزل المدارس.
- School Time Zone وحدود موعد التسليم.
- idempotency للنشر والتسليم.

### UI tests

- بناء كل نوع سؤال ومعاينته.
- الحفظ كمسودة والنشر ورسائل validation المترجمة.
- تصحيح النص والإكمال غير المطابق وإرجاع النتيجة.
- يؤجل حل الطالب والحفظ التلقائي والتسليم إلى مرحلة Family App.

## 16. معايير القبول

- يرى المعلم داخل الفصل مواده فقط ويستطيع إدارة واجباتها وفق صلاحياته.
- ينشر واجب Online بأسئلة اختيار ونص وإكمال دون كشف الحل للطالب.
- ينشئ النشر snapshot لكل طلاب الفصل الفعالين.
- يحتفظ النموذج بكل ما يلزم للمحاولات والتأخير بتوقيت المدرسة دون تنفيذ Family API حاليًا.
- تصحح الاختيارات والإكمال آليًا، وتظهر النصوص وغير المطابق المطلوب للمراجعة في قائمة المعلم.
- لا تعتمد النتيجة قبل إنهاء جميع العناصر اليدوية.
- يستطيع المعلم تسجيل استلام وتقييم الواجب Offline من School App.
- تحفظ كل محاولة تاريخيًا وتمنع الكتابة فوقها.
- تعرض النتيجة والإجابات الصحيحة حسب سياسة الواجب فقط.
- تعمل إدارة الواجبات في School Web وSchool Flutter مع عزل المدرسة والصلاحيات والتوطين العربي والإنجليزي.
- يصبح تشغيل Family channel معيار قبول لمرحلة Family App المؤجلة، وليس للإصدار المدرسي الحالي.

## 17. الإعدادات الافتراضية المقترحة

- `MaximumAttempts = 1`
- `AllowLateSubmission = true` مع وسم Late
- `AllowUnsubmitBeforeDue = false`
- `ShowCorrectAnswersAfter = AfterClosing`
- `ShuffleQuestions = false`
- `ShuffleOptions = false`
- Multiple choice بدون partial credit
- Fill-in-blank: trim وcollapse whitespace وتجاهل case والتشكيل، دون fuzzy matching
- الإجابة غير المطابقة في Fill-in-blank تذهب للمراجعة اليدوية افتراضيًا
