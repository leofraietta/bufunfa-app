using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Bufunfa.Api.Data;
using Bufunfa.Api.Models;

namespace Bufunfa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Debug: Log all available claims
            Console.WriteLine("=== GoogleCallback Debug ===");
            Console.WriteLine($"User authenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine($"Authentication type: {User.Identity.AuthenticationType}");
            
            if (User.Identity.IsAuthenticated)
            {
                Console.WriteLine("Available claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }

            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            Console.WriteLine($"Authenticate result succeeded: {authenticateResult.Succeeded}");

            if (!authenticateResult.Succeeded)
            {
                Console.WriteLine($"Authentication failed. Error: {authenticateResult.Failure?.Message}");
                return BadRequest("Autenticação Google falhou.");
            }

            var claims = authenticateResult.Principal.Claims;
            Console.WriteLine($"Claims count: {claims.Count()}");
            
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            Console.WriteLine($"Extracted - Email: {email}, Name: {name}, GoogleId: {googleId}");
            Console.WriteLine($"GoogleId length: {googleId?.Length}, GoogleId bytes: {System.Text.Encoding.UTF8.GetByteCount(googleId ?? "")}");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                Console.WriteLine("Email or GoogleId is null/empty - returning BadRequest");
                return BadRequest("Dados do Google incompletos.");
            }

            // Verifica se o usuário já existe no banco de dados
            Console.WriteLine($"Querying database for GoogleId: '{googleId}'");
            var usuario = _context.Usuarios.FirstOrDefault(u => u.GoogleId == googleId);
            Console.WriteLine($"Database query result: {(usuario != null ? "User found" : "User not found")}");

            if (usuario == null)
            {
                // Cria um novo usuário se não existir
                usuario = new Usuario
                {
                    Nome = name,
                    Email = email,
                    GoogleId = googleId
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            // Gera o token JWT
            var token = GenerateJwtToken(usuario);

            // Redireciona para o frontend com o token como parâmetro de query
            var frontendUrl = "http://localhost:4200/auth/callback";
            var redirectUrl = $"{frontendUrl}?token={token}&userId={usuario.Id}&userName={Uri.EscapeDataString(usuario.Nome)}&userEmail={Uri.EscapeDataString(usuario.Email)}";
            
            return Redirect(redirectUrl);
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

