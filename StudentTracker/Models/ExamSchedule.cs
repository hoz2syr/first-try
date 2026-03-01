namespace StudentTracker.Models;

/// <summary>
/// نموذج جدول الامتحانات - يمثل موعد امتحان لمادة معينة.
/// </summary>
public class ExamSchedule
{
    /// <summary>المعرف الفريد لسجل الامتحان.</summary>
    public int Id { get; set; }

    /// <summary>معرف المادة المرتبطة.</summary>
    public int SubjectId { get; set; }

    /// <summary>نوع الامتحان (عملي / نظري).</summary>
    public string ExamType { get; set; } = string.Empty;

    /// <summary>تاريخ ووقت الامتحان.</summary>
    public DateTime ExamDateTime { get; set; }
}
