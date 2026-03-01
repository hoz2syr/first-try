using StudentTracker.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StudentTracker.ViewModels;

/// <summary>
/// نموذج عرض المادة - يُستخدم لعرض معلومات المادة في الواجهة
/// </summary>
/// <remarks>
/// فئة ViewModel تغلف فئة Subject وتوفر:
/// - خصائص للربط مع واجهة المستخدم
/// - حالة التحرير المضمنة
/// - أوامر لإضافة الدرجات والامتحانات
/// - طرق تنسيق للعرض
/// </remarks>
public class SubjectViewModel : INotifyPropertyChanged
{
    // ==================== حالة التحرير المضمن ====================
    
    /// <summary>
    /// هل المادة في وضع التحرير
    /// </summary>
    private bool _isEditing;
    
    /// <summary>
    /// الدرجة العملية المُدخلة في وضع التحرير
    /// </summary>
    private string _editPracticalGrade = "";
    
    /// <summary>
    /// الدرجة النظرية المُدخلة في وضع التحرير
    /// </summary>
    private string _editTheoryGrade = "";
    
    /// <summary>
    /// تاريخ امتحان العملي في وضع التحرير
    /// </summary>
    private DateTime? _editPracticalExamDate = null;
    
    /// <summary>
    /// وقت امتحان العملي في وضع التحرير
    /// </summary>
    private string _editPracticalExamTime = "";
    
    /// <summary>
    /// تاريخ امتحان النظري في وضع التحرير
    /// </summary>
    private DateTime? _editTheoryExamDate = null;
    
    /// <summary>
    /// وقت امتحان النظري في وضع التحرير
    /// </summary>
    private string _editTheoryExamTime = "";

    // ==================== خصائص البيانات ====================
    
    /// <summary>
    /// معرف المادة الفريد
    /// </summary>
    public int SubjectId { get; set; }
    
    /// <summary>
    /// رقم السنة الدراسية
    /// </summary>
    public int YearNumber { get; set; }
    
    /// <summary>
    /// رقم الفصل الدراسي
    /// </summary>
    public int SemesterNumber { get; set; }
    
    /// <summary>
    /// اسم المادة
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// الدرجة العملية (nullable)
    /// </summary>
    public double? PracticalGrade { get; set; }
    
    /// <summary>
    /// الدرجة النظرية (nullable)
    /// </summary>
    public double? TheoryGrade { get; set; }
    
    /// <summary>
    /// تاريخ امتحان العملي
    /// </summary>
    public DateTime? PracticalExamDate { get; set; }
    
    /// <summary>
    /// تاريخ امتحان النظري
    /// </summary>
    public DateTime? TheoryExamDate { get; set; }
    
    /// <summary>
    /// الدرجة النهائية للمادة
    /// </summary>
    public double? FinalGrade { get; set; }
    
    /// <summary>
    /// حالة المادة (ناجح، راسب، معلّق، الخ)
    /// </summary>
    public SubjectStatus Status { get; set; }
    
    /// <summary>
    /// هل المادة فيها إعادة عملي
    /// </summary>
    public bool IsRetakeLab { get; set; }
    
    /// <summary>
    /// هل يجب إظهار زر إعادة العملي
    /// </summary>
    public bool ShowRetakeLabButton { get; set; }
    
    /// <summary>
    /// هل المادة مقفلة (لا يمكن تعديلها)
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// هل يوجد تعارض في مواعيد الامتحانات
    /// </summary>
    public bool HasExamConflict { get; set; }

    // ==================== خصائص حالة التحرير ====================
    
