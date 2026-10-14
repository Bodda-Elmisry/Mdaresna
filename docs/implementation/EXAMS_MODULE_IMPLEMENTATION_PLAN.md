# الخطة التقنية لتنفيذ وحدة الاختبارات

آخر تحديث: 2026-09-29

حالة الخطة: مقترحة وتنتظر اعتماد القرارات الواردة في القسم 23، ولم يبدأ تنفيذها بعد.

## 1. الهدف والنطاق

بناء وحدة اختبارات موحدة تدعم من البداية:

- الاختبارات الأسبوعية والشهرية و`MidTerm` ونهاية الترم ونهاية العام.
- الاختبارات التي تنفذ داخل الفصل، والاختبارات الرسمية التي تنفذ من خلال لجان وأرقام جلوس.
- جهة إصدار مستقلة عن جهة تحديد الموعد: الإدارة التعليمية أو المدرسة أو القسم أو مدرس المادة.
- اختبار مادة لفصل واحد، أو عدة فصول، أو صف كامل، ودورة اختبارات متعددة المواد والصفوف.
- Snapshot للطلاب عند النشر، وحضور الاختبار، والدرجات، واعتماد النتائج وإعادة فتحها بصورة مدققة.
- كشف تعارض الطلاب والفصول والغرف واللجان والمراقبين والجداول والإجازات.
- تشغيل مركزي من شاشة «الاختبارات»، وتشغيل سياقي للمعلم من تاب الاختبارات داخل الفصل.

الوحدة لا تفترض أن نوعًا معينًا يساوي طريقة تنفيذ أو سلطة بعينها. هذه أبعاد مستقلة تحكمها `ExamPolicy` قابلة للتهيئة.

### 1.1 نطاق الإصدار المدرسي الأول

- `Schools Domain/Persistence/API`.
- `Schools Web` للإدارة المركزية والتشغيل الكامل.
- `Schools Flutter` لعرض اختبارات المستخدم، والاختبار السريع داخل الفصل، والحضور والدرجات والتشغيل الميداني.
- تخزين بيانات المصدر الخارجي يدويًا داخل قاعدة المدرسة مع قفل الحقول الخاضعة للإدارة التعليمية.

الإطلاق الأول بعد Phases 1–3 يغطي نمط `InClass`. تصميم اللجان كامل في النموذج النهائي، لكن جداولها وواجهاتها التشغيلية تفعل في Phase 4 بصورة additive حتى لا تؤخر تشغيل الاختبارات البسيطة.

### 1.2 خارج النطاق الأول

- بنك أسئلة واختبار Online للطالب.
- `Family API/App` وإظهار الجداول والنتائج للأسرة.
- Gradebook شامل وحساب نتيجة المادة أو الترتيب العام والشهادات.
- منصة مركزية فعلية للإدارة التعليمية ترسل الاختبار إلى عدة مدارس.
- اعتراضات النتائج والتظلمات ومسار الكنترول المتقدم.

تبقى حدود التكامل موجودة من البداية حتى تضاف هذه الوحدات دون إعادة تصميم بيانات الاختبار الأساسية.

## 2. مراجعة الوضع الحالي

- الموجود حاليًا هو `SchoolCalendarEventType.Exam` فقط، وهو حدث تقويم بتاريخ بداية ونهاية وليس سجل اختبار تشغيليًا.
- تاب الاختبارات داخل مساحة الفصل في Web وFlutter ما زالت Placeholder.
- لا توجد كيانات أو APIs أو permissions للامتحانات أو اللجان أو أرقام الجلوس أو النتائج.
- يعاد استخدام `ProgramAcademicYear`, `AcademicTerm`, `AcademicPeriod`, `GradeOffering`, `GradeSubjectOffering`, `ClassSection`, `ClassSectionSubject`, `StudentEnrollment`, `SchoolRoom`, `SchoolClock` ونطاقات المعلمين والأقسام.
- `SchoolCalendarEvent` يصبح Projection للجدول المنشور، ولا يكون مصدر الحقيقة لوحدة الاختبارات.

## 3. القرارات المعمارية

### 3.1 الأبعاد المستقلة

| البعد | القيم الأساسية |
| --- | --- |
| `ExamKind` | `Weekly`, `Monthly`, `MidTerm`, `TermFinal`, `YearFinal`, `Other` |
| `ExamAuthorityType` | `EducationalAuthority`, `School`, `Department`, `Teacher` |
| `ExamAuthorityResponsibility` | `Issuing`, `Scheduling`, `Content`, `Grading`, `ResultApproval` |
| `ExamAdministrationMode` | `InClass`, `Committee` |
| `ExamFormat` | `Written`, `Oral`, `Practical`, `Online`, `Mixed` |
| `ExamSittingPurpose` | `Regular`, `MakeUp`, `Retake`, `Special` |

- لا يحدد `ExamKind` طريقة التنظيم أو جهة الجدولة تلقائيًا.
- القيم الافتراضية تأتي من policy، لكنها تنسخ إلى دورة الاختبار عند الاعتماد حتى لا يغير تعديل الإعدادات التاريخ.
- اختبار الإعادة أو الدور الثاني يمثل Sitting أو Series مرتبطة بالأصل، ولا يمحو نتيجة المحاولة السابقة.

### 3.2 فصل مسارات الحالة

لا تستخدم حالة واحدة تجمع الجدولة والتشغيل والنتائج؛ لأن ذلك ينتج عددًا كبيرًا من التركيبات غير الواضحة.

- `ExamSeriesStatus`: `Draft`, `PendingApproval`, `Approved`, `Published`, `Closed`, `Cancelled`.
- `ExamScheduleWindowStatus`: `Draft`, `Scheduled`, `Postponed`, `Cancelled`؛ وهو يملك الموعد وإعادة الجدولة والتأجيل والإلغاء المشترك.
- `ExamSittingExecutionStatus`: `NotStarted`, `InProgress`, `Completed`, `Cancelled`؛ وهو يملك التشغيل الفعلي للورقة فقط.
- `ExamResultsStatus`: `NotStarted`, `EntryOpen`, `PendingApproval`, `Approved`, `Published`.

تعرض الواجهة حالة مركبة للمستخدم، بينما يحتفظ الـDomain بكل مسار بصورة مستقلة. حالتا التشغيل والتصحيح على مستوى الدورة مشتقتان من نوافذ الجدول والجلسات وحالات نتائج الأوراق، ولا تخزنان مرة أخرى داخل `ExamSeries` حتى لا تتناقض الحالات.

إعادة فتح نتيجة منشورة ليست status دائمة: الأمر `Reopen` ينشئ revision مدققًا ثم يعيد الورقة إلى `EntryOpen` لتسلك دورة الإرسال والاعتماد والنشر مرة أخرى. إلغاء Sitting منفردة لا يلغي النافذة أو بقية الأوراق، بينما تأجيل أو إلغاء النافذة يوقف كل sittings التابعة لها.

#### جدول التحولات الأساسي

| المسار | من | الأمر | إلى |
| --- | --- | --- | --- |
| Series | `Draft` | Submit | `PendingApproval` أو `Approved` حسب policy |
| Series | `PendingApproval` | Reject بسبب إلزامي | `Draft` مع revision جديد |
| Series | `PendingApproval` | Approve | `Approved` |
| Series | `Approved` | Publish | `Published` |
| Series | `Published` | Close | `Closed` |
| Series | أي حالة غير مغلقة | Cancel بسبب إلزامي | `Cancelled` |
| Window | `Draft` | Schedule | `Scheduled` |
| Window | `Scheduled` | Postpone | `Postponed` |
| Window | `Scheduled/Postponed` | Reschedule | القديمة `Postponed` ونافذة جديدة `Scheduled` مرتبطة بها |
| Window | `Postponed` | Restore | `Scheduled` إذا لم تنشأ نافذة بديلة |
| Window | غير ملغاة | Cancel | `Cancelled` |
| Sitting | `NotStarted` | Start | `InProgress` |
| Sitting | `InProgress` | Complete | `Completed` |
| Sitting | `NotStarted/InProgress` | Cancel | `Cancelled` |
| Results | `NotStarted/EntryOpen` | Submit | `PendingApproval` أو `Approved` حسب policy |
| Results | `PendingApproval` | Reject | `EntryOpen` |
| Results | `PendingApproval` | Approve | `Approved` |
| Results | `Approved` | Publish | `Published` |
| Results | `Published` | Reopen بسبب إلزامي | `EntryOpen` مع زيادة `ResultRevision` |

