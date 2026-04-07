using Udemy.WebUI.Models;
using Duende.IdentityModel.Client;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IIdentityService
    {
        Task<string> SignIn(SigninInput signinInput);
        Task SignUp(SignUpInput signUpInput);
        Task<TokenResponse> GetAccessTokenByRefreshToken();
        Task RevokeRefreshToken();
    }
}
