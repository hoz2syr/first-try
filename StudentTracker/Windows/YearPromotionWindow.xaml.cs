using System.Windows;

namespace StudentTracker.Windows;

public partial class YearPromotionWindow : Window
{
    public int NewYear { get; private set; }

    public YearPromotionWindow(int newYear)
    {
        InitializeComponent();
        NewYear = newYear;
        YearTextBlock.Text = $"🏆 مرحباً بك في السنة {newYear}";
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
