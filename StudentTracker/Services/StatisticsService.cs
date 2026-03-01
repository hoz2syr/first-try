using StudentTracker.Models;

namespace StudentTracker.Services;

public class StatisticsService
{
    private readonly IDatabaseService _db;

    public StatisticsService(IDatabaseService db)
    {
        _db = db;
    }

    public (int passed, int failed, int pending, int remaining, double percentage) CalculateCurrentYearStatistics(int currentYear)
    {
        var subjects = _db.GetAllSubjects().Where(s => s.YearNumber == currentYear).ToList();
        var retakeIds = _db.GetRetakeLabSubjectIds();

        int passed = 0, failed = 0, pending = 0, remaining = 0;

        foreach (var subject in subjects)
        {
            var practical = _db.GetLatestGrade(subject.Id, "عملي");
            var theory = _db.GetLatestGrade(subject.Id, "نظري");
            var status = GradeCalculator.ComputeStatus(currentYear, subject.YearNumber, practical, theory, retakeIds.Contains(subject.Id));

            switch (status)
            {
                case SubjectStatus.Passed: passed++; break;
                case SubjectStatus.Failed: failed++; break;
                case SubjectStatus.Pending: pending++; break;
                default: remaining++; break;
            }
        }

        var total = subjects.Count;
        var completedSubjects = passed + failed;
        var percentage = total > 0 ? (completedSubjects * 100.0 / total) : 0;

        return (passed, failed, pending, remaining, percentage);
    }

    public (int passed, int total, double percentage, double average) CalculateOverallStatistics()
    {
        var subjects = _db.GetAllSubjects();
        var retakeIds = _db.GetRetakeLabSubjectIds();

        int passed = 0;
        double totalGrades = 0;
        int gradeCount = 0;

        foreach (var subject in subjects)
        {
            var practical = _db.GetLatestGrade(subject.Id, "عملي");
            var theory = _db.GetLatestGrade(subject.Id, "نظري");
            var finalGrade = GradeCalculator.ComputeFinalGrade(practical, theory, retakeIds.Contains(subject.Id));

            if (finalGrade.HasValue && finalGrade >= 50)
            {
                passed++;
                totalGrades += finalGrade.Value;
                gradeCount++;
            }
        }

        var total = subjects.Count();
        var percentage = total > 0 ? (passed * 100.0 / total) : 0;
        var average = gradeCount > 0 ? totalGrades / gradeCount : 0;

        return (passed, total, percentage, average);
    }
}