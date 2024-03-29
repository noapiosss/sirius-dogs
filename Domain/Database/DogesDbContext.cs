using Contracts.Database;

using Microsoft.EntityFrameworkCore;

namespace Domain.Database
{
    public class DogesDbContext : DbContext
    {
        public DbSet<Dog> Doges { get; init; }
        public DbSet<Image> Images { get; init; }

        public DogesDbContext() : base()
        {
        }

        public DogesDbContext(DbContextOptions<DogesDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<Image>().HasKey(nameof(Image.DogId), nameof(Image.PhotoPath));
            _ = modelBuilder.Entity<Image>()
                .HasOne(i => i.Dog)
                .WithMany(d => d.Photos)
                .HasForeignKey(i => i.DogId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // _ = optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("SIRIUSDOGS__PostgreConnectionString", EnvironmentVariableTarget.Machine));
        }
    }
}