using CampoLibre.Api.Domain.Entities;

namespace CampoLibre.Api.Application.DTOs
{
    public class UsuarioCreateDto
    {
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserRole Rol { get; set; }
    }
}
