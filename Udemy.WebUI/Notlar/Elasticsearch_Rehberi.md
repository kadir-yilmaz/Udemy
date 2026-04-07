# Elasticsearch Kullanım Rehberi ve Catalog Servisi Entegrasyonu

Bu doküman, projemizde arama ve filtreleme işlemleri için **Elasticsearch**'ün neden ve nasıl kullanılacağını adım adım anlatır.

---

## 🎯 Önce-Sonra Karşılaştırması (Büyük Resim)

### 📍 ŞU ANKİ DURUM (Sadece MongoDB)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              ŞU ANKİ AKIŞ                                    │
└─────────────────────────────────────────────────────────────────────────────┘

  [MVC / Mobil App]
         │
         │  HTTP Request: "C# kurslarını getir"
         ▼
  ┌─────────────────┐
  │ CoursesController│
  │ (Catalog.API)    │
  └────────┬────────┘
           │
           │  _courseService.GetAllAsync()
           ▼
  ┌─────────────────┐
  │  CourseService  │  ◀── MongoDB Driver ile sorgu
  └────────┬────────┘
           │
           │  _courseCollection.Find(_ => true).ToListAsync()
           │
           │  ⚠️ Sonra her kurs için ayrı Category sorgusu (N+1 Problemi!)
           │  foreach(course) { category = _categoryCollection.Find(...) }
           ▼
  ┌─────────────────┐
  │    MongoDB      │
  │  localhost:27017│
  └─────────────────┘
           │
           │  Courses Collection + Categories Collection
           ▼
  ┌─────────────────┐
  │   JSON Response │
  └─────────────────┘
           │
           ▼
  [MVC View / Mobil App]
```

**Kod Akışı (Şu An):**

```csharp
// 1. Controller
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var courses = await _courseService.GetAllAsync();  // MongoDB'ye gidiyor
    return Ok(courses);
}

// 2. Service (MongoDB ile)
public async Task<List<CourseDto>> GetAllAsync()
{
    // MongoDB'den tüm kursları çek
    var courses = await _courseCollection.Find(_ => true).ToListAsync();

    // ⚠️ N+1 Problemi! Her kurs için ayrı sorgu
    foreach (var course in courses)
    {
        course.Category = await _categoryCollection
            .Find<Category>(x => x.Id == course.CategoryId)
            .FirstOrDefaultAsync();
    }
    
    return _mapper.Map<List<CourseDto>>(courses);
}
```

---

### 📍 ELASTICSEARCH İLE (Docker'da Çalışıyor)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ELASTICSEARCH İLE AKIŞ                               │
└─────────────────────────────────────────────────────────────────────────────┘

  [MVC / Mobil App]
         │
         │  HTTP Request: "C# kurslarını getir"
         ▼
  ┌─────────────────┐
  │ SearchController │  ◀── YENİ! Sadece arama için
  │ (Catalog.API)    │
  └────────┬────────┘
           │
           │  _searchService.SearchAsync(query: "C#")
           ▼
  ┌─────────────────────┐
  │ CourseSearchService │  ◀── YENİ! NEST ile Elasticsearch sorgusu
  └────────┬────────────┘
           │
           │  _elasticClient.SearchAsync<CourseDocument>(...)
           │
           │  ✅ Full-text search, tek sorgu, milisaniye hızında!
           ▼
  ┌─────────────────┐
  │  Elasticsearch  │  ◀── Docker Container
  │  localhost:9200 │
  └─────────────────┘
           │
           │  courses Index (denormalize veri)
           ▼
  ┌─────────────────┐
  │   JSON Response │
  └─────────────────┘
           │
           ▼
  [MVC View / Mobil App]


  ════════════════════════════════════════════════════════════════════════════
  PEKI MONGODB'YE NE OLDU? HÂlâ DURUYOR! (YAZMA İŞLEMLERİ İÇİN)
  ════════════════════════════════════════════════════════════════════════════

  [Admin Panel - Kurs Ekleme]
         │
         │  POST: Yeni kurs oluştur
         ▼
  ┌─────────────────┐
  │ CoursesController│  ◀── ESKİ Controller (değişmedi)
  └────────┬────────┘
           │
           │  _courseService.CreateAsync(dto)
           ▼
  ┌─────────────────┐
  │  CourseService  │  ◀── ESKİ Service (değişmedi)
  └────────┬────────┘
           │
           ├──────────────────────────────────────┐
           │  1. MongoDB'ye kaydet               │
           ▼                                      │
  ┌─────────────────┐                            │
  │    MongoDB      │                            │
  │  localhost:27017│                            │
  └─────────────────┘                            │
                                                 │
           │  2. Event fırlat (CourseCreated)    │
           ▼                                      │
  ┌─────────────────┐                            │
  │   RabbitMQ      │  ◀── Zaten projede var!   │
  │  (MassTransit)  │                            │
  └────────┬────────┘                            │
           │                                      │
           │  3. Consumer event'i dinler         │
           ▼                                      │
  ┌─────────────────────┐                        │
  │CourseCreatedConsumer│  ◀── YENİ!            │
  └────────┬────────────┘                        │
           │                                      │
           │  4. Elasticsearch'e index'le        │
           ▼                                      │
  ┌─────────────────┐                            │
  │  Elasticsearch  │  ◀── Veri senkronize!     │
  │  localhost:9200 │                            │
  └─────────────────┘◀────────────────────────────┘
```

