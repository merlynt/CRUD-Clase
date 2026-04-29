using appWeb2.Models;
using Microsoft.EntityFrameworkCore;

namespace appWeb2.Data
{
    public class AppDbContext : DbContext   
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<VideoJuego> VideoJuegos { get; set; }
        public DbSet<Compra> Compras { get; set; }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Rol> Roles { get; set; }

        public DbSet<DetalleCompra> DetallesCompra { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VideoJuego>()
                .Property(v => v.precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<VideoJuego>()
                .Property(v => v.porcentajeDescuento)
                .HasColumnType("decimal(5,2)");
        }
    }
}
