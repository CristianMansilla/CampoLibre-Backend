namespace CampoLibre.Api.Application.DTOs
{
    public class CanchaCreateUpdateDto
    {
        public string Nombre { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public bool Techada { get; set; }
        public bool Iluminacion { get; set; }
        public decimal PrecioHora { get; set; }
    }
}
