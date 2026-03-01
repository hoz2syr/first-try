namespace StudentTracker.Models;

/// <summary>
/// نموذج السنة الدراسية - يمثل سنة واحدة في الخطة الأكاديمية.
/// </summary>
public class StudyYear
{
    /// <summary>المعرف الفريد.</summary>
    public int Id { get; set; }

    /// <summary>رقم السنة الدراسية (1, 2, 3, ...).</summary>
    public int YearNumber { get; set; }
}
