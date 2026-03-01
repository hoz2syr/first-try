namespace StudentTracker.Models;

/// <summary>
/// بيانات إدخال مادة عند تحرير الخطة الدراسية.
/// </summary>
public class PlanSubjectInput
{
    /// <summary>معرف المادة الموجودة (null للمادة الجديدة).</summary>
    public int? SubjectId { get; set; }

    /// <summary>اسم المادة.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ترتيب العرض.</summary>
    public int DisplayOrder { get; set; }
}
