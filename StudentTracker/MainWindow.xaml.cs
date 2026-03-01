using StudentTracker.Models;
using StudentTracker.ViewModels;
using StudentTracker.Services;
using StudentTracker.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;

namespace StudentTracker;

/// <summary>
/// النافذة الرئيسية لتطبيق StudentTracker
/// </summary>
/// <remarks>
/// الوظائف الرئيسية:
/// - عرض لوحة التحكم والإحصائيات
/// - عرض مواد الفصل الحالي والخطة العامة
/// - عرض التقويم وأحداث الامتحانات
/// - إدارة الملاحظات
/// 
/// بنية النافذة:
/// - TabControl رئيسي مع عدة تبويبات
/// - ObservableCollection للربط مع واجهة المستخدم
/// - أوامر Commands للتنفيذ الفوري
/// </remarks>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    /// <summary>
    /// خدمة قاعدة البيانات - تُستخدم لجميع عمليات البيانات
    /// </summary>
    private readonly IDatabaseService _db;
    
    /// <summary>
    /// السنة الحالية للطالب
    /// </summary>
    private int _currentYear = 1;
    
    /// <summary>
    /// الفصل الدراسي الحالي
    /// </summary>
    private int _currentSemester = 1;
    
    /// <summary>
    /// مؤشر التبويب النشط
    /// </summary>
    private int _activeTabIndex = 0;
    
    /// <summary>
    /// نقطة بدء السحب (للإيماءات باللمس)
    /// </summary>
    private Point _swipeStart;
    
    /// <summary>
    /// هل يتم السحب حالياً
    /// </summary>
    private bool _isSwiping;

    // ==================== مساعد رسائل الخطأ ====================

    /// <summary>
    /// تحويل رسالة الخطأ التقنية إلى رسالة مفهومة للمستخدم
    /// </summary>
    /// <remarks>
    /// يحلل نوع الخطأ ويعرض رسالة مبسطة بدلاً من رسائل .NET التقنية
    /// </remarks>
    private static string GetUserFriendlyError(Exception ex, string context)
    {
        // تسجيل الخطأ التقني للمطور
        System.Diagnostics.Debug.WriteLine($"[{context}] {ex.GetType().Name}: {ex.Message}");
        Services.Logger.LogError(context, ex);

        return ex switch
        {
            // أخطاء قاعدة البيانات
            Microsoft.Data.Sqlite.SqliteException => "حدث خطأ في قاعدة البيانات. يرجى إعادة تشغيل التطبيق.",
            System.IO.IOException => "لا يمكن الوصول إلى ملف البيانات. تأكد من أن الملف غير مفتوح في برنامج آخر.",
            InvalidOperationException => "حدث خطأ أثناء العملية. يرجى المحاولة مرة أخرى.",
            FormatException => "تم إدخال بيانات غير صالحة. يرجى التحقق من المدخلات.",
            ArgumentException => "بيانات غير صحيحة. يرجى مراجعة القيم المُدخلة.",
            _ => "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى."
        };
    }

    /// <summary>
    /// المُنشئ الافتراضي - ينشئ خدمة قاعدة بيانات جديدة
    /// </summary>
    public MainWindow() : this(new DatabaseService()) { }

    /// <summary>
    /// المُنشئ الرئيسي - يستقبل خدمة قاعدة البيانات
    /// </summary>
    /// <param name="db">خدمة قاعدة البيانات</param>
    public MainWindow(IDatabaseService db)
    {
        _db = db;
        InitializeComponent();
        DataContext = this;

        // تهيئة الخدمات والأوامر والإعدادات
        InitializeServices();
        InitializeCommands();
        LoadWindowSettings();

        // تحميل جميع البيانات سيتم في MainWindow_Loaded بعد تحميل النافذة

        // تسجيل المعالجات للأحداث
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    /// <summary>
    /// تهيئة الخدمات الأساسية للتطبيق
    /// </summary>
    /// <remarks>
    /// - تهيئة قاعدة البيانات
    /// - جلب الفصل الدراسي الحالي
    /// </remarks>
    private void InitializeServices()
    {
        try
        {
            _db.InitializeDatabase();
            _currentSemester = _db.GetCurrentSemester();
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "InitializeServices"), "خطأ في التهيئة", this);
        }
    }

    /// <summary>
    /// تهيئة الأوامر (Commands)
    /// </summary>
    private void InitializeCommands()
    {
        SelectCalendarDayCommand = new RelayCommand(SelectCalendarDay);
    }

    /// <summary>
    /// معالج حدث تحميل النافذة
    /// </summary>
    /// <remarks>
    /// يُعرض نوافذ الاحتفالات (تخرج، ترقي)
    /// </remarks>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // تحميل البيانات بعد تحميل النافذة بالكامل لضمان تحديث الـ UI
        Dispatcher.BeginInvoke(new Action(() =>
        {
            RefreshAll();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
        
        CheckAndShowCelebrations();
    }
    
    /// <summary>
    /// معالج حدث إغلاق النافذة
    /// </summary>
    /// <remarks>
    /// يحفظ إعدادات النافذة قبل الإغلاق
    /// </remarks>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveWindowSettings();
    }
    
    /// <summary>
    /// تحميل إعدادات النافذة المحفوظة
    /// </summary>
    /// <remarks>
    /// يحمّل الأبعاد والموضع وحالة التكبير
    /// </remarks>
    private void LoadWindowSettings()
    {
        try
        {
            if (Properties.Settings.Default.WindowWidth > 0)
            {
                Width = Properties.Settings.Default.WindowWidth;
                Height = Properties.Settings.Default.WindowHeight;
                
                if (Properties.Settings.Default.WindowLeft >= 0)
                {
                    Left = Properties.Settings.Default.WindowLeft;
                    Top = Properties.Settings.Default.WindowTop;
                }
                
                if (Properties.Settings.Default.WindowMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }
        catch
        {
            // تجاهل أخطاء تحميل الإعدادات
        }
    }
    
    /// <summary>
    /// حفظ إعدادات النافذة
    /// </summary>
    private void SaveWindowSettings()
    {
        try
        {
            Properties.Settings.Default.WindowMaximized = WindowState == WindowState.Maximized;
            
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.WindowWidth = Width;
                Properties.Settings.Default.WindowHeight = Height;
                Properties.Settings.Default.WindowLeft = Left;
                Properties.Settings.Default.WindowTop = Top;
            }
            
            Properties.Settings.Default.Save();
        }
        catch
        {
            // تجاهل أخطاء حفظ الإعدادات
        }
    }

    /// <summary>
    /// فحص وعرض نوافذ الاحتفالات
    /// </summary>
    /// <remarks>
    /// يتحقق من:
    /// - إذا أكمل الطالب كل المواد -> نافذة التخرج
    /// - إذا انتقل الطالب لسنة جديدة -> نافذة الترقي
    /// </remarks>
    private void CheckAndShowCelebrations()
    {
        try
        {
            var lastCelebratedYearStr = _db.GetSetting("LastCelebratedYear");
            var lastCelebratedYear = int.TryParse(lastCelebratedYearStr, out var year) ? year : 0;

            var graduationCelebrated = _db.GetSetting("GraduationCelebrated") == "true";
            var calculatedYear = YearCalculator.CalculateCurrentYear(_db);

            if (OverallPassedCount == OverallTotalCount && OverallTotalCount > 0)
            {
                if (!graduationCelebrated)
                {
                    var graduationWindow = new GraduationWindow(OverallPassedCount, OverallTotalCount) { Owner = this };
                    graduationWindow.ShowDialog();
                    _db.SetSetting("GraduationCelebrated", "true");
                }
            }
            else if (calculatedYear > lastCelebratedYear && calculatedYear > 1)
            {
                var promotionWindow = new YearPromotionWindow(calculatedYear) { Owner = this };
                promotionWindow.ShowDialog();
                _db.SetSetting("LastCelebratedYear", calculatedYear.ToString());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in celebrations: {ex.Message}");
        }
    }

    // ==================== مجموعات البيانات للواجهة ====================
    
    /// <summary>
    /// مواد الفصل الدراسي الحالي
    /// </summary>
    public ObservableCollection<SubjectViewModel> CurrentSemesterSubjects { get; } = new();
    
    /// <summary>
    /// فصول الخطة العامة (جميع السنوات)
    /// </summary>
    public ObservableCollection<SemesterGroupViewModel> GeneralPlanSemesters { get; } = new();
    
    /// <summary>
    /// ملاحظات المستخدم
    /// </summary>
    public ObservableCollection<NoteViewModel> Notes { get; } = new();

    public string CurrentYearText => _currentYear.ToString();
    public string CurrentSemesterText => _currentSemester switch
    {
        1 => "الأول",
        2 => "الثاني",
        3 => "الصيفي",
        _ => "الأول"
    };

    public int PassedCountCurrentYear { get; private set; }
    public int FailedCountCurrentYear { get; private set; }
    public int PendingCountCurrentYear { get; private set; }
    public int RemainingCountCurrentYear { get; private set; }
    public double CurrentYearCompletionPercentage { get; private set; }
    public string CurrentYearCompletionText => $"{CurrentYearCompletionPercentage:0.#}%";
    public int OverallPassedCount { get; private set; }
    public int OverallTotalCount { get; private set; }
    public double OverallCompletionPercentage { get; private set; }
    public string OverallCompletionText => $"{OverallCompletionPercentage:0.#}%";
    public double OverallAverageGrade { get; private set; }
    public string OverallAverageGradeText => $"{OverallAverageGrade:0.#}";

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (_activeTabIndex == value) return;
            _activeTabIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDashboardActive));
            OnPropertyChanged(nameof(IsCurrentSemesterActive));
            OnPropertyChanged(nameof(IsOverallPlanActive));
            OnPropertyChanged(nameof(IsCalendarActive));
            OnPropertyChanged(nameof(IsNotesActive));
        }
    }

    public bool IsDashboardActive => _activeTabIndex == 0;
    public bool IsCurrentSemesterActive => _activeTabIndex == 1;
    public bool IsOverallPlanActive => _activeTabIndex == 2;
    public bool IsCalendarActive => _activeTabIndex == 3;
    public bool IsNotesActive => _activeTabIndex == 4;

    private DateTime _calendarCurrentDate = DateTime.Today;
    private CalendarDayViewModel? _selectedCalendarDay;

    // ==================== بيانات التقويم ====================
    
    /// <summary>
    /// أيام التقويم للشهر المعروض
    /// </summary>
    public ObservableCollection<CalendarDayViewModel> CalendarDays { get; } = new();
    
    /// <summary>
    /// أحداث اليوم المحدد
    /// </summary>
    public ObservableCollection<CalendarEvent> SelectedDayEvents { get; } = new();

    /// <summary>
    /// ثقافة عربية بتقويم ميلادي (بدلاً من ar-SA الذي يستخدم التقويم الهجري)
    /// </summary>
    private static readonly CultureInfo ArabicGregorianCulture = new("ar")
    {
        DateTimeFormat = { Calendar = new System.Globalization.GregorianCalendar() }
    };

    public string CurrentMonthName => _calendarCurrentDate.ToString("MMMM", ArabicGregorianCulture);
    public int CurrentCalendarYear => _calendarCurrentDate.Year;
    public bool HasSelectedDayEvents => SelectedDayEvents.Count > 0;
    public ICommand SelectCalendarDayCommand { get; set; } = null!;

    private bool _isSettingsActive = false;
    public bool IsSettingsActive
    {
        get => _isSettingsActive;
        set
        {
            if (_isSettingsActive != value)
            {
                _isSettingsActive = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void RefreshAll()
    {
        try
        {
            _currentYear = YearCalculator.CalculateCurrentYear(_db);
            _db.SetSetting("CurrentYear", _currentYear.ToString());

            RefreshCurrentYearStatistics();
            RefreshOverallStatistics();
            LoadCurrentSemesterPlan();
            LoadGeneralPlan();
            LoadCalendar();
            LoadNotes();

            NotifyAllPropertiesChanged();
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "RefreshAll"), "خطأ في التحديث", this);
        }
    }

    private void NotifyAllPropertiesChanged()
    {
        var properties = new[]
        {
            nameof(CurrentYearText), nameof(CurrentSemesterText),
            nameof(PassedCountCurrentYear), nameof(FailedCountCurrentYear),
            nameof(PendingCountCurrentYear), nameof(RemainingCountCurrentYear),
            nameof(CurrentYearCompletionPercentage), nameof(CurrentYearCompletionText),
            nameof(OverallPassedCount), nameof(OverallTotalCount),
            nameof(OverallCompletionPercentage), nameof(OverallCompletionText),
            nameof(OverallAverageGrade), nameof(OverallAverageGradeText)
        };

        foreach (var property in properties)
        {
            OnPropertyChanged(property);
        }
    }

    private void RefreshCurrentYearStatistics()
    {
        try
        {
            var subjects = _db.GetAllSubjects().Where(s => s.YearNumber == _currentYear).ToList();
            var retakeIds = new HashSet<int>(_db.GetRetakeLabSubjectIds());

            PassedCountCurrentYear = 0;
            FailedCountCurrentYear = 0;
            PendingCountCurrentYear = 0;
            RemainingCountCurrentYear = 0;

            foreach (var subject in subjects)
            {
                var practical = _db.GetLatestGrade(subject.Id, "عملي");
                var theory = _db.GetLatestGrade(subject.Id, "نظري");
                var status = GradeCalculator.ComputeStatus(_currentYear, subject.YearNumber, practical, theory, retakeIds.Contains(subject.Id));

                switch (status)
                {
                    case SubjectStatus.Passed:
                        PassedCountCurrentYear++;
                        break;
                    case SubjectStatus.Failed:
                        FailedCountCurrentYear++;
                        break;
                    case SubjectStatus.Pending:
                        PendingCountCurrentYear++;
                        break;
                    default:
                        RemainingCountCurrentYear++;
                        break;
                }
            }

            var total = subjects.Count;
            // حساب النسبة بناءً على المواد المكتملة (ناجحة أو راسبة) وليس فقط الناجحة
            var completedSubjects = PassedCountCurrentYear + FailedCountCurrentYear;
            CurrentYearCompletionPercentage = total == 0 ? 0 : (double)completedSubjects / total * 100.0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing current year stats: {ex.Message}");
        }
    }

    private void RefreshOverallStatistics()
    {
        try
        {
            var subjects = _db.GetAllSubjects();
            var retakeIds = new HashSet<int>(_db.GetRetakeLabSubjectIds());

            OverallPassedCount = 0;
            OverallTotalCount = subjects.Count;
            var totalGradeSum = 0.0;
            var gradeCount = 0;

            foreach (var subject in subjects)
            {
                var practical = _db.GetLatestGrade(subject.Id, "عملي");
                var theory = _db.GetLatestGrade(subject.Id, "نظري");
                var status = GradeCalculator.ComputeStatus(_currentYear, subject.YearNumber, practical, theory, retakeIds.Contains(subject.Id));

                if (status == SubjectStatus.Passed)
                {
                    OverallPassedCount++;
                }

                var finalGrade = GradeCalculator.ComputeFinalGrade(practical, theory, retakeIds.Contains(subject.Id));
                if (finalGrade.HasValue)
                {
                    totalGradeSum += finalGrade.Value;
                    gradeCount++;
                }
            }

            OverallCompletionPercentage = OverallTotalCount == 0 ? 0 : (double)OverallPassedCount / OverallTotalCount * 100.0;
            OverallAverageGrade = gradeCount == 0 ? 0 : totalGradeSum / gradeCount;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing overall stats: {ex.Message}");
        }
    }

    private void SetupSubjectCommands(SubjectViewModel viewModel)
    {
        viewModel.AddGradeCommand = new RelayCommand(_ =>
        {
            try
            {
                var gradeWindow = new AddGradeWindow(viewModel.SubjectName) { Owner = this };
                if (gradeWindow.ShowDialog() == true)
                {
                    var gradeAttempt = new GradeAttempt
                    {
                        SubjectId = viewModel.SubjectId,
                        AttemptType = gradeWindow.AttemptType,
                        Grade = gradeWindow.Grade,
                        AttemptDate = gradeWindow.AttemptDate,
                        Notes = gradeWindow.Notes
                    };
                    _db.AddGradeAttempt(gradeAttempt);
                    CustomMessageBox.ShowSuccess($"تم إضافة علامة {gradeWindow.AttemptType} بنجاح!", "تم الحفظ", this);
                    RefreshAll();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(GetUserFriendlyError(ex, "AddGrade"), "خطأ في إضافة العلامة", this);
            }
        });

        viewModel.AddExamCommand = new RelayCommand(_ =>
        {
            try
            {
                var examWindow = new AddExamWindow(viewModel.SubjectName) { Owner = this };
                if (examWindow.ShowDialog() == true)
                {
                    if (_db.HasExamConflict(examWindow.ExamDateTime, viewModel.SubjectId))
                    {
                        CustomMessageBox.ShowWarning("يوجد تعارض مع موعد امتحان آخر في نفس الوقت!", "تنبيه", this);
                        return;
                    }
                    _db.UpsertExamSchedule(viewModel.SubjectId, examWindow.ExamType, examWindow.ExamDateTime);
                    CustomMessageBox.ShowSuccess($"تم حفظ موعد امتحان {examWindow.ExamType} بنجاح!", "تم الحفظ", this);
                    RefreshAll();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(GetUserFriendlyError(ex, "AddExam"), "خطأ في إضافة الامتحان", this);
            }
        });

        viewModel.ShowHistoryCommand = new RelayCommand(_ =>
        {
            try
            {
                var history = _db.GetSubjectHistory(viewModel.SubjectId);
                var historyWindow = new SubjectHistoryWindow(viewModel.SubjectName, history) { Owner = this };
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(GetUserFriendlyError(ex, "ShowHistory"), "خطأ في عرض السجل", this);
            }
        });

        viewModel.RetakeLabCommand = new RelayCommand(_ =>
        {
            try
            {
                if (viewModel.IsRetakeLab)
                {
                    _db.ClearRetakeLab(viewModel.SubjectId);
                }
                else
                {
                    var confirm = CustomMessageBox.ShowConfirm(
                        "هل أنت متأكد من تسجيل إعادة العملي؟ سيتم احتساب علامة العملي كـ 0.",
                        "تأكيد إعادة العملي", this);
                    if (confirm)
                    {
                        _db.MarkRetakeLab(viewModel.SubjectId);
                    }
                    else
                    {
                        return;
                    }
                }
                RefreshAll();
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(GetUserFriendlyError(ex, "RetakeLab"), "خطأ", this);
            }
        });

        viewModel.StartEditCommand = new RelayCommand(_ =>
        {
            viewModel.IsEditing = true;
            viewModel.EditPracticalGrade = viewModel.PracticalGrade?.ToString("0.##") ?? "";
            viewModel.EditTheoryGrade = viewModel.TheoryGrade?.ToString("0.##") ?? "";
            viewModel.EditPracticalExamDate = viewModel.PracticalExamDate;
            viewModel.EditPracticalExamTime = viewModel.PracticalExamDate?.ToString("HH:mm") ?? "";
            viewModel.EditTheoryExamDate = viewModel.TheoryExamDate;
            viewModel.EditTheoryExamTime = viewModel.TheoryExamDate?.ToString("HH:mm") ?? "";
        });

        viewModel.SaveEditCommand = new RelayCommand(_ =>
        {
            try
            {
                // حفظ الدرجة العملية مع التحقق
                if (!SaveInlineGrade(viewModel.SubjectId, "عملي", viewModel.EditPracticalGrade))
                    return;

                // حفظ الدرجة النظرية مع التحقق
                if (!SaveInlineGrade(viewModel.SubjectId, "نظري", viewModel.EditTheoryGrade))
                    return;

                // حفظ تاريخ امتحان العملي
                SaveInlineExamDate(viewModel.SubjectId, "عملي", viewModel.EditPracticalExamDate, viewModel.EditPracticalExamTime);

                // حفظ تاريخ امتحان النظري
                SaveInlineExamDate(viewModel.SubjectId, "نظري", viewModel.EditTheoryExamDate, viewModel.EditTheoryExamTime);

                viewModel.IsEditing = false;
                CustomMessageBox.ShowSuccess($"تم حفظ تعديلات '{viewModel.SubjectName}' بنجاح!", "تم الحفظ", this);
                RefreshAll();
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(GetUserFriendlyError(ex, "SaveEdit"), "خطأ في حفظ التعديلات", this);
            }
        });

        viewModel.CancelEditCommand = new RelayCommand(_ =>
        {
            viewModel.IsEditing = false;
        });
    }

    private void LoadCurrentSemesterPlan()
    {
        try
        {
            CurrentSemesterSubjects.Clear();
            var subjects = _db.GetAllSubjects()
                .Where(s => s.YearNumber == _currentYear && s.SemesterNumber == _currentSemester)
                .ToList();

            var retakeIds = new HashSet<int>(_db.GetRetakeLabSubjectIds());

            foreach (var subject in subjects)
            {
                CurrentSemesterSubjects.Add(BuildSubjectViewModel(subject, retakeIds));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading current semester: {ex.Message}");
        }
    }

    private void LoadGeneralPlan()
    {
        try
        {
            GeneralPlanSemesters.Clear();
            var subjects = _db.GetAllSubjects();
            var retakeIds = new HashSet<int>(_db.GetRetakeLabSubjectIds());

            var groupedSubjects = subjects
                .GroupBy(s => new { s.YearNumber, s.SemesterNumber })
                .OrderBy(g => g.Key.YearNumber)
                .ThenBy(g => g.Key.SemesterNumber);

            foreach (var group in groupedSubjects)
            {
                var semesterViewModel = new SemesterGroupViewModel
                {
                    YearNumber = group.Key.YearNumber,
                    SemesterNumber = group.Key.SemesterNumber,
                    SemesterName = GetSemesterName(group.Key.SemesterNumber),
                    Title = $"السنة {group.Key.YearNumber} - {GetSemesterName(group.Key.SemesterNumber)}"
                };

                foreach (var subject in group.OrderBy(s => s.DisplayOrder))
                {
                    semesterViewModel.Subjects.Add(BuildSubjectViewModel(subject, retakeIds));
                }

                GeneralPlanSemesters.Add(semesterViewModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading general plan: {ex.Message}");
        }
    }

    private void LoadCalendar()
    {
        try
        {
            CalendarDays.Clear();
            var firstDay = new DateTime(_calendarCurrentDate.Year, _calendarCurrentDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var startDate = firstDay.AddDays(-(int)firstDay.DayOfWeek);
            // Ensure we fill complete weeks (at least 6 rows)
            var endDate = startDate.AddDays(41); // 6 weeks = 42 days

            var examSchedules = _db.GetAllExamSchedules()
                .Where(e => e.ExamDateTime.Date >= startDate && e.ExamDateTime.Date <= endDate)
                .GroupBy(e => e.ExamDateTime.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var subjectsLookup = _db.GetAllSubjects().ToDictionary(s => s.Id, s => s.Name);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayViewModel = new CalendarDayViewModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == _calendarCurrentDate.Month,
                    IsToday = date.Date == DateTime.Today,
                    HasEvents = examSchedules.ContainsKey(date)
                };

                // Populate Events for display on calendar cells
                if (examSchedules.TryGetValue(date, out var dayExams))
                {
                    foreach (var exam in dayExams)
                    {
                        subjectsLookup.TryGetValue(exam.SubjectId, out var subjectName);
                        dayViewModel.Events.Add(new CalendarEvent
                        {
                            Title = $"{exam.ExamType} - {subjectName ?? "غير معروف"}",
                            DateTime = exam.ExamDateTime,
                            Type = "امتحان",
                            SubjectName = subjectName ?? "غير معروف",
                            ExamType = exam.ExamType
                        });
                    }
                }

                CalendarDays.Add(dayViewModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading calendar: {ex.Message}");
        }
    }

    private void LoadNotes()
    {
        try
        {
            Notes.Clear();
            var notes = _db.GetAllNotes();

            foreach (var note in notes)
            {
                var noteViewModel = new NoteViewModel
                {
                    Id = note.Id,
                    Title = note.Title,
                    Content = note.Content,
                    CreatedDate = note.CreatedDate,
                    ModifiedDate = note.ModifiedDate
                };

                Notes.Add(noteViewModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading notes: {ex.Message}");
        }
    }

    private void SelectCalendarDay(object? parameter)
    {
        if (parameter is CalendarDayViewModel day)
        {
            // Deselect previously selected day
            if (_selectedCalendarDay != null)
            {
                _selectedCalendarDay.IsSelected = false;
            }

            day.IsSelected = true;
            _selectedCalendarDay = day;
            LoadSelectedDayEvents(day.Date);
        }
    }

    private void LoadSelectedDayEvents(DateTime date)
    {
        try
        {
            SelectedDayEvents.Clear();
            var examSchedules = _db.GetAllExamSchedules()
                .Where(e => e.ExamDateTime.Date == date.Date)
                .ToList();

            var subjects = _db.GetAllSubjects().ToDictionary(s => s.Id, s => s.Name);

            foreach (var exam in examSchedules)
            {
                subjects.TryGetValue(exam.SubjectId, out var subjectName);
                var calendarEvent = new CalendarEvent
                {
                    Title = $"{exam.ExamType} - {subjectName ?? "غير معروف"}",
                    DateTime = exam.ExamDateTime,
                    Type = "امتحان",
                    SubjectName = subjectName ?? "غير معروف",
                    ExamType = exam.ExamType
                };

                SelectedDayEvents.Add(calendarEvent);
            }

            OnPropertyChanged(nameof(HasSelectedDayEvents));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading day events: {ex.Message}");
        }
    }

    private static string GetSemesterName(int semesterNumber) => semesterNumber switch
    {
        1 => "الفصل الأول",
        2 => "الفصل الثاني",
        3 => "الفصل الصيفي",
        _ => "فصل غير معروف"
    };

    // ==================== مساعد بناء SubjectViewModel ====================

    /// <summary>
    /// بناء نموذج عرض المادة من بيانات المادة الخام
    /// </summary>
    /// <remarks>
    /// يجلب أحدث الدرجات والامتحانات ويحسب الحالة النهائية.
    /// يُستخدم بشكل مشترك من LoadCurrentSemesterPlan وLoadGeneralPlan.
    /// </remarks>
    private SubjectViewModel BuildSubjectViewModel(Subject subject, HashSet<int> retakeIds)
    {
        var practical = _db.GetLatestGrade(subject.Id, "عملي");
        var theory = _db.GetLatestGrade(subject.Id, "نظري");
        var practicalExam = _db.GetLatestExamDate(subject.Id, "عملي");
        var theoryExam = _db.GetLatestExamDate(subject.Id, "نظري");
        var isRetake = retakeIds.Contains(subject.Id);

        var vm = new SubjectViewModel
        {
            SubjectId = subject.Id,
            YearNumber = subject.YearNumber,
            SemesterNumber = subject.SemesterNumber,
            SubjectName = subject.Name,
            PracticalGrade = practical,
            TheoryGrade = theory,
            PracticalExamDate = practicalExam,
            TheoryExamDate = theoryExam,
            FinalGrade = GradeCalculator.ComputeFinalGrade(practical, theory, isRetake),
            Status = GradeCalculator.ComputeStatus(_currentYear, subject.YearNumber, practical, theory, isRetake),
            IsRetakeLab = isRetake,
            ShowRetakeLabButton = practical.HasValue && practical.Value < 30,
            IsLocked = subject.YearNumber > _currentYear
        };

        SetupSubjectCommands(vm);
        return vm;
    }

    // ==================== مساعدات التحرير المضمن ====================

    /// <summary>
    /// حفظ درجة مُدخلة من التحرير السريع
    /// </summary>
    /// <returns>true إذا تم الحفظ بنجاح أو كان الحقل فارغاً</returns>
    private bool SaveInlineGrade(int subjectId, string attemptType, string gradeText)
    {
        if (string.IsNullOrWhiteSpace(gradeText)) return true;

        if (double.TryParse(gradeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var grade) ||
            double.TryParse(gradeText, NumberStyles.Float, CultureInfo.CurrentCulture, out grade))
        {
            if (grade >= 0 && grade <= 100)
            {
                _db.AddGradeAttempt(new GradeAttempt
                {
                    SubjectId = subjectId,
                    AttemptType = attemptType,
                    Grade = grade,
                    AttemptDate = DateTime.Now,
                    Notes = "تعديل سريع"
                });
                return true;
            }
            else
            {
                CustomMessageBox.ShowWarning($"علامة {attemptType} يجب أن تكون بين 0 و 100. القيمة المُدخلة: {grade}", "قيمة غير صالحة", this);
                return false;
            }
        }
        else
        {
            CustomMessageBox.ShowWarning($"علامة {attemptType} يجب أن تكون رقماً. القيمة المُدخلة: '{gradeText}'", "قيمة غير صالحة", this);
            return false;
        }
    }

    /// <summary>
    /// حفظ تاريخ امتحان مُدخل من التحرير السريع
    /// </summary>
    private void SaveInlineExamDate(int subjectId, string examType, DateTime? examDate, string timeText)
    {
        if (!examDate.HasValue) return;

        var time = TimeSpan.Zero;
        if (!string.IsNullOrWhiteSpace(timeText) && TimeSpan.TryParse(timeText, out var parsed))
        {
            time = parsed;
        }

        _db.UpsertExamSchedule(subjectId, examType, examDate.Value.Date.Add(time));
    }

    // Event handlers
    private void NavDashboard_Click(object sender, RoutedEventArgs e) => ActiveTabIndex = 0;
    private void NavCurrentSemester_Click(object sender, RoutedEventArgs e) => ActiveTabIndex = 1;
    private void NavOverallPlan_Click(object sender, RoutedEventArgs e) => ActiveTabIndex = 2;
    private void NavCalendar_Click(object sender, RoutedEventArgs e) => ActiveTabIndex = 3;
    private void NavNotes_Click(object sender, RoutedEventArgs e) => ActiveTabIndex = 4;

    private void EditPlan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsSettingsActive = true;
            var planWindow = new EditPlanWindow(_db) { Owner = this };
            planWindow.ShowDialog();
            RefreshAll();
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "EditPlan"), "خطأ في فتح نافذة تحرير الخطة", this);
        }
        finally
        {
            IsSettingsActive = false;
        }
    }

    private void ChangeSemester_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _currentYear = YearCalculator.CalculateCurrentYear(_db);
            var changeSemesterWindow = new ChangeSemesterWindow(_currentYear, _currentSemester, _db.GetStudyYears().Count) { Owner = this };
            if (changeSemesterWindow.ShowDialog() == true)
            {
                _db.SetCurrentSemester(changeSemesterWindow.SelectedSemester);
                _currentSemester = changeSemesterWindow.SelectedSemester;
                RefreshAll();
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "ChangeSemester"), "خطأ في تغيير الفصل", this);
        }
    }

    private void AddNote_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var addNoteWindow = new AddNoteWindow { Owner = this };
            if (addNoteWindow.ShowDialog() == true)
            {
                var note = new Note
                {
                    Title = addNoteWindow.NoteTitle,
                    Content = addNoteWindow.NoteContent,
                    CreatedDate = addNoteWindow.NoteDate,
                    ModifiedDate = DateTime.Now
                };

                _db.AddNote(note);
                CustomMessageBox.ShowSuccess("تم حفظ الملاحظة بنجاح!", "تم الحفظ", this);
                
                // Use Dispatcher to ensure UI update happens immediately
                Dispatcher.Invoke(() =>
                {
                    LoadNotes();
                    OnPropertyChanged(nameof(Notes));
                });
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "AddNote"), "خطأ في إضافة الملاحظة", this);
        }
    }
    
    private void Statistics_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var statisticsWindow = new StatisticsWindow(_db, _currentYear) { Owner = this };
            statisticsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "Statistics"), "خطأ في فتح الإحصائيات", this);
        }
    }

    private void DeleteNote_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is FrameworkElement fe && fe.DataContext is NoteViewModel noteVm)
            {
                var confirm = CustomMessageBox.ShowDeleteConfirm(
                    $"هل أنت متأكد من حذف الملاحظة '{noteVm.Title}'؟\nهذه العملية لا يمكن التراجع عنها.", 
                    "تأكيد حذف الملاحظة", 
                    this);
                if (confirm)
                {
                    _db.DeleteNote(noteVm.Id);
                    CustomMessageBox.ShowSuccess("تم حذف الملاحظة بنجاح", "تم الحذف", this);
                    LoadNotes();
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox.ShowError(GetUserFriendlyError(ex, "DeleteNote"), "خطأ في حذف الملاحظة", this);
        }
    }

    private void PreviousMonth_Click(object sender, RoutedEventArgs e) => NavigateCalendarMonth(-1);

    private void NextMonth_Click(object sender, RoutedEventArgs e) => NavigateCalendarMonth(1);

    /// <summary>
    /// تنقل التقويم شهراً للأمام أو الخلف
    /// </summary>
    private void NavigateCalendarMonth(int direction)
    {
        _calendarCurrentDate = _calendarCurrentDate.AddMonths(direction);
        _selectedCalendarDay = null;
        SelectedDayEvents.Clear();
        OnPropertyChanged(nameof(HasSelectedDayEvents));
        LoadCalendar();
        OnPropertyChanged(nameof(CurrentMonthName));
        OnPropertyChanged(nameof(CurrentCalendarYear));
    }

    private void TabContent_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _swipeStart = e.GetPosition(this);
        _isSwiping = true;
    }

    private void TabContent_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSwiping) return;
        _isSwiping = false;

        var endPoint = e.GetPosition(this);
        var deltaX = endPoint.X - _swipeStart.X;
        var deltaY = Math.Abs(endPoint.Y - _swipeStart.Y);

        // حساب القيم بناءً على حجم النافذة لتحسين تجربة المستخدم
        var minSwipeDistance = Math.Min(ActualWidth * 0.15, 150); // 15% من عرض النافذة أو 150 بيكسل كحد أقصى
        var maxVerticalDeviation = Math.Min(ActualHeight * 0.1, 80); // 10% من ارتفاع النافذة أو 80 بيكسل
        
        // Only trigger swipe if horizontal distance is significant and vertical is small
        if (Math.Abs(deltaX) > minSwipeDistance && deltaY < maxVerticalDeviation)
        {
            if (deltaX > 0 && ActiveTabIndex > 0)
                ActiveTabIndex--;
            else if (deltaX < 0 && ActiveTabIndex < 4)
                ActiveTabIndex++;
        }
    }
}
