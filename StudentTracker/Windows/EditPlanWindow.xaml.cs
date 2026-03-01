using StudentTracker.Models;
using StudentTracker.Services;
using System.Windows;
using System.Windows.Controls;

namespace StudentTracker.Windows;

public partial class EditPlanWindow : Window
{
    private readonly IDatabaseService _db;
    private readonly Dictionary<(int Year, int Semester), StackPanel> _semesterInputs = new();
    private Dictionary<(int Year, int Semester), List<Subject>> _existingBySemester = new();

    public EditPlanWindow(IDatabaseService db)
    {
        InitializeComponent();
        _db = db;
        LoadExistingPlan();
    }

    public bool IsSaved { get; private set; }

    private void BuildTables_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(YearsTextBox.Text, out var years) || years <= 0)
        {
            CustomMessageBox.ShowWarning("الرجاء إدخال عدد سنوات صحيح.", "تنبيه", this);
            return;
        }

        if (!int.TryParse(SemestersTextBox.Text, out var semesters) || semesters <= 0)
        {
            CustomMessageBox.ShowWarning("الرجاء إدخال عدد فصول صحيح.", "تنبيه", this);
            return;
        }

        BuildTables(years, semesters);
    }

    private void LoadExistingPlan()
    {
        var subjects = _db.GetAllSubjects();
        _existingBySemester = subjects
            .GroupBy(s => (s.YearNumber, s.SemesterNumber))
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DisplayOrder).ToList());

        var years = Math.Max(1, subjects.Select(s => s.YearNumber).DefaultIfEmpty(1).Max());
        var semesters = Math.Max(2, subjects.Select(s => s.SemesterNumber).DefaultIfEmpty(2).Max());

        YearsTextBox.Text = years.ToString();
        SemestersTextBox.Text = semesters.ToString();

        BuildTables(years, semesters);
    }

    private void BuildTables(int years, int semesters)
    {
        TablesHost.Children.Clear();
        _semesterInputs.Clear();

        for (var year = 1; year <= years; year++)
        {
            for (var sem = 1; sem <= semesters; sem++)
            {
                var border = new Border
                {
                    Background = System.Windows.Media.Brushes.White,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var root = new StackPanel();
                root.Children.Add(new TextBlock
                {
                    Text = $"السنة {year} - الفصل {sem}",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                var inputPanel = new StackPanel();
                if (_existingBySemester.TryGetValue((year, sem), out var existingSubjects) && existingSubjects.Count > 0)
                {
                    foreach (var subject in existingSubjects)
                    {
                        AddSubjectRow(inputPanel, subject.Name, subject.Id, isExisting: true);
                    }
                }
                else
                {
                    AddSubjectRow(inputPanel);
                }

                var addBtn = new Button
                {
                    Content = "+",
                    Width = 40,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                addBtn.Click += (_, _) => AddSubjectRow(inputPanel);

                root.Children.Add(addBtn);
                root.Children.Add(inputPanel);
                border.Child = root;
                TablesHost.Children.Add(border);

                _semesterInputs[(year, sem)] = inputPanel;
            }
        }
    }

    private static void AddSubjectRow(Panel panel, string name = "", int? subjectId = null, bool isExisting = false)
    {
        var textBox = new TextBox
        {
            Width = 350,
            Margin = new Thickness(0, 5, 0, 0),
            Text = name,
            ToolTip = isExisting ? "مادة موجودة (يمكن تعديل الاسم)" : "مادة جديدة"
        };

        textBox.Tag = new SubjectRowTag
        {
            SubjectId = subjectId,
            OriginalName = name
        };

        panel.Children.Add(textBox);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(YearsTextBox.Text, out var years) || years <= 0 ||
            !int.TryParse(SemestersTextBox.Text, out var semesters) || semesters <= 0)
        {
            CustomMessageBox.ShowWarning("الرجاء إدخال قيم صحيحة قبل الحفظ.", "تنبيه", this);
            return;
        }

        if (_semesterInputs.Count == 0)
        {
            CustomMessageBox.ShowWarning("الرجاء الضغط على إنشاء الجداول أولاً.", "تنبيه", this);
            return;
        }

        var data = new Dictionary<(int Year, int Semester), List<PlanSubjectInput>>();

        foreach (var item in _semesterInputs)
        {
            var rows = item.Value.Children
                .OfType<TextBox>()
                .Select((textBox, index) =>
                {
                    var tag = textBox.Tag as SubjectRowTag;
                    var enteredName = textBox.Text.Trim();
                    var finalName = string.IsNullOrWhiteSpace(enteredName)
                        ? tag?.OriginalName ?? string.Empty
                        : enteredName;

                    return new PlanSubjectInput
                    {
                        SubjectId = tag?.SubjectId,
                        Name = finalName,
                        DisplayOrder = index + 1
                    };
                })
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .ToList();

            data[item.Key] = rows;
        }

        _db.UpsertStudyPlan(years, semesters, data);
        IsSaved = true;

        CustomMessageBox.ShowSuccess("تم حفظ المخطط العام بنجاح.", "تم", this);
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private sealed class SubjectRowTag
    {
        public int? SubjectId { get; set; }
        public string OriginalName { get; set; } = string.Empty;
    }
}
