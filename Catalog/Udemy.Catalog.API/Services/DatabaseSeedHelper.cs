using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Udemy.Catalog.API.Models;
using Udemy.Catalog.API.Options;

namespace Udemy.Catalog.API.Services
{
    public static class DatabaseSeedHelper
    {
        public static async Task SeedCategoriesAsync(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DatabaseName);
            var categoryCollection = database.GetCollection<Category>(options.CategoryCollectionName);

            // Udemy kategorileri (BT ve Yazılım hariç) - Yazılım eklendi
            var categories = new List<string>
            {
                "Yazılım",
                "İşletme",
                "Finans ve Muhasebe",
                "Ofiste Verimlilik",
                "Kişisel Gelişim",
                "Tasarım",
                "Pazarlama",
                "Sağlık ve Fitness",
                "Müzik"
            };

            var newCategories = new List<Category>();

            foreach (var categoryName in categories)
            {
                var exist = await categoryCollection.Find(x => x.Name == categoryName).AnyAsync();
                if (!exist)
                {
                    newCategories.Add(new Category { Name = categoryName });
                }
            }

            if (newCategories.Any())
            {
                await categoryCollection.InsertManyAsync(newCategories);
                Console.WriteLine($"[DatabaseSeedHelper] ✅ Seeded {newCategories.Count} new categories.");
            }
            else
            {
                Console.WriteLine($"[DatabaseSeedHelper] All categories already exist. Skipping seed.");
            }
        }
    }
}
