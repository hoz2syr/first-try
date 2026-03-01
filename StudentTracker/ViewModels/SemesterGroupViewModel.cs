using System.Collections.ObjectModel;

namespace StudentTracker.ViewModels;

/// <summary>
/// نموذج عرض مجموعة الفصل - يجمع مواد فصل دراسي واحد للعرض في المخطط العام.
/// </summary>
public class SemesterGroupViewModel
{
    /// <summary>رقم السنة الدراسية.</summary>
    public int YearNumber { get; set; }

    /// <summary>رقم الفصل الدراسي.</summary>
    public int SemesterNumber { get; set; }

    /// <summary>اسم الفصل (مثال: "الفصل الأول").</summary>
    public string SemesterName { get; set; } = string.Empty;

    /// <summary>العنوان الكامل (مثال: "السنة 1 - الفصل الأول").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>العنوان المعروض في XAML - مرادف لـ <see cref="Title"/>.</summary>
    public string Header => Title;

    /// <summary>مواد هذا الفصل.</summary>
    public ObservableCollection<SubjectViewModel> Subjects { get; set; } = new();
}
