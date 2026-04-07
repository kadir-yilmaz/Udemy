using MassTransit;
using Udemy.Order.Application.Messages;
using Udemy.Order.Infrastructure;

namespace Udemy.Order.Application.Consumers
{
    public class CreateOrderMessageCommandConsumer : IConsumer<CreateOrderMessageCommand>
    {
        private readonly OrderDbContext _orderDbContext;

        public CreateOrderMessageCommandConsumer(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }

        public async Task Consume(ConsumeContext<CreateOrderMessageCommand> context)
        {
            Console.WriteLine($"[CreateOrderMessageCommandConsumer] Message received. BuyerId: {context.Message.BuyerId}");

            // Yeni adres oluştur
            var newAddress = new Domain.Entities.Address
            {
                Province = context.Message.Province,
                District = context.Message.District,
                Street = context.Message.Street,
                ZipCode = context.Message.ZipCode,
                Line = context.Message.Line
            };

            // Yeni sipariş oluştur
            var order = new Domain.Entities.Order
            {
                BuyerId = context.Message.BuyerId,
                CreatedDate = DateTime.Now,
                Address = newAddress
            };

            // Sipariş kalemlerini ekle
            foreach (var item in context.Message.OrderItems)
            {
                Console.WriteLine($"[CreateOrderMessageCommandConsumer] Adding item: {item.ProductName}, Price: {item.Price}");
                order.OrderItems.Add(new Domain.Entities.OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    PictureUrl = item.PictureUrl,
                    Price = item.Price
                });
            }

            // Veritabanına kaydet
            await _orderDbContext.Orders.AddAsync(order);
            await _orderDbContext.SaveChangesAsync();
            
            Console.WriteLine($"[CreateOrderMessageCommandConsumer] Order saved successfully. OrderId: {order.Id}");
        }
    }
}
