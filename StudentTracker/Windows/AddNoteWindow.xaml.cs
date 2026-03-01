using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StudentTracker.Windows
{
    public partial class AddNoteWindow : Window, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool _disposed = false;
        private DispatcherTimer? _autoSaveTimer;
        private string _lastSavedContent = string.Empty;
        
        public string NoteTitle { get; private set; } = string.Empty;
        public string NoteContent { get; private set; } = string.Empty;
        public DateTime NoteDate { get; private set; } = DateTime.Today;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddNoteWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            NoteDatePicker.SelectedDate = DateTime.Today;
            ContentRendered += OnWindowContentRendered;
            ContentTextBox.TextChanged += OnContentTextChanged;
            KeyDown += OnWindowKeyDown;
            
            UpdateCharacterCount();
            SetupAutoSave();
        }
        
        private void OnWindowContentRendered(object? sender, EventArgs e)
        {
            TitleTextBox.Focus();
            Keyboard.Focus(TitleTextBox);
            TitleTextBox.SelectAll();
            LoadDraftIfExists();
        }
        
        private void OnContentTextChanged(object? sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateCharacterCount();
        }
        
        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveButton_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                CancelButton_Click(this, new RoutedEventArgs());
            }
        }
        
        private void SetupAutoSave()
        {
            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _autoSaveTimer.Tick += AutoSave_Tick;
            _autoSaveTimer.Start();
        }
        
        private void AutoSave_Tick(object? sender, EventArgs e)
        {
            var currentContent = ContentTextBox?.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(currentContent) && currentContent != _lastSavedContent)
            {
                try
                {
                    var title = TitleTextBox?.Text?.Trim() ?? "ملاحظة مؤقتة";
                    Properties.Settings.Default.DraftNoteTitle = title;
                    Properties.Settings.Default.DraftNoteContent = currentContent;
                    Properties.Settings.Default.DraftNoteDate = NoteDatePicker.SelectedDate ?? DateTime.Today;
                    Properties.Settings.Default.Save();
                    _lastSavedContent = currentContent;
                }
                catch { }
            }
        }
        
        private void LoadDraftIfExists()
        {
            try
            {
                var draftContent = Properties.Settings.Default.DraftNoteContent;
                if (!string.IsNullOrWhiteSpace(draftContent))
                {
                    var result = CustomMessageBox.ShowConfirm(
                        "تم العثور على مسودة ملاحظة محفوظة مسبقاً. هل تريد استعادتها؟",
                        "استعادة مسودة", this);
                        
                    if (result)
                    {
                        TitleTextBox.Text = Properties.Settings.Default.DraftNoteTitle;
                        ContentTextBox.Text = draftContent;
                        NoteDatePicker.SelectedDate = Properties.Settings.Default.DraftNoteDate;
                        _lastSavedContent = draftContent;
                    }
                    else
                    {
                        ClearDraft();
                    }
                }
            }
            catch { }
        }
        
        private void ClearDraft()
        {
            try
            {
                Properties.Settings.Default.DraftNoteTitle = string.Empty;
                Properties.Settings.Default.DraftNoteContent = string.Empty;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        
        private void UpdateCharacterCount()
        {
            var count = ContentTextBox?.Text?.Length ?? 0;
            var characterCountElement = FindName("CharacterCountText") as System.Windows.Controls.TextBlock;
            
            if (characterCountElement != null)
            {
                characterCountElement.Text = $"{count}/2000";
                
                if (count > 1900)
                    characterCountElement.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 38, 127));
                else if (count > 1800)
                    characterCountElement.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68));
                else if (count > 1500)
                    characterCountElement.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 158, 11));
                else if (count > 1000)
                    characterCountElement.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246));
                else
                    characterCountElement.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128));
            }
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var title = TitleTextBox.Text?.Trim() ?? string.Empty;
                var content = ContentTextBox.Text?.Trim() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(title))
                {
                    ShowValidationError("يرجى إدخال عنوان للملاحظة", TitleTextBox);
                    return;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    ShowValidationError("يرجى إدخال محتوى للملاحظة", ContentTextBox);
                    return;
                }

                if (NoteDatePicker.SelectedDate == null)
                {
                    ShowValidationError("يرجى اختيار تاريخ للملاحظة", NoteDatePicker);
                    return;
                }

                if (title.Length < 3)
                {
                    ShowValidationError("يجب أن يكون عنوان الملاحظة 3 أحرف على الأقل", TitleTextBox);
                    return;
                }
                
                if (content.Length < 3)
                {
                    ShowValidationError("يجب أن يكون محتوى الملاحظة 3 أحرف على الأقل", ContentTextBox);
                    return;
                }

                NoteTitle = title;
                NoteContent = content;
                NoteDate = NoteDatePicker.SelectedDate.Value;
                
                ClearDraft();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError($"حدث خطأ أثناء حفظ الملاحظة:\n{ex.Message}", "خطأ في الحفظ", this);
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            var hasData = !string.IsNullOrWhiteSpace(TitleTextBox.Text) || !string.IsNullOrWhiteSpace(ContentTextBox.Text);
            
            if (hasData)
            {
                var result = CustomMessageBox.ShowConfirm(
                    "هل أنت متأكد من إلغاء العملية؟\nسيتم فقدان جميع البيانات المدخلة.",
                    "تأكيد الإلغاء", this);
                    
                if (!result) return;
                ClearDraft();
            }
            
            DialogResult = false;
            Close();
        }
        
        private void ShowValidationError(string message, System.Windows.Controls.Control control)
        {
            CustomMessageBox.ShowWarning(message, "خطأ في الإدخال", this);
            control.Focus();
            if (control is System.Windows.Controls.TextBox textBox) textBox.SelectAll();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                ContentRendered -= OnWindowContentRendered;
                ContentTextBox.TextChanged -= OnContentTextChanged;
                KeyDown -= OnWindowKeyDown;
                
                if (_autoSaveTimer != null)
                {
                    _autoSaveTimer.Tick -= AutoSave_Tick;
                    _autoSaveTimer.Stop();
                    _autoSaveTimer = null;
                }
                
                _disposed = true;
            }
        }
        
        ~AddNoteWindow()
        {
            Dispose(false);
        }
    }
}