---

### 🔄 İKİ DURUMUN KARŞILAŞTIRMASI

| Özellik | Şu An (MongoDB) | Elasticsearch İle |
|---------|-----------------|-------------------|
| **Arama Sorgusu** | `_courseService.GetAllAsync()` | `_searchService.SearchAsync(...)` |
| **Veritabanı** | MongoDB (tek) | MongoDB (yazma) + Elasticsearch (okuma) |
| **Controller** | `CoursesController` | `CoursesController` + `SearchController` |
| **Service** | `CourseService` | `CourseService` + `CourseSearchService` |
| **NuGet Paketi** | `MongoDB.Driver` | `MongoDB.Driver` + `NEST` |
| **LINQ Kullanımı** | Evet (MongoDB LINQ) | Hayır! (NEST Fluent API) |
| **ORM** | Yok (Document tabanlı) | Yok (Document tabanlı) |
| **Full-text Search** | ❌ Yok | ✅ Var |
| **N+1 Problemi** | ⚠️ Var (foreach içinde sorgu) | ✅ Yok (denormalize veri) |
| **Hız** | Orta | Çok hızlı (milisaniye) |

---

### 📦 NE EKLENİR?

```
Catalog.API/
├── Controllers/
│   ├── CoursesController.cs      ◀── MEVCUT (değişmez, yazma için)
│   └── SearchController.cs       ◀── YENİ! (okuma/arama için)
├── Services/
│   ├── Abstract/
│   │   ├── ICourseService.cs     ◀── MEVCUT
│   │   └── ICourseSearchService.cs  ◀── YENİ!
│   └── Concrete/
│       ├── CourseService.cs      ◀── MEVCUT (event publish eklenir)
│       └── CourseSearchService.cs   ◀── YENİ!
├── Models/
│   └── Documents/
│       └── CourseDocument.cs     ◀── YENİ! (Elasticsearch için)
├── Consumers/
│   └── CourseCreatedConsumer.cs  ◀── YENİ!
├── Options/
│   └── ElasticsearchSettings.cs  ◀── YENİ!
└── appsettings.json              ◀── Elasticsearch URL eklenir

Docker/
└── docker-compose.yml            ◀── Elasticsearch + Kibana eklenir
```

---

## 🧠 Elasticsearch Nedir?

**Elasticsearch bir veritabanı DEĞİL, bir arama motorudur.**

### Temel Fark

| Özellik | MongoDB / SQL Server | Elasticsearch |
| :--- | :--- | :--- |
| **Ana amaç** | Veri depolama (Source of Truth) | Hızlı arama ve filtreleme |
| **Veri bütünlüğü** | ACID / Transaction desteği | Yok (eventual consistency) |
| **Join/İlişki** | MongoDB: Sınırlı, SQL: Güçlü | JOIN yok, denormalize veri |
| **Full-text search** | Zayıf | Çok güçlü |
| **Gerçek zamanlı arama** | Yavaş (index taraması) | Milisaniyeler (inverted index) |