كل transition ينفذ في Domain service واحدة، يراجع الصلاحية والـscope والـpolicy والـrow version، ويسجل audit. لا يسمح بتعديل status مباشرة من CRUD DTO.

### 3.3 عزل المدارس والمصدر الخارجي

- كل البيانات التشغيلية تحفظ في School Database ولا توجد FK بين قواعد المدارس أو إلى Platform Database.
- في المرحلة الحالية يحفظ الاختبار الخارجي `ExternalSourceCode`, `ExternalAuthorityName`, `ExternalReferenceId`, `ExternalRevision`, `ExternalPayloadHash` ونسخة من البيانات المستلمة.
- حقول الجهة الخارجية المقفلة لا تعدل محليًا؛ أي استثناء يسجل Revision مستقلًا بسبب وموافق.
- يوجد unique filtered index على `(ExternalSourceCode, ExternalReferenceId, ExternalRevision)` حتى يكون الإدخال اليدوي والتكامل المستقبلي idempotent من البداية.
- مستقبلًا تنشر وحدة Authority/Control-plane event إلى كل مدرسة، وتحفظ المدرسة Projection محلية idempotent باستخدام نفس المفتاح الخارجي.
- لا يعاد استخدام صلاحيات Platform operators كبديل لموظفي الإدارة التعليمية.

### 3.4 مصدر الحقيقة والتقويم

- `ExamSeries/ExamSitting` هما مصدر الحقيقة للمواعيد.
- عند النشر ينشئ النظام أو يحدث `SchoolCalendarEvent` من النوع `Exam` لكل `ExamScheduleWindow`، ويحفظ الربط في `ExamCalendarProjection`.
- الحدث المولد آليًا يعرض في التقويم ولا يعدل يدويًا.
- أحداث التقويم اليدوية القديمة لا تحول تلقائيًا إلى اختبارات؛ تظل أحداثًا مستقلة ويمكن توفير Action تحويل لاحقًا.

### 3.5 ربط الاختبارات الختامية بالتيرم

- `AcademicTermId` مطلوب للاختبار الأساسي من نوع `TermFinal` أو `YearFinal`.
- `TermFinal` يحجز خانة أساسية واحدة لكل تيرم غير أخير في العام والنطاق الدراسي، ولا يقبل الترم الأخير.
- `YearFinal` يحجز خانة أساسية واحدة فقط لكل عام ونطاق، ويرتبط تلقائيًا بآخر تيرم مرتب حسب تكوين العام.
- إذا كان العام يحتوي على `N` تيرمات، فالخانات الأساسية هي `N - 1` لاختبارات `TermFinal` وخانة واحدة لـ`YearFinal`.
- يطبق التفرد على هوية الخانة `(ProgramAcademicYear, AcademicTerm, Scope, BaseExamKind)` ولا يعتمد على عدّ السجلات. المسودة الملغاة لا تحجز الخانة، أما المنشور أو المعتمد فيحافظ عليها وفق سياسة الإلغاء والاستبدال المدققة.
- `MakeUp`, `Retake` و`Replacement` دورات مرتبطة بالاختبار الأساسي ولا تنافس تفرده. لا يستخدم إنشاء اختبار أساسي مكرر كحل للدور الثاني أو الغياب بعذر.
- في Web وFlutter يعرض `TermFinal` التيرمات غير الأخيرة المتاحة فقط، ويعرض سبب تعطيل الخانة المحجوزة. يختار `YearFinal` آخر تيرم تلقائيًا ويعرضه للقراءة فقط.

### 3.6 التسمية المولدة

- لا يظهر حقل اسم يدوي للاختبار الأساسي أو ورقة المادة؛ الاسم مشتق من `ExamKind`, الشهر أو الترم، واسم المادة المحلي.
- قوالب العربية الأساسية: `اختبار شهر {الشهر} — مادة {المادة}`، `اختبار نهاية {الترم} — مادة {المادة}`، و`اختبار نهاية العام — مادة {المادة}`، مع قوالب إنجليزية مقابلة.
- اسم السلسلة يلخص الفترة والنطاق، بينما تحفظ بيانات نوع التعليم والمرحلة والصف والفصل كحقول وفلاتر مستقلة وتعرض في سطر السياق.
- تتجدد الأسماء أثناء المسودة إذا تغيرت المدخلات، وتثبت snapshot عند النشر لأغراض التاريخ والتقارير. لا يستخدم النص المولد لمنع التكرار؛ تعتمد القيود على المفاتيح المنظمة.

### 3.7 مداخل الإنشاء ونطاق الاختبار الشهري

تقدم شاشة الاختبارات ثلاثة Actions رئيسية بدل محرر عام يبدأ من نوع خام:

1. `إضافة اختبار شهري`: يطلب نوع التعليم والتيرم و`YearMonth` ومستوى التطبيق ثم النطاق والمواد والمواعيد.
2. `إضافة اختبار تيرم/نهاية عام`: يطلب نوع التعليم والتيرم والمرحلة والصف، ويشتق النوع النهائي من ترتيب الترم دون اختيار الفصل.
3. `اختبارات إضافية`: يفتح أنواع `Weekly`, `MidTerm` و`Other`، ويطبق لكل نوع حقوله وسياساته المناسبة.

نطاق الشهري صريح عبر `ExamScopeLevel = Stage | Grade | ClassSection`:

- `Stage`: يختار نوع التعليم والمرحلة، ثم يبني النظام أهداف المواد لكل الصفوف والفصول الداخلة في المرحلة.
- `Grade`: يختار المرحلة والصف، ثم يستهدف فصول الصف الفعالة.
- `ClassSection`: يختار المرحلة والصف والفصل، ثم يستهدف الفصل وحده.
- لا يمثل ترك selector فارغًا نطاقًا؛ مستوى التطبيق هو الذي يحدد الحقول المطلوبة والمعطلة.
- يحفظ الشهر بصيغة سنة/شهر لا رقم شهر منفردًا، ويجب أن يتقاطع مع `AcademicTerm` المختار حسب تقويم المدرسة.
- يكشف منع التكرار تقاطع الأهداف الفعلية `(Year, Term, YearMonth, Subject, ClassSection)` حتى لا يسمح لاختبار مرحلة واختبار صف أو فصل بتغطية الطالب والمادة نفسيهما في الخانة الأساسية.
- تثبت الصفوف والفصول والمواد والطلاب المستهدفون عند النشر، ولا تؤثر إضافات لاحقة إلى النطاق في الاختبار المنشور دون أمر مزامنة مدقق إذا سمحت السياسة بذلك.

## 4. نموذج السياسات

### 4.1 `ExamPolicy`

يحدد default على المدرسة ويمكن تخصيصه حسب نوع التعليم أو المرحلة أو `ExamKind` مع تطبيق الأكثر تحديدًا:

- `Id`
- `EducationProgramId?`
- `EducationStageId?`
- `ExamKind?`
- `DefaultAdministrationMode`
- `TeacherCanCreate`
- `TeacherCanPublishWithoutApproval`
- `DepartmentApprovalRequired`
- `SchoolApprovalRequired`
- `ResultApprovalRequired`
- `PreventTeacherInvigilatingOwnSubject`
- `SeatNumberScope`: الإصدار الأول `ExamSeries`
- `AllowMixedGradesInCommittee`
- `AllowScheduleWarningOverride`
- `EffectiveFrom`, `EffectiveTo?`
- `Version`, `IsActive`
- audit fields و`RowVersion`

### 4.2 `ExamPolicyApprovalStep`

- `Id`, `ExamPolicyId`
- `Stage`: `Schedule`, `Publish`, `Results`
- `SortOrder`
- `ApproverScope`: `School`, `Department`, `SubjectCoordinator`, `SpecificUser`
- `SpecificUserId?`
- `IsRequired`

عند إنشاء الدورة يحفظ `PolicyId`, `PolicyVersion` و`PolicySnapshotJson`، بينما تحفظ القيم التشغيلية المؤثرة أيضًا كأعمدة صريحة داخل الدورة.

## 5. نموذج البيانات

### 5.1 `ExamSeries`

يمثل دورة الاختبار الإدارية، سواء كانت اختبارًا أسبوعيًا بسيطًا أو دورة نهاية ترم متعددة المواد:

