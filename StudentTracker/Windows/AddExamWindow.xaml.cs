using System.Globalization;
using System.Windows;
using System.Windows.Controls;

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
        UpdateExamDateTimeTextBox();
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
            // التحقق من صحة الوقت
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

    private void PickDateTime_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new DateTimeDialog(ExamDateTime, ExamDateTime.ToString("HH:mm"));
        dialog.Owner = this;
        if (dialog.ShowDialog() == true && dialog.SelectedDateTime.HasValue)
        {
            ExamDateTime = dialog.SelectedDateTime.Value;
            UpdateExamDateTimeTextBox();
        }
    }

    private void UpdateExamDateTimeTextBox()
    {
        if (ExamDateTimeTextBox != null)
            ExamDateTimeTextBox.Text = ExamDateTime.ToString("yyyy/MM/dd HH:mm");
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
