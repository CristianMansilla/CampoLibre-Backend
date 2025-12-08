namespace CampoLibre.Api.Application.DTOs
{
    public class ReservaDto
    {
        public int Id { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public bool Pagada { get; set; }

        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = null!;

        public int CanchaId { get; set; }
        public string CanchaNombre { get; set; } = null!;
    }
}
