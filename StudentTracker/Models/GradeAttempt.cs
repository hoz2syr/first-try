namespace StudentTracker.Models;

public class GradeAttempt
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string AttemptType { get; set; } = string.Empty;
    public double Grade { get; set; }
    public DateTime AttemptDate { get; set; }
    public string? Notes { get; set; }
}
