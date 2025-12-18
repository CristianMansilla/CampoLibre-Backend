using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampoLibre.Api.Application.DTOs;
using CampoLibre.Api.Domain.Entities;
using CampoLibre.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CampoLibre.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] AuthRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.NombreCompleto))
            {
                return BadRequest("Nombre, email y password son obligatorios.");
            }

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExiste)
                return BadRequest("Ya existe un usuario con ese email.");

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                Rol = UserRole.Cliente,
                Activo = true
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var response = GenerarToken(usuario);

            return CreatedAtAction(nameof(Register), response);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] AuthLoginDto dto)
        {

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario is null)
                return Unauthorized("Credenciales inválidas.");

            if (!usuario.Activo)
                return Unauthorized("Usuario dado de baja.");

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                dto.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
                return Unauthorized("Credenciales inválidas.");

            var response = GenerarToken(usuario);

            return Ok(response);
        }

        private AuthResponseDto GenerarToken(Usuario usuario)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var minutos = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60;
            var expires = DateTime.UtcNow.AddMinutes(minutos);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                ExpiraEn = expires,
                UsuarioId = usuario.Id,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString()
            };
        }
    }
}
