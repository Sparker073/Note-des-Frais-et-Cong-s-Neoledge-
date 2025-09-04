using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MonBackend.Services.Interfaces;
using MonBackend.DTOs;
using System;
using System.Threading.Tasks;

namespace MonBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

       [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                // Add validation logging
                if (dto == null)
                {
                    return BadRequest(new { message = "Invalid data received" });
                }

                // Log the received DTO for debugging
                Console.WriteLine($"Received DTO: Nom={dto.Nom}, Email={dto.Email}, Poste={dto.Poste}, ManagerId={dto.ManagerId}");

                var result = await _authService.RegisterAsync(dto);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to false for local HTTP development
                    SameSite = SameSiteMode.Lax, // Changed from Strict to Lax for better cross-origin support
                    Expires = DateTime.UtcNow.AddHours(24),
                    Path = "/"
                };

                Response.Cookies.Append("authToken", result.Token, cookieOptions);

                return Ok(new
                {
                    message = "Registration successful",
                    role = result.Role,
                    user = result.User
                });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Erreur interne : " + ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // false for local dev if no HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(24),
                    Path = "/"
                };

                Response.Cookies.Append("authToken", result.Token, cookieOptions);

                return Ok   (new
                {
                    message = "Login successful",
                    role = result.Role,
                    user = result.User
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne : " + ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {   
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            };

            Response.Cookies.Append("authToken", "", cookieOptions);

            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyToken()
        {
            try
            {
                if (Request.Cookies.TryGetValue("authToken", out string token))
                {
                    var result = await _authService.VerifyTokenAsync(token);

                    if (result.IsValid)
                    {
                        return Ok(new
                        {
                            authenticated = true,
                            role = result.Role,
                            user = result.User
                        });
                    }
                }

                return Unauthorized(new { authenticated = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne : " + ex.Message });
            }
        }
    }
}
