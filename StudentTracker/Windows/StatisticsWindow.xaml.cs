using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using StudentTracker.Models;
using StudentTracker.Services;

namespace StudentTracker.Windows
{
    public partial class StatisticsWindow : Window
    {
        private readonly IDatabaseService _db;
        private readonly int _currentYear;

        public StatisticsWindow(IDatabaseService db, int currentYear)
        {
            _db = db;
            _currentYear = currentYear;
            InitializeComponent();
            LoadStatistics();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // إضافة تأثيرات الحركة عند تحميل النافذة
            AddSlideAnimation(TotalSubjectsText, 0);
            AddSlideAnimation(PassedSubjectsText, 100);
            AddSlideAnimation(FailedSubjectsText, 200);
            AddSlideAnimation(AverageGradeText, 300);
            AddSlideAnimation(PendingSubjectsText, 400);
            AddSlideAnimation(RemainingSubjectsText, 500);
        }

        private void AddSlideAnimation(UIElement element, int delayMs)
        {
            if (element == null) return;
            
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400),
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            var translateTransform = new TranslateTransform(0, 20);
            element.RenderTransform = translateTransform;
            
            var slideIn = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, slideIn);
        }

        private void LoadStatistics()
        {
            try
            {
                var subjects = _db.GetAllSubjects().ToList();
                var retakeIds = _db.GetRetakeLabSubjectIds();

                // Overall statistics
                var totalSubjects = subjects.Count;
                var passedCount = 0;
                var failedCount = 0;
                var pendingCount = 0;
                var remainingCount = 0;
                var totalGradeSum = 0.0;
                var gradeCount = 0;

                foreach (var subject in subjects)
                {
                    var practical = _db.GetLatestGrade(subject.Id, "عملي");
                    var theory = _db.GetLatestGrade(subject.Id, "نظري");
                    var status = GradeCalculator.ComputeStatus(_currentYear, subject.YearNumber, practical, theory, retakeIds.Contains(subject.Id));

                    switch (status)
                    {
                        case SubjectStatus.Passed:
                            passedCount++;
                            break;
                        case SubjectStatus.Failed:
                            failedCount++;
                            break;
                        case SubjectStatus.Pending:
                            pendingCount++;
                            break;
                        default:
                            remainingCount++;
                            break;
                    }

                    var finalGrade = GradeCalculator.ComputeFinalGrade(practical, theory, retakeIds.Contains(subject.Id));
                    if (finalGrade.HasValue)
                    {
                        totalGradeSum += finalGrade.Value;
                        gradeCount++;
                    }
                }

                // Update UI
                TotalSubjectsText.Text = totalSubjects.ToString();
                PassedSubjectsText.Text = passedCount.ToString();
                FailedSubjectsText.Text = failedCount.ToString();
                PendingSubjectsText.Text = pendingCount.ToString();
                RemainingSubjectsText.Text = remainingCount.ToString();

                var averageGrade = gradeCount > 0 ? totalGradeSum / gradeCount : 0;
                AverageGradeText.Text = averageGrade.ToString("0.##");

                // Progress bars
                if (totalSubjects > 0)
                {
                    PassedProgressBar.Value = (double)passedCount / totalSubjects * 100;
                    FailedProgressBar.Value = (double)failedCount / totalSubjects * 100;
                    PassedPercentageText.Text = $"{PassedProgressBar.Value:0.#}%";
                    FailedPercentageText.Text = $"{FailedProgressBar.Value:0.#}%";
                }

                // Year-wise statistics
                LoadYearStatistics(subjects, retakeIds);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError($"خطأ في تحميل الإحصائيات: {ex.Message}", "خطأ", this);
            }
        }

        private void LoadYearStatistics(List<Subject> subjects, HashSet<int> retakeIds)
        {
            var yearStats = new List<YearStatistic>();
            var yearGroups = subjects.GroupBy(s => s.YearNumber).OrderBy(g => g.Key);

            foreach (var yearGroup in yearGroups)
            {
                var yearNumber = yearGroup.Key;
                var yearSubjects = yearGroup.ToList();
                
                var passed = 0;
                var failed = 0;
                var pending = 0;
                var remaining = 0;

                foreach (var subject in yearSubjects)
                {
                    var practical = _db.GetLatestGrade(subject.Id, "عملي");
                    var theory = _db.GetLatestGrade(subject.Id, "نظري");
                    var status = GradeCalculator.ComputeStatus(_currentYear, subject.YearNumber, practical, theory, retakeIds.Contains(subject.Id));

                    switch (status)
                    {
                        case SubjectStatus.Passed:
                            passed++;
                            break;
                        case SubjectStatus.Failed:
                            failed++;
                            break;
                        case SubjectStatus.Pending:
                            pending++;
                            break;
                        default:
                            remaining++;
                            break;
                    }
                }

                var total = yearSubjects.Count;
                var completed = passed + failed;
                var completionPercentage = total > 0 ? (double)completed / total * 100 : 0;

                yearStats.Add(new YearStatistic
                {
                    YearNumber = yearNumber,
                    PassedCount = passed,
                    FailedCount = failed,
                    PendingCount = pending,
                    RemainingCount = remaining,
                    TotalCount = total,
                    CompletionPercentage = completionPercentage
                });
            }

            YearStatisticsPanel.ItemsSource = yearStats;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class YearStatistic
    {
        public int YearNumber { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public int RemainingCount { get; set; }
        public int TotalCount { get; set; }
        public double CompletionPercentage { get; set; }
    }
}