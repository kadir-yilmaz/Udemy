using Udemy.WebUI.Models.FakePayments;
using Udemy.WebUI.Models.Orders;
using Udemy.WebUI.Services.Abstract;
using System.Net.Http.Json;

namespace Udemy.WebUI.Services.Concrete
{
    /// <summary>
    /// Senkron sipariş servisi.
    /// 1. Önce Payment.API'ye ödeme isteği (sync)
    /// 2. Başarılıysa Order.API'ye sipariş kaydet
    /// 3. Başarısızsa hata dön
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IBasketService _basketService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly Helpers.PhotoHelper _photoHelper;
        private readonly ICatalogService _catalogService;

        public OrderService(
            HttpClient httpClient, 
            IBasketService basketService, 
            IUserService userService,
            IPaymentService paymentService,
            Helpers.PhotoHelper photoHelper,
            ICatalogService catalogService)
        {
            _httpClient = httpClient;
            _basketService = basketService;
            _userService = userService;
            _paymentService = paymentService;
            _photoHelper = photoHelper;
            _catalogService = catalogService;
        }

        public async Task<OrderCreatedViewModel> CreateOrder(CheckoutInfoInput checkoutInfoInput)
        {
            var basket = await _basketService.Get();
            
            if (basket == null || !basket.BasketItems.Any())
            {
                return new OrderCreatedViewModel() { Error = "Sepet boş", IsSuccessful = false };
            }

            // Kullanıcı bilgilerini al
            var userId = _userService.GetUserId;
            var userEmail = _userService.GetUserEmail;
            if (string.IsNullOrEmpty(userId)) userId = "guest-user";
            if (string.IsNullOrEmpty(userEmail)) userEmail = "guest@example.com";

            // 1️⃣ ÖNCE SİPARİŞ KAYDET (Pending status) - OrderId al
            var orderCreateInput = new OrderCreateInput()
            {
                BuyerId = userId,
                Address = new AddressCreateInput 
                { 
                    Province = checkoutInfoInput.Province, 
                    District = checkoutInfoInput.District, 
                    Street = checkoutInfoInput.Street, 
                    Line = checkoutInfoInput.Line, 
                    ZipCode = checkoutInfoInput.ZipCode 
                },
            };

            basket.BasketItems.ForEach(x =>
            {
                var orderItem = new OrderItemCreateInput 
                { 
                    ProductId = x.CourseId, 
                    Price = x.GetCurrentPrice, 
                    PictureUrl = string.IsNullOrEmpty(x.PictureUrl) ? "https://via.placeholder.com/500x280?text=Kurs+Resmi" : x.PictureUrl, 
                    ProductName = x.CourseName 
                };
                orderCreateInput.OrderItems.Add(orderItem);
            });

            var orderResponse = await _httpClient.PostAsJsonAsync<OrderCreateInput>("orders", orderCreateInput);

            if (!orderResponse.IsSuccessStatusCode)
            {
                return new OrderCreatedViewModel() { Error = "Sipariş oluşturulamadı", IsSuccessful = false };
            }

            var orderCreatedViewModel = await orderResponse.Content.ReadFromJsonAsync<OrderCreatedViewModel>();
            var orderId = orderCreatedViewModel?.OrderId ?? 0;

            // 2️⃣ ŞİMDİ ÖDEME AL (OrderId ile birlikte)
            // Kart isminden ad-soyad ayırma
            var nameParts = checkoutInfoInput.CardName?.Split(' ') ?? new[] { "Ad", "Soyad" };
            var firstName = nameParts.Length > 0 ? nameParts[0] : "Ad";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "Soyad";
            
            var paymentInfo = new PaymentInfoInput
            {
                CardName = checkoutInfoInput.CardName,
                CardNumber = checkoutInfoInput.CardNumber,
                ExpireMonth = checkoutInfoInput.ExpireMonth,
                ExpireYear = "20" + checkoutInfoInput.ExpireYear,
                CVV = checkoutInfoInput.CVV,
                TotalPrice = basket.TotalPrice,
                OrderId = orderId, // Artık gerçek OrderId
                CustomerName = firstName,
                CustomerSurname = lastName,
                CustomerEmail = userEmail,
                CustomerPhone = checkoutInfoInput.Phone ?? "+905555555555",
                AddressDetail = $"{checkoutInfoInput.Street}, {checkoutInfoInput.Line}",
                City = checkoutInfoInput.Province ?? "Istanbul",
                ZipCode = checkoutInfoInput.ZipCode ?? "34000",
                Items = basket.BasketItems.Select(x => new PaymentItemInput
                {
                    ProductName = x.CourseName,
                    Price = x.GetCurrentPrice
                }).ToList()
            };

            var paymentResult = await _paymentService.ReceivePayment(paymentInfo);
            
            if (!paymentResult.IsSuccess)
            {
                return new OrderCreatedViewModel() { Error = paymentResult.ErrorMessage ?? "Odeme basarisiz", IsSuccessful = false };
            }

            // 3️⃣ ÖDEME BAŞARILI - Sepeti temizle
            if (orderCreatedViewModel != null)
            {
                orderCreatedViewModel.IsSuccessful = true;
                await _basketService.Delete();
            }

            return orderCreatedViewModel ?? new OrderCreatedViewModel { Error = "Beklenmeyen hata", IsSuccessful = false };
        }

        public async Task<List<OrderViewModel>> GetOrder()
        {
            var userId = _userService.GetUserId;
            if (string.IsNullOrEmpty(userId))
            {
                userId = "guest-user";
            }
            
            var response = await _httpClient.GetFromJsonAsync<List<OrderViewModel>>($"orders/{userId}");
            
            if (response != null)
            {
                var courses = await _catalogService.GetAllCourseAsync();

                foreach (var order in response)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var course = courses?.FirstOrDefault(x => x.Id == item.ProductId);
                        if (course != null)
                        {
                             // Use the fresh image from Catalog (which is guaranteed to exist and be formatted)
                             item.PictureUrl = course.StockPictureUrl;
                        }
                        else
                        {
                             // Fallback for courses not found in catalog (deleted courses?)
                             var url = _photoHelper.GetPhotoStockUrl(item.PictureUrl);
                             item.PictureUrl = string.IsNullOrEmpty(url) 
                                 ? "https://via.placeholder.com/500x280?text=Kurs+Yayindan+Kaldirildi" 
                                 : url;
                        }
                    }
                }
            }

            return response ?? new List<OrderViewModel>();
        }

        public async Task<OrderSuspendViewModel> SuspendOrder(CheckoutInfoInput checkoutInfoInput)
        {
            var result = await CreateOrder(checkoutInfoInput);
            
            return new OrderSuspendViewModel 
            { 
                IsSuccessful = result.IsSuccessful, 
                Error = result.Error 
            };
        }

        public async Task<List<string>> GetOwnedCourseIds()
        {
            var userId = _userService.GetUserId;
            if (string.IsNullOrEmpty(userId)) return new List<string>();

            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<string>>($"orders/{userId}/owned-courses");
                return response ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<CourseOwnershipResult> CheckCourseOwnership(string courseId)
        {
            var userId = _userService.GetUserId;
            if (string.IsNullOrEmpty(userId)) 
                return new CourseOwnershipResult { Owns = false };

            try
            {
                var response = await _httpClient.GetFromJsonAsync<CourseOwnershipResult>($"orders/{userId}/owns/{courseId}");
                return response ?? new CourseOwnershipResult { Owns = false };
            }
            catch
            {
                return new CourseOwnershipResult { Owns = false };
            }
        }
    }
}