- `Id`
- `ProgramAcademicYearId`
- `AcademicTermId?`
- `AcademicPeriodId?`
- `ParentExamSeriesId?`
- `RelationshipToParent?`: `MakeUp/Retake/Replacement`
- `Code`, `NameAr`, `NameEn`
- `Kind`
- `DefaultAdministrationMode`
- `Status`
- `PolicyId?`, `PolicyVersion`, `PolicySnapshotJson`
- `TimeZoneIdSnapshot`
- `ExternalSourceCode?`, `ExternalAuthorityName?`, `ExternalReferenceId?`, `ExternalRevision?`
- `ExternalPayloadHash?`, `ExternalPayloadJson?`
- `IsExternalScheduleLocked`
- `CreatedByUserId`, `ApprovedByUserId?`, `PublishedByUserId?`, `ClosedByUserId?`, `CancelledByUserId?`
- timestamps، soft delete للمسودة فقط، و`RowVersion`

قواعد أساسية:

- `Code` فريد داخل العام الدراسي للبرنامج.
- المفتاح الخارجي `(ExternalSourceCode, ExternalReferenceId, ExternalRevision)` فريد عندما تكون قيمه موجودة.
- لا تحذف دورة بعد النشر؛ تستخدم `Cancelled` مع السبب.
- لا تعدل البيانات المؤثرة بعد النشر مباشرة؛ تستخدم Revision.
- `AcademicTermId/PeriodId` يجب أن يتبعا نفس `ProgramAcademicYearId`.

### 5.2 `ExamSeriesAuthority`

يفصل سلطتي الإصدار والجدولة الافتراضيتين عن `CreatedBy`:

- `Id`, `ExamSeriesId`
- `Responsibility`: `Issuing`, `Scheduling`
- `AuthorityType`
- `DepartmentId?`
- `UserId?`
- `ExternalAuthorityName?`, `ExternalReferenceId?`
- `IsFieldOwner`, `IsLocked`
- audit fields

Unique index على `(ExamSeriesId, Responsibility)`. يتحقق الـBackend من الحقول المطلوبة حسب `AuthorityType`. مسؤوليات المحتوى والتصحيح واعتماد النتائج توضع على كل Paper لأن الدورة متعددة المواد قد تضم أقسامًا ومدرسين مختلفين.

### 5.3 `ExamSeriesTarget`

يعرف النطاق المطلوب قبل النشر:

- `Id`, `ExamSeriesId`
- `GradeOfferingId`
- `ClassSectionId?`
- `IncludeFutureActiveSectionsBeforePublish`
- audit fields

`ClassSectionId=null` يعني كل الفصول الفعالة التابعة للصف عند تكوين Snapshot. يمنع الجمع بين صف كامل وفصول جزئية متداخلة دون تطبيع.

### 5.4 `ExamPaper`

يمثل اختبار مادة أو مكون تقييم واحدًا داخل الدورة:

- `Id`, `ExamSeriesId`
- `GradeSubjectOfferingId`
- `PaperCode`, `TitleAr`, `TitleEn`
- `Format`
- `Instructions?`
- `TotalScore`, `PassScore?`
- `DurationMinutes`
- `SortOrder`
- `ContentOwnerUserId?`
- `ResultsStatus`
- `ResultsRevision`
- `SubjectCodeSnapshot?`, `SubjectNameArSnapshot?`, `SubjectNameEnSnapshot?`, `GradeNameArSnapshot?`, `GradeNameEnSnapshot?` وتملأ عند النشر.
- `IsReady`
- audit fields و`RowVersion`

يسمح بأكثر من Paper لنفس المادة عند وجود تحريري وشفهي أو عملي، ويكون `PaperCode` فريدًا داخل الدورة.

### 5.5 `ExamPaperTarget`

- `Id`, `ExamPaperId`
- `ClassSectionId?`
- `ClassSectionSubjectId?`
- `GradeOfferingId`

يجب أن ينتمي الفصل والمادة إلى `GradeSubjectOffering` الخاصة بالـPaper وإلى نطاق الدورة. اختبار المعلم السريع يحمل فصلًا و`ClassSectionSubjectId` محددين.

#### 5.5.1 `ExamPaperAuthority`

- `Id`, `ExamPaperId`
- `Responsibility`: `Content`, `Grading`, `ResultApproval`
- `AuthorityType`: `EducationalAuthority`, `School`, `Department`, `Teacher`
- `DepartmentId?`, `UserId?`, `ExternalAuthorityName?`, `ExternalReferenceId?`
- `IsLocked`
- audit fields و`RowVersion`

Unique على `(ExamPaperId, Responsibility)`. يمكن أن يرث الـPaper default من الدورة أو policy ثم يحفظ Snapshot صريحًا عند الاعتماد.

#### 5.5.2 `ExamPaperStaffAssignment`

- `Id`, `ExamPaperId`, `UserId`
- `Role`: `ContentEditor`, `Marker`, `Moderator`
- `IsPrimary`, `EffectiveFrom`, `EffectiveTo?`
- audit fields و`RowVersion`

يسمح بأكثر من مصحح أو مراجع للورقة، مع منع التكرار الفعال لنفس المستخدم والدور.

### 5.6 `ExamCandidate`

Snapshot على مستوى الدورة عند النشر:

- `Id`, `ExamSeriesId`
- `StudentId` كـFK ثابت إلى طالب المدرسة مع `DeleteBehavior.Restrict`
- `StudentEnrollmentIdSnapshot` كمرجع إلى القيد الذي أخذت منه الأهلية وقت النشر مع `DeleteBehavior.Restrict`
- `GlobalStudentIdSnapshot`
- `GradeOfferingIdSnapshot`, `ClassSectionIdSnapshot`
- `StudentCodeSnapshot`, `NameArSnapshot`, `NameEnSnapshot`
- `Status`: `Eligible`, `Exempt`, `Withdrawn`
- `SeatNumber?`
- `ExtraTimeMinutes?`
- `RequiresSeparateRoom`
- `AccommodationNotes?`
- audit fields و`RowVersion`

قيود:

- Unique على `(ExamSeriesId, StudentId)` حتى لا يتكرر الطالب إذا تغير قيده أو فصله أثناء إعداد الدورة أو المزامنة.
- Unique filtered على `(ExamSeriesId, SeatNumber)` عندما لا يكون الرقم null.
- رقم الجلوس يخص الدورة كلها افتراضيًا، ولا يوضع كحقل دائم على الطالب.

### 5.7 `ExamPaperCandidate`

يفصل أهلية الطالب لكل مادة، ويدعم المواد الاختيارية والإعفاء:

- `Id`, `ExamPaperId`, `ExamCandidateId`
- `Eligibility`: `Eligible`, `Exempt`, `Withdrawn`
- `FinalResultAttemptId?`
- audit fields و`RowVersion`

Unique على `(ExamPaperId, ExamCandidateId)`.

### 5.8 `ExamScheduleWindow`

يمثل نافذة زمنية مشتركة يمكن أن تضم أوراقًا مختلفة في الوقت نفسه؛ وهذا يمنع إعادة التصميم إذا ضمت اللجنة طلابًا من صفوف يؤدون مواد مختلفة:

- `Id`, `ExamSeriesId`
- `LocalDate`, `StartsAtLocal`, `EndsAtLocal`
- `StartsAtUtc`, `EndsAtUtc`
- `TimeZoneIdSnapshot`
- `Status`: `Draft`, `Scheduled`, `Postponed`, `Cancelled`
- `IsScheduleLocked`
- `PostponementReason?`, `CancellationReason?`, `RescheduledFromWindowId?`
- `ScheduledByUserId`
- audit fields و`RowVersion`

الفترة تعامل كـ`[start, end)` حتى يسمح باختبار يبدأ لحظة انتهاء اختبار آخر.

قبل النشر تعدل نافذة `Draft` في مكانها. بعد النشر تنشئ إعادة الجدولة نافذة جديدة مرتبطة بالقديمة، وتطلق حجوزات القديمة وتحدث Calendar projections داخل transaction واحدة، حتى يبقى التاريخ التشغيلي قابلًا للتدقيق.

### 5.9 `ExamSitting`

يمثل تنفيذ Paper محددة داخل نافذة الجدول:

- `Id`, `ExamPaperId`, `ExamScheduleWindowId`
- `ParentSittingId?`
- `Purpose`
- `AdministrationMode`
- `StartsAtOffsetMinutes`
- `DurationMinutesSnapshot`
- `ExecutionStatus`: `NotStarted`, `InProgress`, `Completed`, `Cancelled`
- `ScheduleLocked`
- audit fields و`RowVersion`

