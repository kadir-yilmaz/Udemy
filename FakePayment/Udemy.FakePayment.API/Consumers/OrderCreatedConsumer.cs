using MassTransit;
using Udemy.Shared.Events;

namespace Udemy.FakePayment.API.Consumers
{
    /// <summary>
    /// OrderCreated event'ini dinler ve ödeme işlemini simüle eder.
    /// Başarılı olursa PaymentCompleted, değilse PaymentFailed publish eder.
    /// </summary>
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            Console.WriteLine($"[OrderCreatedConsumer] Received OrderCreated for OrderId: {context.Message.OrderId}");
            Console.WriteLine($"[OrderCreatedConsumer] BuyerId: {context.Message.BuyerId}, TotalPrice: {context.Message.TotalPrice}");

            // Ödeme simülasyonu - Gerçek projede burada Iyzico/Stripe vb. çağrılır
            var paymentSuccess = SimulatePayment(context.Message);

            if (paymentSuccess)
            {
                Console.WriteLine($"[OrderCreatedConsumer] Payment succeeded for OrderId: {context.Message.OrderId}");
                
                await _publishEndpoint.Publish(new PaymentCompleted
                {
                    OrderId = context.Message.OrderId
                });
                
                Console.WriteLine($"[OrderCreatedConsumer] PaymentCompleted event published");
            }
            else
            {
                Console.WriteLine($"[OrderCreatedConsumer] Payment failed for OrderId: {context.Message.OrderId}");
                
                await _publishEndpoint.Publish(new PaymentFailed
                {
                    OrderId = context.Message.OrderId,
                    Reason = "Ödeme işlemi başarısız oldu."
                });
                
                Console.WriteLine($"[OrderCreatedConsumer] PaymentFailed event published");
            }
        }

        /// <summary>
        /// Ödeme simülasyonu - Şimdilik her zaman başarılı
        /// İleride Iyzico entegrasyonu buraya gelecek
        /// </summary>
        private bool SimulatePayment(OrderCreated order)
        {
            // TODO: Iyzico entegrasyonu buraya gelecek
            // Şimdilik her zaman başarılı dönüyor
            return true;
        }
    }
}