    /// <summary>
    /// هل المادة في وضع التحرير - للربط مع الواجهة
    /// </summary>
    public bool IsEditing
    {
        get => _isEditing;
        set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotEditing)); }
    }
    
    /// <summary>
    /// العكس المنطقي لـ IsEditing
    /// </summary>
    public bool IsNotEditing => !_isEditing;

    /// <summary>
    /// الدرجة العملية المُدخلة - للربط مع TextBox
    /// </summary>
    public string EditPracticalGrade
    {
        get => _editPracticalGrade;
        set { _editPracticalGrade = value; OnPropertyChanged(); }
    }
    
    /// <summary>
    /// الدرجة النظرية المُدخلة - للربط مع TextBox
    /// </summary>
    public string EditTheoryGrade
    {
        get => _editTheoryGrade;
        set { _editTheoryGrade = value; OnPropertyChanged(); }
    }
    
    /// <summary>
    /// تاريخ امتحان العملي المُدخل
    /// </summary>
    public DateTime? EditPracticalExamDate
    {
        get => _editPracticalExamDate;
        set { _editPracticalExamDate = value; OnPropertyChanged(); }
    }
    
    /// <summary>
    /// وقت امتحان العملي المُدخل
    /// </summary>
    public string EditPracticalExamTime
    {
        get => _editPracticalExamTime;
        set { _editPracticalExamTime = value; OnPropertyChanged(); }
    }
    
    /// <summary>
    /// تاريخ امتحان النظري المُدخل
    /// </summary>
    public DateTime? EditTheoryExamDate
    {
        get => _editTheoryExamDate;
        set { _editTheoryExamDate = value; OnPropertyChanged(); }
    }
    
    /// <summary>
    /// وقت امتحان النظري المُدخل
    /// </summary>
    public string EditTheoryExamTime
    {
        get => _editTheoryExamTime;
        set { _editTheoryExamTime = value; OnPropertyChanged(); }
    }

    // ==================== خصائص العرض المنسق ====================
    
    /// <summary>
    /// الدرجة العملية كنص - "—" إذا لم تكن موجودة
    /// </summary>
    public string PracticalGradeText => PracticalGrade.HasValue ? PracticalGrade.Value.ToString("0.##") : "—";
    
    /// <summary>
    /// الدرجة النظرية كنص - "—" إذا لم تكن موجودة
    /// </summary>
    public string TheoryGradeText => TheoryGrade.HasValue ? TheoryGrade.Value.ToString("0.##") : "—";
    
    /// <summary>
    /// الدرجة النهائية كنص - "—" إذا لم تكن موجودة
    /// </summary>
    public string FinalGradeText => FinalGrade.HasValue ? FinalGrade.Value.ToString("0.##") : "—";
    
    /// <summary>
    /// تاريخ امتحان العملي كنص
    /// </summary>
    public string PracticalExamDateText => PracticalExamDate.HasValue ? PracticalExamDate.Value.ToString("yyyy-MM-dd  HH:mm") : "غير محدد";
    
    /// <summary>
    /// تاريخ امتحان النظري كنص
    /// </summary>
    public string TheoryExamDateText => TheoryExamDate.HasValue ? TheoryExamDate.Value.ToString("yyyy-MM-dd  HH:mm") : "غير محدد";
    
    /// <summary>
    /// نطاق الدرجات العملية (للعرض)
    /// </summary>
    /// <remarks>
    /// مثال: "50_50" تعني أن العملي 50 والنظري المطلوب 50 لإكمال 100
    /// </remarks>
    public string PracticalGradeRangeText
    {
        get
        {
            if (!PracticalGrade.HasValue) return "—";
            var practical = PracticalGrade.Value;
            var maxTheoryAllowed = 100 - practical;
            return $"{practical:0}_{maxTheoryAllowed:0}";
        }
    }
    
    /// <summary>
    /// نص حالة المادة للعرض
    /// </summary>
    public string StatusText => Status switch
    {
        SubjectStatus.Passed => "ناجح",
        SubjectStatus.Failed => "راسب",
        SubjectStatus.Pending => "معلّق",
        SubjectStatus.Future => "مستقبلية",
        _ => "لم تبدأ"
    };

    // ==================== الأوامر ====================
    
    /// <summary>
    /// أمر إضافة درجة
    /// </summary>
    public ICommand? AddGradeCommand { get; set; }
    
    /// <summary>
    /// أمر إضافة امتحان
    /// </summary>
    public ICommand? AddExamCommand { get; set; }
    
    /// <summary>
    /// أمر عرض السجل
    /// </summary>
    public ICommand? ShowHistoryCommand { get; set; }
    
    /// <summary>
    /// أمر إعادة العملي
    /// </summary>
    public ICommand? RetakeLabCommand { get; set; }

    /// <summary>
    /// أمر بدء التحرير
    /// </summary>
    public ICommand? StartEditCommand { get; set; }
    
    /// <summary>
    /// أمر حفظ التعديلات
    /// </summary>
    public ICommand? SaveEditCommand { get; set; }
    
    /// <summary>
    /// أمر إلغاء التحرير
    /// </summary>
    public ICommand? CancelEditCommand { get; set; }

    // ==================== أحداث INotifyPropertyChanged ====================
    
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
