namespace StudentTracker.Models;

public class Subject
{
    public int Id { get; set; }
    public int SemesterId { get; set; }
    public int YearNumber { get; set; }
    public int SemesterNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
