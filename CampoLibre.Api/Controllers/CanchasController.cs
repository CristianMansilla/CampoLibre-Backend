using CampoLibre.Api.Application.DTOs;
using CampoLibre.Api.Domain.Entities;
using CampoLibre.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampoLibre.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanchasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CanchasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/canchas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CanchaDto>>> GetCanchas()
        {
            var canchas = await _context.Canchas
                .Select(c => new CanchaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Techada = c.Techada,
                    Iluminacion = c.Iluminacion,
                    PrecioHora = c.PrecioHora
                })
                .ToListAsync();

            return Ok(canchas);
        }

        // GET: api/canchas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CanchaDto>> GetCancha(int id)
        {
            var cancha = await _context.Canchas
                .Where(c => c.Id == id)
                .Select(c => new CanchaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Techada = c.Techada,
                    Iluminacion = c.Iluminacion,
                    PrecioHora = c.PrecioHora
                })
                .FirstOrDefaultAsync();

            if (cancha is null)
                return NotFound();

            return Ok(cancha);
        }

        // POST: api/canchas
        [HttpPost]
        public async Task<ActionResult<CanchaDto>> CreateCancha([FromBody] CanchaCreateUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre de la cancha es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return BadRequest("El tipo de cancha es obligatorio.");

            var cancha = new Cancha
            {
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                Techada = dto.Techada,
                Iluminacion = dto.Iluminacion,
                PrecioHora = dto.PrecioHora
            };

            _context.Canchas.Add(cancha);
            await _context.SaveChangesAsync();

            var result = new CanchaDto
            {
                Id = cancha.Id,
                Nombre = cancha.Nombre,
                Tipo = cancha.Tipo,
                Techada = cancha.Techada,
                Iluminacion = cancha.Iluminacion,
                PrecioHora = cancha.PrecioHora
            };

            return CreatedAtAction(nameof(GetCancha), new { id = result.Id }, result);
        }

        // PUT: api/canchas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCancha(int id, [FromBody] CanchaCreateUpdateDto dto)
        {
            var cancha = await _context.Canchas.FindAsync(id);
            if (cancha is null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre de la cancha es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return BadRequest("El tipo de cancha es obligatorio.");

            cancha.Nombre = dto.Nombre;
            cancha.Tipo = dto.Tipo;
            cancha.Techada = dto.Techada;
            cancha.Iluminacion = dto.Iluminacion;
            cancha.PrecioHora = dto.PrecioHora;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/canchas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCancha(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);
            if (cancha is null)
                return NotFound();

            _context.Canchas.Remove(cancha);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