---

## ❓ Neden Elasticsearch Lazım?

### Mevcut Problemler (CourseService.cs)

Şu anki `GetAllAsync()` metodumuza bakalım:

```csharp
public async Task<List<CourseDto>> GetAllAsync()
{
    var courses = await _courseCollection.Find(_ => true).ToListAsync();

    // ⚠️ N+1 Problemi! Her kurs için ayrı Category sorgusu
    foreach (var course in courses)
    {
        course.Category = await _categoryCollection
            .Find<Category>(x => x.Id == course.CategoryId)
            .FirstOrDefaultAsync();
    }
    
    return _mapper.Map<List<CourseDto>>(courses);
}
```

**Problemler:**
1. **N+1 Sorgusu:** MongoDB'de JOIN yapamadığımız için her kurs için ayrı sorgu atıyoruz
2. **Full-text Search Yok:** "C# programlama" araması yapılamıyor
3. **Filtreleme Yavaş:** Binlerce kurs olduğunda performans düşer
4. **Faceted Search Yok:** "Programlama kategorisinde 50-100 TL arası" gibi filtreler zor

---

## 🏗️ Mimari: CQRS Yaklaşımı

Elasticsearch entegrasyonunda **CQRS (Command Query Responsibility Segregation)** pattern'i kullanılır:

```
┌─────────────────────────────────────────────────────────────────┐
│                         API LAYER                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   YAZMA (Command)                    OKUMA (Query)              │
│   ─────────────────                  ──────────────             │
│   Create / Update / Delete           Search / Filter / List     │
│           │                                  │                  │
│           ▼                                  ▼                  │
│   ┌───────────────┐                 ┌────────────────┐          │
│   │   MongoDB     │                 │ Elasticsearch  │          │
│   │   (Master)    │ ───Event───▶   │   (Replica)    │          │
│   └───────────────┘                 └────────────────┘          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Neden İki Farklı Depo?

- **MongoDB (Source of Truth):** Gerçek veri burada. Transaction, tutarlılık önemli.
- **Elasticsearch (Search Index):** Sadece arama için optimize edilmiş kopya. Milisaniye hızında.

---

## 📦 Gerekli NuGet Paketleri

> [!CAUTION]
> **NEST ve Elasticsearch.Net paketleri deprecated oldu!**
> Elasticsearch 8.13 ile birlikte NEST resmi olarak kullanımdan kaldırıldı ve 2024 sonunda End-of-Life oldu.
> Yeni projeler için **Elastic.Clients.Elasticsearch** kullanılmalıdır.

### ❌ ESKİ (Deprecated - Kullanma!)

```bash
# ⛔ KULLANMA! Deprecated
dotnet add package NEST
dotnet add package Elasticsearch.Net
```

### ✅ YENİ (Önerilen)

```bash
# Catalog.API projesine ekle
dotnet add package Elastic.Clients.Elasticsearch
```

**Elastic.Clients.Elasticsearch Nedir?**
- Elasticsearch 8.x için **resmi .NET client**
- NEST'in modern successor'ı
- `System.Text.Json` tabanlı serialization (daha hızlı)
- Strongly-typed sorgular (LINQ benzeri ama LINQ DEĞİL)
- Fluent API ile sorgu yazımı
- Major/minor version Elasticsearch server versiyonuyla uyumlu

### Paket Karşılaştırması

| Özellik | NEST (Eski) | Elastic.Clients.Elasticsearch (Yeni) |
|---------|-------------|--------------------------------------|
| **Durum** | ❌ Deprecated | ✅ Aktif |
| **ES 8.x Desteği** | Sınırlı | Tam |
| **Serialization** | Newtonsoft.Json | System.Text.Json |
| **Namespace** | `Nest` | `Elastic.Clients.Elasticsearch` |
| **Client** | `IElasticClient` | `ElasticsearchClient` |

---

## ⚠️ LINQ Kullanılır mı?

**HAYIR! Elasticsearch sorguları LINQ ile yazılmaz.**

### Karşılaştırma

```csharp
// ❌ MongoDB + LINQ (Şu anki durum)
var courses = await _courseCollection
    .Find(x => x.Name.Contains("C#") && x.Price < 100)
    .ToListAsync();

