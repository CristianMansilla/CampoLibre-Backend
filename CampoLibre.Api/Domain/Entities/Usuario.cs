using System.Collections.Generic;

namespace CampoLibre.Api.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public UserRole Rol { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
