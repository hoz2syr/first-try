using System.Collections.Generic;
using System.Windows;

namespace StudentTracker.Windows;

public partial class ChangeSemesterWindow : Window
{
    private readonly int _currentYear;

    public ChangeSemesterWindow(int currentYear, int currentSemester, int totalYears)
    {
        InitializeComponent();

        _currentYear = currentYear;
        SelectedYear = currentYear;

        // السنة الحالية محسوبة تلقائياً (قراءة فقط)
        YearComboBox.ItemsSource = new List<string> { $"السنة {currentYear}" };
        YearComboBox.SelectedIndex = 0;
        YearComboBox.IsEnabled = false;
        YearComboBox.ToolTip = "السنة الحالية تُحسب تلقائياً حسب آلية النجاح";
        
        // Set current semester
        SelectedSemester = currentSemester;
        Semester1Radio.IsChecked = currentSemester == 1;
        Semester2Radio.IsChecked = currentSemester == 2;
        SummerRadio.IsChecked = currentSemester == 3;
    }

    public int SelectedYear { get; private set; }
    public int SelectedSemester { get; private set; }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        // منع التعديل اليدوي للسنة
        SelectedYear = _currentYear;
        
        // Get selected semester
        if (Semester1Radio.IsChecked == true)
            SelectedSemester = 1;
        else if (Semester2Radio.IsChecked == true)
            SelectedSemester = 2;
        else if (SummerRadio.IsChecked == true)
            SelectedSemester = 3;
        
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
