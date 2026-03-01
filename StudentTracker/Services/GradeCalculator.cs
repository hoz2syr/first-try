using StudentTracker.Models;

namespace StudentTracker.Services;

/// <summary>
/// خدمة حساب الدرجات - حاسبة درجات المواد الدراسية
/// </summary>
/// <remarks>
/// الوظائف:
/// - حساب الدرجة النهائية للمادة
/// - تحديد حالة المادة (ناجح، راسب، معلقة، مستقبلية)
/// - التحقق من نجاح المادة
/// 
/// القواعد:
/// - درجة النجاح هي 60% أو أعلى
/// - الدرجة القصوى هي 100
/// - إذا كان هناك إعادة عملي، تحتسب الدرجة العملية كـ 0
/// </remarks>
public static class GradeCalculator
{
    /// <summary>
    /// حساب الدرجة النهائية للمادة
    /// </summary>
    /// <param name="practical">الدرجة العملية (nullable)</param>
    /// <param name="theory">الدرجة النظرية (nullable)</param>
    /// <param name="isRetakeLab">هل المادة فيها إعادة عملي</param>
    /// <returns>
    /// الدرجة النهائية أو null إذا لم تكتمل المتطلبات
    /// </returns>
    /// <remarks>
    /// إذا لم تكن هناك درجة نظرية، تُرجع null
    /// الدرجة العملية تُحتسب كـ 0 إذا كان هناك إعادة عملي
    /// الدرجة النهائية تكون بين 0 و 100
    /// </remarks>
    public static double? ComputeFinalGrade(double? practical, double? theory, bool isRetakeLab)
    {
        // التحقق من وجود الدرجة النظرية - ضرورية لحساب الدرجة النهائية
        if (!theory.HasValue)
        {
            return null;
        }

        // حساب القيمة العملية:
        // - إذا كان هناك إعادة عملي، تُحتسب كـ 0
        // - وإلا تُستخدم القيمة العملية أو 0 إذا كانت null
        var practicalValue = isRetakeLab ? 0 : practical ?? 0;
        
        // حساب الدرجة النهائية بجمع الدرجتين
        var total = practicalValue + theory.Value;
        
        // تحديد الحد الأقصى للدرجة بـ 100
        return Math.Min(total, 100); 
    }

    /// <summary>
    /// تحديد حالة المادة الدراسية
    /// </summary>
    /// <param name="currentYear">السنة الحالية للطالب</param>
    /// <param name="subjectYear">سنة المادة</param>
    /// <param name="practical">الدرجة العملية</param>
    /// <param name="theory">الدرجة النظرية</param>
    /// <param name="isRetakeLab">هل المادة فيها إعادة عملي</param>
    /// <returns>حالة المادة</returns>
    /// <remarks>
    /// حالة المادة تعتمد على:
    /// - سنة المادة مقارنة بالسنة الحالية
    /// - الدرجة النهائية
    /// - وجود إعادة عملي
    /// </remarks>
    public static SubjectStatus ComputeStatus(
        int currentYear,
        int subjectYear,
        double? practical,
        double? theory,
        bool isRetakeLab)
    {
        // إذا كانت سنة المادة في المستقبل، تكون حالة المادة "مستقبلية"
        if (subjectYear > currentYear)
        {
            return SubjectStatus.Future;
        }

        // حساب الدرجة النهائية
        var finalGrade = ComputeFinalGrade(practical, theory, isRetakeLab);
        
        // إذا وجدت درجة نهائية، تحديد النجاح أو الرسوب
        if (finalGrade.HasValue)
        {
            return finalGrade.Value >= 60 ? SubjectStatus.Passed : SubjectStatus.Failed;
        }

        // إذا لم توجد درجة نهائية:
        // - إذا كان هناك إعادة عملي أو درجة عملية، الحالة "معلقة"
        // - وإلا لم تبدأ المادة بعد
        if (isRetakeLab || practical.HasValue)
        {
            return SubjectStatus.Pending;
        }

        return SubjectStatus.NotStarted;
    }

    /// <summary>
    /// التحقق من نجاح المادة
    /// </summary>
    /// <param name="practical">الدرجة العملية</param>
    /// <param name="theory">الدرجة النظرية</param>
    /// <param name="isRetakeLab">هل المادة فيها إعادة عملي</param>
    /// <returns>true إذا نجحت المادة</returns>
    public static bool IsPassed(double? practical, double? theory, bool retakeLab)
    {
        var finalGrade = ComputeFinalGrade(practical, theory, retakeLab);
        return finalGrade.HasValue && finalGrade.Value >= 60;
    }
}
