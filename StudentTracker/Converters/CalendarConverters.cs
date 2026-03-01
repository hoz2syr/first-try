using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StudentTracker.Converters;

/// <summary>
/// محول خلفية يوم التقويم
/// </summary>
/// <remarks>
/// يحسب لون الخلفية بناءً على:
/// - إذا كان محدداً: أزرق داكن (#6366F1)
/// - إذا كان اليوم الحالي: أزرق فاتح (#EEF2FF)
/// - غير ذلك: شفاف
/// </remarks>
public class CalendarDayBackgroundConverter : IMultiValueConverter
{
    // تعريف الألوان الثابتة لتحسين الأداء
    private static readonly SolidColorBrush SelectedBrush = new((Color)ColorConverter.ConvertFromString("#6366F1"));
    private static readonly SolidColorBrush TodayBrush = new((Color)ColorConverter.ConvertFromString("#EEF2FF"));

    /// <summary>
    /// تحويل القيم المتعددة إلى لون الخلفية
    /// </summary>
    /// <param name="values">القيم: [isSelected, isToday, isCurrentMonth]</param>
    /// <param name="targetType">النوع الهدف</param>
    /// <param name="parameter">معامل إضافي</param>
    /// <param name="culture">الثقافة</param>
    /// <returns>فرشاة اللون المناسبة</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3) return Brushes.Transparent;
        
        bool isSelected = values[0] is bool b0 && b0;
        bool isToday = values[1] is bool b1 && b1;

        if (isSelected)
            return SelectedBrush;
        if (isToday)
            return TodayBrush;
        
        return Brushes.Transparent;
    }

    /// <summary>
    /// التحويل العكسي غير مدعوم
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// محول لون نص يوم التقويم
/// </summary>
/// <remarks>
/// يحسب لون النص بناءً على:
/// - محدد: أبيض
/// - ليس في الشهر الحالي: رمادي (#94A3B8)
/// - غير ذلك: داكن (#1E293B)
/// </remarks>
public class CalendarDayForegroundConverter : IMultiValueConverter
{
    // تعريف الألوان الثابتة لتحسين الأداء
    private static readonly SolidColorBrush OutOfMonthBrush = new((Color)ColorConverter.ConvertFromString("#94A3B8"));
    private static readonly SolidColorBrush NormalBrush = new((Color)ColorConverter.ConvertFromString("#1E293B"));

    /// <summary>
    /// تحويل القيم المتعددة إلى لون النص
    /// </summary>
    /// <param name="values">القيم: [isSelected, isToday, isCurrentMonth]</param>
    /// <param name="targetType">النوع الهدف</param>
    /// <param name="parameter">معامل إضافي</param>
    /// <param name="culture">الثقافة</param>
    /// <returns>فرشاة اللون المناسبة</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3) return Brushes.Black;
        
        bool isSelected = values[0] is bool b0 && b0;
        bool isCurrentMonth = values[2] is bool b2 && b2;

        if (isSelected)
            return Brushes.White;
        if (!isCurrentMonth)
            return OutOfMonthBrush;
        
        return NormalBrush;
    }

    /// <summary>
    /// التحويل العكسي غير مدعوم
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// محول القيمة العكسية لـ Boolean إلى Visibility
/// </summary>
/// <remarks>
/// يُخفي العنصر إذا كانت القيمة true أو كان العدد > 0
/// يُظهر العنصر في الحالات الأخرى
/// </remarks>
public class BoolToVisibilityInverseConverter : IValueConverter
{
    /// <summary>
    /// تحويل القيمة إلى Visibility معكوس
    /// </summary>
    /// <param name="value">قيمة منطقية أو عدد</param>
    /// <param name="targetType">النوع الهدف</param>
    /// <param name="parameter">معامل إضافي</param>
    /// <param name="culture">الثقافة</param>
    /// <returns>Visibility.Collapsed إذا كان true/عدد>0، وإلا Visible</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // دعم كل من Boolean والعدد (عدد العناصر)
        if (value is bool b)
            return b ? Visibility.Collapsed : Visibility.Visible;
        if (value is int count)
            return count > 0 ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    /// <summary>
    /// التحويل العكسي غير مدعوم
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
