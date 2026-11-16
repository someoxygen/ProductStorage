using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductStorageAPI.Abstractions;
using ProductStorageAPI.Data;
using ProductStorageAPI.DTOs.Auth;
using ProductStorageAPI.Models;

namespace ProductStorageAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(ITokenService tokenService, AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _tokenService = tokenService;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(result == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            return CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var exists = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreateAuthResponse(user);
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var jwt = _tokenService.GenerateToken(user);
            return new AuthResponseDto
            {
                Token = jwt.Token,
                ExpiresAt = jwt.ExpiresAt,
                Email = user.Email,
                FullName = user.FullName,
            };
        }
    }
}
