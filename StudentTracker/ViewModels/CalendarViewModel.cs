using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentTracker.ViewModels;

/// <summary>
/// نموذج يوم التقويم - يمثل يوماً واحداً في عرض التقويم
/// </summary>
/// <remarks>
/// يُستخدم لعرض حالة كل يوم في التقويم الشهرية
/// يتضمن معلومات عن التاريخ وما إذا كان اليوم محدداً أو اليوم الحالي
/// </remarks>
public class CalendarDayViewModel : INotifyPropertyChanged
{
    // حالة التحديد - هل اليوم المحدد حالياً
    private bool _isSelected;
    
    // حالة اليوم الحالي - هل هذا هو يومنا الحالي
    private bool _isToday;
    
    // حالة الشهر الحالي - هل اليوم ينتمي للشهر المعروض
    private bool _isCurrentMonth;

    /// <summary>
    /// تاريخ اليوم
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// رقم اليوم في الشهر (1-31)
    /// </summary>
    public int DayNumber => Date.Day;
    
    /// <summary>
    /// هل اليوم يحتوي على أحداث (امتحانات أو ملاحظات)
    /// </summary>
    public bool HasEvents { get; set; }
    
    /// <summary>
    /// قائمة الأحداث في هذا اليوم
    /// </summary>
    public List<CalendarEvent> Events { get; set; } = new();

    /// <summary>
    /// خاصية تحديد اليوم - متاحة للربط مع الواجهة
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// خاصية اليوم الحالي - متاحة للربط مع الواجهة
    /// </summary>
    public bool IsToday
    {
        get => _isToday;
        set { _isToday = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// خاصية الشهر الحالي - متاحة للربط مع الواجهة
    /// </summary>
    public bool IsCurrentMonth
    {
        get => _isCurrentMonth;
        set { _isCurrentMonth = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// حدث تغيير الخصائص
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// دالة إشعار بتغيير الخاصية
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// حدث التقويم - يمثل امتحاناً أو حدثاً في يوم معين
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// عنوان الحدث
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// تاريخ ووقت الحدث
    /// </summary>
    public DateTime DateTime { get; set; }
    
    /// <summary>
    /// نوع الحدث (امتحان، ملاحظة، etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم المادة المرتبطة بالحدث
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع الامتحان (عملي، نظري)
    /// </summary>
    public string ExamType { get; set; } = string.Empty;
    
    /// <summary>
    /// نص الوقت (HH:mm)
    /// </summary>
    public string TimeText => DateTime.ToString("HH:mm");
    
    /// <summary>
    /// نص العرض للحدث
    /// </summary>
    public string DisplayText => string.IsNullOrEmpty(SubjectName) ? Title : $"{ExamType}: {SubjectName}";
}

/// <summary>
/// نموذج عرض الملاحظة - يُستخدم في واجهة المستخدم
/// </summary>
/// <remarks>
/// يُغلف فئة Note ويوفر خصائص تنسيق للعرض
/// </remarks>
public class NoteViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// معرف الملاحظة
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// عنوان الملاحظة
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// محتوى الملاحظة
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// تاريخ آخر تعديل
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// نص تاريخ الإنشاء منسق
    /// </summary>
    public string CreatedDateText => CreatedDate.ToString("yyyy-MM-dd HH:mm");
    
    /// <summary>
    /// نص تاريخ التعديل منسق
    /// </summary>
    public string ModifiedDateText => ModifiedDate.ToString("yyyy-MM-dd HH:mm");
    
    /// <summary>
    /// المحتوى المختصر للعرض في القوائم
    /// </summary>
    /// <remarks>
    /// إذا كان المحتوى أطول من 200 حرف، يتم قصه وإضافة "..."
    /// يتعامل مع الحالات الفارغة بشكل آمن
    /// </remarks>
    public string ShortContent => string.IsNullOrEmpty(Content) 
        ? string.Empty 
        : (Content.Length > 200 ? Content[..200] + "..." : Content);

    /// <summary>
    /// حدث تغيير الخصائص
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// دالة إشعار بتغيير الخاصية
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
