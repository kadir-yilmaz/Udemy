using Udemy.WebUI.Models;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IUserService
    {
        Task<UserViewModel> GetUser();
        public string GetUserId { get; }
        public string GetUserEmail { get; }
    }
}
