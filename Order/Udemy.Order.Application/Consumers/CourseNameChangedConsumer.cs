using MassTransit;
using Microsoft.EntityFrameworkCore;
using Udemy.Order.Infrastructure;
using Udemy.Shared.Events;

namespace Udemy.Order.Application.Consumers
{
    /// <summary>
    /// CourseNameChanged event'ini dinler ve ilgili OrderItem'ların ProductName'ini günceller.
    /// Eventual Consistency: Catalog'da kurs adı değiştiğinde Order DB'deki sipariş kalemleri de güncellenir.
    /// </summary>
    public class CourseNameChangedConsumer : IConsumer<CourseNameChanged>
    {
        private readonly OrderDbContext _context;

        public CourseNameChangedConsumer(OrderDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<CourseNameChanged> context)
        {
            Console.WriteLine($"[CourseNameChangedConsumer] Received for CourseId: {context.Message.CourseId}, NewName: {context.Message.NewName}");

            var orderItems = await _context.OrderItems
                .Where(x => x.ProductId == context.Message.CourseId)
                .ToListAsync();

            if (!orderItems.Any())
            {
                Console.WriteLine($"[CourseNameChangedConsumer] No order items found for CourseId: {context.Message.CourseId}");
                return;
            }

            foreach (var item in orderItems)
            {
                item.ProductName = context.Message.NewName;
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"[CourseNameChangedConsumer] Updated {orderItems.Count} order items with new course name");
        }
    }
}
