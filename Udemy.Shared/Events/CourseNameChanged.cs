namespace Udemy.Shared.Events
{
    /// <summary>
    /// Kurs adı değiştiğinde Catalog API tarafından publish edilir.
    /// Order Service tarafından consume edilerek OrderItem.ProductName güncellenir.
    /// </summary>
    public class CourseNameChanged
    {
        public string CourseId { get; set; } = null!;
        public string NewName { get; set; } = null!;
    }
}
