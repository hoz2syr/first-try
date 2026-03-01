using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using StudentTracker.Controls;

namespace StudentTracker.Windows;

public partial class DateTimeDialog : Window
{
    public DateTime? SelectedDateTime { get; private set; }
    private int _selectedHour = 9;
    private int _selectedMinute = 0;
    private bool _isInitializing = true;

    public DateTimeDialog(DateTime? initialDate, string initialTime)
    {
        InitializeComponent();
        
        // Set initial date
        DatePickerControl.SelectedDate = initialDate ?? DateTime.Today;
        
        // Parse initial time if provided
        if (!string.IsNullOrWhiteSpace(initialTime) && TimeSpan.TryParse(initialTime, out var ts))
        {
            _selectedHour = ts.Hours;
            _selectedMinute = ts.Minutes;
        }
        
        // Allow dragging the borderless window
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        };
        
        Loaded += DateTimeDialog_Loaded;
    }

    private void DateTimeDialog_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            InitializeWheels();
            SelectInitialTime();
            _isInitializing = false;
            UpdateSelectedTimeText();
            UpdateHeaderPreview();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void InitializeWheels()
    {
        try
        {
            var hours = new List<string>();
            for (int i = 0; i < 24; i++) hours.Add(i.ToString("D2"));
            if (HoursWheelPicker != null)
            {
                HoursWheelPicker.ItemsSource = hours;
                HoursWheelPicker.SelectedIndex = Math.Max(0, Math.Min(23, _selectedHour));
            }

            var minutes = new List<string>();
            for (int i = 0; i < 60; i++) minutes.Add(i.ToString("D2"));
            if (MinutesWheelPicker != null)
            {
                MinutesWheelPicker.ItemsSource = minutes;
                MinutesWheelPicker.SelectedIndex = Math.Max(0, Math.Min(59, _selectedMinute));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing wheels: {ex.Message}");
        }
    }

    private void SelectInitialTime()
    {
        try
        {
            if (HoursWheelPicker != null)
                HoursWheelPicker.SelectedIndex = Math.Max(0, Math.Min(23, _selectedHour));

            if (MinutesWheelPicker != null)
                MinutesWheelPicker.SelectedIndex = Math.Max(0, Math.Min(59, _selectedMinute));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error selecting initial time: {ex.Message}");
        }
    }

    private void Picker_SelectionChanged(object sender, string value)
    {
        if (_isInitializing) return;
        if (string.IsNullOrEmpty(value)) return;

        if (sender == HoursWheelPicker)
        {
            if (int.TryParse(value, out int h)) _selectedHour = h;
        }
        else if (sender == MinutesWheelPicker)
        {
            if (int.TryParse(value, out int m)) _selectedMinute = m;
        }

        UpdateSelectedTimeText();
        UpdateHeaderPreview();
    }

    private void UpdateSelectedTimeText()
    {
        if (SelectedTimeText != null)
            SelectedTimeText.Text = $"{_selectedHour:D2}:{_selectedMinute:D2}";
    }

    private void UpdateHeaderPreview()
    {
        if (HeaderPreviewText == null || DatePickerControl?.SelectedDate == null) return;
        
        var date = DatePickerControl.SelectedDate.Value;
        HeaderPreviewText.Text = $"{date:yyyy/MM/dd} - {_selectedHour:D2}:{_selectedMinute:D2}";
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!DatePickerControl.SelectedDate.HasValue)
        {
            MessageBox.Show(this, "يرجى اختيار تاريخ صحيح.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var time = new TimeSpan(_selectedHour, _selectedMinute, 0);
        SelectedDateTime = DatePickerControl.SelectedDate.Value.Date.Add(time);
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
