using System.Collections.ObjectModel;

namespace StudentTracker.Models;

public class SemesterGroupViewModel
{
    public int YearNumber { get; set; }
    public int SemesterNumber { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    /// <summary>Alias for Title used by XAML bindings.</summary>
    public string Header => Title;
    public ObservableCollection<SubjectViewModel> Subjects { get; set; } = new();
}
