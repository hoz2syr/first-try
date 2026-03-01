namespace StudentTracker.Models;

public class PlanSubjectInput
{
    public int? SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
