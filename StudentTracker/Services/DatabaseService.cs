using Microsoft.Data.Sqlite;
using StudentTracker.Models;
using System.IO;

namespace StudentTracker.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        _dbPath = Path.Combine(AppContext.BaseDirectory, "student_tracker.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        var commands = new[]
        {
            @"CREATE TABLE IF NOT EXISTS StudyYears (
                Id INTEGER PRIMARY KEY,
                YearNumber INTEGER NOT NULL
            );",
            @"CREATE TABLE IF NOT EXISTS Semesters (
                Id INTEGER PRIMARY KEY,
                YearId INTEGER REFERENCES StudyYears(Id),
                SemesterNumber INTEGER NOT NULL
            );",
            @"CREATE TABLE IF NOT EXISTS Subjects (
                Id INTEGER PRIMARY KEY,
                SemesterId INTEGER REFERENCES Semesters(Id),
                Name TEXT NOT NULL,
                DisplayOrder INTEGER
            );",
            @"CREATE TABLE IF NOT EXISTS GradeAttempts (
                Id INTEGER PRIMARY KEY,
                SubjectId INTEGER REFERENCES Subjects(Id),
                AttemptType TEXT NOT NULL,
                Grade REAL NOT NULL,
                AttemptDate TEXT NOT NULL,
                Notes TEXT
            );",
            @"CREATE TABLE IF NOT EXISTS ExamSchedule (
                Id INTEGER PRIMARY KEY,
                SubjectId INTEGER REFERENCES Subjects(Id),
                ExamType TEXT NOT NULL,
                ExamDateTime TEXT NOT NULL
            );",
            @"CREATE TABLE IF NOT EXISTS AppSettings (
                Key TEXT PRIMARY KEY,
                Value TEXT
            );",
            @"CREATE TABLE IF NOT EXISTS Notes (
                Id INTEGER PRIMARY KEY,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedDate TEXT NOT NULL,
                ModifiedDate TEXT NOT NULL
            );"
        };

        foreach (var sql in commands)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        if (GetSetting("CurrentSemester") is null)
        {
            SetSetting("CurrentSemester", "1");
        }
    }

    public List<StudyYear> GetStudyYears()
    {
        var result = new List<StudyYear>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, YearNumber FROM StudyYears ORDER BY YearNumber";
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new StudyYear
            {
                Id = reader.GetInt32(0),
                YearNumber = reader.GetInt32(1)
            });
        }

        return result;
    }

    public bool HasStudyPlan()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Subjects";
        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    public List<Subject> GetAllSubjects()
    {
        var result = new List<Subject>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.Id, s.SemesterId, y.YearNumber, sem.SemesterNumber, s.Name, COALESCE(s.DisplayOrder, 0)
                                FROM Subjects s
                                JOIN Semesters sem ON sem.Id = s.SemesterId
                                JOIN StudyYears y ON y.Id = sem.YearId
                                ORDER BY y.YearNumber, sem.SemesterNumber, COALESCE(s.DisplayOrder, 0), s.Id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Subject
            {
                Id = reader.GetInt32(0),
                SemesterId = reader.GetInt32(1),
                YearNumber = reader.GetInt32(2),
                SemesterNumber = reader.GetInt32(3),
                Name = reader.GetString(4),
                DisplayOrder = reader.GetInt32(5)
            });
        }

        return result;
    }

    public IEnumerable<Subject> GetSubjectsByYear(int yearNumber) =>
        GetAllSubjects().Where(s => s.YearNumber == yearNumber);

    public double? GetLatestGrade(int subjectId, string attemptType)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Grade
                                    FROM GradeAttempts
                                    WHERE SubjectId = $subjectId AND AttemptType = $attemptType
                                    ORDER BY datetime(AttemptDate) DESC, Id DESC
                                    LIMIT 1";
            command.Parameters.AddWithValue("$subjectId", subjectId);
            command.Parameters.AddWithValue("$attemptType", attemptType);

            var value = command.ExecuteScalar();
            return value is null ? null : Convert.ToDouble(value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting latest grade: {ex.Message}");
            return null;
        }
    }

    public void AddGradeAttempt(GradeAttempt gradeAttempt)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO GradeAttempts (SubjectId, AttemptType, Grade, AttemptDate, Notes)
                                VALUES ($subjectId, $attemptType, $grade, $attemptDate, $notes)";

        command.Parameters.AddWithValue("$subjectId", gradeAttempt.SubjectId);
        command.Parameters.AddWithValue("$attemptType", gradeAttempt.AttemptType);
        command.Parameters.AddWithValue("$grade", gradeAttempt.Grade);
        command.Parameters.AddWithValue("$attemptDate", gradeAttempt.AttemptDate.ToString("o"));
        command.Parameters.AddWithValue("$notes", (object?)gradeAttempt.Notes ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public List<GradeAttempt> GetSubjectHistory(int subjectId)
    {
        var result = new List<GradeAttempt>();
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, SubjectId, AttemptType, Grade, AttemptDate, Notes
                                    FROM GradeAttempts
                                    WHERE SubjectId = $subjectId
                                    ORDER BY datetime(AttemptDate) DESC, Id DESC";
            command.Parameters.AddWithValue("$subjectId", subjectId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var attemptDateStr = reader.GetString(4);
                if (DateTime.TryParse(attemptDateStr, out var attemptDate))
                {
                    result.Add(new GradeAttempt
                    {
                        Id = reader.GetInt32(0),
                        SubjectId = reader.GetInt32(1),
                        AttemptType = reader.GetString(2),
                        Grade = reader.GetDouble(3),
                        AttemptDate = attemptDate,
                        Notes = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting subject history: {ex.Message}");
        }
        return result;
    }

    public bool HasExamConflict(DateTime examDateTime, int? ignoreSubjectId = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT COUNT(*)
                                FROM ExamSchedule
                                WHERE ExamDateTime = $examDateTime
                                AND ($ignoreSubjectId IS NULL OR SubjectId <> $ignoreSubjectId)";
        command.Parameters.AddWithValue("$examDateTime", examDateTime.ToString("o"));
        command.Parameters.AddWithValue("$ignoreSubjectId", ignoreSubjectId is null ? DBNull.Value : ignoreSubjectId.Value);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public void UpsertExamSchedule(int subjectId, string examType, DateTime examDateTime)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // حذف الامتحان القديم أولاً
        using var deleteCmd = connection.CreateCommand();
        deleteCmd.Transaction = transaction;
        deleteCmd.CommandText = "DELETE FROM ExamSchedule WHERE SubjectId = $subjectId AND ExamType = $examType";
        deleteCmd.Parameters.AddWithValue("$subjectId", subjectId);
        deleteCmd.Parameters.AddWithValue("$examType", examType);
        deleteCmd.ExecuteNonQuery();

        // إضافة الامتحان الجديد
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"INSERT INTO ExamSchedule (SubjectId, ExamType, ExamDateTime)
                                VALUES ($subjectId, $examType, $examDateTime)";
        command.Parameters.AddWithValue("$subjectId", subjectId);
        command.Parameters.AddWithValue("$examType", examType);
        command.Parameters.AddWithValue("$examDateTime", examDateTime.ToString("o"));
        command.ExecuteNonQuery();

        transaction.Commit();
    }

    public DateTime? GetLatestExamDate(int subjectId, string examType)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT ExamDateTime
                                    FROM ExamSchedule
                                    WHERE SubjectId = $subjectId AND ExamType = $examType
                                    ORDER BY datetime(ExamDateTime) DESC, Id DESC
                                    LIMIT 1";
            command.Parameters.AddWithValue("$subjectId", subjectId);
            command.Parameters.AddWithValue("$examType", examType);

            var value = command.ExecuteScalar()?.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : 
                DateTime.TryParse(value, out var date) ? date : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting latest exam date: {ex.Message}");
            return null;
        }
    }

    public List<ExamSchedule> GetAllExamSchedules()
    {
        var result = new List<ExamSchedule>();
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, SubjectId, ExamType, ExamDateTime
                                    FROM ExamSchedule
                                    ORDER BY ExamDateTime";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var examDateTimeStr = reader.GetString(3);
                if (DateTime.TryParse(examDateTimeStr, out var examDateTime))
                {
                    result.Add(new ExamSchedule
                    {
                        Id = reader.GetInt32(0),
                        SubjectId = reader.GetInt32(1),
                        ExamType = reader.GetString(2),
                        ExamDateTime = examDateTime
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting exam schedules: {ex.Message}");
        }
        return result;
    }

    public int GetCurrentSemester()
    {
        var value = GetSetting("CurrentSemester");
        return int.TryParse(value, out var semester) && (semester >= 1 && semester <= 3) ? semester : 1;
    }

    public void SetCurrentSemester(int semester)
    {
        SetSetting("CurrentSemester", semester.ToString());
    }

    public HashSet<int> GetRetakeLabSubjectIds()
    {
        var value = GetSetting("RetakeLabSubjects");
        return string.IsNullOrWhiteSpace(value) ? [] : value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v => int.TryParse(v, out var id) ? id : -1)
            .Where(id => id > 0)
            .ToHashSet();
    }

    public bool IsRetakeLab(int subjectId) => GetRetakeLabSubjectIds().Contains(subjectId);

    public void MarkRetakeLab(int subjectId)
    {
        var ids = GetRetakeLabSubjectIds();
        ids.Add(subjectId);
        SetSetting("RetakeLabSubjects", string.Join(',', ids.OrderBy(i => i)));
    }

    public void ClearRetakeLab(int subjectId)
    {
        var ids = GetRetakeLabSubjectIds();
        if (ids.Remove(subjectId))
        {
            SetSetting("RetakeLabSubjects", string.Join(',', ids.OrderBy(i => i)));
        }
    }

    public string? GetSetting(string key)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM AppSettings WHERE Key = $key LIMIT 1";
        command.Parameters.AddWithValue("$key", key);
        return command.ExecuteScalar()?.ToString();
    }

    public void SetSetting(string key, string value)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO AppSettings (Key, Value)
                                VALUES ($key, $value)
                                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value";
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    public void SaveStudyPlan(int yearsCount, int semestersPerYear, Dictionary<(int Year, int Semester), List<string>> subjectsBySemester)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var deleteCommands = new[]
        {
            "DELETE FROM ExamSchedule",
            "DELETE FROM GradeAttempts",
            "DELETE FROM Subjects",
            "DELETE FROM Semesters",
            "DELETE FROM StudyYears"
        };

        foreach (var sql in deleteCommands)
        {
            using var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = sql;
            deleteCommand.ExecuteNonQuery();
        }

        for (var year = 1; year <= yearsCount; year++)
        {
            using var yearCommand = connection.CreateCommand();
            yearCommand.Transaction = transaction;
            yearCommand.CommandText = "INSERT INTO StudyYears (YearNumber) VALUES ($yearNumber); SELECT last_insert_rowid();";
            yearCommand.Parameters.AddWithValue("$yearNumber", year);
            var yearId = Convert.ToInt32((long)yearCommand.ExecuteScalar()!);

            for (var semester = 1; semester <= semestersPerYear; semester++)
            {
                using var semCommand = connection.CreateCommand();
                semCommand.Transaction = transaction;
                semCommand.CommandText = "INSERT INTO Semesters (YearId, SemesterNumber) VALUES ($yearId, $semesterNumber); SELECT last_insert_rowid();";
                semCommand.Parameters.AddWithValue("$yearId", yearId);
                semCommand.Parameters.AddWithValue("$semesterNumber", semester);
                var semesterId = Convert.ToInt32((long)semCommand.ExecuteScalar()!);

                if (!subjectsBySemester.TryGetValue((year, semester), out var names))
                {
                    continue;
                }

                var order = 1;
                foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    using var subCommand = connection.CreateCommand();
                    subCommand.Transaction = transaction;
                    subCommand.CommandText = "INSERT INTO Subjects (SemesterId, Name, DisplayOrder) VALUES ($semesterId, $name, $displayOrder)";
                    subCommand.Parameters.AddWithValue("$semesterId", semesterId);
                    subCommand.Parameters.AddWithValue("$name", name.Trim());
                    subCommand.Parameters.AddWithValue("$displayOrder", order++);
                    subCommand.ExecuteNonQuery();
                }
            }
        }

        using var clearRetake = connection.CreateCommand();
        clearRetake.Transaction = transaction;
        clearRetake.CommandText = "DELETE FROM AppSettings WHERE Key = 'RetakeLabSubjects'";
        clearRetake.ExecuteNonQuery();

        transaction.Commit();
    }

    public void UpsertStudyPlan(int yearsCount, int semestersPerYear, Dictionary<(int Year, int Semester), List<PlanSubjectInput>> subjectsBySemester)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        for (var year = 1; year <= yearsCount; year++)
        {
            var yearId = EnsureYear(connection, transaction, year);
            for (var semester = 1; semester <= semestersPerYear; semester++)
            {
                EnsureSemester(connection, transaction, yearId, semester);
            }
        }

        foreach (var kv in subjectsBySemester)
        {
            var yearId = EnsureYear(connection, transaction, kv.Key.Year);
            var semesterId = EnsureSemester(connection, transaction, yearId, kv.Key.Semester);

            var order = 1;
            foreach (var item in kv.Value.Where(v => !string.IsNullOrWhiteSpace(v.Name)))
            {
                if (item.SubjectId.HasValue)
                {
                    using var update = connection.CreateCommand();
                    update.Transaction = transaction;
                    update.CommandText = @"UPDATE Subjects
                                           SET Name = $name, DisplayOrder = $displayOrder
                                           WHERE Id = $id";
                    update.Parameters.AddWithValue("$name", item.Name.Trim());
                    update.Parameters.AddWithValue("$displayOrder", order++);
                    update.Parameters.AddWithValue("$id", item.SubjectId.Value);
                    update.ExecuteNonQuery();
                }
                else
                {
                    using var insert = connection.CreateCommand();
                    insert.Transaction = transaction;
                    insert.CommandText = @"INSERT INTO Subjects (SemesterId, Name, DisplayOrder)
                                           VALUES ($semesterId, $name, $displayOrder)";
                    insert.Parameters.AddWithValue("$semesterId", semesterId);
                    insert.Parameters.AddWithValue("$name", item.Name.Trim());
                    insert.Parameters.AddWithValue("$displayOrder", order++);
                    insert.ExecuteNonQuery();
                }
            }
        }

        transaction.Commit();
    }

    private static int EnsureYear(SqliteConnection connection, SqliteTransaction transaction, int yearNumber)
    {
        using var select = connection.CreateCommand();
        select.Transaction = transaction;
        select.CommandText = "SELECT Id FROM StudyYears WHERE YearNumber = $yearNumber LIMIT 1";
        select.Parameters.AddWithValue("$yearNumber", yearNumber);

        var existing = select.ExecuteScalar();
        if (existing is not null)
        {
            return Convert.ToInt32((long)existing);
        }

        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = "INSERT INTO StudyYears (YearNumber) VALUES ($yearNumber); SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$yearNumber", yearNumber);
        return Convert.ToInt32((long)insert.ExecuteScalar()!);
    }

    private static int EnsureSemester(SqliteConnection connection, SqliteTransaction transaction, int yearId, int semesterNumber)
    {
        using var select = connection.CreateCommand();
        select.Transaction = transaction;
        select.CommandText = @"SELECT Id
                               FROM Semesters
                               WHERE YearId = $yearId AND SemesterNumber = $semesterNumber
                               LIMIT 1";
        select.Parameters.AddWithValue("$yearId", yearId);
        select.Parameters.AddWithValue("$semesterNumber", semesterNumber);

        var existing = select.ExecuteScalar();
        if (existing is not null)
        {
            return Convert.ToInt32((long)existing);
        }

        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = "INSERT INTO Semesters (YearId, SemesterNumber) VALUES ($yearId, $semesterNumber); SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$yearId", yearId);
        insert.Parameters.AddWithValue("$semesterNumber", semesterNumber);
        return Convert.ToInt32((long)insert.ExecuteScalar()!);
    }

    // Notes methods
    public void AddNote(Note note)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Notes (Title, Content, CreatedDate, ModifiedDate)
                                VALUES ($title, $content, $createdDate, $modifiedDate)";
        command.Parameters.AddWithValue("$title", note.Title);
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$createdDate", note.CreatedDate.ToString("o"));
        command.Parameters.AddWithValue("$modifiedDate", note.ModifiedDate.ToString("o"));
        command.ExecuteNonQuery();
    }

    public List<Note> GetAllNotes()
    {
        var result = new List<Note>();
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Content, CreatedDate, ModifiedDate FROM Notes ORDER BY CreatedDate DESC";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var createdDateStr = reader.GetString(3);
                var modifiedDateStr = reader.GetString(4);
                
                if (DateTime.TryParse(createdDateStr, out var createdDate) && 
                    DateTime.TryParse(modifiedDateStr, out var modifiedDate))
                {
                    result.Add(new Note
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Content = reader.GetString(2),
                        CreatedDate = createdDate,
                        ModifiedDate = modifiedDate
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting notes: {ex.Message}");
        }
        return result;
    }

    public void DeleteNote(int noteId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Notes WHERE Id = $id";
        command.Parameters.AddWithValue("$id", noteId);
        command.ExecuteNonQuery();
    }

    public void UpdateNote(Note note)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE Notes SET Title = $title, Content = $content, ModifiedDate = $modifiedDate WHERE Id = $id";
        command.Parameters.AddWithValue("$id", note.Id);
        command.Parameters.AddWithValue("$title", note.Title);
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$modifiedDate", note.ModifiedDate.ToString("o"));
        command.ExecuteNonQuery();
    }
}