// ✅ Elasticsearch + Elastic.Clients.Elasticsearch (Yeni API)
var response = await _elasticClient.SearchAsync<CourseDocument>(s => s
    .Index("courses")
    .Query(q => q
        .Bool(b => b
            .Must(
                m => m.Match(ma => ma.Field(f => f.Name).Query("C#")),
                m => m.Range(r => r.NumberRange(nr => nr.Field(f => f.Price).Lt(100)))
            )
        )
    )
);
```

### Neden LINQ Değil?

1. **Farklı Query Language:** Elasticsearch kendi DSL (Domain Specific Language) kullanır
2. **Full-text Search:** LINQ `Contains()` ile "C# programlama" araması yapılamaz
3. **Relevance Scoring:** Elasticsearch sonuçları alaka düzeyine göre sıralar
4. **Aggregations:** Faceted search, histogram, nested aggregations LINQ ile mümkün değil

---

## 🛠️ ORM Var mı?

**Hayır, klasik ORM yoktur.** Ama `Elastic.Clients.Elasticsearch` bize strongly-typed çalışma imkanı sağlar.

### Document Modeli (POCO)

```csharp
// Models/Documents/CourseDocument.cs
// Bu MongoDB'deki Course entity'sinin Elasticsearch versiyonu

public class CourseDocument
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    
    // ✅ Denormalize! Category bilgisi burada
    // MongoDB'de JOIN yapmak yerine veriyi düzleştirdik
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    
    public string InstructorId { get; set; }
    public string InstructorName { get; set; }
    
    public DateTime CreatedTime { get; set; }
    
    // Arama için özel alanlar
    public int Duration { get; set; }  // dakika
    public double Rating { get; set; }
    public int StudentCount { get; set; }
}
```

### Neden Denormalize?

MongoDB'de şöyle bir ilişki var:
```
Course.CategoryId  ──▶  Category.Id
```

Elasticsearch'te JOIN olmadığı için:
```json
{
    "id": "course123",
    "name": "C# Mastery",
    "categoryId": "cat1",
    "categoryName": "Programlama"  // ← Doğrudan burada!
}
```

---

## ⚙️ Konfigürasyon

### 1. appsettings.json

```json
{
  "ElasticsearchSettings": {
    "Uri": "http://localhost:9200",
    "DefaultIndex": "courses",
    "Username": "",
    "Password": ""
  }
}
```

### 2. Options Sınıfı

```csharp
// Options/ElasticsearchSettings.cs
public class ElasticsearchSettings
{
    public string Uri { get; set; }
    public string DefaultIndex { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
```

### 3. Program.cs Konfigürasyonu

```csharp
// Program.cs
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

// 1. Settings'i oku
builder.Services.Configure<ElasticsearchSettings>(
    builder.Configuration.GetSection("ElasticsearchSettings"));

// 2. ElasticsearchClient'ı DI'a ekle (YENİ API!)
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;
    
    var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Uri))
        .DefaultIndex(settings.DefaultIndex);
    
    // Development için debug aktifleştir
    if (builder.Environment.IsDevelopment())
    {
        clientSettings.EnableDebugMode();
        clientSettings.PrettyJson();
    }
    
    // Eğer authentication varsa
    if (!string.IsNullOrEmpty(settings.Username))
    {
        clientSettings.Authentication(
            new BasicAuthentication(settings.Username, settings.Password));
    }
    
    return new ElasticsearchClient(clientSettings);
});

// 3. Service'leri ekle
builder.Services.AddScoped<ICourseSearchService, CourseSearchService>();
```

> [!NOTE]
> **Önemli Farklar:**
> - `IElasticClient` → `ElasticsearchClient` (interface yerine concrete class)
> - `ConnectionSettings` → `ElasticsearchClientSettings`
> - `BasicAuthentication(user, pass)` → `Authentication(new BasicAuthentication(...))`

---

## 🔍 Service Katmanı

### Interface

```csharp
// Services/Abstract/ICourseSearchService.cs
public interface ICourseSearchService
{
    // Arama
    Task<SearchResult<CourseDocument>> SearchAsync(CourseSearchRequest request);
    