تستطيع نافذة واحدة حمل عدة sittings لمواد أو صفوف مختلفة، بينما الاختبار البسيط يحمل نافذة وsitting واحدة. يبدأ الـSitting داخل النافذة ويشترط أن يكون `StartsAtOffsetMinutes >= 0` وأن تكون نهايته المشتقة من `DurationMinutesSnapshot` داخل نهاية النافذة. تستخدم الحدود المشتقة UTC في الحجوزات وكشف التعارضات.

### 5.10 `ExamCommittee`

تعريف لجنة مستقرة داخل الدورة:

- `Id`, `ExamSeriesId`
- `Code`, `NameAr`, `NameEn`
- `DefaultRoomId`
- `Capacity`
- `AllowMixedGrades`
- `IsSpecialAccommodationCommittee`
- audit fields و`RowVersion`

يجب ألا تتجاوز السعة الامتحانية سعة الغرفة، مع السماح بسعة أقل.

### 5.11 `ExamWindowVenue`

يعرف مكانًا فعليًا متاحًا داخل نافذة الجدول، وهو جزء من Core حتى تعمل اختبارات الفصل قبل إضافة اللجان:

- `Id`, `ExamScheduleWindowId`
- `RoomId`
- `ClassSectionId?` لوضع `InClass`
- `ExamCommitteeId?` لوضع `Committee`
- `CapacitySnapshot`
- `VenueLabel?`
- `Status`
- `RowVersion`

يجب وجود واحد فقط من `ClassSectionId/ExamCommitteeId` وفق `AdministrationMode`.

في Migration الـCore يكون المكان من نوع الفصل ويحمل `ClassSectionId`. تضيف Migration اللجان `ExamCommitteeId` والـconstraint النهائي بصورة additive.

#### 5.11.1 `ExamSittingVenue`

- `Id`, `ExamSittingId`, `ExamWindowVenueId`
- audit fields و`RowVersion`

Unique على `(ExamSittingId, ExamWindowVenueId)`. يسمح الربط بأن تخدم لجنة مختلطة أكثر من Sitting داخل النافذة نفسها، ويحدد صراحةً الأماكن الصالحة لكل ورقة.

### 5.12 `ExamCandidateSittingAssignment`

- `Id`, `ExamPaperId`, `ExamPaperCandidateId`
- `ExamSittingId`, `ExamSittingVenueId`
- `DeskOrSeatLabel?`
- `AssignedAtUtc`, `AssignedByUserId`
- `RowVersion`

Unique على `(ExamSittingId, ExamPaperCandidateId)`. تستخدم composite FKs/alternate keys لضمان أن `ExamPaperCandidate` و`ExamSitting` يتبعان `ExamPaperId` نفسها، وأن `ExamSittingVenue` يتبع نفس Sitting. يسمح ذلك بتغيير اللجنة بين المواد مع بقاء رقم الجلوس ثابتًا للدورة.

### 5.13 `ExamInvigilatorAssignment`

- `Id`, `ExamWindowVenueId`
- `LocalUserId`
- `Role`: `CommitteeLead`, `Invigilator`, `Reserve`
- `IsPrimary`
- audit fields و`RowVersion`

يخضع لنطاق المستخدم، وغيابه يعالج باستبدال مؤرخ دون حذف التكليف السابق.

### 5.14 `ExamAttendance`

- `Id`, `ExamCandidateSittingAssignmentId`
- `Status`: `NotRecorded`, `Present`, `Late`, `AbsentExcused`, `AbsentUnexcused`
- `ArrivedAt?`, `Notes?`
- `RecordedByUserId`, `RecordedAtUtc`
- `FinalizedByUserId?`, `FinalizedAtUtc?`
- `RowVersion`

حضور الاختبار مستقل عن حضور اليوم المدرسي، ولا يحول الغياب إلى درجة صفر تلقائيًا.

### 5.15 `ExamResultAttempt`

- `Id`, `ExamPaperCandidateId`
- `ExamCandidateSittingAssignmentId?`
- `AttemptNumber`
- `Disposition`: `Pending`, `Scored`, `Exempt`, `AbsentExcused`, `AbsentUnexcused`, `Disqualified`, `Withheld`
- `Score?`
- `IsFinal`
- `MarkerUserId?`, `MarkedAtUtc?`
- `ModeratorUserId?`, `ModeratedAtUtc?`
- `ApprovedByUserId?`, `ApprovedAtUtc?`
- `PublishedAtUtc?`
- `Notes?`
- `RowVersion`

قواعد:

- `Score` مطلوب فقط في `Scored` وبين صفر و`TotalScore`.
- الحالات غير الرقمية لا تخزن صفرًا بدل الحالة.
- Unique على `(ExamPaperCandidateId, AttemptNumber)` وFinal واحد فقط لكل PaperCandidate.
- التكليف مطلوب للنتيجة المرتبطة بمحاولة فعلية، ويجوز أن يكون null فقط لحالات لا تحتاج جلسة مثل الإعفاء أو الحجب الإداري. Composite constraint يضمن أن التكليف والمرشح يتبعان الورقة نفسها.
- سياسة الإعادة تحدد استبدال النتيجة أو الأعلى أو سقف الدرجة، مع الاحتفاظ بكل المحاولات.

### 5.16 الموافقات والتعديلات والتدقيق

#### `ExamApproval`

- `Id`, `ExamSeriesId`, `ExamPaperId?`
- `Stage`, `StepOrder`
- `ApproverScope`, `ApproverUserId?`
- `Status`: `Pending`, `Approved`, `Rejected`, `Skipped`
- `DecisionReason?`, timestamps و`RowVersion`

#### `ExamRevision`

- `Id`, `ExamSeriesId`, `ExamPaperId?`, `ExamSittingId?`
- `RevisionType`
- `BeforeJson`, `AfterJson`
- `Reason`
- `RequestedByUserId`, `ApprovedByUserId?`
- `Status`, timestamps و`RowVersion`

#### `ExamAudit`

يسجل الإنشاء والتعديل والموافقة والنشر والتأجيل والإلغاء وتوليد المرشحين والمقاعد وتغيير اللجان والمراقبين والحضور والنتائج وإعادة الفتح.

### 5.17 `ExamScheduleReservation`

صف موحد لحجز الموارد وقت النشر أو تعديل الجدول:

- `Id`, `ExamScheduleWindowId`, `ExamSittingId?`
- `ResourceType`: `Student`, `ClassSection`, `Room`, `User`
- `ResourceId`
- `StartsAtUtc`, `EndsAtUtc`
- `ReleasedAtUtc?`

Index على `(ResourceType, ResourceId, StartsAtUtc, EndsAtUtc)`. تعاد بناء الحجوزات داخل نفس transaction، ويعاد فحص التعارضات قبل commit. يحجز المكان مرة واحدة لكل `ExamWindowVenue` لا مرة لكل Paper داخل اللجنة المختلطة، بينما يحجز الطالب حسب المدة الفعلية للـSitting، والمراقب حسب النافذة/المكان؛ وبذلك لا تنتج الأوراق المتزامنة داخل اللجنة نفسها تعارض غرفة وهميًا.

### 5.18 `ExamCalendarProjection`

- `Id`, `ExamScheduleWindowId`, `SchoolCalendarEventId`
- `ProjectionVersion`, `LastProjectedAtUtc`, `LastError?`
- `RowVersion`

Unique على `ExamScheduleWindowId` و`SchoolCalendarEventId`. كل نافذة منشورة لها حدث تقويم مستقل، لذلك دورة متعددة الأيام لا تختزل في `CalendarEventId` واحد على مستوى السلسلة.

## 6. دورة العمل

### 6.1 إنشاء دورة داخلية

1. إنشاء Draft وتحديد النوع والعام والترم.
2. تحديد جهات الإصدار والجدولة والمحتوى والتقييم والاعتماد.
3. اختيار الصفوف والفصول والمواد.
4. إنشاء الجلسات والمواعيد وطريقة التنفيذ.
5. تشغيل كشف التعارضات.
6. إرسال للموافقة إذا تطلبت policy.
7. النشر داخل transaction واحدة: يعاد التحقق من النطاق والتعارضات، ثم ينشأ Candidate snapshot لأول مرة وتثبت الـpolicy والتوقيت، وبعدها تنشأ الحجوزات وCalendar projections.

