using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IClientCredentialTokenService
    {
        Task<string> GetToken();
    }
}
