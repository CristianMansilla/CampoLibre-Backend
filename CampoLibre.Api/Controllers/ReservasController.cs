using CampoLibre.Api.Application.DTOs;
using CampoLibre.Api.Domain.Entities;
using CampoLibre.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace CampoLibre.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Cancha)
                .OrderBy(r => r.FechaHoraInicio)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    FechaHoraInicio = r.FechaHoraInicio,
                    FechaHoraFin = r.FechaHoraFin,
                    Pagada = r.Pagada,
                    UsuarioId = r.UsuarioId,
                    UsuarioNombre = r.Usuario.NombreCompleto,
                    CanchaId = r.CanchaId,
                    CanchaNombre = r.Cancha.Nombre
                })
                .ToListAsync();

            return Ok(reservas);
        }


        // GET: api/Reservas/mias 
        [HttpGet("mias")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetMisReservas()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("No se pudo obtener el usuario del token.");

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Usuario inválido en el token.");

            var reservas = await _context.Reservas
                .Where(r => r.UsuarioId == userId)
                .Include(r => r.Cancha)
                .Include(r => r.Usuario)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    FechaHoraInicio = r.FechaHoraInicio,
                    FechaHoraFin = r.FechaHoraFin,
                    Pagada = r.Pagada,
                    UsuarioId = r.UsuarioId,
                    UsuarioNombre = r.Usuario!.NombreCompleto,
                    CanchaId = r.CanchaId,
                    CanchaNombre = r.Cancha!.Nombre
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/Reservas/ocupadas?canchaId=1&fecha=2025-12-17
        [HttpGet("ocupadas")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<int>>> GetHorasOcupadas(
            [FromQuery] int canchaId,
            [FromQuery] DateOnly fecha
        )
        {
            if (canchaId <= 0)
                return BadRequest("canchaId inválido.");

            // Día completo (00:00 a 00:00 del siguiente)
            var dayStart = fecha.ToDateTime(new TimeOnly(0, 0));
            var dayEnd = dayStart.AddDays(1);

            // Traemos reservas que se solapen con ese día
            var reservas = await _context.Reservas
                .Where(r =>
                    r.CanchaId == canchaId &&
                    r.FechaHoraInicio < dayEnd &&
                    r.FechaHoraFin > dayStart
                )
                .Select(r => new { r.FechaHoraInicio, r.FechaHoraFin })
                .ToListAsync();

            // Horario negocio: 14:00 a 00:00 => horas 14..23
            var ocupadas = new HashSet<int>();

            for (int h = 14; h <= 23; h++)
            {
                var slotStart = fecha.ToDateTime(new TimeOnly(h, 0));
                var slotEnd = slotStart.AddHours(1);

                var overlaps = reservas.Any(r =>
                    r.FechaHoraInicio < slotEnd &&
                    r.FechaHoraFin > slotStart
                );

                if (overlaps)
                    ocupadas.Add(h);
            }

            return Ok(ocupadas.OrderBy(x => x));
        }



        // GET: api/reservas/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReservaDto>> GetReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Cancha)
                .Where(r => r.Id == id)
                .Select(r => new ReservaDto
                {
                    Id = r.Id,
                    FechaHoraInicio = r.FechaHoraInicio,
                    FechaHoraFin = r.FechaHoraFin,
                    Pagada = r.Pagada,
                    UsuarioId = r.UsuarioId,
                    UsuarioNombre = r.Usuario != null ? r.Usuario.NombreCompleto : string.Empty,
                    CanchaId = r.CanchaId,
                    CanchaNombre = r.Cancha != null ? r.Cancha.Nombre : string.Empty
                })
                .FirstOrDefaultAsync();

            if (reserva is null)
                return NotFound();

            return Ok(reserva);
        }

        // POST: api/reservas
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReservaDto>> CreateReserva([FromBody] ReservaCreateUpdateDto dto)
        {
            if (dto.FechaHoraInicio >= dto.FechaHoraFin)
                return BadRequest("La fecha/hora de inicio debe ser menor a la de fin.");

            // Obtener usuario desde el token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario is null)
                return Unauthorized("Usuario no válido.");

            // Validar cancha
            var cancha = await _context.Canchas.FindAsync(dto.CanchaId);
            if (cancha is null)
                return BadRequest($"No existe una cancha con Id = {dto.CanchaId}.");

            // Validar solapamiento
            var haySolapamiento = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == dto.CanchaId &&
                r.FechaHoraInicio < dto.FechaHoraFin &&
                dto.FechaHoraInicio < r.FechaHoraFin
            );

            if (haySolapamiento)
                return BadRequest("Ya existe una reserva en ese horario para esta cancha.");

            // Crear reserva
            var reserva = new Reserva
            {
                FechaHoraInicio = dto.FechaHoraInicio,
                FechaHoraFin = dto.FechaHoraFin,
                Pagada = dto.Pagada,
                UsuarioId = userId,
                CanchaId = dto.CanchaId
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            // Respuesta DTO
            var result = new ReservaDto
            {
                Id = reserva.Id,
                FechaHoraInicio = reserva.FechaHoraInicio,
                FechaHoraFin = reserva.FechaHoraFin,
                Pagada = reserva.Pagada,
                UsuarioId = usuario.Id,
                UsuarioNombre = usuario.NombreCompleto,
                CanchaId = cancha.Id,
                CanchaNombre = cancha.Nombre
            };

            return CreatedAtAction(nameof(GetReserva), new { id = result.Id }, result);
        }


        // PUT: api/reservas/5
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateReserva(int id, [FromBody] ReservaCreateUpdateDto dto)
        {
            // Buscar la reserva
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva is null)
                return NotFound();

            // Validar fechas
            if (dto.FechaHoraInicio >= dto.FechaHoraFin)
                return BadRequest("La fecha/hora de inicio debe ser menor a la de fin.");

            // Obtener usuario desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            // Si el usuario es Cliente, solo puede modificar sus propias reservas
            if (rol == "Cliente" && reserva.UsuarioId != userId)
                return Forbid("No podés modificar reservas de otro usuario.");

            // Validar cancha
            var canchaExiste = await _context.Canchas
                .AnyAsync(c => c.Id == dto.CanchaId);
            if (!canchaExiste)
                return BadRequest($"No existe una cancha con Id = {dto.CanchaId}.");

            // Validar solapamiento en esa cancha (excepto esta misma reserva)
            var haySolapamiento = await _context.Reservas.AnyAsync(r =>
                r.Id != id &&
                r.CanchaId == dto.CanchaId &&
                r.FechaHoraInicio < dto.FechaHoraFin &&
                dto.FechaHoraInicio < r.FechaHoraFin
            );

            if (haySolapamiento)
                return BadRequest("Ya existe otra reserva en ese horario para esta cancha.");

            // Actualizar solo campos permitidos
            reserva.FechaHoraInicio = dto.FechaHoraInicio;
            reserva.FechaHoraFin = dto.FechaHoraFin;
            reserva.Pagada = dto.Pagada;
            reserva.CanchaId = dto.CanchaId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/reservas/5/pagada
        [HttpPatch("{id:int}/pagada")]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> SetPagada(int id, [FromBody] bool pagada)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva is null)
                return NotFound();

            reserva.Pagada = pagada;
            await _context.SaveChangesAsync();

            return NoContent();
        }



        // DELETE: api/reservas/5
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva is null)
                return NotFound();

            // Obtener usuario desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            // Si es Cliente, solo puede borrar SUS reservas
            if (rol == "Cliente" && reserva.UsuarioId != userId)
                return Forbid("No podés eliminar reservas de otro usuario.");

            // Admin u Operador pueden borrar cualquiera
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
