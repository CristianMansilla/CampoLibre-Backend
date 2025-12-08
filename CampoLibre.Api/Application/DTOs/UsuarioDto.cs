using CampoLibre.Api.Domain.Entities;

namespace CampoLibre.Api.Application.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Rol { get; set; }
    }
}
