using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentTracker.Models
{
    /// <summary>
    /// نموذج الملاحظة - يمثل ملاحظة واحدة في النظام
    /// </summary>
    /// <remarks>
    /// تُستخدم لتخزين ملاحظات المستخدم الشخصية
    /// كل ملاحظة تحتوي على عنوان ومحتوى وتاريخ الإنشاء والتعديل
    /// </remarks>
    public class Note
    {
        /// <summary>
        /// معرف الملاحظة الفريد
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// عنوان الملاحظة
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// محتوى الملاحظة النصي
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// تاريخ إنشاء الملاحظة
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// تاريخ آخر تعديل للملاحظة
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
