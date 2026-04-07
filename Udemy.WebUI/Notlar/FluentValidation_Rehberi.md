# FluentValidation Kullanım Rehberi ve Proje Uygulaması

Bu doküman, projemizde doğrulama (validation) işlemleri için neden ve nasıl **FluentValidation** kullandığımızı adım adım anlatır.

---

## 1. Adım: Kütüphanelerin Kurulumu

Projemize (WebUI) şu iki NuGet paketini ekledik:
1.  `FluentValidation` (Çekirdek kütüphane)
2.  `FluentValidation.AspNetCore` (ASP.NET Core entegrasyonu için)

Bu paketler sayesinde doğrulama kurallarını Model class'larının içine sıkıştırmak yerine, ayrı sınıflarda temiz bir şekilde yönetebiliyoruz.

---

## 2. Adım: Program.cs Konfigürasyonu

Projeyi başlattığımızda FluentValidation'ın devreye girmesi ve yazdığımız kuralları tanıması için `Program.cs` dosyasına şu satırları ekledik:

```csharp
// Program.cs

using FluentValidation;
using FluentValidation.AspNetCore;

// ...

// 1. Otomatik doğrulamayı aktif et (Controller'a gelen veriyi otomatik kontrol eder)
builder.Services.AddFluentValidationAutoValidation();

// 2. Validator sınıflarının nerede olduğunu söyle (Bu Assembly'deki tüm validator'ları bulur)
builder.Services.AddValidatorsFromAssemblyContaining<CourseCreateInputValidator>();
```

Bu sayede tek tek her validator için `AddScoped` yazmamıza gerek kalmaz.

---

## 3. Adım: Validator Sınıfının Yazılması

Bir model için kural yazmak istediğimizde `AbstractValidator<T>` sınıfından miras alan bir class oluştururuz.

**Örnek:** `Udemy.WebUI\Validators\CourseCreateInputValidator.cs`

```csharp
public class CourseCreateInputValidator : AbstractValidator<CourseCreateInput>
{
    public CourseCreateInputValidator()
    {
        // Kural: İsim boş olamaz
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("isim alanı boş olamaz");

        // Kural: Fiyat 0 olamaz ve boş olamaz
        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("fiyat alanı boş olamaz")
            .ScalePrecision(2, 6).WithMessage("fiyat formatı hatalı"); // (örn: 9999.99)
    }
}
```

---

## 4. Adım: Controller ve View Tarafı

Harika olan kısım burası: **Controller'da neredeyse hiçbir şey değiştirmiyoruz!**

Çünkü `AddFluentValidationAutoValidation()` methodu sayesinde, Request geldiği anda FluentValidation araya girer, kuralları kontrol eder ve hataları `ModelState` içine doldurur.

**Controller:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(CourseCreateInput input)
{
    // AutoValidation burayı doldurdu bile!
    if (!ModelState.IsValid) 
    {
        return View(input); // Hatalarla birlikte formu geri döndür
    }
    // ... işlem başarılı devam et
}
```

**View (.cshtml):**
Standart ASP.NET Core tag helper'ları çalışmaya devam eder:
```html
<span asp-validation-for="Name" class="text-danger"></span>
```

---

## 5. Karşılaştırma: FluentValidation vs DataAnnotations

ASP.NET Core'un içinde gelen `[Required]`, `[MaxLength]` gibi attribute'lara **Data Annotations** denir. Peki biz neden onları kullanmadık?

| Özellik | Data Annotations (Klasik) | FluentValidation (Modern) |
| :--- | :--- | :--- |
| **Yazım Yeri** | Model class'ının içi (Attribute olarak) | Ayrı bir Validator sınıfı |
| **Kod Temizliği** | Modeli kirletir (SOLID'e aykırı olabilir) | **Temiz Model (POCO)**, kurallar ayrı yerde |
| **Karmaşık Kurallar** | Zor (CustomAttribute yazmak gerekir) | **Çok Kolay** (If, When, DBAccess vb. yapılabilir) |
| **Koşullu Doğrulama** | Zor | Kolay (`When(x => x.IsActive, ...)` gibi) |
| **Test Edilebilirlik** | Zordur | Çok kolay Unit Test yazılır |
| **Popülerlik** | Standart, her yerde var | Kurumsal projelerde standart |

**Özet:**
Projemiz "Clean Architecture" ve "Clean Code" prensiplerine uygun olması için **Model sınıflarımızı (Input dosylarını) temiz tutmak** istedik. Kuralları ayrı dosyalara (`Validators` klasörü) taşıyarak yönetimi kolaylaştırdık. Bu yüzden **FluentValidation** tercih ettik.
