using StudentTracker.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentTracker.Windows;

public partial class FirstTimeSetupWindow : Window
{
    private readonly DatabaseService _db;
    private readonly Dictionary<(int Year, int Semester), StackPanel> _semesterInputs = new();

    public FirstTimeSetupWindow(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        YearsTextBox.TextChanged += YearsOrSemesters_TextChanged;
        SemestersTextBox.TextChanged += YearsOrSemesters_TextChanged;
    }

    private void NumbersOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }

    private void YearsOrSemesters_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(YearsTextBox.Text, out var years) && years > 0 &&
            int.TryParse(SemestersTextBox.Text, out var semesters) && semesters > 0)
        {
            SaveButton.IsEnabled = true;
            // Build tables when valid years/semesters are entered
            BuildTables(years, semesters);
        }
        else
        {
            SaveButton.IsEnabled = false;
        }
    }

    private void BuildTables(int years, int semesters)
    {
        TablesHost.Children.Clear();
        _semesterInputs.Clear();

        for (var year = 1; year <= years; year++)
        {
            for (var semester = 1; semester <= semesters; semester++)
            {
                var border = new Border
                {
                    Background = System.Windows.Media.Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var root = new StackPanel();
                root.Children.Add(new TextBlock
                {
                    Text = $"السنة {ToArabicOrdinal(year)} - الفصل {ToArabicOrdinal(semester)}",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                var inputPanel = new StackPanel();
                AddSubjectRow(inputPanel);

                var addButton = new Button
                {
                    Content = "+",
                    Width = 40,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                addButton.Click += (_, _) => AddSubjectRow(inputPanel);

                root.Children.Add(addButton);
                root.Children.Add(inputPanel);
                border.Child = root;
                TablesHost.Children.Add(border);

                _semesterInputs[(year, semester)] = inputPanel;
            }
        }
    }

    private static void AddSubjectRow(Panel panel)
    {
        panel.Children.Add(new TextBox
        {
            Width = 360,
            Margin = new Thickness(0, 5, 0, 0)
        });
    }

    private void SaveAndStart_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(YearsTextBox.Text, out var years) || years <= 0)
        {
            CustomMessageBox.ShowWarning("أدخل عدد سنوات صحيح.", "تنبيه", this);
            return;
        }
        if (!int.TryParse(SemestersTextBox.Text, out var semesters) || semesters <= 0)
        {
            CustomMessageBox.ShowWarning("أدخل عدد فصول صحيح.", "تنبيه", this);
            return;
        }

        // Do NOT call BuildTables here - it clears user input!

        // تحقق من أن المستخدم أدخل مواد في كل فصل
        var subjectsBySemester = new Dictionary<(int Year, int Semester), List<string>>();
        foreach (var kv in _semesterInputs)
        {
            var names = kv.Value.Children
                .OfType<TextBox>()
                .Select(x => x.Text.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            subjectsBySemester[kv.Key] = names;
        }

        _db.SaveStudyPlan(years, semesters, subjectsBySemester);
        _db.SetSetting("InitialSetupCompleted", "1");

        DialogResult = true;
        Close();
    }

    private static string ToArabicOrdinal(int value)
    {
        return value switch
        {
            1 => "الأولى",
            2 => "الثانية",
            3 => "الثالثة",
            4 => "الرابعة",
            5 => "الخامسة",
            6 => "السادسة",
            _ => value.ToString()
        };
    }
}
