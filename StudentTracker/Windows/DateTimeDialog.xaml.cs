using System;
using System.Collections.Generic;
using System.Windows;
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
        
        // Initialize wheel pickers after the window is loaded
        Loaded += DateTimeDialog_Loaded;
    }

    private void DateTimeDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Use dispatcher to ensure UI is fully loaded
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // Initialize wheel pickers
            InitializeWheels();
            
            // Select initial values
            SelectInitialTime();
            
            _isInitializing = false;
            UpdateSelectedTimeText();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void InitializeWheels()
    {
        try
        {
            // Populate hours (00-23)
            var hours = new List<string>();
            for (int i = 0; i < 24; i++) hours.Add(i.ToString("D2"));
            if (HoursWheelPicker != null)
            {
                HoursWheelPicker.ItemsSource = hours;
                HoursWheelPicker.SelectedIndex = Math.Max(0, Math.Min(23, _selectedHour));
            }

            // Populate minutes (00-59)
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
            // Log error or handle gracefully
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
    }

    private void UpdateSelectedTimeText()
    {
        SelectedTimeText.Text = $"{_selectedHour:D2}:{_selectedMinute:D2}";
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

    private void TodayButton_Click(object sender, RoutedEventArgs e)
    {
        DatePickerControl.SelectedDate = DateTime.Today;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
