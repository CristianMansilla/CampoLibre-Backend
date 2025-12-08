using CampoLibre.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampoLibre.Api.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cancha> Canchas => Set<Cancha>();
        public DbSet<Reserva> Reservas => Set<Reserva>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.NombreCompleto)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.PasswordHash)
                      .IsRequired();
            });

            // Cancha
            modelBuilder.Entity<Cancha>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.Tipo)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(c => c.PrecioHora)
                      .HasColumnType("decimal(18,2)");
            });

            // Reserva
            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.Usuario)
                      .WithMany(u => u.Reservas)
                      .HasForeignKey(r => r.UsuarioId);

                entity.HasOne(r => r.Cancha)
                      .WithMany(c => c.Reservas)
                      .HasForeignKey(r => r.CanchaId);
            });
        }
    }
}
