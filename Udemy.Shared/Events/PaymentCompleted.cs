namespace Udemy.Shared.Events
{
    /// <summary>
    /// Ödeme başarılı olduğunda yayınlanan event.
    /// FakePayment.API tarafından publish edilir, Order.API tarafından consume edilir.
    /// </summary>
    public class PaymentCompleted
    {
        public int OrderId { get; set; }
    }
}
