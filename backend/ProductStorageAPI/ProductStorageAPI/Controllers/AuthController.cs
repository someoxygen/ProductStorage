using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductStorageAPI.Abstractions;
using ProductStorageAPI.DTOs.Auth;

namespace ProductStorageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
