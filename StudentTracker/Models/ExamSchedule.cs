namespace StudentTracker.Models;

public class ExamSchedule
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public DateTime ExamDateTime { get; set; }
}