قبل النشر لا توجد Candidate rows دائمة؛ يعيد Preview جمهورًا محسوبًا فقط. بعد النشر يمكن لأمر `sync-new` إضافة الطلاب الجدد فقط كـamendment مدقق، ولا يحذف أو يعيد كتابة snapshot قديمًا، ويعيد كشف التعارضات والحجوزات قبل commit.

### 6.2 الاختبار السريع للمعلم

- يفتح المعلم تاب الاختبارات داخل الفصل ويختار مادته.
- يعرض نموذجًا مختصرًا: النوع والعنوان والدرجة والتاريخ والوقت والمدة.
- ينشئ النظام داخليًا `ExamSeries + ExamSeriesTarget + ExamPaper + ExamPaperTarget + ExamScheduleWindow + ExamSitting + ExamWindowVenue + ExamSittingVenue`.
- مسار الموافقة والنشر يحدد من policy دون إظهار الخطوات غير المطلوبة.

### 6.3 الاختبار الخارجي

- يسجل المستخدم المصرح له المصدر والمرجع والبيانات الصادرة.
- حقول الموعد والنطاق المقفلة لا تغير محليًا.
- تضيف المدرسة الفصول واللجان والغرف والمراقبين المسموح بها.
- أي اختلاف محلي يحفظ Revision وليس overwrite للبيانات الخارجية.

### 6.4 نشر النتائج

1. فتح إدخال النتائج بعد بدء الجلسة.
2. حفظ جزئي للدرجات والحالات.
3. Submit من المدرس أو المصحح.
4. مراجعة واعتماد حسب policy.
5. Publish يقفل النتائج ويرسل event للـGradebook/Family مستقبلًا.
6. إعادة الفتح تتطلب permission وسببًا وتحفظ before/after audit.

## 7. محرك التعارضات

### 7.1 تعارضات مانعة

- طالب في جلستين متداخلتين.
- غرفة أو لجنة محجوزة في نفس الوقت.
- مراقب أو مستخدم مكلف في مكانين.
- فصل في اختبارين متداخلين داخل الفصل.
- تجاوز سعة المكان أو تكرار المقعد/رقم الجلوس.
- جلسة خارج حدود العام/الترم أو داخل إغلاق مدرسي.
- Paper أو فصل أو مادة خارج نطاق الدورة.
- مرشح غير فعال أو غير مؤهل للمادة وقت تكوين Snapshot.

### 7.2 تحذيرات قابلة للتجاوز بسياسة

- أكثر من اختبار لنفس الطالب أو الصف في اليوم نفسه.
- اختبار خارج ساعات المدرسة المعتادة.
- مراقبة مدرس للمادة التي يدرسها.
- وقت راحة أقل من القيمة المعرفة بين اختبارين.
- استخدام غرفة غير الغرفة الأساسية للفصل في اختبار `InClass`.
- تقاطع اختبار مع حصة الجدول الأسبوعي أو نشاط منشور؛ تحدد policy هل هو warning أم blocker لأن الاختبار قد يستبدل الحصة عمدًا.

كل override يحتاج permission وسببًا ويحفظ `ExamConflictOverride` في audit.

### 7.3 التزامن

- يستخدم `RowVersion` في كل aggregate وصفوف النتائج والتكليفات.
- كل mutation ترسل النسخة الحالية، ويعاد `409 exams.concurrency_conflict` عند التعارض.
- الجدولة والنشر وتوليد المقاعد واعتماد النتائج تعمل في transaction.
- يستخدم transaction isolation مناسبًا مع إعادة فحص overlap قبل commit لمنع سباق حجز الموارد.
- العمليات القابلة للتكرار تدعم `Idempotency-Key`: publish بما فيه Candidate snapshot، sync-new، توليد المقاعد، اعتماد/نشر النتائج، واستقبال event خارجي.

## 8. الصلاحيات ونطاق البيانات

### 8.1 الصلاحيات

- `school.exams.view`
- `school.exams.manage`
- `school.exams.schedule`
- `school.exams.approve`
- `school.exams.publish`
- `school.exams.committees.manage`
- `school.exams.seating.manage`
- `school.exams.invigilators.manage`
- `school.exams.attendance.record`
- `school.exams.results.enter`
- `school.exams.results.approve`
- `school.exams.results.publish`
- `school.exams.results.reopen`
- `school.exams.cancel`
- `school.exams.reports`
- `school.exams.override_warnings`
- `school.exams.policy.manage`

يحصل `School Admin` عليها من seed. لا تمنح عضوية القسم صلاحية RBAC تلقائيًا.

### 8.2 `ExamScopeResolver`

- Admin: كل المدرسة.
- رئيس القسم أو المنسق: المواد والفصول التابعة لقسمه وتكليفاته الفعالة.
- المدرس: `ClassSectionTeacherScope/TeacherGradeSubjectScope` الخاصة به فقط.
- المراقب: الجلسات واللجان المكلف بها، دون كشف درجات مواد أخرى.
- المصحح أو المراجع: Papers المسندة إليه فقط.
- مصدر الاختبار الخارجي لا يوسع صلاحيات المستخدم المحلي.

كل endpoint يطبق permission وdata scope معًا، ولا يعتمد على إخفاء الأزرار في الواجهة.

## 9. الـAPIs

### 9.1 الخيارات والقائمة المركزية

- `GET /api/schools/v1/exams/options`
- `GET /api/schools/v1/exam-policies`
- `GET /api/schools/v1/exam-policies/effective?programId=&stageId=&kind=`
- `POST/PUT /api/schools/v1/exam-policies[/id]` بصلاحية `school.exams.policy.manage`.
- `GET /api/schools/v1/exam-series?yearId=&termId=&kind=&source=&status=&gradeId=&subjectId=&from=&to=`
- `POST /api/schools/v1/exam-series`
- `GET /api/schools/v1/exam-series/{id}`
- `PUT /api/schools/v1/exam-series/{id}`
- `DELETE /api/schools/v1/exam-series/{id}` للمسودة فقط.

### 9.2 دورة الحياة والموافقات

- `POST /api/schools/v1/exam-series/{id}/validate`
- `POST /api/schools/v1/exam-series/{id}/submit-approval`
- `POST /api/schools/v1/exam-series/{id}/approve`
- `POST /api/schools/v1/exam-series/{id}/reject`
- `POST /api/schools/v1/exam-series/{id}/publish`
- `POST /api/schools/v1/exam-series/{id}/cancel`
- `POST /api/schools/v1/exam-series/{id}/close`
- `POST /api/schools/v1/exam-series/{id}/revisions`

### 9.3 المواد والجلسات

- `POST/PUT/DELETE /api/schools/v1/exam-series/{seriesId}/papers[/paperId]`
- `POST/PUT/DELETE /api/schools/v1/exam-series/{seriesId}/schedule-windows[/windowId]`
- `POST/PUT/DELETE /api/schools/v1/exam-papers/{paperId}/sittings[/sittingId]`
- `GET/PUT /api/schools/v1/exam-schedule-windows/{id}/venues`
- `GET/PUT /api/schools/v1/exam-sittings/{id}/venues`
- `POST /api/schools/v1/exam-schedule-windows/{id}/conflicts/preview`
- `GET /api/schools/v1/exam-sittings/{id}/conflicts`
- `POST /api/schools/v1/exam-schedule-windows/{id}/reschedule`
- `POST /api/schools/v1/exam-schedule-windows/{id}/postpone`
- `POST /api/schools/v1/exam-schedule-windows/{id}/cancel`
- `POST /api/schools/v1/exam-sittings/{id}/start`
- `POST /api/schools/v1/exam-sittings/{id}/complete`
- `POST /api/schools/v1/exam-sittings/{id}/cancel`

### 9.4 المرشحون واللجان

- `GET /api/schools/v1/exam-series/{id}/candidates/preview`
- ينشأ Candidate snapshot فقط داخل أمر Publish، ولا يوجد endpoint مستقل يمكن أن يترك snapshot جزئيًا.
- `POST /api/schools/v1/exam-series/{id}/candidates/sync-new` بعد النشر كإضافة فقط وبـamendment مدقق.
- `GET/PUT /api/schools/v1/exam-series/{id}/committees`
- `POST /api/schools/v1/exam-series/{id}/seat-numbers/generate`
- `POST /api/schools/v1/exam-sittings/{id}/assign-candidates`
- `GET/PUT /api/schools/v1/exam-sittings/{id}/invigilators`

