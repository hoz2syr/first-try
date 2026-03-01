using System.Windows.Input;

namespace StudentTracker.Models;

/// <summary>
/// فئة أمر إعادة التوجيه - تنفذ واجهة ICommand
/// </summary>
/// <remarks>
/// تُستخدم لتنفيذ الأوامر من واجهة المستخدم (XAML)
/// تدعم التحقق من إمكانية التنفيذ (CanExecute) والتمرير معاملات
/// 
/// مثال الاستخدام:
/// <code>
/// MyCommand = new RelayCommand(ExecuteMethod, CanExecuteMethod);
/// </code>
/// </remarks>
public class RelayCommand : ICommand
{
    /// <summary>
    /// الإجراء المراد تنفيذه
    /// </summary>
    private readonly Action<object?> _execute;
    
    /// <summary>
    /// دالة التحقق من إمكانية التنفيذ (اختياري)
    /// </summary>
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// مُنشئ الأمر
    /// </summary>
    /// <param name="execute">الإجراء المراد تنفيذه</param>
    /// <param name="canExecute">دالة التحقق من إمكانية التنفيذ (اختياري)</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// مُنشئ الأمر بدون معاملات
    /// </summary>
    /// <param name="execute">الإجراء المراد تنفيذه</param>
    public RelayCommand(Action execute) : this(_ => execute(), null) { }

    /// <summary>
    /// التحقق من إمكانية تنفيذ الأمر
    /// </summary>
    /// <param name="parameter">المعامل الممرر للأمر</param>
    /// <returns>صحيح إذا كان يمكن تنفيذ الأمر</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// تنفيذ الأمر
    /// </summary>
    /// <param name="parameter">المعامل الممرر للأمر</param>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// حدث تغيير إمكانية التنفيذ
    /// </summary>
    /// <remarks>
    /// يُستخدم لإشعار واجهة المستخدم بتغيير حالة الأمر
    /// </remarks>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// إشعار بتغيير حالة التنفيذ
    /// </summary>
    /// <remarks>
    /// يستدعي هذا لإجبار إعادة التحقق من CanExecute
    /// </remarks>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
