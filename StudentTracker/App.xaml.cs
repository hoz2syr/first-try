using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using StudentTracker.Services;
using StudentTracker.Windows;

namespace StudentTracker;

/// <summary>
/// الفئة الرئيسية لتطبيق StudentTracker - مدير تتبع تقدم الطلاب
/// هذا الملف يحتوي على نقطة الدخول الرئيسية للتطبيق وإعداد الثقافة واللغة
/// </summary>
/// <remarks>
/// وظائف الفئة:
/// - إعداد ثقافة التطبيق للغة العربية (السعودية)
/// - تهيئة قاعدة البيانات
/// - التحقق من وجود خطة دراسية
/// - عرض نافذة الإعداد الأولية عند الحاجة
/// - عرض النافذة الرئيسية للتطبيق
/// </remarks>
public partial class App : Application
{
	/// <summary>
	/// دالة التهيئة الرئيسية للتطبيق
	/// تُستدعى عند بدء تشغيل التطبيق
	/// </summary>
	/// <param name="e">معاملات بدء التشغيل</param>
	/// <remarks>
	/// الترتيب:
	/// 1. إعداد ثقافة التطبيق (اللغة العربية - السعودية)
	/// 2. تهيئة قاعدة البيانات
	/// 3. التحقق من وجود خطة دراسية
	/// 4. عرض النافذة المناسبة (إعداد أولي أو رئيسية)
	/// </remarks>
	protected override void OnStartup(StartupEventArgs e)
	{
		// تسجيل بدء التشغيل
		Logger.LogInfo("بدء تشغيل التطبيق...");
		
		// تنظيف السجلات القديمة
		Logger.CleanupOldLogs();
		
		// إنشاء كائن الثقافة للغة العربية (السعودية)
		// تُستخدم لتنسيق التواريخ والأرقام باللغة العربية
		var culture = new CultureInfo("ar-SA");
		
		// تعيين الثقافة الحالية للمؤشر والترجمة
		// يؤثر على تنسيق التاريخ والوقت والأرقام في جميع أنحاء التطبيق
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		// تجاوز خاصية اللغة لـ WPF لضمان استخدام العربية
		// ضرورية لدعم النصوص العربية بشكل صحيح في واجهة المستخدم
		FrameworkElement.LanguageProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

		// استدعاء دالة التهيئة الأساسية
		base.OnStartup(e);

		// إنشاء كائن خدمة قاعدة البيانات
		// المسؤول عن جميع عمليات قراءة وكتابة البيانات
		var db = new DatabaseService();
		
		// تهيئة قاعدة البيانات وإنشاء الجداول إذا لم تكن موجودة
		db.InitializeDatabase();

	// تعيين وضع إيقاف التشغيل ليكون يدوياً
		// التطبيق لا يتم إغلاقه تلقائياً عند إغلاق النافذة الرئيسية
		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		// التحقق من وجود خطة دراسية
		// إذا لم تكن موجودة، نعرض نافذة الإعداد الأولي
		if (!db.HasStudyPlan())
		{
			// إنشاء نافذة الإعداد الأولي
			var setup = new FirstTimeSetupWindow(db);
			
			// عرض النافذة كـ dialogue وانتظار نتيجة المستخدم
			var setupResult = setup.ShowDialog();
			
			// إذا رفض المستخدم الإعداد، نقوم بإغلاق التطبيق
			if (setupResult != true)
			{
				Logger.LogInfo("المستخدم رفض الإعداد الأولي، إغلاق التطبيق");
				Shutdown();
				return;
			}
		}

		// إنشاء وعرض النافذة الرئيسية للتطبيق
		var mainWindow = new MainWindow(db);
		MainWindow = mainWindow;
		
		// تغيير وضع الإيقاف ليتم إغلاق التطبيق عند إغلاق النافذة الرئيسية
		ShutdownMode = ShutdownMode.OnMainWindowClose;
		
		// عرض النافذة الرئيسية
		mainWindow.Show();
		
		Logger.LogInfo("تم تشغيل التطبيق بنجاح");
	}
}

