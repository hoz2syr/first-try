using System.IO;
using System.Text;

namespace StudentTracker.Services;

/// <summary>
/// خدمة تسجيل الأحداث - تُسجل الأخطاء والمعلومات في ملفات نصية يومية.
/// </summary>
/// <remarks>
/// المسار: <c>%LocalAppData%\StudentTracker\Logs\app_yyyyMMdd.log</c><br/>
/// تُنظف السجلات الأقدم من 7 أيام تلقائياً عند بدء التشغيل.
/// </remarks>
public static class Logger
{
    private static readonly string LogDirectory;
    private static readonly string LogFilePath;
    private static readonly object LockObject = new();

    static Logger()
    {
        LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StudentTracker", "Logs");

        if (!Directory.Exists(LogDirectory))
            Directory.CreateDirectory(LogDirectory);

        LogFilePath = Path.Combine(LogDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
    }

    /// <summary>تسجيل رسالة خطأ مع الاستثناء.</summary>
    public static void LogError(string message, Exception? ex = null) =>
        WriteLog("ERROR", message, ex);

    /// <summary>تسجيل رسالة تحذير.</summary>
    public static void LogWarning(string message) =>
        WriteLog("WARNING", message);

    /// <summary>تسجيل رسالة معلومات.</summary>
    public static void LogInfo(string message) =>
        WriteLog("INFO", message);

    /// <summary>تسجيل رسالة تصحيح (فقط في وضع DEBUG).</summary>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogDebug(string message) =>
        WriteLog("DEBUG", message);

    /// <summary>الحصول على مسار ملف السجل الحالي.</summary>
    public static string GetLogFilePath() => LogFilePath;

    /// <summary>حذف السجلات الأقدم من العدد المحدد من الأيام.</summary>
    public static void CleanupOldLogs(int daysToKeep = 7)
    {
        try
        {
            if (!Directory.Exists(LogDirectory)) return;

            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            foreach (var file in Directory.GetFiles(LogDirectory, "*.log"))
            {
                if (new FileInfo(file).CreationTime < cutoffDate)
                    File.Delete(file);
            }
        }
        catch
        {
            // تجاهل أخطاء حذف السجلات القديمة
        }
    }

    /// <summary>كتابة سطر في ملف السجل.</summary>
    private static void WriteLog(string level, string message, Exception? ex = null)
    {
        try
        {
            lock (LockObject)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");

                if (ex is not null)
                {
                    sb.AppendLine($"  Exception : {ex.GetType().FullName}");
                    sb.AppendLine($"  Message   : {ex.Message}");
                    sb.AppendLine($"  StackTrace: {ex.StackTrace}");
                    if (ex.InnerException is not null)
                        sb.AppendLine($"  Inner     : {ex.InnerException.Message}");
                }

                File.AppendAllText(LogFilePath, sb.ToString());
            }
        }
        catch
        {
            // تجاهل أخطاء الكتابة في السجل
        }
    }
}
