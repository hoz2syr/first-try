using System;
using System.IO;
using System.Text;

namespace StudentTracker.Services
{
    /// <summary>
    /// خدمة تسجيل الأخطاء والمعلومات
    /// </summary>
    /// <remarks>
    /// تُستخدم لتسجيل الأخطاء والمعلومات في ملف سجل
    /// يمكن الوصول إلى السجلات في مجلد AppData\Local\StudentTracker\Logs
    /// </remarks>
    public static class Logger
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;
        private static readonly object LockObject = new();

        static Logger()
        {
            // إنشاء مجلد السجلات
            LogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StudentTracker",
                "Logs");

            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            // اسم ملف السجل مع التاريخ
            string logFileName = $"app_{DateTime.Now:yyyyMMdd}.log";
            LogFilePath = Path.Combine(LogDirectory, logFileName);
        }

        /// <summary>
        /// تسجيل رسالة خطأ
        /// </summary>
        /// <param name="message">رسالة الخطأ</param>
        /// <param name="ex">الاستثناء (اختياري)</param>
        public static void LogError(string message, Exception? ex = null)
        {
            WriteLog("ERROR", message, ex);
        }

        /// <summary>
        /// تسجيل رسالة تحذير
        /// </summary>
        /// <param name="message">رسالة التحذير</param>
        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// تسجيل رسالة معلومات
        /// </summary>
        /// <param name="message">رسالة المعلومات</param>
        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// تسجيل معلومات التصحيح
        /// </summary>
        /// <param name="message">رسالة التصحيح</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }

        private static void WriteLog(string level, string message, Exception? ex = null)
        {
            try
            {
                lock (LockObject)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");

                    if (ex != null)
                    {
                        sb.AppendLine($"Exception: {ex.GetType().FullName}");
                        sb.AppendLine($"Message: {ex.Message}");
                        sb.AppendLine($"StackTrace: {ex.StackTrace}");

                        if (ex.InnerException != null)
                        {
                            sb.AppendLine($"InnerException: {ex.InnerException.Message}");
                        }
                    }

                    File.AppendAllText(LogFilePath, sb.ToString());
                }
            }
            catch
            {
                // تجاهل أخطاء الكتابة في السجل
            }
        }

        /// <summary>
        /// الحصول على مسار ملف السجل الحالي
        /// </summary>
        public static string GetLogFilePath() => LogFilePath;

        /// <summary>
        /// حذف السجلات القديمة (أقدم من 7 أيام)
        /// </summary>
        public static void CleanupOldLogs(int daysToKeep = 7)
        {
            try
            {
                if (!Directory.Exists(LogDirectory)) return;

                var files = Directory.GetFiles(LogDirectory, "*.log");
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch
            {
                // تجاهل أخطاء الحذف
            }
        }
    }
}
