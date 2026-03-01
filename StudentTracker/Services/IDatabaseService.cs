using StudentTracker.Models;

namespace StudentTracker.Services;

/// <summary>
/// واجهة خدمة قاعدة البيانات - تُعرّف جميع عمليات الوصول للبيانات.
/// </summary>
/// <remarks>
/// تُستخدم لفصل المنطق عن طبقة البيانات وتسهيل الاختبار.
/// </remarks>
public interface IDatabaseService
{
    // ─── التهيئة ───
    
    /// <summary>إنشاء الجداول وتهيئة القيم الافتراضية.</summary>
    void InitializeDatabase();

    // ─── الإعدادات ───
    
    /// <summary>قراءة قيمة إعداد من جدول الإعدادات.</summary>
    string? GetSetting(string key);

    /// <summary>حفظ أو تحديث قيمة إعداد.</summary>
    void SetSetting(string key, string value);

    /// <summary>الحصول على رقم الفصل الحالي.</summary>
    int GetCurrentSemester();

    /// <summary>تعيين رقم الفصل الحالي.</summary>
    void SetCurrentSemester(int semester);

    // ─── البيانات الأكاديمية ───
    
    /// <summary>الحصول على السنوات الدراسية مرتبة.</summary>
    List<StudyYear> GetStudyYears();

    /// <summary>التحقق من وجود خطة دراسية (مواد مُدخلة).</summary>
    bool HasStudyPlan();

    /// <summary>الحصول على جميع المواد مرتبة بالسنة والفصل.</summary>
    List<Subject> GetAllSubjects();

    /// <summary>الحصول على مواد سنة محددة.</summary>
    IEnumerable<Subject> GetSubjectsByYear(int yearNumber);

    // ─── الدرجات ───
    
    /// <summary>الحصول على آخر درجة مُسجلة لمادة ونوع محدد.</summary>
    double? GetLatestGrade(int subjectId, string type);

    /// <summary>إضافة محاولة درجة جديدة.</summary>
    void AddGradeAttempt(GradeAttempt gradeAttempt);

    /// <summary>الحصول على سجل درجات مادة محددة.</summary>
    List<GradeAttempt> GetSubjectHistory(int subjectId);

    // ─── إعادة العملي ───
    
    /// <summary>الحصول على معرّفات المواد المُعادة عملياً.</summary>
    HashSet<int> GetRetakeLabSubjectIds();

    /// <summary>تسجيل إعادة عملي لمادة.</summary>
    void MarkRetakeLab(int subjectId);

    /// <summary>إلغاء إعادة العملي لمادة.</summary>
    void ClearRetakeLab(int subjectId);

    // ─── الامتحانات ───
    
    /// <summary>الحصول على آخر موعد امتحان لمادة ونوع محدد.</summary>
    DateTime? GetLatestExamDate(int subjectId, string examType);

    /// <summary>التحقق من وجود تعارض في مواعيد الامتحانات.</summary>
    bool HasExamConflict(DateTime examDateTime, int? ignoreSubjectId = null);

    /// <summary>حفظ أو تحديث موعد امتحان.</summary>
    void UpsertExamSchedule(int subjectId, string examType, DateTime examDateTime);

    /// <summary>الحصول على جميع مواعيد الامتحانات.</summary>
    List<ExamSchedule> GetAllExamSchedules();

    // ─── الملاحظات ───
    
    /// <summary>الحصول على جميع الملاحظات مرتبة بالتاريخ.</summary>
    List<Note> GetAllNotes();

    /// <summary>إضافة ملاحظة جديدة.</summary>
    void AddNote(Note note);

    /// <summary>حذف ملاحظة بالمعرّف.</summary>
    void DeleteNote(int noteId);

    /// <summary>تحديث ملاحظة موجودة.</summary>
    void UpdateNote(Note note);

    // ─── الخطة الدراسية ───
    
    /// <summary>حفظ أو تحديث الخطة الدراسية (إضافة/تعديل مواد).</summary>
    void UpsertStudyPlan(int yearsCount, int semestersPerYear,
        Dictionary<(int Year, int Semester), List<PlanSubjectInput>> subjectsBySemester);
}
