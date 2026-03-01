using System.Globalization;
using System.Windows.Data;

namespace StudentTracker.Converters;

/// <summary>
/// محول القيمة العكسية للمنطقي
/// </summary>
/// <remarks>
/// يُستخدم لعكس قيمة Boolean في واجهة المستخدم
/// مثلاً: تحويل true إلى false والعكس صحيح
/// مفيد لـ Visual State مثل إظهار/إخفاء العناصر
/// 
/// مثال استخدام في XAML:
/// <code>
/// &lt;Button Visibility="{Binding IsEditing, Converter={StaticResource InverseBooleanConverter}}"&gt;
/// </code>
/// </remarks>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// تحويل القيمة إلى عكسها
    /// </summary>
    /// <param name="value">القيمة المنطقية</param>
    /// <param name="targetType">النوع الهدف</param>
    /// <param name="parameter">معامل إضافي</param>
    /// <param name="culture">الثقافة</param>
    /// <returns>القيمة المعكوسة (True→False, False→True)</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    /// <summary>
    /// التحويل العكسي (نفس العملية)
    /// </summary>
    /// <remarks>
    /// عملية العكس هي نفسها في الاتجاهين
    /// </remarks>
    /// <param name="value">القيمة المنطقية</param>
    /// <param name="targetType">النوع الهدف</param>
    /// <param name="parameter">معامل إضافي</param>
    /// <param name="culture">الثقافة</param>
    /// <returns>القيمة المعكوسة</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}
