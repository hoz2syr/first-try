using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentTracker.Controls;

public partial class DateWheelPicker : UserControl
{
    // أسماء الأشهر الميلادية بالعربية
    private static readonly string[] GregorianMonthNames =
    [
        "يناير", "فبراير", "مارس", "أبريل",
        "مايو", "يونيو", "يوليو", "أغسطس",
        "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    ];

    // أسماء أيام الأسبوع بالعربية
    private static readonly string[] ArabicDayNames =
    [
        "الأحد", "الاثنين", "الثلاثاء", "الأربعاء",
        "الخميس", "الجمعة", "السبت"
    ];

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(DateWheelPicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

    public static readonly DependencyProperty MinYearProperty =
        DependencyProperty.Register(
            nameof(MinYear),
            typeof(int),
            typeof(DateWheelPicker),
            new PropertyMetadata(2000, OnYearRangeChanged));

    public static readonly DependencyProperty MaxYearProperty =
        DependencyProperty.Register(
            nameof(MaxYear),
            typeof(int),
            typeof(DateWheelPicker),
            new PropertyMetadata(2035, OnYearRangeChanged));

    private bool _isInternalUpdate;

    public DateWheelPicker()
    {
        InitializeComponent();
        Loaded += DateWheelPicker_Loaded;
        PreviewKeyDown += DateWheelPicker_PreviewKeyDown;
    }

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public int MinYear
    {
        get => (int)GetValue(MinYearProperty);
        set => SetValue(MinYearProperty, value);
    }

    public int MaxYear
    {
        get => (int)GetValue(MaxYearProperty);
        set => SetValue(MaxYearProperty, value);
    }

    private void DateWheelPicker_Loaded(object sender, RoutedEventArgs e)
    {
        BuildAllWheels();
        SyncWheelsFromSelectedDate();
    }

    private void DateWheelPicker_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateWheelPicker picker && picker.IsLoaded)
        {
            picker.SyncWheelsFromSelectedDate();
        }
    }

    private static void OnYearRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateWheelPicker picker && picker.IsLoaded)
        {
            picker.BuildYearWheel();
            picker.SyncWheelsFromSelectedDate();
        }
    }

    private void BuildAllWheels()
    {
        BuildYearWheel();
        BuildMonthWheel();
        BuildDayWheel(GetEffectiveDate().Year, GetEffectiveDate().Month);
    }

    private void BuildYearWheel()
    {
        var min = Math.Min(MinYear, MaxYear);
        var max = Math.Max(MinYear, MaxYear);

        var years = new List<string>();
        for (var year = max; year >= min; year--)
        {
            years.Add(year.ToString(CultureInfo.InvariantCulture));
        }

        YearWheel.ItemsSource = years;
    }

    private void BuildMonthWheel()
    {
        MonthWheel.ItemsSource = new List<string>(GregorianMonthNames);
    }

    private void BuildDayWheel(int year, int month)
    {
        var safeYear = Math.Clamp(year, Math.Min(MinYear, MaxYear), Math.Max(MinYear, MaxYear));
        var safeMonth = Math.Clamp(month, 1, 12);

        var count = DateTime.DaysInMonth(safeYear, safeMonth);
        var days = new List<string>(count);
        for (var day = 1; day <= count; day++)
        {
            days.Add(day.ToString("00", CultureInfo.InvariantCulture));
        }

        DayWheel.ItemsSource = days;
    }

    private DateTime GetEffectiveDate()
    {
        var baseDate = SelectedDate ?? DateTime.Today;
        var min = Math.Min(MinYear, MaxYear);
        var max = Math.Max(MinYear, MaxYear);

        var year = Math.Clamp(baseDate.Year, min, max);
        var month = Math.Clamp(baseDate.Month, 1, 12);
        var day = Math.Min(baseDate.Day, DateTime.DaysInMonth(year, month));

        return new DateTime(year, month, day);
    }

    private void SyncWheelsFromSelectedDate()
    {
        _isInternalUpdate = true;
        try
        {
            var date = GetEffectiveDate();

            BuildDayWheel(date.Year, date.Month);

            var yearLabel = date.Year.ToString(CultureInfo.InvariantCulture);
            var yearIndex = YearWheel.ItemsSource?.IndexOf(yearLabel) ?? -1;
            if (yearIndex >= 0)
            {
                YearWheel.SelectedIndex = yearIndex;
            }

            MonthWheel.SelectedIndex = date.Month - 1;
            DayWheel.SelectedIndex = date.Day - 1;

            UpdatePreviewText(date);
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void Wheel_SelectionChanged(object sender, string value)
    {
        if (_isInternalUpdate)
        {
            return;
        }

        if (YearWheel.SelectedIndex < 0 || MonthWheel.SelectedIndex < 0 || DayWheel.SelectedIndex < 0)
        {
            return;
        }

        if (!int.TryParse(YearWheel.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
        {
            return;
        }

        var month = MonthWheel.SelectedIndex + 1;
        var dayIndex = DayWheel.SelectedIndex + 1;
        var maxDay = DateTime.DaysInMonth(year, month);
        var day = Math.Clamp(dayIndex, 1, maxDay);

        // عند تغيير السنة/الشهر، قد يتغير عدد الأيام
        BuildDayWheel(year, month);

        _isInternalUpdate = true;
        try
        {
            DayWheel.SelectedIndex = day - 1;
            var newDate = new DateTime(year, month, day);
            SelectedDate = newDate;
            UpdatePreviewText(newDate);
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void UpdatePreviewText(DateTime date)
    {
        if (SelectedDatePreview == null) return;

        var dayName = ArabicDayNames[(int)date.DayOfWeek];
        var monthName = GregorianMonthNames[date.Month - 1];
        SelectedDatePreview.Text = $"{dayName}، {date.Day} {monthName} {date.Year}";
    }

    private void TodayButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedDate = DateTime.Today;
    }
}
