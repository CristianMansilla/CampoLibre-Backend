using CampoLibre.Api.Application.DTOs;
using CampoLibre.Api.Domain.Entities;
using CampoLibre.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampoLibre.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Cancha)
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
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/reservas/5
        [HttpGet("{id:int}")]
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
        public async Task<ActionResult<ReservaDto>> CreateReserva([FromBody] ReservaCreateUpdateDto dto)
        {
            if (dto.FechaHoraInicio >= dto.FechaHoraFin)
                return BadRequest("La fecha/hora de inicio debe ser menor a la de fin.");

            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario is null)
                return BadRequest($"No existe un usuario con Id = {dto.UsuarioId}.");

            var cancha = await _context.Canchas.FindAsync(dto.CanchaId);
            if (cancha is null)
                return BadRequest($"No existe una cancha con Id = {dto.CanchaId}.");

            var haySolapamiento = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == dto.CanchaId &&
                r.FechaHoraInicio < dto.FechaHoraFin &&
                dto.FechaHoraInicio < r.FechaHoraFin
            );

            if (haySolapamiento)
                return BadRequest("Ya existe una reserva en ese horario para esta cancha.");

            var reserva = new Reserva
            {
                FechaHoraInicio = dto.FechaHoraInicio,
                FechaHoraFin = dto.FechaHoraFin,
                Pagada = dto.Pagada,
                UsuarioId = dto.UsuarioId,
                CanchaId = dto.CanchaId
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

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
        public async Task<IActionResult> UpdateReserva(int id, [FromBody] ReservaCreateUpdateDto dto)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva is null)
                return NotFound();

            if (dto.FechaHoraInicio >= dto.FechaHoraFin)
                return BadRequest("La fecha/hora de inicio debe ser menor a la de fin.");

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.Id == dto.UsuarioId);
            if (!usuarioExiste)
                return BadRequest($"No existe un usuario con Id = {dto.UsuarioId}.");

            var canchaExiste = await _context.Canchas
                .AnyAsync(c => c.Id == dto.CanchaId);
            if (!canchaExiste)
                return BadRequest($"No existe una cancha con Id = {dto.CanchaId}.");

            var haySolapamiento = await _context.Reservas.AnyAsync(r =>
                r.Id != id &&
                r.CanchaId == dto.CanchaId &&
                r.FechaHoraInicio < dto.FechaHoraFin &&
                dto.FechaHoraInicio < r.FechaHoraFin
            );

            if (haySolapamiento)
                return BadRequest("Ya existe otra reserva en ese horario para esta cancha.");

            reserva.FechaHoraInicio = dto.FechaHoraInicio;
            reserva.FechaHoraFin = dto.FechaHoraFin;
            reserva.Pagada = dto.Pagada;
            reserva.UsuarioId = dto.UsuarioId;
            reserva.CanchaId = dto.CanchaId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/reservas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva is null)
                return NotFound();

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
