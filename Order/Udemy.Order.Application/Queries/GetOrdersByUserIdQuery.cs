using MediatR;
using Udemy.Order.Application.Dtos;

namespace Udemy.Order.Application.Queries
{
    /// <summary>
    /// Kullanıcıya ait siparişleri getirme sorgusu
    /// </summary>
    public class GetOrdersByUserIdQuery : IRequest<List<OrderDto>>
    {
        public string UserId { get; set; } = null!;
    }
}
