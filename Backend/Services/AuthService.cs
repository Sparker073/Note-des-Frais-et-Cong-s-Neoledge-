using MonBackend.Models;
using MonBackend.DTOs;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MonBackend.Services
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public object User { get; set; }
        public bool IsValid { get; set; } 
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

       public async Task<AuthResult> RegisterAsync(UserRegisterDto dto)
        {
            var existing = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new ArgumentException("Email déjà utilisé.");

            // Validate ManagerId if provided
            if (dto.ManagerId.HasValue && dto.ManagerId.Value > 0)
            {
                var manager = await _userRepo.GetUserByIdAsync(dto.ManagerId.Value);
                if (manager == null)
                    throw new ArgumentException($"Le manager avec l'ID {dto.ManagerId.Value} n'existe pas.");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.motDePasse);
            var user = new User
            {
                Nom = dto.Nom,
                Email = dto.Email,
                MotDePasse = hash,
                Role = "employe",
                Poste = dto.Poste,
                ManagerId = dto.ManagerId.HasValue && dto.ManagerId.Value > 0 ? dto.ManagerId : null
            };

            await _userRepo.CreateUserAsync(user);

            return new AuthResult
            {
                Token = GenerateJwt(user),
                Role = user.Role,
                User = new
                {
                    user.Id,
                    user.Nom,
                    user.Email,
                    user.Role,
                    user.Poste
                }
            };
        }

        public async Task<AuthResult> LoginAsync(UserLoginDto dto)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.motDePasse, user.MotDePasse))
                throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

            return new AuthResult
            {
                Token = GenerateJwt(user),
                Role = user.Role,
                User = new
                {
                    user.Id,
                    user.Nom,
                    user.Email,
                    user.Role,
                    user.Poste
                }
            };
        }

        public async Task<VerifyResult> VerifyTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value;

                var user = await _userRepo.GetUserByIdAsync(int.Parse(userId));

                return new VerifyResult
                {
                    IsValid = true,
                    Role = role,
                    User = new
                    {
                        user.Id,
                        user.Nom,
                        user.Email,
                        user.Role,
                        user.Poste
                    }
                };
            }
            catch
            {
                return new VerifyResult { IsValid = false };
            }
        }

        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