    // Tek döküman getir
    Task<CourseDocument> GetByIdAsync(string id);
    
    // Index işlemleri (veri senkronizasyonu için)
    Task IndexAsync(CourseDocument document);
    Task UpdateAsync(CourseDocument document);
    Task DeleteAsync(string id);
    
    // Toplu işlemler
    Task BulkIndexAsync(IEnumerable<CourseDocument> documents);
}
```

### Request/Response Modelleri

```csharp
// Dtos/CourseSearchRequest.cs
public class CourseSearchRequest
{
    public string Query { get; set; }           // Arama terimi
    public string CategoryId { get; set; }      // Kategori filtresi
    public decimal? MinPrice { get; set; }      // Min fiyat
    public decimal? MaxPrice { get; set; }      // Max fiyat
    public int Page { get; set; } = 1;          // Sayfa numarası
    public int PageSize { get; set; } = 10;     // Sayfa boyutu
    public string SortBy { get; set; }          // Sıralama alanı
    public bool SortDescending { get; set; }    // Azalan sıralama
}

// Dtos/SearchResult.cs
public class SearchResult<T>
{
    public List<T> Items { get; set; }
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    // Facets (Aggregations)
    public Dictionary<string, long> CategoryFacets { get; set; }
    public Dictionary<string, long> PriceRangeFacets { get; set; }
}
```

### Concrete Implementation

```csharp
// Services/Concrete/CourseSearchService.cs
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

public class CourseSearchService : ICourseSearchService
{
    private readonly ElasticsearchClient _elasticClient;  // YENİ TİP!
    private const string IndexName = "courses";

    public CourseSearchService(ElasticsearchClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task<SearchResult<CourseDocument>> SearchAsync(CourseSearchRequest request)
    {
        var response = await _elasticClient.SearchAsync<CourseDocument>(s => s
            .Index(IndexName)
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => BuildQuery(request))
            .Sort(sort => BuildSort(sort, request))
            .Aggregations(agg => agg
                // Kategori bazlı sayım
                .Add("categories", a => a
                    .Terms(t => t
                        .Field(f => f.CategoryName.Suffix("keyword"))
                        .Size(20)
                    )
                )
                // Fiyat aralıkları
                .Add("price_ranges", a => a
                    .Range(r => r
                        .Field(f => f.Price)
                        .Ranges(
                            new AggregationRange { From = 0, To = 50, Key = "0-50" },
                            new AggregationRange { From = 50, To = 100, Key = "50-100" },
                            new AggregationRange { From = 100, To = 200, Key = "100-200" },
                            new AggregationRange { From = 200, Key = "200+" }
                        )
                    )
                )
            )
        );

        return new SearchResult<CourseDocument>
        {
            Items = response.Documents.ToList(),
            TotalCount = response.Total,
            Page = request.Page,
            PageSize = request.PageSize,
            CategoryFacets = ExtractTermsAggregation(response, "categories"),
            PriceRangeFacets = ExtractRangeAggregation(response, "price_ranges")
        };
    }

