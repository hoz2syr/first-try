using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StudentTracker.Controls
{
    public partial class WheelPickerControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(List<string>), typeof(WheelPickerControl),
                new PropertyMetadata(null, OnItemsSourceChanged));
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(WheelPickerControl),
                new PropertyMetadata(0, OnSelectedIndexChanged, CoerceSelectedIndex));
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(nameof(SelectedValue), typeof(string), typeof(WheelPickerControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

        public List<string> ItemsSource { get => (List<string>)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
        public string SelectedValue { get => (string)GetValue(SelectedValueProperty); set => SetValue(SelectedValueProperty, value); }
        public event EventHandler<string>? SelectionChanged;

        private const double ItemHeight = 42;
        private const double ContainerHeight = 200;
        private const double CenterOffset = (ContainerHeight - ItemHeight) / 2.0;

        private double _currentOffset = 0;
        private bool _isDragging = false;
        private bool _isUpdatingFromCode = false;
        private double _dragStartY, _dragStartOffset, _velocity, _lastY;
        private DateTime _lastTime;
        private System.Windows.Threading.DispatcherTimer _inertiaTimer;
        private List<TextBlock> _textBlocks = new();
        private int _lastSelectedIndex = -1;

        private int LastIndex() => Math.Max(0, (ItemsSource?.Count ?? 1) - 1);

        public WheelPickerControl()
        {
            InitializeComponent();
            _inertiaTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _inertiaTimer.Tick += (s, e) => { _inertiaTimer.Stop(); SnapToNearest(0); };
            this.Loaded += (s, e) => { BuildItems(); };
            
            // إضافة دعم لوحة المفاتيح
            this.PreviewKeyDown += OnPreviewKeyDown;
            this.IsTabStop = true;
        }

        // دعم لوحة المفاتيح (الأعلى والأسفل)
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ItemsSource == null || ItemsSource.Count == 0) return;
            
            switch (e.Key)
            {
                case Key.Up:
                    MoveSelection(-1);
                    e.Handled = true;
                    break;
                case Key.Down:
                    MoveSelection(1);
                    e.Handled = true;
                    break;
                case Key.PageUp:
                    MoveSelection(-3);
                    e.Handled = true;
                    break;
                case Key.PageDown:
                    MoveSelection(3);
                    e.Handled = true;
                    break;
                case Key.Home:
                    SelectedIndex = 0;
                    e.Handled = true;
                    break;
                case Key.End:
                    SelectedIndex = LastIndex();
                    e.Handled = true;
                    break;
            }
        }

        private void MoveSelection(int delta)
        {
            if (ItemsSource == null || ItemsSource.Count == 0) return;
            int newIndex = Math.Max(0, Math.Min(LastIndex(), SelectedIndex + delta));
            if (newIndex != SelectedIndex)
            {
                SelectedIndex = newIndex;
            }
        }

        private void BuildItems()
        {
            if (ItemsCanvas == null) return;
            ItemsCanvas.Children.Clear(); _textBlocks.Clear();
            if (ItemsSource == null || ItemsSource.Count == 0) return;
            
            double width = ActualWidth > 0 ? ActualWidth : 120;
            for (int i = 0; i < ItemsSource.Count; i++)
            {
                var tb = new TextBlock { 
                    Text = ItemsSource[i], 
                    Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)), 
                    FontSize = 16, 
                    FontFamily = new FontFamily("Segoe UI"), 
                    TextAlignment = TextAlignment.Center, 
                    Width = width, 
                    Height = ItemHeight, 
                    IsHitTestVisible = false,
                    Padding = new Thickness(0, 10, 0, 0)
                };
                _textBlocks.Add(tb); ItemsCanvas.Children.Add(tb);
            }
            
            // Initialize with current SelectedIndex
            int validIndex = Math.Max(0, Math.Min(ItemsSource.Count - 1, SelectedIndex));
            _currentOffset = GetOffsetForIndex(validIndex);
            _lastSelectedIndex = validIndex;
            UpdatePositions(_currentOffset, true);
        }

        private void UpdatePositions(double offset, bool forceUpdate = false)
        {
            if (_textBlocks.Count == 0 || ItemsSource == null || ItemsSource.Count == 0) return;
            double width = ActualWidth > 0 ? ActualWidth : 120;
            for (int i = 0; i < _textBlocks.Count; i++)
            {
                Canvas.SetLeft(_textBlocks[i], 0);
                Canvas.SetTop(_textBlocks[i], CenterOffset + offset + i * ItemHeight);
                _textBlocks[i].Width = width;
                double dist = Math.Abs(offset + i * ItemHeight) / ItemHeight;
                bool sel = dist < 0.5;
                _textBlocks[i].Opacity = sel ? 1.0 : Math.Max(0.25, 1.0 - dist * 0.35);
                _textBlocks[i].FontWeight = sel ? FontWeights.Bold : FontWeights.Normal;
                _textBlocks[i].FontSize = sel ? 18 : 15;
                _textBlocks[i].Foreground = sel 
                    ? new SolidColorBrush(Color.FromRgb(0x63, 0x66, 0xF1)) 
                    : new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
                double scale = Math.Max(0.7, 1.0 - dist * 0.08);
                _textBlocks[i].RenderTransformOrigin = new Point(0.5, 0.5);
                _textBlocks[i].RenderTransform = new ScaleTransform(scale, scale);
            }
            
            int idx = GetIndexFromOffset(offset);
            if ((idx != _lastSelectedIndex || forceUpdate) && ItemsSource != null && idx >= 0 && idx < ItemsSource.Count)
            {
                _lastSelectedIndex = idx;
                
                if (!_isUpdatingFromCode)
                {
                    _isUpdatingFromCode = true;
                    try
                    {
                        SelectedIndex = idx;
                        SelectedValue = ItemsSource[idx];
                        SelectionChanged?.Invoke(this, ItemsSource[idx]);
                    }
                    finally
                    {
                        _isUpdatingFromCode = false;
                    }
                }
            }
        }

        private double GetOffsetForIndex(int i) { if (ItemsSource == null || ItemsSource.Count == 0) return 0; i = Math.Max(0, Math.Min(ItemsSource.Count - 1, i)); return -i * ItemHeight; }
        private int GetIndexFromOffset(double o) { if (ItemsSource == null || ItemsSource.Count == 0) return 0; return Math.Max(0, Math.Min(ItemsSource.Count - 1, (int)Math.Round(-o / ItemHeight))); }
        private double ClampOffset(double o) { double min = GetOffsetForIndex(LastIndex()); return Math.Max(min, Math.Min(0, o)); }

        private static object CoerceSelectedIndex(DependencyObject d, object baseValue)
        {
            var control = (WheelPickerControl)d;
            int value = (int)baseValue;
            if (control.ItemsSource == null || control.ItemsSource.Count == 0) return 0;
            return Math.Max(0, Math.Min(control.ItemsSource.Count - 1, value));
        }

        private void AnimateTo(double target, bool notifyChange = true)
        {
            _inertiaTimer.Stop();
            double start = _currentOffset, elapsed = 0;
            var t = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            t.Tick += (s, e) => { 
                elapsed += 16; 
                double p = Math.Min(elapsed / 260.0, 1.0); 
                _currentOffset = start + (target - start) * (1 - Math.Pow(1 - p, 3)); 
                UpdatePositions(_currentOffset); 
                if (p >= 1) { 
                    _currentOffset = target; 
                    UpdatePositions(_currentOffset); 
                    if (notifyChange && ItemsSource != null && _lastSelectedIndex >= 0 && _lastSelectedIndex < ItemsSource.Count)
                    {
                        SelectionChanged?.Invoke(this, ItemsSource[_lastSelectedIndex]);
                    }
                    if (s is System.Windows.Threading.DispatcherTimer dt) dt.Stop(); 
                } 
            };
            t.Start();
        }

        private void SnapToNearest(double vel = 0) { double proj = ClampOffset(_currentOffset + vel * 80); AnimateTo(GetOffsetForIndex(GetIndexFromOffset(proj))); }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsCanvas == null) return;
            _inertiaTimer.Stop(); _isDragging = true; _dragStartY = e.GetPosition(ItemsCanvas).Y; _dragStartOffset = _currentOffset; _velocity = 0; _lastY = _dragStartY; _lastTime = DateTime.Now; ItemsCanvas.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ItemsCanvas == null) return;
            if (!_isDragging) return; double y = e.GetPosition(ItemsCanvas).Y; double dt = (DateTime.Now - _lastTime).TotalMilliseconds; if (dt > 0) _velocity = (y - _lastY) / dt; _lastY = y; _lastTime = DateTime.Now; double o = _dragStartOffset + (y - _dragStartY); double min = GetOffsetForIndex(LastIndex()); if (o > 0) o *= 0.12; else if (o < min) o = min + (o - min) * 0.12; _currentOffset = o; UpdatePositions(_currentOffset);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemsCanvas == null) return;
            if (!_isDragging) return; _isDragging = false; ItemsCanvas.ReleaseMouseCapture(); SnapToNearest(_velocity);
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ItemsCanvas == null) return;
            if (_isDragging) { _isDragging = false; ItemsCanvas.ReleaseMouseCapture(); SnapToNearest(_velocity); }
        }
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e) { _inertiaTimer.Stop(); AnimateTo(GetOffsetForIndex(GetIndexFromOffset(ClampOffset(_currentOffset + (e.Delta > 0 ? -ItemHeight : ItemHeight))))); }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (WheelPickerControl)d;
            if (c.IsLoaded) c.BuildItems();
        }
        
        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (WheelPickerControl)d;
            if (c._isUpdatingFromCode || c._isDragging) return;
            
            int newIndex = (int)e.NewValue;
            if (c.ItemsSource != null && newIndex >= 0 && newIndex < c.ItemsSource.Count)
            {
                // If control is already loaded, animate to the new position
                if (c.IsLoaded)
                {
                    c._isUpdatingFromCode = true;
                    try
                    {
                        c.SelectedValue = c.ItemsSource[newIndex];
                    }
                    finally
                    {
                        c._isUpdatingFromCode = false;
                    }
                    c.AnimateTo(c.GetOffsetForIndex(newIndex));
                }
                else
                {
                    // Just set the offset directly before control is loaded
                    c._currentOffset = c.GetOffsetForIndex(newIndex);
                }
            }
        }
        
        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (WheelPickerControl)d;
            if (c._isUpdatingFromCode || c.ItemsSource == null) return;
            
            string? newValue = e.NewValue as string;
            if (!string.IsNullOrEmpty(newValue))
            {
                int index = c.ItemsSource.IndexOf(newValue);
                if (index >= 0 && index != c.SelectedIndex)
                {
                    c._isUpdatingFromCode = true;
                    try
                    {
                        c.SelectedIndex = index;
                        if (c.IsLoaded)
                        {
                            c.AnimateTo(c.GetOffsetForIndex(index));
                        }
                    }
                    finally
                    {
                        c._isUpdatingFromCode = false;
                    }
                }
            }
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo s)
        {
            base.OnRenderSizeChanged(s);
            if (IsLoaded)
            {
                BuildItems();
            }
        }
    }
}