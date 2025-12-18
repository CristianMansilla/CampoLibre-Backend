using CampoLibre.Api.Domain.Entities;

namespace CampoLibre.Api.Application.DTOs
{
    public class AuthRegisterDto
    {
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
