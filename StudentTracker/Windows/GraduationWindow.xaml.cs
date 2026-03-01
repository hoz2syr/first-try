using System.Windows;

namespace StudentTracker.Windows;

public partial class GraduationWindow : Window
{
    public GraduationWindow(int passedCount, int totalCount)
    {
        InitializeComponent();
        StatsTextBlock.Text = $"✅ {passedCount} مادة مكتملة من أصل {totalCount}";
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
