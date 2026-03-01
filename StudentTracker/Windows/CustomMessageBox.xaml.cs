using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace StudentTracker.Windows;

public partial class CustomMessageBox : Window
{
    public bool ResultYes { get; private set; }

    private CustomMessageBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Show an information message (OK button only).
    /// </summary>
    public static void ShowInfo(string message, string title = "معلومات", Window? owner = null)
    {
        var box = Create(message, title, "#6366F1", PackIconMaterialKind.Information, owner);
        box.AddButton("حسناً", "#6366F1", true);
        box.ShowDialog();
    }

    /// <summary>
    /// Show a success message (OK button only, green header).
    /// </summary>
    public static void ShowSuccess(string message, string title = "تم بنجاح", Window? owner = null)
    {
        var box = Create(message, title, "#10B981", PackIconMaterialKind.CheckCircle, owner);
        box.AddButton("ممتاز", "#10B981", true);
        box.ShowDialog();
    }

    /// <summary>
    /// Show a warning message (OK button only).
    /// </summary>
    public static void ShowWarning(string message, string title = "تحذير", Window? owner = null)
    {
        var box = Create(message, title, "#F59E0B", PackIconMaterialKind.AlertCircle, owner);
        box.AddButton("فهمت", "#F59E0B", true);
        box.ShowDialog();
    }

    /// <summary>
    /// Show an error message (OK button only).
    /// </summary>
    public static void ShowError(string message, string title = "خطأ", Window? owner = null)
    {
        var box = Create(message, title, "#EF4444", PackIconMaterialKind.CloseCircle, owner);
        box.AddButton("حسناً", "#EF4444", true);
        box.ShowDialog();
    }

    /// <summary>
    /// Show a Yes/No confirmation. Returns true if user clicked Yes.
    /// </summary>
    public static bool ShowConfirm(string message, string title = "تأكيد العملية", Window? owner = null)
    {
        var box = Create(message, title, "#F59E0B", PackIconMaterialKind.HelpCircle, owner);
        box.AddButton("نعم", "#10B981", true);
        box.AddButton("لا", "#6B7280", false);
        box.ShowDialog();
        return box.ResultYes;
    }

    /// <summary>
    /// Show a delete confirmation. Returns true if user clicked Delete.
    /// </summary>
    public static bool ShowDeleteConfirm(string message, string title = "تأكيد الحذف", Window? owner = null)
    {
        var box = Create(message, title, "#EF4444", PackIconMaterialKind.DeleteAlert, owner);
        box.AddButton("حذف", "#EF4444", true);
        box.AddButton("إلغاء", "#6B7280", false);
        box.ShowDialog();
        return box.ResultYes;
    }

    private static CustomMessageBox Create(string message, string title, string headerColor, PackIconMaterialKind iconKind, Window? owner)
    {
        var box = new CustomMessageBox();
        box.MessageText.Text = message;
        box.TitleText.Text = title;

        // تعيين لون الخلفية
        var brush = new BrushConverter().ConvertFromString(headerColor) as Brush ?? Brushes.DodgerBlue;
        box.HeaderBar.Background = brush;

        // تعيين الأيقونة
        box.MessageIcon.Kind = iconKind;
        box.MessageIcon.Foreground = brush;

        if (owner != null)
        {
            box.Owner = owner;
        }

        return box;
    }

    private void AddButton(string text, string bgColor, bool isYes)
    {
        var btn = new Button
        {
            Content = text,
            Style = FindResource("ModernButtonStyle") as Style,
            Background = new BrushConverter().ConvertFromString(bgColor) as Brush
        };

        btn.Click += (_, _) =>
        {
            ResultYes = isYes;
            Close();
        };

        ButtonsPanel.Children.Add(btn);
    }
}