    private Query BuildQuery(CourseSearchRequest request)
    {
        var mustQueries = new List<Query>();

        // Full-text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            mustQueries.Add(new MultiMatchQuery
            {
                Query = request.Query,
                Fields = new[] { "name^3", "description", "instructorName^2" },  // ^3 = 3x boost
                Fuzziness = new Fuzziness("AUTO")  // Yazım hataları toleransı
            });
        }

        // Kategori filtresi
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            mustQueries.Add(new TermQuery("categoryId") { Value = request.CategoryId });
        }

        // Fiyat aralığı
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            mustQueries.Add(new NumberRangeQuery("price")
            {
                Gte = request.MinPrice.HasValue ? (double)request.MinPrice.Value : null,
                Lte = request.MaxPrice.HasValue ? (double)request.MaxPrice.Value : null
            });
        }

        // Eğer hiç filtre yoksa tüm dökümanları getir
        if (!mustQueries.Any())
        {
            return new MatchAllQuery();
        }

        return new BoolQuery { Must = mustQueries };
    }

    private SortOptionsDescriptor<CourseDocument> BuildSort(
        SortOptionsDescriptor<CourseDocument> sort, 
        CourseSearchRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "price" => sort.Field(f => f.Price, new FieldSort 
            { 
                Order = request.SortDescending ? SortOrder.Desc : SortOrder.Asc 
            }),
            "date" => sort.Field(f => f.CreatedTime, new FieldSort { Order = SortOrder.Desc }),
            "rating" => sort.Field(f => f.Rating, new FieldSort { Order = SortOrder.Desc }),
            _ => sort.Score(new ScoreSort { Order = SortOrder.Desc })  // Varsayılan: Relevance
        };
    }

    // Index işlemleri
    public async Task IndexAsync(CourseDocument document)
    {
        await _elasticClient.IndexAsync(document, idx => idx.Index(IndexName).Id(document.Id));
    }

    public async Task UpdateAsync(CourseDocument document)
    {
        await _elasticClient.UpdateAsync<CourseDocument, CourseDocument>(
            IndexName, 
            document.Id, 
            u => u.Doc(document).DocAsUpsert(true)
        );
    }

    public async Task DeleteAsync(string id)
    {
        await _elasticClient.DeleteAsync(IndexName, id);
    }

    public async Task BulkIndexAsync(IEnumerable<CourseDocument> documents)
    {
        var bulkResponse = await _elasticClient.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(documents)
        );
        
        if (bulkResponse.Errors)
        {
            foreach (var item in bulkResponse.ItemsWithErrors)
            {
                Console.WriteLine($"Indexing error: {item.Error?.Reason}");
            }
        }
    }

    // Helper methods
    private Dictionary<string, long> ExtractTermsAggregation(
        SearchResponse<CourseDocument> response, string name)
    {
        var agg = response.Aggregations?.GetStringTerms(name);
        return agg?.Buckets.ToDictionary(b => b.Key.ToString(), b => b.DocCount) 
            ?? new Dictionary<string, long>();
    }

    private Dictionary<string, long> ExtractRangeAggregation(
        SearchResponse<CourseDocument> response, string name)
    {
        var agg = response.Aggregations?.GetRange(name);
        return agg?.Buckets.ToDictionary(b => b.Key ?? "", b => b.DocCount) 
            ?? new Dictionary<string, long>();
    }
}
```

> [!TIP]
> **Yeni API'deki Önemli Değişiklikler:**
> - `IElasticClient` → `ElasticsearchClient`
> - `QueryContainer` → `Query`
> - `QueryContainerDescriptor<T>` → Doğrudan `Query` nesneleri
> - `ISearchResponse<T>` → `SearchResponse<T>`
> - Aggregation erişimi: `.Aggregations.Terms()` → `.Aggregations?.GetStringTerms()`

---

## 🔄 Veri Senkronizasyonu (Event-Driven)

MongoDB'de veri değiştiğinde Elasticsearch'ü güncellemek için **Event-Driven** yaklaşım kullanılır.

### 1. Event Tanımları (Shared Projesi)

```csharp
// Udemy.Shared/Events/CourseEvents.cs
public class CourseCreated
{
    public string CourseId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string InstructorId { get; set; }
    public string InstructorName { get; set; }
}

public class CourseUpdated
{
    public string CourseId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    // ... diğer alanlar
}