### 9.5 التشغيل والنتائج

- `GET /api/schools/v1/exam-sittings/{id}/roster`
- `PUT /api/schools/v1/exam-sittings/{id}/attendance`
- `POST /api/schools/v1/exam-sittings/{id}/attendance/finalize`
- `POST /api/schools/v1/exam-sittings/{id}/attendance/reopen`
- `GET /api/schools/v1/exam-papers/{id}/results`
- `PUT /api/schools/v1/exam-papers/{id}/results` للحفظ الجزئي batch.
- `POST /api/schools/v1/exam-papers/{id}/results/submit`
- `POST /api/schools/v1/exam-papers/{id}/results/approve`
- `POST /api/schools/v1/exam-papers/{id}/results/publish`
- `POST /api/schools/v1/exam-papers/{id}/results/reopen`
- `POST /api/schools/v1/exam-series/{id}/results/{action}` عملية bulk اختيارية؛ تنفذ ذريًا فقط إذا كانت كل الأوراق صالحة، وإلا تعيد نتيجة validation مفصلة لكل Paper دون تغيير جزئي.

### 9.6 مساحة الفصل والمستخدم الحالي

- `GET /api/schools/v1/class-workspace/{classId}/exams`
- `POST /api/schools/v1/class-workspace/{classId}/exams/quick`
- `GET /api/schools/v1/me/exams?from=&to=&role=teacher|invigilator|marker`

كل Action يعيد error code ثابتًا ورسالة قابلة للترجمة، مثل:

- `exams.schedule_conflict`
- `exams.external_schedule_locked`
- `exams.audience_changed`
- `exams.capacity_exceeded`
- `exams.approval_required`
- `exams.results_incomplete`
- `exams.concurrency_conflict`

## 10. DTOs والأمان

- DTO القائمة لا يعيد بيانات طلاب أو درجات.
- DTO المرشحين متاح فقط لمن لديه scope على الدورة أو الجلسة.
- المراقب يرى الاسم والكود ورقم الجلوس والحضور فقط، ولا يرى النتيجة دون `results.enter/view` المناسب.
- Family/Student projections مستقبلًا لا تعيد نتائج الورقة قبل `ExamPaper.ResultsStatus=Published`.
- لا يقبل الـBackend `CreatedBy`, authority ownership، أو scope مشتقة كقيم موثوقة من العميل.
- external locks والـapproval requirements تتحقق Server-side.
- audit لا يخزن ملفات الاختبار السرية أو payloads ضخمة أو بيانات اعتماد.
- كل التواريخ التشغيلية تحسب بواسطة `SchoolClock`; يحفظ Local date/time وUTC و`TimeZoneIdSnapshot`.
- Batch النتيجة يرسل `rowVersion` لكل نتيجة، ولا يستخدم last-write-wins.

## 11. قواعد التحقق

- العام والـTerm والـPeriod والنطاق الأكاديمي يجب أن ينتموا إلى نفس البرنامج السنوي.
- لا يقبل صف أو فصل أو مادة خارج نطاق الدورة.
- `TotalScore > 0` و`0 <= PassScore <= TotalScore`.
- تخزن الدرجات بـ`decimal` بدقة موحدة، ولا تستخدم floating point.
- نهاية نافذة الجدول بعد بدايتها، وكل الجلسات داخل النافذة نفسها.
- وقت الاختبار داخل حدود العام والترم ما لم يوجد override مصرح ومدقق.
- لا ينشر Paper دون target وSitting ومرشح مؤهل واحد على الأقل.
- عند النشر تصبح الأوراق والنطاق وSnapshot المرشحين immutable؛ التغيير يستخدم Revision أو Action إضافة فقط.
- `sync-new` لا يعمل بعد بدء أي Sitting متأثرة إلا بصلاحية استثنائية وamendment معتمد، ولا يحذف مرشحًا موجودًا.
- لا ينشر جدول بوجود blocker، ولا يتجاوز warning دون permission وسبب.
- لا يخصص الطالب مرتين لنفس Paper/Sitting، ولا يتكرر رقم الجلوس داخل الدورة أو المقعد داخل المكان.
- السعة المستخدمة لا تتجاوز `CapacitySnapshot`، والسعة لا تتجاوز السعة الفعلية للغرفة.
- كل نتيجة إما رقم صحيح ضمن الحد أو disposition غير رقمية، وليس الاثنين معًا.
- لا تعتمد نتائج Paper مع مرشحين بلا حالة نهائية أو درجات مطلوبة.
- لا تنشر نتائج قبل اعتمادها عندما تفرض policy الاعتماد.
- لا تعدل نتيجة منشورة إلا بعد Reopen بصلاحية وسبب.
- كل FKs للتاريخ التشغيلي تستخدم `Restrict`; لا تستخدم cascade يحذف اختبارًا أو مرشحًا أو نتيجة منشورة.

## 12. بنية الـBackend

يقترح فصل الوحدة إلى Domain مستقل داخل Schools Backend:

- `Domain/Exams/SchoolExamEntities.cs`
- `Domain/Exams/SchoolExamPolicies.cs`
- `Application/Exams/Commands`
- `Application/Exams/Queries`
- `Application/Exams/IExamPolicyResolver.cs`
- `Application/Exams/IExamScopeResolver.cs`
- `Application/Exams/IExamConflictEngine.cs`
- `Application/Exams/IExamCandidateSnapshotService.cs`
- `Application/Exams/IExamSeatAllocationService.cs`
- `Application/Exams/IExamCalendarProjectionService.cs`
- `Infrastructure/Persistence/SchoolExamConfigurations.cs`
- `Api/Controllers/SchoolExamSeriesController.cs`
- `Api/Controllers/SchoolExamOperationsController.cs`
- `Api/Controllers/SchoolClassExamsController.cs`

الـclass workspace endpoints تكون façade فوق نفس Application Services، ولا تنشئ كيان `ClassExam` أو business rules منفصلة.

ينفذ كل command حساس في Domain/Application service واحدة حتى تكون قواعد Web وFlutter وأي integration مستقبلية متطابقة.

## 13. تجربة Schools Web

### 13.1 شاشة «الاختبارات» الرئيسية

تظهر في الـSidebar للمستخدم الذي يملك `school.exams.view`، وتتكون من:

- Summary: اختبارات اليوم، جلسات تحتاج تجهيزًا، نتائج معلقة، تعارضات غير محسومة.
- List/Calendar مع فلاتر العام والترم والنوع والمصدر وسلطة الجدولة والنمط والحالة والصف والفصل والمادة والتاريخ.
- دورات الاختبارات.
- جدول الجلسات.
- اللجان وأرقام الجلوس في المرحلة الرسمية.
- الحضور والنتائج المعلقة.

### 13.2 Wizard إنشاء الدورة

1. البيانات والنوع والمصدر والسلطات.
2. نطاق الصفوف والفصول.
3. المواد والأوراق والدرجات والمدد.
4. نوافذ الجدول والجلسات.
5. أسلوب التنظيم واللجان والمراقبون عند تفعيله.
6. Preview المرشحين والتعارضات ثم الحفظ أو الإرسال للموافقة أو النشر.

يحفظ كل Step مسودة، وتظل معرفات المسودة ثابتة عند فشل الموافقة أو النشر.

### 13.3 صفحة تفاصيل الدورة

Tabs:

- نظرة عامة.
- النطاق والمواد.
- الجدول.
- المرشحون.
- اللجان والتسكين.
- المراقبون.
- الحضور.
- النتائج.
- الموافقات وسجل التدقيق.

### 13.4 رصد النتائج

- Grid كبيرة قابلة للبحث بالاسم والكود ورقم الجلوس والفصل والحالة.
- إدخال سريع بلوحة المفاتيح مع validation فوري.
- حفظ جزئي صريح وindicator للتغييرات غير المحفوظة.
- لا تمسح تعديل مستخدم آخر؛ يظهر `409` مع مقارنة وإعادة تحميل.
- Bulk actions للحالات غير الرقمية، دون تحويل الغياب إلى صفر.

### 13.5 تاب الفصل

