using Microsoft.EntityFrameworkCore;
using Prueba.Models;

namespace Prueba.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Correo).IsUnique();
            entity.Property(u => u.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(u => u.Correo).HasMaxLength(180).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.PasswordSalt).IsRequired();
        });
    }
}
