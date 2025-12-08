using System.Collections.Generic;

namespace CampoLibre.Api.Domain.Entities
{
    public class Cancha
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string Tipo { get; set; } = null!; // Ej: "Fútbol 5", "Pádel"

        public bool Techada { get; set; }

        public bool Iluminacion { get; set; }

        public decimal PrecioHora { get; set; }

        // Navegación
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
