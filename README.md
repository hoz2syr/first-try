# 📚 StudentTracker — متابعة المسيرة الأكاديمية

تطبيق سطح مكتب احترافي مبني بتقنية **WPF (.NET 8)** لمتابعة التقدم الأكاديمي للطلاب الجامعيين.
يدعم اللغة العربية بالكامل مع واجهة مستخدم حديثة وسلسة.

---

## ✨ المميزات الرئيسية

| الميزة | الوصف |
|--------|-------|
| **لوحة التحكم** | إحصائيات فورية للسنة الحالية والأداء العام |
| **إدارة الدرجات** | إدخال درجات عملي/نظري مع سجل كامل للمحاولات |
| **جدول الامتحانات** | تسجيل مواعيد الامتحانات مع كشف التعارضات |
| **التقويم** | عرض شهري لأحداث الامتحانات |
| **الملاحظات** | تدوين الملاحظات مع حفظ تلقائي للمسودات |
| **الخطة الدراسية** | تحرير المخطط العام (السنوات/الفصول/المواد) |
| **الترقي التلقائي** | حساب السنة الحالية بناءً على قاعدة (المواد − 4) |
| **احتفالات** | نوافذ تهنئة عند الترقي أو التخرج |

---

## 🏗️ البنية التقنية

```
StudentTracker/
├── Models/              # نماذج البيانات و ViewModels
│   ├── Subject.cs              — المادة الدراسية
│   ├── SubjectViewModel.cs     — نموذج عرض المادة (MVVM)
│   ├── CalendarViewModel.cs    — نماذج التقويم والملاحظات
│   ├── RelayCommand.cs         — تنفيذ ICommand
│   └── ...
├── Services/            # طبقة البيانات والمنطق
│   ├── DatabaseService.cs      — عمليات SQLite (CRUD)
│   ├── IDatabaseService.cs     — الواجهة التجريدية
│   ├── GradeCalculator.cs      — حساب الدرجات والحالة
│   ├── YearCalculator.cs       — حساب السنة الحالية
│   ├── StatisticsService.cs    — إحصائيات الأداء
│   └── Logger.cs               — تسجيل الأحداث
├── Windows/             # النوافذ الفرعية
│   ├── AddGradeWindow          — إدخال العلامات
│   ├── AddExamWindow           — مواعيد الامتحانات
│   ├── AddNoteWindow           — الملاحظات
│   ├── StatisticsWindow        — الإحصائيات المفصلة
│   ├── EditPlanWindow          — تحرير الخطة
│   ├── CustomMessageBox        — رسائل مخصصة
│   └── ...
├── Controls/            # عناصر تحكم مخصصة
│   └── WheelPickerControl      — منتقي عجلة (iOS-style)
├── Converters/          # محولات القيم
│   ├── StatusToColorConverter  — حالة → لون
│   ├── CalendarConverters      — محولات التقويم
│   └── InverseBooleanConverter — عكس منطقي
├── Resources/           # الموارد المشتركة
│   ├── Styles.xaml             — نظام التصميم المركزي
│   └── app.ico                 — أيقونة التطبيق
├── MainWindow.xaml/cs   # النافذة الرئيسية
├── App.xaml/cs          # نقطة الدخول والتهيئة
└── StudentTracker.csproj
```

---

## 🛠️ المتطلبات

- **.NET 8.0 SDK** (Windows Desktop)
- **Windows 10/11**

### الحزم المستخدمة

| الحزمة | الإصدار | الغرض |
|--------|---------|-------|
| `Microsoft.Data.Sqlite` | 10.0.3 | قاعدة بيانات SQLite |
| `MahApps.Metro.IconPacks.Material` | 6.2.1 | أيقونات Material Design |
| `LottieSharp` | 2.4.3 | رسوم متحركة Lottie |

---

## 🚀 التشغيل

```bash
# استنساخ المستودع
git clone <repo-url>
cd alpha

# البناء والتشغيل
dotnet build
dotnet run --project StudentTracker
```

---

## 🎨 نظام التصميم

جميع رموز التصميم (الألوان، الأنماط، الظلال) مُعرّفة مركزياً في [`Resources/Styles.xaml`](StudentTracker/Resources/Styles.xaml):

| الرمز | القيمة | الاستخدام |
|-------|--------|-----------|
| `PrimaryColor` | `#6366F1` | اللون الرئيسي (Indigo) |
| `SecondaryColor` | `#10B981` | النجاح/الإيجابي (Emerald) |
| `AccentColor` | `#F59E0B` | التحذير/المعلّق (Amber) |
| `DangerColor` | `#EF4444` | الخطأ/الرسوب (Red) |
| `BackgroundColor` | `#F8FAFC` | خلفية التطبيق |
| `SurfaceColor` | `#FFFFFF` | خلفية البطاقات |
| `TextPrimaryColor` | `#1E293B` | النص الرئيسي |
| `TextSecondaryColor` | `#64748B` | النص الثانوي |

---

## 🧩 البنية البرمجية

يتبع التطبيق نمط **MVVM-Lite** مع الفصل بين الطبقات:

```
┌──────────────────────────────────────────┐
│            XAML Views (Windows/)          │  طبقة العرض
├──────────────────────────────────────────┤
│   ViewModels (SubjectViewModel, etc.)    │  نماذج العرض + RelayCommand
├──────────────────────────────────────────┤
│   Services (IDatabaseService ↔ SQLite)   │  منطق الأعمال + الوصول للبيانات
├──────────────────────────────────────────┤
│            Models (POCOs)                │  نماذج البيانات البسيطة
└──────────────────────────────────────────┘
```

### المبادئ المتبعة
- **واجهة `IDatabaseService`** لتجريد قاعدة البيانات (قابلة للاختبار)
- **`HashSet<int>`** لتحسين أداء بحث إعادة العملي
- **`BuildSubjectViewModel()`** لتوحيد بناء نماذج العرض
- **`CustomMessageBox`** بدلاً من `MessageBox` لتوحيد تجربة المستخدم
- **أنماط مركزية** في `Styles.xaml` لكل عناصر التحكم
- **دعم RTL** عبر `FlowDirection="RightToLeft"` في جميع النوافذ

---

## 📄 الترخيص

هذا المشروع للاستخدام الأكاديمي والشخصي.
