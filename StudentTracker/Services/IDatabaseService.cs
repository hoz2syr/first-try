using StudentTracker.Models;

namespace StudentTracker.Services;

public interface IDatabaseService
{
    void InitializeDatabase();
    int GetCurrentSemester();
    void SetCurrentSemester(int semester);
    string? GetSetting(string key);
    void SetSetting(string key, string value);
    List<Subject> GetAllSubjects();
    IEnumerable<Subject> GetSubjectsByYear(int yearNumber);
    List<StudyYear> GetStudyYears();
    HashSet<int> GetRetakeLabSubjectIds();
    void MarkRetakeLab(int subjectId);
    void ClearRetakeLab(int subjectId);
    double? GetLatestGrade(int subjectId, string type);
    DateTime? GetLatestExamDate(int subjectId, string examType);
    bool HasExamConflict(DateTime examDateTime, int? ignoreSubjectId = null);
    void AddGradeAttempt(GradeAttempt gradeAttempt);
    List<GradeAttempt> GetSubjectHistory(int subjectId);
    List<Note> GetAllNotes();
    void AddNote(Note note);
    void DeleteNote(int noteId);
    List<ExamSchedule> GetAllExamSchedules();
    void UpsertExamSchedule(int subjectId, string examType, DateTime examDateTime);
    void UpsertStudyPlan(int yearsCount, int semestersPerYear, Dictionary<(int Year, int Semester), List<PlanSubjectInput>> subjectsBySemester);
}