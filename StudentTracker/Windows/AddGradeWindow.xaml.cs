using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace StudentTracker.Windows;

public partial class AddGradeWindow : Window
{
    public AddGradeWindow(string subjectName)
    {
        InitializeComponent();
        SubjectName = subjectName;
        AttemptDate = DateTime.Today;
        DataContext = this;
        
        // تفعيل الـ placeholder عند فقدان التركيز
        GradeTextBox.LostFocus += GradeTextBox_LostFocus;
        GradeTextBox.GotFocus += GradeTextBox_GotFocus;
        NotesTextBox.LostFocus += NotesTextBox_LostFocus;
        NotesTextBox.GotFocus += NotesTextBox_GotFocus;
    }
    
    private void GradeTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && !string.IsNullOrEmpty(tb.Text))
        {
            tb.Style = (Style)Application.Current.FindResource("ValidatedTextBoxStyle");
        }
    }
    
    private void GradeTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            // التحقق من صحة العلامة
            if (!double.TryParse(tb.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var grade) &&
                !double.TryParse(tb.Text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out grade))
            {
                tb.Style = (Style)FindResource("InvalidTextBoxStyle");
            }
            else if (grade < 0 || grade > 100)
            {
                tb.Style = (Style)FindResource("InvalidTextBoxStyle");
            }
            else
            {
                tb.Style = (Style)FindResource("ValidatedTextBoxStyle");
            }
        }
    }
    
    private void NotesTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && !string.IsNullOrEmpty(tb.Text))
        {
            tb.Style = (Style)FindResource("ValidatedTextBoxStyle");
        }
    }
    
    private void NotesTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // الملاحظات اختيارية، لا تحتاج تحقق
        if (sender is TextBox tb)
        {
            tb.Style = (Style)FindResource("ValidatedTextBoxStyle");
        }
    }

    public string SubjectName { get; }
    public string AttemptType { get; private set; } = "عملي";
    public double Grade { get; private set; }
    public DateTime AttemptDate { get; private set; }
    public string? Notes { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        bool hasError = false;
        
        // التحقق من العلامة
        if (!double.TryParse(GradeTextBox.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var grade) &&
            !double.TryParse(GradeTextBox.Text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out grade))
        {
            GradeTextBox.Style = (Style)FindResource("InvalidTextBoxStyle");
            hasError = true;
        }
        else if (grade < 0 || grade > 100)
        {
            GradeTextBox.Style = (Style)FindResource("InvalidTextBoxStyle");
            hasError = true;
        }
        
        if (hasError)
        {
            CustomMessageBox.ShowWarning("الرجاء إدخال علامة صحيحة بين 0 و 100.", "تنبيه", this);
            return;
        }

        if (DatePickerControl.SelectedDate is null)
        {
            CustomMessageBox.ShowWarning("الرجاء اختيار تاريخ العلامة.", "تنبيه", this);
            return;
        }

        AttemptType = PracticalRadio.IsChecked == true ? "عملي" : "نظري";
        Grade = grade;
        AttemptDate = DatePickerControl.SelectedDate.Value;
        Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim();

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
