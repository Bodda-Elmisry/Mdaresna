-- =========================================================================
-- SQL Script: Clear School Student Transactions and Report Queues
-- Description: Deletes all dynamic student transactions and report generation
--              queues for a specific school (if @SchoolId is provided) OR
--              for ALL schools in the system (if @SchoolId is NULL).
-- =========================================================================

-- ⚠️ ضع معرف المدرسة المطلوب تصغير حركاتها هنا، أو اتركه NULL لتصفير جميع المدارس
DECLARE @SchoolId UNIQUEIDENTIFIER = NULL; -- 'C9062B95-9ABC-4EDE-87CE-C4AE87B8D07A';

BEGIN TRANSACTION;
BEGIN TRY
    
    -- 1. حذف درجات وتسليمات الواجبات الخاصة بالطلاب
    DELETE FROM [ClassRoomStudentAssignments]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 2. حذف درجات ونقاط الطلاب وتفاعلهم في الأنشطة اليومية
    DELETE FROM [ClassRoomStudentActivities]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 3. حذف درجات الطلاب في الاختبارات الصفية
    DELETE FROM [ClassRoomStudentExams]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 4. حذف تقييمات ودرجات الطلاب الدورية (الشهرية/السنوية المجمعة)
    DELETE FROM [studentExamRates]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 5. حذف حركات حضور وغياب الطلاب اليومية
    DELETE FROM [StudentAttendances]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 6. حذف أذونات غياب الطلاب
    DELETE FROM [StudentAbsencePermits]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 7. حذف الملاحظات السلوكية والتنبيهات المسجلة على الطلاب
    DELETE FROM [studentNotes]
    WHERE [StudentId] IN (
        SELECT [Id] FROM [Students] 
        WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId
    );

    -- 8. حذف طلبات إنشاء التقارير الشهرية الخاصة بالمدرسة من طابور التقارير
    DELETE FROM [ReportQueues]
    WHERE @SchoolId IS NULL OR [SchoolId] = @SchoolId;

    COMMIT TRANSACTION;
    
    IF @SchoolId IS NULL
        PRINT 'SUCCESS: Student transactions and report queues cleared for ALL schools.';
    ELSE
        PRINT 'SUCCESS: Student transactions and report queues cleared for the specified school.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: Transaction rolled back due to an error.';
    THROW;
END CATCH;
