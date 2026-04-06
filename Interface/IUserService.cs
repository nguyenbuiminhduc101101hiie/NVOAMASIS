using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Interface
{
    public interface IUserService
    {
        Task<LoginResponse> SignInAsync(string userCredential);
    }
}
