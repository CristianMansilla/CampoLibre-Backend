using CampoLibre.Api.Application.DTOs;
using CampoLibre.Api.Domain.Entities;
using CampoLibre.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CampoLibre.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuariosController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/usuarios
        [HttpGet]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    Rol = u.Rol,
                    Activo = u.Activo
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/usuarios/5
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    Rol = u.Rol,
                    Activo = u.Activo
                })
                .FirstOrDefaultAsync();

            if (usuario is null)
                return NotFound();

            return Ok(usuario);
        }

        // POST: api/usuarios
        [HttpPost]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario([FromBody] UsuarioCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
                return BadRequest("El nombre completo es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("El email es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("La contraseña es obligatoria.");

            var emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExiste)
                return BadRequest("Ya existe un usuario con ese email.");

            if (!Enum.IsDefined(typeof(UserRole), dto.Rol))
                return BadRequest("Rol inválido.");


            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                Rol = dto.Rol,
                Activo = true
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var result = new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = usuario.Rol
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = result.Id }, result);
        }

        // PUT: api/usuarios/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UsuarioUpdateDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
                return BadRequest("El nombre completo es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("El email es obligatorio.");

            // Validar email único si cambió
            var emailEnUso = await _context.Usuarios
                .AnyAsync(u => u.Id != id && u.Email == dto.Email);

            if (emailEnUso)
                return BadRequest("Ya existe otro usuario con ese email.");

            if (!Enum.IsDefined(typeof(UserRole), dto.Rol))
                return BadRequest("Rol inválido.");


            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.Email = dto.Email;
            usuario.Rol = dto.Rol;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/usuarios/5/activo
        [HttpPatch("{id:int}/activo")]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> SetActivo(int id, [FromBody] bool activo)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound();

            usuario.Activo = activo;
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