- يعرض كل اختبارات الفصل المنشورة أو التي يملك المستخدم العمل عليها.
- Quick create للمواد المسندة للمدرس فقط.
- فتح الجلسة، تسجيل الحضور، إدخال الدرجات، ومعرفة حالة الاعتماد.
- اختبارات اللجان تظهر ضمن جدول الفصل، لكن إدارتها المركزية تتم من الشاشة الرئيسية.

كل الشاشات RTL/LTR، responsive، وتحتوي loading/empty/error/retry/permission/concurrency states واضحة.

## 14. تجربة Schools Flutter

### المرحلة الأولى

- تاب الاختبارات داخل «فتح الفصل».
- قائمة اختبارات المعلم والفصل.
- إنشاء اختبار سريع داخل الفصل.
- تفاصيل الموعد والمصدر والحالة.
- تشغيل الجلسة وتسجيل الحضور.
- رصد الدرجات والحفظ الجزئي وإرسالها للاعتماد.

### مرحلة اللجان

- جدول مراقبات المستخدم.
- فتح اللجنة وقائمة الطلاب وأرقام الجلوس.
- تسجيل الحضور والملاحظات التشغيلية.
- عرض التسكين والقاعة والمقعد.

إنشاء توزيع اللجان وأرقام الجلوس والتقارير يظل Web-first، بينما Flutter يركز على التنفيذ الميداني.

يجب استهلاك access token من authenticated client/session provider مشترك بدل الاحتفاظ بنسخة token داخل route طويلة العمر.

## 15. التكاملات

### 15.1 التقويم والجدول

- ينشئ النشر Calendar projection ويراجعها عند التأجيل أو الإلغاء.
- اختبار الفصل يمكنه ربط `ExamSitting` بخانة أو عدة خانات عبر `ExamSittingTimetableSlot` دون تعديل قالب `WeeklyTimetableSlot` الأساسي.
- تعطيل اليوم الدراسي أو إغلاق المدرسة بعد النشر يولد تعارضًا/تنبيهًا ولا يغير موعد الاختبار بصمت.

### 15.2 حضور المدرسة

- `ExamAttendance` مستقل عن `StudentAttendanceRegister`.
- يمكن لاحقًا إصدار event للمقارنة أو اقتراح غياب اليوم، لكن لا يسجل غياب المدرسة تلقائيًا لأن الطالب قد يحضر المدرسة ويغيب عن الاختبار أو العكس.

### 15.3 Gradebook

- Exam module يملك الدرجة الخام وحالة الاعتماد والتعديلات.
- عند نشر النتائج يصدر `ExamResultsPublishedV1` بمراجع ثابتة وidempotent.
- Gradebook يملك الوزن والتجميع والنتيجة النهائية، ولا تنقل ملكية التصحيح إلى Gradebook.
- لا يحفر وزن كل نوع في Exam Core قبل اعتماد تصميم Gradebook.

### 15.4 الإدارة التعليمية

- المرحلة الحالية: إدخال يدوي لمصدر خارجي ومرجعه وrevision.
- المستقبل: Inbox/Outbox keyed by `(ExternalSourceCode, ExternalReferenceId, ExternalRevision)`.
- لا يكتب Platform مباشرة في جداول قاعدة المدرسة.
- تظل النسخة المحلية قابلة للقراءة والعمل عند توقف النظام المركزي.

### 15.5 الإشعارات وFamily App

أحداث integration المقترحة:

- `ExamSeriesPublishedV1`
- `ExamSittingRescheduledV1`
- `ExamSittingCancelledV1`
- `ExamCandidateSeatingPublishedV1`
- `ExamResultsPublishedV1`
- `ExamResultsReopenedV1`

Family App لا تعرض الجدول أو النتيجة إلا بعد status المناسب، وتستخدم projections لا تكشف بيانات طلاب آخرين أو بيانات التصحيح الداخلية.

## 16. التقارير والطباعة

تضاف في مرحلة اللجان:

- جدول الاختبارات حسب الصف والفصل والطالب والمعلم.
- بطاقة رقم الجلوس.
- كشف اللجنة والمقاعد.
- كشف حضور وتوقيع.
- جدول المراقبين والاحتياطي.
- كشف الدرجات والمراجعة.
- تصدير PDF وExcel مع العربية وRTL.

التقارير تقرأ snapshots المنشورة ولا تعيد اشتقاق الأسماء أو الفصول من الحالة الحالية.

## 17. قاعدة البيانات والمهاجرات

### 17.1 `AddSchoolExamCore`

- السياسات والموافقات.
- Series/Authorities/Targets/Papers/PaperTargets/PaperAuthorities/StaffAssignments.
- ScheduleWindows/Sittings وWindowVenues الخاصة بالفصل وSittingVenues.
- Candidates/PaperCandidates/CandidateSittingAssignments.
- Attendance/Results/Audits/Revisions/Reservations.
- External source identity والـidempotency index.
- CalendarProjections؛ ويعرف الحدث المدار آليًا من وجود هذا الربط، دون تكرار مصدر الحقيقة داخل `SchoolCalendarEvent`.
- permissions وSchool Admin grants.

### 17.2 `AddSchoolExamCommittees`

- Committees وإضافة `ExamCommitteeId` الاختياري إلى WindowVenues الموجودة.
- Seat number/desk allocation fields والسياسات.
- Invigilator assignments.
- تقارير ومراجع التسكين اللازمة.

هذا فصل additive؛ Generic venues والربط بالـSitting والتكليف والحضور موجودة في Core لاختبارات الفصل، لذلك إضافة اللجان توسع نوع المكان والتسكين دون تغيير Series/Paper/Sitting أو النتائج.

### 17.3 متطلبات كل migration

- Migration مستقلة لـPostgreSQL وSQL Server وتحديث model snapshots.
- تسجيل كيانات `RowVersion` صراحة في provider-specific concurrency setup: PostgreSQL `xmin` وSQL Server `rowversion`.
- `dotnet ef migrations has-pending-model-changes` بلا فروق للمزودين.
- توليد idempotent migration scripts بنجاح.
- اختبار قاعدة مدرسة جديدة وقاعدة مرفوعة من آخر إصدار حالي.
- لا تغيير أو حذف لأحداث التقويم اليدوية القديمة.
- لا Platform database changes في الإصدار المدرسي الأول.
- التطبيق على المدارس القائمة يتم من Action التحديث المركزي بعد backup ومراجعة النتيجة.

## 18. مراحل التنفيذ

### Phase 0 — تثبيت العقود

- اعتماد enums والحالات والـinvariants.
- اعتماد scope والصلاحيات وأكواد الأخطاء.
- اعتماد OpenAPI examples ومصفوفة الاختبارات.
- حسم القرارات المفتوحة في القسم 23.

بوابة الخروج: لا توجد حالة تشغيلية أو سلطة أو transition دون قرار مكتوب.

### Phase 1 — Exam Core وIn-Class API

- Domain/Persistence/Policy Resolver/Scope Resolver.
- migrations للمزودين.
- Series/Papers/Windows/Sittings والاختبار السريع.
- Candidate snapshot والحضور والنتائج.
- الموافقات الأساسية والتدقيق.
- Conflict Engine للطلاب والفصول والغرف والإغلاقات.
- Calendar projection.
- Unit وintegration tests.

بوابة الخروج: سيناريو API كامل من Draft حتى نشر نتائج اختبار داخل الفصل.

### Phase 2 — Schools Web

- الشاشة المركزية وWizard والتفاصيل والفلاتر.
- تاب الفصل والاختبار السريع.
- تشغيل الجلسة والحضور وGrid النتائج.
- التعامل مع permissions/concurrency/retry/localization.

بوابة الخروج: Admin وقسم ومدرس يكملون السيناريوهات المصرح بها من Web.

### Phase 3 — Schools Flutter

- تاب الفصل وجدول اختبارات المستخدم.
- quick create والتفاصيل.
- الحضور والدرجات والحفظ الجزئي.
- Widget/API contract tests.

بوابة الخروج: عقد وسلوك متطابقان مع Web في النطاق الميداني.

### Phase 4 — اللجان وأرقام الجلوس

- migration additive للجنة والمكان والتسكين والمراقبين.
- Preview ثم generation deterministic لأرقام الجلوس والمقاعد.
- كشف السعة والتعارضات وغياب المراقب والبديل.
- Web management وFlutter operations.
- PDF/Excel reports.

بوابة الخروج: دورة نهاية ترم متعددة المواد تعمل باللجان من التخطيط حتى كشف الحضور.

### Phase 5 — التكامل والنتائج المتقدمة

