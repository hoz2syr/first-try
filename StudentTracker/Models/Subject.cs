namespace StudentTracker.Models;

/// <summary>
/// نموذج المادة الدراسية - يمثل مادة واحدة ضمن فصل دراسي.
/// </summary>
public class Subject
{
    /// <summary>المعرف الفريد.</summary>
    public int Id { get; set; }

    /// <summary>معرف الفصل الدراسي.</summary>
    public int SemesterId { get; set; }

    /// <summary>رقم السنة الدراسية (للعرض والتصنيف).</summary>
    public int YearNumber { get; set; }

    /// <summary>رقم الفصل الدراسي.</summary>
    public int SemesterNumber { get; set; }

    /// <summary>اسم المادة.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ترتيب العرض.</summary>
    public int DisplayOrder { get; set; }
}
