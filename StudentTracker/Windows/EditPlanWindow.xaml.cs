using StudentTracker.Models;
using StudentTracker.Services;
using StudentTracker.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentTracker.Windows;

/// <summary>
/// نافذة تعديل المخطط العام - محسنة مع دعم MVVM و Drag & Drop
/// </summary>
public partial class EditPlanWindow : Window
{
    private readonly EditPlanViewModel _viewModel;
    private Point _dragStartPoint;

    public EditPlanWindow(IDatabaseService db)
    {
        InitializeComponent();
        
        // Initialize ViewModel
        _viewModel = new EditPlanViewModel(db);
        DataContext = _viewModel;

        // Handle close command from ViewModel
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public bool IsSaved => _viewModel.IsSaved;

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditPlanViewModel.HasUnsavedChanges) && _viewModel.HasUnsavedChanges)
        {
            // Track unsaved changes
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.HasUnsavedChanges)
        {
            var result = CustomMessageBox.ShowConfirm(
                "هل أنت متأكد من الإغلاق؟",
                "تغييرات غير محفوظة",
                this);

            if (!result)
                return;
        }

        DialogResult = _viewModel.IsSaved;
        Close();
    }

    private void NumbersOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }

    private void SubjectName_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (sender is not TextBox textBox || textBox.DataContext is not SubjectItemViewModel subject)
            return;

        var semester = _viewModel.SemesterCollections.FirstOrDefault(s => s.Subjects.Contains(subject));
        if (semester == null)
            return;

        _viewModel.AddSubject(semester);
        e.Handled = true;
    }

    #region Drag and Drop

    private void SubjectRow_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(this);
            var diff = _dragStartPoint - position;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var border = sender as Border;
                var subject = border?.DataContext as SubjectItemViewModel;
                
                if (subject != null)
                {
                    DragDrop.DoDragDrop(border!, subject, DragDropEffects.Move);
                }
            }
        }
    }

    private void SubjectRow_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(SubjectItemViewModel)))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var draggedSubject = e.Data.GetData(typeof(SubjectItemViewModel)) as SubjectItemViewModel;
        var targetBorder = sender as Border;
        var targetSubject = targetBorder?.DataContext as SubjectItemViewModel;

        if (draggedSubject != null && targetSubject != null && draggedSubject != targetSubject)
        {
            // Check if they are in the same semester
            var sourceSemester = _viewModel.SemesterCollections
                .FirstOrDefault(s => s.Subjects.Contains(draggedSubject));
            var targetSemester = _viewModel.SemesterCollections
                .FirstOrDefault(s => s.Subjects.Contains(targetSubject));

            if (sourceSemester != null && targetSemester != null && 
                sourceSemester.Year == targetSemester.Year && 
                sourceSemester.Semester == targetSemester.Semester)
            {
                e.Effects = DragDropEffects.Move;
                
                // Visual feedback
                targetBorder!.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#6366F1"));
                targetBorder.BorderThickness = new Thickness(2);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void SubjectRow_DragLeave(object sender, DragEventArgs e)
    {
        var border = sender as Border;
        if (border != null)
        {
            border.BorderBrush = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E8F0"));
            border.BorderThickness = new Thickness(1);
        }
    }

    private void SubjectRow_Drop(object sender, DragEventArgs e)
    {
        var targetBorder = sender as Border;
        if (targetBorder != null)
        {
            // Reset visual feedback
            targetBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E8F0"));
            targetBorder.BorderThickness = new Thickness(1);
        }

        if (!e.Data.GetDataPresent(typeof(SubjectItemViewModel)))
            return;

        var draggedSubject = e.Data.GetData(typeof(SubjectItemViewModel)) as SubjectItemViewModel;
        var targetSubject = targetBorder?.DataContext as SubjectItemViewModel;

        if (draggedSubject == null || targetSubject == null)
            return;

        // Find semesters
        var sourceSemester = _viewModel.SemesterCollections
            .FirstOrDefault(s => s.Subjects.Contains(draggedSubject));
        var targetSemester = _viewModel.SemesterCollections
            .FirstOrDefault(s => s.Subjects.Contains(targetSubject));

        if (sourceSemester == null || targetSemester == null)
            return;

        // Only allow dropping within the same semester
        if (sourceSemester.Year == targetSemester.Year && 
            sourceSemester.Semester == targetSemester.Semester)
        {
            var sourceIndex = sourceSemester.Subjects.IndexOf(draggedSubject);
            var targetIndex = targetSemester.Subjects.IndexOf(targetSubject);

            if (sourceIndex != targetIndex)
            {
                sourceSemester.Subjects.Move(sourceIndex, targetIndex);
                _viewModel.HasUnsavedChanges = true;
                _viewModel.StatusMessage = "تم نقل المادة";
            }
        }

        e.Handled = true;
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        _dragStartPoint = e.GetPosition(this);
    }

    #endregion

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_viewModel.HasUnsavedChanges && !_viewModel.IsSaved)
        {
            var result = CustomMessageBox.ShowConfirm(
                "هل أنت متأكد من الإغلاق؟",
                "تغييرات غير محفوظة",
                this);

            if (!result)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnClosing(e);
    }
}