- Inbox اختبارات الإدارة التعليمية.
- Outbox نتائج Gradebook/Family.
- الإشعارات.
- الإعادة والدور الثاني والسياسات المتقدمة.
- التظلمات والتعديلات بعد النشر وتقارير التحليل.

## 19. خطة الاختبارات

### 19.1 Unit tests

- استقلال النوع والمصدر وسلطة الجدولة والنمط.
- كل transition صحيح ورفض transition غير صحيح.
- policy resolution حسب درجة التخصيص.
- authority field locks.
- snapshot الطلاب والمواد الاختيارية.
- الدرجة والنجاح والحالات غير الرقمية.
- الإعادة واختيار النتيجة النهائية.
- seat number generation deterministic.
- conflict interval semantics `[start,end)`.

### 19.2 Persistence وmigrations

- unique constraints لكل candidate/paper/seat/assignment.
- Check constraints للدرجات والوقت والمسار الحصري للمكان.
- concurrency لكل provider.
- Restrict delete على التاريخ المنشور.
- pending model changes وidempotent scripts.
- ترقية قاعدة حقيقية بنسخة من schema الحالي.

### 19.3 API integration tests

- 401/403 ومصفوفة Admin/Department/Teacher/Invigilator/Marker.
- منع الوصول لفصل أو مادة خارج scope.
- الوصول التاريخي بعد تعطيل الإسناد مع منع إنشاء جديد عليه.
- publish idempotency وعدم تكرار المرشحين.
- تغير القيد بعد النشر وsync add-only.
- external schedule lock.
- warning override بصلاحية وسبب.
- النشر المتزامن وحجز نفس المورد.
- الحفظ الجزئي ورفض نتيجة خارج الحد أو غير مكتملة.
- reopen بسبب إلزامي وbefore/after audit.
- calendar projection والتأجيل والإلغاء.
- School timezone والتوقيت الصيفي.

### 19.4 UI tests

- permission-gated tabs/actions.
- Wizard لكل نمط relevant.
- الاحتفاظ بالمسودة عند فشل النشر.
- فلاتر Server-side وحالات empty/loading/error/retry.
- معالجة `409` دون فقد تعديلات صامت.
- Grid درجات كبيرة واستخدام لوحة المفاتيح.
- RTL/LTR وresponsive Web.
- Flutter widget tests مع وبدون permissions.
- منع double-submit وإظهار progress واضح.

### 19.5 Smoke tests

- اختبار أسبوعي سريع ينشئه مدرس لمادته وفصله.
- اختبار شهري ينشئه القسم لعدة فصول ويوافق عليه المسؤول.
- اختبار بمرجع إدارة تعليمية وموعد مركزي مقفول.
- نشر Candidate snapshot ثم نقل طالب والتأكد من ثبات التاريخ.
- إضافة طالب وتشغيل sync والتأكد أنه add-only.
- تعارض طالب وغرفة ومراقب وإغلاق مدرسي.
- تسجيل حضور ودرجات واعتماد ونشر وإعادة فتح.
- دورة نهاية ترم متعددة المواد واللجان وأرقام الجلوس.

## 20. معايير القبول

### 20.1 Exam Core/In-Class

- الأنواع الخمسة تعمل دون ربطها قسرًا بأسلوب تنفيذ أو سلطة.
- جهة الإصدار وسلطة الجدولة مستقلتان ومحميتان Server-side.
- Admin يرى المدرسة، والقسم والمدرس يريان نطاقهما فقط.
- المعلم ينفذ اختبار فصل كاملًا من Web وFlutter حسب policy.
- النشر يثبت الطلاب والمواد والتوقيت دون تكرار.
- الحضور منفصل عن النتيجة والغياب ليس صفرًا.
- لا تعتمد أو تنشر نتيجة ناقصة أو غير صالحة.
- التعارضات المانعة توقف النشر، والتحذيرات تحتاج override مدققًا.
- كل تعديل متزامن يعيد Conflict واضحًا وليس last-write-wins.
- Calendar projection يعكس النشر والتأجيل والإلغاء.

### 20.2 اللجان

- رقم الجلوس ثابت وفريد داخل الدورة.
- يمكن تغيير اللجنة أو المقعد بين النوافذ دون تغيير رقم الجلوس.
- لا تتجاوز اللجنة أو الغرفة السعة ولا يتكرر المقعد.
- لا يتعارض المراقب أو الغرفة أو الطالب.
- يمكن أن تضم نافذة واحدة sittings لأوراق مختلفة دون حجز الغرفة مرتين بصورة خاطئة.
- كشوف اللجنة والبطاقات والحضور تصدر بالعربية بصورة صحيحة.

### 20.3 الجودة والتشغيل

- migrations للمزودين ناجحة على قاعدة جديدة وقاعدة مطورة.
- build/lint/analyze والاختبارات الآلية ناجحة.
- جميع الأخطاء بأكواد `exams.*` مترجمة.
- لا بيانات حساسة في logs أو audits غير الضرورية.
- smoke test بأدوار فعلية ومدرسة تجريبية ناجح قبل التفعيل العام.

## 21. النشر التدريجي

1. Backup لقاعدة المدرسة التجريبية.
2. نشر Backend متوافق والميزة غير ظاهرة دون permissions/flag.
3. تطبيق migration من المسار المركزي ومراجعة SchemaVersion.
4. تشغيل API smoke على Staging.
5. نشر Web ثم Flutter.
6. اختبار Admin ورئيس قسم ومدرس.
7. تفعيل مدرسة Pilot ومراقبتها 48–72 ساعة.
8. التوسع التدريجي.
9. تشغيل migrations اللجان وتفعيلها بصورة مستقلة لاحقًا.

عند مشكلة تشغيلية تغلق الميزة ويعاد نشر API متوافق؛ لا تنفذ down migration مدمرة على بيانات امتحانات منشورة.

## 22. المراقبة

Structured logs دون أسماء الطلاب أو درجاتهم:

- `SchoolId`, `ExamSeriesId`, `PaperId`, `ScheduleWindowId`, `SittingId`.
- action، actor، from/to status، conflict codes، correlation id.

Metrics:

- عدد الدورات حسب الحالة والنوع.
- فشل النشر حسب conflict code.
- زمن Preview/Publish وحجم candidate snapshot.
- الجلسات المفتوحة بعد موعدها.
- اكتمال الحضور والنتائج.
- النتائج المعلقة للاعتماد.
- concurrency conflicts.
- Calendar projection وInbox/Outbox failures.
- migration failures.

Alerts على ارتفاع `5xx`، وفشل projections، والجلسات المتروكة مفتوحة، والنتائج المعلقة مدة طويلة، وفشل migrations أو الرسائل.

## 23. القرارات المطلوب اعتمادها قبل التنفيذ

التوصيات الافتراضية:

1. رقم الجلوس فريد وثابت داخل `ExamSeries`، والمقعد/اللجنة قابلان للتغيير لكل نافذة.
2. `ExamScheduleWindow` تدعم أوراقًا مختلفة في الوقت نفسه داخل لجان مختلطة من البداية.
3. الموعد الصادر من الإدارة التعليمية لا يعدل محليًا؛ التغيير Revision بمصدر خارجي جديد أو استثناء مصرح ومدقق.
4. اختبار المعلم الأسبوعي يمكن نشره مباشرة فقط إذا سمحت policy؛ الشهري وMidTerm يطلبان موافقة القسم افتراضيًا، واختبارات الترم والعام موافقة المدرسة.
5. اعتماد النتائج إلزامي افتراضيًا للاختبارات الرسمية، وقابل للتخفيف للاختبارات البسيطة من policy.
6. التحريري والشفهي والعملي مدعومة كـFormat من البداية، لكن Question Bank/Online delivery مؤجلان.
7. Exam Core يحفظ الدرجة الخام والنجاح فقط؛ وزن الدرجة وتجميعها يملكه Gradebook مستقبلًا.
8. المرشح المنسحب بعد النشر لا يحذف؛ تتغير أهليته أو disposition مع بقاء التاريخ.
9. تعديل النتائج بعد النشر يستخدم Reopen + ResultRevision ولا يكتب فوق التاريخ بصمت.
10. الأحداث اليدوية القديمة من نوع Exam لا تتحول تلقائيًا.

بعد اعتماد هذه النقاط تصبح Phase 0 مكتملة ويمكن بدء التنفيذ دون قرارات بنيوية معلقة.
