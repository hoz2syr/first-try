# تقرير مشاكل التصميم وتجربة المستخدم
## تطبيق متتبع الطالب - Student Tracker

---

## الملخص التنفيذي

تم تحليل واجهات المستخدم المختلفة في التطبيق بما في ذلك النافذة الرئيسية (MainWindow)، نوافذ الإضافة (AddNoteWindow, AddGradeWindow, EditPlanWindow)، نافذة الإحصائيات (StatisticsWindow)، ونافذة الإعداد الأولي (FirstTimeSetupWindow)، بالإضافة إلى عناصر التحكم (Controls) والأنماط (Styles.xaml).

---

## المشاكل الرئيسية

### 1. مشاكل التصميم البصري (Visual Design)

#### 1.1 عدم تناسق الألوان بين المكونات
| الموقع | المشكلة |
|--------|---------|
| FirstTimeSetupWindow.xaml | عنوان "مرحباً بك في تطبيق خطة的学生" يحتوي على نص صيني غير مقصود "的学生" يجب إزالته |
| FirstTimeSetupWindow.xaml | استخدام "#F0F4F8" للخلفية الخارجية بينما تستخدم النوافذ الأخرى "#F1F5F9" |
| EditPlanWindow.xaml | تكرار تعريف `ModernProgressBarStyle` محلياً بينما موجود مسبقاً في Styles.xaml |

#### 1.2 مشاكل الخطوط والحجم
| الموقع | المشكلة |
|--------|---------|
| MainWindow.xaml | استخدام أحجام خطوط غير متناسقة (24, 20, 18, 16, 14, 13, 12) دون نظام واضح |
| AddGradeWindow.xaml | حجم خط حقل العلامة 18px كبير جداً مقارنة بحقول أخرى |
| StatisticsWindow.xaml | عناوين الأقسام بحجم 22px بينما في MainWindow بحجم 20px |

#### 1.3 مشاكل التدرجات اللونية (Gradients)
| الموقع | المشكلة |
|--------|---------|
| EditPlanWindow.xaml | تدرج Header يختلف عن باقي النوافذ (يبدأ بـ #1E1B4B بدلاً من #6366F1) |
| Multiple Windows | عدم وجود معايير موحدة لاختيار التدرجات |

---

### 2. مشاكل تجربة المستخدم (UX)

#### 2.1 مشاكل التفاعل
| المشكلة | التأثير |
|---------|--------|
| DateWheelPicker ارتفاع 200px في AddNoteWindow | مساحة كبيرة جداً غير مستغلة بشكل جيد |
| AddGradeWindow عرض 500px | ضيق جداً ويجعل DateWheelPicker غير مريح |
| عدم وجودvalidation مرئي لحقول الدرجات | المستخدم قد يدخل قيم غير صالحة |

#### 2.2 مشاكل التنقل
| الموقع | المشكلة |
|--------|---------|
| MainWindow.xaml | TabControl يحتوي على 5 تبويبات معقدة التحميل |
| EditPlanWindow.xaml | استخدام WrapPanel可能导致 ترتيب غير متوقع للبطاقات |

#### 2.3 مشاكل不回、退 (Feedback)
| الموقع | المشكلة |
|--------|---------|
| AddGradeWindow | لا توجد رسالة نجاح بعد الحفظ |
| EditPlanWindow | رسالة الحالة تختفي بسرعة دون إتاحة الوقت للقراءة |
| AddNoteWindow | عداد الأحرف (0/2000) غير محدث تلقائياً |

---

### 3. مشاكل إمكانية الوصول (Accessibility)

#### 3.1 مشاكل التباين
| العنصر | المشكلة |
|--------|---------|
| StatusToColorConverter | الألوان المتوقعة للألوان (#F59E0B اصفر) قد لا تكون واضحة بدرجة كافية |
| Calendar days in other months | النص الرمادي الفاتح (#B0BEC5) قد يكون صعب القراءة |

#### 3.2 مشاكل لوحة المفاتيح
| المشكلة |
|--------|
| لا يوجد تركيز تلقائي (Auto-focus) على الحقول الأولى في معظم النوافذ |
| ترتيب التنقل (TabIndex) غير موجود أو غير متسق |

---

### 4. مشاكل التصميم المتجاوب

#### 4.1 مشاكل الحجم الثابت
| النافذة | المشكلة |
|---------|---------|
| AddGradeWindow | Height="480" Width="500" - لا تستجيب لحجم المحتوى |
| StatisticsWindow | Width="900" قد يكون صغيراً على شاشات كبيرة |
| EditPlanWindow | Width="1000" MinWidth="800" - البطاقات بعرض 450px ثابت |

#### 4.2 مشاكل ScrollViewer
| الموقع | المشكلة |
|--------|---------|
| MainWindow | VerticalScrollBarVisibility="Auto" على كل TabItem |
| EditPlanWindow | ScrollViewer يحيط بـ StackPanel فقط |

---

### 5. مشاكل الكود والأنماط (Code & Styles)

#### 5.1 تكرار الأنماط
```
- ModernProgressBarStyle معرف في Styles.xaml وEditPlanWindow.xaml وStatisticsWindow.xaml
- RadioButtonStyle موجود في AddGradeWindow و Styles.xaml
- CardBorderStyle موجود في Styles.xaml
```

#### 5.2 مشاكل Resource Dictionary
| المشكلة |
|--------|
| Styles.xaml يستخدم ألوان من معرفات Color بينما Brushes معرّفة بأسماء مختلفة |
| بعض الأنماط تستدعي StaticResource وأخرى DynamicResource |

#### 5.3 مشاكل Binding
```
- AddGradeWindow: النافذة تستخدم Code-bhind بدلاً من ViewModel
- AddNoteWindow: CharacterCountText غير مربوط بـ TextBox
- StatisticsWindow: ItemsControl بدون ItemsSource محدد في XAML
```

---

### 6. مشاكل الأداء

| المشكلة | التأثير |
|---------|--------|
| DateWheelPickerContains 3 WheelPickerControls | تحميل بطيء محتمل |
| عدم استخدام Virtualization | ListItems قد يؤثر على الأداء مع كثير من المواد |
| MultiDataTrigger في EditPlanWindow | قد يؤثر على سرعة التحديث |

---

## توصيات التحسين

### الأولوية القصوى (P0)
1. إصلاح نص "的学生" في FirstTimeSetupWindow
2. توحيد نظام الألوان والخطوط
3. إضافة التحقق من صحة المدخلات (Validation)

### الأولوية العالية (P1)
4. تحسين تجربة DateWheelPicker
5. إضافة رسائل Feedback للمستخدم
6. توحيد استخدام أنماط الموارد

### الأولوية المتوسطة (P2)
7. تحسين إمكانية الوصول
8. تحسين الأداء
9. إضافة دعم Languages RTL بشكل أفضل

---

*تاريخ التقرير: 2024*
*الإصدار: 1.0*
