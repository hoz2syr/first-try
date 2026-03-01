using Microsoft.Data.Sqlite;
using StudentTracker.Models;
using System.IO;

namespace StudentTracker.Services;

/// <summary>
/// خدمة قاعدة البيانات - تُدير جميع عمليات SQLite للتطبيق.
/// </summary>
/// <remarks>
/// قاعدة البيانات: <c>student_tracker.db</c> في مجلد التطبيق.<br/>
/// تدعم: السنوات، الفصول، المواد، الدرجات، الامتحانات، الملاحظات والإعدادات.
/// </remarks>
public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    /// <summary>إنشاء خدمة قاعدة البيانات.</summary>
    public DatabaseService()
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "student_tracker.db");
        _connectionString = $"Data Source={dbPath}";
    }

    #region التهيئة

    /// <inheritdoc />
    public void InitializeDatabase()
    {
        using var connection = Open();

        // تفعيل المفاتيح الأجنبية
        Execute(connection, "PRAGMA foreign_keys = ON;");

        // إنشاء الجداول
        var ddlStatements = new[]
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

        foreach (var sql in ddlStatements)
            Execute(connection, sql);

        // القيمة الافتراضية للفصل الحالي
        if (GetSetting("CurrentSemester") is null)
            SetSetting("CurrentSemester", "1");
    }

    #endregion

    #region التحقق من الخطة

    /// <inheritdoc />
    public bool HasStudyPlan()
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Subjects";
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    #endregion

    #region السنوات والمواد

    /// <inheritdoc />
    public List<StudyYear> GetStudyYears()
    {
        var result = new List<StudyYear>();
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, YearNumber FROM StudyYears ORDER BY YearNumber";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            result.Add(new StudyYear { Id = reader.GetInt32(0), YearNumber = reader.GetInt32(1) });

        return result;
    }

    /// <inheritdoc />
    public List<Subject> GetAllSubjects()
    {
        var result = new List<Subject>();
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT s.Id, s.SemesterId, y.YearNumber, sem.SemesterNumber, s.Name, COALESCE(s.DisplayOrder, 0)
            FROM Subjects s
            JOIN Semesters sem ON sem.Id = s.SemesterId
            JOIN StudyYears y  ON y.Id  = sem.YearId
            ORDER BY y.YearNumber, sem.SemesterNumber, COALESCE(s.DisplayOrder, 0), s.Id";

        using var reader = cmd.ExecuteReader();
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

    /// <inheritdoc />
    public IEnumerable<Subject> GetSubjectsByYear(int yearNumber) =>
        GetAllSubjects().Where(s => s.YearNumber == yearNumber);

    #endregion

    #region الدرجات

    /// <inheritdoc />
    public double? GetLatestGrade(int subjectId, string attemptType)
    {
        try
        {
            using var connection = Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Grade FROM GradeAttempts
                WHERE SubjectId = $subjectId AND AttemptType = $attemptType
                ORDER BY datetime(AttemptDate) DESC, Id DESC
                LIMIT 1";
            cmd.Parameters.AddWithValue("$subjectId", subjectId);
            cmd.Parameters.AddWithValue("$attemptType", attemptType);

            var value = cmd.ExecuteScalar();
            return value is null ? null : Convert.ToDouble(value);
        }
        catch (Exception ex)
        {
            Logger.LogError($"خطأ في جلب درجة المادة {subjectId}", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public void AddGradeAttempt(GradeAttempt ga)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO GradeAttempts (SubjectId, AttemptType, Grade, AttemptDate, Notes)
            VALUES ($subjectId, $attemptType, $grade, $attemptDate, $notes)";
        cmd.Parameters.AddWithValue("$subjectId", ga.SubjectId);
        cmd.Parameters.AddWithValue("$attemptType", ga.AttemptType);
        cmd.Parameters.AddWithValue("$grade", ga.Grade);
        cmd.Parameters.AddWithValue("$attemptDate", ga.AttemptDate.ToString("o"));
        cmd.Parameters.AddWithValue("$notes", (object?)ga.Notes ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public List<GradeAttempt> GetSubjectHistory(int subjectId)
    {
        var result = new List<GradeAttempt>();
        try
        {
            using var connection = Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, SubjectId, AttemptType, Grade, AttemptDate, Notes
                FROM GradeAttempts
                WHERE SubjectId = $subjectId
                ORDER BY datetime(AttemptDate) DESC, Id DESC";
            cmd.Parameters.AddWithValue("$subjectId", subjectId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader.GetString(4), out var attemptDate))
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
            Logger.LogError($"خطأ في جلب سجل المادة {subjectId}", ex);
        }
        return result;
    }

    #endregion

    #region إعادة العملي

    /// <inheritdoc />
    public HashSet<int> GetRetakeLabSubjectIds()
    {
        var value = GetSetting("RetakeLabSubjects");
        return string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Select(v => int.TryParse(v, out var id) ? id : -1)
                   .Where(id => id > 0)
                   .ToHashSet();
    }

    /// <inheritdoc />
    public void MarkRetakeLab(int subjectId)
    {
        var ids = GetRetakeLabSubjectIds();
        ids.Add(subjectId);
        SetSetting("RetakeLabSubjects", string.Join(',', ids.OrderBy(i => i)));
    }

    /// <inheritdoc />
    public void ClearRetakeLab(int subjectId)
    {
        var ids = GetRetakeLabSubjectIds();
        if (ids.Remove(subjectId))
            SetSetting("RetakeLabSubjects", string.Join(',', ids.OrderBy(i => i)));
    }

    #endregion

    #region الامتحانات

    /// <inheritdoc />
    public bool HasExamConflict(DateTime examDateTime, int? ignoreSubjectId = null)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM ExamSchedule
            WHERE ExamDateTime = $examDateTime
            AND ($ignoreSubjectId IS NULL OR SubjectId <> $ignoreSubjectId)";
        cmd.Parameters.AddWithValue("$examDateTime", examDateTime.ToString("o"));
        cmd.Parameters.AddWithValue("$ignoreSubjectId",
            ignoreSubjectId is null ? DBNull.Value : ignoreSubjectId.Value);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    /// <inheritdoc />
    public void UpsertExamSchedule(int subjectId, string examType, DateTime examDateTime)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();

        // حذف الموعد القديم
        var del = connection.CreateCommand();
        del.Transaction = transaction;
        del.CommandText = "DELETE FROM ExamSchedule WHERE SubjectId = $subjectId AND ExamType = $examType";
        del.Parameters.AddWithValue("$subjectId", subjectId);
        del.Parameters.AddWithValue("$examType", examType);
        del.ExecuteNonQuery();

        // إدخال الموعد الجديد
        var ins = connection.CreateCommand();
        ins.Transaction = transaction;
        ins.CommandText = @"
            INSERT INTO ExamSchedule (SubjectId, ExamType, ExamDateTime)
            VALUES ($subjectId, $examType, $examDateTime)";
        ins.Parameters.AddWithValue("$subjectId", subjectId);
        ins.Parameters.AddWithValue("$examType", examType);
        ins.Parameters.AddWithValue("$examDateTime", examDateTime.ToString("o"));
        ins.ExecuteNonQuery();

        transaction.Commit();
    }

    /// <inheritdoc />
    public DateTime? GetLatestExamDate(int subjectId, string examType)
    {
        try
        {
            using var connection = Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT ExamDateTime FROM ExamSchedule
                WHERE SubjectId = $subjectId AND ExamType = $examType
                ORDER BY datetime(ExamDateTime) DESC, Id DESC
                LIMIT 1";
            cmd.Parameters.AddWithValue("$subjectId", subjectId);
            cmd.Parameters.AddWithValue("$examType", examType);

            var value = cmd.ExecuteScalar()?.ToString();
            return string.IsNullOrWhiteSpace(value) ? null :
                DateTime.TryParse(value, out var date) ? date : null;
        }
        catch (Exception ex)
        {
            Logger.LogError($"خطأ في جلب موعد امتحان المادة {subjectId}", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public List<ExamSchedule> GetAllExamSchedules()
    {
        var result = new List<ExamSchedule>();
        try
        {
            using var connection = Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, SubjectId, ExamType, ExamDateTime FROM ExamSchedule ORDER BY ExamDateTime";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader.GetString(3), out var examDateTime))
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
            Logger.LogError("خطأ في جلب جدول الامتحانات", ex);
        }
        return result;
    }

    #endregion

    #region الفصل الحالي والإعدادات

    /// <inheritdoc />
    public int GetCurrentSemester()
    {
        var value = GetSetting("CurrentSemester");
        return int.TryParse(value, out var semester) && semester is >= 1 and <= 3 ? semester : 1;
    }

    /// <inheritdoc />
    public void SetCurrentSemester(int semester) =>
        SetSetting("CurrentSemester", semester.ToString());

    /// <inheritdoc />
    public string? GetSetting(string key)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Value FROM AppSettings WHERE Key = $key LIMIT 1";
        cmd.Parameters.AddWithValue("$key", key);
        return cmd.ExecuteScalar()?.ToString();
    }

    /// <inheritdoc />
    public void SetSetting(string key, string value)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO AppSettings (Key, Value) VALUES ($key, $value)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value";
        cmd.Parameters.AddWithValue("$key", key);
        cmd.Parameters.AddWithValue("$value", value);
        cmd.ExecuteNonQuery();
    }

    #endregion

    #region الملاحظات

    /// <inheritdoc />
    public void AddNote(Note note)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Notes (Title, Content, CreatedDate, ModifiedDate)
            VALUES ($title, $content, $createdDate, $modifiedDate)";
        cmd.Parameters.AddWithValue("$title", note.Title);
        cmd.Parameters.AddWithValue("$content", note.Content);
        cmd.Parameters.AddWithValue("$createdDate", note.CreatedDate.ToString("o"));
        cmd.Parameters.AddWithValue("$modifiedDate", note.ModifiedDate.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public List<Note> GetAllNotes()
    {
        var result = new List<Note>();
        try
        {
            using var connection = Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Title, Content, CreatedDate, ModifiedDate FROM Notes ORDER BY CreatedDate DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParse(reader.GetString(3), out var created) &&
                    DateTime.TryParse(reader.GetString(4), out var modified))
                {
                    result.Add(new Note
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Content = reader.GetString(2),
                        CreatedDate = created,
                        ModifiedDate = modified
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("خطأ في جلب الملاحظات", ex);
        }
        return result;
    }

    /// <inheritdoc />
    public void DeleteNote(int noteId)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Notes WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", noteId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>تحديث ملاحظة موجودة.</summary>
    public void UpdateNote(Note note)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Notes SET Title = $title, Content = $content, ModifiedDate = $modifiedDate
            WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", note.Id);
        cmd.Parameters.AddWithValue("$title", note.Title);
        cmd.Parameters.AddWithValue("$content", note.Content);
        cmd.Parameters.AddWithValue("$modifiedDate", note.ModifiedDate.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    #endregion

    #region الخطة الدراسية

    /// <summary>حفظ خطة دراسية جديدة (يحذف البيانات السابقة).</summary>
    public void SaveStudyPlan(int yearsCount, int semestersPerYear,
        Dictionary<(int Year, int Semester), List<string>> subjectsBySemester)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();

        // حذف البيانات السابقة
        foreach (var table in new[] { "ExamSchedule", "GradeAttempts", "Subjects", "Semesters", "StudyYears" })
            Execute(connection, $"DELETE FROM {table}", transaction);

        // إدخال البيانات الجديدة
        for (var year = 1; year <= yearsCount; year++)
        {
            var yearId = InsertAndGetId(connection, transaction,
                "INSERT INTO StudyYears (YearNumber) VALUES ($v); SELECT last_insert_rowid();",
                ("$v", year));

            for (var sem = 1; sem <= semestersPerYear; sem++)
            {
                var semId = InsertAndGetId(connection, transaction,
                    "INSERT INTO Semesters (YearId, SemesterNumber) VALUES ($yearId, $sem); SELECT last_insert_rowid();",
                    ("$yearId", yearId), ("$sem", sem));

                if (!subjectsBySemester.TryGetValue((year, sem), out var names)) continue;

                var order = 1;
                foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = "INSERT INTO Subjects (SemesterId, Name, DisplayOrder) VALUES ($s, $n, $o)";
                    cmd.Parameters.AddWithValue("$s", semId);
                    cmd.Parameters.AddWithValue("$n", name.Trim());
                    cmd.Parameters.AddWithValue("$o", order++);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Execute(connection, "DELETE FROM AppSettings WHERE Key = 'RetakeLabSubjects'", transaction);
        transaction.Commit();
    }

    /// <inheritdoc />
    public void UpsertStudyPlan(int yearsCount, int semestersPerYear,
        Dictionary<(int Year, int Semester), List<PlanSubjectInput>> subjectsBySemester)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();

        // ضمان وجود السنوات والفصول
        for (var year = 1; year <= yearsCount; year++)
        {
            var yearId = EnsureYear(connection, transaction, year);
            for (var sem = 1; sem <= semestersPerYear; sem++)
                EnsureSemester(connection, transaction, yearId, sem);
        }

        // تحديث/إدراج المواد
        foreach (var kv in subjectsBySemester)
        {
            var yearId = EnsureYear(connection, transaction, kv.Key.Year);
            var semId = EnsureSemester(connection, transaction, yearId, kv.Key.Semester);

            var order = 1;
            foreach (var item in kv.Value.Where(v => !string.IsNullOrWhiteSpace(v.Name)))
            {
                if (item.SubjectId.HasValue)
                {
                    var upd = connection.CreateCommand();
                    upd.Transaction = transaction;
                    upd.CommandText = "UPDATE Subjects SET Name = $name, DisplayOrder = $order WHERE Id = $id";
                    upd.Parameters.AddWithValue("$name", item.Name.Trim());
                    upd.Parameters.AddWithValue("$order", order++);
                    upd.Parameters.AddWithValue("$id", item.SubjectId.Value);
                    upd.ExecuteNonQuery();
                }
                else
                {
                    var ins = connection.CreateCommand();
                    ins.Transaction = transaction;
                    ins.CommandText = "INSERT INTO Subjects (SemesterId, Name, DisplayOrder) VALUES ($s, $n, $o)";
                    ins.Parameters.AddWithValue("$s", semId);
                    ins.Parameters.AddWithValue("$n", item.Name.Trim());
                    ins.Parameters.AddWithValue("$o", order++);
                    ins.ExecuteNonQuery();
                }
            }
        }

        transaction.Commit();
    }

    #endregion

    #region أدوات مساعدة داخلية

    /// <summary>فتح اتصال جديد بقاعدة البيانات.</summary>
    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>تنفيذ أمر SQL بدون نتائج.</summary>
    private static void Execute(SqliteConnection conn, string sql, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    /// <summary>تنفيذ INSERT والحصول على المعرّف المُنشأ.</summary>
    private static int InsertAndGetId(SqliteConnection conn, SqliteTransaction tx,
        string sql, params (string name, object value)[] parameters)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        return Convert.ToInt32((long)cmd.ExecuteScalar()!);
    }

    /// <summary>ضمان وجود سنة دراسية وإرجاع معرّفها.</summary>
    private static int EnsureYear(SqliteConnection conn, SqliteTransaction tx, int yearNumber)
    {
        using var sel = conn.CreateCommand();
        sel.Transaction = tx;
        sel.CommandText = "SELECT Id FROM StudyYears WHERE YearNumber = $y LIMIT 1";
        sel.Parameters.AddWithValue("$y", yearNumber);
        var existing = sel.ExecuteScalar();
        if (existing is not null) return Convert.ToInt32((long)existing);

        return InsertAndGetId(conn, tx,
            "INSERT INTO StudyYears (YearNumber) VALUES ($y); SELECT last_insert_rowid();",
            ("$y", yearNumber));
    }

    /// <summary>ضمان وجود فصل دراسي وإرجاع معرّفه.</summary>
    private static int EnsureSemester(SqliteConnection conn, SqliteTransaction tx, int yearId, int semesterNumber)
    {
        using var sel = conn.CreateCommand();
        sel.Transaction = tx;
        sel.CommandText = "SELECT Id FROM Semesters WHERE YearId = $y AND SemesterNumber = $s LIMIT 1";
        sel.Parameters.AddWithValue("$y", yearId);
        sel.Parameters.AddWithValue("$s", semesterNumber);
        var existing = sel.ExecuteScalar();
        if (existing is not null) return Convert.ToInt32((long)existing);

        return InsertAndGetId(conn, tx,
            "INSERT INTO Semesters (YearId, SemesterNumber) VALUES ($y, $s); SELECT last_insert_rowid();",
            ("$y", yearId), ("$s", semesterNumber));
    }

    #endregion
}
