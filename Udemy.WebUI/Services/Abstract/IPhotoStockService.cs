using Udemy.WebUI.Models.PhotoStocks;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IPhotoStockService
    {
        Task<PhotoViewModel?> UploadPhoto(IFormFile? photo, string? courseName = null);

        Task<bool> DeletePhoto(string photoUrl);
    }
}
