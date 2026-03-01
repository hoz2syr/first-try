using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StudentTracker.Models;

namespace StudentTracker.Converters;

/// <summary>
/// محول حالة المادة إلى لون
/// </summary>
/// <remarks>
/// يحول حالة المادة إلى لون مناسب للعرض:
/// - ناجح: أخضر (SecondaryColor = #10B981)
/// - راسب: أحمر (DangerColor = #EF4444)
/// - معلّق: برتقالي (AccentColor = #F59E0B)
/// - مستقبلي: رمادي (MutedColor = #94A3B8)
/// </remarks>
public class StatusToColorConverter : IValueConverter
{
    // تعريف الألوان - مُوحَّدة مع لوحة الألوان في Styles.xaml
    private static readonly SolidColorBrush SuccessBrush = new((Color)ColorConverter.ConvertFromString("#10B981"));  // SecondaryColor
    private static readonly SolidColorBrush ErrorBrush = new((Color)ColorConverter.ConvertFromString("#EF4444"));    // DangerColor
    private static readonly SolidColorBrush WarningBrush = new((Color)ColorConverter.ConvertFromString("#F59E0B"));  // AccentColor
    private static readonly SolidColorBrush FutureBrush = new((Color)ColorConverter.ConvertFromString("#94A3B8"));   // MutedColor
    private static readonly SolidColorBrush DefaultBrush = new((Color)ColorConverter.ConvertFromString("#1E293B"));  // TextPrimaryColor

    /// <summary>
    /// تحويل الحالة إلى لون
    /// </summary>
    /// <param name="value">حالة المادة (SubjectStatus)</param>
    /// <param name="targetType">النوع الهدف</param>
    /// <param name="parameter">معامل إضافي</param>
    /// <param name="culture">الثقافة</param>
    /// <returns>فرشاة اللون المناسبة</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // التحقق من نوع القيمة
        if (value is not SubjectStatus status)
        {
            return DefaultBrush;
        }

        // تحويل الحالة إلى لون
        return status switch
        {
            SubjectStatus.Passed => SuccessBrush,
            SubjectStatus.Failed => ErrorBrush,
            SubjectStatus.Pending => WarningBrush,
            SubjectStatus.Future => FutureBrush,
            _ => DefaultBrush
        };
    }

    /// <summary>
    /// التحويل العكسي (غير مدعوم)
    /// </summary>
    /// <remarks>
    /// تحويل اللون إلى حالة غير منطقي في هذا السياق
    /// نُرجع الحالة الافتراضية
    /// </remarks>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // تحويل اللون إلى حالة غير مدعوم - نُرجع الحالة الافتراضية
        // هذه الدالة مطلوبة بموجب واجهة IValueConverter لكنها غير مستخدمة في التطبيق
        return SubjectStatus.NotStarted;
    }
}