public class CourseDeleted
{
    public string CourseId { get; set; }
}
```

### 2. Event Publish (CourseService)

```csharp
// CourseService.cs - CreateAsync metodu güncelleme
public async Task<CourseDto> CreateAsync(CourseCreateDto dto)
{
    var newCourse = _mapper.Map<Course>(dto);
    newCourse.CreatedTime = DateTime.Now;
    
    // Instructor bilgisini token'dan al
    // ... mevcut kod ...
    
    await _courseCollection.InsertOneAsync(newCourse);
    
    // ✅ Event fırlat - Elasticsearch Consumer bunu dinleyecek
    var category = await _categoryCollection
        .Find<Category>(x => x.Id == newCourse.CategoryId)
        .FirstOrDefaultAsync();
    
    await _publishEndpoint.Publish(new CourseCreated
    {
        CourseId = newCourse.Id,
        Name = newCourse.Name,
        Description = newCourse.Description,
        Price = newCourse.Price,
        CategoryId = newCourse.CategoryId,
        CategoryName = category?.Name,
        InstructorId = newCourse.UserId,
        InstructorName = newCourse.UserName
    });

    return _mapper.Map<CourseDto>(newCourse);
}
```

### 3. Consumer (Elasticsearch Sync)

```csharp
// Consumers/CourseCreatedConsumer.cs
public class CourseCreatedConsumer : IConsumer<CourseCreated>
{
    private readonly ICourseSearchService _searchService;

    public CourseCreatedConsumer(ICourseSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task Consume(ConsumeContext<CourseCreated> context)
    {
        var message = context.Message;
        
        var document = new CourseDocument
        {
            Id = message.CourseId,
            Name = message.Name,
            Description = message.Description,
            Price = message.Price,
            CategoryId = message.CategoryId,
            CategoryName = message.CategoryName,
            InstructorId = message.InstructorId,
            InstructorName = message.InstructorName,
            CreatedTime = DateTime.UtcNow
        };
        
        await _searchService.IndexAsync(document);
        
        Console.WriteLine($"[Elasticsearch] Indexed course: {message.CourseId}");
    }
}
```

---

## 🎯 Controller Kullanımı

```csharp
// Controllers/SearchController.cs
[Route("api/[controller]")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly ICourseSearchService _searchService;

    public SearchController(ICourseSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> SearchCourses([FromQuery] CourseSearchRequest request)
    {
        var result = await _searchService.SearchAsync(request);
        return Ok(result);
    }
}
```

### Örnek API Çağrıları

```
# Basit arama
GET /api/search/courses?query=C# programlama

# Filtreli arama
GET /api/search/courses?query=web&categoryId=cat1&minPrice=50&maxPrice=200

# Sayfalama ve sıralama
GET /api/search/courses?query=python&page=2&pageSize=20&sortBy=price&sortDescending=true
```

---

## 🐳 Docker ile Elasticsearch Kurulumu

### docker-compose.yml

```yaml
version: '3.8'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false  # Development için
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    container_name: kibana
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch

volumes:
  elasticsearch-data:
```

### Çalıştırma

```bash
docker-compose up -d
```

### Kontrol

```bash
# Elasticsearch çalışıyor mu?
curl http://localhost:9200

# Kibana UI
http://localhost:5601
```

---

## 📊 Özet: Ne Zaman Elasticsearch Kullanılır?

### ✅ Kullan

- Full-text search gerekiyorsa ("C# web programlama" araması)
- Faceted search gerekiyorsa (kategori, fiyat aralığı filtreleri)
- Autocomplete / Suggest özelliği lazımsa
- Log aggregation (ELK Stack)
- Büyük veri setlerinde hızlı arama

### ❌ Kullanma

- Sadece CRUD işlemleri varsa
- Transaction gerekiyorsa
- İlişkisel veri yoğunsa
- Küçük veri setleri (< 10.000 kayıt)

---

## 🔗 Faydalı Kaynaklar

> [!IMPORTANT]
> **NEST deprecated oldu!** Aşağıdaki yeni dokümantasyonu kullan.

- [Elastic.Clients.Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html) ← **YENİ CLIENT**
- [Migration Guide (NEST → New Client)](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/migration-guide.html)
- [Elasticsearch Query DSL](https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html)
- [NuGet Package](https://www.nuget.org/packages/Elastic.Clients.Elasticsearch)
- [Kibana Dev Tools](http://localhost:5601/app/dev_tools#/console)

---

**Sonraki Adım:** Catalog servisine Elasticsearch entegrasyonu için `ICourseSearchService` ve `CourseSearchService` implementasyonu yapılabilir. Mevcut `ICourseService` yazma işlemleri için, yeni `ICourseSearchService` okuma/arama işlemleri için kullanılacak.
