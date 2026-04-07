namespace Udemy.Order.Domain.Enums
{
    /// <summary>
    /// Sipariş durumu
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Sipariş oluşturuldu, ödeme bekleniyor
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Ödeme tamamlandı, sipariş onaylandı
        /// </summary>
        Completed = 1,
        
        /// <summary>
        /// Ödeme başarısız, sipariş iptal edildi
        /// </summary>
        Failed = 2
    }
}
