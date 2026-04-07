using MediatR;
using Udemy.Order.Application.Dtos;

namespace Udemy.Order.Application.Commands
{
    /// <summary>
    /// Yeni sipariş oluşturma komutu
    /// </summary>
    public class CreateOrderCommand : IRequest<CreatedOrderDto>
    {
        public string BuyerId { get; set; } = null!;
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public AddressDto Address { get; set; } = null!;
    }
}
