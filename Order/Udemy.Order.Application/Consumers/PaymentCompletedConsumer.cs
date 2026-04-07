using MassTransit;
using Microsoft.EntityFrameworkCore;
using Udemy.Order.Domain.Enums;
using Udemy.Order.Infrastructure;
using Udemy.Shared.Events;

namespace Udemy.Order.Application.Consumers
{
    /// <summary>
    /// PaymentCompleted event'ini dinler ve sipariş durumunu Completed olarak günceller.
    /// </summary>
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
    {
        private readonly OrderDbContext _context;

        public PaymentCompletedConsumer(OrderDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            // 🔴 BREAKPOINT: Event-driven akışı test etmek için buraya breakpoint koy
            // Order.API debug modda çalışırken, ödeme yapılınca burada duracak
            Console.WriteLine($"[PaymentCompletedConsumer] Received PaymentCompleted for OrderId: {context.Message.OrderId}");

            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == context.Message.OrderId);
            
            if (order == null)
            {
                Console.WriteLine($"[PaymentCompletedConsumer] Order not found: {context.Message.OrderId}");
                return;
            }

            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[PaymentCompletedConsumer] Order {order.Id} status updated to Completed");
        }
    }
}
