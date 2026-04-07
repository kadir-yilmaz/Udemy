using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Udemy.Order.Application.Commands;
using Udemy.Order.Application.Dtos;
using Udemy.Order.Application.Mapping;
using Udemy.Order.Domain.Entities;
using Udemy.Order.Domain.Enums;
using Udemy.Order.Infrastructure;

namespace Udemy.Order.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;

        public OrdersController(OrderDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kullanıcıya ait siparişleri getirir
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetOrders(string userId)
        {
            var orders = await _context.Orders
                .Include(x => x.OrderItems)
                .Include(x => x.Address)
                .Where(x => x.BuyerId == userId)
                .ToListAsync();

            var orderDtos = ObjectMapper.Mapper.Map<List<OrderDto>>(orders);
            return Ok(orderDtos);
        }

        /// <summary>
        /// Kullanıcının satın aldığı tüm kurs ID'lerini döner
        /// </summary>
        [HttpGet("{userId}/owned-courses")]
        public async Task<IActionResult> GetOwnedCourseIds(string userId)
        {
            var ownedCourseIds = await _context.Orders
                .Where(x => x.BuyerId == userId && x.Status == OrderStatus.Completed)
                .SelectMany(x => x.OrderItems)
                .Select(x => x.ProductId)
                .Distinct()
                .ToListAsync();

            return Ok(ownedCourseIds);
        }

        /// <summary>
        /// Kullanıcının belirli bir kursa sahip olup olmadığını kontrol eder
        /// </summary>
        [HttpGet("{userId}/owns/{courseId}")]
        public async Task<IActionResult> CheckCourseOwnership(string userId, string courseId)
        {
            var owns = await _context.Orders
                .Where(x => x.BuyerId == userId && x.Status == OrderStatus.Completed)
                .SelectMany(x => x.OrderItems)
                .AnyAsync(x => x.ProductId == courseId);

            if (owns)
            {
                // Satın alma tarihini de bul
                var purchaseDate = await _context.Orders
                    .Where(x => x.BuyerId == userId && x.Status == OrderStatus.Completed)
                    .Where(x => x.OrderItems.Any(oi => oi.ProductId == courseId))
                    .Select(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

                return Ok(new { Owns = true, PurchaseDate = purchaseDate });
            }

            return Ok(new { Owns = false, PurchaseDate = (DateTime?)null });
        }

        /// <summary>
        /// Yeni sipariş oluşturur (Ödeme zaten alındı, Status = Completed)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderCommand command)
        {
            Console.WriteLine($"[OrdersController] POST CreateOrder called. BuyerId: {command.BuyerId}");

            var newAddress = new Address
            {
                Province = command.Address.Province,
                District = command.Address.District,
                Street = command.Address.Street,
                ZipCode = command.Address.ZipCode,
                Line = command.Address.Line
            };

            var newOrder = new Udemy.Order.Domain.Entities.Order
            {
                BuyerId = command.BuyerId,
                CreatedDate = DateTime.Now,
                Status = OrderStatus.Pending, // Ödeme bekleniyor - RabbitMQ event ile güncellenecek
                Address = newAddress
            };

            foreach (var item in command.OrderItems)
            {
                newOrder.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    PictureUrl = item.PictureUrl,
                    Price = item.Price
                });
            }

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[OrdersController] ✅ Order saved with Id: {newOrder.Id}, Status: Pending (awaiting payment)");

            return Ok(new CreatedOrderDto { OrderId = newOrder.Id });
        }
    }
}



