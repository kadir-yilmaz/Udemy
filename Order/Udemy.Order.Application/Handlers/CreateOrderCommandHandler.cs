using MediatR;
using Udemy.Order.Application.Commands;
using Udemy.Order.Application.Dtos;
using Udemy.Order.Domain.Entities;
using Udemy.Order.Infrastructure;

namespace Udemy.Order.Application.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreatedOrderDto>
    {
        private readonly OrderDbContext _context;

        public CreateOrderCommandHandler(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<CreatedOrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var newAddress = new Address
            {
                Province = request.Address.Province,
                District = request.Address.District,
                Street = request.Address.Street,
                ZipCode = request.Address.ZipCode,
                Line = request.Address.Line
            };

            var newOrder = new Domain.Entities.Order
            {
                BuyerId = request.BuyerId,
                CreatedDate = DateTime.Now,
                Address = newAddress
            };

            foreach (var item in request.OrderItems)
            {
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    PictureUrl = item.PictureUrl,
                    Price = item.Price
                });
            }

            await _context.Orders.AddAsync(newOrder, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreatedOrderDto { OrderId = newOrder.Id };
        }
    }
}
