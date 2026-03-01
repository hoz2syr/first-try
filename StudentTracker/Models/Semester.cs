namespace StudentTracker.Models;

/// <summary>
/// نموذج الفصل الدراسي - يمثل فصلاً واحداً ضمن سنة دراسية.
/// </summary>
public class Semester
{
    /// <summary>المعرف الفريد.</summary>
    public int Id { get; set; }

    /// <summary>معرف السنة الدراسية.</summary>
    public int YearId { get; set; }

    /// <summary>رقم السنة الدراسية (للعرض).</summary>
    public int YearNumber { get; set; }

    /// <summary>رقم الفصل (1 = أول، 2 = ثاني، 3 = صيفي).</summary>
    public int SemesterNumber { get; set; }
}
