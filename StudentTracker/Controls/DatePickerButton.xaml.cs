using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StudentTracker.Controls;

/// <summary>
/// زر يعرض Popup يحتوي على DateWheelPicker عند النقر عليه.
/// يُستخدم كبديل مباشر لـ WPF DatePicker في الأماكن المضمنة.
/// </summary>
public partial class DatePickerButton : UserControl
{
    private static readonly string[] MonthNames =
    [
        "يناير", "فبراير", "مارس", "أبريل",
        "مايو", "يونيو", "يوليو", "أغسطس",
        "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    ];

    private DateWheelPicker? _picker;
    private Popup? _popup;
    private bool _isSubscribed;

    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(DatePickerButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(
            nameof(DisplayText),
            typeof(string),
            typeof(DatePickerButton),
            new PropertyMetadata("اختر تاريخ..."));

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public DatePickerButton()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private DateWheelPicker Picker => _picker ??= (DateWheelPicker)FindName("InternalPicker");
    private Popup Popup => _popup ??= (Popup)FindName("DatePopup");

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateDisplayText();
        Picker.SelectedDate = SelectedDate ?? DateTime.Today;
        SubscribeToPickerChanges();
    }

    private void SubscribeToPickerChanges()
    {
        if (_isSubscribed) return;
        _isSubscribed = true;

        var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
            DateWheelPicker.SelectedDateProperty, typeof(DateWheelPicker));
        dpd?.AddValueChanged(Picker, (_, _) =>
        {
            if (Popup.IsOpen && Picker.SelectedDate.HasValue)
            {
                SelectedDate = Picker.SelectedDate;
            }
        });
    }

    private void PickerButton_Click(object sender, RoutedEventArgs e)
    {
        Picker.SelectedDate = SelectedDate ?? DateTime.Today;
        Popup.IsOpen = true;
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DatePickerButton btn)
        {
            btn.UpdateDisplayText();
            if (btn.IsLoaded)
            {
                btn.Picker.SelectedDate = (DateTime?)e.NewValue ?? DateTime.Today;
            }
        }
    }

    private void UpdateDisplayText()
    {
        if (SelectedDate.HasValue)
        {
            var d = SelectedDate.Value;
            DisplayText = $"{d.Day} {MonthNames[d.Month - 1]} {d.Year}";
        }
        else
        {
            DisplayText = "اختر تاريخ...";
        }
    }
}
