using StudentTracker.Models;
using StudentTracker.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StudentTracker.ViewModels;

/// <summary>
/// ViewModel لنافذة تعديل المخطط العام
/// يدعم نمط MVVM مع ميزات Undo/Redo و Drag & Drop
/// </summary>
public class EditPlanViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _db;
    private int _years;
    private int _semesters;
    private bool _isSaved;
    private string _statusMessage = string.Empty;
    private bool _hasUnsavedChanges;
    private string _searchQuery = string.Empty;
    private int _totalSubjectsCount;
    private int _existingSubjectsCount;
    private int _newSubjectsCount;
    private int _modifiedSubjectsCount;

    //Undo/Redo stacks
    private readonly Stack<UndoRedoAction> _undoStack = new();
    private readonly Stack<UndoRedoAction> _redoStack = new();

    public EditPlanViewModel(IDatabaseService db)
    {
        _db = db;
        SemesterCollections = new ObservableCollection<SemesterViewModel>();
        
        // Initialize commands
        BuildTablesCommand = new RelayCommand(BuildTables);
        SaveCommand = new RelayCommand(Save, CanSave);
        CloseCommand = new RelayCommand(Close);
        AddSubjectCommand = new RelayCommand(AddSubject);
        RemoveSubjectCommand = new RelayCommand(RemoveSubject);
        UndoCommand = new RelayCommand(Undo, CanUndo);
        RedoCommand = new RelayCommand(Redo, CanRedo);
        MoveSubjectUpCommand = new RelayCommand(MoveSubjectUp);
        MoveSubjectDownCommand = new RelayCommand(MoveSubjectDown);

        Years = 1;
        Semesters = 2;
        
        LoadExistingPlan();
    }

    #region Properties

    public int Years
    {
        get => _years;
        set
        {
            if (_years != value)
            {
                _years = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public int Semesters
    {
        get => _semesters;
        set
        {
            if (_semesters != value)
            {
                _semesters = value;
                OnPropertyChanged();
                HasUnsavedChanges = true;
            }
        }
    }

    public bool IsSaved
    {
        get => _isSaved;
        private set
        {
            _isSaved = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set
        {
            _hasUnsavedChanges = value;
            OnPropertyChanged();
            if (SaveCommand is RelayCommand saveCommand)
            {
                saveCommand.RaiseCanExecuteChanged();
            }
            UpdateSummary();
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged();
                ApplySearchHighlight();
            }
        }
    }

    public int TotalSubjectsCount
    {
        get => _totalSubjectsCount;
        private set
        {
            _totalSubjectsCount = value;
            OnPropertyChanged();
        }
    }

    public int ExistingSubjectsCount
    {
        get => _existingSubjectsCount;
        private set
        {
            _existingSubjectsCount = value;
            OnPropertyChanged();
        }
    }

    public int NewSubjectsCount
    {
        get => _newSubjectsCount;
        private set
        {
            _newSubjectsCount = value;
            OnPropertyChanged();
        }
    }

    public int ModifiedSubjectsCount
    {
        get => _modifiedSubjectsCount;
        private set
        {
            _modifiedSubjectsCount = value;
            OnPropertyChanged();
        }
    }

    public bool CanUndoValue => _undoStack.Count > 0;
    public bool CanRedoValue => _redoStack.Count > 0;

    public ObservableCollection<SemesterViewModel> SemesterCollections { get; }

    #endregion

    #region Commands

    public ICommand BuildTablesCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand AddSubjectCommand { get; }
    public ICommand RemoveSubjectCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand MoveSubjectUpCommand { get; }
    public ICommand MoveSubjectDownCommand { get; }

    #endregion

    #region Methods

    private void LoadExistingPlan()
    {
        var subjects = _db.GetAllSubjects();

        Years = Math.Max(1, subjects.Select(s => s.YearNumber).DefaultIfEmpty(1).Max());
        Semesters = Math.Max(2, subjects.Select(s => s.SemesterNumber).DefaultIfEmpty(2).Max());

        BuildTables(null);
        HasUnsavedChanges = false;
    }

    private void BuildTables(object? parameter)
    {
        if (Years <= 0 || Semesters <= 0)
        {
            StatusMessage = "الرجاء إدخال قيم صحيحة للسنوات والفصول";
            return;
        }

        SemesterCollections.Clear();
        var allSubjects = _db.GetAllSubjects();
        
        // Create semester ViewModels
        for (int year = 1; year <= Years; year++)
        {
            for (int sem = 1; sem <= Semesters; sem++)
            {
                var semesterVm = new SemesterViewModel(year, sem);
                
                // Load existing subjects
                var existingSubjects = allSubjects
                    .Where(s => s.YearNumber == year && s.SemesterNumber == sem)
                    .OrderBy(s => s.DisplayOrder)
                    .ToList();

                if (existingSubjects.Any())
                {
                    foreach (var subject in existingSubjects)
                    {
                        var vm = new SubjectItemViewModel
                        {
                            Id = subject.Id,
                            Name = subject.Name,
                            OriginalName = subject.Name,
                            IsExisting = true,
                            IsModified = false,
                            Semester = semesterVm
                        };
                        SubscribeSubject(vm);
                        semesterVm.Subjects.Add(vm);
                    }
                }
                else
                {
                    // Add one empty row for new semesters
                    var vm = new SubjectItemViewModel
                    {
                        IsNew = true,
                        Semester = semesterVm
                    };
                    SubscribeSubject(vm);
                    semesterVm.Subjects.Add(vm);
                }
                
                SemesterCollections.Add(semesterVm);
            }
        }

        StatusMessage = $"تم إنشاء {Years} سنوات × {Semesters} فصول";
        HasUnsavedChanges = true;
        ClearUndoRedo();
        ApplySearchHighlight();
        UpdateSummary();
    }

    private bool CanSave(object? parameter)
    {
        return Years > 0 && Semesters > 0 && SemesterCollections.Count > 0;
    }

    private void Save(object? parameter)
    {
        if (Years <= 0 || Semesters <= 0)
        {
            StatusMessage = "الرجاء إدخال قيم صحيحة";
            return;
        }

        if (SemesterCollections.Count == 0)
        {
            StatusMessage = "الرجاء إنشاء الجداول أولاً";
            return;
        }

        var data = new Dictionary<(int Year, int Semester), List<PlanSubjectInput>>();

        foreach (var semester in SemesterCollections)
        {
            var rows = semester.Subjects
                .Select((subject, index) =>
                {
                    var name = string.IsNullOrWhiteSpace(subject.Name) 
                        ? subject.OriginalName 
                        : subject.Name;

                    return new PlanSubjectInput
                    {
                        SubjectId = subject.Id,
                        Name = name,
                        DisplayOrder = index + 1
                    };
                })
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .ToList();

            data[(semester.Year, semester.Semester)] = rows;
        }

        _db.UpsertStudyPlan(Years, Semesters, data);
        IsSaved = true;
        HasUnsavedChanges = false;
        StatusMessage = "تم حفظ المخطط بنجاح";
    }

    private void Close(object? parameter)
    {
        // Close is handled in the code-behind
    }

    public void AddSubject(SemesterViewModel semester)
    {
        var newSubject = new SubjectItemViewModel
        {
            IsNew = true,
            Semester = semester
        };
        SubscribeSubject(newSubject);
        
        // Record for undo
        var action = new UndoRedoAction
        {
            ActionType = ActionType.Add,
            SemesterYear = semester.Year,
            SemesterNumber = semester.Semester,
            SubjectName = string.Empty,
            SubjectIndex = semester.Subjects.Count
        };
        
        semester.Subjects.Add(newSubject);
        
        PushUndo(action);
        HasUnsavedChanges = true;
        StatusMessage = "تمت إضافة مادة جديدة";
        ApplySearchHighlight();
    }

    private void AddSubject(object? parameter)
    {
        if (parameter is SemesterViewModel semester)
        {
            AddSubject(semester);
        }
    }

    public void RemoveSubject(SubjectItemViewModel subject, SemesterViewModel semester)
    {
        var index = semester.Subjects.IndexOf(subject);
        
        // Record for undo
        var action = new UndoRedoAction
        {
            ActionType = ActionType.Remove,
            SemesterYear = semester.Year,
            SemesterNumber = semester.Semester,
            SubjectName = subject.Name,
            SubjectIndex = index,
            SubjectId = subject.Id ?? 0
        };
        
        semester.Subjects.Remove(subject);
        UnsubscribeSubject(subject);
        
        PushUndo(action);
        HasUnsavedChanges = true;
        StatusMessage = "تمت إزالة المادة";
        ApplySearchHighlight();
    }

    private void RemoveSubject(object? parameter)
    {
        if (parameter is SubjectItemViewModel subject)
        {
            var semester = SemesterCollections.FirstOrDefault(s => 
                s.Subjects.Contains(subject));
            if (semester != null)
            {
                RemoveSubject(subject, semester);
            }
        }
    }

    public void MoveSubjectUp(SubjectItemViewModel subject)
    {
        var semester = SemesterCollections.FirstOrDefault(s => s.Subjects.Contains(subject));
        if (semester == null) return;

        var index = semester.Subjects.IndexOf(subject);
        if (index > 0)
        {
            var action = new UndoRedoAction
            {
                ActionType = ActionType.Move,
                SemesterYear = semester.Year,
                SemesterNumber = semester.Semester,
                SubjectName = subject.Name,
                SubjectIndex = index,
                TargetIndex = index - 1
            };

            semester.Subjects.Move(index, index - 1);
            PushUndo(action);
            HasUnsavedChanges = true;
            UpdateSummary();
        }
    }

    private void MoveSubjectUp(object? parameter)
    {
        if (parameter is SubjectItemViewModel subject)
        {
            MoveSubjectUp(subject);
        }
    }

    public void MoveSubjectDown(SubjectItemViewModel subject)
    {
        var semester = SemesterCollections.FirstOrDefault(s => s.Subjects.Contains(subject));
        if (semester == null) return;

        var index = semester.Subjects.IndexOf(subject);
        if (index < semester.Subjects.Count - 1)
        {
            var action = new UndoRedoAction
            {
                ActionType = ActionType.Move,
                SemesterYear = semester.Year,
                SemesterNumber = semester.Semester,
                SubjectName = subject.Name,
                SubjectIndex = index,
                TargetIndex = index + 1
            };

            semester.Subjects.Move(index, index + 1);
            PushUndo(action);
            HasUnsavedChanges = true;
            UpdateSummary();
        }
    }

    private void MoveSubjectDown(object? parameter)
    {
        if (parameter is SubjectItemViewModel subject)
        {
            MoveSubjectDown(subject);
        }
    }

    #region Undo/Redo

    private void PushUndo(UndoRedoAction action)
    {
        _undoStack.Push(action);
        _redoStack.Clear();
        OnPropertyChanged(nameof(CanUndoValue));
        OnPropertyChanged(nameof(CanRedoValue));
    }

    private bool CanUndo(object? parameter) => _undoStack.Count > 0;
    private bool CanRedo(object? parameter) => _redoStack.Count > 0;

    private void Undo(object? parameter)
    {
        if (_undoStack.Count == 0) return;

        var action = _undoStack.Pop();
        _redoStack.Push(action);

        var semester = SemesterCollections.FirstOrDefault(s => 
            s.Year == action.SemesterYear && s.Semester == action.SemesterNumber);
        
        if (semester == null) return;

        switch (action.ActionType)
        {
            case ActionType.Add:
                if (semester.Subjects.Count > action.SubjectIndex)
                {
                    var removed = semester.Subjects[action.SubjectIndex];
                    semester.Subjects.RemoveAt(action.SubjectIndex);
                    UnsubscribeSubject(removed);
                }
                break;
            case ActionType.Remove:
                var restoredSubject = new SubjectItemViewModel
                {
                    Name = action.SubjectName,
                    Id = action.SubjectId,
                    OriginalName = action.SubjectName,
                    IsExisting = action.SubjectId != 0,
                    Semester = semester
                };
                SubscribeSubject(restoredSubject);
                semester.Subjects.Insert(action.SubjectIndex, restoredSubject);
                break;
            case ActionType.Move:
                semester.Subjects.Move(action.TargetIndex, action.SubjectIndex);
                break;
            case ActionType.Edit:
                var editedSubject = semester.Subjects.FirstOrDefault(s => s.Name == action.NewName);
                if (editedSubject != null)
                {
                    editedSubject.Name = action.SubjectName;
                }
                break;
        }

        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(CanUndoValue));
        OnPropertyChanged(nameof(CanRedoValue));
        StatusMessage = "تم التراجع";
        ApplySearchHighlight();
        UpdateSummary();
    }

    private void Redo(object? parameter)
    {
        if (_redoStack.Count == 0) return;

        var action = _redoStack.Pop();
        _undoStack.Push(action);

        var semester = SemesterCollections.FirstOrDefault(s => 
            s.Year == action.SemesterYear && s.Semester == action.SemesterNumber);
        
        if (semester == null) return;

        switch (action.ActionType)
        {
            case ActionType.Add:
                var added = new SubjectItemViewModel
                {
                    IsNew = true,
                    Semester = semester
                };
                SubscribeSubject(added);
                semester.Subjects.Insert(action.SubjectIndex, added);
                break;
            case ActionType.Remove:
                if (semester.Subjects.Count > action.SubjectIndex)
                {
                    var removed = semester.Subjects[action.SubjectIndex];
                    semester.Subjects.RemoveAt(action.SubjectIndex);
                    UnsubscribeSubject(removed);
                }
                break;
            case ActionType.Move:
                semester.Subjects.Move(action.SubjectIndex, action.TargetIndex);
                break;
            case ActionType.Edit:
                var editedSubject = semester.Subjects.FirstOrDefault(s => s.OriginalName == action.SubjectName);
                if (editedSubject != null)
                {
                    editedSubject.Name = action.NewName;
                }
                break;
        }

        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(CanUndoValue));
        OnPropertyChanged(nameof(CanRedoValue));
        StatusMessage = "تم إعادة التنفيذ";
        ApplySearchHighlight();
        UpdateSummary();
    }

    private void ClearUndoRedo()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnPropertyChanged(nameof(CanUndoValue));
        OnPropertyChanged(nameof(CanRedoValue));
    }

    #endregion

    public void RecordEdit(SubjectItemViewModel subject, string oldName, string newName)
    {
        var action = new UndoRedoAction
        {
            ActionType = ActionType.Edit,
            SemesterYear = subject.Semester?.Year ?? 0,
            SemesterNumber = subject.Semester?.Semester ?? 0,
            SubjectName = oldName,
            NewName = newName,
            SubjectIndex = subject.Semester?.Subjects.IndexOf(subject) ?? 0
        };
        PushUndo(action);
        HasUnsavedChanges = true;
    }

    private void SubscribeSubject(SubjectItemViewModel subject)
    {
        subject.PropertyChanged += Subject_PropertyChanged;
    }

    private void UnsubscribeSubject(SubjectItemViewModel subject)
    {
        subject.PropertyChanged -= Subject_PropertyChanged;
    }

    private void Subject_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SubjectItemViewModel.Name) ||
            e.PropertyName == nameof(SubjectItemViewModel.IsModified))
        {
            HasUnsavedChanges = true;
            ApplySearchHighlight();
            UpdateSummary();
        }
    }

    private void ApplySearchHighlight()
    {
        var hasQuery = !string.IsNullOrWhiteSpace(SearchQuery);
        var query = SearchQuery?.Trim() ?? string.Empty;

        foreach (var semester in SemesterCollections)
        {
            foreach (var subject in semester.Subjects)
            {
                subject.IsHighlighted = hasQuery &&
                    !string.IsNullOrWhiteSpace(subject.Name) &&
                    subject.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }

    private void UpdateSummary()
    {
        var allSubjects = SemesterCollections
            .SelectMany(s => s.Subjects)
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .ToList();

        TotalSubjectsCount = allSubjects.Count;
        ExistingSubjectsCount = allSubjects.Count(s => s.IsExisting && !s.IsModified);
        ModifiedSubjectsCount = allSubjects.Count(s => s.IsModified);
        NewSubjectsCount = allSubjects.Count - ExistingSubjectsCount - ModifiedSubjectsCount;
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

/// <summary>
/// ViewModel لفصل دراسي واحد
/// </summary>
public class SemesterViewModel : INotifyPropertyChanged
{
    public SemesterViewModel(int year, int semester)
    {
        Year = year;
        Semester = semester;
        Subjects = new ObservableCollection<SubjectItemViewModel>();
    }

    public int Year { get; }
    public int Semester { get; }
    public string DisplayName => $"السنة {Year} - الفصل {Semester}";
    public ObservableCollection<SubjectItemViewModel> Subjects { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// ViewModel لمادة واحدة
/// </summary>
public class SubjectItemViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _originalName = string.Empty;
    private bool _isExisting;
    private bool _isNew;
    private bool _isModified;
    private bool _isHighlighted;

    public int? Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
                IsModified = IsExisting && _name != _originalName;
            }
        }
    }

    public string OriginalName
    {
        get => _originalName;
        set
        {
            _originalName = value;
            OnPropertyChanged();
        }
    }

    public bool IsExisting
    {
        get => _isExisting;
        set
        {
            _isExisting = value;
            OnPropertyChanged();
        }
    }

    public bool IsNew
    {
        get => _isNew;
        set
        {
            _isNew = value;
            OnPropertyChanged();
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            _isModified = value;
            OnPropertyChanged();
        }
    }

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            _isHighlighted = value;
            OnPropertyChanged();
        }
    }

    public SemesterViewModel? Semester { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? OriginalName : Name;
    public string StatusColor => IsModified ? "#F59E0B" : (IsExisting ? "#10B981" : "#6366F1");
    public string StatusIcon => IsModified ? "Pencil" : (IsExisting ? "Check" : "Plus");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// نوع إجراء undo/redo
/// </summary>
public enum ActionType
{
    Add,
    Remove,
    Move,
    Edit
}

/// <summary>
/// عنصر في سجل undo/redo
/// </summary>
public class UndoRedoAction
{
    public ActionType ActionType { get; set; }
    public int SemesterYear { get; set; }
    public int SemesterNumber { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public int SubjectIndex { get; set; }
    public int TargetIndex { get; set; }
}
