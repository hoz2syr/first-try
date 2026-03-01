namespace StudentTracker.Models;

/// <summary>
/// نموذج محاولة درجة - يمثل إدخال درجة واحدة لمادة ما.
/// </summary>
public class GradeAttempt
{
    /// <summary>المعرف الفريد.</summary>
    public int Id { get; set; }

    /// <summary>معرف المادة المرتبطة.</summary>
    public int SubjectId { get; set; }

    /// <summary>نوع المحاولة (عملي / نظري).</summary>
    public string AttemptType { get; set; } = string.Empty;

    /// <summary>الدرجة المُسجلة (0 - 100).</summary>
    public double Grade { get; set; }

    /// <summary>تاريخ إدخال الدرجة.</summary>
    public DateTime AttemptDate { get; set; }

    /// <summary>ملاحظات إضافية (اختياري).</summary>
    public string? Notes { get; set; }
}
