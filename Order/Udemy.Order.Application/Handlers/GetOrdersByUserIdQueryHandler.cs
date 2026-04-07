using MediatR;
using Microsoft.EntityFrameworkCore;
using Udemy.Order.Application.Dtos;
using Udemy.Order.Application.Mapping;
using Udemy.Order.Application.Queries;
using Udemy.Order.Infrastructure;

namespace Udemy.Order.Application.Handlers
{
    public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, List<OrderDto>>
    {
        private readonly OrderDbContext _context;

        public GetOrdersByUserIdQueryHandler(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .Include(x => x.OrderItems)
                .Include(x => x.Address)
                .Where(x => x.BuyerId == request.UserId)
                .ToListAsync(cancellationToken);

            if (!orders.Any())
            {
                return new List<OrderDto>();
            }

            return ObjectMapper.Mapper.Map<List<OrderDto>>(orders);
        }
    }
}
