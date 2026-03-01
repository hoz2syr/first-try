using StudentTracker.Models;

namespace StudentTracker.Services;

/// <summary>
/// خدمة حساب السنة الدراسية الحالية للطالب
/// </summary>
/// <remarks>
/// الوظيفة: تحديد السنة التي يجب أن يكون فيها الطالب بناءً على أداءه الأكاديمي
/// 
/// القاعدة المعتمدة:
/// - النجاح يُحسب تراكمياً للمواد حتى السنة الجاري تقييمها (السنة الحالية + كل السنوات السابقة)
/// - عدد المواد المطلوبة للنجاح = (إجمالي المواد التراكمية) - 4
/// - إذا كانت المواد الناجحة أقل من المطلوب، يبقى الطالب في تلك السنة
/// - إذا حقق الشرط لكل السنوات، ينتقل لآخر سنة تحتوي مواد
/// 
/// تحسينات الأداء:
/// - جلب جميع الدرجات دفعة واحدة قبل الحساب
/// - استخدام Dictionary للبحث السريع عن الدرجات
/// </remarks>
public static class YearCalculator
{
    /// <summary>
    /// حساب السنة الحالية للطالب
    /// </summary>
    /// <param name="db">خدمة قاعدة البيانات</param>
    /// <returns>رقم السنة الحالية</returns>
    /// <remarks>
    /// الخوارزمية:
    /// 1. جلب السنوات التي تحتوي مواد
    /// 2. جلب درجات كل المواد دفعة واحدة
    /// 3. بناء عدّاد تراكمي للمواد والمواد الناجحة
    /// 4. لكل سنة: required = total - 4، وإذا passed &lt; required تبقى السنة الحالية هي هذه السنة
    /// </remarks>
    public static int CalculateCurrentYear(IDatabaseService db)
    {
        var yearsWithSubjects = db.GetStudyYears()
            .OrderBy(y => y.YearNumber)
            .Where(y => db.GetSubjectsByYear(y.YearNumber).Any())
            .ToList();

        if (yearsWithSubjects.Count == 0)
            return 1;

        var retakeIds = db.GetRetakeLabSubjectIds();

        // تحسين الأداء: تحميل درجات جميع المواد مرة واحدة
        var allGrades = new Dictionary<(int SubjectId, string Type), double?>();
        foreach (var year in yearsWithSubjects)
        {
            foreach (var subject in db.GetSubjectsByYear(year.YearNumber))
            {
                allGrades[(subject.Id, "عملي")] = db.GetLatestGrade(subject.Id, "عملي");
                allGrades[(subject.Id, "نظري")] = db.GetLatestGrade(subject.Id, "نظري");
            }
        }

        var cumulativeTotalSubjects = 0;
        var cumulativePassedSubjects = 0;

        foreach (var year in yearsWithSubjects)
        {
            var subjects = db.GetSubjectsByYear(year.YearNumber).ToList();

            var passedInThisYear = subjects.Count(subject =>
            {
                var practical = allGrades.GetValueOrDefault((subject.Id, "عملي"));
                var theory = allGrades.GetValueOrDefault((subject.Id, "نظري"));
                return GradeCalculator.IsPassed(practical, theory, retakeIds.Contains(subject.Id));
            });

            cumulativeTotalSubjects += subjects.Count;
            cumulativePassedSubjects += passedInThisYear;

            // المطلوب للنجاح التراكمي: إجمالي المواد - 4 (مع حد أدنى 0)
            var requiredPassed = Math.Max(0, cumulativeTotalSubjects - 4);

            if (cumulativePassedSubjects < requiredPassed)
            {
                return year.YearNumber;
            }
        }

        return yearsWithSubjects.Max(y => y.YearNumber);
    }
}
