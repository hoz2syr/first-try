namespace StudentTracker.Models;

/// <summary>
/// حالات المادة الدراسية المحتملة.
/// </summary>
public enum SubjectStatus
{
    /// <summary>لم تبدأ بعد.</summary>
    NotStarted,

    /// <summary>معلّقة - يوجد درجة جزئية لم تكتمل.</summary>
    Pending,

    /// <summary>ناجح - الدرجة النهائية ≥ 60.</summary>
    Passed,

    /// <summary>راسب - الدرجة النهائية أقل من 60.</summary>
    Failed,

    /// <summary>مستقبلية - تنتمي لسنة لاحقة.</summary>
    Future
}
