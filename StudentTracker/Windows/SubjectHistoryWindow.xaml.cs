using StudentTracker.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace StudentTracker.Windows;

public partial class SubjectHistoryWindow : Window
{
    public SubjectHistoryWindow(string subjectName, IEnumerable<GradeAttempt> attempts)
    {
        InitializeComponent();
        Title = subjectName;
        SubjectTitle = subjectName;

        var attemptsList = attempts.ToList();
        Attempts = new ObservableCollection<GradeAttempt>(attemptsList);

        LatestPracticalGrade = attemptsList.FirstOrDefault(a => a.AttemptType == "عملي")?.Grade;
        LatestTheoryGrade = attemptsList.FirstOrDefault(a => a.AttemptType == "نظري")?.Grade;
        LatestFinalGrade = LatestTheoryGrade.HasValue ? (LatestPracticalGrade ?? 0) + LatestTheoryGrade.Value : null;

        DataContext = this;
    }

    public string SubjectTitle { get; }
    public ObservableCollection<GradeAttempt> Attempts { get; }
    public double? LatestPracticalGrade { get; }
    public double? LatestTheoryGrade { get; }
    public double? LatestFinalGrade { get; }

    public string LatestPracticalGradeText => LatestPracticalGrade?.ToString("0.##") ?? "—";
    public string LatestTheoryGradeText => LatestTheoryGrade?.ToString("0.##") ?? "—";
    public string LatestFinalGradeText => LatestFinalGrade?.ToString("0.##") ?? "—";

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
