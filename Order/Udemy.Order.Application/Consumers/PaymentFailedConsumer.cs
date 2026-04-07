using MassTransit;
using Microsoft.EntityFrameworkCore;
using Udemy.Order.Domain.Enums;
using Udemy.Order.Infrastructure;
using Udemy.Shared.Events;

namespace Udemy.Order.Application.Consumers
{
    /// <summary>
    /// PaymentFailed event'ini dinler ve sipariş durumunu Failed olarak günceller.
    /// </summary>
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly OrderDbContext _context;

        public PaymentFailedConsumer(OrderDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            Console.WriteLine($"[PaymentFailedConsumer] Received PaymentFailed for OrderId: {context.Message.OrderId}, Reason: {context.Message.Reason}");

            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == context.Message.OrderId);
            
            if (order == null)
            {
                Console.WriteLine($"[PaymentFailedConsumer] Order not found: {context.Message.OrderId}");
                return;
            }

            order.Status = OrderStatus.Failed;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[PaymentFailedConsumer] Order {order.Id} status updated to Failed");
        }
    }
}
