using System;

namespace CampoLibre.Api.Domain.Entities
{
    public class Reserva
    {
        public int Id { get; set; }

        public DateTime FechaHoraInicio { get; set; }

        public DateTime FechaHoraFin { get; set; }

        public bool Pagada { get; set; }

        // FK y navegaciones
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; } = null!;

        public int CanchaId { get; set; }
        public Cancha? Cancha { get; set; } = null!;
    }
}
