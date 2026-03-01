using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentTracker.Windows;

public partial class AddExamWindow : Window
{
    public AddExamWindow(string subjectName)
    {
        InitializeComponent();
        SubjectName = subjectName;
        ExamDate = DateTime.Today;
        DataContext = this;
        ExamDateTime = DateTime.Today.AddHours(9);
        
        // Set initial date on the date picker
        ExamDatePicker.SelectedDate = ExamDateTime;
        HourTextBox.Text = ExamDateTime.ToString("HH");
        MinuteTextBox.Text = ExamDateTime.ToString("mm");
        
        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        
        // تركيز تلقائي على حقل الساعة عند فتح النافذة
        ContentRendered += (_, _) =>
        {
            HourTextBox.Focus();
            Keyboard.Focus(HourTextBox);
            HourTextBox.SelectAll();
        };
    }
    
    private void ExamTimeTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && !string.IsNullOrEmpty(tb.Text) && tb.Text != "09:00")
        {
            tb.Style = (Style)FindResource("ValidatedTextBoxStyle");
        }
    }
    
    private void ExamTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (!TimeSpan.TryParseExact(tb.Text.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out var time))
            {
                tb.Style = (Style)FindResource("InvalidTextBoxStyle");
            }
            else
            {
                tb.Style = (Style)FindResource("ValidatedTextBoxStyle");
            }
        }
    }

    public string SubjectName { get; }
    public string ExamType { get; private set; } = "عملي";
    public DateTime ExamDate { get; private set; }
    public DateTime ExamDateTime { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Parse time from text boxes
        if (!int.TryParse(HourTextBox.Text.Trim(), out int hour) || hour < 0 || hour > 23)
        {
            CustomMessageBox.ShowWarning("الرجاء إدخال ساعة صحيحة (0-23).", "تنبيه", this);
            return;
        }
        if (!int.TryParse(MinuteTextBox.Text.Trim(), out int minute) || minute < 0 || minute > 59)
        {
            CustomMessageBox.ShowWarning("الرجاء إدخال دقيقة صحيحة (0-59).", "تنبيه", this);
            return;
        }
        
        var selectedDate = ExamDatePicker.SelectedDate ?? DateTime.Today;
        ExamDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hour, minute, 0);
        
        if (ExamDateTime == default)
        {
            CustomMessageBox.ShowWarning("الرجاء اختيار تاريخ ووقت صحيح.", "تنبيه", this);
            return;
        }
        ExamType = PracticalRadio.IsChecked == true ? "عملي" : "نظري";
        ExamDate = ExamDateTime.Date;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
