namespace CampoLibre.Api.Application.DTOs
{
    public class ReservaCreateUpdateDto
    {
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public bool Pagada { get; set; }
        public int UsuarioId { get; set; }
        public int CanchaId { get; set; }
    }
}
