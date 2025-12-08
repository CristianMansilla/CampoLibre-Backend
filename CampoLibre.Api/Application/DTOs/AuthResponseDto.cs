namespace CampoLibre.Api.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiraEn { get; set; }

        public int UsuarioId { get; set; }
        public string Email { get; set; } = null!;
        public string Rol { get; set; } = null!;
    }
}
