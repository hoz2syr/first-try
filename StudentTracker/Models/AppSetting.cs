namespace StudentTracker.Models;

/// <summary>
/// نموذج إعداد التطبيق - يمثل زوج مفتاح/قيمة في جدول الإعدادات.
/// </summary>
public class AppSetting
{
    /// <summary>مفتاح الإعداد الفريد.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>قيمة الإعداد (قد تكون فارغة).</summary>
    public string? Value { get; set; }
}
