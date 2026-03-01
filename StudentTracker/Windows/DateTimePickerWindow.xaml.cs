using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using StudentTracker.Controls;

namespace StudentTracker.Windows
{
    public partial class DateTimePickerWindow : Window
    {
        private static readonly string[] Months = { "يناير","فبراير","مارس","إبريل","مايو","يونيو","يوليو","أغسطس","سبتمبر","أكتوبر","نوفمبر","ديسمبر" };

        public DateTime? SelectedDateTime { get; private set; }
        public bool Confirmed { get; private set; } = false;

        private int _selectedDay, _selectedMonth, _selectedYear, _selectedHour, _selectedMinute;

        public DateTimePickerWindow(DateTime? initialDate = null)
        {
            InitializeComponent();
            var now = initialDate ?? DateTime.Now;
            _selectedDay = now.Day; _selectedMonth = now.Month; _selectedYear = now.Year;
            _selectedHour = now.Hour; _selectedMinute = (now.Minute / 5) * 5;
            this.Loaded += (s, e) => { SetupPickers(); UpdateDisplay(); };
            this.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
        }

        // دعم لوحة المفاتيح
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    BackButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.Enter:
                    ConfirmButton_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
            }
        }

        private void SetupPickers()
        {
            int days = DateTime.DaysInMonth(_selectedYear, _selectedMonth);
            DayPicker.ItemsSource = Enumerable.Range(1, days).Select(d => d.ToString("D2")).ToList();
            DayPicker.SelectedIndex = _selectedDay - 1;

            MonthPicker.ItemsSource = new List<string>(Months);
            MonthPicker.SelectedIndex = _selectedMonth - 1;

            YearPicker.ItemsSource = Enumerable.Range(_selectedYear - 1, 20).Select(y => y.ToString()).ToList();
            YearPicker.SelectedIndex = 1;

            HourPicker.ItemsSource = Enumerable.Range(0, 24).Select(h => h.ToString("D2")).ToList();
            HourPicker.SelectedIndex = _selectedHour;

            MinutePicker.ItemsSource = Enumerable.Range(0, 12).Select(m => (m * 5).ToString("D2")).ToList();
            MinutePicker.SelectedIndex = _selectedMinute / 5;
        }

        private void RefreshDays()
        {
            int days = DateTime.DaysInMonth(_selectedYear, _selectedMonth);
            _selectedDay = Math.Min(_selectedDay, days);
            DayPicker.ItemsSource = Enumerable.Range(1, days).Select(d => d.ToString("D2")).ToList();
            DayPicker.SelectedIndex = _selectedDay - 1;
        }

        private void UpdateDisplay()
        {
            DateDisplayText.Text = $"{_selectedDay:D2} {Months[_selectedMonth - 1]}";
            TimeDisplayText.Text = $"{_selectedYear}  —  {_selectedHour:D2}:{_selectedMinute:D2}";
        }

        private void Picker_SelectionChanged(object sender, string value)
        {
            var p = sender as WheelPickerControl;
            if (p == null || string.IsNullOrEmpty(value)) return;

            if (p == DayPicker && int.TryParse(value, out int d)) _selectedDay = d;
            else if (p == MonthPicker) { int i = Array.IndexOf(Months, value); if (i >= 0) { _selectedMonth = i + 1; RefreshDays(); } }
            else if (p == YearPicker && int.TryParse(value, out int y)) { _selectedYear = y; RefreshDays(); }
            else if (p == HourPicker && int.TryParse(value, out int h)) _selectedHour = h;
            else if (p == MinutePicker && int.TryParse(value, out int m)) _selectedMinute = m;

            UpdateDisplay();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) { Confirmed = false; DialogResult = false; Close(); }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedDateTime = new DateTime(_selectedYear, _selectedMonth, _selectedDay, _selectedHour, _selectedMinute, 0);
                Confirmed = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تاريخ غير صالح: " + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}