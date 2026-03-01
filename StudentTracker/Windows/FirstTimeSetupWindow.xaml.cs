using StudentTracker.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StudentTracker.Windows;

public partial class FirstTimeSetupWindow : Window
{
    private readonly DatabaseService _db;
    private readonly Dictionary<(int Year, int Semester), StackPanel> _semesterInputs = new();
    private int _selectedYears = 1;
    private int _selectedSemesters = 2;

    public FirstTimeSetupWindow(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        
        // Initialize with default values
        BuildTables(_selectedYears, _selectedSemesters);
    }

    private void YearsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (YearsComboBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out var years))
        {
            _selectedYears = years;
            UpdateTables();
        }
    }

    private void SemestersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (SemestersComboBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out var semesters))
        {
            _selectedSemesters = semesters;
            UpdateTables();
        }
    }

    private void UpdateTables()
    {
        UpdateInfoText();
        BuildTables(_selectedYears, _selectedSemesters);
    }

    private void UpdateInfoText()
    {
        var totalSemesters = _selectedYears * _selectedSemesters;
        InfoText.Text = $"إجمالي الفصول: {totalSemesters} فصل دراسي | إجمالي السنوات: {_selectedYears} سنة";
    }

    private void BuildTables(int years, int semesters)
    {
        TablesHost.Items.Clear();
        _semesterInputs.Clear();

        for (var year = 1; year <= years; year++)
        {
            for (var semester = 1; semester <= semesters; semester++)
            {
                // Create a card for each semester
                var cardBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(16),
                    Margin = new Thickness(8),
                    Width = 320,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0")),
                    BorderThickness = new Thickness(1)
                };

                var cardContent = new StackPanel();

                // Header with year and semester
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
                
                // Icon for semester
                var semesterIcon = new System.Windows.Shapes.Ellipse
                {
                    Width = 32,
                    Height = 32,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                    Margin = new Thickness(0, 0, 10, 0)
                };
                
                var iconText = new TextBlock
                {
                    Text = $"{year}/{semester}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                var iconPanel = new Grid { Width = 32, Height = 32, Margin = new Thickness(0, 0, 10, 0) };
                iconPanel.Children.Add(semesterIcon);
                iconPanel.Children.Add(iconText);
                headerPanel.Children.Add(iconPanel);

                var titleText = new TextBlock
                {
                    Text = $"السنة {ToArabicOrdinal(year)} - {GetSemesterName(semester)}",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                    VerticalAlignment = VerticalAlignment.Center
                };
                headerPanel.Children.Add(titleText);
                cardContent.Children.Add(headerPanel);

                // Input panel for subjects
                var inputPanel = new StackPanel();
                AddSubjectRow(inputPanel, cardContent);
                _semesterInputs[(year, semester)] = inputPanel;
                cardContent.Children.Add(inputPanel);

                // Add subject button
                var addButton = new Button
                {
                    Content = new StackPanel { Orientation = Orientation.Horizontal },
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 8, 0, 0),
                    Padding = new Thickness(12, 6, 12, 6),
                    Background = Brushes.Transparent,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                    BorderThickness = new Thickness(1),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                    Cursor = Cursors.Hand
                };
                
                var addButtonContent = (StackPanel)addButton.Content;
                addButtonContent.Children.Add(new System.Windows.Shapes.Path
                {
                    Data = System.Windows.Media.Geometry.Parse("M12 4v16m-8-8h16"),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                    StrokeThickness = 2,
                    Width = 14,
                    Height = 14,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 6, 0)
                });
                addButtonContent.Children.Add(new TextBlock { Text = "إضافة مادة" });
                
                addButton.Click += (_, _) => AddSubjectRow(inputPanel, cardContent);
                cardContent.Children.Add(addButton);

                cardBorder.Child = cardContent;
                TablesHost.Items.Add(cardBorder);
            }
        }

        // Enable save button by default when tables are built
        SaveButton.IsEnabled = true;
    }

    private void AddSubjectRow(Panel inputPanel, StackPanel cardContent)
    {
        var rowPanel = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var textBox = new TextBox
        {
            Padding = new Thickness(12, 8, 12, 8),
            FontSize = 13,
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0")),
            BorderThickness = new Thickness(1),
            Background = Brushes.White
        };

        // Add focus effects
        textBox.GotFocus += (_, _) =>
        {
            textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1"));
            textBox.BorderThickness = new Thickness(2);
        };
        textBox.LostFocus += (_, _) =>
        {
            textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            textBox.BorderThickness = new Thickness(1);
        };

        Grid.SetColumn(textBox, 0);
        rowPanel.Children.Add(textBox);

        // Delete button
        var deleteButton = new Button
        {
            Width = 28,
            Height = 28,
            Margin = new Thickness(6, 0, 0, 0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            ToolTip = "حذف"
        };

        var deleteIcon = new System.Windows.Shapes.Path
        {
            Data = System.Windows.Media.Geometry.Parse("M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"),
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
            StrokeThickness = 1.5,
            Width = 14,
            Height = 14,
            Stretch = Stretch.Uniform
        };
        deleteButton.Content = deleteIcon;
        
        deleteButton.Click += (_, _) =>
        {
            if (inputPanel.Children.Count > 1)
            {
                inputPanel.Children.Remove(rowPanel);
            }
            else
            {
                // Clear the text if it's the last row
                textBox.Text = "";
            }
        };
        
        Grid.SetColumn(deleteButton, 1);
        rowPanel.Children.Add(deleteButton);

        inputPanel.Children.Add(rowPanel);
    }

    private void SaveAndStart_Click(object sender, RoutedEventArgs e)
    {
        // Check if user entered subjects in at least one semester
        var hasAnySubjects = false;
        var subjectsBySemester = new Dictionary<(int Year, int Semester), List<string>>();

        foreach (var kv in _semesterInputs)
        {
            var names = kv.Value.Children
                .OfType<Grid>()
                .Select(g => g.Children.OfType<TextBox>().FirstOrDefault())
                .Where(tb => tb != null)
                .Select(tb => tb!.Text.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (names.Count > 0)
            {
                hasAnySubjects = true;
            }
            subjectsBySemester[kv.Key] = names;
        }

        if (!hasAnySubjects)
        {
            CustomMessageBox.ShowWarning("يرجى إضافة مادة واحدة على الأقل في أحد الفصول الدراسية.", "تنبيه", this);
            return;
        }

        _db.SaveStudyPlan(_selectedYears, _selectedSemesters, subjectsBySemester);
        _db.SetSetting("InitialSetupCompleted", "1");

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var result = CustomMessageBox.ShowConfirm(
            "هل أنت متأكد من إلغاء الإعداد؟ ستضطر لإدخال بياناتك مرة أخرى.",
            "تأكيد الإلغاء",
            this);

        if (result)
        {
            DialogResult = false;
            Close();
        }
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

    private static string GetSemesterName(int semester)
    {
        return semester switch
        {
            1 => "الفصل الأول",
            2 => "الفصل الثاني",
            3 => "الفصل الصيفي",
            _ => $"الفصل {semester}"
        };
    }
}
