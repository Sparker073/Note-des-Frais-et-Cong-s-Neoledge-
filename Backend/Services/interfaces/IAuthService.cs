using MonBackend.Models;
using MonBackend.DTOs;

namespace MonBackend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(UserRegisterDto dto);
    Task<AuthResult> LoginAsync(UserLoginDto dto);
    Task<VerifyResult> VerifyTokenAsync(string token);
}
